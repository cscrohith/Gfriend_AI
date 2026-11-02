from __future__ import annotations

import ast
import re
from pathlib import Path

from .models import ConditionalBlock, RepeatBlock, Runnable, Status, Statement, TestCase, TestSuite

_VARIABLE = re.compile(r"^\$\{([A-Za-z0-9_-]+)\}\s*=\s*(.*)$")
_USING = re.compile(r"^using\s+([^\s]+)(?:\s+as\s+([^\s]+))?", re.IGNORECASE)
_BLOCK = re.compile(r"^(@)?(repeat|if)\s*:\s*(.*?)\s*(\{)?$", re.IGNORECASE)
_BRANCH = re.compile(r"^(@)?(fail|error)\s*:\s*(\{)?$", re.IGNORECASE)
_VARIABLE_REFERENCE = re.compile(r"\$\{[A-Za-z0-9_-]+\}")


class ParseError(ValueError):
    """Raised when a GFriend script cannot be parsed."""


def _strip_comment(line: str) -> str:
    quote: str | None = None
    escaped = False
    paren_depth = 0

    for index, character in enumerate(line):
        if escaped:
            escaped = False
            continue
        if character == "\\":
            escaped = True
            continue
        if quote:
            if character == quote:
                quote = None
            continue
        if character in ("'", '"'):
            quote = character
        elif character == "(":
            paren_depth += 1
        elif character == ")":
            paren_depth = max(0, paren_depth - 1)
        elif line[index:index + 2] == "//" and paren_depth == 0:
            return line[:index]
    return line


def _read_arguments(source: str, line_number: int) -> list[str]:
    arguments: list[str] = []
    current: list[str] = []
    quote: str | None = None
    escaped = False
    nested = 0

    for character in source:
        if escaped:
            current.append(character)
            escaped = False
            continue
        if character == "\\":
            escaped = True
            current.append(character)
            continue
        if quote:
            current.append(character)
            if character == quote:
                quote = None
            continue
        if character in ("'", '"'):
            quote = character
            current.append(character)
        elif character == "(":
            nested += 1
            current.append(character)
        elif character == ")":
            nested -= 1
            if nested < 0:
                raise ParseError(f"Unexpected ')' on line {line_number}")
            current.append(character)
        elif character == "," and nested == 0:
            arguments.append(_argument_value("".join(current), line_number))
            current = []
        else:
            current.append(character)

    if escaped:
        current.append("\\")
    if quote:
        raise ParseError(f"Unclosed quoted argument on line {line_number}")
    if nested:
        raise ParseError(f"Unclosed nested argument on line {line_number}")
    if current or source.strip():
        arguments.append(_argument_value("".join(current), line_number))
    return arguments


def _argument_value(value: str, line_number: int) -> str:
    value = value.strip()
    if len(value) >= 2 and value[0] in ("'", '"') and value[-1] == value[0]:
        try:
            parsed = ast.literal_eval(value)
        except (SyntaxError, ValueError) as error:
            raise ParseError(f"Invalid quoted argument on line {line_number}: {error}") from error
        if not isinstance(parsed, str):
            raise ParseError(f"Arguments must be text on line {line_number}")
        return parsed
    return value.replace(r"\,", ",").replace(r"\(", "(").replace(r"\)", ")")


def _parse_statement(source: str, line_number: int) -> Statement:
    always_pass = source.startswith("@")
    text = source[1:].lstrip() if always_pass else source
    text = text.strip()

    if not text:
        raise ParseError(f"Empty statement on line {line_number}")

    open_paren = text.find("(")
    if open_paren == -1:
        keyword_text = text
        arguments: list[str] = []
    else:
        if not text.endswith(")"):
            raise ParseError(f"Expected ')' at end of statement on line {line_number}")
        keyword_text = text[:open_paren].strip()
        arguments = _read_arguments(text[open_paren + 1:-1], line_number)

    if not keyword_text:
        raise ParseError(f"Missing keyword name on line {line_number}")

    if "." in keyword_text:
        library, keyword = keyword_text.split(".", 1)
    else:
        library, keyword = "BuiltIn", keyword_text

    return Statement(
        keyword=keyword.strip(),
        library=library.strip(),
        arguments=arguments,
        line_number=line_number,
        always_pass=always_pass,
    )


def _require_open_brace(lines: list[tuple[int, str]], index: int, inline: bool, line_number: int) -> int:
    if inline:
        return index
    if index < len(lines) and lines[index][1] == "{":
        return index + 1
    raise ParseError(f"Expected '{{' after block declaration on line {line_number}")


def _parse_body(
    lines: list[tuple[int, str]],
    index: int,
    *,
    case_name: str,
) -> tuple[list[Runnable], int]:
    body: list[Runnable] = []
    while index < len(lines):
        line_number, text = lines[index]
        if text == "}":
            return body, index + 1

        block_match = _BLOCK.match(text)
        if block_match:
            ignore_result, kind, value, inline_open = block_match.groups()
            index = _require_open_brace(lines, index + 1, bool(inline_open), line_number)
            nested_body, index = _parse_body(lines, index, case_name=case_name)
            if kind.casefold() == "repeat":
                if not value:
                    raise ParseError(f"Repeat count is missing on line {line_number}")
                body.append(RepeatBlock(value.strip(), nested_body, bool(ignore_result)))
                continue

            condition = _parse_statement(value, line_number)
            branches = {Status.PASS: nested_body}
            ignored_branches: set[Status] = set()
            while index < len(lines):
                branch_match = _BRANCH.match(lines[index][1])
                if not branch_match:
                    break
                branch_line, branch_text = lines[index]
                branch_ignore, branch, branch_inline = branch_match.groups()
                index = _require_open_brace(lines, index + 1, bool(branch_inline), branch_line)
                branch_body, index = _parse_body(lines, index, case_name=case_name)
                result = Status.FAIL if branch.casefold() == "fail" else Status.ERROR
                branches[result] = branch_body
                if branch_ignore:
                    ignored_branches.add(result)
            body.append(ConditionalBlock(condition, branches, bool(ignore_result), ignored_branches))
            continue

        if _BRANCH.match(text):
            raise ParseError(f"Fail/Error branch without an If block in test case '{case_name}' on line {line_number}")
        if text == "{":
            raise ParseError(f"Unexpected '{{' on line {line_number}")
        if text.endswith("{"):
            raise ParseError(f"Unsupported block declaration on line {line_number}: {text}")

        body.append(_parse_statement(text, line_number))
        index += 1

    raise ParseError(f"Unclosed test case or block '{case_name}'")


def parse_script(
    source: str,
    *,
    name: str = "TestSuite",
    execution_variables: dict[str, str] | None = None,
) -> TestSuite:
    """Parse a GFriend script's variables, library declarations, cases, and steps."""
    lines = [
        (line_number, stripped)
        for line_number, raw in enumerate(source.splitlines(), start=1)
        if (stripped := _strip_comment(raw).strip()) and not stripped.startswith("///")
    ]
    suite = TestSuite(name=name, test_cases=[])

    index = 0
    while index < len(lines):
        line_number, text = lines[index]
        using_match = _USING.match(text)
        variable_match = _VARIABLE.match(text)
        if using_match:
            library, alias = using_match.groups()
            suite.libraries[alias or library] = library
            index += 1
            continue
        if variable_match:
            variable, value = variable_match.groups()
            suite.variables[f"${{{variable}}}"] = value.strip()
            index += 1
            continue

        inline_open = text.endswith("{")
        if inline_open:
            case_name = text[:-1].strip()
            body_start = index + 1
        elif index + 1 < len(lines) and lines[index + 1][1] == "{":
            case_name = text
            body_start = index + 2
        else:
            raise ParseError(f"Expected a test case declaration followed by '{{' on line {line_number}")

        if not case_name:
            raise ParseError(f"Test case name is missing on line {line_number}")
        steps, index = _parse_body(lines, body_start, case_name=case_name)
        suite.test_cases.append(TestCase(case_name, steps, line_number))

    if not suite.test_cases:
        raise ParseError("The script does not contain any test cases")

    if execution_variables:
        for key, value in execution_variables.items():
            variable = key if key.startswith("${") else f"${{{key}}}"
            if not variable.endswith("}"):
                variable += "}"
            suite.variables[variable] = value
    return suite


def parse_file(path: str | Path, execution_variables: dict[str, str] | None = None) -> TestSuite:
    script_path = Path(path)
    try:
        source = script_path.read_text(encoding="utf-8-sig")
    except OSError as error:
        raise ParseError(f"Cannot read script '{script_path}': {error}") from error
    return parse_script(
        source,
        name=script_path.stem,
        execution_variables=execution_variables,
    )

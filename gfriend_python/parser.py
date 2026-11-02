from __future__ import annotations

import ast
import re
from pathlib import Path

from .models import (
    ConditionalBlock,
    ForEachRowBlock,
    ForSelectedRowBlock,
    ForBlock,
    RepeatBlock,
    RemoteBlock,
    Runnable,
    Status,
    Statement,
    TestCase,
    TestSuite,
    WhileBlock,
)

_VARIABLE = re.compile(r"^\$\{([A-Za-z0-9_-]+)\}\s*=\s*(.*)$")
_USING = re.compile(r"^using\s+([^\s]+)(?:\s+as\s+([^\s]+))?", re.IGNORECASE)
_DATASET = re.compile(r"^dataset\s+(.+?)\s+as\s+([^\s]+)$", re.IGNORECASE)
_RESOURCE = re.compile(r"^resource\s+(.+?)\s+as\s+([^\s]+)$", re.IGNORECASE)
_BLOCK = re.compile(
    r"^(@)?(repeat|for|while|for\s*each\s*row|for\s*selected\s*row|spread\s*sheet\s*for\s*each\s*row|remote\s*run|foreachrow|forselectedrow|spreadsheetforeachrow|remoterun|if)\s*:\s*(.*?)\s*(\{)?$",
    re.IGNORECASE,
)
_BRANCH = re.compile(r"^(@)?(fail|error)\s*:\s*(\{)?$", re.IGNORECASE)
_VARIABLE_REFERENCE = re.compile(r"\$\{[A-Za-z0-9_-]+\}")
_ROW_FILTER = re.compile(r"^([A-Za-z0-9_-]+)\s*(>=|<=|!=|==|=|>|<)\s*(.+)$")


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


def _logical_lines(source: str) -> list[tuple[int, str]]:
    lines: list[tuple[int, str]] = []
    pending: str | None = None
    pending_line = 0
    parenthesis_depth = 0
    quote: str | None = None
    escaped = False

    for line_number, raw in enumerate(source.splitlines(), start=1):
        stripped = _strip_comment(raw).strip()
        if not stripped:
            continue
        if stripped.startswith("\\\\\\") or stripped.startswith("/ <"):
            continue
        if pending is None:
            pending = stripped
            pending_line = line_number
        else:
            pending = f"{pending} {stripped}"

        for character_index, character in enumerate(stripped):
            if escaped:
                escaped = False
                continue
            if character == "\\":
                escaped = True
                continue
            if quote:
                if character == quote:
                    quote = None
            elif character in ("'", '"'):
                if character_index + 1 >= len(stripped) or stripped[character_index + 1] != ",":
                    quote = character
            elif character == "(":
                parenthesis_depth += 1
            elif character == ")":
                parenthesis_depth = max(0, parenthesis_depth - 1)

        if parenthesis_depth == 0:
            if pending.startswith("{") and pending != "{":
                lines.append((pending_line, "{"))
                remainder = pending[1:].strip()
                if remainder.endswith("}"):
                    remainder = remainder[:-1].strip()
                    if remainder:
                        lines.append((pending_line, remainder))
                    lines.append((pending_line, "}"))
                else:
                    lines.append((pending_line, remainder))
            else:
                lines.append((pending_line, pending))
            pending = None
            quote = None
            escaped = False

    if pending is not None:
        lines.append((pending_line, pending))
    normalized: list[tuple[int, str]] = []
    for line_number, text in lines:
        if text != "{":
            normalized.append((line_number, text))
            continue
        declaration_index = -1
        for index in range(len(normalized) - 1, -1, -1):
            previous_text = normalized[index][1]
            if previous_text == "}" or previous_text.endswith("{"):
                break
            if _BLOCK.match(previous_text):
                declaration_index = index
                break
        if declaration_index >= 0:
            declaration_line, declaration = normalized[declaration_index]
            normalized[declaration_index] = (declaration_line, f"{declaration} {{")
        else:
            normalized.append((line_number, text))
    return normalized


def _read_arguments(source: str, line_number: int) -> list[str]:
    arguments: list[str] = []
    current: list[str] = []
    quote: str | None = None
    escaped = False
    nested = 0

    for index, character in enumerate(source):
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
            if not current and index + 1 < len(source) and source[index + 1] == ",":
                current.append(character)
            else:
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
            normalized_kind = re.sub(r"\s+", "", kind).casefold()
            if normalized_kind == "repeat":
                if not value:
                    raise ParseError(f"Repeat count is missing on line {line_number}")
                body.append(RepeatBlock(value.strip(), nested_body, bool(ignore_result)))
                continue
            if normalized_kind == "for":
                values = _read_arguments(value, line_number)
                if not values:
                    raise ParseError(f"For list is empty on line {line_number}")
                body.append(ForBlock(values, nested_body, bool(ignore_result)))
                continue
            if normalized_kind == "while":
                condition_text = value.strip()
                max_iterations = "50"
                if ":" in condition_text:
                    candidate_condition, candidate_limit = condition_text.rsplit(":", 1)
                    candidate_limit = candidate_limit.strip()
                    if re.fullmatch(r"\d+|\$\{[A-Za-z0-9_-]+\}", candidate_limit):
                        condition_text = candidate_condition.strip()
                        max_iterations = candidate_limit
                    elif not candidate_limit:
                        condition_text = candidate_condition.strip()
                if not condition_text:
                    raise ParseError(f"While condition is missing on line {line_number}")
                while_pass = not condition_text.startswith("!")
                if not while_pass:
                    condition_text = condition_text[1:].strip()
                if not condition_text:
                    raise ParseError(f"While condition is missing on line {line_number}")
                body.append(
                    WhileBlock(
                        _parse_statement(condition_text, line_number),
                        nested_body,
                        max_iterations,
                        while_pass,
                        bool(ignore_result),
                    )
                )
                continue
            if normalized_kind in {"foreachrow", "spreadsheetforeachrow"}:
                parts = value.split(".")
                if len(parts) != 3 or not all(part.strip() for part in parts):
                    raise ParseError(
                        f"Expected dataset.sheet.table in {kind} declaration on line {line_number}"
                    )
                body.append(
                    ForEachRowBlock(
                        parts[0].strip(),
                        parts[1].strip(),
                        parts[2].strip(),
                        nested_body,
                        line_number,
                        bool(ignore_result),
                        normalized_kind == "spreadsheetforeachrow",
                    )
                )
                continue
            if normalized_kind == "forselectedrow":
                parts = value.split(".")
                if len(parts) != 4 or not all(part.strip() for part in parts):
                    raise ParseError(
                        f"Expected dataset.sheet.table.column<operator>value in ForSelectedRow on line {line_number}"
                    )
                filter_match = _ROW_FILTER.match(parts[3].strip())
                if filter_match is None:
                    raise ParseError(f"Invalid ForSelectedRow filter on line {line_number}")
                column, operator, raw_values = filter_match.groups()
                filters = {column: (operator, [part.strip() for part in raw_values.split(",")])}
                if not all(filters[column][1]):
                    raise ParseError(f"ForSelectedRow filter has an empty value on line {line_number}")
                body.append(
                    ForSelectedRowBlock(
                        parts[0].strip(),
                        parts[1].strip(),
                        parts[2].strip(),
                        filters,
                        nested_body,
                        line_number,
                        bool(ignore_result),
                    )
                )
                continue
            if normalized_kind == "remoterun":
                remote_id = value.strip()
                if not remote_id:
                    raise ParseError(f"Remote executor ID is missing on line {line_number}")
                body.append(
                    RemoteBlock(remote_id, nested_body, line_number, bool(ignore_result))
                )
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

        branch_match = _BRANCH.match(text)
        if branch_match:
            branch_ignore, branch, branch_inline = branch_match.groups()
            target = next(
                (item for item in reversed(body) if isinstance(item, ConditionalBlock)),
                None,
            )
            if target is None:
                raise ParseError(
                    f"Fail/Error branch without an If block in test case '{case_name}' on line {line_number}"
                )
            result = Status.FAIL if branch.casefold() == "fail" else Status.ERROR
            if result in target.branches:
                raise ParseError(f"Duplicate {branch} branch on line {line_number}")
            index = _require_open_brace(lines, index + 1, bool(branch_inline), line_number)
            branch_body, index = _parse_body(lines, index, case_name=case_name)
            target.branches[result] = branch_body
            if branch_ignore:
                target.ignored_branches.add(result)
            continue
        if text == "{":
            raise ParseError(f"Unexpected '{{' on line {line_number}")
        if text.endswith("{"):
            condition_text = text[:-1].strip()
            nested_body, index = _parse_body(lines, index + 1, case_name=case_name)
            body.append(
                ConditionalBlock(
                    _parse_statement(condition_text, line_number),
                    {Status.PASS: nested_body},
                )
            )
            continue
        if index + 1 < len(lines) and lines[index + 1][1] == "{":
            nested_body, index = _parse_body(lines, index + 2, case_name=case_name)
            body.append(
                ConditionalBlock(
                    _parse_statement(text, line_number),
                    {Status.PASS: nested_body},
                )
            )
            continue

        body.append(_parse_statement(text, line_number))
        index += 1

    raise ParseError(f"Unclosed test case or block '{case_name}'")


def parse_script(
    source: str,
    *,
    name: str = "TestSuite",
    execution_variables: dict[str, str] | None = None,
    base_dir: str | Path | None = None,
) -> TestSuite:
    """Parse a GFriend script's variables, library declarations, cases, and steps."""
    lines = [
        (line_number, text)
        for line_number, text in _logical_lines(source)
        if not text.startswith("///")
    ]
    suite = TestSuite(name=name, test_cases=[])
    suite.script_folder = str(Path(base_dir).resolve()) if base_dir is not None else str(Path.cwd())

    index = 0
    while index < len(lines):
        line_number, text = lines[index]
        using_match = _USING.match(text)
        variable_match = _VARIABLE.match(text)
        if using_match:
            library, alias = using_match.groups()
            library_path = Path(library)
            suffix = library_path.suffix.casefold()
            if suffix in {".gflib", ".gfvar"} and not library_path.is_absolute() and base_dir is not None:
                library = str((Path(base_dir) / library_path).resolve())
                library_path = Path(library)
            if suffix == ".gfvar":
                suite.variables.update(parse_variable_file(library_path))
                index += 1
                continue
            library_name = alias or (library_path.stem if suffix == ".gflib" else library)
            suite.libraries[library_name] = library
            index += 1
            continue
        dataset_match = _DATASET.match(text)
        if dataset_match:
            path_text, alias = dataset_match.groups()
            path_text = path_text.strip()
            if len(path_text) >= 2 and path_text[0] in ("'", '"') and path_text[-1] == path_text[0]:
                path_text = _argument_value(path_text, line_number)
            dataset_path = Path(path_text)
            if not dataset_path.is_absolute() and base_dir is not None:
                dataset_path = Path(base_dir) / dataset_path
            suite.datasets[alias] = str(dataset_path.resolve())
            index += 1
            continue
        resource_match = _RESOURCE.match(text)
        if resource_match:
            path_text, alias = resource_match.groups()
            path_text = path_text.strip()
            if len(path_text) >= 2 and path_text[0] in ("'", '"') and path_text[-1] == path_text[0]:
                path_text = _argument_value(path_text, line_number)
            resource_path = Path(path_text)
            if not resource_path.is_absolute() and base_dir is not None:
                resource_path = Path(base_dir) / resource_path
            suite.resources[alias] = str(resource_path.resolve())
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
        base_dir=script_path.parent,
    )


def parse_variable_file(path: str | Path) -> dict[str, str]:
    variable_path = Path(path)
    try:
        contents = variable_path.read_text(encoding="utf-8-sig")
    except OSError as error:
        raise ParseError(f"Cannot read variable file '{variable_path}': {error}") from error

    variables: dict[str, str] = {}
    for line_number, raw in enumerate(contents.splitlines(), start=1):
        line = raw.strip()
        if not line or line.startswith("//"):
            continue
        match = _VARIABLE.match(line)
        if not match:
            continue
        name, value = match.groups()
        variables[f"${{{name}}}"] = value.strip()
    return variables

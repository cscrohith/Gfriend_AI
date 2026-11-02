from __future__ import annotations

import re
import time
import csv
from collections.abc import Callable
from pathlib import Path
from typing import Any

from .models import (
    ConditionalBlock,
    ExecutionResult,
    ForEachRowBlock,
    ForBlock,
    KeywordResult,
    RepeatBlock,
    Runnable,
    Status,
    Statement,
    StepResult,
    TestCaseResult,
    TestSuite,
    WhileBlock,
)

_VARIABLE = re.compile(r"\$\{[A-Za-z0-9_-]+\}")


def _normalize(value: str) -> str:
    return re.sub(r"\s+", "", value).casefold()


class Executor:
    """Executes parsed GFriend scripts against built-ins and registered Python libraries."""

    def __init__(
        self,
        *,
        variables: dict[str, str] | None = None,
        sleep: Callable[[float], None] = time.sleep,
    ) -> None:
        self.variables: dict[str, str] = {}
        self._execution_variables: dict[str, str] = {}
        self._libraries: dict[str, object] = {}
        self._sleep = sleep
        self._suite: TestSuite | None = None
        self._flow = ""
        if variables:
            self._set_variables(variables)

    def register_library(self, name: str, library: object) -> None:
        """Register a Python object whose public methods implement GFriend keywords."""
        if not name.strip():
            raise ValueError("Library name must not be empty")
        self._libraries[name.casefold()] = library

    def run(self, suite: TestSuite, test_cases: set[str] | None = None) -> ExecutionResult:
        self._suite = suite
        self.variables = dict(suite.variables)
        self.variables.update(self._execution_variables)
        self.variables.setdefault("${EMPTY}", "")
        self.variables.setdefault("${CARRIAGE_RETURN}", "\r")
        self.variables.setdefault("${LINE_FEED}", "\n")

        results: list[TestCaseResult] = []
        selected = {name.casefold() for name in test_cases} if test_cases is not None else None
        for test_case in suite.test_cases:
            if selected is not None and test_case.name.casefold() not in selected:
                continue
            self._flow = ""
            self.variables["${TESTCASE_NAME}"] = test_case.name
            steps: list[StepResult] = []
            status = self._run_items(test_case.steps, steps)
            results.append(TestCaseResult(test_case.name, status, steps))
            if self._flow == "suite":
                break

        suite_status = self._aggregate(result.status for result in results)
        return ExecutionResult(suite.name, suite_status, results)

    def _set_variables(self, variables: dict[str, str]) -> None:
        for key, value in variables.items():
            variable = key if key.startswith("${") else f"${{{key}}}"
            if not variable.endswith("}"):
                variable += "}"
            self._execution_variables[variable] = str(value)

    def _run_items(self, items: list[Runnable], results: list[StepResult]) -> Status:
        status = Status.PASS
        for item in items:
            if isinstance(item, Statement):
                result = self._run_statement(item)
                results.append(result)
                if item.always_pass:
                    result.status = Status.PASS
                status = self._aggregate((status, result.status))
            elif isinstance(item, RepeatBlock):
                block_status = self._run_repeat(item, results)
                if item.always_pass:
                    block_status = Status.PASS
                status = self._aggregate((status, block_status))
            elif isinstance(item, ForBlock):
                block_status = self._run_for(item, results)
                if item.always_pass:
                    block_status = Status.PASS
                status = self._aggregate((status, block_status))
            elif isinstance(item, WhileBlock):
                block_status = self._run_while(item, results)
                if item.always_pass:
                    block_status = Status.PASS
                status = self._aggregate((status, block_status))
            elif isinstance(item, ForEachRowBlock):
                block_status = self._run_each_row(item, results)
                if item.always_pass:
                    block_status = Status.PASS
                status = self._aggregate((status, block_status))
            else:
                block_status = self._run_conditional(item, results)
                if item.always_pass:
                    block_status = Status.PASS
                status = self._aggregate((status, block_status))
            if self._flow in {"case", "suite", "break", "continue"}:
                break
        return status

    def _run_statement(self, statement: Statement) -> StepResult:
        keyword_name = _normalize(statement.keyword)
        variable_arguments = {
            "reassignvaluetovariable": {0},
            "setvariable": {0},
            "calculateandassign": {1},
        }.get(keyword_name, set())
        arguments = [
            value if index in variable_arguments else self._expand(value)
            for index, value in enumerate(statement.arguments)
        ]
        library_name = statement.library.casefold()
        if self._suite:
            for alias, library in self._suite.libraries.items():
                if alias.casefold() == library_name:
                    library_name = library.casefold()
                    break

        try:
            if library_name == "builtin":
                result = self._execute_builtin(statement.keyword, arguments)
            else:
                library = self._libraries.get(library_name)
                if library is None:
                    raise LookupError(f"Python library '{statement.library}' is not registered")
                method = self._find_method(library, statement.keyword)
                if method is None:
                    raise LookupError(
                        f"Keyword '{statement.keyword}' is not defined by library '{statement.library}'"
                    )
                result = self._coerce_result(method(*arguments))
        except Exception as error:
            result = KeywordResult(Status.ERROR, str(error))

        return StepResult(
            keyword=f"{statement.library}.{statement.keyword}",
            line_number=statement.line_number,
            status=Status.PASS if statement.always_pass else result.status,
            output=result.output,
        )

    def _execute_builtin(self, keyword: str, arguments: list[str]) -> KeywordResult:
        normalized = _normalize(keyword)
        if normalized == "pass":
            return KeywordResult(Status.PASS, arguments[0] if arguments else "")
        if normalized == "fail":
            return KeywordResult(Status.FAIL, arguments[0] if arguments else "Keyword reported failure")
        if normalized == "error":
            return KeywordResult(Status.ERROR, arguments[0] if arguments else "Keyword reported an error")
        if normalized == "equals":
            self._require_arguments(keyword, arguments, 2)
            if arguments[0] == arguments[1]:
                return KeywordResult(Status.PASS)
            return KeywordResult(Status.FAIL, f"{arguments[0]!r} does not equal {arguments[1]!r}")
        if normalized == "log":
            self._require_arguments(keyword, arguments, 1)
            return KeywordResult(Status.PASS, arguments[0])
        if normalized == "sleep":
            self._require_arguments(keyword, arguments, 1)
            seconds = float(arguments[0])
            if seconds < 0:
                raise ValueError("Sleep duration must not be negative")
            self._sleep(seconds)
            return KeywordResult(Status.PASS)
        if normalized in {"exittestcase", "stoptestcase", "stopcase"}:
            self._flow = "case"
            return KeywordResult(Status.PASS)
        if normalized in {"stoptestsuite", "exittestsuite"}:
            self._flow = "suite"
            return KeywordResult(Status.PASS)
        if normalized in {"break", "breakloop"}:
            self._flow = "break"
            return KeywordResult(Status.PASS)
        if normalized in {"continue", "continueloop"}:
            self._flow = "continue"
            return KeywordResult(Status.PASS)
        if normalized in {"reassignvaluetovariable", "setvariable"}:
            self._require_arguments(keyword, arguments, 2)
            self.variables[arguments[0]] = arguments[1]
            return KeywordResult(Status.PASS)
        if normalized == "calculateandassign":
            self._require_arguments(keyword, arguments, 2)
            self.variables[arguments[1]] = str(self._calculate(arguments[0]))
            return KeywordResult(Status.PASS)
        if normalized == "equalsignorecase":
            self._require_arguments(keyword, arguments, 2)
            if arguments[0].casefold() == arguments[1].casefold():
                return KeywordResult(Status.PASS)
            return KeywordResult(Status.FAIL, f"{arguments[0]!r} does not equal {arguments[1]!r} ignoring case")
        raise LookupError(f"Built-in keyword '{keyword}' is not implemented")

    @staticmethod
    def _require_arguments(keyword: str, arguments: list[str], count: int) -> None:
        if len(arguments) != count:
            raise ValueError(f"{keyword} expects {count} argument(s); got {len(arguments)}")

    @staticmethod
    def _find_method(library: object, keyword: str) -> Callable[..., Any] | None:
        target = _normalize(keyword)
        for name in dir(library):
            if name.startswith("_") or _normalize(name.replace("_", " ")) != target:
                continue
            method = getattr(library, name)
            if callable(method):
                return method
        return None

    @staticmethod
    def _coerce_result(value: Any) -> KeywordResult:
        if isinstance(value, KeywordResult):
            return value
        if value is None:
            return KeywordResult(Status.PASS)
        if isinstance(value, bool):
            return KeywordResult(Status.PASS if value else Status.FAIL)
        raise TypeError("Python keyword must return KeywordResult, bool, or None")

    def _run_repeat(self, block: RepeatBlock, results: list[StepResult]) -> Status:
        raw_count = self._expand(block.count)
        duration = self._duration_seconds(raw_count)
        deadline = time.monotonic() + duration if duration is not None else None
        try:
            count = int(raw_count) if duration is None else 2**63 - 1
        except ValueError:
            return Status.ERROR
        if count < 0:
            return Status.ERROR

        status = Status.PASS
        for iteration in range(1, count + 1):
            self.variables["${R}"] = str(iteration)
            status = self._aggregate((status, self._run_items(block.body, results)))
            if self._flow in {"case", "suite"}:
                break
            if self._flow == "break":
                self._flow = ""
                break
            if self._flow == "continue":
                self._flow = ""
            if deadline is not None and time.monotonic() >= deadline:
                break
            if deadline is not None:
                self._sleep(min(0.01, max(0.0, deadline - time.monotonic())))
        return status

    def _run_for(self, block: ForBlock, results: list[StepResult]) -> Status:
        status = Status.PASS
        previous_item = self.variables.get("${ITEM}")
        for index, value in enumerate(block.values):
            self.variables["${ITEM}"] = self._expand(value)
            self.variables["${R}"] = str(index + 1)
            status = self._aggregate((status, self._run_items(block.body, results)))
            if self._flow in {"case", "suite"}:
                break
            if self._flow == "break":
                self._flow = ""
                break
            if self._flow == "continue":
                self._flow = ""
        if previous_item is None:
            self.variables.pop("${ITEM}", None)
        else:
            self.variables["${ITEM}"] = previous_item
        return status

    def _run_while(self, block: WhileBlock, results: list[StepResult]) -> Status:
        try:
            max_iterations = int(self._expand(block.max_iterations))
        except ValueError:
            return Status.ERROR
        if max_iterations < 0:
            return Status.ERROR

        status = Status.PASS
        for iteration in range(1, max_iterations + 1):
            self.variables["${R}"] = str(iteration)
            condition = self._run_statement(block.condition)
            results.append(condition)
            if condition.status is Status.ERROR:
                return Status.ERROR
            if (condition.status is Status.PASS) != block.while_pass:
                break
            status = self._aggregate((status, self._run_items(block.body, results)))
            if self._flow in {"case", "suite"}:
                break
            if self._flow == "break":
                self._flow = ""
                break
            if self._flow == "continue":
                self._flow = ""
        return status

    def _run_each_row(self, block: ForEachRowBlock, results: list[StepResult]) -> Status:
        if self._suite is None or block.dataset.casefold() not in {
            name.casefold() for name in self._suite.datasets
        }:
            results.append(
                StepResult(
                    keyword="ForEachRow",
                    line_number=block.line_number,
                    status=Status.ERROR,
                    output=f"Dataset '{block.dataset}' is not declared",
                )
            )
            return Status.ERROR

        dataset_path = next(
            path
            for name, path in self._suite.datasets.items()
            if name.casefold() == block.dataset.casefold()
        )
        try:
            rows = self._load_rows(Path(dataset_path), block.sheet, block.table)
        except (OSError, ValueError, ImportError, KeyError) as error:
            results.append(
                StepResult(
                    keyword="ForEachRow",
                    line_number=block.line_number,
                    status=Status.ERROR,
                    output=str(error),
                )
            )
            return Status.ERROR

        status = Status.PASS
        for index, row in enumerate(rows, start=1):
            for header, value in row.items():
                self.variables[f"${{{header}}}"] = value
            self.variables["${R}"] = str(index)
            status = self._aggregate((status, self._run_items(block.body, results)))
            if self._flow in {"case", "suite"}:
                break
            if self._flow == "break":
                self._flow = ""
                break
            if self._flow == "continue":
                self._flow = ""
        return status

    @staticmethod
    def _load_rows(path: Path, sheet_name: str, table_name: str) -> list[dict[str, str]]:
        if not path.is_file():
            raise FileNotFoundError(f"Dataset file does not exist: {path}")
        if path.suffix.casefold() == ".csv":
            with path.open("r", encoding="utf-8-sig", newline="") as stream:
                return [
                    {
                        re.sub(r"\s+", "", header.strip()): value or ""
                        for header, value in row.items()
                        if header is not None
                    }
                    for row in csv.DictReader(stream)
                ]
        if path.suffix.casefold() not in {".xlsx", ".xlsm"}:
            raise ValueError(f"Unsupported dataset type: {path.suffix or '(none)'}")

        try:
            from openpyxl import load_workbook
            from openpyxl.utils.cell import range_boundaries
        except ImportError as error:
            raise ImportError("Excel datasets require openpyxl; install the gfriend-python package dependencies") from error

        workbook = load_workbook(path, read_only=False, data_only=True)
        try:
            if sheet_name not in workbook.sheetnames:
                raise KeyError(f"Worksheet '{sheet_name}' was not found in '{path.name}'")
            worksheet = workbook[sheet_name]
            if table_name not in worksheet.tables:
                raise KeyError(f"Table '{table_name}' was not found on worksheet '{sheet_name}'")
            min_col, min_row, max_col, max_row = range_boundaries(worksheet.tables[table_name].ref)
            headers = [
                re.sub(r"\s+", "", str(worksheet.cell(min_row, column).value or "").strip())
                for column in range(min_col, max_col + 1)
            ]
            if not headers or any(not header for header in headers):
                raise ValueError(f"Table '{table_name}' has an empty column header")
            if len(set(header.casefold() for header in headers)) != len(headers):
                raise ValueError(f"Table '{table_name}' has duplicate column headers")
            return [
                {
                    header: str(worksheet.cell(row, column).value or "")
                    for header, column in zip(headers, range(min_col, max_col + 1))
                }
                for row in range(min_row + 1, max_row + 1)
            ]
        finally:
            workbook.close()

    def _run_conditional(self, block: ConditionalBlock, results: list[StepResult]) -> Status:
        condition = self._run_statement(block.condition)
        results.append(condition)
        branch = block.branches.get(condition.status, [])
        if not branch:
            return Status.PASS
        status = self._run_items(branch, results)
        if condition.status in block.ignored_branches:
            return Status.PASS
        return status

    @staticmethod
    def _duration_seconds(value: str) -> float | None:
        match = re.fullmatch(r"(\d+(?:\.\d+)?)([hms])", value.strip(), re.IGNORECASE)
        if not match:
            return None
        amount = float(match.group(1))
        unit = match.group(2).casefold()
        return amount * {"h": 3600, "m": 60, "s": 1}[unit]

    @staticmethod
    def _calculate(expression: str) -> int | float:
        import ast
        import operator

        operations: dict[type[ast.operator], Callable[[Any, Any], Any]] = {
            ast.Add: lambda left, right: left + right,
            ast.Sub: lambda left, right: left - right,
            ast.Mult: lambda left, right: left * right,
            ast.Div: lambda left, right: left / right,
            ast.FloorDiv: lambda left, right: left // right,
            ast.Mod: lambda left, right: left % right,
            ast.Pow: lambda left, right: left**right,
        }
        unary_operations: dict[type[ast.unaryop], Callable[[Any], Any]] = {
            ast.UAdd: operator.pos,
            ast.USub: operator.neg,
        }

        def evaluate(node: ast.AST) -> int | float:
            if isinstance(node, ast.Expression):
                return evaluate(node.body)
            if isinstance(node, ast.Constant) and isinstance(node.value, (int, float)):
                return node.value
            if isinstance(node, ast.BinOp) and type(node.op) in operations:
                if isinstance(node.op, ast.Pow):
                    exponent = evaluate(node.right)
                    if abs(exponent) > 100:
                        raise ValueError("Exponent magnitude must not exceed 100")
                    return evaluate(node.left) ** exponent
                return operations[type(node.op)](evaluate(node.left), evaluate(node.right))
            if isinstance(node, ast.UnaryOp) and type(node.op) in unary_operations:
                return unary_operations[type(node.op)](evaluate(node.operand))
            raise ValueError("Only numeric arithmetic is allowed")

        return evaluate(ast.parse(expression, mode="eval"))

    def _expand(self, value: str) -> str:
        def replace(match: re.Match[str]) -> str:
            return self.variables.get(match.group(0), match.group(0))

        return _VARIABLE.sub(replace, value)

    @staticmethod
    def _aggregate(statuses: Any) -> Status:
        values = list(statuses)
        if Status.ERROR in values:
            return Status.ERROR
        if Status.FAIL in values:
            return Status.FAIL
        return Status.PASS

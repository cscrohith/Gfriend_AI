from __future__ import annotations

import re
import time
from collections.abc import Callable
from typing import Any

from .models import (
    ConditionalBlock,
    ExecutionResult,
    KeywordResult,
    RepeatBlock,
    Runnable,
    Status,
    Statement,
    StepResult,
    TestCaseResult,
    TestSuite,
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
            self.variables["${TESTCASE_NAME}"] = test_case.name
            steps: list[StepResult] = []
            status = self._run_items(test_case.steps, steps)
            results.append(TestCaseResult(test_case.name, status, steps))

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
            else:
                block_status = self._run_conditional(item, results)
                if item.always_pass:
                    block_status = Status.PASS
                status = self._aggregate((status, block_status))
        return status

    def _run_statement(self, statement: Statement) -> StepResult:
        arguments = [self._expand(value) for value in statement.arguments]
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
        try:
            count = int(raw_count)
        except ValueError:
            return Status.ERROR
        if count < 0:
            return Status.ERROR

        status = Status.PASS
        for iteration in range(1, count + 1):
            self.variables["${R}"] = str(iteration)
            status = self._aggregate((status, self._run_items(block.body, results)))
        return status

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

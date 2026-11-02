from __future__ import annotations

import re
import time
import csv
import random
from collections.abc import Callable
from datetime import datetime, timedelta, timezone
from pathlib import Path
from typing import Any

from .models import (
    ConditionalBlock,
    ExecutionResult,
    ForEachRowBlock,
    ForBlock,
    ForSelectedRowBlock,
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
from .parser import ParseError, _parse_statement, parse_file

_VARIABLE = re.compile(r"\$\{[A-Za-z0-9_-]+\}")


def _normalize(value: str) -> str:
    return re.sub(r"\s+", "", value).casefold()


class _ScriptLibrary:
    def __init__(self, executor: Executor, path: str) -> None:
        self._executor = executor
        self._suite = parse_file(path)
        self._keywords: dict[str, tuple[list[str], list[Runnable]]] = {}
        for test_case in self._suite.test_cases:
            signature = re.fullmatch(r"\s*(.*?)\s*\((.*?)\)\s*", test_case.name)
            if signature:
                keyword, raw_parameters = signature.groups()
                parameters = [item.strip() for item in raw_parameters.split(",") if item.strip()]
            else:
                keyword, parameters = test_case.name.strip(), []
            normalized = _normalize(keyword)
            if normalized in self._keywords:
                raise ParseError(f"Duplicate custom keyword '{keyword}' in '{path}'")
            self._keywords[normalized] = (parameters, test_case.steps)

    def __getattr__(self, name: str) -> Callable[..., KeywordResult]:
        keyword = self._keywords.get(_normalize(name))
        if keyword is None:
            raise AttributeError(name)
        parameters, body = keyword

        def invoke(*arguments: str) -> KeywordResult:
            if len(arguments) != len(parameters):
                raise ValueError(f"Custom keyword '{name}' expects {len(parameters)} argument(s); got {len(arguments)}")
            previous = self._executor.variables.copy()
            previous_suite = self._executor._suite
            try:
                self._executor._suite = self._suite
                self._executor._register_script_libraries(self._suite)
                self._executor.variables.update(self._suite.variables)
                for parameter, value in zip(parameters, arguments):
                    self._executor.variables[parameter] = value
                nested_results: list[StepResult] = []
                status = self._executor._run_items(body, nested_results)
                output = next(
                    (step.output for step in reversed(nested_results) if step.output),
                    "",
                )
                if self._executor._flow == "custom-return":
                    status = self._executor._custom_return_status
                    self._executor._flow = ""
                return KeywordResult(status, output)
            finally:
                self._executor.variables = previous
                self._executor._suite = previous_suite

        return invoke


class Executor:
    """Executes parsed GFriend scripts against built-ins and registered Python libraries."""

    def __init__(
        self,
        *,
        variables: dict[str, str] | None = None,
        sleep: Callable[[float], None] = time.sleep,
        output_folder: str | Path | None = None,
    ) -> None:
        self.variables: dict[str, str] = {}
        self._execution_variables: dict[str, str] = {}
        self._libraries: dict[str, object] = {}
        self._sleep = sleep
        self._suite: TestSuite | None = None
        self.output_folder = Path(output_folder).resolve() if output_folder else Path.cwd()
        self._flow = ""
        self._custom_return_status = Status.PASS
        self._active_dataset_row: tuple[Path, str, str, int, dict[str, str]] | None = None
        if variables:
            self._set_variables(variables)

    def register_library(self, name: str, library: object) -> None:
        """Register a Python object whose public methods implement GFriend keywords."""
        if not name.strip():
            raise ValueError("Library name must not be empty")
        self._libraries[name.casefold()] = library

    def run(self, suite: TestSuite, test_cases: set[str] | None = None) -> ExecutionResult:
        started_at = datetime.now(timezone.utc)
        started_counter = time.perf_counter()
        self._suite = suite
        self.variables = dict(suite.variables)
        self.variables.update(self._execution_variables)
        self.variables.setdefault("${EMPTY}", "")
        self.variables.setdefault("${CARRIAGE_RETURN}", "\r")
        self.variables.setdefault("${LINE_FEED}", "\n")
        self.variables["${SCRIPT_FOLDER}"] = suite.script_folder
        self.variables["${OUTPUT_FOLDER}"] = str(self.output_folder)
        self.variables["${KEYWORD_OUTPUT}"] = ""
        self.variables["${KEYWORD_RESULT}"] = Status.PASS.value
        for alias, path in suite.resources.items():
            self.variables[f"${{{alias}}}"] = path
        self._register_script_libraries(suite)

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
        return ExecutionResult(
            suite.name,
            suite_status,
            results,
            started_at,
            time.perf_counter() - started_counter,
        )

    def _register_script_libraries(self, suite: TestSuite) -> None:
        for alias, library in suite.libraries.items():
            if library.casefold().endswith(".gflib") and Path(library).is_file():
                self.register_library(alias, _ScriptLibrary(self, library))

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
            elif isinstance(item, ForSelectedRowBlock):
                block_status = self._run_selected_rows(item, results)
                if item.always_pass:
                    block_status = Status.PASS
                status = self._aggregate((status, block_status))
            else:
                block_status = self._run_conditional(item, results)
                if item.always_pass:
                    block_status = Status.PASS
                status = self._aggregate((status, block_status))
            if self._flow in {"case", "suite", "break", "continue", "custom-return"}:
                break
        return status

    def _run_statement(self, statement: Statement) -> StepResult:
        keyword_name = _normalize(statement.keyword)
        variable_arguments = {
            "reassignvaluetovariable": {0},
            "setvariable": {0},
            "calculateandassign": {1},
            "calculate": {1},
            "readfromfile": {1},
            "gettimestamp": {0},
            "gettimedifference": {2},
            "getrandomnumber": {2},
            "trim": {0},
            "replace": {0},
            "substring": {3},
            "count": {0},
            "splitandstoreword": {3},
        }.get(keyword_name, set())
        resource_names = {
            alias.casefold(): path for alias, path in (self._suite.resources.items() if self._suite else [])
        }
        arguments = []
        for index, value in enumerate(statement.arguments):
            if index in variable_arguments:
                arguments.append(value)
                continue
            expanded = self._expand(value)
            arguments.append(resource_names.get(expanded.casefold(), expanded))
        library_name = statement.library.casefold()
        if self._suite:
            for alias, library in self._suite.libraries.items():
                if alias.casefold() == library_name:
                    library_name = library.casefold()
                    if library.casefold().endswith(".gflib"):
                        library_name = alias.casefold()
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

        self.variables["${KEYWORD_OUTPUT}"] = result.output
        self.variables["${KEYWORD_RESULT}"] = result.status.value
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
        if normalized == "notequals":
            self._require_arguments(keyword, arguments, 2)
            if arguments[0] != arguments[1]:
                return KeywordResult(Status.PASS)
            return KeywordResult(Status.FAIL, f"{arguments[0]} and {arguments[1]} are same")
        if normalized == "compareamounts":
            self._require_arguments(keyword, arguments, 2)
            amount = arguments[0].lstrip("₹").replace("\\", "")
            try:
                left = float(amount)
            except ValueError:
                left = 0.0
            try:
                right = float(arguments[1])
            except ValueError:
                right = 0.0
            if left == right:
                return KeywordResult(Status.PASS)
            return KeywordResult(Status.FAIL, f"{amount} and {arguments[1]} are not same")
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
        if normalized == "randomsleep":
            self._require_arguments(keyword, arguments, 2)
            minimum, maximum = map(float, arguments)
            if minimum < 0 or maximum < minimum:
                raise ValueError("Random Sleep requires 0 <= min seconds <= max seconds")
            self._sleep(random.uniform(minimum, maximum))
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
        if normalized in {"returnwithfail", "returnwithpass", "returnwitherror"}:
            self._require_arguments(keyword, arguments, 1)
            status = {
                "returnwithfail": Status.FAIL,
                "returnwithpass": Status.PASS,
                "returnwitherror": Status.ERROR,
            }[normalized]
            self._flow = "custom-return"
            self._custom_return_status = status
            return KeywordResult(status, arguments[0])
        if normalized in {"reassignvaluetovariable", "setvariable"}:
            self._require_arguments(keyword, arguments, 2)
            self.variables[arguments[0]] = arguments[1]
            return KeywordResult(Status.PASS)
        if normalized in {"calculateandassign", "calculate"}:
            self._require_arguments(keyword, arguments, 2)
            self.variables[arguments[1]] = str(self._calculate(arguments[0]))
            return KeywordResult(Status.PASS)
        if normalized == "equalsignorecase":
            self._require_arguments(keyword, arguments, 2)
            if arguments[0].casefold() == arguments[1].casefold():
                return KeywordResult(Status.PASS)
            return KeywordResult(Status.FAIL, f"{arguments[0]!r} does not equal {arguments[1]!r} ignoring case")
        if normalized in {"contains", "containsignorecase"}:
            self._require_arguments(keyword, arguments, 2)
            left, right = (value.replace(r"\n", "\n") for value in arguments)
            found = right.casefold() in left.casefold() if normalized == "containsignorecase" else right in left
            return KeywordResult(
                Status.PASS if found else Status.FAIL,
                "" if found else f"{right!r} was not found",
            )
        if normalized in {"converttouppercase", "converttolowercase"}:
            self._require_arguments(keyword, arguments, 2)
            converted = (
                arguments[0].upper()
                if normalized == "converttouppercase"
                else arguments[0].lower()
            )
            self.variables[arguments[1]] = converted
            return KeywordResult(Status.PASS, f"{arguments[1]} = {converted}")
        if normalized == "writetodataset":
            self._require_arguments(keyword, arguments, 2)
            return self._write_to_dataset(arguments[0], arguments[1])
        if normalized == "fileexist":
            self._require_arguments(keyword, arguments, 1)
            return KeywordResult(Status.PASS if self._resolve_path(arguments[0]).is_file() else Status.FAIL)
        if normalized == "folderexist":
            self._require_arguments(keyword, arguments, 1)
            return KeywordResult(Status.PASS if self._resolve_path(arguments[0]).is_dir() else Status.FAIL)
        if normalized == "createfolder":
            self._require_arguments(keyword, arguments, 1)
            self._resolve_path(arguments[0], for_output=True).mkdir(parents=True, exist_ok=True)
            return KeywordResult(Status.PASS)
        if normalized == "readfromfile":
            self._require_arguments(keyword, arguments, 2)
            value = self._resolve_path(arguments[0]).read_text(encoding="utf-8-sig")
            self.variables[arguments[1]] = value
            return KeywordResult(Status.PASS, value)
        if normalized == "savetofile":
            self._require_arguments(keyword, arguments, 2)
            path = self._resolve_path(arguments[0], for_output=True)
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_text(arguments[1], encoding="utf-8")
            return KeywordResult(Status.PASS, str(path))
        if normalized == "appendtofile":
            self._require_arguments(keyword, arguments, 2)
            file_name = arguments[0] or f"{datetime.now():%Y%m%d_%H%M%S}_SaveToFile.txt"
            path = self._resolve_path(file_name, for_output=True)
            try:
                with path.open("a", encoding="utf-8") as stream:
                    stream.write("\n" + arguments[1])
            except OSError as error:
                return KeywordResult(Status.ERROR, f"Error during write value to file: {error}")
            return KeywordResult(Status.PASS)
        if normalized == "filecontainstext":
            self._require_arguments(keyword, arguments, 2)
            contents = self._resolve_path(arguments[0]).read_text(encoding="utf-8-sig")
            return KeywordResult(
                Status.PASS if arguments[1] in contents else Status.FAIL,
                "" if arguments[1] in contents else f"Text was not found in '{arguments[0]}'",
            )
        if normalized in {"checkiffilesaresame", "checkiffilesarenotsame"}:
            self._require_arguments(keyword, arguments, 2)
            same = self._resolve_path(arguments[0]).read_bytes() == self._resolve_path(arguments[1]).read_bytes()
            passed = same if normalized == "checkiffilesaresame" else not same
            return KeywordResult(Status.PASS if passed else Status.FAIL)
        if normalized == "gettimestamp":
            if len(arguments) not in {1, 2}:
                raise ValueError("Get Time Stamp expects 1 or 2 arguments")
            format_string = arguments[1] if len(arguments) == 2 else "yyyyMMdd_HHmmss"
            timestamp = datetime.now().strftime(self._strftime_format(format_string))
            self.variables[arguments[0]] = timestamp
            return KeywordResult(Status.PASS, f"{arguments[0]} = {timestamp}")
        if normalized == "gettimedifference":
            self._require_arguments(keyword, arguments, 3)
            try:
                first = self._parse_datetime(arguments[0])
                second = self._parse_datetime(arguments[1])
                difference = self._format_timespan(second - first)
            except (ValueError, OverflowError, TypeError):
                return KeywordResult(Status.ERROR, "Cannot convert given string into DateTime Format")
            self.variables[arguments[2].strip()] = difference
            return KeywordResult(Status.PASS, f"{arguments[2]} = {difference}")
        if normalized == "getrandomnumber":
            self._require_arguments(keyword, arguments, 3)
            minimum, maximum = int(arguments[0]), int(arguments[1])
            if maximum < minimum:
                raise ValueError("Maximum must be greater than or equal to minimum")
            value = str(random.randint(minimum, maximum))
            self.variables[arguments[2].strip()] = value
            return KeywordResult(Status.PASS, f"{arguments[2]} = {value}")
        if normalized == "trim":
            if len(arguments) not in {1, 2}:
                raise ValueError("Trim expects 1 or 2 arguments")
            variable = arguments[0].strip()
            if variable not in self.variables:
                raise KeyError(f"Variable '{variable}' is not defined")
            value = self.variables[variable]
            value = value.strip() if len(arguments) == 1 else value.strip(arguments[1])
            self.variables[variable] = value
            return KeywordResult(Status.PASS, f"{arguments[0]} = {value}")
        if normalized == "replace":
            self._require_arguments(keyword, arguments, 3)
            variable = arguments[0].strip()
            if variable not in self.variables:
                raise KeyError(f"Variable '{variable}' is not defined")
            value = self.variables[variable].replace(arguments[1], arguments[2])
            self.variables[variable] = value
            return KeywordResult(Status.PASS, f"{arguments[0]} = {value}")
        if normalized == "substring":
            self._require_arguments(keyword, arguments, 4)
            start = int(arguments[1])
            length = int(arguments[2])
            if start < 0 or length < 0 or start > len(arguments[0]):
                raise ValueError("Substring start and length must be non-negative and start must be within the value")
            value = arguments[0][start:start + length]
            self.variables[arguments[3].strip()] = value
            return KeywordResult(
                Status.PASS,
                f"Substring of {arguments[0]} stored in {arguments[3]}: {value}",
            )
        if normalized == "count":
            self._require_arguments(keyword, arguments, 1)
            variable = arguments[0].strip()
            if variable not in self.variables:
                raise KeyError(f"Variable '{variable}' is not defined")
            count = len(self.variables[variable])
            return KeywordResult(Status.PASS, f"Number of characters in {variable}: {count}")
        if normalized == "splitandstoreword":
            self._require_arguments(keyword, arguments, 4)
            delimiter = " " if arguments[1] in {'""', ""} else arguments[1]
            words = [word for word in arguments[0].split(delimiter) if word]
            index = int(arguments[2])
            if index < 0 or index >= len(words):
                raise IndexError(f"Word index {index} is outside the {len(words)} available word(s)")
            value = words[index]
            self.variables[arguments[3].strip()] = value
            return KeywordResult(
                Status.PASS,
                f"Word at index {index} from {arguments[0]} stored in {arguments[3]}: {value}",
            )
        if normalized == "runkeyword":
            self._require_arguments(keyword, arguments, 1)
            nested_statement = _parse_statement(arguments[0], 0)
            nested_result = self._run_statement(nested_statement)
            return KeywordResult(nested_result.status, nested_result.output)
        raise LookupError(f"Built-in keyword '{keyword}' is not implemented")

    def _write_to_dataset(self, column_name: str, value: str) -> KeywordResult:
        if self._active_dataset_row is None:
            return KeywordResult(
                Status.ERROR,
                "Write To Data Set must run inside ForEachRow or ForSelectedRow.",
            )
        path, sheet, table_name, row_number, headers = self._active_dataset_row
        matching_header = next(
            (header for header in headers if header.casefold() == column_name.strip().casefold()),
            None,
        )
        if matching_header is None:
            return KeywordResult(
                Status.ERROR,
                f"Column '{column_name}' was not found. Available columns: {', '.join(headers)}",
            )

        try:
            if path.suffix.casefold() == ".csv":
                with path.open("r", encoding="utf-8-sig", newline="") as stream:
                    reader = csv.DictReader(stream)
                    fieldnames = reader.fieldnames
                    if fieldnames is None:
                        raise ValueError(f"Dataset '{path.name}' has no header row")
                    rows = list(reader)
                data_index = row_number - 2
                if data_index < 0 or data_index >= len(rows):
                    raise ValueError(f"Dataset row {row_number} is outside '{path.name}'")
                original_header = next(
                    (header for header in fieldnames if re.sub(r"\s+", "", header).casefold() == matching_header.casefold()),
                    None,
                )
                if original_header is None:
                    raise ValueError(f"Column '{column_name}' is not present in '{path.name}'")
                rows[data_index][original_header] = value
                with path.open("w", encoding="utf-8", newline="") as stream:
                    writer = csv.DictWriter(stream, fieldnames=fieldnames)
                    writer.writeheader()
                    writer.writerows(rows)
                return KeywordResult(Status.PASS)

            from openpyxl import load_workbook
            from openpyxl.utils.cell import range_boundaries

            workbook = load_workbook(path)
            try:
                worksheet = workbook[sheet]
                if table_name not in worksheet.tables:
                    raise KeyError(f"Table '{table_name}' was not found in worksheet '{sheet}'")
                min_col, min_row, max_col, max_row = range_boundaries(worksheet.tables[table_name].ref)
                if row_number <= min_row or row_number > max_row:
                    raise ValueError(f"Dataset row {row_number} is outside table '{table_name}'")
                header_columns = {
                    re.sub(r"\s+", "", str(worksheet.cell(min_row, column).value or "")).casefold(): column
                    for column in range(min_col, max_col + 1)
                }
                column_number = header_columns.get(matching_header.casefold())
                if column_number is None:
                    raise ValueError(f"Column '{column_name}' was not found in table '{table_name}'")
                worksheet.cell(row_number, column_number).value = value
                workbook.save(path)
            finally:
                workbook.close()
        except (OSError, ValueError, ImportError, KeyError) as error:
            return KeywordResult(Status.ERROR, f"Could not write dataset value: {error}")
        return KeywordResult(Status.PASS)

    def _dataset_row_context(
        self,
        dataset: str,
        sheet: str,
        table: str,
        data_index: int,
        row: dict[str, str],
    ) -> tuple[Path, str, str, int, dict[str, str]]:
        if self._suite is None:
            raise RuntimeError("No test suite is active")
        dataset_path = next(
            Path(path)
            for name, path in self._suite.datasets.items()
            if name.casefold() == dataset.casefold()
        )
        if dataset_path.suffix.casefold() == ".csv":
            row_number = data_index + 2
        else:
            from openpyxl import load_workbook
            from openpyxl.utils.cell import range_boundaries

            workbook = load_workbook(dataset_path, read_only=False, data_only=True)
            try:
                worksheet = workbook[sheet]
                if table not in worksheet.tables:
                    raise KeyError(f"Table '{table}' was not found in worksheet '{sheet}'")
                _, min_row, _, _ = range_boundaries(worksheet.tables[table].ref)
                row_number = min_row + data_index + 1
            finally:
                workbook.close()
        return dataset_path, sheet, table, row_number, row

    def _resolve_path(self, value: str, *, for_output: bool = False) -> Path:
        path = Path(value)
        if path.is_absolute():
            return path
        root = self.output_folder if for_output else Path(self.variables["${SCRIPT_FOLDER}"])
        return root / path

    @staticmethod
    def _strftime_format(format_string: str) -> str:
        tokens = {
            "yyyy": "%Y",
            "yy": "%y",
            "MMMM": "%B",
            "MMM": "%b",
            "MM": "%m",
            "M": "%m",
            "dddd": "%A",
            "ddd": "%a",
            "dd": "%d",
            "d": "%d",
            "HH": "%H",
            "H": "%H",
            "hh": "%I",
            "h": "%I",
            "mm": "%M",
            "m": "%M",
            "ss": "%S",
            "s": "%S",
            "tt": "%p",
        }
        pattern = re.compile("|".join(sorted(tokens, key=len, reverse=True)))
        return pattern.sub(lambda match: tokens[match.group(0)], format_string)

    @staticmethod
    def _parse_datetime(value: str) -> datetime:
        value = value.strip()
        try:
            return datetime.fromisoformat(value)
        except ValueError:
            pass
        for date_format in (
            "%H:%M:%S",
            "%H:%M",
            "%m/%d/%Y %H:%M:%S",
            "%m/%d/%Y %H:%M",
            "%m/%d/%Y",
            "%Y-%m-%d %H:%M:%S",
            "%Y-%m-%d %H:%M",
            "%Y-%m-%d",
        ):
            try:
                return datetime.strptime(value, date_format)
            except ValueError:
                continue
        raise ValueError("Cannot convert given string into DateTime Format")

    @staticmethod
    def _format_timespan(value: timedelta) -> str:
        total_microseconds = int(value.total_seconds() * 1_000_000)
        sign = "-" if total_microseconds < 0 else ""
        remaining = abs(total_microseconds)
        days, remaining = divmod(remaining, 86_400_000_000)
        hours, remaining = divmod(remaining, 3_600_000_000)
        minutes, remaining = divmod(remaining, 60_000_000)
        seconds, microseconds = divmod(remaining, 1_000_000)
        prefix = f"{days}." if days else ""
        fractional = f".{microseconds:06d}".rstrip("0") if microseconds else ""
        return f"{sign}{prefix}{hours:02}:{minutes:02}:{seconds:02}{fractional}"

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
        try:
            dynamic_method = getattr(library, keyword)
        except AttributeError:
            return None
        if callable(dynamic_method):
            return dynamic_method
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
            if self._flow in {"case", "suite", "custom-return"}:
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
            if self._flow in {"case", "suite", "custom-return"}:
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
            if self._flow in {"case", "suite", "custom-return"}:
                break
            if self._flow == "break":
                self._flow = ""
                break
            if self._flow == "continue":
                self._flow = ""
        return status

    def _run_each_row(self, block: ForEachRowBlock, results: list[StepResult]) -> Status:
        rows, error = self._get_dataset_rows(block.dataset, block.sheet, block.table, block.line_number)
        if error is not None:
            results.append(error)
            return Status.ERROR

        status = Status.PASS
        for index, row in enumerate(rows, start=1):
            for header, value in row.items():
                self.variables[f"${{{header}}}"] = value
            self.variables["${R}"] = str(index)
            previous_context = self._active_dataset_row
            try:
                self._active_dataset_row = self._dataset_row_context(
                    block.dataset, block.sheet, block.table, index - 1, row
                )
                status = self._aggregate((status, self._run_items(block.body, results)))
            except (OSError, ValueError, ImportError, KeyError, RuntimeError) as error:
                results.append(
                    StepResult("ForEachRow", block.line_number, Status.ERROR, str(error))
                )
                return Status.ERROR
            finally:
                self._active_dataset_row = previous_context
            if self._flow in {"case", "suite", "custom-return"}:
                break
            if self._flow == "break":
                self._flow = ""
                break
            if self._flow == "continue":
                self._flow = ""
        return status

    def _run_selected_rows(self, block: ForSelectedRowBlock, results: list[StepResult]) -> Status:
        rows, error = self._get_dataset_rows(block.dataset, block.sheet, block.table, block.line_number)
        if error is not None:
            results.append(error)
            return Status.ERROR

        status = Status.PASS
        for index, row in enumerate(rows):
            if not self._matches_filters(row, block.filters):
                continue
            for header, value in row.items():
                self.variables[f"${{{header}}}"] = value
            self.variables["${R}"] = str(index + 1)
            previous_context = self._active_dataset_row
            try:
                self._active_dataset_row = self._dataset_row_context(
                    block.dataset, block.sheet, block.table, index, row
                )
                status = self._aggregate((status, self._run_items(block.body, results)))
            except (OSError, ValueError, ImportError, KeyError, RuntimeError) as error:
                results.append(
                    StepResult("ForSelectedRow", block.line_number, Status.ERROR, str(error))
                )
                return Status.ERROR
            finally:
                self._active_dataset_row = previous_context
            if self._flow in {"case", "suite", "custom-return"}:
                break
            if self._flow == "break":
                self._flow = ""
                break
            if self._flow == "continue":
                self._flow = ""
        return status

    def _get_dataset_rows(
        self,
        dataset: str,
        sheet: str,
        table: str,
        line_number: int,
    ) -> tuple[list[dict[str, str]], StepResult | None]:
        if self._suite is None or dataset.casefold() not in {
            name.casefold() for name in self._suite.datasets
        }:
            return [], StepResult(
                keyword="ForEachRow",
                line_number=line_number,
                status=Status.ERROR,
                output=f"Dataset '{dataset}' is not declared",
            )

        dataset_path = next(
            path
            for name, path in self._suite.datasets.items()
            if name.casefold() == dataset.casefold()
        )
        try:
            return self._load_rows(Path(dataset_path), sheet, table), None
        except (OSError, ValueError, ImportError, KeyError) as error:
            return [], StepResult(
                keyword="ForEachRow",
                line_number=line_number,
                status=Status.ERROR,
                output=str(error),
            )

    @staticmethod
    def _matches_filters(
        row: dict[str, str],
        filters: dict[str, tuple[str, list[str]]],
    ) -> bool:
        normalized_row = {key.casefold(): value for key, value in row.items()}
        for column, (operator, expected_values) in filters.items():
            actual = normalized_row.get(column.casefold())
            if actual is None:
                return False
            if operator in {"=", "!="}:
                matched = actual.casefold() in {value.casefold() for value in expected_values}
                if matched != (operator == "="):
                    return False
                continue
            matched = False
            for expected in expected_values:
                try:
                    left, right = float(actual), float(expected)
                except ValueError:
                    left, right = actual, expected
                if operator == ">" and left > right:
                    matched = True
                elif operator == "<" and left < right:
                    matched = True
                elif operator == ">=" and left >= right:
                    matched = True
                elif operator == "<=" and left <= right:
                    matched = True
            if not matched:
                return False
        return True

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

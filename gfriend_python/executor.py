from __future__ import annotations

import re
import time
import csv
import random
import os
import platform
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
    RemoteBlock,
    Runnable,
    Status,
    Statement,
    StepResult,
    TestCaseResult,
    TestSuite,
    WhileBlock,
)
from .parser import ParseError, _parse_statement, parse_file
from .json_library import JsonLibrary
from .info_library import InfoLibrary
from .log_info_library import LogInfoLibrary
from .regex_library import RegexLibrary
from .spreadsheet_library import SpreadSheetLibrary
from .xml_library import XmlLibrary

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
        user_interaction: Callable[[str, bool], KeywordResult] | None = None,
        remote_runner: Callable[[str, list[Runnable], dict[str, str]], KeywordResult] | None = None,
    ) -> None:
        self.variables: dict[str, str] = {}
        self._execution_variables: dict[str, str] = {}
        self._libraries: dict[str, object] = {}
        self._sleep = sleep
        self._user_interaction = user_interaction
        self._remote_runner = remote_runner
        self._suite: TestSuite | None = None
        self.output_folder = Path(output_folder).resolve() if output_folder else Path.cwd()
        self._flow = ""
        self._custom_return_status = Status.PASS
        self._last_repeat_result = (0, 0, 0)
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
            elif library.casefold() == "regex":
                self.register_library("Regex", RegexLibrary(self.variables))
            elif library.casefold() == "json":
                self.register_library("Json", JsonLibrary(self.variables))
            elif library.casefold() == "xml":
                self.register_library(
                    "XML",
                    XmlLibrary(
                        self.variables,
                        script_folder=suite.script_folder,
                    ),
                )
            elif library.casefold() == "info":
                self.register_library(
                    "Info",
                    InfoLibrary(self.variables, self.output_folder),
                )
            elif library.casefold() == "loginfo":
                self.register_library(
                    "LogInfo",
                    LogInfoLibrary(self.variables, self.output_folder),
                )
            elif library.casefold() == "spreadsheet":
                self.register_library(
                    "SpreadSheet",
                    SpreadSheetLibrary(
                        self.variables,
                        script_folder=suite.script_folder,
                        output_folder=self.output_folder,
                    ),
                )

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
            elif isinstance(item, RemoteBlock):
                remote_result = self._run_remote(item)
                results.append(remote_result)
                status = self._aggregate((status, remote_result.status))
            else:
                block_status = self._run_conditional(item, results)
                if item.always_pass:
                    block_status = Status.PASS
                status = self._aggregate((status, block_status))
            if self._flow in {"case", "suite", "break", "continue", "custom-return"}:
                break
        return status

    def _run_remote(self, block: RemoteBlock) -> StepResult:
        if self._remote_runner is None:
            result = KeywordResult(
                Status.ERROR,
                "Remote execution requires a configured remote_runner adapter",
            )
        else:
            try:
                result = self._remote_runner(
                    self._expand(block.remote_id),
                    block.body,
                    self.variables.copy(),
                )
                if not isinstance(result, KeywordResult):
                    raise TypeError("remote_runner must return a KeywordResult")
            except Exception as error:
                result = KeywordResult(Status.ERROR, str(error))
        return StepResult(
            keyword=f"RemoteRun({block.remote_id})",
            line_number=block.line_number,
            status=Status.PASS if block.always_pass else result.status,
            output=result.output,
        )

    def _run_statement(self, statement: Statement) -> StepResult:
        keyword_name = _normalize(statement.keyword)
        library_name = statement.library.casefold()
        if self._suite:
            for alias, library in self._suite.libraries.items():
                if alias.casefold() == library_name:
                    library_name = library.casefold()
                    if library.casefold().endswith(".gflib"):
                        library_name = alias.casefold()
                    break
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
            "converttimestampformat": {0},
            "concatenatevariablesandstoreresult": {0, 1, 2},
            "concatenatevariablevaluesandstoreresult": {2},
            "getsysteminformation": {0},
            "getram": {0},
            "getwindowsversion": {0},
        }.get(keyword_name, set())
        if library_name == "regex":
            if keyword_name == "getmatch":
                variable_arguments = {2} if len(statement.arguments) == 3 else {1}
            elif keyword_name == "getmatchcount":
                variable_arguments = {1}
        elif library_name == "json":
            if keyword_name == "getvalue":
                variable_arguments = {0}
            elif keyword_name == "addjsonelement":
                variable_arguments = {0}
        elif library_name == "xml" and keyword_name in {
            "getattribute",
            "getinnertext",
        }:
            variable_arguments = {0}
        elif library_name == "spreadsheet":
            variable_arguments = {
                "totalrowcount": {0},
                "totalcolumncount": {0},
                "readcellvalue": {2},
                "readcellvaluebyindex": {2},
                "readlastcolumn": {0},
                "readlastrow": {0},
                "readvaluesbycolumnnames": {0},
                "readvaluesbyrownames": {0},
                "readrowdata": {0},
                "readcolumndata": {0},
                "calculateformulafromcells": {5},
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
        if normalized == "hasvalueincreased":
            self._require_arguments(keyword, arguments, 2)
            if not arguments[0]:
                return KeywordResult(Status.FAIL, "oldValue cannot be null or empty.")
            if not arguments[1]:
                return KeywordResult(Status.FAIL, "newValue cannot be null or empty.")
            try:
                old_value = float(arguments[0])
            except ValueError:
                return KeywordResult(Status.FAIL, f"Invalid oldValue: '{arguments[0]}' is not a valid number.")
            try:
                new_value = float(arguments[1])
            except ValueError:
                return KeywordResult(Status.FAIL, f"Invalid newValue: '{arguments[1]}' is not a valid number.")
            if new_value > old_value:
                return KeywordResult(Status.PASS, f"Value increased: {new_value} > {old_value}")
            return KeywordResult(Status.FAIL, f"Value not increased: {new_value} <= {old_value}")
        if normalized == "assertlastrepeat":
            self._require_arguments(keyword, arguments, 2)
            result_name = arguments[0].strip().upper()
            result_index = {"PASS": 0, "FAIL": 1, "ERROR": 2}.get(result_name)
            if result_index is None:
                return KeywordResult(Status.ERROR, f"{arguments[0].strip()} is unknown result type.")
            condition = arguments[1].strip()
            match = re.fullmatch(r"(>=|<=|>|<|=)\s*(\d+)", condition)
            if match is None:
                return KeywordResult(
                    Status.ERROR,
                    f"Can not find threshold number in the condition: {condition}",
                )
            operator, raw_threshold = match.groups()
            threshold = int(raw_threshold)
            count = self._last_repeat_result[result_index]
            passed = {
                ">": count > threshold,
                "<": count < threshold,
                ">=": count >= threshold,
                "<=": count <= threshold,
                "=": count == threshold,
            }[operator]
            return KeywordResult(
                Status.PASS if passed else Status.FAIL,
                f"{result_name} count of last repeat is {count}.",
            )
        if normalized == "log":
            self._require_arguments(keyword, arguments, 1)
            return KeywordResult(Status.PASS, arguments[0])
        if normalized in {"waitforuserconfirm", "requestuserverification"}:
            self._require_arguments(keyword, arguments, 1)
            if self._user_interaction is None:
                return KeywordResult(
                    Status.ERROR,
                    f"{keyword} requires an interactive user callback.",
                )
            result = self._user_interaction(
                arguments[0],
                normalized == "requestuserverification",
            )
            if not isinstance(result, KeywordResult):
                raise TypeError("User interaction callback must return KeywordResult")
            return result
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
        if normalized == "writetodatasetadvanced":
            if len(arguments) not in {4, 5}:
                raise ValueError(
                    "Write To Data Set Advanced expects 4 arguments, or 5 with a dataset alias"
                )
            sheet_name, row_value, column_letter, value = arguments[:4]
            dataset = arguments[4] if len(arguments) == 5 else None
            return self._write_to_dataset_advanced(
                sheet_name, row_value, column_letter, value, dataset
            )
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
        if normalized == "getsysteminformation":
            self._require_arguments(keyword, arguments, 1)
            cpu_info = platform.processor().strip() or platform.machine().strip()
            if not cpu_info:
                return KeywordResult(Status.ERROR, "Could not determine processor information.")
            self.variables[arguments[0].strip()] = cpu_info
            return KeywordResult(Status.PASS, f"System Information: {cpu_info}")
        if normalized == "getram":
            self._require_arguments(keyword, arguments, 1)
            total_gigabytes = self._total_memory_gigabytes()
            if total_gigabytes is None:
                return KeywordResult(Status.ERROR, "Could not determine total physical memory.")
            rounded = round(total_gigabytes)
            self.variables[arguments[0].strip()] = f"RAM:{rounded}GB"
            return KeywordResult(Status.PASS, f"RAM Size: {rounded} GB")
        if normalized == "getwindowsversion":
            self._require_arguments(keyword, arguments, 1)
            if platform.system() != "Windows":
                return KeywordResult(Status.ERROR, "Get Windows Version is only available on Windows.")
            windows_version = self._windows_version()
            if not windows_version:
                return KeywordResult(Status.ERROR, "Could not determine Windows edition and version.")
            self.variables[arguments[0].strip()] = windows_version
            return KeywordResult(Status.PASS, f"Windows Version: {windows_version}")
        if normalized == "converttimestampformat":
            self._require_arguments(keyword, arguments, 3)
            variable = arguments[0].strip()
            if variable not in self.variables:
                raise KeyError(f"Variable '{variable}' is not defined")
            source_format = self._strftime_format(arguments[1])
            target_format = self._strftime_format(arguments[2])
            try:
                value = datetime.strptime(self.variables[variable], source_format).strftime(target_format)
            except ValueError as error:
                return KeywordResult(Status.ERROR, f"Could not convert timestamp: {error}")
            self.variables[variable] = value
            return KeywordResult(Status.PASS, f"{variable} = {value}")
        if normalized == "concatenatevariablesandstoreresult":
            self._require_arguments(keyword, arguments, 3)
            variable_suffix = "".join(
                argument.replace("$", "").replace("{", "").replace("}", "")
                for argument in arguments[:2]
            )
            value = self.variables.get(f"${{{variable_suffix}}}", "")
            if not value:
                return KeywordResult(Status.PASS, f"{arguments[2]} : {variable_suffix}")
            self.variables[arguments[2].strip()] = value
            return KeywordResult(Status.PASS, f"{arguments[2]} : {value}")
        if normalized == "concatenatevariablevaluesandstoreresult":
            self._require_arguments(keyword, arguments, 3)
            if not arguments[0]:
                return KeywordResult(Status.FAIL, "First string value cannot be null or empty.")
            if not arguments[1]:
                return KeywordResult(Status.FAIL, "Second string value cannot be null or empty.")
            if not arguments[2]:
                return KeywordResult(Status.FAIL, "Result variable name cannot be null or empty.")
            value = arguments[0] + arguments[1]
            self.variables[arguments[2].strip()] = value
            return KeywordResult(Status.PASS, f"{arguments[2]} = {value}")
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

            workbook = load_workbook(path)
            try:
                worksheet = workbook[sheet]
                if table_name:
                    from openpyxl.utils.cell import range_boundaries

                    if table_name not in worksheet.tables:
                        raise KeyError(f"Table '{table_name}' was not found in worksheet '{sheet}'")
                    min_col, min_row, max_col, max_row = range_boundaries(worksheet.tables[table_name].ref)
                    if row_number <= min_row or row_number > max_row:
                        raise ValueError(f"Dataset row {row_number} is outside table '{table_name}'")
                else:
                    min_col, max_col, min_row = 1, worksheet.max_column, 1
                    if row_number <= min_row or row_number > worksheet.max_row:
                        raise ValueError(f"Dataset row {row_number} is outside worksheet '{sheet}'")
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

    def _write_to_dataset_advanced(
        self,
        sheet_name: str,
        row_value: str,
        column_letter: str,
        value: str,
        dataset: str | None,
    ) -> KeywordResult:
        if self._suite is None:
            return KeywordResult(Status.ERROR, "No test suite is active")

        datasets = [
            (name, Path(path))
            for name, path in self._suite.datasets.items()
            if Path(path).suffix.casefold() in {".xlsx", ".xlsm"}
        ]
        if dataset is not None:
            datasets = [
                (name, path)
                for name, path in datasets
                if name.casefold() == dataset.casefold()
            ]
            if not datasets:
                return KeywordResult(
                    Status.ERROR,
                    f"Excel dataset '{dataset}' is not declared",
                )
        elif len(datasets) != 1:
            return KeywordResult(
                Status.ERROR,
                "Write To Data Set Advanced requires one declared Excel dataset "
                "or an explicit dataset alias as the fifth argument.",
            )

        try:
            row_number = int(row_value)
        except ValueError:
            return KeywordResult(
                Status.ERROR,
                f"Invalid row index: {row_value}. Must be a positive number (1-based).",
            )
        if row_number < 1:
            return KeywordResult(
                Status.ERROR,
                f"Invalid row index: {row_value}. Must be a positive number (1-based).",
            )
        if not re.fullmatch(r"[A-Za-z]+", column_letter.strip()):
            return KeywordResult(
                Status.ERROR,
                "Column letter must be an Excel column such as 'A', 'B', or 'AA'.",
            )

        dataset_name, path = datasets[0]
        if not path.is_file():
            return KeywordResult(Status.ERROR, f"Dataset file does not exist: {path}")
        try:
            from openpyxl import load_workbook
            from openpyxl.utils.cell import column_index_from_string

            column_number = column_index_from_string(column_letter.strip().upper())
            workbook = load_workbook(path)
            try:
                if not sheet_name.strip() or sheet_name.strip().casefold() == "active":
                    worksheet = workbook.active
                else:
                    worksheet = next(
                        (
                            workbook[name]
                            for name in workbook.sheetnames
                            if name.casefold() == sheet_name.strip().casefold()
                        ),
                        None,
                    )
                    if worksheet is None:
                        return KeywordResult(
                            Status.ERROR,
                            f"Sheet '{sheet_name}' not found in dataset '{dataset_name}'.",
                        )
                worksheet.cell(row=row_number, column=column_number).value = value
                workbook.save(path)
            finally:
                workbook.close()
        except (OSError, ValueError, ImportError, KeyError) as error:
            return KeywordResult(Status.ERROR, f"Could not write dataset cell: {error}")
        return KeywordResult(Status.PASS)

    def _dataset_row_context(
        self,
        dataset: str,
        sheet: str,
        table: str,
        data_index: int,
        row: dict[str, str],
        *,
        spreadsheet: bool = False,
    ) -> tuple[Path, str, str, int, dict[str, str]]:
        if self._suite is None:
            raise RuntimeError("No test suite is active")
        dataset_path = next(
            Path(path)
            for name, path in self._suite.datasets.items()
            if name.casefold() == dataset.casefold()
        )
        if spreadsheet:
            from openpyxl import load_workbook

            workbook = load_workbook(dataset_path, read_only=True, data_only=True)
            try:
                worksheet = workbook[sheet]
                row_number = data_index + 2
            finally:
                workbook.close()
        elif dataset_path.suffix.casefold() == ".csv":
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
    def _total_memory_gigabytes() -> float | None:
        if platform.system() == "Windows":
            import ctypes

            class MemoryStatus(ctypes.Structure):
                _fields_ = [
                    ("dwLength", ctypes.c_ulong),
                    ("dwMemoryLoad", ctypes.c_ulong),
                    ("ullTotalPhys", ctypes.c_ulonglong),
                    ("ullAvailPhys", ctypes.c_ulonglong),
                    ("ullTotalPageFile", ctypes.c_ulonglong),
                    ("ullAvailPageFile", ctypes.c_ulonglong),
                    ("ullTotalVirtual", ctypes.c_ulonglong),
                    ("ullAvailVirtual", ctypes.c_ulonglong),
                    ("ullAvailExtendedVirtual", ctypes.c_ulonglong),
                ]

            status = MemoryStatus()
            status.dwLength = ctypes.sizeof(status)
            if not ctypes.windll.kernel32.GlobalMemoryStatusEx(ctypes.byref(status)):
                return None
            return status.ullTotalPhys / (1024**3)

        try:
            page_size = os.sysconf("SC_PAGE_SIZE")
            page_count = os.sysconf("SC_PHYS_PAGES")
        except (AttributeError, OSError, ValueError):
            return None
        return page_size * page_count / (1024**3)

    @staticmethod
    def _windows_version() -> str:
        try:
            import winreg

            with winreg.OpenKey(
                winreg.HKEY_LOCAL_MACHINE,
                r"SOFTWARE\Microsoft\Windows NT\CurrentVersion",
            ) as key:
                product_name = winreg.QueryValueEx(key, "ProductName")[0]
                try:
                    display_version = winreg.QueryValueEx(key, "DisplayVersion")[0]
                except OSError:
                    display_version = winreg.QueryValueEx(key, "ReleaseId")[0]
            return f"{product_name} {display_version}".strip()
        except (ImportError, OSError):
            version = platform.win32_ver()
            return " ".join(part for part in version if part)

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
        iteration_statuses: list[Status] = []
        for iteration in range(1, count + 1):
            self.variables["${R}"] = str(iteration)
            iteration_status = self._run_items(block.body, results)
            iteration_statuses.append(iteration_status)
            status = self._aggregate((status, iteration_status))
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
        self._set_last_repeat_result(iteration_statuses)
        return status

    def _run_for(self, block: ForBlock, results: list[StepResult]) -> Status:
        status = Status.PASS
        iteration_statuses: list[Status] = []
        previous_item = self.variables.get("${ITEM}")
        for index, value in enumerate(block.values):
            self.variables["${ITEM}"] = self._expand(value)
            self.variables["${R}"] = str(index + 1)
            iteration_status = self._run_items(block.body, results)
            iteration_statuses.append(iteration_status)
            status = self._aggregate((status, iteration_status))
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
        self._set_last_repeat_result(iteration_statuses)
        return status

    def _run_while(self, block: WhileBlock, results: list[StepResult]) -> Status:
        try:
            max_iterations = int(self._expand(block.max_iterations))
        except ValueError:
            return Status.ERROR
        if max_iterations < 0:
            return Status.ERROR

        status = Status.PASS
        iteration_statuses: list[Status] = []
        for iteration in range(1, max_iterations + 1):
            self.variables["${R}"] = str(iteration)
            condition = self._run_statement(block.condition)
            results.append(condition)
            if condition.status is Status.ERROR:
                return Status.ERROR
            if (condition.status is Status.PASS) != block.while_pass:
                break
            iteration_status = self._run_items(block.body, results)
            iteration_statuses.append(iteration_status)
            status = self._aggregate((status, iteration_status))
            if self._flow in {"case", "suite", "custom-return"}:
                break
            if self._flow == "break":
                self._flow = ""
                break
            if self._flow == "continue":
                self._flow = ""
        self._set_last_repeat_result(iteration_statuses)
        return status

    def _run_each_row(self, block: ForEachRowBlock, results: list[StepResult]) -> Status:
        rows, error = self._get_dataset_rows(
            block.dataset, block.sheet, block.table, block.line_number, spreadsheet=block.spreadsheet
        )
        if error is not None:
            results.append(error)
            return Status.ERROR

        status = Status.PASS
        iteration_statuses: list[Status] = []
        for index, row in enumerate(rows, start=1):
            for header, value in row.items():
                self.variables[f"${{{header}}}"] = value
            self.variables["${R}"] = str(index)
            previous_context = self._active_dataset_row
            try:
                self._active_dataset_row = self._dataset_row_context(
                    block.dataset,
                    block.sheet,
                    "" if block.spreadsheet else block.table,
                    index - 1,
                    row,
                    spreadsheet=block.spreadsheet,
                )
                iteration_status = self._run_items(block.body, results)
                iteration_statuses.append(iteration_status)
                status = self._aggregate((status, iteration_status))
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
        self._set_last_repeat_result(iteration_statuses)
        return status

    def _run_selected_rows(self, block: ForSelectedRowBlock, results: list[StepResult]) -> Status:
        rows, error = self._get_dataset_rows(block.dataset, block.sheet, block.table, block.line_number)
        if error is not None:
            results.append(error)
            return Status.ERROR

        status = Status.PASS
        iteration_statuses: list[Status] = []
        expanded_filters = {
            column: (operator, [self._expand(value) for value in values])
            for column, (operator, values) in block.filters.items()
        }
        for index, row in enumerate(rows):
            if not self._matches_filters(row, expanded_filters):
                continue
            for header, value in row.items():
                self.variables[f"${{{header}}}"] = value
            self.variables["${R}"] = str(index + 1)
            previous_context = self._active_dataset_row
            try:
                self._active_dataset_row = self._dataset_row_context(
                    block.dataset, block.sheet, block.table, index, row
                )
                iteration_status = self._run_items(block.body, results)
                iteration_statuses.append(iteration_status)
                status = self._aggregate((status, iteration_status))
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
        self._set_last_repeat_result(iteration_statuses)
        return status

    def _get_dataset_rows(
        self,
        dataset: str,
        sheet: str,
        table: str,
        line_number: int,
        *,
        spreadsheet: bool = False,
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
            return self._load_rows(Path(dataset_path), sheet, table, spreadsheet=spreadsheet), None
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
            if operator in {"=", "==", "!="}:
                try:
                    actual_number = float(actual)
                    expected_numbers = [float(value) for value in expected_values]
                except ValueError:
                    actual_number = None
                    expected_numbers = []
                if actual_number is None:
                    matched = actual in expected_values
                else:
                    matched = actual_number in expected_numbers
                if operator == "!=":
                    if matched:
                        return False
                elif not matched:
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
    def _load_rows(
        path: Path,
        sheet_name: str,
        table_name: str,
        *,
        spreadsheet: bool = False,
    ) -> list[dict[str, str]]:
        if not path.is_file():
            raise FileNotFoundError(f"Dataset file does not exist: {path}")
        if path.suffix.casefold() == ".csv":
            if spreadsheet:
                raise ValueError("SpreadsheetForEachRow requires an Excel dataset")
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
            if spreadsheet:
                min_col, max_col, min_row, max_row = 1, worksheet.max_column, 1, worksheet.max_row
            else:
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

    def _set_last_repeat_result(self, statuses: list[Status]) -> None:
        self._last_repeat_result = (
            statuses.count(Status.PASS),
            statuses.count(Status.FAIL),
            statuses.count(Status.ERROR),
        )

from __future__ import annotations

import shlex

from .models import KeywordResult, Status


class LpLibrary:
    """Build CUPS lp commands without executing them."""

    def __init__(self, variables: dict[str, str]) -> None:
        self._variables = variables
        self._file = ""
        self._destination = ""
        self._copies = -1
        self._options: list[tuple[str, str]] = []
        self._job_selected = False

    def select_file(self, file_name: str) -> KeywordResult:
        if not file_name.strip():
            return KeywordResult(Status.ERROR, "File name cannot be empty.")
        self._file = file_name
        self._destination = ""
        self._copies = -1
        self._options = []
        self._job_selected = True
        return KeywordResult(Status.PASS)

    def select_printer(self, printer_name: str) -> KeywordResult:
        if not self._job_selected:
            return KeywordResult(Status.ERROR, "File selection should be done before selecting printer.")
        self._destination = printer_name.strip()
        return KeywordResult(Status.PASS)

    def set_copies(self, copies: str) -> KeywordResult:
        if not self._job_selected:
            return KeywordResult(Status.ERROR, "File selection should be done before setting options.")
        try:
            self._copies = int(copies.strip())
        except ValueError:
            return KeywordResult(Status.ERROR, "Copies must be a number.")
        return KeywordResult(Status.PASS)

    def set_option(self, option: str, value: str = "") -> KeywordResult:
        if not self._job_selected:
            return KeywordResult(Status.ERROR, "File selection should be done before setting options.")
        if not option.strip():
            return KeywordResult(Status.ERROR, "Option name cannot be empty.")
        self._options.append((option.strip(), value.strip()))
        return KeywordResult(Status.PASS)

    def get_lp_command(self, variable: str) -> KeywordResult:
        if not self._job_selected:
            return KeywordResult(Status.ERROR, "Print job is not created.")
        parts = ["lp"]
        if self._destination:
            parts.extend(["-d", self._destination])
        if self._copies > 0:
            parts.extend(["-n", str(self._copies)])
        for option, value in self._options:
            parts.extend(["-o", f"{option}={value}" if value else option])
        parts.append(self._file)
        command = " ".join(shlex.quote(part) for part in parts)
        if not variable.strip():
            return KeywordResult(Status.FAIL, "A variable name must be provided to save the command.")
        self._variables[self._variable_key(variable)] = command
        return KeywordResult(Status.PASS, command)

    @staticmethod
    def _variable_key(name: str) -> str:
        name = name.strip()
        if name.startswith("${") and name.endswith("}"):
            return name
        return f"${{{name.strip('${}')}}}"

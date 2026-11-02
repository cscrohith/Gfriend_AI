from __future__ import annotations

import re
from typing import Pattern

from .models import KeywordResult, Status


class RegexLibrary:
    """Implements the stateful GFriend Regex keyword library."""

    def __init__(self, variables: dict[str, str]) -> None:
        self._variables = variables
        self._pattern: Pattern[str] | None = None

    def set_pattern(self, pattern: str) -> KeywordResult:
        try:
            self._pattern = re.compile(pattern.replace(r"\z", r"\Z"))
        except re.error:
            self._pattern = None
            return KeywordResult(Status.ERROR, "Can not initialize regex.")
        return KeywordResult(Status.PASS)

    def is_match(self, value: str) -> KeywordResult:
        if self._pattern is None:
            return self._pattern_error()
        match = self._pattern.search(value)
        if match is None:
            return KeywordResult(
                Status.FAIL,
                "Can not find string with given input and pattern.",
            )
        return KeywordResult(Status.PASS, match.group(0))

    def get_match(self, *arguments: str) -> KeywordResult:
        if len(arguments) == 2:
            value, save_to = arguments
            index = 0
        elif len(arguments) == 3:
            value, raw_index, save_to = arguments
            try:
                index = int(raw_index)
            except ValueError:
                return KeywordResult(Status.ERROR, "Index must be a number")
        else:
            return KeywordResult(
                Status.ERROR,
                f"Get Match expects 2 or 3 argument(s); got {len(arguments)}",
            )
        if index < 0:
            return KeywordResult(Status.ERROR, "Match index is outside the available matches.")
        if self._pattern is None:
            return self._pattern_error()
        matches = list(self._pattern.finditer(value))
        if not matches:
            return KeywordResult(
                Status.FAIL,
                "Can not find string with given input and pattern.",
            )
        try:
            result = matches[index].group(0)
        except IndexError:
            return KeywordResult(Status.ERROR, "Match index is outside the available matches.")
        self._variables[self._variable_key(save_to)] = result
        return KeywordResult(Status.PASS, result)

    def get_match_count(self, value: str, save_to: str) -> KeywordResult:
        if self._pattern is None:
            return self._pattern_error()
        count = sum(1 for _ in self._pattern.finditer(value))
        self._variables[self._variable_key(save_to)] = str(count)
        return KeywordResult(Status.PASS, f"Matched count : {count}")

    @staticmethod
    def _variable_key(name: str) -> str:
        name = name.strip()
        if name.startswith("${") and name.endswith("}"):
            return name
        return f"${{{name.strip('${}')}}}"

    @staticmethod
    def _pattern_error() -> KeywordResult:
        return KeywordResult(
            Status.ERROR,
            "Pattern is not set. Use Set Pattern keyword first.",
        )

from __future__ import annotations

import json
import re
from typing import Any

from .models import KeywordResult, Status

_PATH_TOKEN = re.compile(
    r"\.([A-Za-z_][A-Za-z0-9_-]*)"
    r"|\[(?:([0-9]+)|'([^']*)'|\"([^\"]*)\")\]"
)


class JsonLibrary:
    """Implements the stateful GFriend Json keyword library."""

    def __init__(self, variables: dict[str, str]) -> None:
        self._variables = variables
        self._document: dict[str, Any] | list[Any] | None = None

    def load_json(self, json_text: str) -> KeywordResult:
        try:
            document = json.loads(json_text)
        except (json.JSONDecodeError, TypeError):
            self._document = None
            return KeywordResult(Status.ERROR, "Json parsing error.")
        if not isinstance(document, (dict, list)):
            self._document = None
            return KeywordResult(Status.ERROR, "Json parsing error.")
        self._document = document
        return KeywordResult(Status.PASS)

    def get_value(self, save_to: str, json_path: str) -> KeywordResult:
        if self._document is None:
            return KeywordResult(
                Status.ERROR,
                "Json is not loaded. Use Load Json keyword before using Get Value.",
            )
        try:
            value = self._select(self._document, json_path)
        except (KeyError, IndexError, ValueError, TypeError):
            return KeywordResult(
                Status.ERROR,
                f"Can not find item with given json path ({json_path}) in json string.",
            )
        rendered = self._render(value)
        self._variables[self._variable_key(save_to)] = rendered
        return KeywordResult(Status.PASS, rendered)

    def add_json_element(self, target: str, name: str, value: str) -> KeywordResult:
        variable = self._variable_key(target)
        raw_document = self._variables.get(variable, "")
        if raw_document:
            try:
                document = json.loads(raw_document)
            except json.JSONDecodeError:
                return KeywordResult(
                    Status.ERROR,
                    f"Target variable is not a JSON formatted string: {raw_document}",
                )
            if not isinstance(document, dict):
                return KeywordResult(
                    Status.ERROR,
                    "Target variable must contain a JSON object.",
                )
        else:
            document = {}

        if name in document:
            return KeywordResult(Status.ERROR, f"JSON property '{name}' already exists.")
        try:
            parsed_value = json.loads(value)
        except json.JSONDecodeError:
            parsed_value = value
        document[name] = parsed_value
        rendered = json.dumps(document, ensure_ascii=False, indent=2)
        self._variables[variable] = rendered
        return KeywordResult(Status.PASS, rendered)

    @staticmethod
    def _select(document: Any, path: str) -> Any:
        if path == "$":
            return document
        if not path.startswith("$"):
            raise ValueError("JSON paths must start with '$'")
        current = document
        remaining = path[1:]
        while remaining:
            match = _PATH_TOKEN.match(remaining)
            if match is None:
                raise ValueError("Unsupported JSON path syntax")
            property_name, index_text, single_quoted, double_quoted = match.groups()
            token: str | int
            if property_name is not None:
                token = property_name
            elif index_text is not None:
                token = int(index_text)
            else:
                token = single_quoted if single_quoted is not None else double_quoted
            if isinstance(token, int):
                if not isinstance(current, list):
                    raise TypeError("Array index used on a non-array value")
                current = current[token]
            else:
                if not isinstance(current, dict):
                    raise TypeError("Object property used on a non-object value")
                current = current[token]
            remaining = remaining[match.end():]
        return current

    @staticmethod
    def _render(value: Any) -> str:
        if isinstance(value, str):
            return value
        if value is None:
            return "null"
        if isinstance(value, (dict, list)):
            return json.dumps(value, ensure_ascii=False, indent=2)
        return json.dumps(value, ensure_ascii=False)

    @staticmethod
    def _variable_key(name: str) -> str:
        name = name.strip()
        if name.startswith("${") and name.endswith("}"):
            return name
        return f"${{{name.strip('${}')}}}"

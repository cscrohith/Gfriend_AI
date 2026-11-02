from __future__ import annotations

import base64
import codecs
from urllib.error import HTTPError, URLError
from urllib.request import Request, urlopen

from .models import KeywordResult, Status


class RestLibrary:
    """HTTP request keywords ported from the legacy GFriend REST library."""

    def __init__(
        self,
        variables: dict[str, str],
        *,
        timeout: float = 30,
    ) -> None:
        self._variables = variables
        self._timeout = timeout
        self._url = ""
        self._method = ""
        self._encoding = "utf-8"
        self._headers: dict[str, str] = {}
        self._body: bytes | None = None

    def initialize_request(
        self,
        url: str,
        method: str,
        encoding: str = "UTF-8",
    ) -> KeywordResult:
        normalized_method = method.strip().upper()
        if normalized_method not in {"GET", "POST", "PUT", "DELETE"}:
            return KeywordResult(Status.ERROR, f"Can not recognize http method : {method}")
        if not url.strip():
            return KeywordResult(Status.ERROR, "Request URL cannot be empty.")
        try:
            normalized_encoding = codecs.lookup(encoding).name
        except LookupError:
            return KeywordResult(
                Status.ERROR,
                "Error during set encoding. Please check if encoding is valid.",
            )
        self._url = url
        self._method = normalized_method
        self._encoding = normalized_encoding
        self._headers = {}
        self._body = None
        return KeywordResult(Status.PASS)

    def disable_security_warning(self) -> KeywordResult:
        return KeywordResult(
            Status.ERROR,
            "Disabling TLS certificate validation is not supported.",
        )

    def enable_security_warning(self) -> KeywordResult:
        return KeywordResult(Status.PASS)

    def add_header(self, name: str, value: str) -> KeywordResult:
        if not self._method:
            return KeywordResult(Status.ERROR, "Initialize the request before adding headers.")
        if not name.strip():
            return KeywordResult(Status.ERROR, "HTTP header name cannot be empty.")
        self._headers[name.strip()] = value
        return KeywordResult(Status.PASS)

    def add_basic_auth(self, username: str, password: str) -> KeywordResult:
        if not self._method:
            return KeywordResult(Status.ERROR, "Initialize the request before adding authentication.")
        try:
            credentials = f"{username}:{password}".encode("iso-8859-1")
        except UnicodeEncodeError as error:
            return KeywordResult(Status.ERROR, f"Basic authentication credentials are not ISO-8859-1: {error}")
        self._headers["Authorization"] = "Basic " + base64.b64encode(credentials).decode("ascii")
        return KeywordResult(Status.PASS)

    def add_json_data(self, json_data: str) -> KeywordResult:
        if not self._method:
            return KeywordResult(Status.ERROR, "Initialize the request before adding request data.")
        try:
            self._body = json_data.encode(self._encoding)
        except (LookupError, UnicodeEncodeError) as error:
            return KeywordResult(Status.ERROR, f"Could not encode request body: {error}")
        self._headers["Content-Type"] = f"application/json; charset={self._encoding}"
        return KeywordResult(Status.PASS)

    def send_request(self, response: str) -> KeywordResult:
        if not self._method:
            return KeywordResult(Status.ERROR, "Initialize the request before sending it.")
        try:
            request = Request(
                self._url,
                data=self._body,
                headers=self._headers,
                method=self._method,
            )
            try:
                with urlopen(request, timeout=self._timeout) as http_response:
                    status_code = http_response.status
                    payload = http_response.read()
            except HTTPError as http_error:
                status_code = http_error.code
                payload = http_error.read()
                http_error.close()
            response_body = payload.decode(self._encoding, errors="replace")
        except (OSError, ValueError, URLError) as error:
            return KeywordResult(Status.ERROR, f"HTTP request failed: {error}")
        if not response.strip():
            return KeywordResult(Status.FAIL, "A variable name must be provided to save the response.")
        self._variables[self._variable_key(response)] = response_body
        return KeywordResult(
            Status.PASS if 200 <= status_code < 300 else Status.FAIL,
            response_body,
        )

    @staticmethod
    def _variable_key(name: str) -> str:
        name = name.strip()
        if name.startswith("${") and name.endswith("}"):
            return name
        return f"${{{name.strip('${}')}}}"

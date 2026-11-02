from __future__ import annotations

import io
import xml.etree.ElementTree as ET
from pathlib import Path

from .models import KeywordResult, Status


class XmlLibrary:
    """Implements the stateful GFriend XML keyword library."""

    def __init__(
        self,
        variables: dict[str, str],
        *,
        script_folder: str = "",
    ) -> None:
        self._variables = variables
        self._script_folder = Path(script_folder) if script_folder else Path.cwd()
        self._root: ET.Element | None = None
        self._namespaces: dict[str, str] = {}

    def load(self, xml: str) -> KeywordResult:
        try:
            self._root, self._namespaces = self._parse(xml)
        except ET.ParseError:
            self._root = None
            self._namespaces = {}
            return KeywordResult(Status.ERROR, "Error with xml")
        return KeywordResult(Status.PASS)

    def load_from_file(self, file_path: str) -> KeywordResult:
        path = Path(file_path)
        if not path.is_absolute():
            path = self._script_folder / path
        try:
            content = path.read_bytes()
        except OSError:
            return KeywordResult(Status.ERROR, f"File not exist : {path}")
        try:
            self._root, self._namespaces = self._parse(content)
        except ET.ParseError:
            self._root = None
            self._namespaces = {}
            return KeywordResult(Status.ERROR, "Error with xml")
        return KeywordResult(Status.PASS)

    def get_attribute(
        self,
        save_to: str,
        xpath: str,
        attribute_name: str,
    ) -> KeywordResult:
        node, failure = self._select_node(xpath)
        if failure is not None:
            return failure
        if node is None:
            return KeywordResult(Status.ERROR, "Can not find xml node with given xpath")
        value = node.get(attribute_name)
        if value is None:
            return KeywordResult(Status.FAIL, "Can not find attribute in given node")
        self._variables[self._variable_key(save_to)] = value
        return KeywordResult(Status.PASS, value)

    def get_inner_text(self, save_to: str, xpath: str) -> KeywordResult:
        node, failure = self._select_node(xpath)
        if failure is not None:
            return failure
        if node is None:
            return KeywordResult(Status.ERROR, "Can not find xml node with given xpath")
        value = "".join(node.itertext())
        self._variables[self._variable_key(save_to)] = value
        return KeywordResult(Status.PASS, value)

    def replace_inner_text(self, new_text: str, xpath: str) -> KeywordResult:
        node, failure = self._select_node(xpath)
        if failure is not None:
            return failure
        if node is None:
            return KeywordResult(Status.ERROR, "Can not find xml node with given xpath")
        previous = "".join(node.itertext())
        for child in list(node):
            node.remove(child)
        node.text = new_text
        return KeywordResult(Status.PASS, previous)

    def append_to_inner_text(self, text_to_append: str, xpath: str) -> KeywordResult:
        node, failure = self._select_node(xpath)
        if failure is not None:
            return failure
        if node is None:
            return KeywordResult(Status.ERROR, "Can not find xml node with given xpath")
        previous = "".join(node.itertext())
        for child in list(node):
            node.remove(child)
        node.text = previous + text_to_append
        return KeywordResult(Status.PASS, previous)

    def append_node(self, xpath: str, new_node: str) -> KeywordResult:
        node, failure = self._select_node(xpath)
        if failure is not None:
            return failure
        if node is None:
            return KeywordResult(Status.ERROR, "Can not find xml node with given xpath")
        try:
            fragment = ET.fromstring(f"<gfriend-fragment>{new_node}</gfriend-fragment>")
        except ET.ParseError:
            return KeywordResult(Status.ERROR, "Error with xml node")
        for child in list(fragment):
            fragment.remove(child)
            node.append(child)
        return KeywordResult(Status.PASS)

    def save_file(self, file_path: str) -> KeywordResult:
        if self._root is None:
            return self._not_loaded()
        try:
            ET.ElementTree(self._root).write(
                file_path,
                encoding="utf-8",
                xml_declaration=True,
            )
        except (OSError, ValueError) as error:
            return KeywordResult(Status.ERROR, str(error))
        return KeywordResult(Status.PASS)

    def _select_node(
        self,
        xpath: str,
    ) -> tuple[ET.Element | None, KeywordResult | None]:
        if self._root is None:
            return None, self._not_loaded()
        path = xpath.strip()
        if path.startswith("/") and not path.startswith("//"):
            path = path[1:]
            root_name, separator, remainder = path.partition("/")
            if self._qualified_name(root_name) == self._root.tag:
                path = remainder if separator else "."
        elif path.startswith("//"):
            path = "." + path
        try:
            node = self._root if path in {"", "."} else self._root.find(path, self._namespaces)
        except (SyntaxError, KeyError, TypeError, ValueError) as error:
            return None, KeywordResult(Status.ERROR, f"Invalid XPath: {error}")
        if node is None:
            return None, KeywordResult(
                Status.FAIL,
                "Can not find xml node with given xpath",
            )
        return node, None

    def _qualified_name(self, name: str) -> str:
        prefix, separator, local_name = name.partition(":")
        if not separator:
            return name
        namespace = self._namespaces.get(prefix)
        if namespace is None:
            return name
        return f"{{{namespace}}}{local_name}"

    @staticmethod
    def _parse(xml: str | bytes) -> tuple[ET.Element, dict[str, str]]:
        source = io.BytesIO(xml) if isinstance(xml, bytes) else io.StringIO(xml)
        namespaces: dict[str, str] = {}
        for _, namespace in ET.iterparse(source, events=("start-ns",)):
            prefix, uri = namespace
            namespaces[prefix] = uri
        root = ET.fromstring(xml)
        return root, namespaces

    @staticmethod
    def _variable_key(name: str) -> str:
        name = name.strip()
        if name.startswith("${") and name.endswith("}"):
            return name
        return f"${{{name.strip('${}')}}}"

    @staticmethod
    def _not_loaded() -> KeywordResult:
        return KeywordResult(
            Status.ERROR,
            "XML is not loaded. Use Load or Load From File before getting values",
        )

from __future__ import annotations

import argparse
import importlib
import json
import sys
from pathlib import Path

from .executor import Executor
from .parser import ParseError, parse_file, parse_variable_file


def _variable(value: str) -> tuple[str, str]:
    if "=" not in value:
        raise argparse.ArgumentTypeError("Variables must use NAME=VALUE format")
    name, contents = value.split("=", 1)
    if not name.strip():
        raise argparse.ArgumentTypeError("Variable name must not be empty")
    return name.strip(), contents


def _load_plugin(specification: str) -> tuple[str, object]:
    module_name, separator, object_name = specification.partition(":")
    if not separator or not module_name or not object_name:
        raise ValueError("Plugins must use module:object format")
    plugin = getattr(importlib.import_module(module_name), object_name)
    if isinstance(plugin, type):
        plugin = plugin()
    name = getattr(plugin, "LIBRARY_NAME", plugin.__class__.__name__)
    return str(name), plugin


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="Run a GFriend test script with Python keyword libraries.")
    parser.add_argument("script", type=Path, help="Path to a .txt, .gfscripts, or other GFriend script")
    parser.add_argument("-c", "--test-case", action="append", dest="test_cases", help="Test case to run (repeatable)")
    parser.add_argument("-e", "--var", action="append", type=_variable, default=[], help="Execution variable NAME=VALUE")
    parser.add_argument("-v", "--variable-file", action="append", type=Path, default=[], help="Load a .gfvar file")
    parser.add_argument("--plugin", action="append", default=[], metavar="MODULE:OBJECT", help="Load a Python keyword library")
    parser.add_argument("--report", type=Path, help="Write the JSON execution report to this path")
    args = parser.parse_args(argv)

    try:
        variables: dict[str, str] = {}
        for variable_file in args.variable_file:
            variables.update(parse_variable_file(variable_file))
        variables.update(dict(args.var))
        suite = parse_file(args.script, variables)
        executor = Executor()
        for plugin_spec in args.plugin:
            name, plugin = _load_plugin(plugin_spec)
            executor.register_library(name, plugin)
        result = executor.run(suite, set(args.test_cases) if args.test_cases else None)
    except (ParseError, ImportError, AttributeError, ValueError) as error:
        print(f"gfriend-python: {error}", file=sys.stderr)
        return 2

    report = json.dumps(result.to_dict(), indent=2)
    if args.report:
        try:
            args.report.parent.mkdir(parents=True, exist_ok=True)
            args.report.write_text(report + "\n", encoding="utf-8")
        except OSError as error:
            print(f"gfriend-python: cannot write report '{args.report}': {error}", file=sys.stderr)
            return 2
    else:
        print(report)
    return 0 if result.status.value == "PASS" else 1


if __name__ == "__main__":
    raise SystemExit(main())

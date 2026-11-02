from __future__ import annotations

import argparse
import json
import sys
import time
import xml.etree.ElementTree as ET
from datetime import datetime
from pathlib import Path

from .executor import Executor
from .models import ExecutionResult, Status, TestCaseResult
from .parser import ParseError, parse_file
from .report import write_html_report, write_xml_report


def _local_name(tag: str) -> str:
    return tag.rsplit("}", 1)[-1]


def _children(element: ET.Element, name: str) -> list[ET.Element]:
    return [child for child in element if _local_name(child.tag) == name]


def _child_text(element: ET.Element, name: str) -> str | None:
    values = _children(element, name)
    if not values:
        return None
    return (values[0].text or "").strip()


def _repeat_count(element: ET.Element, field: str) -> int:
    value = element.get("Repeat")
    if value is None:
        return 1
    try:
        count = int(value)
    except ValueError as error:
        raise ValueError(f"{field} Repeat must be a non-negative integer") from error
    if count < 0:
        raise ValueError(f"{field} Repeat must be a non-negative integer")
    return count


def _resolve_path(root: Path, value: str) -> Path:
    path = Path(value)
    return path.resolve() if path.is_absolute() else (root / path).resolve()


def _aggregate_status(results: list[TestCaseResult]) -> Status:
    if any(result.status == Status.ERROR for result in results):
        return Status.ERROR
    if any(result.status == Status.FAIL for result in results):
        return Status.FAIL
    return Status.PASS


def run_spec(
    spec_path: str | Path,
    execution_variables: dict[str, str] | None = None,
) -> ExecutionResult:
    """Run suites and selected cases from a legacy-compatible XML TestRuns file."""
    spec_file = Path(spec_path).resolve()
    try:
        root = ET.parse(spec_file).getroot()
    except (OSError, ET.ParseError) as error:
        raise ValueError(f"Cannot read run spec: {error}") from error
    if _local_name(root.tag) != "TestRuns":
        raise ValueError("Run spec root element must be TestRuns")

    suite_nodes = _children(root, "TestSuite")
    if not suite_nodes:
        raise ValueError("Run spec must contain at least one TestSuite")
    root_repeat = _repeat_count(root, "TestRuns")
    default_output_text = _child_text(root, "OutputPath")
    default_output_root = (
        _resolve_path(spec_file.parent, default_output_text)
        if default_output_text
        else spec_file.parent / "GFriendOutput"
    )

    suite_specs: list[tuple[Path, int, Path | None, list[str] | None]] = []
    for index, node in enumerate(suite_nodes, start=1):
        suite_path_text = _child_text(node, "TestSuitePath")
        if not suite_path_text:
            raise ValueError(f"TestSuite #{index} is missing TestSuitePath")
        if _children(node, "DeviceUnderTest") or _child_text(node, "DefaultDevice"):
            raise ValueError(
                f"TestSuite #{index} configures a device, which is not supported by the Python runtime"
            )
        suite_path = _resolve_path(spec_file.parent, suite_path_text)
        suite_output_text = _child_text(node, "OutputPath")
        suite_output = (
            _resolve_path(spec_file.parent, suite_output_text)
            if suite_output_text
            else None
        )
        selected_cases = [
            (case.text or "").strip()
            for case in _children(node, "TestCase")
            if (case.text or "").strip()
        ]
        suite_specs.append(
            (
                suite_path,
                _repeat_count(node, f"TestSuite #{index}"),
                suite_output,
                selected_cases or None,
            )
        )

    started_at = datetime.now().astimezone()
    start_clock = time.monotonic()
    results: list[TestCaseResult] = []
    for root_iteration in range(1, root_repeat + 1):
        for suite_path, suite_repeat, suite_output, selected_cases in suite_specs:
            for suite_iteration in range(1, suite_repeat + 1):
                output_path = suite_output
                if output_path is None:
                    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
                    output_path = default_output_root / (
                        f"{suite_path.stem}_{root_iteration:03}_{suite_iteration:03}_{timestamp}"
                    )
                output_path.mkdir(parents=True, exist_ok=True)
                suite = parse_file(suite_path, execution_variables)
                result = Executor(output_folder=output_path).run(
                    suite,
                    set(selected_cases) if selected_cases is not None else None,
                )
                results.extend(result.test_cases)

    return ExecutionResult(
        suite_name=spec_file.stem,
        status=_aggregate_status(results),
        test_cases=results,
        started_at=started_at,
        duration_seconds=time.monotonic() - start_clock,
    )


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(
        description="Run GFriend test suites from an XML TestRuns file."
    )
    parser.add_argument("run_spec", type=Path, help="Path to a TestRuns XML file")
    parser.add_argument(
        "-e",
        "--var",
        action="append",
        default=[],
        metavar="NAME=VALUE",
        help="Execution variable override",
    )
    parser.add_argument(
        "--report",
        type=Path,
        help="Write the JSON execution report to this path",
    )
    parser.add_argument("--html-report", type=Path, help="Write a standalone HTML report")
    parser.add_argument("--xml-report", type=Path, help="Write a structured XML report")
    args = parser.parse_args(argv)

    variables: dict[str, str] = {}
    for variable in args.var:
        name, separator, value = variable.partition("=")
        if not separator or not name.strip():
            parser.error("Variables must use NAME=VALUE format")
        variables[name.strip()] = value
    try:
        result = run_spec(args.run_spec, variables)
        report = json.dumps(result.to_dict(), indent=2)
        if args.report:
            args.report.parent.mkdir(parents=True, exist_ok=True)
            args.report.write_text(report + "\n", encoding="utf-8")
        if args.html_report:
            write_html_report(result, args.html_report)
        if args.xml_report:
            write_xml_report(result, args.xml_report)
    except (OSError, ParseError, ValueError, ET.ParseError) as error:
        print(f"gfriend-run-spec: {error}", file=sys.stderr)
        return 2
    if not args.report:
        print(report)
    return 0 if result.status == Status.PASS else 1


if __name__ == "__main__":
    raise SystemExit(main())

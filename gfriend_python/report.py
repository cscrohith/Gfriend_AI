from __future__ import annotations

import xml.etree.ElementTree as ET
from datetime import timedelta
from html import escape
from pathlib import Path

from .models import ExecutionResult


def _xml_safe(value: str) -> str:
    return "".join(
        character
        for character in value
        if character in "\t\n\r"
        or "\x20" <= character <= "\ud7ff"
        or "\ue000" <= character <= "\ufffd"
        or "\U00010000" <= character <= "\U0010ffff"
    )


def render_xml_report(result: ExecutionResult) -> str:
    """Render a structured XML execution report."""
    root = ET.Element("TestSuite", {"name": _xml_safe(result.suite_name)})
    ET.SubElement(root, "Result").text = result.status.value
    ET.SubElement(root, "StartTime").text = (
        result.started_at.isoformat() if result.started_at else ""
    )
    ET.SubElement(root, "DurationSeconds").text = f"{result.duration_seconds:.3f}"

    cases = ET.SubElement(root, "TestCases")
    for case in result.test_cases:
        case_element = ET.SubElement(
            cases, "TestCase", {"name": _xml_safe(case.name)}
        )
        ET.SubElement(case_element, "Result").text = case.status.value
        steps = ET.SubElement(case_element, "Steps")
        for step in case.steps:
            step_element = ET.SubElement(
                steps,
                "Step",
                {"line": str(step.line_number), "keyword": _xml_safe(step.keyword)},
            )
            ET.SubElement(step_element, "Result").text = step.status.value
            ET.SubElement(step_element, "Output").text = _xml_safe(step.output)

    ET.indent(root, space="  ")
    return ET.tostring(root, encoding="unicode", xml_declaration=True)


def write_xml_report(result: ExecutionResult, path: str | Path) -> Path:
    report_path = Path(path)
    report_path.parent.mkdir(parents=True, exist_ok=True)
    report_path.write_text(render_xml_report(result), encoding="utf-8")
    return report_path


def render_legacy_xml_execution_report(result: ExecutionResult) -> str:
    """Render a Python execution result in the legacy GFriend output.xml shape."""
    root = ET.Element("TestSuite", {"Name": _xml_safe(result.suite_name)})
    started_at = result.started_at
    ended_at = started_at + timedelta(seconds=result.duration_seconds) if started_at else None
    ET.SubElement(root, "StartTime").text = started_at.isoformat() if started_at else ""
    ET.SubElement(root, "EndTime").text = ended_at.isoformat() if ended_at else ""
    for case in result.test_cases:
        case_element = ET.SubElement(root, "TestCase", {"Name": _xml_safe(case.name)})
        ET.SubElement(case_element, "Result").text = case.status.value
        for step in case.steps:
            statement = ET.SubElement(
                case_element,
                "Statement",
                {"StatementName": _xml_safe(step.keyword)},
            )
            ET.SubElement(statement, "StartTime").text = ""
            ET.SubElement(statement, "Result").text = step.status.value
            ET.SubElement(statement, "Output").text = _xml_safe(step.output)
            ET.SubElement(statement, "EndTime").text = ""
    ET.indent(root, space="  ")
    return ET.tostring(root, encoding="unicode", xml_declaration=True)


def write_legacy_xml_execution_report(
    result: ExecutionResult,
    path: str | Path,
) -> Path:
    report_path = Path(path)
    report_path.parent.mkdir(parents=True, exist_ok=True)
    report_path.write_text(
        render_legacy_xml_execution_report(result),
        encoding="utf-8",
    )
    return report_path


def render_html_report(result: ExecutionResult) -> str:
    """Render a self-contained, safely escaped execution report."""
    counts = {
        status: sum(case.status.value == status for case in result.test_cases)
        for status in ("PASS", "FAIL", "ERROR")
    }
    summary = "".join(
        f'<div class="summary {status.lower()}"><strong>{count}</strong> {status}</div>'
        for status, count in counts.items()
    )
    cases = []
    for case in result.test_cases:
        steps = "".join(
            "<tr>"
            f"<td>{step.line_number}</td>"
            f"<td>{escape(step.keyword)}</td>"
            f'<td><span class="badge {step.status.value.lower()}">{step.status.value}</span></td>'
            f"<td><pre>{escape(step.output)}</pre></td>"
            "</tr>"
            for step in case.steps
        )
        cases.append(
            f'<details class="case {case.status.value.lower()}" open>'
            "<summary>"
            f"<span>{escape(case.name)}</span>"
            f'<span class="badge {case.status.value.lower()}">{case.status.value}</span>'
            f"<span>{len(case.steps)} steps</span>"
            "</summary>"
            '<div class="table-wrap"><table><thead><tr>'
            "<th>Line</th><th>Keyword</th><th>Status</th><th>Output</th>"
            "</tr></thead><tbody>"
            f"{steps}"
            "</tbody></table></div>"
            "</details>"
        )
    started = result.started_at.isoformat() if result.started_at else "Unknown"
    return f"""<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>GFriend report - {escape(result.suite_name)}</title>
<style>
:root {{ color-scheme: light dark; font: 15px/1.5 system-ui, sans-serif; }}
body {{ max-width: 1100px; margin: 2rem auto; padding: 0 1rem; }}
h1 {{ margin-bottom: .25rem; }}
.meta {{ opacity: .75; }}
.summaries {{ display: flex; flex-wrap: wrap; gap: .75rem; margin: 1.5rem 0; }}
.summary {{ padding: .8rem 1rem; border: 1px solid #8886; border-radius: .5rem; }}
.summary strong {{ font-size: 1.5rem; }}
.badge {{ padding: .15rem .5rem; border-radius: 999px; font-size: .8rem; font-weight: 700; }}
.pass .badge, .summary.pass {{ background: #22863a22; color: #22863a; }}
.fail .badge, .summary.fail {{ background: #cb243122; color: #cb2431; }}
.error .badge, .summary.error {{ background: #b0880022; color: #9a7200; }}
details.case {{ border: 1px solid #8886; border-radius: .5rem; margin: .75rem 0; overflow: hidden; }}
details summary {{ cursor: pointer; display: flex; gap: 1rem; align-items: center; padding: .85rem; }}
details summary span:first-child {{ flex: 1; font-weight: 650; }}
.table-wrap {{ overflow-x: auto; }}
table {{ border-collapse: collapse; width: 100%; }}
th, td {{ border-top: 1px solid #8884; padding: .6rem; text-align: left; vertical-align: top; }}
pre {{ margin: 0; white-space: pre-wrap; overflow-wrap: anywhere; font: inherit; }}
</style>
</head>
<body>
<h1>{escape(result.suite_name)}</h1>
<div class="meta">Result: <strong>{result.status.value}</strong> · Started: {escape(started)}
· Duration: {result.duration_seconds:.3f}s · Cases: {len(result.test_cases)}</div>
<section class="summaries" aria-label="Test case summary">{summary}</section>
<main>{"".join(cases) if cases else "<p>No test cases were executed.</p>"}</main>
</body>
</html>
"""


def write_html_report(result: ExecutionResult, path: str | Path) -> Path:
    report_path = Path(path)
    report_path.parent.mkdir(parents=True, exist_ok=True)
    report_path.write_text(render_html_report(result), encoding="utf-8")
    return report_path


def _remove_invalid_xml_characters(value: str) -> str:
    return "".join(
        character
        for character in value
        if character in "\t\n\r"
        or "\x20" <= character <= "\ud7ff"
        or "\ue000" <= character <= "\ufffd"
        or "\U00010000" <= character <= "\U0010ffff"
    )


def _legacy_remote_report(root: ET.Element, report_path: Path) -> None:
    for remote in root.iter("Remote"):
        block_id = remote.get("BlockID")
        if not block_id or Path(block_id).name != block_id:
            continue
        remote_path = report_path.parent / f"output_{block_id}.xml"
        if not remote_path.is_file():
            continue
        remote_root = ET.fromstring(
            _remove_invalid_xml_characters(remote_path.read_text(encoding="utf-8-sig"))
        )
        remote_node = remote_root if remote_root.tag == "Remote" else remote_root.find(".//Remote")
        if remote_node is not None:
            for child in list(remote_node):
                remote.append(child)


def _legacy_report_node(element: ET.Element) -> str:
    status_text = (element.findtext("Result") or "").strip().upper()
    status = status_text if status_text in {"PASS", "FAIL", "ERROR"} else ""
    css_status = status.casefold() if status else "neutral"
    attributes = " · ".join(
        f"{escape(key)}: {escape(value)}"
        for key, value in element.attrib.items()
    )
    statement = element.get("StatementName") or element.get("Name") or element.tag
    heading = escape(statement)
    if attributes and statement == element.tag:
        heading += f" <span class=\"metadata\">{attributes}</span>"
    badge = (
        f'<span class="badge {css_status}">{status}</span>'
        if status
        else f'<span class="badge neutral">{escape(element.tag)}</span>'
    )
    messages = []
    for name in ("Condition", "ConditionResult", "Output", "ErrorDescription", "AdditionalInfo", "Deprecated"):
        text = element.findtext(name)
        if text and text.strip():
            label = "Warning" if name == "Deprecated" else name
            messages.append(
                f'<div class="message {("warning" if name == "Deprecated" else "")}">'
                f"<strong>{escape(label)}:</strong><pre>{escape(text.strip())}</pre></div>"
            )
    child_nodes = [
        child
        for child in element
        if child.tag not in {
            "StartTime", "EndTime", "Result", "Condition", "ConditionResult",
            "Output", "ErrorDescription", "AdditionalInfo", "Deprecated",
            "ScreenShot", "AlwaysPass",
        }
    ]
    children = "".join(_legacy_report_node(child) for child in child_nodes)
    timing = " · ".join(
        f"{name}: {escape((element.findtext(name) or '').strip())}"
        for name in ("StartTime", "EndTime")
        if (element.findtext(name) or "").strip()
    )
    details = "".join(messages) + (f'<div class="metadata">{timing}</div>' if timing else "") + children
    return (
        f'<details class="result-node {css_status}" open><summary>'
        f"<span>{heading}</span>{badge}</summary><div class=\"node-body\">{details}</div></details>"
    )


def render_legacy_xml_report(path: str | Path) -> str:
    """Render a legacy GFriend output.xml file as a safe standalone HTML report."""
    xml_path = Path(path)
    root = ET.fromstring(
        _remove_invalid_xml_characters(xml_path.read_text(encoding="utf-8-sig"))
    )
    _legacy_remote_report(root, xml_path)
    suite = root if root.tag == "TestSuite" else root.find(".//TestSuite")
    if suite is None:
        raise ValueError(f"No TestSuite element found in legacy report: {xml_path}")
    test_cases = suite.findall("TestCase")
    counts = {
        status: sum((case.findtext("Result") or "").strip().upper() == status for case in test_cases)
        for status in ("PASS", "FAIL", "ERROR")
    }
    summary = "".join(
        f'<div class="summary {status.casefold()}"><strong>{count}</strong> {status}</div>'
        for status, count in counts.items()
    )
    case_sections = "".join(_legacy_report_node(case) for case in test_cases)
    suite_name = suite.get("Name") or xml_path.stem
    return f"""<!doctype html>
<html lang="en"><head><meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>GFriend legacy report - {escape(suite_name)}</title>
<style>
:root {{ color-scheme: light dark; font: 15px/1.5 system-ui, sans-serif; }}
body {{ max-width: 1100px; margin: 2rem auto; padding: 0 1rem; }}
.summaries {{ display: flex; flex-wrap: wrap; gap: .75rem; margin: 1.5rem 0; }}
.summary, .result-node {{ border: 1px solid #8886; border-radius: .5rem; }}
.summary {{ padding: .8rem 1rem; }} .summary strong {{ font-size: 1.5rem; }}
.pass .badge, .summary.pass {{ background: #22863a22; color: #22863a; }}
.fail .badge, .summary.fail {{ background: #cb243122; color: #cb2431; }}
.error .badge, .summary.error {{ background: #b0880022; color: #9a7200; }}
.neutral .badge {{ background: #8882; }} .badge {{ padding: .15rem .5rem; border-radius: 999px; font-size: .8rem; }}
.result-node {{ margin: .65rem 0; overflow: hidden; }} .result-node summary {{ cursor: pointer; display: flex; justify-content: space-between; gap: 1rem; padding: .7rem; }}
.result-node summary span:first-child {{ overflow-wrap: anywhere; }} .node-body {{ padding: 0 .8rem .8rem; }}
.metadata {{ opacity: .7; font-size: .9rem; }} .message {{ margin: .4rem 0; }}
.message pre {{ margin: .2rem 0; white-space: pre-wrap; overflow-wrap: anywhere; }}
.warning {{ color: #b08800; }}
</style></head><body>
<h1>{escape(suite_name)}</h1>
<p>Legacy GFriend execution report · Source: {escape(xml_path.name)}</p>
<section class="summaries" aria-label="Test case summary">{summary}</section>
<main>{case_sections if case_sections else "<p>No test cases found.</p>"}</main>
</body></html>"""


def write_legacy_html_report(
    source: str | Path,
    destination: str | Path | None = None,
) -> Path:
    source_path = Path(source)
    report_path = Path(destination) if destination is not None else source_path.with_name("report.html")
    report_path.parent.mkdir(parents=True, exist_ok=True)
    report_path.write_text(render_legacy_xml_report(source_path), encoding="utf-8")
    return report_path

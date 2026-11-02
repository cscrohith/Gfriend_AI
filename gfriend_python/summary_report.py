from __future__ import annotations

import argparse
import os
import sys
import xml.etree.ElementTree as ET
from dataclasses import dataclass
from datetime import datetime
from html import escape
from pathlib import Path
from urllib.parse import quote

from .report import _remove_invalid_xml_characters


@dataclass(frozen=True, slots=True)
class SuiteSummary:
    report_path: Path
    suite_name: str
    passed: int
    failed: int
    errors: int
    started: str
    ended: str
    duration: str

    @property
    def overall(self) -> str:
        if self.errors:
            return "ERROR"
        if self.failed:
            return "FAIL"
        return "PASS"


def _read_suite_summary(root_folder: Path, report_path: Path) -> SuiteSummary:
    try:
        root = ET.fromstring(
            _remove_invalid_xml_characters(report_path.read_text(encoding="utf-8-sig"))
        )
    except (OSError, ET.ParseError) as error:
        raise ValueError(f"Could not read report '{report_path}': {error}") from error
    suite = root if root.tag == "TestSuite" else root.find(".//TestSuite")
    if suite is None:
        raise ValueError(f"No TestSuite element found in report: {report_path}")
    cases = suite.findall("TestCase")
    counts = {
        status: sum((case.findtext("Result") or "").strip().upper() == status for case in cases)
        for status in ("PASS", "FAIL", "ERROR")
    }
    started = (suite.findtext("StartTime") or "").strip()
    ended = (suite.findtext("EndTime") or "").strip()
    duration = ""
    if started and ended:
        try:
            delta = datetime.fromisoformat(ended) - datetime.fromisoformat(started)
            duration = str(delta)
        except ValueError:
            duration = ""
    if not duration:
        raw_duration = (suite.findtext("DurationSeconds") or "").strip()
        if raw_duration:
            try:
                duration = f"{float(raw_duration):.3f}s"
            except ValueError:
                duration = raw_duration
    try:
        relative_path = report_path.relative_to(root_folder)
    except ValueError:
        relative_path = report_path
    return SuiteSummary(
        report_path=relative_path,
        suite_name=suite.get("Name") or report_path.parent.name,
        passed=counts["PASS"],
        failed=counts["FAIL"],
        errors=counts["ERROR"],
        started=started,
        ended=ended,
        duration=duration,
    )


def render_summary_report(
    root_folder: str | Path,
    destination: str | Path | None = None,
) -> str:
    """Render a summary for all legacy GFriend output.xml files under a folder."""
    root = Path(root_folder).resolve()
    if not root.is_dir():
        raise ValueError(f"Report root is not a directory: {root}")
    xml_reports = sorted(
        root.rglob("output.xml"),
        key=lambda path: (path.stat().st_mtime_ns, path.as_posix().casefold()),
    )
    reports = [_read_suite_summary(root, report_path) for report_path in xml_reports]
    totals = {
        "PASS": sum(report.passed for report in reports),
        "FAIL": sum(report.failed for report in reports),
        "ERROR": sum(report.errors for report in reports),
    }
    cards = "".join(
        f'<div class="summary {status.casefold()}"><strong>{count}</strong> {status}</div>'
        for status, count in totals.items()
    )
    rows = []
    for report in reports:
        sibling_html = report.report_path.with_name("report.html")
        target = root / sibling_html if (root / sibling_html).is_file() else root / report.report_path
        output_folder = Path(destination).resolve().parent if destination else root
        relative_target = Path(os.path.relpath(target, output_folder)).as_posix()
        href = quote(relative_target, safe="/._-")
        rows.append(
            "<tr>"
            f"<td>{escape(report.suite_name)}</td>"
            f'<td><span class="badge {report.overall.casefold()}">{report.overall}</span></td>'
            f"<td>{report.passed}</td><td>{report.failed}</td><td>{report.errors}</td>"
            f"<td>{escape(report.started)}</td><td>{escape(report.ended)}</td>"
            f"<td>{escape(report.duration)}</td>"
            f'<td><a href="{escape(href, quote=True)}">Open report</a></td>'
            "</tr>"
        )
    return f"""<!doctype html>
<html lang="en"><head><meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>GFriend execution summary</title>
<style>
:root {{ color-scheme: light dark; font: 15px/1.5 system-ui, sans-serif; }}
body {{ max-width: 1200px; margin: 2rem auto; padding: 0 1rem; }}
.summaries {{ display: flex; flex-wrap: wrap; gap: .75rem; margin: 1.5rem 0; }}
.summary {{ padding: .8rem 1rem; border: 1px solid #8886; border-radius: .5rem; }}
.summary strong {{ font-size: 1.5rem; }}
.pass {{ color: #22863a; }} .fail {{ color: #cb2431; }} .error {{ color: #b08800; }}
.badge {{ padding: .15rem .5rem; border-radius: 999px; font-size: .8rem; font-weight: 700; }}
.table-wrap {{ overflow-x: auto; }} table {{ border-collapse: collapse; width: 100%; }}
th, td {{ border-bottom: 1px solid #8884; padding: .65rem; text-align: left; }}
</style></head><body>
<h1>Test Summary</h1>
<p>{len(reports)} test suite report(s)</p>
<section class="summaries" aria-label="Aggregated test case summary">{cards}</section>
<div class="table-wrap"><table><thead><tr>
<th>Test ID</th><th>Result</th><th>Pass</th><th>Fail</th><th>Error</th>
<th>Start Time</th><th>End Time</th><th>Duration</th><th>Report</th>
</tr></thead><tbody>
{"".join(rows) if rows else '<tr><td colspan="9">No output.xml reports found.</td></tr>'}
</tbody></table></div></body></html>"""


def write_summary_report(
    root_folder: str | Path,
    destination: str | Path | None = None,
) -> Path:
    root = Path(root_folder).resolve()
    output = Path(destination).resolve() if destination else root / "SummaryReport.html"
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(render_summary_report(root, output), encoding="utf-8")
    return output


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(
        description="Aggregate legacy GFriend output.xml reports into one HTML summary."
    )
    parser.add_argument("report_folder", type=Path, help="Folder containing output.xml reports")
    parser.add_argument("-o", "--output", type=Path, help="HTML destination (default: SummaryReport.html in the folder)")
    args = parser.parse_args(argv)
    try:
        output = write_summary_report(args.report_folder, args.output)
    except (OSError, ValueError) as error:
        print(f"gfriend-summary-report: {error}", file=sys.stderr)
        return 2
    print(output)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

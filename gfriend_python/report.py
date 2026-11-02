from __future__ import annotations

from html import escape
from pathlib import Path

from .models import ExecutionResult


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

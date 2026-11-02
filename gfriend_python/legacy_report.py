from __future__ import annotations

import argparse
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

from .report import write_legacy_html_report


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(
        description="Render a legacy GFriend output.xml file as standalone HTML."
    )
    parser.add_argument("xml_report", type=Path, help="Path to a legacy GFriend output.xml")
    parser.add_argument("-o", "--output", type=Path, help="HTML destination (default: sibling report.html)")
    args = parser.parse_args(argv)
    try:
        output = write_legacy_html_report(args.xml_report, args.output)
    except (OSError, ET.ParseError, ValueError) as error:
        print(f"gfriend-legacy-report: {error}", file=sys.stderr)
        return 2
    print(output)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

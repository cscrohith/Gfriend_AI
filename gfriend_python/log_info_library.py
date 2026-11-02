from __future__ import annotations

import csv
import html
import math
import time
from dataclasses import dataclass, field
from datetime import datetime
from pathlib import Path

from .models import KeywordResult, Status

_RESULT_COLORS = {"PASS": "#22863a", "FAIL": "#cb2431", "ERROR": "#d18b00"}


@dataclass(slots=True)
class _LogMeasureSet:
    path: Path
    started_at: datetime = field(default_factory=datetime.now)
    started_counter: float = field(default_factory=time.perf_counter)
    points: list[tuple[str, float, str]] = field(default_factory=list)


class LogInfoLibrary:
    """Portable timing and result-chart subset of the legacy LogInfo library."""

    def __init__(
        self,
        variables: dict[str, str],
        output_folder: str | Path,
    ) -> None:
        self._variables = variables
        self._output_folder = Path(output_folder)
        self._measure_sets: dict[str, _LogMeasureSet] = {}

    def start_time_check(self, measure_set: str) -> KeywordResult:
        measure_set = measure_set.strip()
        if not measure_set:
            return KeywordResult(Status.ERROR, "Measure set name cannot be empty.")
        try:
            self._output_folder.mkdir(parents=True, exist_ok=True)
            path = self._output_folder / f"{measure_set}.csv"
            with path.open("w", encoding="utf-8-sig", newline="") as stream:
                csv.writer(stream).writerow(["Point", "Time (ms)"])
            self._measure_sets[measure_set.casefold()] = _LogMeasureSet(path)
        except OSError as error:
            return KeywordResult(Status.ERROR, f"Could not create timing CSV: {error}")
        return KeywordResult(
            Status.PASS,
            f"Time check is started and output will be saved to {path}",
        )

    def start_time(self, measure_set: str, checkpoint: str) -> KeywordResult:
        measurement = self._measure_sets.get(measure_set.casefold())
        if measurement is None:
            return KeywordResult(
                Status.ERROR,
                f"Cannot find Measure set with name {measure_set}",
            )
        measurement.started_at = datetime.now()
        measurement.started_counter = time.perf_counter()
        return KeywordResult(Status.PASS)

    def end_time(self, measure_set: str, checkpoint: str) -> KeywordResult:
        measurement = self._measure_sets.get(measure_set.casefold())
        if measurement is None:
            return KeywordResult(
                Status.ERROR,
                f"Cannot find Measure set with name {measure_set}",
            )
        ended_at = datetime.now()
        elapsed = time.perf_counter() - measurement.started_counter
        result = self._variables.get(
            "${LoopResult}",
            self._variables.get("${KEYWORD_RESULT}", Status.PASS.value),
        ).strip().upper()
        point = checkpoint.strip()
        elapsed_text = (
            f"{int(elapsed // 3600)} h "
            f"{int(elapsed // 60) % 60} m "
            f"{int(elapsed) % 60} s"
        )
        try:
            with measurement.path.open("a", encoding="utf-8", newline="") as stream:
                csv.writer(stream).writerow(
                    [
                        point,
                        measurement.started_at.strftime("%Y-%m-%d %H:%M:%S"),
                        ended_at.strftime("%Y-%m-%d %H:%M:%S"),
                        elapsed_text,
                        result,
                    ]
                )
        except OSError as error:
            return KeywordResult(Status.ERROR, f"Could not write timing data: {error}")
        measurement.points.append((point, elapsed, result))
        return KeywordResult(Status.PASS)

    def draw_charts(self, measure_set: str) -> KeywordResult:
        measurement = self._measure_sets.get(measure_set.casefold())
        if measurement is None:
            return KeywordResult(
                Status.ERROR,
                f"Cannot find Measure set with name {measure_set}",
            )
        if not measurement.points:
            return KeywordResult(Status.ERROR, "No timing points are available to chart.")
        timeline = self._timeline_svg(measure_set, measurement.points)
        distribution = self._distribution_svg(measure_set, measurement.points)
        timeline_path = self._output_folder / f"{measure_set}_ColumnChart.svg"
        distribution_path = self._output_folder / f"{measure_set}_PieChart.svg"
        try:
            timeline_path.write_text(timeline, encoding="utf-8")
            distribution_path.write_text(distribution, encoding="utf-8")
        except OSError as error:
            return KeywordResult(Status.ERROR, f"Could not create timing charts: {error}")
        return KeywordResult(
            Status.PASS,
            f"Timing charts saved to {timeline_path} and {distribution_path}",
        )

    @staticmethod
    def _timeline_svg(
        name: str,
        points: list[tuple[str, float, str]],
    ) -> str:
        width, height = 900, 420
        left, right, top, bottom = 72, 24, 44, 80
        plot_width, plot_height = width - left - right, height - top - bottom
        maximum = max(duration for _, duration, _ in points) or 1.0
        slot = plot_width / len(points)
        bar_width = min(54.0, slot * 0.62)
        bars = []
        labels = []
        for index, (point, duration, result) in enumerate(points):
            x = left + slot * index + (slot - bar_width) / 2
            bar_height = duration / maximum * plot_height
            y = top + plot_height - bar_height
            color = _RESULT_COLORS.get(result, "#718096")
            bars.append(
                f'<rect x="{x:.1f}" y="{y:.1f}" width="{bar_width:.1f}" '
                f'height="{bar_height:.1f}" fill="{color}">'
                f'<title>{html.escape(point)}: {duration:.3f}s · {html.escape(result)}</title></rect>'
            )
            label = point if len(point) <= 16 else point[:13] + "..."
            labels.append(
                f'<text x="{x + bar_width / 2:.1f}" y="{height - bottom + 20}" '
                f'text-anchor="middle" transform="rotate(-35 {x + bar_width / 2:.1f} '
                f'{height - bottom + 20})">{html.escape(label)}</text>'
            )
        return (
            f'<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" '
            f'viewBox="0 0 {width} {height}"><title>{html.escape(name)} elapsed times</title>'
            f'<text x="{left}" y="24">Elapsed time by checkpoint (seconds)</text>'
            f'<path d="M {left} {top} V {height - bottom} H {width - right}" '
            f'fill="none" stroke="#64748b"/>{"".join(bars)}{"".join(labels)}</svg>'
        )

    @staticmethod
    def _distribution_svg(
        name: str,
        points: list[tuple[str, float, str]],
    ) -> str:
        counts: dict[str, int] = {}
        for _, _, result in points:
            counts[result] = counts.get(result, 0) + 1
        total = len(points)
        center_x, center_y, radius = 180.0, 170.0, 110.0
        angle = -math.pi / 2
        paths = []
        legend = []
        for index, (result, count) in enumerate(sorted(counts.items())):
            sweep = count / total * math.tau
            end_angle = angle + sweep
            x1, y1 = center_x + radius * math.cos(angle), center_y + radius * math.sin(angle)
            x2, y2 = center_x + radius * math.cos(end_angle), center_y + radius * math.sin(end_angle)
            large_arc = 1 if sweep > math.pi else 0
            color = _RESULT_COLORS.get(result, "#718096")
            paths.append(
                f'<path d="M {center_x} {center_y} L {x1:.2f} {y1:.2f} '
                f'A {radius} {radius} 0 {large_arc} 1 {x2:.2f} {y2:.2f} Z" '
                f'fill="{color}"><title>{html.escape(result)}: {count}</title></path>'
            )
            legend.append(
                f'<text x="340" y="{120 + index * 32}" fill="{color}">'
                f'{html.escape(result)}: {count} ({count / total * 100:.1f}%)</text>'
            )
            angle = end_angle
        return (
            '<svg xmlns="http://www.w3.org/2000/svg" width="520" height="360" '
            'viewBox="0 0 520 360">'
            f'<title>{html.escape(name)} result distribution</title>'
            f'<text x="24" y="32">Result distribution</text>'
            f'{"".join(paths)}{"".join(legend)}</svg>'
        )

from __future__ import annotations

import csv
import html
import time
from dataclasses import dataclass, field
from pathlib import Path

from .models import KeywordResult, Status


@dataclass(slots=True)
class _MeasureSet:
    path: Path
    global_set: bool
    start_counter: float = field(default_factory=time.perf_counter)
    starts: dict[str, float] = field(default_factory=dict)
    points: list[tuple[str, float]] = field(default_factory=list)


class InfoLibrary:
    """Portable timing and chart subset of the legacy Info library."""

    def __init__(
        self,
        variables: dict[str, str],
        output_folder: str | Path,
    ) -> None:
        self._variables = variables
        self._output_folder = Path(output_folder)
        self._measure_sets: dict[str, _MeasureSet] = {}

    def start_time_check(self, measure_set: str) -> KeywordResult:
        measure_set = measure_set.strip()
        if not measure_set:
            return KeywordResult(Status.ERROR, "Measure set name cannot be empty.")
        try:
            self._output_folder.mkdir(parents=True, exist_ok=True)
            path = self._output_folder / f"{measure_set}.csv"
            global_set = measure_set.casefold().startswith("global_")
            if not global_set or not path.exists():
                with path.open("w", encoding="utf-8-sig", newline="") as stream:
                    csv.writer(stream).writerow(["Point", "Time (ms)"])
            self._measure_sets[measure_set.casefold()] = _MeasureSet(path, global_set)
        except OSError as error:
            return KeywordResult(Status.ERROR, f"Can not create csv file: {error}")
        return KeywordResult(
            Status.PASS,
            f"Time check is started and output will be saved to {path}",
        )

    def mark_start(self, measure_set: str, checkpoint: str) -> KeywordResult:
        measurement = self._measure_sets.get(measure_set.casefold())
        if measurement is None:
            return KeywordResult(
                Status.ERROR,
                f"Can not find Measure set with name {measure_set}",
            )
        measurement.starts[checkpoint] = time.perf_counter()
        return KeywordResult(Status.PASS)

    def mark_end(self, measure_set: str, checkpoint: str) -> KeywordResult:
        measurement = self._measure_sets.get(measure_set.casefold())
        if measurement is None:
            return KeywordResult(
                Status.ERROR,
                f"Can not find Measure set with name {measure_set}",
            )
        start = measurement.starts.get(checkpoint, measurement.start_counter)
        elapsed_ms = (time.perf_counter() - start) * 1000
        point_name = checkpoint.strip()
        if measurement.global_set and self._variables.get("${R}"):
            point_name = f"{self._variables['${R}']}_{point_name}"
        if any(point == point_name for point, _ in measurement.points):
            return KeywordResult(Status.PASS)
        try:
            with measurement.path.open("a", encoding="utf-8", newline="") as stream:
                csv.writer(stream).writerow([point_name, f"{elapsed_ms:.3f}"])
        except OSError as error:
            return KeywordResult(Status.ERROR, f"Could not write timing data: {error}")
        measurement.points.append((point_name, elapsed_ms))
        return KeywordResult(Status.PASS)

    def draw_time_graph(self, measure_set: str) -> KeywordResult:
        measurement = self._measure_sets.get(measure_set.casefold())
        if measurement is None:
            return KeywordResult(
                Status.ERROR,
                f"Can not find Measure set with name {measure_set}",
            )
        if not measurement.points:
            return KeywordResult(Status.ERROR, "No timing points are available to chart.")

        width, height = 900, 420
        left, right, top, bottom = 72, 24, 32, 72
        plot_width, plot_height = width - left - right, height - top - bottom
        maximum = max(duration for _, duration in measurement.points) or 1.0
        bars = []
        slot = plot_width / len(measurement.points)
        bar_width = min(54.0, slot * 0.62)
        labels = []
        for index, (name, duration) in enumerate(measurement.points):
            x = left + slot * index + (slot - bar_width) / 2
            bar_height = duration / maximum * plot_height
            y = top + plot_height - bar_height
            bars.append(
                f'<rect x="{x:.1f}" y="{y:.1f}" width="{bar_width:.1f}" '
                f'height="{bar_height:.1f}" rx="4" fill="#3977d5">'
                f'<title>{html.escape(name)}: {duration:.3f} ms</title></rect>'
            )
            label = name if len(name) <= 16 else name[:13] + "..."
            labels.append(
                f'<text x="{x + bar_width / 2:.1f}" y="{height - bottom + 22}" '
                f'text-anchor="middle" transform="rotate(-35 {x + bar_width / 2:.1f} '
                f'{height - bottom + 22})">{html.escape(label)}</text>'
            )

        chart_path = self._output_folder / f"{Path(measurement.path).stem}.svg"
        svg = (
            f'<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" '
            f'viewBox="0 0 {width} {height}" role="img">'
            f'<title>{html.escape(measure_set)} timing results</title>'
            f'<rect width="100%" height="100%" fill="#ffffff"/>'
            f'<text x="{left}" y="22" font-family="sans-serif" font-size="16">'
            f'{html.escape(measure_set)} · elapsed milliseconds</text>'
            f'<path d="M {left} {top} V {height - bottom} H {width - right}" '
            'fill="none" stroke="#64748b"/>'
            f'{"".join(bars)}{"".join(labels)}</svg>'
        )
        try:
            chart_path.write_text(svg, encoding="utf-8")
        except OSError as error:
            return KeywordResult(Status.ERROR, f"Could not create time chart: {error}")
        return KeywordResult(Status.PASS, f"Time chart saved to {chart_path}")

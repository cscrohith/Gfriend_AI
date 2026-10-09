#!/usr/bin/env python3
"""
Print Load Tester v1.4
──────────────────────
Concurrent Windows printer load testing tool.
Sends parallel print jobs to a shared printer and measures performance.

Features:
  • SafeQ-compatible printing via shellexec (triggers Desktop Interface popup)
  • SumatraPDF CLI support for fast, reliable PDF printing
  • Auto-detect installed Windows printers
  • Separate Printer Name vs UNC Path fields for correct SafeQ routing
  • Ultra-simple configuration - just enter job count
  • Automatic job distribution over 1 hour
  • Repeat/continuous mode for ongoing load testing
  • Folder support - cycle through multiple files
  • Real-time progress tracking and statistics
  • High-load warning for jobs > 3600
  • CSV export for detailed analysis

SafeQ Note:
  SafeQ Desktop Interface popup is triggered by the Windows spooler.
  Use method = "shellexec" with the INSTALLED PRINTER NAME (not UNC path).
  Run:  Get-Printer | Select Name   in PowerShell to find the exact name.

Dependencies:
  pip install pywin32          (optional — enables win32print & shellexec modes)
  SumatraPDF.exe               (optional — for sumatra method, add to PATH or specify full path)

Usage:
  python print_load_tester.py
"""

# ── Standard Library Imports ───────────────────────────────────────────────
import tkinter as tk
from tkinter import ttk, filedialog, messagebox, scrolledtext
import threading
import concurrent.futures
import time
import os
import queue
import csv
import subprocess
from datetime import datetime
from dataclasses import dataclass
import json
from pathlib import Path

# ── Optional Windows printing APIs ──────────────────────────────────────────
try:
    import win32print  # type: ignore
    WIN32_AVAILABLE = True
except ImportError:
    WIN32_AVAILABLE = False

# ── Configuration ────────────────────────────────────────────────────────────
CONFIG_FILE = Path.home() / ".print_load_tester.json"

# SumatraPDF executable path - will be auto-detected if in PATH
SUMATRA_EXE = "SumatraPDF.exe"


# ════════════════════════════════════════════════════════════════════════════
#  Data Model
# ════════════════════════════════════════════════════════════════════════════

@dataclass
class JobResult:
    """Stores the result of a single print job."""
    job_id: int
    printer: str
    file_path: str
    start_time: float
    end_time: float = 0.0
    status: str = "Pending"
    error: str = ""

    @property
    def duration_ms(self) -> float:
        if self.end_time > 0:
            return (self.end_time - self.start_time) * 1000.0
        return 0.0


# ════════════════════════════════════════════════════════════════════════════
#  Printer Discovery
# ════════════════════════════════════════════════════════════════════════════

def get_installed_printers() -> list[str]:
    """
    Return a list of installed Windows printer names via PowerShell.
    Falls back to win32print if pywin32 is available.
    Returns an empty list on failure.
    """
    names: list[str] = []

    # Method 1: win32print (fastest, no subprocess)
    if WIN32_AVAILABLE:
        try:
            for p in win32print.EnumPrinters(
                    win32print.PRINTER_ENUM_LOCAL | win32print.PRINTER_ENUM_CONNECTIONS,
                    None, 2):
                name = p.get("pPrinterName", "")
                if name:
                    names.append(name)
            if names:
                return sorted(names)
        except Exception:
            pass

    # Method 2: PowerShell fallback
    try:
        result = subprocess.run(
            ["powershell", "-NoProfile", "-NonInteractive", "-Command",
             "Get-Printer | Select-Object -ExpandProperty Name"],
            capture_output=True, timeout=10,
        )
        if result.returncode == 0:
            for line in result.stdout.decode(errors="replace").splitlines():
                name = line.strip()
                if name:
                    names.append(name)
    except Exception:
        pass

    return sorted(names)


# ════════════════════════════════════════════════════════════════════════════
#  Print Workers
# ════════════════════════════════════════════════════════════════════════════

def _make_job_name(job_id: int, file_path: str) -> str:
    filename = os.path.basename(file_path)
    return f"[{job_id:04d}] {filename}"


def send_print_job(job_id: int, printer_name: str, printer_unc: str,
                   file_path: str, copies: int, method: str) -> JobResult:
    """
    Entry point for each worker thread — sends one print job.

    Args:
        job_id:       Sequential job number
        printer_name: Windows installed printer display name (for shellexec/sumatra)
        printer_unc:  UNC path  \\\\server\\share  (for win32print / copy)
        file_path:    Full path to the file to print
        copies:       Number of copies
        method:       "shellexec" | "win32print" | "copy" | "sumatra"
    """
    result = JobResult(
        job_id=job_id,
        printer=printer_name or printer_unc,
        file_path=file_path,
        start_time=time.time(),
        status="Running",
    )
    job_name = _make_job_name(job_id, file_path)

    try:
        if method == "shellexec" and WIN32_AVAILABLE:
            # ── SafeQ-compatible path ──────────────────────────────────────
            # PrintTo via the installed printer name → goes through Windows
            # spooler → SafeQ Desktop Interface sees it → popup fires.
            target = printer_name if printer_name else printer_unc
            _print_via_shellexec(target, file_path, copies)

        elif method == "win32print" and WIN32_AVAILABLE:
            # RAW bytes directly to UNC — bypasses SafeQ client popup
            _print_via_win32(printer_unc, file_path, copies, job_name)

        else:
            # copy /B fallback — also bypasses SafeQ client popup
            _print_via_copy(printer_unc, file_path, copies)

        result.status = "Success"
    except Exception as exc:
        result.status = "Failed"
        result.error = str(exc)
    finally:
        result.end_time = time.time()
    return result


def _print_via_shellexec(printer_target: str, file_path: str,
                          copies: int) -> None:
    """
    Shell Print verb — routes through Windows spooler.

    Uses the 'Print' verb (not 'PrintTo') which is far more widely registered
    across file types. Temporarily sets the target printer as the Windows
    default, triggers Print, then restores the previous default.

    NOTE: Because this swaps the system default printer, concurrent shellexec
    workers may interfere with each other. For high-concurrency load tests
    prefer the 'copy' method which has no such limitation.

    printer_target should be the INSTALLED PRINTER NAME, e.g.:
        "SafeQ - Floor02"   ← from Get-Printer | Select Name
    """
    # Build a PowerShell script that:
    #   1. Saves current default printer
    #   2. Sets target as default
    #   3. Invokes Print verb on the file
    #   4. Waits for the process to exit
    #   5. Restores original default printer
    ps_cmd = f"""
$ErrorActionPreference = 'Stop'
$wsh = New-Object -ComObject WScript.Shell
$prev = (Get-WmiObject -Query 'SELECT * FROM Win32_Printer WHERE Default=True').Name
Set-DefaultPrinter -PrinterName "{printer_target}"
try {{
    $proc = Start-Process -FilePath "{file_path}" -Verb Print -PassThru
    $proc.WaitForExit(30000) | Out-Null
}} finally {{
    if ($prev) {{ Set-DefaultPrinter -PrinterName $prev }}
}}
"""
    for _ in range(copies):
        result = subprocess.run(
            ["powershell", "-NoProfile", "-NonInteractive", "-Command", ps_cmd],
            capture_output=True, timeout=120,
        )
        if result.returncode != 0:
            stderr = result.stderr.decode(errors="replace").strip()
            stdout = result.stdout.decode(errors="replace").strip()
            raise RuntimeError(
                f"Shell Print failed (rc={result.returncode}): {stderr or stdout}"
            )


def _print_via_win32(printer_unc: str, file_path: str, copies: int,
                     job_name: str) -> None:
    """RAW win32print API — best for .txt / PCL / PS files.
    NOTE: Bypasses SafeQ Desktop Interface — no popup will fire."""
    h_printer = win32print.OpenPrinter(printer_unc)
    try:
        with open(file_path, "rb") as fh:
            data = fh.read()
        for _ in range(copies):
            win32print.StartDocPrinter(h_printer, 1, (job_name, None, "RAW"))
            try:
                win32print.StartPagePrinter(h_printer)
                win32print.WritePrinter(h_printer, data)
                win32print.EndPagePrinter(h_printer)
            finally:
                win32print.EndDocPrinter(h_printer)
    finally:
        win32print.ClosePrinter(h_printer)


def _print_via_sumatra(printer_target: str, file_path: str, copies: int) -> None:
    """
    SumatraPDF CLI - fast and reliable for PDF files.
    
    Uses SumatraPDF.exe command-line interface:
        SumatraPDF.exe -print-to "printer name" "file.pdf"
    
    Works with both printer names and UNC paths (if supported by SumatraPDF).
    Much faster and more reliable than shellexec for PDF files.
    No dependencies required - just needs SumatraPDF.exe in PATH or full path specified.
    
    Args:
        printer_target: Printer name or UNC path
        file_path: Full path to PDF file to print
        copies: Number of copies to print
    """
    for _ in range(copies):
        result = subprocess.run(
            [SUMATRA_EXE, "-print-to", printer_target, file_path],
            capture_output=True, timeout=60,
        )
        if result.returncode != 0:
            stderr = result.stderr.decode(errors="replace").strip()
            stdout = result.stdout.decode(errors="replace").strip()
            # Check if SumatraPDF is not found
            if "not found" in stderr.lower() or "not recognized" in stderr.lower():
                raise RuntimeError(
                    f"SumatraPDF.exe not found. Install SumatraPDF and add to PATH, "
                    f"or update SUMATRA_EXE variable with full path.")
            raise RuntimeError(
                f"SumatraPDF print failed (rc={result.returncode}): {stderr or stdout}")


def _print_via_copy(printer_unc: str, file_path: str, copies: int) -> None:
    """Binary copy to UNC printer share — fallback, no pywin32 required.
    NOTE: Bypasses SafeQ Desktop Interface — no popup will fire."""
    for _ in range(copies):
        result = subprocess.run(
            f'copy /B "{file_path}" "{printer_unc}"',
            shell=True, capture_output=True, timeout=30,
        )
        if result.returncode != 0:
            stderr = result.stderr.decode(errors="replace").strip()
            raise RuntimeError(f"Copy failed (rc={result.returncode}): {stderr}")


def map_printer_credentials(printer_unc: str, username: str,
                             password: str, domain: str) -> str:
    """
    Authenticate against the print server via net use.
    Clears any existing conflicting session (Error 1219) before connecting.
    Returns empty string on success, or an error message on failure.
    """
    parts = printer_unc.replace("/", "\\").split("\\")
    server = "\\\\" + parts[2] if len(parts) > 2 else printer_unc
    user = f"{domain}\\{username}" if domain else username

    subprocess.run(["net", "use", server, "/delete", "/yes"],
                   capture_output=True, timeout=15)

    result = subprocess.run(
        ["net", "use", server, f"/user:{user}", password, "/persistent:no"],
        capture_output=True, timeout=20,
    )
    if result.returncode != 0:
        stderr = result.stderr.decode(errors="replace").strip()
        stdout = result.stdout.decode(errors="replace").strip()
        return stderr or stdout or f"net use failed (rc={result.returncode})"
    return ""


# ════════════════════════════════════════════════════════════════════════════
#  Themes & Styling
# ════════════════════════════════════════════════════════════════════════════

PALETTE = {
    "bg":       "#f0f2f5",
    "surface":  "#ffffff",
    "surface2": "#e8eaed",
    "border":   "#d0d5dd",
    "text":     "#111827",
    "muted":    "#6b7280",
    "accent":   "#2563eb",
    "success":  "#059669",
    "danger":   "#dc2626",
    "warning":  "#d97706",
    "pending":  "#9ca3af",
    "safeq":    "#0f766e",   # Teal highlight for SafeQ-specific UI elements
}

FONT_MONO  = ("Consolas", 11)
FONT_UI    = ("Segoe UI", 11)
FONT_BOLD  = ("Segoe UI", 11, "bold")
FONT_LARGE = ("Segoe UI", 15, "bold")
FONT_SMALL = ("Segoe UI", 9)


# ════════════════════════════════════════════════════════════════════════════
#  Main Application
# ════════════════════════════════════════════════════════════════════════════

class PrintLoadTesterApp(tk.Tk):

    def __init__(self):
        super().__init__()
        self.title("Print Load Tester  v1.4")
        self.geometry("1120x780")
        self.minsize(900, 640)
        self.configure(bg=PALETTE["bg"])

        self._results: list[JobResult] = []
        self._queue: queue.Queue = queue.Queue()
        self._running = False
        self._total_submitted = 0
        self._folder_files: list[str] = []
        self._iteration_count = 0
        self._repeat_config = None
        self._installed_printers: list[str] = []

        self._apply_styles()
        self._build_ui()
        self._load_settings()
        self._poll_queue()
        self.protocol("WM_DELETE_WINDOW", self._on_close)

        # Discover installed printers in background so startup isn't delayed
        threading.Thread(target=self._discover_printers, daemon=True).start()

    # ── ttk Styles ────────────────────────────────────────────────────────

    def _apply_styles(self) -> None:
        s = ttk.Style(self)
        s.theme_use("clam")
        P = PALETTE

        s.configure("TFrame",       background=P["bg"])
        s.configure("Card.TFrame",  background=P["surface"])
        s.configure("Inner.TFrame", background=P["surface"])

        s.configure("TLabel",
                    background=P["bg"], foreground=P["text"], font=FONT_UI)
        s.configure("Muted.TLabel",
                    background=P["surface"], foreground=P["muted"], font=FONT_UI)
        s.configure("Card.TLabel",
                    background=P["surface"], foreground=P["text"], font=FONT_UI)
        s.configure("Header.TLabel",
                    background=P["bg"], foreground=P["text"], font=FONT_LARGE)
        s.configure("SafeQ.TLabel",
                    background=P["surface"], foreground=P["safeq"],
                    font=("Segoe UI", 9, "bold"))

        for name in ("TEntry", "TSpinbox", "TCombobox"):
            s.configure(name,
                        fieldbackground=P["surface2"], foreground=P["text"],
                        bordercolor=P["border"], lightcolor=P["border"],
                        darkcolor=P["border"], insertcolor=P["text"],
                        selectbackground=P["accent"], selectforeground=P["bg"],
                        font=FONT_UI)

        s.configure("TButton", font=FONT_BOLD, padding=(10, 6),
                    borderwidth=0, relief="flat")
        s.configure("Primary.TButton",
                    background=P["accent"], foreground=P["bg"])
        s.map("Primary.TButton",
              background=[("active", "#3a8ae8"), ("disabled", P["border"])],
              foreground=[("disabled", P["muted"])])
        s.configure("Danger.TButton",
                    background=P["danger"], foreground=P["bg"])
        s.map("Danger.TButton",
              background=[("active", "#e04040"), ("disabled", P["border"])],
              foreground=[("disabled", P["muted"])])
        s.configure("Ghost.TButton",
                    background=P["surface2"], foreground=P["text"])
        s.map("Ghost.TButton",
              background=[("active", P["border"])])
        s.configure("Teal.TButton",
                    background=P["safeq"], foreground=P["bg"])
        s.map("Teal.TButton",
              background=[("active", "#0d9488"), ("disabled", P["border"])],
              foreground=[("disabled", P["muted"])])

        s.configure("Treeview",
                    background=P["surface"], foreground=P["text"],
                    fieldbackground=P["surface"], rowheight=30, font=FONT_MONO,
                    borderwidth=0)
        s.configure("Treeview.Heading",
                    background=P["surface2"], foreground=P["accent"],
                    font=FONT_BOLD, relief="flat", borderwidth=0)
        s.map("Treeview",
              background=[("selected", P["surface2"])],
              foreground=[("selected", P["text"])])

        s.configure("TProgressbar",
                    troughcolor=P["surface2"], background=P["accent"],
                    borderwidth=0, thickness=4)
        s.configure("TLabelframe",
                    background=P["surface"], bordercolor=P["border"],
                    relief="flat")
        s.configure("TLabelframe.Label",
                    background=P["surface"], foreground=P["accent"],
                    font=FONT_BOLD)
        s.configure("TSeparator", background=P["border"])

    # ── UI Layout ─────────────────────────────────────────────────────────

    def _build_ui(self) -> None:
        P = PALETTE

        tk.Frame(self, bg=P["accent"], height=2).pack(fill="x")

        body = tk.Frame(self, bg=P["bg"])
        body.pack(fill="both", expand=True)

        left = tk.Frame(body, bg=P["bg"], width=370)
        left.pack(side="left", fill="y", padx=(12, 0), pady=10)
        left.pack_propagate(False)
        self._build_config(left)

        tk.Frame(body, bg=P["border"], width=1).pack(side="left", fill="y", padx=12)

        right = tk.Frame(body, bg=P["bg"])
        right.pack(side="left", fill="both", expand=True, pady=10, padx=(0, 12))
        self._build_results(right)

        self._build_stats_bar()

    # ── Config Panel ──────────────────────────────────────────────────────

    def _build_config(self, parent: tk.Frame) -> None:
        P = PALETTE

        def section(title):
            lf = ttk.LabelFrame(parent, text=f"  {title}  ", padding=(10, 8))
            lf.pack(fill="x", pady=(0, 8))
            return lf

        def lbl(frame, text, color=None):
            tk.Label(frame, text=text, width=14, anchor="w",
                     bg=P["surface"], fg=color or P["muted"],
                     font=FONT_UI).pack(side="left")

        # ── SafeQ / Printer Section ────────────────────────────────────────
        pf = section("Printer")

        # SafeQ tip banner - HIDDEN (using SumatraPDF)
        # tip = tk.Frame(pf, bg="#f0fdf9", relief="flat")
        # tip.pack(fill="x", pady=(0, 8))
        # tk.Label(tip, text="⚑  SafeQ 6: use copy method (confirmed). shellexec = Print verb fallback.",
        #          bg="#f0fdf9", fg=P["safeq"], font=FONT_SMALL,
        #          wraplength=310, justify="left").pack(
        #              padx=6, pady=4, anchor="w")

        # Printer Name (for shellexec / SafeQ) - HIDDEN
        # Initialize variable but don't show in UI
        self._v_printer_name = tk.StringVar()
        self._installed_printers = []
        # row_name = tk.Frame(pf, bg=P["surface"])
        # row_name.pack(fill="x", pady=3)
        # lbl(row_name, "Printer Name", P["safeq"])
        # self._combo_printer = ttk.Combobox(
        #     row_name, textvariable=self._v_printer_name,
        #     state="normal", width=22)
        # self._combo_printer.pack(side="left", fill="x", expand=True)

        # Refresh + detect button - HIDDEN
        # refresh_row = tk.Frame(pf, bg=P["surface"])
        # refresh_row.pack(fill="x", pady=(0, 4))
        # tk.Label(refresh_row, text="", width=14, bg=P["surface"]).pack(side="left")
        # ttk.Button(refresh_row, text="⟳ Detect Printers", style="Teal.TButton",
        #            command=self._refresh_printers).pack(side="left")
        self._lbl_detect_status = tk.Label(pf, text="", bg=P["surface"], fg=P["muted"], font=FONT_SMALL)
        # self._lbl_detect_status.pack(side="left", padx=(6, 0))

        # UNC Path (for win32print / copy methods)
        row_unc = tk.Frame(pf, bg=P["surface"])
        row_unc.pack(fill="x", pady=3)
        lbl(row_unc, "Queue Path")
        self._v_unc = tk.StringVar(value=r"\\PrintServer\ShareName")
        ttk.Entry(row_unc, textvariable=self._v_unc).pack(
            side="left", fill="x", expand=True)

        # Print Method - HIDDEN (hardcoded to sumatra)
        # Initialize variable but don't show in UI
        default_method = "sumatra"  # SumatraPDF is now default for best PDF support
        self._v_method = tk.StringVar(value=default_method)
        # row_method = tk.Frame(pf, bg=P["surface"])
        # row_method.pack(fill="x", pady=3)
        # lbl(row_method, "Method")
        # method_choices = (
        #     ["sumatra", "shellexec", "win32print", "copy"] if WIN32_AVAILABLE
        #     else ["sumatra", "copy"]
        # )
        # method_combo = ttk.Combobox(
        #     row_method, textvariable=self._v_method,
        #     values=method_choices, state="readonly", width=14)
        # method_combo.pack(side="left")
        # method_combo.bind("<<ComboboxSelected>>", self._on_method_change)


        # ── Print File Section ────────────────────────────────────────────
        ff = section("Print File")
        self._v_file = tk.StringVar()
        file_row = tk.Frame(ff, bg=P["surface"])
        file_row.pack(fill="x", pady=3)
        ttk.Entry(file_row, textvariable=self._v_file).pack(
            side="left", fill="x", expand=True)
        ttk.Button(file_row, text="…", width=3, style="Ghost.TButton",
                   command=self._browse_file).pack(side="left", padx=(4, 0))

        self._v_folder = tk.StringVar()
        folder_row = tk.Frame(ff, bg=P["surface"])
        folder_row.pack(fill="x", pady=3)
        tk.Label(folder_row, text="or Folder:", width=14, anchor="w",
                 bg=P["surface"], fg=P["muted"], font=FONT_UI).pack(side="left")
        ttk.Entry(folder_row, textvariable=self._v_folder).pack(
            side="left", fill="x", expand=True)
        ttk.Button(folder_row, text="…", width=3, style="Ghost.TButton",
                   command=self._browse_folder).pack(side="left", padx=(4, 0))

        self._lbl_file_count = tk.Label(
            ff, text="", bg=P["surface"], fg=P["muted"], font=FONT_UI)
        self._lbl_file_count.pack(fill="x", pady=(2, 0))

        # ── Load Config Section ───────────────────────────────────────────
        lf = section("Load Config")
        self._v_total = tk.IntVar(value=1000)

        total_row = tk.Frame(lf, bg=P["surface"])
        total_row.pack(fill="x", pady=3)
        tk.Label(total_row, text="Jobs / hour", width=14, anchor="w",
                 bg=P["surface"], fg=P["muted"], font=FONT_UI).pack(side="left")
        ttk.Spinbox(total_row, textvariable=self._v_total,
                    from_=1, to=99999, width=8).pack(side="left")

        repeat_row = tk.Frame(lf, bg=P["surface"])
        repeat_row.pack(fill="x", pady=(8, 0))
        self._v_repeat = tk.BooleanVar(value=False)
        ttk.Checkbutton(repeat_row, text="Repeat continuously",
                        variable=self._v_repeat).pack(side="left")

        self._lbl_iteration = tk.Label(
            lf, text="", bg=P["surface"], fg=P["muted"], font=FONT_UI)
        self._lbl_iteration.pack(fill="x", pady=(4, 0))

        # ── Action Buttons ────────────────────────────────────────────────
        btn_frame = tk.Frame(parent, bg=P["bg"])
        btn_frame.pack(fill="x", pady=(4, 0))

        self._btn_start = ttk.Button(
            btn_frame, text="▶  Start Test",
            style="Primary.TButton", command=self._start_test)
        self._btn_start.pack(fill="x", pady=(0, 6))

        self._btn_stop = ttk.Button(
            btn_frame, text="■  Stop",
            style="Danger.TButton", command=self._stop_test, state="disabled")
        self._btn_stop.pack(fill="x", pady=(0, 6))

        row2 = tk.Frame(btn_frame, bg=P["bg"])
        row2.pack(fill="x")
        ttk.Button(row2, text="Clear", style="Ghost.TButton",
                   command=self._clear_results).pack(
                       side="left", fill="x", expand=True, padx=(0, 4))
        ttk.Button(row2, text="Export CSV", style="Ghost.TButton",
                   command=self._export_csv).pack(
                       side="left", fill="x", expand=True)

    # ── Results Panel ─────────────────────────────────────────────────────

    def _build_results(self, parent: tk.Frame) -> None:
        P = PALETTE

        prog_row = tk.Frame(parent, bg=P["bg"])
        prog_row.pack(fill="x", pady=(0, 8))

        self._lbl_progress = tk.Label(
            prog_row, text="Idle", bg=P["bg"], fg=P["muted"], font=FONT_UI)
        self._lbl_progress.pack(side="left")

        self._v_progress = tk.DoubleVar()
        ttk.Progressbar(prog_row, variable=self._v_progress,
                        maximum=100, length=120).pack(
                            side="right", fill="x", expand=True, padx=(12, 0))

        self._tree_wrap = tk.Frame(
            parent, bg=P["surface"],
            highlightbackground=P["border"], highlightthickness=1)
        self._tree_wrap.pack(fill="both", expand=True)

        cols = ("job_id", "status", "filename", "duration_ms", "start_time", "error")
        self._tree = ttk.Treeview(
            self._tree_wrap, columns=cols, show="headings", selectmode="browse")

        col_defs = [
            ("job_id",      "Job #",        60,  "center"),
            ("status",      "Status",       90,  "center"),
            ("filename",    "File",         150, "w"),
            ("duration_ms", "Duration ms",  110, "e"),
            ("start_time",  "Time",         120, "center"),
            ("error",       "Error / Info", 1,   "w"),
        ]
        for col, hdr, width, anchor in col_defs:
            self._tree.heading(col, text=hdr,
                               command=lambda c=col: self._sort_tree(c))
            self._tree.column(col, width=width, anchor=anchor,
                              minwidth=width, stretch=(col == "error"))

        self._tree.tag_configure("Success", foreground=P["success"])
        self._tree.tag_configure("Failed",  foreground=P["danger"])
        self._tree.tag_configure("Running", foreground=P["warning"])
        self._tree.tag_configure("Pending", foreground=P["pending"])

        vsb = ttk.Scrollbar(self._tree_wrap, orient="vertical",
                            command=self._tree.yview)
        self._tree.configure(yscrollcommand=vsb.set)
        vsb.pack(side="right", fill="y")
        self._tree.pack(fill="both", expand=True)

        log_frame = ttk.LabelFrame(parent, text="  Event Log", padding=(8, 6))
        log_frame.pack(fill="x", pady=(10, 0))

        self._log = scrolledtext.ScrolledText(
            log_frame, height=7, state="disabled",
            bg=P["surface"], fg=P["muted"], font=FONT_MONO,
            borderwidth=0, wrap="word", insertbackground=P["text"],
        )
        self._log.pack(fill="x")
        self._log.tag_config("ok",    foreground=P["success"])
        self._log.tag_config("err",   foreground=P["danger"])
        self._log.tag_config("info",  foreground=P["accent"])
        self._log.tag_config("warn",  foreground=P["warning"])
        self._log.tag_config("safeq", foreground=P["safeq"])

    # ── Stats Bar ─────────────────────────────────────────────────────────

    def _build_stats_bar(self) -> None:
        P = PALETTE
        bar = tk.Frame(self, bg=P["surface"], height=56)
        bar.pack(fill="x", side="bottom")
        bar.pack_propagate(False)

        tk.Frame(bar, bg=P["border"], height=1).pack(fill="x", side="top")

        inner = tk.Frame(bar, bg=P["surface"])
        inner.pack(expand=True)

        stats = [
            ("Total",      "0"),
            ("Success",    "0"),
            ("Failed",     "0"),
            ("Avg ms",     "—"),
            ("Min ms",     "—"),
            ("Max ms",     "—"),
            ("Throughput", "—"),
        ]
        self._stat_vals: dict[str, tk.Label] = {}

        for label, default in stats:
            cell = tk.Frame(inner, bg=P["surface"])
            cell.pack(side="left", padx=18, pady=8)
            tk.Label(cell, text=label.upper(), bg=P["surface"], fg=P["muted"],
                     font=("Segoe UI", 9, "bold")).pack()
            val_lbl = tk.Label(cell, text=default, bg=P["surface"], fg=P["text"],
                               font=("Segoe UI", 14, "bold"))
            val_lbl.pack()
            self._stat_vals[label] = val_lbl

        self._lbl_elapsed = tk.Label(
            bar, text="", bg=P["surface"], fg=P["muted"], font=FONT_UI)
        self._lbl_elapsed.pack(side="right", padx=20)
        self._test_start_time: float = 0.0

    # ── Printer Discovery ─────────────────────────────────────────────────
    # DISABLED - printer name field is hidden

    def _discover_printers(self) -> None:
        """Background thread: enumerate installed printers."""
        # Disabled - using hardcoded sumatra method
        return
        # printers = get_installed_printers()
        # self._installed_printers = printers
        # # Update combo on main thread
        # self.after(0, self._populate_printer_combo, printers)

    def _populate_printer_combo(self, printers: list[str]) -> None:
        # Disabled - combo box is hidden
        return
        # self._combo_printer["values"] = printers
        # count = len(printers)
        # if count:
        #     self._lbl_detect_status.config(
        #         text=f"{count} found", fg=PALETTE["success"])
        #     # Auto-select first SafeQ printer if recognisable
        #     safeq_names = [p for p in printers
        #                    if "safeq" in p.lower() or "ysoftsafeq" in p.lower()
        #                    or "safe" in p.lower()]
        #     if safeq_names and not self._v_printer_name.get():
        #         self._v_printer_name.set(safeq_names[0])
        #     elif printers and not self._v_printer_name.get():
        #         self._v_printer_name.set(printers[0])
        # else:
        #     self._lbl_detect_status.config(
        #         text="none found", fg=PALETTE["warning"])

    def _refresh_printers(self) -> None:
        # Disabled - using hardcoded sumatra method
        return
        # self._lbl_detect_status.config(
        #     text="scanning…", fg=PALETTE["muted"])
        # threading.Thread(target=self._discover_printers, daemon=True).start()
        # self._log_event("Scanning for installed printers…", tag="info")


  
    # ── Event Handlers ────────────────────────────────────────────────────

    def _browse_file(self) -> None:
        path = filedialog.askopenfilename(
            title="Select Print File",
            filetypes=[
                ("Printable files",
                 "*.pdf *.txt *.prn *.pcl *.ps *.png *.jpg *.bmp "
                 "*.doc *.docx *.xls *.xlsx *.ppt *.pptx"),
                ("PDF",    "*.pdf"),
                ("Office", "*.doc *.docx *.xls *.xlsx *.ppt *.pptx"),
                ("Text",   "*.txt *.prn"),
                ("Image",  "*.png *.jpg *.bmp"),
                ("All files", "*.*"),
            ],
        )
        if path:
            self._v_file.set(path)
            self._v_folder.set("")
            self._folder_files.clear()
            self._lbl_file_count.config(text="")

    def _browse_folder(self) -> None:
        path = filedialog.askdirectory(title="Select Folder with Print Files")
        if path:
            self._v_folder.set(path)
            self._v_file.set("")
            self._load_folder_files(path)

    def _load_folder_files(self, folder_path: str) -> None:
        self._folder_files.clear()
        exts = (".pdf", ".txt", ".prn", ".pcl", ".ps",
                ".png", ".jpg", ".jpeg", ".bmp",
                ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx")
        try:
            for fn in os.listdir(folder_path):
                fp = os.path.join(folder_path, fn)
                if os.path.isfile(fp) and fn.lower().endswith(exts):
                    self._folder_files.append(fp)
            self._folder_files.sort()
            count = len(self._folder_files)
            if count:
                self._lbl_file_count.config(
                    text=f"✓ Found {count} printable file{'s' if count != 1 else ''}")
                self._log_event(
                    f"Loaded {count} files from folder: {folder_path}", tag="info")
            else:
                self._lbl_file_count.config(text="⚠ No printable files found")
                self._log_event(
                    f"No printable files found in: {folder_path}", tag="warn")
        except Exception as e:
            self._lbl_file_count.config(text="✗ Error reading folder")
            self._log_event(f"Error loading folder: {e}", tag="err")
            self._folder_files.clear()

    def _start_test(self) -> None:
        method       = self._v_method.get()
        printer_name = self._v_printer_name.get().strip()
        printer_unc  = self._v_unc.get().strip()
        filepath     = self._v_file.get().strip()
        folder       = self._v_folder.get().strip()

        # ── Validation ────────────────────────────────────────────────────
        if method == "shellexec":
            if not printer_name:
                messagebox.showerror(
                    "Missing Input",
                    "shellexec requires a Printer Name.\n\n"
                    "Click '⟳ Detect Printers' or type the name that appears in\n"
                    "Windows Settings → Printers & scanners.")
                return
        else:
            if not printer_unc or printer_unc == r"\\PrintServer\ShareName":
                messagebox.showerror(
                    "Missing Input",
                    "Please enter the printer Queue path.\n\nExample: \\\\PrintServer\\PrinterName")
                return
            if not printer_unc.startswith("\\\\"):
                messagebox.showerror(
                    "Invalid Queue Path",
                    "Queue path must start with \\\\ (e.g., \\\\PrintServer\\ShareName)")
                return

        use_folder = bool(folder and self._folder_files)
        if use_folder:
            pass
        elif filepath and os.path.exists(filepath):
            pass
        else:
            messagebox.showerror(
                "Missing Input",
                "Select a valid print file or a folder containing printable files.")
            return

        total = self._v_total.get()

        if total > 3600:
            if not messagebox.askokcancel(
                "High Job Count Warning",
                f"You are about to submit {total:,} print jobs.\n\n"
                f"• ~{total/3600:.1f} jobs/second\n"
                f"• {total:,} rows in the results grid\n"
                f"• May overload the print server\n\nContinue?",
                icon="warning",
            ):
                return

        jobs_per_sec = total / 3600.0
        delay_s      = 1.0 / jobs_per_sec if jobs_per_sec > 0 else 0.0
        max_workers  = max(5, min(50, int(jobs_per_sec * 2))) if jobs_per_sec > 0 else 20

        files_to_print = self._folder_files[:] if use_folder else [filepath]

        if not self._repeat_config:
            self._results.clear()
            self._iteration_count = 0
            for item in self._tree.get_children():
                self._tree.delete(item)

        self._total_submitted = total
        self._v_progress.set(0)
        self._running = True
        self._test_start_time = time.time()
        self._btn_start.config(state="disabled")
        self._btn_stop.config(state="normal")

        self._repeat_config = {
            "printer_name": printer_name,
            "printer_unc":  printer_unc,
            "files_to_print": files_to_print[:],
            "total":        total,
            "max_workers":  max_workers,
            "delay_s":      delay_s,
            "method":       method,
            "use_folder":   use_folder,
        }

        file_info = (f"{len(files_to_print)} files from folder"
                     if use_folder else os.path.basename(filepath))
        target_label = printer_name if method == "shellexec" else printer_unc

        self._log_event(
            f"Starting load test → {target_label}",
            tag="info",
            detail=(f"Jobs: {total} over 1 hr ({jobs_per_sec:.3f}/s)  "
                    f"Workers: {max_workers}  Method: {method}  File(s): {file_info}"),
        )
        if method == "shellexec":
            self._log_event(
                "SafeQ mode: jobs routed via Windows spooler — "
                "Desktop Interface popup should fire.", tag="safeq")

        threading.Thread(
            target=self._run_load_test,
            args=(printer_name, printer_unc, files_to_print,
                  total, max_workers, 1, delay_s, method),
            daemon=True,
        ).start()

        self._tick_elapsed()

    def _stop_test(self) -> None:
        self._running = False
        self._repeat_config = None
        self._btn_stop.config(state="disabled")
        self._log_event("Stop requested — draining in-flight jobs…", tag="warn")

    def _restart_test(self) -> None:
        if not self._repeat_config:
            return
        cfg = self._repeat_config

        self._running = True
        self._test_start_time = time.time()
        self._btn_start.config(state="disabled")
        self._btn_stop.config(state="normal")

        file_info = (f"{len(cfg['files_to_print'])} files from folder"
                     if cfg["use_folder"]
                     else os.path.basename(cfg["files_to_print"][0]))
        target = cfg["printer_name"] if cfg["method"] == "shellexec" else cfg["printer_unc"]

        self._log_event(
            f"Starting iteration {self._iteration_count + 1} → {target}",
            tag="info",
            detail=f"Jobs: {cfg['total']}  Method: {cfg['method']}  File(s): {file_info}",
        )

        threading.Thread(
            target=self._run_load_test,
            args=(cfg["printer_name"], cfg["printer_unc"], cfg["files_to_print"],
                  cfg["total"], cfg["max_workers"], 1, cfg["delay_s"], cfg["method"]),
            daemon=True,
        ).start()

        self._tick_elapsed()

    def _clear_results(self) -> None:
        if self._running:
            messagebox.showwarning("Running", "Stop the test before clearing.")
            return
        self._results.clear()
        self._iteration_count = 0
        self._lbl_iteration.config(text="")
        for item in self._tree.get_children():
            self._tree.delete(item)
        self._v_progress.set(0)
        self._lbl_progress.config(text="Idle")
        self._lbl_elapsed.config(text="")
        self._update_stats()
        self._log_event("Results cleared.", tag="info")

    def _export_csv(self) -> None:
        if not self._results:
            messagebox.showinfo("Export", "No results to export yet.")
            return
        path = filedialog.asksaveasfilename(
            defaultextension=".csv",
            filetypes=[("CSV", "*.csv"), ("All files", "*.*")],
            initialfile=f"print_load_{datetime.now():%Y%m%d_%H%M%S}.csv",
        )
        if not path:
            return
        with open(path, "w", newline="", encoding="utf-8") as fh:
            w = csv.writer(fh)
            w.writerow(["Job#", "Status", "Duration_ms", "Start_Time",
                        "Printer", "File", "Error"])
            for r in self._results:
                w.writerow([
                    r.job_id, r.status, f"{r.duration_ms:.2f}",
                    datetime.fromtimestamp(r.start_time).strftime("%H:%M:%S.%f")[:-3],
                    r.printer, os.path.basename(r.file_path), r.error,
                ])
        self._log_event(f"Exported {len(self._results)} rows → {path}", tag="ok")
        messagebox.showinfo("Export Complete", f"Saved to:\n{path}")

    def _sort_tree(self, col: str) -> None:
        items = [(self._tree.set(k, col), k) for k in self._tree.get_children("")]
        try:
            items.sort(key=lambda t: float(t[0]))
        except ValueError:
            items.sort()
        for i, (_, k) in enumerate(items):
            self._tree.move(k, "", i)

    # ── Load Test Engine ──────────────────────────────────────────────────

    def _run_load_test(self, printer_name: str, printer_unc: str,
                       files_to_print: list[str], total: int,
                       max_workers: int, copies: int, delay_s: float,
                       method: str) -> None:
        """Core load test — runs in background thread."""
        
        def result_callback(future, job_id_val, printer_val, files_val):
            """Called immediately when a job completes."""
            try:
                result = future.result()
            except Exception as exc:
                result = JobResult(
                    job_id=job_id_val, printer=printer_val,
                    file_path=files_val[0],
                    start_time=time.time(), end_time=time.time(),
                    status="Failed", error=str(exc),
                )
            # Send result to UI immediately
            self._queue.put(("result", result))
        
        with concurrent.futures.ThreadPoolExecutor(max_workers=max_workers) as pool:
            # Submit all jobs with pacing
            for job_id in range(1, total + 1):
                if not self._running:
                    break

                file_index   = (job_id - 1) % len(files_to_print)
                current_file = files_to_print[file_index]

                # Show job as "Running" in UI immediately
                self._queue.put(("pending", job_id))

                # Submit job to worker pool with callback for immediate result handling
                fut = pool.submit(
                    send_print_job,
                    job_id, printer_name, printer_unc, current_file, copies, method,
                )
                # Attach callback to send result as soon as this job completes
                fut.add_done_callback(
                    lambda f, jid=job_id, p=printer_name or printer_unc, fps=files_to_print: 
                        result_callback(f, jid, p, fps)
                )

                if delay_s > 0:
                    time.sleep(delay_s)
            
            # The 'with' block will automatically wait for all jobs to complete

        self._queue.put(("done", None))

    # ── Queue Consumer ────────────────────────────────────────────────────

    def _poll_queue(self) -> None:
        processed = 0
        try:
            while processed < 100:
                msg, payload = self._queue.get_nowait()
                processed += 1

                if msg == "pending":
                    job_id: int = payload
                    iid = str(job_id)
                    if not self._tree.exists(iid):
                        start_str = datetime.now().strftime("%H:%M:%S.%f")[:-3]
                        self._tree.insert(
                            "", "end", iid=iid,
                            values=(job_id, "Running", "...", "—", start_str, ""),
                            tags=("Running",),
                        )
                        self._tree.yview_moveto(1.0)

                elif msg == "result":
                    r: JobResult = payload
                    self._results.append(r)
                    iid = str(r.job_id)
                    start_str = datetime.fromtimestamp(r.start_time).strftime(
                        "%H:%M:%S.%f")[:-3]
                    dur_str  = f"{r.duration_ms:.0f}" if r.end_time > 0 else "—"
                    filename = os.path.basename(r.file_path)

                    if self._tree.exists(iid):
                        self._tree.item(
                            iid,
                            values=(r.job_id, r.status, filename,
                                    dur_str, start_str, r.error),
                            tags=(r.status,),
                        )
                        # Force immediate visual update for this specific row
                        self._tree.see(iid)
                        self.update_idletasks()
                    else:
                        self._tree.insert(
                            "", "end", iid=iid,
                            values=(r.job_id, r.status, filename,
                                    dur_str, start_str, r.error),
                            tags=(r.status,),
                        )
                        self._tree.yview_moveto(1.0)

                    done = len(self._results)
                    pct  = (done / self._total_submitted * 100
                            if self._total_submitted else 0)
                    self._v_progress.set(pct)
                    self._lbl_progress.config(
                        text=f"{done} / {self._total_submitted} jobs")

                    icon    = "✓" if r.status == "Success" else "✗"
                    tag     = "ok" if r.status == "Success" else "err"
                    msg_txt = (
                        f"[{icon}] Job #{r.job_id:>4}  {dur_str:>7} ms"
                        + (f"  ↳ {r.error}" if r.error else "")
                    )
                    self._log_event(msg_txt, tag=tag)
                    self._update_stats()

                elif msg == "log":
                    text, tag = (payload if isinstance(payload, tuple)
                                 else (payload, "info"))
                    self._log_event(text, tag=tag)

                elif msg == "done":
                    self._iteration_count += 1
                    self._running = False
                    elapsed = time.time() - self._test_start_time

                    self._lbl_iteration.config(
                        text=f"Iteration {self._iteration_count} completed")
                    self._log_event(
                        f"Iteration {self._iteration_count} complete  ·  "
                        f"{len(self._results)} total jobs  ·  {elapsed:.1f}s",
                        tag="info",
                    )
                    self._update_stats()

                    if self._v_repeat.get() and self._repeat_config:
                        self._lbl_progress.config(text="Restarting…")
                        self._log_event(
                            "Repeat mode — restarting in 2 s…", tag="info")
                        self.after(2000, self._restart_test)
                    else:
                        self._btn_start.config(state="normal")
                        self._btn_stop.config(state="disabled")
                        self._lbl_progress.config(text="Complete")
                        self._repeat_config = None

                # Force UI update every 10 messages for better real-time visibility
                if processed % 10 == 0:
                    self.update()

        except queue.Empty:
            pass

        if processed > 0:
            self.update()

        # Poll more frequently (20ms) for better real-time updates
        self.after(20, self._poll_queue)

    # ── Stats & Utilities ─────────────────────────────────────────────────

    def _update_stats(self) -> None:
        P = PALETTE
        results = self._results
        total   = len(results)
        success = sum(1 for r in results if r.status == "Success")
        failed  = sum(1 for r in results if r.status == "Failed")
        durs    = [r.duration_ms for r in results if r.end_time > 0]

        self._stat_vals["Total"].config(text=str(total), fg=P["text"])
        self._stat_vals["Success"].config(
            text=str(success),
            fg=P["success"] if success > 0 else P["text"])
        self._stat_vals["Failed"].config(
            text=str(failed),
            fg=P["danger"] if failed > 0 else P["text"])

        if durs:
            avg = sum(durs) / len(durs)
            self._stat_vals["Avg ms"].config(text=f"{avg:.0f}", fg=P["text"])
            self._stat_vals["Min ms"].config(text=f"{min(durs):.0f}", fg=P["text"])
            self._stat_vals["Max ms"].config(text=f"{max(durs):.0f}", fg=P["text"])
            elapsed = time.time() - self._test_start_time
            tps = len(durs) / elapsed if elapsed > 0 else 0
            self._stat_vals["Throughput"].config(text=f"{tps:.1f}/s", fg=P["text"])
        else:
            for key in ("Avg ms", "Min ms", "Max ms", "Throughput"):
                self._stat_vals[key].config(text="—", fg=P["text"])

    def _log_event(self, message: str, tag: str = "info",
                   detail: str = "") -> None:
        ts = datetime.now().strftime("%H:%M:%S.%f")[:-3]
        self._log.config(state="normal")
        self._log.insert("end", f"[{ts}] ", "info")
        self._log.insert("end", message + "\n", tag)
        if detail:
            self._log.insert("end", f"          {detail}\n", "info")
        self._log.yview_moveto(1.0)
        self._log.config(state="disabled")

    def _tick_elapsed(self) -> None:
        if not self._running:
            return
        elapsed = time.time() - self._test_start_time
        self._lbl_elapsed.config(text=f"Elapsed: {elapsed:.1f}s")
        self.after(500, self._tick_elapsed)

    # ── Settings Persistence ──────────────────────────────────────────────

    def _load_settings(self) -> None:
        if not CONFIG_FILE.exists():
            return
        try:
            with open(CONFIG_FILE, "r") as f:
                config = json.load(f)

            printer_name = config.get("printer_name", "")
            if printer_name:
                self._v_printer_name.set(printer_name)

            printer_unc = config.get("printer_unc", "")
            if printer_unc and printer_unc != r"\\PrintServer\ShareName":
                self._v_unc.set(printer_unc)

            method = config.get("method", "sumatra")
            # Always use sumatra, ignore saved method
            self._v_method.set("sumatra")
            # self._on_method_change()  # Disabled - method is hidden

            file_path   = config.get("file_path", "")
            folder_path = config.get("folder_path", "")

            if folder_path and os.path.exists(folder_path):
                self._v_folder.set(folder_path)
                self._load_folder_files(folder_path)
            elif file_path and os.path.exists(file_path):
                self._v_file.set(file_path)

            self._v_repeat.set(config.get("repeat_mode", False))
        except Exception:
            pass

    def _save_settings(self) -> None:
        try:
            config = {
                "printer_name": self._v_printer_name.get(),
                "printer_unc":  self._v_unc.get(),
                "method":       self._v_method.get(),
                "file_path":    self._v_file.get(),
                "folder_path":  self._v_folder.get(),
                "repeat_mode":  self._v_repeat.get(),
            }
            with open(CONFIG_FILE, "w") as f:
                json.dump(config, f, indent=2)
        except Exception:
            pass

    def _on_close(self) -> None:
        self._save_settings()
        self.destroy()


# ════════════════════════════════════════════════════════════════════════════
#  Entry Point
# ════════════════════════════════════════════════════════════════════════════

if __name__ == "__main__":
    app = PrintLoadTesterApp()
    app.mainloop()
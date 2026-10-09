#!/usr/bin/env python3
"""
Print Load Tester v1.8
──────────────────────
Concurrent Windows printer load testing tool with full diagnostic logging.
Every print attempt is logged to:
  %USERPROFILE%\\Desktop\\print_load_tester.log

Method is hardcoded to win32print (hidden from UI).
File-type routing:
  .pdf                      → PowerShell Print verb (default printer set temporarily)
  .png / .jpg / .jpeg / .bmp → Pillow + win32ui GDI rendering
  .prn / .pcl / .ps / .txt  → win32print RAW bytes via spooler

Dependencies:
  pip install pywin32 pillow
"""

import tkinter as tk
from tkinter import ttk, filedialog, messagebox, scrolledtext
import threading
import concurrent.futures
import time
import os
import queue
import csv
import subprocess
import logging
import sys
from datetime import datetime
from dataclasses import dataclass
import json
from pathlib import Path
import win32print
import win32ui
from PIL import Image, ImageDraw, ImageFont
import img2pdf  
import fitz 
import io
import openpyxl
from docx import Document


# ── Optional Windows APIs ────────────────────────────────────────────────────
try:   
    import win32api     # type: ignore
    import win32con     # type: ignore    
    WIN32_AVAILABLE = True
except ImportError:
    WIN32_AVAILABLE = False

try:
    from PIL import Image, ImageWin  # type: ignore
    PIL_AVAILABLE = True
except ImportError:
    PIL_AVAILABLE = False

# ── Config ────────────────────────────────────────────────────────────────────
CONFIG_FILE   = Path.home() / ".print_load_tester.json"
LOG_FILE      = Path.home() / "Desktop" / "print_load_tester.log"
FIXED_METHOD  = "win32print"   # hardcoded — not shown in UI


# ── Logging setup ─────────────────────────────────────────────────────────────
def _setup_logger() -> logging.Logger:
    LOG_FILE.parent.mkdir(parents=True, exist_ok=True)
    fmt = logging.Formatter(
        "%(asctime)s.%(msecs)03d  [%(levelname)-8s]  %(message)s",
        datefmt="%Y-%m-%d %H:%M:%S",
    )
    logger = logging.getLogger("PrintLoadTester")
    logger.setLevel(logging.DEBUG)
    if not logger.handlers:
        fh = logging.FileHandler(LOG_FILE, encoding="utf-8", mode="a")
        fh.setLevel(logging.DEBUG)
        fh.setFormatter(fmt)
        logger.addHandler(fh)
        ch = logging.StreamHandler(sys.stdout)
        ch.setLevel(logging.INFO)
        ch.setFormatter(fmt)
        logger.addHandler(ch)
    return logger

log = _setup_logger()


# ════════════════════════════════════════════════════════════════════════════
#  Data Model
# ════════════════════════════════════════════════════════════════════════════

@dataclass
class JobResult:
    job_id: int
    printer: str
    file_path: str
    start_time: float
    end_time: float = 0.0
    status: str = "Pending"
    error: str = ""

    @property
    def duration_ms(self) -> float:
        return (self.end_time - self.start_time) * 1000.0 if self.end_time > 0 else 0.0


# ════════════════════════════════════════════════════════════════════════════
#  Printer Discovery
# ════════════════════════════════════════════════════════════════════════════

def get_installed_printers() -> list[str]:
    names: list[str] = []
    if WIN32_AVAILABLE:
        try:
            for p in win32print.EnumPrinters(
                    win32print.PRINTER_ENUM_LOCAL | win32print.PRINTER_ENUM_CONNECTIONS,
                    None, 2):
                name = p.get("pPrinterName", "")
                if name:
                    names.append(name)
            log.debug("win32print found %d printers: %s", len(names), names)
            if names:
                return sorted(names)
        except Exception as exc:
            log.warning("win32print EnumPrinters failed: %s", exc)
    try:
        result = subprocess.run(
            ["powershell", "-NoProfile", "-NonInteractive", "-Command",
             "Get-Printer | Select-Object -ExpandProperty Name"],
            capture_output=True, timeout=10,
        )
        log.debug("Get-Printer rc=%d stdout=%r stderr=%r",
                  result.returncode,
                  result.stdout.decode(errors="replace"),
                  result.stderr.decode(errors="replace"))
        if result.returncode == 0:
            for line in result.stdout.decode(errors="replace").splitlines():
                name = line.strip()
                if name:
                    names.append(name)
    except Exception as exc:
        log.warning("PowerShell Get-Printer failed: %s", exc)
    return sorted(names)


# ════════════════════════════════════════════════════════════════════════════
#  Print Workers
# ════════════════════════════════════════════════════════════════════════════

def _make_job_name(job_id: int, file_path: str) -> str:
    return f"[{job_id:04d}] {os.path.basename(file_path)}"


def send_print_job(job_id: int, printer_name: str, printer_unc: str,
                   file_path: str, copies: int) -> JobResult:
    """Entry point for each worker thread."""
    result = JobResult(
        job_id=job_id,
        printer=printer_name or printer_unc,
        file_path=file_path,
        start_time=time.time(),
        status="Running",
    )
    job_name = _make_job_name(job_id, file_path)
    ext = os.path.splitext(file_path)[1].lower()

    log.info("JOB %04d START  printer=%r  file=%r  ext=%s  copies=%d",
             job_id, printer_name or printer_unc, file_path, ext, copies)
    try:
        if ext == ".pdf":
            log.debug("JOB %04d  routing: PDF → PowerShell PrintTo", job_id)
            _print_via_spooler_pdf(printer_name, file_path, copies,job_id)

        elif ext in (".png", ".jpg", ".jpeg", ".bmp", ".tif", ".tiff"):
            log.debug("JOB %04d  routing: image → Pillow GDI", job_id)
            _print_image_pillow(job_id, printer_name, file_path, copies)

        elif ext == ".docx":
            _print_docx(job_id, printer_name, file_path, copies)

        elif ext in (".xlsx", ".xls"):
            log.debug("JOB %04d  routing: image → Pillow GDI", job_id)
            _print_xlsx(job_id, printer_name, file_path, copies)    

        else:
            log.debug("JOB %04d  routing: raw → win32print spooler", job_id)
            _print_via_win32_spooler(job_id, printer_name, file_path, copies, job_name)

        result.status = "Success"
        log.info("JOB %04d SUCCESS  duration=%.0fms",
                 job_id, (time.time() - result.start_time) * 1000)

    except Exception as exc:
        result.status = "Failed"
        result.error = str(exc)
        log.error("JOB %04d FAILED  error=%s", job_id, exc, exc_info=True)

    finally:
        result.end_time = time.time()

    return result

def _print_docx(job_id: int, printer_name: str,
                         file_path: str, copies: int):
    doc = Document(file_path)

    # Extract all paragraphs as text lines
    lines = [p.text for p in doc.paragraphs]

    pdc = win32ui.CreateDC()
    pdc.CreatePrinterDC(printer_name)

    printer_w = pdc.GetDeviceCaps(win32con.HORZRES)
    printer_h = pdc.GetDeviceCaps(win32con.VERTRES)

    for copy_num in range(copies):
        pdc.StartDoc(file_path)
        _render_text_to_dc(pdc, lines, printer_w, printer_h)
        pdc.EndDoc()

    pdc.DeleteDC()

def _print_xlsx(job_id: int, printer_name: str,
                         file_path: str, copies: int):
    wb = openpyxl.load_workbook(file_path, data_only=True)

    pdc = win32ui.CreateDC()
    pdc.CreatePrinterDC(printer_name)

    printer_w = pdc.GetDeviceCaps(win32con.HORZRES)
    printer_h = pdc.GetDeviceCaps(win32con.VERTRES)

    for copy_num in range(copies):
        pdc.StartDoc(file_path)

        for sheet in wb.worksheets:
            # Convert sheet rows to text lines
            lines = []
            for row in sheet.iter_rows(values_only=True):
                line = "  |  ".join(
                    str(cell) if cell is not None else ""
                    for cell in row
                )
                lines.append(line)

            _render_text_to_dc(pdc, lines, printer_w, printer_h)

        pdc.EndDoc()

    pdc.DeleteDC()

def _render_text_to_dc(pdc, lines, printer_w, printer_h):
    """Render text lines onto printer DC page by page."""
    margin = 100
    font_size = 20
    line_height = font_size + 8

    try:
        font = ImageFont.truetype("arial.ttf", font_size)
    except:
        font = ImageFont.load_default()

    lines_per_page = (printer_h - 2 * margin) // line_height
    pages = [lines[i:i + lines_per_page]
             for i in range(0, max(len(lines), 1), lines_per_page)]

    for page_lines in pages:
        img = Image.new("RGB", (printer_w, printer_h), "white")
        draw = ImageDraw.Draw(img)

        y = margin
        for line in page_lines:
            if line:
                draw.text((margin, y), line, fill="black", font=font)
            y += line_height

        pdc.StartPage()

        # ✅ Replaces BMP → BytesIO → LoadBitmapFile → MemoryDC → StretchBlt
        dib = ImageWin.Dib(img)
        dib.draw(pdc.GetHandleOutput(), (0, 0, printer_w, printer_h))

        pdc.EndPage()
        
# ── Image via Pillow + win32ui GDI ────────────────────────────────────────────

def _print_image_pillow(job_id: int, printer_name: str,
                         file_path: str, copies: int) -> None:
    """
    Render an image directly to the printer DC using Pillow + win32ui.
    Routes through the Windows spooler → SafeQ popup fires.
    Requires: pip install pywin32 pillow
    """
    if not WIN32_AVAILABLE:
        raise RuntimeError("pywin32 not installed — run: pip install pywin32")
    if not PIL_AVAILABLE:
        raise RuntimeError("Pillow not installed — run: pip install pillow")

    for copy_num in range(copies):
        log.debug("JOB %04d  image copy %d/%d  file=%r",
                  job_id, copy_num + 1, copies, file_path)
        hDC = win32ui.CreateDC()
        try:
            hDC.CreatePrinterDC(printer_name)

            # Physical page size in device units
            page_w = hDC.GetDeviceCaps(110)   # PHYSICALWIDTH
            page_h = hDC.GetDeviceCaps(111)   # PHYSICALHEIGHT
            # Printable area offset
            offset_x = hDC.GetDeviceCaps(112)  # PHYSICALOFFSETX
            offset_y = hDC.GetDeviceCaps(113)  # PHYSICALOFFSETY
            printable_w = hDC.GetDeviceCaps(8)  # HORZRES
            printable_h = hDC.GetDeviceCaps(10) # VERTRES

            log.debug("JOB %04d  page=%dx%d  printable=%dx%d  offset=%d,%d",
                      job_id, page_w, page_h, printable_w, printable_h,
                      offset_x, offset_y)

            img = Image.open(file_path)
            # Convert unsupported modes to RGB
            if img.mode not in ("RGB", "L"):
                img = img.convert("RGB")

            # Scale image to fit printable area while preserving aspect ratio
            img_w, img_h = img.size
            scale = min(printable_w / img_w, printable_h / img_h)
            new_w = int(img_w * scale)
            new_h = int(img_h * scale)
            img = img.resize((new_w, new_h), Image.LANCZOS)

            # Centre on the printable area
            x = (printable_w - new_w) // 2
            y = (printable_h - new_h) // 2

            hDC.StartDoc(os.path.basename(file_path))
            hDC.StartPage()
            dib = ImageWin.Dib(img)
            dib.draw(hDC.GetHandleOutput(), (x, y, x + new_w, y + new_h))
            hDC.EndPage()
            hDC.EndDoc()
            log.debug("JOB %04d  image rendered %dx%d at (%d,%d)",
                      job_id, new_w, new_h, x, y)
        finally:
            hDC.DeleteDC()

        if copies > 1:
            time.sleep(0.3)

# ── Win32 Spooler ─────────────────────────────────────────────────────────────
def _print_via_win32_spooler(job_id: int, printer_target: str,
                              file_path: str, copies: int, job_name: str) -> None:
    ext = os.path.splitext(file_path)[1].lower()
    raw_types = {".prn", ".pcl", ".ps"}
    log.debug("JOB %04d  win32_spooler  target=%r  ext=%s  raw=%s",
              job_id, printer_target, ext, ext in raw_types)
    if ext in raw_types:
        h_printer = win32print.OpenPrinter(printer_target)
        try:
            with open(file_path, "rb") as fh:
                data = fh.read()
            log.debug("JOB %04d  read %d bytes from file", job_id, len(data))
            for copy_num in range(copies):
                log.debug("JOB %04d  copy %d/%d  StartDocPrinter", job_id,
                          copy_num + 1, copies)
                win32print.StartDocPrinter(h_printer, 1, (job_name, None, "RAW"))
                try:
                    win32print.StartPagePrinter(h_printer)
                    written = win32print.WritePrinter(h_printer, data)
                    log.debug("JOB %04d  WritePrinter wrote %s bytes", job_id, written)
                    win32print.EndPagePrinter(h_printer)
                finally:
                    win32print.EndDocPrinter(h_printer)
        finally:
            win32print.ClosePrinter(h_printer)
    else:
        # ShellExecute PrintTo — routes through registered app + Windows spooler
        for copy_num in range(copies):
            params = f'"{printer_target}"'
            log.debug("JOB %04d  copy %d/%d  ShellExecute printto  "
                      "file=%r  params=%r",
                      job_id, copy_num + 1, copies, file_path, params)
            ret = win32api.ShellExecute(
                0, "printto", file_path, params,
                os.path.dirname(file_path), win32con.SW_HIDE,
            )
            log.debug("JOB %04d  ShellExecute returned %s (>32 = ok)", job_id, ret)
            if ret <= 32:
                raise RuntimeError(
                    f"ShellExecute PrintTo returned error code {ret}. "
                    "The file type may not have a PrintTo handler registered. "
                    "Install Adobe Reader/Edge for PDFs or Microsoft Office "
                    "for Office files.")
            if copies > 1:
                time.sleep(0.5)


def _print_via_spooler_pdf_working(printer_name: str, file_path: str,
                        copies: int, job_name: str) -> None:
    doc = fitz.open(file_path)

    pdc = win32ui.CreateDC()
    pdc.CreatePrinterDC(printer_name)

    pdc.StartDoc(file_path)

    for page_num in range(len(doc)):
        page = doc[page_num]

        # Render page to pixmap at 200 DPI
        mat = fitz.Matrix(200 / 72, 200 / 72)
        pix = page.get_pixmap(matrix=mat)

        # Convert to PIL Image
        img = Image.frombytes("RGB", [pix.width, pix.height], pix.samples)

        # Get printer page size
        printer_w = pdc.GetDeviceCaps(win32con.HORZRES)
        printer_h = pdc.GetDeviceCaps(win32con.VERTRES)

        # Resize image to fit printer page
        img = img.resize((printer_w, printer_h), Image.LANCZOS)

        # Convert PIL image to BITMAPINFO format for win32
        img_bmp = io.BytesIO()
        img.save(img_bmp, format="BMP")
        img_bmp.seek(0)

        # Create a compatible memory DC and bitmap
        mdc = pdc.CreateCompatibleDC()
        bmp = win32ui.CreateBitmap()
        bmp.LoadBitmapFile(img_bmp)          # ← load BMP bytes

        mdc.SelectObject(bmp)

        pdc.StartPage()

        # StretchBlt: copy from memory DC → printer DC
        pdc.StretchBlt(
            (0, 0),                          # dest origin
            (printer_w, printer_h),          # dest size
            mdc,                             # source DC
            (0, 0),                          # source origin
            (img.width, img.height),         # source size
            win32con.SRCCOPY
        )

        pdc.EndPage()

        mdc.DeleteDC()

    pdc.EndDoc()
    pdc.DeleteDC()
    doc.close()

def _print_via_spooler_pdf(printer_name: str, file_path: str,
                            copies: int, job_id: int) -> None:
    doc = fitz.open(file_path)

    pdc = win32ui.CreateDC()
    pdc.CreatePrinterDC(printer_name)

    printer_w = pdc.GetDeviceCaps(win32con.HORZRES)
    printer_h = pdc.GetDeviceCaps(win32con.VERTRES)

    for copy_num in range(copies):
        pdc.StartDoc(file_path)

        for page_num in range(len(doc)):
            page = doc[page_num]

            mat = fitz.Matrix(200 / 72, 200 / 72)
            pix = page.get_pixmap(matrix=mat, colorspace=fitz.csRGB, alpha=False)

            img = Image.frombytes("RGB", [pix.width, pix.height], pix.samples)
            img = img.resize((printer_w, printer_h), Image.LANCZOS)

            pdc.StartPage()

            # ✅ Dib.draw — no StretchBlt, no memory DC, no BMP roundtrip
            dib = ImageWin.Dib(img)
            dib.draw(pdc.GetHandleOutput(), (0, 0, printer_w, printer_h))

            pdc.EndPage()

        pdc.EndDoc()

    pdc.DeleteDC()
    doc.close()
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
    "safeq":    "#0f766e",
}

FONT_MONO  = ("Consolas", 11)
FONT_UI    = ("Segoe UI", 11)
FONT_BOLD  = ("Segoe UI", 11, "bold")
FONT_SMALL = ("Segoe UI", 9)


# ════════════════════════════════════════════════════════════════════════════
#  Main Application
# ════════════════════════════════════════════════════════════════════════════

class PrintLoadTesterApp(tk.Tk):

    def __init__(self):
        super().__init__()
        self.title("Print Load Tester  v1.8")
        self.geometry("1160x780")
        self.minsize(960, 640)
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

        log.info("=" * 70)
        log.info("Print Load Tester v1.8 started  (method=win32print, hidden)")
        log.info("Log file: %s", LOG_FILE)
        log.info("pywin32=%s  pillow=%s", WIN32_AVAILABLE, PIL_AVAILABLE)
        log.info("=" * 70)

        threading.Thread(target=self._discover_printers, daemon=True).start()

    # ── ttk Styles ────────────────────────────────────────────────────────

    def _apply_styles(self) -> None:
        s = ttk.Style(self)
        s.theme_use("clam")
        P = PALETTE
        s.configure("TFrame",       background=P["bg"])
        s.configure("Card.TFrame",  background=P["surface"])
        s.configure("TLabel",
                    background=P["bg"], foreground=P["text"], font=FONT_UI)
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

    # ── UI Layout ─────────────────────────────────────────────────────────

    def _build_ui(self) -> None:
        P = PALETTE
        tk.Frame(self, bg=P["accent"], height=2).pack(fill="x")
        body = tk.Frame(self, bg=P["bg"])
        body.pack(fill="both", expand=True)
        left = tk.Frame(body, bg=P["bg"], width=400)
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

        def field_row(frame, label_text):
            row = tk.Frame(frame, bg=P["surface"])
            row.pack(fill="x", pady=3)
            tk.Label(row, text=label_text, width=14, anchor="w",
                     bg=P["surface"], fg=P["muted"], font=FONT_UI).pack(side="left")
            return row

        # ── Printer ───────────────────────────────────────────────────────
        pf = section("Printer")        

        row_pn = tk.Frame(pf, bg=P["surface"])
        row_pn.pack(fill="x", pady=3)
        tk.Label(row_pn, text="Printer Name", width=14, anchor="w",
                 bg=P["surface"], fg=P["safeq"], font=FONT_BOLD).pack(side="left")
        self._v_printer_name = tk.StringVar()
        self._combo_printer = ttk.Combobox(
            row_pn, textvariable=self._v_printer_name,
            state="normal", width=22)
        self._combo_printer.pack(side="left", fill="x", expand=True)

        rr = tk.Frame(pf, bg=P["surface"])
        rr.pack(fill="x", pady=(2, 4))
        tk.Label(rr, text="", width=14, bg=P["surface"]).pack(side="left")
        ttk.Button(rr, text="⟳  Detect Printers", style="Teal.TButton",
                   command=self._refresh_printers).pack(side="left")
        self._lbl_detect_status = tk.Label(
            rr, text="", bg=P["surface"], fg=P["muted"], font=FONT_SMALL)
        self._lbl_detect_status.pack(side="left", padx=(6, 0))        

        # ── Print File ────────────────────────────────────────────────────
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

        # ── Load Config ───────────────────────────────────────────────────
        lf2 = section("Load Config")
        self._v_total = tk.IntVar(value=1000)

        tr = tk.Frame(lf2, bg=P["surface"])
        tr.pack(fill="x", pady=3)
        tk.Label(tr, text="Jobs / hour", width=14, anchor="w",
                 bg=P["surface"], fg=P["muted"], font=FONT_UI).pack(side="left")
        ttk.Spinbox(tr, textvariable=self._v_total,
                    from_=1, to=99999, width=8).pack(side="left")

        self._v_copies = tk.IntVar(value=1)
        cr = tk.Frame(lf2, bg=P["surface"])
        cr.pack(fill="x", pady=3)       

        repr_row = tk.Frame(lf2, bg=P["surface"])
        repr_row.pack(fill="x", pady=(8, 0))
        self._v_repeat = tk.BooleanVar(value=False)
        ttk.Checkbutton(repr_row, text="Repeat continuously",
                        variable=self._v_repeat).pack(side="left")

        self._lbl_iteration = tk.Label(
            lf2, text="", bg=P["surface"], fg=P["muted"], font=FONT_UI)
        self._lbl_iteration.pack(fill="x", pady=(4, 0))

        # ── Buttons ───────────────────────────────────────────────────────
        bf = tk.Frame(parent, bg=P["bg"])
        bf.pack(fill="x", pady=(8, 0))

        self._btn_start = ttk.Button(
            bf, text="▶  Start Test",
            style="Primary.TButton", command=self._start_test)
        self._btn_start.pack(fill="x", pady=(0, 6))

        self._btn_stop = ttk.Button(
            bf, text="■  Stop",
            style="Danger.TButton", command=self._stop_test, state="disabled")
        self._btn_stop.pack(fill="x", pady=(0, 6))

        r2 = tk.Frame(bf, bg=P["bg"])
        r2.pack(fill="x")
        ttk.Button(r2, text="Clear", style="Ghost.TButton",
                   command=self._clear_results).pack(
                       side="left", fill="x", expand=True, padx=(0, 4))
        ttk.Button(r2, text="Export CSV", style="Ghost.TButton",
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
        self._log_widget = scrolledtext.ScrolledText(
            log_frame, height=7, state="disabled",
            bg=P["surface"], fg=P["muted"], font=FONT_MONO,
            borderwidth=0, wrap="word", insertbackground=P["text"],
        )
        self._log_widget.pack(fill="x")
        self._log_widget.tag_config("ok",    foreground=P["success"])
        self._log_widget.tag_config("err",   foreground=P["danger"])
        self._log_widget.tag_config("info",  foreground=P["accent"])
        self._log_widget.tag_config("warn",  foreground=P["warning"])
        self._log_widget.tag_config("safeq", foreground=P["safeq"])

    # ── Stats Bar ─────────────────────────────────────────────────────────

    def _build_stats_bar(self) -> None:
        P = PALETTE
        bar = tk.Frame(self, bg=P["surface"], height=56)
        bar.pack(fill="x", side="bottom")
        bar.pack_propagate(False)
        tk.Frame(bar, bg=P["border"], height=1).pack(fill="x", side="top")
        inner = tk.Frame(bar, bg=P["surface"])
        inner.pack(expand=True)
        stats = [("Total","0"),("Success","0"),("Failed","0"),
                 ("Avg ms","—"),("Min ms","—"),("Max ms","—"),("Throughput","—")]
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

    def _discover_printers(self) -> None:
        printers = get_installed_printers()
        self._installed_printers = printers
        self.after(0, self._populate_printer_combo, printers)

    def _populate_printer_combo(self, printers: list[str]) -> None:
        self._combo_printer["values"] = printers
        count = len(printers)
        if count:
            self._lbl_detect_status.config(
                text=f"{count} found", fg=PALETTE["success"])
            safeq_names = [p for p in printers
                           if any(k in p.lower()
                                  for k in ("safeq", "ysoftsafeq", "safe"))]
            if safeq_names and not self._v_printer_name.get():
                self._v_printer_name.set(safeq_names[0])
            elif printers and not self._v_printer_name.get():
                self._v_printer_name.set(printers[0])
        else:
            self._lbl_detect_status.config(
                text="none found", fg=PALETTE["warning"])

    def _refresh_printers(self) -> None:
        self._lbl_detect_status.config(text="scanning…", fg=PALETTE["muted"])
        threading.Thread(target=self._discover_printers, daemon=True).start()
        self._log_ui("Scanning for installed printers…", tag="info")

    def _open_log(self) -> None:
        try:
            os.startfile(str(LOG_FILE))
        except Exception:
            messagebox.showinfo("Log File", str(LOG_FILE))

    # ── File / Folder Browsing ────────────────────────────────────────────

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
            log.info("File selected: %s", path)

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
            for fn in sorted(os.listdir(folder_path)):
                fp = os.path.join(folder_path, fn)
                if os.path.isfile(fp) and fn.lower().endswith(exts):
                    self._folder_files.append(fp)
            count = len(self._folder_files)
            log.info("Folder %r: %d files: %s", folder_path, count, self._folder_files)
            if count:
                self._lbl_file_count.config(
                    text=f"✓ Found {count} printable file{'s' if count != 1 else ''}")
                self._log_ui(f"Loaded {count} files from folder: {folder_path}",
                             tag="info")
            else:
                self._lbl_file_count.config(text="⚠ No printable files found")
                self._log_ui(f"No printable files in: {folder_path}", tag="warn")
        except Exception as e:
            log.error("Error loading folder %r: %s", folder_path, e)
            self._lbl_file_count.config(text="✗ Error reading folder")
            self._log_ui(f"Error loading folder: {e}", tag="err")
            self._folder_files.clear()

    # ── Start / Stop ──────────────────────────────────────────────────────

    def _start_test(self) -> None:
        printer_name = self._v_printer_name.get().strip()
        printer_unc  = self._v_unc.get().strip() if hasattr(self, "_v_unc") else ""
        filepath     = self._v_file.get().strip()
        folder       = self._v_folder.get().strip()
        copies       = self._v_copies.get()

        if not printer_name:
            messagebox.showerror(
                "Missing Input",
                "Please enter a Printer Name.\n\n"
                "Click '⟳ Detect Printers' or type the exact name from\n"
                "Windows Settings → Printers & scanners.\n\n"
                "PowerShell:  Get-Printer | Select Name")
            return

        use_folder = bool(folder and self._folder_files)
        if not use_folder and not (filepath and os.path.exists(filepath)):
            messagebox.showerror("Missing Input",
                                 "Select a valid print file or folder.")
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
            "printer_name":  printer_name,
            "printer_unc":   printer_unc,
            "files_to_print": files_to_print[:],
            "total":         total,
            "max_workers":   max_workers,
            "delay_s":       delay_s,
            "copies":        copies,
            "use_folder":    use_folder,
        }

        file_info = (f"{len(files_to_print)} files from folder"
                     if use_folder else os.path.basename(filepath))

        log.info("TEST START  target=%r  jobs=%d  delay=%.3fs  "
                 "workers=%d  copies=%d  file=%s",
                 printer_name, total, delay_s, max_workers, copies, file_info)

        self._log_ui(f"Starting load test → {printer_name}", tag="info",
                     detail=(f"Jobs: {total}/hr  Workers: {max_workers}  "
                             f"Copies: {copies}  File(s): {file_info}"))
        self._log_ui("SafeQ mode: jobs routed via Windows spooler — "
                     "Desktop Interface popup should fire.", tag="safeq")
        self._log_ui(f"Diagnostic log → {LOG_FILE}", tag="info")

        threading.Thread(
            target=self._run_load_test,
            args=(printer_name, printer_unc, files_to_print,
                  total, max_workers, copies, delay_s),
            daemon=True,
        ).start()
        self._tick_elapsed()

    def _stop_test(self) -> None:
        self._running = False
        self._repeat_config = None
        self._btn_stop.config(state="disabled")
        self._btn_start.config(state="normal")
        log.info("Stop requested by user.")
        self._log_ui("Stop requested — draining in-flight jobs…", tag="warn")

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
        target = cfg["printer_name"]
        log.info("ITERATION %d START  target=%r", self._iteration_count + 1, target)
        self._log_ui(
            f"Starting iteration {self._iteration_count + 1} → {target}",
            tag="info",
            detail=f"Jobs: {cfg['total']}  File(s): {file_info}")
        threading.Thread(
            target=self._run_load_test,
            args=(cfg["printer_name"], cfg["printer_unc"], cfg["files_to_print"],
                  cfg["total"], cfg["max_workers"], cfg["copies"], cfg["delay_s"]),
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
        self._log_ui("Results cleared.", tag="info")

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
        log.info("CSV exported: %s  (%d rows)", path, len(self._results))
        self._log_ui(f"Exported {len(self._results)} rows → {path}", tag="ok")
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
                       max_workers: int, copies: int, delay_s: float) -> None:

        def result_callback(future, job_id_val, printer_val, files_val):
            try:
                result = future.result()
            except Exception as exc:
                log.error("JOB %04d  future raised: %s", job_id_val, exc,
                          exc_info=True)
                result = JobResult(
                    job_id=job_id_val, printer=printer_val,
                    file_path=files_val[0],
                    start_time=time.time(), end_time=time.time(),
                    status="Failed", error=str(exc),
                )
            self._queue.put(("result", result))

        with concurrent.futures.ThreadPoolExecutor(max_workers=max_workers) as pool:
            for job_id in range(1, total + 1):
                if not self._running:
                    log.info("Submission stopped at job %d (user stop)", job_id)
                    break
                file_index   = (job_id - 1) % len(files_to_print)
                current_file = files_to_print[file_index]
                self._queue.put(("pending", job_id))
                fut = pool.submit(
                    send_print_job,
                    job_id, printer_name, printer_unc, current_file, copies,
                )
                fut.add_done_callback(
                    lambda f, jid=job_id, p=printer_name,
                    fps=files_to_print:
                        result_callback(f, jid, p, fps)
                )
                if delay_s > 0:
                    time.sleep(delay_s)

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

                    icon = "✓" if r.status == "Success" else "✗"
                    tag  = "ok" if r.status == "Success" else "err"
                    txt  = (f"[{icon}] Job #{r.job_id:>4}  {dur_str:>7} ms"
                            + (f"  ↳ {r.error}" if r.error else ""))
                    self._log_ui(txt, tag=tag)
                    self._update_stats()

                elif msg == "log":
                    text, tag = (payload if isinstance(payload, tuple)
                                 else (payload, "info"))
                    self._log_ui(text, tag=tag)

                elif msg == "done":
                    self._iteration_count += 1
                    self._running = False
                    elapsed = time.time() - self._test_start_time
                    success = sum(1 for r in self._results if r.status == "Success")
                    failed  = sum(1 for r in self._results if r.status == "Failed")
                    log.info("ITERATION %d DONE  elapsed=%.1fs  success=%d  failed=%d",
                             self._iteration_count, elapsed, success, failed)
                    
                    self._log_ui(
                        f"Iteration {self._iteration_count} complete  ·  "
                        f"{len(self._results)} total jobs  ·  "
                        f"{success} ok  {failed} failed  ·  {elapsed:.1f}s",
                        tag="info",
                    )
                    self._update_stats()

                    if self._v_repeat.get() and self._repeat_config:
                        self._lbl_progress.config(text="Restarting…")
                        self._log_ui("Repeat mode — restarting in 2 s…", tag="info")
                        self.after(2000, self._restart_test)
                    else:
                        self._btn_start.config(state="normal")
                        self._btn_stop.config(state="disabled")
                        self._lbl_progress.config(text="Complete")
                        self._repeat_config = None

                if processed % 10 == 0:
                    self.update()

        except queue.Empty:
            pass

        if processed > 0:
            self.update()

        self.after(20, self._poll_queue)

    # ── Stats ─────────────────────────────────────────────────────────────

    def _update_stats(self) -> None:
        P = PALETTE
        results = self._results
        total   = len(results)
        success = sum(1 for r in results if r.status == "Success")
        failed  = sum(1 for r in results if r.status == "Failed")
        durs    = [r.duration_ms for r in results if r.end_time > 0]
        self._stat_vals["Total"].config(text=str(total), fg=P["text"])
        self._stat_vals["Success"].config(
            text=str(success), fg=P["success"] if success > 0 else P["text"])
        self._stat_vals["Failed"].config(
            text=str(failed), fg=P["danger"] if failed > 0 else P["text"])
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

    def _log_ui(self, message: str, tag: str = "info", detail: str = "") -> None:
        ts = datetime.now().strftime("%H:%M:%S.%f")[:-3]
        self._log_widget.config(state="normal")
        self._log_widget.insert("end", f"[{ts}] ", "info")
        self._log_widget.insert("end", message + "\n", tag)
        if detail:
            self._log_widget.insert("end", f"          {detail}\n", "info")
        self._log_widget.yview_moveto(1.0)
        self._log_widget.config(state="disabled")

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
            if v := config.get("printer_name", ""):
                self._v_printer_name.set(v)
            if v := config.get("folder_path", ""):
                if os.path.exists(v):
                    self._v_folder.set(v)
                    self._load_folder_files(v)
            elif v := config.get("file_path", ""):
                if os.path.exists(v):
                    self._v_file.set(v)
            self._v_repeat.set(config.get("repeat_mode", False))
            self._v_copies.set(config.get("copies", 1))
            self._v_total.set(config.get("total", 1000))
        except Exception as exc:
            log.warning("Failed to load settings: %s", exc)

    def _save_settings(self) -> None:
        try:
            config = {
                "printer_name": self._v_printer_name.get(),
                "file_path":    self._v_file.get(),
                "folder_path":  self._v_folder.get(),
                "repeat_mode":  self._v_repeat.get(),
                "copies":       self._v_copies.get(),
                "total":        self._v_total.get(),
            }
            with open(CONFIG_FILE, "w") as f:
                json.dump(config, f, indent=2)
        except Exception as exc:
            log.warning("Failed to save settings: %s", exc)

    def _on_close(self) -> None:
        log.info("Application closed.")
        self._save_settings()
        self.destroy()


# ════════════════════════════════════════════════════════════════════════════
#  Entry Point
# ════════════════════════════════════════════════════════════════════════════

if __name__ == "__main__":
    app = PrintLoadTesterApp()
    app.mainloop()
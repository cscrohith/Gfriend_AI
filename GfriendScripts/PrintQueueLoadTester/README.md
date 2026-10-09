# 🖨️ Print Load Tester

A Windows printer load testing tool with real-time monitoring, multi-format support, and enterprise-grade spooler routing. Just configure jobs per hour — the app automatically distributes load, manages concurrency, and logs everything for analysis.

![Version](https://img.shields.io/badge/version-1.8-blue)
![Python](https://img.shields.io/badge/python-3.8+-green)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)

---

## 🚀 Features

- **⚡ Simple Load Configuration** — Enter jobs/hour, everything else is auto-managed
- **🕐 Per-Hour Distribution** — Jobs evenly spread across 1 hour automatically
- **🔁 Repeat Mode** — Continuous execution for endurance testing
- **📊 Real-Time Metrics** — Success rate, latency (min/avg/max), throughput
- **📁 Folder Support** — Automatically cycles through multiple files
- **📄 Multi-Format Printing**
  - PDF (PyMuPDF rendering)
  - Images (Pillow + GDI)
  - DOCX / XLSX (text rendering)
  - RAW formats (PRN, PCL, PS, TXT)
- **🖨️ Windows Spooler Integration** — Uses `win32print` for realistic behavior
- **📈 Live Progress Tracking** — Progress bar + real-time job table
- **📝 Detailed Logging** — Full diagnostics saved to Desktop log file
- **📤 CSV Export** — Export job results for analysis
- **⚠️ High Load Protection** — Warning for >3600 jobs/hour

---

## 🖥️ UI Overview

The application includes:

- **Configuration Panel**
  - Printer selection
  - File / folder input
  - Load configuration

- **Results Table**
  - Live job status
  - Sortable columns

- **Statistics Dashboard**
  - Total / Success / Failed
  - Avg / Min / Max latency
  - Throughput (jobs/sec)

- **Event Log**
  - Timestamped logs
  - Color-coded status messages

---

## 📦 Requirements

### Python
- Python 3.8+

### Dependencies

```bash
pip install pywin32 pillow pymupdf openpyxl python-docx img2pdf

## Installation

1. **Clone or download** this repository:
   ```bash
   git clone <repository-url>
   cd PrinterLoadTest
   ```

2. **Install dependencies** (optional but recommended):
   ```bash
   pip install pywin32 pillow pymupdf openpyxl python-docx img2pdf
   ```

3. **Run the application**:
   ```bash
   python print_load_tester.py

▶️ Usage
1. Select Printer
Click Detect Printers
Choose from dropdown or enter manually
2. Select Input
Single File
Click … and select file
Folder Mode
Select folder containing printable files
Tool cycles through files automatically

Supported formats:

PDF, TXT, PRN, PCL, PS
PNG, JPG, BMP
DOCX, XLSX
3. Configure Load
Field	Description
Jobs/hour	Total jobs over 1 hour
Copies	Copies per job
Repeat	Continuous execution
4. Start Test
Click ▶ Start Test
Monitor real-time progress
5. Stop Test
Click ■ Stop
Gracefully stops after current jobs
6. Export Results
Click Export CSV

### Print Methods Explained

| Method | Use Case | Requirements | Format Support |
|--------|----------|--------------|----------------|
| `copy` | RAW/text files | None | TXT, PRN, PCL, PS |
| `win32print` | Direct Windows API printing | pywin32 | TXT, PRN, PCL, PS (RAW data) |
| `shellexec` | Document printing via shell | pywin32 | PDF, Office docs, images |

### Per-Hour Job Distribution

The application automatically distributes your jobs evenly over 1 hour:

**Examples:**
- **Enter 1000 jobs** = 1 job every 3.6 seconds (0.278 jobs/sec)
- **Enter 1800 jobs** = 1 job every 2 seconds (0.5 jobs/sec)
- **Enter 3600 jobs** = 1 job every second (1 job/sec)
- **Enter 7200 jobs** = 2 jobs every second (2 jobs/sec)

The app automatically calculates:
- **Delay between jobs** — To spread them evenly over 1 hour
- **Concurrent workers** — Auto-scaled from 5 to 50 based on rate

**Important Notes:**
- The test stops after sending all jobs (doesn't wait for full hour if jobs complete sooner)
- Jobs above 3600 trigger a confirmation warning to prevent accidental server overload
- Use the Stop button to halt submission gracefully

No manual configuration needed!

### Folder Mode

When you select a folder instead of a single file:
- All printable files in the folder are loaded
- Jobs cycle through the files in alphabetical order
- Example: With 3 files and 100 jobs, each file gets printed ~33 times
- This simulates real-world mixed document printing scenarios

### Repeat/Continuous Mode

Enable the "Repeat continuously" checkbox for ongoing load testing:
- Test automatically restarts after completion with same configuration
- Results accumulate across all iterations
- Iteration counter shows current cycle (e.g., "Iteration 3 completed")
- Useful for:
  - Long-term stress testing
  - Endurance testing over hours or days
  - Continuous availability monitoring
  - Baseline load maintenance

**To use:**
1. Configure your test settings (printer, file, job count)
2. Check the "Repeat continuously" box
3. Click Start Test
4. Test will run continuously until you click Stop

**Note:** Clicking Stop cancels repeat mode and allows current iteration to finish gracefully.

## Output Metrics

### Statistics Dashboard
- **Total** — Total jobs processed
- **Success** — Successfully completed jobs (green)
- **Failed** — Failed jobs (red)
- **Avg ms** — Average job duration in milliseconds
- **Min ms** — Fastest job duration
- **Max ms** — Slowest job duration
- **Throughput** — Jobs processed per second
- **Elapsed** — Total test runtime

### CSV Export Columns
- Job# — Sequential job identifier
- Status — Success/Failed/Running/Pending
- Duration_ms — Job execution time in milliseconds
- Start_Time — Timestamp when job started (HH:MM:SS.mmm)
- Printer — UNC path of the printer
- File — Name of the print file
- Error — Error message (if failed)

## Troubleshooting

### Common Issues

**"pywin32 not found — copy mode only"**
- Install pywin32: `pip install pywin32`
- Or continue using the `copy` method (works for most RAW/text files)

**No printable files found in folder**
- Ensure the folder contains supported file types (PDF, TXT, Office docs, images)
- Check file extensions match the supported formats

**Print jobs fail with access errors**
- Verify you have permission to access the print server
- Try running as Administrator if accessing domain printers
- Check network connectivity and firewall settings

**Jobs succeed but nothing prints**
- Verify the print file format matches the selected method
- Check printer queue on the print server for stuck jobs
- Try a different print method
- Ensure the file contains valid printable data

**Timeout errors**
- Increase timeout values in the code if needed
- Lower the rate to reduce load on the printer
- Check network connectivity

**Permission denied / Access denied**
- Run as Administrator if accessing domain printers
- Verify network connectivity to the print server
- Check firewall settings

**High job count warning appears**
- This is intentional for counts above 3600 to prevent accidental overload
- Click OK to proceed if you're sure, or Cancel to adjust the count
- Consider starting with smaller tests (100-1000 jobs) to verify setup

## Advanced Configuration

### Modifying Timeouts

Edit the timeout values in [print_load_tester.py](print_load_tester.py):

```python
# In _print_via_shellexec():
timeout=90,  # Increase for slower printers

# In _print_via_copy():
timeout=30,  # Increase for large files

# In map_printer_credentials():
timeout=15,  # Increase for slow network
timeout=20,  # Increase for authentication delay
```

### Custom Job Naming

Jobs appear in the Windows print queue with descriptive names:
```
[0001] document.pdf
[0002] document.pdf
...
```

Modify `_make_job_name()` to customize the format.

## Architecture

### Key Components

- **UI Layer** — Tkinter-based GUI with modern styling
- **Worker Pool** — ThreadPoolExecutor for concurrent job execution
- **Job Queue** — Thread-safe queue for result collection
- **Print Methods** — Pluggable backend (win32print, shellexec, copy)
- **Folder Support** — Automatic file cycling for multi-file tests

### Threading Model

- **Main Thread** — UI event loop and queue polling
- **Coordinator Thread** — Submits jobs and collects results
- **Worker Threads** — Execute individual print jobs (configurable pool size)

All results are passed through a thread-safe queue to avoid UI race conditions.

## License

This project is provided as-is for testing and educational purposes. Use responsibly and do not overload production printers.

## Contributing

Contributions are welcome! Areas for improvement:
- Support for Linux/CUPS printers
- Additional print protocols (IPP, LPD)
- Graphical charts/plots for metrics
- Scheduled/automated test runs
- Load pattern simulation (ramp-up, burst, steady)
- Authentication/credential support if needed for specific environments

## Changelog

### v1.3
- **Repeat/Continuous Mode** — Added checkbox to automatically restart tests for ongoing load testing
- **Iteration Tracking** — Shows iteration count for repeated test cycles
- **Enhanced Comments** — Comprehensive code documentation for better understanding
- **Increased Font Sizes** — Improved readability with larger text throughout the UI (11pt standard, 15pt headers)
- Auto-restart with 2-second delay between iterations
- Results accumulate across all iterations in repeat mode
- Larger row height in results grid for better visibility

### v1.2
- **Removed authentication fields** — Simplified UI by removing Username, Password, and Domain fields
- **Added folder support** — Load and cycle through multiple files from a folder
- **High load warning** — Automatic confirmation dialog for job counts above 3600
- **Improved panel sizing** — Better layout with wider config panel for clear labels
- Real-time job status in results grid
- Enhanced error handling and validation

### v1.1
- **Ultra-simplified interface** — Single "Jobs (per hour)" input field
- **Automatic per-hour distribution** — Jobs spread evenly over 1 hour
- **Automatic worker scaling** — Concurrent workers auto-calculated from rate
- Improved UI layout and styling
- Enhanced error handling and logging
- Added elapsed time display
- Better progress tracking

### v1.0
- Initial release
- Basic concurrent printing
- CSV export
- Win32 print support

---

**Created for Windows printer load testing and performance benchmarking.**

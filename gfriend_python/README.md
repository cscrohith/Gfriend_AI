# GFriend Python runtime (migration in progress)

This package is the first executable Python replacement slice for the legacy
GFriend runtime. It parses test cases, static and execution variables, `using`
declarations, keyword statements, `Repeat`, `For`, and `While` blocks, and
`If`/`Fail`/`Error` branches. It executes built-in keywords and registered
Python keyword libraries, reads `.gfvar` files and Excel named tables for
`ForEachRow` and whole-worksheet loops, and emits JSON, XML, and standalone HTML
execution reports.

The .NET solution remains in the repository as the behavioral reference while
the migration proceeds. Device drivers, the remote transport, the legacy
XML/session report format, Windows desktop UI, and the remaining GFriend
keyword libraries are not yet ported; scripts using those features require
later migration phases. A local browser-based execution workspace is available
for the Python runtime.

## Run a script

```powershell
python -m gfriend_python path\to\script.txt
python -m gfriend_python path\to\script.txt --var UserName=alice --test-case "Login"
python -m gfriend_python path\to\script.txt --variable-file path\to\common.gfvar
python -m gfriend_python path\to\script.txt --report output\result.json --output-folder output
python -m gfriend_python path\to\script.txt --html-report output\report.html
python -m gfriend_python path\to\script.txt --xml-report output\report.xml
python -m gfriend_python.run_spec path\to\run-spec.xml --var UserName=alice
python -m gfriend_python.legacy_report path\to\legacy-output.xml
python -m gfriend_python.summary_report path\to\report-folder
```

The command exits with status `0` when selected cases pass, `1` when execution
reports a failure or error, and `2` for invalid input or file errors.

The run-spec command reads the legacy `TestRuns` XML shape, including suite
selection, repeat counts, and output paths, and aggregates the repeated suite
results into one report. Relative suite and output paths resolve from the
run-spec file. Device configuration fields are rejected until Python device
libraries are available.

## Open the web execution workspace

The local web interface runs on loopback and uses the `GfriendScripts` folder
under the current directory by default:

```powershell
python -m gfriend_python.web
```

Or point it at a different folder containing `.txt` or `.gfscripts` files:

```powershell
python -m gfriend_python.web --scripts-dir path\to\trusted-scripts --port 8765
```

The workspace opens in your browser and supports script search, test-case
selection, per-run variable overrides, and live execution results. It binds
only to `127.0.0.1`; it is not intended to be exposed to a network. Run only
scripts and datasets you trust because GFriend scripts can write files and
interact with local resources. Execution is single-run-at-a-time. Manual
confirmation keywords need the interactive CLI and report an error in the web
workspace.

Use **Script editor** to create or edit `.txt` and `.gfscripts` files inside the
configured scripts folder. Validate changes before saving; invalid scripts can
still be saved as drafts. Existing files use revision checks to prevent
overwriting changes made after the editor loaded them.

## Add a Python keyword library

Register an object with `Executor.register_library`. Public Python methods are
available to scripts by method name (spaces and underscores are interchangeable).
Methods may return `KeywordResult`, `bool`, or `None`.

```python
from gfriend_python import Executor, KeywordResult, Status, parse_script


class Sample:
    def greet(self, name: str) -> KeywordResult:
        return KeywordResult(Status.PASS, f"Hello, {name}")


suite = parse_script("Greeting\n{\n  Sample.Greet(World)\n}\n")
executor = Executor()
executor.register_library("Sample", Sample())
result = executor.run(suite)
print(result.to_dict())
```

`RemoteRun: executor-id { ... }` blocks can be dispatched through an injected
adapter. The callback receives the expanded executor ID, parsed block body, and
a snapshot of current variables, and must return a `KeywordResult`. Without an
adapter, executing a remote block reports an error.

## Implemented syntax and built-ins

- `using Library` and `using Library as Alias`; `using file.gfvar` loads
  script-relative static variables
- `using Regex` provides `Set Pattern`, `Is Match`, `Get Match`, and
  `Get Match Count` keywords with state scoped to the current script run
- `using Json` provides `Load Json`, `Get Value`, and `Add Json Element`;
  value lookup supports root, object-property, and array-index JSON paths
- `using XML` provides XML loading, basic XPath-compatible attribute/text
  lookup and mutation, node appending, and file saving using Python's standard
  XML library
- `using Info` provides portable `Start Time Check`, `Mark Start`, `Mark End`,
  and `Draw Time Graph` timing; charts are written as SVG files
- `using LogInfo` records checkpoint start/end times with loop results and
  creates SVG elapsed-time and result-distribution charts
- `using SpreadSheet` supports creating/selecting workbooks and worksheets,
  appending rows and columns, writing/formatting cells, validating status
  columns, reading cells and ranges, and basic numeric/date-difference formulas
  for `.xlsx`, `.xlsm`, and CSV data where applicable
- `using Rest` supports GET, POST, PUT, and DELETE requests with headers, Basic
  authentication, JSON bodies, and response variables; TLS certificate
  validation remains enabled and cannot be disabled
- `using LP` builds a shell-quoted CUPS `lp` command from a selected file,
  printer, copy count, and options; it returns the command without executing it
- `gfriend-legacy-report output.xml` converts a legacy GFriend XML execution
  report to standalone HTML, preserving nested results and available remote
  block output; the installed command also accepts `--output path.html`
- `gfriend-summary-report folder` aggregates suite pass/fail/error counts from
  nested `output.xml` files and links to sibling HTML reports where present
- `${Name}=value` declarations, `.gfvar` files, and `--var Name=value` overrides
- `Library.Keyword(arg1, arg2)` and built-in `Keyword(...)`
- `Repeat: count { ... }` with `${R}` iteration value
- `Repeat: 10s { ... }` for time-based repetition; `For: a, b { ... }` with `${ITEM}`
- `While: Keyword(...): max_iterations { ... }` (default maximum: 50), with
  literal or variable iteration limits, colons inside arguments, and multiline
  keyword arguments
- `dataset data.xlsx as Data` with `ForEachRow: Data.Sheet.Table { ... }`
  for Excel named tables (and CSV rows), or
  `SpreadSheetForEachRow: Data.Sheet.Name { ... }` for all worksheet rows,
  using `openpyxl`
- `For Selected Row: Data.Sheet.Table.Column>=value` with comparison and
  comma-separated equality filters and variable-expanded thresholds
- `resource path as Name`, resolved relative to the script and available as
  `${Name}` or a direct keyword argument
- `using MyLibrary.gflib` loads script-defined custom keyword cases from that
  file, with declared parameters substituted during execution
- `RemoteRun: executor-id { ... }` blocks through an injected Python adapter
- `If: Keyword(...) { ... }`, followed optionally by `Fail:` and `Error:`
- `Pass`, `Fail`, `Error`, `Equals`, `Equals IgnoreCase`, `Log`, `Sleep`,
  `Contains`, `ConvertToUpperCase`, `ConvertToLowerCase`, `Calculate`,
  `Calculate And
  Assign`, `ReAssign Value To Variable`, `NotEquals`, and `CompareAmounts`
- String helpers: `Trim`, `Replace`, `Substring`, `Count`, and
  `SplitAndStoreWord`
- `Get Time Difference`, `Get Random Number`, and `Append To File`
- `Convert Time Stamp Format`, both concatenation helpers, and
  `Has Value Increased`, and `Assert Last Repeat` for result counts from
  Repeat/For/While/dataset-row blocks
- `Get System Information` (processor), `Get RAM`, and Windows-only
  `Get Windows Version`
- `Wait For User Confirm` and `Request User Verification` through an injected
  callback; the CLI uses text prompts and library users can provide their own
  interaction handler
- `Stop Test Case`, `Stop Test Suite`, `BreakLoop`, and `ContinueLoop`
- Custom `.gflib` methods support `Return With Pass`, `Return With Fail`, and
  `Return With Error`; a custom library can use its own `using` declarations
- File built-ins: `File Exist`, `Folder Exist`, `Create Folder`,
  `Read From File`, `Save To File`, `File Contains Text`, and file equality
- `Write To Data Set Advanced(sheet, row, column, value[, dataset])` writes a
  worksheet cell in a declared Excel dataset; specify the dataset alias when
  the script declares multiple Excel datasets
- `Get Time Stamp` and `Run Keyword`
- Prefix a statement with `@` to ignore its result when aggregating the case

This is an incremental port, not feature parity with GFriend yet. The runtime
intentionally returns an error for unimplemented keywords and unregistered
libraries instead of silently treating them as successful.

The XML option writes the Python runtime's structured execution result format.
It is not a replacement for the legacy XML event/session reporter or its
session-report generation.

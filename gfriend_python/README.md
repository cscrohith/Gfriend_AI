# GFriend Python runtime (migration in progress)

This package is the first executable Python replacement slice for the legacy
GFriend runtime. It parses test cases, static and execution variables, `using`
declarations, keyword statements, `Repeat`, `For`, and `While` blocks, and
`If`/`Fail`/`Error` branches. It executes built-in keywords and registered
Python keyword libraries, reads `.gfvar` files and Excel named tables for
`ForEachRow`, and emits a JSON result report.

The .NET solution remains in the repository as the behavioral reference while
the migration proceeds. Device drivers, remote execution, datasets, spreadsheets,
resources, remote execution, datasets other than Excel named tables,
advanced spreadsheet loops, reports, desktop UI, device drivers, and the
remaining GFriend keyword libraries are not yet ported; scripts using those
features require later migration phases.

## Run a script

```powershell
python -m gfriend_python path\to\script.txt
python -m gfriend_python path\to\script.txt --var UserName=alice --test-case "Login"
python -m gfriend_python path\to\script.txt --variable-file path\to\common.gfvar
python -m gfriend_python path\to\script.txt --report output\result.json --output-folder output
```

The command exits with status `0` when selected cases pass, `1` when execution
reports a failure or error, and `2` for invalid input or file errors.

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

## Implemented syntax and built-ins

- `using Library` and `using Library as Alias`
- `${Name}=value` declarations, `.gfvar` files, and `--var Name=value` overrides
- `Library.Keyword(arg1, arg2)` and built-in `Keyword(...)`
- `Repeat: count { ... }` with `${R}` iteration value
- `Repeat: 10s { ... }` for time-based repetition; `For: a, b { ... }` with `${ITEM}`
- `While: Keyword(...): max_iterations { ... }` (default maximum: 50)
- `dataset data.xlsx as Data` with `ForEachRow: Data.Sheet.Table { ... }`
  for Excel named tables (and CSV rows) using `openpyxl`
- `For Selected Row: Data.Sheet.Table.Column>=value` with comparison and
  comma-separated equality filters
- `resource path as Name`, resolved relative to the script and available as
  `${Name}` or a direct keyword argument
- `using MyLibrary.gflib` loads script-defined custom keyword cases from that
  file, with declared parameters substituted during execution
- `If: Keyword(...) { ... }`, followed optionally by `Fail:` and `Error:`
- `Pass`, `Fail`, `Error`, `Equals`, `Equals IgnoreCase`, `Log`, `Sleep`,
  `Calculate And Assign`, and `ReAssign Value To Variable`
- `Stop Test Case`, `Stop Test Suite`, `BreakLoop`, and `ContinueLoop`
- File built-ins: `File Exist`, `Folder Exist`, `Create Folder`,
  `Read From File`, `Save To File`, `File Contains Text`, and file equality
- `Get Time Stamp` and `Run Keyword`
- Prefix a statement with `@` to ignore its result when aggregating the case

This is an incremental port, not feature parity with GFriend yet. The runtime
intentionally returns an error for unimplemented keywords and unregistered
libraries instead of silently treating them as successful.

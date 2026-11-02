# GFriend Python runtime (migration phase 1)

This package is the first executable Python replacement slice for the legacy
GFriend runtime. It parses test cases, static and execution variables, `using`
declarations, keyword statements, `Repeat` blocks, and `If`/`Fail`/`Error`
branches. It executes built-in keywords and registered Python keyword libraries
and emits a JSON result report.

The .NET solution remains in the repository as the behavioral reference while
the migration proceeds. Device drivers, remote execution, datasets, spreadsheets,
reports, desktop UI, and the remaining GFriend keyword libraries are not yet
ported; scripts using those features require later migration phases.

## Run a script

```powershell
python -m gfriend_python path\to\script.txt
python -m gfriend_python path\to\script.txt --var UserName=alice --test-case "Login"
python -m gfriend_python path\to\script.txt --report output\result.json
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
- `${Name}=value` declarations and `--var Name=value` overrides
- `Library.Keyword(arg1, arg2)` and built-in `Keyword(...)`
- `Repeat: count { ... }` with `${R}` iteration value
- `If: Keyword(...) { ... }`, followed optionally by `Fail:` and `Error:`
- `Pass`, `Fail`, `Error`, `Equals`, `Log`, and `Sleep`
- Prefix a statement with `@` to ignore its result when aggregating the case

This is an incremental port, not feature parity with GFriend yet. The initial
runtime intentionally returns an error for unimplemented keywords and unregistered
libraries instead of silently treating them as successful.

import tempfile
import unittest
from pathlib import Path

from gfriend_python import Executor, KeywordResult, ParseError, Status, parse_script, parse_variable_file
from gfriend_python.parser import parse_file


class KeywordLibrary:
    def add_to_output(self, value: str) -> KeywordResult:
        return KeywordResult(Status.PASS, value)

    def return_false(self) -> bool:
        return False


class ParserTests(unittest.TestCase):
    def test_parses_variables_libraries_cases_and_quoted_commas(self) -> None:
        suite = parse_script(
            '${User}=Ada\n'
            'using Sample as S\n'
            'Login\n'
            '{\n'
            '  S.Add To Output ("hello, world", ${User}) // comment\n'
            '}\n',
            name="login",
        )

        self.assertEqual(suite.name, "login")
        self.assertEqual(suite.variables, {"${User}": "Ada"})
        self.assertEqual(suite.libraries, {"S": "Sample"})
        statement = suite.test_cases[0].steps[0]
        self.assertEqual(statement.arguments, ["hello, world", "${User}"])
        self.assertEqual(statement.line_number, 5)

    def test_parses_repeat_and_conditional_blocks(self) -> None:
        suite = parse_script(
            'Flow\n{\n'
            '  Repeat: 2\n  {\n'
            '    If: Equals(ok, ok)\n    {\n'
            '      Pass(done)\n'
            '    }\n'
            '    Fail:\n    {\n'
            '      Fail(not equal)\n'
            '    }\n'
            '  }\n'
            '}\n'
        )

        result = Executor().run(suite)
        self.assertEqual(result.status, Status.PASS)
        self.assertEqual(len(result.test_cases[0].steps), 4)

    def test_parses_for_and_while_blocks(self) -> None:
        suite = parse_script(
            'Loops\n{\n'
            '  For: first, "second, item"\n  {\n'
            '    Pass(${ITEM})\n'
            '  }\n'
            '  While: Equals(${R}, 1): 3\n  {\n'
            '    Pass(loop)\n'
            '  }\n'
            '}\n'
        )
        result = Executor().run(suite)
        self.assertEqual(result.status, Status.PASS)
        self.assertEqual(
            [
                step.output
                for step in result.test_cases[0].steps
                if step.keyword.endswith(".Pass")
            ],
            ["first", "second, item", "loop"],
        )

    def test_reports_malformed_script_with_line_number(self) -> None:
        with self.assertRaisesRegex(ParseError, "line 3"):
            parse_script("Case\n{\n  Pass(ok\n}\n")

    def test_requires_test_case(self) -> None:
        with self.assertRaisesRegex(ParseError, "does not contain"):
            parse_script("using BuiltIn\n${X}=1\n")


class ExecutorTests(unittest.TestCase):
    def test_builtin_failure_and_error_are_aggregated(self) -> None:
        suite = parse_script("Case\n{\n  Fail(no)\n  Pass(continued)\n}\n")
        result = Executor().run(suite)
        self.assertEqual(result.status, Status.FAIL)
        self.assertEqual([step.status for step in result.test_cases[0].steps], [Status.FAIL, Status.PASS])

    def test_registered_python_library_and_variable_precedence(self) -> None:
        suite = parse_script(
            '${Name}=script\n'
            'using Sample\n'
            'Case\n{\n  Sample.Add To Output(${Name})\n}\n',
            execution_variables={"Name": "execution"},
        )
        executor = Executor()
        executor.register_library("Sample", KeywordLibrary())
        result = executor.run(suite)
        self.assertEqual(result.status, Status.PASS)
        self.assertEqual(result.test_cases[0].steps[0].output, "execution")

    def test_repeat_variable_and_branch_selection(self) -> None:
        suite = parse_script(
            'Flow\n{\n'
            '  Repeat: 3\n  {\n'
            '    Pass(${R})\n'
            '  }\n'
            '  If: Equals(a, b)\n  {\n'
            '    Pass(not selected)\n'
            '  }\n'
            '  Fail:\n  {\n'
            '    Pass(selected)\n'
            '  }\n'
            '}\n'
        )
        result = Executor().run(suite)
        self.assertEqual(result.status, Status.PASS)
        self.assertEqual(
            [step.output for step in result.test_cases[0].steps],
            ["1", "2", "3", "'a' does not equal 'b'", "selected"],
        )

    def test_execution_variables_do_not_leak_between_runs(self) -> None:
        suite = parse_script(
            '${Name}=script\n'
            'Case\n{\n'
            '  Log(${Name})\n'
            '}\n'
        )
        executor = Executor(variables={"Name": "execution"})
        first = executor.run(suite)
        second = executor.run(suite)
        self.assertEqual(first.test_cases[0].steps[0].output, "execution")
        self.assertEqual(second.test_cases[0].steps[0].output, "execution")

    def test_ignored_fail_branch_suppresses_nested_failure(self) -> None:
        suite = parse_script(
            'Case\n{\n'
            '  If: Equals(a, b)\n  {\n'
            '    Pass(ok)\n'
            '  }\n'
            '  @Fail:\n  {\n'
            '    Repeat: 1\n    {\n'
            '      Fail(ignored)\n'
            '    }\n'
            '  }\n'
            '}\n'
        )
        self.assertEqual(Executor().run(suite).status, Status.PASS)

    def test_for_loop_continue_and_break(self) -> None:
        suite = parse_script(
            'Case\n{\n'
            '  For: a, b, c\n  {\n'
            '    If: Equals(${ITEM}, b)\n    {\n'
            '      ContinueLoop()\n'
            '    }\n'
            '    Pass(${ITEM})\n'
            '    If: Equals(${ITEM}, c)\n    {\n'
            '      BreakLoop()\n'
            '    }\n'
            '  }\n'
            '  Pass(done)\n'
            '}\n'
        )
        result = Executor().run(suite)
        self.assertEqual(result.status, Status.PASS)
        self.assertEqual(
            [
                step.output
                for step in result.test_cases[0].steps
                if step.keyword.endswith(".Pass")
            ],
            ["a", "c", "done"],
        )

    def test_stop_test_case_and_suite_are_respected(self) -> None:
        suite = parse_script(
            'First\n{\n'
            '  Stop Test Case\n'
            '  Fail(not run)\n'
            '}\n'
            'Second\n{\n'
            '  Stop Test Suite\n'
            '}\n'
            'Third\n{\n'
            '  Fail(not run)\n'
            '}\n'
        )
        result = Executor().run(suite)
        self.assertEqual([case.name for case in result.test_cases], ["First", "Second"])
        self.assertEqual(result.status, Status.PASS)

    def test_variable_assignment_and_arithmetic(self) -> None:
        suite = parse_script(
            '${Count}=1\n'
            'Case\n{\n'
            '  Calculate And Assign(${Count} + 2, ${Count})\n'
            '  Pass(${Count})\n'
            '}\n'
        )
        result = Executor().run(suite)
        self.assertEqual(result.status, Status.PASS)
        self.assertEqual(result.test_cases[0].steps[-1].output, "3")

    def test_file_builtins_and_keyword_result_variables(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            script = root / "suite.txt"
            script.write_text(
                'Case\n{\n'
                '  Save To File (result.txt,hello)\n'
                '  File Contains Text (${OUTPUT_FOLDER}/result.txt,hello)\n'
                '  Read From File (${OUTPUT_FOLDER}/result.txt,${Contents})\n'
                '  Pass(${Contents})\n'
                '  Pass(${KEYWORD_RESULT})\n'
                '}\n',
                encoding="utf-8",
            )
            result = Executor(output_folder=root / "output").run(parse_file(script))
            self.assertEqual(result.status, Status.PASS)
            self.assertEqual(
                [step.output for step in result.test_cases[0].steps],
                [str(root / "output" / "result.txt"), "", "hello", "hello", "PASS"],
            )

    def test_get_timestamp_stores_value_in_variable(self) -> None:
        result = Executor().run(
            parse_script(
                '${Stamp}=unset\n'
                'Case\n{\n'
                '  Get Time Stamp (${Stamp},yyyy)\n'
                '  Pass(${Stamp})\n'
                '}\n'
            )
        )
        self.assertEqual(result.status, Status.PASS)
        self.assertEqual(len(result.test_cases[0].steps[-1].output), 4)

    def test_ignore_error_marks_statement_pass(self) -> None:
        result = Executor().run(parse_script("Case\n{\n  @Missing.Keyword()\n}\n"))
        self.assertEqual(result.status, Status.PASS)
        self.assertEqual(result.test_cases[0].steps[0].status, Status.PASS)

    def test_plugin_exception_becomes_error_and_execution_continues(self) -> None:
        class BadLibrary:
            def explode(self) -> None:
                raise RuntimeError("device unavailable")

        suite = parse_script("using Bad\nCase\n{\n  Bad.Explode()\n  Pass(done)\n}\n")
        executor = Executor()
        executor.register_library("Bad", BadLibrary())
        result = executor.run(suite)
        self.assertEqual(result.status, Status.ERROR)
        self.assertEqual(len(result.test_cases[0].steps), 2)

    def test_selects_cases_by_name(self) -> None:
        suite = parse_script("One\n{\n  Fail(no)\n}\nTwo\n{\n  Pass(yes)\n}\n")
        result = Executor().run(suite, {"Two"})
        self.assertEqual(result.status, Status.PASS)
        self.assertEqual([case.name for case in result.test_cases], ["Two"])

    def test_reads_utf8_bom_file(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "sample.gfscripts"
            path.write_text("Case\n{\n  Pass(ok)\n}\n", encoding="utf-8-sig")
            self.assertEqual(parse_file(path).test_cases[0].name, "Case")

    def test_reads_gfvar_file(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "common.gfvar"
            path.write_text("// ignored\n${User}=alice=admin\ninvalid\n", encoding="utf-8")
            self.assertEqual(parse_variable_file(path), {"${User}": "alice=admin"})

    def test_selected_rows_filters_csv_data(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            data = root / "users.csv"
            data.write_text("ID,Role\n1,Admin\n2,User\n3,Admin\n", encoding="utf-8")
            script = root / "suite.txt"
            script.write_text(
                'dataset "users.csv" as Users\n'
                'AdminRows\n{\n'
                '  ForSelectedRow: Users.Users.Accounts.Role=Admin\n  {\n'
                '    Pass(${ID}-${Role})\n'
                '  }\n'
                '}\n',
                encoding="utf-8",
            )
            result = Executor().run(parse_file(script))
            self.assertEqual(result.status, Status.PASS)
            self.assertEqual(
                [step.output for step in result.test_cases[0].steps],
                ["1-Admin", "3-Admin"],
            )

    def test_selected_rows_numeric_filter(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root / "values.csv").write_text("TCID\n1\n5\n7\n9\n10\n", encoding="utf-8")
            script = root / "suite.txt"
            script.write_text(
                'dataset "values.csv" as Test\n'
                'LargeRows\n{\n'
                '  For Selected Row:Test.Sheet.Table.TCID>=7\n  {\n'
                '    Pass(${TCID})\n'
                '  }\n'
                '}\n',
                encoding="utf-8",
            )
            result = Executor().run(parse_file(script))
            self.assertEqual(result.status, Status.PASS)
            self.assertEqual(
                [step.output for step in result.test_cases[0].steps],
                ["7", "9", "10"],
            )

    def test_resource_alias_resolves_relative_to_script(self) -> None:
        class CaptureLibrary:
            def __init__(self) -> None:
                self.path = ""

            def capture(self, path: str) -> None:
                self.path = path

        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            resource = root / "sample.pdf"
            resource.write_bytes(b"pdf")
            script = root / "suite.txt"
            script.write_text(
                'resource sample.pdf as Sample\n'
                'using Capture\n'
                'Case\n{\n'
                '  Capture.Capture(Sample)\n'
                '}\n',
                encoding="utf-8",
            )
            library = CaptureLibrary()
            executor = Executor()
            executor.register_library("Capture", library)
            result = executor.run(parse_file(script))
            self.assertEqual(result.status, Status.PASS)
            self.assertEqual(library.path, str(resource))

    def test_gflib_custom_keyword_executes_with_arguments(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root / "Greetings.gflib").write_text(
                'Say Hello(${person})\n{\n'
                '  Pass(Hello ${person})\n'
                '}\n',
                encoding="utf-8",
            )
            script = root / "suite.txt"
            script.write_text(
                'using Greetings.gflib\n'
                'Case\n{\n'
                '  Greetings.Say Hello(World)\n'
                '}\n',
                encoding="utf-8",
            )
            result = Executor().run(parse_file(script))
            self.assertEqual(result.status, Status.PASS)
            self.assertEqual(result.test_cases[0].steps[0].output, "Hello World")

    def test_gflib_custom_keyword_uses_its_own_library_declarations(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root / "Greetings.gflib").write_text(
                'using Sample\n'
                '${Greeting}=Hello\n'
                'Say(${person})\n{\n'
                '  Sample.Add To Output(${Greeting} ${person})\n'
                '}\n',
                encoding="utf-8",
            )
            script = root / "suite.txt"
            script.write_text(
                'using Greetings.gflib\n'
                'Case\n{\n'
                '  Greetings.Say(World)\n'
                '}\n',
                encoding="utf-8",
            )
            executor = Executor()
            executor.register_library("Sample", KeywordLibrary())
            result = executor.run(parse_file(script))
            self.assertEqual(result.status, Status.PASS)
            self.assertEqual(result.test_cases[0].steps[0].output, "Hello World")

    def test_custom_keyword_return_forces_status_and_stops_body(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root / "Actions.gflib").write_text(
                'Do Work()\n{\n'
                '  Fail(intermediate failure)\n'
                '  Return With Pass(done)\n'
                '  Fail(should not execute)\n'
                '}\n',
                encoding="utf-8",
            )
            script = root / "suite.txt"
            script.write_text(
                'using Actions.gflib\n'
                'Case\n{\n'
                '  Actions.Do Work()\n'
                '}\n',
                encoding="utf-8",
            )
            result = Executor().run(parse_file(script))
            self.assertEqual(result.status, Status.PASS)
            self.assertEqual(len(result.test_cases[0].steps), 1)
            self.assertEqual(result.test_cases[0].steps[0].status, Status.PASS)

    def test_string_builtins_and_contains(self) -> None:
        result = Executor().run(
            parse_script(
                'Case\n{\n'
                '  ConvertToUpperCase(hello,${Upper})\n'
                '  Contains Ignore Case(${Upper},ell)\n'
                '  Pass(${Upper})\n'
                '}\n'
            )
        )
        self.assertEqual(result.status, Status.PASS)
        self.assertEqual(result.test_cases[0].steps[-1].output, "HELLO")

    def test_foreachrow_reads_excel_table(self) -> None:
        from openpyxl import Workbook
        from openpyxl.worksheet.table import Table

        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "data.xlsx"
            workbook = Workbook()
            worksheet = workbook.active
            worksheet.title = "Data"
            worksheet.append(["Name", "Role"])
            worksheet.append(["Ada", "Admin"])
            worksheet.append(["Lin", "User"])
            worksheet.add_table(Table(displayName="Accounts", ref="A1:B3"))
            workbook.save(path)

            suite_path = Path(directory) / "suite.txt"
            suite_path.write_text(
                'dataset "data.xlsx" as Users\n'
                'Case\n{\n'
                '  ForEachRow: Users.Data.Accounts\n  {\n'
                '    Pass(${Name}-${Role})\n'
                '  }\n'
                '}\n',
                encoding="utf-8",
            )
            result = Executor().run(parse_file(suite_path))
            self.assertEqual(result.status, Status.PASS)
            self.assertEqual(
                [step.output for step in result.test_cases[0].steps],
                ["Ada-Admin", "Lin-User"],
            )

    def test_write_to_dataset_updates_excel_table_rows(self) -> None:
        from openpyxl import Workbook, load_workbook
        from openpyxl.worksheet.table import Table

        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            path = root / "data.xlsx"
            workbook = Workbook()
            worksheet = workbook.active
            worksheet.title = "Data"
            worksheet.append(["Name", "Result"])
            worksheet.append(["Ada", ""])
            worksheet.append(["Lin", ""])
            worksheet.add_table(Table(displayName="Accounts", ref="A1:B3"))
            workbook.save(path)
            suite_path = root / "suite.txt"
            suite_path.write_text(
                'dataset "data.xlsx" as Users\n'
                'Case\n{\n'
                '  ForEachRow: Users.Data.Accounts\n  {\n'
                '    Write To Data Set (Result,${Name} passed)\n'
                '  }\n'
                '}\n',
                encoding="utf-8",
            )
            result = Executor().run(parse_file(suite_path))
            self.assertEqual(result.status, Status.PASS)
            saved = load_workbook(path, data_only=True)
            try:
                sheet = saved["Data"]
                self.assertEqual(sheet["B2"].value, "Ada passed")
                self.assertEqual(sheet["B3"].value, "Lin passed")
            finally:
                saved.close()

    def test_write_to_dataset_updates_selected_csv_row(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            path = root / "data.csv"
            path.write_text("ID,Role,Result\n1,Admin,\n2,User,\n3,Admin,\n", encoding="utf-8")
            suite_path = root / "suite.txt"
            suite_path.write_text(
                'dataset "data.csv" as Users\n'
                'Case\n{\n'
                '  ForSelectedRow: Users.Data.Table.Role=Admin\n  {\n'
                '    Write To Data Set (Result,${ID} done)\n'
                '  }\n'
                '}\n',
                encoding="utf-8",
            )
            result = Executor().run(parse_file(suite_path))
            self.assertEqual(result.status, Status.PASS)
            self.assertEqual(
                path.read_text(encoding="utf-8"),
                "ID,Role,Result\n1,Admin,1 done\n2,User,\n3,Admin,3 done\n",
            )


if __name__ == "__main__":
    unittest.main()

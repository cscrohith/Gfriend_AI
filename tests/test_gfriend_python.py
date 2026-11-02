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


if __name__ == "__main__":
    unittest.main()

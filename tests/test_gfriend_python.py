import csv
import threading
import tempfile
import unittest
import hashlib
import io
import zipfile
from http.server import BaseHTTPRequestHandler, HTTPServer
import xml.etree.ElementTree as ET
from pathlib import Path

from gfriend_python import (
    Executor,
    KeywordResult,
    ParseError,
    Status,
    parse_script,
    parse_variable_file,
    render_html_report,
    render_xml_report,
)
from gfriend_python.parser import parse_file
from gfriend_python.run_spec import main as run_spec_main, run_spec
from gfriend_python.report import (
    render_legacy_xml_execution_report,
    render_legacy_xml_report,
    write_legacy_html_report,
)
from gfriend_python.summary_report import render_summary_report, write_summary_report
from gfriend_python.updater import install_release


class RunSpecTests(unittest.TestCase):
    def test_runs_repeated_suites_and_selected_cases_from_relative_paths(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            script = root / "suite.txt"
            script.write_text(
                "Selected\n{\n"
                "  Pass(selected)\n"
                "}\n"
                "Skipped\n{\n"
                "  Fail(skipped)\n"
                "}\n",
                encoding="utf-8",
            )
            spec = root / "run.xml"
            spec.write_text(
                '<TestRuns Repeat="2">\n'
                "  <OutputPath>results</OutputPath>\n"
                '  <TestSuite Repeat="2">\n'
                "    <TestSuitePath>suite.txt</TestSuitePath>\n"
                "    <TestCase>Selected</TestCase>\n"
                "  </TestSuite>\n"
                "</TestRuns>\n",
                encoding="utf-8",
            )

            result = run_spec(spec)

            self.assertEqual(result.status, Status.PASS)
            self.assertEqual(len(result.test_cases), 4)
            self.assertTrue(all(case.name == "Selected" for case in result.test_cases))
            self.assertTrue(
                all(case.steps[0].output == "selected" for case in result.test_cases)
            )
            self.assertEqual(len(list((root / "results").iterdir())), 4)

    def test_run_spec_cli_writes_aggregated_xml_and_json_reports(self) -> None:
        import json
        from contextlib import redirect_stdout
        from io import StringIO

        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root / "suite.txt").write_text(
                "Case\n{\n  Pass(ok)\n}\n",
                encoding="utf-8",
            )
            spec = root / "run.xml"
            spec.write_text(
                "<TestRuns><TestSuite><TestSuitePath>suite.txt</TestSuitePath>"
                "<TestCase>Case</TestCase></TestSuite></TestRuns>",
                encoding="utf-8",
            )
            json_report = root / "nested" / "report.json"
            xml_report = root / "nested" / "report.xml"
            stdout = StringIO()
            with redirect_stdout(stdout):
                exit_code = run_spec_main(
                    [
                        str(spec),
                        "--report",
                        str(json_report),
                        "--xml-report",
                        str(xml_report),
                    ]
                )

            self.assertEqual(exit_code, 0)
            self.assertEqual(json.loads(json_report.read_text())["status"], "PASS")
            self.assertEqual(ET.parse(xml_report).getroot().findtext("Result"), "PASS")
            self.assertEqual(stdout.getvalue(), "")

    def test_run_spec_rejects_unsupported_device_configuration(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root / "suite.txt").write_text("Case\n{\n}\n", encoding="utf-8")
            spec = root / "run.xml"
            spec.write_text(
                "<TestRuns><TestSuite><TestSuitePath>suite.txt</TestSuitePath>"
                "<DefaultDevice>printer</DefaultDevice></TestSuite></TestRuns>",
                encoding="utf-8",
            )

            with self.assertRaisesRegex(ValueError, "device.*not supported"):
                run_spec(spec)


class KeywordLibrary:
    def add_to_output(self, value: str) -> KeywordResult:
        return KeywordResult(Status.PASS, value)

    def return_false(self) -> bool:
        return False


class ParserTests(unittest.TestCase):
    def test_updater_verifies_and_atomically_installs_release_archive(self) -> None:
        archive_buffer = io.BytesIO()
        with zipfile.ZipFile(archive_buffer, "w") as archive:
            archive.writestr("gfriend/README.txt", "verified release")
            archive.writestr("gfriend/bin/runner.py", "print('ready')")
        archive_bytes = archive_buffer.getvalue()
        checksum = hashlib.sha256(archive_bytes).hexdigest()

        class Handler(BaseHTTPRequestHandler):
            def do_GET(self) -> None:
                self.send_response(200)
                self.send_header("Content-Length", str(len(archive_bytes)))
                self.end_headers()
                self.wfile.write(archive_bytes)

            def log_message(self, format: str, *args: object) -> None:
                pass

        server = HTTPServer(("127.0.0.1", 0), Handler)
        thread = threading.Thread(target=server.serve_forever, daemon=True)
        thread.start()
        try:
            with tempfile.TemporaryDirectory() as directory:
                installed = install_release(
                    f"http://127.0.0.1:{server.server_port}/release.zip",
                    checksum,
                    directory,
                )
                self.assertEqual(installed.name, checksum[:16])
                self.assertEqual(
                    (installed / "gfriend" / "README.txt").read_text(),
                    "verified release",
                )
                with self.assertRaises(FileExistsError):
                    install_release(
                        f"http://127.0.0.1:{server.server_port}/release.zip",
                        checksum,
                        directory,
                    )
        finally:
            server.shutdown()
            server.server_close()
            thread.join(timeout=2)

    def test_updater_rejects_bad_checksum_insecure_url_and_zip_traversal(self) -> None:
        def serve(payload: bytes) -> tuple[HTTPServer, threading.Thread]:
            class Handler(BaseHTTPRequestHandler):
                def do_GET(self) -> None:
                    self.send_response(200)
                    self.send_header("Content-Length", str(len(payload)))
                    self.end_headers()
                    self.wfile.write(payload)

                def log_message(self, format: str, *args: object) -> None:
                    pass

            server = HTTPServer(("127.0.0.1", 0), Handler)
            thread = threading.Thread(target=server.serve_forever, daemon=True)
            thread.start()
            return server, thread

        archive_buffer = io.BytesIO()
        with zipfile.ZipFile(archive_buffer, "w") as archive:
            archive.writestr("../outside.txt", "unsafe")
        payload = archive_buffer.getvalue()
        server, thread = serve(payload)
        try:
            with tempfile.TemporaryDirectory() as directory:
                url = f"http://127.0.0.1:{server.server_port}/release.zip"
                with self.assertRaisesRegex(ValueError, "SHA-256 mismatch"):
                    install_release(url, "0" * 64, directory)
                self.assertEqual(list(Path(directory).iterdir()), [])
                checksum = hashlib.sha256(payload).hexdigest()
                with self.assertRaisesRegex(ValueError, "Unsafe archive path"):
                    install_release(url, checksum, directory)
                self.assertEqual(list(Path(directory).iterdir()), [])
                with self.assertRaisesRegex(ValueError, "HTTPS"):
                    install_release("http://example.com/release.zip", checksum, directory)
        finally:
            server.shutdown()
            server.server_close()
            thread.join(timeout=2)

    def test_windows_library_uses_accessibility_backend_for_ui_keywords(self) -> None:
        class FakeControl:
            def __init__(self) -> None:
                self.text = ""
                self.clicked = False

            def window_text(self) -> str:
                return "Sample text"

            def click_input(self) -> None:
                self.clicked = True

            def set_edit_text(self, text: str) -> None:
                self.text = text

        class FakeBackend:
            has_window = True

            def __init__(self) -> None:
                self.control = FakeControl()
                self.selected: tuple[str, float] | None = None

            def select(self, title: str, timeout: float) -> None:
                self.selected = (title, timeout)

            def find(self, automation_id: str, timeout: float) -> FakeControl:
                if automation_id != "field-id":
                    raise LookupError(f"Unknown control {automation_id}")
                return self.control

            def close(self) -> None:
                return None

        backend = FakeBackend()
        from gfriend_python.windows_library import WindowsLibrary

        script = parse_script(
            "using Windows\n"
            "Case\n"
            "{\n"
            "  Windows.SetTimeoutScale(2)\n"
            "  Windows.SelectApplication(Sample Window, 3)\n"
            "  Windows.WaitForId(field-id, 2)\n"
            "  Windows.GetText(field-id, ${text})\n"
            "  Windows.SetText(field-id, new value)\n"
            "  Windows.ClickId(field-id)\n"
            "  Windows.CloseApplication()\n"
            "}\n"
        )
        class FakeWindowsExecutor(Executor):
            def _register_script_libraries(self, suite) -> None:
                super()._register_script_libraries(suite)
                self.register_library(
                    "Windows",
                    WindowsLibrary(self.variables, backend_factory=lambda: backend),
                )

        executor = FakeWindowsExecutor()

        result = executor.run(script)

        self.assertEqual(result.status, Status.PASS)
        self.assertEqual(backend.selected, ("Sample Window", 6))
        self.assertEqual(executor.variables["${text}"], "Sample text")
        self.assertEqual(backend.control.text, "new value")
        self.assertTrue(backend.control.clicked)

    def test_windows_library_reports_missing_optional_driver_and_unknown_environment(self) -> None:
        from gfriend_python.windows_library import WindowsLibrary

        unavailable = WindowsLibrary({}, backend_factory=lambda: (_ for _ in ()).throw(
            RuntimeError("UI driver unavailable")
        ))
        result = unavailable.select_application("Sample")
        self.assertEqual(result.status, Status.ERROR)
        self.assertIn("UI driver unavailable", result.output)

        environment = WindowsLibrary({})
        result = environment.get_environment_variable(
            "GFRIEND_TEST_MISSING_ENVIRONMENT_VARIABLE",
            "${value}",
        )
        self.assertEqual(result.status, Status.FAIL)
        self.assertIn("is not defined", result.output)

    def test_execution_can_export_legacy_xml_consumed_by_summary_and_html_reports(self) -> None:
        result = Executor().run(
            parse_script(
                "Case & Suite\n"
                "{\n"
                "  Pass(ok & safe)\n"
                "  Fail(expected failure)\n"
                "}\n"
            )
        )

        legacy_xml = render_legacy_xml_execution_report(result)
        root = ET.fromstring(legacy_xml)
        self.assertEqual(root.tag, "TestSuite")
        self.assertEqual(root.get("Name"), result.suite_name)
        self.assertEqual(root.find("TestCase").get("Name"), "Case & Suite")
        self.assertEqual(root.findtext("TestCase/Result"), "FAIL")
        self.assertEqual(root.findtext("TestCase/Statement[1]/Output"), "ok & safe")
        self.assertIn("StatementName=\"BuiltIn.Pass\"", legacy_xml)

        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "output.xml"
            path.write_text(legacy_xml, encoding="utf-8")
            html = render_legacy_xml_report(path)
            self.assertIn("<strong>1</strong> FAIL", html)
            summary = render_summary_report(directory)
            self.assertIn("<strong>1</strong> FAIL", summary)

    def test_summary_report_aggregates_nested_output_xmls_and_links_reports(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            first = root / "suite-a"
            second = root / "nested" / "suite-b"
            first.mkdir()
            second.mkdir(parents=True)
            (first / "output.xml").write_text(
                '<TestSuite Name="Suite &lt;A&gt;"><StartTime>2025-01-01T10:00:00</StartTime>'
                "<EndTime>2025-01-01T10:00:03</EndTime>"
                "<TestCase Name=\"one\"><Result>PASS</Result></TestCase>"
                "<TestCase Name=\"two\"><Result>FAIL</Result></TestCase></TestSuite>",
                encoding="utf-8",
            )
            (first / "report.html").write_text("<html>suite report</html>", encoding="utf-8")
            (second / "output.xml").write_text(
                '<TestSuite Name="Suite B"><DurationSeconds>1.25</DurationSeconds>'
                "<TestCase Name=\"three\"><Result>ERROR</Result></TestCase>"
                "</TestSuite>",
                encoding="utf-8",
            )

            html = render_summary_report(root)
            output_path = write_summary_report(root)

            self.assertIn("<strong>1</strong> PASS", html)
            self.assertIn("<strong>1</strong> FAIL", html)
            self.assertIn("<strong>1</strong> ERROR", html)
            self.assertIn("Suite &lt;A&gt;", html)
            self.assertIn("suite-a/report.html", html)
            self.assertIn("nested/suite-b/output.xml", html)
            self.assertIn("0:00:03", html)
            self.assertIn("1.250s", html)
            self.assertEqual(output_path, root / "SummaryReport.html")
            nested_summary = root / "reports" / "SummaryReport.html"
            external_html = render_summary_report(root, nested_summary)
            self.assertIn("../suite-a/report.html", external_html)

    def test_summary_report_reports_invalid_xml_and_handles_empty_folder(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            self.assertIn("No output.xml reports found", render_summary_report(root))
            (root / "output.xml").write_text("<bad", encoding="utf-8")
            with self.assertRaisesRegex(ValueError, "Could not read report"):
                render_summary_report(root)

    def test_legacy_xml_report_renders_case_tree_and_merges_remote_results(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            source = root / "output.xml"
            source.write_text(
                '<?xml version="1.0"?>'
                '<TestSuite Name="Legacy &amp; Suite">'
                "<StartTime>2025-01-01T10:00:00</StartTime>"
                "<TestCase Name=\"Pass &lt;case&gt;\"><Result>PASS</Result>"
                "<StartTime>2025-01-01T10:00:01</StartTime>"
                '<Statement StatementName="Pass(&lt;script&gt;)">'
                "<Result>PASS</Result><Output>&lt;script&gt;bad&lt;/script&gt;</Output>"
                "</Statement></TestCase>"
                '<TestCase Name="Remote case"><Result>FAIL</Result>'
                '<Remote BlockID="remote-1"><Result>FAIL</Result></Remote>'
                "</TestCase></TestSuite>",
                encoding="utf-8",
            )
            (root / "output_remote-1.xml").write_text(
                '<Remote><Statement StatementName="Remote.Log()">'
                "<Result>FAIL</Result><Output>remote failure</Output>"
                "</Statement></Remote>",
                encoding="utf-8",
            )

            html = render_legacy_xml_report(source)
            output_path = write_legacy_html_report(source)

            self.assertIn("Legacy &amp; Suite", html)
            self.assertIn("&lt;script&gt;bad&lt;/script&gt;", html)
            self.assertNotIn("<script>bad</script>", html)
            self.assertIn("remote failure", html)
            self.assertIn("<strong>1</strong> PASS", html)
            self.assertIn("<strong>1</strong> FAIL", html)
            self.assertEqual(output_path, root / "report.html")
            self.assertEqual(output_path.read_text(encoding="utf-8"), html)

    def test_legacy_xml_report_strips_invalid_xml_characters(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            source = Path(directory) / "output.xml"
            source.write_text(
                '<TestSuite Name="Legacy"><TestCase Name="Case">'
                "<Result>PASS</Result><Statement><Output>valid\x01text</Output>"
                "</Statement></TestCase></TestSuite>",
                encoding="utf-8",
            )

            html = render_legacy_xml_report(source)

            self.assertIn("validtext", html)

    def test_lp_library_builds_safe_command_and_stores_it(self) -> None:
        result = Executor().run(
            parse_script(
                "using LP\n"
                "Case\n"
                "{\n"
                "  LP.SelectFile(/tmp/sample document.pdf)\n"
                "  LP.SelectPrinter(Office Printer)\n"
                "  LP.SetCopies(2)\n"
                "  LP.SetOption(orientation-requested, 4)\n"
                "  LP.GetLpCommand(${command})\n"
                "}\n"
            )
        )

        self.assertEqual(result.status, Status.PASS)
        self.assertEqual(
            result.test_cases[0].steps[-1].output,
            "lp -d 'Office Printer' -n 2 -o orientation-requested=4 "
            "'/tmp/sample document.pdf'",
        )

    def test_lp_library_requires_file_before_printer_and_escapes_shell_values(self) -> None:
        result = Executor().run(
            parse_script(
                "using Lp\n"
                "Case\n"
                "{\n"
                "  Lp.SelectPrinter(printer; touch /tmp/unwanted)\n"
                "}\n"
            )
        )

        self.assertEqual(result.status, Status.ERROR)
        self.assertIn("File selection should be done", result.test_cases[0].steps[0].output)

        from gfriend_python.lp_library import LpLibrary

        variables: dict[str, str] = {}
        library = LpLibrary(variables)
        library.select_file("file; touch /tmp/unwanted")
        library.select_printer("printer; touch /tmp/unwanted")
        generated = library.get_lp_command("${cmd}")
        self.assertEqual(generated.status, Status.PASS)
        self.assertEqual(
            generated.output,
            "lp -d 'printer; touch /tmp/unwanted' "
            "'file; touch /tmp/unwanted'",
        )
        self.assertEqual(variables["${cmd}"], generated.output)

    def test_rest_library_sends_json_and_stores_response(self) -> None:
        received: dict[str, str] = {}

        class Handler(BaseHTTPRequestHandler):
            def do_POST(self) -> None:
                received["path"] = self.path
                received["body"] = self.rfile.read(int(self.headers["Content-Length"])).decode()
                received["header"] = self.headers["X-Test"]
                received["authorization"] = self.headers["Authorization"]
                self.send_response(200)
                self.end_headers()
                self.wfile.write(b'{"result":"ok"}')

            def log_message(self, format: str, *args: object) -> None:
                pass

        server = HTTPServer(("127.0.0.1", 0), Handler)
        thread = threading.Thread(target=server.serve_forever, daemon=True)
        thread.start()
        try:
            script = parse_script(
                "using REST\n"
                "Case\n"
                "{\n"
                f"  REST.InitializeRequest(http://127.0.0.1:{server.server_port}/echo, POST)\n"
                "  REST.AddHeader(X-Test, yes)\n"
                "  REST.AddBasicAuth(user, pass)\n"
                '  REST.AddJsonData({"ok":true})\n'
                "  REST.SendRequest(${body})\n"
                "}\n"
            )

            executor = Executor()
            result = executor.run(script)

            self.assertEqual(result.status, Status.PASS)
            self.assertEqual(executor.variables["${body}"], '{"result":"ok"}')
            self.assertEqual(received["path"], "/echo")
            self.assertEqual(received["body"], '{"ok":true}')
            self.assertEqual(received["header"], "yes")
            self.assertEqual(received["authorization"], "Basic dXNlcjpwYXNz")
        finally:
            server.shutdown()
            server.server_close()
            thread.join(timeout=2)

    def test_rest_library_stores_non_success_body_and_rejects_tls_bypass(self) -> None:
        class Handler(BaseHTTPRequestHandler):
            def do_GET(self) -> None:
                self.send_response(400)
                self.end_headers()
                self.wfile.write(b"bad request")

            def log_message(self, format: str, *args: object) -> None:
                pass

        server = HTTPServer(("127.0.0.1", 0), Handler)
        thread = threading.Thread(target=server.serve_forever, daemon=True)
        thread.start()
        try:
            result = Executor().run(
                parse_script(
                    "using Rest\n"
                    "Case\n"
                    "{\n"
                    f"  Rest.InitializeRequest(http://127.0.0.1:{server.server_port}/, GET)\n"
                    "  Rest.SendRequest(${response})\n"
                    "}\n"
                )
            )

            self.assertEqual(result.status, Status.FAIL)
            self.assertEqual(result.test_cases[0].steps[-1].output, "bad request")
        finally:
            server.shutdown()
            server.server_close()
            thread.join(timeout=2)

        result = Executor().run(
            parse_script("using Rest\nCase\n{\n  Rest.DisableSecurityWarning()\n}\n")
        )
        self.assertEqual(result.status, Status.ERROR)
        self.assertIn("not supported", result.test_cases[0].steps[0].output)

    def test_spreadsheet_library_creates_writes_reads_and_calculates(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            script = root / "spreadsheet.txt"
            script.write_text(
                "using SpreadSheet\n"
                "Case\n"
                "{\n"
                "  SpreadSheet.CreateNewSpreadSheetWithPath(data.xlsx)\n"
                "  SpreadSheet.AddRowHeader(Name, Status, Score)\n"
                "  SpreadSheet.AddRowData(alice, success, 8)\n"
                "  SpreadSheet.AddRowData(bob, failed, 5)\n"
                '  SpreadSheet.FormatExcelCell(2, 1, "fontstyle:bold;fontcolor:blue;backgroundcolor:yellow;border:1")\n'
                "  SpreadSheet.TotalRowCount(${rows})\n"
                "  SpreadSheet.TotalColumnCount(${columns})\n"
                "  SpreadSheet.ReadCellValueByIndex(2, 1, ${name})\n"
                "  SpreadSheet.CalculateFormulaFromCells(2, 3, 3, 3, add, ${sum}, 4, 3)\n"
                "  SpreadSheet.ReadCellValueByIndex(4, 3, ${calculated})\n"
                "  SpreadSheet.ValidateStatusFromSpreadsheet(Status, Name, success)\n"
                "}\n",
                encoding="utf-8",
            )

            executor = Executor(output_folder=root)
            result = executor.run(parse_file(str(script)))

            self.assertEqual(result.status, Status.FAIL)
            self.assertEqual(executor.variables["${rows}"], "3")
            self.assertEqual(executor.variables["${columns}"], "3")
            self.assertEqual(executor.variables["${name}"], "alice")
            self.assertEqual(executor.variables["${sum}"], "13.0")
            self.assertEqual(executor.variables["${calculated}"], "13")
            self.assertTrue((root / "data.xlsx").is_file())
            self.assertIn("bob (failed)", result.test_cases[0].steps[-1].output)
            from openpyxl import load_workbook

            workbook = load_workbook(root / "data.xlsx")
            try:
                self.assertTrue(workbook.active["A2"].font.bold)
                self.assertEqual(workbook.active["A2"].font.color.rgb, "FF0000FF")
                self.assertEqual(workbook.active["A2"].fill.fgColor.rgb, "FFFFFF00")
            finally:
                workbook.close()

    def test_spreadsheet_library_resolves_sheet_and_reports_invalid_cells(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            script = root / "spreadsheet.txt"
            script.write_text(
                "using SpreadSheet\n"
                "Case\n"
                "{\n"
                "  SpreadSheet.CreateNewSpreadSheetWithPath(data.xlsx)\n"
                "  SpreadSheet.SetWorksheet(Missing)\n"
                "}\n",
                encoding="utf-8",
            )

            result = Executor(output_folder=root).run(parse_file(str(script)))

            self.assertEqual(result.status, Status.ERROR)
            self.assertIn("Worksheet 'Missing' not found", result.test_cases[0].steps[-1].output)

    def test_info_library_records_timing_and_writes_svg_chart(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            result = Executor(output_folder=directory).run(
                parse_script(
                    'using Info\n'
                    'Case\n{\n'
                    '  Info.Start Time Check(Login)\n'
                    '  Info.Mark Start(Login,Authenticate)\n'
                    '  Sleep(0.001)\n'
                    '  Info.Mark End(Login,Authenticate)\n'
                    '  Info.Draw Time Graph(Login)\n'
                    '}\n'
                )
            )

            self.assertEqual(result.status, Status.PASS)
            csv_path = Path(directory) / "Login.csv"
            with csv_path.open(encoding="utf-8-sig", newline="") as stream:
                rows = list(csv.reader(stream))
            self.assertEqual(rows[0], ["Point", "Time (ms)"])
            self.assertEqual(rows[1][0], "Authenticate")
            self.assertGreater(float(rows[1][1]), 0)
            chart = (Path(directory) / "Login.svg").read_text(encoding="utf-8")
            self.assertIn("<svg", chart)
            self.assertIn("Authenticate", chart)

    def test_info_library_requires_measurement_before_marks_or_graphs(self) -> None:
        result = Executor().run(
            parse_script(
                'using Info\n'
                'Case\n{\n'
                '  Info.Mark Start(Missing,point)\n'
                '  Info.Mark End(Missing,point)\n'
                '  Info.Draw Time Graph(Missing)\n'
                '}\n'
            )
        )

        self.assertEqual(result.status, Status.ERROR)
        self.assertEqual(
            [step.output for step in result.test_cases[0].steps],
            [
                "Can not find Measure set with name Missing",
                "Can not find Measure set with name Missing",
                "Can not find Measure set with name Missing",
            ],
        )

    def test_log_info_library_records_results_and_writes_two_charts(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            result = Executor(output_folder=directory).run(
                parse_script(
                    'using LogInfo\n'
                    'Case\n{\n'
                    '  LogInfo.Start Time Check(Login)\n'
                    '  LogInfo.Start Time(Login,Authenticate)\n'
                    '  Sleep(0.001)\n'
                    '  LogInfo.End Time(Login,Authenticate)\n'
                    '  LogInfo.Draw Charts(Login)\n'
                    '}\n'
                )
            )

            self.assertEqual(result.status, Status.PASS)
            csv_path = Path(directory) / "Login.csv"
            with csv_path.open(encoding="utf-8-sig", newline="") as stream:
                rows = list(csv.reader(stream))
            self.assertEqual(rows[0], ["Point", "Time (ms)"])
            self.assertEqual(rows[1][0], "Authenticate")
            self.assertIn(rows[1][4], {"PASS", "FAIL", "ERROR"})
            self.assertTrue((Path(directory) / "Login_ColumnChart.svg").exists())
            pie = (Path(directory) / "Login_PieChart.svg").read_text(encoding="utf-8")
            self.assertIn("PASS", pie)

    def test_xml_library_loads_queries_mutates_and_saves_document(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            saved_xml = root / "saved.xml"
            result = Executor().run(
                parse_script(
                    'using XML\n'
                    'Case\n{\n'
                    '  XML.Load(<root><item id="1">before</item></root>)\n'
                    '  XML.Get Attribute(${Id},/root/item,id)\n'
                    '  XML.Get Inner Text(${Text},/root/item)\n'
                    '  XML.Replace Inner Text(after,/root/item)\n'
                    '  XML.Append To Inner Text(!,/root/item)\n'
                    '  XML.AppendNode(/root,<extra>value</extra>)\n'
                    f'  XML.Save File({saved_xml})\n'
                    '}\n'
                )
            )

            self.assertEqual(result.status, Status.PASS)
            outputs = [step.output for step in result.test_cases[0].steps]
            self.assertEqual(outputs[1:5], ["1", "before", "before", "after"])
            self.assertEqual(ET.parse(saved_xml).getroot().findtext("item"), "after!")
            self.assertEqual(ET.parse(saved_xml).getroot().findtext("extra"), "value")

    def test_xml_library_reports_unloaded_invalid_and_missing_nodes(self) -> None:
        result = Executor().run(
            parse_script(
                'using XML\n'
                'Case\n{\n'
                '  XML.Get Inner Text(${Text},/root/item)\n'
                '  XML.Load(<root>)\n'
                '  XML.Load(<root />)\n'
                '  XML.Get Inner Text(${Text},/root/missing)\n'
                '}\n'
            )
        )

        self.assertEqual(result.status, Status.ERROR)
        self.assertEqual(
            [step.output for step in result.test_cases[0].steps],
            [
                "XML is not loaded. Use Load or Load From File before getting values",
                "Error with xml",
                "",
                "Can not find xml node with given xpath",
            ],
        )

    def test_xml_library_resolves_namespaced_absolute_paths(self) -> None:
        result = Executor().run(
            parse_script(
                'using XML\n'
                'Case\n{\n'
                '  XML.Load(<p:root xmlns:p="urn:example"><p:item>found</p:item></p:root>)\n'
                '  XML.Get Inner Text(${Text},/p:root/p:item)\n'
                '  Pass(${Text})\n'
                '}\n'
            )
        )

        self.assertEqual(result.status, Status.PASS)
        self.assertEqual(result.test_cases[0].steps[-1].output, "found")

    def test_json_library_loads_selects_and_updates_json_values(self) -> None:
        result = Executor().run(
            parse_script(
                'using Json as J\n'
                '${Document}=\n'
                'Case\n{\n'
                '  J.Load Json({"customer":{"name":"Ada"}})\n'
                '  J.Get Value(${Name},$.customer.name)\n'
                '  J.Load Json([{"id":7}])\n'
                '  J.Get Value(${Item},$[0].id)\n'
                '  J.Add Json Element(${Document},enabled,true)\n'
                '  Pass(${Name}/${Item}/${Document})\n'
                '}\n'
            )
        )

        self.assertEqual(
            result.status,
            Status.PASS,
            [(step.keyword, step.status, step.output) for step in result.test_cases[0].steps],
        )
        self.assertEqual(result.test_cases[0].steps[1].output, "Ada")
        self.assertEqual(result.test_cases[0].steps[3].output, "7")
        self.assertIn('"enabled": true', result.test_cases[0].steps[4].output)
        self.assertIn("Ada/7/", result.test_cases[0].steps[5].output)

    def test_json_library_reports_load_path_and_target_errors(self) -> None:
        result = Executor().run(
            parse_script(
                'using Json\n'
                '${Target}=not-json\n'
                'Case\n{\n'
                '  Json.Get Value(${Value},$.name)\n'
                '  Json.Load Json("scalar")\n'
                '  Json.Get Value(${Value},$.name)\n'
                '  Json.Load Json({"name":"Ada"})\n'
                '  Json.Get Value(${Value},$.missing)\n'
                '  Json.Add Json Element(${Target},name,Ada)\n'
                '}\n'
            )
        )

        self.assertEqual(result.status, Status.ERROR)
        outputs = [step.output for step in result.test_cases[0].steps]
        self.assertEqual(
            outputs,
            [
                "Json is not loaded. Use Load Json keyword before using Get Value.",
                "Json parsing error.",
                "Json is not loaded. Use Load Json keyword before using Get Value.",
                "",
                "Can not find item with given json path ($.missing) in json string.",
                "Target variable is not a JSON formatted string: not-json",
            ],
        )

    def test_regex_library_matches_and_stores_selected_values(self) -> None:
        result = Executor().run(
            parse_script(
                'using Regex as Re\n'
                '${Message}=Issue INC-123 and INC-456\n'
                'Case\n{\n'
                '  Re.Set Pattern(INC-[0-9]+)\n'
                '  Re.Is Match(${Message})\n'
                '  Re.Get Match(${Message},${First})\n'
                '  Re.Get Match(${Message},1,${Second})\n'
                '  Re.Get Match Count(${Message},${Count})\n'
                '  Pass(${First}/${Second}/${Count})\n'
                '}\n'
            )
        )

        self.assertEqual(result.status, Status.PASS)
        self.assertEqual(result.test_cases[0].steps[1].output, "INC-123")
        self.assertEqual(result.test_cases[0].steps[2].output, "INC-123")
        self.assertEqual(result.test_cases[0].steps[3].output, "INC-456")
        self.assertEqual(result.test_cases[0].steps[4].output, "Matched count : 2")
        self.assertEqual(result.test_cases[0].steps[5].output, "INC-123/INC-456/2")

    def test_regex_library_reports_missing_invalid_and_out_of_range_patterns(self) -> None:
        result = Executor().run(
            parse_script(
                'using Regex\n'
                'Case\n{\n'
                '  Regex.Is Match(value)\n'
                '  Regex.Set Pattern([)\n'
                '  Regex.Get Match(value,save)\n'
                '  Regex.Set Pattern(value)\n'
                '  Regex.Get Match(value,-1,save)\n'
                '}\n'
            )
        )

        self.assertEqual(result.status, Status.ERROR)
        outputs = [step.output for step in result.test_cases[0].steps]
        self.assertEqual(
            outputs,
            [
                "Pattern is not set. Use Set Pattern keyword first.",
                "Can not initialize regex.",
                "Pattern is not set. Use Set Pattern keyword first.",
                "",
                "Match index is outside the available matches.",
            ],
        )

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

    def test_parses_a_literal_quote_argument(self) -> None:
        suite = parse_script(
            'Case\n{\n'
            '  Replace(${Value},",)\n'
            '}\n'
        )
        self.assertEqual(suite.test_cases[0].steps[0].arguments, ["${Value}", '"', ""])

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

    def test_remote_run_requires_and_uses_configured_adapter(self) -> None:
        suite = parse_script(
            '${Target}=worker-1\n'
            'Case\n{\n'
            '  RemoteRun: ${Target}\n'
            '  {\n'
            '    Pass(remote step)\n'
            '  }\n'
            '}\n'
        )

        missing_adapter = Executor().run(suite)
        self.assertEqual(missing_adapter.status, Status.ERROR)
        self.assertIn("remote_runner", missing_adapter.test_cases[0].steps[0].output)

        calls: list[tuple[str, list[object], dict[str, str]]] = []

        def remote_runner(remote_id, body, variables):
            calls.append((remote_id, body, variables))
            return KeywordResult(Status.PASS, "remote completed")

        result = Executor(remote_runner=remote_runner).run(suite)
        self.assertEqual(result.status, Status.PASS)
        self.assertEqual(result.test_cases[0].steps[0].output, "remote completed")
        self.assertEqual(calls[0][0], "worker-1")
        self.assertEqual(calls[0][2]["${Target}"], "worker-1")
        self.assertEqual(calls[0][1][0].keyword, "Pass")

    def test_parses_keyword_invocation_as_implicit_conditional_block(self) -> None:
        class ConditionLibrary:
            def condition(self, value: str) -> bool:
                return value == "ready"

        suite = parse_script(
            'using Condition\n'
            'Case\n{\n'
            '  Condition.Condition(ready)\n'
            '  {\n'
            '    Pass(ready branch)\n'
            '  }\n'
            '  Condition.Condition(not-ready) {\n'
            '    Fail(unselected)\n'
            '  }\n'
            '}\n'
        )
        executor = Executor()
        executor.register_library("Condition", ConditionLibrary())
        result = executor.run(suite)
        self.assertEqual(result.status, Status.PASS)
        self.assertEqual(
            [(step.status, step.output) for step in result.test_cases[0].steps],
            [(Status.PASS, ""), (Status.PASS, "ready branch"), (Status.FAIL, "")],
        )

    def test_parses_inline_opening_brace_and_ignores_legacy_comments(self) -> None:
        suite = parse_script(
            '\\\\\\ Prerequisites\n'
            '/ <Name>Inline</Name>\n'
            'Case\n'
            '{ Pass(inline body) }\n'
        )
        result = Executor().run(suite)
        self.assertEqual(result.status, Status.PASS)
        self.assertEqual(result.test_cases[0].steps[0].output, "inline body")

    def test_parses_delayed_if_opening_brace_with_preceding_step(self) -> None:
        suite = parse_script(
            'Case\n{\n'
            '  If: Equals(ready,ready)\n'
            '  Log(conditional setup)\n'
            '  {\n'
            '    Pass(conditional body)\n'
            '  }\n'
            '}\n'
        )
        result = Executor().run(suite)
        self.assertEqual(result.status, Status.PASS)
        self.assertEqual(
            [step.output for step in result.test_cases[0].steps],
            ["", "conditional setup", "conditional body"],
        )

    def test_parses_late_fail_branch_at_same_scope(self) -> None:
        suite = parse_script(
            'Case\n{\n'
            '  If: Equals(left,right)\n'
            '  {\n'
            '    Pass(unselected)\n'
            '  }\n'
            '  Pass(intervening statement)\n'
            '  Fail:\n'
            '  {\n'
            '    Pass(late fail branch)\n'
            '  }\n'
            '}\n'
        )
        result = Executor().run(suite)
        self.assertEqual(result.status, Status.PASS)
        self.assertEqual(
            [step.output for step in result.test_cases[0].steps],
            ["'left' does not equal 'right'", "late fail branch", "intervening statement"],
        )

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

    def test_while_condition_accepts_colons_and_variable_limit(self) -> None:
        result = Executor(variables={"Limit": "2"}).run(
            parse_script(
                'Loop\n{\n'
                '  While: Equals(http://device:8080, http://device:8080): ${Limit}\n'
                '  {\n'
                '    Pass(loop)\n'
                '  }\n'
                '}\n'
            )
        )
        self.assertEqual(result.status, Status.PASS)
        self.assertEqual(
            len([step for step in result.test_cases[0].steps if step.keyword.endswith(".Pass")]),
            2,
        )

    def test_parser_joins_multiline_keyword_arguments(self) -> None:
        class CaptureLibrary:
            def capture(self, first: str, second: str) -> KeywordResult:
                return KeywordResult(Status.PASS, f"{first}:{second}")

        suite = parse_script(
            'using Capture\n'
            'Case\n{\n'
            '  Capture.Capture(first,\n'
            '      second)\n'
            '}\n'
        )
        executor = Executor()
        executor.register_library("Capture", CaptureLibrary())
        result = executor.run(suite)
        self.assertEqual(result.status, Status.PASS)
        self.assertEqual(result.test_cases[0].steps[0].output, "first:second")
        self.assertEqual(result.test_cases[0].steps[0].line_number, 4)

    def test_while_accepts_trailing_colon_as_default_limit(self) -> None:
        result = Executor().run(
            parse_script(
                'Loop\n{\n'
                '  While:Equals(one,two):\n'
                '  {\n'
                '    Fail(should not execute)\n'
                '  }\n'
                '  Pass(done)\n'
                '}\n'
            )
        )
        self.assertEqual(result.status, Status.PASS)
        self.assertEqual([step.output for step in result.test_cases[0].steps], ["'one' does not equal 'two'", "done"])

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
        report_data = result.to_dict()
        self.assertEqual(report_data["status"], "FAIL")
        self.assertGreaterEqual(report_data["duration_seconds"], 0)
        self.assertIsNotNone(report_data["started_at"])

    def test_html_report_contains_results_and_escapes_user_content(self) -> None:
        result = Executor().run(
            parse_script("Unsafe <case>\n{\n  Fail(<script>alert(1)</script>)\n}\n")
        )
        report = render_html_report(result)
        self.assertIn("Unsafe &lt;case&gt;", report)
        self.assertIn("&lt;script&gt;alert(1)&lt;/script&gt;", report)
        self.assertNotIn("<script>alert(1)</script>", report)
        self.assertIn("FAIL", report)
        self.assertIn("Duration:", report)

    def test_xml_report_escapes_content_and_contains_case_results(self) -> None:
        result = Executor().run(
            parse_script("Unsafe <case>\n{\n  Fail(a & b < c\x01)\n}\n")
        )
        report = render_xml_report(result)
        root = ET.fromstring(report)
        self.assertEqual(root.tag, "TestSuite")
        self.assertEqual(root.attrib["name"], "TestSuite")
        self.assertEqual(root.findtext("Result"), "FAIL")
        case = root.find("./TestCases/TestCase")
        self.assertIsNotNone(case)
        self.assertEqual(case.attrib["name"], "Unsafe <case>")
        self.assertEqual(case.findtext("./Steps/Step/Output"), "a & b < c")


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

    def test_cli_writes_html_report_without_suppressing_json_stdout(self) -> None:
        import json
        import subprocess
        import sys

        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            script = root / "case.txt"
            html_report = root / "nested" / "report.html"
            script.write_text("CLI\n{\n  Pass(ok)\n}\n", encoding="utf-8")
            completed = subprocess.run(
                [
                    sys.executable,
                    "-m",
                    "gfriend_python",
                    str(script),
                    "--html-report",
                    str(html_report),
                ],
                capture_output=True,
                text=True,
                check=False,
            )
            self.assertEqual(completed.returncode, 0, completed.stderr)
            self.assertEqual(json.loads(completed.stdout)["status"], "PASS")
            self.assertIn("<title>GFriend report - case</title>", html_report.read_text(encoding="utf-8"))

    def test_cli_writes_xml_report_without_suppressing_json_stdout(self) -> None:
        import json
        import subprocess
        import sys

        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            script = root / "case.txt"
            xml_report = root / "nested" / "report.xml"
            script.write_text("CLI\n{\n  Pass(ok)\n}\n", encoding="utf-8")
            completed = subprocess.run(
                [
                    sys.executable,
                    "-m",
                    "gfriend_python",
                    str(script),
                    "--xml-report",
                    str(xml_report),
                ],
                capture_output=True,
                text=True,
                check=False,
            )
            self.assertEqual(completed.returncode, 0, completed.stderr)
            self.assertEqual(json.loads(completed.stdout)["status"], "PASS")
            self.assertEqual(ET.parse(xml_report).getroot().findtext("Result"), "PASS")

    def test_cli_writes_legacy_xml_report_without_suppressing_json_stdout(self) -> None:
        import json
        import subprocess
        import sys

        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            script = root / "case.txt"
            legacy_xml = root / "nested" / "output.xml"
            script.write_text("CLI\n{\n  Pass(ok)\n}\n", encoding="utf-8")
            completed = subprocess.run(
                [
                    sys.executable,
                    "-m",
                    "gfriend_python",
                    str(script),
                    "--legacy-xml-report",
                    str(legacy_xml),
                ],
                capture_output=True,
                text=True,
                check=False,
            )
            self.assertEqual(completed.returncode, 0, completed.stderr)
            self.assertEqual(json.loads(completed.stdout)["status"], "PASS")
            self.assertEqual(ET.parse(legacy_xml).getroot().findtext("TestCase/Result"), "PASS")

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

    def test_using_gfvar_imports_relative_variables_before_overrides(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            variables = root / "common.gfvar"
            variables.write_text(
                '${User}=library\n${Role}=Admin\n',
                encoding="utf-8",
            )
            script = root / "suite.txt"
            script.write_text(
                'using common.gfvar\n'
                '${User}=script\n'
                'Case\n{\n'
                '  Pass(${User}-${Role})\n'
                '}\n',
                encoding="utf-8",
            )
            suite = parse_file(script, {"User": "execution"})
            result = Executor().run(suite)
            self.assertEqual(suite.libraries, {})
            self.assertEqual(result.status, Status.PASS)
            self.assertEqual(result.test_cases[0].steps[0].output, "execution-Admin")

    def test_using_gfvar_reports_missing_file(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            script = Path(directory) / "suite.txt"
            script.write_text(
                'using missing.gfvar\n'
                'Case\n{\n  Pass(ok)\n}\n',
                encoding="utf-8",
            )
            with self.assertRaisesRegex(ParseError, "Cannot read variable file"):
                parse_file(script)

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

    def test_selected_rows_expand_variables_and_accept_double_equals(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root / "values.csv").write_text(
                "TCID,Role\n7,Admin\n8,User\n9,Admin\n",
                encoding="utf-8",
            )
            script = root / "suite.txt"
            script.write_text(
                '${Minimum}=8\n'
                'dataset "values.csv" as Test\n'
                'Selected\n{\n'
                '  For Selected Row:Test.Sheet.Table.TCID>=${Minimum}\n'
                '  {\n'
                '    Pass(${TCID})\n'
                '  }\n'
                '  For Selected Row:Test.Sheet.Table.Role==Admin\n'
                '  {\n'
                '    Pass(${Role})\n'
                '  }\n'
                '}\n',
                encoding="utf-8",
            )
            result = Executor().run(parse_file(script))
            self.assertEqual(result.status, Status.PASS)
            self.assertEqual(
                [step.output for step in result.test_cases[0].steps],
                ["8", "9", "Admin", "Admin"],
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

    def test_comparison_and_variable_string_builtins(self) -> None:
        result = Executor().run(
            parse_script(
                '${Text}=  foo-bar  \n'
                'Case\n{\n'
                '  NotEquals(one,two)\n'
                '  CompareAmounts(₹1,1.0)\n'
                '  Trim(${Text})\n'
                '  Replace(${Text},foo,baz)\n'
                '  Substring(${Text},0,3,${Part})\n'
                '  Split And Store Word(one two three, ,1,${Word})\n'
                '  Count(${Text})\n'
                '  Pass(${Part}-${Word}-${Text})\n'
                '}\n'
            )
        )
        self.assertEqual(result.status, Status.PASS)
        outputs = [step.output for step in result.test_cases[0].steps]
        self.assertEqual(outputs[2], "${Text} = foo-bar")
        self.assertEqual(outputs[3], "${Text} = baz-bar")
        self.assertEqual(outputs[4], "Substring of baz-bar stored in ${Part}: baz")
        self.assertEqual(outputs[5], "Word at index 1 from one two three stored in ${Word}: two")
        self.assertEqual(outputs[6], "Number of characters in ${Text}: 7")
        self.assertEqual(outputs[7], "baz-two-baz-bar")

    def test_date_difference_random_number_and_append_to_file(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            script = root / "suite.txt"
            script.write_text(
                'Case\n{\n'
                '  Get Time Difference(2:48:25,2:54:25,${Difference})\n'
                '  Get Random Number(7,7,${Random})\n'
                '  Calculate(2+3,${Total})\n'
                '  Append To File(result.txt,entry)\n'
                '  Pass(${Difference}-${Random}-${Total})\n'
                '}\n',
                encoding="utf-8",
            )
            result = Executor(output_folder=root).run(parse_file(script))
            self.assertEqual(result.status, Status.PASS)
            self.assertEqual(result.test_cases[0].steps[-1].output, "00:06:00-7-5")
            self.assertEqual((root / "result.txt").read_text(encoding="utf-8"), "\nentry")

    def test_invalid_date_difference_returns_error(self) -> None:
        result = Executor().run(
            parse_script(
                'Case\n{\n'
                '  Get Time Difference(not-a-date,2:54:25,${Difference})\n'
                '}\n'
            )
        )
        self.assertEqual(result.status, Status.ERROR)
        self.assertEqual(
            result.test_cases[0].steps[0].output,
            "Cannot convert given string into DateTime Format",
        )

    def test_timestamp_conversion_concatenation_and_numeric_comparison(self) -> None:
        result = Executor().run(
            parse_script(
                '${Date}=03/12/24 14:05\n'
                '${First}=Hello\n'
                '${Second}=World\n'
                '${FirstSecond}=joined\n'
                'Case\n{\n'
                '  Convert Time Stamp Format(${Date},MM/dd/yy H:mm,"MMM dd, yyyy")\n'
                '  Concatenate Variable Values And Store Result(${First},${Second},${Combined})\n'
                '  Concatenate Variables And Store Result(${First},${Second},${Lookup})\n'
                '  Has Value Increased(2,3)\n'
                '  Pass(${Date}-${Combined}-${Lookup})\n'
                '}\n'
            )
        )
        self.assertEqual(result.status, Status.PASS)
        outputs = [step.output for step in result.test_cases[0].steps]
        self.assertEqual(outputs[0], "${Date} = Mar 12, 2024")
        self.assertEqual(outputs[1], "${Combined} = HelloWorld")
        self.assertEqual(outputs[2], "${Lookup} : joined")
        self.assertEqual(outputs[3], "Value increased: 3.0 > 2.0")
        self.assertEqual(outputs[4], "Mar 12, 2024-HelloWorld-joined")

    def test_timestamp_conversion_reports_invalid_source(self) -> None:
        result = Executor().run(
            parse_script(
                '${Date}=not-a-date\n'
                'Case\n{\n'
                '  ConvertTimeStampFormat(${Date},MM/dd/yy,yyyy)\n'
                '}\n'
            )
        )
        self.assertEqual(result.status, Status.ERROR)
        self.assertIn("Could not convert timestamp", result.test_cases[0].steps[0].output)

    def test_assert_last_repeat_checks_iteration_result_counts(self) -> None:
        result = Executor().run(
            parse_script(
                'Case\n{\n'
                '  For: a, b, c\n'
                '  {\n'
                '    If: Equals(${ITEM},b)\n'
                '    {\n'
                '      Fail(matched failure)\n'
                '    }\n'
                '    Fail:\n'
                '    {\n'
                '      Pass(other iteration)\n'
                '    }\n'
                '    Pass(iteration)\n'
                '  }\n'
                '  Assert Last Repeat(PASS,>=2)\n'
                '  AssertLastRepeat(FAIL,=1)\n'
                '  AssertLastRepeat(ERROR,<1)\n'
                '}\n'
            )
        )
        self.assertEqual(result.status, Status.FAIL)
        self.assertEqual(
            [step.output for step in result.test_cases[0].steps[-3:]],
            [
                "PASS count of last repeat is 2.",
                "FAIL count of last repeat is 1.",
                "ERROR count of last repeat is 0.",
            ],
        )

    def test_assert_last_repeat_rejects_invalid_status_or_condition(self) -> None:
        result = Executor().run(
            parse_script(
                'Case\n{\n'
                '  AssertLastRepeat(SKIP,=1)\n'
                '  AssertLastRepeat(PASS,!=1)\n'
                '}\n'
            )
        )
        self.assertEqual(result.status, Status.ERROR)
        self.assertIn("unknown result type", result.test_cases[0].steps[0].output)
        self.assertIn("threshold number", result.test_cases[0].steps[1].output)

    def test_portable_system_information_keywords(self) -> None:
        from unittest.mock import patch

        with patch("gfriend_python.executor.platform.processor", return_value="Test CPU"):
            result = Executor().run(
                parse_script(
                    'Case\n{\n'
                    '  Get System Information(${CPU})\n'
                    '  Get RAM(${Memory})\n'
                    '  Pass(${CPU}-${Memory})\n'
                    '}\n'
                )
            )
        self.assertEqual(result.status, Status.PASS)
        outputs = [step.output for step in result.test_cases[0].steps]
        self.assertEqual(outputs[0], "System Information: Test CPU")
        self.assertRegex(outputs[1], r"^RAM Size: \d+ GB$")
        self.assertRegex(outputs[2], r"^Test CPU-RAM:\d+GB$")

    def test_windows_version_keyword_errors_on_non_windows(self) -> None:
        from unittest.mock import patch

        with patch("gfriend_python.executor.platform.system", return_value="Linux"):
            result = Executor().run(
                parse_script("Case\n{\n  Get Windows Version(${Version})\n}\n")
            )
        self.assertEqual(result.status, Status.ERROR)
        self.assertEqual(
            result.test_cases[0].steps[0].output,
            "Get Windows Version is only available on Windows.",
        )

    def test_manual_interaction_keywords_use_injected_callback(self) -> None:
        interactions: list[tuple[str, bool]] = []

        def interact(message: str, verification: bool) -> KeywordResult:
            interactions.append((message, verification))
            if verification:
                return KeywordResult(Status.FAIL, "image is blurry")
            return KeywordResult(Status.PASS)

        result = Executor(user_interaction=interact).run(
            parse_script(
                'Case\n{\n'
                '  Wait For User Confirm(Load paper)\n'
                '  Request User Verification(Check output)\n'
                '}\n'
            )
        )
        self.assertEqual(result.status, Status.FAIL)
        self.assertEqual(interactions, [("Load paper", False), ("Check output", True)])
        self.assertEqual(result.test_cases[0].steps[1].output, "image is blurry")

    def test_manual_interaction_reports_missing_callback(self) -> None:
        result = Executor().run(
            parse_script("Case\n{\n  Wait For User Confirm(Continue)\n}\n")
        )
        self.assertEqual(result.status, Status.ERROR)
        self.assertEqual(
            result.test_cases[0].steps[0].output,
            "Wait For User Confirm requires an interactive user callback.",
        )

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

    def test_spreadsheet_foreachrow_reads_and_writes_worksheet_rows(self) -> None:
        from openpyxl import Workbook, load_workbook

        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            data_path = root / "data.xlsx"
            workbook = Workbook()
            worksheet = workbook.active
            worksheet.title = "Data"
            worksheet.append(["Name", "Role"])
            worksheet.append(["Ada", "Admin"])
            worksheet.append(["Lin", "User"])
            workbook.save(data_path)

            script_path = root / "suite.txt"
            script_path.write_text(
                'dataset "data.xlsx" as Users\n'
                'Case\n{\n'
                '  SpreadSheetForEachRow: Users.Data.Rows\n  {\n'
                '    If: Equals(${Role}, Admin)\n    {\n'
                '      Write To Data Set(Role,Privileged)\n'
                '    }\n'
                '    Pass(${Name}-${Role})\n'
                '  }\n'
                '}\n',
                encoding="utf-8",
            )
            result = Executor().run(parse_file(script_path))
            self.assertEqual(result.status, Status.PASS)
            self.assertEqual(
                [step.output for step in result.test_cases[0].steps if step.keyword.endswith(".Pass")],
                ["Ada-Admin", "Lin-User"],
            )
            updated = load_workbook(data_path, data_only=True)
            try:
                self.assertEqual(updated["Data"]["B2"].value, "Privileged")
                self.assertEqual(updated["Data"]["B3"].value, "User")
            finally:
                updated.close()

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

    def test_write_to_dataset_advanced_targets_declared_excel_dataset(self) -> None:
        from openpyxl import Workbook, load_workbook

        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            first_path = root / "first.xlsx"
            second_path = root / "second.xlsx"
            for path in (first_path, second_path):
                workbook = Workbook()
                workbook.active.title = "Data"
                workbook.save(path)
                workbook.close()

            suite_path = root / "suite.txt"
            suite_path.write_text(
                'dataset "first.xlsx" as First\n'
                'dataset "second.xlsx" as Second\n'
                'Case\n{\n'
                '  Write To Data Set Advanced(Data,3,AA,updated,Second)\n'
                '}\n',
                encoding="utf-8",
            )
            result = Executor().run(parse_file(suite_path))
            self.assertEqual(result.status, Status.PASS)

            first = load_workbook(first_path, data_only=True)
            second = load_workbook(second_path, data_only=True)
            try:
                self.assertIsNone(first["Data"]["AA3"].value)
                self.assertEqual(second["Data"]["AA3"].value, "updated")
            finally:
                first.close()
                second.close()

    def test_write_to_dataset_advanced_requires_unambiguous_dataset(self) -> None:
        result = Executor().run(
            parse_script(
                'Case\n{\n'
                '  Write To Data Set Advanced(Data,1,A,value)\n'
                '}\n'
            )
        )
        self.assertEqual(result.status, Status.ERROR)
        self.assertIn("one declared Excel dataset", result.test_cases[0].steps[0].output)

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

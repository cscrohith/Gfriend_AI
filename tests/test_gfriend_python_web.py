from __future__ import annotations

import base64
import json
import tempfile
import threading
import time
import unittest
from pathlib import Path
from urllib.error import HTTPError
from urllib.request import Request, urlopen

from gfriend_python.web import WebApplication, create_server


class WebWorkspaceTests(unittest.TestCase):
    def setUp(self) -> None:
        self.temporary_directory = tempfile.TemporaryDirectory()
        self.root = Path(self.temporary_directory.name)
        self.scripts = self.root / "scripts"
        self.scripts.mkdir()
        (self.scripts / "smoke.txt").write_text(
            "${Account}=default-value\n"
            "Smoke\n{\n"
            "  Pass(${Account})\n"
            "}\n",
            encoding="utf-8",
        )
        self.app = WebApplication(self.scripts, output_folder=self.root / "output")
        self.server = create_server(self.app, port=0)
        self.server_thread = threading.Thread(target=self.server.serve_forever)
        self.server_thread.start()
        self.base_url = f"http://127.0.0.1:{self.server.server_address[1]}"

    def tearDown(self) -> None:
        self.server.shutdown()
        self.server.server_close()
        self.server_thread.join(timeout=5)
        self.app.close()
        self.temporary_directory.cleanup()

    def get_json(self, path: str) -> dict:
        with urlopen(f"{self.base_url}{path}", timeout=5) as response:
            return json.loads(response.read())

    def post_json(self, path: str, payload: object) -> tuple[int, dict]:
        request = Request(
            f"{self.base_url}{path}",
            data=json.dumps(payload).encode("utf-8"),
            headers={"Content-Type": "application/json"},
            method="POST",
        )
        try:
            with urlopen(request, timeout=5) as response:
                return response.status, json.loads(response.read())
        except HTTPError as error:
            return error.code, json.loads(error.read())

    def test_workspace_serves_enterprise_ui_and_local_security_headers(self) -> None:
        with urlopen(self.base_url, timeout=5) as response:
            html = response.read().decode("utf-8")
            self.assertEqual(response.status, 200)
            self.assertEqual(response.headers["X-Content-Type-Options"], "nosniff")
            self.assertIn("Content-Security-Policy", response.headers)
        self.assertIn("Execution workspace", html)
        self.assertIn("Script library", html)
        self.assertIn("Run configuration", html)

    def test_script_listing_and_preview_hide_default_variable_values(self) -> None:
        scripts = self.get_json("/api/scripts")["scripts"]
        self.assertEqual(len(scripts), 1)
        details = self.get_json(f"/api/scripts/{scripts[0]['id']}")
        self.assertEqual(details["cases"], ["Smoke"])
        self.assertEqual(details["variables"], ["Account"])
        self.assertNotIn("default-value", json.dumps(details))

    def test_run_endpoint_executes_selected_script_with_variable_override(self) -> None:
        script = self.get_json("/api/scripts")["scripts"][0]
        status, created = self.post_json(
            "/api/runs",
            {
                "script_id": script["id"],
                "test_cases": ["Smoke"],
                "variables": {"Account": "enterprise"},
            },
        )
        self.assertEqual(status, 202)

        deadline = time.monotonic() + 5
        while time.monotonic() < deadline:
            run = self.get_json(f"/api/runs/{created['id']}")
            if run["status"] != "RUNNING":
                break
            time.sleep(0.02)
        self.assertEqual(run["status"], "PASS")
        self.assertEqual(run["result"]["test_cases"][0]["steps"][0]["output"], "enterprise")
        self.assertIn("<title>GFriend report", run["html_report"])

    def test_run_rejects_invalid_script_ids_and_variables(self) -> None:
        traversal_id = base64.urlsafe_b64encode(b"../outside.txt").decode().rstrip("=")
        status, response = self.post_json(
            "/api/runs",
            {"script_id": traversal_id, "variables": {}},
        )
        self.assertEqual(status, 400)
        self.assertIn("Script is not available", response["error"])

        script = self.get_json("/api/scripts")["scripts"][0]
        status, response = self.post_json(
            "/api/runs",
            {"script_id": script["id"], "variables": {"../file": "value"}},
        )
        self.assertEqual(status, 400)
        self.assertIn("Variable names or values", response["error"])

    def test_server_refuses_non_loopback_bind_addresses(self) -> None:
        with self.assertRaisesRegex(ValueError, "loopback"):
            create_server(self.app, host="0.0.0.0", port=0)


if __name__ == "__main__":
    unittest.main()

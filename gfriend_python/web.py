from __future__ import annotations

import base64
import json
import re
import threading
import uuid
from concurrent.futures import Future, ThreadPoolExecutor
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
from typing import Any
from urllib.parse import unquote, urlsplit

from .executor import Executor
from .parser import ParseError, parse_file
from .report import render_html_report

_SCRIPT_SUFFIXES = {".txt", ".gfscripts"}
_VARIABLE_NAME = re.compile(r"^[A-Za-z0-9_-]+$")
_MAX_REQUEST_BYTES = 1_000_000
_MAX_VARIABLES = 250
_MAX_VARIABLE_VALUE_LENGTH = 10_000
_MAX_RETAINED_JOBS = 50


class WebApplication:
    """Local web workspace for selecting and running trusted GFriend scripts."""

    def __init__(
        self,
        scripts_root: str | Path,
        *,
        output_folder: str | Path | None = None,
    ) -> None:
        self.scripts_root = Path(scripts_root).resolve()
        if not self.scripts_root.is_dir():
            raise ValueError(f"Scripts directory does not exist: {self.scripts_root}")
        self.output_folder = Path(output_folder).resolve() if output_folder else Path.cwd() / "gfriend-output"
        self._lock = threading.Lock()
        self._jobs: dict[str, dict[str, Any]] = {}
        self._active_job: str | None = None
        self._worker = ThreadPoolExecutor(max_workers=1, thread_name_prefix="gfriend-web")
        self._futures: set[Future[None]] = set()

    def close(self) -> None:
        self._worker.shutdown(wait=True, cancel_futures=True)

    @staticmethod
    def _encode_id(relative_path: Path) -> str:
        return base64.urlsafe_b64encode(
            relative_path.as_posix().encode("utf-8")
        ).decode("ascii").rstrip("=")

    def _resolve_script(self, script_id: str) -> Path:
        try:
            encoded = script_id.encode("ascii")
            encoded += b"=" * (-len(encoded) % 4)
            relative_name = base64.b64decode(encoded, altchars=b"-_", validate=True).decode("utf-8")
            relative_path = Path(relative_name)
            candidate = (self.scripts_root / relative_path).resolve()
        except (UnicodeError, ValueError, OSError) as error:
            raise ValueError("Invalid script selection") from error

        if (
            relative_path.is_absolute()
            or candidate == self.scripts_root
            or not candidate.is_relative_to(self.scripts_root)
            or not candidate.is_file()
            or candidate.suffix.casefold() not in _SCRIPT_SUFFIXES
        ):
            raise ValueError("Script is not available in the configured scripts directory")
        return candidate

    def list_scripts(self) -> list[dict[str, str]]:
        scripts = []
        for path in self.scripts_root.rglob("*"):
            if not path.is_file() or path.suffix.casefold() not in _SCRIPT_SUFFIXES:
                continue
            try:
                resolved = path.resolve()
                relative_path = resolved.relative_to(self.scripts_root)
            except (OSError, ValueError):
                continue
            if any(part.startswith(".") for part in relative_path.parts):
                continue
            scripts.append(
                {
                    "id": self._encode_id(relative_path),
                    "name": path.stem,
                    "path": relative_path.as_posix(),
                }
            )
        return sorted(scripts, key=lambda script: script["path"].casefold())

    def describe_script(self, script_id: str) -> dict[str, Any]:
        path = self._resolve_script(script_id)
        suite = parse_file(path)
        return {
            "id": script_id,
            "name": path.stem,
            "path": path.relative_to(self.scripts_root).as_posix(),
            "cases": [case.name for case in suite.test_cases],
            "variables": sorted(
                variable.removeprefix("${").removesuffix("}")
                for variable in suite.variables
            ),
        }

    def submit_run(self, request: object) -> dict[str, str]:
        if not isinstance(request, dict):
            raise ValueError("Request body must be a JSON object")
        script_id = request.get("script_id")
        if not isinstance(script_id, str):
            raise ValueError("script_id must be a string")
        script_path = self._resolve_script(script_id)

        selected_cases = request.get("test_cases")
        if selected_cases is not None and (
            not isinstance(selected_cases, list)
            or not selected_cases
            or len(selected_cases) > 500
            or not all(isinstance(case, str) and len(case) <= 500 for case in selected_cases)
        ):
            raise ValueError("test_cases must be a non-empty list of test case names")

        variables = request.get("variables", {})
        if not isinstance(variables, dict) or len(variables) > _MAX_VARIABLES:
            raise ValueError(f"variables must be an object with at most {_MAX_VARIABLES} entries")
        validated_variables: dict[str, str] = {}
        for name, value in variables.items():
            if (
                not isinstance(name, str)
                or not _VARIABLE_NAME.fullmatch(name)
                or not isinstance(value, str)
                or len(value) > _MAX_VARIABLE_VALUE_LENGTH
            ):
                raise ValueError("Variable names or values are invalid")
            validated_variables[name] = value

        with self._lock:
            if self._active_job is not None:
                raise RuntimeError("Another execution is already running")
            job_id = uuid.uuid4().hex
            self._active_job = job_id
            self._jobs[job_id] = {
                "id": job_id,
                "status": "RUNNING",
                "script": script_path.relative_to(self.scripts_root).as_posix(),
                "result": None,
                "error": None,
                "html_report": None,
            }
            self._trim_jobs()
            future = self._worker.submit(
                self._run_script,
                job_id,
                script_path,
                validated_variables,
                selected_cases,
            )
            self._futures.add(future)
            future.add_done_callback(self._discard_future)
        return {"id": job_id, "status": "RUNNING"}

    def get_job(self, job_id: str) -> dict[str, Any] | None:
        with self._lock:
            job = self._jobs.get(job_id)
            return dict(job) if job is not None else None

    def _discard_future(self, future: Future[None]) -> None:
        with self._lock:
            self._futures.discard(future)

    def _trim_jobs(self) -> None:
        finished = [
            job_id
            for job_id, job in self._jobs.items()
            if job_id != self._active_job and job["status"] != "RUNNING"
        ]
        while len(self._jobs) > _MAX_RETAINED_JOBS and finished:
            self._jobs.pop(finished.pop(0), None)

    def _run_script(
        self,
        job_id: str,
        script_path: Path,
        variables: dict[str, str],
        selected_cases: list[str] | None,
    ) -> None:
        try:
            suite = parse_file(script_path, variables)
            result = Executor(output_folder=self.output_folder).run(
                suite,
                set(selected_cases) if selected_cases is not None else None,
            )
            payload = result.to_dict()
            html_report = render_html_report(result)
            with self._lock:
                job = self._jobs.get(job_id)
                if job is not None:
                    job.update(
                        status=result.status.value,
                        result=payload,
                        html_report=html_report,
                    )
        except Exception as error:
            with self._lock:
                job = self._jobs.get(job_id)
                if job is not None:
                    job.update(status="ERROR", error=str(error))
        finally:
            with self._lock:
                if self._active_job == job_id:
                    self._active_job = None
                self._trim_jobs()


class _LocalThreadingHTTPServer(ThreadingHTTPServer):
    daemon_threads = True
    allow_reuse_address = True


def create_server(
    app: WebApplication,
    *,
    host: str = "127.0.0.1",
    port: int = 8765,
) -> ThreadingHTTPServer:
    if host != "127.0.0.1":
        raise ValueError("The GFriend web workspace must bind to a loopback address")

    class RequestHandler(BaseHTTPRequestHandler):
        server_version = "GFriendWeb/1.0"
        sys_version = ""

        def _send(self, status: int, content: bytes, content_type: str) -> None:
            self.send_response(status)
            self.send_header("Content-Type", content_type)
            self.send_header("Content-Length", str(len(content)))
            self.send_header("Cache-Control", "no-store")
            self.send_header("X-Content-Type-Options", "nosniff")
            self.send_header("Referrer-Policy", "no-referrer")
            self.send_header(
                "Content-Security-Policy",
                "default-src 'self'; style-src 'self' 'unsafe-inline'; "
                "script-src 'self' 'unsafe-inline'; connect-src 'self'; "
                "img-src 'self' data:; base-uri 'none'; frame-ancestors 'none'",
            )
            self.end_headers()
            self.wfile.write(content)

        def _send_json(self, status: int, value: object) -> None:
            content = json.dumps(value, ensure_ascii=False).encode("utf-8")
            self._send(status, content, "application/json; charset=utf-8")

        def _read_json(self) -> object:
            raw_length = self.headers.get("Content-Length", "")
            try:
                content_length = int(raw_length)
            except ValueError as error:
                raise ValueError("A valid Content-Length header is required") from error
            if content_length < 0 or content_length > _MAX_REQUEST_BYTES:
                raise ValueError("Request body is too large")
            try:
                return json.loads(self.rfile.read(content_length))
            except (UnicodeDecodeError, json.JSONDecodeError) as error:
                raise ValueError("Request body must contain valid JSON") from error

        def do_GET(self) -> None:
            path = urlsplit(self.path).path
            if path == "/":
                try:
                    content = Path(__file__).with_name("web_ui.html").read_bytes()
                except OSError:
                    self._send_json(500, {"error": "Web interface asset is unavailable"})
                    return
                self._send(200, content, "text/html; charset=utf-8")
                return
            if path == "/api/scripts":
                self._send_json(200, {"scripts": app.list_scripts()})
                return
            if path.startswith("/api/scripts/"):
                script_id = unquote(path.removeprefix("/api/scripts/"))
                try:
                    self._send_json(200, app.describe_script(script_id))
                except (ValueError, ParseError) as error:
                    self._send_json(400, {"error": str(error)})
                return
            if path.startswith("/api/runs/"):
                job_id = path.removeprefix("/api/runs/")
                if not re.fullmatch(r"[a-f0-9]{32}", job_id):
                    self._send_json(404, {"error": "Run not found"})
                    return
                job = app.get_job(job_id)
                if job is None:
                    self._send_json(404, {"error": "Run not found"})
                else:
                    self._send_json(200, job)
                return
            self._send_json(404, {"error": "Not found"})

        def do_POST(self) -> None:
            if urlsplit(self.path).path != "/api/runs":
                self._send_json(404, {"error": "Not found"})
                return
            try:
                run = app.submit_run(self._read_json())
            except RuntimeError as error:
                self._send_json(409, {"error": str(error)})
            except (ValueError, ParseError) as error:
                self._send_json(400, {"error": str(error)})
            else:
                self._send_json(202, run)

        def log_message(self, format: str, *args: object) -> None:
            return

    return _LocalThreadingHTTPServer((host, port), RequestHandler)


def main(argv: list[str] | None = None) -> int:
    import argparse
    import webbrowser

    parser = argparse.ArgumentParser(description="Open the local GFriend web execution workspace.")
    parser.add_argument(
        "--scripts-dir",
        type=Path,
        default=Path.cwd() / "GfriendScripts",
        help="Root directory containing trusted GFriend scripts (default: ./GfriendScripts)",
    )
    parser.add_argument("--port", type=int, default=8765, help="Local web port")
    parser.add_argument("--no-browser", action="store_true", help="Do not open a browser")
    args = parser.parse_args(argv)

    try:
        app = WebApplication(args.scripts_dir)
        server = create_server(app, port=args.port)
    except (OSError, ValueError) as error:
        parser.error(str(error))

    url = f"http://127.0.0.1:{server.server_address[1]}/"
    print(f"GFriend web workspace: {url}")
    print("Only run scripts and datasets you trust. Press Ctrl+C to stop.")
    if not args.no_browser:
        webbrowser.open(url)
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        print("\nStopping GFriend web workspace.")
    finally:
        server.shutdown()
        server.server_close()
        app.close()
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

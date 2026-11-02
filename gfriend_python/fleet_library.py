from __future__ import annotations

import socket
from pathlib import Path

from .models import DeviceUnderTest, KeywordResult, Status


class FleetLibrary:
    """Send raw text or file bytes to a configured printer TCP port."""

    def __init__(
        self,
        device: DeviceUnderTest | None,
        *,
        script_folder: str = "",
        timeout: float = 10,
    ) -> None:
        self._device = device
        self._script_folder = Path(script_folder or Path.cwd())
        self._timeout = timeout

    def send_text(self, text: str, port: str = "9100") -> KeywordResult:
        return self._send(text.encode("utf-8"), port)

    def send_file(self, file_path: str, port: str = "9100") -> KeywordResult:
        path = Path(file_path)
        if not path.is_absolute():
            path = self._script_folder / path
        try:
            data = path.read_bytes()
        except OSError as error:
            return KeywordResult(Status.ERROR, f"Can not read file '{path}': {error}")
        return self._send(data, port)

    def _send(self, payload: bytes, port: str) -> KeywordResult:
        if self._device is None:
            return KeywordResult(
                Status.ERROR,
                "Fleet requires a configured DeviceUnderTest in the run spec.",
            )
        host = self._device.device_address.strip()
        if not host:
            return KeywordResult(
                Status.ERROR,
                "Configured DeviceUnderTest has no DeviceAddress.",
            )
        try:
            port_number = int(port.strip())
        except ValueError:
            return KeywordResult(Status.ERROR, f"Port must be a number {port}")
        if not 1 <= port_number <= 65535:
            return KeywordResult(Status.ERROR, f"Port must be between 1 and 65535: {port}")
        try:
            with socket.create_connection((host, port_number), timeout=self._timeout) as connection:
                connection.sendall(payload)
        except (OSError, OverflowError) as error:
            return KeywordResult(Status.ERROR, f"Error during IP Send to {host}:{port_number}: {error}")
        return KeywordResult(
            Status.PASS,
            f"Sent {len(payload)} byte(s) to {host}:{port_number}.",
        )

from __future__ import annotations

import re
import shlex
import subprocess
import time
import xml.etree.ElementTree as ET
from collections.abc import Callable
from pathlib import Path
from typing import Any

from .models import DeviceUnderTest, KeywordResult, Status


class _AdbBackend:
    def __init__(
        self,
        device: DeviceUnderTest | None,
        *,
        adb_path: str = "adb",
        run: Callable[..., Any] = subprocess.run,
    ) -> None:
        self._device = device
        self._adb_path = adb_path
        self._run = run
        serial = ""
        if device is not None:
            serial = device.device_address.strip() or device.device_id.strip()
            if serial and device.port and ":" not in serial:
                serial = f"{serial}:{device.port}"
        self._serial = serial

    def _invoke(self, *arguments: str, timeout: float = 30) -> bytes:
        command = [self._adb_path]
        if self._serial:
            command.extend(("-s", self._serial))
        command.extend(arguments)
        try:
            completed = self._run(
                command,
                check=False,
                capture_output=True,
                timeout=max(timeout, 0.1),
            )
        except FileNotFoundError as error:
            raise RuntimeError("Android Debug Bridge (adb) was not found on PATH.") from error
        if completed.returncode:
            stderr = completed.stderr.decode("utf-8", errors="replace").strip()
            raise RuntimeError(stderr or f"adb exited with status {completed.returncode}")
        return completed.stdout

    def _shell(self, *arguments: str, timeout: float = 30) -> bytes:
        return self._invoke("shell", shlex.join(arguments), timeout=timeout)

    def launch(self, activity_name: str) -> None:
        if not activity_name.strip():
            raise ValueError("Activity or package name cannot be empty.")
        if "/" in activity_name:
            package, activity = activity_name.split("/", 1)
            self._shell("am", "start", "-n", f"{package}/{activity}")
        else:
            self._shell("monkey", "-p", activity_name, "-c", "android.intent.category.LAUNCHER", "1")

    def ping(self) -> None:
        self._invoke("get-state")

    def hierarchy(self) -> ET.Element:
        self._shell("uiautomator", "dump", "/sdcard/window.xml", timeout=60)
        xml = self._invoke("exec-out", "cat", "/sdcard/window.xml", timeout=30)
        try:
            return ET.fromstring(xml)
        except ET.ParseError as error:
            raise RuntimeError(f"Device returned invalid UI hierarchy XML: {error}") from error

    @staticmethod
    def _matches(root: ET.Element, selector: str, by: str) -> list[ET.Element]:
        if by == "xpath":
            path = selector if selector.startswith(".") else f".{selector}"
            try:
                return list(root.findall(path))
            except SyntaxError as error:
                raise ValueError(f"Unsupported UI XPath '{selector}': {error}") from error
        attribute = {"text": "text", "id": "resource-id"}[by]
        return [item for item in root.iter("node") if item.get(attribute) == selector]

    def find(self, selector: str, by: str) -> list[ET.Element]:
        return self._matches(self.hierarchy(), selector, by)

    def find_text_contains(self, text: str) -> list[ET.Element]:
        root = self.hierarchy()
        return [
            item
            for item in root.iter("node")
            if text in item.get("text", "")
        ]

    @staticmethod
    def _bounds(element: ET.Element) -> tuple[int, int, int, int]:
        match = re.fullmatch(r"\[(\d+),(\d+)\]\[(\d+),(\d+)\]", element.get("bounds", ""))
        if not match:
            raise RuntimeError("UI element has no valid screen bounds.")
        return tuple(map(int, match.groups()))  # type: ignore[return-value]

    def tap(self, element: ET.Element) -> None:
        left, top, right, bottom = self._bounds(element)
        self._shell("input", "tap", str((left + right) // 2), str((top + bottom) // 2))

    def input_text(self, text: str, element: ET.Element | None = None) -> None:
        if element is not None:
            self.tap(element)
        safe_text = text.replace(" ", "%s")
        self._shell("input", "text", safe_text)

    def key(self, keycode: str) -> None:
        self._shell("input", "keyevent", keycode)

    def swipe(self, direction: str) -> None:
        root = self.hierarchy()
        nodes = [node for node in root.iter("node") if node.get("bounds")]
        if not nodes:
            raise RuntimeError("Device UI hierarchy does not expose screen bounds.")
        left, top, right, bottom = self._bounds(nodes[0])
        center_x = (left + right) // 2
        center_y = (top + bottom) // 2
        delta_x = max(1, (right - left) // 3)
        delta_y = max(1, (bottom - top) // 3)
        vectors = {
            "up": (center_x, center_y + delta_y, center_x, center_y - delta_y),
            "down": (center_x, center_y - delta_y, center_x, center_y + delta_y),
            "left": (center_x + delta_x, center_y, center_x - delta_x, center_y),
            "right": (center_x - delta_x, center_y, center_x + delta_x, center_y),
        }
        try:
            start_x, start_y, end_x, end_y = vectors[direction.strip().casefold()]
        except KeyError as error:
            raise ValueError("Swipe direction must be up, down, left, or right.") from error
        self._shell(
            "input", "swipe", str(start_x), str(start_y), str(end_x), str(end_y), "300"
        )

    def screenshot(self) -> bytes:
        return self._invoke("exec-out", "screencap", "-p", timeout=60)

    def disconnect(self) -> None:
        if self._serial and ":" in self._serial:
            self._invoke("disconnect", self._serial)


class AndroidLibrary:
    """ADB-backed subset of the legacy Android keyword library."""

    def __init__(
        self,
        variables: dict[str, str],
        device: DeviceUnderTest | None,
        output_folder: str | Path,
        *,
        backend_factory: Callable[[], Any] | None = None,
        sleep: Callable[[float], None] = time.sleep,
    ) -> None:
        self._variables = variables
        self._device = device
        self._output_folder = Path(output_folder)
        self._backend_factory = backend_factory or (lambda: _AdbBackend(device))
        self._sleep = sleep
        self._backend: Any | None = None
        self._timeout_scale = 1
        self._timeout = 5

    def set_timeout_scale(self, scale_factor: str) -> KeywordResult:
        try:
            scale = int(scale_factor.strip())
        except ValueError:
            return KeywordResult(Status.ERROR, "Scale factor must be a positive number.")
        if scale < 1:
            return KeywordResult(Status.ERROR, "Scale factor must be a positive number.")
        self._timeout_scale = scale
        return KeywordResult(Status.PASS, f"Scale factor is set to {scale}")

    def set_timeout(self, timeout: str) -> KeywordResult:
        try:
            seconds = float(timeout.strip())
        except ValueError:
            return KeywordResult(Status.FAIL, "Timeout must be a number.")
        if seconds < 0:
            return KeywordResult(Status.FAIL, "Timeout cannot be negative.")
        self._timeout = seconds * self._timeout_scale
        return KeywordResult(Status.PASS)

    def launch(self, activity_name: str) -> KeywordResult:
        return self._action("launch", activity_name)

    def touch_text(self, text: str) -> KeywordResult:
        return self._touch(text, "text")

    def touch_id(self, resource_id: str) -> KeywordResult:
        return self._touch(resource_id, "id")

    def touch_xpath(self, xpath: str) -> KeywordResult:
        return self._touch(xpath, "xpath")

    def input_text(self, *arguments: str) -> KeywordResult:
        if len(arguments) == 1:
            selector, text, by = None, arguments[0], None
        elif len(arguments) == 2:
            selector, text, by = arguments[0], arguments[1], "id"
        else:
            return KeywordResult(Status.ERROR, "InputText expects text or resource ID and text.")
        try:
            backend = self._get_backend()
            element = self._wait_for(selector, by) if selector is not None else None
            backend.input_text(text, element)
        except (RuntimeError, ValueError, TimeoutError, OSError) as error:
            return KeywordResult(Status.FAIL, f"Could not input Android text: {error}")
        except Exception as error:
            return KeywordResult(Status.ERROR, f"Could not input Android text: {error}")
        return KeywordResult(Status.PASS)

    def check_screen_contains_full_text(self, text: str) -> KeywordResult:
        return self._check_text(text, partial=False, expected=True)

    def check_screen_contains_partial_text(self, text: str) -> KeywordResult:
        return self._check_text(text, partial=True, expected=True)

    def check_screen_not_contains_full_text(self, text: str) -> KeywordResult:
        return self._check_text(text, partial=False, expected=False)

    def check_screen_not_contains_partial_text(self, text: str) -> KeywordResult:
        return self._check_text(text, partial=True, expected=False)

    def get_text(self, resource_id: str, save_to: str) -> KeywordResult:
        return self._get_text(resource_id, save_to, "id")

    def get_text_with_xpath(self, xpath: str, save_to: str) -> KeywordResult:
        return self._get_text(xpath, save_to, "xpath")

    def is_enabled(self, resource_id: str) -> KeywordResult:
        return self._check_attribute(resource_id, "id", "enabled", True)

    def is_text_enabled(self, text: str) -> KeywordResult:
        return self._check_attribute(text, "text", "enabled", True)

    def is_selected(self, resource_id: str) -> KeywordResult:
        return self._check_attribute(resource_id, "id", "selected", True)

    def is_unselected(self, resource_id: str) -> KeywordResult:
        return self._check_attribute(resource_id, "id", "selected", False)

    def is_checked(self, resource_id: str) -> KeywordResult:
        return self._check_attribute(resource_id, "id", "checked", True)

    def is_unchecked(self, resource_id: str) -> KeywordResult:
        return self._check_attribute(resource_id, "id", "checked", False)

    def swipe(self, direction: str) -> KeywordResult:
        return self._action("swipe", direction)

    def press_back_key(self) -> KeywordResult:
        return self._action("key", "BACK")

    def press_app_switch_key(self) -> KeywordResult:
        return self._action("key", "APP_SWITCH")

    def press_enter_key(self) -> KeywordResult:
        return self._action("key", "ENTER")

    def press_home_key(self) -> KeywordResult:
        return self._action("key", "HOME")

    def press_tab_key(self) -> KeywordResult:
        return self._action("key", "TAB")

    def hide_keyboard(self) -> KeywordResult:
        return self.press_back_key()

    def wait_for_object(self, resource_id: str, timeout: str) -> KeywordResult:
        return self._wait_keyword(resource_id, "id", timeout)

    def wait_for_text(self, text: str, timeout: str) -> KeywordResult:
        return self._wait_keyword(text, "text", timeout)

    def wait_for_xpath(self, xpath: str, timeout: str) -> KeywordResult:
        return self._wait_keyword(xpath, "xpath", timeout)

    def wait_for_object_gone(self, resource_id: str, timeout: str) -> KeywordResult:
        return self._wait_gone(resource_id, "id", timeout)

    def wait_for_text_gone(self, text: str, timeout: str) -> KeywordResult:
        return self._wait_gone(text, "text", timeout)

    def capture_screen_shot(self, filename: str) -> KeywordResult:
        if not filename.strip():
            return KeywordResult(Status.FAIL, "Screenshot filename cannot be empty.")
        try:
            destination = (self._output_folder / filename).resolve()
            destination.relative_to(self._output_folder.resolve())
            destination.parent.mkdir(parents=True, exist_ok=True)
            destination.write_bytes(self._get_backend().screenshot())
        except ValueError:
            return KeywordResult(Status.FAIL, "Screenshot path must stay inside the output folder.")
        except (RuntimeError, OSError, TimeoutError) as error:
            return KeywordResult(Status.FAIL, f"Could not capture Android screenshot: {error}")
        except Exception as error:
            return KeywordResult(Status.ERROR, f"Could not capture Android screenshot: {error}")
        return KeywordResult(Status.PASS, str(destination))

    def get_ui_dump(self, save_to: str = "${ANDROID_UI_DUMP}") -> KeywordResult:
        try:
            root = self._get_backend().hierarchy()
            dump = ET.tostring(root, encoding="unicode")
            self._variables[self._variable_key(save_to)] = dump
        except (RuntimeError, OSError, TimeoutError) as error:
            return KeywordResult(Status.FAIL, f"Could not read Android UI hierarchy: {error}")
        except Exception as error:
            return KeywordResult(Status.ERROR, f"Could not read Android UI hierarchy: {error}")
        return KeywordResult(Status.PASS, dump)

    def reconnect(self) -> KeywordResult:
        self._backend = None
        return self._action("ping")

    def disconnect(self) -> KeywordResult:
        if self._backend is None:
            return KeywordResult(Status.PASS)
        return self._action("disconnect")

    def _get_text(self, selector: str, save_to: str, by: str) -> KeywordResult:
        try:
            element = self._wait_for(selector, by)
            value = element.get("text", "")
            self._variables[self._variable_key(save_to)] = value
        except (RuntimeError, ValueError, TimeoutError, OSError) as error:
            return KeywordResult(Status.FAIL, f"Could not read Android text: {error}")
        except Exception as error:
            return KeywordResult(Status.ERROR, f"Could not read Android text: {error}")
        return KeywordResult(Status.PASS, value)

    def _check_attribute(self, selector: str, by: str, attribute: str, expected: bool) -> KeywordResult:
        try:
            element = self._wait_for(selector, by)
            actual = element.get(attribute, "false").casefold() == "true"
        except (RuntimeError, ValueError, TimeoutError, OSError) as error:
            return KeywordResult(Status.FAIL, f"Could not inspect Android control: {error}")
        except Exception as error:
            return KeywordResult(Status.ERROR, f"Could not inspect Android control: {error}")
        return KeywordResult(Status.PASS if actual is expected else Status.FAIL)

    def _check_text(self, text: str, *, partial: bool, expected: bool) -> KeywordResult:
        try:
            backend = self._get_backend()
            elements = (
                backend.find_text_contains(text)
                if partial
                else backend.find(text, "text")
            )
        except (RuntimeError, ValueError, TimeoutError, OSError) as error:
            return KeywordResult(Status.FAIL, f"Could not inspect Android screen: {error}")
        except Exception as error:
            return KeywordResult(Status.ERROR, f"Could not inspect Android screen: {error}")
        found = bool(elements)
        success = found is expected
        return KeywordResult(Status.PASS if success else Status.FAIL)

    def _touch(self, selector: str, by: str) -> KeywordResult:
        try:
            element = self._wait_for(selector, by)
            self._get_backend().tap(element)
        except (RuntimeError, ValueError, TimeoutError, OSError) as error:
            return KeywordResult(Status.FAIL, f"Could not touch Android control: {error}")
        except Exception as error:
            return KeywordResult(Status.ERROR, f"Could not touch Android control: {error}")
        return KeywordResult(Status.PASS)

    def _wait_keyword(self, selector: str, by: str, timeout: str) -> KeywordResult:
        try:
            self._wait_for(selector, by, self._timeout_seconds(timeout))
        except (RuntimeError, ValueError, TimeoutError, OSError) as error:
            return KeywordResult(Status.FAIL, f"Android control did not appear: {error}")
        except Exception as error:
            return KeywordResult(Status.ERROR, f"Could not wait for Android control: {error}")
        return KeywordResult(Status.PASS)

    def _wait_gone(self, selector: str, by: str, timeout: str) -> KeywordResult:
        try:
            deadline = time.monotonic() + self._timeout_seconds(timeout)
            while True:
                if not self._get_backend().find(selector, by):
                    return KeywordResult(Status.PASS)
                if time.monotonic() >= deadline:
                    return KeywordResult(Status.FAIL, "Android control did not disappear before timeout.")
                self._sleep(min(0.25, max(0, deadline - time.monotonic())))
        except (RuntimeError, ValueError, TimeoutError, OSError) as error:
            return KeywordResult(Status.FAIL, f"Could not wait for Android control to disappear: {error}")
        except Exception as error:
            return KeywordResult(Status.ERROR, f"Could not wait for Android control to disappear: {error}")

    def _wait_for(self, selector: str, by: str, timeout: float | None = None) -> Any:
        seconds = self._timeout if timeout is None else timeout
        deadline = time.monotonic() + seconds
        while True:
            elements = self._get_backend().find(selector, by)
            if elements:
                return elements[0]
            if time.monotonic() >= deadline:
                raise TimeoutError(f"Could not find {by} '{selector}' within {seconds:g} seconds.")
            self._sleep(min(0.25, max(0, deadline - time.monotonic())))

    def _action(self, action: str, *arguments: Any) -> KeywordResult:
        try:
            getattr(self._get_backend(), action)(*arguments)
        except (RuntimeError, ValueError, TimeoutError, OSError) as error:
            return KeywordResult(Status.FAIL, f"Android {action} failed: {error}")
        except Exception as error:
            return KeywordResult(Status.ERROR, f"Android {action} failed: {error}")
        return KeywordResult(Status.PASS)

    def _get_backend(self) -> Any:
        if self._device is None:
            raise RuntimeError("Android automation requires a configured DeviceUnderTest.")
        if self._backend is None:
            self._backend = self._backend_factory()
        return self._backend

    def _timeout_seconds(self, value: str) -> float:
        try:
            seconds = float(value.strip()) * self._timeout_scale
        except ValueError as error:
            raise ValueError("Timeout must be a number.") from error
        if seconds < 0:
            raise ValueError("Timeout cannot be negative.")
        return seconds

    @staticmethod
    def _variable_key(name: str) -> str:
        name = name.strip()
        return name if name.startswith("${") and name.endswith("}") else f"${{{name.strip('${}')}}}"

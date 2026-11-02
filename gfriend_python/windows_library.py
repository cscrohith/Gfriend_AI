from __future__ import annotations

import os
import platform
import re
import subprocess
import time
from collections.abc import Callable
from typing import Any

from .models import KeywordResult, Status


class _PywinautoBackend:
    def __init__(self) -> None:
        if platform.system() != "Windows":
            raise RuntimeError("Windows UI automation is only available on Windows.")
        try:
            from pywinauto import Desktop
            from pywinauto.application import Application
        except ImportError as error:
            raise RuntimeError(
                "Install the optional 'windows' dependencies to use Windows UI automation."
            ) from error
        self._desktop_type = Desktop
        self._application_type = Application
        self._application: Any = None
        self._window: Any = None

    def open(self, executable: str, wait_seconds: float) -> None:
        self._application = self._application_type(backend="uia").start(executable)
        self._window = self._application.top_window()
        self._window.wait("exists enabled visible", timeout=wait_seconds)
        self._window.set_focus()

    def select(self, title: str, wait_seconds: float) -> None:
        self._window = self._desktop_type(backend="uia").window(
            title_re=f".*{re.escape(title)}.*"
        )
        self._window.wait("exists enabled visible", timeout=wait_seconds)
        self._window.set_focus()
        self._application = None

    def select_child(self, title: str, wait_seconds: float) -> None:
        if self._window is None:
            raise RuntimeError("Select an application before accessing child windows.")
        child = self._window.child_window(title=title, control_type="Window")
        child.wait("exists enabled visible", timeout=wait_seconds)
        child.set_focus()
        self._window = child

    def find(self, selector: str, wait_seconds: float, by_name: bool = False) -> Any:
        if self._window is None:
            raise RuntimeError("Select an application before accessing controls.")
        criteria = {"title": selector} if by_name else {"auto_id": selector}
        control = self._window.child_window(**criteria)
        control.wait("exists", timeout=wait_seconds)
        return control

    def send_keys(self, keys: str) -> None:
        if self._window is None:
            raise RuntimeError("Select an application before sending keys.")
        self._window.type_keys(keys, with_spaces=True)

    def maximize(self) -> None:
        if self._window is None:
            raise RuntimeError("Select an application before maximizing it.")
        self._window.maximize()

    def move_window(self, x: float, y: float) -> None:
        if self._window is None:
            raise RuntimeError("Select an application before moving its window.")
        rect = self._window.rectangle()
        self._window.move_window(
            x=rect.left if x < 0 else x,
            y=rect.top if y < 0 else y,
            width=rect.width(),
            height=rect.height(),
        )

    def select_combo_item(self, automation_id: str, item_name: str) -> None:
        combo = self.find(automation_id, 5)
        combo.select(item_name)

    def set_toggle(self, selector: str, enabled: bool, by_name: bool = False) -> None:
        control = self.find(selector, 5, by_name)
        state = bool(control.get_toggle_state())
        if state != enabled:
            control.toggle()

    def check_toggle(self, selector: str, enabled: bool, by_name: bool = False) -> bool:
        control = self.find(selector, 5, by_name)
        return bool(control.get_toggle_state()) == enabled

    def set_radio(self, selector: str, enabled: bool, by_name: bool = False) -> None:
        control = self.find(selector, 5, by_name)
        if enabled:
            control.select()
        elif control.is_selected():
            return

    def wait_disappear(self, automation_id: str, wait_seconds: float) -> None:
        started = time.monotonic()
        control = self.find(automation_id, wait_seconds)
        remaining = max(0, wait_seconds - (time.monotonic() - started))
        control.wait_not("visible", timeout=remaining)

    def close(self) -> None:
        if self._window is not None:
            try:
                self._window.close()
            finally:
                self._window = None
                self._application = None


class WindowsLibrary:
    """Portable process and Windows UI Automation keywords."""

    def __init__(
        self,
        variables: dict[str, str],
        *,
        backend_factory: Callable[[], Any] | None = None,
        sleep: Callable[[float], None] = time.sleep,
    ) -> None:
        self._variables = variables
        self._backend_factory = backend_factory or _PywinautoBackend
        self._sleep = sleep
        self._backend: Any | None = None
        self._timeout_scale = 1
        self._processes: list[subprocess.Popen[bytes]] = []

    def get_environment_variable(self, environment_variable: str, save_to: str) -> KeywordResult:
        name = environment_variable.strip()
        if not name:
            return KeywordResult(Status.FAIL, "Environment variable name cannot be empty.")
        if name not in os.environ:
            return KeywordResult(Status.FAIL, f"Environment variable '{name}' is not defined.")
        if not save_to.strip():
            return KeywordResult(Status.FAIL, "A variable name must be provided to save the value.")
        value = os.environ[name]
        self._variables[self._variable_key(save_to)] = value
        return KeywordResult(Status.PASS, f"{save_to} = {value}")

    def get_enviornment_variable(self, environment_variable: str, save_to: str) -> KeywordResult:
        return self.get_environment_variable(environment_variable, save_to)

    def set_timeout_scale(self, scale_factor: str) -> KeywordResult:
        try:
            scale = int(scale_factor.strip())
        except ValueError:
            return KeywordResult(
                Status.ERROR,
                "Scale factor must be a number and should be larger than 0",
            )
        if scale < 1:
            return KeywordResult(
                Status.ERROR,
                "Scale factor must be a number and should be larger than 0",
            )
        self._timeout_scale = scale
        return KeywordResult(Status.PASS, f"Scale factor is set to {scale}")

    def open_with_path(self, executable_path: str, wait_time: str = "0") -> KeywordResult:
        if not executable_path.strip():
            return KeywordResult(Status.ERROR, "Executable path cannot be empty.")
        try:
            delay = float(wait_time)
            if delay < 0:
                return KeywordResult(Status.ERROR, "Wait time must not be negative.")
            process = subprocess.Popen([executable_path], shell=False)
            self._processes.append(process)
            if delay:
                self._sleep(delay)
        except (OSError, ValueError) as error:
            return KeywordResult(Status.ERROR, f"Could not open '{executable_path}': {error}")
        return KeywordResult(Status.PASS, f"Opened {executable_path}")

    def select_application(self, window_title: str, timeout: str = "5") -> KeywordResult:
        if not window_title.strip():
            return KeywordResult(Status.ERROR, "Window title cannot be empty.")
        backend, error = self._get_backend()
        if error is not None:
            return error
        try:
            backend.select(window_title, self._scaled_timeout(timeout))
        except (RuntimeError, ValueError, TimeoutError, OSError) as exception:
            return KeywordResult(Status.FAIL, f"Can not find target application: {exception}")
        except Exception as exception:
            return KeywordResult(Status.ERROR, f"Could not select application: {exception}")
        return KeywordResult(Status.PASS)

    def select_child_window_with_title(
        self,
        window_title: str,
        timeout: str = "5",
    ) -> KeywordResult:
        if not window_title.strip():
            return KeywordResult(Status.ERROR, "Child window title cannot be empty.")
        backend, error = self._get_backend()
        if error is not None:
            return error
        try:
            backend.select_child(window_title, self._scaled_timeout(timeout))
        except (RuntimeError, ValueError, TimeoutError, OSError) as exception:
            return KeywordResult(Status.FAIL, f"Can not find target child window: {exception}")
        except Exception as exception:
            return KeywordResult(Status.ERROR, f"Could not select child window: {exception}")
        return KeywordResult(Status.PASS)

    def wait_for_id(self, automation_id: str, timeout_in_sec: str) -> KeywordResult:
        _, error = self._find_control(automation_id, timeout_in_sec)
        return error or KeywordResult(Status.PASS)

    def wait_for_wpath(self, automation_path: str, timeout: str) -> KeywordResult:
        return self.wait_for_id(automation_path, timeout)

    def wait_for_name(self, name: str, timeout_in_sec: str) -> KeywordResult:
        _, error = self._find_control(name, timeout_in_sec, by_name=True)
        return error or KeywordResult(Status.PASS)

    def wait_for_id_disappear(self, automation_id: str, timeout: str) -> KeywordResult:
        backend, error = self._get_backend()
        if error is not None:
            return error
        try:
            backend.wait_disappear(
                automation_id,
                self._scaled_timeout(timeout),
            )
        except (RuntimeError, ValueError, TimeoutError, OSError) as exception:
            return KeywordResult(Status.FAIL, f"UI control did not disappear: {exception}")
        except Exception as exception:
            return KeywordResult(Status.ERROR, f"Could not wait for UI control: {exception}")
        return KeywordResult(Status.PASS)

    def is_exist(self, automation_id: str) -> KeywordResult:
        control, error = self._find_control(automation_id, "0")
        if error is not None:
            if error.status == Status.FAIL:
                return KeywordResult(Status.FAIL, error.output)
            return error
        return KeywordResult(Status.PASS if control is not None else Status.FAIL)

    def is_clickable(self, automation_id: str) -> KeywordResult:
        control, error = self._find_control(automation_id, "0")
        if error is not None:
            return error
        try:
            clickable = control.is_enabled() and control.is_visible()
        except Exception as exception:
            return KeywordResult(Status.ERROR, f"Could not inspect UI control: {exception}")
        return KeywordResult(Status.PASS if clickable else Status.FAIL)

    def get_text(self, automation_id: str, save_to: str) -> KeywordResult:
        if not save_to.strip():
            return KeywordResult(Status.FAIL, "A variable name must be provided to save the text.")
        control, error = self._find_control(automation_id, "5")
        if error is not None:
            return error
        try:
            text = control.window_text()
        except Exception as exception:
            return KeywordResult(Status.ERROR, f"Could not read UI control text: {exception}")
        self._variables[self._variable_key(save_to)] = text
        return KeywordResult(Status.PASS, text)

    def get_text_with_name(self, name: str, save_to: str) -> KeywordResult:
        if not save_to.strip():
            return KeywordResult(Status.FAIL, "A variable name must be provided to save the text.")
        control, error = self._find_control(name, "5", by_name=True)
        if error is not None:
            return error
        try:
            text = control.window_text()
        except Exception as exception:
            return KeywordResult(Status.ERROR, f"Could not read UI control text: {exception}")
        self._variables[self._variable_key(save_to)] = text
        return KeywordResult(Status.PASS, text)

    def get_text_with_wpath(self, automation_path: str, save_to: str) -> KeywordResult:
        return self.get_text(automation_path, save_to)

    def click_id(self, automation_id: str) -> KeywordResult:
        control, error = self._find_control(automation_id, "5")
        if error is not None:
            return error
        try:
            control.click_input()
        except Exception as exception:
            return KeywordResult(Status.ERROR, f"Could not click UI control: {exception}")
        return KeywordResult(Status.PASS)

    def click_wpath(self, automation_path: str) -> KeywordResult:
        return self.click_id(automation_path)

    def click_name(self, name: str) -> KeywordResult:
        control, error = self._find_control(name, "5", by_name=True)
        if error is not None:
            return error
        try:
            control.click_input()
        except Exception as exception:
            return KeywordResult(Status.ERROR, f"Could not click UI control: {exception}")
        return KeywordResult(Status.PASS)

    def set_text(self, automation_id: str, text_to_set: str) -> KeywordResult:
        control, error = self._find_control(automation_id, "5")
        if error is not None:
            return error
        try:
            control.set_edit_text(text_to_set)
        except Exception as exception:
            return KeywordResult(Status.ERROR, f"Could not set UI control text: {exception}")
        return KeywordResult(Status.PASS)

    def set_text_with_name(self, name: str, text_to_set: str) -> KeywordResult:
        control, error = self._find_control(name, "5", by_name=True)
        if error is not None:
            return error
        try:
            control.set_edit_text(text_to_set)
        except Exception as exception:
            return KeywordResult(Status.ERROR, f"Could not set UI control text: {exception}")
        return KeywordResult(Status.PASS)

    def send_key(self, key: str) -> KeywordResult:
        return self._send_keys(key)

    def send_special_key(self, key: str) -> KeywordResult:
        special_keys = {
            "ENTER": "{ENTER}",
            "TAB": "{TAB}",
            "ESC": "{ESC}",
            "ESCAPE": "{ESC}",
            "SPACE": "{SPACE}",
            "BACKSPACE": "{BACKSPACE}",
            "DELETE": "{DELETE}",
            "UP": "{UP}",
            "DOWN": "{DOWN}",
            "LEFT": "{LEFT}",
            "RIGHT": "{RIGHT}",
        }
        normalized = key.strip().upper()
        if normalized not in special_keys:
            return KeywordResult(Status.ERROR, f"Unsupported special key: {key}")
        return self._send_keys(special_keys[normalized])

    def maximize_window(self) -> KeywordResult:
        backend, error = self._get_backend()
        if error is not None:
            return error
        try:
            backend.maximize()
        except Exception as exception:
            return KeywordResult(Status.ERROR, f"Could not maximize window: {exception}")
        return KeywordResult(Status.PASS)

    def move_window(self, x: str, y: str) -> KeywordResult:
        try:
            position_x = float(x.strip())
            position_y = float(y.strip())
        except ValueError:
            return KeywordResult(Status.ERROR, "PosX and PosY should be number")
        backend, error = self._get_backend()
        if error is not None:
            return error
        try:
            backend.move_window(position_x, position_y)
        except (RuntimeError, ValueError, TimeoutError, OSError) as exception:
            return KeywordResult(Status.FAIL, f"Window cannot be moved: {exception}")
        except Exception as exception:
            return KeywordResult(Status.ERROR, f"Error during move window: {exception}")
        return KeywordResult(Status.PASS)

    def set_combo_box_with_name(self, automation_id: str, value_name: str) -> KeywordResult:
        return self._backend_action(
            "select_combo_item",
            automation_id,
            value_name,
            failure=f"Could not select '{value_name}' in combo box '{automation_id}'",
        )

    def set_combo_box(self, automation_id: str, value_name: str) -> KeywordResult:
        return self.set_combo_box_with_name(automation_id, value_name)

    def set_toggle(self, automation_id: str, toggle_on_off: str) -> KeywordResult:
        enabled, error = self._parse_on_off(toggle_on_off, "toggle")
        if error is not None:
            return error
        return self._backend_action(
            "set_toggle",
            automation_id,
            enabled,
            failure=f"Could not set toggle '{automation_id}'",
        )

    def check_toggle_state(self, automation_id: str, toggle_on_off: str) -> KeywordResult:
        enabled, error = self._parse_on_off(toggle_on_off, "toggle")
        if error is not None:
            return error
        try:
            backend, backend_error = self._get_backend()
            if backend_error is not None:
                return backend_error
            matches = backend.check_toggle(automation_id, enabled)
        except Exception as exception:
            return KeywordResult(Status.ERROR, f"Could not inspect toggle '{automation_id}': {exception}")
        return KeywordResult(
            Status.PASS if matches else Status.FAIL,
            "" if matches else "Current toggle state is not matching with given toggle state",
        )

    def set_radio_button_with_id(self, automation_id: str, radio_on_off: str) -> KeywordResult:
        enabled, error = self._parse_on_off(radio_on_off, "radio button")
        if error is not None:
            return error
        return self._backend_action(
            "set_radio",
            automation_id,
            enabled,
            failure=f"Could not set radio button '{automation_id}'",
        )

    def set_radio_button_with_name(self, name: str, radio_on_off: str) -> KeywordResult:
        enabled, error = self._parse_on_off(radio_on_off, "radio button")
        if error is not None:
            return error
        return self._backend_action(
            "set_radio",
            name,
            enabled,
            True,
            failure=f"Could not set radio button '{name}'",
        )

    def close_application(self, window_title: str = "") -> KeywordResult:
        if self._backend is None:
            return KeywordResult(Status.PASS)
        try:
            if window_title.strip():
                self._backend.select(window_title, self._scaled_timeout("5"))
            self._backend.close()
        except Exception as exception:
            return KeywordResult(Status.ERROR, f"Could not close application: {exception}")
        return KeywordResult(Status.PASS)

    def _get_backend(self) -> tuple[Any | None, KeywordResult | None]:
        if self._backend is not None:
            return self._backend, None
        try:
            self._backend = self._backend_factory()
        except (ImportError, RuntimeError) as error:
            return None, KeywordResult(Status.ERROR, str(error))
        return self._backend, None

    def _find_control(
        self,
        automation_id: str,
        timeout: str,
        *,
        by_name: bool = False,
    ) -> tuple[Any | None, KeywordResult | None]:
        if not automation_id.strip():
            return None, KeywordResult(Status.ERROR, "Automation ID cannot be empty.")
        backend, error = self._get_backend()
        if error is not None:
            return None, error
        if not getattr(backend, "has_window", True):
            return None, KeywordResult(Status.ERROR, "Select an application before accessing controls.")
        try:
            control = backend.find(
                automation_id,
                self._scaled_timeout(timeout),
                by_name,
            )
            return control, None
        except (RuntimeError, ValueError, TimeoutError, OSError) as exception:
            return None, KeywordResult(Status.FAIL, f"Could not find UI control '{automation_id}': {exception}")
        except Exception as exception:
            return None, KeywordResult(Status.ERROR, f"Could not access UI control '{automation_id}': {exception}")

    def _send_keys(self, keys: str) -> KeywordResult:
        backend, error = self._get_backend()
        if error is not None:
            return error
        try:
            backend.send_keys(keys)
        except Exception as exception:
            return KeywordResult(Status.ERROR, f"Could not send keyboard input: {exception}")
        return KeywordResult(Status.PASS)

    def _backend_action(
        self,
        action: str,
        *arguments: Any,
        failure: str,
    ) -> KeywordResult:
        backend, error = self._get_backend()
        if error is not None:
            return error
        try:
            getattr(backend, action)(*arguments)
        except (RuntimeError, ValueError, TimeoutError, OSError, LookupError) as exception:
            return KeywordResult(Status.FAIL, f"{failure}: {exception}")
        except Exception as exception:
            return KeywordResult(Status.ERROR, f"{failure}: {exception}")
        return KeywordResult(Status.PASS)

    @staticmethod
    def _parse_on_off(value: str, label: str) -> tuple[bool, KeywordResult | None]:
        normalized = value.strip().casefold()
        if normalized == "on":
            return True, None
        if normalized == "off":
            return False, None
        return False, KeywordResult(Status.FAIL, f"Option value of {label} must be On or Off")

    def _scaled_timeout(self, timeout: str) -> float:
        seconds = float(timeout.strip())
        if seconds < 0:
            raise ValueError("Timeout cannot be negative.")
        return seconds * self._timeout_scale

    @staticmethod
    def _variable_key(name: str) -> str:
        name = name.strip()
        if name.startswith("${") and name.endswith("}"):
            return name
        return f"${{{name.strip('${}')}}}"

from __future__ import annotations

import csv
from copy import copy
from datetime import date, datetime
from pathlib import Path
from typing import Any

from .models import KeywordResult, Status


class SpreadSheetLibrary:
    """Portable workbook and CSV operations from the legacy SpreadSheet library."""

    def __init__(
        self,
        variables: dict[str, str],
        *,
        script_folder: str = "",
        output_folder: str | Path = ".",
    ) -> None:
        self._variables = variables
        self._script_folder = Path(script_folder or Path.cwd())
        self._output_folder = Path(output_folder)
        self._excel_path: Path | None = None
        self._worksheet_name: str | None = None

    def create_new_spread_sheet(self) -> KeywordResult:
        from datetime import datetime as current_datetime

        path = self._output_folder / f"Spreadsheet_{current_datetime.now():%Y%m%d_%H%M%S}.xlsx"
        return self._create_workbook(path)

    def create_new_spread_sheet_with_path(self, file_path: str) -> KeywordResult:
        return self._create_workbook(self._resolve_path(file_path))

    def set_excel_path(self, path: str) -> KeywordResult:
        if not path.strip():
            return KeywordResult(Status.ERROR, "Provided path is empty.")
        resolved = self._resolve_path(path)
        if not resolved.is_file():
            return KeywordResult(Status.ERROR, f"File does not exist at path: {resolved}")
        if resolved.suffix.casefold() not in {".xlsx", ".xlsm", ".csv"}:
            return KeywordResult(Status.ERROR, "Only .xlsx, .xlsm, and .csv files are supported.")
        self._excel_path = resolved
        self._worksheet_name = None
        return KeywordResult(Status.PASS, f"Excel path set successfully: {resolved}")

    def set_worksheet(self, worksheet_identifier: str) -> KeywordResult:
        path, error = self._require_workbook()
        if error is not None:
            return error
        if path.suffix.casefold() == ".csv":
            return KeywordResult(Status.ERROR, "CSV files do not contain worksheets.")
        try:
            from openpyxl import load_workbook

            workbook = load_workbook(path, read_only=True, data_only=True)
            try:
                sheet = next(
                    (
                        name
                        for name in workbook.sheetnames
                        if name.casefold() == worksheet_identifier.casefold()
                    ),
                    None,
                )
                if sheet is None:
                    try:
                        index = int(worksheet_identifier)
                    except ValueError:
                        index = 0
                    if 1 <= index <= len(workbook.sheetnames):
                        sheet = workbook.sheetnames[index - 1]
                if sheet is None:
                    return KeywordResult(
                        Status.ERROR,
                        f"Worksheet '{worksheet_identifier}' not found in file: {path}",
                    )
                self._worksheet_name = sheet
            finally:
                workbook.close()
        except (OSError, ValueError, ImportError) as error:
            return KeywordResult(Status.ERROR, f"Failed to set worksheet: {error}")
        return KeywordResult(Status.PASS, f"Worksheet set successfully: {self._worksheet_name}")

    def total_row_count(self, variable: str) -> KeywordResult:
        return self._save_count(variable, axis=0)

    def total_column_count(self, variable: str) -> KeywordResult:
        return self._save_count(variable, axis=1)

    def add_row_header(self, *headers: str) -> KeywordResult:
        return self._append_values(headers, row=True, bold=True)

    def add_row_data(self, *values: str) -> KeywordResult:
        return self._append_values(values, row=True)

    def add_column_data(self, *values: str) -> KeywordResult:
        return self._append_values(values, row=False)

    def add_row_data_with_style(self, style: str, *values: str) -> KeywordResult:
        properties = self._parse_properties(style)
        return self._append_values(values, row=True, properties=properties)

    def add_text_to_given_position(
        self,
        text: str,
        row_value: str,
        column_value: str,
    ) -> KeywordResult:
        try:
            row, column = int(row_value), int(column_value)
        except ValueError:
            return KeywordResult(Status.ERROR, "Row and column must be positive integers.")
        return self._write_cell(row, column, text)

    def format_excel_cell(self, row: str, column: str, properties: str) -> KeywordResult:
        try:
            row_number, column_number = int(row), int(column)
        except ValueError:
            return KeywordResult(Status.ERROR, "Row and column must be positive integers.")
        if row_number < 1 or column_number < 1:
            return KeywordResult(Status.ERROR, "Row and column must be greater than zero.")
        parsed = self._parse_properties(properties)
        supported = {"fontstyle", "fontcolor", "backgroundcolor", "border", "value"}
        unsupported = set(parsed) - supported
        if unsupported:
            return KeywordResult(Status.FAIL, f"Unsupported property: {sorted(unsupported)[0]}")
        path, error = self._require_workbook()
        if error is not None:
            return error
        if path.suffix.casefold() == ".csv":
            return KeywordResult(Status.ERROR, "Cell formatting is only supported for Excel files.")
        try:
            from openpyxl import load_workbook

            workbook = load_workbook(path)
            try:
                worksheet = workbook[self._sheet_name(workbook)]
                cell = worksheet.cell(row_number, column_number)
                self._apply_style(cell, parsed)
                workbook.save(path)
            finally:
                workbook.close()
        except (OSError, ValueError, ImportError, KeyError) as error:
            return KeywordResult(Status.ERROR, f"Could not format Excel cell: {error}")
        return KeywordResult(
            Status.PASS,
            f"Cell at row {row_number}, column {column_number} formatted successfully.",
        )

    def read_cell_value_by_index(self, row_no: str, col_no: str, variable: str) -> KeywordResult:
        try:
            row, column = int(row_no), int(col_no)
        except ValueError:
            return KeywordResult(Status.FAIL, "Row and column must be positive integers.")
        value, error = self._read_cell(row, column)
        if error is not None:
            return error
        return self._store(variable, value, f"{variable} = {value}")

    def read_cell_value(self, row_value: str, column_value: str, variable: str) -> KeywordResult:
        rows, error = self._read_all_rows()
        if error is not None:
            return error
        target_row = next(
            (
                index
                for index, row in enumerate(rows, start=1)
                if any(value.strip().casefold() == row_value.strip().casefold() for value in row)
            ),
            None,
        )
        if target_row is None:
            return KeywordResult(Status.FAIL, f"Row value '{row_value}' not found.")
        header_rows, header_error = self._read_all_rows()
        if header_error is not None:
            return header_error
        target_column: int | None = None
        for row in header_rows:
            for index, value in enumerate(row, start=1):
                if value.strip().casefold() == column_value.strip().casefold():
                    target_column = index
                    break
            if target_column is not None:
                break
        if target_column is None:
            return KeywordResult(Status.FAIL, f"Column value '{column_value}' not found.")
        value, cell_error = self._read_cell(target_row, target_column)
        if cell_error is not None:
            return cell_error
        if not value:
            return KeywordResult(
                Status.FAIL,
                f"No data found at the intersection of '{row_value}' and '{column_value}'.",
            )
        return self._store(variable, value, f"{variable} = {value}")

    def read_last_column(self, variable: str) -> KeywordResult:
        rows, error = self._read_all_rows()
        if error is not None:
            return error
        column = max((len(row) for row in rows), default=0)
        if column == 0:
            return KeywordResult(Status.FAIL, "No column with data found.")
        values = [row[column - 1] for row in rows if len(row) >= column]
        data = ", ".join(values)
        return self._store(variable, data, f"{variable} = Column {column} data: {data}")

    def read_last_row(self, variable: str) -> KeywordResult:
        rows, error = self._read_all_rows()
        if error is not None:
            return error
        if not rows:
            return KeywordResult(Status.FAIL, "No data found in the worksheet.")
        row_number = len(rows)
        data = " | ".join(rows[-1])
        return self._store(variable, data, f"{variable} = Row {row_number} data: {data}")

    def read_row_data(self, variable: str, start_row: str, end_row: str) -> KeywordResult:
        try:
            start, end = int(start_row), int(end_row)
        except ValueError:
            return KeywordResult(Status.FAIL, "Row numbers must be positive integers.")
        if start < 1 or end < start:
            return KeywordResult(Status.FAIL, "Invalid row range.")
        rows, error = self._read_all_rows()
        if error is not None:
            return error
        if start > len(rows) or end > len(rows):
            return KeywordResult(Status.FAIL, "Requested row range exceeds the used worksheet rows.")
        output = "\n".join(
            f"Row {index}: {' | '.join(rows[index - 1]) or '(empty)'}"
            for index in range(start, end + 1)
        )
        return self._store(variable, output, f"{variable} = {output}")

    def read_column_data(self, variable: str, start_column: str, end_column: str) -> KeywordResult:
        try:
            start, end = int(start_column), int(end_column)
        except ValueError:
            return KeywordResult(Status.FAIL, "Column numbers must be positive integers.")
        if start < 1 or end < start:
            return KeywordResult(Status.FAIL, "Invalid column range.")
        rows, error = self._read_all_rows()
        if error is not None:
            return error
        last_column = max((len(row) for row in rows), default=0)
        if start > last_column or end > last_column:
            return KeywordResult(Status.FAIL, "Requested column range exceeds the used worksheet columns.")
        output = "\n".join(
            f"Column {column}: "
            + " | ".join(row[column - 1] for row in rows if len(row) >= column)
            for column in range(start, end + 1)
        )
        return self._store(variable, output, f"{variable} = {output}")

    def read_values_by_column_names(self, variable: str, *column_names: str) -> KeywordResult:
        rows, error = self._read_all_rows()
        if error is not None:
            return error
        if not rows or not column_names:
            return KeywordResult(Status.FAIL, "No column names were provided to read.")
        headers = {name.strip().casefold(): index for index, name in enumerate(rows[0])}
        output_lines = []
        for name in column_names:
            index = headers.get(name.strip().casefold())
            if index is None:
                continue
            output_lines.append(f"Column: {name}")
            output_lines.extend(row[index] for row in rows[1:] if len(row) > index)
            output_lines.append("")
        if not output_lines:
            return KeywordResult(Status.FAIL, "None of the specified columns were found.")
        output = "\n".join(output_lines).strip()
        return self._store(variable, output, f"{variable} = {output}")

    def read_values_by_row_names(self, variable: str, *values: str) -> KeywordResult:
        rows, error = self._read_all_rows()
        if error is not None:
            return error
        search_values = {value.strip().casefold() for value in values}
        matches = [
            f"Row {index}: {' | '.join(row)}"
            for index, row in enumerate(rows, start=1)
            if any(value.strip().casefold() in search_values for value in row)
        ]
        if not matches:
            return KeywordResult(Status.FAIL, "No rows found containing the specified values.")
        output = "\n".join(matches)
        return self._store(variable, output, f"{variable} = {output}")

    def validate_status_from_spreadsheet(
        self,
        status_column_name: str,
        identifier_column_name: str,
        expected_status_value: str,
    ) -> KeywordResult:
        if not expected_status_value.strip():
            return KeywordResult(Status.ERROR, "Expected status value cannot be empty.")
        rows, error = self._read_all_rows()
        if error is not None:
            return error
        if not rows:
            return KeywordResult(Status.ERROR, "The spreadsheet is empty or could not be read.")
        headers = {name.strip().casefold(): index for index, name in enumerate(rows[0])}
        status_column = headers.get(status_column_name.strip().casefold())
        identifier_column = headers.get(identifier_column_name.strip().casefold())
        if status_column is None:
            return KeywordResult(Status.ERROR, f"Column '{status_column_name}' not found.")
        if identifier_column is None:
            return KeywordResult(Status.ERROR, f"Column '{identifier_column_name}' not found.")
        expected = expected_status_value.strip().casefold()
        failed_rows = []
        for row in rows[1:]:
            if status_column >= len(row) or identifier_column >= len(row):
                continue
            status, identifier = row[status_column].strip(), row[identifier_column].strip()
            if status.casefold() != expected:
                failed_rows.append(f"{identifier} ({status})")
        if failed_rows:
            details = ", ".join(failed_rows)
            return KeywordResult(
                Status.FAIL,
                f"Some entries did not match expected status '{expected_status_value}': {details}",
            )
        return KeywordResult(
            Status.PASS,
            f"All entries matched expected status '{expected_status_value}'.",
        )

    def _create_workbook(self, path: Path) -> KeywordResult:
        if path.suffix.casefold() not in {".xlsx", ".xlsm"}:
            return KeywordResult(Status.FAIL, "Excel workbook path must end in .xlsx or .xlsm.")
        try:
            from openpyxl import Workbook

            path.parent.mkdir(parents=True, exist_ok=True)
            workbook = Workbook()
            workbook.save(path)
            workbook.close()
        except (OSError, ValueError, ImportError) as error:
            return KeywordResult(Status.ERROR, f"Could not create spreadsheet: {error}")
        self._excel_path = path
        self._worksheet_name = None
        return KeywordResult(Status.PASS, f"Created new excel file {path}")

    def _append_values(
        self,
        values: tuple[str, ...],
        *,
        row: bool,
        bold: bool = False,
        properties: dict[str, str] | None = None,
    ) -> KeywordResult:
        if not values:
            return KeywordResult(Status.FAIL, "At least one cell value is required.")
        path, error = self._require_workbook()
        if error is not None:
            return error
        if path.suffix.casefold() == ".csv":
            try:
                with path.open("r", encoding="utf-8-sig", newline="") as stream:
                    csv_rows = list(csv.reader(stream))
                if row:
                    csv_rows.append(list(values))
                else:
                    width = max((len(current) for current in csv_rows), default=0)
                    for index, value in enumerate(values):
                        while len(csv_rows) <= index:
                            csv_rows.append([])
                        csv_rows[index] += [""] * (width - len(csv_rows[index]))
                        csv_rows[index].append(value)
                with path.open("w", encoding="utf-8-sig", newline="") as stream:
                    csv.writer(stream).writerows(csv_rows)
            except OSError as write_error:
                return KeywordResult(Status.ERROR, f"Could not append spreadsheet data: {write_error}")
            return KeywordResult(Status.PASS, f"Data appended successfully to {path}")
        try:
            from openpyxl import load_workbook

            workbook = load_workbook(path)
            try:
                worksheet = workbook[self._sheet_name(workbook)]
                if row:
                    used_rows, _ = self._used_dimensions(worksheet)
                    row_number = max(used_rows + 1, 1)
                    for column, value in enumerate(values, start=1):
                        cell = worksheet.cell(row_number, column)
                        self._set_cell_value(cell, value)
                        if bold or properties:
                            self._apply_style(cell, properties or {}, bold=bold)
                else:
                    _, used_columns = self._used_dimensions(worksheet)
                    column_number = max(used_columns + 1, 1)
                    for row_number, value in enumerate(values, start=1):
                        cell = worksheet.cell(row_number, column_number)
                        self._set_cell_value(cell, value)
                        if bold or properties:
                            self._apply_style(cell, properties or {}, bold=bold)
                workbook.save(path)
            finally:
                workbook.close()
        except (OSError, ValueError, ImportError, KeyError) as write_error:
            return KeywordResult(Status.ERROR, f"Could not append spreadsheet data: {write_error}")
        return KeywordResult(Status.PASS, f"Data appended successfully to {path}")

    def _write_cell(self, row: int, column: int, value: str) -> KeywordResult:
        if row < 1 or column < 1:
            return KeywordResult(Status.ERROR, "Row and column must be greater than zero.")
        path, error = self._require_workbook()
        if error is not None:
            return error
        if path.suffix.casefold() == ".csv":
            try:
                with path.open("r", encoding="utf-8-sig", newline="") as stream:
                    rows = list(csv.reader(stream))
                while len(rows) < row:
                    rows.append([])
                while len(rows[row - 1]) < column:
                    rows[row - 1].append("")
                rows[row - 1][column - 1] = value
                with path.open("w", encoding="utf-8-sig", newline="") as stream:
                    csv.writer(stream).writerows(rows)
            except OSError as write_error:
                return KeywordResult(Status.ERROR, f"Could not write spreadsheet cell: {write_error}")
        else:
            try:
                from openpyxl import load_workbook

                workbook = load_workbook(path)
                try:
                    worksheet = workbook[self._sheet_name(workbook)]
                    self._set_cell_value(worksheet.cell(row, column), value)
                    workbook.save(path)
                finally:
                    workbook.close()
            except (OSError, ValueError, ImportError, KeyError) as write_error:
                return KeywordResult(Status.ERROR, f"Could not write spreadsheet cell: {write_error}")
        return KeywordResult(Status.PASS, f"Appended to excel {path}")

    def _read_cell(self, row: int, column: int) -> tuple[str, KeywordResult | None]:
        if row < 1 or column < 1:
            return "", KeywordResult(Status.FAIL, "Row and column must be greater than zero.")
        rows, error = self._read_all_rows()
        if error is not None:
            return "", error
        if row > len(rows) or column > len(rows[row - 1]):
            return "", KeywordResult(Status.FAIL, f"No data found at Row {row}, Column {column}.")
        value = rows[row - 1][column - 1]
        if not value:
            return "", KeywordResult(Status.FAIL, f"No data found at Row {row}, Column {column}.")
        return value, None

    def calculate_formula_from_cells(
        self,
        cell1_row: str,
        cell1_col: str,
        cell2_row: str,
        cell2_col: str,
        operation: str,
        result_variable: str = "",
        result_row: str = "",
        result_col: str = "",
    ) -> KeywordResult:
        try:
            row1, column1, row2, column2 = map(
                int, (cell1_row, cell1_col, cell2_row, cell2_col)
            )
        except ValueError:
            return KeywordResult(Status.FAIL, "Row and column numbers must be positive integers.")
        if min(row1, column1, row2, column2) < 1:
            return KeywordResult(Status.FAIL, "Row and column numbers must be greater than zero.")
        if not result_variable.strip() and not (result_row.strip() and result_col.strip()):
            return KeywordResult(
                Status.FAIL,
                "Either resultVariable or both resultRow and resultCol must be provided.",
            )
        try:
            first = self._read_cell(row1, column1)
            second = self._read_cell(row2, column2)
            if first[1] is not None:
                return first[1]
            if second[1] is not None:
                return second[1]
            value1, value2 = float(first[0]), float(second[0])
            normalized = operation.strip().casefold()
            operations = {
                "add": ("addition", value1 + value2),
                "+": ("addition", value1 + value2),
                "subtract": ("subtraction", value1 - value2),
                "-": ("subtraction", value1 - value2),
                "multiply": ("multiplication", value1 * value2),
                "*": ("multiplication", value1 * value2),
                "/": ("division", value1 / value2),
                "divide": ("division", value1 / value2),
                "%": ("remainder", value1 % value2),
                "remainder": ("remainder", value1 % value2),
                "^": ("power", value1**value2),
                "power": ("power", value1**value2),
            }
            if normalized in {"days", "hours"}:
                date1 = datetime.fromisoformat(first[0])
                date2 = datetime.fromisoformat(second[0])
                delta = date2 - date1
                result = delta.total_seconds() / (3600 if normalized == "hours" else 86400)
                operation_name = f"{normalized} difference"
            else:
                operation_name, result = operations[normalized]
            result_text = str(result)
        except (ValueError, KeyError, ZeroDivisionError, OverflowError) as error:
            return KeywordResult(Status.FAIL, f"Could not calculate formula: {error}")
        if result_variable.strip():
            store_result = self._store(result_variable, result_text, "")
            if store_result.status != Status.PASS:
                return store_result
        if result_row.strip() or result_col.strip():
            if not result_row.strip() or not result_col.strip():
                return KeywordResult(
                    Status.FAIL,
                    "Both resultRow and resultCol must be provided together.",
                )
            try:
                output_row, output_column = int(result_row), int(result_col)
            except ValueError:
                return KeywordResult(Status.FAIL, "Result row and column must be positive integers.")
            write_result = self._write_cell(output_row, output_column, result_text)
            if write_result.status != Status.PASS:
                return write_result
        return KeywordResult(
            Status.PASS,
            f"Calculated {operation_name}: {first[0]} {operation} {second[0]} = {result_text}",
        )

    def _read_all_rows(self) -> tuple[list[list[str]], KeywordResult | None]:
        path, error = self._require_workbook()
        if error is not None:
            return [], error
        try:
            if path.suffix.casefold() == ".csv":
                with path.open("r", encoding="utf-8-sig", newline="") as stream:
                    return [list(row) for row in csv.reader(stream)], None
            from openpyxl import load_workbook

            workbook = load_workbook(path, read_only=True, data_only=True)
            try:
                worksheet = workbook[self._sheet_name(workbook)]
                rows = [
                    [self._cell_text(value) for value in row]
                    for row in worksheet.iter_rows(values_only=True)
                ]
                while rows and not any(rows[-1]):
                    rows.pop()
                return [self._trim_trailing_empty_cells(row) for row in rows], None
            finally:
                workbook.close()
        except (OSError, ValueError, ImportError, KeyError) as read_error:
            return [], KeywordResult(Status.ERROR, f"Could not read spreadsheet: {read_error}")

    def _save_count(self, variable: str, *, axis: int) -> KeywordResult:
        rows, error = self._read_all_rows()
        if error is not None:
            return error
        count = len(rows) if axis == 0 else max((len(row) for row in rows), default=0)
        if count == 0:
            return KeywordResult(Status.FAIL, "No data found in the worksheet.")
        return self._store(variable, str(count), f"{variable} = {count}")

    def _require_workbook(self) -> tuple[Path | None, KeywordResult | None]:
        if self._excel_path is None:
            return None, KeywordResult(
                Status.ERROR,
                "First create or specify an Excel file before performing this operation.",
            )
        if not self._excel_path.is_file():
            return None, KeywordResult(
                Status.ERROR,
                f"File does not exist at path: {self._excel_path}",
            )
        return self._excel_path, None

    def _sheet_name(self, workbook: Any) -> str:
        if self._worksheet_name in workbook.sheetnames:
            return self._worksheet_name
        self._worksheet_name = workbook.active.title
        return self._worksheet_name

    @staticmethod
    def _used_dimensions(worksheet: Any) -> tuple[int, int]:
        last_row = 0
        last_column = 0
        for row_number, row in enumerate(worksheet.iter_rows(values_only=True), start=1):
            used = [index for index, value in enumerate(row, start=1) if value is not None]
            if used:
                last_row = row_number
                last_column = max(last_column, max(used))
        return last_row, last_column

    @staticmethod
    def _trim_trailing_empty_cells(row: list[str]) -> list[str]:
        while row and not row[-1]:
            row.pop()
        return row

    def _resolve_path(self, path: str) -> Path:
        resolved = Path(path)
        if resolved.is_absolute():
            return resolved
        return (self._script_folder / resolved).resolve()

    def _store(self, variable: str, value: str, output: str) -> KeywordResult:
        if not variable.strip():
            return KeywordResult(Status.FAIL, "A variable name must be provided.")
        self._variables[self._variable_key(variable)] = value
        return KeywordResult(Status.PASS, output)

    @staticmethod
    def _variable_key(name: str) -> str:
        name = name.strip()
        if name.startswith("${") and name.endswith("}"):
            return name
        return f"${{{name.strip('${}')}}}"

    @staticmethod
    def _cell_text(value: Any) -> str:
        if value is None:
            return ""
        if isinstance(value, (datetime, date)):
            return value.isoformat(sep=" ") if isinstance(value, datetime) else value.isoformat()
        return str(value).strip()

    @staticmethod
    def _set_cell_value(cell: Any, value: str) -> None:
        try:
            cell.value = int(value)
            return
        except ValueError:
            pass
        try:
            cell.value = float(value)
            return
        except ValueError:
            cell.value = value

    @staticmethod
    def _parse_properties(properties: str) -> dict[str, str]:
        parsed: dict[str, str] = {}
        for item in properties.split(";"):
            key, separator, value = item.partition(":")
            if separator:
                parsed[key.strip().casefold()] = value.strip()
        return parsed

    @staticmethod
    def _excel_color(value: str) -> str:
        named_colors = {
            "black": "FF000000",
            "white": "FFFFFFFF",
            "red": "FFFF0000",
            "green": "FF008000",
            "blue": "FF0000FF",
            "yellow": "FFFFFF00",
            "orange": "FFFFA500",
            "purple": "FF800080",
            "gray": "FF808080",
            "grey": "FF808080",
            "pink": "FFFFC0CB",
            "cyan": "FF00FFFF",
            "magenta": "FFFF00FF",
        }
        color = value.strip()
        if color.casefold() in named_colors:
            return named_colors[color.casefold()]
        color = color.lstrip("#")
        return "FF" + color if len(color) == 6 else color

    @staticmethod
    def _apply_style(cell: Any, properties: dict[str, str], *, bold: bool = False) -> None:
        from openpyxl.styles import Border, Font, PatternFill, Side

        current_font = cell.font
        font_updates: dict[str, Any] = {"bold": bold or current_font.bold}
        style = properties.get("fontstyle", properties.get("font", "")).casefold()
        if style in {"bold", "italic"}:
            font_updates[style] = True
        if "fontcolor" in properties or "forecolor" in properties:
            color = properties.get("fontcolor", properties.get("forecolor", ""))
            font_updates["color"] = SpreadSheetLibrary._excel_color(color)
        if any(key in properties for key in ("fontstyle", "font", "fontcolor", "forecolor")) or bold:
            font = copy(current_font)
            for name, value in font_updates.items():
                setattr(font, name, value)
            cell.font = font

        background = properties.get("backgroundcolor", properties.get("backcolor"))
        if background:
            cell.fill = PatternFill(
                fill_type="solid",
                fgColor=SpreadSheetLibrary._excel_color(background),
            )
        border = properties.get("border")
        if border == "1" or (border and border not in {"0", "false"}):
            side = Side(style="thin", color="FF000000")
            cell.border = Border(left=side, right=side, top=side, bottom=side)
        elif border == "0":
            cell.border = Border()
        if "value" in properties:
            SpreadSheetLibrary._set_cell_value(cell, properties["value"])

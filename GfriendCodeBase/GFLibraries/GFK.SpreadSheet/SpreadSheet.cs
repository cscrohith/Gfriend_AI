using ClosedXML.Excel;
using HP.GFriend.GFLogger;
using HP.GFriend.Keywords;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HP.GFriend.Keywords
{
    [LibraryDescription("<p><br><b>Steps to Use Read Keywords:</b></p>" +
  "<p>1. Use the <b>Set ExcelPath</b> keyword to set the Excel file path.</p>" +
  "<p>2. Use the <b>Set Worksheet</b> keyword to set the worksheet name or index.</p>" +
  "<p>3. Now you can use any Read keywords.</p>" +
  "<p><b>Note:</b></p>" +
  "<p><b>1. The Set ExcelPath keyword must be called before using the Set Worksheet keyword.</b></p>" +
  "<p><b>2. The Set Worksheet keyword should be called after setting the Excel path to specify the target worksheet for reading data. <b></p>"
  )]
    public class SpreadSheet : IGFLibrary
    {
        private static string _outputDir;

        // Hold the Excel path for all operations.
        private string _excelPath = "";
        // Hold the Worksheet Name for all operations.
        private string _worksheetName;

        public void Dispose()
        {

        }

        public bool DutUsed()
        {
            return false;
        }

        public List<string> GetDependencies()
        {
            return null;
        }

        public string GetName()
        {
            return "SpreadSheet";
        }

        public void Initialize(DeviceUnderTest dut, string outputDir)
        {
            _outputDir = outputDir;
        }


        [KeywordDescription("Create new spread sheet in the output folder")]
        [KeywordDisplayName("Create New Spread Sheet")]
        [SampleScript("CreateNewSpreadSheet()")]
        public KeywordResult CreateNewSpreadSheet()
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss"); 
                string fileName = $"Spreadsheet_{timestamp}.xlsx";
                _excelPath = Path.Combine(_outputDir, fileName);
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add();

                    workbook.SaveAs(_excelPath);
                }
                return new KeywordResult(KeywordResults.Pass, $"Created new excel file {_excelPath}");
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, $"An error occurred: {ex.Message}");
                Logger.Error(error.Output, ex);
                return error;
            }

        }
        [KeywordDescription("Create new spread sheet with given filename and path")]
        [KeywordDisplayName("Create New Spread Sheet With Path")]
        [KeywordParameters("FilePath", "The path of the Excel file to be created and saved.")]
        [SampleScript("CreateNewSpreadSheetWithPath(C:\\Users\\YourUsername\\Documents\\Filename.xlsx)")]
        public KeywordResult CreateNewSpreadSheetWithPath(string filePath)
        {
            _excelPath = filePath;
            if (!string.IsNullOrEmpty(_excelPath))
            {
                // regex pattern to validate the file path format
                string filePathPattern = @"^[a-zA-Z]:\\(?:[^\\\/:*?""<>|]+\\)*[^\\\/:*?""<>|]+\.(xlsx|xls)$";
                if (!Regex.IsMatch(_excelPath, filePathPattern))
                {
                    return new KeywordResult(KeywordResults.Fail, "Invalid file path format. Please provide a valid Excel file path.");
                }
                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add();

                        workbook.SaveAs(_excelPath);
                    }
                    return new KeywordResult(KeywordResults.Pass, $"Created new excel file {_excelPath}");
                }
                catch (Exception ex)
                {
                    return new KeywordResult(KeywordResults.Error, $"An error occurred: {ex.Message}");
                }
            }
            else
            {
                return new KeywordResult(KeywordResults.Fail, $"Please enter a valid file path.");
            }
        }

        [KeywordDescription("Add a row header with bold text")]
        [KeywordDisplayName("Add Row Header")]
        [KeywordParameters("rowHeader", "Data to append in the Excel file as row header")]
        [SampleScript("AddRowHeader(a,b,c,d,,,,,")]
        public KeywordResult AddRowHeader(params string[] headers)
        {
            if (string.IsNullOrWhiteSpace(_excelPath))
            {
                return new KeywordResult(KeywordResults.Error, " First Create New Excel and then try to add Header");
            }
            try
            {
                using (var workbook = new XLWorkbook(_excelPath))
                {
                    var worksheet = workbook.Worksheet(1); 
                    int lastRow = FindLastUsedRow(worksheet); 

                    // Append the header values starting from the next row
                    for (int col = 0; col < headers.Length; col++)
                    {
                        var cell = worksheet.Cell(lastRow + 2, col + 1);

                        if (int.TryParse(headers[col], out int intValue))
                        {
                            cell.Value = intValue; 
                        }
                        else if (double.TryParse(headers[col], out double doubleValue))
                        {
                            cell.Value = doubleValue; 
                        }
                        else
                        {
                            cell.Value = headers[col]; 
                        }
                        // Format cell to be bold
                        FormatCell(cell, isBold: true); 
                    }

                    worksheet.Columns().AdjustToContents(); 
                    workbook.Save();

                    return new KeywordResult(KeywordResults.Pass, $"Row Header Appended to {_excelPath}");
                }
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, $"An error occurred: {ex.Message}");
                Logger.Error(error.Output, ex);
                return error;
            }
        }
        /// <summary>
        /// Method to find the last used row in the worksheet
        /// </summary>
        /// <param name="worksheet"></param>
        /// <returns></returns>
        private int FindLastUsedRow(IXLWorksheet worksheet)
        {
            return worksheet.LastRowUsed()?.RowNumber() ?? 1;
        }

        /// <summary>
        /// Method to format a cell
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="isBold"></param>
        private void FormatCell(IXLCell cell, bool isBold = false)
        {
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;  
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center; 
            cell.Style.Font.Bold = isBold; 
        }
        [KeywordDescription("Add a row data")]
        [KeywordDisplayName("Add Row Data")]
        [KeywordParameters("rowData", "Data to append in the excel file as row data")]
        [SampleScript("AddRowData(a,b,c,d,,,,,,)")]
        public KeywordResult AddRowData(params string[] rowData)
        {
            if (string.IsNullOrWhiteSpace(_excelPath))
            {
                return new KeywordResult(KeywordResults.Error, " First Create New Excel and then try to add Row data");
            }
            try
            {
                using (var workbook = new XLWorkbook(_excelPath))
                {
                    var worksheet = workbook.Worksheet(1); 

                    int lastRow = FindLastUsedRow(worksheet);

                    // Loop through each value and add them to consecutive columns in the next available row
                    for (int col = 0; col < rowData.Length; col++)
                    {
                        var cell = worksheet.Cell(lastRow + 1, col + 1);
                        if (int.TryParse(rowData[col], out int intValue))
                        {
                            cell.Value = intValue;
                        }
                        else if (double.TryParse(rowData[col], out double doubleValue))
                        {
                            cell.Value = doubleValue;
                        }
                        else
                        {
                            cell.Value = rowData[col];
                        }

                        FormatCell(cell);
                    }

                    worksheet.Columns().AdjustToContents(); 

                    workbook.Save();
                    return new KeywordResult(KeywordResults.Pass, $"Data appended successfully as Row Data to {_excelPath}");
                }
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, $"An error occurred: {ex.Message}");
                Logger.Error(error.Output, ex);
                return error;
            }
        }
        [KeywordDescription("Add column data")]
        [KeywordDisplayName("Add Column Data")]
        [KeywordParameters("columnData", "Data to append in the excel file as column data")]
        [SampleScript("AddColumnData(a,b,c,d,,,,,,,)")]
        public KeywordResult AddColumnData(params string[] columnData)
        {
            if (string.IsNullOrWhiteSpace(_excelPath))
            {
                return new KeywordResult(KeywordResults.Error, " First Create New Excel and then try to add column data");
            }
            try
            {
                using (var workbook = new XLWorkbook(_excelPath))
                {
                    var worksheet = workbook.Worksheet(1); 

                    int lastUsedColumn = FindLastUsedColumn(worksheet);

                    int startColumn = lastUsedColumn + 2;

                    // Loop through each value and add them to consecutive rows, starting from the specified column
                    for (int row = 0; row < columnData.Length; row++)
                    {
                        // Start adding values from the first row in the defined column
                        var cell = worksheet.Cell(row + 1, startColumn); 

                        if (int.TryParse(columnData[row], out int intValue))
                        {
                            cell.Value = intValue; 
                        }
                        else if (double.TryParse(columnData[row], out double doubleValue))
                        {
                            cell.Value = doubleValue; 
                        }
                        else
                        {
                            cell.Value = columnData[row]; 
                        }

                        FormatCell(cell);
                    }

                    worksheet.Columns().AdjustToContents(); 

                    workbook.Save();
                    return new KeywordResult(KeywordResults.Pass, $"Data appended successfully as Column data to {_excelPath}");
                }
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, $"An error occurred: {ex.Message}");
                Logger.Error(error.Output, ex);
                return error;
            }
        }
        private int FindLastUsedColumn(IXLWorksheet worksheet)
        {
            var lastCell = worksheet.LastCellUsed();
            // If the sheet is empty, start from column 1
            if (lastCell == null)
            {
                return 0; 
            }

            return lastCell.Address.ColumnNumber;
        }
        [KeywordDescription("Add text to given position row and column")]
        [KeywordDisplayName("Add Text To Given Position")]
        [KeywordParameters("text", "Text to add/update in excel")]
        [KeywordParameters("rowvalue", "row value")]
        [KeywordParameters("columnvalue", "column value")]
        [SampleScript("AddTextToGivenPosition(text,10,15)")]
        public KeywordResult AddTextToGivenPosition(string text, string rowvalue, string columnvalue)
        {
            if (string.IsNullOrWhiteSpace(_excelPath))
            {
                return new KeywordResult(KeywordResults.Error, " First Create New Excel and then try to add text");
            }
            try
            {
                using (var workbook = new XLWorkbook(_excelPath))
                {
                    IXLWorksheet worksheet = workbook.Worksheets.Worksheet(1);

                    int row = int.Parse(rowvalue);
                    int col = int.Parse(columnvalue);
                    string displayText = text; 

                    worksheet.Cell(row, col).Value = displayText;

                    workbook.SaveAs(_excelPath);
                }
                return new KeywordResult(KeywordResults.Pass, $"Appended to excel {_excelPath}");
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, $"An error occurred: {ex.Message}");
                Logger.Error(error.Output, ex);
                return error;
            }
        }
        [KeywordDescription("Opens the excel file of given path")]
        [KeywordDisplayName("Open Excel File")]
        [SampleScript("OpenExcelFile()")]
        public KeywordResult OpenExcelFile()
        {
            if (string.IsNullOrWhiteSpace(_excelPath))
            {
                return new KeywordResult(KeywordResults.Error, " First Create New Excel and then try to open");
            }
            try
            {
                using (var workbook = new XLWorkbook(_excelPath))
                {
                    // Optionally, open the Excel file with the default application
                    Process.Start(new ProcessStartInfo(_excelPath) { UseShellExecute = true });
                }
                return new KeywordResult(KeywordResults.Pass, $"Appended to excel {_excelPath}");
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, $"An error occurred: {ex.Message}");
                Logger.Error(error.Output, ex);
                return error;
            }
        }
        [KeywordDescription("Add a row data with optional styling along with style")]
        [KeywordDisplayName("Add Row Data With Style")]
        [KeywordParameters("rowData", "Data to append in the excel file as row data")]
        [KeywordParameters("style", "Optional style string in format 'ForeColor:color;BackColor:color;Font:style;Border:width'")]
        [SampleScript("")]
        public KeywordResult AddRowDataWithStyle(string style, params string[] rowData)
        {
            if (string.IsNullOrWhiteSpace(_excelPath))
            {
                return new KeywordResult(KeywordResults.Error, "First Create New Excel and then try to add Row data");
            }
            try
            {
                using (var workbook = new XLWorkbook(_excelPath))
                {
                    var worksheet = workbook.Worksheet(1); 
                    int lastRow = FindLastUsedRow(worksheet); 

                    var styleParams = ParseStyleString(style);

                    // Loop through each value and add them to consecutive columns in the next available row
                    for (int col = 0; col < rowData.Length; col++)
                    {
                        var cell = worksheet.Cell(lastRow + 1, col + 1);
                        if (int.TryParse(rowData[col], out int intValue))
                        {
                            cell.Value = intValue;
                        }
                        else if (double.TryParse(rowData[col], out double doubleValue))
                        {
                            cell.Value = doubleValue;
                        }
                        else
                        {
                            cell.Value = rowData[col];
                        }

                        ApplyStyle(cell, styleParams);
                    }

                    worksheet.Columns().AdjustToContents();
                    workbook.Save();
                    return new KeywordResult(KeywordResults.Pass, $"Data appended successfully as Row Data to {_excelPath}");
                }
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, $"An error occurred: {ex.Message}");
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        private Dictionary<string, string> ParseStyleString(string style)
        {
            var styleDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(style)) return styleDict;

            var styles = style.Split(';');
            foreach (var item in styles)
            {
                var parts = item.Split(':');
                if (parts.Length == 2)
                {
                    styleDict[parts[0].Trim()] = parts[1].Trim();
                }
            }
            return styleDict;
        }

        public void ApplyStyle(IXLCell cell, Dictionary<string, string> styles)
        {
            foreach (var style in styles)
            {
                switch (style.Key.ToLower())
                {
                    case "forecolor":
                        cell.Style.Font.FontColor = XLColor.FromName(style.Value);
                        break;
                    case "backcolor":
                        cell.Style.Fill.BackgroundColor = XLColor.FromName(style.Value);
                        break;
                    case "font":
                        if (style.Value.ToLower() == "italic")
                            cell.Style.Font.Italic = true;
                        else if (style.Value.ToLower() == "bold")
                            cell.Style.Font.Bold = true;
                        break;
                    case "border":
                        if (int.TryParse(style.Value, out int borderWidth))
                        {
                            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            cell.Style.Border.OutsideBorderColor = XLColor.Black;
                        }
                        break;
                }
            }
        }
        [KeywordDescription("Format cell properties like fontstyle, fontcolor, background color, border and value using colon-separated values")]
        [KeywordDisplayName("Format Excel Cell")]
        [KeywordParameters("row", "Row number of the cell")]
        [KeywordParameters("column", "Column number of the cell")]
        [KeywordParameters("properties", "Colon-separated key value pairs for formatting")]
        [SampleScript("FormatExcelCell(5, 2, \"FontStyle:Bold/Italic;fontcolor:blue;BackgroundColor:Yellow/#eff5ab;border:1/0;Value:Sample Text\")")]
        public KeywordResult FormatExcelCell(string row, string column, string properties)
        {
            if (string.IsNullOrWhiteSpace(_excelPath))
            {
                return new KeywordResult(KeywordResults.Fail, "First create new Excel and then try to format the cell.");
            }

            try
            {
                // Parse colon-separated values into a dictionary
                var propertyDictionary = properties.Split(';')
                    .Select(item => item.Split(':'))
                    .Where(pair => pair.Length == 2)
                    .ToDictionary(pair => pair[0].Trim(), pair => pair[1].Trim());

                using (var workbook = new XLWorkbook(_excelPath))
                {
                    IXLWorksheet worksheet = workbook.Worksheets.Worksheet(1);

                    IXLCell cell = worksheet.Cell(int.Parse(row), int.Parse(column));

                    // Iterate through the properties and apply formatting
                    foreach (var property in propertyDictionary)
                    {
                        switch (property.Key.ToLower())
                        {
                            case "fontstyle":
                                if (property.Value.Equals("Bold", StringComparison.OrdinalIgnoreCase))
                                    cell.Style.Font.Bold = true;
                                else if (property.Value.Equals("Italic", StringComparison.OrdinalIgnoreCase))
                                    cell.Style.Font.Italic = true;
                                break;

                            case "backgroundcolor":
                                // Check if it's a hex color
                                if (property.Value.StartsWith("#")) 
                                {
                                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml(property.Value);
                                }
                                else 
                                {
                                    cell.Style.Fill.BackgroundColor = XLColor.FromName(property.Value.ToLower());
                                }
                                break;

                            case "fontcolor":
                                if (property.Value.StartsWith("#")) // Check if it's a hex color
                                {
                                    cell.Style.Font.FontColor = XLColor.FromHtml(property.Value);
                                }
                                else 
                                {
                                    cell.Style.Font.FontColor = XLColor.FromName(property.Value.ToLower());
                                }
                                break;

                            case "border":
                                if (property.Value == "1")
                                {
                                    // Apply a thin border to all sides of the cell
                                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                }
                                else if (property.Value == "0")
                                {
                                    // Remove the border
                                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.None;
                                }
                                break;

                            case "value":
                                cell.Value = property.Value;
                                break;

                            default:
                                return new KeywordResult(KeywordResults.Fail, $"Unsupported property: {property.Key}");
                        }
                    }
                    workbook.SaveAs(_excelPath);
                }
                return new KeywordResult(KeywordResults.Pass, $"Cell at row {row}, column {column} formatted successfully.");
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, $"An error occurred: {ex.Message}");
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        [KeywordDescription("Validates a status column from a spreadsheet (Excel or CSV) by comparing it against an expected status value. " +
                    "Note: Ensure that the file path is set first using the 'SetExcelPath' keyword.")]
        [KeywordDisplayName("Validate Status From Spreadsheet")]
        [KeywordParameters("statusColumnName", "The name of the column containing the status values.")]
        [KeywordParameters("identifierColumnName", "The name of the column containing the identifier (like HostName or IP).")]
        [KeywordParameters("expectedStatusValue", "The expected value that each status cell should match (e.g., 'success').")]
        [SampleScript("ValidateStatusFromSpreadsheet(\"Status\", \"HostName\", \"success\")")]
        public KeywordResult ValidateStatusFromSpreadsheet(string statusColumnName, string identifierColumnName, string expectedStatusValue)
        {
            if (string.IsNullOrWhiteSpace(_excelPath))
            {
                return new KeywordResult(KeywordResults.Error, "File path is not set. Please create or open an Excel/CSV file first.");
            }

            if (string.IsNullOrWhiteSpace(expectedStatusValue))
            {
                return new KeywordResult(KeywordResults.Error, "Expected status value cannot be empty.");
            }

            try
            {
                var extension = Path.GetExtension(_excelPath).ToLower();
                var failedRows = new List<string>();
                string expectedStatusLower = expectedStatusValue.Trim().ToLower();

                if (extension == ".xlsx")
                {
                    // --- Excel handling ---
                    using (var workbook = new XLWorkbook(_excelPath))
                    {
                        var worksheet = workbook.Worksheets.First();
                        var usedRange = worksheet.RangeUsed();
                        if (usedRange == null)
                        {
                            return new KeywordResult(KeywordResults.Error, "The spreadsheet is empty or could not be read.");
                        }

                        var headers = usedRange.FirstRow().Cells().Select((c, i) => new { Name = c.GetString().Trim(), Index = i + 1 })
                                                                .ToDictionary(h => h.Name, h => h.Index, StringComparer.OrdinalIgnoreCase);

                        if (!headers.ContainsKey(statusColumnName))
                            return new KeywordResult(KeywordResults.Error, $"Column '{statusColumnName}' not found.");

                        if (!headers.ContainsKey(identifierColumnName))
                            return new KeywordResult(KeywordResults.Error, $"Column '{identifierColumnName}' not found.");

                        var statusColIndex = headers[statusColumnName];
                        var idColIndex = headers[identifierColumnName];

                        foreach (var row in usedRange.RowsUsed().Skip(1)) // Skip header
                        {
                            string status = row.Cell(statusColIndex).GetString().Trim().ToLower();
                            string identifier = row.Cell(idColIndex).GetString().Trim();

                            if (status != expectedStatusLower)
                            {
                                failedRows.Add($"{identifier} ({status})");
                            }
                        }
                    }
                }
                else if (extension == ".csv")
                {
                    // --- CSV handling ---
                    var lines = File.ReadAllLines(_excelPath);
                    if (lines.Length < 2)
                    {
                        return new KeywordResult(KeywordResults.Error, "The CSV file is empty or has no data rows.");
                    }

                    var headers = lines[0].Split(',').Select((h, i) => new { Name = h.Trim(), Index = i })
                                          .ToDictionary(h => h.Name, h => h.Index, StringComparer.OrdinalIgnoreCase);

                    if (!headers.ContainsKey(statusColumnName))
                        return new KeywordResult(KeywordResults.Error, $"Column '{statusColumnName}' not found in CSV.");

                    if (!headers.ContainsKey(identifierColumnName))
                        return new KeywordResult(KeywordResults.Error, $"Column '{identifierColumnName}' not found in CSV.");

                    var statusColIndex = headers[statusColumnName];
                    var idColIndex = headers[identifierColumnName];

                    foreach (var line in lines.Skip(1)) // Skip header
                    {
                        var cols = line.Split(',');
                        if (cols.Length <= Math.Max(statusColIndex, idColIndex)) continue;

                        string status = cols[statusColIndex].Trim().ToLower();
                        string identifier = cols[idColIndex].Trim();

                        if (status != expectedStatusLower)
                        {
                            failedRows.Add($"{identifier} ({status})");
                        }
                    }
                }
                else
                {
                    return new KeywordResult(KeywordResults.Error, $"Unsupported file format '{extension}'. Only .xlsx and .csv are supported.");
                }

                // Final result
                if (failedRows.Count == 0)
                {
                    return new KeywordResult(KeywordResults.Pass, $"All entries matched expected status '{expectedStatusValue}'.");
                }
                else
                {
                    string details = string.Join(", ", failedRows);
                    return new KeywordResult(KeywordResults.Fail, $"Some entries did not match expected status '{expectedStatusValue}': {details}");
                }
            }
            catch (Exception ex)
            {
                var error = new KeywordResult(KeywordResults.Error, $"Exception while validating file: {ex.Message}");
                Logger.Error(error.Output, ex);
                return error;
            }
        }



        [KeywordDescription("Sets the Excel file path to be used for further processing. " +
                    "This path will be referenced by other keywords such as 'ValidateStatusFromSpreadsheet'.")]
        [KeywordDisplayName("Set Excel Path")]
        [SampleScript("SetExcelPath(\"C:\\\\Reports\\\\Printers.xlsx\")")]
        public KeywordResult SetExcelPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return new KeywordResult(KeywordResults.Error, "Provided path is empty.");
            }

            if (!File.Exists(path))
            {
                return new KeywordResult(KeywordResults.Error, $"File does not exist at path: {path}");
            }

            try
            {
                _excelPath = path;

                // Open file in default Excel app for user visibility
               // Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });

                return new KeywordResult(KeywordResults.Pass, $"Excel path set successfully: {_excelPath}");
            }
            catch (Exception ex)
            {
                var error = new KeywordResult(KeywordResults.Error, $"Failed to open Excel file: {ex.Message}");
                Logger.Error(error.Output, ex);
                return error;
            }
        }


        [KeywordDescription("Sets the worksheet name or index to be used for further Excel operations. " +
            "Before using the Set Worksheet keyword, the Set Excel Path keyword must be called.\n " +
            "This worksheet will be referenced by other read keywords. ")]
        [KeywordDisplayName("Set Worksheet")]
        [KeywordParameters("worksheetIdentifier", "Specifies the worksheet name or worksheet index.")]
        [SampleScript("SpreadSheet.Set Worksheet(SheetName) OR SpreadSheet.Set Worksheet(SheetNumber)")]
        public KeywordResult SetWorksheet(string worksheetIdentifier)
        {
            if (string.IsNullOrWhiteSpace(_excelPath))
            {
                return new KeywordResult(KeywordResults.Error, "Excel path not set. Please use 'SetExcelPath' before setting the worksheet.");
            }
            if (string.IsNullOrWhiteSpace(worksheetIdentifier))
            {
                return new KeywordResult(KeywordResults.Error, "Provided worksheet name or index is empty.");
            }

            try
            {
                using (var workbook = new XLWorkbook(_excelPath))
                {
                    IXLWorksheet worksheet = null;

                    // Try to get worksheet by name first
                    if (workbook.Worksheets.TryGetWorksheet(worksheetIdentifier, out var foundSheet))
                    {
                        worksheet = foundSheet;
                    }
                    else if (int.TryParse(worksheetIdentifier, out int sheetIndex) && sheetIndex > 0 && sheetIndex <= workbook.Worksheets.Count)
                    {
                        worksheet = workbook.Worksheet(sheetIndex);
                    }
                    else
                    {
                        return new KeywordResult(KeywordResults.Error, $"Worksheet '{worksheetIdentifier}' not found in file: {_excelPath}");
                    }

                    _worksheetName = worksheet.Name;

                    return new KeywordResult(KeywordResults.Pass, $"Worksheet set successfully: {_worksheetName}");
                }
            }
            catch (Exception ex)
            {
                var error = new KeywordResult(KeywordResults.Error, $"Failed to set worksheet: {ex.Message}");
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        public void SetWorkSheet()
        {
            SetWorksheet("1");
        }

        [KeywordDescription("Get the total number of rows with data in the worksheet and save to variable")]
        [KeywordDisplayName("Total Row Count")]
        [KeywordParameters("variable", "Variable name to save the row count")]
        [SampleScript("SpreadSheet.Total Row Count(${rowCount})")]
        public KeywordResult TotalRowCount(string variable)
        {
            if (!IsExcelFileReady(out var err)) return err;
            if (string.IsNullOrWhiteSpace(variable))
            {
                return new KeywordResult(KeywordResults.Fail, "A variable name must be provided to save the row count.");
            }

            try
            {
                using (var workbook = new XLWorkbook(_excelPath))
                {
                    if (_worksheetName == null)
                    {
                        SetWorkSheet();
                    }
                    var worksheet = workbook.Worksheet(_worksheetName);
                    var lastRowUsed = worksheet.LastRowUsed();
                    int rowCount = lastRowUsed.RowNumber();

                    if (lastRowUsed == null)
                    {
                        return new KeywordResult(KeywordResults.Fail, "No data found in the worksheet.");
                    }
                    CommonExecutionInfo.SetVariable(variable.Trim(), rowCount.ToString());
                    KeywordResult result = new KeywordResult(KeywordResults.Pass)
                    {
                        Output = $"{variable} = {rowCount}"
                    };
                    return result;
                }
            }
            catch (Exception ex)
            {
                var error = new KeywordResult(KeywordResults.Error, $"An error occurred: {ex.Message}");
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        [KeywordDescription("Get the total number of columns with data in the worksheet and save to variable")]
        [KeywordDisplayName("Total Column Count")]
        [KeywordParameters("variable", "Variable name to save the column count")]
        [SampleScript("SpreadSheet.Total Column Count(${colCount})")]
        public KeywordResult TotalColumnCount(string variable)
        {
            if (!IsExcelFileReady(out var err)) return err;

            if (string.IsNullOrWhiteSpace(variable))
            {
                return new KeywordResult(KeywordResults.Fail, "A variable name must be provided to save the column count.");
            }
            try
            {
                using (var workbook = new XLWorkbook(_excelPath))
                {
                    if (_worksheetName == null)
                    {
                        SetWorkSheet();
                    }
                    var worksheet = workbook.Worksheet(_worksheetName);
                    var lastColumnUsed = worksheet.LastColumnUsed();
                    int columnCount = lastColumnUsed.ColumnNumber();

                    if (lastColumnUsed == null)
                    {
                        return new KeywordResult(KeywordResults.Fail, "No data found in the worksheet.");
                    }
                    CommonExecutionInfo.SetVariable(variable.Trim(), columnCount.ToString());

                    KeywordResult result = new KeywordResult(KeywordResults.Pass)
                    {
                        Output = $"{variable} = {columnCount}"
                    };
                    return result;
                }
            }
            catch (Exception ex)
            {
                var error = new KeywordResult(KeywordResults.Error, $"An error occurred: {ex.Message}");
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        [KeywordDescription("Read a cell value by matching a row value and a column value and save to variable")]
        [KeywordDisplayName("Read Cell Value")]
        [KeywordParameters("rowValue", "Existing cell value used to identify the row")]
        [KeywordParameters("columnValue", "Existing cell value used to identify the column")]
        [KeywordParameters("variable", "Variable name to save the intersecting cell value")]
        [SampleScript("SpreadSheet.Read Cell Value(color, orientation, ${result})")]
        public KeywordResult ReadCellValue(string rowValue, string columnValue, string variable)
        {
            if (!IsExcelFileReady(out var err)) return err;

            if (string.IsNullOrWhiteSpace(rowValue) || string.IsNullOrWhiteSpace(columnValue))
            {
                return new KeywordResult(KeywordResults.Fail, "Both rowValue and columnValue must be provided.");
            }
            if (string.IsNullOrWhiteSpace(variable))
            {
                return new KeywordResult(KeywordResults.Fail, "A variable name must be provided to save the cell value.");
            }
            try
            {
                using (var workbook = new XLWorkbook(_excelPath))
                {
                    if (_worksheetName == null)
                    {
                        SetWorkSheet();
                    }
                    var worksheet = workbook.Worksheet(_worksheetName);
                    int targetRow = -1;
                    foreach (var row in worksheet.RowsUsed())
                    {
                        foreach (var cell in row.CellsUsed())
                        {
                            if (cell.GetString().Trim().Equals(rowValue.Trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                targetRow = row.RowNumber();
                                break;
                            }
                        }
                        if (targetRow != -1)
                            break;
                    }
                    if (targetRow == -1)
                    {
                        return new KeywordResult(KeywordResults.Fail, $"Row value '{rowValue}' not found.");
                    }
                    int targetColumn = -1;
                    foreach (var col in worksheet.ColumnsUsed())
                    {
                        foreach (var cell in col.CellsUsed())
                        {
                            if (cell.GetString().Trim().Equals(columnValue.Trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                targetColumn = cell.Address.ColumnNumber;
                                break;
                            }
                        }
                        if (targetColumn != -1)
                            break;
                    }
                    if (targetColumn == -1)
                    {
                        return new KeywordResult(KeywordResults.Fail, $"Column value '{columnValue}' not found.");
                    }
                    var targetCell = worksheet.Cell(targetRow, targetColumn);
                    string cellValue = targetCell.GetString().Trim();

                    if (string.IsNullOrEmpty(cellValue))
                    {
                        return new KeywordResult(KeywordResults.Fail, $"No data found at the intersection of '{rowValue}' and '{columnValue}'.");
                    }
                    CommonExecutionInfo.SetVariable(variable.Trim(), cellValue);

                    KeywordResult result = new KeywordResult(KeywordResults.Pass)
                    {
                        Output = $"{variable} = {cellValue}"
                    };
                    return result;
                }
            }
            catch (Exception ex)
            {
                var error = new KeywordResult(KeywordResults.Error, $"Error: {ex.Message}");
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        [KeywordDescription("Read a cell value by specifying the row number and column number, then save it to a variable.")]
        [KeywordDisplayName("Read Cell Value By Index")]
        [KeywordParameters("rowNo", "Row number of the cell to read (starting from 1).")]
        [KeywordParameters("colNo", "Column number of the cell to read (starting from 1).")]
        [KeywordParameters("variable", "Variable name to save the cell value.")]
        [SampleScript("SpreadSheet.Read Cell Value By Index(2, 3, ${result})")]
        public KeywordResult ReadCellValueByIndex(string rowNo, string colNo, string variable)
        {
            if (!TryConvertToInt(rowNo, "startRow", out int r, out var error)) return error;
            if (!TryConvertToInt(colNo, "endRow", out int c, out error)) return error;
            int rowNumber = r;
            int columnNumber = c;

            if (!IsExcelFileReady(out var err))
                return err;
            if (rowNumber <= 0 || columnNumber <= 0)
            {
                return new KeywordResult(KeywordResults.Fail, "Row number and column number must be greater than zero.");
            }
            if (string.IsNullOrWhiteSpace(variable))
            {
                return new KeywordResult(KeywordResults.Fail, "A variable name must be provided to save the cell value.");
            }

            try
            {
                using (var workbook = new XLWorkbook(_excelPath))
                {
                    if (_worksheetName == null)
                    {
                        SetWorkSheet();
                    }
                    var worksheet = workbook.Worksheet(_worksheetName);
                    var targetCell = worksheet.Cell(rowNumber, columnNumber);
                    string cellValue = targetCell.GetString().Trim();

                    if (targetCell == null || targetCell.IsEmpty())
                    {
                        return new KeywordResult(KeywordResults.Fail,
                            $"No data found at Row {rowNumber}, Column {columnNumber}.");
                    }

                    CommonExecutionInfo.SetVariable(variable.Trim(), cellValue);

                    KeywordResult result = new KeywordResult(KeywordResults.Pass)
                    {
                        Output = $"{variable} = {cellValue}"
                    };
                    return result;
                }
            }
            catch (Exception ex)
            {
                var errors = new KeywordResult(KeywordResults.Error, $"Error: {ex.Message}");
                Logger.Error(errors.Output, ex);
                return errors;
            }
        }

        [KeywordDescription("Read the last column with data and save the values to a variable")]
        [KeywordDisplayName("Read Last Column")]
        [KeywordParameters("variable", "Variable name to save the last column data")]
        [SampleScript("SpreadSheet.Read Last Column(${lastColumnData})")]
        public KeywordResult ReadLastColumn(string variable)
        {
            if (!IsExcelFileReady(out var err)) return err;

            if (string.IsNullOrWhiteSpace(variable))
            {
                return new KeywordResult(KeywordResults.Fail, "A variable name must be provided to save the last column data.");
            }

            try
            {
                using (var workbook = new XLWorkbook(_excelPath))
                {
                    if (_worksheetName == null)
                    {
                        SetWorkSheet();
                    }
                    var worksheet = workbook.Worksheet(_worksheetName);
                    int lastUsedColumn = FindLastUsedColumn(worksheet);
                    var values = new List<string>();

                    if (lastUsedColumn == 0)
                    {
                        return new KeywordResult(KeywordResults.Fail, "No column with data found.");
                    }
                    foreach (var cell in worksheet.Column(lastUsedColumn).CellsUsed())
                    {
                        values.Add(cell.GetString().Trim());
                    }
                    string columnData = string.Join(", ", values);
                    CommonExecutionInfo.SetVariable(variable.Trim(), columnData);

                    KeywordResult result = new KeywordResult(KeywordResults.Pass)
                    {
                        Output = $"{variable} = Column {lastUsedColumn} data: {columnData}"
                    };
                    return result;
                }
            }
            catch (Exception ex)
            {
                var error = new KeywordResult(KeywordResults.Error, $"An error occurred: {ex.Message}");
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        [KeywordDescription("Read the last row with data and save it to a variable")]
        [KeywordDisplayName("Read Last Row")]
        [KeywordParameters("variable", "Variable name to save the last row data")]
        [SampleScript("SpreadSheet.Read Last Row(${lastRowData})")]
        public KeywordResult ReadLastRow(string variable)
        {
            if (!IsExcelFileReady(out var err)) return err;

            if (string.IsNullOrWhiteSpace(variable))
            {
                return new KeywordResult(KeywordResults.Fail, "A variable name must be provided to save the last row data.");
            }
            try
            {
                using (var workbook = new XLWorkbook(_excelPath))
                {
                    if (_worksheetName == null)
                    {
                        SetWorkSheet();
                    }
                    var worksheet = workbook.Worksheet(_worksheetName);
                    var lastUsedRow = worksheet.LastRowUsed();
                    int rowNumber = lastUsedRow.RowNumber();
                    var cellValues = lastUsedRow.CellsUsed()
                        .Select(cell => cell.GetString().Trim())
                        .ToList();

                    if (lastUsedRow == null)
                    {
                        return new KeywordResult(KeywordResults.Fail, "No data found in the worksheet.");
                    }
                    if (cellValues.Count == 0)
                    {
                        return new KeywordResult(KeywordResults.Fail, $"Row {rowNumber} is empty.");
                    }
                    string rowData = string.Join(", ", cellValues);
                    CommonExecutionInfo.SetVariable(variable.Trim(), rowData);
                    KeywordResult result = new KeywordResult(KeywordResults.Pass)
                    {
                        Output = $"{variable} = Row {rowNumber} data: {rowData}"
                    };
                    return result;
                }
            }
            catch (Exception ex)
            {
                var error = new KeywordResult(KeywordResults.Error, $"An error occurred: {ex.Message}");
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        [KeywordDescription("Read one or more columns based on column Names and save the result to a variable")]
        [KeywordDisplayName("Read Values By ColumnNames")]
        [KeywordParameters("variable", "Variable name to save the read column data")]
        [KeywordParameters("columnNames", "Names of the columns to read")]
        [SampleScript("SpreadSheet.Read Values By ColumnNames(${colData}, Name, Age, Email)")]
        public KeywordResult ReadValuesByColumnNames(string variable, params string[] columnNames)
        {
            if (!IsExcelFileReady(out var err)) return err;

            if (string.IsNullOrWhiteSpace(variable))
            {
                return new KeywordResult(KeywordResults.Fail, "A variable name must be provided to save the column data.");
            }
            if (columnNames == null || columnNames.Length == 0)
            {
                return new KeywordResult(KeywordResults.Fail, "No column names were provided to read.");
            }
            try
            {
                using (var workbook = new XLWorkbook(_excelPath))
                {
                    if (_worksheetName == null)
                    {
                        SetWorkSheet();
                    }
                    var worksheet = workbook.Worksheet(_worksheetName);
                    var headerRow = worksheet.Row(1);
                    var columnIndexMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    var resultBuilder = new StringBuilder();
                    bool anyColumnFound = false;

                    foreach (var cell in headerRow.CellsUsed())
                    {
                        var headerText = cell.GetString().Trim();
                        if (!columnIndexMap.ContainsKey(headerText))
                        {
                            columnIndexMap[headerText] = cell.Address.ColumnNumber;
                        }
                    }

                    foreach (var columnName in columnNames)
                    {

                        if (columnIndexMap.TryGetValue(columnName.Trim(), out int columnIndex))
                        {
                            anyColumnFound = true;
                            resultBuilder.AppendLine($"Column: {columnName}");
                            foreach (var cell in worksheet.Column(columnIndex).CellsUsed())
                            {
                                if (cell.Address.RowNumber > 1)
                                {
                                    resultBuilder.AppendLine(cell.GetString().Trim());
                                }
                            }
                            resultBuilder.AppendLine();
                        }
                        else
                        {
                            Logger.Warn($"Column '{columnName}' not found in header row.");
                        }
                    }
                    if (!anyColumnFound)
                    {
                        return new KeywordResult(KeywordResults.Fail, "None of the specified columns were found.");
                    }
                    string outputData = resultBuilder.ToString().Trim();
                    CommonExecutionInfo.SetVariable(variable.Trim(), outputData);

                    KeywordResult result = new KeywordResult(KeywordResults.Pass)
                    {
                        Output = $"{variable} = {outputData}"
                    };
                    return result;
                }
            }
            catch (Exception ex)
            {
                var error = new KeywordResult(KeywordResults.Error, $"An error occurred: {ex.Message}");
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        [KeywordDescription("Read one or more rows containing specified values anywhere in the sheet and save to a variable")]
        [KeywordDisplayName("Read Values By RowNames")]
        [KeywordParameters("variable", "Variable name to save the matching row data")]
        [KeywordParameters("values", "One or more values to search for in any column")]
        [SampleScript("SpreadSheet.Read Values By RowNames(${rowData}, Name, Id)")]
        public KeywordResult ReadValuesByRowNames(string variable, params string[] values)
        {
            if (!IsExcelFileReady(out var err)) return err;

            if (string.IsNullOrWhiteSpace(variable))
            {
                return new KeywordResult(KeywordResults.Fail, "A variable name must be provided to save the matching rows.");
            }
            if (values == null || values.Length == 0)
            {
                return new KeywordResult(KeywordResults.Fail, "At least one search value must be provided.");
            }
            try
            {
                using (var workbook = new XLWorkbook(_excelPath))
                {
                    if (_worksheetName == null)
                    {
                        SetWorkSheet();
                    }
                    var worksheet = workbook.Worksheet(_worksheetName);
                    var nameSet = new HashSet<string>(values.Select(v => v.Trim()), StringComparer.OrdinalIgnoreCase);
                    var matchingRows = new List<string>();
                    var output = new StringBuilder();

                    foreach (var row in worksheet.RowsUsed())
                    {
                        bool isMatch = row.CellsUsed()
                            .Any(cell => nameSet.Contains(cell.GetString().Trim()));

                        if (isMatch)
                        {
                            var rowValues = row.CellsUsed()
                                .Select(c => c.GetString().Trim())
                                .ToArray();

                            matchingRows.Add($"Row {row.RowNumber()}: {string.Join(" | ", rowValues)}");
                        }
                    }
                    if (matchingRows.Count == 0)
                    {
                        return new KeywordResult(KeywordResults.Fail, "No rows found containing the specified values.");
                    }
                    foreach (var row in matchingRows)
                        output.AppendLine(row);

                    string resultData = output.ToString().Trim();
                    CommonExecutionInfo.SetVariable(variable.Trim(), string.Empty);
                    CommonExecutionInfo.SetVariable(variable.Trim(), resultData);
                    return new KeywordResult(KeywordResults.Pass, $"{variable} = {resultData}");
                }
            }
            catch (Exception ex)
            {
                var error = new KeywordResult(KeywordResults.Error, $"An error occurred: {ex.Message}");
                Logger.Error(error.Output, ex);
                return error;
            }
        }


        private bool TryConvertToInt(string input, string paramName, out int result, out KeywordResult errorResult)
        {
            if (!int.TryParse(input, out result))
            {
                errorResult = new KeywordResult(
                    KeywordResults.Fail,
                    $"Invalid value '{input}' for parameter '{paramName}'. Must be a number."
                );
                return false;
            }
            errorResult = null;
            return true;
        }

        [KeywordDescription("Read rows in a specified range and store the result in a variable")]
        [KeywordDisplayName("Read Row Data")]
        [KeywordParameters("variable", "Variable name to save the read row data")]
        [KeywordParameters("startRow", "Start row number to read")]
        [KeywordParameters("endRow", "End row number to read")]
        [SampleScript("SpreadSheet.Read Row Data(${rowsData}, 1, 3)")]
        public KeywordResult ReadRowData(string variable, string startRow, string endRow)
        {
            if (!TryConvertToInt(startRow, "startRow", out int s, out var error)) return error;
            if (!TryConvertToInt(endRow, "endRow", out int e, out error)) return error;
            int startrow = s;
            int endrow = e;

            if (!IsExcelFileReady(out var err)) return err;

            if (string.IsNullOrWhiteSpace(variable))
            {
                return new KeywordResult(KeywordResults.Fail, "A variable name must be provided to store the row data.");
            }
            if (startrow <= 0 || endrow <= 0)
            {
                return new KeywordResult(KeywordResults.Fail, "Row numbers must be greater than 0.");
            }
            if (endrow < startrow)
            {
                return new KeywordResult(KeywordResults.Fail, "End row number must be greater than or equal to start row number.");
            }
            try
            {
                using (var workbook = new XLWorkbook(_excelPath))
                {
                    if (_worksheetName == null)
                    {
                        SetWorkSheet();
                    }
                    var worksheet = workbook.Worksheet(_worksheetName);
                    int lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
                    int readTo = Math.Min(endrow, lastRow);
                    var output = new StringBuilder();

                    if (startrow > lastRow)
                    {
                        return new KeywordResult(KeywordResults.Fail, $"Start row {startRow} is beyond the last used row ({lastRow}).");

                    }
                    if (endrow >= lastRow)
                    {
                        return new KeywordResult(KeywordResults.Fail, $"end row {endRow} is beyond the last used row ({lastRow}).");
                    }
                    for (int rowNum = startrow; rowNum <= readTo; rowNum++)
                    {
                        var row = worksheet.Row(rowNum);
                        var rowValues = row.CellsUsed().Select(c => c.GetString().Trim()).ToArray();

                        if (rowValues.Length > 0)
                        {
                            output.AppendLine($"Row {rowNum}: {string.Join(" | ", rowValues)}");
                        }
                        else
                        {
                            output.AppendLine($"Row {rowNum}: (empty)");
                        }
                    }
                    string result = output.ToString().Trim();
                    CommonExecutionInfo.SetVariable(variable.Trim(), result);
                    return new KeywordResult(KeywordResults.Pass, $"{variable} = {result}");
                }
            }
            catch (Exception ex)
            {
                var errors = new KeywordResult(KeywordResults.Error, $"An error occurred: {ex.Message}");
                Logger.Error(errors.Output, ex);
                return errors;
            }
        }

        [KeywordDescription("Read columns in a specified range and store the result in a variable")]
        [KeywordDisplayName("Read Column Data")]
        [KeywordParameters("variable", "Variable name to save the column data")]
        [KeywordParameters("startColumn", "Start column number to read")]
        [KeywordParameters("endColumn", "End column number to read")]
        [SampleScript("SpreadSheet.Read Column Data(${columnData}, 1, 3)")]
        public KeywordResult ReadColumnData(string variable, string startColumn, string endColumn)
        {
            if (!TryConvertToInt(startColumn, "startColumn", out int s, out var error)) return error;
            if (!TryConvertToInt(endColumn, "endColumn", out int e, out error)) return error;
            int startcol = s;
            int endcol = e;

            if (!IsExcelFileReady(out var err)) return err;

            if (string.IsNullOrWhiteSpace(variable))
            {
                return new KeywordResult(KeywordResults.Fail, "A variable name must be provided to store the column data.");
            }
            if (startcol <= 0 || endcol <= 0)
            {
                return new KeywordResult(KeywordResults.Fail, "Column numbers must be greater than 0.");
            }
            if (endcol < startcol)
            {
                return new KeywordResult(KeywordResults.Fail, "End column number must be greater than or equal to start column number.");
            }
            try
            {
                using (var workbook = new XLWorkbook(_excelPath))
                {
                    if (_worksheetName == null)
                    {
                        SetWorkSheet();
                    }
                    var worksheet = workbook.Worksheet(_worksheetName);
                    int lastColumn = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;
                    if (startcol > lastColumn)
                    {
                        return new KeywordResult(KeywordResults.Fail, $"Start column {startColumn} is beyond the last used column ({lastColumn}).");
                    }
                    if (endcol >= lastColumn)
                    {
                        return new KeywordResult(KeywordResults.Fail, $"end column {endColumn} is beyond the last used column ({lastColumn}).");
                    }

                    int readTo = Math.Min(endcol, lastColumn);
                    var output = new StringBuilder();
                    for (int colNum = startcol; colNum <= readTo; colNum++)
                    {
                        var column = worksheet.Column(colNum);
                        var cells = column.CellsUsed().ToArray();

                        if (cells.Length == 0)
                        {
                            output.AppendLine($"Column {colNum}: (empty)");
                            continue;
                        }
                        var values = cells.Select(c => c.GetString().Trim()).ToArray();
                        output.AppendLine($"Column {colNum}: {string.Join(" | ", values)}");
                    }

                    string result = output.ToString().Trim();
                    CommonExecutionInfo.SetVariable(variable.Trim(), result);
                    return new KeywordResult(KeywordResults.Pass, $"{variable} = {result}");
                }
            }
            catch (Exception ex)
            {
                var errors = new KeywordResult(KeywordResults.Error, $"An error occurred: {ex.Message}");
                Logger.Error(errors.Output, ex);
                return errors;
            }
        }

        private bool IsExcelFileReady(out KeywordResult errorResult)
        {
            if (string.IsNullOrWhiteSpace(_excelPath))
            {
                errorResult = new KeywordResult(
                    KeywordResults.Error,
                    "First create or specify an Excel file before performing this operation."
                );
                return false;
            }
            errorResult = null;
            return true;
        }

        [KeywordDescription("Calculate a formula using values from two cells and save the result to a variable or write to a cell. " +
    "Supports both numeric operations (add, subtract, multiply, divide, power, modulo) and date/time operations (subtract, days, hours, minutes, seconds). " +
    "For date/time cells, the keyword calculates the time difference in the specified unit.")]
        [KeywordDisplayName("Calculate Formula From Cells")]
        [KeywordParameters("cell1Row", "Row number of the first cell (starting from 1)")]
        [KeywordParameters("cell1Col", "Column number of the first cell (starting from 1)")]
        [KeywordParameters("cell2Row", "Row number of the second cell (starting from 1)")]
        [KeywordParameters("cell2Col", "Column number of the second cell (starting from 1)")]
        [KeywordParameters("operation", "Operation to perform. <ol><li><b>Numeric</b> add(+) | subtract(-) | multiply(*) | divide(/) | power(^) | modulo(%).</li> <li><b>DateTime</b> subtract/difference | days | hours | minutes | seconds</li></ol>")]
        [KeywordParameters("resultVariable", "Variable name to save the calculated result (optional if resultRow and resultCol are provided)")]
        [KeywordParameters("resultRow", "Row number where the result should be written (optional)")]
        [KeywordParameters("resultCol", "Column number where the result should be written (optional)")]
        [SampleScript("<br><b>Numeric Operations:</b><br>" +
              "SpreadSheet.Calculate Formula From Cells(2, 3, 4, 5, add, ${result})<br>" +
              "SpreadSheet.Calculate Formula From Cells(1, 1, 1, 2, +, ${sum})<br>" +
              "SpreadSheet.Calculate Formula From Cells(3, 4, 5, 6, -, ${difference})<br>" +
              "SpreadSheet.Calculate Formula From Cells(2, 2, 2, 3, multiply, ${product}, 7, 8)<br>" +
              "SpreadSheet.Calculate Formula From Cells(10, 1, 10, 2, /, ${quotient})<br>" +
              "SpreadSheet.Calculate Formula From Cells(5, 5, 5, 6, ^, ${power})<br>" +
              "SpreadSheet.Calculate Formula From Cells(8, 1, 8, 2, %, ${remainder})<br>" +
              "<b>Date/Time Operations:</b><br>" +
              "SpreadSheet.Calculate Formula From Cells(2, 1, 3, 1, days, ${daysDiff})<br>" +
              "SpreadSheet.Calculate Formula From Cells(2, 1, 3, 1, hours, ${hoursDiff})<br>" +
              "SpreadSheet.Calculate Formula From Cells(5, 2, 6, 2, subtract, ${timeDiff})")]
        public KeywordResult CalculateFormulaFromCells(string cell1Row, string cell1Col, string cell2Row, string cell2Col,
                                                        string operation, string resultVariable = "", string resultRow = "", string resultCol = "")
        {
            if (!IsExcelFileReady(out var err)) return err;

            // Validate and parse cell coordinates
            var coordinatesValidation = ValidateAndParseCellCoordinates(cell1Row, cell1Col, cell2Row, cell2Col,
                out int row1, out int col1, out int row2, out int col2);
            if (coordinatesValidation != null) return coordinatesValidation;

            // Validate operation
            if (string.IsNullOrWhiteSpace(operation))
            {
                return new KeywordResult(KeywordResults.Fail, "Operation parameter cannot be empty.");
            }

            // Validate output parameters
            var outputValidation = ValidateOutputParameters(resultVariable, resultRow, resultCol,
                out bool hasVariable, out bool hasResultCell, out int resRow, out int resCol);
            if (outputValidation != null) return outputValidation;

            try
            {
                using (var workbook = new XLWorkbook(_excelPath))
                {
                    if (_worksheetName == null)
                    {
                        SetWorkSheet();
                    }
                    var worksheet = workbook.Worksheet(_worksheetName);

                    // Read and validate cells
                    var cellValidation = GetAndValidateCells(worksheet, row1, col1, row2, col2,
                        out IXLCell cell1, out IXLCell cell2);
                    if (cellValidation != null) return cellValidation;

                    // Detect and extract DateTime values
                    if (TryExtractDateTimeValues(cell1, cell2, out DateTime dateTime1, out DateTime dateTime2))
                    {
                        return CalculateDateTimeDifference(dateTime1, dateTime2, operation, resultVariable,
                            hasVariable, hasResultCell, resRow, resCol, worksheet, workbook);
                    }

                    // Handle numeric operations
                    return CalculateNumericOperation(cell1, cell2, row1, col1, row2, col2, operation,
                        resultVariable, hasVariable, hasResultCell, resRow, resCol, worksheet, workbook);
                }
            }
            catch (Exception ex)
            {
                var exceptionError = new KeywordResult(KeywordResults.Error, $"Error: {ex.Message}");
                Logger.Error(exceptionError.Output, ex);
                return exceptionError;
            }
        }

        /// <summary>
        /// Validates and parses cell coordinates
        /// </summary>
        private KeywordResult ValidateAndParseCellCoordinates(string cell1Row, string cell1Col, string cell2Row, string cell2Col,
            out int row1, out int col1, out int row2, out int col2)
        {
            row1 = col1 = row2 = col2 = 0;

            if (!TryConvertToInt(cell1Row, "cell1Row", out row1, out var error)) return error;
            if (!TryConvertToInt(cell1Col, "cell1Col", out col1, out error)) return error;
            if (!TryConvertToInt(cell2Row, "cell2Row", out row2, out error)) return error;
            if (!TryConvertToInt(cell2Col, "cell2Col", out col2, out error)) return error;

            if (row1 <= 0 || col1 <= 0 || row2 <= 0 || col2 <= 0)
            {
                return new KeywordResult(KeywordResults.Fail, "Row and column numbers must be greater than zero.");
            }

            return null; 
        }

        /// <summary>
        /// Retrieves and validates cells from worksheet
        /// </summary>
        private KeywordResult GetAndValidateCells(IXLWorksheet worksheet, int row1, int col1, int row2, int col2,
            out IXLCell cell1, out IXLCell cell2)
        {
            cell1 = worksheet.Cell(row1, col1);
            cell2 = worksheet.Cell(row2, col2);

            if (cell1.IsEmpty() || cell2.IsEmpty())
            {
                return new KeywordResult(KeywordResults.Fail,
                    $"One or both cells are empty. Cell1({row1},{col1}), Cell2({row2},{col2})");
            }

            return null; 
        }

        /// <summary>
        /// Attempts to extract DateTime values from cells
        /// </summary>
        private bool TryExtractDateTimeValues(IXLCell cell1, IXLCell cell2, out DateTime dateTime1, out DateTime dateTime2)
        {
            // Try to get DateTime values from Excel cells
            bool isCell1DateTime = cell1.TryGetValue(out dateTime1);
            bool isCell2DateTime = cell2.TryGetValue(out dateTime2);

            // If Excel didn't recognize as DateTime, try parsing the string as time/date
            if (!isCell1DateTime)
            {
                isCell1DateTime = TryParseDateTime(cell1.GetString().Trim(), out dateTime1);
            }
            if (!isCell2DateTime)
            {
                isCell2DateTime = TryParseDateTime(cell2.GetString().Trim(), out dateTime2);
            }

            return isCell1DateTime && isCell2DateTime;
        }

        /// <summary>
        /// Attempts to parse a string as DateTime, supporting various date/time formats
        /// </summary>
        private bool TryParseDateTime(string value, out DateTime result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = DateTime.MinValue;
                return false;
            }

            // Try standard DateTime parsing
            if (DateTime.TryParse(value, out result))
            {
                return true;
            }

            // Try parsing as time-only format (HH:mm:ss or HH:mm:ss.fff)
            if (TimeSpan.TryParse(value, out TimeSpan timeSpan))
            {
                // Use today's date with the parsed time
                result = DateTime.Today.Add(timeSpan);
                return true;
            }

            // Try specific formats
            string[] formats = {
                "HH:mm:ss",
                "HH:mm:ss.fff",
                "h:mm:ss tt",
                "hh:mm:ss",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd",
                "MM/dd/yyyy HH:mm:ss",
                "MM/dd/yyyy"
            };

            if (DateTime.TryParseExact(value, formats, null, System.Globalization.DateTimeStyles.None, out result))
            {
                return true;
            }

            result = DateTime.MinValue;
            return false;
        }

        /// <summary>
        /// Validates output parameters (variable and/or result cell)
        /// </summary>
        private KeywordResult ValidateOutputParameters(string resultVariable, string resultRow, string resultCol,
            out bool hasVariable, out bool hasResultCell, out int resRow, out int resCol)
        {
            hasVariable = !string.IsNullOrWhiteSpace(resultVariable);
            hasResultCell = !string.IsNullOrWhiteSpace(resultRow) && !string.IsNullOrWhiteSpace(resultCol);
            resRow = 0;
            resCol = 0;

            if (!hasVariable && !hasResultCell)
            {
                return new KeywordResult(KeywordResults.Fail, "Either resultVariable or both resultRow and resultCol must be provided.");
            }

            if (hasResultCell)
            {
                if (!TryConvertToInt(resultRow, "resultRow", out resRow, out var error)) return error;
                if (!TryConvertToInt(resultCol, "resultCol", out resCol, out error)) return error;
                if (resRow <= 0 || resCol <= 0)
                {
                    return new KeywordResult(KeywordResults.Fail, "Result row and column numbers must be greater than zero.");
                }
            }

            return null; 
        }

        /// <summary>
        /// Calculates numeric operations between two cell values
        /// </summary>
        private KeywordResult CalculateNumericOperation(IXLCell cell1, IXLCell cell2, int row1, int col1, int row2, int col2,
            string operation, string resultVariable, bool hasVariable, bool hasResultCell, int resRow, int resCol,
            IXLWorksheet worksheet, XLWorkbook workbook)
        {
            string value1Str = cell1.GetString().Trim();
            string value2Str = cell2.GetString().Trim();

            // Try to parse as double
            if (!double.TryParse(value1Str, out double value1))
            {
                return new KeywordResult(KeywordResults.Fail,
                    $"Cell1({row1},{col1}) value '{value1Str}' is not a valid number or date.");
            }

            if (!double.TryParse(value2Str, out double value2))
            {
                return new KeywordResult(KeywordResults.Fail,
                    $"Cell2({row2},{col2}) value '{value2Str}' is not a valid number or date.");
            }

            // Perform the calculation
            var calculationResult = PerformNumericCalculation(value1, value2, operation);
            if (!calculationResult.success)
            {
                return new KeywordResult(KeywordResults.Fail, calculationResult.errorMessage);
            }

            // Save and output the result
            return SaveAndOutputResult(calculationResult.result.ToString(), resultVariable, hasVariable,
                hasResultCell, resRow, resCol, worksheet, workbook,
                $"Calculated {calculationResult.operationName}: {value1} {operation} {value2} = {calculationResult.result}");
        }

        /// <summary>
        /// Performs the actual numeric calculation based on operation
        /// </summary>
        private (bool success, double result, string operationName, string errorMessage) PerformNumericCalculation(double value1, double value2, string operation)
        {
            switch (operation.ToLower().Trim())
            {
                case "add":
                case "+":
                    return (true, value1 + value2, "addition", null);

                case "subtract":
                case "-":
                    return (true, value1 - value2, "subtraction", null);

                case "multiply":
                case "*":
                    return (true, value1 * value2, "multiplication", null);

                case "divide":
                case "/":
                    if (value2 == 0)
                        return (false, 0, null, "Cannot divide by zero.");
                    return (true, value1 / value2, "division", null);

                case "power":
                case "^":
                    return (true, Math.Pow(value1, value2), "power", null);

                case "modulo":
                case "%":
                    if (value2 == 0)
                        return (false, 0, null, "Cannot perform modulo with divisor zero.");
                    return (true, value1 % value2, "modulo", null);

                default:
                    return (false, 0, null, $"Unsupported operation '{operation}'. Valid operations: add, subtract, multiply, divide, power, modulo");
            }
        }

        /// <summary>
        /// Saves result to variable and/or cell and formats output message
        /// </summary>
        private KeywordResult SaveAndOutputResult(string resultValue, string resultVariable, bool hasVariable,
            bool hasResultCell, int resRow, int resCol, IXLWorksheet worksheet, XLWorkbook workbook, string baseMessage)
        {
            string outputMsg = baseMessage;

            // Save result to variable if specified
            if (hasVariable)
            {
                CommonExecutionInfo.SetVariable(resultVariable.Trim(), resultValue);
                outputMsg += $" | {resultVariable} = {resultValue}";
            }

            // Write result to cell if specified
            if (hasResultCell)
            {
                var resultCell = worksheet.Cell(resRow, resCol);
                if (double.TryParse(resultValue, out double numericResult))
                {
                    resultCell.Value = numericResult;
                }
                else
                {
                    resultCell.Value = resultValue;
                }
                workbook.Save();
                outputMsg += $" | Written to cell({resRow},{resCol})";
            }

            return new KeywordResult(KeywordResults.Pass, outputMsg);
        }

        /// <summary>
        /// Helper method to calculate time/date differences
        /// </summary>
        private KeywordResult CalculateDateTimeDifference(DateTime date1, DateTime date2, string operation,
            string resultVariable, bool hasVariable, bool hasResultCell, int resRow, int resCol,
            IXLWorksheet worksheet, XLWorkbook workbook)
        {
            var calculationResult = PerformDateTimeCalculation(date1, date2, operation);

            if (!calculationResult.success)
            {
                return new KeywordResult(KeywordResults.Fail, calculationResult.errorMessage);
            }

            return SaveAndOutputResult(calculationResult.resultValue, resultVariable, hasVariable,
                hasResultCell, resRow, resCol, worksheet, workbook, calculationResult.outputMessage);
        }

        /// <summary>
        /// Performs date/time difference calculations
        /// </summary>
        private (bool success, string resultValue, string outputMessage, string errorMessage) PerformDateTimeCalculation(DateTime date1, DateTime date2, string operation)
        {
            string resultValue;
            string outputMsg;

            // Check if these are time-only values (both have the same date component or default Excel date)
            bool isTimeOnly = date1.Date == date2.Date ||
                             (date1.Year < 1900 && date2.Year < 1900) ||
                             (date1.Date == DateTime.MinValue.Date && date2.Date == DateTime.MinValue.Date);

            // Check if these are date-only values (both have time as 00:00:00)
            bool isDateOnly = date1.TimeOfDay == TimeSpan.Zero && date2.TimeOfDay == TimeSpan.Zero;

            switch (operation.ToLower().Trim())
            {
                case "subtract":
                case "-":
                case "difference":
                    TimeSpan timeDiff = date1 - date2;
                    resultValue = timeDiff.ToString();

                    if (isTimeOnly)
                    {
                        outputMsg = $"Time difference: {date1:HH:mm:ss} - {date2:HH:mm:ss} = {timeDiff}";
                    }
                    else if (isDateOnly)
                    {
                        outputMsg = $"Date difference: {date1:yyyy-MM-dd} - {date2:yyyy-MM-dd} = {timeDiff}";
                    }
                    else
                    {
                        outputMsg = $"DateTime difference: {date1:yyyy-MM-dd HH:mm:ss} - {date2:yyyy-MM-dd HH:mm:ss} = {timeDiff}";
                    }
                    return (true, resultValue, outputMsg, null);

                case "days":
                case "daysdifference":
                    double daysDiff = (date1 - date2).TotalDays;
                    resultValue = $"{daysDiff:F2}";
                    if (isDateOnly)
                    {
                        outputMsg = $"Days difference: {date1:yyyy-MM-dd} - {date2:yyyy-MM-dd} = {daysDiff:F0} days";
                    }
                    else
                    {
                        outputMsg = $"Days difference: {date1:yyyy-MM-dd HH:mm:ss} - {date2:yyyy-MM-dd HH:mm:ss} = {daysDiff:F2} days";
                    }
                    return (true, resultValue, outputMsg, null);

                case "hours":
                case "hoursdifference":
                    double hoursDiff = (date1 - date2).TotalHours;
                    resultValue = $"{hoursDiff:F2}";
                    if (isTimeOnly)
                    {
                        outputMsg = $"Hours difference: {date1:HH:mm:ss} - {date2:HH:mm:ss} = {hoursDiff:F2} hours";
                    }
                    else
                    {
                        outputMsg = $"Hours difference: {date1:yyyy-MM-dd HH:mm:ss} - {date2:yyyy-MM-dd HH:mm:ss} = {hoursDiff:F2} hours";
                    }
                    return (true, resultValue, outputMsg, null);

                case "minutes":
                case "minutesdifference":
                    double minutesDiff = (date1 - date2).TotalMinutes;
                    resultValue = $"{minutesDiff:F2}";
                    if (isTimeOnly)
                    {
                        outputMsg = $"Minutes difference: {date1:HH:mm:ss} - {date2:HH:mm:ss} = {minutesDiff:F2} minutes";
                    }
                    else
                    {
                        outputMsg = $"Minutes difference: {date1:yyyy-MM-dd HH:mm:ss} - {date2:yyyy-MM-dd HH:mm:ss} = {minutesDiff:F2} minutes";
                    }
                    return (true, resultValue, outputMsg, null);

                case "seconds":
                case "secondsdifference":
                    double secondsDiff = (date1 - date2).TotalSeconds;
                    resultValue = $"{secondsDiff:F2}";
                    if (isTimeOnly)
                    {
                        outputMsg = $"Seconds difference: {date1:HH:mm:ss} - {date2:HH:mm:ss} = {secondsDiff:F2} seconds";
                    }
                    else
                    {
                        outputMsg = $"Seconds difference: {date1:yyyy-MM-dd HH:mm:ss} - {date2:yyyy-MM-dd HH:mm:ss} = {secondsDiff:F2} seconds";
                    }
                    return (true, resultValue, outputMsg, null);

                default:
                    return (false, null, null,
                        $"Operation '{operation}' is not supported for date/time values. Supported: subtract, days, hours, minutes, seconds");
            }
        }
    }
}

using HP.GFriend.GFLogger;
using HP.GFriend.Keywords;
using HP.GFriend.Support;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HP.GFriend.Core.Execution
{
    public class SpreadSheetForEachRowBlock : IGFRunnable
    {
        private List<IGFRunnable> _subBlocks;

        public string OriginalStatement { get; internal set; }
        public string DataSetName { get; internal set; }
        public string SheetName { get; set; }
        public string TableName { get; internal set; }
        public Dictionary<int, string> Headers;
        public string Result { get; private set; }
        public string TagName { get; protected set; }
        public bool IsStatement => false;
        public bool IsAlwaysPass { get; internal set; }

        private int _passCount = 0;
        private int _failCount = 0;
        private int _errorCount = 0;

        public SpreadSheetForEachRowBlock()
        {
            Result = "PASS";
            TagName = "SpreadSheetForEachRow";
            _subBlocks = new List<IGFRunnable>();
        }

        public void AddSubBlock(IGFRunnable line)
        {
            _subBlocks.Add(line);
        }

        public int GetNumberOfStatements()
        {
            return _subBlocks.Count;
        }

        public void Run(TestDataManager testDataManager, int repeatCount, int stackLevel = 0, Dictionary<string, string> arguments = null)
        {
            // Reset Result
            Result = "PASS";
            _passCount = 0;
            _failCount = 0;
            _errorCount = 0;

            Reporter.WriteToOutput(TagName, new Dictionary<string, string>() { { "TableName", TableName } }, false);
            Reporter.WriteToOutput("StartTime", Utils.GetTime());

            SpreadsheetDocument spreadsheetDocument = null;
            try
            {
                if (!testDataManager.DataSets.ContainsKey(DataSetName))
                {
                    Logger.Error("Dataset not exist");
                    Result = "ERROR";
                    Reporter.WriteToOutput("ErrorDescription", "Dataset not exist");
                    Reporter.WriteToOutput("EndTime", Utils.GetTime());
                    Reporter.WriteToOutput("Result", Result);
                    Reporter.WriteToOutput(TagName, false);
                    return;
                }

                // Open the spreadsheet document for editing so OpenXML row updates can be persisted.
                spreadsheetDocument = SpreadsheetDocument.Open(testDataManager.DataSets[DataSetName], true);

                Dictionary<string, string> displayName = new Dictionary<string, string>();
                displayName["DisplayName"] = $"Click Here to Dataset {DataSetName} : {Path.GetFileName(testDataManager.DataSets[DataSetName])}";
                Reporter.WriteToOutput("Link", displayName, Path.GetFileName(testDataManager.DataSets[DataSetName]));

                // Find the sheet by name
                WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                Sheet sheet = workbookPart.Workbook.Descendants<Sheet>()
                    .FirstOrDefault(s => s.Name == SheetName);

                if (sheet == null)
                {
                    throw new Exception($"Sheet '{SheetName}' not found");
                }

                // Get the worksheet part
                WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);

                // Get the sheet data
                SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();

                // Get headers
                Headers = new Dictionary<int, string>();
                var headerRow = sheetData.Elements<Row>().First();
                int columnIndex = 1;
                foreach (Cell cell in headerRow.Elements<Cell>())
                {
                    string headerValue = GetCellValue(workbookPart, cell)
                        .Trim().Replace(" ", "");
                    Headers[columnIndex++] = headerValue;
                }

                CommonExecutionInfo.SetSharedObject("DataSetHeaders", Headers);

                // Process each data row (skip header row)
                foreach (Row row in sheetData.Elements<Row>().Skip(1))
                {
                    string currentDataSet = string.Empty;
                    var cellValues = new List<string>();

                    // Get values for each column based on headers
                    foreach (var header in Headers)
                    {
                        Cell cell = row.Elements<Cell>().ElementAtOrDefault(header.Key - 1);
                        string currentData = cell != null
                            ? GetCellValue(workbookPart, cell)
                            : string.Empty;

                        CommonExecutionInfo.SetVariable("${" + header.Value + "}", currentData);
                        cellValues.Add(currentData);
                    }

                    currentDataSet = string.Join(",", cellValues);
                    CommonExecutionInfo.SetSharedObject("DataSetRow", row);

                    Reporter.WriteToOutput("Loop", new Dictionary<string, string>() { { "DataSet", currentDataSet } }, false);
                    Reporter.WriteToOutput("StartTime", Utils.GetTime());
                    Reporter.WriteToOutput("Output", $"Row Data: {currentDataSet}");

                    string loopResult = "PASS";
                    stackLevel++;

                    foreach (IGFRunnable subBlock in _subBlocks)
                    {
                        subBlock.Run(testDataManager, repeatCount, stackLevel, arguments);
                        if (subBlock.Result.ToUpper().Equals("FAIL"))
                        {
                            Result = "FAIL";
                            loopResult = "FAIL";
                        }
                        else if (subBlock.Result.ToUpper().Equals("ERROR"))
                        {
                            Result = "ERROR";
                            loopResult = "ERROR";
                        }

                        if (BuiltInLibrary.ExitTC || BuiltInLibrary.ExitTS)
                        {
                            break;
                        }
                    }

                    stackLevel--;
                    switch (loopResult)
                    {
                        case "PASS":
                            _passCount++;
                            break;
                        case "FAIL":
                            _failCount++;
                            break;
                        case "ERROR":
                            _errorCount++;
                            break;
                    }

                    Reporter.WriteToOutput("Result", loopResult);
                    Reporter.WriteToOutput("EndTime", Utils.GetTime());
                    Reporter.WriteToOutput("Loop", false);
                    CommonExecutionInfo.SetSharedObject("DataSetRow", null);

                    if (BuiltInLibrary.ExitTC || BuiltInLibrary.ExitTS)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error processing Excel table", ex);
                Result = "ERROR";
                Reporter.WriteToOutput("ErrorDescription", "Error processing Excel table");
                Reporter.WriteToOutput("AdditionalInfo", ex.ToString());
                Reporter.WriteToOutput("EndTime", Utils.GetTime());
                Reporter.WriteToOutput("Result", Result);
                Reporter.WriteToOutput(TagName, false);
                return;
            }
            finally
            {
                spreadsheetDocument?.Dispose();
                CommonExecutionInfo.SetSharedObject("DataSetHeaders", null);
            }

            if (IsAlwaysPass)
            {
                Result = "PASS";
                Reporter.WriteToOutput("AlwaysPass", "True");
            }

            BuiltInLibrary.LastRepeatResult = new Tuple<int, int, int>(_passCount, _failCount, _errorCount);
            Reporter.WriteToOutput("Result", Result);
            Reporter.WriteToOutput("EndTime", Utils.GetTime());
            Reporter.WriteToOutput(TagName, false);
        }

        // Helper method to get cell value
        private string GetCellValue(WorkbookPart workbookPart, Cell cell)
        {
            if (cell == null)
                return string.Empty;

            string value = cell.InnerText;

            // If the cell contains a shared string
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                var stringTable = workbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                if (stringTable != null)
                {
                    value = stringTable.SharedStringTable.Elements<SharedStringItem>()
                        .ElementAt(int.Parse(value)).InnerText;
                }
            }

            return value ?? string.Empty;
        }
    }
}
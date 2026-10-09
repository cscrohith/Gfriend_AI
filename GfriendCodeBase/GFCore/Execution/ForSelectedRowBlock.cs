using HP.GFriend.GFLogger;
using HP.GFriend.Keywords;
using HP.GFriend.Support;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.IO;

namespace HP.GFriend.Core.Execution
{
    public class ForSelectedRowBlock : IGFRunnable
    {
        private List<IGFRunnable> _subBlocks;

        public string OriginalStatement { get; internal set; }

        public string DataSetName { get; internal set; }

        public string SheetName { get; set; }

        public string TableName { get; internal set; }

        public Dictionary<int, string> Headers;
        public Dictionary<string, List<(string Operator, string Value)>> ColumnFilter { get; set; }


        public string Result { get; private set; }

        public string TagName { get; protected set; }
        public bool IsStatement
        {
            get { return false; }
        }

        public bool IsAlwaysPass { get; internal set; }


        private int _passCount = 0;
        private int _failCount = 0;
        private int _errorCount = 0;

        public ForSelectedRowBlock()
        {
            Result = "PASS";
            TagName = "ForSelectedRow";
            _subBlocks = new List<IGFRunnable>();
            ColumnFilter = new Dictionary<string, List<(string Operator, string Value)>>();

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
            // #312 Reset Result
            Result = "PASS";
            _passCount = 0;
            _failCount = 0;
            _errorCount = 0;

            Reporter.WriteToOutput(TagName, new Dictionary<string, string>() { { "TableName", TableName } }, false);
            Reporter.WriteToOutput("StartTime", Utils.GetTime());

            // Load Data from Excel
            ListObject excelTable = null;
            Workbook workbook = null;
            try
            {
                Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();
                if (!testDataManager.DataSets.ContainsKey(DataSetName))
                {
                    Logger.Error("Dataset not exist");
                    Result = "ERROR";
                    Reporter.WriteToOutput("ErrorDescription", "Dataset not exist");
                    Reporter.WriteToOutput("EndTime", Utils.GetTime());
                    Reporter.WriteToOutput("Result", Result);
                    Reporter.WriteToOutput("EndTime", Utils.GetTime());
                    Reporter.WriteToOutput(TagName, false);
                    return;
                }
                workbook = excelApp.Workbooks.Open(testDataManager.DataSets[DataSetName], false, false);
                Dictionary<string, string> displayName = new Dictionary<string, string>();
                displayName["DisplayName"] = $"Click Here to Dataset {DataSetName} : {Path.GetFileName(testDataManager.DataSets[DataSetName])}";
                Reporter.WriteToOutput("Link", displayName, Path.GetFileName(testDataManager.DataSets[DataSetName]));

                _Worksheet worksheet = workbook.Sheets[SheetName];

                excelTable = worksheet.ListObjects[TableName];
            }
            catch (Exception ex)
            {
                Logger.Error("Can not get the table from dataset", ex);
                Result = "ERROR";
                Reporter.WriteToOutput("ErrorDescription", "Can not get the table from dataset");
                Reporter.WriteToOutput("AdditionalInfo", ex.ToString());
                Reporter.WriteToOutput("EndTime", Utils.GetTime());
                Reporter.WriteToOutput("Result", Result);
                Reporter.WriteToOutput("EndTime", Utils.GetTime());
                Reporter.WriteToOutput(TagName, false);
                if (workbook != null)
                {
                    workbook.Close(false);
                }

                return;
            }


            // Get Header Information
            Headers = new Dictionary<int, string>();
            int i = 1;
            try
            {
                foreach (Range headerRow in excelTable.HeaderRowRange)
                {
                    object headerValue = headerRow.Value;
                    string sHeadervalue = headerValue.ToString();
                    Headers[i++] = sHeadervalue.Trim().Replace(" ", "");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Data table does not have cloumn header", ex);
                Result = "ERROR";
                Reporter.WriteToOutput("ErrorDescription", "Data table does not have cloumn header");
                Reporter.WriteToOutput("AdditionalInfo", ex.ToString());
                Reporter.WriteToOutput("EndTime", Utils.GetTime());
                Reporter.WriteToOutput("Result", Result);
                Reporter.WriteToOutput("EndTime", Utils.GetTime());
                Reporter.WriteToOutput(TagName, false);
                if (workbook != null)
                {
                    workbook.Close(false);
                }

                return;
            }

            CommonExecutionInfo.SetSharedObject("DataSetHeaders", Headers);
            foreach (Range row in excelTable.DataBodyRange.Rows)
            {
                // Set Variables with Header name
                string currentDataSet = string.Empty;
                object[,] data = row.Value;
                bool rowMatches = true;

                foreach (KeyValuePair<int, string> header in Headers)
                {
                    string currentData = data[1, header.Key]?.ToString() ?? string.Empty;
                    CommonExecutionInfo.SetVariable("${" + header.Value + "}", currentData);
                    currentDataSet += $",{currentData}";

                    if (ColumnFilter.ContainsKey(header.Value))
                    {
                        if (!EvaluateFilterConditions(ColumnFilter[header.Value], currentData))
                        {
                            rowMatches = false; // Row doesn't match the filter criteria
                            break;
                        }
                    }
                }

                if (!rowMatches) // Skip rows that don't match the filter criteria
                {
                    continue;
                }
                currentDataSet = currentDataSet.Trim(',');
                CommonExecutionInfo.SetSharedObject("DataSetRow", row);

                Reporter.WriteToOutput("Loop", new Dictionary<string, string>() { { "DataSet", currentDataSet } }, false);
                Reporter.WriteToOutput("StartTime", Utils.GetTime());
                Reporter.WriteToOutput("Output", $"Row Data: {currentDataSet}");
                string loopResult = "PASS";
                stackLevel++;
                foreach (IGFRunnable subBlock in _subBlocks)
                {

                    subBlock.Run(testDataManager, i, stackLevel, arguments);
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


            if (IsAlwaysPass)
            {
                Result = "PASS";
                Reporter.WriteToOutput("AlwaysPass", "True");
            }
            if (workbook != null)
            {
                workbook.Close(true);
            }
            CommonExecutionInfo.SetSharedObject("DataSetHeaders", null);

            BuiltInLibrary.LastRepeatResult = new Tuple<int, int, int>(_passCount, _failCount, _errorCount);
            Reporter.WriteToOutput("Result", Result);
            Reporter.WriteToOutput("EndTime", Utils.GetTime());
            Reporter.WriteToOutput(TagName, false);
        }
        private bool EvaluateFilterConditions(List<(string Operator, string Value)> filterConditions, string currentData)
        {

            bool conditionResult = false;
            foreach (var condition in filterConditions)
            {
                string filterOperator = condition.Operator;
                string filterValue = condition.Value;


                // Check if currentData is numeric before parsing
                if (double.TryParse(currentData, out double currentNum) &&
                    double.TryParse(filterValue, out double conditionNum))
                {
                    switch (filterOperator)
                    {
                        case "==":
                            conditionResult = currentNum == conditionNum;
                            break;
                        case "<":
                            conditionResult = currentNum < conditionNum;
                            break;
                        case "<=":
                            conditionResult = currentNum <= conditionNum;
                            break;
                        case ">":
                            conditionResult = currentNum > conditionNum;
                            break;
                        case ">=":
                            conditionResult = currentNum >= conditionNum;
                            break;
                        case "!=":
                            conditionResult = currentNum != conditionNum;
                            break;
                        default:
                            conditionResult = false; // Unknown operator
                            break;
                    }
                }
                else
                {
                    // Handle case where currentData or filterValue isn't numeric
                    switch (filterOperator)
                    {
                        case "==":
                            conditionResult = currentData == filterValue;
                            break;
                        case "!=":
                            conditionResult = currentData != filterValue;
                            break;
                        default:
                            conditionResult = false; // Unsupported comparison
                            break;
                    }
                }

                if (conditionResult && !filterOperator.Equals("!="))
                {
                    return true;
                }
                else if (!conditionResult && filterOperator.Equals("!="))
                {
                    return false;
                }
            }

            return conditionResult; // All conditions passed
        }


    }
}
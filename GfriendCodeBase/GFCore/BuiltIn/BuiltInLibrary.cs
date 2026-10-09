using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using HP.GFriend.Core.Execution;
using HP.GFriend.Core.Remote;
using HP.GFriend.GFLogger;
using HP.GFriend.Keywords.Support;
using Microsoft.Office.Interop.Excel;
using System.Globalization;
using System.Runtime.InteropServices;

using System.Net.Mail;
using System.Net;
using HP.DeviceAutomation.Dune;
using HP.DeviceAutomation;
using HP.DeviceAutomation.Jedi;
using Logger = HP.GFriend.GFLogger.Logger;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.Reflection;
using System.Xml.Serialization;
using System.Security.Cryptography;
using static HP.GFriend.Support.Utils;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using HP.GFriend.Core.BuiltIn;
using System.Net.Http;
using System.Windows.Forms;
using DocumentFormat.OpenXml.Spreadsheet;

namespace HP.GFriend.Keywords
{
    public class BuiltInLibrary : IGFLibrary
    {
        private string _outputDir;
        public static bool ExitTC { get; set; } = false;
        public static bool ExitTS { get; set; } = false;
        public static bool ExitCustomKeyword { get; set; } = false;
        public static bool Break { get; set; } = false;
        public static bool Continue { get; set; } = false;
        public static KeywordResults ForcedCustomKeywordResult { get; set; } = KeywordResults.Pass;
        public static bool _enableSystemandPrinterInformation = false;
        public static Tuple<int, int, int> LastRepeatResult = new Tuple<int, int, int>(0,0,0);

        private const string _varPattern = @"\$\{[0-9a-zA-Z_-]+\}";
        private static Regex varRegex = new Regex(_varPattern);
        private string xmlFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "EncryptedData.xml");
        private static readonly string _gEncryptionKey = "gfriend@123";
        private CancellationTokenSource cancellationTokenSource;
        private Task monitoringTask;
        private static readonly int PingIntervalSeconds = 30;
        private string LogFilePath;
        public static event EventHandler<LatencyStatusChangedEventArgs> OnLatencyChanged;
         public static event EventHandler<LatencyStatusChangedEventArgs> OnNetworkUnreachble;
        public static ManualResetEventSlim NetworkAvailableEvent = new ManualResetEventSlim(true); // Initially network is up
        private bool _lastLatencyCritical = false;
        public void Initialize(DeviceUnderTest dut, string outputDir)
        {
            _outputDir = outputDir;
        }

        public void Dispose() { }

        public string GetName()
        {
            return "BuiltIn";
        }

        public bool DutUsed()
        {
            return false;
        }

        public List<string> GetDependencies()
        {
            return null;
        }

        private bool ContentsCompre(string leftFile, string rightFile)
        {
            string leftContents;
            string rightContetns;

            if (string.IsNullOrEmpty(Path.GetDirectoryName(leftFile)))
            {
                leftFile = Path.Combine(_outputDir, leftFile);
            }

            if (string.IsNullOrEmpty(Path.GetDirectoryName(rightFile)))
            {
                rightFile = Path.Combine(_outputDir, rightFile);
            }


            leftContents = File.ReadAllText(leftFile);
            rightContetns = File.ReadAllText(rightFile);

            return leftContents.Equals(rightContetns);
        }

        #region Keywords
        [KeywordDescription("Sleep given seconds")]
        [KeywordParameters("seconds", "Time to sleep")]
        [SampleScript("Sleep(10)")]
        public KeywordResult Sleep(string seconds)
        {
            Thread.Sleep(Int32.Parse(seconds) * 1000);
            return new KeywordResult(KeywordResults.Pass);
        }


        [KeywordDescription("Compare two files and check if they have same contents")]
        [KeywordParameters("leftFile", "First file to compare")]
        [KeywordParameters("rightFile", "Second file to compare")]
        [KeywordDisplayName("Check If Files Are Same")]
        [SampleScript("Check If Files Are Same (C:\\Users\\ReVi345\\Downloads\\675\\sample1.txt,C:\\Users\\ReVi345\\Downloads\\675\\sample2.txt)")]
        public KeywordResult CheckIfFilesAreSame(string leftFile, string rightFile)
        {
            KeywordResults results = KeywordResults.Fail;
            if (ContentsCompre(leftFile, rightFile))
            {
                results = KeywordResults.Pass;
            }
            return new KeywordResult(results);
        }

        [KeywordDescription("Compare two files and check if they DO NOT have same contents")]
        [KeywordParameters("leftFile", "First file to compare")]
        [KeywordParameters("rightFile", "Second file to compare")]
        [KeywordDisplayName("Check If Files Are Not Same")]
        [SampleScript("Check If Files Are Not Same (C:\\Users\\ReVi345\\Downloads\\675\\sample1.txt,C:\\Users\\ReVi345\\Downloads\\675\\sample2.txt)")]
        public KeywordResult CheckIfFilesAreNotSame(string leftFile, string rightFile)
        {
            KeywordResults results = KeywordResults.Fail;
            if (!ContentsCompre(leftFile, rightFile))
            {
                results = KeywordResults.Pass;
            }
            return new KeywordResult(results);
        }

        [KeywordDescription("Sleep random seconds between minSec and maxSec")]
        [KeywordParameters("minSec", "Minimum seconds to sleep")]
        [KeywordParameters("maxSec", "Maximum seconds to sleep")]
        [KeywordDisplayName("Random Sleep")]
        [SampleScript("Random Sleep (2,5)")]
        public KeywordResult RandomSleep(string minSec, string maxSec)
        {
            Random r = new Random();
            int sleepSec = r.Next(Int32.Parse(minSec), Int32.Parse(maxSec) + 1);
            Thread.Sleep(sleepSec * 1000);
            return new KeywordResult(KeywordResults.Pass);
        } 


        [KeywordDescription("Wait until user click the OK button on popup dialog")]
        [KeywordDisplayName("Wait For User Confirm")]
        [KeywordParameters("message", "Message to show in the popup dialog. (ex. Load paper on ADF)")]
        [SampleScript("Wait For User Confirm (please confirm)")]
        public KeywordResult WaitForUserConfirm(string message)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            UserInteractionForm form = new UserInteractionForm(UserInteractionForm.Type.Confirm, kr, message);
            form.ShowDialog();

            return kr;
        }

        [KeywordDescription("Request manual verification by user with popup dialog")]
        [KeywordDisplayName("Request User Verification")]
        [KeywordParameters("message", "Message to show in the popup dialog (ex. Check printed output)")]
        [SampleScript("Request User Verification (Please Confirm)")]
        public KeywordResult RequestUserVerification(string message)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            UserInteractionForm form = new UserInteractionForm(UserInteractionForm.Type.Verification, kr, message);
            form.ShowDialog();

            return kr;
        }

        [KeywordDescription("Compare two given values. Can be used as if condition")]
        [KeywordDisplayName("Equals")]
        [KeywordParameters("value1", "First value to compare")]
        [KeywordParameters("value2", "Second value to compare")]
        [SampleScript("Equals(${Val1},${Val2})  || Usage-- If: Equals (${name},BM){Pass (account logged in succesfully)}")]
        public KeywordResult Equals(string value1, string value2)
        {
            if(value1.Equals(value2))
            {
                return new KeywordResult(KeywordResults.Pass);
            }
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            kr.Output = $"{value1} and {value2} are not same";
            return kr;
        }
        [KeywordDescription("Compare two given values. Can be used as if condition")]
        [KeywordDisplayName("NotEquals")]
        [KeywordParameters("value1", "First value to compare")]
        [KeywordParameters("value2", "Second value to compare")]
        [SampleScript("NotEquals(${var1},${var2})")]
        public KeywordResult NotEquals(string value1, string value2)
        {
            if (!value1.Equals(value2))
            {
                return new KeywordResult(KeywordResults.Pass);
            }
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            kr.Output = $"{value1} and {value2} are same";
            return kr;
        }

        [KeywordDescription("Compare two given amounts values. Can be used as if condition")]
        [KeywordDisplayName("CompareAmounts")]
        [KeywordParameters("value1", "First value to compare")]
        [KeywordParameters("value2", "Second value to compare")]
        [SampleScript("CompareAmounts (10,10)")]
        public KeywordResult CompareAmounts(string value1, string value2)
        {
            // Removes if rupees symbol contains
            if(value1.Contains('₹'))
            {
                value1 = value1.TrimStart('₹');
            }
            value1 = value1.Replace("\\", "");
            double v1Input;
            double v2Input;

            double.TryParse(value1, out v1Input);
            double.TryParse(value2, out v2Input);

            if (v1Input.Equals(v2Input))
            {
                return new KeywordResult(KeywordResults.Pass);
            }

            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            kr.Output = $"{value1} and {value2} are not same";
            return kr;
        }

        [KeywordDescription("Compare two given values (case insensitive compare). Can be used as if condition")]
        [KeywordDisplayName("Equals IgnoreCase")]
        [KeywordParameters("value1", "First value to compare")]
        [KeywordParameters("value2", "Second value to compare")]
        [SampleScript(" Equals IgnoreCase(${Val1},${Val2}) || Usage-- If: Equals IgnoreCase (${Enable_Email},Y) {}")]
        public KeywordResult EqualsIgnoreCase(string value1, string value2)
        {
            if (value1.Equals(value2, StringComparison.CurrentCultureIgnoreCase))
            {
                return new KeywordResult(KeywordResults.Pass);
            }
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            kr.Output = $"{value1} and {value2} are not same";
            return kr;
        }

        [KeywordDescription("Check if needle text is exist in haystack text. If the variables  contains comma \",\" then add escape character backward slash \\ before it. Ex: Please\\, retry sending or cancel.")]
        [KeywordDisplayName("Contains")]
        [KeywordParameters("haystack", "target text")]
        [KeywordParameters("needle", "text to find")]
        [SampleScript(" Contains(Welcome Back,Welcome) || Usage--If: Contains (${DefaultResolution},${){}")]
        public KeywordResult Contains(string haystack, string needle)
        {
            if (haystack.Contains("\\n"))
            {
                haystack = haystack.Replace("\\n", "\n");
            }
            if (needle.Contains("\\n"))
            {
                needle = needle.Replace("\\n", "\n");
            }
            if (haystack.Contains(needle))
            {
                return new KeywordResult(KeywordResults.Pass);
            }
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            kr.Output = $"Can not find {needle} in {haystack}";
            return kr;
        }

        [KeywordDescription("Check if needle text is exist in haystack text (case insensitive check)")]
        [KeywordDisplayName("Contains Ignore Case")]
        [KeywordParameters("haystack", "target text")]
        [KeywordParameters("needle", "text to find")]
        [SampleScript(" Contains Ignore Case(Welcome Back,Welcome) || Usage--If: Contains Ignore Case (${FromAddressSource},User){}")]
        public KeywordResult ContainsIgnoreCase(string haystack, string needle)
        {
            if (haystack.ToUpper().Contains(needle.ToUpper()))
            {
                return new KeywordResult(KeywordResults.Pass);
            }
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            kr.Output = $"Can not find {needle} in {haystack}";
            return kr;
        }

        [KeywordDescription("Always Pass")]
        [KeywordDisplayName("Pass")]
        [KeywordParameters("message", "Pass Message")]
        [SampleScript("Pass (${KEYWORD_OUTPUT})")]
        public KeywordResult Pass(string message)
        {
            if(message.Contains("$"))
            {
                int variableEndIndex = message.IndexOf('}');
                string variablename = message.Substring(message.IndexOf('$'), variableEndIndex + 1);

                string result = CommonExecutionInfo.GetVariable(variablename);
                if(!string.IsNullOrEmpty(result))
                {
                    message = result;
                }
            }
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            kr.Output = message;

            return kr;
        }

        [KeywordDescription("Always Fail")]
        [KeywordDisplayName("Fail")]
        [KeywordParameters("message", "Fail Message")]
        [SampleScript("Fail(Above test case failed)")]
        public KeywordResult Fail(string message)
        {
            if (message.Contains("$"))
            {
                int variableEndIndex = message.IndexOf('}');
                string variablename = message.Substring(message.IndexOf('$'), variableEndIndex + 1);

                string result = CommonExecutionInfo.GetVariable(variablename);
                if (!string.IsNullOrEmpty(result))
                {
                    message = result;
                }
            }
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            kr.Output = message;

            return kr;
        }

        [KeywordDescription("Always Error")]
        [KeywordDisplayName("Error")]
        [KeywordParameters("message", "Error Message")]
        [SampleScript("Error(Above test case getting Error)")]
        public KeywordResult Error(string message)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Error);
            kr.Output = message;

            return kr;
        }

        [KeywordDescription("Stop Testcase and ignore rest of Testcase")]
        [KeywordDisplayName("Stop Test Case")]
        [SampleScript("Stop Test Case")]
        public KeywordResult StopTestCase()
        {
            ExitTC = true;
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            kr.Output = "Stop Test Case by user";
            return kr;
        }

        [KeywordDescription("Stop Testsuite and ignore rest of Testcase and Testsuite")]
        [KeywordDisplayName("Stop Test Suite")]
        [SampleScript("Stop Test Suite")]
        public KeywordResult StopTestSuite()
        {
            ExitTS = true;
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            kr.Output = "Stop Test Suite by user";
            return kr;
        }

        [KeywordDescription("(Custom keyword only) Exit current custom keyword with fail and make custom keyword result to Fail in spite of previous result.")]
        [KeywordDisplayName("Return With Fail")]
        [KeywordParameters("message", "Fail description")]
        [SampleScript("Return With Fail (Open device EWS failed to complete!)")]
        public KeywordResult ReturnWithFail(string message)
        {
            ExitCustomKeyword = true;
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            ForcedCustomKeywordResult = KeywordResults.Fail;
            kr.Output = message;
            return kr;
        }

        [KeywordDescription("(Custom keyword only) Exit current custom keyword with pass and make custom keyword result to Pass in spite of previous result.")]
        [KeywordDisplayName("Return With Pass")]
        [KeywordParameters("message", "Pass description")]
        [SampleScript("Return With Pass (OpenEWS completed!)")]
        public KeywordResult ReturnWithPass(string message)
        {
            ExitCustomKeyword = true;
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            ForcedCustomKeywordResult = KeywordResults.Pass;
            kr.Output = message;
            return kr;
        }

        [KeywordDescription("(Custom keyword only) Exit current custom keyword with error and make custom keyword result to Error in spite of previous result.")]
        [KeywordDisplayName("Return With Error")]
        [KeywordParameters("message", "Error description")]
        [SampleScript("Return With Error (Error while opeing device EWS!)")]
        public KeywordResult ReturnWithError(string message)
        {
            ExitCustomKeyword = true;
            KeywordResult kr = new KeywordResult(KeywordResults.Error);
            ForcedCustomKeywordResult = KeywordResults.Error;
            kr.Output = message;
            return kr;
        }

        [KeywordDescription("Check if file is exist")]
        [KeywordDisplayName("File Exist")]
        [KeywordParameters("filePath", "file path to check")]
        [SampleScript("File Exist (C:\\Users\\ReVi345\\Downloads\\675\\sample1.txt)")]
        public KeywordResult FileExist(string filePath)
        {
            if(File.Exists(filePath))
            {
                return new KeywordResult(KeywordResults.Pass);
            }
            return new KeywordResult(KeywordResults.Fail);
        }

        [KeywordDescription("Check if file is exist")]
        [KeywordDisplayName("Folder Exist")]
        [KeywordParameters("folderPath", "folder path to check")]
        [SampleScript("Folder Exit(C:\\Users\\ReVi345\\Downloads\\675)")]
        public KeywordResult FolderExist(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                return new KeywordResult(KeywordResults.Pass);
            }
            return new KeywordResult(KeywordResults.Fail);
        }

        [KeywordDescription("Create Folder with given path")]
        [KeywordDisplayName("Create Folder")]
        [KeywordParameters("folderPath", "folder path to create")]
        [SampleScript("Create Folder (C:\\Files\\samplefiles)")]
        public KeywordResult CreateFolder(string folderPath)
        {
            if(Directory.CreateDirectory(folderPath).Exists)
            {
                return new KeywordResult(KeywordResults.Pass);
            }
            return new KeywordResult(KeywordResults.Fail);
        }

        [GetKeyword]
        [KeywordDescription("Read all text from file")]
        [KeywordDisplayName("Read From File")]
        [KeywordParameters("filePath", "file path to check")]
        [KeywordParameters("saveTo", "Variable name to save file contents")]
        [SampleScript("Read From File (${FilePath}\\orderid.json,${json_output})")]
        public KeywordResult ReadFromFile(string filePath, string saveTo)
        {
            filePath = Utils.GetVariablevalueIfExist(filePath);
            filePath = Utils.GetAbsolutePath(filePath, CommonExecutionInfo.ScriptFolder);
            if (!File.Exists(filePath))
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "File is not exist";
                return fail;
            }
            string fileContens = File.ReadAllText(filePath);
            CommonExecutionInfo.SetVariable(saveTo, fileContens);

            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Save all text to file and if file exists, overwrites contents")]
        [KeywordDisplayName("Save To File")]
        [KeywordParameters("filePath", "file path to save")]
        [KeywordParameters("value", "value to save into file")]
        [SampleScript("Save To File (C:\\Users\\ReVi345\\Downloads\\675\\sample.txt,abcdefgh)")]
        public KeywordResult SaveToFile(string filePath, string value)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + "SaveToFile.txt";
            }
            filePath = Utils.GetAbsolutePath(filePath, _outputDir);
            try
            {
                File.WriteAllText(filePath, value);
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error during write value to file.";
                error.AdditionalInfo = ex.ToString();
                Logger.Error("Error during write value to file.", ex);
                return error;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Check if file contains given text")]
        [KeywordDisplayName("File Contains Text")]
        [KeywordParameters("filePath", "file path to check")]
        [KeywordParameters("text", "text to check")]
        [SampleScript("File Contains Text (${OUTPUT_FOLDER}\\test.prn,<</MediaType \\(Plain\\)>> setpagedevice)")]
        public KeywordResult FileContainsText(string filePath, string text)
        {
            filePath = Utils.GetAbsolutePath(filePath, CommonExecutionInfo.ScriptFolder);
            if (!File.Exists(filePath))
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "File is not exist";
                return fail;
            }
            string fileContens = File.ReadAllText(filePath);

            if(!fileContens.Contains(text))
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Can not find [{text}] in file. See Additional Info for check file contents";
                fail.AdditionalInfo = fileContens;
                return fail;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Write value to data set with given column name and value. This keyword can be called within For Each Row or For Selected Row loops.")]
        [KeywordDisplayName("Write To Data Set")]
        [KeywordParameters("columnName", "Column name to write. No space (ex. Paper Type - PaperType).")]
        [KeywordParameters("value", "Value to write")]
        [SampleScript("Write To Data Set (ShoesPrice,${savehere})")]
        public KeywordResult WriteToDataSet(string columnName, string value)
        {
            object rowObject = HP.GFriend.Core.CommonExecutionInfo.GetSharedObject("DataSetRow");
            IDictionary<int, string> headers = HP.GFriend.Core.CommonExecutionInfo.GetSharedObject("DataSetHeaders") as IDictionary<int, string>;

            if (rowObject == null || headers == null)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "This keyword must be called within For Each Row, For Selected Row, or Spreadsheet For Each Row.";
                Logger.Error(error.Output);
                return error;
            }

            int index = 0;
            string availableColumns = string.Join(", ", headers.Values.OrderBy(x => x));
            string trimmedColumnName = columnName.Trim();
            foreach (KeyValuePair<int, string> header in headers)
            {
                if (header.Value.Trim().Equals(trimmedColumnName, StringComparison.OrdinalIgnoreCase))
                {
                    index = header.Key;
                    break;
                }
            }

            if (index == 0)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"Column '{columnName}' not found. Available columns: {availableColumns}";
                Logger.Error(error.Output);
                return error;
            }

            try
            {
                switch (rowObject)
                {
                    case Range excelRow:
                        object rowValue = excelRow.Value;
                        if (rowValue is object[,] data)
                        {
                            if (data.GetLength(1) < index)
                            {
                                KeywordResult rangeError = new KeywordResult(KeywordResults.Error);
                                rangeError.Output = $"Column index {index} is outside the current row range.";
                                Logger.Error(rangeError.Output);
                                return rangeError;
                            }

                            data[1, index] = value;
                            excelRow.Value = data;
                        }
                        else
                        {
                            excelRow.Value = value;
                        }

                        return new KeywordResult(KeywordResults.Pass);
                    case Row openXmlRow:
                        return WriteValueToOpenXmlRow(openXmlRow, index, value);
                    default:
                        KeywordResult error = new KeywordResult(KeywordResults.Error);
                        error.Output = $"Unsupported dataset row type: {rowObject.GetType().FullName}";
                        Logger.Error(error.Output);
                        return error;
                }
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error during write value to dataset.";
                error.AdditionalInfo = ex.ToString();
                Logger.Error("Error during write value to dataset.", ex);
                return error;
            }
        }

        private KeywordResult WriteValueToOpenXmlRow(Row row, int index, string value)
        {
            int rowNumber = (int)(row.RowIndex?.Value ?? 1U);
            int columnNumber = index;
            StringBuilder columnName = new StringBuilder();
            while (columnNumber > 0)
            {
                columnNumber--;
                columnName.Insert(0, (char)('A' + (columnNumber % 26)));
                columnNumber /= 26;
            }

            string cellReference = $"{columnName}{rowNumber.ToString(CultureInfo.InvariantCulture)}";
            Cell cell = row.Elements<Cell>().FirstOrDefault(currentCell =>
                string.Equals(currentCell.CellReference?.Value, cellReference, StringComparison.OrdinalIgnoreCase));

            if (cell == null)
            {
                cell = new Cell { CellReference = cellReference };
                row.Append(cell);
            }

            cell.CellValue = new CellValue(value ?? string.Empty);
            cell.DataType = CellValues.String;

            DocumentFormat.OpenXml.Spreadsheet.Worksheet worksheet = row.Ancestors<DocumentFormat.OpenXml.Spreadsheet.Worksheet>().FirstOrDefault();
            if (worksheet == null)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Unable to save the OpenXML worksheet after updating the dataset row.";
                Logger.Error(error.Output);
                return error;
            }            
            worksheet.Save();
            return new KeywordResult(KeywordResults.Pass);
        }


        [KeywordDescription("Write value to data set at specified row and column. Can work within or outside For Each Row context by using Workbook/Worksheet names.")]
        [KeywordDisplayName("Write To Data Set Advanced")]
        [KeywordParameters("sheetName", "Name like 'Sheet1' or use active sheet")]
        [KeywordParameters("rowIndex", "Row index to write (1-based, 1 is header, 2 is first data row)")]
        [KeywordParameters("columnLetter", "Column letter like 'A', 'B', 'AA' etc.")]
        [KeywordParameters("value", "Value to write to the cell")]
        [SampleScript("Write To Data Set Advanced (Sheet1,2,C,100)")]
        public KeywordResult WriteToDataSetAdvanced(string sheetName, string rowIndex, string columnLetter, string value)
        {
            try
            {
                if (!int.TryParse(rowIndex, out int row) || row < 1)
                {
                    KeywordResult error = new KeywordResult(KeywordResults.Error);
                    error.Output = $"Invalid row index: {rowIndex}. Must be a positive number (1-based).";
                    Logger.Error($"Invalid row index: {rowIndex}");
                    return error;
                }

                if (string.IsNullOrWhiteSpace(columnLetter))
                {
                    KeywordResult error = new KeywordResult(KeywordResults.Error);
                    error.Output = "Column letter cannot be empty. Use 'A', 'B', 'AA' etc.";
                    Logger.Error("Column letter is empty");
                    return error;
                }

                Microsoft.Office.Interop.Excel.Application excelApp = null;
                try
                {
                    excelApp = (Microsoft.Office.Interop.Excel.Application)Marshal.GetActiveObject("Excel.Application");
                }
                catch
                {
                    KeywordResult error = new KeywordResult(KeywordResults.Error);
                    error.Output = "No Excel application found. Open the dataset file in Excel first or use Write To Data Set within For Each Row block.";
                    Logger.Error("Excel application not found");
                    return error;
                }

                if (excelApp == null || excelApp.Workbooks.Count == 0)
                {
                    KeywordResult error = new KeywordResult(KeywordResults.Error);
                    error.Output = "No workbook is currently open. Please open the dataset file first.";
                    Logger.Error("No workbook open");
                    return error;
                }

                // Get the active workbook
                Microsoft.Office.Interop.Excel.Workbook workbook = excelApp.ActiveWorkbook;
                if (workbook == null)
                {
                    KeywordResult error = new KeywordResult(KeywordResults.Error);
                    error.Output = "No active workbook found.";
                    Logger.Error("No active workbook");
                    return error;
                }

                // Find the sheet
                Microsoft.Office.Interop.Excel.Worksheet worksheet = null;
                if (string.IsNullOrWhiteSpace(sheetName) || sheetName.Equals("active", StringComparison.OrdinalIgnoreCase))
                {
                    worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.ActiveSheet;
                }
                else
                {
                    try
                    {
                        worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Sheets[sheetName];
                    }
                    catch
                    {
                        KeywordResult error = new KeywordResult(KeywordResults.Error);
                        error.Output = $"Sheet '{sheetName}' not found in workbook.";
                        Logger.Error($"Sheet '{sheetName}' not found");
                        return error;
                    }
                }

                if (worksheet == null)
                {
                    KeywordResult error = new KeywordResult(KeywordResults.Error);
                    error.Output = "Unable to access worksheet.";
                    Logger.Error("Cannot access worksheet");
                    return error;
                }

                // Write the value to the specified cell
                string cellAddress = columnLetter + row;
                Range cell = worksheet.Range[cellAddress];
                cell.Value = value;

                Logger.Warn($"Successfully wrote '{value}' to cell {cellAddress} on sheet '{worksheet.Name}'");
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error writing to dataset.";
                error.AdditionalInfo = ex.ToString();
                Logger.Error("Error writing to dataset.", ex);
                return error;
            }
        }


        [KeywordDescription("Generate time stamp with given format and save to variable")]
        [KeywordDisplayName("Get Time Stamp")]
        [KeywordParameters("variable", "variable name to save")]
        [KeywordParameters("format", "string format. See https://docs.microsoft.com/dotnet/standard/base-types/custom-date-and-time-format-strings")]
        [SampleScript("Get Time Stamp (${a},dd MMM yyyy hh:mm tt )")]
        public KeywordResult GetTimeStamp(string variable, string format)
        {
            format = Utils.GetVariablevalueIfExist(format);
            string value = DateTime.Now.ToString(format.Trim(), CultureInfo.InvariantCulture);
            CommonExecutionInfo.SetVariable(variable.Trim(), value);
            KeywordResult result = new KeywordResult(KeywordResults.Pass)
            {
                Output = $"{variable} = {value}"
            };
            return result;
        }

        [GetKeyword]
        [KeywordDescription("Generate time stamp with format yyyyMMdd_HHmmss and save to variable")]
        [KeywordDisplayName("Get Time Stamp")]
        [KeywordParameters("variable", "variable name to save")]
        [SampleScript("Get Time Stamp (${time_stamp})")]
        public KeywordResult GetTimeStamp(string variable)
        {
            return GetTimeStamp(variable, "yyyyMMdd_HHmmss");

        }

        [GetKeyword]
        [KeywordDescription("Generate time difference of the two variables and save to variable")]
        [KeywordDisplayName("Get Time Difference")]
        [KeywordParameters("firstvariable", "firstvariable to comapare")]
        [KeywordParameters("secondvariable", "second variable to compare")]
        [KeywordParameters("variable", "variable name to save")]
        [SampleScript("Get Time Difference (2:48:25,2:54:25,${c})")]
        public KeywordResult GetTimeDifference(string firstDate, string secondDate, string saveTo)
        {
            firstDate = Utils.GetVariablevalueIfExist(firstDate);
            secondDate = Utils.GetVariablevalueIfExist(secondDate);
            DateTime secoundresults;
            DateTime firstresults;
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                firstresults = DateTime.Parse(firstDate.Trim(), CultureInfo.InvariantCulture);
                secoundresults = DateTime.Parse(secondDate.Trim(), CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                result = new KeywordResult(KeywordResults.Error);
                result.Output = "Cannot convert given string into DateTime Format";
                Logger.Error(result.Output, ex);
                return result;
            }
            try
            {
                string value = secoundresults.Subtract(firstresults).ToString();
                CommonExecutionInfo.SetVariable(saveTo.Trim(), value);
                result = new KeywordResult(KeywordResults.Pass)
                {
                    Output = $"{saveTo} = {value}"
                };
            }
            catch (Exception ex)
            {
                result = new KeywordResult(KeywordResults.Error);
                result.Output = "Failed to get the DateTime difference";
                Logger.Error(result.Output, ex);
                return result;
            }
            return result;

        }

        [GetKeyword]
        [KeywordDescription("Generate random number with range between min and max")]
        [KeywordDisplayName("Get Random Number")]
        [KeywordParameters("min", "Minium range of random number")]
        [KeywordParameters("max", "Maximum range of random number")]
        [KeywordParameters("variable", "variable name to save")]
        [SampleScript("Get Random Number (100,10000,${random_last})")]
        public KeywordResult GetRandomNumber(string min, string max, string variable)
        {
            min = Utils.GetVariablevalueIfExist(min);
            max = Utils.GetVariablevalueIfExist(max);
            Random r = new Random();
            int randomNumber = r.Next(Int32.Parse(min), Int32.Parse(max) + 1);
            CommonExecutionInfo.SetVariable(variable.Trim(), randomNumber.ToString());
            KeywordResult result = new KeywordResult(KeywordResults.Pass)
            {
                Output = $"{variable} = {randomNumber}"
            };
            return result;
        }

        [GetKeyword]
        [KeywordDescription("Calculate and save result to variable")]
        [KeywordDisplayName("Calculate")]
        [KeywordParameters("formular", "formular to calculate (ex. 2+3)")]
        [KeywordParameters("variable", "variable name to save")]
        [SampleScript("Calculate (12+1,${count})")]
        public KeywordResult Calculate(string formular, string variable)
        {
            formular = Utils.GetVariablevalueIfExist(formular);
            string value = new System.Data.DataTable().Compute(formular, null).ToString();
            CommonExecutionInfo.SetVariable(variable.Trim(), value);
            KeywordResult result = new KeywordResult(KeywordResults.Pass)
            {
                Output = $"{variable} = {value}"
            };
            return result;
        }

        [GetKeyword]
        [KeywordDescription("Remove white space at the variable value")]
        [KeywordDisplayName("Trim")]
        [KeywordParameters("variable", "variable name to trim")]
        [SampleScript("Trim (${TestRunID})")]
        public KeywordResult Trim(string variable)
        {
            string value = CommonExecutionInfo.GetVariable(variable.Trim());
            value = value.Trim();
            CommonExecutionInfo.SetVariable(variable.Trim(), value);
            KeywordResult result = new KeywordResult(KeywordResults.Pass)
            {
                Output = $"{variable} = {value}"
            };
            return result;
        }

        [GetKeyword]
        [KeywordDescription("Remove given leading and trailing character at the variable value")]
        [KeywordDisplayName("Trim")]
        [KeywordParameters("variable", "variable name to trim")]
        [KeywordParameters("trimChar", "trim character")]
        [SampleScript("Trim (${TestRunID},R)")]
        public KeywordResult Trim(string variable, string trimChar)
        {
            trimChar = Utils.GetVariablevalueIfExist(trimChar);
            string value = CommonExecutionInfo.GetVariable(variable.Trim());
            value = value.Trim(trimChar.ToCharArray());
            CommonExecutionInfo.SetVariable(variable.Trim(), value);
            KeywordResult result = new KeywordResult(KeywordResults.Pass)
            {
                Output = $"{variable} = {value}"
            };
            return result;
        }


        [GetKeyword]
        [KeywordDescription("Replace string oldValue to newValue in given variable value")]
        [KeywordDisplayName("Replace")]
        [KeywordParameters("variable", "variable name to replace")]
        [KeywordParameters("oldValue", "find value")]
        [KeywordParameters("newValue", "replace value")]
        [SampleScript("Replace (${FilePath},\\UI_tests,\\UI_tests\\Python_files\\api_ui.py)")]
        public KeywordResult Replace(string variable, string oldValue, string newValue)
        {
            oldValue = Utils.GetVariablevalueIfExist(oldValue);
            newValue = Utils.GetVariablevalueIfExist(newValue);
            string value = CommonExecutionInfo.GetVariable(variable.Trim());
            value = value.Replace(oldValue, newValue);
            CommonExecutionInfo.SetVariable(variable.Trim(), value);
            KeywordResult result = new KeywordResult(KeywordResults.Pass)
            {
                Output = $"{variable} = {value}"
            };
            return result;
        }

        [GetKeyword]
        [KeywordDisplayName("Set Variable")]
        [KeywordDescription("Assing value to variable. This keyword only works with dynamic variable.")]
        [KeywordParameters("variable", "name of variable")]
        [KeywordParameters("value", "value of variable")]
        [SampleScript("Set Variable (${FilePath},C:\\Users\\ReVi345\\Downloads\\675)")]
        public KeywordResult SetVariable(string variable, string value)
        {
            value = Utils.GetVariablevalueIfExist(value);

            CommonExecutionInfo.SetVariable(variable.Trim(), value.Trim());
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Assert last repeat's result with given condition.\r\nPass if condition is met, else fail.")]
        [KeywordDisplayName("Assert Last Repeat")]
        [KeywordParameters("result", "Result to check. Can be Pass, Fail and Error")]
        [KeywordParameters("condition", ">Assert condition. Can be <, >, =, <= and >= with numbers. (ex. >3)")]
        [SampleScript("Assert Last Repeat(PASS,>990)")]
        public KeywordResult AssertLastRepeat(string result, string condition)
        {
            int checkCount = 0;
            result = result.Trim();
            condition = condition.Trim();
            switch(result.ToUpper())
            {
                case "PASS":
                    checkCount = LastRepeatResult.Item1;
                    break;
                case "FAIL":
                    checkCount = LastRepeatResult.Item2;
                    break;
                case "ERROR":
                    checkCount = LastRepeatResult.Item3;
                    break;
                default:
                    KeywordResult error = new KeywordResult(KeywordResults.Error);
                    error.Output = $"{result} is unknown result type.";
                    return error;
            }

            string conditionType = string.Empty;
            int conditionValue = 0;
            Regex regex = new Regex("[0-9]+$");
            if(regex.IsMatch(condition))
            {
                conditionValue = int.Parse(regex.Match(condition).Value);
            }
            else
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"Can not find threshold number in the conditoon : {condition}";
                return error;
            }

            conditionType = condition.Replace(conditionValue.ToString(), "").Trim();

            KeywordResult kr;

            bool conditionMet = false;
            switch (conditionType)
            {
                case ">":
                    if (checkCount > conditionValue)
                    {
                        conditionMet = true;
                    }
                    break;
                case "<":
                    if (checkCount < conditionValue)
                    {
                        conditionMet = true;
                    }
                    break;
                case ">=":
                    if (checkCount >= conditionValue)
                    {
                        conditionMet = true;
                    }
                    break;
                case "<=":
                    if (checkCount <= conditionValue)
                    {
                        conditionMet = true;
                    }
                    break;
                case "=":
                    if (checkCount == conditionValue)
                    {
                        conditionMet = true;
                    }
                    break;
                default:
                    KeywordResult error = new KeywordResult(KeywordResults.Error);
                    error.Output = $"{conditionType} is unknown type.";
                    return error;
            }

            if(conditionMet)
            {
                kr = new KeywordResult(KeywordResults.Pass);
            }
            else
            {
                kr = new KeywordResult(KeywordResults.Fail);
            }
            kr.Output = $"{result} count of last repeat is {checkCount}.";

            return kr;
        }

        [KeywordDescription("Wait until remote executions are completed")]
        [KeywordDisplayName("Wait For Remote Complete")]
        [KeywordParameters("RemoteIDs", "Comma seperated remote id (ex. Remote01, Remote02)")]
        [SampleScript("Will Add in Next Release")]
        public KeywordResult WaitForRemoteComplete(string[] remoteIds)
        {
            KeywordResult keywordResult = new KeywordResult(KeywordResults.Pass);
            foreach(string remoteId in remoteIds)
            {
                string targetId = remoteId.Trim();
                RemoteExecutor remoteExecutor = (RemoteExecutor)CommonExecutionInfo.GetRemoteExecutor(targetId);
                remoteExecutor.RunTask.Wait();
                CommonExecutionInfo.SetVariable("${" + targetId + "_Result}", remoteExecutor.RunTask.Result);
            }
            return keywordResult;
        }

        [KeywordDescription("Run command and get output within given timeout")]
        [KeywordDisplayName("Run Command")]
        [KeywordParameters("command", "Command to run")]
        [KeywordParameters("saveTo", "Variable name to save output")]
        [KeywordParameters("timeOutInSec", "Timeout to run command in seconds")]
        [SampleScript("Run Command (cmd.exe /c start hp.smart.exe,${temp},10)")]
        public KeywordResult RunCommand(string command, string saveTo, string timeOutInSec)
        {
            string executable = command.Split(' ')[0];
            string argument = string.Empty;
            if(command.Length > executable.Length)
            {
                argument = command.Substring(executable.Length + 1, command.Length - executable.Length - 1);
            }

            if(!int.TryParse(timeOutInSec, out int iTimeout))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Timeout should be a number");
                return error;
            }

            Process p = new Process();
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = executable;
                startInfo.Arguments = argument;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.StandardErrorEncoding = Encoding.UTF8;
                startInfo.StandardOutputEncoding = Encoding.UTF8;
                p.StartInfo = startInfo;
                p.Start();
                p.WaitForExit(iTimeout * 1000);

                if(!p.HasExited)
                {
                    p.Kill();
                }
            }
            catch(Exception ex)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "Exception during run command";
                fail.AdditionalInfo = p.StandardError.ReadToEnd();
                Logger.Error(fail.Output, ex);
                return fail;
            }

            string output = p.StandardOutput.ReadToEnd();
            CommonExecutionInfo.SetVariable(saveTo, output);

            KeywordResults processResult = KeywordResults.Pass;
            if(p.ExitCode !=0)
            {
                processResult = KeywordResults.Fail;
                output = p.StandardError.ReadToEnd();
                CommonExecutionInfo.SetVariable(saveTo, output);
            }

            KeywordResult result = new KeywordResult(processResult, output);
            return result;

        }

        [KeywordDescription("Run command and get output. This keyword wait until process is done. Need to avoid process that never ends.")]
        [KeywordDisplayName("Run Command")]
        [KeywordParameters("command", "Command to run")]
        [KeywordParameters("saveTo", "Variable name to save output")]
        [SampleScript("Run Command (cmd.exe /c python ${FilePath},${Null})")]
        public KeywordResult RunCommand(string command, string saveTo)
        {
            string executable = command.Split(' ')[0];
            string argument = string.Empty;
            if (command.Length > executable.Length)
            {
                argument = command.Substring(executable.Length + 1, command.Length - executable.Length - 1);
            }
            Process p = new Process();
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = executable;
                startInfo.Arguments = argument;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.StandardErrorEncoding = Encoding.UTF8;
                startInfo.StandardOutputEncoding = Encoding.UTF8;
                p.StartInfo = startInfo;
                p.Start();
                p.WaitForExit();
            }
            catch (Exception ex)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "Exception during run command";
                fail.AdditionalInfo = p.StandardError.ReadToEnd();
                Logger.Error(fail.Output, ex);
                return fail;
            }

            string output = p.StandardOutput.ReadToEnd();
            CommonExecutionInfo.SetVariable(saveTo, output);

            KeywordResults processResult = KeywordResults.Pass;
            if (p.ExitCode != 0)
            {
                processResult = KeywordResults.Fail;
                output = p.StandardError.ReadToEnd();
                CommonExecutionInfo.SetVariable(saveTo, output);
            }

            KeywordResult result = new KeywordResult(processResult, output);
            return result;

        }

        [KeywordDescription("Run command as administrator and get output within given timeout")]
        [KeywordDisplayName("Run Command As Admin")]
        [KeywordParameters("command", "Command to run")]
        [KeywordParameters("saveTo", "Variable name to save output")]
        [KeywordParameters("timeOutInSec", "Timeout to run command in seconds")]
        [SampleScript("Run Command As Admin (start HP smart,${Hp_smart},10)")]
        public KeywordResult RunCommandAsAdmin(string command, string saveTo, string timeOutInSec)
        {
            string executable = command.Split(' ')[0];
            string argument = string.Empty;
            if (command.Length > executable.Length)
            {
                argument = command.Substring(executable.Length + 1, command.Length - executable.Length - 1);
            }
            if (!int.TryParse(timeOutInSec, out int iTimeout))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Timeout should be a number");
                return error;
            }

            Process p = new Process();
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = executable;
                startInfo.Arguments = argument;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.StandardErrorEncoding = Encoding.UTF8;
                startInfo.StandardOutputEncoding = Encoding.UTF8;
                startInfo.Verb = "runas";
                p.StartInfo = startInfo;
                p.Start();
                p.WaitForExit(iTimeout * 1000);

                if (!p.HasExited)
                {
                    p.Kill();
                }
            }
            catch (Exception ex)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "Exception during run command";
                Logger.Error(fail.Output, ex);
                return fail;
            }

            string output = p.StandardOutput.ReadToEnd();
            CommonExecutionInfo.SetVariable(saveTo, output);

            KeywordResults processResult = KeywordResults.Pass;
            if (p.ExitCode != 0)
            {
                processResult = KeywordResults.Fail;
            }

            KeywordResult result = new KeywordResult(processResult, output);
            return result;

        }

        [KeywordDescription("Run command as administrator and get output. This keyword wait until process is done. Need to avoid process that never ends.")]
        [KeywordDisplayName("Run Command As Admin")]
        [KeywordParameters("command", "Command to run")]
        [KeywordParameters("saveTo", "Variable name to save output")]
        [SampleScript("Run Command As Admin (taskkill /F /IM HP.myHP.exe,${buff})")]
        public KeywordResult RunCommandAsAdmin(string command, string saveTo)
        {
            string executable = command.Split(' ')[0];
            string argument = string.Empty;
            if (command.Length > executable.Length)
            {
                argument = command.Substring(executable.Length + 1, command.Length - executable.Length - 1);
            }
            Process p = new Process();
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = executable;
                startInfo.Arguments = argument;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.StandardErrorEncoding = Encoding.UTF8;
                startInfo.StandardOutputEncoding = Encoding.UTF8;
                startInfo.Verb = "runas";
                p.StartInfo = startInfo;
                p.Start();
                p.WaitForExit();
            }
            catch (Exception ex)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "Exception during run command";
                Logger.Error(fail.Output, ex);
                return fail;
            }

            string output = p.StandardOutput.ReadToEnd();
            CommonExecutionInfo.SetVariable(saveTo, output);

            KeywordResults processResult = KeywordResults.Pass;
            if (p.ExitCode != 0)
            {
                processResult = KeywordResults.Fail;
            }

            KeywordResult result = new KeywordResult(processResult, output);
            return result;

        }

        [KeywordDescription("Runs a command within a given timeout and marks result as FAIL only if an actual error or installation failure is detected.")]
        [KeywordDisplayName("Run Command And Ignore Exit Code")]
        [KeywordParameters("command", "Command to run (e.g., cmd.exe /c <your_script>.bat)")]
        [KeywordParameters("saveTo", "Variable name to save the command output")]
        [KeywordParameters("timeOutInSec", "Timeout to run command in seconds")]
        [SampleScript("Run Command And Ignore Exit Code (cmd.exe /c ${SCRIPT_FOLDER}\\Install.bat ${Workpath_path} ${hpk_file} ${output_name},${output_var},1200)")]
        public KeywordResult RunCommandAndIgnoreExitCode(string command, string saveTo, string timeOutInSec)
        {
            // Validate timeout parameter
            if (!int.TryParse(timeOutInSec, out int timeout))
            {
                return new KeywordResult(KeywordResults.Error, "Timeout should be a valid number (in seconds).");
            }
            Process process = null;
            StringBuilder fullOutput = new StringBuilder();
            DateTime startTime = DateTime.Now;

            try
            {
                fullOutput.AppendLine($"Start Time: {startTime:yyyy-MM-dd HH:mm:ss.fff}");

                // Parse executable and arguments
                string executable = command.Split(' ')[0];
                string arguments = command.Length > executable.Length
                    ? command.Substring(executable.Length + 1)
                    : string.Empty;

                // Configure process
                process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = executable,
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8
                    }
                };

                // Attach event handlers
                process.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        fullOutput.AppendLine(e.Data);
                        Logger.Trace(e.Data);
                    }
                };

                process.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        fullOutput.AppendLine("[ERROR] " + e.Data);
                        Logger.Warn(e.Data);
                    }
                };

                // Start the process
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Wait for process exit
                bool exited = process.WaitForExit(timeout * 1000);
                DateTime endTime = DateTime.Now;
                TimeSpan elapsed = endTime - startTime;

                fullOutput.AppendLine($"End Time: {endTime:yyyy-MM-dd HH:mm:ss.fff} | Elapsed Time: {elapsed}");
                string collectedOutput = fullOutput.ToString();

                // Store result to variable
                CommonExecutionInfo.SetVariable(saveTo, collectedOutput);

                // Handle timeout case
                if (!exited)
                {
                    Logger.Warn($"Timeout reached after {timeout}s. Attempting to kill process...");

                    try
                    {
                        process.Kill();
                    }
                    catch (Exception killEx)
                    {
                        Logger.Warn($"Failed to kill process: {killEx.Message}");
                    }

                    // Wait a few seconds to flush output
                    process.WaitForExit(3000);

                    string finalOutput = fullOutput.ToString();
                    bool successMarkerFound = finalOutput.Contains("100%") || finalOutput.Contains("100 %");

                    if (successMarkerFound)
                    {
                        Logger.Trace("Process timed out, but installation completed successfully (100% marker found).");
                        return new KeywordResult(KeywordResults.Pass,
                            $"Process timed out after {timeout}s but installation completed successfully.\n{finalOutput}");
                    }

                    return new KeywordResult(KeywordResults.Fail,
                        $"Command timed out after {timeout}s and installation not confirmed.\n{finalOutput}");
                }

                // Process exited normally
                string output = fullOutput.ToString();
                bool containsError = output.ToLower().Contains("error") || output.ToLower().Contains("failed");
                bool successMarker = output.Contains("100%") || output.Contains("100 %");

                if (containsError)
                {
                    return new KeywordResult(KeywordResults.Fail, $"Error found in output.\n{output}");
                }

                if (successMarker)
                {
                    return new KeywordResult(KeywordResults.Pass, $"Installation completed successfully (100% found).\n{output}");
                }

                // If process exit code non-zero but no explicit error found
                if (process.ExitCode != 0)
                {
                    Logger.Warn($"Process exited with non-zero code {process.ExitCode}, but no error detected. Treating as PASS.");
                }

                // Default: PASS
                return new KeywordResult(KeywordResults.Pass, output);
            }
            catch (Exception ex)
            {
                string errorMsg = $"Exception during command execution: {ex.Message}";
                Logger.Error(errorMsg, ex);
                CommonExecutionInfo.SetVariable(saveTo, errorMsg);
                return new KeywordResult(KeywordResults.Fail, errorMsg);
            }
            finally
            {
                try
                {
                    process?.Dispose();
                }
                catch (Exception disposeEx)
                {
                    Logger.Warn($"Failed to dispose process properly: {disposeEx.Message}");
                }

                // Always record total output safely
                try
                {
                    string finalOutput = fullOutput.ToString();
                    CommonExecutionInfo.SetVariable(saveTo, finalOutput);
                }
                catch (Exception ex)
                {
                    Logger.Warn($"Failed to set final output variable: {ex.Message}");
                }
            }
        }
        [KeywordDescription("Run command in interactive mode (supports password input). Use this keyword before using 'Send Console Password' Keyword.")]
        [KeywordDisplayName("Run Command Interactive")]
        [KeywordParameters("command", "Command to run")]
        [SampleScript("Run Command Interactive (ssh root@146.205.4.221)\nSend Console Password (myroot)")]
        public KeywordResult RunCommandInteractive(string command)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(command))
                {
                    return new KeywordResult(
                        KeywordResults.Fail,
                        "Command cannot be empty. Please provide a valid command."
                    );
                }
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c " + command,
                    UseShellExecute = true,     // REQUIRED for console
                    CreateNoWindow = false      // REQUIRED for password prompt
                };

                Process process = new Process
                {
                    StartInfo = startInfo
                };

                process.Start();

                return new KeywordResult(
                    KeywordResults.Pass,
                    "Command started in interactive mode. Please enter password in the opened console."
                );
            }
            catch (Exception ex)
            {
                Logger.Error("Exception during Run Command Interactive", ex);
                return new KeywordResult(KeywordResults.Fail, ex.Message);
            }
        }

        [KeywordDescription("Send password to the active console window. This keyword should be used only after executing 'Run Command Interactive' Keyword.")]
        [KeywordDisplayName("Send Console Password")]
        [KeywordParameters("password", "Password to send")]
        [SampleScript("Run Command Interactive (ssh root@146.205.4.221)\nSend Console Password (myroot)")]
        public KeywordResult SendConsolePassword(string password)
        {
            try
            {
                // Give time for prompt to appear
                Thread.Sleep(2000);

                // Send password + Enter
                SendKeys.SendWait(password);
                SendKeys.SendWait("{ENTER}");

                return new KeywordResult(
                    KeywordResults.Pass,
                    "Password sent to console window"
                );
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to send password", ex);
                return new KeywordResult(KeywordResults.Fail, ex.Message);
            }
        }

        [KeywordDescription("Run given keyword")]
        [KeywordDisplayName("Run Keyword")]
        [KeywordParameters("keyword", "Keyword to run. Can include argument. Ex) Fleet.Send File(test.prn)")]
        [SampleScript("Run Keyword (Web.OpenWithChrome(www.hp.com))")]
        public KeywordResult RunKeyword(string keyword)
        {
            Statement statement = null;
            try
            {
                statement = Parser.ParseStatemet(keyword);
            }
            catch(Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Keyword parsing error";
                error.AdditionalInfo = ex.ToString();
                Logger.Error(error.Output, ex);
                return error;
            }
            Console.WriteLine("");
            statement.Run(Statement._lastTestDataManager, Statement._lastRepeatCount, Statement._lastStackLevel+1, Statement._lastArguments);
            return statement.Result;
        }

        [KeywordDescription("Converts the string to uppercase")]
        [KeywordDisplayName("ConvertToUpperCase")]
        [KeywordParameters("oldValue","value which needs to be converted.  The value can be direct string or from a variable")]
        [KeywordParameters("newvalue","Variable name which stores the converted strings")]
        [SampleScript("ConvertToUpperCase(somestring,${variableToStore})")]
        public KeywordResult ConvertToUpperCase(string oldValue, string newValue)
        {
            try
            {
                oldValue = Utils.GetVariablevalueIfExist(oldValue);
                string caseChangedValue = oldValue.ToUpper();
                CommonExecutionInfo.SetVariable(newValue, caseChangedValue);
                KeywordResult result = new KeywordResult(KeywordResults.Pass)
                {
                    Output = $"{newValue} = {caseChangedValue}"
                };
                return result;
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Invalid string value";
                error.AdditionalInfo = ex.ToString();
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        [KeywordDescription("Converts the string to Lowercase")]
        [KeywordDisplayName("ConvertToLowerCase")]
        [KeywordParameters("oldValue", "value which needs to be converted.  The value can be direct string or from a variable")]
        [KeywordParameters("newvalue", "Variable name which stores the converted strings")]
        [SampleScript("ConvertToLowerCase(SomeString,${variableToStore})")]
        public KeywordResult ConvertToLowerCase(string oldValue,string newValue)
        {
            try
            {
                oldValue = Utils.GetVariablevalueIfExist(oldValue);
                string caseChangedValue = oldValue.ToLower();

                CommonExecutionInfo.SetVariable(newValue, caseChangedValue);
                KeywordResult result = new KeywordResult(KeywordResults.Pass)
                {
                    Output = $"{newValue} = {caseChangedValue}"
                };
                return result;
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Invalid string value";
                error.AdditionalInfo = ex.ToString();
                Logger.Error(error.Output, ex);
                return error;
            }

        }

        [GetKeyword]
        [KeywordDescription("convert time stamp with given format and save to variable.  " +
                    "BuiltIn.Get Time Stamp(${orderDate1}, MM/dd/yy H:mm.ss)\r\n    " +
                    "BuiltIn.ConvertTimeStampFormat (${orderDate1},MM/dd/yy H:mm.ss,MMM dd \\,yyyy)")]
        [KeywordDisplayName("ConvertTimeStampFormat")]
        [KeywordParameters("variable", "variable name where the date time format is saved")]
        [KeywordParameters("srcFormat", "the source format in which the date time")]
        [KeywordParameters("targetFormat", "the target date format required to store in the variable")]
        [SampleScript("ConvertTimeStampFormat(${formattedDate},MM/dd/yy H:mm.ss,MMM dd \\,yyyy)")]
        public KeywordResult ConvertTimeStampFormat(string variable, string srcFormat, string targFormat)
        {
            srcFormat = Utils.GetVariablevalueIfExist(srcFormat);
            string value = CommonExecutionInfo.GetVariable(variable);
            DateTime parsedDate;
            DateTime.TryParseExact(value, srcFormat, null, DateTimeStyles.None, out parsedDate);
            string convertedValue = parsedDate.ToString(targFormat, CultureInfo.InvariantCulture);

            CommonExecutionInfo.SetVariable(variable.Trim(), convertedValue);
            KeywordResult result = new KeywordResult(KeywordResults.Pass)
            {
                Output = $"{variable} = {convertedValue}"
            };
            return result;
        }
        [KeywordDescription("Append content to the given file")]
        [KeywordDisplayName("Append To File")]
        [KeywordParameters("filePath", "value to append into file")]
        [SampleScript("Append To File (C:\\Users\\ReVi345\\Downloads\\675\\sample.txt,abcdefgh)")]
        public KeywordResult AppendToFile(string filePath, string value)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + "SaveToFile.txt";
            }
            filePath = Utils.GetAbsolutePath(filePath, _outputDir);
            try
            {
                File.AppendAllText(filePath, Environment.NewLine + value);
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error during write value to file.";
                error.AdditionalInfo = ex.ToString();
                Logger.Error("Error during write value to file.", ex);
                return error;
            }
            return new KeywordResult(KeywordResults.Pass);
        }
        #endregion

        [GetKeyword]
        [KeywordDescription("Calculate and assign result to variable")]
        [KeywordDisplayName("Calculate And Assign")]
        [KeywordParameters("formular", "formular to calculate (ex. 2+3)")]
        [KeywordParameters("variable", "variable name to save")]
        [SampleScript("Calculate And Assign(2+3,${result})")]
        public KeywordResult CalculateAndAssign(string formular, string variable)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                while (varRegex.IsMatch(formular))
                {
                    int variableEndIndex = formular.Trim().IndexOf('}');
                    int variableStartIndex = formular.Trim().IndexOf('$');
                    string variablename = formular.Substring(variableStartIndex, variableEndIndex - variableStartIndex);

                    if (!variablename.Contains("}"))
                    {
                        variablename = variablename + "}";
                    }

                    string variablevalue = CommonExecutionInfo.GetVariable(variablename);
                    formular = formular.Replace(variablename, variablevalue);
                }
                string value = new System.Data.DataTable().Compute(formular, null).ToString();
                CommonExecutionInfo.SetVariable(variable.Trim(), value);
                result.Output = $"{variable} = {value}";
            }
            catch(Exception ex)
            {
                Logger.Debug("Exception in CalculateAndAssign : " + ex.Message);
                result= new KeywordResult(KeywordResults.Fail);
                result.Output=ex.Message;
            }
            return result;
        }


        [KeywordDisplayName("ReAssign Value To Variable")]
        [KeywordDescription("ReAssingn value to variable.")]
        [KeywordParameters("variable", "name of variable")]
        [KeywordParameters("value", "value of variable or a variable which contains value")]
        [SampleScript("ReAssign Value To Variable(${variable},newvalue)")]
        public KeywordResult ReAssignValueToVariable(string variable, string value)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (varRegex.IsMatch(value))
                {
                    if (value.StartsWith("$"))
                    {
                        value = CommonExecutionInfo.GetVariable(value);
                    }
                    else
                    {
                        int variableEndIndex = value.IndexOf('}');
                        string variablename = value.Substring(value.IndexOf('$'), variableEndIndex - 1);

                        string variablevalue = CommonExecutionInfo.GetVariable(variablename);

                        string formular = value.Replace(variablename, variablevalue);
                        value = new System.Data.DataTable().Compute(formular, null).ToString();
                    }
                }
                else
                {
                    if (Regex.IsMatch(value, @"^[\d\s\+\-\*/\(\)]+$"))
                    {
                        value = new System.Data.DataTable().Compute(value, null).ToString();
                    }
                }
                CommonExecutionInfo.SetVariable(variable.Trim(), value.Trim());
                result.Output = $"{variable} = {value}";
            }
            catch(Exception ex)
            {
                Logger.Debug("Exception in CalculateAndAssign : " + ex.Message);
                result = new KeywordResult(KeywordResults.Fail);
                result.Output = ex.Message;
            }
            return result;
        }


        [KeywordDescription("Retrieve a substring of the variable value and store it in a new variable")]
        [KeywordDisplayName("Substring")]
        [KeywordParameters("sourceVariable", "value which needs to be get substring")]
        [KeywordParameters("startIndex", "Index position of the word")]
        [KeywordParameters("length", "Lenght of Substring")]
        [KeywordParameters("destinationVariable", "Variable name which stores the sub strings")]
        [SampleScript("Substring (${TestRunID}, ${SubstrVariable}, 0, 5)")]
        public KeywordResult Substring(string sourceVariable, string startIndex, string length, string destinationVariable)
        {
            int startindex = Convert.ToInt32(startIndex);
            int lenght = Convert.ToInt32(length);
            string substring = sourceVariable.Substring(startindex, Math.Min(lenght, sourceVariable.Length - startindex));

            CommonExecutionInfo.SetVariable(destinationVariable, substring);

            KeywordResult result = new KeywordResult(KeywordResults.Pass)
            {
                Output = $"Substring of {sourceVariable} stored in {destinationVariable}: {substring}"
            };
            return result;
        }
        [KeywordDescription("Count the number of characters in the variable value")]
        [KeywordDisplayName("Count")]
        [KeywordParameters("variable", "variable name to count characters")]
        [SampleScript("Count (${TestRunID})")]
        public KeywordResult Count(string variable)
        {
            string value = CommonExecutionInfo.GetVariable(variable);
            int characterCount = variable.Length;

            KeywordResult result = new KeywordResult(KeywordResults.Pass)
            {
                Output = $"Number of characters in {variable}: {characterCount}"
            };
            return result;
        }
        [KeywordDescription("Retrieve a particular word from the variable value and store it in a new variable")]
        [KeywordDisplayName("SplitAndStoreWord")]
        [KeywordParameters("sourceVariable", "value which needs to be get substring")]
        [KeywordParameters("delimiter", " Identification to separate the word")]
        [KeywordParameters("wordIndex", "Index of the word")]
        [KeywordParameters("destinationVariable", "Variable name which stores the sub strings")]
        [SampleScript("SplitAndStoreWord (${Sentence}, ' ', 0, ${FirstWord})")]
        public KeywordResult SplitAndStoreWord(string sourceVariable, string delimiter, string wordIndex, string destinationVariable)
        {
            if (delimiter == "\"\"")
                delimiter = " ";
            string[] words = sourceVariable.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);

            int index = Convert.ToInt32(wordIndex);

            CommonExecutionInfo.SetVariable(destinationVariable, words[index]);

            KeywordResult result = new KeywordResult(KeywordResults.Pass)
            {
                Output = $"Word at index {index} from {sourceVariable} stored in {destinationVariable}: {words[index]}"
            };
            return result;
        }
        [KeywordDescription("Send an email")]
        [KeywordDisplayName("SendEmail")]
        [KeywordParameters("toEmail", "Address to send email")]
        [KeywordParameters("body", "message to send ")]
        [SampleScript("SendEmail('vijay-kumar.reddy-k@hp.com', 'Please find the attached report.')")]
        public KeywordResult SendEmail(string toEmail, string body)
        {

            try
            {
                SendEmail(toEmail, body,false,false);
                return new KeywordResult(KeywordResults.Pass)
                {
                    Output = "Email sent successfully!"
                };
            }
            catch (Exception ex)
            {
                return new KeywordResult(KeywordResults.Fail)
                {
                    Output = $"Error sending email: {ex.Message}"
                };
            }

        }

        [KeywordDescription("Send an email with report summary")]
        [KeywordDisplayName("Send Email With Report Summary")]
        [KeywordParameters("toEmail", "Address to send email")]
        [KeywordParameters("body", "message to send ")]
        [SampleScript("Send Email With Report Summary('vijay-kumar.reddy-k@hp.com', 'Please find the attached report.')")]
        public KeywordResult SendEmailWithReportSummary(string toEmail, string body)
        {

            try
            {
                CommonExecutionInfo.SendMailwithReportSummary = true;
                CommonExecutionInfo.SetVariable("mailbody", body);
                CommonExecutionInfo.SetVariable("toEmail", toEmail);
                return new KeywordResult(KeywordResults.Pass)
                {
                    Output = "Email sent successfully!"
                };
            }
            catch (Exception ex)
            {
                return new KeywordResult(KeywordResults.Fail)
                {
                    Output = $"Error sending email: {ex.Message}"
                };
            }

        }


        public void SendEmail(string toEmail, string body,bool isAttachment,bool isReportSummary)
        {
            // Validate if the machine is in HP network. If not, do not send email.
            if (!IsInHPNetwork())
            {
                Console.WriteLine("System is not in a HP network (15.x.x.x or 16.x.x.x). Email cannot be sent.");
                return;
            }

            // Split the recipient email addresses
            string[] emailAddresses = toEmail.Split(',');

            // Set up the email address
            MailAddress fromAddress = new MailAddress("siva.kumar@hp.com");

            // Create the email message
            var smtp = new SmtpClient
            {
                Host = "smtp3.glb1.hp.com",
                Port = 25,
                EnableSsl = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = true,
            };

            using (var message = new MailMessage())
            {
                message.From = fromAddress;
                message.Body = body;
                message.Subject = "GFriend2-Test-Email";
                if(isReportSummary)
                {
                    message.Body += CommonExecutionInfo.GetVariable("ReportSummary");
                    message.IsBodyHtml = true;
                }
                if(isAttachment)
                {
                    // Attach the report
                    Attachment attachment = new Attachment(CommonExecutionInfo.GetVariable("ReportPath"));
                    message.Attachments.Add(attachment);

                }

                // Add multiple recipients
                foreach (var email in emailAddresses)
                {
                    message.To.Add(new MailAddress(email.Trim()));
                }

                try
                {
                    smtp.Send(message);
                    Console.WriteLine("Email sent successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending email: {ex.Message}");
                }
            }
        }


        /// <summary>
        /// This method checks whether the machine is in HP network or not (LR0,LR1)
        /// Because, the email settings works when the machine is in HP network. 
        /// Else we are ignoring the mail send.
        /// </summary>
        /// <returns></returns>
        private bool IsInHPNetwork()
        {
            try
            {
                // Get the local system's IP addresses
                string hostName = Dns.GetHostName();
                var ipAddresses = Dns.GetHostAddresses(hostName);

                foreach (var ip in ipAddresses)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        string ipString = ip.ToString();
                        if (ipString.StartsWith("15.") || ipString.StartsWith("16."))
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {
                // Handle any exceptions related to retrieving IP addresses
                return false;
            }

            return false;
        }

        [KeywordDescription("Gets System Information.")]
        [KeywordDisplayName("Get System Information")]
        [KeywordParameters("variable", "Variable name to save system information")]
        [SampleScript("Get System Information(variableName)")]
        public KeywordResult GetSystemInformation(string variable)
        {
            try
            {
                // PowerShell script to get CPU information
                string sysinfoscript = "Get-CimInstance -ClassName Win32_Processor | ForEach-Object { \\\"$($_.Name) $($_.MaxClockSpeed / 1000) GHz\\\" } | Select-Object -Unique -First 1";
                string cpuInfo = ExecutePowerShell(sysinfoscript).Trim();
                CommonExecutionInfo.SetVariable(variable.Trim(), cpuInfo); // Save result to variable

                return new KeywordResult(KeywordResults.Pass)
                {
                    Output = $"System Information: {cpuInfo}"
                };
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, $"An error occurred: {ex.Message}");
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        [KeywordDescription("Gets the total physical RAM in GB.")]
        [KeywordDisplayName("Get RAM")]
        [KeywordParameters("variable", "Variable name to save RAM size in GB")]
        [SampleScript("Get RAM(variableName)")]
        public KeywordResult GetRam(string variable)
        {
            try
            {
                // PowerShell script to get total physical RAM in GB
                string ramScript = "[math]::Round((Get-CimInstance -ClassName Win32_ComputerSystem).TotalPhysicalMemory / 1GB, 0)";
                string ramInfo = ExecutePowerShell(ramScript).Trim();

                CommonExecutionInfo.SetVariable(variable.Trim(), "RAM:" + ramInfo + "GB"); // Save result to variable

                return new KeywordResult(KeywordResults.Pass)
                {
                    Output = $"RAM Size: {ramInfo} GB"
                };
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, $"An error occurred: {ex.Message}");
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        [KeywordDescription("Gets the Windows edition and version.")]
        [KeywordDisplayName("Get Windows Version")]
        [KeywordParameters("variable", "Variable name to save Windows edition and version")]
        [SampleScript("Get Windows(variableName)")]
        public KeywordResult GetWindowsVersion(string variable)
        {
            try
            {
                // PowerShell script to get Windows edition and version
                string winScript = "$OSName = (Get-ComputerInfo).OSName; " +
           "$DisplayVersion = (Get-ItemProperty -Path \\\"HKLM:\\\\SOFTWARE\\\\Microsoft\\\\Windows NT\\\\CurrentVersion\\\" -Name DisplayVersion).DisplayVersion; " +
           "\\\"$OSName $DisplayVersion\\\"";
                string windowsInfo = ExecutePowerShell(winScript).Trim();

                CommonExecutionInfo.SetVariable(variable.Trim(), windowsInfo); // Save result to variable

                return new KeywordResult(KeywordResults.Pass)
                {
                    Output = $"Windows Version: {windowsInfo}"
                };
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, $"An error occurred: {ex.Message}");
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        [KeywordDescription("Gets the Printer details - printer Name | driver details | firmware version | printer model")]
        [KeywordDisplayName("Get Printer Details")]
        [KeywordParameters("ipAddress", "IP Address of the printer.")]
        [KeywordParameters("password", "Password of the printer.")]
        [SampleScript("Get Printer Details(146.205.4.183,rdl@12345)")]
        public KeywordResult GetPrinterDetails(string ipAddress, string password)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                IDevice device = null;

                if (!string.IsNullOrEmpty(ipAddress) || !string.IsNullOrEmpty(password))
                {
                    string deviceType = GetFamily(ipAddress, password, out device);


                    //Printer Model Name
                    Dictionary<string,string> printerDetails = GetPrinterModelAndFirmwareDetails(ipAddress,device,deviceType);
                    string modelName = String.IsNullOrEmpty(printerDetails["MODELNAME"]) ? "" : printerDetails["MODELNAME"];
                    string modelNumber = String.IsNullOrEmpty(printerDetails["MODELNUMBER"]) ? "" : printerDetails["MODELNUMBER"].Replace("?","");
                    string firmware = String.IsNullOrEmpty(printerDetails["FIRMWARE"]) ? "" : printerDetails["FIRMWARE"].Replace("?", "");
                    string dateCode = String.IsNullOrEmpty(printerDetails["DATEFORMAT"]) ? "" : printerDetails["DATEFORMAT"].Replace("?", "");


                    //printer driver details
                    var allDrivers = GetAllPrinterDriversForIp(ipAddress);
                    StringBuilder driverInfo = new StringBuilder();
                    if (allDrivers == null || !allDrivers.Any())
                    {
                        driverInfo.AppendLine("No drivers Found");
                    }
                    foreach (var driver in allDrivers)
                    {
                        driverInfo.AppendLine($"Port: {driver.portName}");
                        driverInfo.AppendLine($"Driver Name: {driver.driverName}");
                        driverInfo.AppendLine($"Driver Version: {driver.driverVersion}");
                    }

                    result.Output = $"Printer Details : \r\nModelName : {modelName}\r\nDriver Info : {driverInfo} Firmware : {firmware}\r\nModelNumber :{modelNumber}\r\nFirmware Datecode : {dateCode}";
                }
                else
                {
                    return new KeywordResult(KeywordResults.Fail, "IP Address / Password should not be null or empty.Please enter a valid ipAddress / Password");
                }
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, $"An error occurred: {ex.Message}");
                Logger.Error(error.Output, ex);
                return error;
            }
            return result;
        }

        private string GetFamily(string ipAddress,string password,out IDevice device)
        {
            try
            {
                IDevice _device = DeviceFactory.Create(ipAddress, password);
                device = _device;

                if (_device is JediOmniDevice)
                {
                    return "Jedi";
                }
                else if(_device is DuneDevice)
                {
                    return "Dune";
                }
            }
            catch (Exception ex)
            {
                Logger.Trace("Exception in the method GetFamily : " + ex.Message);
                device = null;
                return null;
            }
            return null;
        }

        // Method to fetch all printer drivers for a given IP address
        private List<(string portName, string driverName, string driverVersion)> GetAllPrinterDriversForIp(string ipAddress)
        {
            List<(string portName, string driverName, string driverVersion)> drivers = new List<(string portName, string driverName, string driverVersion)>();
            try
            {
                // Loop through ports 1 to 10 (you can adjust the range as needed)
                for (int i = 0; i <= 10; i++)
                {
                    string portName = i == 0 ? ipAddress : $"{ipAddress}_{i}";

                    // Call method to fetch driver info for this specific port
                    var driverInfo = GetPrinterDriverInfo(portName);

                    if (driverInfo.driverName != "Not Found" && driverInfo.driverVersion != "Not Found")
                    {
                        // If a valid driver is found, add to the list
                        drivers.Add(driverInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in the method GetAllPrinterDriversForIp : ", ex);
            }
            return drivers;
        }
        // Method to fetch driver info for a specific port
        private (string portName, string driverName, string driverVersion) GetPrinterDriverInfo(string portName)
        {
            string driverName = "Not Found";
            string driverVersion = "Not Found";

            try
            {
                // Prepare PowerShell script for this specific port
                string powerShellScript = $@"
$printerPort = '{portName}';
$printer = Get-Printer | Where-Object {{ $_.PortName -eq $printerPort }};
if ($printer) {{
    $driver = Get-PrinterDriver | Where-Object {{ $_.Name -eq $printer.DriverName }};
    if ($driver) {{
        Write-Output $driver.Name;
        Write-Output $driver.DriverVersion;
    }} else {{
        Write-Output 'Driver not found for the printer.';
    }}
}} else {{
    Write-Output 'Printer not found.';
}}";

                // Run the PowerShell script and capture output
                string output = ExecutePowerShell(powerShellScript);

                string[] lines = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                if (lines.Length >= 2)
                {
                    driverName = lines[0].Trim(); // First line is driver name

                    // Parse driver version and convert to long
                    if (long.TryParse(lines[1].Trim(), out long driverVersionLong))
                    {
                        var revision = driverVersionLong & 0xFFFF;
                        var build = (driverVersionLong >> 16) & 0xFFFF;
                        var minor = (driverVersionLong >> 32) & 0xFFFF;
                        var major = (driverVersionLong >> 48) & 0xFFFF;

                        driverVersion = $"{major}.{minor}.{build}.{revision}";
                    }
                    else
                    {
                        driverVersion = "Not Found";
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error fetching printer driver info: {ex.Message}");
            }

            return (portName, driverName, driverVersion);
        }
        // SNMP method to fetch printer name
        private Dictionary<string,string> GetPrinterModelAndFirmwareDetails(string ipAddress,IDevice device,string deviceType)
        {
            Dictionary<string, string> printerDetails = new Dictionary<string, string>();
            try
            {
                string printerModelNameoid = "1.3.6.1.2.1.25.3.2.1.3.1";
                string printerModelNumberoid = "1.3.6.1.4.1.11.2.3.9.4.2.1.1.3.1.0";
                string printerfirmwareoid = "1.3.6.1.4.1.11.2.3.9.4.2.1.1.3.6.0";
                string printerFirmwareDateFormatoid = "1.3.6.1.4.1.11.2.3.9.4.2.1.1.3.5.0";
                if (deviceType == "Jedi")
                {
                    JediOmniDevice jediDevice = (JediOmniDevice)device;
                    SnmpOidValue modelNameSnmpOidValue = jediDevice.Snmp.GetRaw(printerModelNameoid);
                    printerDetails["MODELNAME"] = modelNameSnmpOidValue.Value.ToString();
                    SnmpOidValue modelNumberSnmpOidValue = jediDevice.Snmp.GetRaw(printerModelNumberoid);
                    printerDetails["MODELNUMBER"] = modelNumberSnmpOidValue.Value.ToString();
                    SnmpOidValue firmwareSnmpOidValue = jediDevice.Snmp.GetRaw(printerfirmwareoid);
                    printerDetails["FIRMWARE"] = firmwareSnmpOidValue.Value.ToString();
                    SnmpOidValue dateCodeSnmpOidValue = jediDevice.Snmp.GetRaw(printerFirmwareDateFormatoid);
                    printerDetails["DATEFORMAT"] = dateCodeSnmpOidValue.Value.ToString();
                }
                else if (deviceType == "Dune")
                {
                    DuneDevice duneDevice = (DuneDevice)device;
                    SnmpOidValue modelNameSnmpOidValue = duneDevice.Snmp.GetRaw(printerModelNameoid);
                    printerDetails["MODELNAME"] = modelNameSnmpOidValue.Value.ToString();
                    SnmpOidValue modelNumberSnmpOidValue = duneDevice.Snmp.GetRaw(printerModelNumberoid);
                    printerDetails["MODELNUMBER"] = modelNumberSnmpOidValue.Value.ToString();
                    if (duneDevice.ControlPanel.GetType().Equals(typeof(ProSelectDialControlPanel)))
                    {
                        Logger.Trace("ProSelect Dial UI");
                        ProSelectDialControlPanel _dialUI = duneDevice.ControlPanel as ProSelectDialControlPanel;
                    }
                    else if (duneDevice.ControlPanel.GetType().Equals(typeof(WorkflowControlPanel)))
                    {
                        Logger.Trace("Workflow Control Panel");
                        WorkflowControlPanel _workflowUI = duneDevice.ControlPanel as WorkflowControlPanel;
                    }

                    ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                    var deviceInfo = duneDevice.GetDeviceInfo();
                    if (deviceInfo != null)
                    {
                        printerDetails["FIRMWARE"] = deviceInfo.FirmwareRevision.ToString();
                        printerDetails["DATEFORMAT"] = deviceInfo.FirmwareDateCode.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in the method GetPrinterName : ", ex);
                return null;
            }
            return printerDetails;
        }

        // Helper method to execute PowerShell script
        private string ExecutePowerShell(string scriptText)
        {
            Process process = new Process();
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{scriptText}\"";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output;
        }

        [KeywordDescription("Added Printer and System Information into the report file header")]
        [KeywordDisplayName("Display System and Printer Information")]
        [SampleScript("Display System and Printer Information()")]
        public KeywordResult DisplaySystemandPrinterInformation()
        {
            try
            {
                _enableSystemandPrinterInformation= true;
                return new KeywordResult(KeywordResults.Pass, "Displayed system and printer information ");
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Error during displayed system and printer information");
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        [GetKeyword]
        [KeywordDescription("Concatenates two string variables and retrieves the value of the resulting variable.")]
        [KeywordDisplayName("Concatenate Variables And Store Result")]
        [KeywordParameters("firstString", "The first string to concatenate.")]
        [KeywordParameters("secondString", "The second string to concatenate.")]
        [KeywordParameters("resultVariableName", "The name of the variable to store the result.")]
        [SampleScript("Concatenate Variables And Store Result (${variable1},${variable2},${storeresult})")]
        public KeywordResult ConcatenateVariablesAndStoreResult(string firstString, string secondString, string resultVariableName)
        {
            try
            {
                if (firstString.StartsWith("$"))
                {
                    firstString = firstString.Replace("$", "").Replace("{", "").Replace("}", "");
                }
                if (secondString.StartsWith("$"))
                {
                    secondString = secondString.Replace("$", "").Replace("{", "").Replace("}", "");
                }
                string concatenatedResult = firstString + secondString;

                string concatedVariable = "${" + concatenatedResult + "}";
                string concatedVariableValue = CommonExecutionInfo.GetVariable(concatedVariable);

                KeywordResult kr = new KeywordResult(KeywordResults.Pass);
                if (string.IsNullOrEmpty(concatedVariableValue))
                {
                    kr.Output = $"{resultVariableName} : {concatenatedResult}";
                    return kr;
                }

                CommonExecutionInfo.SetVariable(resultVariableName, concatedVariableValue);
                kr.Output = $"{resultVariableName} : {concatedVariableValue}";
                return kr;
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, $"An error occurred: {ex.Message}");
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        [KeywordDescription("Concatenates two string variable values and retrieves the value of the resulting variable.")]
        [KeywordDisplayName("Concatenate Variable Values And Store Result")]
        [KeywordParameters("firstString", "The first string to concatenate.")]
        [KeywordParameters("secondString", "The second string to concatenate.")]
        [KeywordParameters("resultVariableName", "The name of the variable to store the result.")]
        [SampleScript("Concatenate Variable Values And Store Result (${variable1}, ${variable2}, ${storeresult})")]
        public KeywordResult ConcatenateVariableValuesAndStoreResult(string firstValue, string secondValue, string resultVariableName)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(firstValue))
                {
                    return new KeywordResult(KeywordResults.Fail, "First string value cannot be null or empty.");
                }

                if (string.IsNullOrEmpty(secondValue))
                {
                    return new KeywordResult(KeywordResults.Fail, "Second string value cannot be null or empty.");
                }
                if (string.IsNullOrEmpty(resultVariableName))
                {
                    return new KeywordResult(KeywordResults.Fail, "Result variable name cannot be null or empty.");
                }

                // Concatenate values
                string concatenatedResult = $"{firstValue}{secondValue}";
                KeywordResult result = new KeywordResult(KeywordResults.Pass)
                {
                    Output = $"{resultVariableName} = {concatenatedResult}"
                };

                // Store the concatenated variable values result in the variable manager
                CommonExecutionInfo.SetVariable(resultVariableName, concatenatedResult);

                return result;
            }
            catch (Exception ex)
            {
                string errorMessage = $"An error occurred while concatenating variable values: {ex.Message}";
                Logger.Error(errorMessage, ex);
                return new KeywordResult(KeywordResults.Error, errorMessage);
            }
        }


        [KeywordDescription("Exports data from CSV file to the existing Data Table in the database")]
        [KeywordDisplayName("Export CSV To DB")]
        [KeywordParameters("csvFilePath", "Path of the given CSV file")]
        [KeywordParameters("tableName", "Database table name to insert the data.")]
        [KeywordParameters("connectionString", "Connection string of the database.")]
        [SampleScript("ExportCSVToDB(csvFilePath, tableName, connectionString)")]
        public KeywordResult ExportCSVToDB(string csvFilePath, string tableName, string connectionString)
        {
            try
            {
                // Validate file existence
                if (!File.Exists(csvFilePath))
                {
                    return new KeywordResult(KeywordResults.Error, "CSV file does not exist.");
                }

                // Read the CSV file and parse it into a DataTable
                System.Data.DataTable csvData = ReadCSVFile(csvFilePath);

                // Check if the data is valid
                if (csvData == null || csvData.Rows.Count == 0)
                {
                    return new KeywordResult(KeywordResults.Error, "No valid data found in the CSV file.");
                }

                // Insert the parsed data into the database
                InsertDataIntoDatabase(csvData, tableName, connectionString);

                return new KeywordResult(KeywordResults.Pass, "Successfully exported CSV data to DB.");
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Error during exporting CSV to database.");
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        private System.Data.DataTable ReadCSVFile(string filePath)
        {
            System.Data.DataTable dt = new System.Data.DataTable();

            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    bool isFirstRow = true;
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');

                        // If it's the first row, add columns to the DataTable
                        if (isFirstRow)
                        {
                            foreach (var value in values)
                            {
                                dt.Columns.Add(value.Trim());
                            }
                            isFirstRow = false;
                        }
                        else
                        {
                            // Add the data rows
                            dt.Rows.Add(values);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error reading CSV file", ex);
                return null;
            }

            return dt;
        }

        private void InsertDataIntoDatabase(System.Data.DataTable csvData, string tableName, string connectionString)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Create a MySqlCommand to perform bulk insert
                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        cmd.Connection = connection;

                        // Prepare the INSERT command to bulk insert rows
                        foreach (DataRow row in csvData.Rows)
                        {
                            var columns = new List<string>();
                            var parameters = new List<MySqlParameter>();

                            // Loop through each column in the DataTable
                            for (int i = 0; i < csvData.Columns.Count; i++)
                            {
                                string columnName = csvData.Columns[i].ColumnName;

                                // Skip the "id" column (auto-increment)
                                if (columnName.Equals("id", StringComparison.OrdinalIgnoreCase))
                                    continue;

                                string value = row[i].ToString();

                                // If value is empty or null, handle it as DBNull
                                if (string.IsNullOrWhiteSpace(value))
                                {
                                    value = null;
                                }
                                else if (IsDateColumn(columnName))
                                {
                                    // Format date if column is a date column
                                    value = FormatDate(value);
                                }

                                // Add the column and corresponding parameter
                                columns.Add(columnName);
                                parameters.Add(new MySqlParameter("@" + columnName, value ?? (object)DBNull.Value));
                            }

                            // Construct the final command text with parameters
                            var columnList = string.Join(",", columns);
                            var valuePlaceholders = string.Join(",", parameters.Select(p => p.ParameterName));

                            cmd.CommandText = $"INSERT INTO {tableName} ({columnList}) VALUES ({valuePlaceholders})";

                            // Add the parameters to the command
                            cmd.Parameters.Clear(); // Clear previous parameters
                            cmd.Parameters.AddRange(parameters.ToArray());

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error inserting data into database", ex);
                throw new Exception("Error inserting data into database", ex);
            }
        }

        private bool IsDateColumn(string columnName)
        {
            // Check if column is a date column. This logic can be customized based on your needs.
            return columnName.ToLower().Contains("date");
        }

        private string FormatDate(string date)
        {
            if (string.IsNullOrWhiteSpace(date))
                return null;

            if (DateTime.TryParse(date, out DateTime parsedDate))
            {
                return parsedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            return null;
        }

        [KeywordDescription("Break the loop and will not execute the next iterations in the loop")]
        [KeywordDisplayName("BreakLoop")]
        [SampleScript("BreakLoop")]
        public KeywordResult BreakLoop()
        {
            try
            {
                Break = true;
                KeywordResult kr = new KeywordResult(KeywordResults.Pass);
                kr.Output = "The user gave the loop to break";
                return kr;
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, $"An error occurred: {ex.Message}");
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        [KeywordDescription("Continue the loop with the next iteration")]
        [KeywordDisplayName("ContinueLoop")]
        [SampleScript("ContinueLoop")]
        public KeywordResult Continueloop()
        {
            try
            {
                Continue = true;
                KeywordResult kr = new KeywordResult(KeywordResults.Pass);
                kr.Output = "The user gave the loop to continue";
                return kr;
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, $"An error occurred: {ex.Message}");
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        [KeywordDescription("Executes a query against the DB")]
        [KeywordDisplayName("ExecuteDBQuery")]
        [KeywordParameters("conStrng", "connection string")]
        [KeywordParameters("query", "query to execute")]
        [KeywordParameters("logFilePath", "logs the failed data into csv file for future reference")]
        [SampleScript("ExecuteDBQuery(Server=servername;Port=3306;Database=dbName;Uid=abc;Pwd=xxx;SslMode=none;, insert into table_name()values(), csv file path)")]
        public KeywordResult ExecuteDBQuery(string conStrng, string query, string logFilePath)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(conStrng))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return new KeywordResult(KeywordResults.Pass, $"{rowsAffected} row(s) inserted successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogFailedInsert(query, ex.Message, logFilePath);
                return new KeywordResult(KeywordResults.Error, $"Error: {ex.Message}");
            }
        }

        [KeywordDescription("Executes a query against the DB")]
        [KeywordDisplayName("ExecuteDBQuery")]
        [KeywordParameters("conStrng", "connection string")]
        [KeywordParameters("query", "query to execute")]
        [SampleScript("ExecuteDBQuery(Server=servername;Port=3306;Database=dbName;Uid=abc;Pwd=xxx;SslMode=none;, insert into table_name()values())")]
        public KeywordResult ExecuteDBQuery(string conStrng, string query)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(conStrng))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return new KeywordResult(KeywordResults.Pass, $"{rowsAffected} row(s) inserted successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(query + " : " + ex.Message);
                return new KeywordResult(KeywordResults.Error, $"Error: {ex.Message}");
            }
        }
        private void LogFailedInsert(string query, string errorMessage, string logFilePath)
        {
            try
            {
                bool fileExists = File.Exists(logFilePath);
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    if (!fileExists)
                    {
                        writer.WriteLine("Query,ErrorMessage"); // Write header if file is new
                    }
                    writer.WriteLine($"\"{query.Replace("\"", "\"")}\",\"{errorMessage.Replace("\"", "\"")}\"");
                }
            }
            catch (Exception logEx)
            {
                Logger.Debug($"Failed to log error: {logEx.Message}");
            }
        }

        [KeywordDisplayName("GetDecryptValueByKey")]
        [KeywordDescription("Decrypts a value based on the given key. Key is the encrypted variable Name stored in the GFriend UI")]
        [KeywordParameters("mySecreteKey", "The key is nothing but the Encrypted Variable Name i.e, stored in the GFriend UI")]
        [KeywordParameters("variable", "The variable to store the decrypted value")]
        [SampleScript("GetDecryptValueByKey(MySecretKey)")]
        public KeywordResult GetDecryptValueByKey(string mySecreteKey, string variable)
        {
            if (!File.Exists(xmlFilePath))
            {
                return new KeywordResult(KeywordResults.Fail, "Encrypted data file not found.");
            }
            // Load encrypted data from XML
            List<EncryptedData> dataList = LoadDataFromXml(xmlFilePath);
            // Find the matching key
            var entry = dataList.FirstOrDefault(d => d.Key == mySecreteKey);
            if (entry == null)
            {
                return new KeywordResult(KeywordResults.Fail, $"Key '{mySecreteKey}' not found.");
            }
            // Decrypt the value
            string decryptedValue = Decryptxml(entry.EncryptedValue, mySecreteKey);
            CommonExecutionInfo.SetVariable(variable, decryptedValue);
            return new KeywordResult(KeywordResults.Pass, "");
        }

        [KeywordDisplayName("GetEncryptedValueByKey")]
        [KeywordDescription("Retrieves the encrypted value based on the given key. Key is Encrypted Variable Name from GFriend UI.")]
        [KeywordParameters("key", "The key is nothing but the Encrypted Variable Name i.e, stored in the GFriend UI")]
        [KeywordParameters("variable", "The variable to store the encrypted value")]
        [SampleScript("GetEncryptedValueByKey(MySecretKey, OutputVariable)")]
        public KeywordResult GetEncryptedValueByKey(string key, string variable)
        {
            // Ensure the XML file exists before proceeding
            if (!File.Exists(xmlFilePath))
            {
                return new KeywordResult(KeywordResults.Fail, "Encrypted data file not found.");
            }
            // Load encrypted data from XML
            List<EncryptedData> dataList = LoadDataFromXml(xmlFilePath);
            // Find the matching key
            var entry = dataList.FirstOrDefault(d => d.Key == key);
            if (entry == null)
            {
                return new KeywordResult(KeywordResults.Fail, $"Key '{key}' not found.");
            }
            // Store the encrypted value in the given variable
            CommonExecutionInfo.SetVariable(variable, entry.EncryptedValue);
            return new KeywordResult(KeywordResults.Pass, entry.EncryptedValue);
        }


        [KeywordDisplayName("Encrypt")]
        [KeywordDescription("Encrypts a given value using AES encryption. Usually add AzureVisionEndpoint url and SubscriptionKey to encrypt the values")]
        [KeywordParameters("value", "The value to be encrypted. like Azure Vision Endpoint URL or Azure vision subscription key ")]
        [KeywordParameters("result", "The output parameter that stores the encrypted value")]
        [SampleScript("Encrypt (rdl123,${var})")]
        public KeywordResult Encrypt(string value, string variable)
        {
            if (string.IsNullOrEmpty(value))
                return new KeywordResult(KeywordResults.Fail, "Value cannot be null or empty.");

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = GenerateAESKey(_gEncryptionKey);
                    aes.IV = new byte[16]; // Zero IV for simplicity (use random IV for better security)

                    using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    {
                        byte[] inputBytes = Encoding.UTF8.GetBytes(value);
                        byte[] encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
                        string encryptedString = Convert.ToBase64String(encryptedBytes);
                        CommonExecutionInfo.SetVariable(variable, encryptedString);
                        return new KeywordResult(KeywordResults.Pass) { Output = encryptedString };
                    }
                }
            }
            catch (Exception ex)
            {
                return new KeywordResult(KeywordResults.Fail, $"Encryption failed: {ex.Message}");
            }
        }

        [KeywordDisplayName("Decrypt")]
        [KeywordDescription("Decrypts a given encrypted value using AES decryption.")]
        [KeywordParameters("encryptedValue", "The Base64-encoded encrypted value to decrypt. The ouput from the keyword Encrypt(value,variable)")]
        [KeywordParameters("variable", "The variable to store the decrypted value")]
        [SampleScript("Decrypt (GD8St9qQ3yqrcxDsbcxlkg==,${decrydata})")]
        public KeywordResult Decrypt(string encryptedValue, string variable)
        {
            if (string.IsNullOrEmpty(encryptedValue) || string.IsNullOrEmpty(_gEncryptionKey))
                return new KeywordResult(KeywordResults.Fail, "Encrypted value and key cannot be null or empty.");

            try
            {
                using (Aes aes = Aes.Create()) //AES (Advanced Encryption Standard)
                {
                    aes.Key = GenerateAESKey(_gEncryptionKey);
                    aes.IV = new byte[16]; // Must match the IV used during encryption (IV - Initialization Vector)

                    using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    {
                        byte[] encryptedBytes = Convert.FromBase64String(encryptedValue);
                        byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                        string decryptedString = Encoding.UTF8.GetString(decryptedBytes);
                        CommonExecutionInfo.SetVariable(variable, decryptedString);
                        return new KeywordResult(KeywordResults.Pass) { Output = decryptedString };
                    }
                }
            }
            catch (Exception ex)
            {
                return new KeywordResult(KeywordResults.Fail, $"Decryption failed: {ex.Message}");
            }
        }

        [KeywordDescription("Start network monitor for the printer based on the DUT_ADDRESS")]
        [KeywordDisplayName("Start Network Monitor for Printer")]
        [SampleScript("Start Network Monitor for Printer()")]
        public KeywordResult StartNetworkMonitorForPrinter()
        {
            try
            {
                // Get the printer IP from CommonExecutionInfo
                string deviceAddress = CommonExecutionInfo.GetVariable(SystemVariables.DUT_ADDRESS);

                if (string.IsNullOrEmpty(deviceAddress))
                {
                    return new KeywordResult(KeywordResults.Fail)
                    {
                        Output = "Device address (DUT_ADDRESS) not found in execution info."
                    };
                }

                // Cancel any previous tasks
                cancellationTokenSource?.Cancel();
                cancellationTokenSource = new CancellationTokenSource();
                var token = cancellationTokenSource.Token;

                // Start monitoring the network in the background
                monitoringTask = Task.Run(() => MonitorDeviceNetworkAsync(deviceAddress, token), token);

                return new KeywordResult(KeywordResults.Pass)
                {
                    Output = $"Started monitoring network for printer with IP {deviceAddress}"
                };
            }
            catch (Exception ex)
            {
                return new KeywordResult(KeywordResults.Fail)
                {
                    Output = $"Error starting network monitor: {ex.Message}"
                };
            }
        }


        // Method to ping the device and log the status (Reachable/Unreachable)
        private async Task MonitorDeviceNetworkAsync(string deviceAddress, CancellationToken token)
        {
            LogFilePath = Path.Combine(_outputDir, "NetworkLog.csv");

            // Add header if the file doesn't exist
            if (!File.Exists(LogFilePath))
            {
                string header = "Timestamp,Device Address,Status,Latency (ms)";
                File.AppendAllText(LogFilePath, header + Environment.NewLine);
            }
            using (Ping ping = new Ping())
            {
                while (!token.IsCancellationRequested)
                {
                    int pingsPerCycle = 5; // Number of pings you want per interval
                    int delayBetweenPingsMs = 200; // Delay between individual pings
                    bool anyFailure = false;
                    string networkStatus = "";

                    for (int i = 0; i < pingsPerCycle && !token.IsCancellationRequested; i++)
                    {
                        try
                        {
                            PingReply reply = await ping.SendPingAsync(deviceAddress, 3000); // 3-second timeout
                            string status = reply.Status == IPStatus.Success ? "Reachable" : "Unreachable";
                            networkStatus = status;
                            long latency = reply.Status == IPStatus.Success ? reply.RoundtripTime : -1;
                            //   CommonExecutionInfo.SetVariable(SystemVariables.NETWORK_LATENCY, Convert.ToString(latency));
                            if (latency == -1) anyFailure = true;
                            // Log network status to CSV
                            string log = $"{DateTime.Now},{deviceAddress},{status},{latency}";
                            File.AppendAllText(LogFilePath, log + Environment.NewLine);
                        }
                        catch (PingException pingEx)
                        {
                            // Handle PingException (issues related to network)
                            string log = $"{DateTime.Now},{deviceAddress},PingError,-1";
                            File.AppendAllText(LogFilePath, log + Environment.NewLine);
                            Console.WriteLine("Ping error: " + pingEx.Message);
                        }
                        catch (Exception ex)
                        {
                            // General error logging
                            string log = $"{DateTime.Now},{deviceAddress},Error,-1";
                            File.AppendAllText(LogFilePath, log + Environment.NewLine);
                            Console.WriteLine("Error: " + ex.Message);
                        }
                        await Task.Delay(delayBetweenPingsMs, token);
                    }
                    // Fire event if status changed
                    if (anyFailure != _lastLatencyCritical)
                    {
                        _lastLatencyCritical = anyFailure;
                        OnLatencyChanged?.Invoke(this, new LatencyStatusChangedEventArgs(anyFailure));
                    }
                    //Fire event when network is unreachable
                    if (networkStatus.Equals("Unreachable"))
                    {
                        OnNetworkUnreachble?.Invoke(this, new LatencyStatusChangedEventArgs(anyFailure));
                    }
                    await Task.Delay(PingIntervalSeconds * 1000, token); // Delay before next ping
                }
            }
        }

        public static void WaitForNetworkAvailableSTA()
        {
            while (!NetworkAvailableEvent.IsSet)
            {
                Console.WriteLine("Waiting for network to become available...");
                Logger.Debug("Waiting for network to become available...");
                Thread.Sleep(1000);
                System.Windows.Forms.Application.DoEvents(); // Optional based on app type
            }
        }

        [KeywordDescription("Run Robot Framework script and get output.")]
        [KeywordDisplayName("Run Robot File")]
        [KeywordParameters("robotFilePath", "Full path to the Robot Framework (.robot) script to execute")]
        [KeywordParameters("saveTo", "Variable name to save the output")]
        [SampleScript("Run Robot File (${FilePath}, ${varOutput})")]
        public KeywordResult RunRobotFile(string robotFilePath, string saveTo)
        {
            if (!File.Exists(robotFilePath))
            {
                return new KeywordResult(KeywordResults.Fail)
                {
                    Output = $"Robot file not found: {robotFilePath}",
                    AdditionalInfo = "Please verify the file path is correct"
                };
            }

            Process p = new Process();
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "robot"; // Ensure 'robot' is available in PATH
                startInfo.Arguments = $"\"{robotFilePath}\"";
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.StandardErrorEncoding = Encoding.UTF8;
                startInfo.StandardOutputEncoding = Encoding.UTF8;

                p.StartInfo = startInfo;
                p.Start();

                string output = p.StandardOutput.ReadToEnd();
                string errorOutput = p.StandardError.ReadToEnd();
                p.WaitForExit();

                // Save output for inspection
                string finalOutput = output + Environment.NewLine + errorOutput;
                CommonExecutionInfo.SetVariable(saveTo, finalOutput);

                // Detect Python not installed (common Robot Framework error)
                if (errorOutput.ToLower().Contains("python") && errorOutput.ToLower().Contains("not found"))
                {
                    return new KeywordResult(KeywordResults.Fail)
                    {
                        Output = "Python is not installed or not accessible.",
                        AdditionalInfo = "Please install Python and ensure it's added to your system PATH."
                    };
                }

                if (p.ExitCode != 0)
                {
                    return new KeywordResult(KeywordResults.Fail)
                    {
                        Output = "Robot script execution failed",
                        AdditionalInfo = finalOutput
                    };
                }

                return new KeywordResult(KeywordResults.Pass, finalOutput);
            }
            catch (System.ComponentModel.Win32Exception win32Ex)
            {
                string errorMessage = "Robot Framework is not installed or not found in system PATH.";
                Logger.Error(errorMessage, win32Ex);
                return new KeywordResult(KeywordResults.Fail)
                {
                    Output = errorMessage,
                    AdditionalInfo = "Please install Robot Framework using 'pip install robotframework' and make sure it's available in the system PATH."
                };
            }
            catch (Exception ex)
            {
                string errorMessage = "Exception during Robot script executionn";
                Logger.Error(errorMessage, ex);

                // Add hint for Python if it's likely the cause
                string additionalInfo = ex.Message.Contains("python") ?
                    "Python may not be installed or is not available in PATH." :
                    ex.Message;

                return new KeywordResult(KeywordResults.Fail)
                {
                    Output = errorMessage,
                    AdditionalInfo = additionalInfo
                };
            }
        }

        [KeywordDescription("Run Python script and get output. ")]
        [KeywordDisplayName("Run Python File")]
        [KeywordParameters("pythonFilePath", "Full path to the Python script to execute")]
        [KeywordParameters("saveTo", "Variable name to save output")]
        [SampleScript("Run Python File (${FilePath},${varOutput})")]
        public KeywordResult RunPythonFile(string pythonFilePath, string saveTo)
        {
            if (!File.Exists(pythonFilePath))
            {
                return new KeywordResult(KeywordResults.Fail)
                {
                    Output = $"Python file not found: {pythonFilePath}",
                    AdditionalInfo = "Please verify the file path is correct"
                };
            }
            // Check if Python is installed
            if (!IsPythonInstalled())
            {
                return new KeywordResult(KeywordResults.Fail)
                {
                    Output = "Python is not installed on this machine.",
                    AdditionalInfo = "Please install Python from https://www.python.org/downloads/"
                };
            }
            Process p = new Process();
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "python";
                startInfo.Arguments = pythonFilePath;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.StandardErrorEncoding = Encoding.UTF8;
                startInfo.StandardOutputEncoding = Encoding.UTF8;

                p.StartInfo = startInfo;
                p.Start();
                p.WaitForExit();

                string output = p.StandardOutput.ReadToEnd();
                CommonExecutionInfo.SetVariable(saveTo, output);

                if (p.ExitCode != 0)
                {
                    string errorOutput = p.StandardError.ReadToEnd();
                    CommonExecutionInfo.SetVariable(saveTo, errorOutput);
                    return new KeywordResult(KeywordResults.Fail)
                    {
                        Output = "Python script execution failed",
                        AdditionalInfo = errorOutput
                    };
                }

                return new KeywordResult(KeywordResults.Pass, output);
            }
            catch (Exception ex)
            {
                string errorMessage = "Exception during Python script execution";
                Logger.Error(errorMessage, ex);
                return new KeywordResult(KeywordResults.Fail)
                {
                    Output = errorMessage,
                    AdditionalInfo = ex.Message
                };
            }

        }

        private bool IsPythonInstalled()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(psi))
                {
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        [KeywordDescription("Execute a command in a given folder path with user inputs and capture output.")]
        [KeywordDisplayName("Execute Cmd In Folder With User Inputs")]
        [KeywordParameters("folderPath", "The folder where the command will be executed")]
        [KeywordParameters("command", "The command to run (e.g., AppAttestation.exe 192.168.1.101)")]
        [KeywordParameters("saveTo", "Variable name to store the output")]
        [KeywordParameters("inputChoice", "The input(s) to send to the process (e.g., '10;0')")]
        public KeywordResult ExecuteCmdInFolderWithUserInputs(string folderPath, string command, string saveTo, string inputChoice)
        {
            if (!Directory.Exists(folderPath))
            {
                return new KeywordResult(KeywordResults.Fail, $"Folder does not exist: {folderPath}");
            }

            string[] parts = command.Split(new[] { ' ' }, 2);
            string executable = Path.Combine(folderPath, parts[0]);
            string arguments = parts.Length > 1 ? parts[1] : string.Empty;

            if (!File.Exists(executable))
            {
                return new KeywordResult(KeywordResults.Fail, $"Executable not found: {executable}");
            }

            string output = "";
            string error = "";

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = executable,
                    Arguments = arguments,
                    WorkingDirectory = folderPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                using (Process process = new Process())
                {
                    process.StartInfo = startInfo;

                    process.OutputDataReceived += (s, e) =>
                    {
                        if (e.Data != null)
                            output += e.Data + Environment.NewLine;
                    };
                    process.ErrorDataReceived += (s, e) =>
                    {
                        if (e.Data != null)
                            error += e.Data + Environment.NewLine;
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // Send inputs like IP selection or app index
                    if (!string.IsNullOrWhiteSpace(inputChoice))
                    {
                        string[] choices = inputChoice.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var choice in choices)
                        {
                            process.StandardInput.WriteLine(choice.Trim());
                            process.StandardInput.Flush();
                            Thread.Sleep(1000); // Adjust delay as needed
                        }
                    }

                    process.StandardInput.WriteLine(); 
                    process.StandardInput.Flush();

                    if (!process.WaitForExit(30000))  
                    {
                        process.Kill();
                        return new KeywordResult(KeywordResults.Fail, $"Command timed out.\nSTDOUT:\n{output}\nSTDERR:\n{error}");
                    }

                    if (process.ExitCode != 0)
                    {
                        CommonExecutionInfo.SetVariable(saveTo, error);
                        return new KeywordResult(KeywordResults.Fail, error);
                    }

                    CommonExecutionInfo.SetVariable(saveTo, output);
                    return new KeywordResult(KeywordResults.Pass, output);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to execute command in folder with user inputs", ex);
                return new KeywordResult(KeywordResults.Fail, $"Exception: {ex.Message}");
            }
        }

        [KeywordDescription("Fetches the Firmware Bundle Version for Jedi devices by sending an HTTP request to the EWS configuration page using the given IP address.")]
        [KeywordDisplayName("Get Firmware Bundle Version")]
        [KeywordParameters("ipAddress", "IP address of the printer.")]
        [SampleScript("Get Firmware Bundle Version (146.205.4.166)")]
        public KeywordResult GetFirmwareBundleVersion(string ipAddress)
        {
            try
            {
                // Build URL from IP
                string url = $"https://{ipAddress}/hp/device/InternalPages/Index?id=ConfigurationPage";
                // Check if the entered IP address is valid or not
                IPAddress validIp;
                bool isValid = IPAddress.TryParse(ipAddress, out validIp);

                // Validate input
                if (string.IsNullOrEmpty(ipAddress))
                {
                    return new KeywordResult(KeywordResults.Fail, "IP address cannot be null or empty.");
                }            
                if (!isValid)
                {
                    return new KeywordResult(KeywordResults.Fail, "Please enter a valid IP address.");
                }

                // Ignore SSL certificate validation (for self-signed HP printer certificates)
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                string firmwareVersion = string.Empty;

                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(url).Result;
                    response.EnsureSuccessStatusCode();

                    string html = response.Content.ReadAsStringAsync().Result;

                    string search = "Firmware Bundle Version";
                    int index = html.IndexOf(search, StringComparison.OrdinalIgnoreCase);

                    if (index > -1)
                    {
                        string snippet = html.Substring(index, Math.Min(200, html.Length - index));

                        // Look for <strong> tag after the label
                        int strongStart = snippet.IndexOf("<strong", StringComparison.OrdinalIgnoreCase);
                        if (strongStart > -1)
                        {
                            int closeTag = snippet.IndexOf(">", strongStart);
                            int strongEnd = snippet.IndexOf("</strong>", closeTag);

                            if (closeTag > -1 && strongEnd > closeTag)
                            {
                                firmwareVersion = snippet.Substring(closeTag + 1, strongEnd - closeTag - 1).Trim();
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(firmwareVersion))
                    {
                        return new KeywordResult(KeywordResults.Fail, "Firmware Bundle Version not found on the page.");
                    }

                    // Return result only — no variable storage
                    KeywordResult result = new KeywordResult(KeywordResults.Pass)
                    {
                        Output = $"Firmware Bundle Version: {firmwareVersion}"
                    };

                    return result;
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"An error occurred while fetching the Firmware Bundle Version: {ex.Message}";
                Logger.Error(errorMessage, ex);
                return new KeywordResult(KeywordResults.Error, errorMessage);
            }
        }
        [KeywordDisplayName("Has Value Increased")]
        [KeywordDescription("Compares two values and returns True if the new value is greater than the old value.")]
        [KeywordParameters("oldValue", "Initial/base numeric value")]
        [KeywordParameters("newValue", "Value to compare against the initial value")]
        [SampleScript("Has Value Increased(${oldValue}, ${newValue})")]
        public KeywordResult HasValueIncreased(string oldValue, string newValue)
        {

            if (string.IsNullOrEmpty(oldValue)) return new KeywordResult(KeywordResults.Fail) { Output = "oldValue cannot be null or empty." };
            if (string.IsNullOrEmpty(newValue)) return new KeywordResult(KeywordResults.Fail) { Output = "newValue cannot be null or empty." };

            try
            {
                if (!double.TryParse(oldValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double oldVal))
                {

                    return new KeywordResult(KeywordResults.Fail) { Output = $"Invalid oldValue: '{oldValue}' is not a valid number." };
                }

                if (!double.TryParse(newValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double newVal))
                {
                    return new KeywordResult(KeywordResults.Fail) { Output = $"Invalid newValue: '{newValue}' is not a valid number." };
                }

                bool isIncreased = newVal > oldVal;

                if (isIncreased)
                {
                    KeywordResult pass = new KeywordResult(KeywordResults.Pass);
                    pass.Output = $"Value increased: {newVal} > {oldVal}";
                    return pass;
                }
                else
                {
                    KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                    fail.Output = $"Value not increased: {newVal} <= {oldVal}";
                    return fail;
                }
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"Unexpected error while comparing values: {ex.Message}";
                Logger.Error(error.Output, ex);
                return error;
            }
        }
    }
}
using HP.GFriend.Keywords;
using HP.GFriend.Support;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace HP.GFriend.Core.Execution
{
    public class Statement : IGFRunnable
    {
        private static List<DeviceUnderTest> _dut;
        private static string _outputDir = null;
        private static string _tsPath = null;


        public string Library { get; set; }
        public string Keyword { get; set; }
        
        public string OriginalStatement { get; set; }
        
        public object[] Parameters { get; set; }
        
        public KeywordResult Result { get; set; }
        
        public string ErrorDescription { get; set; }

        public string ScreenShotFilePath { get; set; }

        public string DeprecatedReason { get; set; } = string.Empty;

        public int LineNumber { get; set; }

        internal static TestDataManager _lastTestDataManager;
        internal static int _lastRepeatCount;
        internal static int _lastStackLevel;
        internal static Dictionary<string, string> _lastArguments;

        string IGFRunnable.Result
        {
            get { return Result.Result.ToString(); }
        }

        public bool IsStatement
        {
            get { return true; }
        }

        public bool IsAlwaysPass { get; internal set; }

        public Statement()
        {
            Result = null;
            ScreenShotFilePath = null;
        }

        public void Run(TestDataManager testDataManager, int repeatCount, int stackLevel = 0, Dictionary<string, string> arguments = null)
        {
            _lastTestDataManager = testDataManager;
            _lastRepeatCount = repeatCount;
            _lastStackLevel = stackLevel;
            _lastArguments = arguments;

            Reporter.WriteToOutput("Statement", new Dictionary<string, string>() { { "StatementName", OriginalStatement } }, false);
            Reporter.WriteToOutput("StartTime", Utils.GetTime());
            BuiltInLibrary.WaitForNetworkAvailableSTA(); // to make sure that network is up to proceed further with executing the next statements 

            testDataManager.Executor.Execute(this, repeatCount, stackLevel, arguments);  // UNCOMMENT AND USE THIS LINE
            // REMOVE THESE LINES:
            // Executor executor = new Executor(_dut, testDataManager);
            // testDataManager.Executor = executor;
            // executor.InitExecutor(_outputDir, _tsPath);
            
            if (!string.IsNullOrEmpty(DeprecatedReason))
            {
                Reporter.WriteToOutput("Deprecated", DeprecatedReason);
            }

            // Write Report of Statement
            Reporter.WriteToOutput("Result", Result.Result.ToString());
            CommonExecutionInfo.SetVariable(SystemVariables.KEYWORD_OUTPUT, Result.Output ?? string.Empty);
            CommonExecutionInfo.SetVariable(SystemVariables.KEYWORD_RESULT, Result.Result.ToString() ?? string.Empty);
            if (Result.Output != null && !Result.Output.Equals(string.Empty))
            {
                if (Result.Result.Equals(KeywordResults.Pass))
                {
                    Reporter.WriteToOutput("Output", Result.Output);
                }
                else
                {
                    Reporter.WriteToOutput("ErrorDescription", Result.Output);
                }
            }
            if (!string.IsNullOrEmpty(ScreenShotFilePath))
            {
                Reporter.WriteToOutput("ScreenShot", ScreenShotFilePath);
            }

            if (!string.IsNullOrEmpty(Result.AdditionalInfo))
            {
                Reporter.WriteToOutput("AdditionalInfo", Result.AdditionalInfo);
            }

            Reporter.WriteToOutput("EndTime", Utils.GetTime());
            Reporter.WriteToOutput("Statement", false);
        }

        public void AddSubBlock(IGFRunnable block)
        {
            throw new NotSupportedException("Adding sub block is not supported in Statement level");
        }
    }
}

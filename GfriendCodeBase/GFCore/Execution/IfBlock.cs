using HP.GFriend.Keywords;
using HP.GFriend.Support;
using System;
using System.Collections.Generic;

namespace HP.GFriend.Core.Execution
{
    public class IfBlock : IGFRunnable
    {
        public string OriginalStatement { get; internal set; }
        public string Result { get; private set; }
        public Statement Condition { get; set; }
        public CommonBlock StatementPass { get; set; }
        public CommonBlock StatementFail { get; set; }
        public CommonBlock StatementError { get; set; }
        public string TagName { get; } = "If";

        public bool IsStatement
        {
            get { return false; }
        }

        public bool IsAlwaysPass { get; internal set; }

        public void AddSubBlock(IGFRunnable subBlock)
        {
            throw new NotSupportedException("Adding sub block is not supported at IfElse block");
        }

        public void Run(TestDataManager testDataManager, int repeatCount = -1, int stackLevel = 0, Dictionary<string, string> arguments = null)
        {
            // #312 Reset Result
            Result = "PASS";
            string statement = Condition.OriginalStatement;
            Reporter.WriteToOutput(TagName, new Dictionary<string, string>() { { "Condition", Condition.OriginalStatement } }, false);
            Reporter.WriteToOutput("StartTime", Utils.GetTime());

            if(!Condition.OriginalStatement.StartsWith("If : "))
            {
                Condition.OriginalStatement = "If : " + Condition.OriginalStatement;
            }
            
            stackLevel++;
            string condition = testDataManager.Executor.Execute(Condition, repeatCount, stackLevel, arguments).ToUpper();
            if (!string.IsNullOrEmpty(Condition.DeprecatedReason))
            {
                Reporter.WriteToOutput("Statement", new Dictionary<string, string>() { { "StatementName", statement } }, false);
                Reporter.WriteToOutput("Deprecated", Condition.DeprecatedReason);
                Reporter.WriteToOutput("Statement", false);
            }
            Reporter.WriteToOutput("ConditionResult", condition);
            if (Condition.Result.Output != null && !Condition.Result.Output.Equals(string.Empty))
            {
                if (Condition.Result.Result.Equals(KeywordResults.Pass))
                {
                    Reporter.WriteToOutput("Output", Condition.Result.Output);
                }
                else
                {
                    Reporter.WriteToOutput("ErrorDescription", Condition.Result.Output);
                }


            }
            if (!string.IsNullOrEmpty(Condition.ScreenShotFilePath))
            {
                Reporter.WriteToOutput("ScreenShot", Condition.ScreenShotFilePath);
            }

            if (!string.IsNullOrEmpty(Condition.Result.AdditionalInfo))
            {
                Reporter.WriteToOutput("AdditionalInfo", Condition.Result.AdditionalInfo);
            }


            Reporter.WriteToOutput("StatementBlock", new Dictionary<string, string>() { { "Kind", condition } }, false);
            Reporter.WriteToOutput("StartTime", Utils.GetTime());
            Result = "PASS";
            switch (condition)
            {
                case "PASS":
                    StatementPass?.Run(testDataManager, repeatCount, stackLevel, arguments);
                    Result = StatementPass?.Result ?? Result;
                    break;
                case "FAIL":
                    StatementFail?.Run(testDataManager, repeatCount, stackLevel, arguments);
                    Result = StatementFail?.Result ?? Result;
                    break;
                case "ERROR":
                    StatementError?.Run(testDataManager, repeatCount, stackLevel, arguments);
                    Result = StatementError?.Result ?? Result;
                    break;
            }
            stackLevel--;
            Reporter.WriteToOutput("EndTime", Utils.GetTime());
            Reporter.WriteToOutput("Result", Result);
            Reporter.WriteToOutput("StatementBlock", false);

            if(IsAlwaysPass)
            {
                Result = "PASS";
                Reporter.WriteToOutput("AlwaysPass", "True");
            }
            Reporter.WriteToOutput("Result", Result);
            Reporter.WriteToOutput("EndTime", Utils.GetTime());
            Reporter.WriteToOutput(TagName, false);
        }
    }
}

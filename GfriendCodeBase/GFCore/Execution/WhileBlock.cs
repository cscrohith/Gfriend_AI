using HP.GFriend.GFLogger;
using HP.GFriend.Keywords;
using HP.GFriend.Support;
using System;
using System.Collections.Generic;


namespace HP.GFriend.Core.Execution
{
    public class WhileBlock : IGFRunnable
    {
        private List<IGFRunnable> _subBlocks;
        public string OriginalStatement { get; internal set; }
        public string Result { get; private set; }
        public string TagName { get; protected set; }
        public bool IsStatement
        {
            get { return false; }
        }
        public Statement Condition { get; set; }
        public int MaxLoop { get; set; } = 50;
        public bool IsTrueLoop { get; set; } = true;

        public bool IsAlwaysPass { get; internal set; }

        public string CountArgument { get; set; } = "";

        public WhileBlock()
        {
            Result = "PASS";
            TagName = "While";
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

        public void Run(TestDataManager testDataManager, int repeatCount = -1, int stackLevel = 0, Dictionary<string, string> arguments = null)
        {
            // #312 Reset Result
            Result = "PASS";
            string statement = Condition.OriginalStatement;
            if (!(statement.StartsWith("While : ")))
            {
                if (!IsTrueLoop)
                {
                    Condition.OriginalStatement = "!" + Condition.OriginalStatement;
                }
            }

            if (!Condition.OriginalStatement.StartsWith("While : "))
            {
                Condition.OriginalStatement = "While : " + Condition.OriginalStatement;
            }

            Reporter.WriteToOutput(TagName, new Dictionary<string, string>() { { "Condition", Condition.OriginalStatement } }, false);
            Reporter.WriteToOutput("StartTime", Utils.GetTime());


            if (MaxLoop == -1)
            {
                if (string.IsNullOrEmpty(CountArgument) || arguments == null || !arguments.ContainsKey(CountArgument))
                {
                    Result = "ERROR";
                    Logger.Error("Arugment is not given for repeat count");
                    Reporter.WriteToOutput("ErrorDescription", "Arugment is not given for repeat count");
                    Reporter.WriteToOutput("EndTime", Utils.GetTime());
                    Reporter.WriteToOutput("Result", Result);
                    Reporter.WriteToOutput("EndTime", Utils.GetTime());
                    Reporter.WriteToOutput(TagName, false);
                    return;
                }
                string strMaxLoop = arguments[CountArgument].Trim();
                if (!int.TryParse(strMaxLoop, out int intMaxLoop))
                {
                    Result = "ERROR";
                    Logger.Error("Arugment is not a number for repeat count");
                    Reporter.WriteToOutput("ErrorDescription", "Arugment is not a number for repeat count");
                    Reporter.WriteToOutput("EndTime", Utils.GetTime());
                    Reporter.WriteToOutput("Result", Result);
                    Reporter.WriteToOutput("EndTime", Utils.GetTime());
                    Reporter.WriteToOutput(TagName, false);
                    return;
                }
                MaxLoop = intMaxLoop;
            }

            stackLevel++;
            int loopCount = 1;
            bool isError = false;
            int currentLoopCount = MaxLoop;
            while (currentLoopCount != 0)
            {
                BuiltInLibrary.Continue = false;
                currentLoopCount--;
                string condition = testDataManager.Executor.Execute(Condition, repeatCount, stackLevel, arguments).ToUpper();
                if (!string.IsNullOrEmpty(Condition.DeprecatedReason))
                {
                    Reporter.WriteToOutput("Statement", new Dictionary<string, string>() { { "StatementName", statement } }, false);
                    Reporter.WriteToOutput("Deprecated", Condition.DeprecatedReason);
                    Reporter.WriteToOutput("Statement", false);
                }
                bool exitLoop = false;
                
                switch (condition)
                {
                    case "PASS":
                        if(!IsTrueLoop)
                        {
                            exitLoop = true;
                        }
                        break;
                    case "FAIL":
                        if(IsTrueLoop)
                        {
                            exitLoop = true;
                        }
                        break;
                    case "ERROR":
                        exitLoop = true;
                        isError = true;
                        break;
                }

                if (exitLoop) break;

                Console.Write(string.Format("{0}[While Loop : {1}]", new string('\t', stackLevel), loopCount.ToString()));
                Console.WriteLine("");
                Reporter.WriteToOutput("Loop", new Dictionary<string, string>() { { "LoopCount", loopCount.ToString() } }, false);
                Reporter.WriteToOutput("StartTime", Utils.GetTime());
                Logger.Trace($"Loop : {loopCount.ToString()}");
                string loopResult = "PASS";
                stackLevel++;
                foreach (IGFRunnable subBlock in _subBlocks)
                {
                    subBlock.Run(testDataManager, loopCount, stackLevel, arguments);
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

                    if (BuiltInLibrary.ExitTC || BuiltInLibrary.ExitTS || BuiltInLibrary.Break)
                    {
                        break;
                    }
                    else if (BuiltInLibrary.Continue)
                    {
                        break;//so it will not continue with other 
                    }
                }
                stackLevel--;
                Reporter.WriteToOutput("Result", loopResult);
                Reporter.WriteToOutput("EndTime", Utils.GetTime());
                Reporter.WriteToOutput("Loop", false);

                // Reset when every 100th repeat
                if (loopCount % 100 == 0)
                {
                    Logger.Trace("Reinitialize libraries");
                    foreach (Library l in testDataManager.AllUsedLib)
                    {
                        if (l.Name.Equals("Android")) // #304 Reset library only for Android
                        {
                            l.ExecuteMethod("ResetConnection", null);
                        }
                    }
                    Logger.Trace("Reinitilize done");
                }
                if (BuiltInLibrary.ExitTC || BuiltInLibrary.ExitTS || BuiltInLibrary.Break)
                {
                    BuiltInLibrary.Break = false;
                    break;
                }
                else if (BuiltInLibrary.Continue)
                {
                    loopCount++;
                    // BuiltInLibrary.Continue = false;
                    continue;
                }
                loopCount++;
            }
            if(isError)
            {
                Result = "ERROR";
            }
            
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

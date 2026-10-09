using HP.GFriend.GFLogger;
using HP.GFriend.Keywords;
using HP.GFriend.Support;
using System;
using System.Collections.Generic;

namespace HP.GFriend.Core.Execution
{
    public class RepeatBlock : IGFRunnable
    {
        private List<IGFRunnable> _subBlocks;

        public string OriginalStatement { get; internal set; }
        public int RepeatCount { set; get; }

        public string Result { get; private set; }

        public string TagName { get; protected set; }
        public bool IsStatement
        {
            get { return false; }
        }

        public bool IsAlwaysPass { get; internal set; }

        public string CountArgument = string.Empty;

        public TimeSpan Duration { get; set; }
        public bool IsTimeBase { get; set; } = false;

        private int _passCount = 0;
        private int _failCount = 0;
        private int _errorCount = 0;

        private DateTime _endTime;

        public RepeatBlock(int repeatCount=1)
        {
            RepeatCount = repeatCount;
            Result = "PASS";
            TagName = "Repeat";
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
            // #312 Reset Result
            Result = "PASS";
            _passCount = 0;
            _failCount = 0;
            _errorCount = 0;

            bool isDynamicCount = false;
            

            int i;
            if (CountArgument.Contains("$"))
            {
                Reporter.WriteToOutput(TagName, new Dictionary<string, string>() { { "RepeatCount", CommonExecutionInfo.GetVariable(CountArgument).ToString() } }, false);
            }
            else
            {
                Reporter.WriteToOutput(TagName, new Dictionary<string, string>() { { "RepeatCount", this.RepeatCount.ToString() } }, false);
            }
            Reporter.WriteToOutput("StartTime", Utils.GetTime());

            //to check the variable value 
            string strRepeatCount = CountArgument;
            if (strRepeatCount.Contains("$"))//its a variable
            {
                IsTimeBase = false;
                RepeatCount = -1;
                strRepeatCount = CommonExecutionInfo.GetVariable(strRepeatCount);

                if (strRepeatCount.EndsWith("h", StringComparison.CurrentCultureIgnoreCase))
                {
                    strRepeatCount = strRepeatCount.Substring(0, strRepeatCount.Length - 1);
                    int timeInterval = int.Parse(strRepeatCount);
                    Duration = TimeSpan.FromHours(timeInterval);
                    IsTimeBase = true;
                }
                else if (strRepeatCount.EndsWith("m", StringComparison.CurrentCultureIgnoreCase))
                {
                    strRepeatCount = strRepeatCount.Substring(0, strRepeatCount.Length - 1);
                    int timeInterval = int.Parse(strRepeatCount);
                    Duration = TimeSpan.FromMinutes(timeInterval);
                    IsTimeBase = true;
                }
                else if (strRepeatCount.EndsWith("s", StringComparison.CurrentCultureIgnoreCase))
                {
                    strRepeatCount = strRepeatCount.Substring(0, strRepeatCount.Length - 1);
                    int timeInterval = int.Parse(strRepeatCount);
                    Duration = TimeSpan.FromSeconds(timeInterval);
                    IsTimeBase = true;
                }

                arguments = new Dictionary<string, string>();
                arguments.Add(CountArgument, strRepeatCount);
            }
            if (IsTimeBase)
            {
                _endTime = DateTime.Now.Add(Duration);
                RepeatCount = int.MaxValue;
            }

            else if (RepeatCount == -1)
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
                //string strRepeatCount = arguments[CountArgument].Trim();
                if(!int.TryParse(strRepeatCount, out int intRepeatCount))
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
                RepeatCount = intRepeatCount;
                isDynamicCount = true;
            }


            for (i = 1; i <= RepeatCount; i++)
            {
                Reporter.WriteToOutput("Loop", new Dictionary<string, string>() { { "LoopCount", i.ToString() } }, false);
                Reporter.WriteToOutput("StartTime", Utils.GetTime());
                Logger.Trace($"Loop : {i.ToString()}");
                string loopResult = "PASS";
                // CommonExecutionInfo.CurrentRepeatCount = i;

                if (RepeatCount > 1 && RepeatCount != int.MaxValue)
                {
                    Console.Write(string.Format("{0}[Repeat : {1} / {2} ]", new string('\t', stackLevel), i.ToString(), RepeatCount.ToString()));
                    Console.WriteLine("");
                }
                else if (RepeatCount == int.MaxValue)
                {
                    Console.Write(string.Format("{0}[Repeat : {1} ]", new string('\t', stackLevel), i.ToString()));
                    Console.WriteLine("");
                }
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
                    CommonExecutionInfo.SetVariable("LoopResult", loopResult);

                    if (BuiltInLibrary.ExitTC || BuiltInLibrary.ExitTS || BuiltInLibrary.Break || BuiltInLibrary.ExitCustomKeyword)
                    {
                        break;
                    }
                    else if (BuiltInLibrary.Continue)
                    {
                        BuiltInLibrary.Continue = false;
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

                // Commenting this reInitialize for every 100 iterations
                //// Reset when every 100th repeat
                //if(i%100 == 0)
                //{
                //    Logger.Trace("Reinitialize libraries");
                //    foreach(Library l in testDataManager.AllUsedLib)
                //    {
                //        if(l.Name.Equals("Android")) // #208 Reset library only for Android
                //        {
                //            l.ExecuteMethod("ResetConnection", null);
                //        }

                //    }
                //    Logger.Trace("Reinitilize done");
                //}

                if (BuiltInLibrary.ExitTC || BuiltInLibrary.ExitTS || BuiltInLibrary.Break || BuiltInLibrary.ExitCustomKeyword)
                {
                    break;
                }
                else if (BuiltInLibrary.Continue)
                {
                    BuiltInLibrary.Continue = false;
                    continue;
                }

                if (IsTimeBase && DateTime.Now > _endTime)
                {
                    Logger.Trace("RepeatCount=" + repeatCount.ToString());
                    Logger.Trace("IsTimeBase && DateTime.Now > _endTime");
                    break;
                }
                Logger.Trace("LoopCount=" + i.ToString());
            }
            
            if(IsAlwaysPass)
            {
                Result = "PASS";
                Reporter.WriteToOutput("AlwaysPass", "True");
            }
            if(isDynamicCount)
            {
                RepeatCount = -1;
            }
            BuiltInLibrary.LastRepeatResult = new Tuple<int, int, int>(_passCount, _failCount, _errorCount);
            Reporter.WriteToOutput("Result", Result);
            Reporter.WriteToOutput("EndTime", Utils.GetTime());
            Reporter.WriteToOutput(TagName, false);
        }
    }
}

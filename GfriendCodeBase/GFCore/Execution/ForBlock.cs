using HP.GFriend.GFLogger;
using HP.GFriend.Keywords;
using CoreCommonInfo = HP.GFriend.Core.CommonExecutionInfo;
using HP.GFriend.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HP.GFriend.Core.Execution
{
    class ForBlock : IGFRunnable
    {
        private List<IGFRunnable> _subBlocks;

        public string OriginalStatement { get; internal set; }
        public int RepeatCount { set; get; }
        //public int subList { set; get; }
        public List<string> Listvalue { get; private set; }
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

        public ForBlock(List<string> Listparam = null)
        {
            Listvalue = Listparam;
            Result = "PASS";
            TagName = "For";
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
            Reporter.WriteToOutput(TagName, new Dictionary<string, string>() { { "Listvalue", this.OriginalStatement.Replace("\\", "") } }, false);
            Reporter.WriteToOutput("StartTime", Utils.GetTime());

            OriginalStatement = OriginalStatement.Replace("\\", "");
            if (OriginalStatement.StartsWith(" "))
            {
                OriginalStatement = OriginalStatement.Replace(" ", "");
            }
            IList<string> strings = new List<string> { OriginalStatement };
            string joined = string.Join(",", strings).Trim();
            List<string> ListNames = joined.Split(',').ToList<string>();
            for (i = 0; i < ListNames.Count; i++)
            {
                Reporter.WriteToOutput("Loop", new Dictionary<string, string>() { { "LoopCount", ListNames[i].ToString() } }, false);
                Reporter.WriteToOutput("StartTime", Utils.GetTime());
                Logger.Trace($"Loop : {ListNames[i].ToString()}");
                string loopResult = "PASS";
                if (ListNames != null && ListNames.Count > 0)
                {
                    Console.Write(string.Format("{0}[List : {1} / {2} ]", new string('\t', stackLevel), ListNames[i].ToString(), joined.ToString()));
                    Console.WriteLine("");
                }

                stackLevel++;
                foreach (IGFRunnable subBlock in _subBlocks)
                {
                    CoreCommonInfo.SetforloopItem("${ITEM}", ListNames[i].ToString());
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

                    if (BuiltInLibrary.ExitTC || BuiltInLibrary.ExitTS || BuiltInLibrary.Break)
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

                if (BuiltInLibrary.ExitTC || BuiltInLibrary.ExitTS || BuiltInLibrary.Break)
                {
                    BuiltInLibrary.Break = false;
                    break;
                }
                else if (BuiltInLibrary.Continue)
                {
                    BuiltInLibrary.Continue = false;
                    continue;
                }

            }

            if (IsAlwaysPass)
            {
                Result = "PASS";
                Reporter.WriteToOutput("AlwaysPass", "True");
            }
            if (isDynamicCount)
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

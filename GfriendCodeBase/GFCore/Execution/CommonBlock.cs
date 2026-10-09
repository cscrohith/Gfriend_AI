using HP.GFriend.Keywords;
using HP.GFriend.Support;
using System.Collections.Generic;


namespace HP.GFriend.Core.Execution
{
    public class CommonBlock : IGFRunnable
    {
        private List<IGFRunnable> _subBlocks;

        public string OriginalStatement { get; internal set; }
        public string Result { get; private set; }
        public string TagName { get; protected set; }
        public bool IsStatement
        {
            get { return false; }
        }

        public bool IsAlwaysPass { get; internal set; }

        public CommonBlock()
        {
            Result = "PASS";
            TagName = "Block";
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

            Reporter.WriteToOutput(TagName, true);
            Reporter.WriteToOutput("StartTime", Utils.GetTime());
 
            foreach (IGFRunnable subBlock in _subBlocks)
            {
                
                subBlock.Run(testDataManager, repeatCount, stackLevel, arguments);
                if (subBlock.Result.ToUpper().Equals("FAIL"))
                {
                    Result = "FAIL";
                }
                else if (subBlock.Result.ToUpper().Equals("ERROR"))
                {
                    Result = "ERROR";
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

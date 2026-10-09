using HP.GFriend.Keywords;
using HP.GFriend.Support;
using HP.GFriend.GFLogger;
using System;
using System.Collections.Generic;

namespace HP.GFriend.Core.Execution
{
    public class TestCase : IGFRunnable
    {
        protected string tagName;
        public string OriginalStatement { get; internal set; }
        public List<IGFRunnable> subBlocks { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Stable 0-based identifier reflecting this test case's declaration order in the
        /// script. Assigned once by <see cref="TestSuite.AddTestCase"/> at parse time and
        /// never changed afterwards. UI selection and execution must key off this Id -
        /// never a display Name (which can collide when duplicate names exist) and never a
        /// UI row index/position (which can change due to sorting, refreshes, etc.).
        /// </summary>
        public int Id { get; internal set; } = -1;

        public string MetaData { get; set; } = string.Empty;
        public Dictionary<string, string> MetaDataDic;

        public string Result
        {
            get; private set;
        }
        public bool IsStatement
        {
            get { return false; }
        }

        public bool IsAlwaysPass { get; } = false;

        public TestCase()
        {
            Result = "Not Tested";
            subBlocks = new List<IGFRunnable>();
            tagName = "TestCase";
            MetaDataDic = new Dictionary<string, string>();
        }

        public void AddSubBlock(IGFRunnable subBlock)
        {
            subBlocks.Add(subBlock);
        }

        public void Run(TestDataManager testDataManager, int repeatCount = -1, int stackLevel = 0, Dictionary<string, string> arguments = null)
        {
            Result = "PASS";
            Reporter.WriteToOutput(tagName, new Dictionary<string, string>() { { "Name", Name } }, false);
            Reporter.WriteToOutput("StartTime", Utils.GetTime());
            foreach(IGFRunnable subBlock in subBlocks)
            {

                try
                {
                    subBlock.Run(testDataManager);
                }
                catch (Exception ex)
                {
                    // Never let an unexpected exception from a sub-block abort the whole
                    // test case (and, by extension, the remaining test cases in the suite,
                    // possibly targeting different platforms). Record it as an error and
                    // continue so subsequent test cases still get executed and reported.
                    Logger.Error($"Unexpected error executing sub-block in Test Case '{Name}'", ex);
                    Result = "ERROR";
                    continue;
                }
                if(subBlock.Result.ToUpper().Equals("FAIL"))
                {
                    Result = "FAIL";
                }
                else if (subBlock.Result.ToUpper().Equals("ERROR"))
                {
                    Result = "ERROR";
                }
                if (BuiltInLibrary.ExitTC || BuiltInLibrary.ExitTS || BuiltInLibrary.Break)
                {
                    BuiltInLibrary.ExitTC = false;
                    BuiltInLibrary.Break = false;
                    break;
                }
                else if (BuiltInLibrary.Continue)
                {
                    BuiltInLibrary.Continue = false;
                    continue;
                }
            }
            Reporter.WriteToOutput("Result", Result);
            Reporter.WriteToOutput("EndTime", Utils.GetTime());
            Reporter.WriteToOutput(tagName, false);
        }

    }
}

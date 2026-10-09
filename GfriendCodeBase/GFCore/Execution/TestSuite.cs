using HP.GFriend.Event;
using HP.GFriend.Keywords;
using HP.GFriend.Support;
using HP.GFriend.GFLogger;
using System;
using System.Collections.Generic;

namespace HP.GFriend.Core.Execution
{
    public class TestSuite
    {
        public List<TestCase> TestCases { get { return _testcases; } }
        public string Description;
        public string MetaData { get; set; } = string.Empty;
        public Dictionary<string, string> MetaDataDic;

        internal List<TestCase> _testcases;
        internal List<string> _usedLibrary;
        internal string _scriptFilePath;
        private string _result;

        public string Name { get; set; }
        
        

        public string Result
        {
            get { return _result; }
        }
        
        public TestSuite()
        {
            _result = "PASS";
            _testcases = new List<TestCase>();
            _usedLibrary = new List<string>();
            MetaDataDic = new Dictionary<string, string>();
        }

        public void AddTestCase(TestCase aTestCase)
        {
            // Assign the stable, 0-based declaration-order Id here - the single place
            // test cases are registered into the suite - so the Id can never drift out
            // of sync with the order the UI/parser enumerate test cases in.
            aTestCase.Id = _testcases.Count;
            _testcases.Add(aTestCase);
        }

        public void UsingLibrary(string libName)
        {
            _usedLibrary.Add(libName);
        }

        public int TcCount
        {
            get { return _testcases.Count; }
        }

        public List<string> GetTestCaseNames()
        {
            List<string> tcName = new List<string>();
            foreach(TestCase tc in _testcases)
            {
                tcName.Add(tc.Name);
            }

            return tcName;
            
        }

       

        public void Run(TestDataManager testDataManager)
        {
            
            Reporter.WriteToOutput("TestSuite", new Dictionary<string, string>() {{ "Name", Name }},false);
            Reporter.WriteToOutput("StartTime", Utils.GetTime());
            GFEvent.Trigger("StartTestSuite");
            foreach (TestCase testcase in _testcases)
            {
                BuiltInLibrary.ExitTC = false; // To prevent unexpected exit
                BuiltInLibrary.Break = false; // To prevent unexpected exit
                BuiltInLibrary.Continue = false; // To prevent unexpected exit
                CommonExecutionInfo.SetVariable("_tcName", testcase.Name);
                CommonExecutionInfo.SetVariable(SystemVariables.TESTCASE_NAME, testcase.Name);
                if(BuiltInLibrary.ExitTS)
                {
                    BuiltInLibrary.ExitTS = false;
                    break;
                }
                Console.WriteLine("\n::: Test Case :::: " + testcase.Name);
                try
                {
                    testcase.Run(testDataManager);
                }
                catch (Exception ex)
                {
                    // A failure (including an unexpected exception) on one test case/platform
                    // must not interrupt execution of the remaining test cases, which may
                    // target different platforms (iOS, Mac, Jedi, Android, Dune, etc.).
                    Logger.Error($"Unexpected error executing Test Case '{testcase.Name}'", ex);
                    Reporter.WriteToOutput("ErrorDescription", ex.Message);
                    _result = "ERROR";
                    Console.WriteLine();
                    continue;
                }
                if (testcase.Result.ToUpper().Equals("FAIL"))
                {
                    _result = "FAIL";
                }
                else if (testcase.Result.ToUpper().Equals("ERROR"))
                {
                    _result = "ERROR";
                }
                Console.WriteLine();
            }
            GFEvent.Trigger("EndTestSuite");
            Reporter.WriteToOutput("EndTime", Utils.GetTime());
            Reporter.WriteToOutput("TestSuite", false);
        }

        public void Run(TestDataManager testDataManager, List<string> tcToRun)
        {
            
            Reporter.WriteToOutput("TestSuite", new Dictionary<string, string>() { { "Name", Name } }, false);
            Reporter.WriteToOutput("StartTime", Utils.GetTime());
            GFEvent.Trigger("StartTestSuite");
            foreach (TestCase testcase in _testcases)
            {
                if (!tcToRun.Contains(testcase.Name))
                    continue;

                // Reset per-testcase flow-control flags before every explicitly selected
                // test case. In particular, "Stop Test Suite" (ExitTS) called from within
                // one selected test case (e.g. an error-handling If block cleaning up after
                // a platform failure) must only stop that test case's own remaining steps
                // -- it must NOT prevent the other explicitly selected test cases (which may
                // target different platforms such as iOS, Mac, Jedi, Android, Dune) from
                // still being executed and reported.
                BuiltInLibrary.ExitTC = false;
                BuiltInLibrary.Break = false;
                BuiltInLibrary.Continue = false;
                BuiltInLibrary.ExitTS = false;

                CommonExecutionInfo.SetVariable(SystemVariables.TESTCASE_NAME, testcase.Name);
                CommonExecutionInfo.SetVariable("_tcName", testcase.Name);
                Console.WriteLine("\n::: Test Case :::: " + testcase.Name);
                try
                {
                    testcase.Run(testDataManager);
                }
                catch (Exception ex)
                {
                    // Same resilience as the full-suite Run overload: don't let one
                    // test case's failure prevent the remaining selected test cases
                    // (potentially on other platforms) from executing.
                    Logger.Error($"Unexpected error executing Test Case '{testcase.Name}'", ex);
                    Reporter.WriteToOutput("ErrorDescription", ex.Message);
                    _result = "ERROR";
                    Console.WriteLine();
                    continue;
                }
                finally
                {
                    // Ensure a "Stop Test Suite" triggered inside this test case cannot
                    // leak into the next selected test case's execution.
                    BuiltInLibrary.ExitTS = false;
                }
                if (testcase.Result.Equals("FAIL"))
                {
                    _result = "FAIL";
                }
                else if (testcase.Result.Equals("ERROR"))
                {
                    _result = "ERROR";
                }
                Console.WriteLine();
            }
            GFEvent.Trigger("EndTestSuite");
            Reporter.WriteToOutput("EndTime", Utils.GetTime());
            Reporter.WriteToOutput("TestSuite", false);
        }

        /// <summary>
        /// Executes only the test cases whose stable <see cref="TestCase.Id"/> is present in
        /// <paramref name="tcIdsToRun"/>. This must be used (in preference to the Name-based
        /// overload) whenever the caller has the stable Ids available, since Names can be
        /// ambiguous when two test cases share the same displayed text.
        /// </summary>
        public void Run(TestDataManager testDataManager, ICollection<int> tcIdsToRun)
        {
            Reporter.WriteToOutput("TestSuite", new Dictionary<string, string>() { { "Name", Name } }, false);
            Reporter.WriteToOutput("StartTime", Utils.GetTime());
            GFEvent.Trigger("StartTestSuite");
            foreach (TestCase testcase in _testcases)
            {
                if (!tcIdsToRun.Contains(testcase.Id))
                    continue;

                BuiltInLibrary.ExitTC = false;
                BuiltInLibrary.Break = false;
                BuiltInLibrary.Continue = false;
                BuiltInLibrary.ExitTS = false;

                CommonExecutionInfo.SetVariable(SystemVariables.TESTCASE_NAME, testcase.Name);
                CommonExecutionInfo.SetVariable("_tcName", testcase.Name);
                Console.WriteLine("\n::: Test Case :::: " + testcase.Name);
                try
                {
                    testcase.Run(testDataManager);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Unexpected error executing Test Case '{testcase.Name}'", ex);
                    Reporter.WriteToOutput("ErrorDescription", ex.Message);
                    _result = "ERROR";
                    Console.WriteLine();
                    continue;
                }
                finally
                {
                    BuiltInLibrary.ExitTS = false;
                }
                if (testcase.Result.Equals("FAIL"))
                {
                    _result = "FAIL";
                }
                else if (testcase.Result.Equals("ERROR"))
                {
                    _result = "ERROR";
                }
                Console.WriteLine();
            }
            GFEvent.Trigger("EndTestSuite");
            Reporter.WriteToOutput("EndTime", Utils.GetTime());
            Reporter.WriteToOutput("TestSuite", false);
        }

        public void PrintResult()
        {
            int passCount = 0;
            int failCount = 0;
            int errorCount = 0;
            foreach(TestCase testcase in _testcases)
            {
                if(testcase.Result.ToUpper().Equals("PASS"))
                {
                    passCount++;
                }
                else if (testcase.Result.ToUpper().Equals("FAIL"))
                {
                    failCount++;
                }
                else if(testcase.Result.ToUpper().Equals("ERROR"))
                {
                    errorCount++;
                }
            }

            Console.WriteLine("\nTotal : {0}\tPass : {1}\tFail : {2}\tError : {3}\n", passCount + failCount + errorCount, passCount, failCount, errorCount);
        }
    }
}

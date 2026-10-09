using HP.GFriend.Client;
using HP.GFriend.Core;
using HP.GFriend.Core.Execution;
using HP.GFriend.Keywords;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace HP.GFriend.Repeater
{
    public class RepeatEngine
    {
        private GFServerConnector _gfServerConnector;
        private TestProject _testProject;
        private ScriptsToRun _scriptsToRun;
        private string _outputRoot;
        private string _scriptRoot;
        private string _executionID;

        private DateTime _startTime;
        private int _currentCount;

        
        public RepeatEngine(string executionID, GFServerConnector gfServerConnector, string scriptRoot, string outputRoot)
        {
            _gfServerConnector = gfServerConnector;
            _executionID = executionID;
            _outputRoot = Path.Combine(outputRoot, _executionID);
            _scriptRoot = Path.Combine(scriptRoot, _executionID);
        }


        public void Run()
        {
            // Prepare Execution
            Console.WriteLine("==== Preparing Execution ====");
            bool isTestProjectCreated = false;

            int waitCount = 0;
            while (!isTestProjectCreated)
            {
                waitCount++;
                Thread.Sleep(TimeSpan.FromMilliseconds(500));
                isTestProjectCreated = _gfServerConnector.IsTestProjectExist(_executionID);

                if(waitCount > 10)
                {
                    Console.WriteLine("Test project creation error!");
                    System.Environment.Exit(-1);
                }
            }

            _testProject = (TestProject)_gfServerConnector.GetTestProject(_executionID).Data;
            _scriptsToRun = new ScriptsToRun(_gfServerConnector, _scriptRoot, _testProject.ScriptsToRun);

            bool running = true;
            _currentCount = 0;
            SummaryReport summaryReport = new SummaryReport(_outputRoot, _testProject.ExecutionID, _gfServerConnector.GFServer);
            _startTime = DateTime.Now;
            Console.WriteLine("-----------------------------------------------------------------");
            Console.WriteLine($"Test Proejct Author : {_testProject.Author}");
            Console.WriteLine($"Repeat Condition : {_testProject.GetRepeat()}");
            Console.WriteLine($"Device Under Test : {_testProject.DUT.DeviceAddress}");
            Console.WriteLine($"Script To Run :");
            Console.WriteLine(string.Join("\r\n", _scriptsToRun.Scripts.Values.ToList().Select(x => Path.GetFileName(x))));
            Console.WriteLine("-----------------------------------------------------------------");


            while (running)
            {
                _currentCount++;
                HP.GFriend.Core.CommonExecutionInfo.CurrentRepeatCount = _currentCount;
                Console.WriteLine("================================================================");
                Console.WriteLine($"Repeat :: {_currentCount}");
                Console.WriteLine("================================================================");
                // Run Test script
                foreach (KeyValuePair<int,string> script in _scriptsToRun.Scripts)
                {
                    TestResult testResult = new TestResult(_testProject.ExecutionID);
                    testResult.Repetation = _currentCount;
                    testResult.TestScriptId = script.Key;
                    TestDataManager testDataManager;
                    string outDir = Path.Combine(_outputRoot, $"{_currentCount}_{Path.GetFileNameWithoutExtension(script.Value)}");
                    
                    // Start test execution
                    testResult.StartTime = DateTime.Now;
                    Runner.InitGFRunner(new List<DeviceUnderTest> { _testProject.DUT }, outDir);
                    Runner.Run(script.Value, out testDataManager);

                    // Update Result
                    testResult.EndTime = DateTime.Now;
                    Report report = new Report(Path.Combine(outDir, "output.xml"));
                    testResult.Pass = report.Pass;
                    testResult.Fail = report.Fail;
                    testResult.Error = report.Error;

                    _gfServerConnector.UpdateTestResult(testResult);
                }

                

                // Check end condition
                if(_testProject.RepeatMethod.Equals(RepeatType.Count))
                {
                    if(_currentCount >= _testProject.RepeatCount)
                    {
                        running = false;
                        _testProject.Status = ExecutionStatus.Completed;
                    }
                }
                else if(_testProject.RepeatMethod.Equals(RepeatType.Duration))
                {
                    if(DateTime.Now > _startTime.Add(_testProject.RepeatDuration))
                    {
                        running = false;
                        _testProject.Status = ExecutionStatus.Completed;
                    }
                }


                if(_gfServerConnector.IsTestProjectCanceling(_testProject.ExecutionID))
                {
                    running = false;
                    _testProject.Status = ExecutionStatus.Canceled;
                }
                
            }
            string summaryReportPath = summaryReport.Generate();
            _testProject.TestReportURL = $"{_gfServerConnector.GFServer}/result/{_testProject.ExecutionID}/SummaryReport.html";
            _gfServerConnector.EndTestProject(_testProject);
            _gfServerConnector.UploadTestResult(_testProject.ExecutionID, _outputRoot);

            // Configure Summary Report for embed in email
            string reportTxt = File.ReadAllText(summaryReportPath);
            reportTxt = reportTxt.Replace("HREF=\".", $"HREF=\"{_gfServerConnector.GFServer}/result/{_testProject.ExecutionID}");
            File.WriteAllText(Path.Combine(_outputRoot, "email.html"), reportTxt);
        }
    }
}

using HP.GFriend.GFLogger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace HP.GFriend.Core.Execution.Spec
{
    public static class RunSpecRunner
    {
        public static event EventHandler<RunSpecEventArgs> SuiteStart;
        public static event EventHandler<RunSpecEventArgs> SuiteEnd;

        public static TestRuns TestRunFromFile(string testRunSpec)
        {
            XmlReader reader = XmlReader.Create(testRunSpec);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(TestRuns));
            TestRuns testRuns = (TestRuns)xmlSerializer.Deserialize(reader);
            return testRuns;
        }

        public static void Run(string testRunSpec, bool headlessMode, TestDataManager testDataManager, Dictionary<string, string> executionVariables = null)
        {
            TestRuns testRuns = TestRunFromFile(testRunSpec);
            string runSpecRoot = Path.GetDirectoryName(testRunSpec);

            if (string.IsNullOrEmpty(testRuns.OutputPath))
            {
                testRuns.OutputPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }

            if (string.IsNullOrEmpty(Path.GetPathRoot(testRuns.OutputPath)))
            {
                // If output path is relative path, make absolute with test run spec root
                string strOutputPath = Path.Combine(runSpecRoot, testRuns.OutputPath);
                Uri uri = new Uri(strOutputPath);
                testRuns.OutputPath = Path.GetFullPath(uri.LocalPath);
            }

            for (int i = 0; i < testRuns.RepeatCount; i++)
            {
                Console.WriteLine($"==== Test Run Repeat :: {i + 1} ====");
                foreach (TestRun testRun in testRuns.TestsToRun)
                {
                    if (!File.Exists(testRun.TestSuitePath))
                    {
                        // Handling relative path
                        testRun.TestSuitePath = Path.Combine(runSpecRoot, testRun.TestSuitePath);
                    }

                    for (int j = 0; j < testRun.RepeatCount; j++)
                    {
                        Console.WriteLine($"==== Test Suite Repeat :: {j + 1} ====");

                        // use test suite's output path if exist
                        string outputPath = testRun.OutputPath;
                        if (string.IsNullOrEmpty(outputPath))
                        {
                            outputPath = Path.Combine(testRuns.OutputPath,
                            string.Join("_", Path.GetFileNameWithoutExtension(testRun.TestSuitePath),
                            (i + 1).ToString("000"), (j + 1).ToString("000"),
                            DateTime.Now.ToString("yyyyMMdd_HHmmss")));
                        }

                        if (string.IsNullOrEmpty(Path.GetPathRoot(outputPath)))
                        {
                            // If output path is relative path, make absolute with test run spec root
                            string strOutputPath = Path.Combine(runSpecRoot, outputPath);
                            Uri uri = new Uri(strOutputPath);
                            outputPath = Path.GetFullPath(uri.LocalPath);
                        }

                        if (!Directory.Exists(outputPath))
                        {
                            Directory.CreateDirectory(outputPath);
                        }

                        string defaultDevice = null;
                        if (!string.IsNullOrEmpty(testRun.DefaultDevice))
                        {
                            defaultDevice = testRun.DefaultDevice;
                        }

                        if (testRun.TestCaseToRun?.Count == 0)
                        {
                            testRun.TestCaseToRun = null;
                        }
                        Console.WriteLine($"\r\n ### Test Suite : {testRun.TestSuitePath} ###");
                        SuiteStart?.Invoke(null, new RunSpecEventArgs(testRuns, testRun, testRun.TestSuitePath, outputPath));
                        bool initPassed = true;
                        try
                        {
                            Runner.InitGFRunner(testRun.DeviceUnderTests, outputPath, headlessMode);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("Error during initilaize", ex);
                            Console.WriteLine($"Error during test initilization.");
                            Console.WriteLine(ex.ToString());
                            initPassed = false;
                        }
                        if (initPassed)
                        {
                            try
                            {
                                Runner.Run(testRun.TestSuitePath, out testDataManager, testRun.TestCaseToRun, null, defaultDevice, null, executionVariables);
                                SuiteEnd?.Invoke(null, new RunSpecEventArgs(testRuns, testRun, testRun.TestSuitePath, outputPath));
                            }
                            catch(ThreadAbortException)
                            {
                                Logger.Error("Test execution is aborted.");
                            }
                            catch (Exception ex)
                            {
                                Logger.Error("Error during test run", ex);
                                Console.WriteLine($"Error during test run.");
                                Console.WriteLine(ex.ToString());
                            }
                        }

                    }
                }
            }
        }
    }
}

using HP.GFriend.Keywords;
using HP.GFriend.GFLogger;
using System.Collections.Generic;
using System;
using System.IO;
using HP.GFriend.Support;
using System.Linq;
using HP.GFriend.Core.BuiltIn;

namespace HP.GFriend.Core.Execution
{
    public  class Runner:IDisposable
    {
        private static List<DeviceUnderTest> _dut;
        private static string _outputDir = null;
        private static bool _headlessMode = false;
        private bool disposedValue;

        public static void InitGFRunner(List<DeviceUnderTest> dut, string outputDirIn, bool headlessMode = false)
        {
            _dut = dut;
            _outputDir = outputDirIn;
            _headlessMode = headlessMode;

            // Statement.Init(dut, outputDirIn, null);

            BuiltInLibrary.OnLatencyChanged += (sender, e) =>
            {
                var latencyArgs = e as LatencyStatusChangedEventArgs;
                if (latencyArgs != null)
                {
                    if (!latencyArgs.IsNetworkDown)
                    {
                        Console.WriteLine("Network is up. Resuming test case execution...");
                        Logger.Debug("Network is up. Resuming test case execution...");
                        BuiltInLibrary.NetworkAvailableEvent.Set(); // Resume
                    }
                }
            };

            BuiltInLibrary.OnNetworkUnreachble += (sender, e) =>
            {
                var latencyArgs = e as LatencyStatusChangedEventArgs;
                if (latencyArgs != null)
                {
                    if (latencyArgs.IsNetworkDown)
                    {
                        Console.WriteLine("Network is down. Pausing test case execution...");
                        Logger.Debug("Network is down. Pausing test case execution...");
                        BuiltInLibrary.NetworkAvailableEvent.Reset(); // Pause
                    }
                }
            };
        }

        public static void Run(string tsPath, out TestDataManager testDataManager,
            List<string> tcToRun = null, string libraryPath = null, string defaultDut = null,
            Dictionary<string,string> variables = null, Dictionary<string, string> executionVariables = null,
            List<int> tcIdsToRun = null)
        {
            tsPath = HP.GFriend.Keywords.Support.Utils.GetAbsolutePath(tsPath, Environment.CurrentDirectory);
            GFriendLoggerServices.InitLogger(_outputDir);
            Executor executor = null;

            try
            {
                try
                {
                testDataManager = Parser.ParseTestSuite(tsPath, executionVariables);
                TestSuite aTestSuite = testDataManager.TargetTestSuite;
                if(!string.IsNullOrEmpty(defaultDut))
                {
                    testDataManager.ChangeDefaultDUTId(defaultDut);
                }

                foreach(Library lib in testDataManager.AllUsedLib)
                {
                    lib.Load(lib.NameAs);
                }
                executor = new Executor(_dut, testDataManager);
                executor.InitExecutor(_outputDir, tsPath);
                testDataManager.Executor = executor;  // ADD THIS LINE - Assign executor to testDataManager

                Reporter.InitReport(_outputDir);

                CommonExecutionInfo.SetVariable(SystemVariables.SCRIPT_FOLDER, Path.GetDirectoryName(tsPath));
                CommonExecutionInfo.ScriptFolder = Path.GetDirectoryName(tsPath);


                if (testDataManager.Variables != null && testDataManager.Variables.Count>0)
                {
                    Logger.Trace("TestDataManager Variables Count : " + testDataManager.Variables.Count);
                    Logger.Trace("CommonExecutionInfo variables Count : " + CommonExecutionInfo.GetVariablesCount());
                    foreach (KeyValuePair<string, string> variable in testDataManager.Variables)
                    {
                        CommonExecutionInfo.SetVariable(variable.Key, variable.Value);
                    }
                }

                if (variables != null && variables.Count>0)
                {
                    Logger.Trace("Variables Count : " + variables.Count);
                    Logger.Trace("CommonExecutionInfo variables Count : " + CommonExecutionInfo.GetVariablesCount());
                    foreach (KeyValuePair<string, string> variable in variables)
                    {
                        if(!CommonExecutionInfo.HasVariable(variable.Key))
                        {
                            CommonExecutionInfo.SetVariable(variable.Key, variable.Value);
                        }
                        else
                        {
                            Logger.Trace($"The variable \"{variable}\" already exists in the variable file");
                        }
                    }
                }

                if(_headlessMode)
                {
                    CommonExecutionInfo.IsHeadlessMode = true;
                }
                else
                {
                    CommonExecutionInfo.IsHeadlessMode = false;
                }

                // Prefer the stable-Id based selection whenever it's supplied: it's the only
                // identifier that can never be ambiguous (unlike Name, which can collide
                // between two test cases sharing the same displayed text) or stale (unlike a
                // UI row index/position, which can change on refresh/sort).
                if (tcIdsToRun != null)
                {
                    aTestSuite.Run(testDataManager, tcIdsToRun);
                }
                else if (tcToRun != null)
                 {
                    aTestSuite.Run(testDataManager, tcToRun);
                }
                else
                {
                    aTestSuite.Run(testDataManager);
                }
                
                aTestSuite.PrintResult();

                Reporter.EndReport();
                Reporter.GenerateReport();
                if (CommonExecutionInfo.SendMailwithReportSummary == true)
                {
                    BuiltInLibrary builtInLibrary = new BuiltInLibrary();
                    builtInLibrary.SendEmail(CommonExecutionInfo.GetVariable("toEmail"), CommonExecutionInfo.GetVariable("mailbody"), false, true);
                }
                }
                finally
                {
                    // Ensure the executor (and every library's live connection/session/timer) is
                    // always released before this method returns, regardless of success or
                    // failure. Without this, a device's connection/session state can leak into
                    // the next Run() call (e.g. the next device in a sequential multi-device
                    // execution), causing the following device to fail to connect/initialize.
                    executor?.Dispose();
                }
            }
            catch(Exception ex)
            {
                Logger.Error("Unexpected Error at", ex);
                GFriendLoggerServices.Dispose();
                if(ex.InnerException != null)
                {
                    throw ex.InnerException;
                }
                throw ex;
            }


            GFriendLoggerServices.Dispose();
        }
        public static void HandleMergeAndReportGeneration(string PreviousOutputFolderPath, string outputDir)
        {
            Utils.MergeOutputFilesAndGenerateReport(PreviousOutputFolderPath, outputDir);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~Runner()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

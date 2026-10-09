using HP.GFriend.Core;
using HP.GFriend.Core.Execution;
using HP.GFriend.GFLogger;
using HP.GFriend.Keywords;
using HP.GFriend.Support;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace HP.GFriend.GFWcfService
{
    public class GFRemoteService : IGFRemoteService
    {
        private static bool _inUse = false;
        private static string _sessionId = null;
        private static TestDataManager _testDataManager;

        public string GetVersion()
        {
            return typeof(Executor).Assembly.GetName().Version.ToString();
        }

        public bool InUse()
        {
            return _inUse;
        }

        public void StartSession(string sessionId)
        {
            _sessionId = sessionId;
            string sessionPath = Path.Combine(Configurations.DEFAULT_PATH, sessionId);
            string scriptPath = Path.Combine(sessionPath, "script");
            string resultPath = Path.Combine(sessionPath, "output");
            Directory.CreateDirectory(sessionPath);
            Directory.CreateDirectory(resultPath);
            Directory.CreateDirectory(scriptPath);
            GFriendLoggerServices.InitLogger(resultPath, $"log_{sessionId}.txt");
            Logger.Trace($"Session Started with Session ID : {sessionId}");

        }

        public string SetFilesToUse(GFFile file)
        {

            string sessionId = file.SessionId;
            string filePath = file.FilePath;
            

            try
            {
                if (!sessionId.Equals(_sessionId))
                {
                    throw new AmbiguousMatchException("Session id is not valid");
                }
                string actualPath = Path.Combine(Configurations.DEFAULT_PATH, sessionId, "script", filePath);
                if (!Directory.Exists(Path.GetDirectoryName(actualPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(actualPath));
                }
                using (FileStream fileStrem = new FileStream(actualPath, FileMode.Create))
                {

                    fileStrem.Write(file.FileByte, 0, file.FileByte.Length);
                }
            }
            catch(Exception ex)
            {
                Logger.Error("File handling error", ex);
                return ex.ToString();
            }
            return "success";
        }
        public void Initialize(string sessionId, List<DeviceUnderTest> duts, List<string> usings)
        {
            if(!sessionId.Equals(_sessionId))
            {
                throw new AmbiguousMatchException("Session id is not valid");
            }
            string resultPath = Path.Combine(Configurations.DEFAULT_PATH, sessionId, "output");
            string tsPath = Path.Combine(Configurations.DEFAULT_PATH, sessionId, "script");
            _inUse = true;
            
            //GFriendLoggerServices.InitLogger(resultPath, $"log_{sessionId}.txt");
            Logger.Trace("Remote Executor Initialize");

            _testDataManager = new TestDataManager();
            
            foreach(string line in usings)
            {
                Parser.ParseUsing(line, tsPath, _testDataManager);
            }
            _testDataManager.PostActionAfterParsingUsings();
            foreach (Library lib in _testDataManager.AllUsedLib)
            {
                Logger.Debug($"Loading library : {lib.Name}");
                lib.Load(lib.NameAs);
            }
            // FIX: Create an Executor instance to call InitExecutor (non-static method)
            var executor = new Executor(duts, _testDataManager);
            executor.InitExecutor(resultPath, tsPath);
            
        }

        public async Task<string> RunRemote(string sessionId, string blockId, RemoteBlock remoteBlock, int repeatCount = -1, int stackLevel = 0, Dictionary<string, string> arguments = null)
        {
            string resultPath = Path.Combine(Configurations.DEFAULT_PATH, sessionId, "output");
            Reporter.InitReport(resultPath, $"output_{blockId}.xml");
            Dictionary<string, string> attributes = new Dictionary<string, string>();
            attributes.Add("SessionID", sessionId);
            attributes.Add("BlockID", blockId);
            Reporter.WriteToOutput("Remote", attributes, false);
            Reporter.WriteToOutput("StartTime", Utils.GetTime());
            remoteBlock.Result = "PASS";
            CommonBlock commonBlock = new CommonBlock();
            IGFRunnable currentBlock = commonBlock;
            Stack<IGFRunnable> blocks = new Stack<IGFRunnable>();
            
            int offset = remoteBlock.BlockContents[0].Item1 -1;
            int previousDepth = 1;
            blocks.Push(currentBlock);

            foreach (Tuple<int, string> content in remoteBlock.BlockContents)
            {
                int currentDepth = content.Item1 - offset;
                while(previousDepth != currentDepth)
                {
                    if (previousDepth > currentDepth)
                    {
                        previousDepth--;
                        currentBlock = blocks.Pop();
                        Logger.Debug($"Pop : {currentBlock.OriginalStatement}");
                    }
                    else
                    {
                        previousDepth++;
                        blocks.Push(currentBlock);
                        Logger.Debug($"Pushing : {currentBlock.OriginalStatement}");
                    }
                }
                currentBlock = blocks.Peek();
                Logger.Debug($"{content.Item1 - offset} : {content.Item2}");
                currentBlock = Parser.ParseLine(content.Item1 - offset, content.Item2, currentBlock, null);
            }

            commonBlock.Run(_testDataManager, repeatCount, stackLevel, arguments);
            if (commonBlock.Result.ToUpper().Equals("FAIL"))
            {
                remoteBlock.Result = "FAIL";
            }
            else if (commonBlock.Result.ToUpper().Equals("ERROR"))
            {
                remoteBlock.Result = "ERROR";
            }

            Reporter.WriteToOutput("Result", remoteBlock.Result);
            Reporter.WriteToOutput("EndTime", Utils.GetTime());
            Reporter.WriteToOutput("Remote", false);

            return remoteBlock.Result;
        }

        public List<string> GetOutputFileList(string sessionId)
        {
            List<string> outputs = null;
            try
            {
                string resultPath = Path.Combine(Configurations.DEFAULT_PATH, sessionId, "output");
            
            
                outputs = new List<string>();
                foreach (string filePath in Directory.GetFiles(resultPath, "*", SearchOption.AllDirectories))
                {
                    outputs.Add(filePath.Replace(resultPath, string.Empty).Trim(Path.DirectorySeparatorChar));
                }
                Reporter.EndReport();
            }
            catch(Exception ex)
            {
                Logger.Error("What error", ex);
            }
            

            GFriendLoggerServices.Dispose();
            
            return outputs;
        }

        public GFFile GetOutputFile(string sessionId, string targetFile)
        {
            string filePath = Path.Combine(Configurations.DEFAULT_PATH, sessionId, "output", targetFile);
            byte[] fileContents = File.ReadAllBytes(filePath);
            return new GFFile(sessionId, targetFile, fileContents);
        }

        public void EndSession(string sessionId)
        {
            foreach (Library lib in _testDataManager.AllUsedLib)
            {
                lib.Dispose();
            }
            _testDataManager = null;
            _sessionId = null;
        }


        public void Update(string gfServerEndpoint)
        {
            // Copy GFUpdater to temp path
            string tempLocation = Path.Combine(Path.GetTempPath(), "GFUpdater");
            if (Directory.Exists(tempLocation))
            {
                Directory.Delete(tempLocation, true);
            }
            Directory.CreateDirectory(tempLocation);
            string updaterLocation = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Updater");

            foreach (string dirPath in Directory.GetDirectories(updaterLocation, "*", SearchOption.AllDirectories))
            {
                string newPath = dirPath.Replace(updaterLocation, tempLocation);
                if (!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }
            }

            foreach (string filePath in Directory.GetFiles(updaterLocation, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(filePath, filePath.Replace(updaterLocation, tempLocation), true);
            }

            Process p = new Process();
            ProcessStartInfo info = new ProcessStartInfo(Path.Combine(tempLocation, "GFUpdater.exe"));
            info.Arguments = $"-g -a -p \"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\" -h {gfServerEndpoint}";
            p.StartInfo = info;
            p.Start();
            Environment.Exit(0);
        }

        public void ForceReset()
        {
            throw new NotImplementedException();
        }

        
        public void Dispose(string sessionId)
        {
            throw new NotImplementedException();
        }

       
    }
}

using HP.GFriend.GFLogger;
using OpenQA.Selenium.Appium.iOS;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace HP.GFriend.Utils.Appium
{
    public static partial class AppiumServerUtils
    {
        private static Dictionary<string, Task<string>> _activeWDA;
        private static Dictionary<string, CancellationTokenSource> _cancellations;

        #region WDA with Enterprise Account
        public static bool IsWdaEntSetup()
        {
            SshCommand checkWdaEnt = _sshClient.RunCommand("ls .wdaentused");
            using (StringReader reader = new StringReader(checkWdaEnt.Result))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains(".wdaentused"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static string GetPrebuiltWDAPath()
        {
            SshCommand checkWdaEnt = _sshClient.RunCommand("source .gfvariable;echo $WDA_Ent");
            using (StringReader reader = new StringReader(checkWdaEnt.Result))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    return line;
                }
            }
            return string.Empty;
        }
        #endregion

        #region Web Driver Agent
        public static string ExecuteWDA(string udid)
        {
            if (_activeWDA == null)
            {
                _activeWDA = new Dictionary<string, Task<string>>();
                _cancellations = new Dictionary<string, CancellationTokenSource>();
            }

            // Initialize entry early to avoid race conditions
            _activeWDA[udid] = null;

            CancellationTokenSource source = new CancellationTokenSource();
            _cancellations[udid] = source;
            
            // Start WDA async task
            Task<string> executeTask = Task.Run(() => ExecuteWDASync(udid), source.Token);
            _activeWDA[udid] = executeTask;
            
            // Wait up to 30 seconds for WDA to actually start (indicated by the test case line)
            DateTime endTime = DateTime.Now.AddSeconds(30);
            while (true)
            {
                // Check if task has begun executing and reached the "started" checkpoint
                if (executeTask.Status == TaskStatus.Running)
                {
                    // Give ExecuteWDASync time to detect the startup line
                    Thread.Sleep(500);
                    
                    // Verify WDA actually registered (ExecuteWDASync sets _activeWDA[udid] = null when ready)
                    if (_activeWDA.ContainsKey(udid))
                    {
                        break;  // WDA is running
                    }
                }
                
                if (endTime < DateTime.Now)
                {
                    Logger.Error($"WDA startup timeout for device {udid}");
                    return string.Empty;  // Timeout
                }
                
                if (executeTask.IsCompleted || executeTask.IsCanceled || executeTask.IsFaulted)
                {
                    Logger.Error($"WDA task failed for device {udid}: {executeTask.Status}");
                    return string.Empty;  // Task failed
                }
                
                Thread.Sleep(1000);
            }
            
            // Restore the task reference after ExecuteWDASync set it to null
            _activeWDA[udid] = executeTask;
            
            // Parse team ID
            SshCommand devGrepDevTeam = _sshClient.RunCommand("source ~/.gfvariable;grep DEVELOPMENT_TEAM \"$WDA_PATH/WebDriverAgent.xcodeproj/project.pbxproj\"");
            using (StringReader reader = new StringReader(devGrepDevTeam.Result))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains("="))
                    {
                        string teamId = line.Split('=')[1].Trim();
                        teamId = teamId.Trim(';').Trim('\"');
                        if (!string.IsNullOrEmpty(teamId))
                        {
                            return teamId;
                        }
                    }
                }
            }
            
            Logger.Debug($"Team ID not found for device {udid}, but WDA is running");
            return string.Empty;
        }


        private static string ExecuteWDASync(string udid)
        {
            var cmd = _sshClient.CreateCommand($"security unlock-keychain -p {UserPassword} \"$HOME/Library/Keychains/login.keychain\" &&source ~/.gfvariable && xcodebuild -project $WDA_PATH/WebDriverAgent.xcodeproj -scheme WebDriverAgentRunner -destination 'id={udid}' test");

            var result = cmd.BeginExecute();

            using (var reader = new StreamReader(cmd.OutputStream))
            {
                while (!reader.EndOfStream || !result.IsCompleted)
                {
                    string line = reader.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        Logger.Debug(line);
                        if (line.Contains("Test Case '-[UITestingUITests testRunner]' started."))
                        {
                            _activeWDA[udid] = null;
                        }
                    }
                }
            }
            try
            {
                cmd.EndExecute(result);
                return cmd.Result;
            }
            catch (Exception) { return string.Empty; }

            
        }

        public static bool IsWDARunning(string udid)
        {
            if (_activeWDA != null && _activeWDA.ContainsKey(udid)) return true;
            return false;
        }

        public static void StopWDA(string udid)
        {
            if(_activeWDA != null && _activeWDA.ContainsKey(udid) && _cancellations.ContainsKey(udid))
            {
                Task<string> executeTask = _activeWDA[udid];
                CancellationTokenSource source = _cancellations[udid];
                if(!executeTask.IsCompleted && !executeTask.IsCanceled && !executeTask.IsFaulted)
                {
                    source.Cancel();
                }
                
                _activeWDA.Remove(udid);
                _cancellations.Remove(udid);
            }    
        }
        #endregion

        #region Appium
        public static bool ExecuteAppiumServer()
        {
            if (CheckAppiumServer()) return true;
            SudoExecute("sudo pmset -a sleep 0; sudo pmset -a hibernatemode 0; sudo pmset -a disablesleep 1;");
            
            _sshClient.RunCommand("echo 'nohup appium --default-capabilities \\x27{\"newCommandTimeout\": 0}\\x27 > ~/gfappium.log &!' > ~/appium.sh");
            Execute("/bin/zsh ~/appium.sh", TimeSpan.FromSeconds(1));

            return CheckAppiumServer();
        }
        #endregion
    }
}

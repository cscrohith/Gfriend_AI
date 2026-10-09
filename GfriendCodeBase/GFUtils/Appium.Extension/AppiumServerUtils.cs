using HP.GFriend.GFLogger;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace HP.GFriend.Utils.Appium
{
    public static partial class AppiumServerUtils
    {
        public static string HostName { get; private set; }
        public static int Port { get; private set; }
        public static string UserName { get; private set; }
        private static string UserPassword { get; set; }
        public static string PromptName { get; private set; }
        public static string deviceModelName = string.Empty;
        public static string deviceId = string.Empty;
        public static string androidVersion = string.Empty;
        public static string macVersion = string.Empty;
        public static bool adblist = false;

        private static ConnectionInfo _connectionInfo;
        private static SshClient _sshClient;

        public static void Initialize(string hostName, int port, string userName, string userPassword)
        {
            HostName = hostName;
            Port = port;
            UserName = userName;
            UserPassword = userPassword;
            PasswordAuthenticationMethod auth = new PasswordAuthenticationMethod(UserName, UserPassword);
            _connectionInfo = new ConnectionInfo(HostName, Port, UserName, auth);
            _sshClient = new SshClient(_connectionInfo);
            _sshClient.Connect();
        }

        public static void PushFile(Stream targetStream, string saveTo)
        {
            try
            {
                using (SftpClient client = new SftpClient(_connectionInfo))
                {
                    client.Connect();
                    client.UploadFile(targetStream, saveTo);
                    client.Disconnect();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static bool CheckFile(string file, TimeSpan waitTime)
        {
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime + waitTime;
            bool fileExist = false;

            try
            {
                using (SftpClient client = new SftpClient(_connectionInfo))
                {
                    client.Connect();
                    while (!(fileExist = client.Exists(file)))
                    {
                        if (DateTime.Now > endTime)
                        {
                            continue;
                        }

                        Thread.Sleep(TimeSpan.FromMilliseconds(500));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return fileExist;
        }

        public static string SudoExecute(string command)
        {
            Regex promptRegex = new Regex(@"[#$>%]"); // regular expression for matching terminal prompt
            var modes = new Dictionary<Renci.SshNet.Common.TerminalModes, uint>();
            using (var stream = _sshClient.CreateShellStream("xterm", 255, 50, 800, 600, 1024, modes))
            {
                stream.Write($"sudo {command}\n");
                Regex passwordRegex = new Regex("password|Password");
                string expect = stream.Expect(passwordRegex, TimeSpan.FromSeconds(10));

                if (string.IsNullOrEmpty(expect))
                {
                    return string.Empty;
                }
                stream.Write($"{UserPassword}\n");
                string output = stream.Expect(promptRegex, TimeSpan.FromSeconds(5));
                return output ?? string.Empty;
            }
        }

        public static string removeCodeString(string output)
        {
            return new Regex(@"\x1B\[[^@-~]*[@-~][%]*[\s]*").Replace(output, "");
        }

        public static bool Execute(string command, TimeSpan timeout)
        {
            Regex promptRegex = new Regex(PromptName + @"[~@#$%^&*:\s$%~]*", RegexOptions.Compiled);
            //Regex promptRegex = new Regex(UserName + @"[@#$%$%~][a-zA-Z0-9~@#$%^&*()_+-\s$%~]*", RegexOptions.Compiled);
            var modes = new Dictionary<Renci.SshNet.Common.TerminalModes, uint>();
            using (var stream = _sshClient.CreateShellStream("xterm", 255, 50, 800, 600, 1024, modes))
            {
                stream.Write($"{command}\n");

                for (int i = 0; i < 3; i++)
                {
                    string line = stream.ReadLine(TimeSpan.FromSeconds(3));
                    Console.WriteLine(removeCodeString(line));
                }

                string output = stream.Expect(promptRegex, timeout);
                if (string.IsNullOrEmpty(output))
                {
                    string line;
                    Console.WriteLine($"Command execution failed({command}): time out.");
                    while ((line = stream.ReadLine(TimeSpan.FromSeconds(2))) != null)
                    {
                        Console.WriteLine(removeCodeString(line));
                        // if a termination pattern is known, check it here and break to exit immediately
                    }
                    return false;
                }
                Console.WriteLine(removeCodeString(output));
                return true;
                /***
                string installlog = null;
                while (string.IsNullOrEmpty(installlog = stream.ReadLine(TimeSpan.FromSeconds(10))))
                {
                    Console.WriteLine(installlog);
                }
                return true;
                ***/
            }
        }
        public static string ExecuteWindowsCmd(string command)
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), // 👈 set working dir
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            return string.IsNullOrEmpty(error) ? output : error;
        }
        public static bool Execute(string command, Regex verifypattern, TimeSpan timeout)
        {
            var modes = new Dictionary<Renci.SshNet.Common.TerminalModes, uint>();
            using (var stream = _sshClient.CreateShellStream("xterm", 255, 50, 800, 600, 1024, modes))
            {
                stream.Write($"{command}\n");

                for (int i = 0; i < 3; i++)
                {
                    string line = stream.ReadLine(TimeSpan.FromSeconds(3));
                    Console.WriteLine(removeCodeString(line));
                }

                string expect = stream.Expect(verifypattern, timeout);
                if (string.IsNullOrEmpty(expect))
                {
                    string line;
                    Console.WriteLine($"Command execution failed({command}): Timeout.");
                    while ((line = stream.ReadLine(TimeSpan.FromSeconds(2))) != null)
                    {
                        Console.WriteLine(removeCodeString(line));
                        // if a termination pattern is known, check it here and break to exit immediately
                    }
                    return false;
                }
                Console.WriteLine(removeCodeString(expect));
                return true;
            }
        }

        public static List<PackageInfo> GetCurrentStatus()
        {
            List<PackageInfo> packageInfos = new List<PackageInfo>();

            foreach (Packages package in Enum.GetValues(typeof(Packages)).Cast<Packages>().ToList())
            {
                PackageInfo info = GetPackageInfo(package);
                if (info.Show)
                {
                    packageInfos.Add(GetPackageInfo(package));
                }
            }

            return packageInfos;
        }

        public static PackageInfo GetPackageInfo(Packages package)
        {
            PackageInfo packageInfo = new PackageInfo();
            packageInfo.Package = package;
            packageInfo.PackageInstalledStatus = PackageStatus.UnKnown;
            switch (package)
            {
                case Packages.HomeBrew:
                    GetHomeBrewVersion(packageInfo);
                    break;
                case Packages.Node:
                    GetNodeVersion(packageInfo);
                    break;
                case Packages.Python:
                    GetPythonVersion(packageInfo);
                    break;
                case Packages.AuthorizeIOS:
                    GetAuthorizeIOSVersion(packageInfo);
                    break;
                case Packages.Appium:
                    GetAppiumVersion(packageInfo);
                    break;
                case Packages.Carthage:
                    GetCarthageVersion(packageInfo);
                    break;
                case Packages.XCtrace:
                    GetXCrunXtraceVersion(packageInfo);
                    break;
                case Packages.WDA:
                    GetWDAInstallInfo(packageInfo);
                    break;
                case Packages.IdeviceInstaller:
                    GetIdeviceinstallerVersion(packageInfo);
                    break;
                    /***
                    case Packages.Libimobiledevice:
                        GetLibimobiledeviceVersion(packageInfo);
                        break;
                    ***/
            }

            return packageInfo;
        }

        public static void GetWDAInstallInfo(PackageInfo packageInfo)
        {
            Regex checkingRegex = new Regex(@"\.wdaentused");
            string command = $"ls .wdaent*";
            GetPackageVersion(command, checkingRegex, packageInfo);
            if (packageInfo.PackageInstalledStatus.Equals(PackageStatus.Installed))
            {
                packageInfo.PackageVersion = "Installed for registered device.";
            }
            else
            {
                checkingRegex = new Regex(@"export WDA_PATH");
                command = $"cat .gfvariable";
                GetPackageVersion(command, checkingRegex, packageInfo);
                if (packageInfo.PackageInstalledStatus.Equals(PackageStatus.Installed))
                {
                    packageInfo.PackageVersion = "Installed with personal account.";
                }
                else
                {
                    packageInfo.Show = false;
                }
            }

        }

        public static void GetHomeBrewVersion(PackageInfo packageInfo)
        {
            Regex checkingRegex = new Regex(@"[0-9]*\.[0-9]*\.[0-9]*");
            string command = $"brew -v";
            GetPackageVersion(command, checkingRegex, packageInfo);
        }

        public static void GetAppiumVersion(PackageInfo packageInfo)
        {
            Regex checkingRegex = new Regex(@"[0-9]*\.[0-9]*\.[0-9]*");
            string command = $"appium -v";
            GetPackageVersion(command, checkingRegex, packageInfo);
        }
        public static void GetNodeVersion(PackageInfo packageInfo)
        {
            Regex checkingRegex = new Regex(@"[0-9]*\.[0-9]*\.[0-9]*");
            string command = $"npm show node version";
            GetPackageVersion(command, checkingRegex, packageInfo);
        }

        public static void GetPythonVersion(PackageInfo packageInfo)
        {
            Regex checkingRegex = new Regex(@"[0-9]*\.[0-9]*\.[0-9]*");
            string command = $"python --version";
            GetPackageVersion(command, checkingRegex, packageInfo);
        }

        public static void GetAuthorizeIOSVersion(PackageInfo packageInfo)
        {
            Regex checkingRegex = new Regex(@"[0-9]*\.[0-9]*\.[0-9]*");
            string command = $"npm show authorize-ios version";
            GetPackageVersion(command, checkingRegex, packageInfo);
        }

        public static void GetCarthageVersion(PackageInfo packageInfo)
        {
            Regex checkingRegex = new Regex(@"[0-9]*\.[0-9]*\.[0-9]*");
            string command = $"carthage version";
            GetPackageVersion(command, checkingRegex, packageInfo);
        }

        public static void GetLibimobiledeviceVersion(PackageInfo packageInfo)
        {
            Regex checkingRegex = new Regex(@"[0-9]*\.[0-9]*\.[0-9]*");
            string command = $"brew info libimobiledevice";
            GetPackageVersion(command, checkingRegex, packageInfo);
        }
        public static void GetIdeviceinstallerVersion(PackageInfo packageInfo)
        {
            Regex checkingRegex = new Regex(@"[0-9]*\.[0-9]*\.[0-9]*");
            string command = $"ideviceinstaller -v";
            GetPackageVersion(command, checkingRegex, packageInfo);
        }

        public static void GetIDBVersion(PackageInfo packageInfo)
        {
            Regex checkingRegex = new Regex(@"[0-9]+\.[0-9]*\.[0-9]*");
            string command = $"brew info idb-companion";
            GetPackageVersion(command, checkingRegex, packageInfo);
        }

        public static void GetXCrunVersion(PackageInfo packageInfo)
        {
            Regex checkingRegex = new Regex(@"(?<=\s)[0-9]+(?=.)");
            string command = $"xcrun --version";
            GetPackageVersion(command, checkingRegex, packageInfo);
        }

        public static void GetXCrunXtraceVersion(PackageInfo packageInfo)
        {
            Regex checkingRegex = new Regex(@"(?<=\s)[0-9]+\.[0-9]*(?=.)");
            string command = $"xcrun xctrace version";
            GetPackageVersion(command, checkingRegex, packageInfo);
        }

        public static void GetPackageVersion(string command, Regex checkingRegex, PackageInfo packageInfo)
        {
            var modes = new Dictionary<Renci.SshNet.Common.TerminalModes, uint>();
            using (var stream = _sshClient.CreateShellStream("xterm", 255, 50, 800, 600, 1024, modes))
            {
                stream.Write($"{command}\n");
                for (int i = 0; i < 3; i++)
                {
                    stream.ReadLine();
                }

                string expect = stream.Expect(checkingRegex, TimeSpan.FromSeconds(3));

                if (string.IsNullOrEmpty(expect))
                {
                    packageInfo.PackageInstalledStatus = PackageStatus.NotInstalled;
                }
                else
                {
                    packageInfo.PackageInstalledStatus = PackageStatus.Installed;
                    packageInfo.PackageVersion = checkingRegex.Match(expect).Value;
                }
            }
        }

        #region Installation
        // Check if installed. If installed, return true
        // if not installed install component

        public static void InstallPackage(PackageInfo packageInfo)
        {

            switch (packageInfo.Package)
            {
                case Packages.HomeBrew:
                    InstallHomeBrew();
                    GetHomeBrewVersion(packageInfo);
                    break;
                case Packages.Node:
                    InstallNode();
                    GetNodeVersion(packageInfo);
                    break;
                case Packages.Python:
                    InstallPython();
                    GetPythonVersion(packageInfo);
                    break;
                case Packages.AuthorizeIOS:
                    InstallAuthorizeIOS();
                    GetAuthorizeIOSVersion(packageInfo);
                    break;
                case Packages.Appium:
                    InstallAppium();
                    GetAppiumVersion(packageInfo);
                    break;
                case Packages.Carthage:
                    InstallCarthage();
                    GetCarthageVersion(packageInfo);
                    break;
                case Packages.XCtrace:
                    InstallXCrunDevToolPath();
                    GetXCrunXtraceVersion(packageInfo);
                    break;
                case Packages.IdeviceInstaller:
                    InstallIdeviceinstaller();
                    GetIdeviceinstallerVersion(packageInfo);
                    break;
                    /***
                 * Below packages are installed when HomeBrew does.
                case Packages.Libimobiledevice:
                    InstallLibimobiledevice();
                    GetLibimobiledeviceVersion(packageInfo);
                    break;
                ***/
            }
        }

        public static bool InstallHomeBrew()
        {
            Regex promptRegex = new Regex(PromptName + @"[~@#$%^&*:\s$%~]*", RegexOptions.Compiled); // regular expression for matching terminal prompt
            string command = $"/bin/bash -c \"$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)\"";

            var modes = new Dictionary<Renci.SshNet.Common.TerminalModes, uint>();
            using (var stream = _sshClient.CreateShellStream("xterm", 255, 50, 800, 600, 1024, modes))
            {
                Console.WriteLine("Installing HomeBrew...(it takes about 10 minutes)");
                stream.Write($"{command}\n");

                Regex passwordRegex = new Regex("password|Password");
                string expect = stream.Expect(passwordRegex, TimeSpan.FromSeconds(10));

                if (string.IsNullOrEmpty(expect))
                {
                    string line;
                    Console.WriteLine($"Command execution failed({command}) : Timeout waiting for password input prompt.");
                    while ((line = stream.ReadLine(TimeSpan.FromSeconds(2))) != null)
                    {
                        Console.WriteLine(removeCodeString(line));
                        // if a termination pattern is known, check it here and break to exit immediately
                    }
                    return false;
                }

                stream.Write($"{UserPassword}\n");

                Regex ReutndRegex = new Regex("^Press RETURN");
                string output = stream.Expect(ReutndRegex, TimeSpan.FromSeconds(10));
                stream.Write($"\n");

                output = stream.Expect(promptRegex, TimeSpan.FromSeconds(600));

                if (string.IsNullOrEmpty(output))
                {
                    string line;
                    Console.WriteLine($"Command execution failed({command}): Timeout.");
                    while ((line = stream.ReadLine(TimeSpan.FromSeconds(2))) != null)
                    {
                        Console.WriteLine(removeCodeString(line));
                        // if a termination pattern is known, check it here and break to exit immediately
                    }
                    return false;
                }
                Console.WriteLine(removeCodeString(output));
                return true;
            }
        }

        public static bool InstallAppium()
        {
            return Execute("npm install -g appium", TimeSpan.FromSeconds(300));
        }

        public static bool InstallNode()
        {
            return Execute("brew install node", TimeSpan.FromSeconds(300));
        }
        public static bool InstallPython()
        {
            // sucessRegex = new Regex("^==> Summary");
            return Execute("brew install python", TimeSpan.FromSeconds(300));
        }

        public static bool InstallAuthorizeIOS()
        {
            return Execute("npm install -g authorize-ios", TimeSpan.FromSeconds(300));
        }

        public static bool InstallCarthage()
        {
            return Execute("brew install carthage", TimeSpan.FromSeconds(300));
        }

        public static bool InstallLibimobiledevice()
        {
            return Execute("brew install libimobiledevice", TimeSpan.FromSeconds(300));
        }

        public static bool InstallIdeviceinstaller()
        {
            return Execute("brew install ideviceinstaller", TimeSpan.FromSeconds(300));
        }

        [Obsolete]
        public static bool InstallIDB()
        {
            Regex sucessRegex = new Regex("~ %");
            if (!CheckSilicon())
            {
                if (!Execute("brew tap facebook/fb", sucessRegex, TimeSpan.FromSeconds(300)))
                {
                    return false;
                }

                if (!Execute("brew install idb-companion", sucessRegex, TimeSpan.FromSeconds(300)))
                {
                    return false;
                }
            }
            else
            {
                if (!Execute("arch -x86_64 /usr/local/bin/brew tap Facebook/fb", sucessRegex, TimeSpan.FromSeconds(300)))
                {
                    return false;
                }

                if (!Execute("arch -x86_64 /usr/local/bin/brew install idb-companion", sucessRegex, TimeSpan.FromSeconds(300)))
                {
                    return false;
                }
            }

            if (!Execute("pip3 install fb-idb", sucessRegex, TimeSpan.FromSeconds(300)))
            {
                return false;
            }

            if (CheckIDB())
            {
                return false;
            }

            return true;
        }

        public static bool InstallXCrunDevToolPath()
        {
            return !String.IsNullOrEmpty(SudoExecute("xcode-select -s /Applications/Xcode.app/Contents/Developer"));
        }

        public static bool InstallXcode()
        {
            throw new NotImplementedException();
        }

        public static bool InstallWDA(string uniqueIdentifier)
        {
            // Push WebDriverAgent source
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    byte[] wdaSource = Properties.Resources.WebDriverAgent_master;
                    stream.Write(wdaSource, 0, wdaSource.Length);
                    stream.Position = 0;
                    PushFile(stream, "WebDriverAgent.zip");
                }
            }
            catch (Exception)
            {
                return false;
            }

            // unzip Webdriver agent
            Execute("rm -rf ~/WebDriverAgent-master", TimeSpan.FromSeconds(3));

            if (!Execute("unzip ~/WebDriverAgent.zip", TimeSpan.FromSeconds(30)))
            {
                return false;
            }

            Execute($"cd ~/WebDriverAgent-master;/bin/bash update.sh {uniqueIdentifier}", TimeSpan.FromSeconds(5));
            Execute($"open ~/WebDriverAgent-master/WebDriverAgent.xcodeproj", TimeSpan.FromSeconds(2));
            Execute("rm .wdaentused", TimeSpan.FromSeconds(1));
            return true;

        }

        public static bool InstallWDA()
        {
            // Push Prebuilt WDA
            Execute("rm -rf ~/WDA_Ent.zip", TimeSpan.FromSeconds(3));
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    byte[] wdaSource = Properties.Resources.WDA_Ent;
                    stream.Write(wdaSource, 0, wdaSource.Length);
                    stream.Position = 0;
                    PushFile(stream, "WDA_Ent.zip");
                }
            }
            catch (Exception)
            {
                return false;
            }

            // check file is uploaded
            if (!CheckFile("WDA_Ent.zip", TimeSpan.FromSeconds(5)))
            {
                Console.WriteLine("WDA_Eent.zip file does not exist");
                return false;
            }


            // unzip Webdriver agent
            Execute("rm -rf ~/WDA_Ent", TimeSpan.FromSeconds(3));

            if (!Execute("unzip ~/WDA_Ent.zip", TimeSpan.FromSeconds(30)))
            {
                return false;
            }
            Execute("cd WDA_Ent;echo export WDA_Ent=$(pwd) >> ~/.gfvariable", TimeSpan.FromSeconds(2));

            // Update provisining profile
            byte[] profileByte = AWSConnector.GetProvisioningProfile();
            if (profileByte.Length == 0)
            {
                return false;
            }
            Execute("rm -f ~/WDA_Ent/Build/Products/Debug-iphoneos/WebDriverAgentRunner-Runner.app/embedded.mobileprovision", TimeSpan.FromSeconds(2));
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(profileByte, 0, profileByte.Length);
                stream.Position = 0;
                PushFile(stream, "WDA_Ent/Build/Products/Debug-iphoneos/WebDriverAgentRunner-Runner.app/embedded.mobileprovision");
            }


            Execute($"touch .wdaentused", TimeSpan.FromSeconds(1));
            return true;

        }

        public static bool UninstallWDA()
        {
            return Execute("rm -rf ~/WDA_Ent*;rm ~/.gfvariable;rm ~/.wdaentused", TimeSpan.FromSeconds(10));
        }

        public static string CheckPrompt()
        {
            string hostname = null;
            var modes = new Dictionary<Renci.SshNet.Common.TerminalModes, uint>();
            using (var stream = _sshClient.CreateShellStream("xterm", 255, 50, 800, 600, 1024, modes))
            {
                stream.Write($"uname -n\n");

                for (int i = 0; i < 3; i++)
                {
                    stream.ReadLine(TimeSpan.FromSeconds(3));
                }
                hostname = stream.ReadLine(TimeSpan.FromSeconds(3));
                if (!String.IsNullOrEmpty(hostname))
                {
                    PromptName = hostname.Split('.')[0];
                }
                return PromptName;
            }
        }

        public static bool CheckSilicon()
        {
            Regex checkingRegex = new Regex(@"arm");
            string command = $"uname -m";
            return Execute(command, checkingRegex, TimeSpan.FromSeconds(5));
        }

        public static bool CheckHomebrew()
        {
            Regex checkingRegex = new Regex(@"Homebrew [0-9]");
            string command = $"brew -v";
            return Execute(command, checkingRegex, TimeSpan.FromSeconds(5));
        }

        public static bool CheckAppium()
        {
            Regex checkingRegex = new Regex(@"[0-9]*\.[0-9]*\.[0-9]*");
            string command = $"npm show appium version";
            return Execute(command, checkingRegex, TimeSpan.FromSeconds(5));
        }

        public static bool CheckAppiumServer()
        {
            Task<bool> getTask = Task.Run(() => CheckAppiumStatus());
            getTask.Wait(TimeSpan.FromSeconds(10));
            if (getTask.IsCompleted)
            {
                return getTask.Result;
            }
            return false;
        }

        public static async Task<bool> CheckAppiumStatus()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync($"http://{HostName}:4723/wd/hub/status");
                    string contents = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return false;
                    }
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool CheckNode()
        {
            Regex checkingRegex = new Regex(@"v[0-9]");
            string command = $"node -v";
            return Execute(command, checkingRegex, TimeSpan.FromSeconds(5));
        }

        public static bool CheckPython()
        {
            Regex checkingRegex = new Regex(@"Python [0-9]*\.[0-9]*\.[0-9]*");
            string command = $"python --version";
            return Execute(command, checkingRegex, TimeSpan.FromSeconds(5));
        }

        public static bool CheckAuthorizeIOS()
        {
            Regex checkingRegex = new Regex(@"[0-9]*\.[0-9]*\.[0-9]*");
            string command = $"npm show authorize-ios version";
            return Execute(command, checkingRegex, TimeSpan.FromSeconds(5));
        }

        [Obsolete]
        public static bool CheckIDB()
        {
            Regex checkingRegex = new Regex("usage: idb");
            string command = $"idb -h";
            return Execute(command, checkingRegex, TimeSpan.FromSeconds(5));
        }

        public static bool CheckXCrun()
        {
            Regex checkingRegex = new Regex("Usage: xcrun");
            string command = $"xcrun";
            return Execute(command, checkingRegex, TimeSpan.FromSeconds(5));
        }

        public static bool CheckXCrunDevToolPath()
        {
            Regex checkingRegex = new Regex("Usage: xctrace");
            string command = $"xcrun xctrace";
            return Execute(command, checkingRegex, TimeSpan.FromSeconds(5));
        }

        public static bool CheckCarthage()
        {
            Regex checkingRegex = new Regex("carthage");
            string command = $"brew list";
            return Execute(command, checkingRegex, TimeSpan.FromSeconds(5));
        }

        public static bool CheckLibimobiledevice()
        {
            Regex checkingRegex = new Regex("libimobiledevice");
            string command = $"brew list";
            return Execute(command, checkingRegex, TimeSpan.FromSeconds(5));
        }

        public static bool CheckIdeviceinstaller()
        {
            Regex checkingRegex = new Regex("ideviceinstaller");
            string command = $"brew list";
            return Execute(command, checkingRegex, TimeSpan.FromSeconds(5));
        }
        #endregion


        #region Querying Test Information
        public static List<IOSDevice> GetConnectedDevices()
        {
            List<IOSDevice> devices = new List<IOSDevice>();
            
            try
            {
                if (!CheckXCrun()) // Windows → Android only
                {
                    ProcessAndroidDevicesWindows(devices);
                }
                else // macOS → iOS + Android
                {
                    ProcessIOSDevicesMac(devices);
                    ProcessAndroidDevicesMac(devices);
                }
            }
            catch (Exception ex)
            {
                Logger.Debug($"Unexpected error while detecting connected devices: {ex.Message}");
            }
            
            return devices;
        }

        private static void ProcessAndroidDevicesWindows(List<IOSDevice> devices)
        {
            try
            {
                ProcessAndroidDevices(devices, false);
            }
            catch (Exception ex)
            {
                Logger.Debug($"Error while fetching ADB device details on Windows: {ex.Message}");
            }
        }

        private static void ProcessAndroidDevicesMac(List<IOSDevice> devices)
        {
            try
            {
                ProcessAndroidDevices(devices, true);
            }
            catch (Exception ex)
            {
                Logger.Debug($"Error while fetching ADB device details on macOS: {ex.Message}");
            }
        }

        private static void ProcessAndroidDevices(List<IOSDevice> devices, bool useSudo)
        {
            string adbDevicesOutput = ExecuteAdbCommand(useSudo, "devices");

            if (string.IsNullOrWhiteSpace(adbDevicesOutput) ||
                adbDevicesOutput.Contains("adb: no devices/emulators found") ||
                adbDevicesOutput.Contains("sudo: adb: command not found") ||
                adbDevicesOutput.IndexOf("not found", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Logger.Debug(useSudo
                    ? "No Android devices found on macOS or adb not available."
                    : "No Android devices found on Windows.");
                return;
            }

            List<string> androidDeviceIds = ParseConnectedAndroidDeviceIds(adbDevicesOutput);
            if (androidDeviceIds.Count == 0)
            {
                Logger.Debug("No online Android devices found in adb devices output.");
                return;
            }

            adblist = true;

            foreach (string connectedDeviceId in androidDeviceIds)
            {
                string manufacturer = ExecuteAdbDeviceProp(useSudo, connectedDeviceId, "ro.product.manufacturer");
                string model = ExecuteAdbDeviceProp(useSudo, connectedDeviceId, "ro.product.model");
                string osVersion = ExecuteAdbDeviceProp(useSudo, connectedDeviceId, "ro.build.version.release");

                string normalizedManufacturer = string.IsNullOrWhiteSpace(manufacturer) ? "Android" : manufacturer.Trim();
                string normalizedModel = string.IsNullOrWhiteSpace(model) ? "Device" : model.Trim();
                string normalizedVersion = string.IsNullOrWhiteSpace(osVersion) ? "Unknown" : osVersion.Trim();

                string friendlyName = normalizedModel;
                if (!normalizedModel.StartsWith(normalizedManufacturer, StringComparison.OrdinalIgnoreCase))
                {
                    friendlyName = normalizedManufacturer + " " + normalizedModel;
                }

                IOSDevice androidDevice = new IOSDevice("Android Device", "Android")
                {
                    DeviceName = friendlyName,
                    Udid = connectedDeviceId,
                    DeviceType = "Android",
                    OsVersion = normalizedVersion
                };

                AddDeviceIfUnique(devices, androidDevice);

                deviceModelName = friendlyName;
                deviceId = connectedDeviceId;
                androidVersion = normalizedVersion;
            }
        }

        private static List<string> ParseConnectedAndroidDeviceIds(string adbDevicesOutput)
        {
            List<string> deviceIds = new List<string>();

            using (StringReader reader = new StringReader(adbDevicesOutput))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    string trimmed = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("List of devices attached"))
                    {
                        continue;
                    }

                    string[] parts = trimmed.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2 && string.Equals(parts[1], "device", StringComparison.OrdinalIgnoreCase))
                    {
                        deviceIds.Add(parts[0]);
                    }
                }
            }

            return deviceIds;
        }

        private static string ExecuteAdbDeviceProp(bool useSudo, string udid, string propName)
        {
            string output = ExecuteAdbCommand(useSudo, $"-s \"{udid}\" shell getprop {propName}");
            if (string.IsNullOrWhiteSpace(output))
            {
                return string.Empty;
            }

            foreach (string line in output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed))
                {
                    continue;
                }

                if (trimmed.StartsWith("* daemon", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.StartsWith("List of devices attached", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.StartsWith("error:", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return trimmed;
            }

            return string.Empty;
        }

        private static string ExecuteAdbCommand(bool useSudo, string arguments)
        {
            if (!useSudo)
            {
                return ExecuteWindowsCmd($"adb {arguments}");
            }

            string[] adbCandidates =
            {
                "adb",
                $"/Users/{UserName}/Library/Android/sdk/platform-tools/adb",
                "/usr/local/bin/adb",
                "/opt/homebrew/bin/adb"
            };

            foreach (string adbPath in adbCandidates)
            {
                string command = $"\"{adbPath}\" {arguments}";
                using (var sshCmd = _sshClient.RunCommand(command))
                {
                    string error = sshCmd.Error ?? string.Empty;
                    string result = sshCmd.Result ?? string.Empty;
                    string output = string.IsNullOrWhiteSpace(result) ? error : result;

                    if (string.IsNullOrWhiteSpace(output) ||
                        output.IndexOf("command not found", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        output.IndexOf("No such file or directory", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        continue;
                    }

                    return output;
                }
            }

            return string.Empty;
        }

        private static void ProcessIOSDevicesMac(List<IOSDevice> devices)
        {
            string output = SudoExecute("xcrun xctrace list devices");

            if (string.IsNullOrWhiteSpace(output))
            {
                Logger.Debug("No iOS devices found or xcrun command failed.");
                return;
            }

            using (StringReader reader = new StringReader(output))
            {
                string line;
                string deviceType = "Unknown";

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains("=="))
                    {
                        deviceType = line.Trim('=').Trim();
                        continue;
                    }

                    IOSDevice device = new IOSDevice(line, deviceType);
                    AddDeviceIfUnique(devices, device);
                }
            }
        }

        private static void AddDeviceIfUnique(List<IOSDevice> devices, IOSDevice device)
        {
            if (string.IsNullOrWhiteSpace(device.Udid) || 
                string.Equals(device.Udid, "unknown", StringComparison.OrdinalIgnoreCase))
            {
                Logger.Debug($"Skipping device with invalid UDID: {device.Udid}");
                return;
            }

            if (devices.Any(d => string.Equals(d.Udid, device.Udid, StringComparison.OrdinalIgnoreCase)))
            {
                Logger.Debug($"Device with UDID {device.Udid} already exists in the list.");
                return;
            }

            devices.Add(device);
            Logger.Debug($"Added device: {device.DeviceName} (UDID: {device.Udid})");
        }



        /// <summary>
        /// Get list of installed apps on a target device.
        /// Auto-detects Mac vs iOS from the UDID:
        ///   - Mac : UDID contains '.' (IP address / hostname) → SSH bash defaults-read
        ///   - iOS : hex UDID → ideviceinstaller
        /// </summary>
        /// <param name="udid">UDID for iOS devices; IP address / hostname for Mac.</param>
        /// <returns>List of installed apps (user-installed only for Mac).</returns>
        public static List<App> GetInstalledApps(string udid)
        {
            List<App> apps = new List<App>();

            if (string.IsNullOrWhiteSpace(udid))
                return apps;

            // ---------------------------------------------------------
            // 1. Mac Detection
            // ---------------------------------------------------------
            bool isMac = udid.Contains('.');

            // ---------------------------------------------------------
            // 2. Android Detection
            // ---------------------------------------------------------
            bool isAndroid = false;

            try
            {
                // Check whether the device is connected through ADB
                string adbDevicesOutput = SudoExecute("adb devices");


                if (!string.IsNullOrWhiteSpace(adbDevicesOutput))
                {
                    using (StringReader reader = new StringReader(adbDevicesOutput))
                    {
                        string line;

                        while ((line = reader.ReadLine()) != null)
                        {
                            line = line.Trim();

                            // Example:
                            // emulator-5554    device
                            // R5CT12345ABC     device

                            if (line.StartsWith(udid + "\t") ||
                                line.StartsWith(udid + " "))
                            {
                                if (line.EndsWith("\tdevice") ||
                                    line.EndsWith(" device"))
                                {
                                    isAndroid = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Debug($"Android device detection failed: {ex.Message}");
            }

            // =========================================================
            // MAC
            // =========================================================
            if (isMac)
            {
                try
                {
                    const string cmd =
                        "for dir in /Applications ~/Applications; do" +
                        "  [ -d \"$dir\" ] || continue;" +
                        "  for app in \"$dir\"/*.app; do" +
                        "    [ -d \"$app\" ] || continue;" +
                        "    name=$(basename \"$app\" .app);" +
                        "    bundle=$(defaults read \"$app/Contents/Info.plist\" CFBundleIdentifier 2>/dev/null);" +
                        "    [ -n \"$bundle\" ] && echo \"$bundle|$name\";" +
                        "  done;" +
                        "done";

                    using (var sshCmd = _sshClient.RunCommand($"bash -c '{cmd}'"))
                    {
                        string output = sshCmd.Result ?? string.Empty;

                        foreach (string line in output.Split(
                            new[] { '\n', '\r' },
                            StringSplitOptions.RemoveEmptyEntries))
                        {
                            string trimmed = line.Trim();

                            if (string.IsNullOrEmpty(trimmed))
                                continue;

                            App app = new App(trimmed);

                            // Skip Apple system applications
                            if (string.IsNullOrWhiteSpace(app.BundleId) ||
                                app.BundleId.StartsWith(
                                    "com.apple.",
                                    StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            apps.Add(app);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Debug($"GetInstalledApps (Mac) failed: {ex.Message}");
                }
            }

            // =========================================================
            // ANDROID
            // =========================================================
            else if (isAndroid)
            {
                Logger.Debug($"Checking Appium Settings for Android device: {udid}");

                try
                {
                    // Check Appium Settings app operations
                    string appOpsOutput = SudoExecute(
                        $"adb -s \"{udid}\" shell appops get io.appium.settings");

                    Logger.Debug($"Appium Settings AppOps:\n{appOpsOutput}");

                    // Android version
                    string androidVersion = SudoExecute(
                        $"adb -s \"{udid}\" shell getprop ro.build.version.release");

                    Logger.Debug($"Android version: {androidVersion.Trim()}");

                    // Android SDK version
                    string sdkVersion = SudoExecute(
                        $"adb -s \"{udid}\" shell getprop ro.build.version.sdk");

                    Logger.Debug($"SDK version: {sdkVersion.Trim()}");

                    // Check Appium-related packages
                    string appiumPackages = SudoExecute(
                        $"adb -s \"{udid}\" shell pm list packages | grep appium");

                    Logger.Debug($"Appium packages:\n{appiumPackages}");

                    Logger.Debug("Android Appium environment check completed.");
                }
                catch (Exception ex)
                {
                    Logger.Debug(
                        $"Android Appium environment check failed: {ex.Message}");
                }
                try
                {
                    // -3 = third-party applications only


                    string output = SudoExecute(
                        $"adb -s \"{udid}\" shell pm list packages -3");

                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        using (StringReader reader = new StringReader(output))
                        {
                            string line;

                            while ((line = reader.ReadLine()) != null)
                            {
                                line = line.Trim();

                                // Example:
                                // package:com.google.android.youtube
                                // package:com.whatsapp

                                if (!line.StartsWith("package:"))
                                    continue;

                                string packageName =
                                    line.Substring("package:".Length).Trim();

                                if (string.IsNullOrWhiteSpace(packageName))
                                    continue;

                                // Android package name is used as BundleId
                                // and package name as AppName initially.
                                App app = new App(
                                    $"{packageName}|{packageName}");

                                apps.Add(app);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Debug(
                        $"GetInstalledApps (Android) failed: {ex.Message}");
                }
            }

            // =========================================================
            // iOS
            // =========================================================
            else
            {
                string output = string.Empty;

                if (!CheckIdeviceinstaller())
                {
                    throw new ApplicationException(
                        "ideviceinstaller is not installed.");
                }

                output = SudoExecute(
                    $"ideviceinstaller -u {udid} list --user").Trim();

                // Check if the old flag is unsupported
                if (output.Contains("unrecognized option `--list-apps'"))
                {
                    output = SudoExecute(
                        $"ideviceinstaller -u {udid} list --user").Trim();
                }

                using (StringReader reader = new StringReader(output))
                {
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Split(',').Count() > 2)
                        {
                            apps.Add(new App(line, ','));
                        }
                    }
                }
            }

            return apps;
        }

      
        public static string GetDeviceName(string udid)
{
    if (string.IsNullOrWhiteSpace(udid))
    {
        Logger.Debug("GetDeviceName called with null or empty UDID");
        return string.Empty;
    }

    Logger.Debug($"Searching for device with UDID: {udid}");
    var devices = GetConnectedDevices();
    
    Logger.Debug($"Found {devices.Count} total devices:");
    foreach (var d in devices)
    {
        Logger.Debug($"  - Device: {d.DeviceName}, UDID: {d.Udid}, Type: {d.DeviceType}");
    }
    
    // Case-insensitive UDID comparison
    var device = devices.FirstOrDefault(d => 
        !string.IsNullOrWhiteSpace(d.Udid) && 
        string.Equals(d.Udid.Trim(), udid.Trim(), StringComparison.OrdinalIgnoreCase));
    
    if (device != null && !string.IsNullOrWhiteSpace(device.DeviceName))
    {
        Logger.Debug($"Device found: {device.DeviceName} for UDID: {udid}");
        return device.DeviceName;
    }
    
    Logger.Debug($"Device not found on first attempt for UDID: {udid}, retrying with fresh device list...");
    
    // Retry after 3 seconds if device not found
    Thread.Sleep(TimeSpan.FromSeconds(3));
    devices = GetConnectedDevices();
    
    Logger.Debug($"Retry found {devices.Count} total devices:");
    foreach (var d in devices)
    {
        Logger.Debug($"  - Device: {d.DeviceName}, UDID: {d.Udid}, Type: {d.DeviceType}");
    }
    
    device = devices.FirstOrDefault(d => 
        !string.IsNullOrWhiteSpace(d.Udid) && 
        string.Equals(d.Udid.Trim(), udid.Trim(), StringComparison.OrdinalIgnoreCase));
    
    if (device != null && !string.IsNullOrWhiteSpace(device.DeviceName))
    {
        Logger.Debug($"Device found on retry: {device.DeviceName} for UDID: {udid}");
        return device.DeviceName;
    }
    
    Logger.Debug($"ERROR: Device not found after retry for UDID: {udid}. Available UDIDs: {string.Join(", ", devices.Select(d => d.Udid))}");

            // ... inside GetDeviceName(string udid)
            // Fallback: try partial match if exact match fails (for truncated UDIDs)
            device = devices.FirstOrDefault(d =>
                !string.IsNullOrWhiteSpace(d.Udid) &&
                (d.Udid.IndexOf(udid, StringComparison.OrdinalIgnoreCase) >= 0 ||
                 udid.IndexOf(d.Udid, StringComparison.OrdinalIgnoreCase) >= 0));

            if (device != null && !string.IsNullOrWhiteSpace(device.DeviceName))
    {
        Logger.Debug($"Device found with partial match: {device.DeviceName} for UDID: {udid}");
        return device.DeviceName;
    }
    
    return string.Empty;
}

        #endregion
    }
}

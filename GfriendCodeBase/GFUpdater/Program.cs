using HP.GFriend.GFLogger;
using Microsoft.Win32;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HP.GFriend.Updater
{
    static class Program
    {

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;

        [STAThread]
        static void Main(string[] args)
        {
            string updaterLocationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            GFriendLoggerServices.InitLogger(updaterLocationPath, "GFriendUpdaterLog_"+ DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt");

            string hostUrl = null;
            bool upload = false;
            bool download = false;
            string binaryPath = null;
            string binaryName = null;
            string binaryType = null;
            string binaryVersion = null;
            string releaseNotePath = null;
            string gfriendBasePath = null;
            bool list = false;
            bool uiMode = false;
            bool fromAgent = false;

            var options = new OptionSet()
            {
                {"h|hostUrl=","Host url for GF service on AWS" ,v => hostUrl = v},
                {"u|uplaod","Upload binary",v => {upload = true; }},
                {"d|download","Download binary",v => {download = true; }},
                {"b|binary=","Binary path to upload",v => binaryPath = v},
                {"n|name=","Binary name to upload",v => binaryName = v},
                {"t|type=","Binary type to upload",v => binaryType = v},
                {"v|version=","Version to release",v => binaryVersion = v},
                {"r|releaseNote=","Release note text file path",v => releaseNotePath = v},
                {"l|list","List",v => {list = true; }},
                {"g|gui", "GUI Mode", v=> { uiMode = true; } },
                {"p|path=", "GFriend base path", v=> gfriendBasePath = v },
                {"a|agent", "Update remote agent", v=> { fromAgent = true; } }
            };

            Logger.Trace("System Environment Details : ");
            Logger.Trace("Machine Name : " + Environment.MachineName);
            Logger.Trace("Operating System : " + Environment.OSVersion);
            Logger.Trace("User Domain Name : " + Environment.UserDomainName);
            Logger.Trace("User Name : " + Environment.UserName);
            Logger.Trace("Current Directory : " + Environment.CurrentDirectory);
            Logger.Trace("Windows Version : " + GetWindowsVersion());

            List<string> extra;
            try
            {
                Logger.Trace("In GFUpdater->Program.cs");
                extra = options.Parse(args);
            }
            catch (OptionException)
            {
                Console.WriteLine("Usage:");
                options.WriteOptionDescriptions(Console.Out);
                Environment.Exit(1);
            }

            string gfServerEndpoint = "";
            string activity = "";
            GFUpdaterOnAws gfUpdater;

            if (args.Length == 0)
            {
                Console.WriteLine("Usage:");
                options.WriteOptionDescriptions(Console.Out);
                Environment.Exit(1);
            }
            
            if (uiMode && gfriendBasePath != null)
            {
                ShowWindow(GetConsoleWindow(), SW_HIDE);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm(gfriendBasePath, hostUrl, fromAgent));
            }


            else
            {
#region Check url parameter 
                if (hostUrl == null)
                {
                    Console.WriteLine($"No GFriend release server url provided. Use default url.");
                    hostUrl = Const.DEFAULT_HOST;
                }
                gfServerEndpoint = hostUrl;
#endregion

#region Check what to do. (upload/download/get latest list/version check...)
                switch (new[] { upload, download, list }.Count(x => x))
                {
                    case 0:
                        Console.WriteLine($"Set the parameters of what you want to do with the Updater.(-u(uplaod)/-d(download)/-l(list)");
                        return;
                    case 1:
                        break;
                    default:
                        Console.WriteLine("Too many options, Please choose one of the following options : (-u(uplaod)/-d(download)/-l(list)");
                        return;
                }
#endregion

#region Execute update.
#region Upload process
                if (upload)
                {
                    if ((binaryName != null) && (binaryVersion != null) && (binaryType != null) && (binaryPath != null))
                    {
                        Console.WriteLine($"Uploading {binaryName} Binaries from the server. {hostUrl}");
                        gfUpdater = new GFUpdaterOnAws(gfServerEndpoint);
                        string uploadUrl = gfUpdater.GetGFBinaryUploadUrlOnAws(binaryName, binaryVersion);
                        gfUpdater.UploadBinaryToAws(uploadUrl, binaryName, binaryVersion, binaryPath);

                        if (releaseNotePath != null)
                        {
                            gfUpdater.UploadReleaseNoteToAws(binaryName, binaryVersion, binaryType, releaseNotePath);
                        }
                        Console.WriteLine($"Upload completed");
                    }
                    else
                    {
                        Console.WriteLine($"Set the following parameters for upload binary to aws.(-n(tool name)/-v(version)/-t(type)/-b(binary path)");
                        return;
                    }
                }
#endregion
#region Download process
                else if (download)
                {
                    
                    if (binaryName != null)
                    {
                        Console.WriteLine($"Downloading {binaryName} Binaries from the server. {hostUrl}");
                        gfUpdater = new GFUpdaterOnAws(gfServerEndpoint);
                        string downloadUrl = gfUpdater.GetGFBinaryDownloadUrlOnAws(binaryName, binaryVersion);
                        gfUpdater.GetGFBinaryOnAws(downloadUrl, binaryName);
                    }
                    else
                    {
                        Console.WriteLine($"Set the following parameters for download binary to aws.(-n(tool name)");
                    }
                }
#endregion
#region Get binary list
                else if (list)
                {
                    if(binaryName == null)
                    {
                        binaryName = string.Empty;
                    }

                    gfUpdater = new GFUpdaterOnAws(gfServerEndpoint);
                    List<GFTools> tools = gfUpdater.GetCurrentVersionOnAws(binaryName);
                    foreach(GFTools tool in tools)
                    {
                        Console.WriteLine(tool.ToString());
                    }
                }
#endregion
#region Check version available.
                else if (activity == "-c")
                {

                }
#endregion
#endregion
            }
        }

        static string GetWindowsVersion()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
            {
                if (key != null)
                {
                    string productName = key.GetValue("ProductName") as string;
                    return productName ?? "Unknown OS";
                }
            }
            return "Registry key not found.";
        }
    }
}

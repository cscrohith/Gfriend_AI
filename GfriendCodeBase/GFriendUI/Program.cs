using HP.GFriend.Updater;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using HP.GFriend.GFLogger;

namespace HP.GFriend.UI
{
    static class Program
    {
        static string _gfServerEndpoint = "https://8v0mj43oxb.execute-api.ap-northeast-2.amazonaws.com/gfriend";
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            string updaterLocationPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Updater");
            GFriendLoggerServices.InitLogger(updaterLocationPath, "UpdaterLogMain");
#if !DEBUG
            
            // Getting GFServerEndporint from INI File, if not use default value.
            string iniPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Settings.ini");
            if (File.Exists(iniPath))
            {
                IniFile settings = new IniFile(iniPath);
                _gfServerEndpoint = settings.GetValue("Server Info", "Release Server Address", _gfServerEndpoint);
                settings.SetValue("Server Info", "Release Server Address", _gfServerEndpoint);
            }

            GFUpdaterOnAws gfUpdater = new GFUpdaterOnAws(_gfServerEndpoint);


            // Check version
            string currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string serverVersion = gfUpdater.GetCurrentVersionOnAws(Const.GFRIEND_BIANRY_NAME)?[0]?.Version ?? string.Empty;

            Logger.Trace("GFriend Server Version=" + serverVersion);

            if(!string.IsNullOrEmpty(serverVersion) && !serverVersion.Equals(currentVersion))
            {
                DialogResult result = MessageBox.Show($"New version of GFriend is released. Click Ok to update. \r\nCurrent Version : {currentVersion} \r\nNew Version : {serverVersion}", "Update available", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if(result.Equals(DialogResult.OK))
                {
                    UpdateGFriend();
                }
                    
                
            }
#endif
            Application.Run(new MainForm());
        }


        /// <summary>
        /// 1.Gets the Temporary path and appends GFUpdater to the the path , tempLocation=C:\temp\GFUpdater . deletes the tempLocation if exists and creates new one .
        /// 2.Gets the GFriend exe path and updaterLocation = GFriend2.exe path + "Updater" .
        /// 3.If any folders/files in updaterLocation then copy it to tempLocation in the same structure.
        /// 4.Calls GFUpdater.exe with arguments -g(GUI Mode) -p(GFriend base path) -h(Host url for GF service on AWS)
        /// 
        /// Logic of GFUpdater.exe 
        /// 1.Gets the current version of GFriend (GetCurrentVersionOnAws) and gets the release information (GetReleaseInfo).
        /// 2.Gets the download url (GetGFBinaryDownloadUrlOnAws)
        /// 3.Downloads the GFriend binary and save it to the tempLocation.
        /// 4.Checks if GFriend2.exe exists in the tempLocation , if not exists step 2 and 3 will be called again.
        /// 5.Copy the files from tempLocation to Local GFriend location
        /// 6.Gets the current version GFriend2.exe from Local GFriend location.
        /// 7.If serverVersion is equals to currentVersion , then will get the msg as "Updated Done to Version" and update is successful.
        /// 8.If serverVersion not equals to currentVersion , update will happen again for the second time(steps 1,2 ,3 ,4 ,5 ,6, 7 will be called again) and if still serverVersion not equals to currentVersion will get the msg as "GFriend update is currently unavailable. Please try again later"
        /// </summary>
        public static void UpdateGFriend()
        {
            // Copy GFUpdater to temp path
            string tempLocation = Path.Combine(Path.GetTempPath(), "GFUpdater");
            Logger.Trace("tempLocation" + tempLocation);

            if (Directory.Exists(tempLocation))
            {
                Directory.Delete(tempLocation, true);
            }
            Directory.CreateDirectory(tempLocation);
            string updaterLocation = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Updater");
            Logger.Trace("updaterLocation =" + updaterLocation);

            foreach (string dirPath in Directory.GetDirectories(updaterLocation, "*", SearchOption.AllDirectories))
            {
                string newPath = dirPath.Replace(updaterLocation, tempLocation);
                Logger.Trace("tempLocation:" + newPath);

                if (!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }
            }

            foreach (string filePath in Directory.GetFiles(updaterLocation, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(filePath, filePath.Replace(updaterLocation, tempLocation), true);

                //filePath
                Logger.Trace("filePath:" + filePath);
            }

            Logger.Trace("Main->After copying files from all dicrectories");

            Process p = new Process();
            ProcessStartInfo info = new ProcessStartInfo(Path.Combine(tempLocation, "GFUpdater.exe"));
            info.Arguments = $"-g -p \"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\" -h {_gfServerEndpoint}";
            p.StartInfo = info;
            p.Start();
            Environment.Exit(0);
        }
    }
}

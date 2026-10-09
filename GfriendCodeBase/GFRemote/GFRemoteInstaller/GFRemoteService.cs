using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HP.GFriend.GFRemoteInstaller
{
    internal static class GFRemoteService
    {
        public const string SERVICE_NAME = "GFRemoteSvc";
        public const string BIN_DIR = @"C:\GFRemote";
        public const string BIN_PATH = @"C:\GFRemote\GFWindowsSvc.exe";

        public static string GetServiceStatus()
        {
            ServiceController ctl = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName.Equals(SERVICE_NAME));
            if(ctl==null)
            {
                return "Not Installed";
            }
            else
            {
                return ctl.Status.ToString();
            }
        }

        public static void StopService()
        {
            if(GetServiceStatus().Equals("Not Installed"))
            {
                return;
            }

            Process p = new Process();
            p.StartInfo.FileName = "sc.exe";
            p.StartInfo.Arguments = $"stop {SERVICE_NAME}";
            p.StartInfo.Verb = "runas";
            p.Start();
            p.WaitForExit();
            Thread.Sleep(1000);
        }

        public static void DeleteService()
        {
            if (GetServiceStatus().Equals("Not Installed"))
            {
                return;
            }
            StopService();
            Process p = new Process();
            p.StartInfo.FileName = "sc.exe";
            p.StartInfo.Arguments = $"delete {SERVICE_NAME}";
            p.StartInfo.Verb = "runas";
            p.Start();
            p.WaitForExit();
        }

        public static void AddService()
        {
            DeleteService();

            Process p = new Process();
            p.StartInfo.FileName = "sc.exe";
            p.StartInfo.Arguments = $"create {SERVICE_NAME} binPath={BIN_PATH} start=auto";
            p.StartInfo.Verb = "runas";
            p.Start();
            p.WaitForExit();
            StartService();
        }

        public static void StartService()
        {
            Process p = new Process();
            p.StartInfo.FileName = "sc.exe";
            p.StartInfo.Arguments = $"start {SERVICE_NAME}";
            p.StartInfo.Verb = "runas";
            p.Start();
            p.WaitForExit();

        }
        public static void CopyBinary()
        {
            string sourcePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                string newPath = dirPath.Replace(sourcePath, BIN_DIR);
                if (!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }
            }

            foreach (string filePath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(filePath, filePath.Replace(sourcePath, BIN_DIR), true);

            }
        }
    }
}

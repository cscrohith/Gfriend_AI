using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace HP.GFriend.Tool
{
    public static class ADB
    {
        private static string _adbLocation;
        private static string _ipBasedPattern = @"\b(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b:[0-9][0-9][0-9][0-9]";
        public static string Connect(string deviceIdentifier)
        {
            WriteADB();
            string ipPattern = @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";
            Regex regex = new Regex(ipPattern);
            Match m = regex.Match(deviceIdentifier);
            if (m.Success)
            {
                return ConnectWithIP(deviceIdentifier);
            }
            else
            {
                return ConnectWithUSB(deviceIdentifier);
            }

        }

        private static string ConnectWithIP(string ip)
        {
            string output = string.Empty;

            Process process = new Process();
            process.StartInfo.FileName = _adbLocation;
            process.StartInfo.Arguments = "connect " + ip;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.Start();
            process.WaitForExit();

            output = process.StandardOutput.ReadToEnd();
            Regex regex = new Regex(_ipBasedPattern);
            Match m = regex.Match(output);
            if (output.Contains("unable") || output.Contains("cannot")) return null;
            if (m.Success)
            {
                return m.Value;
            }
            return null;
        }

        private static string ConnectWithUSB(string deviceID)
        {
            string output = string.Empty;

            Process process = new Process();
            process.StartInfo.FileName = _adbLocation;
            process.StartInfo.Arguments = "devices";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.Start();
            process.WaitForExit();

            output = process.StandardOutput.ReadToEnd();
            if (output.Contains("unable")) return null;
            if (output.Contains(deviceID))
            {
                return deviceID;
            }

            return null;
        }
        public static string DisConnect(string ip)
        {
            string output = string.Empty;

            Process process = new Process();
            process.StartInfo.FileName = _adbLocation;
            process.StartInfo.Arguments = "disconnect " + ip;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.Start();
            process.WaitForExit();

            output = process.StandardOutput.ReadToEnd();

            return output;
        }

        public static string Execute(string target, string command)
        {
            string output = string.Empty;

            Process process = new Process();
            process.StartInfo.FileName = _adbLocation;
            process.StartInfo.Arguments = $"-s {target} {command}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.Start();
            process.WaitForExit();


            output = process.StandardOutput.ReadToEnd().Trim();

            return output;
        }

        public static string Pull(string target, string source, string destination)
        {
            string output = string.Empty;

            Process process = new Process();
            process.StartInfo.FileName = _adbLocation;
            process.StartInfo.Arguments = $"-s {target} pull \"{source}\" \"{destination}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.Start();
            process.WaitForExit(180000);

            output = process.StandardOutput.ReadToEnd();

            return output;
        }

        public static string GetUIDump(string target)
        {
            string output = Execute(target, "shell uiautomator dump");
            Regex regex = new Regex(@"(/[^/ ]*)+\.xml");
            Match m = regex.Match(output);
            string xmlPath = m.Value;
            string saveTo = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "dump.xml");
            Pull(target, xmlPath, saveTo);
            using (FileStream fs = new FileStream(saveTo, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(fs))
                {
                    return reader.ReadToEnd();
                }
            }
            
        }

        public static byte[] GetScreenCapture(string target)
        {
            string local = "/data/local/tmp/cap.png";
            string saveTo = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "android.png");
            Execute(target, $"shell screencap -p {local}");
            Pull(target, local, saveTo);
            using (FileStream fs = new FileStream(saveTo, FileMode.Open))
            {
                byte[] buffer = new byte[16 * 1024];
                using (MemoryStream ms = new MemoryStream())
                {
                    int read;
                    while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                    return ms.ToArray();
                }
            }
        }

        public static void WriteADB()
        {
            // Write ADB.exe
            byte[] byteToWrite = Properties.Resources.adb;
            string pathTo;

            pathTo = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "adb.exe");
            _adbLocation = pathTo;
            if (!File.Exists(pathTo))
            {
                using (FileStream fs = new FileStream(pathTo, FileMode.Create))
                {
                    fs.Write(byteToWrite, 0, byteToWrite.Length);
                }
            }


            // Write AdbWinApi.dll
            byteToWrite = Properties.Resources.AdbWinApi;
            pathTo = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "AdbWinApi.dll");

            if (!File.Exists(pathTo))
            {
                using (FileStream fs = new FileStream(pathTo, FileMode.Create))
                {
                    fs.Write(byteToWrite, 0, byteToWrite.Length);
                }
            }

            // Write AdbWinUsbApi.dll
            byteToWrite = Properties.Resources.AdbWinUsbApi;
            pathTo = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "AdbWinUsbApi.dll");

            if (!File.Exists(pathTo))
            {
                using (FileStream fs = new FileStream(pathTo, FileMode.Create))
                {
                    fs.Write(byteToWrite, 0, byteToWrite.Length);
                }
            }
        }
    }
}

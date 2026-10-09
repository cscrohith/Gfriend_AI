using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace HP.GFriend.Tool
{
    public static class FileLogger
    {
        private static readonly object _lock = new object();
        private static string _logFile;

        public static void Initialize()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string logDir = Path.Combine(baseDir, "Logs");

            Directory.CreateDirectory(logDir);

            _logFile = Path.Combine(
                logDir,
                $"DeviceDetails_{DateTime.Now:yyyyMMdd_HHmmss}.log");

            Write("===== APPLICATION STARTED =====");
            Write("BaseDirectory = " + baseDir);
            Write("WorkingDirectory = " + Environment.CurrentDirectory);
        }

        public static void Debug(string msg)
        {
            Write("[DEBUG] " + msg);
        }
        public static void Error(string message)
        {
            Write("[ERROR] " + message);
        }

        public static void Error(string msg, Exception ex)
        {
            Write("[ERROR] " + msg);
            Write(ex.ToString());
        }

        private static void Write(string msg)
        {
            lock (_lock)
            {
                File.AppendAllText(
                    _logFile,
                    $"{DateTime.Now:HH:mm:ss.fff} {msg}{Environment.NewLine}");
            }
        }
    }
}

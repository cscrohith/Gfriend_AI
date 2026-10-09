using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace HP.GFriend.Tool
{
    public static class ExceptionLogger
    {
        private static readonly string logFilePath;

        // Static constructor to set up the path once
        static ExceptionLogger()
        {
            string appDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "DeviceDetails"
            );

            Directory.CreateDirectory(appDataFolder);
            logFilePath = Path.Combine(appDataFolder, "ExceptionLog.txt");
        }

        public static string LogException(Exception ex)
        {
            string logMessage = $"[{DateTime.Now}] Error: {ex.Message}\nStack Trace:\n{ex.StackTrace}\n\n";

            try
            {
                File.AppendAllText(logFilePath, logMessage);

            }
            catch
            {
                // MessageBox.Show("Logging failed: " + ex.Message);
                string logPath = ExceptionLogger.LogException(ex);
            }
            //MessageBox.Show("An error occurred. Please check the log file for details.");
            return logFilePath;
        }
    }
}
    


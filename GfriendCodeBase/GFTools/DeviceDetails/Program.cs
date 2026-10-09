using System;
using System.Collections.Generic;
using System.Linq;
using HP.GFriend.GFLogger;
using System.IO;
using System.Windows.Forms;
using HP.GFriend.Tool;

namespace DeviceDetails
{
   public static class Program
   {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <summary>
        /// The main entry point for the application.
        /// Start Daligi
        /// </summary>
        
       [STAThread]
       public static void Main(string[] args)
       {
            FileLogger.Initialize();
            InitializeLogger();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            FileLogger.Debug("Application launched");
            //args = new string[2];
            //args[0] = "-i 146.205.5.130";
            //args[1] = "-p rdl@12345";
            if (args.Length > 0)
            {
                Dictionary<string, string> parsedArg = ParseArgs(args);
                string deviceId = null;
                string password = null;
                if (parsedArg.ContainsKey("i"))
                {
                    deviceId = parsedArg["i"];
                }
                if (parsedArg.ContainsKey("p"))
                {
                    password = parsedArg["p"];
                }
                HP.GFriend.GFLogger.Logger.Debug("Launching DeviceDetailsForm with parameters");
                Application.Run(new DeviceDetailsForm(deviceId, password));
            }
            else
            {
                HP.GFriend.GFLogger.Logger.Debug("No command-line arguments. Launching default DeviceDetailsForm");
                Application.Run(new DeviceDetailsForm());
            }

       }
        private static void InitializeLogger()
        {
            try
            {
                string logDir = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "Logs");

                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);

                HP.GFriend.GFLogger.Logger.Debug("Logger initialized");
                HP.GFriend.GFLogger.Logger.Debug($"Log directory: {logDir}");
            }
            catch (Exception ex)
            {
                // Fallback if logger itself fails
                File.WriteAllText("LoggerInitError.txt", ex.ToString());
            }
        }

        static Dictionary<string, string> ParseArgs(string[] args)
        {
            var argDict = new Dictionary<string, string>();
            string.Join(" ", args).Split('-').ToList().ForEach(s => argDict.Add(s.Split()[0], (s.Split().Count() > 1 ? s.Split()[1] : "")));

            return argDict;
        }
    }
}

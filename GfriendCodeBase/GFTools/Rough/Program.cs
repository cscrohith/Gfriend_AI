using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

namespace HP.GFriend.Tool
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// Start Daligi
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            /*if(args.Length > 0)
            {
                Dictionary<string, string> parsedArg = ParseArgs(args);
                string deviceId = null;
                string password = null;
                if(parsedArg.ContainsKey("i"))
                {
                    deviceId = parsedArg["i"];
                }
                if(parsedArg.ContainsKey("p"))
                {
                    password = parsedArg["p"];
                }
                Application.Run(new MainForm(deviceId, password));
            }
            else
            {
                Application.Run(new MainForm());
            }*/
            Application.Run(new MainForm());
        }

        static Dictionary<string, string> ParseArgs(string[] args)
        {
            var argDict = new Dictionary<string, string>();
            string.Join(" ", args).Split('-').ToList().ForEach(s => argDict.Add(s.Split()[0], (s.Split().Count() > 1 ? s.Split()[1] : "")));

            return argDict;
        }
    }
}
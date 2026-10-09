using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HP.GFriend.Utils.Appium
{
    public class App
    {
        public string BundleId { get; set; }
        public string AppName { get; set; }

        public App(string commandOutput, char splitter = '|')
        {
            string[] splitted = commandOutput.Split(splitter);
            if (splitter.Equals(','))
            {
                BundleId = splitted[0].Trim();
                AppName = splitted[2].Trim().Trim('\"');
            }
            else
            {
                BundleId = splitted[0].Trim();
                AppName = splitted[1].Trim();
            }
        }
        public App(string packageName, string displayName)
        {
            AppName = packageName;
            BundleId = displayName;
        }
    }
}

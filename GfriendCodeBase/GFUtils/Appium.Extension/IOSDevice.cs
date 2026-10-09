using System.Linq;
using System.Text.RegularExpressions;

namespace HP.GFriend.Utils.Appium
{
    public class IOSDevice:AndroidDevice
    {
        public string DeviceName { get; set; }
        public string Udid { get; set; }
        public string DeviceType { get; set; }          
        public string OsVersion { get; set; }

        public static string CheckPattern = @"\(([^\)]+)\)";

        public IOSDevice(string xctraceOutput, string deviceType)
        {
            Regex regex = new Regex(CheckPattern);
            MatchCollection collection = regex.Matches(xctraceOutput);
            
            DeviceName = xctraceOutput.Split('(')[0].Trim();
            var lastMatch = collection.OfType<Match>().LastOrDefault();
            Udid = lastMatch != null ? lastMatch.Value.Trim('(').Trim(')') : "Unknown";

            DeviceType = deviceType;
            if(collection.Count > 1)
            {
                OsVersion = collection[collection.Count - 2].Value.Trim('(').Trim(')');
            }
            else
            {
                OsVersion = "Unknown";
            }
        }
    }
}

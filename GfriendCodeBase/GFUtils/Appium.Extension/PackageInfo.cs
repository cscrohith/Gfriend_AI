using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HP.GFriend.Utils.Appium
{
    public enum Packages
    {
        HomeBrew,
        Node,
        Python,
        AuthorizeIOS,
        XCtrace,
        Carthage,
        Appium,
        WDA,
        IdeviceInstaller,
    }

    public enum PackageStatus
    {
        Installed,
        NotInstalled,
        UnKnown
    }
    public class PackageInfo
    {
        public Packages Package { get; set; }
        public string PackageVersion { get; set; }
        public PackageStatus PackageInstalledStatus { get; set; }
        public bool Show = true;
    }
}

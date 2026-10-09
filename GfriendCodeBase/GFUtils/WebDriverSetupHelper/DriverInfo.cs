namespace HP.GFriend.Utils.Web
{
    public enum Browsers
    {
        IE,
        Edge,
        Chrome,
        FireFox
    }

    public enum DriverStatus
    {
        Installed,
        NotInstalled,
        UpdateNeeded,
        NotApplicable,
        UnKnown
    }
    public class DriverInfo
    {
        public Browsers Browser { get; set; }
        public string BrowserVersion { get; set; }
        public string BrowserPath { get; set; }
        public string DriverVersion { get; set; }
        public bool IsBrowserInstalled { get; set; }
        public DriverStatus DriverInstalledStatus { get; set; }
        public string DriverDownloadAddresses { get; set; }
        public string DriverDownloadDestination { get; set; }
        public bool IsBrowserCompatible { get; set; } = true;
        public string Notes { get; set; } = string.Empty;
    }
}

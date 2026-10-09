namespace HP.GFriend.Keywords
{
    public class DevicePackageInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Uuid { get; set; }
        public string installedFileName { get; set; }
        public string Description { get; set; }
        public DevicePackageInfo() { }

        public DevicePackageInfo(string name, string version, string uuid, string fileName, string description)
        {
            Name = name;
            Version = version;
            Uuid = uuid;
            installedFileName = fileName;
            Description = description;
        }
    }
}

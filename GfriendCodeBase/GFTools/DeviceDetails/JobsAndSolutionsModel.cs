using HP.GFriend.Keywords;
using System.Collections.Generic;

namespace DeviceDetails
{
    public class JobAndSolutionsData
    {
        public List<DevicePackageInfo> InstalledSolutions { get; set; }
        public List<NativeAppData> NativeAppData { get; set; }
    }
    public class InstalledSolution
    {
        public string PackageName { get; set; }
        public string Version { get; set; }
        public string UUID { get; set; }
        public string InstalledFile { get; set; }
        public string Description { get; set; }
    }
    public class MakeAndModelName
    {
        public string Base { get; set; }
        public string Family { get; set; }
        public string Name { get; set; }
    }
    public class JobQueueDetails
    {
        public string State { get; set; }
        public string JobId { get; set; }
        public string JobName { get; set; }
        public string JobType { get; set; }
        public string UserName { get; set; }
        public string StartTime { get; set; }
        public string PauseReason { get; set; }
        public string EndTime { get; set; }
        public string CompletionState { get; set; }
    }
    public class NativeAppData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
    }
    public class JobHistoryDetails
    {
        public string State { get; set; }
        public string JobId { get; set; }
        public string JobName { get; set; }
        public string JobType { get; set; }
        public string UserName { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
    public class CDMIdentityData
    {
        public string CountryRegion { get; set; }
        public string DerivativeNumber { get; set; }
        public string DeviceLanguage { get; set; }
        public string DeviceUuid { get; set; }
        public string FirmwareCompatibilityId { get; set; }
        public string FirmwareDateCode { get; set; }
        public string FirmwareRelease { get; set; }
        public string FirmwareVersion { get; set; }
        public string InstallDate { get; set; }
        public MakeAndModelName MakeAndModel { get; set; }
        public string Manufacturer { get; set; }
        public string ProductNumber { get; set; }
        public string SerialNumber { get; set; }
        public string ServiceId { get; set; }
        public string SkuIdentifier { get; set; }
        public string ManagementProfile { get; set; }
        public string Version { get; set; }
        public List<JobQueueDetails> JobQueueDetails { get; set; }
        public List<JobHistoryDetails> JobHistoryDetails { get; set; }
    }
}

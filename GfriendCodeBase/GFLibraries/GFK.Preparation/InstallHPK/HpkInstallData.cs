using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;
using Logger = HP.GFriend.GFLogger.Logger;

namespace HP.GFriend.Keywords
{
    public class HpkInstallData
    {
        public List<HpkFileInfo> InstallFileList { get; set; }
        private StringBuilder _failedSettings = new StringBuilder();

        public HpkInstallData(string[] hpkfiles)
        {
            InstallFileList = new List<HpkFileInfo>();
            LoadHpkFiles(hpkfiles);
        }

        public HpkInstallData(string hpkfile)
        {
            InstallFileList = new List<HpkFileInfo>();
            LoadHpkFiles(hpkfile);
        }

        public HpkInstallData()
        {
            InstallFileList = new List<HpkFileInfo>();
        }

        private void LoadHpkFiles(string[] hpkfiles)
        {
            foreach (string file in hpkfiles)
            {
                HpkFileInfo openedHpkFileInfo = new HpkFileInfo(file);
                InstallFileList.Add(openedHpkFileInfo);
            }
        }

        private void LoadHpkFiles(string hpkfile)
        {
            HpkFileInfo openedHpkFileInfo = new HpkFileInfo(hpkfile);
            InstallFileList.Add(openedHpkFileInfo);
        }

        public bool ExecuteInstall(DeviceUnderTest device)
        {
            List<DevicePackageInfo> InstalledPackages = null;
            bool result = true;

            bool isIdle = GetInstallerStatus(device);
            while (!isIdle)
            {
                isIdle = GetInstallerStatus(device);
            }
            InstalledPackages = GetPackages(device);

            foreach (HpkFileInfo hpkfile in InstallFileList)
            {
                isIdle = GetInstallerStatus(device);
                while (!isIdle)
                {
                    Thread.Sleep(3000);
                    isIdle = GetInstallerStatus(device);
                }
                if (!isExistPackage(device, hpkfile, InstalledPackages))
                {
                    result &= UpdateHpk(hpkfile, device);
                }
            }
            return result;
        }

        public bool ExecuteUninstall(DeviceUnderTest device)
        {
            bool result = true;

            bool isIdle = GetInstallerStatus(device);
            while (!isIdle)
            {
                isIdle = GetInstallerStatus(device);
            }

            foreach (HpkFileInfo hpkfile in InstallFileList)
            {
                isIdle = GetInstallerStatus(device);
                while (!isIdle)
                {
                    Thread.Sleep(3000);
                    isIdle = GetInstallerStatus(device);
                }
                result &= RemoveHpk(device, hpkfile);
            }
            return result;
        }

        public bool ExecuteClear(DeviceUnderTest device)
        {
            bool result = true;
            List<DevicePackageInfo> package = GetPackages(device);
            foreach (DevicePackageInfo hpkfile in package)
            {
                bool isIdle = GetInstallerStatus(device);
                while (!isIdle)
                {
                    isIdle = GetInstallerStatus(device);

                }
                result = RemoveHpk(device, hpkfile);
            }
            return result;
        }

        public bool isExistPackage(DeviceUnderTest device, HpkFileInfo hpkfile, List<DevicePackageInfo> installedPackages)
        {
            string fileName = hpkfile.FilePath.Split('\\').Last();
            foreach (DevicePackageInfo p in installedPackages)
            {
                if (p.installedFileName.Split('.').First() == fileName.Split('.').First())
                {
                    Logger.Debug($"{hpkfile.FilePath.Split('\\').Last()}({device.DeviceAddress}) is already installed");
                    return true;
                }
            }
            return false;
        }

        public bool UpdateHpk(HpkFileInfo hpkfile, DeviceUnderTest device)
        {
            bool success = false;
            int RetryCount = 10;
            int count = 0;
            string progressState = null;

            try
            {
                while (!success && count < RetryCount)
                {
                    Logger.Debug($"Request installation : {hpkfile.PackageName}:({count})");
                    if (InstallPackage(device, hpkfile))
                    {
                        progressState = TrackPackage(device, hpkfile);

                        while (progressState == "psInProgress" || progressState == "404 Not Found" || progressState == "503 Service Unavailable" || string.IsNullOrEmpty(progressState))
                        {
                            Logger.Debug($"Installaion is in progress : {progressState}");
                            Thread.Sleep(3000);
                            progressState = TrackPackage(device, hpkfile);                            
                        }

                        if (progressState == "psCompleted")
                        {
                            Logger.Debug($"Installaion result : {progressState}");
                            success = true;
                        }

                        else if (progressState == "psFailed")
                        {
                            Logger.Debug($"Installaion result : {progressState}");
                            success = false;
                        }
                        else
                        {
                            Logger.Debug("Installaion result : unKown");
                            success = false;
                        }                        
                    }
                    else
                    {
                        Logger.Debug($"Status : {device.DeviceAddress}:{hpkfile.PackageName}:({count}):{" UpdateHpk :: InstallPackage fail -  Update will retry after (10)sec."}");
                        success = false;
                        Thread.Sleep(10000);
                    }
                    count++;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to install HPK: {hpkfile.PackageName}, {ex.Message}");
                success = false;
            }

            return success;
        }

        public bool RemoveHpk(DeviceUnderTest device, HpkFileInfo hpkfile)
        {
            bool success = false;
            int RetryCount = 10;
            int count = 0;
            string progressState = null;

            try
            {
                while (!success && count < RetryCount)
                {
                    Logger.Debug($"Request UnInstall Package : {device.DeviceAddress}:{hpkfile.PackageName}:({count})");
                    if (UninstallPackage(device, hpkfile.Uuid))
                    {
                        Logger.Debug($"Trying UnInstall Package : {device.DeviceAddress}:{hpkfile.PackageName}:({count})");
                        progressState = TrackUninstallPackage(device, hpkfile.Uuid);

                        while (progressState == "psInProgress" || progressState == "404 Not Found" || progressState == "503 Service Unavailable" || progressState == null)
                        {
                            progressState = TrackUninstallPackage(device, hpkfile.Uuid);
                            Logger.Debug($"Status : {device.DeviceAddress}:{hpkfile.PackageName}:({count}):{progressState}");
                        }
                        if (progressState == "psCompleted")
                        {
                            Logger.Debug($"Status : {device.DeviceAddress}:{hpkfile.PackageName}:({count}):{progressState}");
                            success = true;
                        }
                        else
                        {
                            Logger.Debug($"Status : {device.DeviceAddress}:{hpkfile.PackageName}:({count}):{progressState}");
                        }
                    }
                    count++;
                }
            }
            catch (Exception ex)
            {
                Logger.Debug($"Failed to Remove HPK: {hpkfile.PackageName}, {ex.Message}");
                _failedSettings.AppendLine($"Failed to Remove HPK: {hpkfile.PackageName}, {ex.Message}");
                success = false;
            }
            return success;
        }

        public bool RemoveHpk(DeviceUnderTest device, DevicePackageInfo hpkfile)
        {
            bool success = false;
            int RetryCount = 10;
            int count = 0;
            string progressState = null;

            try
            {
                while (!success && count < RetryCount)
                {
                    Logger.Debug($"Request UnInstall Package : {device.DeviceAddress}:{hpkfile.Name}:({count})");
                    if (UninstallPackage(device, hpkfile.Uuid))
                    {
                        Logger.Debug($"Trying UnInstall Package : {device.DeviceAddress}:{hpkfile.Name}:({count})");
                        progressState = TrackUninstallPackage(device, hpkfile.Uuid);

                        while (progressState == "psInProgress" || progressState == "404 Not Found" || progressState == "503 Service Unavailable" || progressState == null)
                        {
                            progressState = TrackUninstallPackage(device, hpkfile.Uuid);
                            Logger.Debug($"Status : {device.DeviceAddress}:{hpkfile.Name}:({count}):{progressState}");
                        }
                        if (progressState == "psCompleted")
                        {
                            Logger.Debug($"Status : {device.DeviceAddress}:{hpkfile.Name}:({count}):{progressState}");
                            success = true;
                        }
                        else
                        {
                            Logger.Debug($"Status : {device.DeviceAddress}:{hpkfile.Name}:({count}):{progressState}");
                        }
                    }
                    count++;
                }
            }
            catch (Exception ex)
            {
                Logger.Debug($"Failed to Remove HPK: {hpkfile.Name}, {ex.Message}");
                _failedSettings.AppendLine($"Failed to Remove HPK: {hpkfile.Name}, {ex.Message}");
                success = false;
            }
            return success;
        }

        public bool GetInstallerStatus(DeviceUnderTest device)
        {
            if (device == null)
            {
                Logger.Debug("Device info is null");
                return false;
            }
            Uri installer_state_uri = new Uri($"https://{device.DeviceAddress}/hp/device/webservices/ext/pkgmgt/installer");
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };            
            HttpClientHandler httphandler = new HttpClientHandler();
            httphandler.Credentials = new NetworkCredential("admin", device.AdminPassword);
            httphandler.PreAuthenticate = true;

            var authValue = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{"admin"}:{device.AdminPassword}")));


            var client = new HttpClient(httphandler);
            client.DefaultRequestHeaders.Authorization = authValue;
            try
            {
                using (HttpResponseMessage message = client.GetAsync(installer_state_uri).Result)
                {
                    string s = message.Content.ReadAsStringAsync().Result;
                    if (s.Contains("insIdle"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            catch (WebException ex)
            {
                Logger.Error(ex.Message);
                return false;
            }

            catch (HttpRequestException ex)
            {
                Logger.Error(ex.Message);
                return false;
            }

            catch (SocketException ex)
            {
                Logger.Error(ex.Message);
                return false;
            }

            catch (AggregateException ex)
            {
                Logger.Error(ex.Message);
                return false;
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                throw;
            }
        }

        public bool InstallPackage(DeviceUnderTest device, HpkFileInfo hpkfile)
        {
            Uri install_packages_uri = new Uri($"https://{device.DeviceAddress}/hp/device/webservices/ext/pkgmgt/installer/install?clientId=ciJamc&installSource=isStandardRepository&forceInstall=true&acceptPermissions=true");
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            HttpClientHandler httphandler = new HttpClientHandler();
            httphandler.Credentials = new NetworkCredential("admin", device.AdminPassword);
            httphandler.PreAuthenticate = true;
            var authValue = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{"admin"}:{device.AdminPassword}")));
            var client = new HttpClient(httphandler);
            client.DefaultRequestHeaders.Authorization = authValue;
            client.Timeout = TimeSpan.FromSeconds(110);
            client.DefaultRequestHeaders.ExpectContinue = false;
            client.DefaultRequestHeaders.Connection.Clear();
            client.DefaultRequestHeaders.ConnectionClose = true;  // true = keepalive off

            ByteArrayContent fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(hpkfile.FilePath));
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
            fileContent.Headers.ContentDisposition.Name = "\"file\"";
            fileContent.Headers.ContentDisposition.FileName = "\"" + hpkfile.PackageName + "\"";
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.hp.package-archive");

            MultipartFormDataContent content = new MultipartFormDataContent();
            content.Add(fileContent);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, install_packages_uri);
            request.Content = content;

            try
            {
                using (HttpResponseMessage message = client.SendAsync(request).Result)
                {
                    string s = message.Headers.Location.ToString();
                    if (s.Contains(hpkfile.Uuid))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (WebException ex)
            {
                Logger.Error(ex.Message);
                return false;
            }
            catch (HttpRequestException ex)
            {
                Logger.Error(ex.Message);
                return false;
            }
            catch (SocketException ex)
            {
                Logger.Error(ex.Message);
                return false;
            }
            catch (AggregateException ex)
            {
                Logger.Error(ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                _failedSettings.AppendLine($"InstallPackage Error: {hpkfile.PackageName}, {ex.Message}");
                throw;
            }
        }

        public bool UninstallPackage(DeviceUnderTest device, string uuid)
        {
            Uri delete_packages_uri = new Uri($"https://{device.DeviceAddress}/hp/device/webservices/ext/pkgmgt/installer/uninstall?uuid={uuid}&clientId=ciGallery");
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            HttpClientHandler httphandler = new HttpClientHandler();
            httphandler.Credentials = new NetworkCredential("admin", device.AdminPassword);
            var authValue = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{"admin"}:{device.AdminPassword}")));
            var client = new HttpClient(httphandler);
            client.DefaultRequestHeaders.Authorization = authValue;

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, delete_packages_uri);
            try
            {
                using (HttpResponseMessage message = client.SendAsync(request).Result)
                {                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception during Uninstall", ex);
                throw;
            }
        }

        public List<DevicePackageInfo> GetPackages(DeviceUnderTest device)
        {
            List<DevicePackageInfo> InstalledPackageList = new List<DevicePackageInfo>();
            Uri delete_packages_uri = new Uri($"https://{device.DeviceAddress}/hp/device/webservices/ext/pkgmgt/packages");
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            HttpClientHandler httphandler = new HttpClientHandler();
            httphandler.Credentials = new NetworkCredential("admin", device.AdminPassword);
            var authValue = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{"admin"}:{device.AdminPassword}")));
            var client = new HttpClient(httphandler);
            client.DefaultRequestHeaders.Authorization = authValue;
            Logger.Debug($"::GetPackages::HttpResponseMessage::Start");
            try
            {
                using (HttpResponseMessage message = client.GetAsync(delete_packages_uri).Result)
                {
                    JArray jarray;
                    string s = message.Content.ReadAsStringAsync().Result;
                    Logger.Debug($"::GetPackages::HttpResponseMessage::Message = {s}");

                    jarray = JArray.Parse(s);

                    foreach (var jtocken in jarray.Children())
                    {
                        var elements = jtocken.Children<JProperty>();
                        string uuid = elements.FirstOrDefault(x => x.Name == "uuid").Value.ToString();
                        string packageName = elements.FirstOrDefault(x => x.Name == "name").Value.ToString();
                        string version = elements.FirstOrDefault(x => x.Name == "version").Value.ToString();
                        string metadata = elements.FirstOrDefault(x => x.Name == "metaData").Value.ToString();
                        string description = elements.FirstOrDefault(x => x.Name == "description").Value.ToString();

                        XmlDocument xml = new XmlDocument();
                        xml.LoadXml(metadata);
                        string installedFile = xml.GetElementsByTagName("installFile")[0].InnerText;
                        InstalledPackageList.Add(new DevicePackageInfo(packageName, version, uuid, installedFile,description));
                    }
                    return InstalledPackageList;
                }
            }
            catch (WebException ex)
            {
                Logger.Error(ex.Message);
                return new List<DevicePackageInfo>();
            }
            catch (HttpRequestException ex)
            {
                Logger.Error(ex.Message);
                return new List<DevicePackageInfo>();
            }
            catch (SocketException ex)
            {
                Logger.Error(ex.Message);
                return new List<DevicePackageInfo>();
            }
            catch (AggregateException ex)
            {
                Logger.Error(ex.Message);
                return new List<DevicePackageInfo>();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                throw;
            }
        }

        public string TrackPackage(DeviceUnderTest device, HpkFileInfo hpkfile)
        {
            Uri track_install_uri = new Uri($"https://{device.DeviceAddress}/hp/device/webservices/ext/pkgmgt/installer/install/{hpkfile.Uuid}");
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            HttpClientHandler httphandler = new HttpClientHandler();
            httphandler.Credentials = new NetworkCredential("admin", device.AdminPassword);
            var authValue = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{"admin"}:{device.AdminPassword}")));
            var client = new HttpClient(httphandler);
            client.DefaultRequestHeaders.Authorization = authValue;

            try
            {
                using (HttpResponseMessage message = client.GetAsync(track_install_uri).Result)
                {
                    string s = message.Content.ReadAsStringAsync().Result;
                    if (s.Contains("psInProgress"))
                    {
                        return "psInProgress";
                    }
                    else if (s.Contains("psCompleted"))
                    {
                        return "psCompleted";
                    }
                    else if (s.Contains("psFailed"))
                    {
                        return "psFailed";
                    }
                    else if (s.Contains("404 Not Found"))
                    {
                        return "404 Not Found";
                    }
                    else if (s.Contains("503 Service Unavailable"))
                    {
                        return "503 Service Unavailable";
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (AggregateException ex)
            {
                Logger.Error("Exception during tracking", ex);
                return null;
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                throw;
            }
        }

        public string TrackUninstallPackage(DeviceUnderTest device, string uuid)
        {
            Uri track_install_uri = new Uri($"https://{device.DeviceAddress}/hp/device/webservices/ext/pkgmgt/installer/uninstall/{uuid}");
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            HttpClientHandler httphandler = new HttpClientHandler();
            httphandler.Credentials = new NetworkCredential("admin", device.AdminPassword);
            var authValue = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{"admin"}:{device.AdminPassword}")));
            var client = new HttpClient(httphandler);
            client.DefaultRequestHeaders.Authorization = authValue;

            try
            {
                using (HttpResponseMessage message = client.GetAsync(track_install_uri).Result)
                {
                    string s = message.Content.ReadAsStringAsync().Result;
                    if (s.Contains("psInProgress"))
                    {
                        return "psInProgress";
                    }
                    else if (s.Contains("psCompleted"))
                    {
                        return "psCompleted";
                    }
                    else if (s.Contains("psFailed"))
                    {
                        return "psFailed";
                    }
                    else if (s.Contains("404 Not Found"))
                    {
                        return "404 Not Found";
                    }
                    else if (s.Contains("503 Service Unavailable"))
                    {
                        return "503 Service Unavailable";
                    }
                    else
                    {
                        throw new ArgumentException($"TrackUninstallPackage Unknown State Error:{device.DeviceAddress}:{uuid}:{s}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                throw;
            }
        }
    }
}

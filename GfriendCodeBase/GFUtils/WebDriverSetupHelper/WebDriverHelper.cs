using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using HP.GFriend.GFLogger;

namespace HP.GFriend.Utils.Web
{
    internal static class Extensions
    {
        ///
        /// if string begins and ends with quotes, they are removed
        ///
        internal static String StripQuotes(this String s)
        {
            if (s.EndsWith("\"") && s.StartsWith("\""))
            {
                return s.Substring(1, s.Length - 2);
            }
            else
            {
                return s;
            }
        }
    }
    public static class WebDriverHelper
    {
        public static string WebDriverPath { get; set; }
        public static string DriverJsonEndPoint = "https://googlechromelabs.github.io/chrome-for-testing/known-good-versions-with-downloads.json";


        public static List<DriverInfo> GetCurrentStatus()
        {
            List<DriverInfo> driverInfos = new List<DriverInfo>();

            foreach (Browsers browser in Enum.GetValues(typeof(Browsers)).Cast<Browsers>().ToList())
            {
                driverInfos.Add(GetDriverInfo(browser));
            }

            return driverInfos;
        }

        public static DriverInfo GetDriverInfo(Browsers browser)
        {
            DriverInfo driverInfo = new DriverInfo();
            driverInfo.Browser = browser;
            driverInfo.DriverInstalledStatus = DriverStatus.NotApplicable;
            switch (browser)
            {
                case Browsers.Chrome:
                    GetBrowserVersionFromRegistry(driverInfo);
                    GetChromeDriverVersion(driverInfo);
                    GetChromeDriverDownloadAddress(driverInfo);
                    break;
                case Browsers.Edge:
                    GetEdgeVersion(driverInfo);
                    GetEdgeDriverVersion(driverInfo);
                    GetEdgeDriverDownloadAddress(driverInfo);
                    break;
                case Browsers.FireFox:
                    GetBrowserVersionFromRegistry(driverInfo);
                    CheckFireFoxCompatibility(driverInfo);
                    GetGeckoDriverVersion(driverInfo);
                    if(driverInfo.IsBrowserInstalled && driverInfo.IsBrowserCompatible)
                    {
                        GetGeckoDriverDownloadAddress(driverInfo);
                    }
                    driverInfo.Notes = "Please use at least 58 version of FireFox";
                    break;
                case Browsers.IE:
                    GetBrowserVersionFromRegistry(driverInfo);
                    GetIEDriverVersion(driverInfo);
                    GetIEDriverDownloadAddress(driverInfo);
                    driverInfo.Notes = "Please disable protection mode in all section, disable HP Sure Click before test";
                    break;
            }

            return driverInfo;
        }

        public static void GetWebDriver(DriverInfo driverInfo)
        {
            
            switch (driverInfo.Browser)
            {
                case Browsers.Chrome:
                    DownloadAndUnzipDriver(driverInfo);
                    GetChromeDriverVersion(driverInfo);
                    break;
                case Browsers.Edge:
                    DownloadAndUnzipDriver(driverInfo);
                    GetEdgeDriverVersion(driverInfo);
                    break;
                case Browsers.FireFox:
                    DownloadAndUnzipDriver(driverInfo);
                    GetGeckoDriverVersion(driverInfo);
                    break;
                case Browsers.IE:
                    DownloadAndUnzipDriver(driverInfo);
                    GetIEDriverVersion(driverInfo);
                    break;
            }
        }

        public static DriverInfo QueryBrowsers(Browsers target)
        {
            RegistryKey browserKeys;
            //on 64bit the browsers are in a different location
            browserKeys = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Clients\StartMenuInternet");
            if (browserKeys == null)
                browserKeys = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Clients\StartMenuInternet");
            string[] browserNames = browserKeys.GetSubKeyNames();
            for (int i = 0; i < browserNames.Length; i++)
            {
                DriverInfo browser = new DriverInfo();
                RegistryKey browserKey = browserKeys.OpenSubKey(browserNames[i]);
                string browserName = (string)browserKey.GetValue(null);

                if (browserName.Contains("Explorer"))
                {
                    browser.Browser = Browsers.IE;
                }
                else if (browserName.Contains("Firefox"))
                {
                    browser.Browser = Browsers.FireFox;
                }
                else if (browserName.Contains("Chrome"))
                {
                    browser.Browser = Browsers.Chrome;
                }

                if(browser.Browser.Equals(target))
                {
                    RegistryKey browserKeyPath = browserKey.OpenSubKey(@"shell\open\command");
                    string browserPath = (string)browserKeyPath.GetValue(null).ToString().StripQuotes();
                    browser.BrowserPath = browserPath;
                    RegistryKey browserIconPath = browserKey.OpenSubKey(@"DefaultIcon");

                    if (browserPath != null)
                    {
                        browser.BrowserVersion = FileVersionInfo.GetVersionInfo(browserPath).FileVersion;
                    }
                    else
                    {
                        browser.BrowserVersion = null;
                    }
                    return browser;

                }

            }

            return null;
        }

        private static void DownloadDriver(DriverInfo driverInfo)
        {
            if (string.IsNullOrEmpty(driverInfo.DriverDownloadAddresses) || string.IsNullOrEmpty(driverInfo.DriverDownloadDestination)) return;

            using (WebClient client = new WebClient())
            {
                client.DownloadFile(driverInfo.DriverDownloadAddresses, driverInfo.DriverDownloadDestination);
            }
            
        }      



        private static void DownloadAndUnzipDriver(DriverInfo driverInfo)
        {
            byte[] ieDriverBytes = null;
            WebClient client = new WebClient();
            try
            {
                if (string.IsNullOrEmpty(driverInfo.DriverDownloadAddresses)) return;
                ieDriverBytes = client.DownloadData(driverInfo.DriverDownloadAddresses);
                UnzipDriver(ieDriverBytes,driverInfo);
            }
            catch(Exception e)
            {
                if (e.Message.ToLower().Contains("not found"))
                {
                    driverInfo.BrowserVersion = driverInfo.BrowserVersion.Substring(0, driverInfo.BrowserVersion.LastIndexOf('.')) + ".0";
                    Logger.Trace("The edge driver download url :" + driverInfo.DriverDownloadAddresses + "is not found. So , trying to download the base version - "+ driverInfo.BrowserVersion);
                    GetEdgeDriverDownloadAddress(driverInfo);
                    ieDriverBytes = client.DownloadData(driverInfo.DriverDownloadAddresses);
                    UnzipDriver(ieDriverBytes, driverInfo);
                }
            }
        }

        private static void UnzipDriver(byte[] ieDriverBytes, DriverInfo driverInfo)
        {
            using (var compressedStream = new MemoryStream(ieDriverBytes))
            using (var archive = new ZipArchive(compressedStream))
            {

                foreach(var entry in archive.Entries)
                {
                    if (entry.Name.ToLower().Contains(".exe"))
                    {
                        using (var unzippedEntryStream = entry.Open())
                        {
                            using (var ms = new MemoryStream())
                            {
                                unzippedEntryStream.CopyTo(ms);
                                byte[] unzippedArray = ms.ToArray();
                                using (FileStream fs = new FileStream(driverInfo.DriverDownloadDestination, FileMode.Create))
                                {
                                    fs.Write(unzippedArray, 0, unzippedArray.Length);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void GetBrowserVersionFromRegistry(DriverInfo driverInfo)
        {
            DriverInfo query = QueryBrowsers(driverInfo.Browser);
            if(query != null)
            {
                driverInfo.BrowserVersion = query.BrowserVersion;
                driverInfo.IsBrowserInstalled = true;
                driverInfo.DriverInstalledStatus = DriverStatus.NotInstalled;
            }
            
        }

        #region Chrome
        private static void GetChromeDriverVersion(DriverInfo driverInfo)
        {
            GFLogger.Logger.Trace("WebDriverPath" + WebDriverPath);
            string chromeDriverPath = Path.Combine(WebDriverPath, "chromedriver.exe");
            driverInfo.DriverDownloadDestination = chromeDriverPath;
            if (!File.Exists(chromeDriverPath)) return;

            Process p = new Process();
            ProcessStartInfo pInfo = new ProcessStartInfo();
            pInfo.FileName = chromeDriverPath;
            pInfo.Arguments = "--version";
            pInfo.RedirectStandardOutput = true;
            pInfo.CreateNoWindow = true;
            pInfo.UseShellExecute = false;
            p.StartInfo = pInfo;
            p.Start();
            p.WaitForExit();
            string output = p.StandardOutput.ReadToEnd();
            output = output.Split('(')[0].Trim();


            // Check Driver Status
            Regex verRegex = new Regex(@"([0-9]+\.){1,}[0-9]+");
            if(verRegex.IsMatch(output))
            {
                driverInfo.DriverVersion = verRegex.Matches(output)[0].Value;
            }
            else
            {
                driverInfo.DriverVersion = output;
                driverInfo.DriverInstalledStatus = DriverStatus.UnKnown;
            }
            if (string.IsNullOrEmpty(driverInfo.BrowserVersion)) return;
            CheckChromeDriverCompatibility(driverInfo);

        }

        private static void CheckChromeDriverCompatibility(DriverInfo driverInfo)
        {
            if (!driverInfo.IsBrowserInstalled || string.IsNullOrEmpty(driverInfo.DriverVersion)) return;

            string chromeMajorVersion = driverInfo.BrowserVersion.Split('.')[0];
            if (driverInfo.DriverVersion.StartsWith(chromeMajorVersion))
            {
                driverInfo.DriverInstalledStatus = DriverStatus.Installed;
            }
            else
            {
                driverInfo.DriverInstalledStatus = DriverStatus.UpdateNeeded;
            }
        }

        private static void GetChromeDriverDownloadAddress(DriverInfo driverInfo)
        {
            if (!driverInfo.IsBrowserInstalled) return;

            GFLogger.Logger.Trace("driverInfo.DriverInstalledStatus" + driverInfo.DriverInstalledStatus.ToString());
            GFLogger.Logger.Trace("driverInfo.BrowserVersion=" + driverInfo.BrowserVersion);
            if (driverInfo.DriverInstalledStatus != DriverStatus.Installed)
            {
                using (WebClient client = new WebClient())
                {
                    Regex versionRegex = new Regex(@"[0-9]+\.[0-9]+\.[0-9]+");
                    string browserChromeVersion = versionRegex.Match(driverInfo.BrowserVersion).Value;

                    GFLogger.Logger.Trace("ChromeBrowserVersion=" + driverInfo.BrowserVersion);

                    string chromeSideLatest = client.DownloadString($"https://chromedriver.storage.googleapis.com/LATEST_RELEASE");
                    string chromeSideLatestVer = versionRegex.Match(chromeSideLatest).Value;

                    string versionCheckPage = "";
                    string browserChromeMajorVersion = browserChromeVersion.Split('.')[0];
                    int repoMajorVersion = Convert.ToInt32(browserChromeMajorVersion);
                    GFLogger.Logger.Trace("browserChromeMajorVersion=" + browserChromeMajorVersion);

                    
                    string chromeSideMajorVersion = chromeSideLatestVer.Split('.')[0];
                    int browserMajorVersion = Convert.ToInt32(chromeSideMajorVersion);

                    string cloudlatestchromeMajorVersion = chromeSideLatest.Split('.')[0];
                    int version = Convert.ToInt32(cloudlatestchromeMajorVersion);
                    string baseversion = $"{browserChromeVersion}.0";

                    GFLogger.Logger.Trace("baseversion=" + baseversion);
                    GFLogger.Logger.Trace("cloudlatestchromeMajorVersion=" + cloudlatestchromeMajorVersion);

                    if (browserChromeVersion == chromeSideLatestVer)
                    {
                        versionCheckPage = client.DownloadString($"https://chromedriver.storage.googleapis.com/LATEST_RELEASE_{browserChromeVersion}");

                    }
                    else if (repoMajorVersion > version)
                    {
                        GFLogger.Logger.Trace("ChromeBrowserVersion=" + browserChromeVersion);
                        versionCheckPage = browserChromeVersion;
                        GetChromeDriverDownloadAddress(browserChromeVersion, baseversion);
                    }
                    else
                    {
                        driverInfo.DriverDownloadAddresses = $"https://chromedriver.storage.googleapis.com/{chromeSideLatest}/chromedriver_win32.zip";
                    }

                    if (versionCheckPage.Contains("Error"))
                    {
                        GFLogger.Logger.Trace("Error=" + versionCheckPage);
                        driverInfo.DriverDownloadAddresses = string.Empty;
                    }
                }
            }

            void GetChromeDriverDownloadAddress(string chromeVersion, string baseVersion)
            {
                try
                {
                    string platform = "";
                    if (Environment.Is64BitOperatingSystem)
                    {
                        platform = "win64";
                    }
                    else
                    {
                        platform = "win32";
                    }
                    GFLogger.Logger.Trace("GetChromeDriverDownloadAddress-Inside");
                    using (HttpClient clients = new HttpClient())
                    {
                        HttpResponseMessage response = clients.GetAsync(DriverJsonEndPoint).Result;


                        if (response.IsSuccessStatusCode)
                        {
                            string jsonData = response.Content.ReadAsStringAsync().Result;
                            JObject data = JObject.Parse(jsonData);


                            bool versionExists = data["versions"].Any(versionEntry => (string)versionEntry["version"] == driverInfo.BrowserVersion);

                            if (versionExists)
                            {
                                driverInfo.DriverDownloadAddresses = GetChromeDriverDownloadAddressFromJsonEndPoint(data, driverInfo.BrowserVersion, platform, driverInfo.BrowserVersion);
                            }
                            else
                            {
                                versionExists = data["versions"].Any(versionEntry => (string)versionEntry["version"] == baseVersion);
                                if (versionExists)
                                {
                                    driverInfo.DriverDownloadAddresses = GetChromeDriverDownloadAddressFromJsonEndPoint(data, baseVersion, platform, driverInfo.BrowserVersion);
                                }
                                else
                                {
                                    string otherBrowserVersion = baseVersion.Substring(0, baseVersion.LastIndexOf('.'));
                                    driverInfo.DriverDownloadAddresses = GetChromeDriverDownloadAddressFromJsonEndPoint(data, otherBrowserVersion, platform, driverInfo.BrowserVersion);
                                }

                            }
                        }
                        
                    }
                }
                catch (Exception ex)
                {
                    GFLogger.Logger.Trace("Exception in GetChromeDriverDownloadAddress" + ex.Message);
                    throw ex;
                }
            }
        }
        private static string GetChromeDriverDownloadAddressFromJsonEndPoint(JObject chromeDriverJson, string driverversion, string operatingSystem,string browserversion)
        {
            string driverdownloadaddress = "";
            try
            {
                JToken getversiondata = chromeDriverJson["versions"].FirstOrDefault(v => (string)v["version"] == driverversion);
                if (getversiondata == null)
                {
                    int latestBrowserversion = Convert.ToInt32(browserversion.Split('.')[3]);
                    for (int i = 0; i <= latestBrowserversion; i++)
                    {
                        driverversion = driverversion + ("." + i);
                        getversiondata = chromeDriverJson["versions"].FirstOrDefault(v => (string)v["version"] == driverversion);
                        if (getversiondata != null)
                        {
                            break;
                        }
                        driverversion = driverversion.Substring(0, driverversion.LastIndexOf('.'));
                    }
                }
                Array versiondata = getversiondata["downloads"]["chromedriver"].ToArray();
                JArray chromedriverdata = new JArray(versiondata);
                foreach (var platformdata in chromedriverdata)
                {
                    string platformname = platformdata["platform"].ToString();
                    if (platformname == operatingSystem)
                    {
                        driverdownloadaddress = platformdata["url"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                GFLogger.Logger.Trace("Exception in DownloadChromeDriverFromJsonEndPoint" + ex.Message);
                throw ex;
            }
            return driverdownloadaddress;
        }

        public static string GetChromeBrowserVersion()
        {
            string browserversion = "";
            DriverInfo query = WebDriverHelper.QueryBrowsers(Browsers.Chrome);
            if (query != null)
            {
                browserversion = query.BrowserVersion;
            }
            return browserversion;
        }
        #endregion


        #region IE
        private static void GetIEDriverVersion(DriverInfo driverInfo)
        {
            string iEDriverPath = Path.Combine(WebDriverPath, "IEDriverServer.exe");
            driverInfo.DriverDownloadDestination = iEDriverPath;
            if (!File.Exists(iEDriverPath)) return;
            driverInfo.DriverVersion = FileVersionInfo.GetVersionInfo(iEDriverPath).FileVersion;
            driverInfo.DriverInstalledStatus = DriverStatus.Installed;
        }

        private static void GetIEDriverDownloadAddress(DriverInfo driverInfo)
        {
            if(is64BitOperatingSystem)
            {
                driverInfo.DriverDownloadAddresses = @"https://goo.gl/AtHQuv";
            }
            else
            {
                driverInfo.DriverDownloadAddresses = @"https://goo.gl/9Cqa4q";
            }
        }




        #endregion


        #region Edge
        private static void GetEdgeVersion(DriverInfo driverInfo)
        {
            string EdgeVersion = string.Empty;
            string edgePath = string.Empty;

            // Retrieve the Edge installation path from the registry
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\msedge.exe"))
            {
                if (key != null)
                {
                    edgePath = key.GetValue(string.Empty)?.ToString();
                }
            }

            // If the path was found, get the version
            if (!string.IsNullOrEmpty(edgePath) && System.IO.File.Exists(edgePath))
            {
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(edgePath);
                EdgeVersion = versionInfo.FileVersion;
                Logger.Trace("Edge Browser version : " + EdgeVersion);
            }

            driverInfo.BrowserVersion = EdgeVersion;
            driverInfo.IsBrowserInstalled = !string.IsNullOrEmpty(EdgeVersion);
            if (!string.IsNullOrEmpty(EdgeVersion))
            {
                driverInfo.DriverInstalledStatus = DriverStatus.NotInstalled;
            }
        }

        private static void GetEdgeDriverVersion(DriverInfo driverInfo)
        {
            string edgeDriverPath = Path.Combine(WebDriverPath, "MicrosoftWebDriver.exe");
            driverInfo.DriverDownloadDestination = edgeDriverPath;
            if (!File.Exists(edgeDriverPath)) return;
            driverInfo.DriverVersion = FileVersionInfo.GetVersionInfo(edgeDriverPath).FileVersion;
            CheckEdgeDriverCompatibility(driverInfo);
        }

        private static void CheckEdgeDriverCompatibility(DriverInfo driverInfo)
        {
            if (!driverInfo.IsBrowserInstalled || string.IsNullOrEmpty(driverInfo.DriverVersion)) return;

            string edgeMajorVersion = driverInfo.BrowserVersion.Split('.')[1];
            if(driverInfo.DriverVersion.Contains(edgeMajorVersion))
            {
                driverInfo.DriverInstalledStatus = DriverStatus.Installed;
            }
            else
            {
                driverInfo.DriverInstalledStatus = DriverStatus.UpdateNeeded;
            }
        }
        private static void GetEdgeDriverDownloadAddress(DriverInfo driverInfo)
        {
            if (!driverInfo.IsBrowserInstalled) return;

            driverInfo.DriverDownloadAddresses = $"https://msedgedriver.microsoft.com/{driverInfo.BrowserVersion}/edgedriver_win32.zip";
            Logger.Trace("Edge DownloadAddress : " + driverInfo.DriverDownloadAddresses);
        }
        #endregion


        #region FireFox
        private static void CheckFireFoxCompatibility(DriverInfo driverInfo)
        {
            if (!driverInfo.IsBrowserInstalled) return;
            int majorVersion = int.Parse(driverInfo.BrowserVersion.Split('.')[0]);
            if(majorVersion < 57)
            {
                driverInfo.IsBrowserCompatible = false;
            }
        }

        private static void GetGeckoDriverVersion(DriverInfo driverInfo)
        {
            string geckoDriverPath = Path.Combine(WebDriverPath, "geckodriver.exe");
            driverInfo.DriverDownloadDestination = geckoDriverPath;
            if (!File.Exists(geckoDriverPath)) return;

            Process p = new Process();
            ProcessStartInfo pInfo = new ProcessStartInfo();
            pInfo.FileName = geckoDriverPath;
            pInfo.Arguments = "--version";
            pInfo.RedirectStandardOutput = true;
            pInfo.CreateNoWindow = true;
            pInfo.UseShellExecute = false;
            p.StartInfo = pInfo;
            p.Start();
            p.WaitForExit();
            string output = p.StandardOutput.ReadToEnd();
            
            Regex verRegex = new Regex(@"([0-9]+\.){1,}[0-9]+");
            if (verRegex.IsMatch(output))
            {
                driverInfo.DriverVersion = verRegex.Matches(output)[0].Value;
                driverInfo.DriverInstalledStatus = DriverStatus.Installed;
                if (string.IsNullOrEmpty(driverInfo.BrowserVersion)) return;
                CheckGeckoDriverCompatibility(driverInfo);
            }
            else
            {
                driverInfo.DriverInstalledStatus = DriverStatus.UnKnown;
            }
            

        }

        private static void CheckGeckoDriverCompatibility(DriverInfo driverInfo)
        {
            if(!driverInfo.IsBrowserInstalled || !driverInfo.IsBrowserCompatible)
            {
                driverInfo.DriverInstalledStatus = DriverStatus.NotApplicable;
            }
            int midVersion = int.Parse(driverInfo.DriverVersion.Split('.')[1]);
            if(midVersion <21)
            {
                driverInfo.DriverInstalledStatus = DriverStatus.UpdateNeeded;
            }
            driverInfo.DriverInstalledStatus = DriverStatus.Installed;
        }


        private static void GetGeckoDriverDownloadAddress(DriverInfo driverInfo)
        {
            if (!driverInfo.IsBrowserInstalled) return;
            string needle = string.Empty;
            if (is64BitOperatingSystem)
            {
                needle = "win64";
            }
            else
            {
                needle = "win32";
            }

            using (WebClient client = new WebClient())
            {
                client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                string jsonResponse = client.DownloadString("https://api.github.com/repos/mozilla/geckodriver/releases/latest");
                dynamic releaseInfo = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonResponse);
                string latestVersion = releaseInfo.tag_name;

                string downloadPage = client.DownloadString($"https://github.com/mozilla/geckodriver/releases/expanded_assets/{latestVersion}");
                Regex hrefTag = new Regex($@"<a[^>]+{needle}\.zip[^>]+[>]");
                foreach (Match m in hrefTag.Matches(downloadPage))
                {
                    Regex linkAddr = new Regex("href=\"[^\"]+[\"]");
                    if(linkAddr.IsMatch(m.Value))
                    {
                        string downloadAddr = linkAddr.Match(m.Value).Value;
                        downloadAddr = downloadAddr.Replace("href=", string.Empty);
                        downloadAddr = downloadAddr.StripQuotes();
                        downloadAddr = "http://github.com" + downloadAddr;
                        driverInfo.DriverDownloadAddresses = downloadAddr;
                        return;
                    }
                }

            }
        }
        #endregion

        #region Others
        static bool is64BitProcess = (IntPtr.Size == 8);
        static bool is64BitOperatingSystem = is64BitProcess || InternalCheckIsWow64();

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process(
            [In] IntPtr hProcess,
            [Out] out bool wow64Process
        );

        public static bool InternalCheckIsWow64()
        {
            if ((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1) ||
                Environment.OSVersion.Version.Major >= 6)
            {
                using (Process p = Process.GetCurrentProcess())
                {
                    bool retVal;
                    if (!IsWow64Process(p.Handle, out retVal))
                    {
                        return false;
                    }
                    return retVal;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}

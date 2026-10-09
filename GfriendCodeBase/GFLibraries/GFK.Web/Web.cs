using HP.GFriend.GFLogger;
using HP.GFriend.Utils.Web;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Automation;

namespace HP.GFriend.Keywords
{
    public class Web : IGFLibrary
    {
        private IWebDriver _driver;
        private int _timeOutInSec;
        private static string _outputDir;
        private static List<Browsers> _installedDriver = null;
        private static String CHROME_DRIVER_ERROR_MESSAGE = "There is an error in downloading chrome driver,download it manually so please visit this URL (https://googlechromelabs.github.io/chrome-for-testing/known-good-versions-with-downloads.json) and download and copy chromedriver.exe to GFriend->Libs Folder manually and restart GFriend to fix this error";
        private static String EDGE_DRIVER_ERROR_MESSAGE = "There is an error in downloading edge driver,download it manually so please visit this URL (https://msedgewebdriverstorage.z22.web.core.windows.net/) and download and copy msedgedriver.exe to GFriend->Libs Folder manually and rename msedgedriver.exe to MicrosoftWebDriver.exe , restart GFriend to fix this error";
        private const int _browserheadlessnewversion = 112;
        private bool _disableScreenShotForFails = false;
        private const int PollingIntervalMilliseconds = 1000;

        public void Dispose()
        {
            if(_driver != null)
            {
                Close();
            }
        }

        public List<string> GetDependencies()
        {
            return null;
        }

        public string GetName()
        {
            return "Web";
        }

        public bool DutUsed()
        {
            return false;
        }

        public void Initialize(DeviceUnderTest dut, string outputDir)
        {
            // Web library doesn't require a DUT - it operates on the local machine's browser
            if (dut != null)
            {
                Logger.Debug($"Web library initialized with DUT: {dut.DeviceId ?? "Unknown"} at {dut.DeviceAddress ?? "N/A"}");
            }
            else
            {
                Logger.Debug("Web library initialized without DUT (local machine browser execution)");
            }

            _outputDir = outputDir;
            _timeOutInSec = 5;
            if(_installedDriver == null)
            {
                _installedDriver = new List<Browsers>();
                WebDriverHelper.WebDriverPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                Logger.Debug($"Web Driver Path = {WebDriverHelper.WebDriverPath}");
            }
        }

        private byte[] GetScreenShot()
        {
            Screenshot screenshot;
            try
            {
                screenshot = ((ITakesScreenshot)_driver).GetScreenshot();
            }
            catch(Exception)
            {
                return null;
            }
            
            return screenshot.AsByteArray;
        }

        private string GetPageSource()
        {
            try
            {
                return _driver.PageSource;
            }
            catch(Exception)
            {
                return string.Empty;
            }
        }

        private bool IsExist(string xPath, TimeSpan timeOut)
        {
            if (FindElement(GetSelectorByText(xPath), timeOut) == null)
            {
                return false;
            }
            return true;
        }

        private By GetSelectorByText(string text)
        {
            text = text.Trim();
            if(text.StartsWith("/"))
            {
                return By.XPath(text);
            }
            else if(text.Contains("="))
            {
                string[] splitted = text.Split('=');
                switch(splitted[0].ToLower())
                {
                    case "id":
                        return By.Id(splitted[1]);
                    case "name":
                        return By.Name(splitted[1]);
                    case "link":
                        return By.LinkText(splitted[1]);
                    case "tag":
                        return By.TagName(splitted[1]);
                    case "class":
                        return By.ClassName(splitted[1]);

                }
            }
            text = text.Replace("css=", string.Empty);
            return By.CssSelector(text);
        }

        private By GetFrameSelector(string text)
        {
            text = text.Trim();
            if (text.StartsWith("/"))
            {
                return By.XPath(text);
            }
            else if (text.Contains("="))
            {
                string[] splitted = text.Split('=');
                switch (splitted[0].ToLower())
                {
                    case "id":
                        return By.Id(splitted[1]);
                    case "name":
                        return By.Name(splitted[1]);
                    case "link":
                        return By.LinkText(splitted[1]);
                    case "tag":
                        return By.TagName(splitted[1]);
                    case "class":
                        return By.ClassName(splitted[1]);

                }
            }
            text = text.Replace("css=", string.Empty);
            return null;
        }

        private IWebElement FindElement(By by, TimeSpan timeOut)
        {
            double totalWaitTime = timeOut.TotalMilliseconds;

            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddMilliseconds(totalWaitTime);
            
            IWebElement ret = null;

            while(DateTime.Now <= endTime)
            {
                try
                {
                    ret = _driver.FindElement(by);
                    if (ret.Displayed && ret.Enabled)
                    {
                        return ret;
                    }

                    else
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(500));
                    }
                }
                catch (Exception ex)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(500));
                    Logger.Debug(ex.ToString());
                }
            }
            return ret;
        }

        private void PrepareDriver(Browsers browser, KeywordResult result)
        {
            if (!_installedDriver.Contains(browser))
            {
                DriverInfo targetDriverInfo = WebDriverHelper.GetDriverInfo(browser);
                if (!targetDriverInfo.IsBrowserInstalled || !targetDriverInfo.IsBrowserCompatible)
                {
                    Logger.Error("Proper browser is not installed.");
                    result.Result = KeywordResults.Error;
                    result.Output = "Browser is not installed. Check if proper version of browser is installed";
                    return;
                }
                if (!targetDriverInfo.DriverInstalledStatus.Equals(DriverStatus.Installed))
                {
                    Logger.Trace("Driver is not installed. Install driver.");
                    WebDriverHelper.GetWebDriver(targetDriverInfo);
                    _installedDriver.Add(browser);
                    return;
                }
            }
        }
        [KeywordDescription("Open web page with GFriend default broswer.\r\nChrome with headless mode will be used as broswer to test functionality of web page.\r\nChrome with headless mode will not show any visible windows")]
        [KeywordDisplayName("Open")]
        [KeywordParameters("address", "Web address to open")]
        [SampleScript("Web.Open (www.hp.com)")]
        public KeywordResult Open(string address)
        {
            return Open(address, "60");
        }

        [KeywordDescription("Open web page with GFriend default broswer.\r\nChrome with headless mode will be used as broswer to test functionality of web page.\r\nChrome with headless mode will not show any visible windows")]
        [KeywordDisplayName("Open")]
        [KeywordParameters("address", "Web address to open")]
        [KeywordParameters("timeout", "Default Timeout in seconds")]
        [SampleScript("Web.Open (www.hp.com,5)")]
        public KeywordResult Open(string address, string timeOut)
        {
            int timeoutSec = int.Parse(timeOut);
            if (!address.StartsWith("http://") && !address.StartsWith("https://"))
            {
                address = "http://" + address;
            }
            try
            {
                KeywordResult result = new KeywordResult(KeywordResults.Pass);
                PrepareDriver(Browsers.Chrome, result);
                if (!result.Result.Equals(KeywordResults.Pass))
                {
                    return result;
                }

                ChromeOptions chromeOptions = new ChromeOptions();
                string version=WebDriverHelper.GetChromeBrowserVersion();
                if (Convert.ToInt32(version.Split('.')[0]) >= _browserheadlessnewversion)
                {
                    chromeOptions.AddArgument("--headless=new");
                    Logger.Debug("Chrome browser version is equal to or greater than "+ _browserheadlessnewversion + " , so using --headless=new");
                }
                else
                {
                    chromeOptions.AddArgument("--headless=old");
                    Logger.Debug("Chrome browser version is less than ,"+ _browserheadlessnewversion + " so using --headless=old");
                }
                chromeOptions.AddArgument("--no-sandbox");
                chromeOptions.AcceptInsecureCertificates = true;
                chromeOptions.AddAdditionalOption("useSeleniumManager", false);
                ChromeDriverService chromeDriverService = ChromeDriverService.CreateDefaultService(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "chromedriver.exe");
                chromeDriverService.HideCommandPromptWindow = true;

                _driver = new ChromeDriver(chromeDriverService, chromeOptions, TimeSpan.FromSeconds(timeoutSec));
                System.Drawing.Size s = new System.Drawing.Size
                {
                    Width = 1920,
                    Height = 1080
                };
                _driver.Manage().Window.Size = s;

                _driver.Navigate().GoToUrl(address);


                return new KeywordResult(KeywordResults.Pass);
            }
            catch(Exception ex)
            {
                Logger.Error("Error open chrome with headless.", ex);
                KeywordResult result = new KeywordResult(KeywordResults.Error);
                result.AdditionalInfo = ex.ToString();
                result.Output = "Error to open browser.";
                return result;
            }
            
        }

        [KeywordDescription("Gets the cookies of the web driver")]
        [KeywordDisplayName("Save Cookie Data")]
        [KeywordParameters("domain", "Domain of the cookie")]
        [KeywordParameters("path", "Path for the cookie")]
        [KeywordParameters("key", "Key for the key value pair")]
        [KeywordParameters("filePath", "Realitive path from current working directory to save the file")]
        [SampleScript("Web.Save Cookie Data (.hp.com,/,MUID,C:/sample/test1.txt)")]
        public KeywordResult SaveCookieData(string domain, string path, string key, string filePath)
        {
            try
            {
                filePath = Support.Utils.GetAbsolutePath(filePath, CommonExecutionInfo.ScriptFolder);
                FileInfo file = new FileInfo(filePath);
                string cookieData = _driver.Manage().Cookies.AllCookies
                        .FirstOrDefault((x) => x.Domain == domain && x.Path == path && x.Name == key)
                        ?.Value;
                string saveTo = Path.Combine(_outputDir, file.Name);
                File.WriteAllText(saveTo, cookieData);
                File.WriteAllText(filePath, cookieData);
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                KeywordResult result = new KeywordResult(KeywordResults.Error);
                result.AdditionalInfo = ex.ToString();
                result.Output = "Error getting cookie information.";
                return result;
            }

        }
        

        [KeywordDescription("Gets the token from cookie of the web driver")]
        [KeywordDisplayName("GetTokenFromCookie")]
        [KeywordParameters("domain", "Domain of the cookie")]
        [KeywordParameters("key", "Key for the key value pair")]
        [SampleScript("Web.GetTokenFromCookie (.hpsmartpie.com,stratus-id-token,${test})")]
        public KeywordResult GetTokenFromCookie(string domain, string key, string saveTo)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            try
            {
                string cookieData = _driver.Manage().Cookies.AllCookies
                        .FirstOrDefault((x) => x.Domain == domain && x.Name == key)
                        ?.Value;

                if(!string.IsNullOrEmpty(cookieData))
                {
                    // Get variable name by value 
                    string savedVarName = CommonExecutionInfo.GetVariableByValue(saveTo);

                    // If VariableName is already there then use the same var name to save cookie data
                    // This will happen if the same variable name is used in getting multiple cookie values
                    if(!string.IsNullOrEmpty(savedVarName))
                    {
                        saveTo = savedVarName;
                    }
                    CommonExecutionInfo.SetVariable(saveTo, cookieData);
                    kr.Output = "";
                    kr.Output = "cookie vlue =" + $": {cookieData}";
                    return kr;
                }
                else
                {
                    kr.Output = "No token value with given key and domain";
                    kr.Result = KeywordResults.Fail;
                    return kr;
                }

            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = "Error getting token from cookie.";
                return kr;
            }

        }


        [KeywordDescription("Close broswer")]
        [KeywordDisplayName("Close")]
        [SampleScript("Web.Close")]
        public KeywordResult Close()
        {
            try
            {
                _driver.Close();
            }
            catch(Exception ex)
            {
                Logger.Error("Error during close", ex);
            }

            try
            {
                _driver.Quit();
            }
            catch (Exception ex)
            {
                Logger.Error("Error during quit", ex);
            }


            int count = 0;
            while(true)
            {
                try
                {
                    string s = _driver.CurrentWindowHandle;
                    count++;
                    Thread.Sleep(500);
                    if(count>10)
                    {
                        break;
                    }
                }
                catch (Exception)
                {
                    _driver = null;
                    return new KeywordResult(KeywordResults.Pass);
                }
                
            }

            KeywordResult error = new KeywordResult(KeywordResults.Error);
            error.Output = "Can not close driver process";
            return error;
        }

        [KeywordDescription("Open web page with Chrome broswer.")]
        [KeywordDisplayName("Open with Chrome")]
        [KeywordParameters("address", "Web address to open")]
        [SampleScript("Web.Open with Chrome (www.hp.com)")]
        public KeywordResult OpenWithChrome(string address)
        {
            return OpenWithChrome(address, "60");
        }
        

        [KeywordDescription("Open web page with Chrome broswer.")]
        [KeywordDisplayName("Open with Chrome")]
        [KeywordParameters("address", "Web address to open")]
        [KeywordParameters("timeout", "Default Timeout in seconds")]
        [SampleScript("Web.Open with Chrome (www.hp.com,5)")]
        public KeywordResult OpenWithChrome(string address, string timeOut)
        {
            if (CommonExecutionInfo.IsHeadlessMode)
            {
                Logger.Trace("GFrined is running under headless mode. Use Open instead of Open With Chrome");
                return Open(address, timeOut);
            }
            int timeoutSec = int.Parse(timeOut);
            if (!address.StartsWith("http://") && !address.StartsWith("https://"))
            {
                address = "http://" + address;
            }
            try
            {
                KeywordResult result = new KeywordResult(KeywordResults.Pass);
                PrepareDriver(Browsers.Chrome, result);
                if (!result.Result.Equals(KeywordResults.Pass))
                {
                    return result;
                }
                ChromeDriverService chromeDriverService = ChromeDriverService.CreateDefaultService(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "chromedriver.exe");
                chromeDriverService.HideCommandPromptWindow = true;
                ChromeOptions chromeOptions = new ChromeOptions();
                chromeOptions.AcceptInsecureCertificates = true;
                chromeOptions.AddAdditionalOption("useSeleniumManager", false);
                chromeOptions.AddArgument("--disable-blink-features=AutomationControlled");


                _driver = new ChromeDriver(chromeDriverService, chromeOptions, TimeSpan.FromSeconds(timeoutSec));
                _driver.Manage().Window.Maximize();
                _driver.Navigate().GoToUrl(address);
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                Logger.Error("Error open chrome.", ex);
                Logger.Error(@"If you are using Chrome version 115 or newer and facing issue please download the chrome driver from here(https://chromedriver.chromium.org/downloads) and copy chromedriver.exe to GFriend->Libs Folder manually and restart GFriend to fix this error.", ex);
                KeywordResult result = new KeywordResult(KeywordResults.Error);
                result.AdditionalInfo = ex.ToString() + CHROME_DRIVER_ERROR_MESSAGE;
                result.Output = "Error to open browser.";
                return result;
            }
        }
        [KeywordDescription("Open web page with Chrome browser by passing arguments")]
        [KeywordDisplayName("Open Chrome with Arguments")]
        [KeywordParameters("address", "Web address to open")]
        [KeywordParameters("timeout", "Default Timeout in seconds")]
        [KeywordParameters("chromeOptions", "Custom ChromeOptions to configure the ChromeDriver")]
        [SampleScript(@"Web.OpenChromewithArguments(www.hp.com,5,--disable-blink-features=AutomationControlled \,--incognito)")]
        public KeywordResult OpenChromewithArguments(string address, string timeOut, string options)
        {
            if (CommonExecutionInfo.IsHeadlessMode)
            {
                Logger.Trace("GFrined is running under headless mode. Use Open instead of Open With Chrome");
                return Open(address, timeOut);
            }

            int timeoutSec = int.Parse(timeOut);

            if (!address.StartsWith("http://") && !address.StartsWith("https://"))
            {
                address = "http://" + address;
            }

            try
            {
                KeywordResult result = new KeywordResult(KeywordResults.Pass);
                PrepareDriver(Browsers.Chrome, result);

                if (!result.Result.Equals(KeywordResults.Pass))
                {
                    return result;
                }
                ChromeDriverService chromeDriverService = ChromeDriverService.CreateDefaultService(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "chromedriver.exe");
                chromeDriverService.HideCommandPromptWindow = true;

                ChromeOptions chromeOptions = new ChromeOptions();
                chromeOptions.AcceptInsecureCertificates = true;
                chromeOptions.AddAdditionalOption("useSeleniumManager", false);
                // Add custom ChromeOptions dynamically
                string[] optionArray = options.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                chromeOptions.AddArguments(optionArray);

                _driver = new ChromeDriver(chromeDriverService, chromeOptions, TimeSpan.FromSeconds(timeoutSec));
                _driver.Manage().Window.Maximize();
                _driver.Navigate().GoToUrl(address);

                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                Logger.Error("Error open chrome with Arguments.", ex);
                Logger.Error(@"If you are using Chrome version 115 or newer and facing issue, please download the chrome driver from here(https://chromedriver.chromium.org/downloads) and copy chromedriver.exe to GFriend->Libs Folder manually and restart GFriend to fix this error.", ex);

                KeywordResult result = new KeywordResult(KeywordResults.Error);
                result.AdditionalInfo = ex.ToString() + CHROME_DRIVER_ERROR_MESSAGE;
                result.Output = "Error to open chrome with Arguments.";

                return result;
            }
        }

        [KeywordDescription("Open web page with chrome with specific window size .")]
        [KeywordDisplayName("Open with Chrome")]
        [KeywordParameters("address", "Web address to open")]
        [KeywordParameters("width", "Windows width to be set ")]
        [KeywordParameters("height", "Windows height to be set ")]
        [SampleScript("Web.Open with Chrome(www.hp.com,50,50)")]
        public KeywordResult OpenWithChrome(string address, string width, string height)
        {
            int w = int.Parse(width);
            int h = int.Parse(height);
            if (CommonExecutionInfo.IsHeadlessMode)
            {
                Logger.Trace("GFrined is running under headless mode. Use Open instead of Open With Chrome");
                return Open(address, "60");
            }
            int timeoutSec = 60;
            if (!address.StartsWith("http://") && !address.StartsWith("https://"))
            {
                address = "http://" + address;
            }
            try
            {
                KeywordResult result = new KeywordResult(KeywordResults.Pass);
                PrepareDriver(Browsers.Chrome, result);
                if (!result.Result.Equals(KeywordResults.Pass))
                {
                    return result;
                }
                ChromeDriverService chromeDriverService = ChromeDriverService.CreateDefaultService(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "chromedriver.exe");
                chromeDriverService.HideCommandPromptWindow = true;
                ChromeOptions chromeOptions = new ChromeOptions();
                chromeOptions.AcceptInsecureCertificates = true;
                chromeOptions.AddAdditionalOption("useSeleniumManager", false);

                _driver = new ChromeDriver(chromeDriverService, chromeOptions, TimeSpan.FromSeconds(timeoutSec));
                System.Drawing.Size s = new System.Drawing.Size
                {
                    Width = w,
                    Height = h
                };
                _driver.Manage().Window.Size = s;
                //   _driver.Manage().Window.Maximize();
                _driver.Navigate().GoToUrl(address);
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                Logger.Error("Error open chrome with specific window size.", ex);
                KeywordResult result = new KeywordResult(KeywordResults.Error);
                result.AdditionalInfo = ex.ToString() + CHROME_DRIVER_ERROR_MESSAGE;
                result.Output = "Error to open browser.";
                return result;

            }
        }

        [KeywordDescription("Open web page with Internet Explorer.\r\nProtection mode in all region should be disabled for using Internet Explorer.")]
        [KeywordDisplayName("Open with IE")]
        [KeywordParameters("address", "Web address to open")]
        [Deprecated("Use open with edge instead of IE as IE has stopped the support")]
        [SampleScript("Web.Open with IE (www.hp.com)")]
        public KeywordResult OpenWithIE(string address)
        {
            return OpenWithIE(address, "60");
        }

        [KeywordDescription("Open web page with Internet Explorer.\r\nProtection mode in all region should be disabled for using Internet Explorer.")]
        [KeywordDisplayName("Open with IE")]
        [KeywordParameters("address", "Web address to open")]
        [KeywordParameters("timeout", "Default Timeout in seconds")]
        [Deprecated("Use open with edge instead of IE as IE has stopped the support")]
        [SampleScript("Web.Open with IE (www.hp.com,5)")]
        public KeywordResult OpenWithIE(string address, string timeOut)
        {
            if(CommonExecutionInfo.IsHeadlessMode)
            {
                Logger.Trace("GFrined is running under headless mode. Use Open instead of Open With IE");
                return Open(address, timeOut);
            }
            int timeoutSec = int.Parse(timeOut);
            if (!address.StartsWith("http://") && !address.StartsWith("https://"))
            {
                address = "http://" + address;
            }
            try
            {
                KeywordResult result = new KeywordResult(KeywordResults.Pass);
                PrepareDriver(Browsers.IE, result);
                if (!result.Result.Equals(KeywordResults.Pass))
                {
                    return result;
                }
                InternetExplorerDriverService internetExplorerDriverService = InternetExplorerDriverService.CreateDefaultService();
                internetExplorerDriverService.HideCommandPromptWindow = true;
                InternetExplorerOptions internetExplorerOptions = new InternetExplorerOptions();
                internetExplorerOptions.IgnoreZoomLevel = true;
                //internetExplorerOptions.AcceptInsecureCertificates = true;

                _driver = new InternetExplorerDriver(internetExplorerDriverService, internetExplorerOptions, TimeSpan.FromSeconds(timeoutSec));
                _driver.Manage().Window.Maximize();
                _driver.Navigate().GoToUrl(address);
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                Logger.Error("Error open IE.", ex);
                KeywordResult result = new KeywordResult(KeywordResults.Error);
                result.AdditionalInfo = ex.ToString();
                result.Output = "Error to open browser.";
                return result;
            }
        }

        [KeywordDescription("Open web page with Microsoft Edge.")]
        [KeywordDisplayName("Open with Edge")]
        [KeywordParameters("address", "Web address to open")]
        [SampleScript("Web.Open with Edge (www.hp.com)")]
        public KeywordResult OpenWithEdge(string address)
        {
            return OpenWithEdge(address, "60");
        }

        [KeywordDescription("Open web page with Microsoft Edge.")]
        [KeywordDisplayName("Open with Edge")]
        [KeywordParameters("address", "Web address to open")]
        [KeywordParameters("timeout", "Default Timeout in seconds")]
        [SampleScript("Web.Open with Edge (www.hp.com,5)")]
        public KeywordResult OpenWithEdge(string address, string timeOut)
        {
            if (CommonExecutionInfo.IsHeadlessMode)
            {
                Logger.Trace("GFrined is running under headless mode. Use Open instead of Open With Edge");
                return Open(address, timeOut);
            }
            int timeoutSec = int.Parse(timeOut);
            if (!address.StartsWith("http://") && !address.StartsWith("https://"))
            {
                address = "http://" + address;
            }
            try
            {
                KeywordResult result = new KeywordResult(KeywordResults.Pass);
                PrepareDriver(Browsers.Edge, result);
                if (!result.Result.Equals(KeywordResults.Pass))
                {
                    return result;
                }
                EdgeDriverService edgeDriverService = EdgeDriverService.CreateDefaultService(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "MicrosoftWebDriver.exe");

                edgeDriverService.HideCommandPromptWindow = true;
                EdgeOptions edgeOptions = new EdgeOptions();
                edgeOptions.AddAdditionalOption("useSeleniumManager", false);
                edgeOptions.AcceptInsecureCertificates = true;

                _driver = new EdgeDriver(edgeDriverService, edgeOptions, TimeSpan.FromSeconds(timeoutSec));
                _driver.Manage().Window.Maximize();
                _driver.Navigate().GoToUrl(address);
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                Logger.Error("Error open Edge.", ex);
                Logger.Error(EDGE_DRIVER_ERROR_MESSAGE, ex);
                KeywordResult result = new KeywordResult(KeywordResults.Error);
                result.AdditionalInfo = ex.ToString() + "\r\n"+EDGE_DRIVER_ERROR_MESSAGE;
                result.Output = "Error to open browser.";
                return result;
            }
        }

        [KeywordDescription("Open web page with Firefox.")]
        [KeywordDisplayName("Open with FireFox")]
        [KeywordParameters("address", "Web address to open")]
        [SampleScript("Web.Open with FireFox (www.hp.com)")]
        public KeywordResult OpenWithFireFox(string address)
        {
            return OpenWithFireFox(address, "60");
        }

        [KeywordDescription("Open web page with Firefox.")]
        [KeywordDisplayName("Open with FireFox")]
        [KeywordParameters("address", "Web address to open")]
        [KeywordParameters("timeout", "Default Timeout in seconds")]
        [SampleScript("Web.Open with FireFox (www.hp.com,5)")]
        public KeywordResult OpenWithFireFox(string address, string timeOut)
        {
            if (CommonExecutionInfo.IsHeadlessMode)
            {
                Logger.Trace("GFrined is running under headless mode. Use Open instead of Open With FireFox");
                return Open(address, timeOut);
            }
            int timeoutSec = int.Parse(timeOut);
            if (!address.StartsWith("http://") && !address.StartsWith("https://"))
            {
                address = "http://" + address;
            }
            try
            {
                KeywordResult result = new KeywordResult(KeywordResults.Pass);
                PrepareDriver(Browsers.FireFox, result);
                if (!result.Result.Equals(KeywordResults.Pass))
                {
                    return result;
                }
                FirefoxDriverService ffDriverService = FirefoxDriverService.CreateDefaultService(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "geckodriver.exe");
                ffDriverService.HideCommandPromptWindow = true;
                FirefoxOptions firefoxOptions = new FirefoxOptions();
                firefoxOptions.AcceptInsecureCertificates = true;
                firefoxOptions.AddAdditionalOption("useSeleniumManager", false);

                _driver = new FirefoxDriver(ffDriverService, firefoxOptions, TimeSpan.FromSeconds(timeoutSec));
                _driver.Manage().Window.Maximize();
                _driver.Navigate().GoToUrl(address);
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                Logger.Error("Error open FireFox.", ex);
                KeywordResult result = new KeywordResult(KeywordResults.Error);
                result.AdditionalInfo = ex.ToString();
                result.Output = "Error to open browser.";
                return result;
            }
        }

        [KeywordDescription("Set Default timeout for finding object")]
        [KeywordDisplayName("Set Default Timeout")]
        [KeywordParameters("timeout", "Timeout in seconds")]
        [SampleScript("Web.Set Default Timeout (5)")]
        public KeywordResult SetDefaultTimeout(string timeout)
        {
            if(int.TryParse(timeout, out int timeOutInt))
            {
                _timeOutInSec = timeOutInt;
                return new KeywordResult(KeywordResults.Pass);
            }
            else
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"{timeout} is not a number";
                return error;
            }
        }

        [KeywordDescription("Go to given address")]
        [KeywordDisplayName("Go To")]
        [KeywordParameters("address", "address to navigate")]
        [SampleScript("Web.Go To(//*[@id=InternalPages_Index_ConfigurationPage])")]
        public KeywordResult GoTo(string address)
        {
            if(!address.StartsWith("http://") && !address.StartsWith("https://"))
            {
                address = "http://" + address;
            }
            try
            {
                _driver.Navigate().GoToUrl(address);
                return new KeywordResult(KeywordResults.Pass);
            }
            catch(Exception ex)
            {
                KeywordResult result = new KeywordResult(KeywordResults.Error);
                result.Output = "Error to navigate";
                result.AdditionalInfo = ex.ToString();
                Logger.Error("Error to navigate", ex);
                return result;
            }
            
        }

        [KeywordDescription("Select given frame. Use this keyword if page has iframe elements")]
        [KeywordDisplayName("Select Frame")]
        [KeywordParameters("frameName", "Frame name to select")]
        [SampleScript("Web.Select Frame (/html/body/iframe[1])")]
        public KeywordResult SelectFrame(string frameName)
        {
            try
            {
                By frameSelector = GetFrameSelector(frameName);
                if(frameName.Contains("index="))
                {
                    int.TryParse(frameName.Split('=')[0], out int index);
                    Logger.Debug($"Select fram by index : index = {index}");
                    _driver = _driver.SwitchTo().Frame(index);
                }
                else if(frameSelector == null)
                {
                    Logger.Debug($"Select fram by name : index = {frameName}");
                    _driver = _driver.SwitchTo().Frame(frameName);
                }
                else
                {
                    Logger.Debug($"Select fram by selector : index = {frameName}");
                    _driver = _driver.SwitchTo().Frame(FindElement(frameSelector, TimeSpan.FromSeconds(3)));
                }
                return new KeywordResult(KeywordResults.Pass);
            }
            catch(Exception ex)
            {
                KeywordResult result = new KeywordResult(KeywordResults.Fail);
                result.AdditionalInfo = ex.ToString();
                result.Output = "Fail to select frame";
                if (!_disableScreenShotForFails)
                {
                    result.ScreenShot = GetScreenShot();
                }
                Logger.Debug(GetPageSource());
                return result;
            }
        }

        [KeywordDescription("Unselect frame and return to select root element")]
        [KeywordDisplayName("Select Default Frame")]
        [SampleScript("Web.Select Default Frame")]
        public KeywordResult SelectDefaultFrame()
        {
            try
            {
                _driver.SwitchTo().DefaultContent();
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                KeywordResult result = new KeywordResult(KeywordResults.Fail);
                result.AdditionalInfo = ex.ToString();
                result.Output = "Fail to select default frame";
                if (!_disableScreenShotForFails)
                {
                    result.ScreenShot = GetScreenShot();
                }
                Logger.Debug(GetPageSource());
                return result;
            }
        }


        [KeywordDescription("Select tab with given index")]
        [KeywordDisplayName("Select Tab")]
        [KeywordParameters("tabIndex", "Tab index to select. Starts with 1")]
        [SampleScript("Web.Select Tab (1)")]
        public KeywordResult SelectTab(string tabIndex)
        {
            int index = int.Parse(tabIndex);
            index--;
            try
            {
                _driver.SwitchTo().Window(_driver.WindowHandles[index]);
                return new KeywordResult(KeywordResults.Pass);
            }
            catch(Exception ex)
            {
                KeywordResult result = new KeywordResult(KeywordResults.Fail);
                result.AdditionalInfo = ex.ToString();
                result.Output = "Fail to select tab page";
                if (!_disableScreenShotForFails)
                {
                    result.ScreenShot = GetScreenShot();
                }
                return result;
            }
        }

        [KeywordDescription("Click object with given xpath")]
        [KeywordDisplayName("Click Object")]
        [KeywordParameters("xPath", "xPath of object to click")]
        [SampleScript("Web.Click Object (//*[@id=collectNameNext]/div/button/span)")]
        public KeywordResult ClickObject(string xPath)
        {
            try
            {
                IWebElement obj = FindElement(GetSelectorByText(xPath), TimeSpan.FromSeconds(_timeOutInSec));
                if(obj == null)
                {
                    KeywordResult result = new KeywordResult(KeywordResults.Fail);
                    result.AdditionalInfo = "Can not find element";
                    result.Output = "Fail to click xpath";
                    if (!_disableScreenShotForFails)
                    {
                        result.ScreenShot = GetScreenShot();
                    }
                    Logger.Debug(GetPageSource());
                    return result;
                }
                obj.Click();
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                KeywordResult result = new KeywordResult(KeywordResults.Error);
                result.AdditionalInfo = ex.ToString();
                result.Output = "Fail to click xpath";
                if (!_disableScreenShotForFails)
                {
                    result.ScreenShot = GetScreenShot();
                }
                Logger.Error("Error during click", ex);
                Logger.Debug(GetPageSource());
                return result;
            }
        }

        [KeywordDescription("Click object with given xpath")]
        [KeywordDisplayName("Click Object With Index")]
        [KeywordParameters("xPath", "xPath of object to click")]
        [KeywordParameters("index", "index to click. Starts with 1")]
        [SampleScript("Web.Click Object With Index (//*[@id=APjFqb],1)")]
        public KeywordResult ClickObjectWithIndex(string xPath, string index)
        {
            try
            {
                int idx = int.Parse(index);
                idx--;
                IWebElement element = _driver.FindElements(GetSelectorByText(xPath))[idx];
                if (element == null)
                {
                    KeywordResult result = new KeywordResult(KeywordResults.Fail);
                    result.AdditionalInfo = "Can not find element";
                    result.Output = "Fail to click xpath";
                    if (!_disableScreenShotForFails)
                    {
                        result.ScreenShot = GetScreenShot();
                    }
                    Logger.Debug(GetPageSource());
                    return result;
                }

                element.Click();
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                KeywordResult result = new KeywordResult(KeywordResults.Error);
                result.AdditionalInfo = ex.ToString();
                result.Output = "Fail to click xpath";
                if (!_disableScreenShotForFails)
                {
                    result.ScreenShot = GetScreenShot();
                }
                Logger.Error("Error during Click with index", ex);
                Logger.Debug(GetPageSource());
                return result;
            }

            throw new NotImplementedException();
        }

        [KeywordDescription("DoubleClick object with given xpath")]
        [KeywordDisplayName("DoubleClick")]
        [KeywordParameters("xPath", "xPath of object to doubleclick")]
        [SampleScript("Web.DoubleClick (/html/body)")]
        public KeywordResult DoubleClick(string xPath)
        {
            try
            {
                Actions actions = new Actions(_driver);
                IWebElement obj = FindHiddenElement(GetSelectorByText(xPath), TimeSpan.FromSeconds(_timeOutInSec));
                if (obj == null)
                {
                    KeywordResult result = new KeywordResult(KeywordResults.Fail);
                    result.AdditionalInfo = "Can not find element";
                    result.Output = "Fail to DoubleClick on given xpath";
                    if (!_disableScreenShotForFails)
                    {
                        result.ScreenShot = GetScreenShot();
                    }
                    Logger.Debug(GetPageSource());
                    return result;
                }
                actions.MoveToElement(obj).DoubleClick().Perform();
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                KeywordResult result = new KeywordResult(KeywordResults.Error);
                result.AdditionalInfo = ex.ToString();
                result.Output = "Fail to DoubleClick xpath";
                if (!_disableScreenShotForFails)
                {
                    result.ScreenShot = GetScreenShot();
                }
                Logger.Error("Error during DoubleClick", ex);
                Logger.Debug(GetPageSource());
                return result;
            }
        }
        [KeywordDescription("checks Object with given xpath is Enabled and Displayed")]
        [KeywordDisplayName("IsClickable")]
        [KeywordParameters("xPath", "xPath of object to validate clickable")]
        [SampleScript("Web.IsClickable(//*[@for=GeneralFaxSendJBIGCompression])")]
        public KeywordResult IsClickable(string xPath)
        {
            try
            {                  
               var r= _driver.FindElement(By.XPath(xPath)).GetAttribute("readOnly");
                if (_driver.FindElement(By.XPath(xPath)).Enabled == false || _driver.FindElement(By.XPath(xPath)).Displayed == false|| r=="true")
                {
                    KeywordResult result = new KeywordResult(KeywordResults.Fail);
                    result.AdditionalInfo = "Can not find element";
                    result.Output = "Fail to click xpath";
                    if (!_disableScreenShotForFails)
                    {
                        result.ScreenShot = GetScreenShot();
                    }
                    Logger.Debug(GetPageSource());
                    return result;
                }
                else
                {
                    return new KeywordResult(KeywordResults.Pass);
                }
            }
            catch (Exception ex)
            {
                KeywordResult result = new KeywordResult(KeywordResults.Error);
                result.AdditionalInfo = ex.Message;
                result.Output = "xpath is neigther Enabled or Displayed";
                 if (!_disableScreenShotForFails)
                {
                    result.ScreenShot = GetScreenShot();
                }
                Logger.Error("Error during IsClickable event", ex);
                Logger.Debug(GetPageSource());
                return result;
            }
        }
        [KeywordDescription("Checks whether checkbox label has checked class")]
        [KeywordDisplayName("IsChecked")]
        [KeywordParameters("xPath", "xPath of label to validate checked state")]
        [SampleScript("Web.IsChecked(//*[@for='ShowHomescreenStopButton'])")] 
        public KeywordResult IsChecked(string xPath)
        {
            if (string.IsNullOrEmpty(xPath))
            {
                KeywordResult result = new KeywordResult(KeywordResults.Fail);
                result.Output = "XPath cannot be null or empty";
                result.AdditionalInfo = "Provided XPath is null or empty";
                return result;
            }
            try
            {
                var element = _driver.FindElement(By.XPath(xPath));
                string classValue = element.GetAttribute("class");

                if (classValue != null && classValue.Contains("checked"))
                {
                    KeywordResult result = new KeywordResult(KeywordResults.Pass);
                    result.Output = "Checkbox is checked";
                    return result;
                }
                else
                {
                    KeywordResult result = new KeywordResult(KeywordResults.Fail);
                    result.AdditionalInfo = $"The 'checked' class was not found for the element with XPath: {xPath}";
                    result.Output = "Checkbox is not checked";

                    if (!_disableScreenShotForFails)
                    {
                        result.ScreenShot = GetScreenShot();
                    }

                    Logger.Debug(GetPageSource());
                    return result;
                }
            }
            catch (Exception ex)
            {
                KeywordResult result = new KeywordResult(KeywordResults.Error);
                result.AdditionalInfo = ex.Message;
                result.Output = "Error while checking checkbox state";

                if (!_disableScreenShotForFails)
                {
                    result.ScreenShot = GetScreenShot();
                }

                Logger.Error("Error during IsChecked event", ex);
                Logger.Debug(GetPageSource());
                return result;
            }
        }

        [KeywordDescription("Click object which contains given text")]
        [KeywordDisplayName("Click Text")]
        [KeywordParameters("text", "text of object to click")]
        [SampleScript("Web.Click Text (Gmail)")]
        public KeywordResult ClickText(string text)
        {
            return ClickObject($"//*[contains(text(),'{text}')]");
        }

        [KeywordDescription("Click nth object with given text")]
        [KeywordDisplayName("Click Text With Index")]
        [KeywordParameters("text", "text of object to click")]
        [KeywordParameters("index", "index to click. Starts with 1")]
        [SampleScript("Web.Click Text With Index(Gmail,1)")]
        public KeywordResult ClickTextWithIndex(string text, string index)
        {
            return ClickObjectWithIndex($"//*[contains(text(),'{text}')]", index);
        }
        [KeywordDescription("Select value from list control")]
        [KeywordDisplayName("Select Value From List Box")]
        [KeywordParameters("List control xpath", "xpath of list control")]
        [KeywordParameters("Value", "Value/Text to select")]
        [SampleScript("Web.Select Value From List Box (//select[@id='sourceSelect'], AES256-SHA)")]
        public KeywordResult SelectValueFromListBox(string xpath, string value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(xpath) || string.IsNullOrWhiteSpace(value))
                {
                    return new KeywordResult(KeywordResults.Fail, "Xpath or value cannot be empty.");
                }

                IWebElement element = FindElement(GetSelectorByText(xpath), TimeSpan.FromSeconds(_timeOutInSec));

                if (element == null)
                {
                    return new KeywordResult(KeywordResults.Fail, "List control not found.");
                }

                var options = element.FindElements(By.TagName("option"));

                bool isSelected = false;

                foreach (var option in options)
                {
                    if (option.Text.Trim().Equals(value.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        option.Click();
                        isSelected = true;
                        break;
                    }
                }

                if (!isSelected)
                {
                    return new KeywordResult(KeywordResults.Fail, $"Value '{value}' not found in list.");
                }

                return new KeywordResult(KeywordResults.Pass, $"Value '{value}' selected successfully.");
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Select From List", ex);
                return new KeywordResult(KeywordResults.Fail, ex.Message);
            }
        }

        [KeywordDescription("Wait object to be shown until given timeout")]
        [KeywordDisplayName("Wait For Object")]
        [KeywordParameters("xPath", "xPath of object to wait")]
        [KeywordParameters("timeout", "Waiting time in seconds")]
        [SampleScript("Web.Wait For Object (//*[@id=DeviceModemSettingsCountryRegion2],5)")]
        public KeywordResult WaitForObject(string xPath, string timeoutInSec)
        {
            TimeSpan waitTime = TimeSpan.FromSeconds(int.Parse(timeoutInSec));

            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            if (!IsExist(xPath, waitTime))
            {
                kr.Result = KeywordResults.Fail;
                kr.Output = $"Can not find {xPath} in the screen within in given timeout {timeoutInSec} second(s)";
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = GetScreenShot();
                }
                Logger.Debug(GetPageSource());
            }
            return kr;
        }

        [KeywordDescription("Wait text to be shown until given timeout")]
        [KeywordDisplayName("Wait For Text")]
        [KeywordParameters("text", "Text of object to wait")]
        [KeywordParameters("timeout", "Waiting time in seconds")]
        [SampleScript("Web.Wait For Text(Gmail, 5)")]
        public KeywordResult WaitForText(string text, string timeoutInSec)
        {
            return WaitForObject($"//*[contains(text(),'{text}')]", timeoutInSec);
        }

        [KeywordDescription("Wait object to be disappeared until given timeout")]
        [KeywordDisplayName("Wait For Object Disappear")]
        [KeywordParameters("xPath", "xPath of object to wait")]
        [KeywordParameters("timeout", "Waiting time in seconds")]
        [SampleScript("Web.Wait For Object Disappear (//*[@id=collectNameNext]/div/button/span,5)")]
        public KeywordResult WaitForObjectDisappear(string xPath, string timeoutInSec)
        {
            TimeSpan waitTime = TimeSpan.FromSeconds(int.Parse(timeoutInSec));
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            double totalWaitTime = waitTime.TotalMilliseconds;
            IWebElement webElement = null;
            while (totalWaitTime >= 0)
            {
                try
                {
                    webElement = _driver.FindElement(GetSelectorByText(xPath));
                    Thread.Sleep(500);
                    totalWaitTime -= 500;
                }
                catch (Exception)
                {
                    return new KeywordResult(KeywordResults.Pass);
                }
            }

            kr.Result = KeywordResults.Fail;
            kr.Output = $"Object with Xpath ({xPath}) is still in the screen after {timeoutInSec} second(s)";
            if (!_disableScreenShotForFails)
            {
                kr.ScreenShot = GetScreenShot();
            }
            Logger.Debug(GetPageSource());
            
            return kr;
        }

        [KeywordDescription("Wait text to be disappeard until given timeout")]
        [KeywordDisplayName("Wait For Text Disappear")]
        [KeywordParameters("text", "Text of object to wait")]
        [KeywordParameters("timeout", "Waiting time in seconds")]
        [SampleScript("Web.Wait For Text Disappear(Gmail, 5)")]
        public KeywordResult WaitForTextDisappear(string text, string timeoutInSec)
        {
            return WaitForObjectDisappear("//*[contains(text(),'{text}')]", timeoutInSec);
        }

        [KeywordDescription("Clear text of object with given xPath. If this keyword is not working, use Clear Text Field Keyword instead")]
        [KeywordDisplayName("Clear Text")]
        [KeywordParameters("xPath", "xPath of object to clear")]
        [SampleScript("Web.Clear Text (//*[@id=lastName])")]
        public KeywordResult ClearText(string xPath)
        {
            IWebElement element = FindElement(GetSelectorByText(xPath), TimeSpan.FromSeconds(_timeOutInSec));
            if(element == null)
            {
                KeywordResult kr = new KeywordResult(KeywordResults.Fail);
                kr.Output = $"Can not find element with xpath : {xPath}";
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = GetScreenShot();
                }
                Logger.Debug(GetPageSource());
                return kr;
            }
            try
            {
                element.Clear();
                return new KeywordResult(KeywordResults.Pass);
            }
            catch(Exception ex)
            {
                Logger.Error("Error during Check Text", ex);
                KeywordResult kr = new KeywordResult(KeywordResults.Error);
                kr.Output = $"Error during clearing field";
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = GetScreenShot();
                }
                kr.AdditionalInfo = ex.ToString();
                Logger.Debug(GetPageSource());
                return kr;
            }
            
        }

        [KeywordDescription("Clear text field with keyboard event. Use this if Clear Text Keyword is not working")]
        [KeywordDisplayName("Clear Text Field")]
        [KeywordParameters("xPath", "xPath of object to clear")]
        [SampleScript("Web.Clear Text Field (//*[@id=lastName])")]
        public KeywordResult ClearTextField(string xPath)
        {
            IWebElement element = FindElement(GetSelectorByText(xPath), TimeSpan.FromSeconds(_timeOutInSec));
            if (element == null)
            {
                KeywordResult kr = new KeywordResult(KeywordResults.Fail);
                kr.Output = $"Can not find element with xpath : {xPath}";
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = GetScreenShot();
                }
                Logger.Debug(GetPageSource());
                return kr;
            }
            try
            {
                element.SendKeys(Keys.Control + "a");
                element.SendKeys(Keys.Delete);
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                Logger.Error("Error during Check Text", ex);
                KeywordResult kr = new KeywordResult(KeywordResults.Error);
                kr.Output = $"Error during clearing field with keyboard event";
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = GetScreenShot();
                }
                kr.AdditionalInfo = ex.ToString();
                Logger.Debug(GetPageSource());
                return kr;
            }

        }

        [KeywordDescription("Clear and set text at object with given xPath.")]
        [KeywordDisplayName("Set Text")]
        [KeywordParameters("xPath", "xPath of object to set")]
        [KeywordParameters("textToSet", "Text to input")]
        [SampleScript("Web.Set Text(//*[@id=APjFqb],hp.com)")]
        public KeywordResult SetText(string xPath, string textToSet)
        {
            IWebElement element = element = FindElement(GetSelectorByText(xPath), TimeSpan.FromSeconds(_timeOutInSec));

            if (element == null)
            {
                KeywordResult kr = new KeywordResult(KeywordResults.Fail);
                kr.Output = $"Can not find element with xpath : {xPath}";
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = GetScreenShot();
                }
                Logger.Debug(GetPageSource());
                return kr;
            }
            try
            {
                if(textToSet.EndsWith(@"\n"))
                {
                    textToSet = textToSet.Replace(@"\n", string.Empty) + Keys.Enter;
                }
                element.Clear();
                    
                if (element.GetAttribute("Type") != null && element.GetAttribute("Type").ToLower() == "search")
                {
                    char[] chars = textToSet.ToCharArray();
                    for (int i = 0; i < chars.Length; i++)                    
                    {
                        element.SendKeys(chars[i].ToString());
                    }
                }
                else
                {
                    element.SendKeys(textToSet);        
                }

                return new KeywordResult(KeywordResults.Pass);
            }
            catch(Exception ex)
            {
                Logger.Error("Error during clearing or setting Text", ex);
                KeywordResult kr = new KeywordResult(KeywordResults.Error);
                kr.Output = $"Error during clearing or setting field";
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = GetScreenShot();
                }
                kr.AdditionalInfo = ex.ToString();
                Logger.Debug(GetPageSource());
                return kr;
            }
            
            
        }

        [KeywordDescription("Get url of a browser which opens on click of a windows button or recently opened browser\r\n. Used to switch from Windows app to Web app testing")]
        [KeywordDisplayName("GetDefBrowsersRecentURL")]
        [KeywordParameters("saveTo", "variable name (ex. ${OutPut}) to store value")]
        [KeywordParameters("defBrowserName", "default browser name like edge, chrome, ie,firefox")]
        [SampleScript("Web.GetDefBrowsersRecentURL (${URL},Edge)")]
        public KeywordResult GetDefBrowsersRecentURL(string saveTo,string defBrowser)
        {
            try
            {
                string siteurl = "";
                string processName = "msedge";

                if (_driver == null)
                {
                    if (defBrowser.Equals("chrome",StringComparison.OrdinalIgnoreCase))
                        processName = "chrome";
                    else if (defBrowser.Equals("firefox",StringComparison.OrdinalIgnoreCase))
                        processName = "firefox";
                    else if (defBrowser.Equals("ie",StringComparison.OrdinalIgnoreCase))
                        processName = "iexplore";
                    else if (defBrowser.Equals("edge",StringComparison.OrdinalIgnoreCase))
                        processName = "msedge";

                    var procList = System.Diagnostics.Process.GetProcessesByName(processName);
                    foreach (var proc in procList)
                    {
                        siteurl = GetBrowserURL(proc);
                        if (!String.IsNullOrEmpty(siteurl))
                        {
                            CommonExecutionInfo.SetVariable(saveTo, siteurl);
                            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
                            kr.Output = $"{saveTo} : {siteurl}";
                            return kr;
                        }
                        Logger.Debug("current url" + siteurl);
                    }
                }

                string webUrl = new Uri(_driver.SwitchTo().Window(_driver.CurrentWindowHandle).Url, UriKind.Absolute).ToString();
                if (string.IsNullOrEmpty(webUrl))
                {
                    KeywordResult kr = new KeywordResult(KeywordResults.Fail);
                    kr.Output = $"Can not get url : {webUrl}";
                    if (!_disableScreenShotForFails)
                    {
                        kr.ScreenShot = GetScreenShot();
                    }
                    Logger.Debug(GetPageSource());
                    return kr;
                }
                else
                {
                    CommonExecutionInfo.SetVariable(saveTo, webUrl);
                    KeywordResult kr = new KeywordResult(KeywordResults.Pass);
                    kr.Output = $"{saveTo} : {webUrl}";
                    return kr;
                }
            }
            catch (Exception ex)
            {
                KeywordResult kr = new KeywordResult(KeywordResults.Error);
                Logger.Error("Error GetDefaultBrowsersURL", ex);
                kr.Output = $"Error during GetDefaultBrowsersURL";
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = GetScreenShot();
                }
                kr.AdditionalInfo = ex.ToString();
                Logger.Debug(GetPageSource());
                return kr;
            }
        }
        [GetKeyword]
        [KeywordDescription("Get CSS Property of the element")]
        [KeywordDisplayName("Get CSS Property")]
        [KeywordParameters("xPath", "xPath of object to compare")]
        [KeywordParameters("PropertName", "ex. font-size/background-color/font-family/line-height/font-weight")]
        [KeywordParameters("saveTo", "variable name (ex. ${Buffer}) to store value")]
        [SampleScript("Web.Get CSS Property (//*[@id=container]/div/div[3]/div[1]/div[2]/div[2]/div/div[2]/div/a/div[1]/div/div/div/img,font-style,${Style})")]
        public KeywordResult GetCSSProperty(string xPath, string PropertyName, string saveTo)
        {
            xPath = Support.Utils.GetVariablevalueIfExist(xPath);
            IWebElement element = element = FindElement(GetSelectorByText(xPath), TimeSpan.FromSeconds(_timeOutInSec));
            if (element == null)
            {
                KeywordResult kr = new KeywordResult(KeywordResults.Fail);
                kr.Output = $"Can not find element with xpath : {xPath}";
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = GetScreenShot();
                }
                Logger.Debug(GetPageSource());
                return kr;
            }

            try
            {
                string propertyValue = element.GetCssValue(PropertyName);

                if (string.IsNullOrEmpty(propertyValue))
                {
                    KeywordResult kr = new KeywordResult(KeywordResults.Fail);
                    kr.Output = $"Can not get text of element with xpath : {xPath}";
                    if (!_disableScreenShotForFails)
                    {
                        kr.ScreenShot = GetScreenShot();
                    }
                    Logger.Debug(GetPageSource());
                    return kr;
                }
                else
                {
                    CommonExecutionInfo.SetVariable(saveTo, propertyValue);
                    KeywordResult kr = new KeywordResult(KeywordResults.Pass);
                    kr.Output = $"{saveTo} : {propertyValue}";
                    return kr;
                }

            }
            catch (Exception ex)
            {
                KeywordResult kr = new KeywordResult(KeywordResults.Error);
                Logger.Error("Error while getting the css property", ex);
                kr.Output = $"Error while getting the css property. property may not exist";
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = GetScreenShot();
                }
                kr.AdditionalInfo = ex.ToString();
                Logger.Debug(GetPageSource());
                return kr;
            }

        }


        [GetKeyword]
        [KeywordDescription("Get url of current tab and store to given variable")]
        [KeywordDisplayName("GetCurrentURL")] 
        [KeywordParameters("saveTo", "variable name (ex. ${OutPut}) to store value")]
        [SampleScript("Web.GetCurrentURL (${URL})")]
        public KeywordResult GetCurrentURL(string saveTo)
        {
            try
            {
               
                string webUrl = new Uri(_driver.SwitchTo().Window(_driver.CurrentWindowHandle).Url, UriKind.Absolute).ToString();
                if (string.IsNullOrEmpty(webUrl))
                {
                    KeywordResult kr = new KeywordResult(KeywordResults.Fail);
                    kr.Output = $"Can not get url : {webUrl}";
                    if (!_disableScreenShotForFails)
                    {
                        kr.ScreenShot = GetScreenShot();
                    }
                    Logger.Debug(GetPageSource());
                    return kr;
                }
                else
                {
                    CommonExecutionInfo.SetVariable(saveTo, webUrl);
                    KeywordResult kr = new KeywordResult(KeywordResults.Pass);
                    kr.Output = $"{saveTo} : {webUrl}";
                    return kr;
                }
            }
            catch (Exception ex)
            {
                KeywordResult kr = new KeywordResult(KeywordResults.Error);
                Logger.Error("Error getting current url", ex);
                kr.Output = $"Error during get current url";
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = GetScreenShot();
                }
                kr.AdditionalInfo = ex.ToString();
                Logger.Debug(GetPageSource());
                return kr;
            }
        }

        [KeywordDescription("checks the current URL with the provoided url")]
        [KeywordDisplayName("IsCurrentURL")]
        [KeywordParameters("URL", "URL should include protocol")]
        [SampleScript("Web.IsCurrentURL (www.flipkart.com)")]
        public KeywordResult IsCurrentURL(string URL)
        {
            try
            {
                Uri uri1 = new Uri(URL, UriKind.Absolute);
                Uri uri2 = new Uri(_driver.SwitchTo().Window(_driver.CurrentWindowHandle).Url, UriKind.Absolute);
                var result = Uri.Compare(uri1, uri2,
                UriComponents.Host,
                UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
                KeywordResult result1 = new KeywordResult(KeywordResults.Fail);
                if (result == 0)
                {
                    return new KeywordResult(KeywordResults.Pass);
                }
                else
                {
                    result1.Result = KeywordResults.Fail;
                    result1.Output = $"Browser URL {uri2} is not the same as given in argument : {uri1}";
                    if (!_disableScreenShotForFails)
                    {
                        result1.ScreenShot = GetScreenShot();
                    }
                    return result1;
                }
            }
            catch (Exception ex)
            {
                KeywordResult result = new KeywordResult(KeywordResults.Error);
                result.Output = "Error in comparing the URL";
                result.AdditionalInfo = ex.ToString();
                Logger.Error("Error in comparing the URL", ex);
                return result;
            }
            }
        [KeywordDescription("Append text at object with given xPath. (Do not clear current text of object)")]
        [KeywordDisplayName("Append Text")]
        [KeywordParameters("xPath", "xPath of object to set")]
        [KeywordParameters("textToSet", "Text to input")]
        [SampleScript("Web.Append Text (//*[@id=APjFqb],amazon)")]
        public KeywordResult AppendText(string xPath, string textToSet)
        {
            IWebElement element = element = FindElement(GetSelectorByText(xPath), TimeSpan.FromSeconds(_timeOutInSec));

            if (element == null)
            {
                KeywordResult kr = new KeywordResult(KeywordResults.Fail);
                kr.Output = $"Can not find element with xpath : {xPath}";
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = GetScreenShot();
                }
                Logger.Debug(GetPageSource());
                return kr;
            }
            try
            {
                if (textToSet.EndsWith(@"\n"))
                {
                    textToSet = textToSet.Replace(@"\n", string.Empty) + Keys.Enter;
                }
                element.SendKeys(textToSet);
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                Logger.Error("Error during clearing or setting Text", ex);
                KeywordResult kr = new KeywordResult(KeywordResults.Error);
                kr.Output = $"Error during clearing or setting field";
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = GetScreenShot();
                }
                kr.AdditionalInfo = ex.ToString();
                Logger.Debug(GetPageSource());
                return kr;
            }


        }

        [KeywordDescription("Submit the form with given xpath. This keyword is added for compatability with GF Web Recorder")]
        [KeywordDisplayName("Submit")]
        [KeywordParameters("xPath", "xPath of object to click")]
        [SampleScript("Web.Submit(//*[@id=collectNameNext]/div/button/span)")]
        public KeywordResult Submit(string xPath)
        {
            try
            {
                IWebElement obj = FindElement(GetSelectorByText(xPath), TimeSpan.FromSeconds(_timeOutInSec));
                if (obj == null)
                {
                    KeywordResult result = new KeywordResult(KeywordResults.Fail);
                    result.AdditionalInfo = "Can not find element";
                    result.Output = "Fail to submit";
                    if (!_disableScreenShotForFails)
                    {
                        result.ScreenShot = GetScreenShot();
                    }
                    Logger.Debug(GetPageSource());
                    return result;
                }
                obj.Submit();
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                KeywordResult result = new KeywordResult(KeywordResults.Error);
                result.AdditionalInfo = ex.ToString();
                result.Output = "Error to submit";
                if (!_disableScreenShotForFails)
                {
                    result.ScreenShot = GetScreenShot();
                }
                Logger.Error("Error during submit", ex);
                Logger.Debug(GetPageSource());
                return result;
            }
        }


        [KeywordDescription("Compare text(inner text or value attiribute if inner text is empty) of object with given text")]
        [KeywordDisplayName("Check Text Of Object")]
        [KeywordParameters("xPath", "xPath of object to compare")]
        [KeywordParameters("text", "Expected text of object")]
        [SampleScript("Web.Check Text Of Object(/html/body/main/div[1]/div/div/div/div/div[1]/div[3]/div[1]/a/span[2],Create an account)")]
        public KeywordResult CheckTextOfObject(string xPath, string text)
        {
            IWebElement element = element = FindElement(GetSelectorByText(xPath), TimeSpan.FromSeconds(_timeOutInSec));
            if (element == null)
            {
                KeywordResult kr = new KeywordResult(KeywordResults.Fail);
                kr.Output = $"Can not find element with xpath : {xPath}";
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = GetScreenShot();
                }
                Logger.Debug(GetPageSource());
                return kr;
            }

            try
            {
                string webText = element.Text;
                if(string.IsNullOrEmpty(webText))
                {
                    webText = element.GetAttribute("value");
                }

                if(string.IsNullOrEmpty(webText))
                {
                    KeywordResult kr = new KeywordResult(KeywordResults.Fail);
                    kr.Output = $"Can not get text of element with xpath : {xPath}";
                    if (!_disableScreenShotForFails)
                    {
                        kr.ScreenShot = GetScreenShot();
                    }
                    Logger.Debug(GetPageSource());
                    return kr;
                }

                if(webText.Equals(text))
                {
                    return new KeywordResult(KeywordResults.Pass);
                }
                else
                {
                    Logger.Error($"Failed to compare [Expected : {text} , Actual : {webText}]");
                    KeywordResult kr = new KeywordResult(KeywordResults.Fail);
                    kr.Output = $"Expected : {text} , Actual : {webText}";
                    if (!_disableScreenShotForFails)
                    {
                        kr.ScreenShot = GetScreenShot();
                    }
                    return kr;
                }
            }
            catch (Exception ex)
            {
                KeywordResult kr = new KeywordResult(KeywordResults.Error);
                Logger.Error("Error during Check Text", ex);
                kr.Output = $"Error during clearing or setting field";
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = GetScreenShot();
                }
                kr.AdditionalInfo = ex.ToString();
                Logger.Debug(GetPageSource());
                return kr;
            }
        }        

        [GetKeyword]
        [KeywordDescription("Get text of given xpath and store to given variable. Automatically searches in main document and all iframes. Supports dynamic content and complex elements. It will add string escape character if commas are there in the text. It replaces comma with backslash and comma")]
        [KeywordDisplayName("Get Text")]
        [KeywordParameters("xPath", "xPath of object to compare")]
        [KeywordParameters("saveTo", "variable name (ex. ${Buffer}) to store value")]
        [SampleScript("Web.Get  Text(//*[@id=DeviceModemSettingsPhoneNumber],${r})")]
        public KeywordResult GetText(string xPath, string saveTo)
        {
            xPath = Support.Utils.GetVariablevalueIfExist(xPath);
            string webText = null;

            webText = TryExtractText(xPath);

            if (string.IsNullOrEmpty(webText))
            {
                KeywordResult kr = new KeywordResult(KeywordResults.Fail);
                kr.Output = $"Can not find element or extract text with xpath : {xPath}";
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = GetScreenShot();
                }
                Logger.Debug(GetPageSource());
                return kr;
            }

            try
            {
                // Format it If it has comma in its value SWQATR-593
                if (webText.Contains(","))
                {
                    webText = webText.Replace(",", @"\,");
                    webText = webText.Replace(@"\\", @"\");
                }

                CommonExecutionInfo.SetVariable(saveTo, webText);
                KeywordResult kr = new KeywordResult(KeywordResults.Pass);
                kr.Output = $"{saveTo} : {webText}";
                return kr;
            }
            catch (Exception ex)
            {
                KeywordResult kr = new KeywordResult(KeywordResults.Error);
                Logger.Error("Error during Get Text", ex);
                kr.Output = $"Error during extracting or storing text";
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = GetScreenShot();
                }
                kr.AdditionalInfo = ex.ToString();
                Logger.Debug(GetPageSource());
                return kr;
            }
        }
        /// <summary>
        /// Attempts to extract text from an element, searching in main document and all iframes
        /// </summary>
        private string TryExtractText(string xPath)
        {
            string text = null;

            Logger.Debug($"Searching in main document for xpath: {xPath}");
            text = ExtractTextFromElement(xPath);
            if (!string.IsNullOrEmpty(text))
            {
                Logger.Debug($"Found element in main document");
                return text;
            }

            Logger.Debug($"Element not found in main document, searching iframes");
            text = SearchInIframes(xPath);
            if (!string.IsNullOrEmpty(text))
            {
                return text;
            }

            Logger.Debug($"Element not found in iframes, trying JavaScript");
            text = SearchWithJavaScript(xPath);

            return text;
        }

        /// <summary>
        /// Extract text from an element using multiple methods
        /// </summary>
        private string ExtractTextFromElement(string xPath)
        {
            try
            {
                IWebElement element = FindElement(GetSelectorByText(xPath), TimeSpan.FromSeconds(_timeOutInSec));
                if (element == null)
                {
                    return null;
                }

                // Try multiple text extraction methods
                string text = element.Text;
                if (string.IsNullOrEmpty(text))
                {
                    text = element.GetAttribute("value");
                }
                if (string.IsNullOrEmpty(text))
                {
                    text = element.GetAttribute("innerText");
                }
                if (string.IsNullOrEmpty(text))
                {
                    text = element.GetAttribute("textContent");
                }
                if (string.IsNullOrEmpty(text))
                {
                    text = element.GetAttribute("innerHTML");
                }

                return text;
            }
            catch (Exception ex)
            {
                Logger.Debug($"Error extracting text: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Search for element in all iframes
        /// </summary>
        private string SearchInIframes(string xPath)
        {
            try
            {
                var frames = _driver.FindElements(By.TagName("iframe"));
                Logger.Debug($"Found {frames.Count} iframe(s) to search");

                for (int i = 0; i < frames.Count; i++)
                {
                    try
                    {
                        _driver.SwitchTo().Frame(frames[i]);
                        Logger.Debug($"Searching in iframe {i + 1}/{frames.Count}");

                        IWebElement element = FindElement(GetSelectorByText(xPath), TimeSpan.FromSeconds(3));
                        if (element != null)
                        {
                            string text = element.Text;
                            if (string.IsNullOrEmpty(text))
                            {
                                text = element.GetAttribute("value");
                            }
                            if (string.IsNullOrEmpty(text))
                            {
                                text = element.GetAttribute("innerText");
                            }
                            if (string.IsNullOrEmpty(text))
                            {
                                text = element.GetAttribute("textContent");
                            }
                            if (string.IsNullOrEmpty(text))
                            {
                                text = element.GetAttribute("innerHTML");
                            }

                            if (!string.IsNullOrEmpty(text))
                            {
                                Logger.Debug($"Found element in iframe {i + 1}, extracted text");
                                _driver.SwitchTo().DefaultContent();
                                return text;
                            }
                        }

                        _driver.SwitchTo().DefaultContent();
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug($"Error searching in iframe {i + 1}: {ex.Message}");
                        try { _driver.SwitchTo().DefaultContent(); } catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Debug($"Error during iframe search: {ex.Message}");
                try { _driver.SwitchTo().DefaultContent(); } catch { }
            }

            return null;
        }

        /// <summary>
        /// Search using JavaScript - works across main document and iframes
        /// </summary>
        private string SearchWithJavaScript(string xPath)
        {
            try
            {
                IJavaScriptExecutor jsExecutor = _driver as IJavaScriptExecutor;

                string script = @"
                    var xpath = arguments[0];

                    function findElementInDocument(doc, xpath) {
                        var element = null;

                        // Try XPath evaluation
                        try {
                            var result = doc.evaluate(xpath, doc, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null);
                            element = result.singleNodeValue;
                        } catch(e) {}

                        // Try getElementById for ID selectors
                        if (!element && xpath.includes('[@id=')) {
                            try {
                                var idMatch = xpath.match(/\[@id=['\""]]?([^'\""\\]]+)['\""]]?\]/);
                                if (idMatch && idMatch[1]) {
                                    element = doc.getElementById(idMatch[1]);
                                }
                            } catch(e) {}
                        }

                        // Try querySelector for ID selectors
                        if (!element && xpath.includes('[@id=')) {
                            try {
                                var idMatch = xpath.match(/\[@id=['\""]]?([^'\""\\]]+)['\""]]?\]/);
                                if (idMatch && idMatch[1]) {
                                    element = doc.querySelector('#' + idMatch[1]);
                                }
                            } catch(e) {}
                        }

                        if (element) {
                            var text = element.textContent || element.innerText || element.innerHTML || element.value || '';
                            return text;
                        }
                        return null;
                    }

                    // Search in main document
                    var text = findElementInDocument(document, xpath);
                    if (text) return text;

                    // Search in all iframes
                    var iframes = document.getElementsByTagName('iframe');
                    for (var i = 0; i < iframes.length; i++) {
                        try {
                            var iframeDoc = iframes[i].contentDocument || iframes[i].contentWindow.document;
                            text = findElementInDocument(iframeDoc, xpath);
                            if (text) return text;
                        } catch(e) {
                            // Cross-origin iframe, skip
                        }
                    }

                    return null;
                ";

                object result = jsExecutor.ExecuteScript(script, xPath);

                if (result != null)
                {
                    string text = result.ToString().Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        Logger.Debug($"JavaScript search succeeded, extracted {text.Length} characters");
                        return text;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Debug($"JavaScript search exception: {ex.Message}");
            }

            return null;
        }

        [KeywordDescription("Get Plain Text of given xpath and store to given variable.This will not add any escape sequences to the value")]
        [KeywordDisplayName("Get Plain Text")]
        [KeywordParameters("xPath", "xPath of object to compare")]
        [KeywordParameters("saveTo", "variable name (ex. ${Buffer}) to store value")]
        [SampleScript("Web.Get Plain Text(//*[@id=DeviceModemSettingsPhoneNumber],${ex})")]
        public KeywordResult GetPlainText(string xPath, string saveTo)
        {
            xPath = Support.Utils.GetVariablevalueIfExist(xPath);
            IWebElement element = element = FindElement(GetSelectorByText(xPath), TimeSpan.FromSeconds(_timeOutInSec));
            if (element == null)
            {
                KeywordResult kr = new KeywordResult(KeywordResults.Fail);
                kr.Output = $"Can not find element with xpath : {xPath}";
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = GetScreenShot();
                }
                Logger.Debug(GetPageSource());
                return kr;
            }

            try
            {
                string webText = element.Text;
                if (string.IsNullOrEmpty(webText))
                {
                    webText = element.GetAttribute("value");

                }
                if (string.IsNullOrEmpty(webText))
                {
                    KeywordResult kr = new KeywordResult(KeywordResults.Fail);
                    kr.Output = $"Can not get plain text of element with xpath : {xPath}";
                    if (!_disableScreenShotForFails)
                    {
                        kr.ScreenShot = GetScreenShot();
                    }
                    Logger.Debug(GetPageSource());
                    return kr;
                }
                else
                {
                    CommonExecutionInfo.SetVariable(saveTo, webText);
                    KeywordResult kr = new KeywordResult(KeywordResults.Pass);
                    kr.Output = $"{saveTo} : {webText}";
                    return kr;
                }

            }
            catch (Exception ex)
            {
                KeywordResult kr = new KeywordResult(KeywordResults.Error);
                Logger.Error("Error during Check Text", ex);
                kr.Output = $"Error during clearing or setting field";
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = GetScreenShot();
                }
                kr.AdditionalInfo = ex.ToString();
                Logger.Debug(GetPageSource());
                return kr;
            }
        }

        [KeywordDescription("Get splitted text of given xpath and store to given variable")]
        [KeywordDisplayName("Get splitted Text")]
        [KeywordParameters("xPath", "xPath of object to compare")]
        [KeywordParameters("splitWith", "split the text with")]
        [KeywordParameters("index", "index position of the statement")]
        [KeywordParameters("saveTo", "variable name (ex. ${Buffer}) to store value")]
        [SampleScript("Web.Get splitted Text (//*[@id=\"mat - dialog - 0\"]/fuse-confirmation-dialog/div/div[1]/div/div[2]/div,:,1,${Releasecode})")]
        public KeywordResult GetSplittedText(string xPath, string splitWith, string index, string saveTo)
        {
            xPath = Support.Utils.GetVariablevalueIfExist(xPath);
            IWebElement element = element = FindElement(GetSelectorByText(xPath), TimeSpan.FromSeconds(_timeOutInSec));
            if (element == null)
            {
                KeywordResult kr = new KeywordResult(KeywordResults.Fail);
                kr.Output = $"Can not find element with xpath : {xPath}";
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = GetScreenShot();
                }
                Logger.Debug(GetPageSource());
                return kr;
            }

            try
            {
                string webText = element.Text;
                if (string.IsNullOrEmpty(webText))
                {
                    webText = element.GetAttribute("value");

                }
                if (string.IsNullOrEmpty(webText))
                {
                    KeywordResult kr = new KeywordResult(KeywordResults.Fail);
                    kr.Output = $"Can not get text of element with xpath : {xPath}";
                    if (!_disableScreenShotForFails)
                    {
                        kr.ScreenShot = GetScreenShot();
                    }
                    Logger.Debug(GetPageSource());
                    return kr;
                }
                else
                {
                    // Format it If it has comma in its value SWQATR-593
                    if (webText.Contains(","))
                    {
                        webText = webText.Replace(",", @"\,");
                        webText = webText.Replace(@"\\", @"\");
                    }

                    webText = webText.Split(Convert.ToChar(splitWith))[Convert.ToInt32(index)];
                    CommonExecutionInfo.SetVariable(saveTo, webText);
                    KeywordResult kr = new KeywordResult(KeywordResults.Pass);
                    kr.Output = $"{saveTo} : {webText}";
                    return kr;
                }

            }
            catch (Exception ex)
            {
                KeywordResult kr = new KeywordResult(KeywordResults.Error);
                Logger.Error("Error during Check Text", ex);
                kr.Output = $"Error during clearing or setting field";
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = GetScreenShot();
                }
                kr.AdditionalInfo = ex.ToString();
                Logger.Debug(GetPageSource());
                return kr;
            }
        }



        [KeywordDescription("Capture screen shot of web page")]
        [KeywordDisplayName("Capture Screenshot")]
        [KeywordParameters("screenshotName", "Name of screenshot")]
        [SampleScript("Web.Capture Screenshot (sample.jpg)")]
        public KeywordResult CaptureScreenShot(string screenShotName)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                result.ScreenShot = GetScreenShot();

                if (result.ScreenShot != null)
                {
                    if (!string.IsNullOrEmpty(screenShotName))
                    {
                        string saveTo = Path.Combine(_outputDir, screenShotName);
                        File.WriteAllBytes(saveTo, result.ScreenShot);
                        if (File.Exists(saveTo))
                        {
                            return result;
                        }
                        else
                        {
                            result.Result = KeywordResults.Fail;
                            result.Output = "Fail to save screen shot image";
                            return result;
                        }
                    }
                    return result;
                }
                else
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = "Fail to capture screen";
                    return result;
                }
                
            }
            catch(Exception ex)
            {
                Logger.Error("Screenshot capture error", ex);
                result.Result = KeywordResults.Error;
                result.Output = $"Can not capture screenshot";
                return result;
            }
            
        }

        [KeywordDescription("Refresh the page")]
        [KeywordDisplayName("Refresh")]
        [SampleScript("Web.Refresh")]
        public KeywordResult Refresh()
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                _driver.Navigate().Refresh();
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error("Error during refreshing", ex);
                result.Result = KeywordResults.Fail;
                result.Output = $"Error during refreshing";
                if (!_disableScreenShotForFails)
                {
                    result.ScreenShot = GetScreenShot();
                }
                return result;
            }

        }
        [KeywordDescription("Execute given javascript")]
        [KeywordDisplayName("Execute Java Script")]
        [KeywordParameters("script", "script to execute")]
        [KeywordParameters("targetXpath", "xpath of target element which will be the argument of script")]
        [SampleScript("Web.Execute Java Script (arguments[0].click();,//*[@id='content']/div[2]/form/div[3]/button)")]
        public KeywordResult ExecuteJavascript(string script, string targetXpath)
        {
            try
            {
                IJavaScriptExecutor jsExecutor = _driver as IJavaScriptExecutor;
                IWebElement element = _driver.FindElement(By.XPath(targetXpath));
                object o = jsExecutor.ExecuteScript(script, element);
                KeywordResult result = new KeywordResult(KeywordResults.Pass, o.ToString());
                return result;
            }
            catch(Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Unexpected error during execute javascript");
                error.AdditionalInfo = ex.ToString();
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        [KeywordDescription("Scroll down to end of selected object by sending Ctrl + End Key")]
        [KeywordDisplayName("Scroll Down To The End")]
        [KeywordParameters("xPath", "xPath of object")]
        [SampleScript("Web.Scroll Down To The End (/html)")]
        public KeywordResult ScrollDownToTheEnd(string xPath)
        {   
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            
            try
            {
                Actions actions = new Actions(_driver);
                actions.SendKeys(FindElement(GetSelectorByText(xPath), TimeSpan.FromSeconds(3)), Keys.Control + Keys.End).Build().Perform();
            }
            catch(Exception ex)
            {
                Logger.Error("Error while sending ctrl + end", ex);
                result.Result = KeywordResults.Error;
                if (!_disableScreenShotForFails)
                {
                    result.ScreenShot = GetScreenShot();
                }
                return result;
            }
            return new KeywordResult(KeywordResults.Pass);
            
        }

        [KeywordDescription("Press Home Key from selected object")]
        [KeywordDisplayName("Press Home Key")]
        [KeywordParameters("xPath", "xPath of object")]
        [SampleScript("Web.Press Home Key (/html)")]
        public KeywordResult PressHomeKey(string xPath)
        {            
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            
            try
            {
                Actions actions = new Actions(_driver);
                actions.SendKeys(FindElement(GetSelectorByText(xPath), TimeSpan.FromSeconds(3)), Keys.Home).Build().Perform();
            }
            catch (Exception ex)
            {
                Logger.Error("Error while press home key", ex);
                result.Result = KeywordResults.Error;
                if (!_disableScreenShotForFails)
                {
                    result.ScreenShot = GetScreenShot();
                }
                return result;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Press Page Up Key from selected object")]
        [KeywordDisplayName("Press PageUp Key")]
        [KeywordParameters("xPath", "xPath of object")]
        [SampleScript("Web.Press PageUp Key (/html)")]
        public KeywordResult PressPageUpKey(string xPath)
        {            
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            
            try
            {
                Actions actions = new Actions(_driver);
                actions.SendKeys(FindElement(GetSelectorByText(xPath), TimeSpan.FromSeconds(3)), Keys.PageUp).Build().Perform();
            }
            catch (Exception ex)
            {
                Logger.Error("Error while press page up key", ex);
                result.Result = KeywordResults.Error;
                if (!_disableScreenShotForFails)
                {
                    result.ScreenShot = GetScreenShot();
                }
                return result;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Press Page Down Key on the keyboard")]
        [KeywordDisplayName("Press PageDown Key")]
        [KeywordParameters("xPath", "xPath of object")]
        [SampleScript("Web.Press PageDown Key (/html)")]
        public KeywordResult PressPageDownKey(string xPath)
        {            
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            
            try
            {
                Actions actions = new Actions(_driver);
                actions.SendKeys(FindElement(GetSelectorByText(xPath), TimeSpan.FromSeconds(3)), Keys.PageDown).Build().Perform();
            }
            catch (Exception ex)
            {
                Logger.Error("Error while press page down key", ex);
                result.Result = KeywordResults.Error;
                if (!_disableScreenShotForFails)
                {
                    result.ScreenShot = GetScreenShot();
                }
                return result;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Click object with given xpath")]
        [KeywordDisplayName("Click Non button Object")]
        [KeywordParameters("xPath", "xPath of object to click")]
        [SampleScript("Web.Click Non button Object (/html/body/div[1]/div[2])")]
        public KeywordResult ClickNonButtonObject(string xPath)
        {
            try
            {
                Actions actions = new Actions(_driver);
                IWebElement obj = FindHiddenElement(GetSelectorByText(xPath), TimeSpan.FromSeconds(_timeOutInSec));

                if (obj == null)
                {
                    KeywordResult result = new KeywordResult(KeywordResults.Fail);
                    result.AdditionalInfo = "Can not find element";
                    result.Output = "Fail to Click NonButtonObject xpath";
                    if (!_disableScreenShotForFails)
                    {
                        result.ScreenShot = GetScreenShot();
                    }
                    Logger.Debug(GetPageSource());
                    return result;
                }

                actions.MoveToElement(obj).Click().Perform();
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                KeywordResult result = new KeywordResult(KeywordResults.Error);
                result.AdditionalInfo = ex.ToString();
                result.Output = "Fail to Click NonButtonObject xpath";
                if (!_disableScreenShotForFails)
                {
                    result.ScreenShot = GetScreenShot();
                }
                Logger.Error("Error during ClickNonButtonObject", ex);
                Logger.Debug(GetPageSource());
                return result;
            }
        }
        [KeywordDescription("Scroll Down the Page To Particular Xpath")]
        [KeywordDisplayName("ScrollDownToParicularObject")]
        [KeywordParameters("xPath", "xPath of object")]
        public KeywordResult ScrollDownToParicularObject(string xPath)
        {            
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            
            try
            {
                Actions actions = new Actions(_driver);
                actions.SendKeys(FindElement(GetSelectorByText(xPath), TimeSpan.FromSeconds(3)), Keys.ArrowDown).Build().Perform();
            }
            catch (Exception ex)
            {
                Logger.Error("Error while Scroll Down the Page To Particular Xpath", ex);
                result.Result = KeywordResults.Error;
                if (!_disableScreenShotForFails)
                {
                    result.ScreenShot = GetScreenShot();
                }
                return result;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Check the screen contains given partial text")]

        [KeywordDisplayName("Check Screen Contains Partial Text")]

        [KeywordParameters("text", "Partail Text to check screen contains given text")]

        [SampleScript("Web.Check Screen Contains Partial Text (Service busy.)")]

        public KeywordResult CheckScreenContainsPartialText(string text)

        {

            KeywordResult result = new KeywordResult(KeywordResults.Pass);

            try

            {

                // XPath to search for partial text match

                string xPath = $"//*[contains(.,'{text}')]";

                // Try to find the element immediately, without waiting

                IWebElement element = FindElement(GetSelectorByText(xPath), TimeSpan.Zero);

                if (element == null)

                {

                    result.Result = KeywordResults.Fail;

                    result.Output = $"Partial text '{text}' not found on screen.";

                    if (!_disableScreenShotForFails)
                    {
                        result.ScreenShot = GetScreenShot();
                    };

                    Logger.Debug(GetPageSource());

                }

                else

                {

                    result.Output = $"Partial text '{text}' found on screen.";

                }

            }

            catch (Exception ex)

            {

                result.Result = KeywordResults.Error;

                result.AdditionalInfo = ex.ToString();

                result.Output = "Exception occurred while checking for partial text.";

                if (!_disableScreenShotForFails)
                {
                    result.ScreenShot = GetScreenShot();
                }

                Logger.Error("Error during partial text check", ex);

                Logger.Debug(GetPageSource());

            }

            return result;

        }

        [KeywordDescription("Click popup option by visible text")]
        [KeywordDisplayName("Click Popup Option By Text")]
        [KeywordParameters("text", "Visible text of the popup option to click")]
        [SampleScript("Web.Click Popup Option By Text (vvvv)")]
        public KeywordResult ClickPopupOptionByText(string text)
        {
            try
            {
                // Target only the popup container
                string xPath = $"//div[contains(@class,'vn-modal--dialog')]//*[normalize-space(text())='{text}']";

                IWebElement obj = FindElement(By.XPath(xPath), TimeSpan.FromSeconds(_timeOutInSec));

                if (obj == null)
                {
                    return new KeywordResult(KeywordResults.Fail)
                    {
                        AdditionalInfo = $"Cannot find popup option '{text}' in modal",
                        Output = "Fail to click popup option",
                        ScreenShot = GetScreenShot()
                    };
                }

                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", obj);
                Thread.Sleep(200);

                try
                {
                    obj.Click();
                }
                catch (ElementClickInterceptedException)
                {
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", obj);
                }

                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                return new KeywordResult(KeywordResults.Error)
                {
                    AdditionalInfo = ex.ToString(),
                    Output = "Error while clicking popup option",
                    ScreenShot = GetScreenShot()
                };
            }
        }


        /// <summary>
        /// This method is introduced to check non button (fileupload) where display is false
        /// </summary>
        /// <param name="by"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        private IWebElement FindHiddenElement(By by, TimeSpan timeOut)
        {
            double totalWaitTime = timeOut.TotalMilliseconds;

            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddMilliseconds(totalWaitTime);

            IWebElement ret = null;

            while (DateTime.Now <= endTime)
            {
                try
                {
                    ret = _driver.FindElement(by);
                    if (ret.Enabled)
                    {
                        return ret;
                    }

                    else
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(500));
                    }
                }
                catch (Exception ex)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(500));
                    Logger.Debug(ex.ToString());
                }
            }
            return ret;
        }

        public static string GetBrowserURL(Process process)
        {
            if (process == null)
                throw new ArgumentNullException("process");

            if (process.MainWindowHandle == IntPtr.Zero)
                return null;

            AutomationElement element = AutomationElement.FromHandle(process.MainWindowHandle);
            if (element == null)
                return null;
            var conditions = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit);
            AutomationElement edit = element.FindFirst(TreeScope.Subtree, conditions);

            return ((ValuePattern)edit.GetCurrentPattern(ValuePattern.Pattern)).Current.Value as string;
        }

        [KeywordDescription("Disables automatic screenshot capture for failed scenarios. \r\n you can still manually capture screenshots using the Dune.Capture Screen Shot keyword as needed ")]
        [KeywordDisplayName("Disable ScreenShot For Fails")]
        [SampleScript("Web.Disable ScreenShot For Fails")]
        public KeywordResult DisableScreenShotForFails()
        {
            try
            {
                _disableScreenShotForFails = true;
                return new KeywordResult(KeywordResults.Pass, "Screenshot will be disabled for Fail scenarios");
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Error during DisableScreenShotForFails");
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        [KeywordDescription("Enables automatic screenshot capture for failed scenarios.")]
        [KeywordDisplayName("Enable ScreenShot For Fails")]
        [SampleScript("Web.Enable ScreenShot For Fails")]
        public KeywordResult EnableScreenShotForFails()
        {
            try
            {
                _disableScreenShotForFails = false;
                return new KeywordResult(KeywordResults.Pass, "Screenshot will be enabled for Fail scenarios");
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Error during EnableScreenShotForFails");
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        [GetKeyword]
        [KeywordDescription("Gets the row count from table body.")]
        [KeywordDisplayName("Get Table Row Count")]
        [KeywordParameters("tableXPath", "XPath with any attribute of the table element which uniquely identifies the table in the page")]
        [KeywordParameters("saveTo", "variable name (ex. ${RowCount}) to store row count")]
        [SampleScript("Web.Get Table Row Count (//table[@class='eop-my-documents-table'],${RowCount})")]
        public KeywordResult GetTableRowCount(string tableXPath, string saveTo)
        {
            try
            {
                IWebElement tableElement = FindElement(GetSelectorByText(tableXPath), TimeSpan.FromSeconds(_timeOutInSec));
                if (tableElement == null)
                {
                    KeywordResult kr = new KeywordResult(KeywordResults.Fail);
                    kr.Output = $"Can not find table element with xpath : {tableXPath}";
                    if (!_disableScreenShotForFails)
                    {
                        kr.ScreenShot = GetScreenShot();
                    }
                    Logger.Debug(GetPageSource());
                    return kr;
                }

                // Find tbody element within the table
                IWebElement tbody = null;
                try
                {
                    tbody = tableElement.FindElement(By.TagName("tbody"));
                }
                catch (NoSuchElementException)
                {
                    KeywordResult kr = new KeywordResult(KeywordResults.Fail);
                    kr.Output = $"Can not find tbody element inside table with xpath : {tableXPath}";
                    if (!_disableScreenShotForFails)
                    {
                        kr.ScreenShot = GetScreenShot();
                    }
                    Logger.Debug(GetPageSource());
                    return kr;
                }

                // Get all rows from tbody
                var allRows = tbody.FindElements(By.TagName("tr"));

                // Filter out hidden rows
                int visibleRowCount = 0;
                foreach (var row in allRows)
                {
                    // Check if row has hidden attribute
                    string hiddenAttr = row.GetAttribute("hidden");

                    // Check if row has display: none style
                    string displayStyle = row.GetCssValue("display");

                    // Count only if row is not hidden
                    if (string.IsNullOrEmpty(hiddenAttr) && displayStyle != "none")
                    {
                        visibleRowCount++;
                    }
                }

                CommonExecutionInfo.SetVariable(saveTo, visibleRowCount.ToString());
                KeywordResult result = new KeywordResult(KeywordResults.Pass);
                result.Output = $"{saveTo} : {visibleRowCount}";
                return result;
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Error during GetTableRowCount");
                error.AdditionalInfo = ex.ToString();
                if (!_disableScreenShotForFails)
                {
                    error.ScreenShot = GetScreenShot();
                }
                Logger.Error("Error while getting table row count", ex);
                Logger.Debug(GetPageSource());
                return error;
            }
        }
        [KeywordDescription("Drag and drop file to web page")]
        [KeywordDisplayName("Drag File To Web")]
        [KeywordParameters("filePath", "Full file path")]
        [KeywordParameters("targetXpath", "Drop area xpath in web")]
        [SampleScript("Web.Drag File To Web(C:\\Temp\\sample.pdf,//*[@id='dropzone'])")]
        public KeywordResult DragFileToWeb(string filePath, string targetXpath)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return new KeywordResult(KeywordResults.Fail, "File path should not be empty");
            }

            if (string.IsNullOrWhiteSpace(targetXpath))
            {
                return new KeywordResult(KeywordResults.Fail, "Target xpath should not be empty");
            }

            if (!File.Exists(filePath))
            {
                return new KeywordResult(KeywordResults.Fail, $"File does not exist : {filePath}");
            }

            try
            {
                IWebElement targetElement = FindHiddenElement(
                    GetSelectorByText(targetXpath),
                    TimeSpan.FromSeconds(_timeOutInSec));

                if (targetElement == null)
                {
                    return new KeywordResult(KeywordResults.Fail, "Target element not found");
                }

                byte[] fileBytes = File.ReadAllBytes(filePath);
                string base64Data = Convert.ToBase64String(fileBytes);

                string fileName = Path.GetFileName(filePath);
                string mimeType = MimeMapping.GetMimeMapping(fileName);

                string script = @"
            var target = arguments[0];
            var base64Data = arguments[1];
            var fileName = arguments[2];
            var mimeType = arguments[3];

            function base64ToUint8Array(base64)
            {
                var binaryString = atob(base64);
                var bytes = new Uint8Array(binaryString.length);

                for (var i = 0; i < binaryString.length; i++)
                {
                    bytes[i] = binaryString.charCodeAt(i);
                }

                return bytes;
            }

            var fileBytes = base64ToUint8Array(base64Data);

            var file = new File(
                [fileBytes],
                fileName,
                {
                    type: mimeType,
                    lastModified: Date.now()
                }
            );

            var dataTransfer = new DataTransfer();
            dataTransfer.items.add(file);

            function fireDragEvent(eventName)
            {
                var evt = new DragEvent(eventName,
                {
                    bubbles: true,
                    cancelable: true,
                    dataTransfer: dataTransfer
                });

                return target.dispatchEvent(evt);
            }

            fireDragEvent('dragenter');
            fireDragEvent('dragover');
            fireDragEvent('drop');
            fireDragEvent('dragleave');

            return true;
        ";

                IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;

                bool result = Convert.ToBoolean(js.ExecuteScript(
                    script,
                    targetElement,
                    base64Data,
                    fileName,
                    mimeType));

                return result
                    ? new KeywordResult(
                        KeywordResults.Pass,
                        $"File '{fileName}' dragged successfully")
                    : new KeywordResult(
                        KeywordResults.Fail,
                        $"Failed to drag file '{fileName}'");

            }
            catch (Exception ex)
            {
                KeywordResult kr = new KeywordResult(KeywordResults.Error);
                kr.Output = $"Failed to drag file '{Path.GetFileName(filePath)}'.";
                kr.AdditionalInfo = ex.ToString();
                return kr;
            }
        }

        [KeywordDescription("Wait until new file download is completed using partial file name")]
        [KeywordDisplayName("Wait Until File Download Complete")]
        [KeywordParameters("downloadFolderPath", "Folder path where file gets downloaded")]
        [KeywordParameters("partialFileName", "Partial downloaded file name")]
        [KeywordParameters("timeoutInSeconds", "Maximum wait time in seconds")]
        [SampleScript("Web.Wait Until File Download Complete(C:\\Downloads,Report,60)")]
        public KeywordResult WaitUntilFileDownloadComplete(string downloadFolderPath, string partialFileName, string timeoutInSeconds)
        {
            try
            {
                if (!int.TryParse(timeoutInSeconds, out int timeout) || timeout <= 0)
                {
                    return new KeywordResult(KeywordResults.Fail, $"Invalid timeout value: '{timeoutInSeconds}'. Must be a positive integer.");
                }

                downloadFolderPath = Support.Utils.GetAbsolutePath(downloadFolderPath, CommonExecutionInfo.ScriptFolder);

                if (!Directory.Exists(downloadFolderPath))
                {
                    return new KeywordResult(
                        KeywordResults.Fail,
                        $"Download folder does not exist : {downloadFolderPath}");
                }

                if (string.IsNullOrWhiteSpace(partialFileName))
                {
                    return new KeywordResult(
                        KeywordResults.Fail,
                        "Partial file name cannot be empty");
                }

                DateTime startTime = DateTime.Now;

                // Store existing matching files before download
                var existingFiles = Directory.GetFiles(downloadFolderPath)
                    .Where(file =>
                        IsMatchingFile(
                            Path.GetFileName(file),
                            partialFileName))
                    .ToList();

                DateTime endTime = startTime.AddSeconds(timeout);

                while (DateTime.Now < endTime)
                {
                    string[] files = Directory.GetFiles(downloadFolderPath);

                    // Find newly downloaded or modified file
                    var downloadedFile = files.FirstOrDefault(file =>
                    {
                        string currentFileName = Path.GetFileName(file);

                        if (!IsMatchingFile(currentFileName, partialFileName) || IsTempFile(currentFileName))
                        {
                            return false;
                        }

                        FileInfo fileInfo = new FileInfo(file);

                        // NEW FILE
                        bool isNewFile = !existingFiles.Contains(file);

                        // EXISTING FILE UPDATED
                        bool isRecentlyModified =
                            fileInfo.LastWriteTime > startTime;

                        return isNewFile || isRecentlyModified;
                    });

                    // Ensure file is not locked before reporting success
                    if (!string.IsNullOrEmpty(downloadedFile) && IsFileAccessible(downloadedFile))
                    {
                        return new KeywordResult(
                            KeywordResults.Pass,
                            $"File download completed successfully : " +
                            $"{Path.GetFileName(downloadedFile)}");
                    }

                    Thread.Sleep(PollingIntervalMilliseconds);
                }

                // Check incomplete download files
                string[] timeoutFiles = Directory.GetFiles(downloadFolderPath);

                var incompleteFile = timeoutFiles.FirstOrDefault(file =>
                {
                    string currentFileName = Path.GetFileName(file);

                    return IsMatchingFile(
                       currentFileName,
                       partialFileName)
                   && IsTempFile(currentFileName);
                });

                if (!string.IsNullOrEmpty(incompleteFile))
                {
                    return new KeywordResult(
                        KeywordResults.Fail,
                        $"File download incomplete : " +
                        $"{Path.GetFileName(incompleteFile)}");
                }

                return new KeywordResult(
                    KeywordResults.Fail,
                    $"No new file download detected for : {partialFileName}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error(
                    $"Access denied to folder : {downloadFolderPath}",
                    ex);

                return new KeywordResult(
                    KeywordResults.Error,
                    $"Access denied to folder : {downloadFolderPath}");
            }
            catch (FormatException ex)
            {
                Logger.Error(
                    $"Invalid timeout value : {timeoutInSeconds}",
                    ex);

                return new KeywordResult(
                    KeywordResults.Fail,
                    $"Invalid timeout value : {timeoutInSeconds}");
            }
            catch (Exception ex)
            {
                Logger.Error(
                    "Error while validating file download completion",
                    ex);

                KeywordResult result =
                    new KeywordResult(KeywordResults.Error);

                result.Output =
                    "Error while validating file download completion";

                result.AdditionalInfo = ex.ToString();

                return result;
            }
        }

        private bool IsMatchingFile(string fileName, string partialFileName)
        {
            return fileName.IndexOf(
                       partialFileName,
                       StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool IsTempFile(string fileName)
        {
            return fileName.EndsWith(
                       ".crdownload",
                       StringComparison.OrdinalIgnoreCase)
                   || fileName.EndsWith(
                       ".part",
                       StringComparison.OrdinalIgnoreCase)
                   || fileName.EndsWith(
                       ".tmp",
                       StringComparison.OrdinalIgnoreCase);
        }

        private bool IsFileAccessible(string filePath)
        {
            try
            {
                using (FileStream stream = File.Open(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.None))
                {
                    return true;
                }
            }
            catch
            {
                return false;

            }
        }

    }
}




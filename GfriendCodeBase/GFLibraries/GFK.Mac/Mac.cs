using HP.GFriend.GFLogger;
using HP.GFriend.Keywords;
using HP.GFriend.Utils.Appium;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Extension;
using OpenQA.Selenium.Appium.Interfaces;
using OpenQA.Selenium.Appium.Mac;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;

namespace HP.GFriend.Keywords
{
    public class Mac : IGFLibrary
    {
        private MacDriver _driver;
        private DeviceUnderTest _dut;
        private AppiumOptions _capabilites;

        public string AppBundleId;
        private string _outputDir;
        private Ssh _ssh;
        private int _scaleFactor = 1;
        public Vision _vision;
        private string _country = string.Empty;
        public void Dispose()
        {
            if (_driver != null)
            {
                _driver.Quit();
                _driver.Dispose();
                _driver = null;
            }
        }

        public bool DutUsed()
        {
            return true;
        }

        public List<string> GetDependencies()
        {
            return new List<string>() { "Ssh" };
        }

        public string GetName()
        {
            return "Mac";
        }

        private byte[] GetScreenCapture()
        {
            try
            {
                return _driver.GetScreenshot().AsByteArray;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private KeywordResult GetFailErrorResult(string message, KeywordResults result, bool screenCapture = true, Exception ex = null)
        {
            KeywordResult keywordResult = new KeywordResult(result);
            keywordResult.Output = message;
            if (ex != null)
            {
                keywordResult.AdditionalInfo = ex.ToString();
                Logger.Error(message, ex);
            }
            else
            {
                Logger.Error(message);
            }

            if (screenCapture)
            {
                byte[] screenShot = GetScreenCapture();
                if (screenShot != null)
                {
                    keywordResult.ScreenShot = screenShot;
                }
            }

            return keywordResult;
        }

        private bool CheckElementNotExist(Func<By, IWebElement> FindElement, string selector, TimeSpan timeOut)
        {
            IWebElement target = null;
            DateTime endTime = DateTime.Now.AddMilliseconds(timeOut.TotalMilliseconds * _scaleFactor);

            while (true)
            {
                try
                {
                    target = FindElement(By.XPath(selector));

                    if (target != null)
                    {
                        Thread.Sleep(500);
                        if (DateTime.Now > endTime)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                catch (Exception)
                {
                    return true;
                }
            }
        }
        // Replace MacElement with IWebElement in the method signature and return type
        private IWebElement FindElementWithTimeout(Func<string, IWebElement> FindElement, string selector, TimeSpan timeOut)
        {
            IWebElement target = null;
            DateTime endTime = DateTime.Now.AddMilliseconds(timeOut.TotalMilliseconds * _scaleFactor);

            while (true)
            {
                try
                {
                    target = FindElement(selector);
                    if (target != null)
                    {
                        break;
                    }
                    Thread.Sleep(500);
                    if (DateTime.Now > endTime)
                    {
                        return null;
                    }
                }
                catch (WebDriverException ex)
                {
                    Thread.Sleep(500);
                    if (DateTime.Now > endTime)
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Thread.Sleep(500);
                    if (DateTime.Now > endTime)
                    {
                        return null;
                    }
                }
            }

            return target;
        }
        public void Initialize(DeviceUnderTest dut, string outputDir)
        {
            _outputDir = outputDir;
            _dut = dut;

            _vision = new Vision();
            _vision.Initialize(_dut, _outputDir);

            if (string.IsNullOrEmpty(_dut.AdminId) || string.IsNullOrEmpty(_dut.AdminPassword) || _dut.Port == 0)
            {
                throw new LibraryInitializeException("Device Address, Admin Id, Admin Password and Port should be given.");
            }
            _capabilites = new AppiumOptions();
            _capabilites.PlatformName = "mac";
            _capabilites.AutomationName = "mac2";
            _capabilites.AddAdditionalAppiumOption("newCommandTimeout", 1200);
            _capabilites.AddAdditionalAppiumOption("noReset", true);

            AppiumServerUtils.Initialize(_dut.DeviceAddress, 22, _dut.AdminId, _dut.AdminPassword);
            _ssh = new Ssh();
            _ssh.Connect(_dut.DeviceAddress, "22", _dut.AdminId, _dut.AdminPassword);
            if (!AppiumServerUtils.ExecuteAppiumServer())
            {
                throw new LibraryInitializeException("Can not start Appium server.");
            }
        }

        [KeywordDescription("Set timeout scacle for applying all keywords in library." +
            "This scale facor will be multiplied of all timeout arguments in keywords." +
            "For example, if scale factor is set to 2 and call wait for object for 3 secondes, it will wait for 6 (3 x 2) seconds.")]
        [KeywordDisplayName("Set Timeout Scale")]
        [KeywordParameters("scaleFactor", "scale factor. Should be a number and larger than 0")]
        [SampleScript("Mac.Set Timeout Scale (10)")]
        public KeywordResult SetTimeoutScale(string scaleFactor)
        {
            if (!int.TryParse(scaleFactor.Trim(), out _scaleFactor) || _scaleFactor < 1)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Scale factor must be a number and should be larger than 0";
                return error;
            }
            KeywordResult pass = new KeywordResult(KeywordResults.Pass);
            pass.Output = $"Scale factor is set to {_scaleFactor}";
            return pass;
        }

        [KeywordDescription("Run command on Mac PC via ssh connection")]
        [KeywordDisplayName("Run Command")]
        [KeywordParameters("command", "command to execute")]
        [SampleScript("Run Command (cmd.exe /c start hp.smart.exe,${temp},10)")]
        public KeywordResult RunCommand(string command)
        {
            return _ssh.Send(command);

        }

        [KeywordDescription("Launch App (or select app if app is already launched) with given bundle id of Mac Application")]
        [KeywordDisplayName("Launch App")]
        [KeywordParameters("App bundle ID", "Bundle ID of app to launch")]
        [SampleScript("Mac.LaunchApp(com.apple.Safari)")]
        public KeywordResult LaunchApp(string appBundleId)
        {
            AppBundleId = appBundleId;
            _capabilites.AddAdditionalAppiumOption("bundleId", AppBundleId);
            if (_driver != null)
            {
                _driver.Quit();
                _driver.Dispose();
                _driver = null;
            }
            try
            {
                _driver = new MacDriver(new Uri($"http://{_dut.DeviceAddress}:{_dut.Port}/wd/hub"), _capabilites);

                CommonExecutionInfo.SetSharedObject("Mac:" + _dut.DeviceId, _driver);
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Error during connect to Appium server.", KeywordResults.Error, false, ex);
            }

            return new KeywordResult(KeywordResults.Pass);
        }


        [KeywordDescription("Click Element with XPath")]
        [KeywordDisplayName("Click")]
        [KeywordParameters("xPath", "XPath of element to click")]
        [SampleScript("Mac.Click(//XCUIElementTypeButton[@identifier='_XCUI:FullScreenWindow'])")]
        public KeywordResult Click(string xPath)
        {
            return Click(xPath, "3");
        }

        [KeywordDescription("Click Element with XPath")]
        [KeywordDisplayName("Click")]
        [KeywordParameters("xPath", "XPath of element to click")]
        [KeywordParameters("finding timeout", "Timeout of finding element in seconds")]
        [SampleScript("Mac.Click(//XCUIElementTypeButton[@identifier='_XCUI:FullScreenWindow'],10)")]
        public KeywordResult Click(string xPath, string findingTimeout)
        {
            if (!int.TryParse(findingTimeout.Trim(), out int timeOut))
            {
                return GetFailErrorResult("Timeout must be a number", KeywordResults.Error, false);
            }

            IWebElement element = FindElementWithTimeout(selector => _driver.FindElement(By.XPath(selector)), xPath, TimeSpan.FromSeconds(timeOut));

            if (element == null)
            {
                return GetFailErrorResult($"Can not find element with given xPath : {xPath}", KeywordResults.Fail);
            }
            try
            {
                if (AppBundleId == "com.google.Chrome" || AppBundleId == "com.apple.Safari" ||AppBundleId== "com.microsoft.edgemac" ||AppBundleId== "org.mozilla.firefox")
                {
                    // For Chrome and Safari, we can use the click method directly
                    element.Click();
                }
                else
                {
                    // For other apps, we need to use Actions to perform the click
                    Actions actions = new Actions(_driver);
                    actions.MoveToElement(element).Click().Perform();
                }

            }
            catch (Exception ex)
            {
                return GetFailErrorResult($"Can not find element with given xPath : {ex}", KeywordResults.Error);
            }

            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Click Element with name property")]
        [KeywordDisplayName("Click With Name")]
        [KeywordParameters("name", "name property of element to click")]
        [SampleScript("Mac.Click With Name (Print Documents)")]
        public KeywordResult ClickWithName(string name)
        {
            return Click($"//*[@name='{name}']", "3");
        }

        [KeywordDescription("Click Element with name property")]
        [KeywordDisplayName("Click With Name")]
        [KeywordParameters("name", "name property of element to click")]
        [KeywordParameters("finding timeout", "Timeout of finding element in seconds")]
        [SampleScript("Mac.Click With Name (Get Supplies,10)")]
        public KeywordResult ClickWithName(string name, string timeOut)
        {
            return Click($"//*[@name='{name}']", timeOut);
        }

        [KeywordDescription("Click Element with label property")]
        [KeywordDisplayName("Click With Label")]
        [KeywordParameters("label", "label property of element to click")]
        [SampleScript("Mac.Click With Label(Skip for now)")]
        public KeywordResult ClickWithLabel(string label)
        {
            return Click($"//*[@label='{label}']", "3");
        }

        [KeywordDescription("Click Element with label property")]
        [KeywordDisplayName("Click With Label")]
        [KeywordParameters("label", "label property of element to click")]
        [KeywordParameters("finding timeout", "Timeout of finding element in seconds")]
        [SampleScript("Mac.Click With Label(Do more with HP Smart,10)")]
        public KeywordResult ClickWithLabel(string label, string timeOut)
        {
            return Click($"//*[@label='{label}']", timeOut);
        }


        [KeywordDescription("Click bottom right position of element")]
        [KeywordDisplayName("Click Bottom Right")]
        [KeywordParameters("xPath", "XPath of element")]
        [SampleScript("Mac.Click Bottom Right (//XCUIElementTypeStaticText[@title='United States'])")]
        public KeywordResult ClickBottomRight(string xPath)
        {
            IWebElement element = FindElementWithTimeout(selector => _driver.FindElement(By.XPath(selector)), xPath, TimeSpan.FromSeconds(3));
            if (element == null)
            {
                return GetFailErrorResult($"Can not find element with given xPath : {xPath}", KeywordResults.Fail);
            }

            try
            {
                _driver.ClickWithPosition(element, ClickPosition.BottomRight); // Pass the required 'element' argument
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Can not click element", KeywordResults.Error, true, ex);
            }

            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Click bottom left position of element")]
        [KeywordDisplayName("Click Bottom Left")]
        [KeywordParameters("xPath", "XPath of element")]
        [SampleScript("Mac.Click Bottom Left (//XCUIElementTypeStaticText[@label='Get Supplies']")]
        public KeywordResult ClickBottomLeft(string xPath)
        {
            IWebElement element = FindElementWithTimeout(selector => _driver.FindElement(By.XPath(selector)), xPath, TimeSpan.FromSeconds(3));
            if (element == null)
            {
                return GetFailErrorResult($"Can not find element with given xPath : {xPath}", KeywordResults.Fail);
            }

            try
            {
                _driver.ClickWithPosition(element,ClickPosition.BottomLeft);
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Can not click element", KeywordResults.Error, true, ex);
            }

            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Click top right position of element")]
        [KeywordDisplayName("Click Top Right")]
        [KeywordParameters("xPath", "XPath of element")]
        [SampleScript(" Mac.Click Top Right (//XCUIElementTypeStaticText[@label='Add Your First Printer'])")]
        public KeywordResult ClickTopRight(string xPath)
        {
            IWebElement element = FindElementWithTimeout(selector => _driver.FindElement(By.XPath(selector)), xPath, TimeSpan.FromSeconds(3));

            if (element == null)
            {
                return GetFailErrorResult($"Can not find element with given xPath : {xPath}", KeywordResults.Fail);
            }

            try
            {
                _driver.ClickWithPosition(element,ClickPosition.TopRight);
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Can not click element", KeywordResults.Error, true, ex);
            }

            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Click top left position of element")]
        [KeywordDisplayName("Click Top Left")]
        [KeywordParameters("xPath", "XPath of element")]
        [SampleScript(" Mac.Click Top Left (//XCUIElementTypeOther[@value='1'])")]
        public KeywordResult ClickTopLeft(string xPath)
        {
            // Replace the line with the correct method to find an element by XPath
            IWebElement element = FindElementWithTimeout(selector => _driver.FindElement(By.XPath(selector)), xPath, TimeSpan.FromSeconds(3));

            if (element == null)
            {
                return GetFailErrorResult($"Can not find element with given xPath : {xPath}", KeywordResults.Fail);
            }

            try
            {
                _driver.ClickWithPosition(element, ClickPosition.TopLeft);
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Can not click element", KeywordResults.Error, true, ex);
            }

            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Check if element is exist within given timeout")]
        [KeywordDisplayName("Is Exist")]
        [KeywordParameters("xPath", "XPath of element to find")]
        [SampleScript("Mac.Is Exist(//XCUIElementTypeButton[@identifier='_XCUI:FullScreenWindow'])")]
        public KeywordResult IsExist(string xPath)
        {
            return IsExist(xPath, "3");
        }

        [KeywordDescription("Check if element is exist within given timeout")]
        [KeywordDisplayName("Is Exist")]
        [KeywordParameters("xPath", "XPath of element to find")]
        [KeywordParameters("finding timeout", "Timeout of finding element in seconds")]
        [SampleScript("Mac.Is Exist (//XCUIElementTypeButton[@identifier='_XCUI:FullScreenWindow'],45)")]
        public KeywordResult IsExist(string xPath, string findingTimeout)
        {
            if (!int.TryParse(findingTimeout.Trim(), out int timeOut))
            {
                return GetFailErrorResult("Timeout must be a number", KeywordResults.Error);
            }
            IWebElement element = FindElementWithTimeout(selector => _driver.FindElement(By.XPath(selector)), xPath, TimeSpan.FromSeconds(timeOut));

            if (element == null)
            {
                return GetFailErrorResult($"Element is not exist : {xPath}", KeywordResults.Fail);
            }

            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Wait until element is gone from screen within given timeout")]
        [KeywordDisplayName("Wait Until Gone")]
        [KeywordParameters("xPath", "XPath of element to find")]
        [KeywordParameters("waiting time", "Waiting time of finding element in seconds")]
        [SampleScript("Mac.Wait Until Gone (//XCUIElementTypeStaticText[@label='Printables'],30)")]
        public KeywordResult WaitUntilGone(string xPath, string findingTimeOut)
        {
            if (!int.TryParse(findingTimeOut.Trim(), out int timeOut))
            {
                return GetFailErrorResult("Timeout must be a number", KeywordResults.Error);
            }

            bool notExist = CheckElementNotExist(_driver.FindElement, xPath, TimeSpan.FromSeconds(timeOut));

            if (notExist)
            {
                return new KeywordResult(KeywordResults.Pass);
            }
            else
            {
                return GetFailErrorResult($"Element is still exist : {xPath}", KeywordResults.Fail);
            }
        }

        [KeywordDescription("Set text to element with given xPath and text")]
        [KeywordDisplayName("Set Text")]
        [KeywordParameters("xPath", "xPath of element")]
        [KeywordParameters("text to set", "Text to set to element")]
        [SampleScript("Mac.Set Text(//XCUIElementTypeStaticText[@label='Printables'],Printables)")]
        public KeywordResult SetText(string xPath, string textToSet)
        {
            IWebElement element = FindElementWithTimeout(selector => _driver.FindElement(By.XPath(selector)), xPath, TimeSpan.FromSeconds(3));
            if (element == null)
            {
                return GetFailErrorResult($"Element is not exist : {xPath}", KeywordResults.Fail);
            }
            try
            {
                if (AppBundleId == "com.google.Chrome" || AppBundleId == "com.apple.Safari"|| AppBundleId == "com.microsoft.edgemac" || AppBundleId == "org.mozilla.firefox")

                {
                    element.SendKeys(textToSet);
                }
                else
                {
                    Actions actions = new Actions(_driver);
                    actions.SendKeys(textToSet);
                    actions.Perform();
                }
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Error during send keys", KeywordResults.Error, true, ex);
            }
            return new KeywordResult(KeywordResults.Pass);

        }

        [KeywordDescription("Set text to current active element with given text")]
        [KeywordDisplayName("Set Text")]
        [KeywordParameters("text to set", "Text to set to element")]
        [SampleScript("Mac.Set Text(Sign In)")]
        public KeywordResult SetText(string textToSet)
        {
            WebElement element = null;
            element = _driver.SwitchTo().ActiveElement() as WebElement;
            try
            {
                if (AppBundleId == "com.google.Chrome" || AppBundleId == "com.apple.Safari" || AppBundleId == "com.microsoft.edgemac" || AppBundleId == "org.mozilla.firefox")
                {
                    element.SendKeys(textToSet);
                }
                else
                {
                    Actions actions = new Actions(_driver);
                    actions.SendKeys(Keys.Tab).SendKeys(textToSet).Perform();
                }
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Error during send keys", KeywordResults.Error, true, ex);
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Send Tab Key to target")]
        [KeywordDisplayName("Press Tab Key")]
        [SampleScript("Mac.Press Tab Key")]
        public KeywordResult PressTabKey()
        {
            WebElement element = null;
            try
            {
                element = _driver.SwitchTo().ActiveElement() as WebElement;
                if (element == null)
                {
                    return GetFailErrorResult($"Can not find active element", KeywordResults.Fail);
                }
            }
            catch (Exception ex)
            {
                return GetFailErrorResult($"Can not find active element", KeywordResults.Error, true, ex);
            }

            try
            {
                var actions = new Actions(_driver);
                actions.SendKeys(Keys.Tab).Perform();
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Error during send keys", KeywordResults.Error, true, ex);
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Send Enter Key to target")]
        [KeywordDisplayName("Press Enter Key")]
        [SampleScript("Mac.Press Enter Key")]
        public KeywordResult PressEnterKey()
        {
            WebElement element = null;
            try
            {
                element = _driver.SwitchTo().ActiveElement() as WebElement;
                if (element == null)
                {
                    return GetFailErrorResult($"Can not find active element", KeywordResults.Fail);
                }
            }
            catch (Exception ex)
            {
                return GetFailErrorResult($"Can not find active element", KeywordResults.Error, true, ex);
            }

            try
            {
                if (AppBundleId == "com.google.Chrome" || AppBundleId == "com.apple.Safari" || AppBundleId == "com.microsoft.edgemac")
                {
                    Actions actions = new Actions(_driver);
                    actions.SendKeys(Keys.Enter).Perform();
                }
                else
                {
                    Actions actions = new Actions(_driver);
                    actions.SendKeys(Keys.Shift).SendKeys(Keys.Return).Perform();
                }

            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Error during send keys", KeywordResults.Error, true, ex);
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Capture Screen Shot of device and save to PC")]
        [KeywordDisplayName("Capture Screen Shot")]
        [KeywordParameters("filename", "filename to save")]
        [SampleScript("Mac.Capture Screen Shot(${ss})")]
        public KeywordResult CaptureScreenShot(string filename)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {

                kr.ScreenShot = GetScreenCapture();
                if (kr.ScreenShot != null && kr.ScreenShot.Length > 0)
                {
                    if (!string.IsNullOrEmpty(filename))
                    {
                        string saveTo = Path.Combine(_outputDir, filename);
                        File.WriteAllBytes(saveTo, kr.ScreenShot);
                        if (File.Exists(saveTo))
                        {
                            return kr;
                        }
                    }
                }
                return GetFailErrorResult("Screen capture fail", KeywordResults.Fail, false);
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Error during capture screen shot", KeywordResults.Error, false, ex);
            }
        }
        [KeywordDescription("Set Navigation to current active element with given Url")]
        [KeywordDisplayName("Navigate Url")]
        [KeywordParameters("xPath", "Url to set to element")]
        [KeywordParameters("xPath", "xPath of element")]
        [SampleScript("Mac.Navigate Url(//XCUIElementTypeTextField[@identifier='WEB_BROWSER_ADDRESS_AND_SEARCH_FIELD'],https://magento-stg.tropos-rnd.com/storefront)")]
        public KeywordResult NavigateUrl(string xPath, string url)
        {
            // Replace the incorrect method call with the correct one
            IWebElement element = FindElementWithTimeout(selector => _driver.FindElement(By.XPath(selector)), xPath, TimeSpan.FromSeconds(3));

            if (element == null)
            {
                return GetFailErrorResult($"Element is not exist : {xPath}", KeywordResults.Fail);
            }
            try
            {
                _driver.Navigate().GoToUrl(url);
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Error during send keys", KeywordResults.Error, true, ex);
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Swipe the page")]
        [KeywordDisplayName("Swipe")]
        [KeywordParameters("direction", "Should be one of <, <<, >, >>, ^, ^^, v, vv\r\n" +
       "< (right to lefet with small amount), << (right to left with large amount) \r\n" +
       "> (left to right with small amount), >> (left to right with large amount) \r\n" +
       "^ (down to up with small amount), ^^ (down to up with larget amount) \r\n" +
       "v (up to down with small amount), vv (up to down with larget amount)")]
        [KeywordParameters("xPath", "xPath of element")]

        [SampleScript("Mac.Swipe (^)")]
        public KeywordResult Swipe(string direction)
        {
            int scrollElement = 2;
            try
            {
                Actions action = new Actions(_driver);
                switch (direction)
                {
                    case "<":
                        action.SendKeys(Keys.ArrowLeft).Perform();
                        break;
                    case ">":
                        action.SendKeys(Keys.ArrowRight).Perform();
                        break;
                    case "^":
                        action.SendKeys(Keys.ArrowUp).Perform();
                        break;
                    case "v":
                        action.SendKeys(Keys.ArrowDown).Perform();
                        break;
                    case "^^":
                        for (int i = 0; i < scrollElement; i++)
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                action.SendKeys(Keys.ArrowUp).Perform();
                                Thread.Sleep(200);
                            }
                        }
                        break;
                    case "vv":

                        for (int i = 0; i < scrollElement; i++)
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                action.SendKeys(Keys.ArrowDown).Perform();
                                Thread.Sleep(200);
                            }
                        }
                        break;
                    case "<<":
                        for (int i = 0; i < scrollElement; i++)
                        {
                            action.SendKeys(Keys.Left).Perform();
                            for (int j = 0; j < 2; j++)
                            {
                                action.SendKeys(Keys.ArrowLeft).Perform();
                                Thread.Sleep(200);
                            }

                        }
                        break;
                    case ">>":

                        for (int i = 0; i < scrollElement; i++)
                        {
                            action.SendKeys(Keys.Right).Perform();
                            Thread.Sleep(200);
                            for (int j = 0; j < 2; j++)
                            {
                                action.SendKeys(Keys.ArrowRight).Perform();
                                Thread.Sleep(200);
                            }
                        }
                        break;

                    default:
                        return GetFailErrorResult($"Invalid direction: {direction}", KeywordResults.Error, false);
                }

            }
            catch (Exception)
            {
                return GetFailErrorResult($"Invalid direction: {direction}", KeywordResults.Error, false);
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription(" Swipe the page with xPath")]
        [KeywordDisplayName("Swipe")]
        [KeywordParameters("direction", "Should be one of <, >, ^, v, \r\n" +
            "< (right to lefet with small amount) \r\n" +
            "> (left to right with small amount) \r\n" +
            "^ (down to up with small amount) \r\n" +
            "v (up to down with small amount)")]
        [KeywordParameters("xPath", "xPath of element")]

        [SampleScript("Mac.Swipe (//XCUIElementTypeStaticText[@value='Explore'],^)")]
        public KeywordResult Swipe(string xPath, string direction)
        {
            IWebElement element = FindElementWithTimeout(selector => _driver.FindElement(By.XPath(selector)), xPath, TimeSpan.FromSeconds(3));

            if (element == null)
            {
                return GetFailErrorResult($"Element is not exist : {xPath}", KeywordResults.Fail);
            }
            var count = 8;
            string SelectCountry = element.GetAttribute("value");
            var Mac = "mac";
            _country = "block";

            try
            {
                switch (direction)
                {
                    case "<":
                        while (true)
                        {
                            var cap = _vision.SetTarget(Mac);
                            var textgroup = _vision.SetTextGrouping(_country);
                            var result = _vision.IsTextExist(SelectCountry);

                            if (result.Result == KeywordResults.Fail)
                            {
                                for (int j = 0; j < count; j++)
                                {
                                    Actions action = new Actions(_driver);
                                    action.SendKeys(Keys.ArrowLeft).Perform();
                                }
                            }
                            else
                            {
                                // Vision Click is not required — mac.click will directly click the element using its XPath
                                return new KeywordResult(KeywordResults.Pass);
                            }
                        }

                        break;
                    case ">":
                        while (true)
                        {
                            var cap = _vision.SetTarget(Mac);
                            var textgroup = _vision.SetTextGrouping(_country);
                            var result = _vision.IsTextExist(SelectCountry);

                            if (result.Result == KeywordResults.Fail)
                            {
                                for (int j = 0; j < count; j++)
                                {
                                    Actions action = new Actions(_driver);
                                    action.SendKeys(Keys.ArrowRight).Perform();

                                }
                            }
                            else
                            {
                                // Vision Click is not required — mac.click will directly click the element using its XPath
                                return new KeywordResult(KeywordResults.Pass);
                            }
                        }

                        break;
                    case "^":
                        while (true)
                        {
                            var cap = _vision.SetTarget(Mac);
                            var textgroup = _vision.SetTextGrouping(_country);
                            var result = _vision.IsTextExist(SelectCountry);

                            if (result.Result == KeywordResults.Fail)
                            {
                                for (int j = 0; j < count; j++)
                                {
                                    Actions action = new Actions(_driver);
                                    action.SendKeys(Keys.ArrowUp).Perform();
                                }
                            }
                            else
                            {
                                // Vision Click is not required — mac.click will directly click the element using its XPath
                                return new KeywordResult(KeywordResults.Pass);
                            }
                        }
                        break;

                    case "v":
                        while (true)
                        {
                            var cap = _vision.SetTarget(Mac);
                            var textgroup = _vision.SetTextGrouping(_country);
                            var result = _vision.IsTextExist(SelectCountry);

                            if (result.Result == KeywordResults.Fail)
                            {
                                for (int j = 0; j < count; j++)
                                {
                                    Actions action = new Actions(_driver);
                                    action.SendKeys(Keys.ArrowDown).Perform();
                                }
                            }
                            else
                            {
                                // Vision Click is not required — mac.click will directly click the element using its XPath
                                return new KeywordResult(KeywordResults.Pass);
                            }
                        }
                        break;

                    default:
                        return GetFailErrorResult($"Invalid direction: {direction}", KeywordResults.Fail, false);
                }
            }
            catch (Exception ex)
            {
                return GetFailErrorResult($"Invalid direction: {direction}", KeywordResults.Error, false);
            }
            return new KeywordResult(KeywordResults.Pass);

        }

        [KeywordDescription("List Swipe the with in the list ")]
        [KeywordDisplayName("List Scroll")]
        [KeywordParameters("direction", "Should be one of <,>,^, v, \r\n" +
       "< (right to lefet with small amount),  \r\n" +
       "> (left to right with small amount),  \r\n" +
       "^ (down to up with small amount), ^^  \r\n" +
       "v (up to down with small amount)")]
        [KeywordParameters("xPath", "xPath of element")]

        [SampleScript("Mac.ListScroll (^)")]
        public KeywordResult ListScroll(string xPath, string direction)
        {
            IWebElement element = FindElementWithTimeout(selector => _driver.FindElement(By.XPath(selector)), xPath, TimeSpan.FromSeconds(3));

            if (element == null)
            {
                return GetFailErrorResult($"Element is not exist : {xPath}", KeywordResults.Fail);
            }
            string SelectCountry = element.GetAttribute("value");
            var Mac = "mac";
            _country = "block";

            try
            {
                switch (direction)
                {
                    case "<":
                        while (true)
                        {
                            var cap = _vision.SetTarget(Mac);
                            var textgroup = _vision.SetTextGrouping(_country);
                            var result = _vision.IsTextExist(SelectCountry);

                            if (result.Result == KeywordResults.Fail || result.Output == "Can not find United States in the screen")
                            {
                                for (int j = 0; j < 5; j++)
                                {
                                    Actions action = new Actions(_driver);
                                    action.SendKeys(Keys.Tab).SendKeys(Keys.ArrowLeft).Perform();
                                }
                            }
                            else
                            {
                                // Vision Click is not required — mac.click will directly click the element using its XPath
                                return new KeywordResult(KeywordResults.Pass);
                            }
                        }

                        break;
                    case ">":
                        while (true)
                        {
                            var cap = _vision.SetTarget(Mac);
                            var textgroup = _vision.SetTextGrouping(_country);
                            var result = _vision.IsTextExist(SelectCountry);

                            if (result.Result == KeywordResults.Fail || result.Output == "Can not find United States in the screen")
                            {
                                for (int j = 0; j < 5; j++)
                                {
                                    Actions action = new Actions(_driver);
                                    action.SendKeys(Keys.Tab).SendKeys(Keys.ArrowRight).Perform();
                                }
                            }
                            else
                            {
                                // Vision Click is not required — mac.click will directly click the element using its XPath
                                return new KeywordResult(KeywordResults.Pass);
                            }
                        }

                        break;
                    case "^":
                        while (true)
                        {
                            var cap = _vision.SetTarget(Mac);
                            var textgroup = _vision.SetTextGrouping(_country);
                            var result = _vision.IsTextExist(SelectCountry);

                            if (result.Result == KeywordResults.Fail || result.Output == "Can not find United States in the screen")
                            {
                                for (int j = 0; j < 12; j++)
                                {
                                    Actions action = new Actions(_driver);
                                    action.SendKeys(Keys.Tab).SendKeys(Keys.ArrowUp).Perform();
                                }
                            }
                            else
                            {
                                // Vision Click is not required — mac.click will directly click the element using its XPath
                                return new KeywordResult(KeywordResults.Pass);
                            }
                        }
                        break;

                    case "v":
                        while (true)
                        {
                            var cap = _vision.SetTarget(Mac);
                            var textgroup = _vision.SetTextGrouping(_country);
                            var result = _vision.IsTextExist(SelectCountry);

                            if (result.Result == KeywordResults.Fail || result.Output == "Can not find United States in the screen")
                            {
                                for (int j = 0; j < 12; j++)
                                {
                                    Actions action = new Actions(_driver);
                                    action.SendKeys(Keys.Tab).SendKeys(Keys.ArrowDown).Perform();
                                }
                            }
                            else
                            {
                                // Vision Click is not required — mac.click will directly click the element using its XPath
                                return new KeywordResult(KeywordResults.Pass);
                            }
                        }
                        break;

                    default:
                        return GetFailErrorResult($"Invalid direction: {direction}", KeywordResults.Error, false);
                }
            }
            catch (Exception ex)
            {
                return GetFailErrorResult($"Invalid direction: {direction}", KeywordResults.Error, false);
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Clear Text From The Text Field")]
        [KeywordDisplayName("Clear Text")]
        [SampleScript("Mac.Clear Text")]
        public KeywordResult ClearText()
        {
            AppiumElement element = null;
            element = _driver.SwitchTo().ActiveElement() as AppiumElement;

            if (element == null || string.IsNullOrWhiteSpace(element.Id))
            {
                return GetFailErrorResult("Cannot find active element or element ID is empty", KeywordResults.Fail);
            }
            try
            {
                if (AppBundleId == "com.google.Chrome" || AppBundleId == "com.apple.Safari" || AppBundleId == "com.microsoft.edgemac")
                {
                    element.Clear();
                }
                else
                {
                    new Actions(_driver)
                    .Click(element)
                    .KeyDown(Keys.Command)
                    .SendKeys("a")
                    .KeyUp(Keys.Command)
                    .SendKeys(Keys.Backspace)
                    .Perform();

                }
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Failing  clearing text using element ID", KeywordResults.Fail, true, ex);
            }
            return new KeywordResult(KeywordResults.Pass);
        }
        [KeywordDescription("Clear Text From The Text Field")]
        [KeywordDisplayName("Clear Text")]
        [KeywordParameters("xPath", "xPath of element")]
        [SampleScript("Mac.Clear Text")]
        public KeywordResult ClearText(string xpath)
        {
            IWebElement element = FindElementWithTimeout(selector => _driver.FindElement(By.XPath(selector)), xpath, TimeSpan.FromSeconds(3));
            if (element == null)
            {
                return GetFailErrorResult("Cannot find active element or element ID is empty", KeywordResults.Fail);
            }
            try
            {
                if (AppBundleId == "com.google.Chrome" || AppBundleId == "com.apple.Safari" || AppBundleId == "com.microsoft.edgemac")
                {
                    element.Clear();
                }
                else
                {
                    new Actions(_driver)
                     .Click(element)
                     .KeyDown(Keys.Command)
                     .SendKeys("a")
                     .KeyUp(Keys.Command)
                     .SendKeys(Keys.Backspace)
                     .Perform();

                }
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Failing  clearing text using element ID", KeywordResults.Fail, true, ex);
            }

        }

        [KeywordDescription("Hover the mouse over the UI element identified by visible text.")]
        [KeywordDisplayName("Mouse Hover On Text")]
        [KeywordParameters("text", "The visible text of the UI element to hover over.")]
        [SampleScript("Mac.Mouse Hover On Text(Print)")]
        public KeywordResult MousehoverOnText(string text)
        {
            AppiumElement element = null;
            try
            {
                element = _driver.FindElement(MobileBy.Name(text));
            }
            catch (Exception ex)
            {
                return GetFailErrorResult($"Cannot find element with text: {text}", KeywordResults.Fail, true, ex);
            }

            if (element == null)
            {
                return GetFailErrorResult($"Cannot find element with text: {text}", KeywordResults.Fail);
            }

            // Get element center coordinates
            var rect = element.Rect;
            var centerX = rect.X + rect.Width / 2;
            var centerY = rect.Y + rect.Height / 2;

            // Move mouse to the element's position
            var hoverArgs = new Dictionary<string, object>()
            {
                { "x", centerX },
                { "y", centerY }
            };

            _driver.ExecuteScript("macos:hover", hoverArgs);
            return new KeywordResult(KeywordResults.Pass);
        }
        [KeywordDescription("Hover the mouse over the UI element identified by XPath.")]
        [KeywordDisplayName("Mouse Hover On XPath")]
        [KeywordParameters("xPath", "The XPath of the UI element to hover over.")]
        [SampleScript("Mac.Mouse Hover On XPath(//XCUIElementTypeStaticText[@label='Print'])")]
        public KeywordResult MousehoverOnXPath(string xPath)
        {
            IWebElement element = null;
            try
            {
                element = FindElementWithTimeout(selector => _driver.FindElement(By.XPath(selector)), xPath, TimeSpan.FromSeconds(3));
                if (element == null)
                {
                    return GetFailErrorResult($"Cannot find element with XPath: {xPath}", KeywordResults.Error);
                }
            }
            catch (Exception ex)
            {
                return GetFailErrorResult($"Cannot find element with XPath: {xPath}", KeywordResults.Error, true, ex);
            }

            var location = element.Location;
            var size = element.Size;
            var rect = new
            {
                X = location.X,
                Y = location.Y,
                Width = size.Width,
                Height = size.Height
            };
            var centerX = rect.X + rect.Width / 2;
            var centerY = rect.Y + rect.Height / 2;

            var hoverArgs = new Dictionary<string, object>()
            {
                { "x", centerX },
                { "y", centerY }
            };

            try
            {
                _driver.ExecuteScript("macos:hover", hoverArgs);
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Failed to hover over element", KeywordResults.Error, true, ex);
            }

            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Scroll within the country selection list based on the provided xpath")]
        [KeywordDisplayName("Country Scroll")]
        [KeywordParameters("xPath", "xPath of element")]
        [KeywordParameters("direction", "Should be either ^ or v\r\n" + "^ (scroll up with a small amount), \r\n" + "v (scroll down with a small amount)")]
        [SampleScript("Mac.Country Scroll (//XCUIElementTypeStaticText[@value='Explore'],^)")]

        public KeywordResult CountryScroll(string xPath, string direction)
        {
            string searchText = string.Empty;
            IWebElement element = FindElementWithTimeout(
                selector => _driver.FindElement(By.XPath(selector)),
                xPath,
                TimeSpan.FromSeconds(3));

            if (element == null)
                return GetFailErrorResult($"Element not found: {xPath}", KeywordResults.Fail);

            searchText = element.GetAttribute("value");
            if (string.IsNullOrWhiteSpace(searchText))
            {
                searchText = element.GetAttribute("label");
            }

            string targetPlatform = "mac";
            _country = "block";

            var windowSize = _driver.Manage().Window.Size;
            int screenHeight = windowSize.Height;
            int screenWidth = windowSize.Width;


            int scrollPixels = (int)(screenHeight * 0.18);

            if (scrollPixels < 180)
                scrollPixels = 180;

            if (scrollPixels > 450)
                scrollPixels = 450;



            int scrollX = screenWidth / 2;
            int scrollY = (int)(screenHeight * 0.30);

            int attempts = 30;
            try
            {
                _vision.SetTarget(targetPlatform);
                _vision.SetTextGrouping(_country);

                int scrollDirection = direction == "^" ? -1 : 1;

                for (int i = 0; i < attempts; i++)
                {
                    var result = _vision.IsTextExist(searchText);
                    if (result.Result == KeywordResults.Pass)
                        return new KeywordResult(KeywordResults.Pass);

                    var scrollArgs = new Dictionary<string, object>
                    {
                        { "x", scrollX },
                        { "y", scrollY },
                        { "deltaX", 0 },
                        { "deltaY", scrollDirection * scrollPixels }
                    };
                    _driver.ExecuteScript("macos: scroll", scrollArgs);
                    Thread.Sleep(150);
                }
                return GetFailErrorResult($"{searchText} not found after scrolling.", KeywordResults.Fail);
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Scrolling failed", KeywordResults.Fail, true, ex);
            }
        }

        [KeywordDescription("Scroll within the country selection list based on the provided steps")]
        [KeywordDisplayName("Steps Scroll")]
        [KeywordParameters("direction", "Should be either ^ or v\r\n" + "^ (scroll up with a small amount), \r\n" + "v (scroll down with a small amount)")]
        [KeywordParameters("steps", "Number of scroll steps to perform")]
        [SampleScript("Mac.Steps Scroll (5,^)")]
        public KeywordResult StepsScroll(string steps, string direction)
        {
            if (!int.TryParse(steps, out int stepCount) || stepCount <= 0)
                return GetFailErrorResult("Invalid steps value", KeywordResults.Error, false);

            var windowSize = _driver.Manage().Window.Size;
            int screenHeight = windowSize.Height;
            int screenWidth = windowSize.Width;

            int scrollPixels = (int)(screenHeight * 0.30);


            if (scrollPixels < 180)
                scrollPixels = 180;

            if (scrollPixels > 450)
                scrollPixels = 450;

            int scrollX = screenWidth / 2;
            int scrollY = (int)(screenHeight * 0.25);

            int directionFactor = direction == "^" ? -1 : direction == "v" ? 1 : 0;
            if (directionFactor == 0)
                return GetFailErrorResult($"Invalid direction: {direction}", KeywordResults.Error, false);

            var scrollArgs = new Dictionary<string, object>
                    {
                        { "x", scrollX },
                        { "y", scrollY },
                        { "deltaX", 0 },
                        { "deltaY", directionFactor * scrollPixels }
                    };

            for (int i = 0; i < stepCount; i++)
            {
                scrollArgs["deltaY"] = directionFactor * scrollPixels;
                _driver.ExecuteScript("macos: scroll", scrollArgs);

                Thread.Sleep(150);
            }

            return new KeywordResult(KeywordResults.Pass);
        }
        [KeywordDescription("Escape key on macOS application")]
        [KeywordDisplayName("Escape Key")]
        [SampleScript("Mac.Escape Key")]
        public KeywordResult EscapeKey()
        {
            try
            {
                Actions actions = new Actions(_driver);
                actions.SendKeys(Keys.Escape).Perform();
                return new KeywordResult(KeywordResults.Pass, "Escape key pressed successfully");
            }
            catch (Exception ex)
            {
                return new KeywordResult(KeywordResults.Fail, "Failed to press Escape key: " + ex.Message);
            }
        }
        /// <summary>
        /// Validates that the given bundle id is not null or empty/whitespace.
        /// Returns a failure KeywordResult if invalid, or null if the bundle id is valid.
        /// </summary>
        private KeywordResult ValidateBundleId(string bundleId)
        {
            if (string.IsNullOrWhiteSpace(bundleId))
            {
                return GetFailErrorResult("Bundle id must not be empty.", KeywordResults.Error, false);
            }

            return null;
        }

        /// <summary>
        /// Escapes a value so it can be safely embedded inside a single-quoted string in a shell command,
        /// preventing shell injection and command-parsing issues from special characters (e.g. spaces, quotes).
        /// </summary>
        private static string EscapeForSingleQuotedShell(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return value.Replace("'", "'\\''");
        }
        [KeywordDescription("Remove installed macOS application using bundle identifier")]
        [KeywordDisplayName("Remove App")]
        [KeywordParameters("Bundle id", "Application bundle identifier to remove")]
        [SampleScript("Mac.Remove App(com.apple.Safari)")]
        public KeywordResult RemoveApp(string bundleId)
        {
            try
            {
                var bundleIdValidationResult = ValidateBundleId(bundleId);
                if (bundleIdValidationResult != null)
                {
                    return bundleIdValidationResult;
                }

                // Find the application path using bundle ID
                var result = AppiumServerUtils.SudoExecute($"mdfind \"kMDItemCFBundleIdentifier == '{bundleId}'\"");
                if (string.IsNullOrWhiteSpace(result))
                {
                    return GetFailErrorResult($"No application found for bundle id: {bundleId}", KeywordResults.Fail, false);
                }

                // Extract app name from the path
                var match = Regex.Match(result, @"([^/\s]+\.app)");
                if (!match.Success || string.IsNullOrWhiteSpace(match.Value))
                {
                    return GetFailErrorResult($"Unable to resolve application name from search result for bundle id: {bundleId}", KeywordResults.Fail, false);
                }

                var appName = match.Value;
                var appNameWithoutExtension = appName.Replace(".app", "");

                // Force quit the application if it's running
                AppiumServerUtils.SudoExecute($"pkill -9 '{appNameWithoutExtension}'");

                // Remove the application bundle
                AppiumServerUtils.SudoExecute($"rm -rf '/Applications/{appName}'");

                // Quick LaunchServices database update (much faster)
                AppiumServerUtils.SudoExecute($"/System/Library/Frameworks/CoreServices.framework/Frameworks/LaunchServices.framework/Support/lsregister -u '/Applications/{appName}'");

                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Error while removing application.", KeywordResults.Error, true, ex);
            }            
        }
        [KeywordDescription("Send a running macOS application to the background (hide it) using its bundle identifier, without terminating or quitting it")]
        [KeywordDisplayName("Hide App")]
        [KeywordParameters("Bundle id", "Bundle ID of the app to hide (send to background). If empty, hides the currently launched app.")]
        [SampleScript("Mac.Hide App(com.apple.Safari)")]
        public KeywordResult HideApp(string appBundleId = "")
        {
            try
            {
                string bundleId = string.IsNullOrWhiteSpace(appBundleId) ? AppBundleId : appBundleId;
                var bundleIdValidationResult = ValidateBundleId(bundleId);
                if (bundleIdValidationResult != null)
                {
                    return bundleIdValidationResult;
                }

                string userName = AppiumServerUtils.UserName;
                string escapedUserName = EscapeForSingleQuotedShell(userName);
                string escapedBundleId = EscapeForSingleQuotedShell(bundleId);
                string script =
                    $"launchctl asuser \"$(id -u '{escapedUserName}')\" sudo -u '{escapedUserName}' " +
                    "osascript -e 'tell application \"System Events\" to set visible of " +
                    $"(first process whose bundle identifier is \"{escapedBundleId}\") to false'";
                AppiumServerUtils.SudoExecute(script);

                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Error while Hide application.", KeywordResults.Error, true, ex);
            }
        }

        [KeywordDescription("Force terminate (kill) a running macOS application using its bundle identifier")]
        [KeywordDisplayName("Terminate App")]
        [KeywordParameters("Bundle id", "Bundle ID of the app to terminate. If empty, terminates the currently launched app.")]
        [SampleScript("Mac.Terminate App(com.apple.Safari)")]
        public KeywordResult TerminateApp(string appBundleId = "")
        {
            try
            {
                string bundleId = string.IsNullOrWhiteSpace(appBundleId) ? AppBundleId : appBundleId;
                var bundleIdValidationResult = ValidateBundleId(bundleId);
                if (bundleIdValidationResult != null)
                {
                    return bundleIdValidationResult;
                }

                bool terminated = false;
                if (_driver != null)
                {
                    try
                    {
                        _driver.ExecuteScript("macos: terminateApp", new Dictionary<string, object>
                        {
                            { "bundleId", bundleId }
                        });
                        terminated = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Error while terminating app '{bundleId}' via driver. Falling back to pkill.", ex);
                        terminated = false;
                    }
                }

                if (!terminated)
                {
                    // Fallback: resolve the app name from the bundle id and force-kill it in a single
                    // SSH round-trip (instead of a separate mdfind call followed by a separate pkill call).
                    string escapedBundleId = EscapeForSingleQuotedShell(bundleId);
                    string oneShotKill =
                        "app=$(mdfind \"kMDItemCFBundleIdentifier == '" + escapedBundleId + "'\" | head -n 1); " +
                        "name=$(basename \"$app\" .app); " +
                        "[ -n \"$name\" ] && pkill -9 \"$name\"";
                    AppiumServerUtils.SudoExecute(oneShotKill);
                }

                if (bundleId == AppBundleId)
                {
                    AppBundleId = null;
                }

                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Error while terminating application.", KeywordResults.Error, true, ex);
            }
        }


    }
}

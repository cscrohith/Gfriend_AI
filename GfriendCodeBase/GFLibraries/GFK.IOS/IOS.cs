using HP.GFriend.GFLogger;
using HP.GFriend.Utils.Appium;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;
using PointerInputDevice = OpenQA.Selenium.Appium.Interactions.PointerInputDevice;
namespace HP.GFriend.Keywords
{
    [LibraryDescription("For using iOS library, appium server environment should be setup.\r\n" +
        "Appium server should be able to access via ssh with default port 22.")]
    public class IOS : IGFLibrary
    {
        private IOSDriver _driver;
        private DeviceUnderTest _dut;
        private AppiumOptions _capabilites;
        private string _outputDir;
        private string _xCodeOrgId;
        private int _displayWidth;
        private int _displayHeight;
        private bool _wdaEnt;
        private string _wdaPrebuiltPath;
        private int _scaleFactor = 1;

        public void Dispose()
        {
            if (_dut != null)
            {
                AppiumServerUtils.StopWDA(_dut.LanDebugAddress);
            }
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
            return null;
        }

        public string GetName()
        {
            return "IOS";
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

        private Tuple<int, int> GetAbsolutePosition(int x, int y)
        {
            try
            {
                double dX = ((x) / (double)100);
                double dY = ((y) / (double)100);

                int aX = (int)(_displayWidth * dX);
                int aY = (int)(_displayHeight * dY);

                return new Tuple<int, int>(aX, aY);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private Tuple<int, int> GetAbsolutePosition(IWebElement element, int x, int y)
        {
            try
            {
                double dX = ((x) / (double)100);
                double dY = ((y) / (double)100);

                int aX = element.Location.X + (int)(element.Size.Width * dX);
                int aY = element.Location.Y + (int)(element.Size.Height * dY);

                return new Tuple<int, int>(aX, aY);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool ExecuteWDA()
        {
            if (AppiumServerUtils.IsWdaEntSetup())
            {
                Logger.Debug("Using prebuilt WDA with Enterprise account.");
                _wdaPrebuiltPath = AppiumServerUtils.GetPrebuiltWDAPath();
                if (string.IsNullOrEmpty(_wdaPrebuiltPath))
                {
                    Logger.Error("Can not find prebuild WDA in the server");
                    return false;
                }
                _xCodeOrgId = "TKM6D5Y743";
                _wdaEnt = true;
                return true;
            }
            else
            {
                _wdaEnt = false;
                _xCodeOrgId = AppiumServerUtils.ExecuteWDA(_dut.DeviceAddress);
                _xCodeOrgId = "TKM6D5Y743";
                if (string.IsNullOrEmpty(_xCodeOrgId))
                {
                    return false;
                }
                return true;
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

        private IWebElement FindElementWithTimeout(Func<By, IWebElement> FindElement, string selector, TimeSpan timeOut)
        {
            IWebElement target = null;
            DateTime endTime = DateTime.Now.AddMilliseconds(timeOut.TotalMilliseconds * _scaleFactor);

            while (DateTime.Now <= endTime)
            {
                try
                {
                    target = FindElement(By.XPath(selector));

                    if (target != null)
                    {
                        return target;
                    }
                }
                catch (NoSuchElementException)
                {
                    // Element not found, continue polling
                }
                catch (WebDriverTimeoutException)
                {
                    // Timeout from WebDriverWait, continue polling
                }
                catch (WebDriverException ex)
                {
                    if (ex.Message.IndexOf("socket hang up", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Logger.Error("Socket hang up. Restart WDA");
                        AppiumServerUtils.StopWDA(_dut.LanDebugAddress);
                        if (!ExecuteWDA())
                        {
                            throw new ApplicationException("Web driver agent is crashed during execution.");
                        }
                    }
                }

                Thread.Sleep(500);
            }

            return null;
        }

        public bool CheckElementNotExist(Func<string, IWebElement> findFunc, string locator, TimeSpan timeout)
        {
            var wait = new DefaultWait<IWebDriver>(_driver)
            {
                Timeout = timeout,
                PollingInterval = TimeSpan.FromMilliseconds(500)
            };
            wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));

            try
            {
                return wait.Until(driver =>
                {
                    try
                    {
                        var element = findFunc(locator);
                        return !element.Displayed;
                    }
                    catch (NoSuchElementException)
                    {
                        return true; // Element not found — this is what we want
                    }
                    catch (StaleElementReferenceException)
                    {
                        return true; // Element became stale — also good
                    }
                });
            }
            catch (WebDriverTimeoutException)
            {
                return false; // Timed out and element was still present
            }
        }


        public void Initialize(DeviceUnderTest dut, string outputDir)
        {
            _outputDir = outputDir;
            _dut = dut;

            // Check dut if it has desired capabilites
            if (string.IsNullOrEmpty(_dut.LanDebugAddress) ||
                string.IsNullOrEmpty(_dut.AdminId) ||
                string.IsNullOrEmpty(_dut.AdminPassword) ||
                _dut.Port == 0)
            {
                throw new LibraryInitializeException("Appium Server's Address, port, user id and password should be given in LanDeubAddress, Port, Admin ID and Password field in device information.");
            }

            _capabilites = new AppiumOptions();
            _capabilites.AddAdditionalAppiumOption("appium:udid", _dut.LanDebugAddress);
            _capabilites.AddAdditionalAppiumOption("appium:xcodeSigningId", "iPhone Developer");
            _capabilites.AddAdditionalAppiumOption("appium:noReset", true);
            _capabilites.PlatformName = "iOS";
            _capabilites.AutomationName = "XCUITest";
            _capabilites.AddAdditionalAppiumOption("appium:wdaStartupRetryInterval", 20000);
            _capabilites.AddAdditionalAppiumOption("appium:wdaLaunchTimeout", 1200000);
            _capabilites.AddAdditionalAppiumOption("appium:wdaConnectionTimeout", 2240000);

            AppiumServerUtils.Initialize(_dut.DeviceAddress, 22, _dut.AdminId, _dut.AdminPassword);
            if (!AppiumServerUtils.ExecuteAppiumServer())
            {
                throw new LibraryInitializeException("Can not start Appium server.");
            }

            if (!ExecuteWDA())
            {
                throw new LibraryInitializeException("Can not start WebDriverAgent from target. Check Web Driver Agent is installed.");
            }
            string deviceName = AppiumServerUtils.GetDeviceName(dut.LanDebugAddress);
            if (string.IsNullOrEmpty(deviceName))
            {
                Logger.Debug("Getting device name fail. Try again.");
                deviceName = AppiumServerUtils.GetDeviceName(dut.LanDebugAddress);
            }
            //  _capabilites.AddAdditionalAppiumOption("appium:deviceName", deviceName);
            _capabilites.AddAdditionalAppiumOption("appium:xcodeOrgId", _xCodeOrgId);
            if (_wdaEnt)
            {
                _capabilites.AddAdditionalAppiumOption("appium:usePrebuiltWDA", true);
                _capabilites.AddAdditionalAppiumOption("appium:updatedWDABundleId", "com.hp.GFriend.WDA");
                _capabilites.AddAdditionalAppiumOption("appium:derivedDataPath", _wdaPrebuiltPath);

            }

        }

        [KeywordDescription("Set timeout scacle for applying all keywords in library." +
            "This scale facor will be multiplied of all timeout arguments in keywords." +
            "For example, if scale factor is set to 2 and call wait for object for 3 secondes, it will wait for 6 (3 x 2) seconds.")]
        [KeywordDisplayName("Set Timeout Scale")]
        [KeywordParameters("scaleFactor", "scale factor. Should be a number and larger than 0")]
        [SampleScript("IOS.Set Timeout Scale (10)")]
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

        [KeywordDescription("Launch App with given bundle id")]
        [KeywordDisplayName("Launch App")]
        [KeywordParameters("App bundle ID", "Bundle ID of app to launch")]
        [SampleScript("IOS.Launch App (com.hp.printer.control)")]
        public KeywordResult LaunchApp(string appBundleId)
        {
            _capabilites.AddAdditionalAppiumOption("appium:bundleId", appBundleId);
            if (_driver != null)
            {
                _driver.Quit();
                _driver.Dispose();
                _driver = null;
            }
            try
            {
                _driver = new IOSDriver(new Uri($"http://{_dut.DeviceAddress}:" +
                $"{_dut.Port}/wd/hub"), _capabilites);

                CommonExecutionInfo.SetSharedObject("Ios:" + _dut.DeviceId, _driver);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Unable to launch WebDriverAgent"))
                {
                    AppiumServerUtils.StopWDA(_dut.LanDebugAddress);
                    ExecuteWDA();
                    _driver = new IOSDriver(new Uri($"http://{_dut.DeviceAddress}:" +
                    $"{_dut.Port}/wd/hub"), _capabilites);
                }
                else
                {
                    throw ex;
                }
            }
            _driver.ActivateApp(appBundleId);
            _driver.GetScreenshot();
            _displayWidth = _driver.Manage().Window.Size.Width;
            _displayHeight = _driver.Manage().Window.Size.Height;

            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Click Element with XPath")]
        [KeywordDisplayName("Click")]
        [KeywordParameters("xPath", "XPath of element to click")]
        [SampleScript("IOS.Click (//XCUIElementTypeButton[@name='Accept All'])")]
        public KeywordResult Click(string xPath)
        {
            return Click(xPath, "3");
        }

        [KeywordDescription("Click Element with XPath")]
        [KeywordDisplayName("Click")]
        [KeywordParameters("xPath", "XPath of element to click")]
        [KeywordParameters("finding timeout", "Timeout of finding element in seconds")]
        [SampleScript("IOS.Click (//XCUIElementTypeButton[@name='Accept All'],20)")]
        public KeywordResult Click(string xPath, string findingTimeout)
        {
            if (!int.TryParse(findingTimeout.Trim(), out int timeOut))
            {
                return GetFailErrorResult("Timeout must be a number", KeywordResults.Error, false);
            }

            IWebElement element = FindElementWithTimeout(_driver.FindElement, xPath, TimeSpan.FromSeconds(timeOut));
            if (element == null)
            {
                return GetFailErrorResult($"Can not find element with given xPath : {xPath}", KeywordResults.Fail);
            }

            try
            {
                element.Click();
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("socket hang up"))
                {
                    Logger.Error("Socket hang up. Restart WDA");
                    AppiumServerUtils.StopWDA(_dut.LanDebugAddress);
                    if (!ExecuteWDA())
                    {
                        throw new ApplicationException("Web driver agent is crashed during execution.");
                    }
                    element.Click();
                }
                else
                {
                    return GetFailErrorResult("Can not click element", KeywordResults.Error, true, ex);
                }
            }

            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Click Element with name property")]
        [KeywordDisplayName("Click With Name")]
        [KeywordParameters("name", "name property of element to click")]
        [SampleScript("IOS.Click With Name (Hp Smart)")]
        public KeywordResult ClickWithName(string name)
        {
            return Click($"//*[@name='{name}']", "3");
        }

        [KeywordDescription("Click Element with name property")]
        [KeywordDisplayName("Click With Name")]
        [KeywordParameters("name", "name property of element to click")]
        [KeywordParameters("finding timeout", "Timeout of finding element in seconds")]
        [SampleScript("IOS.Click With Name (Hp Smart,30)")]
        public KeywordResult ClickWithName(string name, string timeOut)
        {
            return Click($"//*[@name='{name}']", timeOut);
        }

        [KeywordDescription("Click Element with label property")]
        [KeywordDisplayName("Click With Label")]
        [KeywordParameters("label", "label property of element to click")]
        [SampleScript("IOS.Long Click With Label (Accept All)")]
        public KeywordResult ClickWithLabel(string label)
        {
            return Click($"//*[@label='{label}']", "3");
        }

        [KeywordDescription("Click Element with label property")]
        [KeywordDisplayName("Click With Label")]
        [KeywordParameters("label", "label property of element to click")]
        [KeywordParameters("finding timeout", "Timeout of finding element in seconds")]
        [SampleScript("IOS.Long Click With Label (Accept All,30)")]
        public KeywordResult ClickWithLabel(string label, string timeOut)
        {
            return Click($"//*[@label='{label}']", timeOut);
        }

        [KeywordDescription("Long Click Element with XPath")]
        [KeywordDisplayName("Long Click")]
        [KeywordParameters("xPath", "XPath of element to long click")]
        [SampleScript("IOS.Long Click(//XCUIElementTypeButton[@name='Skip account activation'])")]
        public KeywordResult LongClick(string xPath)
        {
            return LongClick(xPath, "3");
        }

        [KeywordDescription("Long Click Element with XPath")]
        [KeywordDisplayName("Long Click")]
        [KeywordParameters("xPath", "XPath of element to long click")]
        [KeywordParameters("finding timeout", "Timeout of finding element in seconds")]
        [SampleScript("IOS.LongClick(//XCUIElementTypeButton[@name='Skip for now'],10)")]
        public KeywordResult LongClick(string xPath, string findingTimeout)
        {
            if (!int.TryParse(findingTimeout.Trim(), out int timeOut))
            {
                return GetFailErrorResult("Timeout must be a number", KeywordResults.Error, false);
            }

            IWebElement element = FindElementWithTimeout(_driver.FindElement, xPath, TimeSpan.FromSeconds(timeOut));
            if (element == null)
            {
                return GetFailErrorResult($"Can not find element with given xPath : {xPath}", KeywordResults.Fail);
            }

            if (element == null)
            {
                return GetFailErrorResult($"Can not find element with given xPath : {xPath}", KeywordResults.Fail);
            }

            try
            {
                Actions action = new Actions(_driver);
                action.ClickAndHold(element);
                Thread.Sleep(500);
                action.Release().Perform();
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("socket hang up"))
                {
                    Logger.Error("Socket hang up. Restart WDA");
                    AppiumServerUtils.StopWDA(_dut.LanDebugAddress);
                    if (!ExecuteWDA())
                    {
                        throw new ApplicationException("Web driver agent is crashed during execution.");
                    }
                    element.Click();
                }
                else
                {
                    return GetFailErrorResult("Can not long click element", KeywordResults.Error, true, ex);
                }
            }

            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Long Click Element with name property")]
        [KeywordDisplayName("Long Click With Name")]
        [KeywordParameters("name", "name property of element to long click")]
        [SampleScript("IOS.Long Click With Name(Create Account)")]
        public KeywordResult LongClickWithName(string name)
        {
            return LongClick($"//*[@name='{name}']", "3");
        }

        [KeywordDescription("Long Click Element with name property")]
        [KeywordDisplayName("Long Click With Name")]
        [KeywordParameters("name", "name property of element to long click")]
        [KeywordParameters("finding timeout", "Timeout of finding element in seconds")]
        [SampleScript("IOS.Long Click With Name(Create Account,10)")]

        public KeywordResult LongClickWithName(string name, string timeOut)
        {
            return LongClick($"//*[@name='{name}']", timeOut);
        }

        [KeywordDescription("Long Click Element with label property")]
        [KeywordDisplayName("Long Click With Label")]
        [KeywordParameters("label", "label property of element to long click")]
        [SampleScript("IOS.Long Click With Label(New printer)")]

        public KeywordResult LongClickWithLabel(string label)
        {
            return LongClick($"//*[@label='{label}']", "3");
        }

        [KeywordDescription("Long Click Element with label property")]
        [KeywordDisplayName("Long Click With Label")]
        [KeywordParameters("label", "label property of element to long click")]
        [KeywordParameters("finding timeout", "Timeout of finding element in seconds")]
        [SampleScript("IOS.Long Click With Label(New printer,10)")]


        public KeywordResult LongClickWithLabel(string label, string timeOut)
        {
            return LongClick($"//*[@label='{label}']", timeOut);
        }

        [KeywordDescription("Check if element is exist")]
        [KeywordDisplayName("Is Exist")]
        [KeywordParameters("xPath", "XPath of element to find")]
        [SampleScript("IOS.Is Exist (//XCUIElementTypeButton[@name='Continue'])")]
        public KeywordResult IsExist(string xPath)
        {
            return IsExist(xPath, "3");
        }

        [KeywordDescription("Check if element is exist within given timeout")]
        [KeywordDisplayName("Is Exist")]
        [KeywordParameters("xPath", "XPath of element to find")]
        [KeywordParameters("finding timeout", "Timeout of finding element in seconds")]
        [SampleScript("IOS.Is Exist (//XCUIElementTypeButton[@name='Continue'],5)")]
        public KeywordResult IsExist(string xPath, string findingTimeout)
        {
            if (!int.TryParse(findingTimeout.Trim(), out int timeOut))
            {
                return GetFailErrorResult("Timeout must be a number", KeywordResults.Error);
            }

            IWebElement element = FindElementWithTimeout(_driver.FindElement, xPath, TimeSpan.FromSeconds(timeOut));
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
        [SampleScript("IOS.Wait Until Gone (//XCUIElementTypeStaticText[@label='Printables'],30)")]
        public KeywordResult WaitUntilGone(string xPath, string findingTimeOut)
        {
            if (!int.TryParse(findingTimeOut.Trim(), out int timeOut))
            {
                return GetFailErrorResult("Timeout must be a number", KeywordResults.Error);
            }
            bool notExist = CheckElementNotExist((selector) => _driver.FindElement(By.XPath(selector)), xPath, TimeSpan.FromSeconds(timeOut));
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
        [SampleScript("IOS.Set Text(//XCUIElementTypeStaticText[@label='Printables'],Printables)")]
        public KeywordResult SetText(string xPath, string textToSet)
        {
            IWebElement element = FindElementWithTimeout(_driver.FindElement, xPath, TimeSpan.FromSeconds(3));
            if (element == null)
            {
                return GetFailErrorResult($"Element is not exist : {xPath}", KeywordResults.Fail);
            }
            try
            {
                element.SendKeys(textToSet);
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
        [SampleScript("IOS.Set Text(Printables)")]
        public KeywordResult SetText(string textToSet)
        {
            WebElement element = null;
            try
            {
                element = _driver.SwitchTo().ActiveElement() as WebElement;
                if (element == null)
                {
                    return GetFailErrorResult($"Can not find active element", KeywordResults.Error);
                }
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("socket hang up"))
                {
                    Logger.Error("Socket hang up. Restart WDA");
                    AppiumServerUtils.StopWDA(_dut.LanDebugAddress);
                    if (!ExecuteWDA())
                    {
                        throw new ApplicationException("Web driver agent is crashed during execution.");
                    }
                    element = _driver.SwitchTo().ActiveElement() as WebElement;
                }
                else
                {
                    return GetFailErrorResult($"Can not find active element", KeywordResults.Error, true, ex);
                }
            }

            try
            {
                element.SendKeys(textToSet);
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("socket hang up"))
                {
                    Logger.Error("Socket hang up. Restart WDA");
                    AppiumServerUtils.StopWDA(_dut.LanDebugAddress);
                    if (!ExecuteWDA())
                    {
                        throw new ApplicationException("Web driver agent is crashed during execution.");
                    }
                    element.SendKeys(textToSet);
                }
                else
                {
                    return GetFailErrorResult("Error during send keys", KeywordResults.Error, true, ex);
                }

            }
            return new KeywordResult(KeywordResults.Pass);

        }

        [KeywordDescription("Swipe screen with given direction. Directions: Left or < : Swipe Right to Left | Right or > : Swipe Left to Right | Up or ^ : Swipe Down to Up | Down or v: Swipe Up to Down")]
        [KeywordDisplayName("Swipe")]
        [KeywordParameters("direction", "directino to swipe(<, >, ^, v)")]
        [SampleScript("IOS.Swipe(^)")]
        public KeywordResult Swipe(string direction)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            Tuple<int, int> startPoint;
            Tuple<int, int> endPoint;
            direction = direction.Trim();

            switch (direction)
            {
                case "<":
                    startPoint = GetAbsolutePosition(80, 50);
                    endPoint = GetAbsolutePosition(20, 50);
                    break;
                case ">":
                    startPoint = GetAbsolutePosition(20, 50);
                    endPoint = GetAbsolutePosition(80, 50);
                    break;
                case "^":
                    startPoint = GetAbsolutePosition(50, 80);
                    endPoint = GetAbsolutePosition(50, 20);
                    break;
                case "v":
                    startPoint = GetAbsolutePosition(50, 20);
                    endPoint = GetAbsolutePosition(50, 80);
                    break;
                default:
                    return GetFailErrorResult($"Invalid argument of direction :{direction}", KeywordResults.Error, false);
            }

            try
            {

                var actions = new Actions(_driver);
                var pointer = new PointerInputDevice(PointerKind.Touch, "touch");
                var swipe = new ActionSequence(pointer);

                // Starting point of swipe
                swipe.AddAction(pointer.CreatePointerMove(CoordinateOrigin.Viewport, startPoint.Item1, startPoint.Item2, TimeSpan.Zero));
                swipe.AddAction(pointer.CreatePointerDown(MouseButton.Left));

                // Ending point of swipe
                swipe.AddAction(pointer.CreatePointerMove(CoordinateOrigin.Viewport, endPoint.Item1, endPoint.Item2, TimeSpan.FromMilliseconds(500)));
                swipe.AddAction(pointer.CreatePointerUp(MouseButton.Left));

                // Perform the action
                _driver.PerformActions(new List<ActionSequence> { swipe });
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("socket hang up"))
                {
                    Logger.Error("Socket hang up. Restart WDA");
                    AppiumServerUtils.StopWDA(_dut.LanDebugAddress);
                    if (!ExecuteWDA())
                    {
                        throw new ApplicationException("Web driver agent is crashed during execution.");
                    }
                }
                else
                {
                    return GetFailErrorResult($"Error during swipe from ({startPoint.Item1},{startPoint.Item2}) to ({endPoint.Item1}, {endPoint.Item2})", KeywordResults.Error, true, ex);
                }
            }
            return kr;
        }

        [KeywordDescription("Swipe element with given direction. Directions: Left or < : Swipe Right to Left | Right or > : Swipe Left to Right | Up or ^ : Swipe Down to Up | Down or v: Swipe Up to Down")]
        [KeywordDisplayName("Swipe")]
        [KeywordParameters("xPath", "XPath of element to swipe")]
        [KeywordParameters("direction", "directino to swipe(<, >, ^, v)")]
        [SampleScript("IOS.Swipe(//XCUIElementTypeOther[@label='Do more with HP Smart',^)")]
        public KeywordResult Swipe(string xPath, string direction)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            IWebElement element = FindElementWithTimeout(_driver.FindElement, xPath, TimeSpan.FromSeconds(3));
            if (element == null)
            {
                return GetFailErrorResult("Can not find element", KeywordResults.Fail, true);
            }

            Tuple<int, int> startPoint;
            Tuple<int, int> endPoint;
            direction = direction.Trim();
            switch (direction)
            {
                case "<":
                    startPoint = GetAbsolutePosition(80, 50);
                    endPoint = GetAbsolutePosition(20, 50);
                    break;
                case ">":
                    startPoint = GetAbsolutePosition(20, 50);
                    endPoint = GetAbsolutePosition(80, 50);
                    break;
                case "^":
                    startPoint = GetAbsolutePosition(50, 80);
                    endPoint = GetAbsolutePosition(50, 20);
                    break;
                case "v":
                    startPoint = GetAbsolutePosition(50, 20);
                    endPoint = GetAbsolutePosition(50, 80);
                    break;
                default:
                    return GetFailErrorResult($"Invalid argument of direction :{direction}", KeywordResults.Error, false);
            }

            try
            {
                var actions = new Actions(_driver);
                var pointer = new PointerInputDevice(PointerKind.Touch, "touch");
                var swipe = new ActionSequence(pointer);

                // Starting point of swipe
                swipe.AddAction(pointer.CreatePointerMove(CoordinateOrigin.Viewport, startPoint.Item1, startPoint.Item2, TimeSpan.Zero));
                swipe.AddAction(pointer.CreatePointerDown(MouseButton.Left));

                // Ending point of swipe
                swipe.AddAction(pointer.CreatePointerMove(CoordinateOrigin.Viewport, endPoint.Item1, endPoint.Item2, TimeSpan.FromMilliseconds(500)));
                swipe.AddAction(pointer.CreatePointerUp(MouseButton.Left));

                // Perform the action
                _driver.PerformActions(new List<ActionSequence> { swipe });
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("socket hang up"))
                {
                    Logger.Error("Socket hang up. Restart WDA");
                    AppiumServerUtils.StopWDA(_dut.LanDebugAddress);
                    if (!ExecuteWDA())
                    {
                        throw new ApplicationException("Web driver agent is crashed during execution.");
                    }
                }
                else
                {
                    return GetFailErrorResult($"Error during swipe from ({startPoint.Item1},{startPoint.Item2}) to ({endPoint.Item1}, {endPoint.Item2})", KeywordResults.Error, true, ex);
                }
            }
            return kr;
        }

        [KeywordDescription("Scroll to element with given direction. Directions: Left or < : Swipe Right to Left | Right or > : Swipe Left to Right | Up or ^ : Swipe Down to Up | Down or v: Swipe Up to Down")]
        [KeywordDisplayName("Scroll To")]
        [KeywordParameters("xPath", "xPath of element")]
        [KeywordParameters("direction", "directino to swipe(<, >, ^, v)")]
        [SampleScript("IOS.Scroll To(//XCUIElementTypeOther[@label='Do more with HP Smart',^)")]
        public KeywordResult ScrollTo(string xPath, string direction)
        {
            IWebElement element = FindElementWithTimeout(_driver.FindElement, xPath, TimeSpan.FromSeconds(0));

            int swipeCount = 0;
            while (element == null || !element.Displayed)
            {
                KeywordResult swipeResult = Swipe(direction);
                swipeCount++;
                if (!swipeResult.Result.Equals(KeywordResults.Pass))
                {
                    return swipeResult;
                }
                element = FindElementWithTimeout(_driver.FindElement, xPath, TimeSpan.FromSeconds(0));
                if (swipeCount > 9)
                {
                    return GetFailErrorResult("Can not find element within 10 swipe", KeywordResults.Fail);
                }
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Scroll to element with given direction. Directions: Left or < : Swipe Right to Left | Right or > : Swipe Left to Right | Up or ^ : Swipe Down to Up | Down or v: Swipe Up to Down")]
        [KeywordDisplayName("Scroll To")]
        [KeywordParameters("toScrollXPath", "XPath of element to scroll")]
        [KeywordParameters("toFindXPath", "XPath of element to find")]
        [KeywordParameters("MaxScrollCount", "Maximum number of scroll")]
        [KeywordParameters("direction", "directino to swipe(<, >, ^, v)")]
        [SampleScript("IOS.Scroll To(//XCUIElementTypeOther[@label='Do more with HP Smart',^,50)")]

        public KeywordResult ScrollTo(string toScrollXPath, string toFindXPath, string direction, string maxScrollCount)
        {
            if (!int.TryParse(maxScrollCount, out int maxCount))
            {
                return GetFailErrorResult("Max Scroll Count must be a number", KeywordResults.Error, false);
            }
            IWebElement element = FindElementWithTimeout(_driver.FindElement, toFindXPath, TimeSpan.FromSeconds(2));

            int swipeCount = 0;
            maxCount--;
            while (element == null || !element.Displayed)
            {
                KeywordResult swipeResult = Swipe(toScrollXPath, direction);
                swipeCount++;
                if (!swipeResult.Result.Equals(KeywordResults.Pass))
                {
                    return swipeResult;
                }
                element = FindElementWithTimeout(_driver.FindElement, toFindXPath, TimeSpan.FromSeconds(0));
                if (swipeCount > maxCount)
                {
                    return GetFailErrorResult("Can not find element within 10 swipe", KeywordResults.Fail);
                }
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Capture Screen Shot of device and save to PC")]
        [KeywordDisplayName("Capture Screen Shot")]
        [KeywordParameters("filename", "filename to save")]
        [SampleScript("IOS.Capture Screen Shot (LaunchHPSmart)")]
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
                if (ex.ToString().Contains("socket hang up"))
                {
                    Logger.Error("Socket hang up. Restart WDA");
                    AppiumServerUtils.StopWDA(_dut.LanDebugAddress);
                    if (!ExecuteWDA())
                    {
                        throw new ApplicationException("Web driver agent is crashed during execution.");
                    }
                }

                return GetFailErrorResult("Error during capture screen shot", KeywordResults.Error, false, ex);

            }
        }

        [KeywordDescription("Allow iOS device Alert")]
        [KeywordDisplayName("Click Allow")]
        [KeywordParameters("finding timeout", "Timeout of finding Alert in seconds")]
        [SampleScript("IOS.Click Allow (10)")]

        public KeywordResult ClickAllow(string findingTimeout)
        {
            if (!int.TryParse(findingTimeout.Trim(), out int timeOut))
            {
                return GetFailErrorResult("Timeout must be a number", KeywordResults.Error);
            }
            try
            {

                IAlert alert = _driver.SwitchTo().Alert();
                Thread.Sleep(TimeSpan.FromSeconds(timeOut));
                alert.Accept();
            }
            catch
            {
                return GetFailErrorResult("Alert not in present", KeywordResults.Fail, false);

            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Dismiss iOS device Alert")]
        [KeywordDisplayName("Click Dismiss")]
        [KeywordParameters("finding timeout", "Timeout of finding Alert in seconds")]
        [SampleScript("IOS.Click Dismiss (10)")]

        public KeywordResult ClickDismiss(string findingTimeout)
        {
            if (!int.TryParse(findingTimeout.Trim(), out int timeOut))
            {
                return GetFailErrorResult("Timeout must be a number", KeywordResults.Error);
            }
            try
            {
                IAlert alert = _driver.SwitchTo().Alert();
                Thread.Sleep(TimeSpan.FromSeconds(timeOut));
                alert.Dismiss();
            }
            catch
            {
                return GetFailErrorResult("Alert not in present", KeywordResults.Fail, false);

            }
            return new KeywordResult(KeywordResults.Pass);
        }


        [KeywordDescription("Dismiss iOS device KeyBoard Alert")]
        [KeywordDisplayName("Click Done")]
        [SampleScript("IOS.Click Done")]
        public KeywordResult ClickDone()
        {
            bool isKeyboardShown = _driver.IsKeyboardShown();
            string direction = "v";
            if (isKeyboardShown)
            {
                Tuple<int, int> startPoint;
                Tuple<int, int> endPoint;
                direction = direction.Trim();
                startPoint = GetAbsolutePosition(80, 50);
                endPoint = GetAbsolutePosition(20, 50);
                try
                {
                    var pointer = new PointerInputDevice(PointerKind.Touch, "touch");
                    var tapAction = new ActionSequence(pointer, 0);
                    tapAction.AddAction(pointer.CreatePointerMove(CoordinateOrigin.Viewport, startPoint.Item1, startPoint.Item2, TimeSpan.Zero));
                    tapAction.AddAction(pointer.CreatePointerUp(MouseButton.Left));
                    tapAction.AddAction(pointer.CreatePointerDown(MouseButton.Left));
                    _driver.PerformActions(new List<ActionSequence> { tapAction });
                }
                catch (Exception ex)
                {
                    return GetFailErrorResult("KeyBoard not in present", KeywordResults.Fail, false);
                }
            }
            return new KeywordResult(KeywordResults.Pass);

        }

        [KeywordDescription("Remove the  iOS applicatio from the device")]
        [KeywordDisplayName("Remove App")]
        [SampleScript("IOS.Remove App (com.hp.printer.control)")]
        public KeywordResult RemoveApp(string appBundleId)
        {
            _capabilites.AddAdditionalAppiumOption("appium:bundleId", appBundleId);
            try
            {
                string bundleid = appBundleId;
                _driver.RemoveApp(bundleid);
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("App not in present", KeywordResults.Fail, false);

            }
            return new KeywordResult(KeywordResults.Pass);

        }

        [KeywordDescription("Dismiss iOS device Alert")]
        [KeywordDisplayName("Allow Once")]
        [KeywordParameters("finding timeout", "Timeout of finding Alert in seconds")]
        [SampleScript("IOS.Allow Once(5)")]

        public KeywordResult AllowOnce(string findingTimeout)
        {
            if (!int.TryParse(findingTimeout.Trim(), out int timeOut))
            {
                return GetFailErrorResult("Timeout must be a number", KeywordResults.Error);
            }
            try
            {
                IAlert alert = _driver.SwitchTo().Alert();
                Thread.Sleep(TimeSpan.FromSeconds(timeOut));
                alert.Dismiss();
            }
            catch
            {
                return GetFailErrorResult("Alert not in present", KeywordResults.Fail, false);

            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Click bottom left position of element")]
        [KeywordDisplayName("List Scroll")]
        [KeywordParameters("xPath", "XPath of element")]
        [SampleScript("IOS.List Scroll (//XCUIElementTypeOther[@name='Wyoming'])")]
        public KeywordResult ListScroll(string xPath)
        {
            IWebElement element = FindElementWithTimeout(_driver.FindElement, xPath, TimeSpan.FromSeconds(3));
            if (element == null)
            {
                return GetFailErrorResult($"Can not find element with given xPath : {xPath}", KeywordResults.Fail);
            }

            try
            {
                element.SendKeys(Keys.ArrowDown);
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Can not find the element", KeywordResults.Fail, false);
            }
            return new KeywordResult(KeywordResults.Pass);
        }
        [KeywordDescription("Clear Text From The Text Field")]
        [KeywordDisplayName("Clear Text")]
        [KeywordParameters("xPath", "xPath of text field ")]
        [SampleScript("IOS.Clear Text(//XCUIElementTypeTextField[@identifier='WEB_BROWSER_ADDRESS_AND_SEARCH_FIELD'])")]
        public KeywordResult ClearText(string xPath)
        {
            IWebElement element = FindElementWithTimeout(_driver.FindElement, xPath, TimeSpan.FromSeconds(3));
            try
            {
                if (element == null)
                {
                    return GetFailErrorResult($"Element is not exist : {xPath}", KeywordResults.Fail);
                }
                element.Clear();
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Error during Clearing the text", KeywordResults.Error, true, ex);
            }
            return new KeywordResult(KeywordResults.Pass);
        }
        [KeywordDescription("Clear Text From The Text Field")]
        [KeywordDisplayName("Clear Text")]
        [SampleScript("IOS.Clear Text")]
        public KeywordResult ClearText()
        {
            WebElement element = null;
            try
            {
                element = _driver.SwitchTo().ActiveElement() as WebElement;
                if (element == null)
                {
                    return GetFailErrorResult($"Can not find active element", KeywordResults.Error);
                }
                element.Clear();
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Error during Clearing the text", KeywordResults.Error, true, ex);
            }
            return new KeywordResult(KeywordResults.Pass);
        }
        [KeywordDescription("Scrolls the screen up or down for a given number of steps on iOS native apps or web/webview.")]
        [KeywordDisplayName("Swipe with Steps")]
        [KeywordParameters("Directions", "Direction of the swipe")]
        [KeywordParameters("steps", "num of steps to swipe")]
        [SampleScript("IOS.Swipe with Steps(^,5)")]
        public KeywordResult SwipewithSteps(string direction, string steps)
        {
            try
            {
                if (!int.TryParse(steps, out int parsedSteps) || parsedSteps <= 0)
                    parsedSteps = 1;

                direction = direction?.Trim().ToLowerInvariant() ?? "v";

                for (int i = 0; i < parsedSteps; i++)
                {
                    if (_driver == null)
                        return GetFailErrorResult("Driver instance is null.", KeywordResults.Fail, false);

                    try
                    {
                        var context = _driver.Context;

                        if (!string.IsNullOrEmpty(context) &&
                            context.StartsWith("WEBVIEW", StringComparison.OrdinalIgnoreCase))
                        {
                            var js = (IJavaScriptExecutor)_driver;
                            int distance = direction == "^" ? -150 : 150;
                            js.ExecuteScript($"window.scrollBy(0, {distance});");
                        }
                        else
                        {
                            var size = _driver.Manage().Window.Size;
                            int startX = size.Width / 2;
                            int startY, endY;

                            if (direction == "^")
                            {
                                startY = (int)(size.Height * 0.45);
                                endY = (int)(size.Height * 0.65);
                            }
                            else
                            {
                                startY = (int)(size.Height * 0.65);
                                endY = (int)(size.Height * 0.45);
                            }

                            var finger = new PointerInputDevice(PointerKind.Touch, "finger1");
                            var sequence = new ActionSequence(finger, 0);

                            sequence.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, startX, startY, TimeSpan.Zero));
                            sequence.AddAction(finger.CreatePointerDown(PointerButton.TouchContact));
                            sequence.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, startX, endY, TimeSpan.FromMilliseconds(200)));
                            sequence.AddAction(finger.CreatePointerUp(PointerButton.TouchContact));

                            _driver.PerformActions(new List<ActionSequence> { sequence });
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug($"Scroll attempt failed: {ex.Message}");

                    }
                    Thread.Sleep(150);
                }

                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                return GetFailErrorResult($"Scroll {direction} failed: {ex.Message}", KeywordResults.Fail, false);
            }
        }
        [KeywordDescription("Runs the application in the background for a specified amount of time.")]
        [KeywordDisplayName("Background Run App")]
        [KeywordParameters("time", "The amount of time (in seconds) to run the application in the background.")]
        [SampleScript("IOS.Background Run App(30)")]
        public KeywordResult BackgroundRunApp(string time)
        {
            try
            {
                _driver.BackgroundApp(TimeSpan.FromSeconds(double.Parse(time)));
                return new KeywordResult(KeywordResults.Pass, "The iOS application has been moved to the background.");
            }
            catch (Exception ex)
            {
                return new KeywordResult(KeywordResults.Fail, ex.Message);
            }
        }


        [KeywordDescription("Closes the currently running iOS application.")]
        [KeywordParameters("BundleId", "The bundle identifier of the iOS application to close.")]
        [KeywordDisplayName("Close App")]
        [SampleScript("IOS.Close App(com.hp.SmartMac)")]
        public KeywordResult CloseApp(string BundleId)
        {
            try
            {
                string bundleId = BundleId;
                _driver.TerminateApp(bundleId);
                return new KeywordResult(KeywordResults.Pass, "iOS App closed successfully");
            }
            catch (Exception ex)
            {
                return new KeywordResult(KeywordResults.Fail, ex.Message);
            }
        }

        [KeywordDescription("Reopen the background running iOS application.")]
        [KeywordParameters("BundleId", "The bundle identifier of the iOS application to close.")]
        [KeywordDisplayName("Reopen App")]
        [SampleScript("IOS.Reopen    App(com.hp.SmartMac)")]
        public KeywordResult ReopenApp(string BundleId)
        {
            try
            {
                string bundleId = BundleId;
                _driver.ActivateApp(bundleId);
                return new KeywordResult(KeywordResults.Pass, "iOS App reopened successfully");
            }
            catch (Exception ex)
            {
                return new KeywordResult(KeywordResults.Fail, ex.Message);
            }
        }
    }
}


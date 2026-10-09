using HP.GFriend.GFLogger;
using HP.GFriend.Keywords.Properties;
using HP.GFriend.Utils.Appium;
using HP.GFriend.Utils.Charter;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Android.Enums;
using OpenQA.Selenium.Appium.Mac;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Internal;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace HP.GFriend.Keywords
{

    [LibraryDescription("ADB (Android Debug Bridge) should be enabled to use Android library")]
    public class Android2 : IGFLibrary
    {
        private AndroidDriver _driver;
        public static string PromptName { get; private set; }
        public string _testCaseName = "";

        private DeviceUnderTest _dut;
        private AppiumOptions _capabilites;
        private static ConnectionInfo _connectionInfo;
        public string PackageName;
        private string _outputDir;
        private int _scaleFactor = 1;
        private int _displayWidth;
        private int _displayHeight;
        private int currentActivity;
        private static MemoryUsageItem _memoryUsage = null;
        private static Dictionary<string, MemoryUsageItem> _pkgMemoryUsage;

        public static string HostName { get; private set; }
        public static int Port { get; private set; }
        public static string UserName { get; private set; }
        private static string UserPassword { get; set; }

        private static SshClient _sshClient;
        private Ssh _ssh;

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
            return null;
        }

        public string GetName()
        {
            return "Android2";
        }

        public void Initialize(DeviceUnderTest dut, string outputDir)
        {
            _outputDir = outputDir;
            _dut = dut;
            HostName = _dut.DeviceAddress;
            Port = _dut.Port;
            UserName = _dut.AdminId;
            UserPassword = _dut.AdminPassword;

            // Check dut if it has desired capabilites
            if (string.IsNullOrEmpty(_dut.LanDebugAddress) ||
                string.IsNullOrEmpty(_dut.AdminId) ||
                string.IsNullOrEmpty(_dut.AdminPassword) ||
                _dut.Port == 0)
            {
                throw new LibraryInitializeException("Appium Server's Address, port, user id and password should be given in LanDeubAddress, Port, Admin ID and Password field in device information.");
            }

            _capabilites = new AppiumOptions();
            _capabilites.PlatformName = "Android";
            _capabilites.AutomationName = "UiAutomator2";
            _capabilites.DeviceName = _dut.LanDebugAddress;
            _capabilites.AddAdditionalAppiumOption("appium:noReset", true);
            _capabilites.AddAdditionalAppiumOption("appium:fullReset", false);
            _capabilites.AddAdditionalAppiumOption("appium:autoGrantPermissions", true);
            _capabilites.AddAdditionalAppiumOption("appium:newCommandTimeout", 6000);
            _capabilites.AddAdditionalAppiumOption("appium:appWaitActivity", "*");
            _capabilites.AddAdditionalAppiumOption("appium:appWaitDuration", 30000);
            _capabilites.AddAdditionalAppiumOption("appium:appWaitForLaunch", true);
            _capabilites.AddAdditionalAppiumOption("appium:noSign", true);
            _capabilites.AddAdditionalAppiumOption("appium:autoLaunch", true);
            _capabilites.AddAdditionalAppiumOption("appium:mockLocationApp", null);
            _capabilites.AddAdditionalAppiumOption("appium:ignoreHiddenApiPolicyError", true);
            _capabilites.AddAdditionalAppiumOption("appium:skipDeviceInitialization", true);
            _capabilites.AddAdditionalAppiumOption("appium:skipServerInstallation", true);
        }
        private IWebElement FindElementByAndroidUIAutomator(string selector)
        {
            return _driver.FindElement(AppiumByCompat.AndroidUIAutomator(selector));
        }
        internal static class AppiumByCompat
        {
            // If you define APPIUM_V5_OR_LATER in your project build symbols once you upgrade,
            // this will switch to the new AppiumBy API. Otherwise it falls back to MobileBy (older driver).
            public static By AndroidUIAutomator(string selector)
            {
#if APPIUM_V5_OR_LATER
                return OpenQA.Selenium.Appium.AppiumBy.AndroidUIAutomator(selector);
#else
                return OpenQA.Selenium.Appium.MobileBy.AndroidUIAutomator(selector);
#endif
            }
        }
        // Helper for XPath
        private IWebElement FindElementByXPath(string xPath)
        {
            return _driver.FindElement(By.XPath(xPath));
        }

        // Helper for resource-id (id)
        private IWebElement FindElementById(string id)
        {
            return _driver.FindElement(By.Id(id));
        }

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

        [KeywordDescription("Launch App (or select app if app is already launched) with given bundle id of Mac Application")]
        [KeywordDisplayName("Launch App")]
        [KeywordParameters("App Package", "Package name app to launch")]
        [SampleScript("Android2.Launch(com.hp.printercontrol)")]
        public KeywordResult LaunchApp(string appPackage)
        {
            _capabilites.AddAdditionalAppiumOption("appium:appPackage", appPackage);
            PackageName = appPackage;
            if (_driver != null)
            {
                _driver.Quit();
                _driver.Dispose();
                _driver = null;
            }
            try
            {

                _driver = new AndroidDriver(new Uri($"http://{_dut.DeviceAddress}:" +
                 $"{_dut.Port}/wd/hub"), _capabilites);
                CommonExecutionInfo.SetSharedObject("Android2:" + _dut.DeviceId, _driver);
                _driver.ActivateApp(PackageName);

            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Error during connect to Appium server.", KeywordResults.Error, false, ex);
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Capture Screen Shot of device and save to PC")]
        [KeywordDisplayName("Capture Screen Shot")]
        [KeywordParameters("filename", "filename to save")]
        [SampleScript("Android2.Capture Screen Shot(${ss})")]
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
        private byte[] GetScreenCapture()
        {
            try
            {
                byte[] screenCapture = _driver.GetScreenshot().AsByteArray;

                return screenCapture;
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

        [KeywordDescription("Wait given text is appeared on screen until given timeout (seconds)")]
        [KeywordDisplayName("Wait For Text")]
        [KeywordParameters("text", "Wait to appeared specific text")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("Android2.Wait For Text (OK,5)")]
        public KeywordResult WaitForText(string text, string timeout)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            var waitTime = Convert.ToInt32(timeout);

            try
            {
                WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(waitTime));
                wait.Until(driver =>
                {
                    try
                    {
                        IWebElement element = FindElementByAndroidUIAutomator($"new UiSelector().text(\"{text}\")");
                        return element != null && element.Displayed;
                    }
                    catch
                    {
                        return false;
                    }
                });
            }
            catch
            {
                return GetFailErrorResult("Element not found", KeywordResults.Error, false);
            }

            return kr;
        }

        [KeywordDescription("Touch object with given text")]
        [KeywordDisplayName("Touch Text")]
        [KeywordParameters("text", "Text to touch")]
        [SampleScript("Android2.Touch Text (OK)")]
        public KeywordResult TouchText(string text)
        {
            text = text.Replace("(", @"\(").Replace(")", @"\)");
            try
            {
                IWebElement element = FindElementByAndroidUIAutomator($"new UiSelector().text(\"{text}\")");

                if (element == null)
                {
                    return GetFailErrorResult($"Element is not exist : {text}", KeywordResults.Fail);
                }
                element.Click();
            }
            catch (Exception ex)
            {
                return GetFailErrorResult($"Can not find element with given text : {text}", KeywordResults.Error);
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Touch object in WebView.\r\nThis Keyword should be called between Get Webview and Return Webview")]
        [KeywordDisplayName("Touch Web Object")]
        [KeywordParameters("xPath", "XPATH to touch")]
        [SampleScript("Android2.Touch Web Object (//android.widget.TextView[@text='Printables']")]
        public KeywordResult TouchWebObject(string xPath)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                IWebElement element = FindElementWithTimeout(FindElementByXPath, xPath, TimeSpan.FromSeconds(5));
                if (element == null)
                {
                    return GetFailErrorResult($"Element is not exist : {xPath}", KeywordResults.Fail);

                }
                element.Click();
            }
            catch (Exception ex)
            {
                return GetFailErrorResult($"Can not find element with given xPath : {xPath}", KeywordResults.Error);
            }
            return kr;
        }


        [KeywordDescription("Check the screen contains given text (exact match)")]
        [KeywordDisplayName("Check Screen Contains Full Text")]
        [KeywordParameters("text", "Text to check screen contains given text")]
        [SampleScript("Android2.Check Screen Contains Full Text (Exit)")]
        public KeywordResult CheckScreenContainsFullText(string text)
        {
            text = text.Replace("(", @"\(").Replace(")", @"\)");
            try
            {
                IWebElement element = FindElementByAndroidUIAutomator($"new UiSelector().text(\"{text}\")");
                if (element == null)
                {
                    return GetFailErrorResult($"Element is not exist : {text}", KeywordResults.Fail);

                }
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Can not find the element", KeywordResults.Error, false);

            }
            return new KeywordResult(KeywordResults.Pass);
        }
        [KeywordDescription("Check the screen partially contains given text")]
        [KeywordDisplayName("Check Screen Contains Partial Text")]
        [KeywordParameters("text", "Text to check screen contains given text")]
        [SampleScript("Android2.Check Screen Contains Partial Text (Service busy.)")]
        public KeywordResult CheckScreenContainsPartialText(string text)
        {
            text = text.Replace("(", @"\(").Replace(")", @"\)");
            try
            {
                IWebElement element = FindElementByAndroidUIAutomator($"new UiSelector().textContains(\"{text}\")");
                if (element == null)
                {
                    return GetFailErrorResult($"Element is not exist : {text}", KeywordResults.Fail);
                }
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Can not find the element", KeywordResults.Error, false);

            }
            return new KeywordResult(KeywordResults.Pass);
        }
        [KeywordDescription("Check the screen does not contains given text (exact match)")]
        [KeywordDisplayName("Check Screen Not Contains Full Text")]
        [KeywordParameters("text", "Text to check screen does not contains given text")]
        [SampleScript("Android2.Check Screen Not Contains Full Text (Exit)")]
        public KeywordResult CheckScreenNotContainsFullText(string text)
        {
            text = text.Replace("(", @"\(").Replace(")", @"\)");
            try
            {
                var elements = _driver.FindElements(new ByAndroidUIAutomator($"new UiSelector().textContains(\"{text}\")"));
                if (elements.Count == 0)
                {
                    Logger.Debug($"Text '{text}' is NOT present on screen.");
                    return new KeywordResult(KeywordResults.Pass);
                }
                else
                {
                    Logger.Debug($"Text '{text}' is present on screen.");
                    return new KeywordResult(KeywordResults.Fail);
                }
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Can not find the element text", KeywordResults.Error, false);

            }
            return new KeywordResult(KeywordResults.Pass);
        }
        [KeywordDescription("Check the screen does not contains any partial given text")]
        [KeywordDisplayName("Check Screen Not Contains Partial Text")]
        [KeywordParameters("text", "Text to check screen does not contains any partial given text")]
        [SampleScript("Android2.Check Screen Not Contains Partial Text (Exit)")]
        public KeywordResult CheckScreenNotContainsPartialText(string text)
        {
            text = text.Replace("(", @"\(").Replace(")", @"\)");
            try
            {
                var elements = _driver.FindElements(new ByAndroidUIAutomator($"new UiSelector().textContains(\"{text}\")"));
                if (elements.Count == 0)
                {
                    Logger.Debug($"Text '{text}' is NOT present on screen.");
                    return new KeywordResult(KeywordResults.Pass);
                }
                else
                {
                    Logger.Debug($"Text '{text}' is present on screen.");
                    return new KeywordResult(KeywordResults.Fail);
                }
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Can not find the element", KeywordResults.Error, false);
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Touch object with given text")]
        [KeywordDisplayName("Click")]
        [KeywordParameters("text", "Text to touch")]
        [SampleScript("Android2.Click (OK)")]
        public KeywordResult Click(string text)
        {
            try
            {
                IWebElement element = FindElementByAndroidUIAutomator($"new UiSelector().text(\"{text}\")");
                if (element == null)
                {
                    return GetFailErrorResult($"Element is not exist : {text}", KeywordResults.Fail);
                }
                element.Click();

            }
            catch (Exception ex)
            {
                return GetFailErrorResult($"Can not find element with given text : {text}", KeywordResults.Error);
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Hide Android's virtual keyboard")]
        [KeywordDisplayName("Hide Keyboard")]
        [SampleScript("Android2.Hide Keyboard ")]
        public KeywordResult HideKeyboard()
        {
            try
            {
                bool isKeyboardShown = _driver.IsKeyboardShown();
                if (isKeyboardShown)
                {
                    _driver.HideKeyboard();
                }
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("KeyBoard not present", KeywordResults.Error, false);
            }

            return new KeywordResult(KeywordResults.Pass);
        }

        private Tuple<int, int> GetAbsolutePosition(string x, string y)
        {
            try
            {
                double dX = ((int.Parse(x)) / (double)100);
                double dY = ((int.Parse(y)) / (double)100);

                int aX = (int)(_displayWidth * dX);
                int aY = (int)(_displayHeight * dY);

                return new Tuple<int, int>(aX, aY);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [KeywordDescription("Check if web object is exist in the current screen")]
        [KeywordDisplayName("Is Web Object Exist")]
        [KeywordParameters("xPath", "XPATH to find")]
        [SampleScript("Android2.Is Web Object Exist (//android.widget.TextView[@text='Shortcuts')")]
        public KeywordResult IsWebObjectExist(string xPath)
        {
            IWebElement element = FindElementWithTimeout(FindElementByXPath, xPath, TimeSpan.FromSeconds(5));
            if (element == null)
            {
                return GetFailErrorResult($"Element is not exist : {xPath}", KeywordResults.Fail);
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Remove the  Android application from the device")]
        [KeywordDisplayName("Remove App")]
        [SampleScript("Android2.Remove App (com.hp.printer.control)")]
        public KeywordResult RemoveApp(string packageName)
        {
            try
            {
                string appPackageName = packageName;
                _driver.RemoveApp(appPackageName);
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("App not in present", KeywordResults.Error, false);
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Disconnect from device")]
        [KeywordDisplayName("Disconnect")]
        [SampleScript("Android2.Disconnect")]
        public KeywordResult Disconnect()
        {
            try
            {
                Dispose();
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                KeywordResult kr = new KeywordResult(KeywordResults.Error);
                kr.AdditionalInfo = ex.ToString();
                kr.Output = "Error during disconnect";
                return kr;
            }
        }

        [KeywordDescription("Touch Object by given resourceid")]
        [KeywordDisplayName("Touch ID")]
        [KeywordParameters("resourceid", "resourceid to touch")]
        [SampleScript("Android2.Touch ID (com.hp.printercontrol:id/addPrinterButton)")]
        public KeywordResult TouchID(string resourceid)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                IWebElement element = FindElementWithTimeout(FindElementById, resourceid, TimeSpan.FromSeconds(10));
                if (element != null)
                {
                    return GetFailErrorResult($"Element is not exist : {resourceid}", KeywordResults.Fail);
                }
                element.Click();

            }
            catch (Exception ex)
            {
                return GetFailErrorResult("can not find the Element", KeywordResults.Error, false);
            }

            return kr;
        }

        [KeywordDescription("Long touch item by given text")]
        [KeywordDisplayName("Touch Long Text")]
        [KeywordParameters("text", "text to long touch")]
        [SampleScript("Android2.Touch Long Text(Exit)")]
        public KeywordResult TouchLongText(string text)
        {
            text = text.Replace("(", @"\(").Replace(")", @"\)");
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            try
            {
                IWebElement element = FindElementByAndroidUIAutomator($"new UiSelector().text(\"{text}\")");
                if (element == null)
                {
                    return GetFailErrorResult($"Element is not exist : {text}", KeywordResults.Fail);
                }
                element.Click();
                Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("can not find the Element", KeywordResults.Error, false);
            }

            return kr;
        }

        [KeywordDescription("Touch with position center of the element using x,y co-ordinates")]
        [KeywordDisplayName("Touch Position")]
        [KeywordParameters("x", "x percent value to drag with given position value")]
        [KeywordParameters("y", "y percent value to drag with given position value")]
        [SampleScript("Android2.Touch Position (8,65)")]
        public KeywordResult TouchPosition(string x, string y)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            try
            {
                // Example X, Y
                int X = Convert.ToInt32(x);
                int Y = Convert.ToInt32(y);

                // Replace the problematic line with the correct instantiation of PointerInputDevice.
                var finger = new PointerInputDevice(PointerKind.Touch);

                // Build touch sequence
                var sequence = new ActionSequence(finger, 0);
                sequence.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, X, Y, TimeSpan.Zero));
                sequence.AddAction(finger.CreatePointerDown(MouseButton.Left));
                sequence.AddAction(finger.CreatePointerUp(MouseButton.Left));

                // Perform
                _driver.PerformActions(new List<ActionSequence> { sequence });

            }
            catch (Exception ex)
            {
                return new KeywordResult(KeywordResults.Fail);
            }

            return kr;
        }

        [KeywordDescription("Check given text is visible or not in current screen")]
        [KeywordDisplayName("Is Text Exists")]
        [KeywordParameters("text", "text to check")]
        [SampleScript("Android2.Is Text Exists (France)")]
        public KeywordResult IsTextExists(string text)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            try
            {
                IWebElement element = FindElementByAndroidUIAutomator($"new UiSelector().text(\"{text}\")");

                if (!element.Enabled || element == null)
                {
                    return GetFailErrorResult($"Text is not exist : {text}", KeywordResults.Fail);
                }
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Element is not in Exits", KeywordResults.Error, false);

            }
            return kr;
        }

        [KeywordDisplayName("Swipe")]
        [KeywordParameters("direction", "Should be one of <, <<, >, >>, ^, ^^, v, vv\r\n" +
     "< (right to lefet with small amount), << (right to left with large amount) \r\n" +
     "> (left to right with small amount), >> (left to right with large amount) \r\n" +
     "^ (down to up with small amount), ^^ (down to up with larget amount) \r\n" +
     "v (up to down with small amount), vv (up to down with larget amount)")]
        [SampleScript("Android2.Swipe (^)")]
        public KeywordResult Swipe(string direction)
        {
            int count = 0;
            try
            {
                switch (direction)
                {
                    case "<":
                        FindElementByAndroidUIAutomator(
                            "new UiScrollable(new UiSelector().scrollable(true)).setAsHorizontalList().flingBackward()");

                        break;
                    case ">":
                        FindElementByAndroidUIAutomator(
                        "new UiScrollable(new UiSelector().scrollable(true)).setAsHorizontalList().flingForward()");

                        break;
                    case "<<":
                        FindElementByAndroidUIAutomator(
                            "new UiScrollable(new UiSelector().scrollable(true)).setAsHorizontalList().flingBackward()");

                        break;
                    case ">>":
                        FindElementByAndroidUIAutomator(
                        "new UiScrollable(new UiSelector().scrollable(true)).setAsHorizontalList().flingForward()");

                        break;
                    case "^":
                        //FindElementByAndroidUIAutomator(
                        //"new UiScrollable(new UiSelector().scrollable(true)).flingBackward()");
                        FindElementByAndroidUIAutomator(
                             "new UiScrollable(new UiSelector().scrollable(true)).scrollBackward()");
                        break;
                    case "v":
                        //FindElementByAndroidUIAutomator(
                        //"new UiScrollable(new UiSelector().scrollable(true)).flingForward()");
                        FindElementByAndroidUIAutomator(
                            "new UiScrollable(new UiSelector().scrollable(true)).scrollForward()");
                        break;
                    case "^^":
                        //FindElementByAndroidUIAutomator(
                        //"new UiScrollable(new UiSelector().scrollable(true)).flingBackward()");
                        while (count < 2)
                        {
                            FindElementByAndroidUIAutomator(
                                 "new UiScrollable(new UiSelector().scrollable(true)).scrollBackward()");
                            count++;
                        }
                        break;
                    case "vv":
                        //FindElementByAndroidUIAutomator(
                        //"new UiScrollable(new UiSelector().scrollable(true)).flingForward()");
                        while (count < 2)
                        {
                            FindElementByAndroidUIAutomator(
                            "new UiScrollable(new UiSelector().scrollable(true)).scrollForward()");
                            count++;


                        }
                        break;
                    default:
                        return GetFailErrorResult($"Invalid argument of direction :{direction}", KeywordResults.Fail, false);
                }

            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Element is not in Exits", KeywordResults.Error, false);

            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Input Text to focused object")]
        [KeywordDisplayName("Input Text")]
        [KeywordParameters("text", "Text to Input")]
        [SampleScript("Android2.Input Text (${DeviceAdminPW})")]
        public KeywordResult InputText(string text)
        {
            IWebElement element = null;

            element = _driver.SwitchTo().ActiveElement() as IWebElement;

            try
            {
                if (element == null)
                {
                    return GetFailErrorResult("Element is not exits", KeywordResults.Fail);
                }

                element.SendKeys(text);

            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Error during send keys", KeywordResults.Error, true, ex);
            }
            return new KeywordResult(KeywordResults.Pass);
        }


        [KeywordDescription("Input Text to element with XPath")]
        [KeywordDisplayName("Input Text With XPath")]
        [KeywordParameters("xPath", "XPath of element to Input")]
        [KeywordParameters("text", "Text to Input")]
        [SampleScript("Android2.Input Text With XPath (//android.view.View[@resource-id='search-disable-click'],u1110)")]
        public KeywordResult InputTextWithXPath(string xPath, string text)
        {
            IWebElement element = FindElementWithTimeout(FindElementByXPath, xPath, TimeSpan.FromSeconds(10));
            if (element == null)
            {
                return GetFailErrorResult($"Element is not exist : {xPath}", KeywordResults.Fail);
            }
            try
            {
                if (PackageName == "com.google.android.apps.chromecast.app" || PackageName == "com.android.chrome" || PackageName == "com.microsoft.emmx" || PackageName == "org.mozilla.firefox")
                {
                    element.SendKeys(text);
                }
                else
                {
                    //Actions actions = new Actions(_driver);
                    //actions.SendKeys(text).Perform();
                    element.SendKeys(text);


                }
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Element is not exits", KeywordResults.Error, true, ex);
            }
            return new KeywordResult(KeywordResults.Pass);

        }
        [KeywordDescription("Input Text to selected object")]
        [KeywordDisplayName("Input Text")]
        [KeywordParameters("resourceid", "resourceid to Input")]
        [KeywordParameters("text", "Text to Input")]
        [SampleScript("Android2.Input Text (search-disable-click,hello)")]
        public KeywordResult InputText(string resourceid, string text)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            IWebElement element = FindElementWithTimeout(FindElementById, resourceid, TimeSpan.FromSeconds(10));
            if (element == null)
            {
                return GetFailErrorResult($"Element is not exist : {resourceid}", KeywordResults.Fail);
            }
            try
            {
                if (PackageName == "com.google.android.apps.chromecast.app" || PackageName == "com.android.chrome" || PackageName == "com.microsoft.emmx")
                {
                    element.SendKeys(text);
                }
                else
                {
                    Actions actions = new Actions(_driver);
                    actions.SendKeys(Keys.Tab);
                    actions.SendKeys(text);
                    actions.Perform();
                }
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Error during send keys", KeywordResults.Error, true, ex);
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Wait until web object is exist in the current screen")]
        [KeywordDisplayName("Wait For Web Object")]
        [KeywordParameters("xPath", "XPATH to find")]
        [KeywordParameters("time", "Time to wait in seconds")]
        [SampleScript("Android2.Wait For Web Object (//android.widget.TextView[@text='Get Supplies'],30)")]
        public KeywordResult WaitForWebObject(string xPath, string time)
        {
            IWebElement element = FindElementWithTimeout(FindElementByXPath, xPath, TimeSpan.FromSeconds(5));
            if (element == null)
            {
                return GetFailErrorResult($"Element is not exist : {xPath}", KeywordResults.Fail);
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Execute ADB Command")]
        [KeywordDisplayName("Execute ADB Command")]
        [KeywordParameters("command", "ADB Command to execute")]
        [SampleScript("Android2.Execute ADB Command (shell settings put global heads_up_notifications_enabled 0)")]
        public KeywordResult ExecuteADBCommand(string command)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            string adbOutput = string.Empty;
            try
            {
                var result = SudoExecute(command);
                if (result == null)
                {
                    return GetFailErrorResult($"commnd  is not exist", KeywordResults.Fail);

                }
            }
            catch (Exception ex)
            {
                Logger.Error("given adb command is in corrcet", ex);
                kr = new KeywordResult(KeywordResults.Error);
                kr.AdditionalInfo = ex.ToString();
                kr.Output = "Error during collecting adb log";
                return kr;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Wait given text is disappeared on screen until given timeout (seconds)")]
        [KeywordDisplayName("Wait For Text Gone")]
        [KeywordParameters("text", "Wait to disappeared specific text")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("Android2.Wait For Text Gone (OK,5)")]
        public KeywordResult WaitForTextGone(string text, string time)
        {
            text = text.Replace("(", @"\(").Replace(")", @"\)");
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int timing = Convert.ToInt32(time);
            try
            {
                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(timing);
                IWebElement element = FindElementByAndroidUIAutomator($"new UiSelector().text(\"{text}\")");

                if (element != null)
                {
                    _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(timing);
                }
            }
            catch (Exception ex)
            {
                return GetFailErrorResult($"Element is not exist : {text}", KeywordResults.Error);
            }

            return new KeywordResult(KeywordResults.Pass);

        }

        [KeywordDescription("Trigger click event by using javascript in WebView. Try to use this keyword if Touch Web Object keyword work abnormal.\r\nThis Keyword should be called between Get Webview and Return Webview")]
        [KeywordDisplayName("Touch Web Object By Event")]
        [KeywordParameters("xPath", "XPATH to touch")]
        [SampleScript("Android2.Touch Web Object By Event(//android.widget.TextView[@text='Get Supplies'])")]
        public KeywordResult TouchWebObjectByEvent(string xPath)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            try
            {
                IWebElement element = FindElementWithTimeout(FindElementByXPath, xPath, TimeSpan.FromSeconds(10));

                if (element == null)
                {
                    return GetFailErrorResult($"Element is not exist : {xPath}", KeywordResults.Fail);
                }
                element.Click();
                return new KeywordResult(KeywordResults.Pass);

            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Unexpected error during click";
                kr.AdditionalInfo = ex.ToString();
                Logger.Error(kr.Output, ex);
            }

            return new KeywordResult(KeywordResults.Pass);

        }

        [KeywordDescription("Touch text in WebView.\r\nThis Keyword should be called between Get Webview and Return Webview")]
        [KeywordDisplayName("Touch Web Text")]
        [KeywordParameters("text", "text to touch")]
        [SampleScript("Android2.Touch Web Text (Stay signed in?)")]
        public KeywordResult TouchWebText(string text)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                IWebElement element = FindElementByAndroidUIAutomator($"new UiSelector().text(\"{text}\")");

                if (element == null)
                {
                    return GetFailErrorResult($"Element is not exist : {text}", KeywordResults.Fail);
                }
                element.Click();
                return kr;
            }
            catch
            {
                kr.Result = KeywordResults.Error;
                kr = new KeywordResult(KeywordResults.Error);
            }
            return kr;
        }

        [KeywordDescription("Trigger click event by using javascript in WebView. Try to use this keyword if Touch Web Object keyword work abnormal.\r\nThis Keyword should be called between Get Webview and Return Webview")]
        [KeywordDisplayName("Touch Web Text By Event")]
        [KeywordParameters("text", "text to touch")]
        [SampleScript("Android2.Touch Web Text By Event (Stay signed in?)")]
        public KeywordResult TouchWebTextByEvent(string text)
        {
            try
            {
                IWebElement element = FindElementByAndroidUIAutomator($"new UiSelector().text(\"{text}\")");
                if (element == null)
                {
                    Logger.Error("Element is not exits ");
                    return new KeywordResult(KeywordResults.Fail);
                }
                element.Click();
            }
            catch (Exception ex)
            {
                Logger.Error("Element is not exits ", ex);
                return GetFailErrorResult($"Element is not exist : {text}", KeywordResults.Error);
            }

            return new KeywordResult(KeywordResults.Pass);

        }

        [KeywordDescription("Touch object with XPath")]
        [KeywordDisplayName("Touch XPath")]
        [KeywordParameters("xPath", "XPath of element to touch")]
        [SampleScript("Android2.Touch XPath (//android.widget.TextView[@text='Printables')")]
        public KeywordResult TouchXPath(string xPath)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {

                IWebElement element = FindElementWithTimeout(FindElementByXPath, xPath, TimeSpan.FromSeconds(5));
                if (element == null)
                {
                    return GetFailErrorResult($"Element is not exist : {xPath}", KeywordResults.Fail);
                }
                element.Click();
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = ex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", ex);
            }
            return new KeywordResult(KeywordResults.Pass);

        }


        [KeywordDescription("Get UI Dump (XML) of current screen")]
        [KeywordDisplayName("Get UI Dump")]
        [SampleScript("Android2.Get UI Dump")]
        public KeywordResult GetUIDump()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                var dump = _driver.PageSource;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = $"Dump failed";
            }
            return new KeywordResult(KeywordResults.Pass);

        }

        [KeywordDescription("Wait given Object is appeared on screen until given timeout (seconds)")]
        [KeywordDisplayName("Wait For Object")]
        [KeywordParameters("resourceid", "Wait to appeared specific object")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("Android2.Wait For Object (com.hp.print.authagent.secureaccess:id/btnSignInIDPW,5)")]
        public KeywordResult WaitForObject(string resourceid, string findingTimeOut)
        {

            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            var waitTime = Convert.ToInt32(findingTimeOut);
            try
            {
                WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(waitTime));
                wait.Until(driver =>
                {
                    try
                    {
                        IWebElement element = FindElementWithTimeout(FindElementById, resourceid, TimeSpan.FromSeconds(waitTime));
                        return element != null && element.Displayed;
                    }
                    catch
                    {
                        return false;
                    }
                });
            }
            catch
            {
                return GetFailErrorResult("Element not found", KeywordResults.Error, false);
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Check if given text is exist in the current screen")]
        [KeywordDisplayName("Is Web Text Exist")]
        [KeywordParameters("text", "text to find")]
        [SampleScript("Android2.Is Web Text Exist (//android.widget.TextView[@text='Printables')")]
        public KeywordResult IsWebTextExist(string xPath)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            IWebElement element = FindElementWithTimeout(FindElementByXPath, xPath, TimeSpan.FromSeconds(3));
            if (element == null)
            {
                return GetFailErrorResult($"Element is not exist : {xPath}", KeywordResults.Fail);
            }

            return new KeywordResult(KeywordResults.Pass);

        }

        [KeywordDescription("Set text of object in WebView")]
        [KeywordDisplayName("Set Text Web Object")]
        [KeywordParameters("xPath", "XPATH to touch")]
        [KeywordParameters("textToSet", "Text to input")]
        [SampleScript("Android2.Set Text Web Object (//android.widget.TextView[@text='Printables'")]
        public KeywordResult SetTextWebObject(string xPath, string textToSet)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            IWebElement element = FindElementWithTimeout(FindElementByXPath, xPath, TimeSpan.FromSeconds(3));
            if (element == null)
            {
                return GetFailErrorResult($"Element is not exist : {xPath}", KeywordResults.Fail);
            }
            try
            {
                if (PackageName == "com.google.android.apps.chromecast.app")
                {
                    element.SendKeys(textToSet);
                }
                else
                {
                    element.SendKeys(textToSet);
                    //Actions actions = new Actions(_driver);
                    //actions.SendKeys(Keys.Tab);
                    //actions.SendKeys(textToSet);
                    //actions.Perform();
                }
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Element is not exits", KeywordResults.Error, true, ex);
            }
            return new KeywordResult(KeywordResults.Pass);
        }
        [KeywordDescription("Check if checkbox/radio button is checked")]
        [KeywordDisplayName("Is Checked")]
        [KeywordParameters("xpath", "xpath to check the element Property")]
        [SampleScript("Android2.Is Checked (com.hp.printercontrol:id/tile_title)")]
        public KeywordResult IsChecked(string xpath)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            IWebElement element = FindElementWithTimeout(FindElementByXPath, xpath, TimeSpan.FromSeconds(3));
            string checkedVal = element.GetAttribute("checked");
            if (checkedVal == "false")
            {
                return GetFailErrorResult($"Element is not exist : {xpath}", KeywordResults.Fail);
            }
            return kr;
        }
        [KeywordDescription("Check if object is enabled. Fail if object is disabled")]
        [KeywordDisplayName("Is Enabled")]
        [KeywordParameters("Xpath", "Xpath  to check element Property ")]
        [SampleScript("Android2.Is Enabled (//android.widget.Button[@index='0')")]
        public KeywordResult IsEnabled(string xpath)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            IWebElement element = FindElementWithTimeout(FindElementByXPath, xpath, TimeSpan.FromSeconds(3));
            if (!element.Enabled)
            {
                return GetFailErrorResult($"Element is not exist : {xpath}", KeywordResults.Fail);
            }
            return kr;
        }

        [KeywordDescription("Check if object is enabled. Fail if object is disabled")]
        [KeywordDisplayName("Is Text Enabled")]
        [KeywordParameters("text", "text to check")]
        [SampleScript("Android2.Is Text Enabled(Will Add in next Release)")]
        public KeywordResult IsTextEnabled(string text)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            try
            {
                IWebElement element = FindElementByAndroidUIAutomator($"new UiSelector().text(\"{text}\")");
                if (!element.Enabled || element == null)
                {
                    return GetFailErrorResult($"Text is not exist : {text}", KeywordResults.Fail);
                }
            }
            catch
            {
                return new KeywordResult(KeywordResults.Fail);
            }
            return kr;
        }


        [KeywordDescription("Check if checkbox/radio button is unchecked")]
        [KeywordDisplayName("Is Unchecked")]
        [KeywordParameters("xpath", "xpath to check element Property")]
        [SampleScript("Android2.Is Unchecked (//android.widget.Button[@index='0')")]
        public KeywordResult IsUnchecked(string Xpath)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            IWebElement element = FindElementWithTimeout(FindElementByXPath, Xpath, TimeSpan.FromSeconds(3));

            string checkedVal = element.GetAttribute("checked");
            if (checkedVal == "true")
            {
                return GetFailErrorResult($"Element is value is false  : {Xpath}", KeywordResults.Fail);
            }
            return kr;
        }

        [KeywordDescription("Check if checkbox/radio button is unselected")]
        [KeywordDisplayName("Is Unselected")]
        [KeywordParameters("xpath", "xpath to check element Property")]
        [SampleScript("Android2.Is UnSelected (//android.widget.Button[@index='0')")]
        public KeywordResult IsUnselected(string xpath)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            IWebElement element = FindElementWithTimeout(FindElementByXPath, xpath, TimeSpan.FromSeconds(3));

            if (element.Selected != false)
            {
                return GetFailErrorResult($"Element is value is false  : {xpath}", KeywordResults.Fail);
            }

            return kr;
        }

        [KeywordDescription("Check if checkbox/radio button is selected")]
        [KeywordDisplayName("Is Selected")]
        [KeywordParameters("xpath", "xpath to check element Property")]
        [SampleScript("Android2.Is Selected (//android.widget.Button[@index='0'")]
        public KeywordResult IsSelected(string Xpath)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            IWebElement element = FindElementWithTimeout(FindElementByXPath, Xpath, TimeSpan.FromSeconds(3));

            if (element.Selected != true)
            {
                return GetFailErrorResult($"Element is value {element.Selected.ToString()}  : {Xpath}", KeywordResults.Fail);
            }

            return kr;
        }

        [KeywordDescription("Get Heap dump file and save with given file name")]
        [KeywordDisplayName("Get Heap Dump")]
        [KeywordParameters("packageName", "target package name to get heap dump")]
        [KeywordParameters("filename", "Heap dump file name to save. Extension will be .prof")]
        public KeywordResult GetHeapDump(string packageName, string fileName)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            if (!fileName.EndsWith(".prof"))
            {
                fileName += ".prof";
            }
            fileName = Support.Utils.GetAbsolutePath(fileName, _outputDir);
            try
            {
                // Define the heap dump file path in device storage
                string heapDumpPath = $"/sdcard/{fileName}.hprof";

                // Run ADB command to dump heap
                string command = $"dumpsys meminfo {packageName} --dumpheap {heapDumpPath}";
                _driver.ExecuteScript("mobile: shell", new Dictionary<string, object>
                {
                    { "command", "sh" },
                    { "args", new string[] { "-c", command } }
                });

            }
            catch (Exception ex)
            {
                return new KeywordResult(KeywordResults.Fail);

            }

            return kr;
        }

        [GetKeyword]
        [KeywordDescription("Get text of selected object")]
        [KeywordDisplayName("Get Text")]
        [KeywordParameters("resourceid", "resourceid to get text")]
        [KeywordParameters("saveTo", "variable name to store")]
        [SampleScript("Android2.Get Text (com.hp.printercontrol:id/tile_title,${Error_Text})")]
        public KeywordResult GetText(string resourceid, string saveTo)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            IWebElement element = FindElementById(resourceid);
            var text = element.Text;
            try
            {
                if (!string.IsNullOrEmpty(text))
                {
                    CommonExecutionInfo.SetVariable(saveTo, text);
                    kr.Output = $"{saveTo} : {text}";
                    return kr;
                }

                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Get text failed with given resourceid :: {@resourceid}";
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = ex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", ex);
            }

            return kr;
        }

        [GetKeyword]
        [KeywordDescription("Get text of selected object with XPath")]
        [KeywordDisplayName("Get Text With XPath")]
        [KeywordParameters("xPath", "XPath of element to get text")]
        [KeywordParameters("saveTo", "variable name to store")]
        [SampleScript("Android2.Get Text With XPath (//android.widget.TextView[@text='Create Account'],${get_version})")]
        public KeywordResult GetTextWithXPath(string xPath, string saveTo)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            IWebElement element = FindElementByXPath(xPath);
            var text = element.GetAttribute("text");
            try
            {
                if (!string.IsNullOrEmpty(text))
                {
                    CommonExecutionInfo.SetVariable(saveTo, text);
                    kr.Output = $"{saveTo} : {text}";
                    return kr;
                }

                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Get text failed with given XPath :: {xPath}";
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = ex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", ex);
            }
            return kr;
        }

        [KeywordDescription("Check if checkbox/radio button is checked with XPath")]
        [KeywordDisplayName("Is XPath Checked")]
        [KeywordParameters("xPath", "XPath to check")]
        [SampleScript("Android2.Is XPath Checked(//android.widget.TextView[@text='Create Account']")]
        public KeywordResult IsXPathChecked(string xPath)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            try
            {
                IWebElement element = FindElementByXPath(xPath);
                string checkedVal = element.GetAttribute("checked");
                if (checkedVal == "true")
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Check checked status failed with given XPath :: {xPath}";
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = ex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", ex);
            }

            return kr;
        }

        [KeywordDescription("Check if object is enabled. Fail if object is disabled with XPath")]
        [KeywordDisplayName("Is XPath Enabled")]
        [KeywordParameters("xPath", "xPath to check")]
        [SampleScript("Android2.Is XPath Enabled(//android.widget.TextView[@text='Create Account']")]
        public KeywordResult IsXPathEnabled(string xPath)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                IWebElement element = FindElementByXPath(xPath);
                if (element.Enabled)
                {
                    return kr;
                }

                kr.Result = KeywordResults.Fail;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = ex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", ex);

            }

            return kr;
        }
        [KeywordDescription("Check if checkbox/radio button is selected with XPath")]
        [KeywordDisplayName("Is XPath Selected")]
        [KeywordParameters("xPath", "xPath to check")]
        [SampleScript("Android2.Is XPath Selected(//android.widget.TextView[@text='Create Account']")]
        public KeywordResult IsXPathSelected(string xPath)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                IWebElement element = FindElementByXPath(xPath);
                if (element.Selected)
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Check selected status failed with given XPath :: {xPath}";
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = ex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", ex);
            }
            return kr;
        }

        [KeywordDescription("Check if checkbox/radio button is unchecked with XPath")]
        [KeywordDisplayName("Is XPath Unchecked")]
        [KeywordParameters("xPath", "xPath to check")]
        [SampleScript("Android2.Is XPath Unchecked(//android.widget.TextView[@text='Create Account']")]
        public KeywordResult IsXPathUnchecked(string xPath)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                IWebElement element = FindElementByXPath(xPath);
                string checkedVal = element.GetAttribute("checked");
                if (checkedVal == "false")
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Check checked status failed with given XPath :: {xPath}";
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = ex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", ex);
            }
            return kr;
        }
        [KeywordDescription("Check if checkbox/radio button is unselected with XPath")]
        [KeywordDisplayName("Is XPath Unselected")]
        [KeywordParameters("xPath", "xPath to check")]
        [SampleScript("Android2.Is XPath Unselected(//android.widget.TextView[@text='Create Account']")]
        public KeywordResult IsXPathUnselected(string xPath)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                IWebElement element = FindElementByXPath(xPath);
                if (!element.Selected)
                {
                    return kr;

                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Check selected status failed with given XPath :: {xPath}";
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = ex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", ex);
            }
            return kr;
        }

        [KeywordDescription("Send Enter Key to target")]
        [KeywordDisplayName("Press Enter Key")]
        [SampleScript("Android2.Press Enter Key")]
        public KeywordResult PressEnterKey()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                Actions actions = new Actions(_driver);
                actions.SendKeys(Keys.Shift).SendKeys(Keys.Return).Perform();
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Error during send keys", KeywordResults.Fail, true, ex);
            }
            return new KeywordResult(KeywordResults.Pass);
        }
        [KeywordDescription("Send Tab Key to target")]
        [KeywordDisplayName("Press Tab Key")]
        [SampleScript("Android2.Press Tab Key")]
        public KeywordResult PressTabKey()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                Actions actions = new Actions(_driver);
                actions.SendKeys(Keys.Tab).Perform();

            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Error during send keys", KeywordResults.Error, true, ex);
            }
            return kr;
        }

        [KeywordDescription("Send Back Key to target")]
        [KeywordDisplayName("Press Back Key")]
        [SampleScript("Android2.Press Back Key")]
        public KeywordResult PressBackKey()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                _driver.LongPressKeyCode(AndroidKeyCode.Back);
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Error during send keys", KeywordResults.Error, true, ex);
            }
            return kr;
        }

        [KeywordDescription("Send Home Key to target")]
        [KeywordDisplayName("Press Home Key")]
        [SampleScript("Android2.Press Home Key")]
        public KeywordResult PressHomeKey()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                _driver.PressKeyCode(new KeyEvent(AndroidKeyCode.Home));
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Error during send keys", KeywordResults.Error, true, ex);
            }
            return kr;
        }

        [KeywordDescription("Send Back Key to target")]
        [KeywordDisplayName("Press App Switch Key")]
        [SampleScript("Android2.Press App Switch Key")]
        public KeywordResult PressAppSwitchKey()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                _driver.PressKeyCode(AndroidKeyCode.Keycode_APP_SWITCH);
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Error during send keys", KeywordResults.Error, true, ex);
            }
            return kr;
        }

        [KeywordDescription("Wait until given text is exist in the current screen")]
        [KeywordDisplayName("Wait For Web Text")]
        [KeywordParameters("text xpath", "text to find")]
        [KeywordParameters("time", "Time to wait in seconds")]
        [SampleScript("Android2.Wait For Web Text (Stay signed in?,10)")]
        public KeywordResult WaitForWebText(string text, string time)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            string xPath = $"//*[contains(@text, '{text}')]";

            TimeSpan waitTime = TimeSpan.FromSeconds(int.Parse(time));
            try
            {
                IWebElement element = FindElementWithTimeout(FindElementByXPath, xPath, waitTime);
                if (element == null)
                {
                    return GetFailErrorResult($"Can not find active element", KeywordResults.Fail);
                }
            }
            catch (Exception ex)
            {
                return GetFailErrorResult($"Element  {text} is not exist", KeywordResults.Error);
            }

            return kr;
        }
        [KeywordDescription("Wait element with given XPath is appeared on screen until given timeout (seconds)")]
        [KeywordDisplayName("Wait For XPath")]
        [KeywordParameters("xPath", "XPath of element")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("Android2.Wait For XPath (//android.widget.TextView[@text='App Settings'],60)")]
        public KeywordResult WaitForXPath(string xPath, string findingTimeOut)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            if (!int.TryParse(findingTimeOut.Trim(), out int timeOut))
            {
                return GetFailErrorResult("Timeout must be a number", KeywordResults.Error, false);
            }

            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var endTime = TimeSpan.FromSeconds(timeOut);

                while (stopwatch.Elapsed < endTime)
                {
                    try
                    {
                        IWebElement element = FindElementByXPath(xPath);
                        if (element != null && element.Displayed)
                        {
                            return kr;
                        }
                    }
                    catch
                    {

                    }
                    System.Threading.Thread.Sleep(100);
                }

                return GetFailErrorResult("Element not found", KeywordResults.Fail, false);
            }
            catch
            {
                return GetFailErrorResult("Unexpected error occurred", KeywordResults.Error, false);
            }
        }

        [KeywordDescription("Wait element with XPath is disappeared on screen until given timeout (seconds)")]
        [KeywordDisplayName("Wait For XPath Gone")]
        [KeywordParameters("xPath", "XPath of element")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("Android2.Wait For XPath Gone (//android.widget.TextView[@text='App Settings'],60)")]
        public KeywordResult WaitForXPathGone(string xPath, string time)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            TimeSpan waitTime = TimeSpan.FromSeconds(int.Parse(time));
            try
            {
                IWebElement element = FindElementWithTimeout(FindElementByXPath, xPath, waitTime);
                if (element == null)
                {
                    return GetFailErrorResult($"Can not find active element", KeywordResults.Fail);
                }
            }
            catch (Exception ex)
            {

                return GetFailErrorResult($"element not exits:{xPath}", KeywordResults.Error);
            }
            return kr;
        }
        [KeywordDescription("Wait given object is disappeared on screen until given timeout (seconds)")]
        [KeywordDisplayName("Wait For Object Gone")]
        [KeywordParameters("resourceid", "Wait to disappeared specific object")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("Android2.Wait For Object Gone (com.hp.print.horizontalconnector.onedriveforpersonal:id/ll_dots,30)")]
        public KeywordResult WaitForObjectGone(string resourceid, string time)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            TimeSpan waitTime = TimeSpan.FromSeconds(int.Parse(time));
            try
            {
                IWebElement element = FindElementWithTimeout(FindElementById, resourceid, waitTime);
                if (element == null)
                {
                    return GetFailErrorResult($"Can not find active element", KeywordResults.Fail);
                }
            }
            catch (Exception ex)
            {
                return GetFailErrorResult($"element not exits:{resourceid}", KeywordResults.Error);
            }
            return kr;
        }

        public static string ExecuteSshCommand(SshClient sshClient, string command)
        {
            try
            {
                if (sshClient == null || !sshClient.IsConnected)
                {
                    throw new InvalidOperationException("SSH client is not connected.");
                }

                using (var cmd = sshClient.CreateCommand(command))
                {
                    return cmd.Execute().Trim(); // Execute command and return output
                }
            }
            catch (Exception ex)
            {
                return $"Error executing command: {ex.Message}";
            }
        }

        [KeywordDescription("Set timeout scacle for applying all keywords in library." +
          "This scale facor will be multiplied of all timeout arguments in keywords." +
          "For example, if scale factor is set to 2 and call wait for object for 3 secondes, it will wait for 6 (3 x 2) seconds.")]
        [KeywordDisplayName("Set Timeout Scale")]
        [KeywordParameters("scaleFactor", "scale factor. Should be a number and larger than 0")]
        [SampleScript("Android2.Set Timeout Scale (10)")]
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

        [KeywordDescription("Set global object find timeout for Android User interaction such as Touch Text\r\nMost of Android Keywords except keyword starts with 'Wait For' wait given timeout if object is not on the screen\r\nThis keyword also be affected by scale factgor.")]
        [KeywordDisplayName("Set Timeout")]
        [KeywordParameters("Timeout", "Wait seconds for finding object")]
        [SampleScript("Android2.Set Timeout (5)")]
        public KeywordResult SetTimeout(string timeout)
        {
            if (!int.TryParse(timeout.Trim(), out _scaleFactor) || _scaleFactor < 1)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Scale factor must be a number and should be larger than 0";
                return error;
            }
            KeywordResult pass = new KeywordResult(KeywordResults.Pass);
            pass.Output = $"Scale factor is set to {_scaleFactor}";
            return pass;
        }
        [KeywordDescription("Long Click object with given resource id and drag to (x2,y2) which is percent of screen")]
        [KeywordDisplayName("Long Click And Drag Object")]
        [KeywordParameters("resourceId", "resource id of object to drag")]
        [KeywordParameters("x2", "percent value to drag with given position value")]
        [KeywordParameters("y2", "percent value to drag with given position value")]
        [SampleScript("Android2.Long Click And Drag Object(com.hp.print.horizontalconnector.googledrive:id/ll_item_name,0,0)")]
        public KeywordResult LongClickAndDragObject(string resourceId, string x, string y)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int startX = int.Parse(x);
            int startY = int.Parse(y);

            try
            {
                var element = FindElementByAndroidUIAutomator($"new UiSelector().resourceId(\"{resourceId}\")");


                // Replace the problematic line with the correct instantiation of PointerInputDevice.
                var finger = new PointerInputDevice(PointerKind.Touch);

                // Build touch sequence
                var sequence = new ActionSequence(finger, 0);
                sequence.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, startX, startY, TimeSpan.Zero));
                sequence.AddAction(finger.CreatePointerDown(MouseButton.Left));
                sequence.AddAction(finger.CreatePointerUp(MouseButton.Left));

                // Perform
                _driver.PerformActions(new List<ActionSequence> { sequence });
                return kr; // Success
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Error during long click and drag", KeywordResults.Error, true, ex);
            }
        }

        [KeywordDescription("Long Click object with given XPath and drag to (x2,y2) which is percent of screen")]
        [KeywordDisplayName("Long Click And Drag Object With XPath")]
        [KeywordParameters("Xapth", "Xpath of object to drag")]
        [KeywordParameters("x2", "percent value to drag with given position value")]
        [KeywordParameters("y2", "percent value to drag with given position value")]
        [SampleScript("Android2.Long Click And Drag Object With XPath (//node[@resource-id='username'],0,0)")]
        public KeywordResult LongClickAndDragObjectWithXPath(string xPath, string x2, string y2)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            try
            {
                IWebElement element = FindElementWithTimeout(FindElementByXPath, xPath, TimeSpan.FromSeconds(3));
                if (element == null)
                {
                    return GetFailErrorResult("Cannot find active element or element ID is empty", KeywordResults.Fail);
                }

                int startX = int.Parse(x2);
                int startY = int.Parse(y2);


                // Replace the problematic line with the correct instantiation of PointerInputDevice.
                var finger = new PointerInputDevice(PointerKind.Touch);

                // Build touch sequence
                var sequence = new ActionSequence(finger, 0);
                sequence.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, startX, startY, TimeSpan.Zero));
                sequence.AddAction(finger.CreatePointerDown(MouseButton.Left));
                sequence.AddAction(finger.CreatePointerUp(MouseButton.Left));

                // Perform
                _driver.PerformActions(new List<ActionSequence> { sequence });
                return kr; // Success
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Error during long click and drag with XPath", KeywordResults.Error, true, ex);
            }
            return kr;
        }
        public static string SudoExecute(string command)
        {

            Regex promptRegex = new Regex(@"[#$>%]"); // regular expression for matching terminal prompt
            using (var client = new SshClient(HostName, UserName, UserPassword))
            {
                client.Connect();
                var modes = new Dictionary<Renci.SshNet.Common.TerminalModes, uint>();

                using (var stream = client.CreateShellStream("xterm", 255, 50, 800, 600, 1024, modes))
                {
                    stream.Write($"sudo {command}\n");

                    Regex passwordRegex = new Regex("(password|Password):");
                    string response = stream.Expect(passwordRegex, TimeSpan.FromSeconds(10));

                    if (response != null)
                    {
                        stream.Write($"{UserPassword}\n"); // Send password
                    }
                    string output = stream.Expect(promptRegex, TimeSpan.FromSeconds(5));
                    return output ?? string.Empty;

                }
            }
        }

        public Dictionary<string, string> ParseMemoryDetails(string meminfoOutput)
        {
            var memoryDetails = new Dictionary<string, string>();
            var lines = meminfoOutput.Split('\n');

            foreach (var line in lines)
            {
                if (line.Contains("TOTAL PSS"))
                {
                    memoryDetails["TOTAL_PSS"] = line.Split(new[] { ':' }, 2)[1].Trim();
                }
                else if (line.Contains("Native Heap"))
                {
                    memoryDetails["Native Heap"] = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[2];
                }
                else if (line.Contains("Java Heap"))
                {
                    memoryDetails["Java Heap"] = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[2];
                }
            }
            return memoryDetails;
        }

        [KeywordDescription("Dump memory usage and save with given check point name")]
        [KeywordDisplayName("Collect Memory Info")]
        [KeywordParameters("checkPoint", "Check Point name to save")]
        [SampleScript("Android2.Collect Memory Info (${PackageName_ODB})")]
        public KeywordResult CollectMemoryInfo(string packageName, string filename, string iterations)
        {
            int iterationsCount = Convert.ToInt32(iterations);
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                string adbCommand = $"adb shell dumpsys meminfo {packageName}";
                string output = SudoExecute(adbCommand);
                string memoryInfo = SudoExecute(adbCommand);
                kr.Output = output;
                var parsedMemory = ParseMemoryDetails(memoryInfo);
                Logger.Debug("Memory Usage Details:");
                foreach (var item in parsedMemory)
                {
                    Logger.Debug($"{item.Key}: {item.Value} KB");
                }
                Logger.Debug(parsedMemory);
                string filePath = filename + ".csv";
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                SaveToCSV(memoryInfo, filePath);
                Logger.Debug($"Memory info saved to: {filePath}");

                List<int> totalPSSValues = new List<int>();

                for (int i = 0; i < iterationsCount; i++)
                {
                    int totalPSS = parsedMemory.Count;

                    if (totalPSS > 0)
                    {
                        totalPSSValues.Add(totalPSS);
                    }

                    System.Threading.Thread.Sleep(2000);
                }
                SaveToCSV(memoryInfo, filePath);

            }
            catch
            {
                return GetFailErrorResult($"Error during Collect Memory Info", KeywordResults.Error);

            }
            return kr;
        }

        static void SaveToCSV(string memoryInfo, string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("Memory Info");
                writer.WriteLine(memoryInfo.Replace("\n", ","));
            }
        }

        [KeywordDescription("Dump memory usage and save with given check point name")]
        [KeywordDisplayName("Collect Memory Info All")]
        [KeywordParameters("checkPoint", "Check Point name to save")]
        [SampleScript("Android2.Collect Memory Info All (com.hp.printercontrol)")]
        public KeywordResult CollectMemoryInfoAll(string filename)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                List<string> packageNames = GetInstalledApps();
                string filePath = filename + ".csv";
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                SaveMemoryUsageToCSV(packageNames, filePath);

                Logger.Debug($"Memory info saved to: {filePath}");
            }
            catch
            {
                return GetFailErrorResult($"Error during Collect Memory Info All", KeywordResults.Error);
            }
            return kr;
        }
        public static List<string> GetInstalledApps()
        {
            List<string> packages = new List<string>();
            string commandOutput = SudoExecute($"adb shell pm list packages -3");

            foreach (string line in commandOutput.Split('\n'))
            {
                if (line.Contains("package:"))
                {
                    packages.Add(line.Replace("package:", "").Trim());
                }
            }
            return packages;
        }

        static void SaveMemoryUsageToCSV(List<string> packageNames, string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {

                writer.WriteLine("Count,Package Name,Total PSS (KB),Java Heap (KB),Native Heap (KB)");

                int count = 1;

                foreach (string package in packageNames)
                {
                    string memInfo = SudoExecute($"adb shell dumpsys meminfo {package}");

                    int totalPSS, javaHeap, nativeHeap;
                    ExtractMemoryInfo(memInfo, out totalPSS, out javaHeap, out nativeHeap);

                    writer.WriteLine($"{count},{package},{totalPSS},{javaHeap},{nativeHeap}");
                    count++;
                }

                Logger.Debug($"Memory info saved to {filePath}");
            }
        }
        public static void ExtractMemoryInfo(string memoryInfo, out int totalPSS, out int javaHeap, out int nativeHeap)
        {
            totalPSS = ExtractValue(memoryInfo, @"TOTAL\s+PSS:\s+(\d+)");
            javaHeap = ExtractValue(memoryInfo, @"Java Heap:\s+(\d+)");
            nativeHeap = ExtractValue(memoryInfo, @"Native Heap:\s+(\d+)");
        }

        private static int ExtractValue(string memoryInfo, string pattern)
        {
            Regex regex = new Regex(pattern);
            Match match = regex.Match(memoryInfo);
            return match.Success ? int.Parse(match.Groups[1].Value) : 0;
        }

        [KeywordDisplayName("Swipe text")]
        [KeywordParameters("direction", "Should be one of <, <<, >, >>, ^, ^^, v, vv\r\n" +
    "< (right to lefet with small amount), << (right to left with large amount) \r\n" +
    "> (left to right with small amount), >> (left to right with large amount) \r\n" +
    "^ (down to up with small amount), ^^ (down to up with larget amount) \r\n" +
    "v (up to down with small amount), vv (up to down with larget amount)")]
        [SampleScript("Android2.Swipe (hello,^)")]
        public KeywordResult Swipetext(string text, string direction)
        {
            string scrollText;

            try
            {
                switch (direction)
                {
                    case "<":
                        FindElementByAndroidUIAutomator(
                            "new UiScrollable(new UiSelector().scrollable(true)).setAsHorizontalList().flingBackward()");

                        break;
                    case ">":
                        FindElementByAndroidUIAutomator(
                        "new UiScrollable(new UiSelector().scrollable(true)).setAsHorizontalList().flingForward()");
                        break;
                    case "^":
                        scrollText = $"new UiScrollable(new UiSelector().scrollable(true)).scrollIntoView(new UiSelector().text(\"{text}\"))";
                        FindElementByAndroidUIAutomator(scrollText);
                        break;
                    case "v":
                        scrollText = $"new UiScrollable(new UiSelector().scrollable(true)).scrollIntoView(new UiSelector().text(\"{text}\"))";
                        FindElementByAndroidUIAutomator(scrollText);
                        break;
                    default:
                        return GetFailErrorResult($"Invalid argument of direction :{direction}", KeywordResults.Fail, false);
                }
            }
            catch (Exception ex)
            {
                return GetFailErrorResult($"Error during swipe text with direction {direction}", KeywordResults.Error, true, ex);
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Initialize Memory monitoring for Android.\r\nStart Memory Monitoring, Collect Memory Info and Draw Memory Usage Graph keywords with packageName arguments will be used together.")]
        [KeywordDisplayName("Start Memory Monitoring")]
        [KeywordParameters("packageName", "Specific package name for monitoring")]
        [SampleScript("Android2.Start Memory Monitoring(${PackageName_ODB})")]
        public KeywordResult StartMemoryMonitoring(string packageName)
        {
            string csvPath;
            if (CommonExecutionInfo.CurrentRepeatCount > 0)
            {
                csvPath = Path.Combine(Directory.GetParent(_outputDir).FullName, $"Android_Memory_Usage_{packageName}.csv");
                Logger.Trace($"Current Repeat Count is greater than 0. CSV Path is : {csvPath}");
            }
            else
            {
                csvPath = Path.Combine(_outputDir, $"{_testCaseName}_AndroidMemoryMonitoring_{packageName}_{DateTime.Now.ToString("yyMMdd_HHmmssff")}.csv");
                Logger.Trace($"CSV Path is : {csvPath}");
            }

            KeywordResult r = new KeywordResult(KeywordResults.Pass);
            r.Output = $"Memory Monitoring data will be saved to : {csvPath}";
            return r;
        }

        [KeywordDescription("Re connect to the device. Call this keyword after Reboot")]
        [KeywordDisplayName("Re Connect")]
        [SampleScript("Android2.Re Connect")]
        public KeywordResult ReConnect()
        {
            try
            {
                string adbCommand = $"adb reboot";
                string output = SudoExecute(adbCommand);
                if (output == null)
                {
                    return GetFailErrorResult($"Error during device reboot", KeywordResults.Fail);
                }
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                return GetFailErrorResult($"Error during connect", KeywordResults.Error, true, ex);
            }
        }

        [KeywordDescription("Get memory stats (Min. PSS, Avg. PSS and Max. Pss) for last # hours")]
        [KeywordDisplayName("Get Memory Stats")]
        [KeywordParameters("packageName", "Specific package name for getting memory stats")]
        [KeywordParameters("durationHours", "Set duration hours for getting stats.")]
        [KeywordParameters("minSaveTo", "Variable name for saving Min. PSS value")]
        [KeywordParameters("avgSaveTo", "Variable name for saving Avg. PSS value")]
        [KeywordParameters("maxSaveTo", "Variable name for saving Max. PSS value")]
        [SampleScript("Android2.Get Memory Stats (${PackageName},8,${minPSS},${avgPss},${maxPSS})")]

        public KeywordResult GetMemoryStats(string filename, string packagename)
        {
            List<string> packageNames = GetInstalledApps();
            List<int> totalPSSValues = new List<int>();
            List<int> javaHeapValues = new List<int>();
            List<int> nativeHeapValues = new List<int>();

            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.WriteLine("Count,Package Name,Total PSS (KB),Java Heap (KB),Native Heap (KB)");
                int count = 0;

                foreach (string package in packageNames)
                {
                    string memInfo = SudoExecute($"adb shell dumpsys meminfo {package}");

                    int totalPSS, javaHeap, nativeHeap;
                    ExtractMemoryInfo(memInfo, out totalPSS, out javaHeap, out nativeHeap);
                    totalPSSValues.Add(totalPSS);
                    javaHeapValues.Add(javaHeap);
                    nativeHeapValues.Add(nativeHeap);
                    writer.WriteLine($"{count},{package},{totalPSS},{javaHeap},{nativeHeap}");
                    count++;
                }


                if (totalPSSValues.Count > 0)
                {
                    writer.WriteLine();
                    writer.WriteLine("Memory Statistics:");
                    writer.WriteLine($"Total PSS - Min: {totalPSSValues.Min()}, Avg: {totalPSSValues.Average():F2}, Max: {totalPSSValues.Max()}");
                    writer.WriteLine($"Java Heap - Min: {javaHeapValues.Min()}, Avg: {javaHeapValues.Average():F2}, Max: {javaHeapValues.Max()}");
                    writer.WriteLine($"Native Heap - Min: {nativeHeapValues.Min()}, Avg: {nativeHeapValues.Average():F2}, Max: {nativeHeapValues.Max()}");
                }

                SaveMemoryStatisticsToCSV(filename + ".csv", totalPSSValues, javaHeapValues, nativeHeapValues);

                Logger.Debug($"Total PSS - Min: {totalPSSValues.Min()}, Avg: {totalPSSValues.Average():F2}, Max: {totalPSSValues.Max()}");
                Logger.Debug($"Total PSS - Min: {totalPSSValues.Min()}, Avg: {totalPSSValues.Average():F2}, Max: {totalPSSValues.Max()}");
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        public static void SaveMemoryStatisticsToCSV(string filePath, List<int> totalPSSValues, List<int> javaHeapValues, List<int> nativeHeapValues)
        {
            if (totalPSSValues.Count > 0)
            {
                using (StreamWriter writer = new StreamWriter(filePath, false))
                {
                    writer.WriteLine("Metric,Min (KB),Avg (KB),Max (KB)");
                    writer.WriteLine($"Total PSS,{totalPSSValues.Min()},{totalPSSValues.Average():F2},{totalPSSValues.Max()}");
                    writer.WriteLine($"Java Heap,{javaHeapValues.Min()},{javaHeapValues.Average():F2},{javaHeapValues.Max()}");
                    writer.WriteLine($"Native Heap,{nativeHeapValues.Min()},{nativeHeapValues.Average():F2},{nativeHeapValues.Max()}");

                    Logger.Debug($"Memory statistics saved to {filePath}");
                }
            }
        }

        [KeywordDescription("Display Status Bar")]
        [KeywordDisplayName("Open Notification")]
        [SampleScript("Android2.Open Notification")]
        public KeywordResult OpenNotification()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            _driver = new AndroidDriver(new Uri($"http://{_dut.DeviceAddress}:" + $"{_dut.Port}/wd/hub"), _capabilites);
            try
            {
                if (_driver == null)
                {
                    return GetFailErrorResult("Driver is not initialized", KeywordResults.Error, true, null);
                }
                else
                {
                    _driver.LongPressKeyCode(AndroidKeyCode.Keycode_NOTIFICATION);
                    kr.ScreenShot = GetScreenCapture();
                }
            }
            catch (WebException ex)
            {
                return GetFailErrorResult($"Error during Open Notification", KeywordResults.Error, true, ex);
            }
            return kr;
        }

        [KeywordDescription("Initialize Memory monitoring for Android")]
        [KeywordDisplayName("Start Memory Monitoring")]
        [SampleScript("Android2.Start Memory Monitoring ")]
        public KeywordResult StartMemoryMonitoring()
        {
            string csvPath;
            _testCaseName = CommonExecutionInfo.GetVariable("_tcName");

            if (CommonExecutionInfo.CurrentRepeatCount > 0)
            {
                csvPath = Path.Combine(Directory.GetParent(_outputDir).FullName, $"Android_Memory_Usage.csv");
                Logger.Trace($"Current Repeat Count is greater than 0. CSV Path is : {csvPath}");
            }
            else
            {
                csvPath = Path.Combine(_outputDir, $"{_testCaseName}_AndroidMemoryMonitoring_{DateTime.Now.ToString("yyMMdd_HHmmssff")}.csv");
                Logger.Trace($"CSV Path is : {csvPath}");
            }
            _memoryUsage = new MemoryUsageItem(csvPath);
            KeywordResult r = new KeywordResult(KeywordResults.Pass);
            r.Output = $"Memory Monitoring data will be saved to : {csvPath}";
            return r;
        }

        [KeywordDescription("To swipe Carousel pages")]
        [KeywordParameters("direction", "Should be one of <, <<, >, >>, ^, ^^," +
    "< (right to lefet with small amount), << (right to left with large amount) \r\n" +
    "> (left to right with small amount), >> (left to right with large amount) \r\n")]
        [KeywordDisplayName("LiveUI Swipe")]
        [SampleScript("Android2.LiveUISwipe(<<)")]
        public KeywordResult LiveUISwipe(string direction)
        {
            string adbCommand;
            try
            {

                if (direction == "<")
                {
                    adbCommand = $"adb shell input swipe 800 1000 200 1000";
                    string output = SudoExecute(adbCommand);
                }
                else if (direction.ToLower() == ">")
                {
                    adbCommand = $"adb shell input swipe 200 1000 800 1000";
                    string output = SudoExecute(adbCommand);
                }
                else if (direction == "<<")
                {
                    for (int i = 0; i < 6; i++)
                    {
                        adbCommand = $"adb shell input swipe 800 1000 200 1000";
                        string output = SudoExecute(adbCommand);
                    }
                }
                else if (direction == ">>")
                {
                    for (int i = 0; i < 6; i++)
                    {
                        adbCommand = $"adb shell input swipe 200 1000 800 1000";
                        string output = SudoExecute(adbCommand);
                    }
                }
            }
            catch
            {
                return GetFailErrorResult($"Error during LiveUI Swipe with direction {direction}", KeywordResults.Error, true);
            }
            return new KeywordResult(KeywordResults.Pass);
        }
        [KeywordDescription("Input Text to focused object")]
        [KeywordDisplayName("WifiInput Text")]
        [KeywordParameters("text", "Text to Input")]
        [SampleScript("Android2.Input Text (${DeviceAdminPW})")]
        public KeywordResult WifiInputText(string text)
        {
            IWebElement element = null;

            element = _driver.SwitchTo().ActiveElement() as IWebElement;
            if (element == null)
            {
                return GetFailErrorResult($"Can not find active element", KeywordResults.Fail);
            }

            try
            {
                Actions actions = new Actions(_driver);
                actions.SendKeys(text).Perform();
            }
            catch (Exception ex)
            {

                return GetFailErrorResult($"Error during Input Text to focused object", KeywordResults.Error, true, ex);
            }
            return new KeywordResult(KeywordResults.Pass);
        }
        [KeywordDescription("Clear Text From The Text Field")]
        [KeywordDisplayName("Clear Text")]
        [SampleScript("Android2.Clear Text")]
        public KeywordResult ClearText()
        {
            IWebElement element = null;
            element = _driver.SwitchTo().ActiveElement() as IWebElement;
            if (element == null)
            {
                return GetFailErrorResult($"Can not find active element", KeywordResults.Fail);
            }
            try
            {

                element.Clear();
            }
            catch (Exception ex)
            {
                return GetFailErrorResult($"Error during Clearing the text", KeywordResults.Error, true, ex);

            }
            return new KeywordResult(KeywordResults.Pass);
        }


        [KeywordDescription("Clear Text From The Text Field")]
        [KeywordDisplayName("Clear Text")]
        [KeywordParameters("xPath", "xPath of text field ")]
        [SampleScript("Android2.Clear Text(//android.widget.EditText[@index='0'])")]
        public KeywordResult ClearText(string xPath)
        {
            IWebElement element = FindElementWithTimeout(FindElementById, xPath, TimeSpan.FromSeconds(3));
            if (element == null)
            {
                return GetFailErrorResult("Cannot find active element or element ID is Null", KeywordResults.Fail);
            }
            try
            {
                element.Clear();
            }
            catch (Exception ex)
            {
                return GetFailErrorResult($"Error during Clearing the xpath", KeywordResults.Error, true, ex);
            }
            return new KeywordResult(KeywordResults.Pass);
        }
        [KeywordDescription("Check if object is diabled . Fail if object is enabled")]
        [KeywordDisplayName("Is Disabled")]
        [KeywordParameters("Xpath", "Xpath to check the element ")]
        [SampleScript("Android2.Is Disabled (//android.widget.TextView[@index='0'])")]
        public KeywordResult IsDisabled(string Xpath)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            IWebElement element = FindElementWithTimeout(FindElementByXPath, Xpath, TimeSpan.FromSeconds(3));
            if (element == null)
            {
                return GetFailErrorResult("Cannot find active element or element ID is Null", KeywordResults.Fail);
            }
            try
            {
                if (!element.Enabled)
                {
                    return kr;
                }
                else
                {
                    return GetFailErrorResult($"Element is not disabled : {Xpath}", KeywordResults.Fail);
                }
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Error during Checking if element is disabled", KeywordResults.Error, true, ex);
            }
            return kr;
        }
        // Pseudocode:
        // - Define a new keyword method GetHeapMemory(packageName, saveTo)
        // - Validate inputs
        // - Execute "adb shell dumpsys meminfo {packageName}" via SudoExecute
        // - Parse Java Heap, Native Heap and Total PSS using existing ExtractMemoryInfo helper
        // - Compute Total Heap = Java Heap + Native Heap
        // - Compose a result string "JavaHeapKB=...;NativeHeapKB=...;TotalHeapKB=...;TotalPSSKB=..."
        // - Save to variable using CommonExecutionInfo.SetVariable(saveTo, result)
        // - Return KeywordResult.Pass with Output set
        // - On failure, return GetFailErrorResult

        [GetKeyword]
        [KeywordDescription("Get heap memory (Java Heap, Native Heap and Total PSS in KB) for a specific package.")]
        [KeywordDisplayName("Get Heap Memory")]
        [KeywordParameters("packageName", "Target package name")]
        [KeywordParameters("saveTo", "Variable name to store the result string")]
        [SampleScript("Android2.Get Heap Memory (com.hp.printercontrol,${heapMem})")]
        public KeywordResult GetHeapMemory(string packageName, string saveTo)
        {
            if (string.IsNullOrWhiteSpace(packageName))
            {
                return GetFailErrorResult("Package name is required", KeywordResults.Fail, false);
            }

            try
            {
                // Use adb via SSH helper for consistency with the rest of this class.
                string memInfo = SudoExecute($"adb shell dumpsys meminfo {packageName}");
                if (string.IsNullOrWhiteSpace(memInfo))
                {
                    return GetFailErrorResult($"Failed to get meminfo for package: {packageName}", KeywordResults.Fail, false);
                }

                int totalPss, javaHeap, nativeHeap;
                ExtractMemoryInfo(memInfo, out totalPss, out javaHeap, out nativeHeap);

                // Fallback: if both heaps are zero, try to parse with the simple parser
                if (javaHeap == 0 && nativeHeap == 0)
                {
                    var parsed = ParseMemoryDetails(memInfo);
                    int.TryParse(parsed.ContainsKey("Java Heap") ? parsed["Java Heap"] : "0", out javaHeap);
                    int.TryParse(parsed.ContainsKey("Native Heap") ? parsed["Native Heap"] : "0", out nativeHeap);
                }

                long totalHeap = (long)javaHeap + (long)nativeHeap;

                string result = $"JavaHeapKB={javaHeap};NativeHeapKB={nativeHeap};TotalHeapKB={totalHeap};TotalPSSKB={totalPss}";
                CommonExecutionInfo.SetVariable(saveTo, result);

                var kr = new KeywordResult(KeywordResults.Pass);
                kr.Output = $"{saveTo} : {result}";
                return kr;
            }
            catch (Exception ex)
            {
                return GetFailErrorResult("Error during getting heap memory", KeywordResults.Error, false, ex);
            }
        }
    }
}


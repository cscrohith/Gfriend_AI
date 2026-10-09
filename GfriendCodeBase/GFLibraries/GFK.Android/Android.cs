using HP.Automation.SES;
using HP.Automation.SES.Helper;
using HP.GFriend.GFLogger;
using HP.GFriend.Utils.Charter;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Xml;

namespace HP.GFriend.Keywords
{
 
    [LibraryDescription("ADB (Android Debug Bridge) should be enabled to use Android library")]        
    public class Android : IGFLibrary
    {
        private SESLib _android;
        private WebviewObject _webView = null;
        private DeviceUnderTest _dut;
        private string _outputDir;

        private int _displayWidth;
        private int _displayHeight;
        private bool _logAttached = false;
        private int _scaleFactor = 1;
        public string _testCaseName = "";

        private static MemoryUsageItem _memoryUsage = null;
        private static Dictionary<string, MemoryUsageItem> _pkgMemoryUsage;

        private EventHandler<Automation.SES.Log.LogEventArgs> logTrace;
        private EventHandler<Automation.SES.Log.LogEventArgs> logDebug;
        private EventHandler<Automation.SES.Log.LogEventArgs> logWarn;
        private EventHandler<Automation.SES.Log.LogEventArgs> logError;

        public void Dispose()
        {
            if(_webView != null)
            {
                try
                {
                    _webView.Dispose();
                }
                catch (Exception) { }
            }
            if (_android != null)
            {
                try                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            
                {
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                
                    _android.Dispose();
                    CommonExecutionInfo.RemoveSharedObject("android:" + _dut.DeviceId);
                    Logger.Trace("Deattach SES Logger");
                    Automation.SES.Log.Logger.OnTrace -= logTrace;
                    Automation.SES.Log.Logger.OnDebug -= logDebug;
                    Automation.SES.Log.Logger.OnWarn -= logWarn;
                    Automation.SES.Log.Logger.OnError -= logError;
                    _logAttached = false;
                }

                catch(Exception ex)
                {
                    Logger.Trace(ex.Message);
                }
            }                

        }

        public string GetName()
        {
            return "Android";
        }
        public List<string> GetDependencies()
        {
            return null;
        }

        public void Initialize(DeviceUnderTest dut, string outputDir)
        {
            _dut = dut;
            if(!_logAttached)
            {
                logTrace = delegate (object s, Automation.SES.Log.LogEventArgs e) { GFriendLoggerServices.GFLogger.LogTrace(e.Message); };
                logWarn = delegate (object s, Automation.SES.Log.LogEventArgs e) { GFriendLoggerServices.GFLogger.LogWarn(e.Message); };
                logDebug = delegate (object s, Automation.SES.Log.LogEventArgs e) { GFriendLoggerServices.GFLogger.LogDebug(e.Message); };
                logError = delegate (object s, Automation.SES.Log.LogEventArgs e) { GFriendLoggerServices.GFLogger.LogError(e.Message); };

                Automation.SES.Log.Logger.OnTrace += logTrace;
                Automation.SES.Log.Logger.OnDebug += logDebug;
                Automation.SES.Log.Logger.OnWarn += logWarn;
                Automation.SES.Log.Logger.OnError += logError;
                _logAttached = true;
            }
            _android = SESLib.Create(dut.DeviceAddress);
            _android.Connect(true, true);
            _outputDir = outputDir;
            System.Drawing.Point displaySize = _android.GetDisplaySize();
            _displayHeight = displaySize.Y;
            _displayWidth = displaySize.X;
            _pkgMemoryUsage = new Dictionary<string, MemoryUsageItem>();
            CommonExecutionInfo.SetSharedObject("android:" + _dut.DeviceId, _android);
        }

        public void ResetConnection()
        {
            Logger.Debug("Reset Android Connection");
            // Dispose connections
            if (_android != null)
            {
                try
                {
                    _android.Dispose();
                    CommonExecutionInfo.RemoveSharedObject("android:" + _dut.DeviceId);
                }
                catch (Exception ex)
                {
                    Logger.Error("Android dispose error", ex);
                }
            }

            // Reconnect Android
            _android = SESLib.Create(_dut.DeviceAddress);
            _android.Connect(true, true);
            CommonExecutionInfo.SetSharedObject("android:" + _dut.DeviceId, _android);
        }



        public bool DutUsed()
        {
            return true;
        }

        /// <summary>
        /// Get Android Controller (SES Lib.) for using Android control from other libraries
        /// </summary>
        /// <returns>SES Library</returns>
        public ref SESLib GetController()
        {
            return ref _android;
        }

        private byte[] GetScreenCapture()
        {
            try
            {
                return _android.GetScreenCapture();
            }
            catch(WebException ex)
            {
                throw ex;
            }
            catch(Exception)
            {
                return null;
            }
        }

        private Tuple<int,int> GetAbsolutePosition(string x, string y)
        {
            try
            {
                double dX = ((int.Parse(x)) / (double)100);
                double dY = ((int.Parse(y)) / (double)100);

                int aX = (int)(_displayWidth * dX);
                int aY = (int)(_displayHeight * dY);

                return new Tuple<int, int>(aX, aY);
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
        }

        [KeywordDescription("Set timeout scacle for applying all keywords in library." +
            "This scale facor will be multiplied of all timeout arguments in keywords." +
            "For example, if scale factor is set to 2 and call wait for object for 3 secondes, it will wait for 6 (3 x 2) seconds.")]
        [KeywordDisplayName("Set Timeout Scale")]
        [KeywordParameters("scaleFactor", "scale factor. Should be a number and larger than 0")]
        [SampleScript("Android.Set Timeout Scale (10)")]
        public KeywordResult SetTimeoutScale(string scaleFactor)
        {
            if(!int.TryParse(scaleFactor.Trim(), out _scaleFactor) || _scaleFactor <1)
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
        [SampleScript("Android.Set Timeout (5)")]
        public KeywordResult SetTimeout(string timeout)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            if (!int.TryParse(timeout, out int iTimeout))
            {
                kr.Output = "Timeout must be the number";
                kr.Result = KeywordResults.Fail;
            }
            iTimeout = _scaleFactor * iTimeout;
            try
            {
                if (!_android.SetTimeout(iTimeout))
                {
                    kr.Output = "Error during setting timeout for Android";
                    kr.Result = KeywordResults.Error;
                }
                
            }
            catch(WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }
            return kr;
        }

        [KeywordDescription("Launch App with activity name")]
        [KeywordDisplayName("Launch")]
        [KeywordParameters("activityName", "Activity name to launch")]
        [SampleScript("Android.Launch (com.android.contacts)")]
        public KeywordResult Launch(string activityName)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if(!_android.StartActivity(activityName))
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Can not start activity :{activityName}";
                    kr.ScreenShot = GetScreenCapture();
                    Logger.Warn($"Can not start activity :{activityName}");
                }

            }
            catch(WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "launch error";
                kr.AdditionalInfo = ex.ToString();
                Logger.Error("launch error", ex);
            }

            return kr;
        }
        
        [KeywordDescription("Touch object with given text")]
        [KeywordDisplayName("Touch Text")]
        [KeywordParameters("text", "Text to touch")]
        [SampleScript("Android.Touch Text (OK)")]
        public KeywordResult TouchText(string text)
        {
            text = text.Replace("(", @"\(").Replace(")",@"\)");
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_android.Click(new UiSelector().Text(text)))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Touch text failed with given text :: {text}";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();
            }
            
            return kr;
        }

        [KeywordDescription("Touch object with XPath")]
        [KeywordDisplayName("Touch XPath")]
        [KeywordParameters("xPath", "XPath of element to touch")]
        [SampleScript("Android.Touch XPath (//node[@resource-id='loginUserName']/node[2])")]
        public KeywordResult TouchXPath(string xPath)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_android.Click(xPath))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Touch text failed with given XPath :: {xPath}";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();
            }

            return kr;
        }

        [KeywordDescription("Capture Screen Shot of device and save to PC")]
        [KeywordDisplayName("Capture Screen Shot")]
        [KeywordParameters("filename", "filename to save")]
        [SampleScript("Android.Capture Screen Shot (ExitApp.jpg)")]
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
                kr.Result = KeywordResults.Fail;
                kr.Output = $"Capture screen shot failed with given filename :: {filename}";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }

            return kr;
        }

        [KeywordDescription("Check the screen contains given text (exact match)")]
        [KeywordDisplayName("Check Screen Contains Full Text")]
        [KeywordParameters("text", "Text to check screen contains given text")]
        [SampleScript("Android.Check Screen Contains Full Text (Exit)")]
        public KeywordResult CheckScreenContainsFullText(string text)
        {
            text = text.Replace("(", @"\(").Replace(")", @"\)");
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            try
            {
                if (_android.DoesScreenContains(new UiSelector().Text(text)))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Check failed with given text :: {text}";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }

            return kr;
        }

        [KeywordDescription("Check the screen partially contains given text")]
        [KeywordDisplayName("Check Screen Contains Partial Text")]
        [KeywordParameters("text", "Text to check screen contains given text")]
        [SampleScript("Android.Check Screen Contains Partial Text (Service busy.)")]
        public KeywordResult CheckScreenContainsPartialText(string text)
        {
            text = text.Replace("(", @"\(").Replace(")", @"\)");
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            try
            {
                if (_android.DoesScreenContains(new UiSelector().TextContains(text)))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Check failed with given text :: {text}";

            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }
            return kr;
        }

        [KeywordDescription("Check the screen does not contains given text (exact match)")]
        [KeywordDisplayName("Check Screen Not Contains Full Text")]
        [KeywordParameters("text", "Text to check screen does not contains given text")]
        [SampleScript("Android.Check Screen Not Contains Full Text (Exit)")]
        public KeywordResult CheckScreenNotContainsFullText(string text)
        {
            text = text.Replace("(", @"\(").Replace(")", @"\)");
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (!_android.DoesScreenContains(new UiSelector().Text(text)))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Check failed with given text :: {text}";

            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }
            return kr;
        }

        [KeywordDescription("Check the screen does not contains any partial given text")]
        [KeywordDisplayName("Check Screen Not Contains Partial Text")]
        [KeywordParameters("text", "Text to check screen does not contains any partial given text")]
        [SampleScript("Android.Check Screen Not Contains Partial Text (Exit)")]
        public KeywordResult CheckScreenNotContainsPartialText(string text)
        {
            text = text.Replace("(", @"\(").Replace(")", @"\)");
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (!_android.DoesScreenContains(new UiSelector().TextContains(text)))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Check failed with given text :: {text}";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }

            return kr;
        }

        [KeywordDescription("Custom Drag with given position (percent value)")]
        [KeywordDisplayName("Drag")]
        [KeywordParameters("x1, y1, x2, y2 ", "percent value to drag with given position value")]
        [SampleScript("Android.Drag (239,430,261,431)")]
        public KeywordResult Drag(string x1, string y1, string x2, string y2)
        {
            Tuple<int, int> from;
            Tuple<int, int> to;
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                from = GetAbsolutePosition(x1, y1);
                to = GetAbsolutePosition(x2, y2);
            }
            catch(Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Parsing error. All arguments must be numbers";
                kr.AdditionalInfo = ex.ToString();
                Logger.Error("Parsing error. All arguments must be numbers", ex);
                return kr;
            }

            try
            {
                if (_android.Drag(from.Item1, from.Item2, to.Item1, to.Item2))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Drag failed with given value :: {x1},{y1},{x2},{y2}";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }

            
            return kr;
        }

        [KeywordDescription("Get UI Dump (XML) of current screen")]
        [KeywordDisplayName("Get UI Dump")]
        [SampleScript("Android.Get UI Dump")]
        public KeywordResult GetUIDump()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (!string.IsNullOrEmpty(kr.Output = _android.GetUIDump()))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Dump failed";

            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }
            return kr;
        }

        [KeywordDescription("Input Text to selected object")]
        [KeywordDisplayName("Input Text")]
        [KeywordParameters("resourceid", "resourceid to Input")]
        [KeywordParameters("text", "Text to Input")]
        [SampleScript("Android.Input Text (com.hp.smartux.appgallery:id/admin_footer,${DeviceAdminPW})")]
        public KeywordResult InputText(string resourceid, string text)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_android.SetText(new UiSelector().ResourceId(@resourceid), text))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Input text failed with given resourceid and text :: {@resourceid}, {text}";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }
            
            return kr;
        }

        [KeywordDescription("Input Text to focused object")]
        [KeywordDisplayName("Input Text")]
        [KeywordParameters("text", "Text to Input")]
        [SampleScript("Android.Input Text (${DeviceAdminPW})")]
        public KeywordResult InputText(string text)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_android.SetText(new UiSelector().Focused(true), text))
                {                    
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Input text failed with currently focused object and text :: {text}";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }

            return kr;
        }

        [KeywordDescription("Input Text to element with XPath")]
        [KeywordDisplayName("Input Text With XPath")]
        [KeywordParameters("xPath", "XPath of element to Input")]
        [KeywordParameters("text", "Text to Input")]
        [SampleScript("Android.Input Text With XPath (//node[@resource-id='android:id/inputExtractEditText'],u1110)")]
        public KeywordResult InputTextWithXPath(string xPath, string text)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_android.SetText(xPath, text))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Input text failed with given xPath and text :: {xPath}, {text}";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }

            return kr;
        }

        [KeywordDescription("Execute ADB Command")]
        [KeywordDisplayName("Execute ADB Command")]
        [KeywordParameters("command", "ADB Command to execute")]
        [SampleScript("Android.Execute ADB Command (shell settings put global heads_up_notifications_enabled 0)")]
        public KeywordResult ExecuteADBCommand(string command)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            string adbOutput = string.Empty;
            try
            {
                adbOutput = _android.ExecuteADBCommand($"{command}", 10);
                kr.Output = adbOutput;
            }
            catch (Exception ex)
            {
                Logger.Error("Error during collecting adb log", ex);
                kr = new KeywordResult(KeywordResults.Error);
                kr.AdditionalInfo = ex.ToString();
                kr.Output = "Error during collecting adb log";
                return kr;
            }            
            return kr;
        }

        [GetKeyword]
        [KeywordDisplayName("Get toast message")]
        [KeywordDescription("Get Toast Message. " +
            "This keyword might not capture toast message due to timing issues because toast message is displayed with very short time." +
            "You can also use Get Last Toast Message keyword for retrieving cached toast message. ")]
        [KeywordParameters("saveTo", "variable name to store")]
        [SampleScript("Android.Get Toast Message (${GET_CAPABILITIES_BUTTON_TOAST_MESSAGE})")]
        public KeywordResult GetToastMessage(string saveTo)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            
            try
            {
                string text = _android.GetToastMessage();
                    CommonExecutionInfo.SetVariable(saveTo, text);
                    kr.Output = $"{saveTo} : {text}";
                    return kr;
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }

            return kr;
        }

        [GetKeyword]
        [KeywordDescription("Get last(cached) toast message.")]
        [KeywordDisplayName("Get Last Toast Message")]
        [KeywordParameters("saveTo", "variable name to store")]
        [SampleScript("Android.Get Last Toast Message (${GET_CAPABILITIES_BUTTON_TOAST_MESSAGE})")]
        public KeywordResult GetLastToastMessage(string saveTo)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            try
            {
                string text = _android.GetLastToastMessage();
                CommonExecutionInfo.SetVariable(saveTo, text);
                kr.Output = $"{saveTo} : {text}";
                return kr;
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }

            return kr;
        }

        [GetKeyword]
        [KeywordDescription("Get text of selected object")]
        [KeywordDisplayName("Get Text")]
        [KeywordParameters("resourceid", "resourceid to get text")]
        [KeywordParameters("saveTo", "variable name to store")]
        [SampleScript("Android.Get Text (com.hp.print.horizontalconnector.onedriveforpersonal:id/tv_contents,${Error_Text})")]
        public KeywordResult GetText(string resourceid, string saveTo)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            resourceid = Support.Utils.GetVariablevalueIfExist(resourceid);
            string text = _android.GetText(new UiSelector().ResourceId(@resourceid));
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
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }
            
            return kr;
        }

        [GetKeyword]
        [KeywordDescription("Get text of selected object with XPath")]
        [KeywordDisplayName("Get Text With XPath")]
        [KeywordParameters("xPath", "XPath of element to get text")]
        [KeywordParameters("saveTo", "variable name to store")]
        [SampleScript("Android.Get Text With XPath (//node[@resource-id='com.hp.workpath.sample.statisticsample:id/menuVersion'],${get_version})")]
        public KeywordResult GetTextWithXPath(string xPath, string saveTo)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            xPath = Support.Utils.GetVariablevalueIfExist(xPath);
            string text = _android.GetText(xPath);
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
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }

            return kr;
        }

        [KeywordDescription("Check if object is enabled. Fail if object is disabled")]
        [KeywordDisplayName("Is Enabled")]
        [KeywordParameters("resourceId", "resource id to check")]
        [SampleScript("Android.Is Enabled (com.intimetec.hpcopykiosk:id/copyButton)")]
        public KeywordResult IsEnabled(string resourceId)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_android.IsEnabled(new UiSelector().ResourceId(@resourceId)))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }
            
            return kr;
        }

        [KeywordDescription("Check if object is enabled. Fail if object is disabled with XPath")]
        [KeywordDisplayName("Is XPath Enabled")]
        [KeywordParameters("xPath", "xPath to check")]
        [SampleScript("Will Add in next Release")]
        public KeywordResult IsXPathEnabled(string xPath)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_android.IsEnabled(xPath))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }

            return kr;
        }

        [KeywordDescription("Check if object is enabled. Fail if object is disabled")]
        [KeywordDisplayName("Is Text Enabled")]
        [KeywordParameters("text", "text to check")]
        [SampleScript("Will Add in next Release")]
        public KeywordResult IsTextEnabled(string text)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_android.IsEnabled(new UiSelector().Text(text)))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }

            return kr;
        }

        [KeywordDescription("Check if object is enabled. Fail if object is disabled")]
        [KeywordDisplayName("Is Text Enabled")]
        [KeywordParameters("text", "text to check")]
        [SampleScript("Will Add in next Release")]
        public KeywordResult IsTextEnabled(string text, string index)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            if(!int.TryParse(index.Trim(), out int indexNumber))
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Index must be a number";
                return kr;
            }

            try
            {
                if (_android.IsEnabled(new UiSelector().Text(text), --indexNumber))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }

            return kr;
        }

        [KeywordDescription("Check if checkbox/radio button is selected")]
        [KeywordDisplayName("Is Selected")]
        [KeywordParameters("resourceId", "resource id to check")]
        [SampleScript("Android.Is Selected (com.hp.print.horizontalconnector.onedriveforpersonal:id/checkBoxDoNotShowAgain)")]
        public KeywordResult IsSelected(string resourceId)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_android.IsSelected(new UiSelector().ResourceId(@resourceId)))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Check selected status failed with given resource id :: {@resourceId}";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }
            
            return kr;
        }

        [KeywordDescription("Check if checkbox/radio button is unselected")]
        [KeywordDisplayName("Is Unselected")]
        [KeywordParameters("resourceId", "resource id to check")]
        [SampleScript("Android.Is UnSelected (com.hp.print.horizontalconnector.onedriveforbusiness:id/checkBoxDoNotShowAgain)")]
        public KeywordResult IsUnselected(string resourceId)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (!_android.IsSelected(new UiSelector().ResourceId(@resourceId)))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Check selected status failed with given resource id :: {@resourceId}";

            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }
            return kr;
        }

        [KeywordDescription("Check if checkbox/radio button is checked")]
        [KeywordDisplayName("Is Checked")]
        [KeywordParameters("resourceId", "resource id to check")]
        [SampleScript("Android.Is Checked (com.hp.print.horizontalconnector.onedriveforpersonal:id/checkBoxDoNotShowAgain)")]
        public KeywordResult IsChecked(string resourceId)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_android.IsChecked(new UiSelector().ResourceId(@resourceId)))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Check checked status failed with given resource id :: {@resourceId}";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }
            
            return kr;
        }

        [KeywordDescription("Check if checkbox/radio button is unchecked")]
        [KeywordDisplayName("Is Unchecked")]
        [KeywordParameters("resourceId", "resource id to check")]
        [SampleScript("Android.Is Unchecked (com.hp.print.horizontalconnector.onedriveforpersonal:id/checkBoxDoNotShowAgain)")]
        public KeywordResult IsUnchecked(string resourceId)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (!_android.IsChecked(new UiSelector().ResourceId(@resourceId)))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Check checked status failed with given resource id :: {@resourceId}";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }
            
            return kr;
        }

        [KeywordDescription("Check if checkbox/radio button is selected with XPath")]
        [KeywordDisplayName("Is XPath Selected")]
        [KeywordParameters("xPath", "xPath to check")]
        [SampleScript("Will Add in next Release")]
        public KeywordResult IsXPathSelected(string xPath)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_android.IsSelected(xPath))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Check selected status failed with given XPath :: {xPath}";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }

            return kr;
        }

        [KeywordDescription("Check if checkbox/radio button is unselected with XPath")]
        [KeywordDisplayName("Is XPath Unselected")]
        [KeywordParameters("xPath", "xPath to check")]
        [SampleScript("Will Add in next Release")]
        public KeywordResult IsXPathnselected(string xPath)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (!_android.IsSelected(xPath))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Check selected status failed with given XPath :: {xPath}";

            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }
            return kr;
        }

        [KeywordDescription("Check if checkbox/radio button is checked with XPath")]
        [KeywordDisplayName("Is XPath Checked")]
        [KeywordParameters("xPath", "XPath to check")]
        [SampleScript("Will Add in next Release")]
        public KeywordResult IsXPathChecked(string xPath)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_android.IsChecked(xPath))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Check checked status failed with given XPath :: {xPath}";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }

            return kr;
        }

        [KeywordDescription("Check if checkbox/radio button is unchecked with XPath")]
        [KeywordDisplayName("Is XPath Unchecked")]
        [KeywordParameters("xPath", "xPath to check")]
        [SampleScript("Will Add in next Release")]
        public KeywordResult IsXPathUnchecked(string xPath)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (!_android.IsChecked(xPath))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Check checked status failed with given XPath :: {xPath}";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }

            return kr;
        }

        [KeywordDescription("Long Click object with given resource id and drag to (x2,y2) which is percent of screen")]
        [KeywordDisplayName("Long Click And Drag Object")]
        [KeywordParameters("resourceId", "resource id of object to drag")]
        [KeywordParameters("x2", "percent value to drag with given position value")]
        [KeywordParameters("y2", "percent value to drag with given position value")]
        [SampleScript("Android.Long Click And Drag Object(com.hp.print.horizontalconnector.googledrive:id/ll_item_name,0,0)")]
        public KeywordResult LongClickAndDragObject(string resourceId, string x2, string y2)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            Tuple<int, int> to;

            try
            {
                to = GetAbsolutePosition(x2, y2);
            }
            catch(Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Parsing error. All arguments must be numbers.";
                kr.AdditionalInfo = ex.ToString();
                Logger.Error("Parsing error. All arguments must be numbers.", ex);
                return kr;
            }
            try
            {
                if (_android.DragTo(new UiSelector().ResourceId(@resourceId), to.Item1, to.Item2))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Drag failed with given resource id :: {@resourceId}";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }
            
            return kr;
        }

        [KeywordDescription("Long Click object with given XPath and drag to (x2,y2) which is percent of screen")]
        [KeywordDisplayName("Long Click And Drag Object With XPath")]
        [KeywordParameters("resourceId", "resource id of object to drag")]
        [KeywordParameters("x2", "percent value to drag with given position value")]
        [KeywordParameters("y2", "percent value to drag with given position value")]
        [SampleScript("Android.Long Click And Drag Object With XPath (//node[@resource-id='username'],0,0)")]
        public KeywordResult LongClickAndDragObjectWithXPath(string xPath, string x2, string y2)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            Tuple<int, int> to;

            try
            {
                to = GetAbsolutePosition(x2, y2);
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Parsing error. All arguments must be numbers.";
                kr.AdditionalInfo = ex.ToString();
                Logger.Error("Parsing error. All arguments must be numbers.", ex);
                return kr;
            }
            try
            {
                if (!_android.DragTo(xPath, to.Item1, to.Item2))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Drag failed with given XPath :: {xPath}";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }

            return kr;
        }

        [KeywordDescription("Send Back Key to target")]
        [KeywordDisplayName("Press Back Key")]
        [SampleScript("Android.Press Back Key")]
        public KeywordResult PressBackKey()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_android.PressKey(KeyCode.KEYCODE_BACK))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Press back key failed";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }
            return kr;
        }

        [KeywordDescription("Send Back Key to target")]
        [KeywordDisplayName("Press App Switch Key")]
        [SampleScript("Android.Press App Switch Key")]
        public KeywordResult PressAppSwitchKey()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_android.PressKey(KeyCode.KEYCODE_APP_SWITCH))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Press app switch key failed";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }
            return kr;
        }

        [KeywordDescription("Send Enter Key to target")]
        [KeywordDisplayName("Press Enter Key")]
        [SampleScript("Android.Press Enter Key")]
        public KeywordResult PressEnterKey()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_android.PressKey(KeyCode.KEYCODE_ENTER))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Press enter key failed";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }
            
            return kr;
        }

        [KeywordDescription("Send Home Key to target")]
        [KeywordDisplayName("Press Home Key")]
        [SampleScript("Android.Press Home Key")]
        public KeywordResult PressHomeKey()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_android.PressKey(KeyCode.KEYCODE_HOME))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Press home key failed";

            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }
            return kr;
        }

        [KeywordDescription("Send Tab Key to target")]
        [KeywordDisplayName("Press Tab Key")]
        [SampleScript("Android.Press Tab Key")]
        public KeywordResult PressTabKey()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_android.PressKey(KeyCode.KEYCODE_TAB))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Press tab key failed";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }
            
            return kr;
        }

        [KeywordDescription("Display Status Bar")]
        [KeywordDisplayName("Open Notification")]
        [SampleScript("Android.Open Notification")]
        public KeywordResult OpenNotification()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_android.OpenNotification())
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Open notification failed";

            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }
            return kr;
        }

        [KeywordDescription("Hide Android's virtual keyboard")]
        [KeywordDisplayName("Hide Keyboard")]
        [SampleScript("Android.Hide Keyboard ")]
        public KeywordResult HideKeyboard()
        {
            try
            {
                if (_android.IsVirtualKeyboardShown() == true)
                {
                    Logger.Debug("Virtual Keyboard is displayed.");
                    return PressBackKey();
                }
                Logger.Debug("Virtual Keyboard is not displayed.");
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (WebException wex)
            {
                KeywordResult kr = new KeywordResult(KeywordResults.Error);
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();
                return kr;

            }
            
        }

        [KeywordDescription("Swipe screen with given direction. Directions: Left or < : Swipe Right to Left | Right or > : Swipe Left to Right | Up or ^ : Swipe Down to Up | Down or v: Swipe Up to Down")]
        [KeywordDisplayName("Swipe")]
        [KeywordParameters("direction", "directino to swipe(<, >, ^, v)")]
        [SampleScript("Android.Swipe (>)")]
        public KeywordResult Swipe(string direction)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            SESLib.To dir = SESLib.To.Left;

            switch (direction) {
                case "<":
                    dir = SESLib.To.Left;
                    break;
                case ">" :
                    dir = SESLib.To.Right;
                    break;
                case "^":
                    dir = SESLib.To.Up;
                    break;
                case "v":
                    dir = SESLib.To.Down;
                    break;
                default:
                    kr.Result = KeywordResults.Error;
                    kr.Output = "Invalied Arguments";
                    return kr;
            }

            try
            {
                if (_android.Swipe(dir))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Swipe failed with given direction :: {direction}";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }

            return kr;
        }

        [KeywordDescription("Swipe screen by resource id with given direction. Directions: Left or < : Swipe Right to Left | Right or > : Swipe Left to Right | Up or ^ : Swipe Down to Up | Down or v: Swipe Up to Down")]
        [KeywordDisplayName("Swipe")]
        [KeywordParameters("resourceid","resourceid to swipe)")]
        [KeywordParameters("direction", "directino to swipe(<, >, ^, v)")]
        [SampleScript("Android.Swipe (com.hp.print.horizontalconnector.onedriveforbusiness:id/rl_content_pane,v)")]
        public KeywordResult Swipe(string resourceid, string direction)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            SESLib.To dir = SESLib.To.Left;

            switch (direction)
            {
                case "<":
                    dir = SESLib.To.Left;
                    break;
                case ">":
                    dir = SESLib.To.Right;
                    break;
                case "^":
                    dir = SESLib.To.Up;
                    break;
                case "v":
                    dir = SESLib.To.Down;
                    break;
                default:
                    kr.Result = KeywordResults.Error;
                    kr.Output = "Invalied Arguments";
                    return kr;
            }
            try
            {
                if (_android.Swipe(new UiSelector().ResourceId(@resourceid), dir))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Swipe failed with given resourceid and direction :: {@resourceid}, {direction}";

            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }
            
            return kr;
        }

        [KeywordDescription("Swipe screen by text with given direction. Directions: Left or < : Swipe Right to Left | Right or > : Swipe Left to Right | Up or ^ : Swipe Down to Up | Down or v: Swipe Up to Down")]
        [KeywordDisplayName("Swipe Text")]
        [KeywordParameters("text", "text to swipe)")]
        [KeywordParameters("direction", "directino to swipe(<, >, ^, v)")]
        [SampleScript("Android.Swipe Text (ok,>)")]
        public KeywordResult SwipeText(string text, string direction)
        {
            text = text.Replace("(", @"\(").Replace(")", @"\)");
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            SESLib.To dir = SESLib.To.Left;

            switch (direction)
            {
                case "<":
                    dir = SESLib.To.Left;
                    break;
                case ">":
                    dir = SESLib.To.Right;
                    break;
                case "^":
                    dir = SESLib.To.Up;
                    break;
                case "v":
                    dir = SESLib.To.Down;
                    break;
                default:
                    kr.Result = KeywordResults.Error;
                    kr.Output = "Invalied Arguments";
                    return kr;
            }

            try
            {
                if (_android.Swipe(new UiSelector().Text(text), dir))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Swipe failed with given text and direction :: {text}, {direction}";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }

            return kr;
        }

        [KeywordDescription("Swipe screen by XPath with given direction. Directions: Left or < : Swipe Right to Left | Right or > : Swipe Left to Right | Up or ^ : Swipe Down to Up | Down or v: Swipe Up to Down")]
        [KeywordDisplayName("Swipe XPath")]
        [KeywordParameters("xPath", "XPath to swipe)")]
        [KeywordParameters("direction", "directino to swipe(<, >, ^, v)")]
        [SampleScript("Android.Swipe XPath (//node[@resource-id='username'],<)")]
        public KeywordResult SwipeXPath(string xPath, string direction)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            SESLib.To dir = SESLib.To.Left;

            switch (direction)
            {
                case "<":
                    dir = SESLib.To.Left;
                    break;
                case ">":
                    dir = SESLib.To.Right;
                    break;
                case "^":
                    dir = SESLib.To.Up;
                    break;
                case "v":
                    dir = SESLib.To.Down;
                    break;
                default:
                    kr.Result = KeywordResults.Error;
                    kr.Output = "Invalied Arguments";
                    return kr;
            }

            try
            {
                if (_android.Swipe(xPath, dir))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Swipe failed with given XPath and direction :: {xPath}, {direction}";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }

            return kr;
        }

        [KeywordDescription("Swipe screen by text with given direction. Directions: Left or < : Swipe Right to Left | Right or > : Swipe Left to Right | Up or ^ : Swipe Down to Up | Down or v: Swipe Up to Down")]
        [KeywordDisplayName("Swipe Text with Index")]
        [KeywordParameters("text", "text to swipe)")]
        [KeywordParameters("index", "text index to swipe)")]
        [KeywordParameters("direction", "directino to swipe(<, >, ^, v)")]
        [SampleScript("Android.Swipe Text with Index (No,0,>)")]
        public KeywordResult SwipeTextwithIndex(string text, string index, string direction)
        {
            text = text.Replace("(", @"\(").Replace(")", @"\)");
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int.TryParse(index.Trim(), out int indexNumber);
            indexNumber--;
            SESLib.To dir = SESLib.To.Left;

            switch (direction)
            {
                case "<":
                    dir = SESLib.To.Left;
                    break;
                case ">":
                    dir = SESLib.To.Right;
                    break;
                case "^":
                    dir = SESLib.To.Up;
                    break;
                case "v":
                    dir = SESLib.To.Down;
                    break;
                default:
                    kr.Result = KeywordResults.Error;
                    kr.Output = "Invalied Arguments";
                    return kr;
            }
            try
            {
                if (_android.Swipe(new UiSelector().Text(text), indexNumber, dir))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Swipe failed with given text and direction :: {text}, {direction}";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }
            
            return kr;
        }

        [KeywordDescription("Long touch item by given text")]
        [KeywordDisplayName("Touch Long Text")]
        [KeywordParameters("text", "text to long touch")]
        [SampleScript("Android.Touch Long Text(Exit)")]
        public KeywordResult TouchLongText(string text)
        {
            text = text.Replace("(", @"\(").Replace(")", @"\)");
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            try
            {
                if (_android.LongClick(new UiSelector().Text(text)))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Touch long click failed with given text :: {text}";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }
            
            return kr;
        }

        [KeywordDescription("Touch Object by given resourceid")]
        [KeywordDisplayName("Touch ID")]
        [KeywordParameters("resourceid", "resourceid to touch")]
        [SampleScript("Android.Touch ID (com.hp.print.authagent.secureaccess:id/btnSignInIDPW)")]
        public KeywordResult TouchID(string resourceid)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_android.Click(new UiSelector().ResourceId(@resourceid)))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Touch ID failed with given resourceid :: {@resourceid}";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }
            
            return kr;
        }

        [KeywordDescription("Touch Object by given resourceid")]
        [KeywordDisplayName("Touch ID With Index")]
        [KeywordParameters("resourceid", "resourceid to touch")]
        [KeywordParameters("index", "index number starts with 1")]
        [SampleScript("Android.Touch ID With Index(${Concent_Accept_Terms_Button},1)")]
        public KeywordResult TouchIDWithIndex(string resourceid, string index)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            int.TryParse(index.Trim(), out int indexNumber);
            indexNumber--;
            try
            {
                if (_android.Click(new UiSelector().ResourceId(@resourceid), indexNumber))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Touch ID failed with given resourceid and index number :: {@resourceid} , {index}";

            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }

            return kr;
        }

        [KeywordDescription("Touch with position with percent of left and percent of top")]
        [KeywordDisplayName("Touch Position")]
        [KeywordParameters("x", "x percent value to drag with given position value")]
        [KeywordParameters("y", "y percent value to drag with given position value")]
        [SampleScript("Android.Touch Position (8,65)")]
        public KeywordResult TouchPosition(string x, string y)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            Tuple<int, int> position;
            try
            {
                 position = GetAbsolutePosition(x, y);
            }
            catch(Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Parsing error. All arguments must be numbers";
                kr.AdditionalInfo = ex.ToString();
                Logger.Error("Parsing error.All arguments must be numbers", ex);
                return kr;
            }
            
            try
            {
                if (_android.PressScreen(position.Item1, position.Item2))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Touch failed with given value :: {x}, {y}";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }
            
            return kr;
        }

        [KeywordDescription("Touch item by Text and Index, Index starts with 1")]
        [KeywordDisplayName("Touch Text With Index")]
        [KeywordParameters("text", "Text to touch")]
        [KeywordParameters("index", "index number starts with 1")]
        [SampleScript("Android.Touch Text With Index (No,0)")]
        public KeywordResult TouchTextWithIndex(string text, string index)
        {
            text = text.Replace("(", @"\(").Replace(")", @"\)");
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int.TryParse(index.Trim(), out int indexNumber);
            indexNumber--;

            try
            {
                if (_android.Click(new UiSelector().Text(text), indexNumber))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Touch text with index failed with given text and index:: {text}, {indexNumber}";

            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }

            return kr;
        }

        [KeywordDescription("Wait given Object is appeared on screen until given timeout (seconds)")]
        [KeywordDisplayName("Wait For Object")]
        [KeywordParameters("resourceid", "Wait to appeared specific object")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("Android.Wait For Object (com.hp.print.authagent.secureaccess:id/btnSignInIDPW,5)")]
        public KeywordResult WaitForObject(string resourceid, string time)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int waitTime = Convert.ToInt32(time);
            waitTime = _scaleFactor * waitTime;
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddSeconds(waitTime);
            TimeSpan waitingTime;
            int timeOut = _android.GetTimeout();
            _android.SetTimeout(0);

            try
            {
                while (!_android.DoesScreenContains(new UiSelector().ResourceId(@resourceid)))
                {
                    if (DateTime.Now > endTime)
                    {
                        waitingTime = DateTime.Now.Subtract(startTime);
                        _android.SetTimeout(timeOut);
                        kr.Result = KeywordResults.Fail;
                        kr.ScreenShot = GetScreenCapture();
                        kr.Output = $"Wait for object failed with given resourceid and timeout:: {@resourceid}, {waitingTime}";
                        return kr;
                    }
                    Thread.Sleep(TimeSpan.FromMilliseconds(500));
                }
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }
            
            _android.SetTimeout(timeOut);
            return kr;
        }

        [KeywordDescription("Wait given text is appeared on screen until given timeout (seconds)")]
        [KeywordDisplayName("Wait For Text")]
        [KeywordParameters("text", "Wait to appeared specific text")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("Android.Wait For Text (OK,5)")]
        public KeywordResult WaitForText(string text, string time)
        {
            text = text.Replace("(", @"\(").Replace(")", @"\)");
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int waitTime = Convert.ToInt32(time);
            waitTime = _scaleFactor * waitTime;
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddSeconds(waitTime);
            TimeSpan waitingTime;
            
            int timeOut = _android.GetTimeout();
            _android.SetTimeout(0);

            try
            {
                while (!_android.DoesScreenContains(new UiSelector().Text(text)))
                {
                    if (DateTime.Now > endTime)
                    {
                        waitingTime = DateTime.Now.Subtract(startTime);
                        _android.SetTimeout(timeOut);
                        kr.Result = KeywordResults.Fail;
                        kr.ScreenShot = GetScreenCapture();
                        kr.Output = $"Wait for text failed with given text and timeout:: {text}, {waitingTime}";
                        return kr;
                    }
                    Thread.Sleep(TimeSpan.FromMilliseconds(500));
                }

            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }
            _android.SetTimeout(timeOut);
            return kr;
        }

        [KeywordDescription("Wait element with given XPath is appeared on screen until given timeout (seconds)")]
        [KeywordDisplayName("Wait For XPath")]
        [KeywordParameters("xPath", "XPath of element")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("Android.Wait For XPath (//node[@resource-id='username'],60)")]
        public KeywordResult WaitForXPath(string xPath, string time)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int waitTime = Convert.ToInt32(time);
            waitTime = _scaleFactor * waitTime;
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddSeconds(waitTime);
            TimeSpan waitingTime;

            int timeOut = _android.GetTimeout();
            _android.SetTimeout(0);

            try
            {
                while (!_android.DoesScreenContains(xPath))
                {
                    if (DateTime.Now > endTime)
                    {
                        waitingTime = DateTime.Now.Subtract(startTime);
                        _android.SetTimeout(timeOut);
                        kr.Result = KeywordResults.Fail;
                        kr.ScreenShot = GetScreenCapture();
                        kr.Output = $"Wait for text failed with given XPath and timeout:: {xPath}, {waitingTime}";
                        return kr;
                    }
                    Thread.Sleep(TimeSpan.FromMilliseconds(500));
                }

            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }
            _android.SetTimeout(timeOut);
            return kr;
        }

        [KeywordDescription("Wait given object is disappeared on screen until given timeout (seconds)")]
        [KeywordDisplayName("Wait For Object Gone")]
        [KeywordParameters("resourceid", "Wait to disappeared specific object")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("Android.Wait For Object Gone (com.hp.print.horizontalconnector.onedriveforpersonal:id/ll_dots,30)")]
        public KeywordResult WaitForObjectGone(string resourceid, string time)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int waitTime = Convert.ToInt32(time);
            waitTime = _scaleFactor * waitTime;
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddSeconds(waitTime);
            TimeSpan waitingTime;
                        
            int timeOut = _android.GetTimeout();
            _android.SetTimeout(0);

            try
            {
                while (_android.DoesScreenContains(new UiSelector().ResourceId(@resourceid)))
                {
                    if (DateTime.Now > endTime)
                    {
                        waitingTime = DateTime.Now.Subtract(startTime);
                        _android.SetTimeout(timeOut);
                        kr.Result = KeywordResults.Fail;
                        kr.ScreenShot = GetScreenCapture();
                        kr.Output = $"Wait for object gone failed with given resourceid and timeout:: {@resourceid}, {waitingTime}";
                        return kr;
                    }
                    Thread.Sleep(TimeSpan.FromMilliseconds(500));
                }
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }
            
            _android.SetTimeout(timeOut);
            return kr;
        }

        [KeywordDescription("Wait given text is disappeared on screen until given timeout (seconds)")]
        [KeywordDisplayName("Wait For Text Gone")]
        [KeywordParameters("text", "Wait to disappeared specific text")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("Android.Wait For Text Gone (OK,5)")]
        public KeywordResult WaitForTextGone(string text, string time)
        {
            text = text.Replace("(", @"\(").Replace(")", @"\)");
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int waitTime = Convert.ToInt32(time);
            waitTime = _scaleFactor * waitTime;
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddSeconds(waitTime);
            TimeSpan waitingTime;
                        
            int timeOut = _android.GetTimeout();
            _android.SetTimeout(0);

            try
            {
                while (_android.DoesScreenContains(new UiSelector().Text(text)))
                {
                    if (DateTime.Now > endTime)
                    {
                        waitingTime = DateTime.Now.Subtract(startTime);
                        _android.SetTimeout(timeOut);
                        kr.Result = KeywordResults.Fail;
                        kr.ScreenShot = GetScreenCapture();
                        kr.Output = $"Wait for text gone failed with given text and timeout:: {text}, {waitingTime}";
                        return kr;
                    }
                    Thread.Sleep(TimeSpan.FromMilliseconds(500));
                }
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }
            
            _android.SetTimeout(timeOut);
            return kr;
        }

        [KeywordDescription("Wait element with XPath is disappeared on screen until given timeout (seconds)")]
        [KeywordDisplayName("Wait For XPath Gone")]
        [KeywordParameters("xPath", "XPath of element")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("Android.Wait For XPath Gone (//node[@resource-id='username'],60)")]
        public KeywordResult WaitForXPathGone(string xPath, string time)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int waitTime = Convert.ToInt32(time);
            waitTime = _scaleFactor * waitTime;
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddSeconds(waitTime);
            TimeSpan waitingTime;

            int timeOut = _android.GetTimeout();
            _android.SetTimeout(0);

            try
            {
                while (_android.DoesScreenContains(xPath))
                {
                    if (DateTime.Now > endTime)
                    {
                        waitingTime = DateTime.Now.Subtract(startTime);
                        _android.SetTimeout(timeOut);
                        kr.Result = KeywordResults.Fail;
                        kr.ScreenShot = GetScreenCapture();
                        kr.Output = $"Wait for text gone failed with given XPath and timeout:: {xPath}, {waitingTime}";
                        return kr;
                    }
                    Thread.Sleep(TimeSpan.FromMilliseconds(500));
                }
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }

            _android.SetTimeout(timeOut);
            return kr;
        }
        [KeywordDescription("Set Seekbar value")]
        [KeywordDisplayName("Set Seekbar")]
        [KeywordParameters("resourceId", "Resource id of seekbar")]
        [KeywordParameters("value", "Relative value to set. 0 (left end) to 100(right end)")]
        [SampleScript("Will Add in next Release")]
        public KeywordResult SetSeekbar(string resourceId, string value)
        {
            if(!int.TryParse(value, out int iValue) || iValue < 0 || iValue > 100)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Value should be a number between 0 and 100";
                return error;
            }
            double dValue = (double)iValue / 100.0;
            try
            {

                if (!_android.SetSeekbar(new UiSelector().ResourceId(resourceId), dValue))
                {
                    KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                    fail.Output = "Fail to set seekbar value";
                    fail.ScreenShot = GetScreenCapture();
                    return fail;
                }
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (WebException wex)
            {
                KeywordResult kr = new KeywordResult(KeywordResults.Error);
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();
                return kr;

            }
        }

        [KeywordDescription("Set Seekbar value")]
        [KeywordDisplayName("Set Seekbar With XPath")]
        [KeywordParameters("xPath", "Xpath of seekbar")]
        [KeywordParameters("value", "Relative value to set. 0 (left end) to 100(right end)")]
        [SampleScript("Will Add in next Release")]
        public KeywordResult SetSeekbarWithXpath(string xPath, string value)
        {
            if (!int.TryParse(value, out int iValue) || iValue < 0 || iValue > 100)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Value should be a number between 0 and 100";
                return error;
            }
            double dValue = (double)iValue / 100.0;
            try
            {

                if (!_android.SetSeekbar(xPath, dValue))
                {
                    KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                    fail.Output = "Fail to set seekbar value";
                    fail.ScreenShot = GetScreenCapture();
                    return fail;
                }
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (WebException wex)
            {
                KeywordResult kr = new KeywordResult(KeywordResults.Error);
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();
                return kr;

            }
        }


        [KeywordDescription("Get the control of Webview on current screen")]
        [KeywordDisplayName("Get Webview")]
        [SampleScript("Android.Get Webview")]
        public KeywordResult GetWebView()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            if(_webView != null)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Already Got Webview and not returned.";
                return kr;
            }

            try
            {
                _webView = _android.GetWebView();
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();
                return kr;

            }
            catch (Exception ex)
            {
                Logger.Error("Error getting web view.", ex);
                kr.Output = "Webview Getting error";
                kr.Result = KeywordResults.Fail;
                return kr;
            }
            if(_webView == null)
            {
                Logger.Error("Fail to getting web view.");
                kr.Output = "Webview Getting Fail";
                kr.Result = KeywordResults.Fail;
            }
            return kr;
        }

        [KeywordDescription("Get the control of Webview on current screen during given waiting time")]
        [KeywordDisplayName("Get Webview")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("Android.Get Webview(60)")]
        public KeywordResult GetWebView(string time)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int Time = Convert.ToInt32(time);
            Time = _scaleFactor * Time;
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddSeconds(Time);
            int retryCount = 1;

            if (_webView != null)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Already Got Webview and not returned.";
                return kr;
            }

            try
            {
                while(DateTime.Now < endTime)
                {
                    _webView = _android.GetWebView();

                    if (_webView == null)
                    {
                        Logger.Debug($"Try to getting Webview again: {retryCount}");
                        retryCount++;                        
                    }
                    else
                    {
                        break;
                    }
                }
                
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();
                return kr;

            }
            catch (Exception ex)
            {
                Logger.Error("Error getting web view.", ex);
                kr.Output = "Webview Getting error";
                kr.Result = KeywordResults.Fail;
                return kr;
            }

            if (_webView == null)
            {
                Logger.Error($"Fail to getting web view during {DateTime.Now.Subtract(startTime)}");
                kr.Output = $"Webview Getting Fail (Waiting Time: {DateTime.Now.Subtract(startTime)})";
                kr.Result = KeywordResults.Fail;
            }
            return kr;
        }

        [KeywordDescription("Return the control of Webview\r\n This Keyword MUST be called after all webview releated keywords")]
        [KeywordDisplayName("Return Webview")]
        [SampleScript("Android.Return Webview")]
        public KeywordResult ReturnWebView()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            if (_webView == null)
            {
                return kr;
            }

            try
            {
                _webView.Dispose();
                _webView = null;
            }
            catch (Exception ex)
            {
                Logger.Error("Error disposing web view.", ex);
                kr.Output = "Webview returning error";
                kr.Result = KeywordResults.Fail;
            }
            return kr;
        }

        [KeywordDescription("Touch object in WebView.\r\nThis Keyword should be called between Get Webview and Return Webview")]
        [KeywordDisplayName("Touch Web Object")]
        [KeywordParameters("xPath", "XPATH to touch")]
        [SampleScript("Android.Touch Web Object (//*[@class=\"auth-google button-primary\"])")]
        public KeywordResult TouchWebObject (string xPath)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            if (!_webView.Click(xPath))
            {
                kr.Result = KeywordResults.Fail;
                kr.Output = $"Fail to click object with xpath {xPath}";
            }
            return kr;
        }

        [KeywordDescription("Trigger click event by using javascript in WebView. Try to use this keyword if Touch Web Object keyword work abnormal.\r\nThis Keyword should be called between Get Webview and Return Webview")]
        [KeywordDisplayName("Touch Web Object By Event")]
        [KeywordParameters("xPath", "XPATH to touch")]
        [SampleScript("Android.Touch Web Object By Event(//*[@id=\"idSIButton9\"])")]
        public KeywordResult TouchWebObjectByEvent(string xPath)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            try
            {
                IWebDriver driver = _webView.GetWebDriver();
                IJavaScriptExecutor jsExecutor = driver as IJavaScriptExecutor;
                IWebElement target = driver.FindElement(By.XPath(xPath));
                jsExecutor.ExecuteScript("arguments[0].click();", target);
            }
            catch(Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Unexpected error during javascript click";
                kr.AdditionalInfo = ex.ToString();
                Logger.Error(kr.Output, ex);
            }
            
            return kr;
        }

        [KeywordDescription("Touch text in WebView.\r\nThis Keyword should be called between Get Webview and Return Webview")]
        [KeywordDisplayName("Touch Web Text")]
        [KeywordParameters("text", "text to touch")]
        [SampleScript("Android.Touch Web Text (Stay signed in?)")]
        public KeywordResult TouchWebText(string text)
        {
            string xPath = $"//*[text()[contains(.,'{text}')]]";
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            if (!_webView.Click(xPath))
            {
                kr.Result = KeywordResults.Fail;
                kr.Output = $"Fail to click object with text {text}";
            }
            return kr;
        }

        [KeywordDescription("Trigger click event by using javascript in WebView. Try to use this keyword if Touch Web Object keyword work abnormal.\r\nThis Keyword should be called between Get Webview and Return Webview")]
        [KeywordDisplayName("Touch Web Text By Event")]
        [KeywordParameters("text", "text to touch")]
        [SampleScript("Android.Touch Web Text By Event (Stay signed in?)")]
        public KeywordResult TouchWebTextByEvent(string text)
        {
            string xPath = $"//*[text()[contains(.,'{text}')]]";
            return TouchWebObjectByEvent(xPath);
        }

        [KeywordDescription("Set text of object in WebView.\r\nThis Keyword should be called between Get Webview and Return Webview")]
        [KeywordDisplayName("Set Text Web Object")]
        [KeywordParameters("xPath", "XPATH to touch")]
        [KeywordParameters("textToSet", "Text to input")]
        [SampleScript("Android.Set Text Web Object (//*[@class='placeholderContainer']/input[1],${AzureID})")]
        public KeywordResult SetTextWebObject(string xPath, string textToSet)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            if (textToSet.EndsWith(@"\n"))
            {
                textToSet = textToSet.Replace(@"\n", string.Empty) + Keys.Enter;
            }
            try
            {
                if (!_webView.SetText(xPath, textToSet, false))
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Fail to click object with xpath {xPath}";
                }
            }
            catch(TargetInvocationException)
            {
                Logger.Debug("TargetInvocationException. Try again");
                // Incase of target invocation exception try again after sleep
                Thread.Sleep(1500);
                if (!_webView.SetText(xPath, textToSet, false))
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Fail to click object with xpath {xPath}";
                }
            }
            catch(Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = "Unexpected exception.";
                Logger.Error(kr.Output, ex);
            }
            
            return kr;
        }

        [KeywordDescription("Check if web object is exist in the current screen")]
        [KeywordDisplayName("Is Web Object Exist")]
        [KeywordParameters("xPath", "XPATH to find")]
        [SampleScript("Android.Is Web Object Exist (//*[@class='placeholderContainer']/input[1])")]
        public KeywordResult IsWebObjectExist(string xPath)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            if (!_webView.IsExist(xPath, TimeSpan.FromSeconds(0)))
            {
                kr.Result = KeywordResults.Fail;
                kr.Output = $"Can not find {xPath} in the screen";
            }
            return kr;
        }

        [KeywordDescription("Check if given text is exist in the current screen")]
        [KeywordDisplayName("Is Web Text Exist")]
        [KeywordParameters("text", "text to find")]
        [SampleScript("Android.Is Web Text Exist (Next)")]
        public KeywordResult IsWebTextExist(string text)
        {
            string xPath = $"//*[contains(text(),'{text}')]";
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            if (!_webView.IsExist(xPath, TimeSpan.FromSeconds(0)))
            {
                kr.Result = KeywordResults.Fail;
                kr.Output = $"Can not find {text} in the screen";
            }
            return kr;
        }

        [KeywordDescription("Wait until web object is exist in the current screen")]
        [KeywordDisplayName("Wait For Web Object")]
        [KeywordParameters("xPath", "XPATH to find")]
        [KeywordParameters("time", "Time to wait in seconds")]
        [SampleScript("Android.Wait For Web Object (//*[@class='placeholderContainer']/input[1],30)")]
        public KeywordResult WaitForWebObject(string xPath, string time)
        {
            int iTime = int.Parse(time) * _scaleFactor;
            TimeSpan waitTime = TimeSpan.FromSeconds(iTime);

            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            if (!_webView.IsExist(xPath, waitTime))
            {
                kr.Result = KeywordResults.Fail;
                kr.Output = $"Can not find {xPath} in the screen within in given timeout {time} second(s)";
            }
            return kr;
        }

        [KeywordDescription("Wait until given text is exist in the current screen")]
        [KeywordDisplayName("Wait For Web Text")]
        [KeywordParameters("text", "text to find")]
        [KeywordParameters("time", "Time to wait in seconds")]
        [SampleScript("Android.Wait For Web Text (Stay signed in?,10)")]
        public KeywordResult WaitForWebText(string text, string time)
        {
            TimeSpan waitTime = TimeSpan.FromSeconds(int.Parse(time));
            string xPath = $"//*[contains(text(),'{text}')]";

            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            if (!_webView.IsExist(xPath, waitTime))
            {
                kr.Result = KeywordResults.Fail;
                kr.Output = $"Can not find {text} in the screen within in given timeout {time} second(s)";
            }
            return kr;
        }

        [KeywordDescription("Re connect to the device. Call this keyword after Reboot")]
        [KeywordDisplayName("Re Connect")]
        [SampleScript("Android.Re Connect")]
        public KeywordResult ReConnect()
        {
            try
            {
                ResetConnection();
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                KeywordResult kr = new KeywordResult(KeywordResults.Error);
                kr.AdditionalInfo = ex.ToString();
                kr.Output = "Error during connect";
                return kr;
            }
        }



        [KeywordDescription("Disconnect from device")]
        [KeywordDisplayName("Disconnect")]
        [SampleScript("Android.Disconnect")]
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

        [KeywordDescription("Save current adb log in the log buffer to file")]
        [KeywordDisplayName("Save ADB Log To File")]
        [KeywordParameters("fileName", "File name to save")]
        [SampleScript("Will Add in next Release")]
        public KeywordResult SaveADBLogToFile(string fileName)
        {
            string saveTo = Path.Combine(_outputDir, fileName);
            string adbLog = string.Empty;
            try
            {
                adbLog = _android.ExecuteADBCommand("logcat -d -v threadtime", 30);
            }
            catch(Exception ex)
            {
                Logger.Error("Error during collecting adb log", ex);
                KeywordResult kr = new KeywordResult(KeywordResults.Error);
                kr.AdditionalInfo = ex.ToString();
                kr.Output = "Error during collecting adb log";
                return kr;
            }
            
            if(string.IsNullOrEmpty(adbLog))
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "Can not get adb log";
                return fail;
            }
            try
            {
                using (StreamWriter writer = new StreamWriter(saveTo, false, System.Text.Encoding.UTF8))
                {
                    writer.Write(adbLog);
                    writer.Flush();
                    writer.Close();
                }
            }
            catch(Exception ex)
            {
                Logger.Error("Error during write file", ex);
                KeywordResult kr = new KeywordResult(KeywordResults.Error);
                kr.AdditionalInfo = ex.ToString();
                kr.Output = "Error during write file";
                return kr;
            }

            return new KeywordResult(KeywordResults.Pass);
        }


        [KeywordDescription("Initialize Memory monitoring for Android")]
        [KeywordDisplayName("Start Memory Monitoring")]
        [SampleScript("Android.Start Memory Monitoring ")]
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

        [KeywordDescription("Dump memory usage and save with given check point name")]
        [KeywordDisplayName("Collect Memory Info")]
        [KeywordParameters("checkPoint", "Check Point name to save")]
        [SampleScript("Android.Collect Memory Info (${PackageName_ODB})")]
        public KeywordResult CollectMemoryInfo(string checkPoint)
        {
            try
            {
                Dictionary<string, long> memInfo = _android.GetMemoryUsage();

                if (CommonExecutionInfo.CurrentRepeatCount > 0)
                {
                    checkPoint = string.Join("_", CommonExecutionInfo.CurrentRepeatCount.ToString(), checkPoint.Trim());
                }

                _memoryUsage.AddData(checkPoint, memInfo);
                KeywordResult r = new KeywordResult(KeywordResults.Pass);
                return r;
            }
            catch (Exception ex)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "Fail during dump memory";
                fail.AdditionalInfo = ex.ToString();
                GFLogger.Logger.Error(ex.ToString());
                return fail;
            }

        }

        [KeywordDescription("Draw Android Memory Usage Graph")]
        [KeywordDisplayName("Draw Memory Usage Graph")]
        [SampleScript("Android.Draw Memory Usage Graph ()")]
        public KeywordResult DrawMemoryUsageGraph()
        {
            try
            {
                ChartCreator chart = new ChartCreator(_memoryUsage.GetCSVPath());
                string chartImage = Path.Combine(_outputDir, $"{_testCaseName}_AndroidMemory.png");
                chart.SaveImage(chartImage, System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line, true, "Total");

                if (CommonExecutionInfo.CurrentRepeatCount > 0)
                {
                    File.Copy(chartImage, Path.Combine(Directory.GetParent(_outputDir).FullName, $"{_testCaseName}_Android_Memory_Usage.png"), true);
                }

                KeywordResult r = new KeywordResult(KeywordResults.Pass);
                r.ScreenShot = File.ReadAllBytes(chartImage);
                return r;
            }
            catch (Exception ex)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "Fail during create chart";
                fail.AdditionalInfo = ex.ToString();
                return fail;
            }
        }

        [KeywordDescription("Initialize Memory monitoring for Android.\r\nStart Memory Monitoring, Collect Memory Info and Draw Memory Usage Graph keywords with packageName arguments will be used together.")]
        [KeywordDisplayName("Start Memory Monitoring")]
        [KeywordParameters("packageName", "Specific package name for monitoring")]
        [SampleScript("Android.Start Memory Monitoring(${PackageName_ODB})")]
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



            _memoryUsage = new MemoryUsageItem(csvPath);
            _pkgMemoryUsage[packageName] = _memoryUsage;

            KeywordResult r = new KeywordResult(KeywordResults.Pass);
            r.Output = $"Memory Monitoring data will be saved to : {csvPath}";
            return r;
        }

        [KeywordDescription("Dump memory usage and save with given check point name. \r\nStart Memory Monitoring, Collect Memory Info and Draw Memory Usage Graph keywords with packageName arguments will be used together.")]
        [KeywordDisplayName("Collect Memory Info")]
        [KeywordParameters("packageName", "Specific package name for monitoring")]
        [KeywordParameters("checkPoint", "Check Point name to save")]
        [SampleScript("Will Add in next Release")]
        public KeywordResult CollectMemoryInfo(string packageName, string checkPoint)
        {
            if(!_pkgMemoryUsage.ContainsKey(packageName))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"Memory collection not initialized with {packageName}. Use Start Memory Monitoring(packageName) before calling this keyword.";
                Logger.Error(error.Output);
                return error;
            }
            MemoryUsageItem memoryUsageItem = _pkgMemoryUsage[packageName];
            try
            {
                Dictionary<string, long> memInfo = _android.GetMemoryUsage(packageName);

                if(memInfo.Count == 0)
                {
                    memInfo = _android.GetMemoryUsage(packageName);
                }

                if (CommonExecutionInfo.CurrentRepeatCount > 0)
                {
                    checkPoint = string.Join("_", CommonExecutionInfo.CurrentRepeatCount.ToString(), checkPoint.Trim());
                }

                memoryUsageItem.AddData(checkPoint, memInfo);
                KeywordResult r = new KeywordResult(KeywordResults.Pass);
                return r;
            }
            catch (Exception ex)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "Fail during dump memory";
                fail.AdditionalInfo = ex.ToString();
                GFLogger.Logger.Error(ex.ToString());
                return fail;
            }

        }

        [KeywordDescription("Draw Android Memory Usage Graph.\r\nStart Memory Monitoring, Collect Memory Info and Draw Memory Usage Graph keywords with packageName arguments will be used together.")]
        [KeywordDisplayName("Draw Memory Usage Graph")]
        [KeywordParameters("packageName", "Specific package name for draw graph")]
        [SampleScript("Android.Draw Memory Usage Graph (${PackageName_ODB})")]
        public KeywordResult DrawMemoryUsageGraph(string packageName)
        {
            if (!_pkgMemoryUsage.ContainsKey(packageName))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"Memory collection not initialized with {packageName}. Use Start Memory Monitoring(packageName) before calling this keyword.";
                Logger.Error(error.Output);
                return error;
            }
            MemoryUsageItem memoryUsageItem = _pkgMemoryUsage[packageName];
            try
            {
                ChartCreator chart = new ChartCreator(memoryUsageItem.GetCSVPath());
                string chartImage = Path.Combine(_outputDir, $"{_testCaseName}_AndroidMemory_{packageName}.png");
                chart.SaveImage(chartImage, System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line, true);

                if (CommonExecutionInfo.CurrentRepeatCount > 0)
                {
                    File.Copy(chartImage, Path.Combine(Directory.GetParent(_outputDir).FullName, $"Android_Memory_Usage_{packageName}.png"), true);
                }

                KeywordResult r = new KeywordResult(KeywordResults.Pass);
                r.ScreenShot = File.ReadAllBytes(chartImage);
                return r;
            }
            catch (Exception ex)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "Fail during create chart";
                fail.AdditionalInfo = ex.ToString();
                return fail;
            }
        }
        [KeywordDescription("Get Heap dump file and save with given file name\r\nTo get the heap dump, your target app must set as debuggable.")]
        [KeywordDisplayName("Get Heap Dump")]
        [KeywordParameters("packageName", "target package name to get heap dump")]
        [KeywordParameters("filename", "Heap dump file name to save. Extension will be .prof")]
        [SampleScript("[SampleScript(\"Will Add in next Release\")]")]
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
                _android.GetHeapDump(packageName, fileName);
                kr.Output = $"Heap dump is saved to {fileName}";
            }
            catch (KeyNotFoundException knfex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Fail to get process id";
                kr.AdditionalInfo = knfex.ToString();
                Logger.Error(kr.Output, knfex);
                return kr;
            }
            catch (InvalidOperationException ioex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Unable to get dump file";
                kr.AdditionalInfo = ioex.ToString();
                Logger.Error(kr.Output, ioex);
                return kr;
            }
            catch (UnauthorizedAccessException uaex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Fail to get heap dump due to security reason. Make sure your target app is debuggable.";
                kr.AdditionalInfo = uaex.ToString();
                Logger.Error(kr.Output, uaex);
                return kr;
            }

            return kr;
        }

        [GetKeyword]
        [KeywordDescription("Get memory stats (Min. PSS, Avg. PSS and Max. Pss) for last # hours")]
        [KeywordDisplayName("Get Memory Stats")]
        [KeywordParameters("packageName", "Specific package name for getting memory stats")]
        [KeywordParameters("durationHours", "Set duration hours for getting stats.")]
        [KeywordParameters("minSaveTo", "Variable name for saving Min. PSS value")]
        [KeywordParameters("avgSaveTo", "Variable name for saving Avg. PSS value")]
        [KeywordParameters("maxSaveTo", "Variable name for saving Max. PSS value")]
        [SampleScript("Android.Get Memory Stats (${PackageName},8,${minPSS},${avgPss},${maxPSS})")]

        public KeywordResult GetMemoryStats(string packageName, string durationHours,
            string minSaveTo, string avgSaveTo, string maxSaveTo)
        {
            packageName = Support.Utils.GetVariablevalueIfExist(packageName);
            durationHours = Support.Utils.GetVariablevalueIfExist(durationHours);

            if(!int.TryParse(durationHours.Trim(), out int iDuration))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Duration hours must be a number";
                Logger.Error(error.Output);
                return error;
            }

            Dictionary<string, string> stats = _android.ProcStats(packageName, iDuration);
            if(stats.Count <3)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Can not get procstat information of given package : {packageName}";
                Logger.Error(fail);
                foreach(KeyValuePair<string, string> kv in stats)
                {
                    Logger.Trace($"{kv.Key} : {kv.Value}");
                }
                return fail;
            }

            CommonExecutionInfo.SetVariable(minSaveTo, stats["Min PSS"]);
            CommonExecutionInfo.SetVariable(avgSaveTo, stats["Avg PSS"]);
            CommonExecutionInfo.SetVariable(minSaveTo, stats["Max PSS"]);

            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            result.Output = $"Min. PSS : {stats["Min PSS"]} | Avg. PSS : {stats["Avg PSS"]} | Max. PSS : {stats["Max PSS"]}";
            return result;
        }
        [KeywordDescription("Check given text is visible or not in current screen")]
        [KeywordDisplayName("Is Text Exists")]
        [KeywordParameters("text", "text to check")]
        [SampleScript("Android.Is Text Exists (France)")]
        public KeywordResult IsTextExists(string text)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_android.DoesScreenContains(new UiSelector().Text(text)))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }

            return kr;
        }
        [KeywordDescription("Swipe screen by XPath with given direction in steps. Directions: Left or < : Swipe Right to Left | Right or > : Swipe Left to Right | Up or ^ : Swipe Down to Up | Down or v: Swipe Up to Down")]
        [KeywordDisplayName("Swipe XPath withStep")]
        [KeywordParameters("xPath", "XPath to swipe)")]
        [KeywordParameters("direction", "directino to swipe(<, >, ^, v)")]
        [KeywordParameters("step", "step to swipe)")]
        [SampleScript("Android.Swipe XPath (//node[@resource-id='username'],<,12")]

        public KeywordResult SwipeXPathwithstep(string xPath, string direction, string steps)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            SESLib.To dir = SESLib.To.Left;

            switch (direction)
            {
                case "<":
                    dir = SESLib.To.Left;
                    break;
                case ">":
                    dir = SESLib.To.Right;
                    break;
                case "^":
                    dir = SESLib.To.Up;
                    break;
                case "v":
                    dir = SESLib.To.Down;
                    break;
                default:
                    kr.Result = KeywordResults.Error;
                    kr.Output = "Invalied Arguments";
                    return kr;
            }

            try
            {
                if (_android.Swipe(xPath, dir, Convert.ToInt32(steps)))
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Swipe failed with given XPath and direction :: {xPath}, {direction},{steps}";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }

            return kr;
        }
        [KeywordDescription("Check Given Xpath text is available in given listbox xpath or not\nNote:this keyword is created for country selection screen in hp smart application")]
        [KeywordDisplayName("Is Text Exists in listbox")]
        [KeywordParameters("xPath", "XPath of text")]
        [KeywordParameters("xPath", "XPath of list box")]
        [SampleScript("Android.(//node[@resource-id='FR'],//node[@resource-id='oobe']/node[3])")]
        public KeywordResult IsTextExistsinlistbox(string xpath, string listboxxpath)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                string xpathBounds = Getbounds(xpath);
                string listboxBounds= Getbounds(listboxxpath);
                if (!AreBoundsWithinManualBounds(listboxBounds, xpathBounds))
                {
                    kr = new KeywordResult(KeywordResults.Fail);
                }
                return kr;
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();
            }

            return kr;
        }

        [KeywordDescription("Check if object is disabled. Fail if object is Enabled")]
        [KeywordDisplayName("Is Disabled")]
        [KeywordParameters("resourceId", "resource id to check")]
        [SampleScript("Android.Is Disabled (com.intimetec.hpcopykiosk:id/copyButton)")]
        public KeywordResult IsDisabled(string resourceId)
        {
            bool isDisable;
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            try
            {
                isDisable = _android.IsEnabled(new UiSelector().ResourceId(@resourceId));
                if (!isDisable)
                {
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();

            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "resourceId is not Exits ";
                kr.AdditionalInfo = ex.ToString();
            }

            

            return kr;
        }

        [KeywordDescription("Double touch object with given text")]
        [KeywordDisplayName("Double Touch Text")]
        [KeywordParameters("text", "Text to double touch")]
        [SampleScript("Android.Double Touch Text (OK)")]
        public KeywordResult DoubleTouchText(string text)
        {
            text = text.Replace("(", @"\(").Replace(")", @"\)");
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                var selector = new UiSelector().Text(text);

                // First tap
                if (_android.Click(selector))
                {
                    System.Threading.Thread.Sleep(200); // Short delay between taps

                    // Second tap
                    if (_android.Click(selector))
                    {
                        return kr;
                    }
                }

                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Double touch failed with given text :: {text}";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();
            }

            return kr;
        }

        [KeywordDescription("Double Touch object with XPath")]
        [KeywordDisplayName("Double Touch XPath")]
        [KeywordParameters("xPath", "XPath of element to double touch")]
        [SampleScript("Android.Double Touch XPath (//node[@resource-id='loginUserName']/node[2])")]
        public KeywordResult DoubleTouchXPath(string xPath)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_android.Click(xPath))
                {
                    Thread.Sleep(150); // Short delay between taps
                    if (_android.Click(xPath))
                    {
                        return kr;
                    }
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Double Touch XPath failed with given XPath :: {xPath}";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();
            }

            return kr;
        }

        [KeywordDescription("Double Touch Object by given resourceid")]
        [KeywordDisplayName("Double Touch ID")]
        [KeywordParameters("resourceid", "resourceid to double touch")]
        [SampleScript("Android.Double Touch ID (com.hp.print.authagent.secureaccess:id/btnSignInIDPW)")]
        public KeywordResult DoubleTouchID(string resourceid)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                var selector = new UiSelector().ResourceId(@resourceid);
                if (_android.Click(selector))
                {
                    Thread.Sleep(150); // Short delay between taps
                    if (_android.Click(selector))
                    {
                        return kr;
                    }
                }
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Double Touch ID failed with given resourceid :: {@resourceid}";
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();
            }

            return kr;
        }


        private string Getbounds(string xpath)
        {
            string androidUi = _android.GetUIDump();


            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(androidUi);
            string xmldoc = xmlDoc.DocumentElement.Name;

            var ctryNameXPath = xpath;

            XmlNode countryNode = xmlDoc.SelectSingleNode(ctryNameXPath);

            string boundValue = countryNode.Attributes["bounds"].Value;

            return boundValue;
        }
        public bool AreBoundsWithinManualBounds(string listboxbounds, string xpathbounds)
        {
            int listboxheight = Convert.ToInt32(listboxbounds.Split(']')[1].Split(',')[1]);
            int xpathheight = Convert.ToInt32(xpathbounds.Split(']')[1].Split(',')[1]);

            if (Convert.ToInt32(listboxheight) < xpathheight)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        [KeywordDescription("Clear Text From The Text Field")]
        [KeywordDisplayName("Clear text")]
        [KeywordParameters("resourceid", "resourceid of text field")]
        [KeywordParameters("classname", "classname of text field")]
        [SampleScript("Android.Clear text (com.microsoft.emmx:id/url_bar,android.widget.EditText)")]
        public KeywordResult Cleartext(string resourceid, string classname)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                var selector = new UiSelector().ResourceId(resourceid).ClassName(classname);

                if (_android.DoesScreenContains(selector))
                {
                    _android.SetText(selector, "");
                }
                else
                {
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = GetScreenCapture();
                kr.Output = $"Clear text failed with given resourceid :: {@resourceid}";
                }
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();
            }

            return kr;
        }

        [KeywordDescription("Clear Text From The Text Field")]
        [KeywordDisplayName("Clear text")]
        [KeywordParameters("resourceid", "resourceid of the text field")]
        [SampleScript("Android.Clear text (com.microsoft.emmx:id/url_bar)")]
        public KeywordResult Cleartext(string resourceid)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            string classname = "android.widget.EditText";
            try
            {
                var selector = new UiSelector().ResourceId(resourceid).ClassName(classname);

                if (_android.DoesScreenContains(selector))
                {
                    _android.SetText(selector, "");
                }
                else
                {
                    kr.Result = KeywordResults.Fail;
                    kr.ScreenShot = GetScreenCapture();
                    kr.Output = $"Clear text failed with given resourceid :: {@resourceid}";
                }
            }
            catch (WebException wex)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Connection lost during test.";
                kr.AdditionalInfo = wex.ToString();
                Logger.Error("Connection lost during test. Try to reconnect.", wex);
                ReConnect();
            }

            return kr;
        }

        [KeywordDescription("Check if application is installed; if not, install it via ADB using local APK")]
        [KeywordDisplayName("Verify And Install Application")]
        [KeywordParameters("packageName", "Application package name")]
        [KeywordParameters("adbFilePath", "Local path to ADB executable")]
        [SampleScript("Android.Verify And Install Application (com.example.app, C:\\path\\to\\adb)")]

        public KeywordResult VerifyAndInstallApplication(string packageName, string adbFilePath)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);

            // Validate parameters
            if ( string.IsNullOrWhiteSpace(packageName))
            {
                result.Result = KeywordResults.Error;
                result.Output = "Package name cannot be null or empty.";
                Logger.Error(result.Output);
                return result;
            }

            if (string.IsNullOrWhiteSpace(adbFilePath))
            {
                result.Result = KeywordResults.Error;
                result.Output = "ADB file path cannot be null or empty.";
                Logger.Error(result.Output);
                return result;
            }

            if (!Directory.Exists(adbFilePath))
            {
                result.Result = KeywordResults.Error;
                result.Output = $"ADB directory does not exist: {adbFilePath}";
                Logger.Error(result.Output);
                return result;
            }

            string adbExePath = Path.Combine(adbFilePath, "adb.exe");
            if (!File.Exists(adbExePath))
            {
                result.Result = KeywordResults.Error;
                result.Output = $"ADB executable not found at: {adbExePath}";
                Logger.Error(result.Output);
                return result;
            }

            try
            {
                string ExecuteADB(string arguments)
                {
                    using (var process = new Process())
                    {
                        process.StartInfo.FileName = adbExePath;
                        process.StartInfo.Arguments = arguments;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.RedirectStandardError = true;
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.CreateNoWindow = true;

                        process.Start();

                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();

                        if (!process.WaitForExit(30000))
                        {
                            process.Kill();
                            throw new TimeoutException("ADB command timed out.");
                        }

                        return string.IsNullOrEmpty(output) ? error : output;
                    }
                }

                // STEP 1: Check if already installed
                string checkOutput = ExecuteADB($"shell pm list packages {packageName}");

                if (!string.IsNullOrEmpty(checkOutput) && checkOutput.Contains(packageName))
                {
                    result.Output = $"Application '{packageName}' is already installed on the device.";
                    Logger.Debug(result.Output);
                    return result;
                }

                Logger.Debug($"Application '{packageName}' not installed. Opening Play Store...");

                // STEP 2: Open Play Store
                ExecuteADB($"shell am start -a android.intent.action.VIEW -d \"market://details?id={packageName}\"");

                // STEP 3: Wait for Play Store to load and click Install
                const int playStoreLoadTimeout = 10;
                DateTime loadEndTime = DateTime.Now.AddSeconds(playStoreLoadTimeout);

                while (DateTime.Now < loadEndTime)
                {
                    if (_android.DoesScreenContains(new UiSelector().Text("Install")))
                    {
                        if (_android.Click(new UiSelector().Text("Install")))
                        {
                            Logger.Debug("Install button clicked successfully.");
                            break;
                        }
                    }
                    Thread.Sleep(500);
                }

                if (DateTime.Now >= loadEndTime)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Failed to find or click Install button for '{packageName}' within {playStoreLoadTimeout} seconds.";
                    Logger.Error(result.Output);
                    return result;
                }

                // STEP 4: Wait for installation to complete
                const int maxWaitTime = 300; // 5 minutes
                DateTime installEndTime = DateTime.Now.AddSeconds(maxWaitTime);
                int checkInterval = 5000; // Check every 5 seconds
                DateTime nextCheck = DateTime.Now;

                while (DateTime.Now < installEndTime)
                {
                    if (DateTime.Now >= nextCheck)
                    {
                        string packageCheck = ExecuteADB($"shell pm list packages {packageName}");

                        if (!string.IsNullOrEmpty(packageCheck) && packageCheck.Contains(packageName))
                        {
                            result.Output = $"Application '{packageName}' installed successfully.";
                            Logger.Debug(result.Output);
                            return result;
                        }

                        // Log progress if available
                        try
                        {
                            if (_android.DoesScreenContains(new UiSelector().ResourceId("com.android.vending:id/progress_bar")))
                            {
                                string progressText = _android.GetText(new UiSelector().ResourceId("com.android.vending:id/progress_bar"));
                                if (!string.IsNullOrEmpty(progressText))
                                {
                                    Logger.Debug($"Installation Progress: {progressText}");
                                }
                            }
                        }
                        catch
                        {
                            int elapsed = (int)(DateTime.Now - installEndTime.AddSeconds(-maxWaitTime)).TotalSeconds;
                            Logger.Debug($"Installing '{packageName}' ... {elapsed} seconds elapsed");
                        }

                        nextCheck = DateTime.Now.AddMilliseconds(checkInterval);
                    }

                    Thread.Sleep(100);
                }

                // Timeout
                result.Result = KeywordResults.Fail;
                result.Output = $"Installation timeout for '{packageName}' after {maxWaitTime} seconds.";
                Logger.Error(result.Output);
                return result;
            }
            catch (Exception ex)
            {
                result.Result = KeywordResults.Error;
                result.Output = $"Error while installing '{packageName}': {ex.Message}";
                result.AdditionalInfo = ex.ToString();
                Logger.Error(result.Output, ex);
                return result;
            }
        }
    }
}

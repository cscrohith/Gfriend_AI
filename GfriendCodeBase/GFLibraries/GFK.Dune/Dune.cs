using HP.DeviceAutomation;
using HP.DeviceAutomation.Dune;
using HP.DeviceAutomation.Dune.QmlTest;
using HP.GFriend.External.GRWM.Jedi.DeviceUsage;
using HP.GFriend.GFLogger;
using HP.GFriend.Keywords;
using HP.GFriend.Keywords.Event;
using HP.GFriend.Utils.Charter;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Logger = HP.GFriend.GFLogger.Logger;

namespace GFK.Dune
{
    [LibraryDescription("Currently, Only dial type UI is supported. Due to low performance of dial UI" +
        "there is default delay of 1 second in Dial operation.\r\n" +
        "If you want to increase delay use Set Delay Scale keyword.")]
    public class Dune : IGFLibrary
    {
        private System.Timers.Timer _memoryTimer;
        private HttpClient _httpClient;
        private string _csvPath;
        public string _testCaseName = "";
        private DeviceUnderTest _dut;
        private DuneDevice _device;
        private string _outputDir;
        private bool _logAttached = false;
        private ProSelectDialControlPanel _dialUI;
        private WorkflowControlPanel _workflowUI;

        private int _defaultDialDelay = 1;
        private int _defaultCPDelay = 3;
        private int _delayScaleFactor = 1;
        private static MemoryUsageItem _memoryUsage = null;
        private bool _disableScreenShotForFails = false;

        private EventHandler<HP.DeviceAutomation.LogEventArgs> logTrace;
        private EventHandler<HP.DeviceAutomation.LogEventArgs> logDebug;
        private EventHandler<HP.DeviceAutomation.LogEventArgs> logWarn;
        private EventHandler<HP.DeviceAutomation.LogEventArgs> logError;

        public void Dispose()
        {
            if (_device != null)
            {
                _device.Dispose();
                _httpClient?.Dispose();
                _memoryTimer?.Dispose();
                CommonExecutionInfo.RemoveSharedObject("dune:" + _dut.DeviceId);
            }
            if (_logAttached)
            {
                HP.DeviceAutomation.Logger.OnTrace -= logTrace;
                HP.DeviceAutomation.Logger.OnDebug -= logDebug;
                HP.DeviceAutomation.Logger.OnWarn -= logWarn;
                HP.DeviceAutomation.Logger.OnError -= logError;
                _logAttached = false;
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
            return "Dune";
        }

        public void Initialize(DeviceUnderTest dut, string outputDir)
        {
            if (!_logAttached)
            {
                logTrace = delegate (object s, HP.DeviceAutomation.LogEventArgs e) { GFriendLoggerServices.GFLogger.LogTrace(e.Message); };
                logDebug = delegate (object s, HP.DeviceAutomation.LogEventArgs e) { GFriendLoggerServices.GFLogger.LogDebug(e.Message); };
                logWarn = delegate (object s, HP.DeviceAutomation.LogEventArgs e) { GFriendLoggerServices.GFLogger.LogWarn(e.Message); };
                logError = delegate (object s, HP.DeviceAutomation.LogEventArgs e) { GFriendLoggerServices.GFLogger.LogError(e.Message); };

                HP.DeviceAutomation.Logger.OnTrace += logTrace;
                HP.DeviceAutomation.Logger.OnDebug += logDebug;
                HP.DeviceAutomation.Logger.OnWarn += logWarn;
                HP.DeviceAutomation.Logger.OnError += logError;
                _logAttached = true;
            }
            _httpClient = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
                {
                    if (message.RequestUri.Host == _dut.DeviceAddress ||
                        message.RequestUri.Host.Contains("deviceaddress"))
                    {
                        return true;
                    }
                    return sslPolicyErrors == System.Net.Security.SslPolicyErrors.None;
                }
            });
            _dut = dut;
            _outputDir = outputDir;
            _device = new DuneDevice(_dut.DeviceAddress, _dut.AdminPassword);
            if(_device.ControlPanel.GetType().Equals(typeof(ProSelectDialControlPanel)))
            {
                Logger.Trace("ProSelect Dial UI");
                _dialUI = _device.ControlPanel as ProSelectDialControlPanel;
            }
            else if (_device.ControlPanel.GetType().Equals(typeof(WorkflowControlPanel)))
            {
                Logger.Trace("Workflow Control Panel");
                _workflowUI = _device.ControlPanel as WorkflowControlPanel;
            }
            CommonExecutionInfo.SetSharedObject("dune:" + _dut.DeviceId, _device);

        }

        /// <summary>
        /// Capture Dune Screen and Convert to byte
        /// </summary>
        private byte[] CaptureScreen()
        {
            Image ControlPanelImage = _device.ControlPanel.ScreenCapture();
            try
            {
                ImageConverter _imageConverter = new ImageConverter();
                byte[] _imageByte = (byte[])_imageConverter.ConvertTo(ControlPanelImage, typeof(byte[]));
                return _imageByte;
            }
            catch (Exception ex)
            {
                Logger.Error("Screen capture fail", ex);
                return null;
            }
        }

        #region General Keywords without UI
        [KeywordDescription("Set Mechless mode with device")]
        [KeywordDisplayName("Enable Mechless Mode")]
        [SampleScript("Dune.Enable Mechless Mode")]
        public KeywordResult EnableMechlessMode()
        {
            try
            {
                if (_device.EngineManagement.SetEngineWorkingMode(DuneEngineManager.EngineWorkingMode.Mechless))
                {
                    return new KeywordResult(KeywordResults.Pass, "Engine is set to mechless mode");
                }
                return new KeywordResult(KeywordResults.Fail, "Set to mechless mode fail.");
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Error during setting mechless mode");
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        [KeywordDescription("Set back to normal mode with device")]
        [KeywordDisplayName("Disable Mechless Mode")]
        [SampleScript("Dune.Disable Mechless Mode")]
        public KeywordResult DisableMechlessMode()
        {
            try
            {
                if (_device.EngineManagement.SetEngineWorkingMode(DuneEngineManager.EngineWorkingMode.Normal))
                {
                    return new KeywordResult(KeywordResults.Pass, "Engine is set to normal mode");
                }
                return new KeywordResult(KeywordResults.Fail, "Set to normal mode fail.");
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Error during setting normal mode");
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        [KeywordDescription("Set bypassing OOBE prompt with udw command")]
        [KeywordDisplayName("Bypassing OOBE Prompt")]
        [SampleScript("Dune.Bypassing OOBE Prompt")]
        public KeywordResult ByPassingOOBEPrompt()
        {
            try
            {
                _device.UDWManagement.ByPassOOBEPrompt();
                return new KeywordResult(KeywordResults.Pass, "Bypassing OOBE prompt is set to ture");
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Bypassing OOBE prompt set to true failed");
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        [KeywordDescription("User setting reset via udw command (ResetManager PUB_performReset 1)")]
        [KeywordDisplayName("Reset User Setting")]
        [SampleScript("Dune.Reset User Setting")]
        public KeywordResult ResetUserSetting()
        {
            try
            {
                _device.PowerManagement.Reset(DunePowerManager.ResetValue.UserSettingReset);
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Error during reset");
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        [KeywordDescription("User data reset via udw command (ResetManager PUB_performReset 2)")]
        [KeywordDisplayName("Reset User Data")]
        [SampleScript("Dune.Reset User Data")]
        public KeywordResult ResetUserData()
        {
            try
            {
                _device.PowerManagement.Reset(DunePowerManager.ResetValue.UserDataReset);
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Error during reset");
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        [KeywordDescription("User data reset via udw command (ResetManager PUB_performReset 3)")]
        [KeywordDisplayName("Reset Factory Data")]
        [SampleScript("Dune.Reset Factory Data")]
        public KeywordResult ResetFactoryData()
        {
            try
            {
                _device.PowerManagement.Reset(DunePowerManager.ResetValue.FactoryDataReset);
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Error during reset");
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        [KeywordDescription("User data reset via udw command (ResetManager PUB_performReset 4)")]
        [KeywordDisplayName("Reset Full Factory Data")]
        [SampleScript("Dune.Reset Full Factory Data")]
        public KeywordResult ResetFullFactoryData()
        {
            try
            {
                _device.PowerManagement.Reset(DunePowerManager.ResetValue.FullFactoryDataReset);
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Error during reset");
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        [KeywordDescription("Disables automatic screenshot capture for failed scenarios. \r\n you can still manually capture screenshots using the Dune.Capture Screen Shot keyword as needed ")]
        [KeywordDisplayName("Disable ScreenShot For Fails")]
        [SampleScript("Dune.Disable ScreenShot For Fails")]
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
        [SampleScript("Dune.Enable ScreenShot For Fails")]
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
        #endregion

        #region General UI Keyword for all types of UI
        [KeywordDescription("Go to the home screen")]
        [KeywordDisplayName("Go To Home Screen")]
        [SampleScript("Dune.Go To Home Screen")]
        public KeywordResult GoToHomeScreen()
        {
            if(_device.ControlPanel.IsHomeScreen())
            {
                Logger.Trace("Already in Home screen.");
                return new KeywordResult(KeywordResults.Pass);
            }
            try
            {
                _device.ControlPanel.GoToHomeScreen();
                Thread.Sleep(TimeSpan.FromSeconds(_defaultDialDelay));
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Error during going to home screen");
                error.AdditionalInfo = ex.ToString();
                Logger.Error(error.Output, ex);
                return error;
            }
            return IsHomeScreen();
        }

        [KeywordDescription("Check if device displays home screen")]
        [KeywordDisplayName("Is Home Screen")]
        [SampleScript("Dune.Is Home Screen")]
        public KeywordResult IsHomeScreen()
        {
            try
            {
                if (_device.ControlPanel.IsHomeScreen())
                {
                    return new KeywordResult(KeywordResults.Pass);
                }
                else
                {
                    KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                    if(!_disableScreenShotForFails)
                    {
                        fail.ScreenShot = CaptureScreen();
                    }
                    fail.Output = "Device is not in Home screen";
                    return fail;
                }
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Error during checking home screen");
                error.AdditionalInfo = ex.ToString();
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        [KeywordDescription("Capture Screen Shot of device and save to PC")]
        [KeywordDisplayName("Capture Screen Shot")]
        [KeywordParameters("filename", "filename to save")]
        [SampleScript("Dune.Capture Screen Shot (ScreenshotName)")]
        public KeywordResult CaptureScreenShot(string filename)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                kr.ScreenShot = CaptureScreen();
                if (kr.ScreenShot != null && !string.IsNullOrEmpty(filename))
                {
                    string saveTo = Path.Combine(_outputDir, filename);
                    File.WriteAllBytes(saveTo, kr.ScreenShot);
                    if (File.Exists(saveTo))
                    {
                        return kr;
                    }
                }
                kr.Result = KeywordResults.Fail;
                kr.Output = $"Capture screen shot failed with given filename :: {filename}";
                return kr;
            }

            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Capture screen shot failed with given filename :: {filename}";
                Logger.Error($"Captyre screen error({filename})", ex);
                return kr;
            }
        }

        #endregion

        #region ProSelect Dial UI
        [KeywordDescription("Set dial delay scacle for applying all dail operation keywords in library." +
            "This scale facor will be multiplied of all timeout arguments in keywords." +
            "For example, if scale factor is set to 2 and dial operation wait default 2 seconds after executing operation.")]
        [KeywordDisplayName("Set Delay Scale")]
        [KeywordParameters("scaleFactor", "scale factor. Give 0 if you want no delay")]
        [SampleScript("Dune.Set Delay Scale (10)")]
        public KeywordResult SetDelayScale(string scaleFactor)
        {
            if (!int.TryParse(scaleFactor.Trim(), out _delayScaleFactor))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Scale factor must be a number.";
                return error;
            }
            _defaultDialDelay = _defaultDialDelay * _delayScaleFactor;
            KeywordResult pass = new KeywordResult(KeywordResults.Pass);
            pass.Output = $"Scale factor is set to {_delayScaleFactor}";
            return pass;
        }


        [KeywordDescription("Move Dial to Left. Only works with ProSelect Dial UI.")]
        [KeywordDisplayName("Dial Left")]
        [SampleScript("Dune.Dial Left")]
        public KeywordResult DialLeft()
        {
            try
            {
                _dialUI.DialLeft();
                Thread.Sleep(TimeSpan.FromSeconds(_defaultDialDelay));
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Error during moving dial");
                error.AdditionalInfo = ex.ToString();
                Logger.Error(error.Output, ex);
                return error;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Move dial to left until reach the given menu (witin 10 times). Only works with ProSelect Dial UI.")]
        [KeywordDisplayName("Dial Left Until")]
        [KeywordParameters("menu", "Menu to search")]
        [SampleScript("Dune.Dial Left Until (Print)")]
        public KeywordResult DialLeftUntil(string menu)
        {
            return DialLeftUntil(menu, "10");
        }

        [KeywordDescription("Move dial to left until reach the given menu. Only works with ProSelect Dial UI.")]
        [KeywordDisplayName("Dial Left Until")]
        [KeywordParameters("menu", "Menu to search")]
        [KeywordParameters("maxTry", "Maximum try to find menu")]
        [SampleScript("Dune.Dial Left Until (Menu,3)")]
        public KeywordResult DialLeftUntil(string menu, string maxTry)
        {
            if (!int.TryParse(maxTry.Trim(), out int maxCount))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Max Try must be a number";
            }
            maxCount++;
            int count = 0;
            while (true)
            {
                string selected = _dialUI.GetCurrentSelectedMenu();
                if (menu.Trim().Equals(selected))
                {
                    break;
                }

                if (count > maxCount)
                {
                    KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                    if (!_disableScreenShotForFails)
                    {
                        fail.ScreenShot = CaptureScreen();
                    }
                    fail.Output = "Can not find given menu";
                    Logger.Error(fail.Output);
                    return fail;
                }

                DialLeft();
                count++;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Move Dial to Right. Only works with ProSelect Dial UI.")]
        [KeywordDisplayName("Dial Right")]
        [SampleScript("Dune.Dial Right")]
        public KeywordResult DialRight()
        {
            try
            {
                _dialUI.DialRight();
                Thread.Sleep(TimeSpan.FromSeconds(_defaultDialDelay));
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Error during moving dial");
                error.AdditionalInfo = ex.ToString();
                Logger.Error(error.Output, ex);
                return error;
            }
            return new KeywordResult(KeywordResults.Pass);
        }


        [KeywordDescription("Move dial to right until reach the given menu (within 10 times). Only works with ProSelect Dial UI.")]
        [KeywordDisplayName("Dial Right Until")]
        [KeywordParameters("menu", "Menu to search")]
        [SampleScript("Dune.Dial Right Until (Help)")]
        public KeywordResult DialRightUntil(string menu)
        {
            return DialRightUntil(menu, "10");
        }

        [KeywordDescription("Move dial to right until reach the given menu. Only works with ProSelect Dial UI.")]
        [KeywordDisplayName("Dial Right Until")]
        [KeywordParameters("menu", "Menu to search")]
        [KeywordParameters("maxTry", "Maximum try to find menu")]
        [SampleScript("Dune.Dial Right Until (Help,3)")]
        public KeywordResult DialRightUntil(string menu, string maxTry)
        {
            if (!int.TryParse(maxTry.Trim(), out int maxCount))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Max Try must be a number";
            }
            maxCount++;
            int count = 0;
            while (true)
            {
                string selected = _dialUI.GetCurrentSelectedMenu();
                if (menu.Trim().Equals(selected))
                {
                    break;
                }

                if (count > maxCount)
                {
                    KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                    if (!_disableScreenShotForFails)
                    {
                        fail.ScreenShot = CaptureScreen();
                    }
                    fail.Output = "Can not find given menu";
                    Logger.Error(fail.Output);
                    return fail;
                }

                DialRight();
                count++;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Click the dial. Only works with ProSelect Dial UI.")]
        [KeywordDisplayName("Dial Click")]
        [SampleScript("Dune.Dial Click")]
        public KeywordResult DialClick()
        {
            try
            {
                _dialUI.DialClick();
                Thread.Sleep(TimeSpan.FromSeconds(_defaultDialDelay));
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Error during clicking dial");
                error.AdditionalInfo = ex.ToString();
                Logger.Error(error.Output, ex);
                return error;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Long press the dial. Only works with ProSelect Dial UI.")]
        [KeywordDisplayName("Dial Long Press")]
        [SampleScript("Dune.Dial Long Press")]
        public KeywordResult DialLongPress()
        {
            try
            {
                _dialUI.DialLongPress();
                Thread.Sleep(TimeSpan.FromSeconds(_defaultDialDelay));
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Error during long pressing dial");
                error.AdditionalInfo = ex.ToString();
                Logger.Error(error.Output, ex);
                return error;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [GetKeyword]
        [KeywordDescription("Get current slected menu text. Only works with ProSelect Dial UI.")]
        [KeywordDisplayName("Get Selected Menu")]
        [KeywordParameters("saveTo", "Variable name to save value")]
        [SampleScript("Dune.Get Selected Menu (VariableName)")]
        public KeywordResult GetSelectedMenu(string variable)
        {
            try
            {
                string text = _dialUI.GetCurrentSelectedMenu();
                CommonExecutionInfo.SetVariable(variable, text);
                return new KeywordResult(KeywordResults.Pass, text);
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Error during get text");
                error.AdditionalInfo = ex.ToString();
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        [KeywordDescription("Move Dial to Right. Only works in Proselect with hybrid UI Theme Device.")]
        [KeywordDisplayName("Keypad Right")]
        [SampleScript("Dune.Keypad Right")]
        public KeywordResult KeypadRight()
        {
            try
            {
                _dialUI.keypadRight();
                Thread.Sleep(TimeSpan.FromSeconds(_defaultDialDelay));
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Error during moving dial");
                error.AdditionalInfo = ex.ToString();
                Logger.Error(error.Output, ex);
                return error;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Move Dial to Left. Only works in Proselect with hybrid UI Theme Device.")]
        [KeywordDisplayName("Keypad Left")]
        [SampleScript("Dune.Keypad Left")]
        public KeywordResult KeypadLeft()
        {
            try
            {
                _dialUI.keypadLeft();
                Thread.Sleep(TimeSpan.FromSeconds(_defaultDialDelay));
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Error during moving dial");
                error.AdditionalInfo = ex.ToString();
                Logger.Error(error.Output, ex);
                return error;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Keypad Home. Only works in Proselect with hybrid UI Theme Device.")]
        [KeywordDisplayName("Keypad Home")]
        [SampleScript("Dune.Keypad Home")]
        public KeywordResult KeypadHome()
        {
            try
            {
                _dialUI.keypadHome();
                Thread.Sleep(TimeSpan.FromSeconds(_defaultDialDelay));
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Error during going to home screen");
                error.AdditionalInfo = ex.ToString();
                Logger.Error(error.Output, ex);
                return error;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Keypad Back. Only works in Proselect with hybrid UI Theme Device.")]
        [KeywordDisplayName("Keypad Back")]
        [SampleScript("Dune.Keypad Back")]
        public KeywordResult KeypadBack()
        {
            try
            {
                _dialUI.keypadBack();
                Thread.Sleep(TimeSpan.FromSeconds(_defaultDialDelay));
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Error during going to Back screen");
                error.AdditionalInfo = ex.ToString();
                Logger.Error(error.Output, ex);
                return error;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Move Dial to Up. Only works in Proselect with hybrid UI Theme Device.")]
        [KeywordDisplayName("Keypad Up")]
        [SampleScript("Dune.Keypad Up")]
        public KeywordResult KeypadUp()
        {
            try
            {
                _dialUI.keypadUp();
                Thread.Sleep(TimeSpan.FromSeconds(_defaultDialDelay));
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Error during moving dial");
                error.AdditionalInfo = ex.ToString();
                Logger.Error(error.Output, ex);
                return error;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Move Dial to Down. Only works in Proselect with hybrid UI Theme Device.")]
        [KeywordDisplayName("Keypad Down")]
        [SampleScript("Dune.Keypad Down")]
        public KeywordResult keypadDown()
        {
            try
            {
                _dialUI.keypadDown();
                Thread.Sleep(TimeSpan.FromSeconds(_defaultDialDelay));
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Error during moving dial");
                error.AdditionalInfo = ex.ToString();
                Logger.Error(error.Output, ex);
                return error;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Click Keypad Enter. Only works in Proselect with hybrid UI Theme Device.")]
        [KeywordDisplayName("Keypad Enter")]
        [SampleScript("Dune.Keypad Enter")]
        public KeywordResult KeypadEnter()
        {
            try
            {
                _dialUI.keypadEnter();
                Thread.Sleep(TimeSpan.FromSeconds(_defaultDialDelay));
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Error during keypad Enter");
                error.AdditionalInfo = ex.ToString();
                Logger.Error(error.Output, ex);
                return error;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Click Keypad Cancel. Only works in Proselect with hybrid UI Theme Device.")]
        [KeywordDisplayName("Keypad Cancel")]
        [SampleScript("Dune.Keypad Cancel")]
        public KeywordResult KeypadCancel()
        {
            try
            {
                _dialUI.keypadCancel();
                Thread.Sleep(TimeSpan.FromSeconds(_defaultDialDelay));
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Error during keypad Cancel");
                error.AdditionalInfo = ex.ToString();
                Logger.Error(error.Output, ex);
                return error;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Move dial to left until reach the given menu (witin 10 times). Only works in Proselect with hybrid UI Theme Device.")]
        [KeywordDisplayName("KeyPad Left Until")]
        [KeywordParameters("menu", "Menu to search")]
        [SampleScript("Dune.KeyPad Left Until (Print)")]
        public KeywordResult KeyPadLeftUntil(string menu)
        {
            return KeyPadLeftUntil(menu, "10");
        }

        [KeywordDescription("Move dial to left until reach the given menu. Only works in Proselect with hybrid UI Theme Device.")]
        [KeywordDisplayName("KeyPad Left Until")]
        [KeywordParameters("menu", "Menu to search")]
        [KeywordParameters("maxTry", "Maximum try to find menu")]
        [SampleScript("Dune.KeyPad Left Until (Menu,3)")]
        public KeywordResult KeyPadLeftUntil(string menu, string maxTry)
        {
            if (!int.TryParse(maxTry.Trim(), out int maxCount))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Max Try must be a number";
            }
            maxCount++;
            int count = 0;
            while (true)
            {
                string selected = _dialUI.GetCurrentSelectedMenu();
                if (menu.Trim().Equals(selected))
                {
                    break;
                }

                if (count > maxCount)
                {
                    KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                    if (!_disableScreenShotForFails)
                    {
                        fail.ScreenShot = CaptureScreen();
                    }
                    fail.Output = "Can not find given menu";
                    Logger.Error(fail.Output);
                    return fail;
                }

                KeypadLeft();
                count++;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Move dial to right until reach the given menu (within 10 times). Only works in Proselect with hybrid UI Theme Device.")]
        [KeywordDisplayName("Keypad Right Until")]
        [KeywordParameters("menu", "Menu to search")]
        [SampleScript("Dune.Keypad Right Until (Help)")]
        public KeywordResult KeypadRightUntil(string menu)
        {
            return KeypadRightUntil(menu, "10");
        }

        [KeywordDescription("Move dial to right until reach the given menu. Only works with ProSelect Dial UI.")]
        [KeywordDisplayName("Keypad Right Until")]
        [KeywordParameters("menu", "Menu to search")]
        [KeywordParameters("maxTry", "Maximum try to find menu")]
        [SampleScript("Dune.Keypad Right Until (Help,3)")]
        public KeywordResult KeypadRightUntil(string menu, string maxTry)
        {
            if (!int.TryParse(maxTry.Trim(), out int maxCount))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Max Try must be a number";
            }
            maxCount++;
            int count = 0;
            while (true)
            {
                string selected = _dialUI.GetCurrentSelectedMenu();
                if (menu.Trim().Equals(selected))
                {
                    break;
                }

                if (count > maxCount)
                {
                    KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                    if (!_disableScreenShotForFails)
                    {
                        fail.ScreenShot = CaptureScreen();
                    }
                    fail.Output = "Can not find given menu";
                    Logger.Error(fail.Output);
                    return fail;
                }

                KeypadRight();
                count++;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Move dial to down until reach the given menu (within 10 times). Only works in Proselect with hybrid UI Theme Device.")]
        [KeywordDisplayName("Keypad Down Until")]
        [KeywordParameters("menu", "Menu to search")]
        [SampleScript("Dune.Keypad Down Until (Help)")]
        public KeywordResult KeypadDownUntil(string menu)
        {
            return KeypadDownUntil(menu, "10");
        }

        [KeywordDescription("Move dial to down until reach the given menu. Only works in Proselect with hybrid UI Theme Device.")]
        [KeywordDisplayName("Keypad Down Until")]
        [KeywordParameters("menu", "Menu to search")]
        [KeywordParameters("maxTry", "Maximum try to find menu")]
        [SampleScript("Dune.Keypad Down Until (Menu,3)")]
        public KeywordResult KeypadDownUntil(string menu, string maxTry)
        {
            if (!int.TryParse(maxTry.Trim(), out int maxCount))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Max Try must be a number";
            }
            maxCount++;
            int count = 0;
            while (true)
            {
                string selected = _dialUI.GetCurrentSelectedMenu();
                if (menu.Trim().Equals(selected))
                {
                    break;
                }

                if (count > maxCount)
                {
                    KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                    if (!_disableScreenShotForFails)
                    {
                        fail.ScreenShot = CaptureScreen();
                    }
                    fail.Output = "Can not find given menu";
                    Logger.Error(fail.Output);
                    return fail;
                }

                keypadDown();
                count++;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Move dial to up until reach the given menu (within 10 times). Only works in Proselect with hybrid UI Theme Device.")]
        [KeywordDisplayName("Keypad Up Until")]
        [KeywordParameters("menu", "Menu to search")]
        [SampleScript("Dune.Keypad Up Until (Help)")]
        public KeywordResult KeypadUpUntil(string menu)
        {
            return KeypadUpUntil(menu, "10");
        }

        [KeywordDescription("Move dial to up until reach the given menu. Only works in Proselect with hybrid UI Theme Device.")]
        [KeywordDisplayName("Keypad Up Until")]
        [KeywordParameters("menu", "Menu to search")]
        [KeywordParameters("maxTry", "Maximum try to find menu")]
        [SampleScript("Dune.Keypad Up Until (Menu,3)")]
        public KeywordResult KeypadUpUntil(string menu, string maxTry)
        {
            if (!int.TryParse(maxTry.Trim(), out int maxCount))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Max Try must be a number";
            }
            maxCount++;
            int count = 0;
            while (true)
            {
                string selected = _dialUI.GetCurrentSelectedMenu();
                if (menu.Trim().Equals(selected))
                {
                    break;
                }

                if (count > maxCount)
                {
                    KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                    if (!_disableScreenShotForFails)
                    {
                        fail.ScreenShot = CaptureScreen();
                    }
                    fail.Output = "Can not find given menu";
                    Logger.Error(fail.Output);
                    return fail;
                }

                KeypadUp();
                count++;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        #endregion

        #region Workflow CP UI
        [KeywordDescription("Click UI element with given text \r\n" +
            "Note:If the text contains special characters like *, (, ), ., %, -, /, etc., this keyword will not work. \r\n" +
            "In such cases, we recommend to use 'Click Id' keyword.")]
        [KeywordDisplayName("Click Text")]
        [KeywordParameters("text", "text to click")]
        [SampleScript("Dune.Click Text (Menu)")]
        public KeywordResult ClickText(string text)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                _workflowUI.ClickText(text);
                kr.Result = KeywordResults.Pass;
                return kr;
            }
            catch (DuneControlPanelOperationException ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = CaptureScreen();
                }

                kr.Output = $"Click text failed with given text :: {ex.Data} text = {text}";
                Logger.Error(kr.Output, ex);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Unexpected error";
                Logger.Error(kr.Output, ex);
                return kr;
            }
        }

        [KeywordDescription("Click UI element with given text \r\n" +
            "Note:If the text contains special characters like *, (, ), ., %, -, /, etc., this keyword will not work. \r\n" +
            "In such cases, we recommend to use 'Click Id' keyword.")]
        [KeywordDisplayName("Click Text And Wait For Transition")]
        [KeywordParameters("text", "text to click")]
        [KeywordParameters("delay", "Give some time(in ms) after a click event to load the next screen. The default is 700 milliSeconds")]
        [SampleScript("Dune.Click Text (Menu) or Dune.Click Text (Menu, 700)")]
        public KeywordResult ClickTextAndWaitForTransition(string text, string delay = "700")
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int loadDelay = 700;
            try
            {
                bool _ = int.TryParse(delay, out loadDelay);
                _workflowUI.ClickText(text, loadDelay);
                kr.Result = KeywordResults.Pass;
                return kr;
            }
            catch (DuneControlPanelOperationException ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = CaptureScreen();
                }

                kr.Output = $"Click text failed with given text :: {ex.Data} text = {text}";
                Logger.Error(kr.Output, ex);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Unexpected error";
                Logger.Error(kr.Output, ex);
                return kr;
            }
        }

        [KeywordDescription("Click UI element with querySelector")]
        [KeywordDisplayName("Click")]
        [KeywordParameters("querySelector", "querySelector of element to click")]
        [SampleScript("Dune.Click (#7db992ba-557a-461c-b941-6023aa8cfa34)")]
        public KeywordResult Click(string querySelector)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if(_workflowUI.IsExist(querySelector + " MouseArea"))
                {
                    querySelector = querySelector + " MouseArea";
                }
                _workflowUI.WaitFor(querySelector, _defaultCPDelay);
                _workflowUI.Click(querySelector);
                kr.Result = KeywordResults.Pass;
                return kr;
            }
            catch (DuneControlPanelOperationException ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = CaptureScreen();
                }
                kr.Output = $"Click failed with given query selector :: {ex.Data} text = {querySelector}";
                Logger.Error(kr.Output, ex);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Unexpected error";
                Logger.Error(kr.Output, ex);
                return kr;
            }
        }

        [KeywordDescription("Click UI element with querySelector")]
        [KeywordDisplayName("Click And Wait For Transition")]
        [KeywordParameters("querySelector", "querySelector of element to click")]
        [KeywordParameters("delay", "Give some time(in ms) after a click event to load the next screen. The default is 700 milliSeconds")]
        [SampleScript("Dune.Click (#7db992ba-557a-461c-b941-6023aa8cfa34, 700)")]
        public KeywordResult ClickAndWaitForTransition(string querySelector, string delay)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int loadDelay = 700;
            try
            {
                bool _ = int.TryParse(delay, out loadDelay);
                if(_workflowUI.IsExist(querySelector + " MouseArea"))
                {
                    querySelector = querySelector + " MouseArea";
                }
                _workflowUI.WaitFor(querySelector, _defaultCPDelay);
                _workflowUI.Click(querySelector, loadDelay);
                kr.Result = KeywordResults.Pass;
                return kr;
            }
            catch (DuneControlPanelOperationException ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = CaptureScreen();
                }
                kr.Output = $"Click failed with given query selector :: {ex.Data} text = {querySelector}";
                Logger.Error(kr.Output, ex);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Unexpected error";
                Logger.Error(kr.Output, ex);
                return kr;
            }
        }

        [KeywordDescription("Click UI element with querySelector without mousearea")]
        [KeywordDisplayName("Click Id")]
        [KeywordParameters("querySelector", "querySelector of element to click")]
        [SampleScript("Dune.Click Id (#userSignInComboBox)")]
        public KeywordResult ClickId(string querySelector)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_workflowUI.IsExist(querySelector))
                {
                    _workflowUI.WaitFor(querySelector, _defaultCPDelay);
                    _workflowUI.Click(querySelector);
                    kr.Result = KeywordResults.Pass;
                }
                else
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Click failed with given query selector ::{querySelector}";
                    return kr;
                }
                return kr;
            }
            catch (DuneControlPanelOperationException ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = CaptureScreen();
                }
                kr.Output = $"Click failed with given query selector :: {ex.Data} text = {querySelector}";
                Logger.Error(kr.Output, ex);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Unexpected error";
                Logger.Error(kr.Output, ex);
                return kr;
            }
        }

        [KeywordDescription("Click UI element with querySelector without mousearea")]
        [KeywordDisplayName("Click Id And Wait For Transition")]
        [KeywordParameters("querySelector", "querySelector of element to click")]
        [KeywordParameters("delay", "Give some time(in ms) after a click event to load the next screen. The default is 700 milliSeconds")]
        [SampleScript("Dune.Click Id And Wait For Transition (#userSignInComboBox, 700)")]
        public KeywordResult ClickIdAndWaitForTransition(string querySelector, string delay = "700")
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int loadDelay = 700;
            try
            {
                bool _ = int.TryParse(delay, out loadDelay);
                if (_workflowUI.IsExist(querySelector))
                {
                    _workflowUI.WaitFor(querySelector, _defaultCPDelay);
                    _workflowUI.Click(querySelector, loadDelay);
                    kr.Result = KeywordResults.Pass;
                }
                else
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Click failed with given query selector ::{querySelector}";
                    return kr;
                }
                return kr;
            }
            catch (DuneControlPanelOperationException ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = CaptureScreen();
                }
                kr.Output = $"Click failed with given query selector :: {ex.Data} text = {querySelector}";
                Logger.Error(kr.Output, ex);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Unexpected error";
                Logger.Error(kr.Output, ex);
                return kr;
            }
        }

        [KeywordDescription("Wait for UI element present in control panel.")]
        [KeywordDisplayName("Wait For")]
        [KeywordParameters("querySelector", "querySelector of element to wait")]
        [KeywordParameters("waitingTime", "Maximum waiting time in seconds")]
        [SampleScript("Dune.Wait For (#8a6dc844-4138-4954-b0c0-0b791fc68587,10)")]
        public KeywordResult WaitFor(string querySelector, string waitingTime)
        {
            if(!int.TryParse(waitingTime.Trim(), out int iWaitTime))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Waiting time should be a number";
                Logger.Error(error.Output);
                return error;
            }
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            try
            {
                QmlTestServerItem item = _workflowUI.WaitFor(querySelector, iWaitTime * 1000);
                if(item == null)
                {
                    kr.Result = KeywordResults.Fail;
                    if (!_disableScreenShotForFails)
                    {
                        kr.ScreenShot = CaptureScreen();
                    }
                    kr.Output = $"Item is not present within given time.";
                    Logger.Error(kr.Output);
                    return kr;
                }
                kr.Result = KeywordResults.Pass;
                return kr;
            }

            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Unexpected error";
                Logger.Error(kr.Output, ex);
                return kr;
            }
        }

        [KeywordDescription("Wait for UI element is enabled in current screen.")]
        [KeywordDisplayName("Wait For Enable")]
        [KeywordParameters("querySelector", "querySelector of element to wait")]
        [KeywordParameters("waitingTime", "Maximum waiting time in seconds")]
        [SampleScript("Dune.Wait For Enable (#mainActionButtonOfDetailPanel,10)")]
        public KeywordResult WaitForEnable(string querySelector, string waitingTime)
        {
            if (!int.TryParse(waitingTime.Trim(), out int iWaitTime))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Waiting time should be a number";
                Logger.Error(error.Output);
                return error;
            }
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            try
            {
                QmlTestServerItem item = _workflowUI.WaitFor(querySelector + "[enabled=true]", iWaitTime * 1000);
                if (item == null)
                {
                    kr.Result = KeywordResults.Fail;
                    if (!_disableScreenShotForFails)
                    {
                        kr.ScreenShot = CaptureScreen();
                    }
                    kr.Output = $"Item is not present within given time.";
                    Logger.Error(kr.Output);
                    return kr;
                }
                kr.Result = KeywordResults.Pass;
                return kr;
            }

            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Unexpected error";
                Logger.Error(kr.Output, ex);
                return kr;
            }
        }

        [KeywordDescription("Wait for given text is appeared on screen until given timeout (seconds) \r\n" +
            "Note:If the text contains special characters like *, (, ), ., %, -, /, etc., this keyword will not work. \r\n" +
            "In such cases, we recommend to use the 'Wait For(Id)' keyword.")]
        [KeywordDisplayName("Wait For Text")]
        [KeywordParameters("text", "text to appear on the screen")]
        [KeywordParameters("waitingTime", "Maximum waiting time in seconds")]
        [SampleScript("Dune.Wait For Text (Menu,10)")]
        public KeywordResult WaitForText(string text, string waitingTime)
        {
            if (!int.TryParse(waitingTime.Trim(), out int iWaitTime))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Waiting time should be a number";
                Logger.Error(error.Output);
                return error;
            }
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            try
            {
                QmlTestServerItem item = _workflowUI.WaitFor($"SpiceText[text={text}]", iWaitTime * 1000);
                if (item == null)
                {
                    kr.Result = KeywordResults.Fail;
                    if(!_disableScreenShotForFails)
                    {
                        kr.ScreenShot = CaptureScreen();
                    }
                    kr.Output = $"{text} is not present within given time.";
                    Logger.Error(kr.Output);
                    return kr;
                }
                kr.Result = KeywordResults.Pass;
                return kr;
            }

            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Unexpected error";
                Logger.Error(kr.Output, ex);
                return kr;
            }
        }

        [KeywordDescription("Swipe screen with given direction. Directions: < : Swipe Right to Left | > : Swipe Left to Right | ^ : Swipe Down to Up | v : Swipe Up to Down")]
        [KeywordDisplayName("Swipe")]
        [KeywordParameters("direction", "directino to swipe(<, >, ^, v)")]
        [Deprecated("Use Swipe By ControlId keyword instead of this.")]
        [SampleScript("Dune.Swipe (^)")]
        public KeywordResult Swipe(string direction)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            try
            {
                if (direction == "<")
                {
                    _workflowUI.ScrollRight();
                    kr.Result = KeywordResults.Pass;
                }
                else if (direction == ">")
                {
                    _workflowUI.ScrollLeft();
                    kr.Result = KeywordResults.Pass;
                }
                else if (direction == "^")
                {
                    _workflowUI.ScrollUp();
                    kr.Result = KeywordResults.Pass;
                }
                else if (direction == "v")
                {
                    _workflowUI.ScrollDown();
                    kr.Result = KeywordResults.Pass;
                }
                else
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Invalid direction";
                    if (!_disableScreenShotForFails)
                    {
                        kr.ScreenShot = CaptureScreen();
                    }
                    return kr;
                }
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Swipe error with direction :: {direction}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"Swipe({direction}) error", ex);
                return kr;
            }
        }

        [KeywordDescription("Swipe screen with given direction. Directions: < : Swipe Right to Left | > : Swipe Left to Right | ^ : Swipe Down to Up | v : Swipe Up to Down")]
        [KeywordDisplayName("Swipe And Wait For Transition")]
        [KeywordParameters("direction", "directino to swipe(<, >, ^, v)")]
        [KeywordParameters("delay", "Time in milliseconds to give some buffer time for new screen to load. Default is 0ms")]
        [Deprecated("Use Swipe By ControlId keyword instead of this.")]
        [SampleScript(" Dune.Swipe And Wait For Transition(^, 10)")]
        public KeywordResult SwipeAndWaitForTransition(string direction, string delay = "0")
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            int loadDelay = 0;
            try
            {
                bool _ = int.TryParse(delay, out loadDelay);
                if (direction == "<")
                {
                    _workflowUI.ScrollRight(loadDelay);
                    kr.Result = KeywordResults.Pass;
                }
                else if (direction == ">")
                {
                    _workflowUI.ScrollLeft(loadDelay);
                    kr.Result = KeywordResults.Pass;
                }
                else if (direction == "^")
                {
                    _workflowUI.ScrollUp(loadDelay);
                    kr.Result = KeywordResults.Pass;
                }
                else if (direction == "v")
                {
                    _workflowUI.ScrollDown(loadDelay);
                    kr.Result = KeywordResults.Pass;
                }
                else
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Invalid direction";
                    if (!_disableScreenShotForFails)
                    {
                        kr.ScreenShot = CaptureScreen();
                    }
                    return kr;
                }
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Swipe error with direction :: {direction}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"Swipe({direction}) error", ex);
                return kr;
            }
        }

        [KeywordDescription("Swipe screen with given direction for control Id. Directions: < : Swipe Right to Left | > : Swipe Left to Right | ^ : Swipe Down to Up | v : Swipe Up to Down")]
        [KeywordDisplayName("Swipe By ControlId")]
        [KeywordParameters("ControlId", "Id of the scrollbar Eg:-#copySettingsPage_list1ScrollBar")]
        [KeywordParameters("direction", "directino to swipe(<, >, ^, v)")]
        [SampleScript("Dune.Swipe By ControlId (#comboBoxScrollBar,v)")]
        public KeywordResult SwipeByControlId(string querySelector, string direction)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            bool result = false;
            try
            {
                if (direction == "<")
                {
                    result = _workflowUI.ScrollRight(querySelector);
                    if (result)
                    {
                        kr.Result = KeywordResults.Pass;
                    }
                    else
                    {
                        kr.Result = KeywordResults.Fail;
                        kr.Output = $"Invalid direction/Control Id";
                        if (!_disableScreenShotForFails)
                        {
                            kr.ScreenShot = CaptureScreen();
                        }
                        Logger.Error(kr.Output);
                        return kr;
                    }
                }
                else if (direction == ">")
                {
                    result = _workflowUI.ScrollLeft(querySelector);
                    if (result)
                    {
                        kr.Result = KeywordResults.Pass;
                    }
                    else
                    {
                        kr.Result = KeywordResults.Fail;
                        kr.Output = $"Invalid direction/Control Id";
                        kr.ScreenShot = CaptureScreen();
                        Logger.Error(kr.Output);
                        return kr;
                    }
                }
                else if (direction == "^")
                {
                    result = _workflowUI.ScrollUp(querySelector);
                    if (result)
                    {
                        kr.Result = KeywordResults.Pass;
                    }
                    else
                    {
                        kr.Result = KeywordResults.Fail;
                        kr.Output = $"Invalid direction/Control Id";
                        kr.ScreenShot = CaptureScreen();
                        Logger.Error(kr.Output);
                        return kr;
                    }
                }
                else if (direction == "v")
                {
                    result = _workflowUI.ScrollDown(querySelector);
                    if (result)
                    {
                        kr.Result = KeywordResults.Pass;
                    }
                    else
                    {
                        kr.Result = KeywordResults.Fail;
                        kr.Output = $"Invalid direction/Control Id";
                        if (!_disableScreenShotForFails)
                        {
                            kr.ScreenShot = CaptureScreen();
                        }
                        Logger.Error(kr.Output);
                        return kr;
                    }
                }
                else
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Invalid direction/Control Id";
                    if (!_disableScreenShotForFails)
                    {
                        kr.ScreenShot = CaptureScreen();
                    }
                    return kr;
                }
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Swipe error with direction/Control Id :: {direction}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"SwipeByControlId({direction},{querySelector}) error", ex);
                return kr;
            }
        }

        [KeywordDescription("Swipe screen with given direction for control Id. Directions: < : Swipe Right to Left | > : Swipe Left to Right | ^ : Swipe Down to Up | v : Swipe Up to Down")]
        [KeywordDisplayName("Swipe By Control Id And Wait For Transition")]
        [KeywordParameters("ControlId", "Id of the scrollbar Eg:-#copySettingsPage_list1ScrollBar")]
        [KeywordParameters("direction", "directino to swipe(<, >, ^, v)")]
        [KeywordParameters("delay", "Time in milliseconds to give some buffer time for new screen to load. Default is 0ms")]
        [SampleScript("Dune.Swipe By Control Id And Wait For Transition (#comboBoxScrollBar,v, 10)")]
        public KeywordResult SwipeByControlIdAndWaitForTransition(string querySelector, string direction, string delay = "0")
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            bool result = false;
            int loadDelay = 0;
            try
            {
                bool _ = int.TryParse(delay, out loadDelay);
                if (direction == "<")
                {
                    result = _workflowUI.ScrollRight(querySelector, loadDelay);
                    if (result)
                    {
                        kr.Result = KeywordResults.Pass;
                    }
                    else
                    {
                        kr.Result = KeywordResults.Fail;
                        kr.Output = $"Invalid direction/Control Id";
                        if (!_disableScreenShotForFails)
                        {
                            kr.ScreenShot = CaptureScreen();
                        }
                        Logger.Error(kr.Output);
                        return kr;
                    }
                }
                else if (direction == ">")
                {
                    result = _workflowUI.ScrollLeft(querySelector, loadDelay);
                    if (result)
                    {
                        kr.Result = KeywordResults.Pass;
                    }
                    else
                    {
                        kr.Result = KeywordResults.Fail;
                        kr.Output = $"Invalid direction/Control Id";
                        kr.ScreenShot = CaptureScreen();
                        Logger.Error(kr.Output);
                        return kr;
                    }
                }
                else if (direction == "^")
                {
                    result = _workflowUI.ScrollUp(querySelector, loadDelay);
                    if (result)
                    {
                        kr.Result = KeywordResults.Pass;
                    }
                    else
                    {
                        kr.Result = KeywordResults.Fail;
                        kr.Output = $"Invalid direction/Control Id";
                        kr.ScreenShot = CaptureScreen();
                        Logger.Error(kr.Output);
                        return kr;
                    }
                }
                else if (direction == "v")
                {
                    result = _workflowUI.ScrollDown(querySelector, loadDelay);
                    if (result)
                    {
                        kr.Result = KeywordResults.Pass;
                    }
                    else
                    {
                        kr.Result = KeywordResults.Fail;
                        kr.Output = $"Invalid direction/Control Id";
                        if (!_disableScreenShotForFails)
                        {
                            kr.ScreenShot = CaptureScreen();
                        }
                        Logger.Error(kr.Output);
                        return kr;
                    }
                }
                else
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Invalid direction/Control Id";
                    if (!_disableScreenShotForFails)
                    {
                        kr.ScreenShot = CaptureScreen();
                    }
                    return kr;
                }
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Swipe error with direction/Control Id :: {direction}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"SwipeByControlId({direction},{querySelector}) error", ex);
                return kr;
            }
        }

        [KeywordDescription("Set text to element with given query selector")]
        [KeywordDisplayName("Set Text")]
        [KeywordParameters("querySelector", "querySelector of text field")]
        [KeywordParameters("text", "Text to Input")]
        [SampleScript("Dune.Set Text(#windowsUsernameInputField,rdladmin)")]
        public KeywordResult SetText(string querySelector, string text)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            try
            {
                _workflowUI.SetText(querySelector, text);
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (NullReferenceException nrex)
            {
                kr.Result = KeywordResults.Fail;
                kr.Output = $"Can not find text field with given query selector:: {querySelector}";
                kr.AdditionalInfo = nrex.ToString();
                Logger.Error(kr.Output, nrex);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Set text error with given text :: {text}";
                Logger.Error($"SetText({querySelector}, {text}) fail", ex);
                return kr;
            }
        }

        [GetKeyword]
        [KeywordDescription("Get text to element with given query selector")]
        [KeywordDisplayName("Get Text")]
        [KeywordParameters("querySelector", "querySelector of text field")]
        [KeywordParameters("SaveTo", "variable name to save text")]
        [SampleScript("It will be Added in Next Release")]
        public KeywordResult GetText(string querySelector, string saveTo)
        {
            querySelector = HP.GFriend.Keywords.Support.Utils.GetVariablevalueIfExist(querySelector);
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                string text = _workflowUI.GetText(querySelector);
                CommonExecutionInfo.SetVariable(saveTo, text);
                kr.Output = $"Text : {text}";
                return kr;
            }
            catch (NullReferenceException nrex)
            {
                kr.Result = KeywordResults.Fail;
                kr.Output = $"Can not find text field with given query selector:: {querySelector}";
                kr.AdditionalInfo = nrex.ToString();
                Logger.Error(kr.Output, nrex);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Get text error";
                Logger.Error($"Get Text fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Copy Dial Pad Click")]
        [KeywordDisplayName("Copy DialPad Click")]
        [KeywordParameters("querySelector", "querySelector of element to click")]
        [SampleScript("Dune.Copy DialPad Click (10)")]
        public KeywordResult CopyDialPadClick(string inputstring)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            var querySelector = string.Empty;
            try
            {
                foreach (char i in inputstring)
                {
                    querySelector = _workflowUI.GetKeyPadKeyId(i.ToString(), "CopyPad");
                    if (_workflowUI.IsExist(querySelector))
                    {
                        _workflowUI.WaitFor(querySelector, _defaultCPDelay);
                        _workflowUI.Click(querySelector);
                    }
                }
                querySelector = "#enterKeyPositiveIntegerKeypad";
                if (_workflowUI.IsExist(querySelector))
                {
                    _workflowUI.WaitFor(querySelector, _defaultCPDelay);
                    _workflowUI.Click(querySelector);
                }
                else
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Click failed with given query selector ::{querySelector}";
                    return kr;
                }
                kr.Result = KeywordResults.Pass;
                return kr;
            }
            catch (DuneControlPanelOperationException ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = CaptureScreen();
                }
                kr.Output = $"Click failed with given inpurstring :: {inputstring}";
                Logger.Error(kr.Output, ex);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Unexpected error";
                Logger.Error(kr.Output, ex);
                return kr;
            }
        }

        [KeywordDescription("Fax Dial Pad Click")]
        [KeywordDisplayName("Fax DialPad Click")]
        [KeywordParameters("querySelector", "querySelector of element to click")]
        [SampleScript("Dune.Fax DialPad Click (724078)")]
        public KeywordResult FaxDialPadClick(string inputstring)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            string querySelector = string.Empty;
            try
            {
                foreach (char i in inputstring)
                {
                    querySelector = _workflowUI.GetKeyPadKeyId(i.ToString(), "FaxPad");
                    if (_workflowUI.IsExist(querySelector))
                    {
                        _workflowUI.WaitFor(querySelector, _defaultCPDelay);
                        _workflowUI.Click(querySelector);
                    }
                }
                querySelector = "#keyOk";
                if (_workflowUI.IsExist(querySelector))
                {
                    _workflowUI.WaitFor(querySelector, _defaultCPDelay);
                    _workflowUI.Click(querySelector);
                }
                else
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Click failed with given query selector ::{querySelector}";
                    return kr;
                }
                kr.Result = KeywordResults.Pass;
                return kr;
            }
            catch (DuneControlPanelOperationException ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = CaptureScreen();
                }
                kr.Output = $"Click failed with given inpurstring :: {inputstring}";
                Logger.Error(kr.Output, ex);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Unexpected error";
                Logger.Error(kr.Output, ex);
                return kr;
            }
        }

        [KeywordDescription("Generic Key Pad Click")]
        [KeywordDisplayName("GenericKeyPadClick")]
        [KeywordParameters("querySelector", "querySelector of element to click")]
        [SampleScript("Dune.GenericKeyPadClick (rdladmin)")]
        public KeywordResult GenericKeyPadClick(string inputstring)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                string querySelector;
                // Generic Keys to switch between Symbols and Alphabet keyboard
                string symbolkey = _workflowUI.GetKeyPadKeyId("SymbolModeKey");
                string key1By21 = _workflowUI.GetKeyPadKeyId("key1By21");
                string key2By21 = _workflowUI.GetKeyPadKeyId("key2By21");

                // Loop each Char 
                for (int i = 0; i < inputstring.Length; i++)
                {
                    char c = inputstring[i];
                    // Get ID based on character given -- DAT will return object Name
                    querySelector = _workflowUI.GetKeyPadKeyId(c.ToString());
                    if (_workflowUI.IsExist(querySelector))
                    {
                        // char Upper                        
                        if (i == 0 && char.IsLetter(c))
                        {
                            _workflowUI.Click(querySelector);
                        }
                        if ((char.IsUpper(c)))
                        {
                            string shiftkey = "keyshift";
                            shiftkey = _workflowUI.GetKeyPadKeyId(shiftkey);

                            // Shift Key click
                            if (_workflowUI.IsExist(shiftkey))
                            {
                                _workflowUI.WaitFor(shiftkey, _defaultCPDelay);
                                _workflowUI.Click(shiftkey);
                                Thread.Sleep(500);
                            }
                            else
                            {
                                kr.Result = KeywordResults.Fail;
                                kr.Output = $"Click failed with given query selector ::{querySelector}";
                                return kr;
                            }
                        }
                        // Actual char click
                        _workflowUI.WaitFor(querySelector, _defaultCPDelay);
                        _workflowUI.Click(querySelector);
                        if (i == 0 && char.IsLetter(c))
                        {
                            _workflowUI.Click("#keyLeft");
                            _workflowUI.Click("#backspaceKey");
                            _workflowUI.Click("#keyRight");
                            _workflowUI.Click("#keyRight");
                        }
                        Thread.Sleep(500);
                    }
                    else
                    {
                        // Numeric
                        if (_workflowUI.IsExist(symbolkey))
                        {
                            _workflowUI.WaitFor(symbolkey, _defaultCPDelay);
                            _workflowUI.Click(symbolkey);
                            Thread.Sleep(500);
                        }
                        else
                        {
                            kr.Result = KeywordResults.Fail;
                            kr.Output = $"Click failed with given query selector ::{symbolkey}";
                            return kr;
                        }
                        // If char exists in Number pad Key1by21
                        if (_workflowUI.IsExist(querySelector))
                        {
                            _workflowUI.WaitFor(querySelector, _defaultCPDelay);
                            _workflowUI.Click(querySelector);
                            Thread.Sleep(500);
                        }
                        else
                        {
                            // Symbols If char exists in Number pad Key1by21
                            if (_workflowUI.IsExist(key1By21))
                            {
                                _workflowUI.WaitFor(key1By21, _defaultCPDelay);
                                _workflowUI.Click(key1By21);
                                Thread.Sleep(500);
                            }
                            else
                            {
                                kr.Result = KeywordResults.Fail;
                                kr.Output = $"Click failed with given query selector ::{key1By21}";
                                return kr;
                            }
                            if (_workflowUI.IsExist(querySelector))
                            {
                                _workflowUI.WaitFor(querySelector, _defaultCPDelay);
                                _workflowUI.Click(querySelector);
                                Thread.Sleep(500);
                            }
                            else
                            {
                                kr.Result = KeywordResults.Fail;
                                kr.Output = $"Click failed with given query selector ::{querySelector}";
                                return kr;
                            }
                        }
                    }
                    // If already in key1by21 or key2by21 clicking symbol key
                    //  symbolkey = _workflowUI.KeyPadDial("SymbolKey");
                    if (_workflowUI.IsExist(key1By21) || _workflowUI.IsExist(key2By21))
                    {
                        if (_workflowUI.IsExist(symbolkey))
                        {
                            _workflowUI.WaitFor(symbolkey, _defaultCPDelay);
                            _workflowUI.Click(symbolkey);
                            Thread.Sleep(500);
                        }
                    }
                }
                querySelector = "#enterKey";
                if (_workflowUI.IsExist(querySelector))
                {
                    _workflowUI.WaitFor(querySelector, _defaultCPDelay);
                    _workflowUI.Click(querySelector);
                }
                else
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Click failed with given query selector ::{querySelector}";
                    return kr;
                }
                kr.Result = KeywordResults.Pass;
                return kr;
            }
            catch (DuneControlPanelOperationException ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = CaptureScreen();
                }
                kr.Output = $"Click failed with given inpurstring :: {inputstring}";
                Logger.Error(kr.Output, ex);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Unexpected error";
                Logger.Error(kr.Output, ex);
                return kr;
            }
        }

        [KeywordDescription("Validated the List Item visibility based on the hierarchy.")]
        [KeywordDisplayName("Wait For visibility")]
        [KeywordParameters("listItem", "Id of main list item")]
        [KeywordParameters("rowItem", "Id of the row item in main list item")]
        [KeywordParameters("elementItem", "Optional paramenter - Id of the list item whose visibility we need to check")]
        [KeywordParameters("isVertical", "Optional parameter - for vertical item list pass \"1\" , for horizontal item pass \"2\"")]
        [SampleScript("Dune.Wait For visibility (#userSignInComboBoxpopupList,#userSignInComboBoxItem_admin,1)")]
        public KeywordResult WaitForVisibility(string listItem, string elementItem, string isVertical)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            try
            {
                int scrollDirection = 0;
                try
                {
                    scrollDirection = int.Parse(isVertical);
                }
                catch (Exception ex)
                {
                    kr.Result = KeywordResults.Error;
                    kr.AdditionalInfo = ex.ToString();
                    kr.Output = $"Error converting isVerical to int:-  {isVertical}";
                    kr.ScreenShot = CaptureScreen();
                    Logger.Error($"isvertical ({isVertical}) error", ex);
                    return kr;
                }
                if (_workflowUI.IsExist(listItem))
                {
                    var item = _workflowUI.ValidateListObjectVisibility(listItem, elementItem, "", scrollDirection);
                    if (item == false)
                    {
                        kr.Result = KeywordResults.Fail;
                        if (!_disableScreenShotForFails)
                        {
                            kr.ScreenShot = CaptureScreen();
                        }
                        kr.Output = $"Item is not visible.";
                        Logger.Error(kr.Output);
                    }
                    else
                    {
                        kr.Result = KeywordResults.Pass;
                    }
                }
                else
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Invalid list item";
                    if (!_disableScreenShotForFails)
                    {
                        kr.ScreenShot = CaptureScreen();
                    }
                }
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Unexpected error while checking the visibility of {listItem}";
                Logger.Error(kr.Output, ex);
                return kr;
            }
        }

        [KeywordDescription("This scroll function is special case for ComboBoxpopupList. And scroll item into view then it can be selected.")]
        [KeywordDisplayName("SwipeUptoComboboxListItem")]
        [KeywordParameters("ComboBoxListItem", "Combobox list object name")]
        [KeywordParameters("scrollBar", "scroll_bar item object name")]
        [KeywordParameters("listItem", "list item object name")]
        [SampleScript("Dune.SwipeUptoComboboxListItem (#countryRegionWFMenuSelectionList,#countryRegionWFMenuSelectionListScrollBar,#ARcountryRegionWF)")]
        public KeywordResult SwipeUptoComboboxListItem(string ComboBoxListItem, string scrollBar, string listItem)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            try
            {
                if (_workflowUI.IsExist(ComboBoxListItem))
                {
                    bool result = _workflowUI.ScrollItemInToView(ComboBoxListItem, scrollBar, listItem);
                    if (result == true)
                    {
                        kr.Result = KeywordResults.Pass;
                    }
                    else
                    {
                        kr.Result = KeywordResults.Fail;
                        kr.Output = $"unable to scroll till the item position";
                        if (!_disableScreenShotForFails)
                        {
                            kr.ScreenShot = CaptureScreen();
                        }
                    }
                }
                else
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Invalid ComboBox list Item";
                    if (!_disableScreenShotForFails)
                    {
                        kr.ScreenShot = CaptureScreen();
                    }
                }
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Error while scrolling the scroll bar :: {ComboBoxListItem}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"Swipe({scrollBar}) error", ex);
                return kr;
            }
        }

        [KeywordDescription("This scroll function is special case for ComboBoxpopupList. And scroll item into view then it can be selected.")]
        [KeywordDisplayName("Swipe Upto Combobox List Item And Wait For Transition")]
        [KeywordParameters("ComboBoxListItem", "Combobox list object name")]
        [KeywordParameters("scrollBar", "scroll_bar item object name")]
        [KeywordParameters("listItem", "list item object name")]
        [KeywordParameters("delay", "Time in milliseconds to give some buffer time for new screen to load. Default is 0ms")]
        [SampleScript("Dune.SwipeUptoComboboxListItem (#countryRegionWFMenuSelectionList,#countryRegionWFMenuSelectionListScrollBar,#ARcountryRegionWF, 700)")]
        public KeywordResult SwipeUptoComboboxListItemAndWaitForTransition(string ComboBoxListItem, string scrollBar, string listItem, string delay = "0")
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            int loadTime = 0;
            try
            {
                bool _ = int.TryParse(delay, out loadTime);
                if (_workflowUI.IsExist(ComboBoxListItem))
                {
                    bool result = _workflowUI.ScrollItemInToView(ComboBoxListItem, scrollBar, listItem, loadTime);
                    if (result == true)
                    {
                        kr.Result = KeywordResults.Pass;
                    }
                    else
                    {
                        kr.Result = KeywordResults.Fail;
                        kr.Output = $"unable to scroll till the item position";
                        if (!_disableScreenShotForFails)
                        {
                            kr.ScreenShot = CaptureScreen();
                        }
                    }
                }
                else
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Invalid ComboBox list Item";
                    if (!_disableScreenShotForFails)
                    {
                        kr.ScreenShot = CaptureScreen();
                    }
                }
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Error while scrolling the scroll bar :: {ComboBoxListItem}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"Swipe({scrollBar}) error", ex);
                return kr;
            }
        }

        [KeywordDescription("This scroll function is used to scroll down vertically till elementId is visible.")]
        [KeywordDisplayName("SwipeVertically")]
        [KeywordParameters("listId", "Id of main list item")]
        [KeywordParameters("scrollBar", "Id of the scrollbar item in combobox list")]
        [KeywordParameters("listItem", "Optional argument -Id of element item")]
        [KeywordParameters("scrollValue", "scrollValue for scroll bar Ex:- 0.05f,0.075f.0.1,0.125f")]
        [SampleScript("Dune.SwipeVertically (#settingsMenuListListViewlist1,#settingsMenuListListViewlist1ScrollBar,#suppliesSettingsSettingsTextImage,0.05)")]
        public KeywordResult SwipeVertically(string listId, string scrollBar, string elementId, string scrollValue)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            int loadTime = 0;
            try
            {
                float scrollingValue = 0.0f;
                try
                {
                    scrollingValue = (float)Convert.ToDouble(scrollValue);
                }
                catch (Exception ex)
                {
                    kr.Result = KeywordResults.Error;
                    kr.AdditionalInfo = ex.ToString();
                    kr.Output = $"Error converting scrollvalue to float:-  {scrollingValue}";
                    kr.ScreenShot = CaptureScreen();
                    Logger.Error($"scrollvalue ({scrollValue}) error", ex);
                    return kr;
                }
                if (_workflowUI.IsExist(listId))
                {
                    bool result = _workflowUI.ScrollFromTopToBottom(listId, scrollBar, elementId, scrollingValue, loadTime);
                    if (result == true)
                    {
                        kr.Result = KeywordResults.Pass;
                    }
                    else
                    {
                        kr.Output = $"unable to scroll to the element position";
                        kr.ScreenShot = CaptureScreen();
                    }
                }
                else
                {
                    kr.Output = $"Invalid popup list";
                    if (!_disableScreenShotForFails)
                    {
                        kr.ScreenShot = CaptureScreen();
                    }
                }
                    return kr;
                
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Error while scrolling the scroll bar :: {scrollBar} in list {listId}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"SwipeVertically({scrollBar}) error", ex);
                return kr;
            } 
        }

        [KeywordDescription("This scroll function is used to scroll down vertically till elementId is visible.")]
        [KeywordDisplayName("Swipe Vertically And Wait For Transition")]
        [KeywordParameters("listId", "Id of main list item")]
        [KeywordParameters("scrollBar", "Id of the scrollbar item in combobox list")]
        [KeywordParameters("listItem", "Optional argument -Id of element item")]
        [KeywordParameters("scrollValue", "scrollValue for scroll bar Ex:- 0.05f,0.075f.0.1,0.125f")]
        [KeywordParameters("delay", "Time in milliseconds to give some buffer time for new screen to load. Default is 10ms")]
        [SampleScript("Dune.SwipeVertically And Wait For Transition (#settingsMenuListListViewlist1,#settingsMenuListListViewlist1ScrollBar" +
            ",#suppliesSettingsSettingsTextImage,0.05, 700)")]
        public KeywordResult SwipeVerticallyAndWaitForTransition(string listId, string scrollBar, string elementId, string scrollValue, string delay = "10")
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            int loadTime = 0;
            try
            {
                bool _ = int.TryParse(delay, out loadTime);
                float scrollingValue = 0.0f;
                try
                {
                    scrollingValue = (float)Convert.ToDouble(scrollValue);
                }
                catch (Exception ex)
                {
                    kr.Result = KeywordResults.Error;
                    kr.AdditionalInfo = ex.ToString();
                    kr.Output = $"Error converting scrollvalue to float:-  {scrollingValue}";
                    kr.ScreenShot = CaptureScreen();
                    Logger.Error($"scrollvalue ({scrollValue}) error", ex);
                    return kr;
                }
                if (_workflowUI.IsExist(listId))
                {
                    bool result = _workflowUI.ScrollFromTopToBottom(listId, scrollBar, elementId, scrollingValue, loadTime);
                    if (result == true)
                    {
                        kr.Result = KeywordResults.Pass;
                    }
                    else
                    {
                        kr.Output = $"unable to scroll to the element position";
                        kr.ScreenShot = CaptureScreen();
                    }
                }
                else
                {
                    kr.Output = $"Invalid popup list";
                    if (!_disableScreenShotForFails)
                    {
                        kr.ScreenShot = CaptureScreen();
                    }
                }
                    
                return kr;
               
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Error while scrolling the scroll bar :: {scrollBar} in list {listId}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"SwipeVertically({scrollBar}) error", ex);
                return kr;
            }
        }

        [KeywordDescription("Get the value of current slider position.")]
        [KeywordDisplayName("GetSliderValue")]
        [KeywordParameters("querySelector", "querySelector of slider")]
        [KeywordParameters("saveTo", "Value to be saved")]
        [SampleScript("Dune.GetSliderValue(#scan_lighterDarkerMenuSlider,${value})")]
        public KeywordResult GetSliderValue(string querySelector, string saveTo)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            try
            {
                if (_workflowUI.IsExist(querySelector))
                {
                    _workflowUI.WaitFor(querySelector, _defaultCPDelay);
                    string value = _workflowUI.GetSliderValue(querySelector);
                    if (value != null)
                    {
                        CommonExecutionInfo.SetVariable(saveTo, value);
                        kr.Result = KeywordResults.Pass;
                        kr.Output = $"{saveTo} : {value}";
                    }
                    else
                    {
                        kr.Result = KeywordResults.Fail;
                        kr.Output = $"Error occured in GetSliderValue";
                        return kr;
                    }
                }
                kr.Output = $"queryselector doesnot exist. check the  id ::{querySelector}";
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Error in GetSliderValue::{ex.Message}";
                Logger.Error(kr.Output);
                return kr;
            }
        }

        [KeywordDescription("Set's the slider value. Slider will be set to given value")]
        [KeywordDisplayName("SetSliderValue")]
        [KeywordParameters("querySelector", "querySelector of slider")]
        [KeywordParameters("value", "Value need to set")]
        [SampleScript("Dune.SetSliderValue(#scan_lighterDarkerMenuSlider,4)")]
        public KeywordResult SetSliderValue(string querySelector, string value)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            try
            {
                if (_workflowUI.IsExist(querySelector))
                {
                    _workflowUI.WaitFor(querySelector, _defaultCPDelay);
                    bool result = _workflowUI.SetSliderValue(querySelector, value);
                    if (result != false)
                    {
                        kr.Result = KeywordResults.Pass;
                        kr.Output = $"Slider value set successfully for {querySelector}";
                        return kr;
                    }
                    else
                    {
                        kr.Result = KeywordResults.Fail;
                        kr.Output = $"unable to SetSliderValue::{querySelector}";
                        return kr;
                    }
                }
                else
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"queryselector doesnot exist. check the  id ::{querySelector}";
                    return kr;
                }

            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $" Error occured in SetSliderValue::{ex.Message}";
                Logger.Error(kr.Output, ex);
                return kr;
            }
        }

        [KeywordDescription("Set's the slider value. Slider will be set to given value")]
        [KeywordDisplayName("Set Slider Value And Wait For Transition")]
        [KeywordParameters("querySelector", "querySelector of slider")]
        [KeywordParameters("value", "Value need to set")]
        [KeywordParameters("delay", "Give some additional time(in milliseconds) to device to set the value. The default is 0ms")]
        [SampleScript("Dune.Set Slider Value And Wait For Transition(#scan_lighterDarkerMenuSlider,4, 700)")]
        public KeywordResult SetSliderValueAndWaitForTransition(string querySelector, string value, string delay = "0")
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            int loadDelay = 0;
            try
            {
                bool _ = int.TryParse(delay, out loadDelay);
                if (_workflowUI.IsExist(querySelector))
                {
                    _workflowUI.WaitFor(querySelector, _defaultCPDelay);
                    bool result = _workflowUI.SetSliderValue(querySelector, value, loadDelay);
                    if (result != false)
                    {
                        kr.Result = KeywordResults.Pass;
                        kr.Output = $"Slider value set successfully for {querySelector}";
                        return kr;
                    }
                    else
                    {
                        kr.Result = KeywordResults.Fail;
                        kr.Output = $"unable to SetSliderValue::{querySelector}";
                        return kr;
                    }
                }
                else
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"queryselector doesnot exist. check the  id ::{querySelector}";
                    return kr;
                }

            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $" Error occured in SetSliderValue::{ex.Message}";
                Logger.Error(kr.Output, ex);
                return kr;
            }
        }

        [KeywordDescription("Check the toggle button, Returns True or false based on current status of Toggle element")]
        [KeywordDisplayName("IsToggleStatus")]
        [KeywordParameters("querySelector", "querySelector of toggle button")]
        [KeywordParameters("saveTo", "status value to be saved")]
        [SampleScript("Dune.IsToggleStatus (#copy_collateToggleMenuSwitch,${value})")]
        public KeywordResult IsToggleStatus(string querySelector, string saveTo)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            try
            {
                if (_workflowUI.IsExist(querySelector))
                {
                    _workflowUI.WaitFor(querySelector, _defaultCPDelay);
                    string status = _workflowUI.GetToggleStatus(querySelector);
                    if (status != null)
                    {
                        CommonExecutionInfo.SetVariable(saveTo, status.ToString());
                        kr.Result = KeywordResults.Pass;
                        kr.Output = $"{saveTo} : {status}";
                    }
                    else
                    {
                        kr.Result = KeywordResults.Fail;
                        kr.Output = $"IsToggleStatus returned null status for {querySelector}";
                        return kr;
                    }
                }
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Error occured in IsToggleStatus::{ex.Message}";
                Logger.Error(kr.Output, ex);
                return kr;
            }
        }

        [KeywordDescription("Swipe slider in the given direction. Directions: < : Swipe Right to Left | > : Swipe Left to Right")]
        [KeywordDisplayName("SwipeSliderControl")]
        [KeywordParameters("querySelector", "QuerySelector of slider to swipe")]
        [KeywordParameters("direction", "direction to swipe(<, >)")]
        [SampleScript("Dune.SwipeSliderControl (#scan_lighterDarkerMenuSlider,<)")]
        public KeywordResult SwipeSliderControl(string querySelector, string direction)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            try
            {
                if (_workflowUI.IsExist(querySelector))
                {
                    if (direction == "<")
                    {
                        _workflowUI.SwipeSliderLeftRight(querySelector, "left");
                        kr.Result = KeywordResults.Pass;
                    }
                    else if (direction == ">")
                    {
                        _workflowUI.SwipeSliderLeftRight(querySelector, "right");
                        kr.Result = KeywordResults.Pass;
                    }
                    else
                    {
                        kr.Result = KeywordResults.Fail;
                        kr.Output = $"Invalid direction";
                        if (!_disableScreenShotForFails)
                        {
                            kr.ScreenShot = CaptureScreen();
                        }
                        return kr;
                    }
                }
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Error occured in SwipeSliderControl::{ex.Message}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"SwipeSliderControl({direction}) error", ex);
                return kr;
            }
        }

        [KeywordDescription("Swipe slider in the given direction. Directions: < : Swipe Right to Left | > : Swipe Left to Right")]
        [KeywordDisplayName("Swipe Slider Control And Wait For Transition")]
        [KeywordParameters("querySelector", "QuerySelector of slider to swipe")]
        [KeywordParameters("direction", "direction to swipe(<, >)")]
        [KeywordParameters("delay", "Time in milliseconds to give some buffer time for new screen to load. Default is 0ms")]
        [SampleScript("Dune.Swipe Slider Control And Wait For Transition (#scan_lighterDarkerMenuSlider,<, 10)")]
        public KeywordResult SwipeSliderControlAndWaitForTransition(string querySelector, string direction, string delay = "10")
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            int loadTime = 0;
            try
            {
                bool _ = int.TryParse(delay, out loadTime);
                if (_workflowUI.IsExist(querySelector))
                {
                    if (direction == "<")
                    {
                        _workflowUI.SwipeSliderLeftRight(querySelector, "left", loadTime);
                        kr.Result = KeywordResults.Pass;
                    }
                    else if (direction == ">")
                    {
                        _workflowUI.SwipeSliderLeftRight(querySelector, "right", loadTime);
                        kr.Result = KeywordResults.Pass;
                    }
                    else
                    {
                        kr.Result = KeywordResults.Fail;
                        kr.Output = $"Invalid direction";
                        if (!_disableScreenShotForFails)
                        {
                            kr.ScreenShot = CaptureScreen();
                        }
                        return kr;
                    }
                }
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Error occured in SwipeSliderControl::{ex.Message}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"SwipeSliderControl({direction}) error", ex);
                return kr;
            }
        }

        [KeywordDescription("Set the OAuth2Standard EnableTokenAuthorization to False. \n" +
            "By default OAuth2 EnableTokenAuth is true, If we set to false then only we can see the job details. \n " +
            "Job details can be seen by using the keyword WaitForActiveJob or WaitForActiveJobGone.")]
        [KeywordDisplayName("SetOAuth2Standard_EnableTokenAuthFalse")]
        [KeywordParameters("portnumber", "server port number to be connected for auth2standard disabling")]
        [KeywordParameters("time", "set the time")]
        public KeywordResult SetOAuth2Standard_EnableTokenAuthFalse(string portnumber, string time)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            try
            {
                bool result = _workflowUI.SetOAuth2StandardEnableTokenAuthFalse(portnumber, time);

                if (result == true)
                {
                    kr.Result = KeywordResults.Pass;
                    kr.Output = $"OAuth2standardEnableToken is set to false.";
                }
                else
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Device is not responding in specified time. Please check whether the device is already connected from other device or from other instance.";
                    if (!_disableScreenShotForFails)
                    {
                        kr.ScreenShot = CaptureScreen();
                    }
                }
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Error occured in SetOAuth2Standard_EnableTokenAuthFalse after {time} seconds::{ex.Message}";
                kr.ScreenShot = CaptureScreen();
                return kr;
            }
        }

        [KeywordDescription("Wait for the job to be disappeared on screen until given timeout (seconds) \r\n" +
    "Before calling this keyword, the keyword 'SetOAuth2StandardEnableTokenFalse' must be called to disable Auth2 Standard.\r\n" +
    "By default the Token Enable is true, So we need to call this to disable, otherwise It doesn't wait for the active job to be gone.")]
        [KeywordDisplayName("Wait For Active Job Gone")]
        [KeywordParameters("jobType", "jobTypes (copy,print,scanFax,scanUsb,usbPrint,scanNetworkFolder,scanEmail, etc.)")]
        [KeywordParameters("waitingTime ", "Time to wait")]
        [SampleScript("Dune.Wait For Active Job Gone (Copy,20)")]
        public KeywordResult WaitForActiveJobGone(string jobType, string waitingTime)
        {
            if (!int.TryParse(waitingTime.Trim(), out int iWaitTime))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Waiting time should be a number";
                Logger.Error(error.Output);
                return error;
            }
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            try
            {

                bool result = _workflowUI.WaitForActiveJobGone(jobType, waitingTime);
                if (result == true)
                {
                    kr.Result = KeywordResults.Pass;
                }
                else
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"The Auth2 Standard may not be set to false for the Device, or The Job is not Disappeared within given time.";
                    if (!_disableScreenShotForFails)
                    {
                        kr.ScreenShot = CaptureScreen();
                    }
                    Logger.Error(kr.Output);
                    return kr;
                }
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Unexpected error";
                kr.ScreenShot = CaptureScreen();
                Logger.Error(kr.Output, ex);
                return kr;
            }
        }

        [KeywordDescription("Wait for the active job to be appeared on screen until given timeout (seconds)\r\n" +
    "Before calling this keyword, the keyword 'SetOAuth2StandardEnableTokenFalse' must be called to disable Auth2 Standard.\r\n" +
    "By default the Token Enable is true, So we need to call this to disable, otherwise we don't get the active job type.")]
        [KeywordDisplayName("Wait For Active Job")]
        [KeywordParameters("jobType", "jobTypes (copy,print,scanFax,scanUsb,usbPrint,scanNetworkFolder,scanEmail, etc.)")]
        [KeywordParameters("waitingTime ", "Time to wait")]
        [SampleScript("Dune.Wait For Active Job(scanEmail,30)")]
        public KeywordResult WaitForActiveJob(string jobType, string waitingTime)
        {
            if (!int.TryParse(waitingTime.Trim(), out int iWaitTime))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Waiting time should be a number";
                Logger.Error(error.Output);
                return error;
            }
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            try
            {
                bool result = _workflowUI.WaitForActiveJob(jobType, waitingTime);
                if (result == true)
                {
                    kr.Result = KeywordResults.Pass;
                }
                else
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"The Auth2 Standard may not be set to false for the Device, or The Job is not Appeared within given time.";
                    if (!_disableScreenShotForFails)
                    {
                        kr.ScreenShot = CaptureScreen();
                    }
                    Logger.Error(kr.Output);
                    return kr;
                }
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Unexpected error";
                kr.ScreenShot = CaptureScreen();
                Logger.Error(kr.Output, ex);
                return kr;
            }
        }

        [KeywordDescription("Press Back Key - This will click on the back button visible on the screen. If back button is not visible on the screen it returns as failed.\r\n" +
            "Note:Depending on the firmware version this back button works on most of the screens.")]
        [KeywordDisplayName("Press Back Key")]
        [SampleScript("Dune.Press Back Key")]
        public KeywordResult PressBackKey()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            var backButton = "#BackButton";
            try
            {
                if (_workflowUI.IsExist(backButton))
                {
                    QmlTestServerItem item = _workflowUI.WaitFor(backButton);
                    _workflowUI.Click(backButton);
                    kr.Result = KeywordResults.Pass;
                }
                else
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Back button is not available in the current screen.";
                    return kr;
                }
                return kr;
            }
            catch (DuneControlPanelOperationException ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = CaptureScreen();
                }
                kr.Output = $"Press Back button failed";
                Logger.Error(kr.Output, ex);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Unexpected error";
                Logger.Error(kr.Output, ex);
                return kr;
            }
        }

        [KeywordDescription("Press Back Key - This will click on the back button visible on the screen. If back button is not visible on the screen it returns as failed.\r\n" +
            "Note:Depending on the firmware version this back button works on most of the screens.")]
        [KeywordDisplayName("Press BackKey And Wait For Transition")]
        [KeywordParameters("delay", "Give some time(in ms) after a click event to load the next screen. The default is 700 milliSeconds")]
        [SampleScript("Dune.Press BackKey And Wait For Transition(700)")]
        public KeywordResult PressBackKeyAndWaitForTransition(string delay = "700")
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            var backButton = "#BackButton";
            int loadDelay = 700;
            try
            {
                bool _ = int.TryParse(delay, out loadDelay);
                if (_workflowUI.IsExist(backButton))
                {
                    QmlTestServerItem item = _workflowUI.WaitFor(backButton);
                    _workflowUI.Click(backButton, loadDelay);
                    kr.Result = KeywordResults.Pass;
                }
                else
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Back button is not available in the current screen.";
                    return kr;
                }
                return kr;
            }
            catch (DuneControlPanelOperationException ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = CaptureScreen();
                }
                kr.Output = $"Press Back button failed";
                Logger.Error(kr.Output, ex);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Unexpected error";
                Logger.Error(kr.Output, ex);
                return kr;
            }
        }

        [KeywordDescription("Check if the device status is sleep")]
        [KeywordDisplayName("IsSleep")]
        [SampleScript("Dune.IsSleep")]
        public KeywordResult IsSleep()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            try
            {
                PowerState currentState = _device.PowerManagement.GetCurrentactivityState();
                if (currentState.Equals(PowerState.Sleep) || currentState.Equals(PowerState.DeepSleep))
                {
                    KeywordResult pass = new KeywordResult(KeywordResults.Pass);
                    pass.Output = $"Current power status : {currentState.ToString()}";
                    return pass;
                }
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Current state is {currentState.ToString()}";
                return fail;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.ScreenShot = CaptureScreen();
                return kr;
            }
        }

        [KeywordDescription("Wait until target device goes to sleep mode")]
        [KeywordDisplayName("Wait Until Sleep")]
        [KeywordParameters("timeout", "time out to wait (in seconds)")]
        [SampleScript("Dune.Wait Until Sleep (300)")]
        public KeywordResult WaitUntilSleep(string timeOut)
        {
            if (!int.TryParse(timeOut, out int timeoutInt))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"Timeout must be a number. :: Timeout - {timeOut}";
                return error;
            }
            DateTime endTime = DateTime.Now.AddSeconds(timeoutInt);
            PowerState currentState = _device.PowerManagement.GetCurrentactivityState();
            while (endTime > DateTime.Now)
            {
                if (currentState.Equals(PowerState.Sleep) || currentState.Equals(PowerState.DeepSleep))
                {
                    KeywordResult pass = new KeywordResult(KeywordResults.Pass);
                    pass.Output = $"Current power status : {currentState.ToString()}";
                    return pass;
                }
                Thread.Sleep(500);
                currentState = _device.PowerManagement.GetCurrentactivityState();
            }
            currentState = _device.PowerManagement.GetCurrentactivityState();
            KeywordResult fail = new KeywordResult(KeywordResults.Fail);
            fail.Output = $"Device didn't go to the sleep within timeout. Current state is {currentState.ToString()}";
            return fail;
        }

        [KeywordDescription("Check if the device status is awake")]
        [KeywordDisplayName("IsAwake")]
        [SampleScript("Dune.IsAwake")]
        public KeywordResult IsAwake()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            try
            {

                PowerState currentState = _device.PowerManagement.GetCurrentactivityState();
                if (currentState.Equals(PowerState.Awake))
                {
                    KeywordResult pass = new KeywordResult(KeywordResults.Pass);
                    pass.Output = $"Current power status : {currentState.ToString()}";
                    return pass;
                }
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Device is not in awake state. Current state is {currentState.ToString()}";
                return fail;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.ScreenShot = CaptureScreen();
                return kr;
            }
        }

        [KeywordDescription("check if the device is in power save mode")]
        [KeywordDisplayName("Is Power Save Mode")]
        [SampleScript("Dune.Is PowerSave Mode")]
        public KeywordResult IsPowerSaveMode()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);

            try
            {
                PowerState currentState = _device.PowerManagement.GetCurrentactivityState();
                if (currentState.Equals(PowerState.PowerSave))
                {
                    KeywordResult pass = new KeywordResult(KeywordResults.Pass);
                    pass.Output = $"Current power status : {currentState.ToString()}";
                    return pass;
                }
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Current state is {currentState.ToString()}";
                return fail;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.ScreenShot = CaptureScreen();
                return kr;
            }
        }

        [KeywordDescription("Wait until target device goes to power save mode with in the given time out")]
        [KeywordDisplayName("Wait Until Power Save")]
        [KeywordParameters("timeout", "time out to wait (in seconds)")]
        [SampleScript("Dune.Wait Until Power Save (10)")]
        public KeywordResult WaitUntilPowerSave(string timeOut)
        {
            if (!int.TryParse(timeOut, out int timeoutInt))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"Timeout must be a number. :: Timeout - {timeOut}";
                return error;
            }
            DateTime endTime = DateTime.Now.AddSeconds(timeoutInt);
            PowerState currentState = _device.PowerManagement.GetCurrentactivityState();
            while (endTime > DateTime.Now)
            {
                if (currentState.Equals(PowerState.PowerSave))
                {
                    KeywordResult pass = new KeywordResult(KeywordResults.Pass);
                    pass.Output = $"Current power status : {currentState.ToString()}";
                    return pass;
                }
                Thread.Sleep(500);
                currentState = _device.PowerManagement.GetCurrentactivityState();
            }
            currentState = _device.PowerManagement.GetCurrentactivityState();
            KeywordResult fail = new KeywordResult(KeywordResults.Fail);
            fail.Output = $"Device did not go to the power save within timeout. Current state is {currentState.ToString()}";
            return fail;
        }

        [KeywordDescription("Initialize Memory monitoring for Dune Device")]
        [KeywordDisplayName("Start Memory Monitoring")]
        [SampleScript("Dune.Start Memory Monitoring")]
        public KeywordResult StartMemoryMonitoring()
        {
            string csvPath;
            _testCaseName = CommonExecutionInfo.GetVariable("_tcName");

            if (CommonExecutionInfo.CurrentRepeatCount > 0)
            {
                csvPath = Path.Combine(Directory.GetParent(_outputDir).FullName, $"Dune_Memory_Usage.csv");
                Logger.Trace($"Current Repeat Count is greater than 0. CSV Path is : {csvPath}");
            }
            else
            {
                csvPath = Path.Combine(_outputDir, $"{_testCaseName}_DuneMemoryMonitoring_{DateTime.Now.ToString("yyMMdd_HHmmssff")}.csv");
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
        [SampleScript("Dune.Collect Memory Info (SingIn / SingOut)")]
        public KeywordResult CollectMemoryInfo(string checkPoint)
        {
            try
            {
                var memoryStats = _device.UDWManagement.CaptureMemoryStats();
                string memoryData = memoryStats.ToString();
                string jsonData = ProcessMemoryJson(memoryData);
                string strXml = ProcessMemoryXml(jsonData);
                XDocument memoryXml = XDocument.Parse(strXml);
                KeywordEvent.Trigger("DuneMemoryCollection", checkPoint);
                Dictionary<string, long> memInfo = ProcessMemoryData(memoryXml);
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
                HP.GFriend.GFLogger.Logger.Error(ex.ToString());
                return fail;
            }
        }

        /// <summary>
        /// Parse the JSON array to JSON String with the required memory pools.
        /// </summary>
        /// <param name="jsonData">JSON string.</param>
        public string ProcessMemoryJson(string jsonData)
        {
            if (!string.IsNullOrEmpty(jsonData))
            {
                try
                {
                    string result = null;

                    //Parse the JSON data
                    JObject ParsedJsondata = JObject.Parse(jsonData);

                    //Extract the pools array from duneMemory
                    JArray pools = (JArray)ParsedJsondata["duneMemory"]["pools"];

                    //Iterate through each pool and extract the values
                    foreach (JObject pool in pools)
                    {
                        string name = (string)pool["name"];
                        long maxBytes = (long)pool["maxBytes"];
                        long extension = (long)pool["extension"];
                        long consumedBytes = (long)pool["consumedBytes"];
                        long highWaterBytes = (long)pool["highWaterBytes"];
                        int currentAllocations = (int)pool["currentAllocations"];
                        result += $"<pools>\r\n <name>{name}</name>\r\n <maxBytes>{maxBytes}</maxBytes>\r\n <extension>{extension}</extension>\r\n <consumedBytes>{consumedBytes}</consumedBytes>\r\n <highWaterBytes>{highWaterBytes}</highWaterBytes>\r\n <currentAllocations>{currentAllocations}</currentAllocations>\r\n</pools>\n";
                    }

                    // Extract the values
                    long kernelRamConsumption = (long)ParsedJsondata["systemStats"]["kernelRamConsumption"];
                    long userRamConsumption = (long)ParsedJsondata["systemStats"]["userRamConsumption"];
                    long unavailableRamUnits = (long)ParsedJsondata["systemStats"]["unavailableRamUnits"];
                    long availableRamUnits = (long)ParsedJsondata["systemStats"]["availableRamUnits"];
                    result += $"<systemStats>\r\n <kernelRamConsumption>{kernelRamConsumption}</kernelRamConsumption>\r\n <userRamConsumption>{userRamConsumption}</userRamConsumption>\r\n <unavailableRamUnits>{unavailableRamUnits}</unavailableRamUnits>\r\n <availableRamUnits>{availableRamUnits}</availableRamUnits>\r\n</systemStats>\n";

                    //Extract System Memory Info
                    long MemTotal = (long)ParsedJsondata["systemMemInfo"]["MemTotal"];
                    long MemFree = (long)ParsedJsondata["systemMemInfo"]["MemFree"];
                    long MemAvailable = (long)ParsedJsondata["systemMemInfo"]["MemAvailable"];
                    long Buffers = (long)ParsedJsondata["systemMemInfo"]["Buffers"];
                    long Cached = (long)ParsedJsondata["systemMemInfo"]["Cached"];
                    long Dirty = (long)ParsedJsondata["systemMemInfo"]["Dirty"];
                    long Mapped = (long)ParsedJsondata["systemMemInfo"]["Mapped"];
                    long Committed_AS = (long)ParsedJsondata["systemMemInfo"]["Committed_AS"];
                    result += $"<systemMemInfo>\r\n <MemTotal>{MemTotal}</MemTotal>\r\n <MemFree>{MemFree}</MemFree>\r\n <MemAvailable>{MemAvailable}</MemAvailable>\r\n <Buffers>{Buffers}</Buffers>\r\n <Cached>{Cached}</Cached>\r\n <Dirty>{Dirty}</Dirty>\r\n <Mapped>{Mapped}</Mapped>\r\n</systemMemInfo>\n";

                    string stringJson = $"<root>\r\n{result}\r\n</root>\n";

                    return stringJson;
                }
                catch (Exception ex)
                {
                    Logger.Debug($"unable to parse the JSON data:-{ex.Message}");
                    return "";
                }
            }
            else
            {
                Logger.Debug("Parameters cannot be null or empty");
                return "";
            }
        }

        /// <summary>
        /// Parse the JSON string to XML String.
        /// </summary>
        /// <param name="xml">JSON String.</param>
        public string ProcessMemoryXml(string xml)
        {
            string result = "";

            //Load the XML into an XDocument
            XDocument doc = XDocument.Parse(xml);

            //Select all pools elements
            var pools = doc.Descendants("pools");

            foreach (var pool in pools)
            {
                // Extract values
                string name = (string)pool.Element("name");
                var properties = new[]
                {
                    new { ElementName = "maxBytes", Suffix = "MaxBytes" },
                    new { ElementName = "extension", Suffix = "Extension" },
                    new { ElementName = "consumedBytes", Suffix = "ConsumedBytes" },
                    new { ElementName = "highWaterBytes", Suffix = "HighWaterBytes" },
                    new { ElementName = "currentAllocations", Suffix = "CurrentAllocations" }
                };
                foreach (var prop in properties)
                {
                    string value = (string)pool.Element(prop.ElementName);
                    result += $"<Counter Category=\"{name}\" Name=\"{prop.Suffix}\" Value=\"{value}\" />\n";
                }
            }
            //Select and process SystemStats and SystemMemInfo
            var systemStats = doc.Element("root").Element("systemStats");
            var systemMemInfo = doc.Element("root").Element("systemMemInfo");

            if (systemStats != null)
            {
                var stats = new[]
                {
                     new { Name = "KernelRamConsumption", Value = (string)systemStats.Element("kernelRamConsumption") },
                     new { Name = "UserRamConsumption", Value = (string)systemStats.Element("userRamConsumption") },
                     new { Name = "UnavailableRamUnits", Value = (string)systemStats.Element("unavailableRamUnits") },
                     new { Name = "AvailableRamUnits", Value = (string)systemStats.Element("availableRamUnits") }
                };

                foreach (var stat in stats)
                {
                    result += $"<Counter Category=\"SystemStats\" Name=\"{stat.Name}\" Value=\"{stat.Value}\" />\n";
                }
            }
            if (systemMemInfo != null)
            {
                var memInfo = new[]
                {
                     new { Name = "MemTotal", Value = (string)systemMemInfo.Element("MemTotal") },
                     new { Name = "MemFree", Value = (string)systemMemInfo.Element("MemFree") },
                     new { Name = "MemAvailable", Value = (string)systemMemInfo.Element("MemAvailable") },
                     new { Name = "Buffers", Value = (string)systemMemInfo.Element("Buffers") },
                     new { Name = "Cached", Value = (string)systemMemInfo.Element("Cached") },
                     new { Name = "Dirty", Value = (string)systemMemInfo.Element("Dirty") },
                     new { Name = "Mapped", Value = (string)systemMemInfo.Element("Mapped") }
                };

                foreach (var info in memInfo)
                {
                    result += $"<Counter Category=\"SystemMemInfo\" Name=\"{info.Name}\" Value=\"{info.Value}\" />\n";
                }
            }
            string stringXml = $"<GetSnapshot>\r\n{result}\r\n</GetSnapshot>\n";
            return stringXml;
        }

        public static Dictionary<string, long> ProcessMemoryData(XDocument memoryData)
        {
            Dictionary<string, long> dictionary = new Dictionary<string, long>();
            CategoryLabelParser categoryLabelParser = new CategoryLabelParser(memoryData, "ExtensionPool,MaxBytes;ExtensionPool,Extension;ExtensionPool,ConsumedBytes;ExtensionPool,HighWaterBytes;ExtensionPool,CurrentAllocations;DataModelSys,MaxBytes;DataModelSys,Extension;DataModelSys,ConsumedBytes;DataModelSys,HighWaterBytes;DataModelSys,CurrentAllocations;DataModelBlob,MaxBytes;DataModelBlob,Extension;DataModelBlob,ConsumedBytes;DataModelBlob,HighWaterBytes;DataModelBlob,CurrentAllocations;PDL,MaxBytes;PDL,Extension;PDL,ConsumedBytes;PDL,HighWaterBytes;PDL,CurrentAllocations;ScanStripPool,MaxBytes;ScanStripPool,Extension;ScanStripPool,ConsumedBytes;ScanStripPool,HighWaterBytes;ScanStripPool,CurrentAllocations;ScanStripPool_Two,MaxBytes;ScanStripPool_Two,Extension;ScanStripPool_Two,ConsumedBytes;ScanStripPool_Two,HighWaterBytes;ScanStripPool_Two,CurrentAllocations;PageStorePool,MaxBytes;PageStorePool,Extension;PageStorePool,ConsumedBytes;PageStorePool,HighWaterBytes;PageStorePool,CurrentAllocations;PageAssemblerPool,MaxBytes;PageAssemblerPool,Extension;PageAssemblerPool,ConsumedBytes;PageAssemblerPool,HighWaterBytes;PageAssemblerPool,CurrentAllocations;ImageProcessor,MaxBytes;ImageProcessor,Extension;ImageProcessor,ConsumedBytes;ImageProcessor,HighWaterBytes;ImageProcessor,CurrentAllocations;DecompressionPool,MaxBytes;DecompressionPool,Extension;DecompressionPool,ConsumedBytes;DecompressionPool,HighWaterBytes;DecompressionPool,CurrentAllocations;MarkingFilterPool,MaxBytes;MarkingFilterPool,Extension;MarkingFilterPool,ConsumedBytes;MarkingFilterPool,HighWaterBytes;MarkingFilterPool,CurrentAllocations;VideoTableStore,MaxBytes;VideoTableStore,Extension;VideoTableStore,ConsumedBytes;VideoTableStore,HighWaterBytes;VideoTableStore,CurrentAllocations;EngineData,MaxBytes;EngineData,Extension;EngineData,ConsumedBytes;EngineData,HighWaterBytes;EngineData,CurrentAllocations;ImagePersisterPool,MaxBytes;ImagePersisterPool,Extension;ImagePersisterPool,ConsumedBytes;ImagePersisterPool,HighWaterBytes;ImagePersisterPool,CurrentAllocations;ImagePipePool,MaxBytes;ImagePipePool,Extension;ImagePipePool,ConsumedBytes;ImagePipePool,HighWaterBytes;ImagePipePool,CurrentAllocations;TableManagerPool,MaxBytes;TableManagerPool,Extension;TableManagerPool,ConsumedBytes;TableManagerPool,HighWaterBytes;TableManagerPool,CurrentAllocations;FinalToc,MaxBytes;FinalToc,Extension;FinalToc,ConsumedBytes;FinalToc,HighWaterBytes;FinalToc,CurrentAllocations;FormatterToJpegHardwarePool,MaxBytes;FormatterToJpegHardwarePool,Extension;FormatterToJpegHardwarePool,ConsumedBytes;FormatterToJpegHardwarePool,HighWaterBytes;FormatterToJpegHardwarePool,CurrentAllocations;VideoControllerTables,MaxBytes;VideoControllerTables,Extension;VideoControllerTables,ConsumedBytes;VideoControllerTables,HighWaterBytes;VideoControllerTables,CurrentAllocations;SystemStats,KernelRamConsumption;SystemStats,UserRamConsumption;SystemStats,UnavailableRamUnits;SystemStats,AvailableRamUnits;SystemMemInfo,MemTotal;SystemMemInfo,MemFree;SystemMemInfo,MemAvailable;SystemMemInfo,Buffers;SystemMemInfo,Cached;SystemMemInfo,Dirty;SystemMemInfo,Mapped");
            IEnumerable<DeviceMemoryCountLog> enumerable1 = categoryLabelParser.ProcessMemoryData();
            foreach (DeviceMemoryCountLog item in enumerable1)
            {
                if (!dictionary.ContainsKey(item.CategoryName + "_" + item.DataLabel))
                {
                    dictionary.Add(item.CategoryName + "_" + item.DataLabel, item.DataValue);
                }
            }
            return dictionary;
        }

        [KeywordDescription("Draw Dune Device Memory Usage Graph")]
        [KeywordDisplayName("Draw Memory Usage Graph")]
        [SampleScript("Dune.Draw Memory Usage Graph")]
        public KeywordResult DrawMemoryUsageGraph()
        {
            try
            {
                ChartCreator chart = new ChartCreator(_memoryUsage.GetCSVPath());
                string chartImage = Path.Combine(_outputDir, $"{_testCaseName}_DuneMemory.png");

                chart.SaveImage(chartImage, System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line, true, null);

                if (CommonExecutionInfo.CurrentRepeatCount > 0)
                {
                    File.Copy(chartImage, Path.Combine(Directory.GetParent(_outputDir).FullName, $"{_testCaseName}_Dune_Memory_Usage.png"), true);
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

        [KeywordDescription("Get the Dune device Firmware Version")]
        [KeywordDisplayName("Get Firmware Version")]
        [KeywordParameters("firmwareVersion", "variable to store Firmware Version")]
        [SampleScript("Dune.Get Firmware Version (${firmwareVersion})")]
        public KeywordResult GetFirmwareVersion(string variable)
        {
            KeywordResult keywordResult = new KeywordResult(KeywordResults.Fail);
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                var firmwareVersion = _device.GetDeviceInfo();
                if (firmwareVersion != null)
                {
                    string version = firmwareVersion.FirmwareRevision.ToString();
                    CommonExecutionInfo.SetVariable(variable.Trim(), version);
                    keywordResult = new KeywordResult(KeywordResults.Pass)
                    {
                        Output = $"{variable} = {version}"
                    };
                    return keywordResult;
                }
                else
                {
                    keywordResult.Result = KeywordResults.Fail;
                    keywordResult.Output = $"Firmware Version is not Available";
                    keywordResult.ScreenShot = CaptureScreen();
                    Logger.Error(keywordResult.Output);
                    return keywordResult;
                }
            }
            catch (Exception ex)
            {
                keywordResult.Result = KeywordResults.Error;
                keywordResult.AdditionalInfo = ex.ToString();
                keywordResult.Output = "Unexpected error";
                keywordResult.ScreenShot = CaptureScreen();
                Logger.Error(keywordResult.Output, ex);
                return keywordResult;

            }
        }

        [KeywordDescription("Check the status of checkbox, returns 'true', if checkbox is checked, 'false' otherwise")]
        [KeywordDisplayName("IsCheckboxStatus")]
        [KeywordParameters("querySelector", "querySelector of checkbox element")]
        [KeywordParameters("saveTo", "status value to be saved")]
        [SampleScript("Dune.IsCheckboxStatus (#sameWidthAllEdgesCheckbox,${value})")]
        public KeywordResult IsCheckboxStatus(string querySelector, string saveTo)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            try
            {
                if (_workflowUI.IsExist(querySelector))
                {
                    _workflowUI.WaitFor(querySelector, _defaultCPDelay);
                    string status = _workflowUI.GetCheckboxStatus(querySelector);
                    if (status != null)
                    {
                        CommonExecutionInfo.SetVariable(saveTo, status.ToString());
                        kr.Result = KeywordResults.Pass;
                        kr.Output = $"{saveTo} : {status}";
                    }
                    else
                    {
                        kr.Result = KeywordResults.Fail;
                        kr.Output = $"IsCheckboxStatus returned null status for {querySelector}";
                        return kr;
                    }
                }
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Error occured in IsCheckboxStatus::{ex.Message}";
                Logger.Error(kr.Output, ex);
                return kr;
            }
        }

        [KeywordDescription("Display list of Partner Solutions installed in the Dune Device")]
        [KeywordDisplayName("Display Partner Solutions")]
        [SampleScript("Dune.Display Partner Solutions")]
        public KeywordResult DisplayPartnerSolutions()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            StringBuilder partnersBuilder = new StringBuilder();
            try
            {
                List<string> listParnetSolutions = new List<string>();
                listParnetSolutions = FetchWebAppTitles();
                if (listParnetSolutions.Count > 0)
                {
                    for (int i = 0; i < listParnetSolutions.Count; i++)
                    {
                        var solutionName = listParnetSolutions[i];
                        partnersBuilder.Append(solutionName);
                        if (i < listParnetSolutions.Count - 1)
                        {
                            partnersBuilder.Append(", ");
                        }
                    }
                }
                else
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"No Partner solutions are installed on the machine";
                    kr.ScreenShot = CaptureScreen();
                    Logger.Error(kr.Output);
                    return kr;
                }
                kr.Output = partnersBuilder.ToString().Trim();
                kr.Result = KeywordResults.Pass;
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Unexpected error";
                Logger.Error(kr.Output, ex);
                return kr;
            }
        }

        [KeywordDescription("Swipe to the page on home screen where application is located")]
        [KeywordDisplayName("Navigate To Application On Home Screen")]
        [KeywordParameters("querySelector", "Query Selector of the application on the screen")]
        [SampleScript("Dune.Navigate To Application On Home Screen(#Random-GUID-Value-or-text)")]
        public KeywordResult NavigateToApplicationOnHomeScreen(string querySelector)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                bool response = _workflowUI.NavigateToAppOnHomeScreen(querySelector);
                if(!response)
                {
                    kr.Result = KeywordResults.Fail;
                }
                kr.Result = KeywordResults.Pass;
                return kr;
            }
            catch (DuneControlPanelOperationException ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                if (!_disableScreenShotForFails)
                {
                    kr.ScreenShot = CaptureScreen();
                }
                kr.Output = $"Navigation to application failed. QuerySelector = {querySelector}";
                Logger.Error(kr.Output, ex);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Unexpected error while swiping to application page on homescreen";
                Logger.Error(kr.Output, ex);
                return kr;
            }
        }

        // Method to fetch titles where type is "webApp"
        public List<string> FetchWebAppTitles()
        {
            List<string> webAppList = new List<string>();
            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = client.GetAsync($"https://{_dut.DeviceAddress}/cdm/shortcut/v1/shortcuts").Result;
                    response.EnsureSuccessStatusCode();
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;
                    JObject jsonData = JObject.Parse(jsonResponse);
                    var webAppTitles = ExtractWebAppTitles(jsonData);
                    Logger.Debug("WebApp Titles:");
                    foreach (var title in webAppTitles)
                    {
                        webAppList.Add(title);
                    }
                    return webAppList;
                }
                catch (Exception ex)
                {
                    Logger.Debug($"An error occurred: {ex.Message}");
                    return null;
                }
            }
        }

        // Method to extract titles where type is "webApp"
        private IEnumerable<string> ExtractWebAppTitles(JObject jsonData)
        {
            List<string> titles = new List<string>();
            var items = jsonData["shortcuts"] ?? jsonData;
            if (items != null)
            {
                foreach (var item in items)
                {
                    var type = item["type"]?.ToString();
                    var title = item["title"]?.ToString();
                    if (!string.IsNullOrEmpty(type) && type.Equals("webApp", StringComparison.OrdinalIgnoreCase))
                    {
                        titles.Add(title);
                    }
                }
            }
            return titles;
        }
        private async Task<string> GetMemoryDataFromUrl(string url)
        {
            try
            {
                Logger.Trace($"Fetching memory data from URL: {url}");

                // Set timeout to prevent indefinite hanging
                using (var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(30)))
                {
                    HttpResponseMessage response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseContentRead, cts.Token);
                    response.EnsureSuccessStatusCode();
                    string jsonData = await response.Content.ReadAsStringAsync();
                    Logger.Trace($"Successfully retrieved JSON data from {url}");
                    return jsonData;
                }
            }
            catch (System.OperationCanceledException ex)
            {
                Logger.Error($"Request timeout while fetching data from URL: {url}. Timeout: 30 seconds");
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error fetching data from URL: {ex.Message}");
                throw;
            }
        }

        [KeywordDescription("Initialize Memory monitoring for Dune Device. Collects memory data at specified interval.")]
        [KeywordDisplayName("Start Memory Monitoring")]
        [KeywordParameters("time", "Interval in seconds for automatic memory data collection")]
        [SampleScript("Dune.Start Memory Monitoring(20)")]

        public KeywordResult StartMemoryMonitoring(string time)
        {
            string csvPath;

            _testCaseName = CommonExecutionInfo.GetVariable("_tcName");

            // Early validation and parsing
            if (string.IsNullOrWhiteSpace(time))
            {
                Logger.Error("Invalid input: 'time' parameter is null or empty. Please provide a valid time value.");
                return new KeywordResult(KeywordResults.Fail)
                {
                    Output = "Invalid input: 'time' parameter is null or empty. Please provide a valid time value."
                };
            }

            if (!int.TryParse(time, out int intervalSeconds) || intervalSeconds <= 0)
            {
                Logger.Error($"Invalid input: 'time' parameter must be a positive integer. Provided value: {time}");
                return new KeywordResult(KeywordResults.Fail)
                {
                    Output = $"Invalid input: 'time' parameter must be a positive integer. Provided value: {time}"
                };
            }

            // Determine CSV path once
            if (CommonExecutionInfo.CurrentRepeatCount > 0)
            {
                string parentDir = Directory.GetParent(_outputDir).FullName;
                csvPath = Path.Combine(parentDir, "Memory_Usage.csv");
                Logger.Trace($"Current Repeat Count is greater than 0. CSV Path is : {csvPath}");
            }
            else
            {
                csvPath = Path.Combine(_outputDir, $"{_testCaseName}_DuneMemoryMonitoring_{DateTime.Now:yyMMdd_HHmmssff}.csv");
                Logger.Trace($"CSV Path is : {csvPath}");
            }

            _csvPath = csvPath;
            _memoryUsage = new MemoryUsageItem(csvPath);

            // Dispose previous timer to prevent resource leaks
            if (_memoryTimer != null)
            {
                _memoryTimer.Stop();
                _memoryTimer.Elapsed -= MemoryTimerElapsed;
                _memoryTimer.Dispose();
            }

            // Configure timer in single block
            int intervalMilliseconds = intervalSeconds * 1000;
            _memoryTimer = new System.Timers.Timer(intervalMilliseconds)
            {
                AutoReset = true,
                Enabled = true
            };
            _memoryTimer.Elapsed += MemoryTimerElapsed;
            _memoryTimer.Start();

            return new KeywordResult(KeywordResults.Pass)
            {
                Output = $"Memory Monitoring started (every {intervalSeconds} seconds). Data will be saved to : {csvPath}"
            };
        }


        [KeywordDescription("Dump memory usage and save with given check point name")]
        [KeywordDisplayName("Collect Memory Details")]
        [KeywordParameters("checkPoint", "Check Point name to save (e.g., job type or stage identifier)")]
        [SampleScript("Ares.Collect Memory Details ()")]
        public KeywordResult CollectMemoryDetails()
        {
            try
            {
                // Execute async operation synchronously - required by keyword framework
                return CollectMemoryDetailsAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return new KeywordResult(KeywordResults.Fail)
                {
                    Output = "Fail during dump memory",
                    AdditionalInfo = ex.ToString()
                };
            }
        }

        private async Task<KeywordResult> CollectMemoryDetailsAsync()
        {
            if (_memoryUsage == null)
            {
                Logger.Error("Memory monitoring not started. Call StartMemoryMonitoring first.");
                throw new InvalidOperationException(
                    "Memory monitoring not started. Call StartMemoryMonitoring first.");
            }

            const string endpoint = "/cdm/system/v1/statistics";
            string url = $"https://{_dut.DeviceAddress}{endpoint}";

            string response;

            try
            {
                response = await GetMemoryDataFromUrl(url).ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(response))
                {
                    Logger.Error($"Memory statistics response is empty. URL: {url}");
                    return new KeywordResult(KeywordResults.Fail)
                    {
                        Output = "Memory statistics response is null or empty."
                    };
                }
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("404"))
            {
                Logger.Error($"Memory statistics endpoint not found: {url}");
                return new KeywordResult(KeywordResults.Fail)
                {
                    Output = "Memory statistics endpoint not found (404). Device may not support this feature.",
                    AdditionalInfo = ex.Message
                };
            }
            catch (System.OperationCanceledException ex)
            {
                Logger.Error($"Request timeout while fetching memory data from {url}");
                return new KeywordResult(KeywordResults.Fail)
                {
                    Output = "Request timeout while fetching memory statistics.",
                    AdditionalInfo = ex.Message
                };
            }
            catch (Exception ex)
            {
                Logger.Error($"Error fetching memory data: {ex.Message}");
                return new KeywordResult(KeywordResults.Fail)
                {
                    Output = "Error fetching memory statistics from device.",
                    AdditionalInfo = ex.Message
                };
            }

            JObject memoryJson;

            try
            {
                memoryJson = JObject.Parse(response);
            }
            catch (Newtonsoft.Json.JsonReaderException ex)
            {
                Logger.Error($"Invalid JSON received from {url}. Error: {ex.Message}");
                return new KeywordResult(KeywordResults.Fail)
                {
                    Output = "Invalid JSON received in memory statistics response.",
                    AdditionalInfo = ex.Message
                };
            }

            long totalMemory = memoryJson.Value<long?>("totalMemory") ?? 0;
            long availableMemory = memoryJson.Value<long?>("availableMemory") ?? 0;
            long usedMemory = Math.Max(0, totalMemory - availableMemory);

            double usagePercentage = totalMemory > 0
                ? (usedMemory * 100.0) / totalMemory
                : 0;

            _memoryUsage.DataAppend(new Dictionary<string, object>
            {
                ["Timestamp"] = DateTime.Now.ToString("HH:mm:ss tt"),
                ["Metric_name"] = "AvailableRamUnits",
                ["AvailableMemory"] = availableMemory,
                ["TotalMemory"] = totalMemory,
                ["UsedMemory_KB"] = usedMemory,
                ["MemoryUsagePercentage"] = $"{usagePercentage:F2}%"
            });

            return new KeywordResult(KeywordResults.Pass)
            {
                Output = $"Memory data collected. Usage: {usagePercentage:F2}% ({usedMemory} KB used / {totalMemory} KB total)"
            };
        }
        private void MemoryTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Check for disposal
            if (_memoryUsage == null || _memoryTimer == null)
            {
                return;
            }

            try
            {
                // Use cached format and null-coalescing for safety
                string timestamp = $"{DateTime.Now:HH:mm:ss tt}";
                CollectMemoryDetails();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error during automatic memory collection: {ex.Message}");
            }
        }

        [KeywordDescription("Stop Memory monitoring for Dune Device")]
        [KeywordDisplayName("Stop Memory Monitoring")]
        [SampleScript("Dune.Stop Memory Monitoring")]
        public KeywordResult StopMemoryMonitoring()
        {
            if (_memoryTimer != null)
            {
                _memoryTimer.Stop();
                _memoryTimer.Dispose();
                _memoryTimer = null;
            }
            return new KeywordResult(KeywordResults.Pass);
        }
  
    }

}
    #endregion



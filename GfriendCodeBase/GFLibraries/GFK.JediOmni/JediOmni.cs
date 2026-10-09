using HP.DeviceAutomation;
using HP.DeviceAutomation.Jedi;
using HP.DeviceAutomation.Jedi.OmniUserInteraction;
using HP.GFriend.External.GRWM.Jedi;
using HP.GFriend.GFLogger;
using HP.GFriend.Keywords.Event;
using HP.GFriend.Support;
using HP.GFriend.Utils.Charter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Logger = HP.GFriend.GFLogger.Logger;

namespace HP.GFriend.Keywords
{
    [LibraryDescription("Admin password must be set to use JediOmni library\r\n" +
        "If you are using debug board, LAN Debug address also must be given in Device information.")]
    public class JediOmni : IGFLibrary
    {
        public string _testCaseName = "";

        public class TempWriter : TextWriter
        {
            public string output = string.Empty;
            public override Encoding Encoding { get { return Encoding.UTF8; } }
            public override void Write(string value)
            {
                output += value;
            }

            public override void WriteLine(string value)
            {
                output += (value + Environment.NewLine);
            }
        }

        /// <summary>
        /// Control the JediOmniDevice
        /// </summary>
        private DeviceUnderTest _dut;
        private JediOmniDevice _device;
        private DeviceControl _deviceControl;
        private static string _outputDir;
        private int _timeOut = 3;
        private int resolution_x = 800, resolution_y = 600;
        private bool _logAttached = false;

        private static string _activeJobsButton = ".hp-button-active-jobs:last";
        private int _scaleFactor = 1;
        private static MemoryUsageItem _memoryUsage = null;

        // Consts
        private const string _uel = "\x1B\x25\x2D\x31\x32\x33\x34\x35\x58";
        private const string _crlf = "\x0D\x0A";
   
        private EventHandler<HP.DeviceAutomation.LogEventArgs> logTrace;
        private EventHandler<HP.DeviceAutomation.LogEventArgs> logDebug;
        private EventHandler<HP.DeviceAutomation.LogEventArgs> logWarn;
        private EventHandler<HP.DeviceAutomation.LogEventArgs> logError;


        /// <summary>
        /// Initialization to connect JediOmni UI
        /// </summary>
        public string GetName()
        {
            return "JediOmni";
        }

        public List<string> GetDependencies()
        {
            return null;
        }

        public bool DutUsed()
        {
            return true;
        }
        /// <summary>
        /// Initialization to connect JediOmni UI
        /// </summary>
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
            _dut = dut;
            _outputDir = outputDir;
            try
            {
                CreateJediOmniDevice(dut);
            }
            catch (Exception ex)
            {
                Logger.Error("Connection error", ex);
                throw;
            }

            _deviceControl = new DeviceControl();
            try
            {
                _deviceControl.Reset(_device);
            }
            catch (Exception)
            {
                Logger.Warn("Exception during reset. Try to force disconnect");
                try
                {
                    WebInspector w;
                    w = new WebInspector(dut.DeviceAddress, 9222, TimeSpan.FromSeconds(30));
                    foreach (WebInspectorPage p in w.DiscoverInspectablePages().ToList())
                    {
                        Logger.Debug($"Disconnect : {p.Uri}");
                        w.ForceDisconnect(p);
                    }
                }
                catch (Exception inspectorEx)
                {
                    Logger.Warn($"WebInspector force-disconnect skipped: {inspectorEx.Message}");
                }
                _device.Dispose();
                CreateJediOmniDevice(dut);
                _deviceControl.Reset(_device);
            }


            var result = _deviceControl.GetResolution(_device);
            resolution_x = result.Item1;
            resolution_y = result.Item2;

            CommonExecutionInfo.SetSharedObject("jediOmni:" + _dut.DeviceId, _device);
        }



        /// <summary>
        /// Get device resolution 
        /// </summary>
        /// <returns> x resolution , y resolution</returns>
        public Tuple<int, int> GetResolution()
        {
            return new Tuple<int, int>(resolution_x, resolution_y);
        }

        /// <summary>
        /// Self-healing of connection problems during keyword processing.
        /// </summary>
        /// <param name="ex">WebInspectorException</param>
        /// <param name="keyword">Keyword name.</param>
        /// <param name="output">KeywordResult output message.</param>
        /// <returns></returns>
        private KeywordResult SelfReconnection(WebInspectorException ex, string keyword, string output)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Error);
            Logger.Error($"WebInspector Connection Error during {keyword}.", ex);
            Logger.Debug("Trying to reconnect the target device.");

            if (ReConnect().Result == KeywordResults.Pass)
            {
                Logger.Debug("Self reconnection successful.");
            }
            else
            {
                Logger.Warn("Self reconnection has failed.");
            }

            kr.AdditionalInfo = ex.ToString();
            kr.ScreenShot = CaptureScreen();
            kr.Output = output;
            return kr;
        }

        private void CreateJediOmniDevice(DeviceUnderTest dut)
        {
            if (!string.IsNullOrEmpty(dut.LanDebugAddress))
            {
                var parameters = new DeviceConstructionParameterCollection(
                    new DeviceAddressParameter(dut.DeviceAddress),
                    new DeviceAdminPasswordParameter(dut.AdminPassword),
                    new JediDebugAddressParameter(dut.LanDebugAddress)
                    );

                _device = new JediOmniDevice(parameters);
            }
            else
            {
                _device = new JediOmniDevice(dut.DeviceAddress, dut.AdminPassword);
            }
        }

        /// <summary>
        /// Dispose fuction to connect JediOmni UI
        /// </summary>
        public void Dispose()
        {
            if (_logAttached)
            {
                HP.DeviceAutomation.Logger.OnTrace -= logTrace;
                HP.DeviceAutomation.Logger.OnDebug -= logDebug;
                HP.DeviceAutomation.Logger.OnWarn -= logWarn;
                HP.DeviceAutomation.Logger.OnError -= logError;
                _logAttached = false;
            }
            if (_device != null)
            {
                TextWriter currentConsoleOut = Console.Out;
                Console.SetOut(new TempWriter());
                _device.Dispose();
                Console.SetOut(currentConsoleOut);
                _device = null;
                CommonExecutionInfo.RemoveSharedObject("jediOmni:" + _dut.DeviceId);
            }
        }

        /// <summary>
        /// Capture Omin Screen and Convert to byte
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

        private void AllowPJLAnyway()
        {
            string urn = "urn:hp:imaging:con:service:security:SecurityService";
            string endpoint = "security";

            WebServiceTicket tic = _device.WebServices.GetDeviceTicket(endpoint, urn);
            string oldValue = tic.FindElement("PjlDeviceAccess").Value;
            if (oldValue == "disabled")
            {
                tic.FindElement("PjlDeviceAccess").SetValue("enabled");
                _device.WebServices.PutDeviceTicket(endpoint, urn, tic);
            }
        }

        private Tuple<int, int> GetAbsolutePosition(string x, string y)
        {
            double dX = ((Int32.Parse(x)) / (double)100);
            double dY = ((Int32.Parse(y)) / (double)100);

            int aX = (int)(resolution_x * dX);
            int aY = (int)(resolution_y * dY);

            return new Tuple<int, int>(aX, aY);
        }

        /// <summary>
        /// Determines whether the Active Jobs button is visible.
        /// </summary>
        /// <returns><c>true</c> if the active jobs button is visible, <c>false</c> otherwise.</returns>
        public bool ActiveJobsButtonVisible()
        {
            return _device.ControlPanel.GetValue(_activeJobsButton, "display", OmniPropertyType.Css) != "none";
        }


        public bool IsDeviceTurnedOn()
        {
            List<PowerState> turnedOnPowerStates = new List<PowerState> { PowerState.Awake, PowerState.PowerSave, PowerState.Sleep, PowerState.OneWatt };
            List<PrinterStatus> turnedOnPrinterStauses = new List<PrinterStatus> { PrinterStatus.Idle, PrinterStatus.Other, PrinterStatus.Printing, PrinterStatus.WarmUp };
            List<DeviceStatus> turnedOffDeviceStatuese = new List<DeviceStatus> { DeviceStatus.Down, DeviceStatus.None };
            PowerState powerState = _device.PowerManagement.GetPowerState();
            PrinterStatus printerStatus = _device.GetPrinterStatus();
            DeviceStatus deviceStatus = _device.GetDeviceStatus();

            Logger.Trace($"Power State : {powerState.ToString()}, Printer Status : {printerStatus.ToString()}, Device Status : {deviceStatus.ToString()}");

            if (turnedOnPowerStates.Contains(powerState) && turnedOnPrinterStauses.Contains(printerStatus) && !turnedOffDeviceStatuese.Contains(deviceStatus))
            {
                return true;
            }
            return false;

        }

        public bool IsDeviceTurnedOff()
        {
            PowerState powerState = _device.PowerManagement.GetPowerState();
            PrinterStatus printerStatus = _device.GetPrinterStatus();
            DeviceStatus deviceStatus = _device.GetDeviceStatus();

            Logger.Trace($"Power State : {powerState.ToString()}, Printer Status : {printerStatus.ToString()}, Device Status : {deviceStatus.ToString()}");

            if (printerStatus.Equals(PrinterStatus.None) && deviceStatus.Equals(DeviceStatus.None))
            {
                return true;
            }
            return false;

        }

        [KeywordDescription("Set timeout scacle for applying all keywords in library.\r\n" +
            "This scale facor will be multiplied of all timeout arguments in keywords.\r\n" +
            "For example, if scale factor is set to 2 and call wait for object for 3 secondes, it will wait for 6 (3 x 2) seconds.")]
        [KeywordDisplayName("Set Timeout Scale")]
        [KeywordParameters("scaleFactor", "scale factor. Should be a number and larger than 0")]
        [SampleScript("JediOmni.Set Timeout Scale (10)")]
        public KeywordResult SetTimeoutScale(string scaleFactor)
        {
            if (!int.TryParse(scaleFactor.Trim(), out _scaleFactor) || _scaleFactor < 1)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Scale factor must be a number and should be larger than 0";
                return error;
            }
            _timeOut = _timeOut * _scaleFactor;
            KeywordResult pass = new KeywordResult(KeywordResults.Pass);
            pass.Output = $"Scale factor is set to {_scaleFactor}";
            return pass;
        }

        [KeywordDescription("Capture Screen Shot of device and save to PC")]
        [KeywordDisplayName("Capture Screen Shot")]
        [KeywordParameters("filename", "filename to save")]
        [SampleScript("JediOmni.Capture Screen Shot (SignOut.jpg)")]
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
            catch (WebInspectorException ex)
            {
                string output = $"Capture screen shot failed with given filename :: {filename}";
                kr = SelfReconnection(ex, "CaptureScreenShot", output);
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


        [KeywordDescription("Check the screen contains given text (exact match)")]
        [KeywordDisplayName("Check Screen Contains Full Text")]
        [KeywordParameters("text", "Text to check screen contains given text")]
        [SampleScript("JedioOmni.Check Screen Contains Full Text(copy)")]
        public KeywordResult CheckScreenContainsFullText(string text)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            try
            {
                if (_device.ControlPanel.WaitForState($"div[aria-label=\"{text}\"]",
                    OmniElementState.VisibleCompletely, TimeSpan.FromMilliseconds(10))
                    || _device.ControlPanel.WaitForState($"span[aria-label=\"{text}\"]",
                    OmniElementState.VisibleCompletely, TimeSpan.FromMilliseconds(10)))
                {
                    kr.Result = KeywordResults.Pass;
                    return kr;
                }
                kr.Output = $"Check failed with given text :: {text}";
                kr.ScreenShot = CaptureScreen();
                return kr;
            }
            catch (WebInspectorException ex)
            {
                string output = $"Check error with given text :: {text}";
                kr = SelfReconnection(ex, "CheckScreenContainsFullText", output);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Check error with given text :: {text}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"CheckScreenContainsFullText({text}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Check the screen partially contains given text")]
        [KeywordDisplayName("Check Screen Contains Partial Text")]
        [KeywordParameters("text", "Text to check screen contains given text")]
        [SampleScript("Jediomni.Check Screen Contains Partial Text(cop)")]
        public KeywordResult CheckScreenContainsPartialText(string text)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            try
            {
                if (_device.ControlPanel.WaitForState($"div[aria-label*=\"{text}\"]",
                    OmniElementState.VisibleCompletely, TimeSpan.FromMilliseconds(10))
                    || _device.ControlPanel.WaitForState($"span[aria-label*=\"{text}\"]",
                    OmniElementState.VisibleCompletely, TimeSpan.FromMilliseconds(10)))
                {
                    kr.Result = KeywordResults.Pass;
                    return kr;
                }
                kr.Output = $"Check failed with given text :: {text}";
                kr.ScreenShot = CaptureScreen();
                return kr;
            }
            catch (WebInspectorException ex)
            {
                string output = $"Check error with given text :: {text}";
                kr = SelfReconnection(ex, "CheckScreenContainsPartialText", output);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Check error with given text :: {text}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"CheckScreenContainsPartialText({text}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Check the screen does not contains given text (exact match)")]
        [KeywordDisplayName("Check Screen Not Contains Full Text")]
        [KeywordParameters("text", "Text to check screen does not contains given text")]
        [SampleScript("JediOmni.Check Screen Not Contains Full Text (Copy)")]
        public KeywordResult CheckScreenNotContainsFullText(string text)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device.ControlPanel.WaitForState($"div[aria-label=\"{text}\"]",
                    OmniElementState.VisibleCompletely, TimeSpan.FromMilliseconds(10))
                    || _device.ControlPanel.WaitForState($"span[aria-label=\"{text}\"]",
                    OmniElementState.VisibleCompletely, TimeSpan.FromMilliseconds(10)))
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Check failed with given text :: {text}";
                    kr.ScreenShot = CaptureScreen();
                    return kr;
                }
                return kr;
            }
            catch (WebInspectorException ex)
            {
                string output = $"Check error with given text :: {text}";
                kr = SelfReconnection(ex, "CheckScreenNotContainsFullText", output);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Check error with given text :: {text}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"CheckScreenNotContainsFullText({text}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Check the screen does not contains any partial given text")]
        [KeywordDisplayName("Check Screen Not Contains Partial Text")]
        [KeywordParameters("text", "Text to check screen does not contains any partial given text")]
        [SampleScript("JediOmni.Check Screen Not Contains Partial Text (Cop)")]
        public KeywordResult CheckScreenNotContainsPartialText(string text)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device.ControlPanel.WaitForState($"div[aria-label*=\"{text}\"]",
                    OmniElementState.VisibleCompletely, TimeSpan.FromMilliseconds(10))
                    || _device.ControlPanel.WaitForState($"span[aria-label*=\"{text}\"]",
                    OmniElementState.VisibleCompletely, TimeSpan.FromMilliseconds(10)))
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Check failed with given text :: {text}";
                    kr.ScreenShot = CaptureScreen();
                    return kr;
                }
                return kr;
            }
            catch (WebInspectorException ex)
            {
                string output = $"Check error with given text :: {text}";
                kr = SelfReconnection(ex, "CheckScreenNotContainsPartialText", output);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Check error with given text :: {text}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"CheckScreenNotContainsPartialText({text}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Custom Drag with given position (percent value)")]
        [KeywordDisplayName("Drag")]
        [KeywordParameters("x1, y1, x2, y2 ", "percent value to drag with given position value")]
        [SampleScript("JediOmni.Drag (50,45,20,40)")]
        public KeywordResult Drag(string X1, string Y1, string X2, string Y2)
        {
            double x1 = Convert.ToInt32(X1), x2 = Convert.ToInt32(X2);
            double y1 = Convert.ToInt32(Y1), y2 = Convert.ToInt32(Y2);
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            if (resolution_x == 0 || resolution_y == 0)
            {
                kr.Output = $"Resolution is not configured in Initialize function";
                return kr;
            }
            Logger.Debug($"\nThis UI Resoultion is  : {resolution_x} * {resolution_y} ");
            x1 = resolution_x * (x1 / (double)100); x2 = resolution_x * (x2 / (double)100);
            y1 = resolution_y * (y1 / (double)100); y2 = resolution_y * (y2 / (double)100);
            Logger.Debug($"X1 = {x1}, Y1 = {y1}, X2 = {x2}, Y2 = {y2}");
            Coordinate start = new Coordinate((int)x1, (int)y1);
            Coordinate end = new Coordinate((int)x2, (int)y2);
            try
            {
                _device.ControlPanel.SwipeScreen(start, end, TimeSpan.FromMilliseconds(250));
                kr.Result = KeywordResults.Pass;
                return kr;
            }
            catch (WebInspectorException ex)
            {
                string output = $"Drag error with given value :: {x1},{y1},{x2},{y2}";
                kr = SelfReconnection(ex, "Drag", output);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Drag error with given value :: {x1},{y1},{x2},{y2}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"Drag({x1}, {y1}, {x2}, {y2}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Input Text to element id")]
        [KeywordDisplayName("Input Text")]
        [KeywordParameters("id", "Omni element id of text field")]
        [KeywordParameters("text", "Text to Input")]
        [Deprecated("Use InputText(text) to current position instead of this.")]
        [SampleScript("JediOmni.Input Text (hpid-signin-textbox-PinTextBox,username)")]
        public KeywordResult InputText(string id, string text)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            try
            {
                if (_device.ControlPanel.WaitForState($"#{id}",
                    OmniElementState.Exists, TimeSpan.FromSeconds(_timeOut)))
                {
                    _deviceControl.InputString(_device, text);
                    _device.ControlPanel.PressScreen(resolution_x / 2, resolution_y / 20);
                    kr.Result = KeywordResults.Pass;
                    return kr;
                }
                kr.Output = $"Can not find object with {id}";
                kr.ScreenShot = CaptureScreen();
                return kr;
            }
            catch (WebInspectorException ex)
            {
                string output = $"Input text error with given text :: {text}";
                kr = SelfReconnection(ex, "InputText", output);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Input text error with given text :: {text}";
                Logger.Error($"InputText({id}, {text}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Input Text to current position")]
        [KeywordDisplayName("Input Text")]
        [KeywordParameters("text", "Text to Input")]
        [SampleScript("JediOmni.Input Text (SafeComID)")]
        public KeywordResult InputText(string text)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            try
            {
                _deviceControl.InputString(_device, text);
                kr.Result = KeywordResults.Pass;
                return kr;
            }
            catch (WebInspectorException ex)
            {
                string output = $"Input text error with given text :: {text}";
                kr = SelfReconnection(ex, "InputText", output);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Input text error with given text :: {text}";
                Logger.Error($"InputText({text}) fail", ex);
                return kr;
            }
        }

        [GetKeyword]
        [KeywordDescription("Get text of selected object")]
        [KeywordDisplayName("Get Text")]
        [KeywordParameters("id", "Omni element id to get text")]
        [KeywordParameters("saveTo", "variable name to store")]
        [SampleScript("JediOmni.Get Text (hpid-copy-homescreen-button,${value})")]
        public KeywordResult GetText(string id, string saveTo)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            id = Support.Utils.GetVariablevalueIfExist(id);
            try
            {
                string text = _device.ControlPanel.GetValue($"#{id}", "innerText", OmniPropertyType.Property);
                if (!string.IsNullOrEmpty(text))
                {
                    CommonExecutionInfo.SetVariable(saveTo, text);
                    kr.Output = $"{saveTo} : {text}";
                    return kr;
                }


                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Get text failed with given id :: {id}";
                return kr;
            }
            catch (WebInspectorException ex)
            {
                string output = $"Get text error with given id :: {id}";
                kr = SelfReconnection(ex, "GetText", output);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Get text error with given id :: {id}";
                Logger.Error($"GetText({id}, {saveTo}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Fail if checkbox/radio button is unselected (Implemented for furture purpose)(Only works for checkbox / radio button with text)")]
        [KeywordDisplayName("Is Selected")]
        [KeywordParameters("text", "Text to check")]
        [SampleScript("JediOmni.Is Selected (Sleep after inactivity)")]
        public KeywordResult IsSelected(string text)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (!_device.ControlPanel.WaitForState($"div[aria-label=\"{text}\"]",
                    OmniElementState.Exists, TimeSpan.FromSeconds(_timeOut)))
                {
                    kr.Output = $"Cannot find element with given text :: {text}";
                    kr.Result = KeywordResults.Fail;
                    kr.ScreenShot = CaptureScreen();
                }
                else if (_device.ControlPanel.WaitForState($"div[aria-label$=\"{text}<pause />Unselected\"]",
                    OmniElementState.Exists, TimeSpan.FromSeconds(_timeOut)))
                {
                    kr.Output = $"Check selected status failed with given text :: {text}";
                    kr.Result = KeywordResults.Fail;
                    kr.ScreenShot = CaptureScreen();
                }
                return kr;
            }
            catch (WebInspectorException ex)
            {
                string output = $"Check selected status error with given text :: {text}";
                kr = SelfReconnection(ex, "IsSelected", output);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Check selected status error with given text :: {text}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"IsSelected({text}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Fail if checkbox/radio button is selected (Implemented for furture purpose)(Only works for checkbox / radio button with text)")]
        [KeywordDisplayName("Is Unselected")]
        [KeywordParameters("text", "Text to check")]
        [SampleScript("JediOmni.Is Unselected (Sleep after inactivity)")]
        public KeywordResult IsUnselected(string text)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            try
            {
                if (!_device.ControlPanel.WaitForState($"div[aria-label=\"{text}\"]",
                    OmniElementState.Exists, TimeSpan.FromSeconds(_timeOut)))
                {
                    kr.Output = $"Cannot find element with given text :: {text}";
                    kr.ScreenShot = CaptureScreen();
                }
                else if (_device.ControlPanel.WaitForState($"div[aria-label$=\"{text}<pause />Unselected\"]",
                    OmniElementState.Exists, TimeSpan.FromSeconds(_timeOut)))
                {
                    kr.Result = KeywordResults.Pass;
                }
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Check selected status failed with given text :: {text}";
                Logger.Error($"IsUnselected({text}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Send Back Key to target")]
        [KeywordDisplayName("Press Back Key")]
        [SampleScript("JediOmni.Press Back Key")]
        public KeywordResult PressBackKey()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            try
            {
                if ((resolution_x == 0) || (resolution_y == 0))
                {
                    _device.ControlPanel.PressScreen(30, 30);
                }
                else
                {
                    _device.ControlPanel.PressScreen(resolution_x / 40, resolution_y / 20);
                }
                kr.Result = KeywordResults.Pass;
                return kr;
            }
            catch (WebInspectorException ex)
            {
                string output = $"Press back key error";
                kr = SelfReconnection(ex, "PressBackKey", output);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Press back key failed";
                Logger.Error("PressBackKey() fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Send Home Key to target")]
        [KeywordDisplayName("Press Home Key")]
        [SampleScript("JediOmni.Press Home Key")]
        public KeywordResult PressHomeKey()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                _device.ControlPanel.PressHome();
                return kr;
            }
            catch (WebInspectorException ex)
            {
                string output = $"Press home key error";
                kr = SelfReconnection(ex, "PressHomeKey", output);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Press home key failed";
                kr.ScreenShot = CaptureScreen();
                Logger.Error("PressHomeKey() fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Send Tab Key to target")]
        [KeywordDisplayName("Press Tab Key")]
        [SampleScript("JediOmni.Press Tab Key")]
        public KeywordResult PressTabKey()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                _device.ControlPanel.Type(SpecialCharacter.Tab);
                return kr;
            }
            catch (WebInspectorException ex)
            {
                string output = $"Press tab key error";
                kr = SelfReconnection(ex, "PressTabKey", output);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Press tab key failed";
                kr.ScreenShot = CaptureScreen();
                Logger.Error("PressTabKey() fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Send Enter Key to target")]
        [KeywordDisplayName("Press Enter Key")]
        [SampleScript("JediOmni.Press Enter Key")]
        public KeywordResult PressEnterKey()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                _device.ControlPanel.Type(SpecialCharacter.Enter);
                return kr;
            }
            catch (WebInspectorException ex)
            {
                string output = $"Press enter key error";
                kr = SelfReconnection(ex, "PressEnterKey", output);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Press Enter key failed";
                kr.ScreenShot = CaptureScreen();
                Logger.Error("PressEnterKey() fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Send Down Key to target")]
        [KeywordDisplayName("Press Down Key")]
        [SampleScript("JediOmni.Press Down Key")]
        public KeywordResult PressDownKey()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                _device.ControlPanel.Type(SpecialCharacter.DownArrow);
                return kr;
            }
            catch (WebInspectorException ex)
            {
                string output = $"Press Down key error";
                kr = SelfReconnection(ex, "PressDownKey", output);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Press Down key failed";
                kr.ScreenShot = CaptureScreen();
                Logger.Error("PressDownKey() fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Send Up Key to target")]
        [KeywordDisplayName("Press Up Key")]
        [SampleScript("JediOmni.Press Up Key")]
        public KeywordResult PressUpKey()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                _device.ControlPanel.Type(SpecialCharacter.UpArrow);
                return kr;
            }
            catch (WebInspectorException ex)
            {
                string output = $"Press Up key error";
                kr = SelfReconnection(ex, "PressUpKey", output);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Press Up key failed";
                kr.ScreenShot = CaptureScreen();
                Logger.Error("PressUpKey() fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Send Left Key to target")]
        [KeywordDisplayName("Press Left Key")]
        [SampleScript("JediOmni.Press Left Key")]
        public KeywordResult PressLeftKey()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                _device.ControlPanel.Type(SpecialCharacter.LeftArrow);
                return kr;
            }
            catch (WebInspectorException ex)
            {
                string output = $"Press Left key error";
                kr = SelfReconnection(ex, "PressLeftKey", output);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Press Left key failed";
                kr.ScreenShot = CaptureScreen();
                Logger.Error("PressLeftKey() fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Send Right Key to target")]
        [KeywordDisplayName("Press Right Key")]
        [SampleScript("JediOmni.Press Right Key")]
        public KeywordResult PressRightKey()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                _device.ControlPanel.Type(SpecialCharacter.RightArrow);
                return kr;
            }
            catch (WebInspectorException ex)
            {
                string output = $"Press Right key error";
                kr = SelfReconnection(ex, "PressRightKey", output);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Press Right key failed";
                kr.ScreenShot = CaptureScreen();
                Logger.Error("PressRightKey() fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Reset the JediOmni device")]
        [KeywordDisplayName("Reset")]
        [SampleScript("JediOmni.Reset")]
        public KeywordResult Reset()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                _deviceControl.Reset(_device);
                return kr;
            }
            catch (WebInspectorException ex)
            {
                string output = $"Reset error";
                kr = SelfReconnection(ex, "Reset", output);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Reset failed";
                Logger.Error("Reset() fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Swipe screen with given direction. Directions: Left or < : Swipe Right to Left | Right or > : Swipe Left to Right")]
        [KeywordDisplayName("Swipe")]
        [KeywordParameters("direction", "directino to swipe(<, >)")]
        [SampleScript("JediOmni.Swipe (<)")]
        public KeywordResult Swipe(string direction)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            if (resolution_x == 0 || resolution_y == 0)
            {
                kr.Output = $"Resolution is not configured in Initialize function";
                return kr;
            }
            double x1 = 0, x2 = 0, y = 0;
            x1 = resolution_x * (30 / (double)100);
            x2 = resolution_x * (60 / (double)100);
            y = resolution_y * (60 / (double)100);

            Coordinate start = new Coordinate((int)x1, (int)y);
            Coordinate end = new Coordinate((int)x2, (int)y);

            try
            {
                if (direction == "<")
                {
                    _device.ControlPanel.SwipeScreen(start, end, TimeSpan.FromMilliseconds(250));
                    kr.Result = KeywordResults.Pass;
                }
                else if (direction == ">")
                {
                    _device.ControlPanel.SwipeScreen(end, start, TimeSpan.FromMilliseconds(250));
                    kr.Result = KeywordResults.Pass;
                }
                else
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Invalid direction";
                    kr.ScreenShot = CaptureScreen();
                    return kr;
                }
                return kr;
            }
            catch (WebInspectorException ex)
            {
                string output = $"Swipe error with direction :: {direction}";
                kr = SelfReconnection(ex, "Swipe", output);
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

        [KeywordDescription("Touch Object by given Omni element id")]
        [KeywordDisplayName("Touch ID")]
        [KeywordParameters("id", "Omni element id to touch")]
        [SampleScript("JediOmni.Touch ID (hpid-message-center-exit-button)")]
        public KeywordResult TouchID(string id)
        {
            return Touch($"#{id}");
        }

        [KeywordDescription("Touch Object by given css selector")]
        [KeywordDisplayName("Touch")]
        [KeywordParameters("selector", "css selector of object to touch")]
        [SampleScript("JediOmni.Touch(.hp-header-levelup-button)")]
        public KeywordResult Touch(string selector)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            try
            {
                if (_device.ControlPanel.WaitForState(selector,
                        OmniElementState.Exists, TimeSpan.FromSeconds(_timeOut)))
                {
                    _device.ControlPanel.ScrollPress(selector);
                    kr.Result = KeywordResults.Pass;
                    return kr;
                }
                kr.Result = KeywordResults.Fail;
                kr.Output = $"Touch failed with given selector :: {selector}";
                return kr;
            }
            catch (WebInspectorException ex)
            {
                string output = $"Touch failed with given selector :: {selector}";
                kr = SelfReconnection(ex, "Touch", output);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Touch error with given selector :: {selector}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"Touch({selector}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Touch with position with percent of left and percent of top")]
        [KeywordDisplayName("Touch Position")]
        [KeywordParameters("x", "x percent value to drag with given position value")]
        [KeywordParameters("y", "y percent value to drag with given position value")]
        [SampleScript("JediOmni.Touch Position (40,49)")]
        public KeywordResult TouchPosition(string x, string y)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int X = Convert.ToInt32(x);
            int Y = Convert.ToInt32(y);
            Tuple<int, int> position = GetAbsolutePosition(x, y);

            try
            {
                _device.ControlPanel.PressScreen(position.Item1, position.Item2);
                return kr;
            }
            catch (WebInspectorException ex)
            {
                string output = $"Touch error with given value :: {X}, {Y}";
                kr = SelfReconnection(ex, "TouchPosition", output);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Touch failed with given value :: {X}, {Y}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"TouchPosition({x}, {y}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Touch object with given text")]
        [KeywordDisplayName("Touch Text")]
        [KeywordParameters("text", "Text to touch")]
        [SampleScript("JediOmni.Touch Text (Sign Out)")]
        public KeywordResult TouchText(string text)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            try
            {
                var elementSelector = $":contains(\"{text}\"):not(:has(:contains(\"{text}\"))):visible:eq(0)";

                Touch($"{elementSelector}");

                kr.Result = KeywordResults.Pass;
                return kr;
            }
            catch (WebInspectorException ex)
            {
                string output = $"Touch text error with given text :: {ex.Data} text = {text}";
                kr = SelfReconnection(ex, "TouchText", output);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Touch text failed with given text :: {ex.Data} text = {text}";
                Logger.Error($"TouchText({text}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Touch item by Text and Index, Index starts with 1")]
        [KeywordDisplayName("Touch Text With Index")]
        [KeywordParameters("text", "Text to touch")]
        [KeywordParameters("index", "index number")]
        [SampleScript("JediOmni.Touch Text With Index(Load,0)")]
        public KeywordResult TouchTextWithIndex(string text, string indexnumber)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int indexNumber = Convert.ToInt32(indexnumber);

            try
            {
                var getCountElementSelector = $":contains('{text}'):not(:has(:contains('{text}'))):visible";
                int indexCount = _device.ControlPanel.GetCount(getCountElementSelector);

                if (indexCount < indexNumber)
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Touch text with index failed with given text and index:: {text}, {indexNumber} - IndexNumber is bigger than real index";
                    kr.ScreenShot = CaptureScreen();
                    return kr;
                }

                var elementSelector = $":contains(\"{text}\"):not(:has(:contains(\"{text}\"))):visible:eq({indexNumber - 1})";

                Touch($"{elementSelector}");

                kr.Result = KeywordResults.Pass;
                return kr;
            }
            catch (WebInspectorException ex)
            {
                string output = $"Touch text with index error with given text and index:: {text}, {indexNumber}";
                kr = SelfReconnection(ex, "TouchTextWithIndex", output);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Touch text with index failed with given text and index:: {text}, {indexNumber}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"TouchTextWithIndex({text}, {indexnumber}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Wait given Object is appeared on screen until given timeout (seconds)")]
        [KeywordDisplayName("Wait For Object")]
        [KeywordParameters("selector", "Omni element id(hpid) or css selector for wait")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("JediOmni.Wait For Object (hpid-message-center-screen,10)")]
        public KeywordResult WaitForObject(string selector, string time)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int Time = Convert.ToInt32(time);
            Time = Time * _scaleFactor;
            kr.Result = KeywordResults.Pass;
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddSeconds(Time);
            TimeSpan waitingTime;

            if (selector.StartsWith("hpid") || selector.StartsWith("hp-"))
            {
                selector = "#" + selector;
            }

            try
            {
                while (!_device.ControlPanel.WaitForState(selector, OmniElementState.Useable, TimeSpan.FromMilliseconds(1)))
                {
                    if (DateTime.Now >= endTime)
                    {
                        waitingTime = DateTime.Now.Subtract(startTime);
                        kr.Result = KeywordResults.Fail;
                        kr.Output = $"Wait for object failed with given selector and timeout:: {selector}, {waitingTime} sec";
                        kr.ScreenShot = CaptureScreen();
                        return kr;
                    }
                    _device.ControlPanel.SignalUserActivity();
                    Thread.Sleep(TimeSpan.FromMilliseconds(500));
                }
                waitingTime = DateTime.Now.Subtract(startTime);
                Logger.Debug($"Waiting time for object: {waitingTime} sec");
                kr.Output = $"Waiting time: {waitingTime}";
                return kr;
            }
            catch (WebInspectorException ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                string output = $"Wait for object error with given selector and timeout:: {selector}, {waitingTime}";
                kr = SelfReconnection(ex, "WaitForObject", output);
                return kr;
            }
            catch (Exception ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Wait for object error with given selector and timeout:: {selector}, {waitingTime}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"WaitForObject({selector}, {time}) error", ex);
                return kr;
            }
        }

        [KeywordDescription("Wait given text is appeared on screen until given timeout (seconds)")]
        [KeywordDisplayName("Wait For Text")]
        [KeywordParameters("text", "Wait to appeared specific text")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("JediOmni.Wait For Text (Sign Out,30)")]
        public KeywordResult WaitForText(string text, string time)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            kr.Result = KeywordResults.Pass;
            int Time = Convert.ToInt32(time);
            Time = Time * _scaleFactor;
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddSeconds(Time);
            TimeSpan waitingTime;

            try
            {
                var elementSelector = $":contains(\"{text}\"):not(:has(:contains(\"{text}\"))):visible:eq(0)";

                while (!_device.ControlPanel.CheckState(elementSelector, OmniElementState.Useable))
                {
                    if (DateTime.Now > endTime)
                    {
                        waitingTime = DateTime.Now.Subtract(startTime);
                        kr.Result = KeywordResults.Fail;
                        kr.Output = $"Wait for text failed with given text and timeout:: {text}, {waitingTime}";
                        kr.ScreenShot = CaptureScreen();
                        return kr;
                    }
                    _device.ControlPanel.SignalUserActivity();
                    Thread.Sleep(TimeSpan.FromMilliseconds(500));
                }
                waitingTime = DateTime.Now.Subtract(startTime);
                Logger.Debug($"Waiting time for text: {waitingTime} sec");
                kr.Output = $"Waiting time: {waitingTime}";
                return kr;
            }
            catch (WebInspectorException ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                string output = $"Exception : Wait for text error with given text and timeout:: {text}, {waitingTime}";
                kr = SelfReconnection(ex, "WaitForText", output);
                return kr;
            }
            catch (Exception ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Exception : Wait for text error with given text and timeout:: {text}, {waitingTime}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"WaitForText({text}, {time}) error", ex);
                return kr;
            }
        }

        [KeywordDescription("Wait given object is disappeared on screen until given timeout (seconds)")]
        [KeywordDisplayName("Wait For Object Gone")]
        [KeywordParameters("selector", "Omni element id(hpid) or css selector for wait")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("JediOmni.Wait For Object Gone (hpid-cloud-status-button,10)")]
        public KeywordResult WaitForObjectGone(string selector, string time)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int Time = Convert.ToInt32(time);
            Time = Time * _scaleFactor;
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddSeconds(Time);
            TimeSpan waitingTime;

            if (selector.StartsWith("hpid") || selector.StartsWith("hp-"))
            {
                selector = "#" + selector;
            }

            try
            {
                while (_device.ControlPanel.WaitForState(selector, OmniElementState.Useable, TimeSpan.FromMilliseconds(1)))
                {
                    if (DateTime.Now > endTime)
                    {
                        waitingTime = DateTime.Now.Subtract(startTime);
                        kr.Result = KeywordResults.Fail;
                        kr.Output = $"Wait for object gone failed with given selector and timeout:: {selector}, {waitingTime}";
                        kr.ScreenShot = CaptureScreen();
                        return kr;
                    }
                    _device.ControlPanel.SignalUserActivity();
                    Thread.Sleep(500);
                }

                waitingTime = DateTime.Now.Subtract(startTime);
                Logger.Debug($"Waiting time for object gone: {waitingTime} sec");
                kr.Output = $"Waiting time: {waitingTime}";
                return kr;
            }
            catch (WebInspectorException ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                string output = $"Wait for object gone error with given selector and timeout:: {selector}, {waitingTime}";
                kr = SelfReconnection(ex, "WaitForObjectGone", output);
                return kr;
            }
            catch (Exception ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Wait for object gone error with given selector and timeout:: {selector}, {waitingTime}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"WaitForObjectGone({selector}, {time}) error", ex);
                return kr;
            }
        }

        [KeywordDescription("Wait given text is disappeared on screen until given timeout (seconds)")]
        [KeywordDisplayName("Wait For Text Gone")]
        [KeywordParameters("text", "Wait to disappeared specific text")]
        [KeywordParameters("time", "Time to wait")]
        [SampleScript("JediOmni.Wait For Text Gone (Connecting to the HP Cloud,60)")]
        public KeywordResult WaitForTextGone(string text, string time)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int Time = Convert.ToInt32(time);
            Time = Time * _scaleFactor;
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddSeconds(Time);
            TimeSpan waitingTime;

            try
            {
                var elementSelector = $":contains(\"{text}\"):not(:has(:contains(\"{text}\"))):visible:eq(0)";

                while (_device.ControlPanel.CheckState(elementSelector, OmniElementState.Useable))
                {
                    if (DateTime.Now > endTime)
                    {
                        waitingTime = DateTime.Now.Subtract(startTime);
                        kr.Result = KeywordResults.Fail;
                        kr.Output = $"Wait for text gone failed with given text and timeout:: {text}, {waitingTime}";
                        kr.ScreenShot = CaptureScreen();
                        return kr;
                    }
                    _device.ControlPanel.SignalUserActivity();
                    Thread.Sleep(TimeSpan.FromMilliseconds(500));
                }
                waitingTime = DateTime.Now.Subtract(startTime);
                Logger.Debug($"Waiting time for text gone: {waitingTime} sec");
                kr.Output = $"Waiting time: {waitingTime}";
                return kr;
            }
            catch (WebInspectorException ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                string output = $"Wait for text gone error with given text and timeout:: {text}, {waitingTime}";
                kr = SelfReconnection(ex, "WaitForTextGone", output);
                return kr;
            }
            catch (Exception ex)
            {
                waitingTime = DateTime.Now - startTime;
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Wait for text gone failed with given text and timeout:: {text}, {waitingTime}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"WaitForTextGone({text}, {time}) error", ex);
                return kr;
            }
        }

        [KeywordDescription("Wait active job is appeared on screen until given timeout (seconds)")]
        [KeywordDisplayName("Wait For Active Job")]
        [KeywordParameters("time", "Time to wait")]
        [SampleScript("JediOmni.Wait For Active Job (5)")]
        public KeywordResult WaitForActiveJob(string time)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int Time = Convert.ToInt32(time);
            Time = Time * _scaleFactor;
            DateTime startTime = DateTime.Now;
            TimeSpan waitingTime;

            try
            {
                if (Wait.ForEquals(ActiveJobsButtonVisible, true, TimeSpan.FromSeconds(Time), TimeSpan.FromMilliseconds(250)))
                {
                    waitingTime = DateTime.Now.Subtract(startTime);
                    kr.Result = KeywordResults.Pass;
                    Logger.Debug($"Waiting time for active job: {waitingTime} sec");
                    kr.Output = $"Waiting time for active job:: {waitingTime}";
                    return kr;
                }
                else
                {
                    waitingTime = DateTime.Now.Subtract(startTime);
                    kr.Result = KeywordResults.Fail;
                    Logger.Error($"WaitForActiveJob({time}) fail");
                    kr.Output = $"Wait time for active job failed with given timeout:: {waitingTime} sec";
                    kr.ScreenShot = CaptureScreen();
                    return kr;
                }
            }
            catch (WebInspectorException ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                string output = $"Wait time for active job error with given timeout:: {waitingTime} sec";
                kr = SelfReconnection(ex, "WaitForActiveJob", output);
                return kr;
            }
            catch (Exception ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                Logger.Error($"WaitForActiveJob({time}) error", ex);
                kr.Output = $"Wait time for active job error with given timeout:: {waitingTime} sec";
                kr.ScreenShot = CaptureScreen();
                return kr;
            }
        }

        [KeywordDescription("Wait active job is disappeared on screen until given timeout (seconds)")]
        [KeywordDisplayName("Wait For Active Job Gone")]
        [KeywordParameters("time", "Time to wait")]
        [SampleScript("JediOmni.Wait For Active Job Gone (100)")]
        public KeywordResult WaitForActiveJobGone(string time)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int Time = Convert.ToInt32(time);
            Time = Time * _scaleFactor;
            DateTime startTime = DateTime.Now;
            TimeSpan waitingTime;

            try
            {
                if (Wait.ForEquals(ActiveJobsButtonVisible, false, TimeSpan.FromSeconds(Time), TimeSpan.FromMilliseconds(250)))
                {
                    waitingTime = DateTime.Now.Subtract(startTime);
                    kr.Result = KeywordResults.Pass;
                    Logger.Debug($"Waiting time for active job gone: {waitingTime} sec");
                    kr.Output = $"Waiting time for active job gone:: {waitingTime}";
                    return kr;
                }
                else
                {
                    waitingTime = DateTime.Now.Subtract(startTime);
                    kr.Result = KeywordResults.Fail;
                    Logger.Error($"WaitForActiveJobGone({time}) fail");
                    kr.Output = $"Wait time for active job failed with given timeout:: {waitingTime} sec";
                    kr.ScreenShot = CaptureScreen();
                    return kr;
                }
            }
            catch (WebInspectorException ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                string output = $"Wait time for active job error with given timeout gone:: {waitingTime} sec";
                kr = SelfReconnection(ex, "WaitForActiveJobGone", output);
                return kr;
            }
            catch (Exception ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                Logger.Error($"WaitForActiveJobGone({time}) error", ex);
                kr.Output = $"Wait time for active job error with given timeout gone:: {waitingTime} sec";
                kr.ScreenShot = CaptureScreen();
                return kr;
            }
        }

        [KeywordDescription("Enable Paperless mode to device")]
        [KeywordDisplayName("Enable Paperless Mode")]
        [SampleScript("JediOmni.Enable Paperless Mode")]
        public KeywordResult EnablePaperLessMode()
        {
            AllowPJLAnyway();
            string paperlessPjl = string.Join(_crlf, _uel, "@PJL RDYMSG DISPLAY=\"Paperless Mode\"", "@PJL SET SERVICEMODE=HPBOISEID", "@PJL SET JOBMEDIA=OFF", "@PJL DEFAULT JOBMEDIA=OFF", _uel);
            Logger.Trace("Paperless Enable PJL");
            Logger.Trace(paperlessPjl);
            return MFPUtils.IpSend(_dut, paperlessPjl);
        }

        [KeywordDescription("Disable Paperless mode to device")]
        [KeywordDisplayName("Disable Paperless Mode")]
        [SampleScript("JediOmni.Disable Paperless Mode")]
        public KeywordResult DisablePaperLessMode()
        {
            AllowPJLAnyway();
            string paperlessPjl = string.Join(_crlf, _uel, "@PJL RDYMSG DISPLAY=\"\"", "@PJL SET SERVICEMODE=HPBOISEID", "@PJL SET JOBMEDIA=ON", "@PJL DEFAULT JOBMEDIA=ON", _uel);
            Logger.Trace("Paperless Disable PJL");
            Logger.Trace(paperlessPjl);
            return MFPUtils.IpSend(_dut, paperlessPjl);
        }

        [KeywordDescription("Enter Access Code at Login screen")]
        [KeywordDisplayName("Enter Access Code")]
        [KeywordParameters("accessCode", "Access Code to enter")]
        [SampleScript("JediOmni.Enter Access Code (12345678)")]
        public KeywordResult EnterAccessCode(string accessCode)
        {
            try
            {
                if (_device.ControlPanel.CheckState(".hp-textbox:last", OmniElementState.Useable))
                {
                    _device.ControlPanel.Press(".hp-textbox:last");
                }
                _device.ControlPanel.TypeOnVirtualKeyboard(accessCode);
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                KeywordResult kr = new KeywordResult(KeywordResults.Fail);
                kr.AdditionalInfo = ex.ToString();
                kr.Output = "Can not enter Access code";
                kr.ScreenShot = CaptureScreen();
                return kr;

            }
        }

        [KeywordDescription("Reboot the device. Check with Wait Until Boot. You should reconnect all libraries (such as JediOmni and Android) to use properly after reboot")]
        [KeywordDisplayName("Reboot")]
        [SampleScript("JediOmni.Reboot")]
        public KeywordResult Reboot()
        {
            try
            {
                //_device.PowerManagement.Reboot();
                _device.Snmp.Set(new SnmpOidValue("1.3.6.1.2.1.43.5.1.1.3.1", 4));

                DateTime checkTime = DateTime.Now.AddSeconds(30);

                while (!IsDeviceTurnedOff())
                {
                    if (DateTime.Now > checkTime)
                    {
                        KeywordResult kr = new KeywordResult(KeywordResults.Fail);
                        kr.Output = "Device is not turned off after 30 seconds";
                        kr.ScreenShot = CaptureScreen();
                        return kr;
                    }
                    Thread.Sleep(1000);
                }

                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                KeywordResult kr = new KeywordResult(KeywordResults.Error);
                kr.AdditionalInfo = ex.ToString();
                kr.Output = "Error during reboot";
                return kr;

            }
        }

        [KeywordDescription("Wait until device is booted and ready to use")]
        [KeywordDisplayName("Wait Until Boot")]
        [SampleScript("JediOmni.Wait Until Boot")]
        public KeywordResult WaitUntilBoot()
        {
            try
            {
                DateTime checkTime = DateTime.Now.AddMinutes(10);

                while (!IsDeviceTurnedOn())
                {
                    if (DateTime.Now > checkTime)
                    {
                        KeywordResult kr = new KeywordResult(KeywordResults.Fail);
                        kr.Output = "Device is not turned on in 10 minutes";
                        kr.ScreenShot = CaptureScreen();
                        return kr;
                    }
                    Thread.Sleep(1000);
                }

                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                KeywordResult kr = new KeywordResult(KeywordResults.Error);
                kr.AdditionalInfo = ex.ToString();
                kr.Output = "Error during reboot";
                return kr;
            }
        }

        [KeywordDescription("Re connect to the device. Call this keyword after Reboot")]
        [KeywordDisplayName("Re Connect")]
        [SampleScript("JediOmni.Re Connect")]
        public KeywordResult ReConnect()
        {
            try
            {
                Dispose();
                Thread.Sleep(5000);
                Initialize(_dut, _outputDir);
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

        [KeywordDescription("Wait until target device goes to power save mode (shallow suspend)\r\nThis keyword DOES NOT affected by scale factor")]
        [KeywordDisplayName("Wait Until Power Save")]
        [KeywordParameters("timeout", "time out to wait (in seconds)")]
        [SampleScript("JediOmni.Wait Until Power Save (10)")]
        public KeywordResult WaitUntilPowerSave(string timeOut)
        {
            if (!int.TryParse(timeOut, out int timeoutInt))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"Timeout must be a number. :: Timeout - {timeOut}";
                return error;
            }
            DateTime endTime = DateTime.Now.AddSeconds(timeoutInt);
            PowerState currentState = _device.PowerManagement.GetPowerState();
            while (endTime > DateTime.Now)
            {
                if (currentState.Equals(PowerState.PowerSave))
                {
                    KeywordResult pass = new KeywordResult(KeywordResults.Pass);
                    pass.Output = $"Current power status : {currentState.ToString()}";
                    return pass;
                }
                Thread.Sleep(500);
                currentState = _device.PowerManagement.GetPowerState();
            }
            currentState = _device.PowerManagement.GetPowerState();
            KeywordResult fail = new KeywordResult(KeywordResults.Fail);
            fail.Output = $"Device is not goes to the power save or sleep within timeout. Current state is {currentState.ToString()}";
            return fail;
        }

        [KeywordDescription("Wait until target device goes to sleep mode (suspend or A1W)\r\nThis keyword DOES NOT affected by scale factor")]
        [KeywordDisplayName("Wait Until Sleep")]
        [KeywordParameters("timeout", "time out to wait (in seconds)")]
        [SampleScript("JediOmni.Wait Until Sleep (8)")]
        public KeywordResult WaitUntilSleep(string timeOut)
        {
            if (!int.TryParse(timeOut, out int timeoutInt))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"Timeout must be a number. :: Timeout - {timeOut}";
                return error;
            }
            DateTime endTime = DateTime.Now.AddSeconds(timeoutInt);
            PowerState currentState = _device.PowerManagement.GetPowerState();
            while (endTime > DateTime.Now)
            {
                if (currentState.Equals(PowerState.Sleep))
                {
                    KeywordResult pass = new KeywordResult(KeywordResults.Pass);
                    pass.Output = $"Current power status : {currentState.ToString()}";
                    return pass;
                }
                Thread.Sleep(500);
                currentState = _device.PowerManagement.GetPowerState();
            }
            currentState = _device.PowerManagement.GetPowerState();
            KeywordResult fail = new KeywordResult(KeywordResults.Fail);
            fail.Output = $"Device is not goes to the power save or sleep within timeout. Current state is {currentState.ToString()}";
            return fail;
        }

        [KeywordDescription("Check if device is now in power save mode (shallow suspend)")]
        [KeywordDisplayName("Is Power Save")]
        [SampleScript("JediOmni.Is Power Save")]
        public KeywordResult IsPowerSave()
        {
            PowerState currentState = _device.PowerManagement.GetPowerState();
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

        [KeywordDescription("Check if device is now in sleep mode (suspend or A1W)")]
        [KeywordDisplayName("Is Sleep")]
        [SampleScript("JediOmni.Is Sleep")]
        public KeywordResult IsSleep()
        {
            PowerState currentState = _device.PowerManagement.GetPowerState();
            if (currentState.Equals(PowerState.Sleep))
            {
                KeywordResult pass = new KeywordResult(KeywordResults.Pass);
                pass.Output = $"Current power status : {currentState.ToString()}";
                return pass;
            }
            KeywordResult fail = new KeywordResult(KeywordResults.Fail);
            fail.Output = $"Current state is {currentState.ToString()}";
            return fail;
        }

        [KeywordDescription("Check if device is now awake")]
        [KeywordDisplayName("Is Awake")]
        [SampleScript("JediOmni.Is Awake")]
        public KeywordResult IsAwake()
        {
            PowerState currentState = _device.PowerManagement.GetPowerState();
            if (currentState.Equals(PowerState.Awake))
            {
                KeywordResult pass = new KeywordResult(KeywordResults.Pass);
                pass.Output = $"Current power status : {currentState.ToString()}";
                return pass;
            }
            KeywordResult fail = new KeywordResult(KeywordResults.Fail);
            fail.Output = $"Current state is {currentState.ToString()}";
            return fail;
        }

        [KeywordDescription("Initialize Memory monitoring for JediOmni Device")]
        [KeywordDisplayName("Start Memory Monitoring")]
        [SampleScript("JediOmni.Start Memory Monitoring")]
        public KeywordResult StartMemoryMonitoring()
        {
            string csvPath;
            _testCaseName = CommonExecutionInfo.GetVariable("_tcName");

            if (CommonExecutionInfo.CurrentRepeatCount > 0)
            {
                csvPath = Path.Combine(Directory.GetParent(_outputDir).FullName, $"JediOmni_Memory_Usage.csv");
                Logger.Trace($"Current Repeat Count is greater than 0. CSV Path is : {csvPath}");
            }
            else
            {
                csvPath = Path.Combine(_outputDir, $"{_testCaseName}_JediOmniMemoryMonitoring_{DateTime.Now.ToString("yyMMdd_HHmmssff")}.csv");
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
        [SampleScript("JediOmni.Collect Memory Info (SingIn / SingOut)")]
        public KeywordResult CollectMemoryInfo(string checkPoint)
        {
            try
            {
                var memoryXml = _device.Diagnostics.GetMemoryCounters();
                KeywordEvent.Trigger("JediOmniMemoryCollection", checkPoint);
                Dictionary<string, long> memInfo = JediMemoryCollection.ProcessMemoryData(memoryXml);

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

        [KeywordDescription("Draw JediOmni Device Memory Usage Graph")]
        [KeywordDisplayName("Draw Memory Usage Graph")]
        [SampleScript("JediOmni.Draw Memory Usage Graph")]
        public KeywordResult DrawMemoryUsageGraph()
        {
            try
            {
                ChartCreator chart = new ChartCreator(_memoryUsage.GetCSVPath());
                string chartImage = Path.Combine(_outputDir, $"{_testCaseName}_JediOmniMemory.png");

                chart.SaveImage(chartImage, System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line, true, null);

                if (CommonExecutionInfo.CurrentRepeatCount > 0)
                {
                    File.Copy(chartImage, Path.Combine(Directory.GetParent(_outputDir).FullName, $"{_testCaseName}_JediOmni_Memory_Usage.png"), true);
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


        [KeywordDescription("Wait given Object is enabled on screen until given timeout (seconds)")]
        [KeywordDisplayName("Wait For Enable of Object")]
        [KeywordParameters("selector", "Omni element id(hpid) or css selector for wait")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("JediOmni.Wait For Enable of Object (hpid-button-homescreen-start,100)")]
        public KeywordResult WaitForEnableOfObject(string selector, string time)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int Time = Convert.ToInt32(time);
            Time = Time * _scaleFactor;
            kr.Result = KeywordResults.Pass;
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddSeconds(Time);
            TimeSpan waitingTime;

            if (selector.StartsWith("hpid") || selector.StartsWith("hp-"))
            {
                selector = "#" + selector;
            }

            try
            {
                while (!_device.ControlPanel.WaitForState(selector, OmniElementState.Enabled, TimeSpan.FromMilliseconds(1)))
                {
                    if (DateTime.Now >= endTime)
                    {
                        waitingTime = DateTime.Now.Subtract(startTime);
                        kr.Result = KeywordResults.Fail;
                        kr.Output = $"Wait for Enable of Object failed with given selector and timeout:: {selector}, {waitingTime} sec";
                        kr.ScreenShot = CaptureScreen();
                        return kr;
                    }
                    _device.ControlPanel.SignalUserActivity();
                    Thread.Sleep(TimeSpan.FromMilliseconds(500));
                }
                waitingTime = DateTime.Now.Subtract(startTime);
                Logger.Debug($"Waiting time for Enable of Object: {waitingTime} sec");
                kr.Output = $"Waiting time: {waitingTime}";
                return kr;
            }
            catch (WebInspectorException ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                string output = $"Wait for Enable of Object error with given selector and timeout:: {selector}, {waitingTime}";
                kr = SelfReconnection(ex, "WaitForEnableofObject", output);
                return kr;
            }
            catch (Exception ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Wait for Enable of Object error with given selector and timeout:: {selector}, {waitingTime}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"WaitForEnableofObject({selector}, {time}) error", ex);
                return kr;
            }
        }


        [KeywordDescription("Wait given Object is disabled on screen until given timeout (seconds)")]
        [KeywordDisplayName("Wait For Disable of Object")]
        [KeywordParameters("selector", "Omni element id(hpid) or css selector for wait")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("JediOmni.Wait For Disable of Object (hpid-button-homescreen-start,100)")]
        public KeywordResult WaitForDisableOfObject(string selector, string time)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int Time = Convert.ToInt32(time);
            Time = Time * _scaleFactor;
            kr.Result = KeywordResults.Pass;
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddSeconds(Time);
            TimeSpan waitingTime;

            if (selector.StartsWith("hpid") || selector.StartsWith("hp-"))
            {
                selector = "#" + selector;
            }

            try
            {
                while (!_device.ControlPanel.WaitForState(selector, OmniElementState.Disabled, TimeSpan.FromMilliseconds(1)))
                {
                    if (DateTime.Now >= endTime)
                    {
                        waitingTime = DateTime.Now.Subtract(startTime);
                        kr.Result = KeywordResults.Fail;
                        kr.Output = $"Wait for disable of Object failed with given selector and timeout:: {selector}, {waitingTime} sec";
                        kr.ScreenShot = CaptureScreen();
                        return kr;
                    }
                    _device.ControlPanel.SignalUserActivity();
                    Thread.Sleep(TimeSpan.FromMilliseconds(500));
                }
                waitingTime = DateTime.Now.Subtract(startTime);
                Logger.Debug($"Waiting time for Disable of Object: {waitingTime} sec");
                kr.Output = $"Waiting time: {waitingTime}";
                return kr;
            }
            catch (WebInspectorException ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                string output = $"Wait for Disable of Object error with given selector and timeout:: {selector}, {waitingTime}";
                kr = SelfReconnection(ex, "WaitForDisableOfObject", output);
                return kr;
            }
            catch (Exception ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Wait for disable of Object error with given selector and timeout:: {selector}, {waitingTime}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"WaitForDisableOfObject({selector}, {time}) error", ex);
                return kr;
            }
        }

    }
}
using HP.DeviceAutomation;
using HP.DeviceAutomation.Sirius;
using HP.GFriend.GFLogger;
using HP.GFriend.Support;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using Logger = HP.GFriend.GFLogger.Logger;

namespace HP.GFriend.Keywords
{
    [LibraryDescription("For enabling test service on NAREL(production) FW, follow step below\r\n" +
        "1. At Home screen go to hidden 'Support' menu by pressing back button 4 times\r\n" +
        "2. Go to Reports Menu -> Print-mech button tap\r\n" +
        "3. Use the arrow buttons to set code=12\r\n" +
        "4. Press OK. An internal page will print out. At the upper left corner of the page, there is the Test Service ID\r\n" +
        "5. Create a file called TestServiceID.txt at the root of the USB drive. The file should only consist of one line with the test service ID as shown on the internal page.\r\n" +
        "6. Insert the USB drive into the device. When the device detects the USB drive with the TestServiceId.txt file containing the secure key, the Test Service will be enabled. A power-cycle is not required for this to happen.")]
    public class Sirius : IGFLibrary
    {
        /// <summary>
        /// Control the SiriusUIv3,v2 Device
        /// </summary>
        private DeviceUnderTest _dut;
        private IDevice _device;
        //private SiriusUIv3Device _device;
        //private DeviceControl _deviceControl;
        private static string _outputDir;
        private bool _logAttached = false;

        private EventHandler<HP.DeviceAutomation.LogEventArgs> logTrace;
        private EventHandler<HP.DeviceAutomation.LogEventArgs> logDebug;
        private EventHandler<HP.DeviceAutomation.LogEventArgs> logWarn;
        private EventHandler<HP.DeviceAutomation.LogEventArgs> logError;

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
                CreateSiriusDevice(dut);
            }
            catch (Exception ex)
            {
                Logger.Error("Connection error", ex);
            }
        }

        private void CreateSiriusDevice(DeviceUnderTest dut)
        {
            if (!string.IsNullOrEmpty(dut.LanDebugAddress))
            {
                var parameters = new DeviceConstructionParameterCollection(
                    new DeviceAddressParameter(dut.DeviceAddress),
                    new DeviceAdminPasswordParameter(dut.AdminPassword)
                    //new SiriusDebugAddressParameter(dut.LanDebugAddress)
                    );

                _device = DeviceFactory.Create(parameters);
            }
            else
            {
                _device = DeviceFactory.Create(dut.DeviceAddress, dut.AdminPassword);
            }
        }

        /// <summary>
        /// Initialization to connect Sirius UI V3
        /// </summary>
        public string GetName()
        {
            return "Sirius";
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
        /// Dispose fuction to connect Sirius UI V3
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
                _device.Dispose();
                _device = null;
                CommonExecutionInfo.RemoveSharedObject("Sirius:" + _dut.DeviceId);
            }
        }

        /// <summary>
        /// Capture Sirius Screen and Convert to byte
        /// </summary>
        private byte[] CaptureScreen()
        {
            Image ControlPanelImage = null;

            if (_device is SiriusUIv3Device)
            {
                ControlPanelImage = ((SiriusUIv3Device)_device).ControlPanel.ScreenCapture();
            }
            else if (_device is SiriusUIv2Device)
            {
                ControlPanelImage = ((SiriusUIv2Device)_device).ControlPanel.ScreenCapture();
            }
                
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

        [KeywordDescription("Capture Screen Shot of device and save to PC (UIv2 , UIv3)")]
        [KeywordDisplayName("Capture Screen Shot")]
        [KeywordParameters("filename", "filename to save")]
        [SampleScript("Sirius.Capture Screen Shot (${Sample})")]
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

        [GetKeyword]
        [KeywordDescription("Get the label of the active (top-most) screen on the display. (UIv2 , UIv3)")]
        [KeywordDisplayName("Get Active Screen Label")]
        [KeywordParameters("saveTo", "variable name to store")]
        [SampleScript("will add in next release")]
        public KeywordResult GetActiveScreenLabel(string saveTo)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                string text = null;
                if (_device is SiriusUIv3Device)
                {
                    text = ((SiriusUIv3Device)_device).ControlPanel.ActiveScreenLabel();
                }
                else if (_device is SiriusUIv2Device)
                {
                    text = ((SiriusUIv2Device)_device).ControlPanel.ActiveScreenLabel();
                }

                if (!string.IsNullOrEmpty(text))
                {
                    CommonExecutionInfo.SetVariable(saveTo, text);
                    kr.Output = $"{saveTo} : {text}";
                    return kr;
                }

                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Get active screen label failed.";
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Get active screen label error.";
                Logger.Error($"GetActiveScreenLabel({saveTo}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Perform the specified action on the widget with the specified ID. (UIv2 , UIv3)")]
        [KeywordDisplayName("Perform Select Action")]
        [KeywordParameters("widgetId", "The widget ID")]
        [SampleScript("Sirius.Perform Select Action (group.group.print)")]
        public KeywordResult PerformSelectAction(string widgetId)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.PerformAction(WidgetAction.Select, widgetId);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.PerformAction(WidgetAction.Select, widgetId);
                }

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Perform Select Action failed with given widgetId :: {ex.Data} widgetId = {widgetId}";
                Logger.Error($"PerformSelectAction({widgetId}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Perform the specified action on the widget with the specified ID. (UIv2 , UIv3)")]
        [KeywordDisplayName("Perform ScrollNext Action")]
        [KeywordParameters("widgetId", "The widget ID")]
        [SampleScript("Sirius.Perform ScrollNext Action (spin)")]
        public KeywordResult PerformScrollNextAction(string widgetId)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.PerformAction(WidgetAction.ScrollNext, widgetId);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.PerformAction(WidgetAction.ScrollNext, widgetId);
                }

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Perform ScrollNext Action failed with given widgetId :: {ex.Data} widgetId = {widgetId}";
                Logger.Error($"PerformScrollNextAction({widgetId}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Perform the specified action on the widget with the specified ID. (UIv2 , UIv3)")]
        [KeywordDisplayName("Perform ScrollPrev Action")]
        [KeywordParameters("widgetId", "The widget ID")]
        [SampleScript("Sirius.Perform ScrollPrev Action (spin)")]
        public KeywordResult PerformScrollPrevAction(string widgetId)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.PerformAction(WidgetAction.ScrollPrev, widgetId);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.PerformAction(WidgetAction.ScrollPrev, widgetId);
                }

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Perform ScrollPrev Action failed with given widgetId :: {ex.Data} widgetId = {widgetId}";
                Logger.Error($"PerformScrollPrevAction({widgetId}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Perform the specified action on the widget with the specified ID. (UIv2 , UIv3)")]
        [KeywordDisplayName("Perform Increment Action")]
        [KeywordParameters("widgetId", "The widget ID")]
        [SampleScript("Sirius.Perform Increment Action (spin)")]
        public KeywordResult PerformIncrementAction(string widgetId)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.PerformAction(WidgetAction.Increment, widgetId);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.PerformAction(WidgetAction.Increment, widgetId);
                }

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Perform Increment Action failed with given widgetId :: {ex.Data} widgetId = {widgetId}";
                Logger.Error($"PerformIncrementAction({widgetId}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Perform the specified action on the widget with the specified ID. (UIv2 , UIv3)")]
        [KeywordDisplayName("Perform Decrement Action")]
        [KeywordParameters("widgetId", "The widget ID")]
        [SampleScript("Sirius.Perform Decrement Action (spin)")]
        public KeywordResult PerformDecrementAction(string widgetId)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.PerformAction(WidgetAction.Decrement, widgetId);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.PerformAction(WidgetAction.Decrement, widgetId);
                }

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Perform Decrement Action failed with given widgetId :: {ex.Data} widgetId = {widgetId}";
                Logger.Error($"PerformDecrementAction({widgetId}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Perform the specified action on the widget with the specified ID. (UIv2 , UIv3)")]
        [KeywordDisplayName("Perform SlideForward Action")]
        [KeywordParameters("widgetId", "The widget ID")]
        [SampleScript("Sirius.Perform SlideForward Action (command.MenuStructure)")]
        public KeywordResult PerformSlideForwardAction(string widgetId)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.PerformAction(WidgetAction.SlideForward, widgetId);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.PerformAction(WidgetAction.SlideForward, widgetId);
                }

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Perform SlideForward Action failed with given widgetId :: {ex.Data} widgetId = {widgetId}";
                Logger.Error($"PerformSlideForwardAction({widgetId}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Perform the specified action on the widget with the specified ID. (UIv2 , UIv3)")]
        [KeywordDisplayName("Perform SlideBackward Action")]
        [KeywordParameters("widgetId", "The widget ID")]
        [SampleScript("Sirius.Perform SlideBackward Action (command.MenuStructure)")]
        public KeywordResult PerformSlideBackwardAction(string widgetId)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.PerformAction(WidgetAction.SlideBackward, widgetId);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.PerformAction(WidgetAction.SlideBackward, widgetId);
                }

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Perform SlideBackward Action failed with given widgetId :: {ex.Data} widgetId = {widgetId}";
                Logger.Error($"PerformSlideBackwardAction({widgetId}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Perform the specified action on the widget with the specified ID. (UIv2 , UIv3)")]
        [KeywordDisplayName("Perform SetValue Action")]
        [KeywordParameters("widgetId", "The widget ID")]
        [SampleScript("Sirius.Perform SetValue Action (spin)")]
        public KeywordResult PerformSetValueAction(string widgetId)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.PerformAction(WidgetAction.SetValue, widgetId);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.PerformAction(WidgetAction.SetValue, widgetId);
                }

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Perform SetValue Action failed with given widgetId :: {ex.Data} widgetId = {widgetId}";
                Logger.Error($"PerformSetValueAction({widgetId}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Perform the specified action on the widget with the specified ID. (UIv2 , UIv3)")]
        [KeywordDisplayName("Perform Check Action")]
        [KeywordParameters("widgetId", "The widget ID")]
        [SampleScript("Sirius.Perform Check Action (model.1)")]
        public KeywordResult PerformCheckAction(string widgetId)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.PerformAction(WidgetAction.Check, widgetId);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.PerformAction(WidgetAction.Check, widgetId);
                }

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Perform Check Action failed with given widgetId :: {ex.Data} widgetId = {widgetId}";
                Logger.Error($"PerformCheckAction({widgetId}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Perform the specified action on the widget with the specified ID. (UIv2 , UIv3)")]
        [KeywordDisplayName("Perform Toggle Action")]
        [KeywordParameters("widgetId", "The widget ID")]
        [SampleScript("Sirius.Perform Toggle Action (command.Hour)")]
        public KeywordResult PerformToggleAction(string widgetId)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.PerformAction(WidgetAction.Toggle, widgetId);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.PerformAction(WidgetAction.Toggle, widgetId);
                }

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Perform Toggle Action failed with given widgetId :: {ex.Data} widgetId = {widgetId}";
                Logger.Error($"PerformToggleAction({widgetId}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Perform the specified action on the widget with the specified ID. (UIv2 , UIv3)")]
        [KeywordDisplayName("Perform Play Action")]
        [KeywordParameters("widgetId", "The widget ID")]
        [SampleScript("will add in next release")]
        public KeywordResult PerformPlayAction(string widgetId)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.PerformAction(WidgetAction.Play, widgetId);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.PerformAction(WidgetAction.Play, widgetId);
                }

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Perform Play Action failed with given widgetId :: {ex.Data} widgetId = {widgetId}";
                Logger.Error($"PerformPlayAction({widgetId}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Perform the specified action on the widget with the specified ID. (UIv2 , UIv3)")]
        [KeywordDisplayName("Perform Pause Action")]
        [KeywordParameters("widgetId", "The widget ID")]
        [SampleScript("will add in next release")]
        public KeywordResult PerformPauseAction(string widgetId)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.PerformAction(WidgetAction.Pause, widgetId);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.PerformAction(WidgetAction.Pause, widgetId);
                }

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Perform Pause Action failed with given widgetId :: {ex.Data} widgetId = {widgetId}";
                Logger.Error($"PerformPauseAction({widgetId}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Perform the specified action on the widget with the specified ID. (UIv2 , UIv3)")]
        [KeywordDisplayName("Perform Stop Action")]
        [KeywordParameters("widgetId", "The widget ID")]
        [SampleScript("will add in next release")]
        public KeywordResult PerformStopAction(string widgetId)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.PerformAction(WidgetAction.Stop, widgetId);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.PerformAction(WidgetAction.Stop, widgetId);
                }

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Perform Stop Action failed with given widgetId :: {ex.Data} widgetId = {widgetId}";
                Logger.Error($"PerformStopAction({widgetId}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Perform the specified action on the widget with the specified ID. (UIv2 , UIv3)")]
        [KeywordDisplayName("Perform Tap Action")]
        [KeywordParameters("widgetId", "The widget ID")]
        [SampleScript("Sirius.Perform Tap Action (group.group.fax)")]
        public KeywordResult PerformTapAction(string widgetId)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.PerformAction(WidgetAction.Tap, widgetId);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.PerformAction(WidgetAction.Tap, widgetId);
                }

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Perform Tap Action failed with given widgetId :: {ex.Data} widgetId = {widgetId}";
                Logger.Error($"PerformTapAction({widgetId}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Perform the specified action on the widget with the specified ID. (UIv2 , UIv3)")]
        [KeywordDisplayName("Perform TapAndHold Action")]
        [KeywordParameters("widgetId", "The widget ID")]
        [SampleScript("Sirius.Perform TapAndHold Action (group.group.fax)")]
        public KeywordResult PerformTapAndHoldAction(string widgetId)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.PerformAction(WidgetAction.TapAndHold, widgetId);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.PerformAction(WidgetAction.TapAndHold, widgetId);
                }

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Perform TapAndHold Action failed with given widgetId :: {ex.Data} widgetId = {widgetId}";
                Logger.Error($"PerformTapAndHoldAction({widgetId}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Perform the specified action on the widget with the specified ID. (UIv2 , UIv3)")]
        [KeywordDisplayName("Perform Pan Action")]
        [KeywordParameters("widgetId", "The widget ID")]
        [SampleScript("will add in next release")]
        public KeywordResult PerformPanAction(string widgetId)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.PerformAction(WidgetAction.Pan, widgetId);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.PerformAction(WidgetAction.Pan, widgetId);
                }

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Perform Pan Action failed with given widgetId :: {ex.Data} widgetId = {widgetId}";
                Logger.Error($"PerformPanAction({widgetId}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Perform the specified action on the widget with the specified ID. (UIv2 , UIv3)")]
        [KeywordDisplayName("Perform MultiTap Action")]
        [KeywordParameters("widgetId", "The widget ID")]
        [SampleScript("Sirius.Perform MultiTap Action (group.group.print)")]
        public KeywordResult PerformMultiTapAction(string widgetId)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.PerformAction(WidgetAction.MultiTap, widgetId);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.PerformAction(WidgetAction.MultiTap, widgetId);
                }

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Perform MultiTap Action failed with given widgetId :: {ex.Data} widgetId = {widgetId}";
                Logger.Error($"PerformMultiTapAction({widgetId}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Send Press to target (UIv2 , UIv3)")]
        [KeywordDisplayName("Press")]
        [KeywordParameters("widgetId", "Press Sirius widget id")]
        [SampleScript("Sirius.Press (group.group.copy)")]
        public KeywordResult Press(string widgetId)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.Press($"{widgetId}");
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.Press($"{widgetId}");
                }

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Press id failed with given id :: {ex.Data} id = {widgetId}";
                Logger.Error($"Press({widgetId}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Press the widget that has a value (UIv2 , UIv3)")]
        [KeywordDisplayName("Press By Value")]
        [KeywordParameters("value", "The widget value")]
        [SampleScript("Sirius.Press By Value (Reports)")]
        public KeywordResult PressByValue(string value)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.PressByValue($"{value}");
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.PressByValue($"{value}");
                }

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Press by value failed with given value :: {ex.Data} id = {value}";
                Logger.Error($"Press By Value({value}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Press the widget that has a value (UIv2 , UIv3)")]
        [KeywordDisplayName("Press By Same Value")]
        [KeywordParameters("value", "The widget value must be the same as the specified string")]
        [SampleScript("Sirius.Press By Same Value (Quick Forms)")]
        public KeywordResult PressBySameValue(string value)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.PressByValue($"{value}", StringMatch.Exact);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.PressByValue($"{value}", StringMatch.Exact);
                }

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Press by same value failed with given value :: {ex.Data} id = {value}";
                Logger.Error($"Press By Same Value({value}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Press the widget that has a value (UIv2 , UIv3)")]
        [KeywordDisplayName("Press By Contains Value")]
        [KeywordParameters("value", "The widget value must contain the specified string")]
        [SampleScript("Sirius.Press By Contains Value (Scan)")]
        public KeywordResult PressByContainsValue(string value)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.PressByValue($"{value}", StringMatch.Contains);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.PressByValue($"{value}", StringMatch.Contains);
                }

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Press by contains value failed with given value :: {ex.Data} id = {value}";
                Logger.Error($"Press By Contains Value({value}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Press the widget that has a value (UIv2 , UIv3)")]
        [KeywordDisplayName("Press By StartsWith Value")]
        [KeywordParameters("value", "The widget value must start with the specified string")]
        [SampleScript("Sirius.Press By StartsWith Value (Network)")]
        public KeywordResult PressByStartsWithValue(string value)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.PressByValue($"{value}", StringMatch.StartsWith);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.PressByValue($"{value}", StringMatch.StartsWith);
                }

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Press by startsWith value failed with given value :: {ex.Data} id = {value}";
                Logger.Error($"Press By StartsWith Value({value}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Press the widget that has a value (UIv2 , UIv3)")]
        [KeywordDisplayName("Press By EndsWith Value")]
        [KeywordParameters("value", "The widget value must end with the specified string")]
        [SampleScript("Sirius.Press By EndsWith Value (Folder) ")]
        public KeywordResult PressByEndsWithValue(string value)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.PressByValue($"{value}", StringMatch.EndsWith);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.PressByValue($"{value}", StringMatch.EndsWith);
                }

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Press by end with value failed with given value :: {ex.Data} id = {value}";
                Logger.Error($"Press By EndsWith Value({value}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Press the widget that has a value (UIv2 , UIv3)")]
        [KeywordDisplayName("Press By Regex Value")]
        [KeywordParameters("value", "The widget value must match the regex pattern of the specified string")]
        [SampleScript("Sirius.Press By Regex Value (\bPrint\b)")]
        public KeywordResult PressByRegexValue(string value)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.PressByValue($"{value}", StringMatch.Regex);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.PressByValue($"{value}", StringMatch.Regex);
                }

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Press by regex value failed with given value :: {ex.Data} id = {value}";
                Logger.Error($"Press By Regex Value({value}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Set the value of the widget with the specified ID. (UIv2 , UIv3)")]
        [KeywordDisplayName("Set Value")]
        [KeywordParameters("widgetId", "The widget id")]
        [KeywordParameters("value", "The widget value")]
        [SampleScript("Sirius.Set Value (spin,10)")]
        public KeywordResult SetValue(string widgetId, string value)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.SetValue(widgetId, $"{value}");
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.SetValue(widgetId, $"{value}");
                }

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Set value failed with given widgetId :: {ex.Data} widgetId = {widgetId}, value = {value}";
                Logger.Error($"Set({value}) fail", ex);
                return kr;
            }
        }

        [GetKeyword]
        [KeywordDescription("Get the text of the widget with the specified ID. (UIv2 , UIv3)")]
        [KeywordDisplayName("Get Text")]
        [KeywordParameters("widgetId", "The widget id")]
        [KeywordParameters("saveTo", "variable name to store")]
        [SampleScript("Sirius.Get Text (group.group.copy,${buff})")]
        public KeywordResult GetText(string widgetId, string saveTo)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            widgetId = Support.Utils.GetVariablevalueIfExist(widgetId);
            try
            {
                string text = null;
                if (_device is SiriusUIv3Device)
                {
                    SiriusUIv3ControlPanel cp = ((SiriusUIv3Device)_device).ControlPanel;
                    Widget W_val = cp.GetScreenInfo().Widgets.Find(widgetId);
                    text = W_val.Values["text"];
                }
                else if (_device is SiriusUIv2Device)
                {
                    SiriusUIv2ControlPanel cp = ((SiriusUIv2Device)_device).ControlPanel;
                    Widget W_val = cp.GetScreenInfo().Widgets.Find(widgetId);
                    text = W_val.Values["text"];
                }

                if (!string.IsNullOrEmpty(text))
                {
                    CommonExecutionInfo.SetVariable(saveTo, text);
                    kr.Output = $"{saveTo} : {text}";
                    return kr;
                }
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Get Text failed with given widgetId :: {ex.Data} widgetId = {widgetId}";
                Logger.Error($"Get Text({widgetId}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Scrolls to the widget with the specified ID. (UIv2 , UIv3)")]
        [KeywordDisplayName("Scroll To Item")]
        [KeywordParameters("scrollWidgetId", "The ID of the scroll widget that contains the item to scroll to.")]
        [KeywordParameters("widgetId", "The widget ID")]
        [SampleScript(" Sirius.Scroll To Item (slistview,command.fax_restore_settings)")]
        public KeywordResult ScrollToItem(string scrollWidgetId, string widgetId)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.ScrollToItem(scrollWidgetId, widgetId);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.ScrollToItem(scrollWidgetId, widgetId);
                }

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Scroll To Item failed with given widgetId :: {ex.Data} scrollWidgetId = {scrollWidgetId}, widgetId = {widgetId}";
                Logger.Error($"ScrollToItem({widgetId}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Scrolls to the widget with a value matching the specified value. (UIv2 , UIv3)")]
        [KeywordDisplayName("Scroll To Item By Value")]
        [KeywordParameters("scrollWidgetId", "The ID of the scroll widget that contains the item to scroll to.")]
        [KeywordParameters("widgetValue", "The widget ID")]
        [SampleScript("Sirius.Scroll To Item By Value(slistview,Restore Settings)")]
        public KeywordResult ScrollToItemByValue(string scrollWidgetId, string widgetValue)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.ScrollToItemByValue(scrollWidgetId, widgetValue);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.ScrollToItemByValue(scrollWidgetId, widgetValue);
                }

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Scroll To Item By Value failed with given widgetValue :: {ex.Data} scrollWidgetId = {scrollWidgetId}, widgetValue = {widgetValue}";
                Logger.Error($"ScrollToItemByValue({widgetValue}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Scroll to and presses the widget with the specified ID. (UIv2 , UIv3)")]
        [KeywordDisplayName("Scroll Press")]
        [KeywordParameters("scrollWidgetId", "The ID of the scroll widget that contains the item to scroll to.")]
        [KeywordParameters("widgetId", "The widget ID")]
        [SampleScript("Sirius.Scroll Press (slistview,command.fax_restore_settings)")]
        public KeywordResult ScrollPress(string scrollWidgetId, string widgetId)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.ScrollPress(scrollWidgetId, widgetId);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.ScrollPress(scrollWidgetId, widgetId);
                }

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Scroll Press failed with given widgetId :: {ex.Data} scrollWidgetId = {scrollWidgetId}, widgetId = {widgetId}";
                Logger.Error($"ScrollPress({widgetId}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Scrolls to and presses the widget with a value matching the specified value. (UIv2 , UIv3)")]
        [KeywordDisplayName("Scroll Press By Value")]
        [KeywordParameters("scrollWidgetId", "The ID of the scroll widget that contains the item to scroll to.")]
        [KeywordParameters("widgetValue", "The widget ID")]
        [SampleScript("Sirius.Scroll Press By Value (slistview,Billing Code)")]
        public KeywordResult ScrollPressByValue(string scrollWidgetId, string widgetValue)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.ScrollPressByValue(scrollWidgetId, widgetValue);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.ScrollPressByValue(scrollWidgetId, widgetValue);
                }

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Scroll Press By Value failed with given widgetValue :: {ex.Data} scrollWidgetId = {scrollWidgetId}, widgetValue = {widgetValue}";
                Logger.Error($"ScrollPressByValue({widgetValue}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Wait for Screen Id until given timeout (seconds) (UIv3)")]
        [KeywordDisplayName("Wait For ScreenId")]
        [KeywordParameters("id", "Wait until Sirius Screen id appears")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("Sirius.Wait For ScreenId (home_grid,5)")]
        public KeywordResult WaitForScreenId(string id, string time)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int Time = Convert.ToInt32(time);
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddSeconds(Time);
            TimeSpan waitingTime;

            try
            {
                if (_device == null)
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Device object is null. Failed to Initialize. Please try after sometime.";                   
                    return kr;
                }
                 

                while (((SiriusUIv3Device)_device).ControlPanel.WaitForScreenId(id) == null)
                {
                    if (DateTime.Now >= endTime)
                    {
                        waitingTime = DateTime.Now.Subtract(startTime);
                        kr.Result = KeywordResults.Fail;
                        kr.Output = $"Wait for ScreenId failed with given ScreenId and timeout:: {id}, {waitingTime} sec";
                        kr.ScreenShot = CaptureScreen();
                        return kr;
                    }
                    Thread.Sleep(500);
                }

                waitingTime = DateTime.Now.Subtract(startTime);
                Logger.Debug($"Waiting time for ScreenId: {waitingTime} sec");
                kr.Output = $"Waiting time: {waitingTime}";
                return kr;
            }
            catch (Exception ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Wait for ScreenId gone error with given ScreenId and timeout:: {id}, {waitingTime}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"WaitForScreenId({id}) error", ex);
                return kr;
            }
        }

        [KeywordDescription("Wait for Active Screen Label until given timeout (seconds) (UIv2)")]
        [KeywordDisplayName("Wait For Active ScreenLabel")]
        [KeywordParameters("label", "Wait until Sirius Active Screen Label appears")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("Sirius.Wait For Active ScreenLabel (_t,5)")]
        public KeywordResult WaitForActiveScreenLabel(string label, string time)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int Time = Convert.ToInt32(time);
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddSeconds(Time);
            TimeSpan waitingTime;
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    while (((SiriusUIv3Device)_device).ControlPanel.WaitForActiveScreenLabel(label) == null)
                    {
                        if (DateTime.Now >= endTime)
                        {
                            waitingTime = DateTime.Now.Subtract(startTime);
                            kr.Result = KeywordResults.Fail;
                            kr.Output = $"Wait for ActiveScreenLabel failed with given ActiveScreenLabel and timeout:: {label}, {waitingTime} sec";
                            kr.ScreenShot = CaptureScreen();
                            return kr;
                        }
                        Thread.Sleep(500);
                    }
                }
                else if (_device is SiriusUIv2Device)
                {
                    while (((SiriusUIv2Device)_device).ControlPanel.WaitForActiveScreenLabel(label) == null)
                    {
                        if (DateTime.Now >= endTime)
                        {
                            waitingTime = DateTime.Now.Subtract(startTime);
                            kr.Result = KeywordResults.Fail;
                            kr.Output = $"Wait for ActiveScreenLabel failed with given ActiveScreenLabel and timeout:: {label}, {waitingTime} sec";
                            kr.ScreenShot = CaptureScreen();
                            return kr;
                        }
                        Thread.Sleep(500);
                    }
                }

                waitingTime = DateTime.Now.Subtract(startTime);
                Logger.Debug($"Waiting time for ActiveScreenLabel: {waitingTime} sec");
                kr.Output = $"Waiting time: {waitingTime}";
                return kr;
            }
            catch (Exception ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Wait for ActiveScreenLabel gone error with given ActiveScreenLabel and timeout:: {label}, {waitingTime}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"WaitForActiveScreenLabel({label}) error", ex);
                return kr;
            }
        }

        [KeywordDescription("Wait for widget with the specified ID until given timeout (seconds) (UIv2 , UIv3)")]
        [KeywordDisplayName("Wait For Widget")]
        [KeywordParameters("widgetId", "Wait until widget Id appears")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript(" Sirius.Wait For Widget (Quick Forms,5)")]
        public KeywordResult WaitForWidget(string widgetId, string time)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int Time = Convert.ToInt32(time);
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddSeconds(Time);
            TimeSpan waitingTime;
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    while (((SiriusUIv3Device)_device).ControlPanel.WaitForWidget(widgetId, TimeSpan.FromMilliseconds(1)) != null)
                    {
                        if (DateTime.Now >= endTime)
                        {
                            waitingTime = DateTime.Now.Subtract(startTime);
                            kr.Result = KeywordResults.Fail;
                            kr.Output = $"Wait for widget failed with given widgetId and timeout:: {widgetId}, {waitingTime} sec";
                            kr.ScreenShot = CaptureScreen();
                            return kr;
                        }
                        Thread.Sleep(500);
                    }
                }
                else if (_device is SiriusUIv2Device)
                {
                    while (((SiriusUIv2Device)_device).ControlPanel.WaitForWidget(widgetId, TimeSpan.FromMilliseconds(1)) != null)
                    {
                        if (DateTime.Now >= endTime)
                        {
                            waitingTime = DateTime.Now.Subtract(startTime);
                            kr.Result = KeywordResults.Fail;
                            kr.Output = $"Wait for widget failed with given widgetId and timeout:: {widgetId}, {waitingTime} sec";
                            kr.ScreenShot = CaptureScreen();
                            return kr;
                        }
                        Thread.Sleep(500);
                    }
                }

                waitingTime = DateTime.Now.Subtract(startTime);
                Logger.Debug($"Waiting time for Widget: {waitingTime} sec");
                kr.Output = $"Waiting time: {waitingTime}";
                return kr;
            }
            catch (Exception ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Wait for widget error with given widgetId and timeout:: {widgetId}, {waitingTime}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"WaitForWidget({widgetId}) error", ex);
                return kr;
            }
        }

        [KeywordDescription("Wait for widget that has a value matching the specified value until given timeout (seconds) (UIv2 , UIv3)")]
        [KeywordDisplayName("Wait For Widget By Value")]
        [KeywordParameters("value", "The widget value")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("Sirius.Wait For Widget By Value (Print From USB,5)")]
        public KeywordResult WaitForWidgetByValue(string value, string time)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int Time = Convert.ToInt32(time);
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddSeconds(Time);
            TimeSpan waitingTime;
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    while (((SiriusUIv3Device)_device).ControlPanel.WaitForWidgetByValue(value) == null)
                    {
                        if (DateTime.Now >= endTime)
                        {
                            waitingTime = DateTime.Now.Subtract(startTime);
                            kr.Result = KeywordResults.Fail;
                            kr.Output = $"Wait for widget by value failed with given value and timeout:: {value}, {waitingTime} sec";
                            kr.ScreenShot = CaptureScreen();
                            return kr;
                        }
                        Thread.Sleep(500);
                    }
                }
                else if (_device is SiriusUIv2Device)
                {
                    while (((SiriusUIv2Device)_device).ControlPanel.WaitForWidgetByValue(value) != null)
                    {
                        if (DateTime.Now >= endTime)
                        {
                            waitingTime = DateTime.Now.Subtract(startTime);
                            kr.Result = KeywordResults.Fail;
                            kr.Output = $"Wait for widget by value failed with given value and timeout:: {value}, {waitingTime} sec";
                            kr.ScreenShot = CaptureScreen();
                            return kr;
                        }
                        Thread.Sleep(500);
                    }
                }

                waitingTime = DateTime.Now.Subtract(startTime);
                Logger.Debug($"Waiting time for Widget by value: {waitingTime} sec");
                kr.Output = $"Waiting time: {waitingTime}";
                return kr;
            }
            catch (Exception ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Wait for widget by value error with given value and timeout:: {value}, {waitingTime}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"WaitForWidgetByValue({value}) error", ex);
                return kr;
            }
        }

        [KeywordDescription("Wait for widget that has a value matching the specified value until given timeout (seconds) (UIv2 , UIv3)")]
        [KeywordDisplayName("Wait For Widget By Same Value")]
        [KeywordParameters("value", "The widget value must be the same as the specified string")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("  Sirius.Wait For Widget By Same Value(Quick Forms,5)")]
        public KeywordResult WaitForWidgetBySameValue(string value, string time)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int Time = Convert.ToInt32(time);
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddSeconds(Time);
            TimeSpan waitingTime;
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    while (((SiriusUIv3Device)_device).ControlPanel.WaitForWidgetByValue(value, StringMatch.Exact)  == null)
                    {
                        if (DateTime.Now >= endTime)
                        {
                            waitingTime = DateTime.Now.Subtract(startTime);
                            kr.Result = KeywordResults.Fail;
                            kr.Output = $"Wait for widget by same value failed with given value and timeout:: {value}, {waitingTime} sec";
                            kr.ScreenShot = CaptureScreen();
                            return kr;
                        }
                        Thread.Sleep(500);
                    }
                }
                else if (_device is SiriusUIv2Device)
                {
                    while (((SiriusUIv2Device)_device).ControlPanel.WaitForWidgetByValue(value, StringMatch.Exact) == null)
                    {
                        if (DateTime.Now >= endTime)
                        {
                            waitingTime = DateTime.Now.Subtract(startTime);
                            kr.Result = KeywordResults.Fail;
                            kr.Output = $"Wait for widget by same value failed with given value and timeout:: {value}, {waitingTime} sec";
                            kr.ScreenShot = CaptureScreen();
                            return kr;
                        }
                        Thread.Sleep(500);
                    }
                }

                waitingTime = DateTime.Now.Subtract(startTime);
                Logger.Debug($"Waiting time for Widget by same value: {waitingTime} sec");
                kr.Output = $"Waiting time: {waitingTime}";
                return kr;
            }
            catch (Exception ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Wait for widget by same value error with given value and timeout:: {value}, {waitingTime}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"WaitForWidgetBySameValue({value}) error", ex);
                return kr;
            }
        }

        [KeywordDescription("Wait for widget that has a value matching the specified value until given timeout (seconds) (UIv2 , UIv3)")]
        [KeywordDisplayName("Wait For Widget By Contains Value")]
        [KeywordParameters("value", "The widget value must contain the specified string")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("Sirius.Wait For Widget By Contains Value (list,5)")]
        public KeywordResult WaitForWidgetByContainsValue(string value, string time)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int Time = Convert.ToInt32(time);
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddSeconds(Time);
            TimeSpan waitingTime;
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    while (((SiriusUIv3Device)_device).ControlPanel.WaitForWidgetByValue(value, StringMatch.Contains) == null)
                    {
                        if (DateTime.Now >= endTime)
                        {
                            waitingTime = DateTime.Now.Subtract(startTime);
                            kr.Result = KeywordResults.Fail;
                            kr.Output = $"Wait for widget by contains value failed with given value and timeout:: {value}, {waitingTime} sec";
                            kr.ScreenShot = CaptureScreen();
                            return kr;
                        }
                        Thread.Sleep(500);
                    }
                }
                else if (_device is SiriusUIv2Device)
                {
                    while (((SiriusUIv2Device)_device).ControlPanel.WaitForWidgetByValue(value, StringMatch.Contains) == null)
                    {
                        if (DateTime.Now >= endTime)
                        {
                            waitingTime = DateTime.Now.Subtract(startTime);
                            kr.Result = KeywordResults.Fail;
                            kr.Output = $"Wait for widget by contains value failed with given value and timeout:: {value}, {waitingTime} sec";
                            kr.ScreenShot = CaptureScreen();
                            return kr;
                        }
                        Thread.Sleep(500);
                    }
                }

                waitingTime = DateTime.Now.Subtract(startTime);
                Logger.Debug($"Waiting time for Widget by contains value: {waitingTime} sec");
                kr.Output = $"Waiting time: {waitingTime}";
                return kr;
            }
            catch (Exception ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Wait for widget by contains value error with given value and timeout:: {value}, {waitingTime}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"WaitForWidgetByContainsValue({value}) error", ex);
                return kr;
            }
        }

        [KeywordDescription("Wait for widget that has a value matching the specified value until given timeout (seconds) (UIv2 , UIv3)")]
        [KeywordDisplayName("Wait For Widget By StartsWith Value")]
        [KeywordParameters("value", "The widget value must start with the specified string")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("Sirius.Wait For Widget By StartsWith Value(Quick,5)")]
        public KeywordResult WaitForWidgetByStartsWithValue(string value, string time)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int Time = Convert.ToInt32(time);
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddSeconds(Time);
            TimeSpan waitingTime;
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    while (((SiriusUIv3Device)_device).ControlPanel.WaitForWidgetByValue(value, StringMatch.StartsWith) == null)
                    {
                        if (DateTime.Now >= endTime)
                        {
                            waitingTime = DateTime.Now.Subtract(startTime);
                            kr.Result = KeywordResults.Fail;
                            kr.Output = $"Wait for widget by starts with value failed with given value and timeout:: {value}, {waitingTime} sec";
                            kr.ScreenShot = CaptureScreen();
                            return kr;
                        }
                        Thread.Sleep(500);
                    }
                }
                else if (_device is SiriusUIv2Device)
                {
                    while (((SiriusUIv2Device)_device).ControlPanel.WaitForWidgetByValue(value, StringMatch.StartsWith) == null)
                    {
                        if (DateTime.Now >= endTime)
                        {
                            waitingTime = DateTime.Now.Subtract(startTime);
                            kr.Result = KeywordResults.Fail;
                            kr.Output = $"Wait for widget by starts with value failed with given value and timeout:: {value}, {waitingTime} sec";
                            kr.ScreenShot = CaptureScreen();
                            return kr;
                        }
                        Thread.Sleep(500);
                    }
                }

                waitingTime = DateTime.Now.Subtract(startTime);
                Logger.Debug($"Waiting time for Widget by starts with value: {waitingTime} sec");
                kr.Output = $"Waiting time: {waitingTime}";
                return kr;
            }
            catch (Exception ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Wait for widget by starts with value error with given value and timeout:: {value}, {waitingTime}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"WaitForWidgetByStartsWithValue({value}) error", ex);
                return kr;
            }
        }

        [KeywordDescription("Wait for widget that has a value matching the specified value until given timeout (seconds) (UIv2 , UIv3)")]
        [KeywordDisplayName("Wait For Widget By StartsWith Value")]
        [KeywordParameters("value", "The widget value must end with the specified string")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        public KeywordResult WaitForWidgetByEndsWithValue(string value, string time)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int Time = Convert.ToInt32(time);
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddSeconds(Time);
            TimeSpan waitingTime;
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    while (((SiriusUIv3Device)_device).ControlPanel.WaitForWidgetByValue(value, StringMatch.EndsWith) == null)
                    {
                        if (DateTime.Now >= endTime)
                        {
                            waitingTime = DateTime.Now.Subtract(startTime);
                            kr.Result = KeywordResults.Fail;
                            kr.Output = $"Wait for widget by ends with value failed with given value and timeout:: {value}, {waitingTime} sec";
                            kr.ScreenShot = CaptureScreen();
                            return kr;
                        }
                        Thread.Sleep(500);
                    }
                }
                else if (_device is SiriusUIv2Device)
                {
                    while (((SiriusUIv2Device)_device).ControlPanel.WaitForWidgetByValue(value, StringMatch.EndsWith, TimeSpan.FromMilliseconds(1)) != null)
                    {
                        if (DateTime.Now >= endTime)
                        {
                            waitingTime = DateTime.Now.Subtract(startTime);
                            kr.Result = KeywordResults.Fail;
                            kr.Output = $"Wait for widget by ends with value failed with given value and timeout:: {value}, {waitingTime} sec";
                            kr.ScreenShot = CaptureScreen();
                            return kr;
                        }
                        Thread.Sleep(500);
                    }
                }

                waitingTime = DateTime.Now.Subtract(startTime);
                Logger.Debug($"Waiting time for Widget by ends with value: {waitingTime} sec");
                kr.Output = $"Waiting time: {waitingTime}";
                return kr;
            }
            catch (Exception ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Wait for widget by ends with value error with given value and timeout:: {value}, {waitingTime}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"WaitForWidgetByEndsWithValue({value}) error", ex);
                return kr;
            }
        }

        [KeywordDescription("Wait for widget that has a value matching the specified value until given timeout (seconds) (UIv2 , UIv3)")]
        [KeywordDisplayName("Wait For Widget By Regex Value")]
        [KeywordParameters("value", "The widget value must match the regex pattern of the specified string")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript(" Sirius.Wait For Widget By Regex Value(\bQuick Forms\b,5)")]
        public KeywordResult WaitForWidgetByRegexValue(string value, string time)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int Time = Convert.ToInt32(time);
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddSeconds(Time);
            TimeSpan waitingTime;
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    while (((SiriusUIv3Device)_device).ControlPanel.WaitForWidgetByValue(value, StringMatch.Regex) == null)
                    {
                        if (DateTime.Now >= endTime)
                        {
                            waitingTime = DateTime.Now.Subtract(startTime);
                            kr.Result = KeywordResults.Fail;
                            kr.Output = $"Wait for widget by ends with value failed with given value and timeout:: {value}, {waitingTime} sec";
                            kr.ScreenShot = CaptureScreen();
                            return kr;
                        }
                        Thread.Sleep(500);
                    }
                }
                else if (_device is SiriusUIv2Device)
                {
                    while (((SiriusUIv2Device)_device).ControlPanel.WaitForWidgetByValue(value, StringMatch.Regex) == null)
                    {
                        if (DateTime.Now >= endTime)
                        {
                            waitingTime = DateTime.Now.Subtract(startTime);
                            kr.Result = KeywordResults.Fail;
                            kr.Output = $"Wait for widget by regex value failed with given value and timeout:: {value}, {waitingTime} sec";
                            kr.ScreenShot = CaptureScreen();
                            return kr;
                        }
                        Thread.Sleep(500);
                    }
                }

                waitingTime = DateTime.Now.Subtract(startTime);
                Logger.Debug($"Waiting time for Widget by regex value: {waitingTime} sec");
                kr.Output = $"Waiting time: {waitingTime}";
                return kr;
            }
            catch (Exception ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Wait for widget by regex value error with given value and timeout:: {value}, {waitingTime}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"WaitForWidgetByRegexValue({value}) error", ex);
                return kr;
            }
        }

        [KeywordDescription("Wait for Screen Label until given timeout (seconds) (UIv3)")]
        [KeywordDisplayName("Wait For ScreenLabel")]
        [KeywordParameters("id", "Wait until Sirius Screen Label appears")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("Sirius.Wait For ScreenLabel (slistview_cwh,5)")]
        public KeywordResult WaitForScreenLabel(string id, string time)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int Time = Convert.ToInt32(time);
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddSeconds(Time);
            TimeSpan waitingTime;
            try
            {
                while (!((SiriusUIv3Device)_device).ControlPanel.WaitForScreenLabel(id, TimeSpan.FromMilliseconds(1)))
                {
                    if (DateTime.Now >= endTime)
                    {
                        waitingTime = DateTime.Now.Subtract(startTime);
                        kr.Result = KeywordResults.Fail;
                        kr.Output = $"Wait for ScreenLabel failed with given ScreenLabel and timeout:: {id}, {waitingTime} sec";
                        kr.ScreenShot = CaptureScreen();
                        return kr;
                    }
                    Thread.Sleep(500);
                }

                waitingTime = DateTime.Now.Subtract(startTime);
                Logger.Debug($"Waiting time for ScreenLabel: {waitingTime} sec");
                kr.Output = $"Waiting time: {waitingTime}";
                return kr;
            }
            catch (Exception ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Wait for ScreenLabel gone error with given ScreenLabel and timeout:: {id}, {waitingTime}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"WaitForScreenLabel({id}) error", ex);
                return kr;
            }
        }

        [KeywordDescription("Send Home Key to Sirius device (UIv2 , UIv3)")]
        [KeywordDisplayName("Press Home Key")]
        [SampleScript("Sirius.Press Home Key")]
        public KeywordResult PressHomeKey()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.PressKey(SiriusSoftKey.Home);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.PressKey(SiriusSoftKey.Home);
                }
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

        [KeywordDescription("Send Back key to Sirius device (UIv2 , UIv3)")]
        [KeywordDisplayName("Press Back Key")]
        [SampleScript("Sirius.Press Back Key")]
        public KeywordResult PressBackKey()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.PressKey(SiriusSoftKey.Back);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.PressKey(SiriusSoftKey.Back);
                }
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

        [KeywordDescription("Send Help key to Sirius device (UIv2 , UIv3)")]
        [KeywordDisplayName("Press Help Key")]
        [SampleScript("Sirius.Press Help Key")]
        public KeywordResult PressHelpKey()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.PressKey(SiriusSoftKey.Help);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.PressKey(SiriusSoftKey.Help);
                }
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Press help key failed";
                Logger.Error("PressHelpKey() fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Send Cancel key to Sirius device (UIv2 , UIv3)")]
        [KeywordDisplayName("Press Cancel Key")]
        [SampleScript("Sirius.Press Cancel Key")]
        public KeywordResult PressCancelKey()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.PressKey(SiriusSoftKey.Cancel);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.PressKey(SiriusSoftKey.Cancel);
                }
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Press cancel key failed";
                Logger.Error("PressCancelKey() fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Send Left key to Sirius device (UIv2 , UIv3)")]
        [KeywordDisplayName("Press Left Key")]
        [SampleScript("Sirius.Press Left Key")]
        public KeywordResult PressLeftKey()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.PressKey(SiriusSoftKey.Left);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.PressKey(SiriusSoftKey.Left);
                }
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Press left key failed";
                Logger.Error("PressLeftKey() fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Send Right key to Sirius device (UIv2 , UIv3)")]
        [KeywordDisplayName("Press Right Key")]
        [SampleScript("Sirius.Press Right Key")]
        public KeywordResult PressRightKey()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    ((SiriusUIv3Device)_device).ControlPanel.PressKey(SiriusSoftKey.Right);
                }
                else if (_device is SiriusUIv2Device)
                {
                    ((SiriusUIv2Device)_device).ControlPanel.PressKey(SiriusSoftKey.Right);
                }
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Press right key failed";
                Logger.Error("PressRightKey() fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Check the contol panel type is SiriusUIv2")]
        [KeywordDisplayName("Is UIv2")]
        [SampleScript(" Sirius.Is UIv2")]
        public KeywordResult IsUIv2()
        {
            try
            {
                if (_device is SiriusUIv2Device)
                {
                    KeywordResult pass = new KeywordResult(KeywordResults.Pass);
                    pass.Output = $"Control panel type is SiriusUIv2";
                    return pass;
                }

                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Control panel type is not SiriusUIv2";
                return fail;
            }
            catch (Exception ex)
            {
                KeywordResult kr = new KeywordResult(KeywordResults.Error);
                kr.AdditionalInfo = ex.ToString();
                kr.Output = "Error during check contoll panel type";
                return kr;
            }
        }

        [KeywordDescription("Check the contol panel type is SiriusUIv3")]
        [KeywordDisplayName("Is UIv3")]
        [SampleScript("Sirius.Is UIv3")]
        public KeywordResult IsUIv3()
        {
            try
            {
                if (_device is SiriusUIv3Device)
                {
                    KeywordResult pass = new KeywordResult(KeywordResults.Pass);
                    pass.Output = $"Control panel type is SiriusUIv3";
                    return pass;
                }

                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Control panel type is not SiriusUIv3";
                return fail;
            }
            catch (Exception ex)
            {
                KeywordResult kr = new KeywordResult(KeywordResults.Error);
                kr.AdditionalInfo = ex.ToString();
                kr.Output = "Error during check contoll panel type";
                return kr;
            }
        }
    }
}

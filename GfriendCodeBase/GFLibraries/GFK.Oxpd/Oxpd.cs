using HP.DeviceAutomation;
using HP.DeviceAutomation.Jedi;
using HP.GFriend.GFLogger;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using Logger = HP.GFriend.GFLogger.Logger;

namespace HP.GFriend.Keywords
{
    [LibraryDescription("<p><br>Steps to Retrieve IDs from the OXPD Screen: <p>1.Ensure the device is on the OXPD Browser Engine screen. <p>2.Open any browser and enter device_ip:9222 in the address bar and click enter(e.g:192.168.1.100:9222).\r\n. <p>3.Capture the IDs from the displayed page by using search option in the page <p><b>Note: <p>1.Use the Get Browser Engine keyword before calling any OXPD keywords.<p>2.Call the Return Browser Engine keyword after you're done.</b>")]
    public class Oxpd : IGFLibrary
    {
        /// <summary>
        /// Control the OXPd
        /// </summary>
        private DeviceUnderTest _dut;
        private JediOmniDevice _device;
        private static string _outputDir;

        private OxpdBrowserEngine _oxpdEngine;

        private Tuple<int, int> _resolution = null;

        private int _scaleFactor = 1;

        /// <summary>
        /// Initialization to connect JediOmni UI
        /// </summary>
        public string GetName()
        {
            return "Oxpd";
        }

        /// <summary>
        /// Initialization to connect Oxpd
        /// </summary>
        public void Initialize(DeviceUnderTest dut, string outputDir)
        {
            _dut = dut;
            _outputDir = outputDir;
        }

        public void Dispose()
        {
            _oxpdEngine = null;
        }

        public bool DutUsed()
        {
            return true;
        }

        public List<string> GetDependencies()
        {
            return new List<string> { "JediOmni" };
        }

        private void GetJediOmniDevice()
        {
            _device = (JediOmniDevice)CommonExecutionInfo.GetSharedObject("jediOmni:" + _dut.DeviceId);
        }

        private bool IsOutOfView(BoundingBox rect)
        {
            string isOutOfView = _oxpdEngine.ExecuteFunction($"getClientRectIsOutOfView", rect.Top, rect.Left, rect.Bottom, rect.Right).Trim('"');
            if (isOutOfView == "true")
            {
                return true;
            }
            return false;
        }

        public void GetResolution()
        {
            int x = 0, y = 0;
            try
            {
                x = _device.ControlPanel.GetBoundingBox("body").Right;
                y = _device.ControlPanel.GetBoundingBox("body").Bottom;
                _resolution = new Tuple<int, int>(x, y);
            }
            catch
            {
                _resolution = new Tuple<int, int>(0, 0);
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

        private Tuple<int, int> GetAbsolutePosition(string x, string y)
        {
            double dX = ((Int32.Parse(x)) / (double)100);
            double dY = ((Int32.Parse(y)) / (double)100);

            int aX = (int)(_resolution.Item1 * dX);
            int aY = (int)(_resolution.Item2 * dY);

            return new Tuple<int, int>(aX, aY);
        }

        [KeywordDescription("Set timeout scacle for applying all keywords in library." +
            "This scale facor will be multiplied of all timeout arguments in keywords." +
            "For example, if scale factor is set to 2 and call wait for object for 3 secondes, it will wait for 6 (3 x 2) seconds.")]
        [KeywordDisplayName("Set Timeout Scale")]
        [KeywordParameters("scaleFactor", "scale factor. Should be a number and larger than 0")]
        [SampleScript("Oxpd.Set Timeout Scale (10)")]
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

        [KeywordDescription("Get the control of Oxpd Browser on current screen. This keyword should be called before using any of Oxpd keywords.")]
        [KeywordDisplayName("Get Browser Engine")]
        [SampleScript("Oxpd.Get Browser Engine ()")]
        public KeywordResult GetBrowserEngine()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            if (_oxpdEngine != null)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Already Got Oxpd Browser Engine and not returned.";
                return kr;
            }

            try
            {
                GetJediOmniDevice();
                _oxpdEngine = new OxpdBrowserEngine(_device.ControlPanel, OxpdResource.OxpdJavaScript);
                _oxpdEngine.ExecuteJavaScript(OxpdResource.OxpdJavaScript);

                if (_oxpdEngine == null)
                {
                    GetJediOmniDevice();
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    Logger.Debug("Try to getting Oxpd Browser Engine again");
                    _oxpdEngine = new OxpdBrowserEngine(_device.ControlPanel, OxpdResource.OxpdJavaScript);
                    _oxpdEngine.ExecuteJavaScript(OxpdResource.OxpdJavaScript);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error getting Oxpd Browser Engine.", ex);
                kr.Output = "Oxpd Browser Engine Getting error";
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                return kr;
            }

            if (_oxpdEngine == null)
            {
                Logger.Error("Fail to getting Oxpd Browser Engine.");
                kr.Output = "Oxpd Browser Engine Getting Fail";
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = CaptureScreen();
            }
            else
            {
                GetResolution();
            }
            return kr;
        }

        [KeywordDescription("Get the control of Oxpd Browser on current screen. This keyword should be called before using any of Oxpd keywords.")]
        [KeywordDisplayName("Get Browser Engine")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("Oxpd.Get Browser Engine (10)")]
        public KeywordResult GetBrowserEngine(string time)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int Time = Convert.ToInt32(time);
            Time = Time * _scaleFactor;
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddSeconds(Time);
            int retryCount = 1;

            if (_oxpdEngine != null)
            {
                kr.Result = KeywordResults.Error;
                kr.Output = "Already Got Oxpd Browser Engine and not returned.";
                return kr;
            }

            GetJediOmniDevice();

            _oxpdEngine = new OxpdBrowserEngine(_device.ControlPanel, OxpdResource.OxpdJavaScript);

            while (DateTime.Now < endTime)
            {
                try
                {
                    _oxpdEngine.ExecuteJavaScript(OxpdResource.OxpdJavaScript);
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Debug($"Try to getting Oxpd Browser Engine again: {retryCount}");
                    retryCount++;

                    if (DateTime.Now >= endTime)
                    {
                        Logger.Error($"Error getting Oxpd Browser Engine over {DateTime.Now.Subtract(startTime)} secs.", ex);
                        kr.Output = "Oxpd Browser Engine Getting error";
                        kr.Result = KeywordResults.Error;
                        kr.AdditionalInfo = ex.ToString();
                        return kr;
                    }
                    _device.ControlPanel.SignalUserActivity();
                }
            }

            if (_oxpdEngine == null)
            {
                Logger.Error("Fail to getting Oxpd Browser Engine.");
                kr.Output = "Oxpd Browser Engine Getting Fail";
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = CaptureScreen();
            }
            else
            {
                GetResolution();
            }
            return kr;
        }

        [KeywordDescription("Return the control of Oxpd Browser Engine\r\n This Keyword MUST be called after all Oxpd Browser Engine releated keywords")]
        [KeywordDisplayName("Return Browser Engine")]
        [SampleScript("Oxpd.Return Browser Engine ()")]
        public KeywordResult ReturnBrowserEngine()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            if (_oxpdEngine == null)
            {
                return kr;
            }

            try
            {
                _oxpdEngine = null;
            }
            catch (Exception ex)
            {
                Logger.Error("Error null to Browser Engine.", ex);
                kr.Output = "Browser Engine returning error";
                kr.AdditionalInfo = ex.ToString();
                kr.Result = KeywordResults.Error;
            }
            return kr;
        }


        [KeywordDescription("Send BackSpace Key to target")]
        [KeywordDisplayName("Press BackSpace Key")]
        [SampleScript("Oxpd.Press BackSpace Key ()")]
        public KeywordResult PressBackSpaceKey()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            try
            {
                _device.ControlPanel.Type(SpecialCharacter.Backspace);
                kr.Result = KeywordResults.Pass;
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = CaptureScreen();
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Press Back Space key failed";
                Logger.Error("PressBackSpaceKey() fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Send Enter Key to target")]
        [KeywordDisplayName("Press Enter Key")]
        [SampleScript("Oxpd.Press Enter Key ()")]
        public KeywordResult PressEnterKey()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            try
            {
                _device.ControlPanel.Type(SpecialCharacter.Enter);
                kr.Result = KeywordResults.Pass;
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = CaptureScreen();
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Press Enter key failed";
                Logger.Error("PressEnterKey() fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Send ESC Key to target")]
        [KeywordDisplayName("Press ESC Key")]
        [SampleScript("Oxpd.Press ESC Key ()")]
        public KeywordResult PressESCKey()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            try
            {
                _device.ControlPanel.Type(SpecialCharacter.Escape);
                kr.Result = KeywordResults.Pass;
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.ScreenShot = CaptureScreen();
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Press ESC key failed";
                Logger.Error("PressESCKey() fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Send Home Key to target")]
        [KeywordDisplayName("Press Home Key")]
        [SampleScript("Oxpd.Press Home Key ()")]
        public KeywordResult PressHomeKey()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                _device.ControlPanel.PressHome();
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

        [KeywordDescription("Check the screen contains given text")]
        [KeywordDisplayName("Check Screen Contains Text")]
        [KeywordParameters("text", "Text to check screen contains given text")]
        [SampleScript("Oxpd.Check Screen Contains Text (Scan)")]
        public KeywordResult CheckScreenContainsText(string text)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Fail);
            try
            {
                if (_oxpdEngine.HtmlContains(text))
                {
                    kr.Result = KeywordResults.Pass;
                    return kr;
                }
                kr.Output = $"Check failed with given text :: {text}";
                kr.ScreenShot = CaptureScreen();
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Check failed with given text :: {text}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"CheckScreenContainsText({text}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Check the screen does not contains given text")]
        [KeywordDisplayName("Check Screen Not Contains Text")]
        [KeywordParameters("text", "Text to check screen does not contains given text")]
        [SampleScript("Oxpd.Check Screen Not Contains Text (Scans)")]
        public KeywordResult CheckScreenNotContainsText(string text)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                if (_oxpdEngine.HtmlContains(text))
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Check failed with given text :: {text}";
                    kr.ScreenShot = CaptureScreen();
                    return kr;
                }
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Check failed with given text :: {text}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"CheckScreenNotContainsText({text}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Touch object with given css selector")]
        [KeywordDisplayName("Touch Css")]
        [KeywordParameters("css", "Css to touch")]
        [SampleScript("Oxpd.Touch Css (#main_buttons .button)")]
        public KeywordResult TouchCss(string css)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            try
            {
                string result = _oxpdEngine.ExecuteFunction($"getIsExistElementbyCss", css).Trim('"');

                if (result == "false")
                {
                    Logger.Error($"Fail to find css on the UI: {css}");
                    throw new Exception();
                }

                string elementSelectionScript = $"document.querySelector('{css}')";
                decimal zoom = _oxpdEngine.GetZoomRequired(elementSelectionScript);
                string boundingArea = _oxpdEngine.ExecuteJavaScript($"{elementSelectionScript}.getBoundingClientRect()");
                BoundingBox initialBoundingArea = _oxpdEngine.ParseBoundingArea(boundingArea, zoom);

                if (!_oxpdEngine.BoundingAreaFoundWithin(_oxpdEngine._oxpdBodyBox.Value, initialBoundingArea))
                {
                    int timeout = 0;
                    _oxpdEngine.ExecuteJavaScript($"document.querySelector('{css}').scrollIntoView()");
                    while (!_oxpdEngine.BoundingAreaFoundWithin(_oxpdEngine._oxpdBodyBox.Value, initialBoundingArea) && timeout < 6)
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(500 * _scaleFactor));
                        timeout++;
                    }
                }

                boundingArea = _oxpdEngine.ExecuteJavaScript($"{elementSelectionScript}.getBoundingClientRect()");
                if (IsOutOfView(_oxpdEngine.ParseBoundingArea(boundingArea, zoom)))
                {
                    Logger.Error($"Given css selector is out of view.");
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Touch css failed with given css :: css is out of view. css = {css}";
                    return kr;
                }

                _oxpdEngine.PressScreen?.Invoke(_oxpdEngine.GetCenterCoordinate(_oxpdEngine.ParseBoundingArea(boundingArea, zoom)));

                kr.Result = KeywordResults.Pass;
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Touch css failed with given css :: {ex.Data} css = {css}";
                Logger.Error($"TouchCss({css}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Touch object with element id")]
        [KeywordDisplayName("Touch Id")]
        [KeywordParameters("id", "Id to touch")]
        [SampleScript("Oxpd.Touch Id (layout_header)")]
        public KeywordResult TouchID(string id)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                string elementSelectionScript = $"document.getElementById('{id}')";
                BoundingBox initialBoundingArea = _oxpdEngine.GetBoundingAreaById(id);

                if (!_oxpdEngine.BoundingAreaFoundWithin(_oxpdEngine._oxpdBodyBox.Value, initialBoundingArea))
                {
                    int timeout = 0;
                    _oxpdEngine.ExecuteJavaScript($"document.getElementById(\"{id}\").scrollIntoView()");
                    while (!_oxpdEngine.BoundingAreaFoundWithin(_oxpdEngine._oxpdBodyBox.Value, initialBoundingArea) && timeout < 6)
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(500 * _scaleFactor));
                        timeout++;
                    }
                }
                decimal zoom = _oxpdEngine.GetZoomRequired(elementSelectionScript);
                string boundingArea = _oxpdEngine.ExecuteJavaScript($"{elementSelectionScript}.getBoundingClientRect()");

                if (IsOutOfView(_oxpdEngine.ParseBoundingArea(boundingArea, zoom)))
                {
                    Logger.Error($"Given element id is out of view.");
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Touch id failed with given id :: id is out of view. id = {id}";
                    return kr;
                }
                _oxpdEngine.PressScreen?.Invoke(_oxpdEngine.GetCenterCoordinate(_oxpdEngine.ParseBoundingArea(boundingArea, zoom)));
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Touch Id failed with given ID :: {ex.Data} ID = {id}";
                Logger.Error($"Touch Id({id}) fail", ex);
                return kr;
            }
        }

        [GetKeyword]
        [KeywordDescription("Returns the total number of rows present in the table body.")]
        [KeywordDisplayName("Get Table Row Count")]
        [KeywordParameters("tableXpath", "Xpath of table")]
        [KeywordParameters("saveTo", "Variable name to store row count")]
        [SampleScript("Oxpd.Get Table Row Count (//table[@id='employeeTable'],${RowCount})")]
        public KeywordResult GetTableRowCount(string tableXpath, string saveTo)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            try
            {
                // Input validation
                if (string.IsNullOrWhiteSpace(tableXpath))
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = "Table XPath cannot be null or empty.";
                    return kr;
                }

                if (string.IsNullOrWhiteSpace(saveTo))
                {
                    kr.Result = KeywordResults.Fail;
                    kr.Output = "SaveTo variable name cannot be null or empty.";
                    return kr;
                }

                string script = $@"
            (function(){{
                var table = document.evaluate(
                    ""{tableXpath}"",
                    document,
                    null,
                    XPathResult.FIRST_ORDERED_NODE_TYPE,
                    null
                ).singleNodeValue;

                if(table == null)
                    return 0;

                return table.querySelectorAll('tbody tr').length;
            }})();
        ";

                string result = _oxpdEngine.ExecuteJavaScript(script);

                kr.Output = result;

                CommonExecutionInfo.SetVariable(saveTo, result);

                Logger.Debug($"Total table row count: { result} ");

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.Output = "Failed to get total table row count";
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();

                Logger.Error("GetTotalTableRowCount failed", ex);

                return kr;
            }
        }

        [KeywordDescription("Touch by element id which will not call GetBoundingAreaById separately")]
        [KeywordDisplayName("TouchByElementId")]
        [KeywordParameters("id", "Id to touch")]
        [SampleScript("Oxpd.TouchByElementId(layout_header)")]
        public KeywordResult TouchByElementId(string id)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            try
            {
                _oxpdEngine.PressElementById(id);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Touch By ElementId failed with given ID:: {ex.Data} ID = {id}";
                Logger.Error($"Touch By ElementId({id}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Touch with position with percent of left and percent of top")]
        [KeywordDisplayName("Touch Position")]
        [KeywordParameters("x", "x percent value to drag with given position value")]
        [KeywordParameters("y", "y percent value to drag with given position value")]
        [SampleScript("Oxpd.Touch Position (203,157)")]
        public KeywordResult TouchPosition(string x, string y)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            int coordx = Convert.ToInt32(x);
            int coordy = Convert.ToInt32(y);

            Tuple<int, int> position = GetAbsolutePosition(x, y);

            try
            {
                _oxpdEngine.PressCoordinate(position.Item1, position.Item2);
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Touch failed with given value :: {coordx}, {coordy}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"TouchPosition({coordx}, {coordy}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Touch object with given text")]
        [KeywordDisplayName("Touch Text")]
        [KeywordParameters("text", "Text to touch")]
        [SampleScript("Oxpd.Touch Text (Email)")]
        public KeywordResult TouchText(string text)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            try
            {
                if (!_oxpdEngine.WaitForHtmlContains(text, TimeSpan.FromSeconds(1)))
                {
                    Logger.Error($"Fail to find text on the UI: {text}");
                    throw new Exception();
                }

                string elementSelectionScript = $"getElementbyText('{text}')";
                BoundingBox initialBoundingArea = _oxpdEngine.GetElementBoundingArea(elementSelectionScript);

                if (!_oxpdEngine.BoundingAreaFoundWithin(_oxpdEngine._oxpdBodyBox.Value, initialBoundingArea))
                {
                    int timeout = 0;
                    _oxpdEngine.ExecuteJavaScript($"{elementSelectionScript}.scrollIntoView()");
                    while (!_oxpdEngine.BoundingAreaFoundWithin(_oxpdEngine._oxpdBodyBox.Value, initialBoundingArea) && timeout < 6)
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(500 * _scaleFactor));
                        timeout++;
                    }
                }

                string boundingArea = _oxpdEngine.ExecuteJavaScript($"{elementSelectionScript}.getBoundingClientRect()");
                decimal zoom = _oxpdEngine.GetZoomRequired(elementSelectionScript);
                if (IsOutOfView(_oxpdEngine.ParseBoundingArea(boundingArea, zoom)))
                {
                    Logger.Error($"Given text is out of view.");
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Touch text failed with given text :: text is out of view. text = {text}";
                    return kr;
                }

                _oxpdEngine.PressScreen?.Invoke(_oxpdEngine.GetCenterCoordinate(_oxpdEngine.ParseBoundingArea(boundingArea, zoom)));

                kr.Result = KeywordResults.Pass;
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
        [SampleScript("Oxpd.Touch Text With Index (Scan,1)")]
        public KeywordResult TouchTextWithIndex(string text, string indexnumber)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            try
            {
                if (!_oxpdEngine.WaitForHtmlContains(text, TimeSpan.FromSeconds(1)))
                {
                    Logger.Error($"Fail to find text on the UI: {text}");
                    throw new Exception();
                }

                string elementSelection = $"getElementbyTextwithIndex('{text}','{indexnumber}')";
                decimal zoom = _oxpdEngine.GetZoomRequired(elementSelection);
                string boundingArea = _oxpdEngine.ExecuteJavaScript($"{elementSelection}.getBoundingClientRect()");
                BoundingBox initialBoundingArea = _oxpdEngine.ParseBoundingArea(boundingArea, zoom);

                if (!_oxpdEngine.BoundingAreaFoundWithin(_oxpdEngine._oxpdBodyBox.Value, initialBoundingArea))
                {
                    int timeout = 0;
                    _oxpdEngine.ExecuteJavaScript($"{elementSelection}.scrollIntoView()");
                    while (!_oxpdEngine.BoundingAreaFoundWithin(_oxpdEngine._oxpdBodyBox.Value, initialBoundingArea) && timeout < 6)
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(500 * _scaleFactor));
                        timeout++;
                    }
                }

                boundingArea = _oxpdEngine.ExecuteJavaScript(elementSelection);
                if (IsOutOfView(_oxpdEngine.ParseBoundingArea(boundingArea, zoom)))
                {
                    Logger.Error($"Given text with index is out of view.");
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Touch text with index failed with given text and index :: text is out of view. {text}, {indexnumber}";
                    return kr;
                }

                _oxpdEngine.PressScreen?.Invoke(_oxpdEngine.GetCenterCoordinate(_oxpdEngine.ParseBoundingArea(boundingArea, zoom)));

                kr.Result = KeywordResults.Pass;
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Touch text with index failed with given text and index:: {text}, {indexnumber}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"TouchTextWithIndex({text}, {indexnumber}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Touch object with given xpath selector")]
        [KeywordDisplayName("Touch Xpath")]
        [KeywordParameters("xpath", "Xpath to touch")]
        [SampleScript("Oxpd.Touch Xpath (//*[@id=\"stay_button\"])")]
        public KeywordResult TouchXpath(string xpath)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);

            try
            {
                string result = _oxpdEngine.ExecuteFunction($"getIsExistElementbyXpath", xpath).Trim('"');

                if (result == "false")
                {
                    Logger.Error($"Fail to find xpath on the UI: {xpath}");
                    throw new Exception();
                }

                string elementSelection = $"document.evaluate('{xpath}', document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue";
                string boundingArea = _oxpdEngine.ExecuteJavaScript($"{elementSelection}.getBoundingClientRect()");
                decimal zoom = _oxpdEngine.GetZoomRequired(elementSelection);
                BoundingBox initialBoundingArea = _oxpdEngine.ParseBoundingArea(boundingArea, zoom);

                if (!_oxpdEngine.BoundingAreaFoundWithin(_oxpdEngine._oxpdBodyBox.Value, initialBoundingArea))
                {
                    int timeout = 0;
                    _oxpdEngine.ExecuteJavaScript($"document.evaluate('{xpath}', document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue.scrollIntoView()");
                    while (!_oxpdEngine.BoundingAreaFoundWithin(_oxpdEngine._oxpdBodyBox.Value, initialBoundingArea) && timeout < 6)
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(500 * _scaleFactor));
                        timeout++;
                    }
                }

                boundingArea = _oxpdEngine.ExecuteJavaScript($"{elementSelection}.getBoundingClientRect()");
                if (IsOutOfView(_oxpdEngine.ParseBoundingArea(boundingArea, zoom)))
                {
                    Logger.Error($"Given xpath is out of view.");
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Touch xpath failed with given xpath  :: xpath is out of view. xpath = {xpath}";
                    return kr;
                }

                _oxpdEngine.PressScreen?.Invoke(_oxpdEngine.GetCenterCoordinate(_oxpdEngine.ParseBoundingArea(boundingArea, zoom)));

                kr.Result = KeywordResults.Pass;
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Touch xpath failed with given xpath :: {ex.Data} xpath = {xpath}";
                Logger.Error($"TouchXpath({xpath}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Wait given css is appeared on screen until given timeout (seconds)")]
        [KeywordDisplayName("Wait For Css")]
        [KeywordParameters("css", "Css to touch")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("Oxpd.Wait For Css (#main_buttons .button,20)")]
        public KeywordResult WaitForCss(string css, string time)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            kr.Result = KeywordResults.Pass;
            int Time = Convert.ToInt32(time);
            Time = Time * _scaleFactor;
            DateTime startTime = DateTime.Now;
            TimeSpan waitingTime;

            try
            {
                if (!Wait.ForTrue(() => _oxpdEngine.ExecuteFunction($"getIsExistElementbyCss", css).Trim('"') == "true", TimeSpan.FromSeconds(Time)))
                {
                    waitingTime = DateTime.Now.Subtract(startTime);
                    Logger.Error($"Fail to wait css on the UI: {css}");
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Wait for css failed with given time  :: css is out of view. css = {css} time = {waitingTime}";
                    return kr;
                }

                string elementSelectionScript = $"document.querySelector('{css}')";
                decimal zoom = _oxpdEngine.GetZoomRequired(elementSelectionScript);
                string boundingArea = _oxpdEngine.ExecuteJavaScript($"{elementSelectionScript}.getBoundingClientRect()");

                if (IsOutOfView(_oxpdEngine.ParseBoundingArea(boundingArea, zoom)))
                {
                    waitingTime = DateTime.Now.Subtract(startTime);
                    Logger.Error($"Given css is out of view.");
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Wait for css failed with given time  :: css is out of view. css = {css} time = {waitingTime}";
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
                kr.Output = $"Wait for css failed with given time :: {ex.Data} css = {css} time = {time}";
                Logger.Error($"WaitForCss({css}({time})) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Wait given css is disappeared on screen until given timeout (seconds)")]
        [KeywordDisplayName("Wait For Css Gone")]
        [KeywordParameters("xpath", "Wait to disappeared specific css")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("Oxpd.Wait For Css Gone (#main_buttons .button,20)")]
        public KeywordResult WaitForCssGone(string css, string time)
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

                if (!Wait.ForTrue(() => _oxpdEngine.ExecuteFunction($"getIsExistElementbyCss", css).Trim('"') == "false", TimeSpan.FromSeconds(Time)))
                {
                    waitingTime = DateTime.Now.Subtract(startTime);
                    Logger.Error($"Wait for css gone failed with given css on the UI: {css}");
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Wait for css gone failed with given id and timeout:: {css}, {waitingTime}";
                    return kr;
                }

                waitingTime = DateTime.Now.Subtract(startTime);
                Logger.Debug($"Waiting time for css: {waitingTime} sec");
                kr.Output = $"Waiting time: {waitingTime}";
                return kr;
            }
            catch (Exception ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Exception : Wait for css gone failed with given css and timeout:: {css}, {waitingTime}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"WaitForCssGone({css}, {time}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Wait given id is appeared on screen until given timeout (seconds)")]
        [KeywordDisplayName("Wait For Id")]
        [KeywordParameters("id", "Wait to appeared specific id")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("Oxpd.Wait For Id (layout_header,20)")]
        public KeywordResult WaitForId(string id, string time)
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

                if (!_oxpdEngine.WaitToExistElementId(id, TimeSpan.FromSeconds(Time)))
                {
                    waitingTime = DateTime.Now.Subtract(startTime);
                    Logger.Error($"Fail to wait id on the UI: {id}");
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Wait for id failed with given id and timeout:: {id}, {waitingTime}";
                    return kr;
                }

                string elementSelectionScript = $"document.getElementById('{id}')";
                decimal zoom = _oxpdEngine.GetZoomRequired(elementSelectionScript);
                string boundingArea = _oxpdEngine.ExecuteJavaScript($"{elementSelectionScript}.getBoundingClientRect()");

                if (IsOutOfView(_oxpdEngine.ParseBoundingArea(boundingArea, zoom)))
                {
                    waitingTime = DateTime.Now.Subtract(startTime);
                    Logger.Error($"Given id is out of view.");
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Wait for id failed with given time  :: id is out of view. id = {id} time = {waitingTime}";
                    return kr;
                }

                waitingTime = DateTime.Now.Subtract(startTime);
                Logger.Debug($"Waiting time for text: {waitingTime} sec");
                kr.Output = $"Waiting time: {waitingTime}";
                return kr;
            }
            catch (Exception ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Exception : Wait for id failed with given id and timeout:: {id}, {waitingTime}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"WaitForId({id}, {time}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Wait given id is disappeared on screen until given timeout (seconds)")]
        [KeywordDisplayName("Wait For Id Gone")]
        [KeywordParameters("id", "Wait to disappeared specific id")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("Oxpd.Wait For Id Gone (layout_header,20)")]
        public KeywordResult WaitForIdGone(string id, string time)
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

                if (!Wait.ForTrue(() => _oxpdEngine.ExecuteFunction($"getIsExistElementbyId", id).Trim('"') == "false", TimeSpan.FromSeconds(Time)))
                {
                    waitingTime = DateTime.Now.Subtract(startTime);
                    Logger.Error($"Wait for id gone failed with given id on the UI: {id}");
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Wait for id gone failed with given id and timeout:: {id}, {waitingTime}";
                    return kr;
                }

                waitingTime = DateTime.Now.Subtract(startTime);
                Logger.Debug($"Waiting time for text: {waitingTime} sec");
                kr.Output = $"Waiting time: {waitingTime}";
                return kr;
            }
            catch (Exception ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Exception : Wait for id gone failed with given id and timeout:: {id}, {waitingTime}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"WaitForIdGone({id}, {time}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Wait given text(exact text) is appeared on screen until given timeout (seconds)")]
        [KeywordDisplayName("Wait For Text")]
        [KeywordParameters("text", "Wait to appeared specific text(exact text)")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("Oxpd.Wait For Text (Email,20)")]
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
                while (!_oxpdEngine.WaitForHtmlContains(text, TimeSpan.FromMilliseconds(500)))
                {
                    if (DateTime.Now > endTime)
                    {
                        waitingTime = DateTime.Now.Subtract(startTime);
                        kr.Result = KeywordResults.Fail;
                        kr.Output = $"Wait for text failed with given text and timeout:: {text}, {waitingTime}";
                        kr.ScreenShot = CaptureScreen();
                        return kr;
                    }
                }

                string elementSelectionScript = $"getElementbyText('{text}')";

                decimal zoom;
                try
                {
                    zoom = _oxpdEngine.GetZoomRequired(elementSelectionScript);
                }
                catch (JavaScriptExecutionException ex)
                {
                    Logger.Error($"Inject the backing script and then run again.", ex);
                    _oxpdEngine.ExecuteJavaScript(OxpdResource.OxpdJavaScript);
                    zoom = _oxpdEngine.GetZoomRequired(elementSelectionScript);
                }

                string boundingArea = _oxpdEngine.ExecuteJavaScript($"{elementSelectionScript}.getBoundingClientRect()");

                while (IsOutOfView(_oxpdEngine.ParseBoundingArea(boundingArea, zoom)))
                {
                    boundingArea = _oxpdEngine.ExecuteJavaScript($"{elementSelectionScript}.getBoundingClientRect()");

                    if (DateTime.Now > endTime)
                    {
                        waitingTime = DateTime.Now.Subtract(startTime);
                        Logger.Error($"Given text is out of view.");
                        kr.Result = KeywordResults.Fail;
                        kr.Output = $"Wait for text failed with given time :: text is out of view. text = {text}, {waitingTime}";
                        kr.ScreenShot = CaptureScreen();
                        return kr;
                    }
                }

                waitingTime = DateTime.Now.Subtract(startTime);
                Logger.Debug($"Waiting time for text: {waitingTime} sec");
                kr.Output = $"Waiting time: {waitingTime}";
                return kr;
            }
            catch (Exception ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Exception : Wait for text failed with given text and timeout:: {text}, {waitingTime}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"WaitForText({text}, {time}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Wait OXPd browser HTML to contain the given text(partial text) until given timeout (seconds)")]
        [KeywordDisplayName("Wait For Text Contains")]
        [KeywordParameters("text", "Wait to appeared specific text(partial text) on OXPd browser HTML")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("Oxpd.Wait For Text Contains (mail,20)")]
        public KeywordResult WaitForTextContains(string text, string time)
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
                while (!_oxpdEngine.WaitForHtmlContains(text, TimeSpan.FromMilliseconds(500)))
                {
                    if (DateTime.Now > endTime)
                    {
                        waitingTime = DateTime.Now.Subtract(startTime);
                        kr.Result = KeywordResults.Fail;
                        kr.Output = $"Wait for text contains failed with given text and timeout:: {text}, {waitingTime}";
                        kr.ScreenShot = CaptureScreen();
                        return kr;
                    }
                }

                waitingTime = DateTime.Now.Subtract(startTime);
                Logger.Debug($"Waiting time for text: {waitingTime} sec");
                kr.Output = $"Waiting time: {waitingTime}";
                return kr;
            }
            catch (Exception ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Exception : Wait for text contains failed with given text and timeout:: {text}, {waitingTime}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"WaitForTextContains({text}, {time}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Wait given text is disappeared on screen until given timeout (seconds)")]
        [KeywordDisplayName("Wait For Text Gone")]
        [KeywordParameters("text", "Wait to disappeared specific text")]
        [KeywordParameters("time", "Time to wait")]
        [SampleScript("Oxpd.Wait For Text Gone (Email,20)")]
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
                while (_oxpdEngine.WaitForHtmlContains(text, TimeSpan.FromMilliseconds(1)))
                {
                    if (DateTime.Now > endTime)
                    {
                        waitingTime = DateTime.Now.Subtract(startTime);
                        kr.Result = KeywordResults.Fail;
                        kr.Output = $"Wait for text gone failed with given text and timeout:: {text}, {waitingTime}";
                        kr.ScreenShot = CaptureScreen();
                        return kr;
                    }

                    string elementSelectionScript = $"getElementbyText('{text}')";
                    decimal zoom = _oxpdEngine.GetZoomRequired(elementSelectionScript);
                    string boundingArea = _oxpdEngine.ExecuteJavaScript($"{elementSelectionScript}.getBoundingClientRect()");

                    if (IsOutOfView(_oxpdEngine.ParseBoundingArea(boundingArea, zoom)))
                    {
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
            catch (Exception ex)
            {
                waitingTime = DateTime.Now - startTime;
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Wait for text gone failed with given text and timeout:: {text}, {waitingTime}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"WaitForTextGone({text}, {time}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Wait given xpath is appeared on screen until given timeout (seconds)")]
        [KeywordDisplayName("Wait For Xpath")]
        [KeywordParameters("xpath", "Xpath to wait")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("Oxpd.Wait For Xpath (//*[@id=\"stay_button\"],20)")]
        public KeywordResult WaitForXpath(string xpath, string time)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            kr.Result = KeywordResults.Pass;
            int Time = Convert.ToInt32(time);
            Time = Time * _scaleFactor;
            DateTime startTime = DateTime.Now;
            TimeSpan waitingTime;

            try
            {
                if (!Wait.ForTrue(() => _oxpdEngine.ExecuteFunction($"getIsExistElementbyXpath", xpath).Trim('"') == "true", TimeSpan.FromSeconds(Time)))
                {
                    waitingTime = DateTime.Now.Subtract(startTime);
                    Logger.Error($"Fail to wait xpath on the UI: {xpath}");
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Wait for xpath failed with given xpath and timeout:: {xpath}, {waitingTime}";
                    return kr;
                }

                string elementSelectionScript = $"document.evaluate('{xpath}', document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue";
                decimal zoom = _oxpdEngine.GetZoomRequired(elementSelectionScript);
                string boundingArea = _oxpdEngine.ExecuteJavaScript($"{elementSelectionScript}.getBoundingClientRect()");

                if (IsOutOfView(_oxpdEngine.ParseBoundingArea(boundingArea, zoom)))
                {
                    waitingTime = DateTime.Now.Subtract(startTime);
                    Logger.Error($"Given xpath is out of view.");
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Wait for xpath failed with given time :: xpath is out of view. xpath = {xpath} time = {waitingTime}";
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
                kr.Output = $"Wait for xpath failed with given time :: {ex.Data} xpath = {xpath} time = {time}";
                Logger.Error($"WaitForXpath({xpath}({time})) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Wait given xpath is disappeared on screen until given timeout (seconds)")]
        [KeywordDisplayName("Wait For Xpath Gone")]
        [KeywordParameters("xpath", "Wait to disappeared specific xpath")]
        [KeywordParameters("time", "Time to wait (seconds)")]
        [SampleScript("Oxpd.Wait For Xpath Gone (//*[@id=\"stay_button\"],20)")]
        public KeywordResult WaitForXpathGone(string xpath, string time)
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

                if (!Wait.ForTrue(() => _oxpdEngine.ExecuteFunction($"getIsExistElementbyXpath", xpath).Trim('"') == "false", TimeSpan.FromSeconds(Time)))
                {
                    waitingTime = DateTime.Now.Subtract(startTime);
                    Logger.Error($"Wait for xpath gone failed with given xpath on the UI: {xpath}");
                    kr.Result = KeywordResults.Fail;
                    kr.Output = $"Wait for xpath gone failed with given xpath and timeout:: {xpath}, {waitingTime}";
                    return kr;
                }

                waitingTime = DateTime.Now.Subtract(startTime);
                Logger.Debug($"Waiting time for xpath: {waitingTime} sec");
                kr.Output = $"Waiting time: {waitingTime}";
                return kr;
            }
            catch (Exception ex)
            {
                waitingTime = DateTime.Now.Subtract(startTime);
                kr.Result = KeywordResults.Error;
                kr.AdditionalInfo = ex.ToString();
                kr.Output = $"Exception : Wait for xpath gone failed with given css and timeout:: {xpath}, {waitingTime}";
                kr.ScreenShot = CaptureScreen();
                Logger.Error($"WaitForXpathGone({xpath}, {time}) fail", ex);
                return kr;
            }
        }

        [KeywordDescription("Input Text to current position")]
        [KeywordDisplayName("Input Text")]
        [KeywordParameters("text", "Text to Input")]
        [SampleScript("Oxpd.Input Text (henry.k@hp.com)")]
        public KeywordResult InputText(string text)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            string KeyboardId = "#hpid-keyboard";

            try
            {
                if (!_device.ControlPanel.WaitForState(KeyboardId, OmniElementState.Useable, TimeSpan.FromSeconds(3 * _scaleFactor)))
                {
                    Logger.Error($"Fail to find keyboard on the UI: Keyboard not displayed");
                    kr.Result = KeywordResults.Error;
                    kr.ScreenShot = CaptureScreen();
                    kr.Output = $"Can not find keyboard in the screen";
                    return kr;
                }

                try
                {
                    _device.ControlPanel.Type(text);
                }
                catch
                {
                    _device.ControlPanel.TypeOnNumericKeypad(text);
                }

                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Input text failed with given text :: {text}";
                Logger.Error($"InputText({text}) fail", ex);
                return kr;
            }
        }

        [GetKeyword]
        [KeywordDescription("Get text of given id and store to given variable")]
        [KeywordDisplayName("Get Text")]
        [KeywordParameters("id", "id of element")]
        [KeywordParameters("saveTo", "variable name (ex. ${Buffer}) to store value")]
        [SampleScript("Oxpd.Get Text (stay_button,${Variable})")]
        public KeywordResult GetText(string id, string saveTo)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            id = Support.Utils.GetVariablevalueIfExist(id);
            try
            {
                string script = $"document.getElementById('{id}').innerText";
                string innerText = _oxpdEngine.ExecuteJavaScript(script);

                CommonExecutionInfo.SetVariable(saveTo, innerText);
                kr = new KeywordResult(KeywordResults.Pass);
                kr.Output = $"{saveTo} : {innerText}";
                return kr;
            }
            catch (Exception ex)
            {
                kr.Result = KeywordResults.Fail;
                kr.AdditionalInfo = ex.ToString();
                kr.ScreenShot = CaptureScreen();
                kr.Output = $"Get text failed with given id :: {id}";
                Logger.Error($"Get Text({id}) fail", ex);
                return kr;
            }
        }
    }
}

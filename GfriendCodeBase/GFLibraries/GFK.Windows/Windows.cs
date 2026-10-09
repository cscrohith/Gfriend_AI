using HP.GFriend.GFLogger;
using HP.GFriend.Utils.Charter;
using HP.GFriend.Utils.UIAManaged;
using HP.GFriend.Utils.Vision;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Printing;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Forms;
using WPath;
using static HP.GFriend.Keywords.MouseOperations;

namespace HP.GFriend.Keywords
{

    public class Windows : IGFLibrary
    {
        public AutomationElement _target;
        private ImageProcessing _visionEngine;

        private ImageConverter _converter = null;
        private static string _outputDir;
        private int _scaleFactor = 1;

        private GFVision _vision;

        public Rect ScreenBound;
        private static MemoryUsageItem _memoryUsage = null;
        public string _testCaseName = "";

        #region ScreenSaver
        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
            // Legacy flag, should not be used.
            // ES_USER_PRESENT = 0x00000004
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern IntPtr SetFocus(HandleRef hWnd);

        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private void PreventScreenSaver(bool enable)
        {
            if (enable)
            {
                SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
            }
            else
            {
                SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
            }
        }
        #endregion
        public Windows()
        {

        }
        public void Dispose()
        {
            PreventScreenSaver(false);
        }

        public List<string> GetDependencies()
        {
            return null;
        }

        public string GetName()
        {
            return "Windows";
        }

        public bool DutUsed()
        {
            return false;
        }
        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        public enum DeviceCap
        {
            VERTRES = 10,
            DESKTOPVERTRES = 117,
        }
        public void Initialize(DeviceUnderTest dut, string outputDir)
        {
            // Windows library doesn't require a DUT - it operates on the local machine
            if (dut != null)
            {
                Logger.Debug($"Windows library initialized with DUT: {dut.DeviceId ?? "Unknown"} at {dut.DeviceAddress ?? "N/A"}");
            }
            else
            {
                Logger.Debug("Windows library initialized without DUT (local machine execution)");
            }

            _converter = new ImageConverter();
            _outputDir = outputDir;
            int width = 0;
            int height = 0;
            foreach (Screen screen in Screen.AllScreens)
            {
                width += screen.Bounds.Width;
                height = Math.Max(height, screen.Bounds.Height);
            }
            ScreenBound = new Rect(0, 0, width, height);
            CommonExecutionInfo.SetSharedObject("windows", this);
            Logger.Trace("Prevent PC to sleep");
            PreventScreenSaver(true);
        }


        #region General Methods

        public string GetOutputDir()
        {
            return _outputDir;
        }
        private Rect FullScreenBound()
        {
            Graphics graphics = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr desktop = graphics.GetHdc();
            int LogicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
            int PhysicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);
            float ScreenScalingFactor = (float)PhysicalScreenHeight / (float)LogicalScreenHeight;

            Rect fullScreenBound = new Rect(0, 0, ScreenBound.Width * ScreenScalingFactor, ScreenBound.Height * ScreenScalingFactor);
            return fullScreenBound;
        }

        public AutomationElement SelectElement(AutomationElement rootElement, Condition condition, TimeSpan timeOut, TreeScope scope = TreeScope.Subtree)
        {
            AutomationElement target = null;
            DateTime endTime = DateTime.Now.AddMilliseconds(timeOut.TotalMilliseconds * _scaleFactor);

            while (true)
            {
                try
                {
                    Logger.Debug($"Finding target in {rootElement?.Current.Name ?? "Nothing"}");
                    if (rootElement == null) { throw new ElementNotAvailableException(); }
                    target = rootElement.FindFirst(scope, condition);

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
                catch (ElementNotAvailableException)
                {
                    Logger.Debug("Missing target. Get focused");
                    int pid = AutomationElement.FocusedElement.Current.ProcessId;
                    rootElement = SelectElement(AutomationElement.RootElement, AutomationElement.ProcessIdProperty, pid, TimeSpan.FromSeconds(5));
                }
                catch (Exception)
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

        public AutomationElement SelectElementByWPath(AutomationElement rootElement, string wPath, TimeSpan timeOut, TreeScope scope = TreeScope.Subtree)
        {
            AutomationElement target = null;
            DateTime endTime = DateTime.Now.AddMilliseconds(timeOut.TotalMilliseconds * _scaleFactor);

            while (true)
            {
                try
                {
                    Logger.Debug($"Finding target in {rootElement?.Current.Name ?? "Nothing"}");
                    if (rootElement == null) { throw new ElementNotAvailableException(); }
                    target = rootElement.FindByWPath(wPath);

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
                catch (ElementNotAvailableException)
                {
                    Logger.Debug("Missing target. Get focused");
                    int pid = AutomationElement.FocusedElement.Current.ProcessId;
                    rootElement = SelectElement(AutomationElement.RootElement, AutomationElement.ProcessIdProperty, pid, TimeSpan.FromSeconds(5));
                }
                catch (Exception)
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

        public AutomationElementCollection SelectElements(AutomationElement rootElement, AutomationProperty prop, object value, TimeSpan timeOut)
        {
            return SelectElements(rootElement, new PropertyCondition(prop, value), timeOut);
        }

        public AutomationElementCollection SelectElements(AutomationElement rootElement, Condition condition, TimeSpan timeOut, TreeScope scope = TreeScope.Subtree)
        {
            AutomationElementCollection target = null;
            DateTime endTime = DateTime.Now.AddMilliseconds(timeOut.TotalMilliseconds * _scaleFactor);

            while (true)
            {
                try
                {
                    Logger.Debug($"Finding target in {rootElement?.Current.Name ?? "Nothing"}");
                    if (rootElement == null) { throw new ElementNotAvailableException(); }
                    target = rootElement.FindAll(scope, condition);


                    if (target != null && target.Count != 0)
                    {
                        break;
                    }


                    Thread.Sleep(500);
                    if (DateTime.Now > endTime)
                    {
                        return null;
                    }

                }
                catch (ElementNotAvailableException)
                {
                    Logger.Debug("Missing target. Get focused");
                    int pid = AutomationElement.FocusedElement.Current.ProcessId;
                    rootElement = SelectElement(AutomationElement.RootElement, AutomationElement.ProcessIdProperty, pid, TimeSpan.FromSeconds(5));
                }
                catch (Exception)
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

        public AutomationElement SelectElement(AutomationElement rootElement, AutomationProperty prop, object value, TimeSpan timeOut)
        {
            return SelectElement(rootElement, new PropertyCondition(prop, value), timeOut);
        }

        public AutomationElement FindApplication(string windowTitle, TimeSpan timeOut, AutomationElement rootElement = null)
        {
            DateTime endTime = DateTime.Now.AddMilliseconds(timeOut.TotalMilliseconds * _scaleFactor);
            AutomationElement target;
            do
            {
                target = FindApplication(windowTitle, rootElement);
                if (target != null)
                {
                    return target;
                }

                Thread.Sleep(500);
            } while (DateTime.Now < endTime);

            return null;
        }

        public AutomationElement FindApplication(string windowTitle, AutomationElement rootElement = null)
        {

            TreeWalker tw = TreeWalker.ControlViewWalker;
            if (rootElement == null)
            {
                rootElement = AutomationElement.RootElement;
            }
            AutomationElement window = tw.GetFirstChild(rootElement);

            while (window != null)
            {
                Logger.Debug($"Checking : {window.Current.Name}");
                if (window.Current.Name.Contains(windowTitle))
                {
                    Logger.Debug("Return");
                    return window;
                }
                Task getNext = Task.Run(() => window = tw.GetNextSibling(window));
                getNext.Wait(3000);
                if (getNext.IsCanceled)
                {
                    Logger.Debug("Canceled");
                }
            }

            if (window == null)
            {
                if (rootElement == null)
                {
                    rootElement = AutomationElement.RootElement;
                }

                Condition condition = new PropertyCondition(AutomationElement.NameProperty, windowTitle);

                // Use FindAll to get all matching windows
                AutomationElementCollection windows = rootElement.FindAll(TreeScope.Children, condition);

                foreach (AutomationElement currentWindow in windows)
                {
                    window = currentWindow;
                    Logger.Debug("Child Window : " + currentWindow.Current.Name);
                    if (currentWindow.Current.Name.Contains(windowTitle) &&
                        currentWindow.Current.IsEnabled &&
                        currentWindow.Current.IsOffscreen == false) // Ensures we get visible windows
                    {
                        return currentWindow;
                    }
                }
            }

            if (window == null)
            {
                if (rootElement == null)
                {
                    rootElement = AutomationElement.RootElement;
                }

                Condition condition = new PropertyCondition(AutomationElement.NameProperty, windowTitle);

                // Use FindAll to get all matching windows
                AutomationElementCollection windows = rootElement.FindAll(TreeScope.Descendants, condition);

                foreach (AutomationElement currentWindow in windows)
                {
                    Logger.Debug("Descendant Window : " + currentWindow.Current.Name);
                    if (currentWindow.Current.Name.Contains(windowTitle) &&
                        currentWindow.Current.IsEnabled &&
                        currentWindow.Current.IsOffscreen == false) // Ensures we get visible windows
                    {
                        return currentWindow;
                    }
                }
            }

            Logger.Debug("Null return");
            return null;

        }

        public bool WaitForEndMove(AutomationElement target)
        {
            Rect boundBefore = (Rect)(target.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty));
            DateTime endTime = DateTime.Now.AddSeconds(5);
            while (true)
            {
                Thread.Sleep(500);
                Rect boundNow = (Rect)(target.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty));
                if (boundBefore.Equals(boundNow))
                {
                    target.SetFocus();
                    return true;
                }
                boundBefore = boundNow;
                if (DateTime.Now > endTime)
                {
                    return false;
                }
            }


        }
        public bool ClickElement(AutomationElement target)
        {
            try
            {
                object pattern = null;
                if (target.TryGetCurrentPattern(InvokePattern.Pattern, out pattern))
                {
                    InvokePattern invokePattern = pattern as InvokePattern;
                    Logger.Trace("Invoke Pattern");

                    Rect bound = (Rect)(target.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty));

                    Automation.AddAutomationEventHandler(InvokePattern.InvokedEvent, target, TreeScope.Element,
               new AutomationEventHandler(OnUIAutomationEvent));
                    try
                    {
                        invokePattern.Invoke();
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(1000);
                        invokePattern.Invoke();
                    }


                    //((InvokePattern)pattern).Invoke();
                    return true;
                }
                else if (target.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out pattern))
                {
                    Logger.Trace("Expand/Collapse Pattern");
                    ExpandCollapseState expandState = ((ExpandCollapsePattern)pattern).Current.ExpandCollapseState;

                    if (expandState == ExpandCollapseState.Expanded || expandState == ExpandCollapseState.PartiallyExpanded)
                    {
                        Logger.Trace("Collapse");
                        ((ExpandCollapsePattern)pattern).Collapse();
                    }
                    else
                    {
                        Logger.Trace("Expand");
                        ((ExpandCollapsePattern)pattern).Expand();
                    }

                    return true;
                }
                else if (target.TryGetCurrentPattern(SelectionItemPattern.Pattern, out pattern))
                {
                    Logger.Trace("Select Pattern");
                    ((SelectionItemPattern)pattern).Select();
                    return true;
                }
                else if (target.TryGetCurrentPattern(TogglePattern.Pattern, out pattern))
                {
                    Logger.Trace("Toggle Pattern");
                    ((TogglePattern)pattern).Toggle();
                    return true;
                }
                else
                {
                    Logger.Trace("Can not find pattern");
                }
                return false;

            }
            catch (ElementNotEnabledException ex)
            {
                Logger.Error("Click : Element is not enalbed", ex);
                return false;
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("Click : Invalid operation", ex);
                return false;
            }
        }

        private void OnUIAutomationEvent(object sender, AutomationEventArgs e)
        {
            Logger.Debug($"UI Automation Event : {e.EventId.ProgrammaticName}");
        }

        public bool SelectItem(AutomationElement parent, Condition itemCondition, bool click = false, bool selectWithParent = false, bool invoke = false)
        {
            if (click && !ClickElement(parent))
            {
                return false;
            }

            AutomationElement item = SelectElement(parent, itemCondition, TimeSpan.FromSeconds(2));
            if (item == null)
            {
                return false;
            }

            if (selectWithParent)
            {

                TreeWalker tWalker = TreeWalker.ControlViewWalker;
                item = tWalker.GetParent(item);
            }

            try
            {
                object pattern = null;
                if (invoke)
                {
                    if (item.TryGetCurrentPattern(InvokePattern.Pattern, out pattern))
                    {
                        ((InvokePattern)pattern).Invoke();
                        return true;
                    }
                    return false;
                }
                if (item.TryGetCurrentPattern(SelectionItemPattern.Pattern, out pattern))
                {
                    ((SelectionItemPattern)pattern).Select();
                    ClickElement(parent);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Error during select item", ex);
                return false;
            }
        }

        public string GetValue(AutomationElement toGet)
        {
            object valuePattern = null;
            if (!toGet.TryGetCurrentPattern(ValuePattern.Pattern, out valuePattern))
            {
                Logger.Error("Can not get ValuePattern");
                return null;
            }
            return ((ValuePattern)valuePattern).Current.Value;
        }

        public string GetText(AutomationElement toGet)
        {
            object patternObj;
            if (toGet.TryGetCurrentPattern(ValuePattern.Pattern, out patternObj))
            {
                var valuePattern = (ValuePattern)patternObj;
                return valuePattern.Current.Value;
            }
            else if (toGet.TryGetCurrentPattern(TextPattern.Pattern, out patternObj))
            {
                var textPattern = (TextPattern)patternObj;
                return textPattern.DocumentRange.GetText(-1).TrimEnd('\r'); // often there is an extra '\r' hanging off the end.
            }
            else
            {
                return toGet.Current.Name;
            }
        }

        public bool SetText(AutomationElement toSet, string textToSet)
        {
            try
            {
                object valuePattern = null;
                if (!toSet.TryGetCurrentPattern(ValuePattern.Pattern, out valuePattern))
                {
                    Logger.Trace("Set Text using send key");
                    toSet.SetFocus();
                    Thread.Sleep(100);
                    SendKeys.SendWait("^{HOME}");   // Move to start of control
                    SendKeys.SendWait("^+{END}");   // Select everything
                    SendKeys.SendWait("{DEL}");     // Delete selection
                    SendKeys.SendWait(textToSet);
                    return true;
                }
                else
                {
                    Logger.Trace("Set Text using value pattern");
                    ((ValuePattern)valuePattern).SetValue(textToSet);
                    return true;
                }

            }
            catch (ElementNotEnabledException ex)
            {
                Logger.Error("SetText : Element is not enalbed", ex);
                return false;

            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("SetText : Invalid Operation", ex);
                return false;

            }
            catch (Exception ex)
            {
                Logger.Error("SetText : Unhandled exception", ex);
                return false;
            }
        }

        public byte[] GetScreenShot(Rect bound)
        {
            try
            {
                Bitmap bitmap = new Bitmap((int)bound.Width, (int)bound.Height);
                Graphics graphics = Graphics.FromImage(bitmap);
                graphics.CopyFromScreen((int)bound.X, (int)bound.Y, 0, 0, new System.Drawing.Size((int)bound.Width, (int)bound.Height));
                return (byte[])_converter.ConvertTo(bitmap, typeof(byte[]));
            }

            catch (Exception ex)
            {
                Logger.Error("Error during capture", ex);
                return null;
            }
        }



        public byte[] GetScreenShot()
        {
            if (_target != null)
            {
                try
                {
                    Rect bound = (Rect)(_target.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty));
                    return GetScreenShot(bound);
                }
                catch (Exception) { }
            }

            return GetScreenShot(ScreenBound);
        }

        public void MouseLeftClick(MousePoint pt)
        {
            MouseOperations.SetCursorPosition(pt);
            MouseOperations.MouseEvent(MouseEventFlags.LeftDown);
            Thread.Sleep(100);
            MouseOperations.MouseEvent(MouseEventFlags.LeftUp);
        }

        public void MouseRightClick(MousePoint pt)
        {
            MouseOperations.SetCursorPosition(pt);
            MouseOperations.MouseEvent(MouseEventFlags.RightDown);
            Thread.Sleep(100);
            MouseOperations.MouseEvent(MouseEventFlags.RightUp);
        }

        public MousePoint GetClickablePoint(AutomationElement element)
        {
            Rect bounds = element.Current.BoundingRectangle;

            double x = bounds.X + bounds.Width / 2;
            double y = bounds.Y + bounds.Height / 2;
            return new MousePoint((int)x, (int)y);
        }

        #endregion

        #region Keywords

        [KeywordDescription("Switch To App")]
        [KeywordDisplayName("SwitchToApp")]
        [KeywordParameters("appName", "processor name of the app to switch")]
        [SampleScript("Windows.SwitchToApp (Chrome)")]
        public KeywordResult SwitchToApp(string appName)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);

            Process[] procList = System.Diagnostics.Process.GetProcessesByName(appName);

            if (procList.Length == 0)
            {
                result.Result = KeywordResults.Fail;
                result.AdditionalInfo = "No such process exists";
                return result;
            }
            foreach (Process p in procList)
            {
                IntPtr windowHandle = p.MainWindowHandle;
                SetForegroundWindow(p.MainWindowHandle);
                ShowWindow(windowHandle, 3);
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Set timeout scacle for applying all keywords in library.\r\n" +
            "This scale facor will be multiplied of all timeout arguments in keywords.\r\n" +
            "For example, if scale factor is set to 2 and call wait for object for 3 secondes, it will wait for 6 (3 x 2) seconds.")]
        [KeywordDisplayName("Set Timeout Scale")]
        [KeywordParameters("scaleFactor", "scale factor. Should be a number and larger than 0")]
        [SampleScript("Windows.Set Timeout Scale (3)")]
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

        [KeywordDescription("Select applicationto control which window title contains given windowTitle")]
        [KeywordDisplayName("Select Application")]
        [KeywordParameters("windowTitle", "Title of the applciation want to control")]
        [SampleScript("Windows.Select Application(Notepad)")]
        public KeywordResult SelectApplication(string windowTitle)
        {
            return SelectApplication(windowTitle, "5");
        }

        [KeywordDescription("Select applicationto control which window title contains given windowTitle")]
        [KeywordDisplayName("Select Application")]
        [KeywordParameters("windowTitle", "Title of the applciation want to control")]
        [KeywordParameters("timeOut", "Finding timeout")]
        [SampleScript("Windows.Select Application (HP Scan,10)")]
        public KeywordResult SelectApplication(string windowTitle, string timeOut)
        {
            TimeSpan timeoutTimeSpan = TimeSpan.FromSeconds(int.Parse(timeOut.Trim()));
            _target = FindApplication(windowTitle, timeoutTimeSpan);
            if (_target == null)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "Can not find target application";
                fail.ScreenShot = GetScreenShot(FullScreenBound());
                return fail;
            }
            try
            {
                _target.SetFocus();
            }
            catch (Exception) { }

            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Select child window of current target which window title of given windowTitle")]
        [KeywordDisplayName("Select Child Window With Title")]
        [KeywordParameters("windowTitle", "Title of the applciation want to control (should be exact same)")]
        [SampleScript("Windows.Select Child Window With Title (Device List)")]
        public KeywordResult SelectChildWindowWithTitle(string windowTitle)
        {
            return SelectChildWindowWithTitle(windowTitle, "5");
        }

        [KeywordDescription("Select child window of current target which window title of given windowTitle")]
        [KeywordDisplayName("Select Child Window With Title")]
        [KeywordParameters("windowTitle", "Title of the applciation want to control (should be exact same)")]
        [KeywordParameters("timeOut", "Finding timeout")]
        [SampleScript("Windows.Select Child Window With Title (Device List,10)")]
        public KeywordResult SelectChildWindowWithTitle(string windowTitle, string timeOut)
        {
            TimeSpan timeoutTimeSpan = TimeSpan.FromSeconds(int.Parse(timeOut.Trim()));
            PropertyCondition windowsPropertyCondition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window);
            PropertyCondition namePropertyCondition = new PropertyCondition(AutomationElement.NameProperty, windowTitle);
            AndCondition condition = new AndCondition(windowsPropertyCondition, namePropertyCondition);
            _target = SelectElement(_target, condition, timeoutTimeSpan);

            if (_target == null)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "Can not find target application";
                fail.ScreenShot = GetScreenShot(FullScreenBound());
                return fail;
            }

            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Maximize window of current target application")]
        [KeywordDisplayName("Maximize Window")]
        [SampleScript("Windows.Maximize Window")]
        public KeywordResult MaximizeWindow()
        {
            object pattern;
            if (!_target.TryGetCurrentPattern(WindowPattern.Pattern, out pattern))
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail, "Can not get Window pattern. Check if current target is Window object.");
                return fail;
            }
            try
            {
                WindowPattern wPattern = pattern as WindowPattern;
                wPattern.SetWindowVisualState(WindowVisualState.Maximized);
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Error during maximize");
                error.AdditionalInfo = ex.ToString();
                Logger.Error(error.Output, ex);
                return error;
            }
            return new KeywordResult(KeywordResults.Pass);

        }

        [KeywordDescription("Move Window to (x,y)")]
        [KeywordDisplayName("Move Window")]
        [KeywordParameters("x", "x Coordinate to move. Give negative value(e.g. -1) to keep current position.")]
        [KeywordParameters("y", "y Coordinate to move. Give negative value(e.g. -1) to keep current position.")]
        [SampleScript("Windows.Move Window (3,80)")]
        public KeywordResult MoveWindow(string x, string y)
        {
            object pattern;

            if (!double.TryParse(x, out double dPosX) || !double.TryParse(y, out double dPosY))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "PosX and PosY should be number");
                return error;
            }

            if (!_target.TryGetCurrentPattern(TransformPattern.Pattern, out pattern))
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail, "Window is not movealbe.");
                return fail;
            }
            try
            {
                Rect boundBefore = (Rect)(_target.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty));
                if (dPosX < 0)
                {
                    dPosX = boundBefore.X;
                }
                if (dPosY < 0)
                {
                    dPosY = boundBefore.Y;
                }

                TransformPattern tPattern = pattern as TransformPattern;
                tPattern.Move(dPosX, dPosY);
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error, "Error during move window");
                error.AdditionalInfo = ex.ToString();
                Logger.Error(error.Output, ex);
                return error;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [GetKeyword]
        [KeywordDescription("Gets the Enviornment Variable and sets it to the given variable")]
        [KeywordDisplayName("Get Enviornment Variable")]
        [KeywordParameters("enviornmentVariable", "Name of the enviornment variable to get")]
        [KeywordParameters("saveTo", "variable name (ex. ${Buffer}) to store value")]
        [SampleScript("Windows.Get Enviornment Variable (java,${ex})")]
        public KeywordResult GetEnviornmentVariable(string enviornmentVariable, string saveTo)
        {
            enviornmentVariable = Support.Utils.GetVariablevalueIfExist(enviornmentVariable);
            string envValue = System.Environment.GetEnvironmentVariable(enviornmentVariable);
            CommonExecutionInfo.SetVariable(saveTo, envValue);
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            result.Output = $"{saveTo} : {envValue}";
            return result;
        }

        [KeywordDescription("Add printer with given driver")]
        [KeywordDisplayName("Add Printer")]
        [KeywordParameters("printerName", "Name of Printer")]
        [KeywordParameters("driverPath", "Path of driver .inf file")]
        [KeywordParameters("driverName", "Driver name of given driver which is defined in inf file")]
        [KeywordParameters("deviceAddress", "IP address of the device")]
        [SampleScript("Windows.Add Printer (Camden,C:\\Users\\XAppanna\\Desktop\\UPD Driver\\7.0.1.24923\\hpbuio200l.inf,HP Universal Printing PCL 6 (v7.0.1),146.205.5.116)")]
        public KeywordResult AddPrinter(string printerName, string driverPath, string driverName, string deviceAddress)
        {
            PrinterDriverUtils.PrinterInstallationStatus status = PrinterDriverUtils.AddPrinter(printerName, deviceAddress, driverPath, driverName);
            if (status.Success)
            {
                return new KeywordResult(KeywordResults.Pass);
            }
            else if (status.Ex != null)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = status.Message;
                error.AdditionalInfo = status.Ex.ToString();
                return error;
            }
            else
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = status.Message;
                return fail;
            }
        }

        [KeywordDescription("Add printer with given driver")]
        [KeywordDisplayName("Add Local Printer")]
        [KeywordParameters("printerName", "Name of Printer")]
        [KeywordParameters("portName", "IP address of the device")]
        [KeywordParameters("driverPath", "Path of driver .inf file")]
        [KeywordParameters("driverName", "Driver name of given driver which is defined in inf file")]
        [SampleScript("Windows.Add Local Printer (Camden,146.205.5.116,C:\\Users\\XAppanna\\Desktop\\UPD Driver\\7.0.1.24923\\hpbuio200l.inf,HP Universal Printing PCL 6 (v7.0.1))")]
        public KeywordResult AddLocalPrinter(string printerName, string portName, string driverPath, string driverName)
        {
            PrinterDriverUtils.PrinterInstallationStatus status = PrinterDriverUtils.AddLocalPrinter(printerName, portName, driverPath, driverName);
            if (status.Success)
            {
                return new KeywordResult(KeywordResults.Pass);
            }
            else if (status.Ex != null)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = status.Message;
                error.AdditionalInfo = status.Ex.ToString();
                return error;
            }
            else
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = status.Message;
                return fail;
            }
        }

        [KeywordDescription("Remove printer from system")]
        [KeywordDisplayName("Remove Printer")]
        [KeywordParameters("printerName", "Name of printer to delete")]
        [SampleScript("Windows.Remove Printer (Camden)")]
        public KeywordResult RemovePrinter(string printerName)
        {
            try
            {
                if (!PrinterDriverUtils.DeletePrinter(printerName))
                {
                    KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                    fail.Output = "Delete printer driver fail";
                    return fail;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error during delete printer", ex);
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error during delete printer";
                error.AdditionalInfo = ex.ToString();
                return error;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Open application and take control with given executable path")]
        [KeywordDisplayName("Open With Path")]
        [KeywordParameters("ExecutablePath", "Executable(*.exe) path to execute and take control")]
        [SampleScript("Windows.Open With Path(C:\\Program Files\\Microsoft Office\\root\\Office16\\WINWORD.EXE)")]
        public KeywordResult OpenWithPath(string executablePath)
        {
            return OpenWithPath(executablePath, "2");
        }

        [KeywordDescription("Open application and take control with given executable path")]
        [KeywordDisplayName("Open With Path")]
        [KeywordParameters("ExecutablePath", "Executable(*.exe) path to execute and take control")]
        [KeywordParameters("WaitTime", "Time(seconds) to wait until application is opened.")]
        [SampleScript("Windows.Open With Path (C:\\Program Files\\Microsoft Office\\root\\Office16\\WINWORD.EXE,10)")]
        public KeywordResult OpenWithPath(string executablePath, string waitTime)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            int waitSec = int.Parse(waitTime.Trim());
            waitSec = waitSec * _scaleFactor;
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.FileName = executablePath; ;
                Process process = new Process();
                process.StartInfo = processStartInfo;
                process.Start();
                Thread.Sleep(2000);
                int pid = process.Id;
                Logger.Trace($"Process is started with {executablePath} and Process Id is {pid}");
                Thread.Sleep(TimeSpan.FromSeconds(waitSec));


                pid = AutomationElement.FocusedElement.Current.ProcessId;
                Logger.Trace($"Current focused process id : {pid}");

                _target = SelectElement(AutomationElement.RootElement, AutomationElement.ProcessIdProperty, pid, TimeSpan.FromSeconds(5));
                if (_target == null)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = "Fail to get control";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }

                result.Output = $"Process is started with {executablePath} and Process Id is {pid}";
                return result;

            }
            catch (Exception ex)
            {
                Logger.Error("Error during open program.", ex);
                result.Result = KeywordResults.Error;
                result.Output = "Error during open program";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                return result;
            }
        }

        [KeywordDescription("Open document and then open printer properties windows of given printer name\r\nApplication should be installed before run this keyword\r\nSupported Applicaton: MS Word, MS PowerPoint, MS Excel and Acrobat Reader")]
        [KeywordDisplayName("Open File For Print")]
        [KeywordParameters("filePath", @"File path to print (ex. c:\testfiles\word2page.doc")]
        [KeywordParameters("printerName", "Printer to use")]
        [SampleScript("Windows.Open File For Print (C:\\SAG_Testfile\\WW2K6121.doc,Camden)")]
        public KeywordResult OpenFileForPrint(string filePath, string printerName)
        {
            filePath = Support.Utils.GetAbsolutePath(filePath, CommonExecutionInfo.ScriptFolder);
            if (!File.Exists(filePath))
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Can not find specified file : {filePath}";
                return fail;
            }
            return ApplicationUtils.OpenPrinterProperties(filePath, printerName, this);
        }

        [KeywordDescription("Close application.\r\nThis keyword MUST be called after Open File For Print")]
        [KeywordDisplayName("Close Application")]
        [SampleScript("Windows.Close Application")]
        public KeywordResult CloseApplication()
        {
            if (ApplicationUtils.ApplicationAutomationElement == null)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "No application to close";
                return fail;
            }
            if (ApplicationUtils.CloseApplication())
            {
                return new KeywordResult(KeywordResults.Pass);
            }
            KeywordResult error = new KeywordResult(KeywordResults.Error);
            error.Output = "Error during close";
            return error;
        }

        [KeywordDescription("Click print button in application. This keyword MUST be used with Open File For Print")]
        [KeywordDisplayName("Click Print Button")]
        [SampleScript("Windows.Click Print Button")]
        public KeywordResult ClickPrintButton()
        {
            try
            {
                if (ApplicationUtils.ClickPrintButton(this))
                {
                    return new KeywordResult(KeywordResults.Pass);
                }
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "Can not click print button";
                return fail;
            }
            catch (Exception ex)
            {
                Logger.Error("Error during click print button", ex);
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error during click print button";
                error.AdditionalInfo = ex.ToString();
                return error;
            }
        }

        [KeywordDescription("Select file at file open(save) dialog window which is currently opend.")]
        [KeywordDisplayName("Select File at Dialog")]
        [KeywordParameters("filePath", "Local file path to open(save)")]
        [SampleScript("Windows.Select File at Dialog(C:\\Users\\XAppanna\\Desktop\\Dune _Observations_.xlsx)")]
        public KeywordResult SelectFileAtDialog(string filePath)
        {
            WindowsManaged managed = new WindowsManaged(_outputDir);

            return managed.SelectFileAtDialog(filePath);
        }

        [GetKeyword]
        [KeywordDescription("Return Text of Element")]
        [KeywordDisplayName("Get Text")]
        [KeywordParameters("automationID", "automationid of object to wait")]
        [KeywordParameters("saveTo", "variable name to store")]
        [SampleScript("Windows.Get Text (SettingsPagePCSystemDisplay,${ex})")]
        public KeywordResult GetText(string automationId, string saveTo)
        {
            automationId = Support.Utils.GetVariablevalueIfExist(automationId);
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                AutomationElement toGet = SelectElement(_target, AutomationElement.AutomationIdProperty, automationId, TimeSpan.FromSeconds(3));

                if (toGet == null)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given automation ID : {automationId}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    Logger.Error(result.Output);
                    return result;
                }
                string value = GetText(toGet);
                CommonExecutionInfo.SetVariable(saveTo, value);
                result.Output = $"{saveTo} : {value}";
                return result;
            }
            catch (Exception ex)
            {
                result.Result = KeywordResults.Error;
                result.Output = $"Error during getting text : {automationId}";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                Logger.Error(result.Output, ex);
                return result;
            }

        }

        [KeywordDescription("Wait object to be shown until given timeout")]
        [KeywordDisplayName("Wait For ID")]
        [KeywordParameters("automationID", "automationid of object to wait")]
        [KeywordParameters("timeout", "Waiting time in seconds")]
        [SampleScript("Windows.Wait For ID (1112,10)")]
        public KeywordResult WaitForId(string automationId, string timeoutInSec)
        {
            TimeSpan waitTime = TimeSpan.FromSeconds(int.Parse(timeoutInSec));

            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                AutomationElement toClick = SelectElement(_target, AutomationElement.AutomationIdProperty, automationId, waitTime);
                if (toClick == null)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given automation ID : {automationId}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                result.Result = KeywordResults.Fail;
                result.Output = $"Can not find element with given automation ID : {automationId}";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                return result;
            }
        }

        [KeywordDescription("Wait object to be shown until given timeout")]
        [KeywordDisplayName("Wait For Name")]
        [KeywordParameters("name", "name of object to wait")]
        [KeywordParameters("timeout", "Waiting time in seconds")]
        [SampleScript("Windows.Wait For Name (Basic,10)")]
        public KeywordResult WaitForName(string name, string timeoutInSec)
        {
            TimeSpan waitTime = TimeSpan.FromSeconds(int.Parse(timeoutInSec));

            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                AutomationElement toClick = SelectElement(_target, AutomationElement.NameProperty, name, waitTime);
                if (toClick == null)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given name : {name}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                result.Result = KeywordResults.Fail;
                result.Output = $"Can not find element with given name : {name}";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                return result;
            }
        }


        [KeywordDescription("Check if object with given automation Id is exist.")]
        [KeywordDisplayName("Is Exist")]
        [KeywordParameters("AutomationId", "Automation ID to check")]
        [SampleScript("Windows.Is Exist (2004)")]
        public KeywordResult IsExist(string automationId)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                AutomationElement toClick = SelectElement(_target, AutomationElement.AutomationIdProperty, automationId, TimeSpan.FromSeconds(3));
                if (toClick == null)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given automation ID : {automationId}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                result.Result = KeywordResults.Fail;
                result.Output = $"Can not find element with given automation ID : {automationId}";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                return result;
            }
        }


        [KeywordDescription("Check if object with given automation Id is in clickable position.")]
        [KeywordDisplayName("Is Clickable")]
        [KeywordParameters("AutomationId", "Automation ID to check")]
        [SampleScript("Windows.Is Clickable (SettingsPagePCSystemDisplay)")]
        public KeywordResult IsClickable(string automationId)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                AutomationElement toClick = SelectElement(_target, AutomationElement.AutomationIdProperty, automationId, TimeSpan.FromSeconds(1));
                if (toClick == null)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given automation ID : {automationId}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }

                Rect bounds = toClick.Current.BoundingRectangle;
                if (bounds.X == 0 && bounds.Y == 0 && bounds.Height == 0 && bounds.Width == 0)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Object is out of bound : {automationId}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }

                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                result.Result = KeywordResults.Fail;
                result.Output = $"Can not find element with given automation ID : {automationId}";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                return result;
            }
        }

        [KeywordDescription("Check if object with given name is in clickable position.")]
        [KeywordDisplayName("Is Name Clickable")]
        [KeywordParameters("Name", "name to check")]
        [SampleScript("Windows.Is Name Clickable (Display)")]
        public KeywordResult IsNameClickable(string name)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                AutomationElement toClick = SelectElement(_target, AutomationElement.NameProperty, name, TimeSpan.FromSeconds(1));
                if (toClick == null)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given name : {name}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }

                Rect bounds = toClick.Current.BoundingRectangle;
                if (bounds.X == 0 && bounds.Y == 0 && bounds.Height == 0 && bounds.Width == 0)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Object is out of bound : {name}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }

                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                result.Result = KeywordResults.Fail;
                result.Output = $"Can not find element with given name : {name}";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                return result;
            }
        }


        [KeywordDescription("Click with ID")]
        [KeywordDisplayName("Click ID")]
        [KeywordParameters("AutomationId", "Automation ID to click")]
        [SampleScript("Windows.Click ID (1231)")]
        public KeywordResult ClickId(string automationId)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                AutomationElement toClick = SelectElement(_target, AutomationElement.AutomationIdProperty, automationId, TimeSpan.FromSeconds(3));
                if (toClick == null)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given automation ID : {automationId}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }
                Logger.Trace($"Selected element name : {toClick.Current.Name}");
                if (ClickElement(toClick))
                {
                    return result;
                }
                else
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given automation ID : {automationId}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error during click", ex);
                result.Result = KeywordResults.Error;
                result.Output = "Error during click";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                return result;
            }
        }

        [KeywordDescription("click icon using tooltip text and automation ID")]
        [KeywordDisplayName("Click By Tooltip")]
        [KeywordParameters("AutomationId", "Automation ID")]
        [KeywordParameters("TooltipText", "Tooltip text")]
        [SampleScript("Windows.Click By Tooltip (NotifyIcon, Printer)")]
        public KeywordResult ClickByTooltip(string automationId, string tooltipText)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            if (string.IsNullOrWhiteSpace(automationId))
            {
                result.Result = KeywordResults.Fail;
                result.Output = "Automation ID should not be empty.";
                return result;
            }

            if (string.IsNullOrWhiteSpace(tooltipText))
            {
                result.Result = KeywordResults.Fail;
                result.Output = "Tooltip text should not be empty.";
                return result;
            }
            try
            {
                AutomationElementCollection elements = SelectElements(_target, AutomationElement.AutomationIdProperty, automationId, TimeSpan.FromSeconds(3));
                if (elements == null || elements.Count == 0)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find elements with given automation ID : {automationId}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }
                AutomationElement matchedElement = null;
                foreach (AutomationElement el in elements)
                {
                    string tooltip = el.Current.HelpText;
                    if (string.IsNullOrEmpty(tooltip))
                    {
                        tooltip = el.Current.Name;
                    }
                    Logger.Trace($"Element Tooltip/Name: {tooltip}");
                    if (!string.IsNullOrEmpty(tooltip) && tooltip.ToLower().Contains(tooltipText.ToLower()))
                    {
                        matchedElement = el;
                        break;
                    }
                }
                if (matchedElement == null)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"No element matched tooltip text : {tooltipText}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }
                Logger.Trace($"Selected element name : {matchedElement.Current.Name}");
                // Get clickable point
                MousePoint pt = GetClickablePoint(matchedElement);
                try
                {
                    MouseLeftClick(pt);
                }
                catch (Exception ex)
                {
                    Logger.Error("Unable to click matched element", ex);

                    result.Result = KeywordResults.Fail;
                    result.Output = "Unable to click matched element";
                    result.AdditionalInfo = ex.ToString();
                    result.ScreenShot = GetScreenShot(FullScreenBound());

                    return result;
                }
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error("Error during tray icon click", ex);
                result.Result = KeywordResults.Error;
                result.Output = "Error during tray icon click";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                return result;
            }
        }

        [KeywordDescription("Click with ID and Index")]
        [KeywordDisplayName("Click ID With Index")]
        [KeywordParameters("AutomationId", "Automation ID to click")]
        [KeywordParameters("Index", "Index of object. Starts with 1")]
        [SampleScript("Windows.Click ID With Index (1231,1)")]
        public KeywordResult ClickIDWithIndex(string automationId, string index)
        {
            if (!int.TryParse(index.Trim(), out int iIdx) || iIdx < 1)
            {
                return new KeywordResult(KeywordResults.Fail, "Index should be a positive number starting from 1");
            }
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                AutomationElementCollection toClicks = SelectElements(_target, AutomationElement.AutomationIdProperty, automationId, TimeSpan.FromSeconds(3));
                if (toClicks == null || toClicks.Count == 0)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"No elements found with Automation ID: {automationId}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }
                if (toClicks.Count < iIdx)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Index {index} out of range. Found {toClicks.Count} element(s) with Automation ID: {automationId}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }
                AutomationElement toClick = toClicks[iIdx - 1];
                Logger.Trace($"Selected element name : {toClick.Current.Name} at index {index}");
                if (ClickElement(toClick))
                {
                    return result;
                }
                else
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not click element with given Automation ID: {automationId}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error during click", ex);
                result.Result = KeywordResults.Error;
                result.Output = "Error during click";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                return result;
            }
        }

        [KeywordDescription("Click with WPath. See <a href='https://github.com/reitn/wpath/blob/master/README.md' target='_blank'>WPath usage documentation</a> for WPath usage.")]
        [KeywordDisplayName("Click WPath")]
        [KeywordParameters("WPath", "WPath of element to click.")]
        [SampleScript("Windows.Click WPath (//Button[@Name='Detect'])")]
        public KeywordResult ClickWPath(string wPath)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                AutomationElement toClick = SelectElementByWPath(_target, wPath, TimeSpan.FromSeconds(3));
                if (toClick == null)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given WPath ID : {wPath}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }
                Logger.Trace($"Selected element name : {toClick.Current.Name}");
                if (ClickElement(toClick))
                {
                    return result;
                }
                else
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given WPath : {wPath}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error during click", ex);
                result.Result = KeywordResults.Error;
                result.Output = "Error during click";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                return result;
            }
        }

        [KeywordDescription("Click Automation ID with Keyboard event. Use this keyword if Click ID is not working properly.")]
        [KeywordDisplayName("Click ID With Keyboard")]
        [KeywordParameters("AutomationId", "Automation ID to click")]
        [SampleScript("Windows.Click ID With Keyboard (2122)")]
        public KeywordResult ClickIDWithKeyboard(string automationId)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                AutomationElement toClick = SelectElement(_target, AutomationElement.AutomationIdProperty, automationId, TimeSpan.FromSeconds(3));
                if (toClick == null)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given automation ID : {automationId}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }
                Logger.Trace($"Selected element name : {toClick.Current.Name}");
                toClick.SetFocus();
                return SendSpecialKey("{ENTER}");
            }
            catch (Exception ex)
            {
                Logger.Error("Error during click", ex);
                result.Result = KeywordResults.Error;
                result.Output = "Error during click";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                return result;
            }
        }

        [KeywordDescription("Click Automation ID with Mouse event. This keyword moves mouse cursor to position of object.")]
        [KeywordDisplayName("Click ID With Mouse")]
        [KeywordParameters("AutomationId", "Automation ID to click")]
        [SampleScript("Windows.Click ID With Mouse (1282)")]
        public KeywordResult ClickIDWithMouse(string automationId)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                AutomationElement toClick = SelectElement(_target, AutomationElement.AutomationIdProperty, automationId, TimeSpan.FromSeconds(3));
                if (toClick == null)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given automation ID : {automationId}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }
                if (!WaitForEndMove(toClick))
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Target object is changing position. Can not click with mouse.";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }
                Logger.Trace($"Selected element name : {toClick.Current.Name}");
                MousePoint pt = GetClickablePoint(toClick);
                MouseLeftClick(pt);
                return new KeywordResult(KeywordResults.Pass);

            }
            catch (Exception ex)
            {
                Logger.Error("Error during click", ex);
                result.Result = KeywordResults.Error;
                result.Output = "Error during click";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                return result;
            }
        }

        [KeywordDescription("Right Click Automation ID with Mouse event. This keyword moves mouse cursor to position of object.")]
        [KeywordDisplayName("Right Click ID With Mouse")]
        [KeywordParameters("AutomationId", "Automation ID to right click")]
        [SampleScript("Windows.Right Click ID With Mouse (15)")]
        public KeywordResult RightClickIDWithMouse(string automationId)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                AutomationElement toClick = SelectElement(_target, AutomationElement.AutomationIdProperty, automationId, TimeSpan.FromSeconds(3));
                if (toClick == null)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given automation ID : {automationId}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }
                if (!WaitForEndMove(toClick))
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Target object is changing position. Can not click with mouse.";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }
                Logger.Trace($"Selected element name : {toClick.Current.Name}");
                MousePoint pt = GetClickablePoint(toClick);
                MouseRightClick(pt);
                return new KeywordResult(KeywordResults.Pass);

            }
            catch (Exception ex)
            {
                Logger.Error("Error during click", ex);
                result.Result = KeywordResults.Error;
                result.Output = "Error during click";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                return result;
            }
        }

        [KeywordDescription("Click with Name")]
        [KeywordDisplayName("Click Name")]
        [KeywordParameters("Name", "Name property to click")]
        [SampleScript("Windows.Click Name (Yes)")]
        public KeywordResult ClickName(string name)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                AutomationElement toClick = SelectElement(_target, AutomationElement.NameProperty, name, TimeSpan.FromSeconds(3));

                if (toClick == null)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given Text : {name}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }

                if (ClickElement(toClick))
                {
                    return result;
                }
                else
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not click element with given text : {name}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error during click", ex);
                result.Result = KeywordResults.Error;
                result.Output = "Error during click";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                return result;
            }
        }

        [KeywordDescription("Click with Name")]
        [KeywordDisplayName("Click Name")]
        [KeywordParameters("Name", "Name property to click")]
        [KeywordParameters("Index", "Index of object. Starts with 1")]
        [SampleScript("Windows.Click Name (Printing Shortcuts,1)")]
        public KeywordResult ClickName(string name, string index)
        {
            if (!int.TryParse(index.Trim(), out int iIdx))
            {
                return new KeywordResult(KeywordResults.Error, "Index should be a number");
            }

            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                AutomationElementCollection toClicks = SelectElements(_target, AutomationElement.NameProperty, name, TimeSpan.FromSeconds(3));
                if (toClicks.Count < iIdx)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given Text (index out of range) : {name}, {index}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }
                AutomationElement toClick = toClicks[--iIdx];

                if (toClick == null)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given Text : {name}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }

                if (ClickElement(toClick))
                {
                    return result;
                }
                else
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not click element with given text : {name}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error during click", ex);
                result.Result = KeywordResults.Error;
                result.Output = "Error during click";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                return result;
            }
        }

        [KeywordDescription("Click Name with Keyboard event. Use this keyword if Click Name is not working properly.")]
        [KeywordDisplayName("Click Name With Keyboard")]
        [KeywordParameters("Name", "Name to click")]
        [SampleScript("Windows.Click Name With Keyboard (Job Storage)")]
        public KeywordResult ClickNameWithKeyboard(string name)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                AutomationElement toClick = SelectElement(_target, AutomationElement.NameProperty, name, TimeSpan.FromSeconds(3));
                if (toClick == null)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given name : {name}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }
                toClick.SetFocus();
                return SendSpecialKey("{ENTER}");
            }
            catch (Exception ex)
            {
                Logger.Error("Error during click", ex);
                result.Result = KeywordResults.Error;
                result.Output = "Error during click";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                return result;
            }
        }

        [KeywordDescription("Click Name with Keyboard event. Use this keyword if Click Name is not working properly.")]
        [KeywordDisplayName("Click Name With Keyboard")]
        [KeywordParameters("Name", "Name to click")]
        [KeywordParameters("Index", "Index of object. Starts with 1")]
        [SampleScript("Windows.Click Name With Keyboard (Job Storage,1)")]
        public KeywordResult ClickNameWithKeyboard(string name, string index)
        {
            if (!int.TryParse(index.Trim(), out int iIdx))
            {
                return new KeywordResult(KeywordResults.Error, "Index should be a number");
            }

            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                AutomationElementCollection toClicks = SelectElements(_target, AutomationElement.NameProperty, name, TimeSpan.FromSeconds(3));
                if (toClicks.Count < iIdx)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given Text (index out of range) : {name}, {index}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }
                AutomationElement toClick = toClicks[--iIdx];

                if (toClick == null)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given Text : {name}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }

                toClick.SetFocus();
                return SendSpecialKey("{ENTER}");
            }
            catch (Exception ex)
            {
                Logger.Error("Error during click", ex);
                result.Result = KeywordResults.Error;
                result.Output = "Error during click";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                return result;
            }
        }

        [KeywordDescription("Click Name with Mouse event. This keyword moves mouse cursor to position of object.")]
        [KeywordDisplayName("Click Name With Mouse")]
        [KeywordParameters("Name", "Name to click")]
        [SampleScript("Windows.Click Name With Mouse (Save)")]
        public KeywordResult ClickNameWithMouse(string name)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                AutomationElement toClick = SelectElement(_target, AutomationElement.NameProperty, name, TimeSpan.FromSeconds(3));
                if (toClick == null)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given automation ID : {name}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }
                if (!WaitForEndMove(toClick))
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Target object is changing position. Can not click with mouse.";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }

                Logger.Trace($"Selected element name : {toClick.Current.Name}");
                _target.SetFocus();
                MousePoint pt = GetClickablePoint(toClick);
                MouseLeftClick(pt);
                return new KeywordResult(KeywordResults.Pass);

            }
            catch (Exception ex)
            {
                Logger.Error("Error during click", ex);
                result.Result = KeywordResults.Error;
                result.Output = "Error during click";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                return result;
            }
        }

        [KeywordDescription("Right Click Name with Mouse event. This keyword moves mouse cursor to position of object.")]
        [KeywordDisplayName("Right Click Name With Mouse")]
        [KeywordParameters("Name", "Name to right click")]
        [SampleScript("Windows.Right Click Name With Mouse(Blank document)")]
        public KeywordResult RightClickNameWithMouse(string name)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                AutomationElement toClick = SelectElement(_target, AutomationElement.NameProperty, name, TimeSpan.FromSeconds(3));
                if (toClick == null)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given automation ID : {name}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }
                if (!WaitForEndMove(toClick))
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Target object is changing position. Can not click with mouse.";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }

                Logger.Trace($"Selected element name : {toClick.Current.Name}");
                _target.SetFocus();
                MousePoint pt = GetClickablePoint(toClick);
                MouseRightClick(pt);
                return new KeywordResult(KeywordResults.Pass);

            }
            catch (Exception ex)
            {
                Logger.Error("Error during click", ex);
                result.Result = KeywordResults.Error;
                result.Output = "Error during click";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                return result;
            }
        }

        [KeywordDescription("Click Name with Mouse event. This keyword moves mouse cursor to position of object.")]
        [KeywordDisplayName("Click Name With Mouse")]
        [KeywordParameters("Name", "Name to click")]
        [KeywordParameters("Index", "Index of object. Starts with 1")]
        [SampleScript("Windows.Click Name With Mouse (Save,1)")]
        public KeywordResult ClickNameWithMouse(string name, string index)
        {
            if (!int.TryParse(index.Trim(), out int iIdx))
            {
                return new KeywordResult(KeywordResults.Error, "Index should be a number");
            }

            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                AutomationElementCollection toClicks = SelectElements(_target, AutomationElement.NameProperty, name, TimeSpan.FromSeconds(3));
                if (toClicks.Count < iIdx)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given Text (index out of range) : {name}, {index}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }
                AutomationElement toClick = toClicks[--iIdx];

                if (toClick == null)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given Text : {name}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }

                if (!WaitForEndMove(toClick))
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Target object is changing position. Can not click with mouse.";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }

                Logger.Trace($"Selected element name : {toClick.Current.Name}");
                _target.SetFocus();
                MousePoint pt = GetClickablePoint(toClick);
                MouseLeftClick(pt);
                return new KeywordResult(KeywordResults.Pass);

            }
            catch (Exception ex)
            {
                Logger.Error("Error during click", ex);
                result.Result = KeywordResults.Error;
                result.Output = "Error during click";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                return result;
            }
        }

        [KeywordDescription("Right Click Name with Mouse event. This keyword moves mouse cursor to position of object.")]
        [KeywordDisplayName("Right Click Name With Mouse")]
        [KeywordParameters("Name", "Name to right click")]
        [KeywordParameters("Index", "Index of object. Starts with 1")]
        [SampleScript("Windows.Right Click Name With Mouse(Blank document,1)")]
        public KeywordResult RightClickNameWithMouse(string name, string index)
        {
            if (!int.TryParse(index.Trim(), out int iIdx))
            {
                return new KeywordResult(KeywordResults.Error, "Index should be a number");
            }

            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                AutomationElementCollection toClicks = SelectElements(_target, AutomationElement.NameProperty, name, TimeSpan.FromSeconds(3));
                if (toClicks.Count < iIdx)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given Text (index out of range) : {name}, {index}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }
                AutomationElement toClick = toClicks[--iIdx];

                if (toClick == null)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given Text : {name}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }

                if (!WaitForEndMove(toClick))
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Target object is changing position. Can not click with mouse.";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }

                Logger.Trace($"Selected element name : {toClick.Current.Name}");
                _target.SetFocus();
                MousePoint pt = GetClickablePoint(toClick);
                MouseRightClick(pt);
                return new KeywordResult(KeywordResults.Pass);

            }
            catch (Exception ex)
            {
                Logger.Error("Error during click", ex);
                result.Result = KeywordResults.Error;
                result.Output = "Error during click";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                return result;
            }
        }

        [KeywordDescription("Scroll in scroll viewer in current selected application.")]
        [KeywordDisplayName("Scroll")]
        [KeywordParameters("direction", "Should be one of <, <<, >, >>, ^, ^^, v, vv\r\n" +
            "< (right to lefet with small amount), << (right to left with large amount) \r\n" +
            "> (left to right with small amount), >> (left to right with large amount) \r\n" +
            "^ (down to up with small amount), ^^ (down to up with larget amount) \r\n" +
            "v (up to down with small amount), vv (up to down with larget amount)")]
        [SampleScript("Windows.Scroll (^)")]
        public KeywordResult Scroll(string direction)
        {
            return Scroll(direction, "1");
        }

        [KeywordDescription("Scroll in scroll viewer in current selected application.")]
        [KeywordDisplayName("Scroll")]
        [KeywordParameters("direction", "Should be one of <, <<, >, >>, ^, ^^, v, vv\r\n" +
            "< (right to lefet with small amount), << (right to left with large amount) \r\n" +
            "> (left to right with small amount), >> (left to right with large amount) \r\n" +
            "^ (down to up with small amount), ^^ (down to up with larget amount) \r\n" +
            "v (up to down with small amount), vv (up to down with larget amount)")]
        [KeywordParameters("index", "index of scroll view. Starts with 1")]
        [SampleScript("Windows.Scroll (^,1)")]
        public KeywordResult Scroll(string direction, string index)
        {
            if (!int.TryParse(index.Trim(), out int intIndex))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Index must be a number";
                Logger.Error(error.Output);
                return error;
            }
            intIndex--;

            AutomationElement toScroll = SelectElements(_target, AutomationElement.ClassNameProperty, "ScrollViewer", TimeSpan.FromSeconds(3))[intIndex];


            if (toScroll == null)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Can not find scroll viewer in current window";
                Logger.Error(fail.Output);
                return fail;
            }
            object objPattern = null;
            if (!toScroll.TryGetCurrentPattern(ScrollPattern.Pattern, out objPattern))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Can not find scroll pattern for given element.";
                Logger.Error(error.Output);
                return error;
            }
            ScrollPattern scrollPattern = objPattern as ScrollPattern;
            ScrollAmount amount = ScrollAmount.NoAmount;
            bool horizontal = false;

            switch (direction.Trim())
            {
                case "<":
                    amount = ScrollAmount.SmallDecrement;
                    horizontal = true;
                    break;
                case ">":
                    amount = ScrollAmount.SmallIncrement;
                    horizontal = true;
                    break;
                case "^":
                    amount = ScrollAmount.SmallDecrement;
                    break;
                case "v":
                    amount = ScrollAmount.SmallIncrement;
                    break;
                case "<<":
                    amount = ScrollAmount.LargeDecrement;
                    horizontal = true;
                    break;
                case ">>":
                    amount = ScrollAmount.LargeIncrement;
                    horizontal = true;
                    break;
                case "^^":
                    amount = ScrollAmount.LargeDecrement;
                    break;
                case "vv":
                    amount = ScrollAmount.LargeIncrement;
                    break;
                default:
                    KeywordResult error = new KeywordResult(KeywordResults.Error);
                    error.Result = KeywordResults.Error;
                    error.Output = "Invalied Arguments";
                    return error;
            }

            toScroll.SetFocus();
            try
            {
                if (horizontal)
                {
                    scrollPattern.ScrollHorizontal(amount);
                }
                else
                {
                    scrollPattern.ScrollVertical(amount);
                }
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Unexpected error with scroll. Check if scroll viewer is scrollable with given direction.";
                error.AdditionalInfo = ex.ToString();
                Logger.Error(error.Output, ex);
                return error;

            }

            return new KeywordResult(KeywordResults.Pass);

        }

        [KeywordDescription("Send special key to the application")]
        [KeywordDisplayName("Send Special Key")]
        [KeywordParameters("Key", "Key to send.\r\nAvailable Keys\r\n" +
            "[BACKSPACE], [BREAK], [CAPSLOCK], [DELETE], [DOWN], [END], [ENTER], [ESC], [HOME], [INSERT], [LEFT], [NUMLOCK], \r\n" +
            "[PGDN], [PGUP], [PRTSC], [RIGHT], [SCROLLLOCK], [TAB], [UP], [F1], [F2], [F3], [F4], [F5], [F6], [F7], [F8], \r\n" +
            "[F9], [F10], [F11], [F12], [F13], [F14], [F15], [F16], [ADD], [SUBTRACT], [MULTIPLY], [DIVIDE]\r\n" +
            "Also for combination  keys, use + for SHIFT, ^ for CTRL and % for ALT. ex)^p will send Ctrl+P")]
        [SampleScript("Windows.Send Special Key([DOWN])")]
        public KeywordResult SendSpecialKey(string key)
        {
            key = key.Replace("[", "{").Replace("]", "}");
            try
            {
                SendKeys.SendWait(key);
            }
            catch (Exception ex)
            {
                KeywordResult result = new KeywordResult(KeywordResults.Error);
                result.Output = "Error during send key";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                Logger.Error("Error during send key", ex);
                return result;
            }

            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Send space key to the application")]
        [KeywordDisplayName("Send space Key")]
        [SampleScript("Windows.Send Space Key()")]
        public KeywordResult SendSpaceKey()
        {
            try
            {
                string spaceKey = " ";
                SendKeys.SendWait(spaceKey);
            }
            catch (Exception ex)
            {
                KeywordResult result = new KeywordResult(KeywordResults.Error);
                result.Output = "Error during send space key";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                Logger.Error("Error during send space key", ex);
                return result;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Send key to the application")]
        [KeywordDisplayName("Send Key")]
        [KeywordParameters("Key", "Key to send")]
        [SampleScript("Windows.Send Key (1111)")]
        public KeywordResult SendKey(string key)
        {
            key = key.Replace("%", "{%}").Replace("^", "{^}").Replace("+", "{+}").Replace("(", "{(}")
                .Replace(")", "{)}").Replace("~", "{~}");
            try
            {
                if (key.Equals(@"\n"))
                {
                    key = "{ENTER}";
                }
                SendKeys.SendWait(key);
            }
            catch (Exception ex)
            {
                KeywordResult result = new KeywordResult(KeywordResults.Error);
                result.Output = "Error during send key";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                Logger.Error("Error during send key", ex);
                return result;
            }

            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Send Ctrl+P to the application")]
        [KeywordDisplayName("Send CtrlP")]
        [SampleScript("Windows.Send CtrlP")]
        public KeywordResult SendCtrlP()
        {
            return SendSpecialKey("^(p)");
        }

        [KeywordDescription("Set text of element with given Automation ID.")]
        [KeywordDisplayName("Set Text")]
        [KeywordParameters("AutomationId", "Automation ID to set text")]
        [KeywordParameters("TextToSet", @"Text to set to the field. If this value ends with '\n' it will send Enter key after set text")]
        [SampleScript("Windows.Set Text (SearchTextBox,settings)")]
        public KeywordResult SetText(string automationId, string textToSet)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            bool endingEnter = false;
            try
            {
                if (textToSet.EndsWith(@"\n"))
                {
                    endingEnter = true;
                    textToSet = textToSet.Substring(textToSet.Length - @"\n".Length);
                }
                AutomationElement toSet = SelectElement(_target, AutomationElement.AutomationIdProperty, automationId, TimeSpan.FromSeconds(3));

                if (toSet == null)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given automation ID : {automationId}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }

                try
                {
                    object valuePattern = null;
                    if (!toSet.TryGetCurrentPattern(ValuePattern.Pattern, out valuePattern))
                    {
                        Logger.Trace("Set Text using send key");
                        toSet.SetFocus();
                        Thread.Sleep(100);
                        SendKeys.SendWait("^{HOME}");   // Move to start of control
                        SendKeys.SendWait("^+{END}");   // Select everything
                        SendKeys.SendWait("{DEL}");     // Delete selection

                        char[] chars = textToSet.ToCharArray();

                        for (int i = 0; i < chars.Length; i++)
                        {
                            SendKeys.SendWait(chars[i].ToString());
                            Thread.Sleep(200);
                        }

                    }
                    else
                    {
                        Logger.Trace("Set Text using value pattern");
                        ((ValuePattern)valuePattern).SetValue(textToSet);
                    }

                    if (endingEnter)
                    {
                        SendKeys.SendWait("{ENTER}");
                    }

                }
                catch (ElementNotEnabledException ex)
                {
                    Logger.Error("SetText : Element is not enalbed", ex);
                    result.Result = KeywordResults.Error;
                    result.Output = "SetText : Element is not enalbed";
                    result.AdditionalInfo = ex.ToString();
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;

                }
                catch (InvalidOperationException ex)
                {
                    Logger.Error("SetText : Invalid Operation", ex);
                    result.Result = KeywordResults.Error;
                    result.Output = "SetText : Invalid Operation";
                    result.AdditionalInfo = ex.ToString();
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;

                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.Error("Error during set text", ex);
                result.Result = KeywordResults.Error;
                result.Output = "Error during set text";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                return result;
            }
        }

        [KeywordDescription("Set text of element with Automation Name Property.")]
        [KeywordDisplayName("Set Text With Name")]
        [KeywordParameters("AutomationName", "Automation Name to set text")]
        [KeywordParameters("TextToSet", @"Text to set to the field. If this value ends with '\n' it will send Enter key after set text")]
        [SampleScript("Windows.Set Text With Name (Type your release code,${Releasecode})")]
        public KeywordResult SetTextWithName(string automationName, string textToSet)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            bool endingEnter = false;
            try
            {
                if (textToSet.EndsWith(@"\n"))
                {
                    endingEnter = true;
                    textToSet = textToSet.Substring(textToSet.Length - @"\n".Length);
                }
                AutomationElement toSet = SelectElement(_target, AutomationElement.NameProperty, automationName, TimeSpan.FromSeconds(3));

                if (toSet == null)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given automation ID : {automationName}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }

                try
                {
                    object valuePattern = null;
                    if (!toSet.TryGetCurrentPattern(ValuePattern.Pattern, out valuePattern))
                    {
                        Logger.Trace("Set Text using send key");
                        toSet.SetFocus();
                        Thread.Sleep(100);
                        SendKeys.SendWait("^{HOME}");   // Move to start of control
                        SendKeys.SendWait("^+{END}");   // Select everything
                        SendKeys.SendWait("{DEL}");     // Delete selection                       

                        char[] chars = textToSet.ToCharArray();

                        for (int i = 0; i < chars.Length; i++)
                        {
                            SendKeys.SendWait(chars[i].ToString());
                            Thread.Sleep(200);
                        }

                    }
                    else
                    {
                        Logger.Trace("Set Text using value pattern");
                        ((ValuePattern)valuePattern).SetValue(textToSet);
                    }

                    if (endingEnter)
                    {
                        SendKeys.SendWait("{ENTER}");
                    }

                }
                catch (ElementNotEnabledException ex)
                {
                    Logger.Error("SetText : Element is not enalbed", ex);
                    result.Result = KeywordResults.Error;
                    result.Output = "SetText : Element is not enalbed";
                    result.AdditionalInfo = ex.ToString();
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;

                }
                catch (InvalidOperationException ex)
                {
                    Logger.Error("SetText : Invalid Operation", ex);
                    result.Result = KeywordResults.Error;
                    result.Output = "SetText : Invalid Operation";
                    result.AdditionalInfo = ex.ToString();
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;

                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.Error("Error during set text", ex);
                result.Result = KeywordResults.Error;
                result.Output = "Error during set text";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                return result;
            }
        }

        [KeywordDescription("Set text of element with given Automation ID via send key method. Use this keyword if Set Text is not working properly.")]
        [KeywordDisplayName("Set Text With Send Key")]
        [KeywordParameters("AutomationId", "Automation ID to set text")]
        [KeywordParameters("TextToSet", @"Text to set to the field. If this value ends with '\n' it will send Enter key after set text")]
        [SampleScript("Windows.Set Text With Send Key (SearchTextBox,settings)")]
        public KeywordResult SetTextWithSendKey(string automationId, string textToSet)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            bool endingEnter = false;
            try
            {
                Logger.Debug("# text entered in SetTextWithSendKey-Begin=" + textToSet.ToString());

                if (textToSet.EndsWith(@"\n"))
                {
                    endingEnter = true;
                    textToSet = textToSet.Substring(textToSet.Length - @"\n".Length);
                }
                AutomationElement toSet = SelectElement(_target, AutomationElement.AutomationIdProperty, automationId, TimeSpan.FromSeconds(3));

                if (toSet == null)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given automation ID : {automationId}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }

                try
                {


                    toSet.SetFocus();
                    Thread.Sleep(100);
                    SendKeys.SendWait("^{HOME}");   // Move to start of control
                    SendKeys.SendWait("^+{END}");   // Select everything
                    SendKeys.SendWait("{DEL}");     // Delete selection
                    Logger.Debug("# text entered=" + textToSet.ToString());

                    char[] chars = textToSet.ToCharArray();
                    for (int i = 0; i < chars.Length; i++)
                    {
                        SendKeys.SendWait(chars[i].ToString());
                        Thread.Sleep(200);
                    }

                    if (endingEnter)
                    {
                        SendKeys.SendWait("{ENTER}");
                    }

                }
                catch (ElementNotEnabledException ex)
                {
                    Logger.Error("SetText : Element is not enalbed", ex);
                    result.Result = KeywordResults.Error;
                    result.Output = "SetText : Element is not enalbed";
                    result.AdditionalInfo = ex.ToString();
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;

                }
                catch (InvalidOperationException ex)
                {
                    Logger.Error("SetText : Invalid Operation", ex);
                    result.Result = KeywordResults.Error;
                    result.Output = "SetText : Invalid Operation";
                    result.AdditionalInfo = ex.ToString();
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;

                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.Error("Error during set text", ex);
                result.Result = KeywordResults.Error;
                result.Output = "Error during set text";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                return result;
            }
        }

        [KeywordDescription("Capture Screen Shot of current application")]
        [KeywordDisplayName("Capture Screen Shot")]
        [KeywordParameters("filename", "filename to save")]
        [SampleScript("Windows.Capture Screen Shot (ScreenShotName)")]
        public KeywordResult CaptureScreenShot(string filename)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            result.ScreenShot = GetScreenShot(FullScreenBound());
            if (result.ScreenShot != null)
            {
                if (!string.IsNullOrEmpty(filename))
                {
                    string saveTo = Path.Combine(_outputDir, filename);
                    File.WriteAllBytes(saveTo, result.ScreenShot);
                    if (File.Exists(saveTo))
                    {
                        return result;
                    }
                }
            }
            result.Result = KeywordResults.Fail;
            result.Output = $"Capture screen shot failed with given filename :: {filename}";
            return result;
        }

        [KeywordDescription("Capture Screen Shot of full screen")]
        [KeywordDisplayName("Capture Full Screen")]
        [KeywordParameters("filename", "filename to save")]
        [SampleScript("Windows.Capture Full Screen (filename)")]
        public KeywordResult CaptureFullScreen(string filename)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);


            result.ScreenShot = GetScreenShot(FullScreenBound());
            if (result.ScreenShot != null)
            {
                if (!string.IsNullOrEmpty(filename))
                {
                    string saveTo = Path.Combine(_outputDir, filename);
                    File.WriteAllBytes(saveTo, result.ScreenShot);
                    if (File.Exists(saveTo))
                    {
                        return result;
                    }
                }
            }
            result.Result = KeywordResults.Fail;
            result.Output = $"Capture screen shot failed with given filename :: {filename}";
            return result;
        }

        [KeywordDescription("Captures a screenshot of the current application and saves it to the specified filename. The captured image can be used for sending to the AI Vision server for text recognition or visual analysis.")]
        [KeywordDisplayName("Screen Shot For AI Vision")]
        [KeywordParameters("filename", "filename to save")]
        [SampleScript("Windows.Screen Shot For AI Vision (ScreenShotName.png)")]
        public KeywordResult ScreenShotForAIVision(string filename)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                // Validate filename
                if (string.IsNullOrWhiteSpace(filename))
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = "Filename cannot be null or empty";
                    Logger.Warn(result.Output);
                    return result;
                }

                char[] invalidChars = Path.GetInvalidFileNameChars();
                if (filename.IndexOfAny(invalidChars) >= 0)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Filename contains invalid characters: {filename}";
                    Logger.Warn(result.Output);
                    return result;
                }

                result.ScreenShot = GetScreenShot();
                if (result.ScreenShot == null)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = "Failed to capture screenshot - GetScreenShot returned null";
                    Logger.Error(result.Output);
                    return result;
                }

                string saveTo = Path.Combine(_outputDir, filename);
                File.WriteAllBytes(saveTo, result.ScreenShot);
                if (File.Exists(saveTo))
                {
                    result.Output = $"Screenshot saved successfully to: {saveTo}";
                    Logger.Debug(result.Output);
                    return result;
                }
                else
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"File verification failed - screenshot not found at: {saveTo}";
                    Logger.Error(result.Output);
                    return result;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                result.Result = KeywordResults.Error;
                result.Output = $"Access denied when saving screenshot to: {filename}";
                result.AdditionalInfo = ex.ToString();
                Logger.Error(result.Output, ex);
                return result;
            }
            catch (IOException ex)
            {
                result.Result = KeywordResults.Error;
                result.Output = $"IO error when saving screenshot: {ex.Message}";
                result.AdditionalInfo = ex.ToString();
                Logger.Error(result.Output, ex);
                return result;
            }
            catch (Exception ex)
            {
                result.Result = KeywordResults.Error;
                result.Output = $"Unexpected error capturing screenshot for AI Vision: {ex.Message}";
                result.AdditionalInfo = ex.ToString();
                Logger.Error(result.Output, ex);
                return result;
            }
        }


        [KeywordDescription("Set combo box value with given automation IDs")]
        [KeywordDisplayName("Set ComboBox")]
        [KeywordParameters("comboboxAutomationID", "Automation ID for combo box")]
        [KeywordParameters("valueAutomationID", "Automation ID for value")]
        [SampleScript("Windows.Set ComboBox (PART_FocusTarget,Release)")]
        public KeywordResult SetComboBox(string comboboxAutomationID, string valueAutomationID)
        {
            AutomationElement comboBox = SelectElement(_target, AutomationElement.AutomationIdProperty, comboboxAutomationID, TimeSpan.FromSeconds(2));
            if (comboBox == null)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Can not find combobox : {comboboxAutomationID}";
                return fail;
            }
            PropertyCondition condition = new PropertyCondition(AutomationElement.AutomationIdProperty, valueAutomationID);
            if (SelectItem(comboBox, condition, true))
            {
                return new KeywordResult(KeywordResults.Pass);
            }
            KeywordResult result = new KeywordResult(KeywordResults.Fail);
            result.Output = $"Combobx click fail : {comboboxAutomationID} - {valueAutomationID}";
            result.ScreenShot = GetScreenShot(FullScreenBound());
            return result;
        }

        [KeywordDescription("Set combo box value with given automation IDs")]
        [KeywordDisplayName("Set ComboBox With Name")]
        [KeywordParameters("comboboxAutomationID", "Automation ID for combo box")]
        [KeywordParameters("valueName", "Name for value")]
        [SampleScript("Windows.Set ComboBox With Name(deviceId_comboBox,Hopper)")]
        public KeywordResult SetComboBoxWithName(string comboboxAutomationID, string valueName)
        {
            AutomationElement comboBox = SelectElement(_target, AutomationElement.AutomationIdProperty, comboboxAutomationID, TimeSpan.FromSeconds(2));
            if (comboBox == null)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Can not find combobox : {comboboxAutomationID}";
                return fail;
            }
            PropertyCondition condition = new PropertyCondition(AutomationElement.NameProperty, valueName);
            if (SelectItem(comboBox, condition, true))
            {
                return new KeywordResult(KeywordResults.Pass);
            }
            KeywordResult result = new KeywordResult(KeywordResults.Fail);
            result.Output = $"Combobx click fail : {comboboxAutomationID} - {valueName}";
            result.ScreenShot = GetScreenShot(FullScreenBound());
            return result;
        }

        [KeywordDescription("Set list item with given automation IDs")]
        [KeywordDisplayName("Set List Item")]
        [KeywordParameters("listboxAutomationID", "Automation ID for list")]
        [KeywordParameters("itemAutomationID", "Automation ID for item to select")]
        [SampleScript("Windows.Set List Item (PART_FocusTarget,Debug)")]
        public KeywordResult SetListItem(string listboxAutomationID, string itemAutomationID)
        {
            AutomationElement listBox = SelectElement(_target, AutomationElement.AutomationIdProperty, listboxAutomationID, TimeSpan.FromSeconds(2));
            if (listBox == null)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Can not find list box : {listboxAutomationID}";
                return fail;
            }
            PropertyCondition condition = new PropertyCondition(AutomationElement.AutomationIdProperty, itemAutomationID);
            if (SelectItem(listBox, condition, false))
            {
                return new KeywordResult(KeywordResults.Pass);
            }
            KeywordResult result = new KeywordResult(KeywordResults.Fail);
            result.Output = $"List box selection fail : {listboxAutomationID} - {itemAutomationID}";
            result.ScreenShot = GetScreenShot(FullScreenBound());
            return result;
        }

        [KeywordDescription("Set list item with given automation ID and Name")]
        [KeywordDisplayName("Set List Item With Name")]
        [KeywordParameters("listboxAutomationID", "Automation ID for list")]
        [KeywordParameters("itemName", "Name for item to select")]
        [SampleScript("Windows.Set List Item With Name (deviceId_comboBox,Bell)")]
        public KeywordResult SetListItemWithName(string listboxAutomationID, string itemName)
        {
            AutomationElement listBox = SelectElement(_target, AutomationElement.AutomationIdProperty, listboxAutomationID, TimeSpan.FromSeconds(2));
            if (listBox == null)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Can not find list box : {listboxAutomationID}";
                return fail;
            }
            PropertyCondition condition = new PropertyCondition(AutomationElement.NameProperty, itemName);
            if (SelectItem(listBox, condition, false, true))
            {
                return new KeywordResult(KeywordResults.Pass);
            }
            KeywordResult result = new KeywordResult(KeywordResults.Fail);
            result.Output = $"List box selection fail : {listboxAutomationID} - {itemName}";
            result.ScreenShot = GetScreenShot(FullScreenBound());
            return result;
        }

        [KeywordDescription("Click list item with given automation IDs")]
        [KeywordDisplayName("Click List Item")]
        [KeywordParameters("listboxAutomationID", "Automation ID for list")]
        [KeywordParameters("itemAutomationID", "Automation ID for item to click")]
        [SampleScript("Windows.Click List Item (PART_FocusTarget,Any CPU)")]
        public KeywordResult ClickListItem(string listboxAutomationID, string itemAutomationID)
        {
            AutomationElement listBox = SelectElement(_target, AutomationElement.AutomationIdProperty, listboxAutomationID, TimeSpan.FromSeconds(2));
            if (listBox == null)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Can not find list box : {listboxAutomationID}";
                return fail;
            }
            PropertyCondition condition = new PropertyCondition(AutomationElement.AutomationIdProperty, itemAutomationID);
            if (SelectItem(listBox, condition, false, false, true))
            {
                return new KeywordResult(KeywordResults.Pass);
            }
            KeywordResult result = new KeywordResult(KeywordResults.Fail);
            result.Output = $"List box selection fail : {listboxAutomationID} - {itemAutomationID}";
            result.ScreenShot = GetScreenShot(FullScreenBound());
            return result;
        }

        [KeywordDescription("Click list item with given automation ID and Name")]
        [KeywordDisplayName("Click List Item With Name")]
        [KeywordParameters("listboxAutomationID", "Automation ID for list")]
        [KeywordParameters("itemName", "Name for item to click")]
        [SampleScript("Windows.Click List Item With Name (PART_FocusTarget,Any CPU)")]
        public KeywordResult ClickListItemWithName(string listboxAutomationID, string itemName)
        {
            AutomationElement listBox = SelectElement(_target, AutomationElement.AutomationIdProperty, listboxAutomationID, TimeSpan.FromSeconds(2));
            if (listBox == null)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Can not find list box : {listboxAutomationID}";
                return fail;
            }
            PropertyCondition condition = new PropertyCondition(AutomationElement.NameProperty, itemName);
            if (SelectItem(listBox, condition, false, true, true))
            {
                return new KeywordResult(KeywordResults.Pass);
            }
            KeywordResult result = new KeywordResult(KeywordResults.Fail);
            result.Output = $"List box selection fail : {listboxAutomationID} - {itemName}";
            result.ScreenShot = GetScreenShot(FullScreenBound());
            return result;
        }


        [KeywordDescription("Click sub item with given automation ID and value")]
        [KeywordDisplayName("Click Sub Item With Value")]
        [KeywordParameters("parentAutomationId", "Automation ID for parent(Gridview)")]
        [KeywordParameters("ItemValue", "Value of item to click")]
        [SampleScript("Windows.Click Sub Item With Value (Device_Description,Bell)")]
        public KeywordResult ClickSubItemWithValue(string parentAutomationId, string itemValue)
        {
            AutomationElement parent = SelectElement(_target, AutomationElement.AutomationIdProperty, parentAutomationId, TimeSpan.FromSeconds(2));
            if (parent == null)
            {
                KeywordResult findFail = new KeywordResult(KeywordResults.Fail);
                findFail.Output = $"Can not find parent object : {parentAutomationId}";
                return findFail;
            }

            AutomationElementCollection collection = parent.FindAll(TreeScope.Subtree, Condition.TrueCondition);
            foreach (AutomationElement element in collection)
            {
                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object valuePattern))
                {
                    string value = ((ValuePattern)valuePattern).Current.Value;
                    if (value.Equals(itemValue))
                    {
                        if (ClickElement(element))
                        {
                            return new KeywordResult(KeywordResults.Pass);
                        }
                        else
                        {
                            KeywordResult clickFail = new KeywordResult(KeywordResults.Fail, "Click failed");
                            clickFail.ScreenShot = GetScreenShot(FullScreenBound());
                            return clickFail;
                        }
                    }
                }
            }
            KeywordResult fail = new KeywordResult(KeywordResults.Fail, $"Can not find item with value : {itemValue}");
            fail.ScreenShot = GetScreenShot(FullScreenBound());
            return fail;
        }

        [KeywordDescription("Check toggle state On or Off for given automation ID")]
        [KeywordDisplayName("CheckToggleState")]
        [KeywordParameters("toggleAutomationID", "Automation ID of toggle or Name")]
        [KeywordParameters("toggleValue", "On or Off")]
        [SampleScript("Windows.CheckToggleState(SystemSettings_Accessibility_IsAnimationsEnabled_ToggleSwitch, Off)")]
        public KeywordResult CheckToggleState(string toggleAutomationID, string toggleOnOff)
        {
            bool toToggleToSet = true;
            if (toggleOnOff.Equals("On", StringComparison.CurrentCultureIgnoreCase))
            {
                toToggleToSet = true;
            }
            else if (toggleOnOff.Equals("Off", StringComparison.CurrentCultureIgnoreCase))
            {
                toToggleToSet = false;
            }
            else
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Option value of toggle button must be On or Off";
                return fail;
            }

            try
            {
                AutomationElement toggleButton = SelectElement(_target, AutomationElement.AutomationIdProperty, toggleAutomationID, TimeSpan.FromSeconds(3));

                if (toggleButton != null)
                {

                    object pattern = null;
                    if (toggleButton.TryGetCurrentPattern(TogglePattern.Pattern, out pattern))
                    {
                        ToggleState toggleState = ((TogglePattern)pattern).Current.ToggleState;
                        Logger.Trace($"Current Toggle State : {toggleState}");

                        if (toggleState.ToString() == toggleOnOff)
                        {
                            return new KeywordResult(KeywordResults.Pass);
                        }

                    }
                }
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "Current toggle state is not matching with given toggle state";
                fail.ScreenShot = GetScreenShot(FullScreenBound());
                return fail;
            }
            catch (Exception ex)
            {
                Logger.Error("Error during CheckToggleState", ex);
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error during CheckToggleState";
                error.AdditionalInfo = ex.ToString();
                return error;
            }
        }

        [KeywordDescription("Set toggle state")]
        [KeywordDisplayName("Set Toggle")]
        [KeywordParameters("toggleAutomationID", "Automation ID of toggle")]
        [KeywordParameters("toggleValue", "On or Off")]
        [SampleScript("Windows.Set Toggle (SystemSettings_Accessibility_IsAnimationsEnabled_ToggleSwitch,Off)")]
        public KeywordResult SetToggle(string toggleAutomationID, string toggleOnOff)
        {
            bool toToggleToSet = true;
            if (toggleOnOff.Equals("On", StringComparison.CurrentCultureIgnoreCase))
            {
                toToggleToSet = true;
            }
            else if (toggleOnOff.Equals("Off", StringComparison.CurrentCultureIgnoreCase))
            {
                toToggleToSet = false;
            }
            else
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Option value of toggle button must be On or Off";
                return fail;
            }
            AutomationElement toggleButton = SelectElement(_target, AutomationElement.AutomationIdProperty, toggleAutomationID, TimeSpan.FromSeconds(2));

            try
            {
                object pattern = null;
                if (toggleButton.TryGetCurrentPattern(TogglePattern.Pattern, out pattern))
                {
                    ToggleState toggleState = ((TogglePattern)pattern).Current.ToggleState;
                    Logger.Trace($"Current Toggle State : {toggleState}");
                    bool currentToggle = false;
                    if (toggleState.Equals(ToggleState.On))
                    {
                        currentToggle = true;
                    }

                    if (toToggleToSet != currentToggle)
                    {
                        ((TogglePattern)pattern).Toggle();
                    }
                    return new KeywordResult(KeywordResults.Pass);
                }
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "Current object is not toggle button";
                fail.ScreenShot = GetScreenShot(FullScreenBound());
                return fail;
            }
            catch (Exception ex)
            {
                Logger.Error("Error during Toggle", ex);
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error during Toggle";
                error.AdditionalInfo = ex.ToString();
                return error;
            }
        }

        [KeywordDescription("Set toggle state by WPath")]
        [KeywordDisplayName("Set Toggle By WPath")]
        [KeywordParameters("toggleWPath", "WPath of toggle. See https://github.com/reitn/wpath/blob/master/README.md for WPath usage.")]
        [KeywordParameters("toggleValue", "On or Off")]
        [SampleScript("Windows.Set Toggle By WPath(//checkbox[@Name='Show suggestions in your timeline',On)")]
        public KeywordResult SetToggleByWPath(string toggleWPath, string toggleOnOff)
        {
            bool toToggleToSet = true;
            if (toggleOnOff.Equals("On", StringComparison.CurrentCultureIgnoreCase))
            {
                toToggleToSet = true;
            }
            else if (toggleOnOff.Equals("Off", StringComparison.CurrentCultureIgnoreCase))
            {
                toToggleToSet = false;
            }
            else
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Option value of toggle button must be On or Off";
                return fail;
            }
            AutomationElement toggleButton = SelectElementByWPath(_target, toggleWPath, TimeSpan.FromSeconds(2)); ;

            try
            {
                object pattern = null;
                if (toggleButton.TryGetCurrentPattern(TogglePattern.Pattern, out pattern))
                {
                    ToggleState toggleState = ((TogglePattern)pattern).Current.ToggleState;
                    Logger.Trace($"Current Toggle State : {toggleState}");
                    bool currentToggle = false;
                    if (toggleState.Equals(ToggleState.On))
                    {
                        currentToggle = true;
                    }

                    if (toToggleToSet != currentToggle)
                    {
                        ((TogglePattern)pattern).Toggle();
                    }
                    return new KeywordResult(KeywordResults.Pass);
                }
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "Current object is not toggle button";
                fail.ScreenShot = GetScreenShot(FullScreenBound());
                return fail;
            }
            catch (Exception ex)
            {
                Logger.Error("Error during Toggle", ex);
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error during Toggle";
                error.AdditionalInfo = ex.ToString();
                return error;
            }
        }

        [KeywordDescription("Set toggle state")]
        [KeywordDisplayName("Set Toggle")]
        [KeywordParameters("toggleAutomationID", "Automation ID of toggle")]
        [KeywordParameters("index", "index of toggle button. Starts from 1.")]
        [KeywordParameters("toggleValue", "On or Off")]
        [SampleScript("Windows.Set Toggle (SystemSettings_Accessibility_IsAnimationsEnabled_ToggleSwitch,1,Off)")]
        public KeywordResult SetToggle(string toggleAutomationID, string index, string toggleOnOff)
        {
            bool toToggleToSet = true;
            if (toggleOnOff.Equals("On", StringComparison.CurrentCultureIgnoreCase))
            {
                toToggleToSet = true;
            }
            else if (toggleOnOff.Equals("Off", StringComparison.CurrentCultureIgnoreCase))
            {
                toToggleToSet = false;
            }
            else
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"Option value of toggle button must be On or Off";
                return error;
            }

            if (!int.TryParse(index.Trim(), out int iIdx))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"Index must be a number";
                return error;
            }

            AutomationElementCollection toggleButtons = SelectElements(_target, AutomationElement.AutomationIdProperty, toggleAutomationID, TimeSpan.FromSeconds(3));
            if (toggleButtons.Count < iIdx)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Can not find element with given automation id (index out of range) : {toggleAutomationID}, {index}";
                fail.ScreenShot = GetScreenShot(FullScreenBound());
                return fail;
            }

            AutomationElement toggleButton = toggleButtons[--iIdx];

            try
            {
                object pattern = null;
                if (toggleButton.TryGetCurrentPattern(TogglePattern.Pattern, out pattern))
                {
                    ToggleState toggleState = ((TogglePattern)pattern).Current.ToggleState;
                    Logger.Trace($"Current Toggle State : {toggleState}");
                    bool currentToggle = false;
                    if (toggleState.Equals(ToggleState.On))
                    {
                        currentToggle = true;
                    }

                    if (toToggleToSet != currentToggle)
                    {
                        ((TogglePattern)pattern).Toggle();
                    }
                    return new KeywordResult(KeywordResults.Pass);
                }
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "Current object is not toggle button";
                fail.ScreenShot = GetScreenShot(FullScreenBound());
                return fail;
            }
            catch (Exception ex)
            {
                Logger.Error("Error during Toggle", ex);
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error during Toggle";
                error.AdditionalInfo = ex.ToString();
                return error;
            }
        }

        [KeywordDescription("Add certificate to Root store")]
        [KeywordDisplayName("Add Certificate")]
        [KeywordParameters("certificate path", "Path of certificate file")]
        [SampleScript("Windows.Add Certificate(Path of the certificate file)")]
        public KeywordResult AddCertificate(string certificatePath)
        {
            certificatePath = Support.Utils.GetAbsolutePath(certificatePath, CommonExecutionInfo.ScriptFolder);

            if (!File.Exists(certificatePath))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"Certificate is not exist in {certificatePath}";
                Logger.Error(error.Output);
                return error;
            }

            try
            {
                X509Certificate2 certificate = new X509Certificate2(certificatePath);
                using (X509Store store = new X509Store(StoreName.Root))
                {
                    store.Open(OpenFlags.ReadWrite);
                    store.Add(certificate);
                    store.Close();
                }
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error during adding certificate to Root store";
                Logger.Error(error.Output, ex);
                error.AdditionalInfo = ex.ToString();
                return error;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Set radiobutton state with name property")]
        [KeywordDisplayName("Set Radio Button With Name")]
        [KeywordParameters("radioButtonAutomationName", "Name property of radio button")]
        [KeywordParameters("radioButtonOnOff", "On")]
        [SampleScript("Windows.Set Radio Button With Name(My devices only,On)")]
        public KeywordResult SetRadioButtonWithName(string radioButtonAutomationName, string radioButtonOnOff)
        {
            bool toRadioButtonToSet = true;
            if (radioButtonOnOff.Equals("On", StringComparison.CurrentCultureIgnoreCase))
            {
                toRadioButtonToSet = true;
            }
            else if (radioButtonOnOff.Equals("Off", StringComparison.CurrentCultureIgnoreCase))
            {
                toRadioButtonToSet = false;
            }
            else
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Option value of radio button must be On or Off";
                return fail;
            }
            AutomationElement radioButton = SelectElement(_target, AutomationElement.NameProperty, radioButtonAutomationName, TimeSpan.FromSeconds(2));

            try
            {
                object pattern = null;
                if (radioButton.TryGetCurrentPattern(SelectionItemPattern.Pattern, out pattern))
                {
                    bool radioButtonState = ((SelectionItemPattern)pattern).Current.IsSelected;
                    Logger.Trace($"Current radio button State : {radioButtonState}");
                    bool currentRadioButtonState = false;
                    if (radioButtonState.Equals(true))
                    {
                        currentRadioButtonState = true;
                    }

                    if (toRadioButtonToSet != currentRadioButtonState)
                    {
                        ((SelectionItemPattern)pattern).Select();
                    }
                    return new KeywordResult(KeywordResults.Pass);
                }
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "Current object is not radio button";
                fail.ScreenShot = GetScreenShot(FullScreenBound());
                return fail;
            }
            catch (Exception ex)
            {
                Logger.Error("Error during SetRadioButton", ex);
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error during SetRadioButton";
                error.AdditionalInfo = ex.ToString();
                return error;
            }
        }

        [KeywordDescription("Set radiobutton state with Id property")]
        [KeywordDisplayName("Set Radio Button With Id")]
        [KeywordParameters("radioButtonAutomationID", "Automation ID of radio button")]
        [KeywordParameters("radioButtonOnOff", "On")]
        [SampleScript("Windows.Set Radio Button With Id(SystemSettings_SharedExperiences_OtherSharedAuthzLevel2_ThirdOptionRadioButton,On)")]
        public KeywordResult SetRadioButtonWithId(string radioButtonAutomationID, string radioButtonOnOff)
        {
            bool toRadioButtonToSet = true;
            if (radioButtonOnOff.Equals("On", StringComparison.CurrentCultureIgnoreCase))
            {
                toRadioButtonToSet = true;
            }
            else if (radioButtonOnOff.Equals("Off", StringComparison.CurrentCultureIgnoreCase))
            {
                toRadioButtonToSet = false;
            }
            else
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Option value of radio button must be On or Off";
                return fail;
            }
            AutomationElement radioButton = SelectElement(_target, AutomationElement.AutomationIdProperty, radioButtonAutomationID, TimeSpan.FromSeconds(2));

            try
            {
                object pattern = null;
                if (radioButton.TryGetCurrentPattern(SelectionItemPattern.Pattern, out pattern))
                {
                    bool radioButtonState = ((SelectionItemPattern)pattern).Current.IsSelected;
                    Logger.Trace($"Current radio button State : {radioButtonState}");
                    bool currentRadioButtonState = false;
                    if (radioButtonState.Equals(true))
                    {
                        currentRadioButtonState = true;
                    }

                    if (toRadioButtonToSet != currentRadioButtonState)
                    {
                        ((SelectionItemPattern)pattern).Select();
                    }
                    return new KeywordResult(KeywordResults.Pass);
                }
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "Current object is not radio button";
                fail.ScreenShot = GetScreenShot(FullScreenBound());
                return fail;
            }
            catch (Exception ex)
            {
                Logger.Error("Error during SetRadioButton", ex);
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error during SetRadioButton";
                error.AdditionalInfo = ex.ToString();
                return error;
            }
        }


        [KeywordDescription("Check toggle state On or Off for given automation ID with index")]
        [KeywordDisplayName("CheckToggleStateWithIdAndIndex")]
        [KeywordParameters("toggleAutomationID", "Automation ID of toggle or Name")]
        [KeywordParameters("index", "index position of index")]
        [KeywordParameters("toggleValue", "On or Off")]
        [SampleScript("Windows.CheckToggleStateWithIdAndIndex(SystemSettings_Accessibility_IsAnimationsEnabled_ToggleSwitch, 1, Off)")]
        public KeywordResult CheckToggleStateWithIdAndIndex(string toggleAutomationID, string index, string toggleOnOff)
        {
            bool toToggleToSet = true;
            if (toggleOnOff.Equals("On", StringComparison.CurrentCultureIgnoreCase))
            {
                toToggleToSet = true;
            }
            else if (toggleOnOff.Equals("Off", StringComparison.CurrentCultureIgnoreCase))
            {
                toToggleToSet = false;
            }
            else
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Option value of toggle button must be On or Off";
                return fail;
            }
            if (!int.TryParse(index.Trim(), out int iIdx))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"Index must be a number";
                return error;
            }

            try
            {
                AutomationElementCollection toggleButtons = SelectElements(_target, AutomationElement.AutomationIdProperty, toggleAutomationID, TimeSpan.FromSeconds(3));

                if (toggleButtons.Count < iIdx)
                {
                    KeywordResult keyfail = new KeywordResult(KeywordResults.Fail);
                    keyfail.Output = $"Can not find element with given automation id (index out of range) : {toggleAutomationID}, {index}";
                    keyfail.ScreenShot = GetScreenShot(FullScreenBound());
                    return keyfail;
                }

                if (toggleButtons != null)
                {

                    object pattern = null;
                    if (toggleButtons[--iIdx].TryGetCurrentPattern(TogglePattern.Pattern, out pattern))
                    {
                        ToggleState toggleState = ((TogglePattern)pattern).Current.ToggleState;
                        Logger.Trace($"Current Toggle State : {toggleState}");

                        if (toggleState.ToString() == toggleOnOff)
                        {
                            return new KeywordResult(KeywordResults.Pass);
                        }

                    }
                }
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "Current toggle state is not matching with given toggle state";
                fail.ScreenShot = GetScreenShot(FullScreenBound());
                return fail;
            }
            catch (Exception ex)
            {
                Logger.Error("Error during CheckToggleStateWithIdAndIndex", ex);
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error during CheckToggleStateWithIdAndIndex";
                error.AdditionalInfo = ex.ToString();
                return error;
            }
        }

        [KeywordDescription("Closes an application whose window title contains the given text, e.g., EXCEL or HP Smart \r\n" +
        "Note: if we are closing Notepad in Windows 11, It closes all files in the application. \r\n" +
        "In Windows 10, It will close Only the Recently opened file.")]
        [KeywordDisplayName("Close Application")]
        [KeywordParameters("windowTitle", "Title of the applciation to Close")]
        [SampleScript("Windows.Close Application (HP Scan)")]
        public KeywordResult CloseApplication(string windowTitle)
        {
            KeywordResult keyWordResult = new KeywordResult(KeywordResults.Pass);
            _target = FindApplication(windowTitle);
            if (_target == null)
            {
                keyWordResult.Result = KeywordResults.Fail;
                keyWordResult.Output = "Can not find target application";
                keyWordResult.ScreenShot = GetScreenShot(FullScreenBound());
                return keyWordResult;
            }
            try
            {
                object pattern = null;
                if (_target.TryGetCurrentPattern(WindowPattern.Pattern, out pattern))
                {
                    ((WindowPattern)pattern).Close();
                }
            }
            catch (Exception ex)
            {
                keyWordResult.Result = KeywordResults.Error;
                keyWordResult.AdditionalInfo = ex.ToString();
                keyWordResult.ScreenShot = GetScreenShot(FullScreenBound());
                keyWordResult.Output = $"Failed to close the application: {ex.Message}";
                Logger.Error(keyWordResult.Output, ex);
                return keyWordResult;
            }
            return keyWordResult;
        }
        [KeywordDescription("Clear TextField in the application")]
        [KeywordDisplayName("Clear TextField")]
        [SampleScript("Windows.ClearTextField")]
        public KeywordResult ClearTextField()
        {
            var key = "^a{BACKSPACE}";
            try
            {
                SendKeys.SendWait(key);
            }
            catch (Exception ex)
            {
                KeywordResult result = new KeywordResult(KeywordResults.Error);
                result.Output = "Error during send key";
                result.AdditionalInfo = ex.ToString();
                Logger.Error("Error during send key", ex);
                return result;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Waits until the specified automation id appears on the screen and continues waiting until it disappears within the given time")]
        [KeywordDisplayName("Wait For ID Disappear")]
        [KeywordParameters("automationID", "automationid of object to wait")]
        [KeywordParameters("timeout", "Waiting time in seconds")]
        [SampleScript("Windows.Wait For ID Disappear(LoadingSpinner ,10)")]
        public KeywordResult WaitForIdDisappear(string automationId, string timeout)
        {
            try
            {
                TimeSpan waitTime = TimeSpan.FromSeconds(int.Parse(timeout));
                bool idFoundAtLeastOnce = false;
                DateTime startTime = DateTime.Now;
                AutomationElement root = AutomationElement.RootElement;

                while ((DateTime.Now - startTime) < waitTime)
                {
                    var element = root.FindFirst(
                        TreeScope.Descendants,
                        new PropertyCondition(
                            AutomationElement.AutomationIdProperty,
                            automationId));

                    if (element != null && !element.Current.IsOffscreen)
                    {
                        idFoundAtLeastOnce = true;
                        break;
                    }

                    Thread.Sleep(500);
                }

                if (!idFoundAtLeastOnce)
                {
                    return new KeywordResult(KeywordResults.Fail)
                    {
                        Output = $"AutomationId '{automationId}' did not appear within {timeout} seconds."
                    };
                }

                while ((DateTime.Now - startTime) < waitTime)
                {
                    var element = root.FindFirst(
                        TreeScope.Descendants,
                        new PropertyCondition(
                            AutomationElement.AutomationIdProperty,
                            automationId));

                    if (element == null || element.Current.IsOffscreen)
                    {
                        return new KeywordResult(KeywordResults.Pass)
                        {
                            Output = $"AutomationId '{automationId}' appeared and then disappeared successfully within {(DateTime.Now - startTime):ss\\.fff} seconds."
                        };
                    }

                    Thread.Sleep(500);
                }

                return new KeywordResult(KeywordResults.Fail)
                {
                    Output = $"AutomationId '{automationId}' appeared but did not disappear within {waitTime} seconds."
                };
            }
            catch (Exception ex)
            {
                return new KeywordResult(KeywordResults.Fail)
                {
                    Output = $"Error while waiting for automation id to disappear: {ex.Message}"
                };
            }
        }


        #region "Memory Monitors"

        [KeywordDescription("Initialize Memory monitoring for Windows Device")]
        [KeywordDisplayName("Start Memory Monitoring")]
        [SampleScript("Windows.Start Memory Monitoring")]
        public KeywordResult StartMemoryMonitoring()
        {
            string csvPath;

            _testCaseName = CommonExecutionInfo.GetVariable("_tcName");
            if (CommonExecutionInfo.CurrentRepeatCount > 0)
            {
                csvPath = Path.Combine(Directory.GetParent(_outputDir).FullName, $"Windows_Memory_Usage.csv");
                Logger.Trace($"Current Repeat Count is greater than 0. CSV Path is : {csvPath}");
            }
            else
            {
                csvPath = Path.Combine(_outputDir, $"{_testCaseName}_WindowsMemoryMonitoring_{DateTime.Now.ToString("yyMMdd_HHmmssff")}.csv");
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
        [SampleScript("Windows.Collect Memory Info (${R})")]
        public KeywordResult CollectMemoryInfo(string checkPoint)
        {
            string processName = string.Empty;
            Dictionary<string, long> memInfo = new Dictionary<string, long>();
            try
            {
                long rowTotal = 0;
                Process[] processes = Process.GetProcesses();

                foreach (Process process in processes)
                {
                    processName = process.ProcessName + "#" + process.Id;
                    memInfo.Add(processName, process.PagedSystemMemorySize64);
                    rowTotal += process.PagedSystemMemorySize64;
                }
                memInfo.Add("Total", rowTotal);

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

        [KeywordDescription("Draw Windows Device Memory Usage Graph")]
        [KeywordDisplayName("Draw Memory Usage Graph")]
        [SampleScript("Windows.Draw Memory Usage Graph")]
        public KeywordResult DrawMemoryUsageGraph()
        {
            try
            {
                ChartCreator chart = new ChartCreator(_memoryUsage.GetCSVPath());
                string chartImage = Path.Combine(_outputDir, $"{_testCaseName}_WindowsMemory.png");

                chart.SaveImage(chartImage, System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line, true, "Total");

                if (CommonExecutionInfo.CurrentRepeatCount > 0)
                {
                    File.Copy(chartImage, Path.Combine(Directory.GetParent(_outputDir).FullName, $"{_testCaseName}_Windows_Memory_Usage.png"), true);
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

        #endregion

        #region PrintQueue

        [KeywordDescription("Check if job is exist in print queue with part of job name")]
        [KeywordDisplayName("Print Job Is Exist")]
        [KeywordParameters("printQueueName", "Name of the print queue")]
        [KeywordParameters("partOfJobName", "Part of job name to find. Finding job which job name contains this value.")]
        [SampleScript("Windows.Print Job Is Exist (Camden,word)")]
        public KeywordResult PrintJobIsExist(string printQueueName, string partOfJobName)
        {
            PrintQueue queue = PrintQueueUtils.GetPrintQueue(printQueueName);
            if (queue == null)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"No print queue with given name : {printQueueName}";
                Logger.Error(error.Output);
                return error;
            }
            PrintSystemJobInfo job = PrintQueueUtils.GetJob(queue, partOfJobName, false);
            if (job == null)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"No job with given {partOfJobName} in print queue {printQueueName}";
                return fail;
            }

            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Check if any job is exist in print queue")]
        [KeywordDisplayName("Print Job Is Exist")]
        [KeywordParameters("printQueueName", "Name of the print queue")]
        [SampleScript("Windows.Print Job Is Exist (Camden)")]
        public KeywordResult PrintJobIsExist(string printQueueName)
        {
            PrintQueue queue = PrintQueueUtils.GetPrintQueue(printQueueName);
            if (queue == null)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"No print queue with given name : {printQueueName}";
                Logger.Error(error.Output);
                return error;
            }

            int count = PrintQueueUtils.GetJobCount(queue);

            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            if (count > 0)
            {
                result.Output = $"Number of job(s) in print queue [{printQueueName}] : {count}";
                return result;
            }
            result.Output = $"There is no job in print queue [{printQueueName}]";
            result.Result = KeywordResults.Fail;
            return result;
        }

        [KeywordDescription("Pause current job in print queue")]
        [KeywordDisplayName("Pause Current Print Job")]
        [KeywordParameters("printQueueName", "Name of the print queue")]
        [SampleScript("Windows.Pause Current Print Job (Camden)")]
        public KeywordResult PauseCurrentPrintJob(string printQueueName)
        {
            PrintQueue queue = PrintQueueUtils.GetPrintQueue(printQueueName);
            if (queue == null)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"No print queue with given name : {printQueueName}";
                Logger.Error(error.Output);
                return error;
            }
            PrintSystemJobInfo job = PrintQueueUtils.GetCurrentJob(queue);
            if (job == null)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"No job in print queue : {printQueueName}";
                Logger.Error(error.Output);
                return error;
            }
            Logger.Debug($"Got current Print job : Name ({job.JobName}");
            if (PrintQueueUtils.PauseJob(job))
            {
                return new KeywordResult(KeywordResults.Pass);
            }
            return new KeywordResult(KeywordResults.Fail);
        }

        [KeywordDescription("Resume current job in print queue")]
        [KeywordDisplayName("Resume Current Print Job")]
        [KeywordParameters("printQueueName", "Name of the print queue")]
        [SampleScript("Windows.Resume Current Print Job (Camden)")]
        public KeywordResult ResumeCurrentPrintJob(string printQueueName)
        {
            PrintQueue queue = PrintQueueUtils.GetPrintQueue(printQueueName);
            if (queue == null)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"No print queue with given name : {printQueueName}";
                Logger.Error(error.Output);
                return error;
            }
            PrintSystemJobInfo job = PrintQueueUtils.GetCurrentJob(queue);
            if (job == null)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"No job in print queue : {printQueueName}";
                Logger.Error(error.Output);
                return error;
            }
            Logger.Debug($"Got current Print job : Name ({job.JobName}");
            if (PrintQueueUtils.ResumeJob(job))
            {
                return new KeywordResult(KeywordResults.Pass);
            }
            return new KeywordResult(KeywordResults.Fail);
        }

        [KeywordDescription("Delete current job in print queue")]
        [KeywordDisplayName("Delete Current Print Job")]
        [KeywordParameters("printQueueName", "Name of the print queue")]
        [SampleScript("Windows.Delete Current Print Job (Camden)")]
        public KeywordResult DeleteCurrentPrintJob(string printQueueName)
        {
            PrintQueue queue = PrintQueueUtils.GetPrintQueue(printQueueName);
            if (queue == null)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"No print queue with given name : {printQueueName}";
                Logger.Error(error.Output);
                return error;
            }
            PrintSystemJobInfo job = PrintQueueUtils.GetCurrentJob(queue);
            if (job == null)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"No job in print queue : {printQueueName}";
                Logger.Error(error.Output);
                return error;
            }
            Logger.Debug($"Got current Print job : Name ({job.JobName}");
            if (PrintQueueUtils.DeleteJob(job))
            {
                return new KeywordResult(KeywordResults.Pass);
            }
            return new KeywordResult(KeywordResults.Fail);
        }

        [KeywordDescription("Delete all jobs in print queue")]
        [KeywordDisplayName("Delete All Jobs In Print Queue")]
        [KeywordParameters("printQueueName", "Name of the print queue")]
        [SampleScript("Windows.Delete All Jobs In Print Queue (Camden)")]
        public KeywordResult DeleteAllJobsInPrintQueue(string printQueueName)
        {
            PrintQueue queue = PrintQueueUtils.GetPrintQueue(printQueueName);
            if (queue == null)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"No print queue with given name : {printQueueName}";
                Logger.Error(error.Output);
                return error;
            }

            if (PrintQueueUtils.EmptyQueue(queue))
            {
                return new KeywordResult(KeywordResults.Pass);
            }
            return new KeywordResult(KeywordResults.Fail);
        }

        [KeywordDescription("Print File To Print Queue Note: for PDF files if not default application is set then it will open in default browser \r\n Note: For Pdf format in 'PrintFileToPrintQueue' keyword if Adobe Acrobat is not set as default app then it will open in default browser and the keyword gives error as \"No application is associated with the specified file for this operation\". So use the keyword PrintPDForImageWithApplication by specifying the full path of \"Adobe Acrobat.exe\"")]
        [KeywordDisplayName("Print File To Print Queue")]
        [KeywordParameters("FilePath", "Name of the print queue")]
        [SampleScript("Windows.Print File To Print Queue (C:\\Users\\XAppanna\\Desktop\\ddd.txt,Busch)")]
        public KeywordResult PrintFileToPrintQueue(string FilePath, string PrintQueueName)
        {
            string[] fileExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".pptx", ".rtf", ".ppt", ".pptm", ".xltx", ".txt" };
            string allowedFileExtensions = String.Join(",", fileExtensions);
            string checkExtensions;
            FilePath = Support.Utils.GetAbsolutePath(FilePath, CommonExecutionInfo.ScriptFolder);
            if (string.IsNullOrEmpty(Path.GetDirectoryName(FilePath)) || (string.IsNullOrEmpty(PrintQueueName)) || string.IsNullOrWhiteSpace(PrintQueueName))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"PrintQueueName cannot be empty: {PrintQueueName}";
                Logger.Error(error.Output);
                return error;
            }
            checkExtensions = Path.GetExtension(FilePath);
            PrinterSettings printerSettings = new PrinterSettings();
            printerSettings.PrinterName = PrintQueueName;
            if (!printerSettings.IsValid)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"InValid PrintQueueName: {PrintQueueName}";
                Logger.Error(error.Output);
                return error;
            }
            if (!File.Exists(FilePath))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"File does not exists in filepath or FilePath cannot be empty Or Invalid FilePath: {FilePath}";
                Logger.Error(error.Output);
                return error;
            }
            if (!allowedFileExtensions.Contains(checkExtensions.ToLower()))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"File With No ExtensionName or Invalid File Extension. Allowed extensions are : {allowedFileExtensions}";
                Logger.Error(error.Output);
                return error;
            }
            else
            {
                PrintQueueUtils.PrintFile(FilePath, PrintQueueName);
                return new KeywordResult(KeywordResults.Pass);
            }

        }

        [KeywordDescription("Open PDF or Image file with respective application exe path. So that the file will be printed to the printer. <p> Note: For PDF files if default application is not set then will get error.")]
        [KeywordDisplayName("Print PDF or Image With Application")]
        [KeywordParameters("appExecutablePath", "Executable(*.exe) path to execute and take control. Ex - Give full path for acrobat reader.")]
        [KeywordParameters("filePath", "Image file path has with or without space to print. Ex - .pdf|.jpg|.jpeg|.png|.bmp|.webp|.gif|.tiff")]
        [KeywordParameters("printQueueName", "Name of the print queue or printer name")]
        [SampleScript("Windows.Print PDF or Image With Application(mspaint.exe,C:\\Users\\MKavyash\\Pictures\\images_formate\\Info_Screenshot_240715_17183745.jpeg,Camden)")]

        public KeywordResult PrintPDForImageWithApplication(string appExecutablePath, string filePath, string printQueueName)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            filePath = Support.Utils.GetAbsolutePath(filePath, CommonExecutionInfo.ScriptFolder);

            if (string.IsNullOrEmpty(Path.GetDirectoryName(filePath)) || (!File.Exists(filePath)))
            {
                result = new KeywordResult(KeywordResults.Fail);
                result.Output = $"File does not exists in filepath or FilePath cannot be empty Or Invalid FilePath: {filePath}";
                Logger.Error(result.Output);
                return result;
            }

            if (string.IsNullOrEmpty(printQueueName) || string.IsNullOrWhiteSpace(printQueueName))
            {
                result = new KeywordResult(KeywordResults.Fail);
                result.Output = $"PrintQueueName cannot be empty: {printQueueName}";
                Logger.Error(result.Output);
                return result;
            }
            PrinterSettings printerSettings = new PrinterSettings();
            printerSettings.PrinterName = printQueueName;

            if (!printerSettings.IsValid)
            {
                result = new KeywordResult(KeywordResults.Fail);
                result.Output = $"No print queue with given name : {printQueueName}";
                Logger.Error(result.Output);
                return result;
            }

            try
            {
                string fileExtension = Path.GetExtension(filePath).ToLower();
                string arguments = "";

                if (fileExtension == ".pdf")
                {
                    arguments = string.Format("/h /t \"{0}\" \"{1}\"", filePath, printQueueName);
                }

                else if (fileExtension == ".jpg" || fileExtension == ".jpeg" ||
                         fileExtension == ".png" || fileExtension == ".tiff" ||
                         fileExtension == ".webp" || fileExtension == ".bmp" ||
                         fileExtension == ".gif")
                {
                    arguments = $"/P \"{filePath}\" /PT {printQueueName}";
                }
                else
                {
                    Logger.Error("The keyword does not support for the fileextension : " + fileExtension + ". The supported file formats are .pdf, .jpg, .jpeg, .bmp, .gif, .png, .tiff, .webp");
                    result = new KeywordResult(KeywordResults.Fail);
                    result.Output = "The keyword does not support for the fileextension : " + fileExtension + ". The supported file formats are .pdf, .jpg, .jpeg, .bmp, .gif, .png, .tiff, .webp";
                    return result;
                }

                ProcessStartInfo processStartInfo = new ProcessStartInfo(appExecutablePath);
                processStartInfo.Arguments = arguments;
                processStartInfo.UseShellExecute = false;
                processStartInfo.CreateNoWindow = true;
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processStartInfo.ErrorDialog = false;

                Process process = new Process();
                process.StartInfo = processStartInfo;
                process.Start();
                Thread.Sleep(5000);
                return result;
            }

            catch (Exception ex)
            {
                Logger.Error("Error during the Print", ex);
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error during the Print";
                error.AdditionalInfo = ex.ToString();
                return error;
            }
        }

        [KeywordDescription("Wait For WPath")]
        [KeywordDisplayName("Wait For WPath")]
        [KeywordParameters("WPath", "Wait for Wpath element exists or not. ")]
        [KeywordParameters("TimeOut", "Time Out in seconds")]
        [SampleScript("Windows.Wait For WPath (//Button[@Name='Detect'],30)")]
        public KeywordResult WaitForWPath(string wPath, string timeOut)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                AutomationElement toClick = SelectElementByWPath(_target, wPath, TimeSpan.FromSeconds(Convert.ToInt32(timeOut)));
                if (toClick == null)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given WPath ID : {wPath}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }
                else
                {
                    result.Output = $"Found element with given WPath : {wPath}";
                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error during WaitForWPath", ex);
                result.Result = KeywordResults.Error;
                result.Output = "Error during WaitForWPath";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                return result;
            }
        }
        [KeywordDescription("Get text of given wpath and store to given variable")]
        [KeywordDisplayName("Get Text With WPath")]
        [KeywordParameters("wPath", "wPath of object to compare See https://github.com/reitn/wpath/blob/master/README.md for WPath usage.")]
        [KeywordParameters("saveTo", "variable name (ex. ${Buffer}) to store value")]
        [SampleScript("Windows.Get Text With WPath(//Button[@Name='Detect'],${ex})")]
        public KeywordResult GetTextWithWPath(string wPath, string saveTo)
        {
            // get element based on wPath
            AutomationElement target = SelectElementByWPath(_target, wPath, TimeSpan.FromSeconds(2)); ;

            if (target == null)
            {
                KeywordResult kr = new KeywordResult(KeywordResults.Fail);
                kr.Output = $"Can not find element with wpath : {wPath}";
                kr.ScreenShot = GetScreenShot(FullScreenBound());
                return kr;
            }

            try
            {
                string winText = target.Current.Name;
                if (string.IsNullOrEmpty(winText))
                {
                    KeywordResult kr = new KeywordResult(KeywordResults.Fail);
                    kr.Output = $"Can not get text of element with wPath : {wPath}";
                    kr.ScreenShot = GetScreenShot(FullScreenBound());
                    return kr;
                }
                else
                {
                    CommonExecutionInfo.SetVariable(saveTo, winText);
                    KeywordResult kr = new KeywordResult(KeywordResults.Pass);
                    kr.Output = $"{saveTo} : {winText}";
                    return kr;
                }
            }

            catch (Exception ex)
            {
                KeywordResult kr = new KeywordResult(KeywordResults.Error);
                Logger.Error("Error during Get Text With Wpath", ex);
                kr.Output = $"Error during Get Text With Wpath";
                kr.ScreenShot = GetScreenShot(FullScreenBound());
                kr.AdditionalInfo = ex.ToString();
                return kr;
            }
        }

        [KeywordDescription("Check Radio button state True or False for given automation ID")]
        [KeywordDisplayName("CheckRadioButtonState")]
        [KeywordParameters("RadioButton AutomationID", "Automation ID of option button or Name")]
        [KeywordParameters("OptionValue", "True or False")]
        [SampleScript("Windows.CheckRadioButtonState (Microsoft.QuietHoursProfile.Unrestricted_Button,True)")]
        public KeywordResult CheckRadioButtonState(string radioButtonAutomationID, string radioButtonValue)
        {
            bool radioButonToSet = true;
            if (radioButtonValue.Equals("True", StringComparison.CurrentCultureIgnoreCase))
            {
                radioButonToSet = true;
            }
            else if (radioButtonValue.Equals("False", StringComparison.CurrentCultureIgnoreCase))
            {
                radioButonToSet = false;
            }
            else
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Option value of Radio button must be True or False";
                return fail;
            }

            try
            {
                AutomationElement radioButton = SelectElement(_target, AutomationElement.AutomationIdProperty, radioButtonAutomationID, TimeSpan.FromSeconds(3));

                if (radioButton != null)
                {
                    object pattern = null;
                    if (radioButton.TryGetCurrentPattern(SelectionItemPattern.Pattern, out pattern))
                    {
                        bool radioButtonState = ((SelectionItemPattern)pattern).Current.IsSelected;
                        Logger.Trace($"Current Radio Button State : {radioButtonState}");

                        if (radioButtonState.ToString().ToLower() == radioButtonValue.ToLower())
                        {
                            return new KeywordResult(KeywordResults.Pass);
                        }

                    }
                }

                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "Current Radio Button state is not matching with given state";
                fail.ScreenShot = GetScreenShot(FullScreenBound());
                return fail;
            }
            catch (Exception ex)
            {
                Logger.Error("Error during CheckRadioButtonState", ex);
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error during CheckRadioButtonState";
                error.AdditionalInfo = ex.ToString();
                return error;
            }
        }
        [KeywordDescription("Select item in combobox by its name and index position. The combobox should be expanded first using Click with automationId or name property.")]
        [KeywordDisplayName("SelectComboboxItemByNameAndIndex")]
        [KeywordParameters("name", "Name of the combobox item to select")]
        [KeywordParameters("index", "Position of the combobox item")]
        [SampleScript("Windows.SelectComboboxItemByNameAndIndex(HP.PSA.V4.Shared.ViewModel.Parts.OptionPartViewModel, 3)")]
        public KeywordResult SelectComboboxItemByNameAndIndex(string name, string index)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                AutomationElement rootElement = AutomationElement.RootElement;

                PropertyCondition nameCondition = new PropertyCondition(AutomationElement.NameProperty, name);
                AutomationElementCollection targetElement = rootElement.FindAll(TreeScope.Descendants, nameCondition);

                if (!int.TryParse(index.Trim(), out int iIdx))
                {
                    KeywordResult error = new KeywordResult(KeywordResults.Error);
                    error.Output = $"Index must be a number";
                    return error;
                }

                if (targetElement.Count < iIdx)
                {
                    KeywordResult keyfail = new KeywordResult(KeywordResults.Fail);
                    keyfail.Output = $"Can not find element with given automation name (index out of range) : {targetElement}, {index}";
                    keyfail.ScreenShot = GetScreenShot(FullScreenBound());
                    return keyfail;
                }

                if (targetElement != null)
                {
                    object pattern = null;
                    if (targetElement[Convert.ToInt32(iIdx)].TryGetCurrentPattern(SelectionItemPattern.Pattern, out pattern))
                    {
                        ((SelectionItemPattern)pattern).Select();
                        return result;
                    }
                }
                else
                {
                    KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                    fail.Output = $"Could not find any combobox items matching the name '{name}'. The collection is empty or the name is incorrect.";
                    fail.ScreenShot = GetScreenShot(FullScreenBound());
                    return fail;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error during SelectComboboxItemByNameAndIndex", ex);
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error during SelectComboboxItemByNameAndIndex";
                error.AdditionalInfo = ex.ToString();
                return error;
            }
            return result;
        }


        [KeywordDescription("Expands the ComboBox using the comboBoxId, iterates through each list item, selects the list item by name, and verifies if the selected item matches the one requested by the user.")]
        [KeywordDisplayName("SelectItemByComboboxIdAndName")]
        [KeywordParameters("comboBoxId", "Automation Id of the Combobox")]
        [KeywordParameters("comboboxListItemName", "Automation name of the combobox list item")]
        [KeywordParameters("comboBoxSelectedName", "Automation name of the selected combobox list item")]
        [SampleScript("SelectItemByComboboxIdAndName(Part.PageMediaSize,HP.PSA.V4.Shared.ViewModel.Parts.OptionPartViewModel,Option.ISOC0)")]
        public KeywordResult SelectItemByComboboxIdAndName(string comboBoxId, string comboboxListItemName, string comboBoxSelectedName)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);

            try
            {
                AutomationElement rootElement = AutomationElement.RootElement;
                int retryCount = 1;

                // Retry mechanism to find the ComboBox
                AutomationElement comboBoxElement = null;
                while (retryCount <= 5)
                {
                    retryCount++;
                    PropertyCondition idCondition = new PropertyCondition(AutomationElement.AutomationIdProperty, comboBoxId);
                    PropertyCondition controlTypeCondition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ComboBox);
                    AndCondition condition = new AndCondition(idCondition, controlTypeCondition);

                    comboBoxElement = rootElement.FindFirst(TreeScope.Descendants, condition);

                    if (comboBoxElement == null)
                    {
                        Logger.Debug("No ComboBox with id : " + comboBoxId);
                        return new KeywordResult(KeywordResults.Fail) { Output = "ComboBox not found after retries." };
                    }

                    // Ensure the ComboBox is expanded
                    object expandPattern = null;
                    if (comboBoxElement.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out expandPattern))
                    {
                        ExpandCollapsePattern expandCollapse = (ExpandCollapsePattern)expandPattern;
                        if (expandCollapse.Current.ExpandCollapseState != ExpandCollapseState.Expanded)
                        {
                            expandCollapse.Expand();
                            Logger.Debug("ComboBox Expanded");
                            Thread.Sleep(500); // Ensure UI updates before proceeding
                        }
                    }

                    // Find list items within the ComboBox instead of all descendants
                    PropertyCondition listItemNameCondition = new PropertyCondition(AutomationElement.NameProperty, comboboxListItemName);
                    AutomationElementCollection comboBoxListItems = comboBoxElement.FindAll(TreeScope.Descendants, listItemNameCondition);

                    Logger.Debug("ComboBox list items count: " + comboBoxListItems.Count);

                    foreach (AutomationElement comboBoxItem in comboBoxListItems)
                    {
                        if (comboBoxItem.Current.IsEnabled) // Check for interactive list item
                        {
                            Logger.Debug("ComboBox list item: " + comboBoxItem.Current.Name);

                            object comboBoxItemPattern = null;
                            if (comboBoxItem.TryGetCurrentPattern(SelectionItemPattern.Pattern, out comboBoxItemPattern))
                            {
                                try
                                {
                                    ((SelectionItemPattern)comboBoxItemPattern).Select();
                                }
                                catch (ElementNotAvailableException ex)
                                {
                                    Logger.Debug("Element became unavailable, retrying...");
                                    Thread.Sleep(500);
                                    continue; // Retry loop
                                }
                            }

                            // Verify selection
                            comboBoxElement = rootElement.FindFirst(TreeScope.Descendants, new AndCondition(
                                new PropertyCondition(AutomationElement.AutomationIdProperty, comboBoxId),
                                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ComboBox)
                            ));

                            if (comboBoxElement != null && comboBoxElement.Current.Name == comboBoxSelectedName)
                            {
                                Logger.Debug(comboBoxSelectedName + " is selected");
                                return result;
                            }
                            else
                            {
                                // Ensure the ComboBox is expanded again if needed
                                if (expandPattern != null && ((ExpandCollapsePattern)expandPattern).Current.ExpandCollapseState != ExpandCollapseState.Expanded)
                                {
                                    ((ExpandCollapsePattern)expandPattern).Expand();
                                    Logger.Debug("ComboBox Expanded Again");
                                }
                            }
                        }
                    }
                }

                result = new KeywordResult(KeywordResults.Fail);
                result.Output = "Unable to select the given comboxbox listitem with in 5 retries";
            }
            catch (Exception ex)
            {
                Logger.Error("Error during SelectItemByComboboxIdAndName", ex);
                return new KeywordResult(KeywordResults.Error)
                {
                    Output = "Error during SelectItemByComboboxIdAndName",
                    AdditionalInfo = ex.ToString()
                };
            }

            return result;
        }

        [KeywordDescription("Check if any UI element contains partial text in its Name")]
        [KeywordDisplayName("Check Screen Contains Partial Text")]
        [KeywordParameters("text", "Partial text to check on screen")]
        [SampleScript("Windows.Check Screen Contains Partial Text (Save)")]
        public KeywordResult CheckScreenContainsPartialText(string text)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);

            try
            {
                // Search all visible (on-screen) elements under the main target window
                Condition visibleCondition = new PropertyCondition(AutomationElement.IsOffscreenProperty, false);
                AutomationElementCollection elements = _target.FindAll(TreeScope.Descendants, visibleCondition);

                bool found = false;

                foreach (AutomationElement element in elements)
                {
                    string name = element.Current.Name;
                    if (!string.IsNullOrEmpty(name) && name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Partial text '{text}' not found on screen.";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                }
                else
                {
                    result.Output = $"Partial text '{text}' found on screen.";
                }
            }
            catch (Exception ex)
            {
                result.Result = KeywordResults.Error;
                result.Output = "Exception occurred while checking for partial text.";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                Logger.Error("Error during partial text check", ex);
            }

            return result;
        }
        [KeywordDescription("Hover the mouse over the UI element identified by visible text.")]
        [KeywordDisplayName("Mouse Hover On Text")]
        [KeywordParameters("text", "The visible text of the UI element to hover over.")]
        [SampleScript("Windows.Mouse Hover On Text(Print)")]
        public KeywordResult MousehoverOnText(string text)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                AutomationElement element = SelectElement(_target, AutomationElement.NameProperty, text, TimeSpan.FromSeconds(3));
                if (element == null)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given name : {text}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }

                Rect bounds = element.Current.BoundingRectangle;
                if (bounds.X == 0 && bounds.Y == 0 && bounds.Height == 0 && bounds.Width == 0)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Object is out of bound : {text}";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }

                if (!WaitForEndMove(element))
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = "Target object is changing position. Can not hover with mouse.";
                    result.ScreenShot = GetScreenShot(FullScreenBound());
                    return result;
                }

                MousePoint pt = GetClickablePoint(element);
                MouseOperations.SetCursorPosition(pt);
                Thread.Sleep(150); // allow hover state to trigger
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error("Error during mouse hover", ex);
                result.Result = KeywordResults.Error;
                result.Output = "Error during mouse hover";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot(FullScreenBound());
                return result;
            }
        }
        [KeywordDescription("Performs a double finger scroll action on Windows.")]
        [KeywordDisplayName("Stepper Scroll")]
        [KeywordParameters("direction", "Scroll direction: UP or DOWN")]
        [KeywordParameters("scrollCount", "Number of scroll steps")]
        [SampleScript("Windows.Stepper Scroll(v, 5)")]
        public KeywordResult StepperScroll(string direction, string scrollCount)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                int count = int.Parse(scrollCount);
                int scrollValue;

                // Implement UP (^) and DOWN (v) logic
                if (direction == "^")
                {
                    scrollValue = 150;     // Scroll UP
                }
                else if (direction.Equals("v", StringComparison.OrdinalIgnoreCase))
                {
                    scrollValue = -150;    // Scroll DOWN
                }
                else
                {
                    throw new Exception("Invalid direction. Use ^ for UP or v for DOWN.");
                }

                for (int i = 0; i < count; i++)
                {
                    MouseOperations.MouseWheel(scrollValue);
                    Thread.Sleep(150);
                }
            }
            catch (Exception ex)
            {
                result = new KeywordResult(KeywordResults.Fail);
            }

            return result;
        }

        [KeywordDescription("Scroll within the country selection list based on the provided xpath")]
        [KeywordDisplayName("Country Scroll")]
        [KeywordParameters("Text", "Text of element")]
        [KeywordParameters("direction", "Should be either ^ or v\r\n" + "^ (scroll up with a small amount), \r\n" + "v (scroll down with a small amount)")]
        [SampleScript("Windows.Country Scroll (India,^)")]
        public KeywordResult CountryScroll(string Text, string direction)
        {
            if (_target == null)
            {
                return new KeywordResult(KeywordResults.Fail, "No target application selected. Use 'Select Application' first.");
            }

            if (string.IsNullOrWhiteSpace(Text))
            {
                return new KeywordResult(KeywordResults.Fail, "Text to search must not be empty.");
            }

            try
            {
                // Determine intent: image template path vs plain text label
                string resolved = Support.Utils.GetAbsolutePath(Text, CommonExecutionInfo.ScriptFolder);
                bool looksLikeImage = !string.IsNullOrWhiteSpace(Path.GetExtension(resolved)) &&
                                      new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tif", ".tiff", ".webp" }
                                      .Contains(Path.GetExtension(resolved).ToLowerInvariant());

                int maxScrollAttempts = 150;
                int scrollAttempts = 0;

                if (looksLikeImage && File.Exists(resolved))
                {
                    // Image search with scrolling
                    var templateBytes = File.ReadAllBytes(resolved);

                    while (scrollAttempts < maxScrollAttempts)
                    {
                        var screenBytes = GetScreenShot(FullScreenBound());
                        if (screenBytes == null || screenBytes.Length == 0)
                        {
                            var fail = new KeywordResult(KeywordResults.Fail);
                            fail.Output = "Unable to capture screen for image matching.";
                            fail.ScreenShot = GetScreenShot(FullScreenBound());
                            return fail;
                        }

                        var matches = FindImage(screenBytes, templateBytes, 0.95f);

                        if (matches != null && matches.Count > 0)
                        {
                            // Found! Click and return success with screenshot
                            ClickText(Text);
                            return new KeywordResult(KeywordResults.Pass)
                            {
                                Output = $"Template image found and clicked: {resolved}",
                                ScreenShot = GetScreenShot(FullScreenBound())
                            };
                        }

                        // Not found, perform scroll
                        PerformScroll(direction);
                        Thread.Sleep(300);
                        scrollAttempts++;
                    }

                    // Image not found after all attempt
                    return new KeywordResult(KeywordResults.Fail, $"Template image not found after {scrollAttempts} scroll attempts: {resolved}");

                }
                else
                {
                    // Text search with scrolling
                    while (scrollAttempts < maxScrollAttempts)
                    {
                        var element = FindVisibleElementContainingText(Text);

                        if (element != null)
                        {
                            // Found! Click and return success with screenshot
                            ClickText(Text);
                            return new KeywordResult(KeywordResults.Pass)
                            {
                                Output = $"Element found and clicked: {Text}",
                                ScreenShot = GetScreenShot(FullScreenBound())
                            };
                        }

                        // Not found, perform scroll
                        PerformScroll(direction);
                        Thread.Sleep(300);
                        scrollAttempts++;
                    }

                    // Text not found after all attempts
                    var fail3 = new KeywordResult(KeywordResults.Fail);
                    fail3.Output = $"Element not found after {scrollAttempts} scroll attempts: {Text}";
                    fail3.ScreenShot = GetScreenShot(FullScreenBound());
                    return fail3;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error during CountryScroll", ex);
                var error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error during CountryScroll";
                error.AdditionalInfo = ex.ToString();
                error.ScreenShot = GetScreenShot(FullScreenBound());
                return error;
            }
        }

        private void PerformScroll(string direction)
        {
            // Get current cursor position
            //int originalX = Cursor.Position.X;
            //int originalY = Cursor.Position.Y;
            try
            {
                int scrollValue;

                // Determine scroll direction
                if (direction == "^")
                {
                    scrollValue = 150;     // Scroll UP
                }
                else if (direction.Equals("v", StringComparison.OrdinalIgnoreCase))
                {
                    scrollValue = -150;    // Scroll DOWN
                }
                else
                {
                    throw new Exception("Invalid direction. Use ^ for UP or v for DOWN.");
                }

                // Get the target window bounds
                //var targetBounds = _target.Current.BoundingRectangle;
                //int centerX = (int)(targetBounds.Left + (targetBounds.Width / 2));
                //int centerY = (int)(targetBounds.Top + (targetBounds.Height / 2));

                //// Move cursor to center of target window
                //Cursor.Position = new System.Drawing.Point(centerX, centerY);
                //Thread.Sleep(100);

                //// Perform scroll
                MouseOperations.MouseWheel(scrollValue);
                Thread.Sleep(150);
            }
            finally
            {
                // Restore cursor to original position
                //  Cursor.Position = new System.Drawing.Point(originalX, originalY);
            }
        }
        public List<ControlRecognitionResult> FindImage(byte[] originalImage, byte[] findImage, float thresdHold = 0.95f)
        {
            _visionEngine = new ImageProcessing(originalImage.ToBitmap());
            return _visionEngine.FindImage(findImage.ToBitmap(), thresdHold);
        }

        // Click visible text using GFVision OCR (image/bytes based). Falls back to UIA Name lookup.
        public KeywordResult ClickText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new KeywordResult(KeywordResults.Fail, "Text to click must not be empty.");
            }

            try
            {
                // 1) Prefer UIA name when available (fast and accurate)
                var element = FindVisibleElementContainingText(text);
                if (element != null)
                {
                    if (!WaitForEndMove(element))
                    {
                        Logger.Error("Target object is changing position. Can not click.");
                    }
                    if (!ClickElement(element))
                    {
                        var pt = GetClickablePoint(element);
                        MouseLeftClick(pt);
                    }
                    return new KeywordResult(KeywordResults.Pass);
                }

                // 2) OCR-based click from screenshot bytes using GFVision
                if (_vision == null)
                {
                    _vision = new GFVision();
                    // Optional tuning: sparse text, word level
                    _vision.ChangePageSegMode(Tesseract.PageSegMode.SparseText);
                    _vision.SetPageIterationLevel("block");
                }

                var screenBytes = GetScreenShot(FullScreenBound());
                if (screenBytes == null || screenBytes.Length == 0)
                {
                    var fail = new KeywordResult(KeywordResults.Fail);
                    fail.Output = "Unable to capture screen for OCR.";
                    fail.ScreenShot = GetScreenShot(FullScreenBound());
                    return fail;
                }

                List<OcrResult> ocrResults = _vision.RunOCR(screenBytes);
                if (ocrResults == null || ocrResults.Count == 0)
                {
                    var fail = new KeywordResult(KeywordResults.Fail);
                    fail.Output = "No OCR results found on screen.";
                    fail.ScreenShot = GetScreenShot(FullScreenBound());
                    return fail;
                }

                OcrResult match = null;
                foreach (var r in ocrResults)
                {
                    if (!string.IsNullOrEmpty(r.Text) &&
                        r.Text.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        match = r;
                        break;
                    }
                }

                if (match == null)
                {
                    var fail = new KeywordResult(KeywordResults.Fail);
                    fail.Output = $"Text not found by OCR: {text}";
                    fail.ScreenShot = GetScreenShot(FullScreenBound());
                    return fail;
                }

                // Click center of the OCR-bound rectangle
                var bound = match.Bound;
                int centerX = (int)(bound.X + bound.Width / 2);
                int centerY = (int)(bound.Y + bound.Height / 2);
                MouseLeftClick(new MousePoint(centerX, centerY));

            }
            catch (Exception ex)
            {
                Logger.Error("Error during ClickText", ex);
            }
            return new KeywordResult(KeywordResults.Pass);

        }

        // INSERT the following helper methods inside the class

        private AutomationElement FindVisibleElementContainingText(string text)
        {
            if (_target == null)
            {
                return null;
            }

            Condition visible = new PropertyCondition(AutomationElement.IsOffscreenProperty, false);
            AutomationElementCollection elements;

            try
            {
                elements = _target.FindAll(TreeScope.Descendants, visible);

            }
            catch
            {
                return null;
            }

            for (int i = 0; i < elements.Count; i++)
            {
                try
                {
                    string name = elements[i].Current.Name;
                    if (!string.IsNullOrEmpty(name) && name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return elements[i];
                    }
                }
                catch (Exception ex)
                {
                    // element may not be available; continue
                    Logger.Debug("Element became unavailable during text search: " + ex.Message);

                }
            }

            return null;
        }
        [KeywordDescription("Waits until the specified text appears on the screen and continues waiting until it disappears within the given time")]
        [KeywordDisplayName("Wait For Text Disappear")]
        [KeywordParameters("text", "Text to check on screen")]
        [KeywordParameters("timeout", "Waiting time in seconds")]
        [SampleScript("Windows.Wait For Text Disappear(Loading ,10)")]
        public KeywordResult WaitForTextDisappear(string text, string timeout)
        {
            try
            {
                TimeSpan waitTime = TimeSpan.FromSeconds(int.Parse(timeout));
                bool textFoundAtLeastOnce = false;
                DateTime startTime = DateTime.Now;

                while ((DateTime.Now - startTime) < waitTime)
                {
                    if (IsTextPresentOnScreen(text))
                    {
                        textFoundAtLeastOnce = true;
                        break;
                    }
                    Thread.Sleep(500);
                }

                if (!textFoundAtLeastOnce)
                {
                    return new KeywordResult(KeywordResults.Fail)
                    {
                        Output = $"Text '{text}' did not appear within {timeout} seconds."
                    };
                }

                while ((DateTime.Now - startTime) < waitTime)
                {
                    if (!IsTextPresentOnScreen(text))
                    {
                        return new KeywordResult(KeywordResults.Pass)
                        {
                            Output = $"Text '{text}' appeared and then disappeared successfully within {DateTime.Now - startTime:ss\\.fff} seconds."
                        };
                    }
                    Thread.Sleep(500);
                }
                return new KeywordResult(KeywordResults.Fail)
                {
                    Output = $"Text '{text}' appeared but did not disappear within {waitTime} seconds."
                };
            }
            catch (Exception ex)
            {
                return new KeywordResult(KeywordResults.Fail)
                {
                    Output = $"Error while waiting for text to disappear: {ex.Message}"
                };
            }
        }

        private bool IsTextPresentOnScreen(string text)
        {
            try
            {
                var desktop = AutomationElement.RootElement;
                var condition = new PropertyCondition(AutomationElement.NameProperty, text);
                var element = desktop.FindFirst(TreeScope.Descendants, condition);

                if (element != null)
                    return true;

                var textCondition = new PropertyCondition(AutomationElement.IsTextPatternAvailableProperty, true);
                var textElements = desktop.FindAll(TreeScope.Descendants, textCondition);

                foreach (AutomationElement elem in textElements)
                {
                    try
                    {
                        var textPattern = elem.GetCurrentPattern(TextPattern.Pattern) as TextPattern;
                        if (textPattern != null)
                        {
                            var textContent = textPattern.DocumentRange.GetText(-1);
                            if (textContent.Contains(text))
                                return true;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        
        [KeywordDisplayName("Open Folder By Path")]
        [KeywordDescription("Opens a folder using the specified file system path and brings its File Explorer window to the foreground")]
        [KeywordParameters("folderPath", "Full file system path of the folder (e.g., C:\\Users\\User\\Downloads)")]
        [SampleScript("Windows.Open Folder By Path (C:\\Users\\User\\Downloads)")]
        public KeywordResult OpenFolderByPath(string folderPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(folderPath))
                    return new KeywordResult(KeywordResults.Fail, "Folder path cannot be empty");

                if (!Directory.Exists(folderPath))
                    return new KeywordResult(KeywordResults.Fail, $"Folder not found: {folderPath}");

                Process.Start(new ProcessStartInfo
                {
                    FileName = folderPath,
                    UseShellExecute = true
                });

                string folderName = Path.GetFileName(folderPath);

                AutomationElement desktop = AutomationElement.RootElement;

                var windowCondition = new PropertyCondition(
                    AutomationElement.ControlTypeProperty,
                    ControlType.Window
                );

                // Retry instead of Thread.Sleep
                var waitTime = TimeSpan.FromSeconds(3);
                var sw = Stopwatch.StartNew(); // Start timer

                while (sw.Elapsed < waitTime)
                {
                    var windows = desktop.FindAll(TreeScope.Children, windowCondition);

                    foreach (AutomationElement win in windows)
                    {
                        string windowName = win.Current.Name;

                        if (!string.IsNullOrEmpty(windowName) &&
                                           windowName.Equals(folderName, StringComparison.OrdinalIgnoreCase))
                        {
                            // Bring window to front
                            if (win.TryGetCurrentPattern(WindowPattern.Pattern, out object pattern))
                            {
                                var wp = (WindowPattern)pattern; // Get the WindowPattern

                                if (wp.Current.WindowVisualState == WindowVisualState.Minimized)
                                    wp.SetWindowVisualState(WindowVisualState.Normal);
                            }

                            win.SetFocus();

                            return new KeywordResult(KeywordResults.Pass,
                                $"Folder opened and focused: {folderPath}");
                        }
                    }

                    Thread.Sleep(200);
                }

                return new KeywordResult(KeywordResults.Pass,
                    $"Folder opened but focus not confirmed: {folderPath}");
                     }
            catch (Exception ex)
            {
                return new KeywordResult(KeywordResults.Fail, $"Exception: {ex.Message}");
            }
            }
        [KeywordDisplayName("Click Element By Partial Text")]
        [KeywordDescription("Clicks the element that matches the given partial text")]
        [KeywordParameters("partialText", "Partial text to search and click")]
        [SampleScript("Windows.Click Element By Partial Text (Settings)")]
        public KeywordResult ClickElementByPartialText(string partialText)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(partialText))
                    return new KeywordResult(KeywordResults.Fail, "Partial text cannot be empty");

                var root = AutomationElement.RootElement;
                if (root == null)
                    return new KeywordResult(KeywordResults.Fail, "Unable to get root element");

                // Filter at source
                var condition = new OrCondition(
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Text),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Document),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ListItem),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Group),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Pane),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Custom)
                );

                var elements = root.FindAll(TreeScope.Descendants, condition);

                foreach (AutomationElement el in elements)
                {
                    try
                    {
                        var current = el.Current;

                        if (current.IsOffscreen || current.BoundingRectangle.IsEmpty)
                            continue;

                        string name = current.Name ?? string.Empty;
                        string automationId = current.AutomationId ?? string.Empty;

                        if (name.IndexOf(partialText, StringComparison.OrdinalIgnoreCase) < 0 &&
                         automationId.IndexOf(partialText, StringComparison.OrdinalIgnoreCase) < 0)
                            continue;
                        // Try Invoke
                        if (el.TryGetCurrentPattern(InvokePattern.Pattern, out var invoke))
                        {
                            ((InvokePattern)invoke).Invoke();
                            return new KeywordResult(KeywordResults.Pass, $"Clicked: '{name}'");
                        }

                        // Try Select
                        if (el.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var select))
                        {
                            ((SelectionItemPattern)select).Select();
                            return new KeywordResult(KeywordResults.Pass, $"Selected: '{name}'");
                        }

                        // Fallback Mouse Click
                        var rect = current.BoundingRectangle;
                        int x = (int)(rect.Left + rect.Width / 2);
                        int y = (int)(rect.Top + rect.Height / 2);

                        Cursor.Position = new System.Drawing.Point(x, y);
                        MouseLeftClick(new MousePoint(x, y));

                        return new KeywordResult(KeywordResults.Pass, $"Clicked at ({x},{y})");
                    }
                    catch (ElementNotAvailableException)
                    {
                        Logger.Debug("Element became unavailable during click attempt, skipping.");
                    }
                }

                return new KeywordResult(KeywordResults.Fail, $"Partial text '{partialText}' not found");
                }
        
         catch (Exception ex)
 {
     return new KeywordResult(KeywordResults.Fail, $"Exception: {ex.Message}");
 } 
 }
    }
}

#endregion
#endregion














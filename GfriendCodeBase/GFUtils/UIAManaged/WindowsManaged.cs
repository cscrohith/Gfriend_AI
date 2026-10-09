using HP.GFriend.GFLogger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Forms;
using System.Drawing;
using System.Windows;
using System.Threading.Tasks;
using System.Printing;
using HP.GFriend.Keywords;

namespace HP.GFriend.Utils.UIAManaged
{
    public class WindowsManaged
    {
        public AutomationElement _target;
        private ImageConverter _converter = null;
        private static string _outputDir;

        public Rect ScreenBound;

        public WindowsManaged(string outputDir)
        {
            
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
            CommonExecutionInfo.SetSharedObject("windowsManaged", this);
            
        }

        public void Dispose()
        {

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

       


        #region General Methods
        public AutomationElement SelectElement(AutomationElement rootElement, Condition condition, TimeSpan timeOut, TreeScope scope = TreeScope.Subtree)
        {
            AutomationElement target = null;
            DateTime endTime = DateTime.Now.AddMilliseconds(timeOut.TotalMilliseconds);

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

        public AutomationElement SelectElement(AutomationElement rootElement, AutomationProperty prop, object value, TimeSpan timeOut)
        {
            return SelectElement(rootElement, new PropertyCondition(prop, value), timeOut);
        }

        public AutomationElement FindApplication(string windowTitle, TimeSpan timeOut, AutomationElement rootElement = null)
        {
            DateTime endTime = DateTime.Now.AddMilliseconds(timeOut.TotalMilliseconds);
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
            Logger.Debug("Null return");
            return null;

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



                    invokePattern.Invoke();

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

        public bool SelectItem(AutomationElement parent, Condition itemCondition, bool click = false, bool selectWithParent = false)
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

        #endregion

        #region Keywords

        [KeywordDescription("Select applicationto control which window title contains given windowTitle")]
        [KeywordDisplayName("Select Application")]
        [KeywordParameters("windowTitle", "Title of the applciation want to control")]
        public KeywordResult SelectApplication(string windowTitle)
        {
            return SelectApplication(windowTitle, "5");
        }

        [KeywordDescription("Select applicationto control which window title contains given windowTitle")]
        [KeywordDisplayName("Select Application")]
        [KeywordParameters("windowTitle", "Title of the applciation want to control")]
        [KeywordParameters("timeOut", "Finding timeout")]
        public KeywordResult SelectApplication(string windowTitle, string timeOut)
        {
            TimeSpan timeoutTimeSpan = TimeSpan.FromSeconds(int.Parse(timeOut.Trim()));
            _target = FindApplication(windowTitle, timeoutTimeSpan);
            if (_target == null)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "Can not find target application";
                fail.ScreenShot = GetScreenShot();
                return fail;
            }

            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Select child window of current target which window title of given windowTitle")]
        [KeywordDisplayName("Select Child Window With Title")]
        [KeywordParameters("windowTitle", "Title of the applciation want to control (should be exact same)")]
        public KeywordResult SelectChildWindowWithTitle(string windowTitle)
        {
            return SelectChildWindowWithTitle(windowTitle, "5");
        }

        [KeywordDescription("Select child window of current target which window title of given windowTitle")]
        [KeywordDisplayName("Select Child Window With Title")]
        [KeywordParameters("windowTitle", "Title of the applciation want to control (should be exact same)")]
        [KeywordParameters("timeOut", "Finding timeout")]
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
                fail.ScreenShot = GetScreenShot();
                return fail;
            }

            return new KeywordResult(KeywordResults.Pass);
        }

        [GetKeyword]
        [KeywordDescription("Gets the Enviornment Variable and sets it to the given variable")]
        [KeywordDisplayName("Get Enviornment Variable")]
        [KeywordParameters("enviornmentVariable", "Name of the enviornment variable to get")]
        [KeywordParameters("saveTo", "variable name (ex. ${Buffer}) to store value")]
        public KeywordResult GetEnviornmentVariable(string enviornmentVariable, string saveTo)
        {
            string envValue = System.Environment.GetEnvironmentVariable(enviornmentVariable);
            CommonExecutionInfo.SetVariable(saveTo, envValue);
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            result.Output = $"{saveTo} : {envValue}";
            return result;
        }


        [KeywordDescription("Open application and take control with given executable path")]
        [KeywordDisplayName("Open With Path")]
        [KeywordParameters("ExecutablePath", "Executable(*.exe) path to execute and take control")]
        public KeywordResult OpenWithPath(string executablePath)
        {
            return OpenWithPath(executablePath, "2");
        }

        [KeywordDescription("Open application and take control with given executable path")]
        [KeywordDisplayName("Open With Path")]
        [KeywordParameters("ExecutablePath", "Executable(*.exe) path to execute and take control")]
        [KeywordParameters("WaitTime", "Time(seconds) to wait until application is opened.")]
        public KeywordResult OpenWithPath(string executablePath, string waitTime)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            int waitSec = int.Parse(waitTime.Trim());
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.FileName = executablePath; ;
                Process process = new Process();
                process.StartInfo = processStartInfo;
                process.Start();

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
                    result.ScreenShot = GetScreenShot();
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
                result.ScreenShot = GetScreenShot();
                return result;
            }
        }



        [KeywordDescription("Select file at file open(save) dialog window which is currently opend.")]
        [KeywordDisplayName("Select File at Dialog")]
        [KeywordParameters("filePath", "Local file path to open(save)")]
        public KeywordResult SelectFileAtDialog(string filePath)
        {
            _target = SelectElement(AutomationElement.RootElement, AutomationElement.ClassNameProperty, "#32770", TimeSpan.FromSeconds(3));

            if (_target == null)
            {
                KeywordResult result = new KeywordResult(KeywordResults.Fail);
                result.Output = $"Can not find file open dialog";
                Logger.Warn($"Can not find file open dialog");
                _target = AutomationElement.RootElement;
                result.ScreenShot = GetScreenShot();
                return result;
            }

            // Detect if File Open Dialog or File Save Dialog
            string shortCut = string.Empty;
            AutomationElement fileNameField = SelectElement(_target, new PropertyCondition(AutomationElement.AutomationIdProperty, "1148"), TimeSpan.FromSeconds(1));
            if (fileNameField == null)
            {
                // File Save Dialog
                fileNameField = SelectElement(_target, new PropertyCondition(AutomationElement.AutomationIdProperty, "1001"), TimeSpan.FromSeconds(1));
                shortCut = "%{s}";
            }
            else
            {
                // File Open Dialog : In this case check if file is exist
                if (!File.Exists(filePath))
                {
                    KeywordResult result = new KeywordResult(KeywordResults.Fail);
                    result.Output = $"Can not find file {filePath}";
                    result.ScreenShot = GetScreenShot();
                    Logger.Warn($"Can not find file {filePath}");
                    return result;
                }
                shortCut = "%{o}";
            }

            if (!SetText(fileNameField, filePath))
            {
                KeywordResult result = new KeywordResult(KeywordResults.Error);
                result.Output = $"Error during set filename at filename field";
                Logger.Warn($"Error during set filename at filename field");
                _target = AutomationElement.RootElement;
                result.ScreenShot = GetScreenShot();
                return result;
            }

            KeywordResult sendKeyResult = SendKey(shortCut);
            if (!sendKeyResult.Result.Equals(KeywordResults.Pass))
            {
                _target.SetFocus();
                return SendKey(shortCut);
            }

            return sendKeyResult;
        }

        [KeywordDescription("Check if object with given automation Id is exist.")]
        [KeywordDisplayName("Is Exist")]
        [KeywordParameters("AutomationId", "Automation ID to check")]
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
                    result.ScreenShot = GetScreenShot();
                    return result;
                }
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                result.Result = KeywordResults.Fail;
                result.Output = $"Can not find element with given automation ID : {automationId}";
                result.ScreenShot = GetScreenShot();
                return result;
            }
        }



        [KeywordDescription("Click with ID")]
        [KeywordDisplayName("Click ID")]
        [KeywordParameters("AutomationId", "Automation ID to click")]
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
                    result.ScreenShot = GetScreenShot();
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
                    result.ScreenShot = GetScreenShot();
                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error during click", ex);
                result.Result = KeywordResults.Error;
                result.Output = "Error during click";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot();
                return result;
            }
        }


        [KeywordDescription("Click Automation ID with Keyboard event. Use this keyword if Click ID is not working properly.")]
        [KeywordDisplayName("Click ID With Keyboard")]
        [KeywordParameters("AutomationId", "Automation ID to click")]
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
                    result.ScreenShot = GetScreenShot();
                    return result;
                }
                Logger.Trace($"Selected element name : {toClick.Current.Name}");
                toClick.SetFocus();
                return SendKey("{ENTER}");
            }
            catch (Exception ex)
            {
                Logger.Error("Error during click", ex);
                result.Result = KeywordResults.Error;
                result.Output = "Error during click";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot();
                return result;
            }
        }

        

        [KeywordDescription("Click with Name")]
        [KeywordDisplayName("Click Name")]
        [KeywordParameters("Name", "Name property to click")]
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
                    result.ScreenShot = GetScreenShot();
                    return result;
                }

                if (ClickElement(toClick))
                {
                    return result;
                }
                else
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given text : {name}";
                    result.ScreenShot = GetScreenShot();
                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error during click", ex);
                result.Result = KeywordResults.Error;
                result.Output = "Error during click";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot();
                return result;
            }
        }

        [KeywordDescription("Click Name with Keyboard event. Use this keyword if Click Name is not working properly.")]
        [KeywordDisplayName("Click Name With Keyboard")]
        [KeywordParameters("Name", "Name to click")]
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
                    result.ScreenShot = GetScreenShot();
                    return result;
                }
                toClick.SetFocus();
                return SendKey("{ENTER}");
            }
            catch (Exception ex)
            {
                Logger.Error("Error during click", ex);
                result.Result = KeywordResults.Error;
                result.Output = "Error during click";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = GetScreenShot();
                return result;
            }
        }

        

        [KeywordDescription("Send key to the application")]
        [KeywordDisplayName("Send Key")]
        [KeywordParameters("Key", "Key to send")]
        public KeywordResult SendKey(string key)
        {
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
                result.ScreenShot = GetScreenShot();
                Logger.Error("Error during send key", ex);
                return result;
            }

            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Send Ctrl+P to the application")]
        [KeywordDisplayName("Send CtrlP")]
        public KeywordResult SendCtrlP()
        {
            return SendKey("^(p)");
        }

        [KeywordDescription("Set text of element with given Automation ID")]
        [KeywordDisplayName("Set Text")]
        [KeywordParameters("AutomationId", "Automation ID to set text")]
        [KeywordParameters("TextToSet", "Text to set to the field")]
        public KeywordResult SetText(string automationId, string textToSet)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            try
            {
                AutomationElement toSet = SelectElement(_target, AutomationElement.AutomationIdProperty, automationId, TimeSpan.FromSeconds(3));

                if (toSet == null)
                {
                    result.Result = KeywordResults.Fail;
                    result.Output = $"Can not find element with given automation ID : {automationId}";
                    result.ScreenShot = GetScreenShot();
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
                        SendKeys.SendWait(textToSet);
                    }
                    else
                    {
                        Logger.Trace("Set Text using value pattern");
                        ((ValuePattern)valuePattern).SetValue(textToSet);
                    }

                }
                catch (ElementNotEnabledException ex)
                {
                    Logger.Error("SetText : Element is not enalbed", ex);
                    result.Result = KeywordResults.Error;
                    result.Output = "SetText : Element is not enalbed";
                    result.AdditionalInfo = ex.ToString();
                    result.ScreenShot = GetScreenShot();
                    return result;

                }
                catch (InvalidOperationException ex)
                {
                    Logger.Error("SetText : Invalid Operation", ex);
                    result.Result = KeywordResults.Error;
                    result.Output = "SetText : Invalid Operation";
                    result.AdditionalInfo = ex.ToString();
                    result.ScreenShot = GetScreenShot();
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
                result.ScreenShot = GetScreenShot();
                return result;
            }
        }

        [KeywordDescription("Capture Screen Shot of current application")]
        [KeywordDisplayName("Capture Screen Shot")]
        [KeywordParameters("filename", "filename to save")]
        public KeywordResult CaptureScreenShot(string filename)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            result.ScreenShot = GetScreenShot();
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
        public KeywordResult CaptureFullScreen(string filename)
        {
            KeywordResult result = new KeywordResult(KeywordResults.Pass);
            result.ScreenShot = GetScreenShot(ScreenBound);
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


        [KeywordDescription("Set combo box value with given automation IDs")]
        [KeywordDisplayName("Set ComboBox")]
        [KeywordParameters("comboboxAutomationID", "Automation ID for combo box")]
        [KeywordParameters("valueAutomationID", "Automation ID for value")]
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
            if (SelectItem(comboBox, condition))
            {
                return new KeywordResult(KeywordResults.Pass);
            }
            KeywordResult result = new KeywordResult(KeywordResults.Fail);
            result.Output = $"Combobx click fail : {comboboxAutomationID} - {valueAutomationID}";
            result.ScreenShot = GetScreenShot();
            return result;
        }

        [KeywordDescription("Set combo box value with given automation IDs")]
        [KeywordDisplayName("Set ComboBox With Name")]
        [KeywordParameters("comboboxAutomationID", "Automation ID for combo box")]
        [KeywordParameters("valueName", "Name for value")]
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
            if (SelectItem(comboBox, condition))
            {
                return new KeywordResult(KeywordResults.Pass);
            }
            KeywordResult result = new KeywordResult(KeywordResults.Fail);
            result.Output = $"Combobx click fail : {comboboxAutomationID} - {valueName}";
            result.ScreenShot = GetScreenShot();
            return result;
        }

        [KeywordDescription("Set list box item with given automation IDs")]
        [KeywordDisplayName("Set List Item")]
        [KeywordParameters("listboxAutomationID", "Automation ID for combo box")]
        [KeywordParameters("itemAutomationID", "Automation ID for item to select")]
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
            result.ScreenShot = GetScreenShot();
            return result;
        }

        [KeywordDescription("Set list box item with given automation IDs")]
        [KeywordDisplayName("Set List Item")]
        [KeywordParameters("listboxAutomationID", "Automation ID for combo box")]
        [KeywordParameters("itemName", "Name for item to select")]
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
            result.ScreenShot = GetScreenShot();
            return result;
        }

        [KeywordDescription("Set toggle state")]
        [KeywordDisplayName("Set Toggle")]
        [KeywordParameters("toggleAutomationID", "Automation ID of toggle")]
        [KeywordParameters("toggleValue", "On or Off")]
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
                fail.ScreenShot = GetScreenShot();
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


        #endregion


    }
}

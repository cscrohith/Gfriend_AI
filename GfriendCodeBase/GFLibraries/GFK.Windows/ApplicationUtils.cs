using HP.GFriend.GFLogger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Forms;
using HP.GFriend.Utils.UIAManaged;
namespace HP.GFriend.Keywords
{
    public enum Application
    {
        MSWord,
        MSPowerPoint,
        MSExcel,
        AdobeAcrobat,
        Unknown
    }

    public enum OfficePrintSetting
    {
        PagesToPrint,
        Duplex,
        Collated,
        Orientation,
        PaperSize,
        Margins,
        PagePerSheet
    }
    public static class ApplicationUtils
    {
        private static Application _currentApplication;
        private static AutomationElement _currentRootElement;
        private static readonly List<string> WORDFILES = new List<string>() { ".doc", ".docx", ".docm", ".dot", ".dotx", ".dotm" };
        private static readonly List<string> POWERPOINTFILES = new List<string>() {".ppt", ".pptx", ".pptm", ".pps", ".ppsx", ".ppsm", ".pot", ".potx", ".potx"};
        private static readonly List<string> EXCELFILES = new List<string>() {".xls", ".xlsm", ".xlsx", ".xlsb", ".xltx", ".xltm" };

        public static AutomationElement ApplicationAutomationElement { get; private set; }


        #region GenericMethods

        public static Application GetTargetApplication(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            if(WORDFILES.Contains(extension))
            {
                return Application.MSWord;
            }

            else if(POWERPOINTFILES.Contains(extension))
            {
                return Application.MSPowerPoint;
            }

            else if (EXCELFILES.Contains(extension))
            {
                return Application.MSExcel;
            }

            else if(extension.Equals(".pdf"))
            {
                return Application.AdobeAcrobat;
            }

            Logger.Error($"Can not find application for this kind of file : {extension}");
            return Application.Unknown;
        }

        public static KeywordResult OpenPrinterProperties(string filePath, string printerName, Windows windowsController)
        {
            _currentApplication = GetTargetApplication(filePath);
            switch(_currentApplication)
            {
                case Application.MSWord:
                case Application.MSPowerPoint:
                case Application.MSExcel:
                    return OpenMSOffice(filePath, printerName, windowsController);
                case Application.AdobeAcrobat:
                    return OpenAcrobatReader(filePath, printerName, windowsController);
            }
            KeywordResult error = new KeywordResult(KeywordResults.Error);
            error.Output = "Can not find application for this kind of file.";
            return error;
        }

        public static bool ClickPrintButton(Windows windowsController)
        {
            switch (_currentApplication)
            {
                case Application.MSWord:
                case Application.MSPowerPoint:
                case Application.MSExcel:
                    return ClickPrintButtonInMSOffice(windowsController);
                case Application.AdobeAcrobat:
                    return ClickPrintButtonInAcrobatReader(windowsController);
            }
            return false;
        }


        public static bool CloseApplication()
        {
            object pattern = null;
            if (ApplicationAutomationElement.TryGetCurrentPattern(WindowPattern.Pattern, out pattern))
            {
                ((WindowPattern)pattern).Close();
                ApplicationAutomationElement = null;
                return true;
            }
            return false;
        }

        #endregion

        #region MSOffice

        public static KeywordResult OpenMSOffice(string filePath, string printerName, Windows windowsController)
        {
            KeywordResult result;
            // Open Application with file
            result = windowsController.OpenWithPath(filePath, "10");
            if (!result.Result.Equals(KeywordResults.Pass))
            {
                return result;
            }

            // Open Print
            result = windowsController.SendCtrlP();
            if (!result.Result.Equals(KeywordResults.Pass))
            {
                return result;
            }

            _currentRootElement = windowsController._target;
            ApplicationAutomationElement = _currentRootElement;

            // Select printer combobox
            PropertyCondition condition = new PropertyCondition(AutomationElement.AccessKeyProperty, "Alt, F, P, I");
            AutomationElement element = windowsController.SelectElement(windowsController._target, condition, TimeSpan.FromSeconds(10));
            AutomationElement printerComboBoxElement = element;
            Thread.Sleep(5000);
            if (element == null || !windowsController.ClickElement(element))
            {
                result = new KeywordResult(KeywordResults.Fail);
                result.Output = "Fail to click printer selection";
                return result;
            }

            // Select printer
            condition = new PropertyCondition(AutomationElement.NameProperty, printerName);
            element = windowsController.SelectElement(element, condition, TimeSpan.FromSeconds(1));
            if (element == null || !windowsController.ClickElement(element))
            {
                result = new KeywordResult(KeywordResults.Fail);
                result.Output = "Fail to click target printer";
                return result;
            }

            // Open properties window
            Logger.Debug("Finding printer properties");
            Thread.Sleep(2000);

            TreeWalker tWalker = TreeWalker.ControlViewWalker;
            element = tWalker.GetParent(tWalker.GetParent(printerComboBoxElement));


            //condition = new PropertyCondition(AutomationElement.AccessKeyProperty, "Alt, F, P, R");
            condition = new PropertyCondition(AutomationElement.ClassNameProperty, "NetUIHyperlink");
            element = windowsController.SelectElement(element, condition, TimeSpan.FromSeconds(20));
            Logger.Debug($"Properties element is null? : {element == null}");
            if (element == null)
            {
                result = new KeywordResult(KeywordResults.Fail);
                result.ScreenShot = windowsController.GetScreenShot();
                result.Output = "Fail to find printer properties";
                return result;
            }

            if (!windowsController.ClickElement(element))
            {
                // retry
                if (!windowsController.ClickElement(element))
                {
                    result = new KeywordResult(KeywordResults.Fail);
                    result.ScreenShot = windowsController.GetScreenShot();
                    result.Output = "Fail to click printer properties";
                    return result;
                }
            }

            return new KeywordResult(KeywordResults.Pass);
        }


        public static bool ClickPrintButtonInMSOffice(Windows windowsController)
        {
            AutomationElement printButton = windowsController.SelectElement(_currentRootElement, AutomationElement.AccessKeyProperty, "Alt, F, P, P", TimeSpan.FromSeconds(5));
            if (printButton == null)
            {
                return false;
            }
            return windowsController.ClickElement(printButton);
        }


        public static KeywordResult SetCopies(int copies, Windows windowsController)
        {
            AutomationElement ae_copies = windowsController.SelectElement(_currentRootElement, new PropertyCondition(AutomationElement.AccessKeyProperty, "Alt, F, P, N"), TimeSpan.FromSeconds(5));
            if (ae_copies == null)
            {
                KeywordResult result = new KeywordResult(KeywordResults.Fail);
                result.Output = "Can not find copies option";
                result.ScreenShot = windowsController.GetScreenShot();
                return result;
            }
            if(!windowsController.SetText(ae_copies, copies.ToString()))
            {
                KeywordResult result = new KeywordResult(KeywordResults.Fail);
                result.Output = "Can not set copies";
                result.ScreenShot = windowsController.GetScreenShot();
                return result;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        public static KeywordResult ChangePrintSettingsInMSOffice(OfficePrintSetting setting, string menu, Windows windowsController)
        {
            string accessKey = string.Empty;

            switch(setting)
            {
                case OfficePrintSetting.PagesToPrint:
                    accessKey = "Alt, F, P, A";
                    break;
                case OfficePrintSetting.Duplex:
                    accessKey = "Alt, F, P, D";
                    break;
                case OfficePrintSetting.Collated:
                    accessKey = "Alt, F, P, C";
                    break;
                case OfficePrintSetting.Orientation:
                    accessKey = "Alt, F, P, O";
                    break;
                case OfficePrintSetting.PaperSize:
                    accessKey = "Alt, F, P, L";
                    break;
                case OfficePrintSetting.Margins:
                    accessKey = "Alt, F, P, M";
                    break;
                case OfficePrintSetting.PagePerSheet:
                    accessKey = "Alt, F, P, H";
                    break;
            }

            AutomationElement option = windowsController.SelectElement(_currentRootElement, new PropertyCondition(AutomationElement.AccessKeyProperty, accessKey), TimeSpan.FromSeconds(5));
            if(option == null)
            {
                KeywordResult result = new KeywordResult(KeywordResults.Fail);
                result.Output = "Can not find combobx";
                result.ScreenShot = windowsController.GetScreenShot();
                return result;
            }

            PropertyCondition valueCondition = new PropertyCondition(AutomationElement.NameProperty, menu);
            if(!windowsController.SelectItem(option, valueCondition,true))
            {
                KeywordResult result = new KeywordResult(KeywordResults.Fail);
                result.Output = "Can not set combobox with given value";
                result.ScreenShot = windowsController.GetScreenShot();
                return result;
            }

            return new KeywordResult(KeywordResults.Pass);
        }

        #endregion

        #region AcrobatReader
        public static KeywordResult OpenAcrobatReader(string filePath, string printerName, Windows windowsController)
        {
            Logger.Trace("Open Acrobat Reader");
            KeywordResult result;
            // Open Application with file
            result = windowsController.OpenWithPath(filePath, "10");
            if (!result.Result.Equals(KeywordResults.Pass))
            {
                return result;
            }

            ApplicationAutomationElement = windowsController._target;
            return ApplicationUtilsManaged.OpenAcrobatReader(filePath, printerName, new WindowsManaged(windowsController.GetOutputDir()));

            /*
            // Open Print
            Thread.Sleep(1000);
            result = windowsController.SendCtrlP();
            Thread.Sleep(1000);
            if (!result.Result.Equals(KeywordResults.Pass))
            {
                return result;
            }

            Logger.Debug("Get Print Dialog");
            PropertyCondition printDialogCondition = new PropertyCondition(AutomationElement.ClassNameProperty, "#32770");
            Thread.Sleep(5000);
            AutomationElement printDialog = windowsController.SelectElement(windowsController._target, printDialogCondition, TimeSpan.FromSeconds(10), TreeScope.Children);
            _currentRootElement = printDialog;

            PropertyCondition labelCondition = new PropertyCondition(AutomationElement.AccessKeyProperty, "Alt+n");
            AutomationElement printerLabel = printDialog.FindFirst(TreeScope.Subtree, labelCondition);

            TreeWalker tWalker = TreeWalker.ControlViewWalker;
            AutomationElement printSection = tWalker.GetParent(printerLabel);

            Logger.Debug("Select printer combobox");
            PropertyCondition condition = new PropertyCondition(AutomationElement.ControlTypeProperty, System.Windows.Automation.ControlType.ComboBox);
            AutomationElement element = windowsController.SelectElement(printSection, condition, TimeSpan.FromSeconds(5));
            AutomationElement printerCombobox = element;

            


            if (element == null || !windowsController.ClickElement(element))
            {
                result = new KeywordResult(KeywordResults.Fail);
                result.Output = "Fail to click printer selection";
                return result;
            }
            

            condition = new PropertyCondition(AutomationElement.NameProperty, printerName);
            AutomationElement printQueue = windowsController.SelectElement(printerCombobox, condition, TimeSpan.FromSeconds(1));

            if(printQueue == null)
            {
                result = new KeywordResult(KeywordResults.Fail);
                result.Output = $"Can not find printer with name : {printerName}";
                return result;
            }

            windowsController.ClickElement(printerCombobox);
            windowsController.ClickElement(printQueue);

            string selected = string.Empty;

            DateTime endTime = DateTime.Now.AddSeconds(10);
            while(!selected.Equals(printerName))
            {
                if(DateTime.Now > endTime)
                {
                    result = new KeywordResult(KeywordResults.Fail);
                    result.Output = $"Can not set printer : {printerName}";
                    return result;
                }

                ValuePattern valuePattern = printerCombobox.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern;
                selected = valuePattern.Current.Value;
                Thread.Sleep(1000);
            }

            SendKeys.SendWait("{TAB}");
            Thread.Sleep(10000);

            // Open properties window
            Logger.Debug("Finding printer properties");
            
            condition = new PropertyCondition(AutomationElement.AccessKeyProperty, "Alt+p");
            element = windowsController.SelectElement(printSection, condition, TimeSpan.FromSeconds(10));
            Logger.Debug($"Properties element is null? : {element == null}");
            if(element == null)
            {
                result = new KeywordResult(KeywordResults.Fail);
                result.Output = "Fail to find printer properties";
                return result;
            }
            element.SetFocus();
            printerCombobox.SetFocus();
            
            if (!windowsController.ClickElement(element))
            {
                result = new KeywordResult(KeywordResults.Fail);
                result.Output = "Fail to click printer properties";
                return result;
            }

            return new KeywordResult(KeywordResults.Pass);

            */
        }

        public static bool ClickPrintButtonInAcrobatReader(Windows windowsController)
        {
            PropertyCondition printDialogCondition = new PropertyCondition(AutomationElement.ClassNameProperty, "#32770");
            Thread.Sleep(2000);
            AutomationElement printDialog = windowsController.SelectElement(windowsController._target, printDialogCondition, TimeSpan.FromSeconds(5), TreeScope.Children);
            _currentRootElement = printDialog;
            AutomationElement group = _currentRootElement.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, System.Windows.Automation.ControlType.Pane));
            AutomationElementCollection buttons = group.FindAll(TreeScope.Subtree, new PropertyCondition(AutomationElement.ControlTypeProperty, System.Windows.Automation.ControlType.Button));
            int index = buttons.Count - 2;
            AutomationElement printButton = buttons[index];
            return windowsController.ClickElement(printButton);
        }

        #endregion



    }
}

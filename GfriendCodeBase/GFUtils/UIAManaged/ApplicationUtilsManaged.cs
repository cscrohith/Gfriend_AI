using HP.GFriend.GFLogger;
using HP.GFriend.Keywords;
using System;
using System.Threading;
using System.Windows.Automation;

namespace HP.GFriend.Utils.UIAManaged
{
    public static class ApplicationUtilsManaged
    {
        public static AutomationElement ApplicationAutomationElement;
        public static AutomationElement _currentRootElement;
        public static KeywordResult OpenAcrobatReader(string filePath, string printerName, WindowsManaged windowsController)
        {
            Logger.Trace("Get Acrobat Reader");
            KeywordResult result;

            ApplicationAutomationElement = AutomationElement.FocusedElement;
            int pid = AutomationElement.FocusedElement.Current.ProcessId;

            ApplicationAutomationElement = windowsController.SelectElement(AutomationElement.RootElement, AutomationElement.ProcessIdProperty, pid, TimeSpan.FromSeconds(5));

            windowsController._target = ApplicationAutomationElement;

            // Fix for SWQATR-497 - retry logic for printer dialogue 
            AutomationElement printDialog = null;
            int retryCount = 3;
            int count = 0;

            while (printDialog == null && count < retryCount)
            {
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
                printDialog = windowsController.SelectElement(windowsController._target, printDialogCondition, TimeSpan.FromSeconds(10), TreeScope.Children);
                count++;
            }

            if (printDialog == null)
            {
                result = new KeywordResult(KeywordResults.Fail);
                result.Output = "Fail to open Printer Dialogue";
                return result;
            }

            _currentRootElement = printDialog;
            PropertyCondition labelCondition = new PropertyCondition(AutomationElement.AccessKeyProperty, "Alt+n");
            AutomationElement printerLabel = printDialog.FindFirst(TreeScope.Subtree, labelCondition);

            TreeWalker tWalker = TreeWalker.ControlViewWalker;
            AutomationElement printSection = tWalker.GetParent(printerLabel);

            if (printSection == null)
            {
                result = new KeywordResult(KeywordResults.Fail);
                result.Output = "Fail to Select printer selection";
                return result;
            }


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

            Logger.Debug("Printer select");
            // Select printer
            condition = new PropertyCondition(AutomationElement.NameProperty, printerName);
            element = windowsController.SelectElement(element, condition, TimeSpan.FromSeconds(1));
            if (element == null || !windowsController.ClickElement(element))
            {
                result = new KeywordResult(KeywordResults.Fail);
                result.Output = "Fail to click target printer";
                return result;
            }
            Thread.Sleep(3000);

            // Open properties window
            Logger.Debug("Finding printer properties");

            condition = new PropertyCondition(AutomationElement.AccessKeyProperty, "Alt+P");
            element = windowsController.SelectElement(printSection, condition, TimeSpan.FromSeconds(10));
            Logger.Debug($"Properties element is null? : {element == null}");
            if (element == null)
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
        }
    }
}

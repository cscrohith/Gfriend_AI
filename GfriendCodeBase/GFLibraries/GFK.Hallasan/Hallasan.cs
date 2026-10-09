using HP.GFriend.GFLogger;
using HP.GFriend.Support;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Automation;


namespace HP.GFriend.Keywords
{
    public class Hallasan : IGFLibrary
    {
        private AutomationElement _hallasan;
        private Windows _windows;
        private DeviceUnderTest _dut;
        private PrinterDriverOptions _options;

        public void Dispose()
        {

        }

        public bool DutUsed()
        {
            return true;
        }

        public List<string> GetDependencies()
        {
            return new List<string> { "Windows" };
        }

        public string GetName()
        {
            return "Hallasan";
        }

        public void Initialize(DeviceUnderTest dut, string outputDir)
        {
            _dut = dut;
            GenerateSupportedOption();
        }

        private byte[] GetScreenCapture()
        {
            if (_hallasan == null)
            {
                return null;
            }
            return _windows.GetScreenShot(_hallasan.Current.BoundingRectangle);
        }

        private void GenerateSupportedOption()
        {
            _options = new PrinterDriverOptions();
        }

        [KeywordDescription("Add printer with given driver")]
        [KeywordDisplayName("Add Printer")]
        [KeywordParameters("printerName", "Name of Printer")]
        [KeywordParameters("driverPath", "Path of driver .inf file")]
        [KeywordParameters("driverName", "Driver name of given driver")]
        [SampleScript("This sample will be Added in Next Release")]
        public KeywordResult AddPrinter(string printerName, string driverPath, string driverName)
        {
            if (_windows == null)
            {
                _windows = (Windows)CommonExecutionInfo.GetSharedObject("windows");
            }
            return _windows.AddPrinter(printerName, driverPath, driverName, _dut.DeviceAddress);
        }

        [KeywordDescription("Remove printer from system")]
        [KeywordDisplayName("Remove Printer")]
        [KeywordParameters("printerName", "Name of printer to delete")]
        [SampleScript("This sample will be Added in Next Release")]
        public KeywordResult RemovePrinter(string printerName)
        {
            if (_windows == null)
            {
                _windows = (Windows)CommonExecutionInfo.GetSharedObject("windows");
            }

            return _windows.RemovePrinter(printerName);
        }

        [KeywordDescription("Open document and then open printer properties windows of given printer name\r\nApplication should be installed before run this keyword\r\nSupported Applicaton: MS Word, MS PowerPoint, MS Excel and Acrobat Reader")]
        [KeywordDisplayName("Open File For Print")]
        [KeywordParameters("filePath", @"File path to print (ex. c:\testfiles\word2page.doc")]
        [KeywordParameters("printerName", "Printer to use")]
        [SampleScript("Hallasan.Open File For Print (C:\\Users\\XAppanna\\Desktop\\Hallasan_Sample.docx,Bell_Jolt)")]
        public KeywordResult OpenFileForPrint(string filePath, string printerName)
        {
            if (_windows == null)
            {
                _windows = (Windows)CommonExecutionInfo.GetSharedObject("windows");
            }
            return _windows.OpenFileForPrint(filePath, printerName);

        }

        [KeywordDescription("Close application.\r\nThis keyword MUST be called after Open File For Print")]
        [KeywordDisplayName("Close Application")]
        [SampleScript("Hallasan.Close Application")]
        public KeywordResult CloseApplication()
        {
            return _windows.CloseApplication();
        }

        [KeywordDescription("Click print button in application. This keyword MUST be used with Open File For Print")]
        [KeywordDisplayName("Click Print Button")]
        [SampleScript("Hallasan.Click Print Button")]   
        public KeywordResult ClickPrintButton()
        {
            return _windows.ClickPrintButton();
        }

        [KeywordDescription("Print given text to the device (ipsend)")]
        [KeywordDisplayName("Print Text")]
        [KeywordParameters("text", @"text to print. Use '\n' for adding new line")]
        [SampleScript("Hallasan.Print Text (SampleText)")]
        public KeywordResult PrintText(string text)
        {
            text = text.Replace(@"\n", Environment.NewLine);
            return MFPUtils.IpSend(_dut, text);
        }

        [KeywordDescription("Find Hallasan driver window in current desktop with given name. \nThis keyword MUST be called before setting options or click print button.")]
        [KeywordDisplayName("Get Driver Window")]
        [KeywordParameters("name", "Title of window")]
        [SampleScript("Hallasan.Get Driver Window (Bell_Jolt)")]
        public KeywordResult GetDriverWindow(string name)
        {
            if (_windows == null)
            {
                _windows = (Windows)CommonExecutionInfo.GetSharedObject("windows");
            }

            PropertyCondition typeCon = new PropertyCondition(AutomationElement.ControlTypeProperty, System.Windows.Automation.ControlType.Window);
            PropertyCondition nameCon = new PropertyCondition(AutomationElement.NameProperty, name);
            AndCondition findCondition = new AndCondition(typeCon, nameCon);

            try
            {

                AutomationElement targetRootElement = ApplicationUtils.ApplicationAutomationElement ?? AutomationElement.RootElement;
                _hallasan = _windows.SelectElement(AutomationElement.RootElement, findCondition, TimeSpan.FromSeconds(10));
                if (_hallasan == null)
                {
                    KeywordResult result = new KeywordResult(KeywordResults.Fail);
                    result.Output = "Can not find window";
                    result.ScreenShot = _windows.GetScreenShot(_windows.ScreenBound);
                    return result;
                }
            }
            catch (Exception ex)
            {
                KeywordResult result = new KeywordResult(KeywordResults.Error);
                result.Output = "Error during getting Hallasan window";
                result.AdditionalInfo = ex.ToString();
                result.ScreenShot = _windows.GetScreenShot(_windows.ScreenBound);
                return result;
            }
            _windows._target = _hallasan;
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Click Confirm button in Hallasan driver")]
        [KeywordDisplayName("Confirm")]
        [SampleScript("Hallasan.Confirm")]
        public KeywordResult Confirm()
        {
            try
            {
                return _windows.ClickId("App.OK");
            }
            catch (Exception ex)
            {
                Logger.Error("Click confirm error", ex);
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error during click confirm button in driver";
                error.ScreenShot = GetScreenCapture();
                error.AdditionalInfo = ex.ToString();
                return error;
            }
        }
        [KeywordDescription("Load Option Information from Excel Menumap file")]
        [KeywordDisplayName("Load Option Setting")]
        [KeywordParameters("menumapFilePath", "File path of menumap")]
        [KeywordParameters("menumapSheetName", "Sheet name of menumap")]
        [SampleScript("This sample will be Added in Next Release")]

        public KeywordResult LoadOptionSetting(string menumapFilePath, string menumapSheetName)
        {
            menumapFilePath = Support.Utils.GetAbsolutePath(menumapFilePath, CommonExecutionInfo.ScriptFolder);
            if (!File.Exists(menumapFilePath))
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail)
                {
                    Output = "Menumap file is not exist."
                };
                return fail;
            }

            try
            {
                _options.FromExcel(menumapFilePath, menumapSheetName);
                KeywordResult result = new KeywordResult(KeywordResults.Pass);
                result.Output = _options.ToString();
                Logger.Trace(result.Output);
                return result;
            }
            catch(NotSupportedException nse)
            {
                Logger.Error("Not supported menumap format", nse);
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = nse.Message;
                return fail;
            }
            catch(Exception ex)
            {
                Logger.Error("Unexpected error", ex);
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Unexpected error";
                error.AdditionalInfo = ex.ToString();
                return error;
            }

        }

        [KeywordDescription("Set option in Hallasan UI")]
        [KeywordDisplayName("Set Option")]
        [KeywordParameters("OptionName", "Option to set")]
        [KeywordParameters("OptionValue", "Option value to set")]
        [SampleScript("This sample will be Added in Next Release")]
        public KeywordResult SetOption(string optionName, string optionValue)
        {
            optionName = optionName.Trim();
            optionValue = optionValue.Trim();
            PrinterDriverOption option = _options.GetOption(optionName);
            if(option == null)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Not Supported option : {optionName}";
                return fail;
            }
            string valueAutomationID = string.Empty;
            switch (option.OptionType)
            {
                case ControlType.COMBOBOX:
                    Logger.Trace("Set Combo box");
                    if (!option.Values.ContainsKey(optionValue))
                    {
                        KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                        fail.Output = $"Not Supported option value : {optionValue}";
                        return fail;
                    }

                    valueAutomationID = option.Values[optionValue];
                    return SetComboBox(option.AutomationId, valueAutomationID);

                case ControlType.TEXTBOX:
                    Logger.Trace("Set Text box");
                    return SetTextBox(option.AutomationId, optionValue);
                case ControlType.TOGGLE:
                    Logger.Trace("Toggle");
                    return SetToggle(option.AutomationId, optionValue);
                case ControlType.RADIO:
                    Logger.Trace("Radio");
                    if (!option.Values.ContainsKey(optionValue))
                    {
                        KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                        fail.Output = $"Not Supported option value : {optionValue}";
                        return fail;
                    }
                    valueAutomationID = option.Values[optionValue];
                    return _windows.ClickId(valueAutomationID);


            }

            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Set combo box value with given automation IDs")]
        [KeywordDisplayName("Set ComboBox")]
        [KeywordParameters("comboboxAutomationID", "Automation ID for combo box")]
        [KeywordParameters("valueAutomationID", "Automation ID for value")]
        [SampleScript("Hallasan.Set ComboBox (ui_PresetComboBox,_Labels)")]
        public KeywordResult SetComboBox(string comboboxAutomationID, string valueAutomationID)
        {
            return _windows.SetComboBox(comboboxAutomationID, valueAutomationID);
        }

        [KeywordDescription("Set text in textbox")]
        [KeywordDisplayName("Set Text Box")]
        [KeywordParameters("textboxAutomationID", "Automation ID of textbox")]
        [KeywordParameters("valueToSet", "Setting value to textbox")]
        [SampleScript("Hallasan.Set Text Box (ui_Keyword,print)")]
        public KeywordResult SetTextBox(string textboxAutomationID, string valueToSet)
        {
            return _windows.SetText(textboxAutomationID, valueToSet);
        }

        [KeywordDescription("Set toggle state")]
        [KeywordDisplayName("Set Toggle")]
        [KeywordParameters("toggleAutomationID", "Automation ID of toggle")]
        [KeywordParameters("toggleValue", "On or Off")]
        [SampleScript("Hallasan.Set Toggle (Part.DocumentCollate,Off)")]
        public KeywordResult SetToggle(string toggleAutomationID, string toggleValue)
        {
            return _windows.SetToggle(toggleAutomationID, toggleValue);
        }

        [KeywordDescription("Set Radio Button")]
        [KeywordDisplayName("Set Radio")]
        [KeywordParameters("radioAutomationID", "Automation ID of Radio item")]
        [SampleScript("This sample will be Added in Next Release")]
        public KeywordResult SetRadio(string radioAutomationID)
        {
            return _windows.ClickId(radioAutomationID);
        }
    }
}

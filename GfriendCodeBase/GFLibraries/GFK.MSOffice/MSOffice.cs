using HP.GFriend.Keywords;
using System.Collections.Generic;

namespace GFK.MSOffice
{
    [LibraryDescription(" - Before using MSOffice keywords, Print setting window of MS Office should be exist with 'Windows.Open File For Print' keyword. ex: Windows.Open File For Print (C:\\Users\\MKavyash\\Documents\\Test1.docx,Diamond_4.183).  <span style=\"color:black;\"> <p><br> Sample Testcase: <p>Tc<p> { <p> Windows.Open File For Print (C:\\Users\\MKavyash\\Documents\\Test1.docx,Diamond_4.183) <p> MSOffice.Set Copies (5) <p>MSOffice.Set Orientation (Landscape Orientation) <p>MSOffice.Set Paper Size (A5) <p>MSOffice.Set Pages Per Sheet (6 Pages Per Sheet)<p> Windows.Click Print Button <p> } </span>")]
    public class MSOffice : IGFLibrary
    {
        public void Dispose()
        {
            
        }

        public bool DutUsed()
        {
            return false;
        }

        public List<string> GetDependencies()
        {
            return new List<string> { "Windows" };
        }

        public string GetName()
        {
            return "MSOffice";
        }

        public void Initialize(DeviceUnderTest dut, string outputDir)
        {
            
        }

        [KeywordDescription("Set copies to print option at MS Office application. (Print setting window of MS Office should be exist with Windows.Open File For Print keyword)")]
        [KeywordDisplayName("Set Copies")]
        [KeywordParameters("copies", "number of copies")]
        [SampleScript("MSOffice.Set Copies (5)")]
        public KeywordResult SetCopies(string copies)
        {
            if(int.TryParse(copies, out int iCopies))
            {
                return ApplicationUtils.SetCopies(iCopies, (Windows)CommonExecutionInfo.GetSharedObject("windows"));
            }
            KeywordResult error = new KeywordResult(KeywordResults.Error);
            error.Output = "Copies must be a number";
            return error;
        }

        [KeywordDescription("Set pages to print option at MS Office application. (Print setting window of MS Office should be exist with Windows.Open File For Print keyword)")]
        [KeywordDisplayName("Set Pages To Print")]
        [KeywordParameters("optionValue", "Name property of option value")]
        [SampleScript("MSOffice.Set Pages To Print (Print Current Page)")]
        public KeywordResult SetPagesToPrint(string optionValue)
        {
            return ApplicationUtils.ChangePrintSettingsInMSOffice(OfficePrintSetting.PagesToPrint, optionValue, (Windows)CommonExecutionInfo.GetSharedObject("windows"));
        }

        [KeywordDescription("Set collated option at MS Office application. (Print setting window of MS Office should be exist with Windows.Open File For Print keyword)")]
        [KeywordDisplayName("Set Collated")]
        [KeywordParameters("optionValue", "Name property of option value")]
        [SampleScript("MSOffice.Set Collated (Uncollated)")]
        public KeywordResult SetCollated(string optionValue)
        {
            return ApplicationUtils.ChangePrintSettingsInMSOffice(OfficePrintSetting.Collated, optionValue, (Windows)CommonExecutionInfo.GetSharedObject("windows"));
        }

        [KeywordDescription("Set duplex option at MS Office application. (Print setting window of MS Office should be exist with Windows.Open File For Print keyword)")]
        [KeywordDisplayName("Set Duplex")]
        [KeywordParameters("optionValue", "Name property of option value")]
        [SampleScript("MSOffice.Set Duplex (Print One Sided)")]
        public KeywordResult SetDuplex(string optionValue)
        {
            return ApplicationUtils.ChangePrintSettingsInMSOffice(OfficePrintSetting.Duplex, optionValue, (Windows)CommonExecutionInfo.GetSharedObject("windows"));
        }

        [KeywordDescription("Set margins option at MS Office application. (Print setting window of MS Office should be exist with Windows.Open File For Print keyword)")]
        [KeywordDisplayName("Set Margins")]
        [KeywordParameters("optionValue", "Name property of option value")]
        [SampleScript("MSOffice.Set Margins (Moderate Margins)")]
        public KeywordResult SetMargins(string optionValue)
        {
            return ApplicationUtils.ChangePrintSettingsInMSOffice(OfficePrintSetting.Margins, optionValue, (Windows)CommonExecutionInfo.GetSharedObject("windows"));
        }

        [KeywordDescription("Set orientation option at MS Office application. (Print setting window of MS Office should be exist with Windows.Open File For Print keyword)")]
        [KeywordDisplayName("Set Orientation")]
        [KeywordParameters("optionValue", "Name property of option value")]
        [SampleScript("MSOffice.Set Orientation (Landscape Orientation)")]
        public KeywordResult SetOrientation(string optionValue)
        {
            return ApplicationUtils.ChangePrintSettingsInMSOffice(OfficePrintSetting.Orientation, optionValue, (Windows)CommonExecutionInfo.GetSharedObject("windows"));
        }

        [KeywordDescription("Set pages per sheet option at MS Office application. (Print setting window of MS Office should be exist with Windows.Open File For Print keyword)")]
        [KeywordDisplayName("Set Pages Per Sheet")]
        [KeywordParameters("optionValue", "Name property of option value")]
        [SampleScript("MSOffice.Set Pages Per Sheet (6 Pages Per Sheet)")]
        public KeywordResult SetPagesPerSheet(string optionValue)
        {
            return ApplicationUtils.ChangePrintSettingsInMSOffice(OfficePrintSetting.PagePerSheet, optionValue, (Windows)CommonExecutionInfo.GetSharedObject("windows"));
        }

        [KeywordDescription("Set paper size option at MS Office application. (Print setting window of MS Office should be exist with Windows.Open File For Print keyword)")]
        [KeywordDisplayName("Set Paper Size")]
        [KeywordParameters("optionValue", "Name property of option value")]
        [SampleScript("MSOffice.Set Paper Size (A5)")]
        public KeywordResult SetPaperSize(string optionValue)
        {
            return ApplicationUtils.ChangePrintSettingsInMSOffice(OfficePrintSetting.PaperSize, optionValue, (Windows)CommonExecutionInfo.GetSharedObject("windows"));
        }
    }
}

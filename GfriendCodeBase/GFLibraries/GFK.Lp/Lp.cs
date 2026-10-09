using HP.GFriend.GFLogger;
using System.Collections.Generic;

namespace HP.GFriend.Keywords
{
    [LibraryDescription(" - The lp command is used to print files on Unix and Linux systems.<p><br>The keyword \"SelectFile\" should be called at first when creating print job and the keyword \"GetLpCommand\" should be used at last , once you use the other keywords of LP library . Please refer to <a href=\"https://openprinting.github.io/cups/doc/options.html\" target=\"_blank\">LP Options</a> for detailed information on options. <div style=\"color:black\"> <p>Sample Test case : <p>tc_GetLPCommand <p>{ <p>LP.Select File (C:\\Users\\BaPr519\\Desktop\\Sample.docx) <p>LP.Select Printer (146.205.4.167)   <p>LP.Set Copies (1) <p>LP.Set Option (orientation-requested,4)    <p>LP.Get Lp Command (${command}) <p>} <p> Result : ${command}= lp -d '146.205.4.167' -n 1 -o 'orientation-requested=4' C:\\Users\\BaPr519\\Desktop\\Sample.docx </div>")]
    public class Lp : IGFLibrary
    {
        private LpPrintJob _printJob;
        public void Dispose()
        {
        }

        public bool DutUsed()
        {
            return false;
        }

        public List<string> GetDependencies()
        {
            return null;
        }

        public string GetName()
        {
            return "LP";
        }

        public void Initialize(DeviceUnderTest dut, string outputDir)
        {
            
        }

        [KeywordDescription("Select file to print. This keyword must be called at first when creating print job.")]
        [KeywordDisplayName("Select File")]
        [KeywordParameters("fileName", "file path to print")]
        [SampleScript("LP.Select File (C:\\Users\\BaPr519\\Desktop\\Sample.docx)")]
        public KeywordResult SelectFile(string fileName)
        {
            _printJob = new LpPrintJob();
            _printJob.File = fileName;
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Select Printer")]
        [KeywordDisplayName("Select Printer")]
        [KeywordParameters("printerName", "Name of Printer")]
        [SampleScript("<p>Pass scenario : LP.Select Printer (146.205.4.167) <p>Error scenario : If LP.Select File(filename) keyword is not used before this keyword , then will get error - File selection should be done before selecting printer.")]
        public KeywordResult SelectPrinter(string printerName)
        {
            printerName = printerName.Trim();
            if (_printJob == null)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "File selection should be done before selecting printer.";
                Logger.Error("File selection should be done before selecting printer.");
                return error;
            }

            _printJob.Destination = printerName;
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Set number of copies")]
        [KeywordDisplayName("Set Copies")]
        [KeywordParameters("copies", "number of copies")]
        [SampleScript("<p>Pass scenario : LP.Set Copies (1) <p>Error scenario : If LP.Select File(filename) keyword is not used before this keyword , then will get error - File selection should be done before selecting printer.")]
        public KeywordResult SetCopies(string copies)
        {
            copies = copies.Trim();
            if(_printJob == null)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "File selection should be done before setting options.";
                Logger.Error("File selection should be done before setting options.");
                return error;
            }

            if(!int.TryParse(copies, out int iCopies))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Copies must be a number.";
                Logger.Error("Copies must be a number");
                return error;
            }
            _printJob.NumCopies = iCopies;
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Set Option")]
        [KeywordDisplayName("Set Option")]
        [KeywordParameters("option", "option name")]
        [KeywordParameters("value", "option value")]
        [SampleScript("<p>Pass scenario : LP.Set Option (orientation-requested,4) <p>Error scenario : If LP.Select File(filename) keyword is not used before this keyword , then will get error - File selection should be done before selecting printer.")]
        public KeywordResult SetOption(string option, string value)
        {
            option = option.Trim();
            value = value.Trim();

            if (_printJob == null)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "File selection should be done before setting options.";
                Logger.Error("File selection should be done before setting options.");
                return error;
            }

            _printJob.AddOption(option, value);
            
            return new KeywordResult(KeywordResults.Pass);
        }

        [GetKeyword]
        [KeywordDescription("Get lp cli command")]
        [KeywordDisplayName("Get Lp Command")]
        [KeywordParameters("to", "variable for saving command")]
        [SampleScript("<p>Pass scenario : LP.Get Lp Command (${command}) <p>Ex: lp -d '146.205.4.167' -n 1 -o 'orientation-requested=4' C:\\Users\\BaPr519\\Desktop\\Sample.docx <p>Error scenario : If LP.Select File(filename) keyword is not used before this keyword , then will get error - File selection should be done before selecting printer.")]
        public KeywordResult GetLpCommand(string to)
        {
            if (_printJob == null)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Print job is not created.";
                Logger.Error("Print job is not created.");
                return error;
            }
            KeywordResult pass = new KeywordResult(KeywordResults.Pass);
            pass.Output = _printJob.GetLpCommand();
            CommonExecutionInfo.SetVariable(to.Trim(), pass.Output);
            return pass;
        }
    }
}

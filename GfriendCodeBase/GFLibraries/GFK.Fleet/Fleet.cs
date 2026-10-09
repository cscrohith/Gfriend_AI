using HP.GFriend.GFLogger;
using HP.GFriend.Keywords.Support;
using HP.GFriend.Support;
using System.Collections.Generic;
using System.IO;

namespace HP.GFriend.Keywords
{
    [LibraryDescription("Library is used to print text or files instantly to the specified printer. It uses the default port 9100." + "For example, to print some text, use Fleet.SendText(text). To print only .txt file, use Fleet.SendFile(filepath).<span style=\"color:black;\"> <p><br> Sample Testcase: <p>Tc<p>{ <p> Fleet.Send File (C:\\Users\\rdladmin\\Desktop\\TestScripts\\tcssh.txt,9100)<p> Fleet.Send Text (Print)<p> } </span>")]
    public class Fleet : IGFLibrary
    {
        private DeviceUnderTest _dut;
        private string _outputDir;
        public void Dispose()
        {

        }

        public bool DutUsed()
        {
            return true;
        }

        public List<string> GetDependencies()
        {
            return null;
        }

        public string GetName()
        {
            return "Fleet";
        }

        public void Initialize(DeviceUnderTest dut, string outputDir)
        {
            _dut = dut;
            _outputDir = outputDir;
        }

        [KeywordDescription("Send file to the device with 9100 port")]
        [KeywordDisplayName("Send File")]
        [KeywordParameters("filePath", "Path of file to send")]
        [SampleScript("Fleet.Send File (C:\\Users\\rdladmin\\Desktop\\TestScripts\\test.txt)")]
        public KeywordResult SendFile(string filePath)
        {
            return SendFile(filePath, "9100");
        }


        [KeywordDescription("Send file to the device")]
        [KeywordDisplayName("Send File")]
        [KeywordParameters("filePath", "Path of file to send")]
        [KeywordParameters("port", "port of printer (ex. 9100)")]
        [SampleScript("Fleet.Send File (C:\\Users\\rdladmin\\Desktop\\TestScripts\\test.txt,9100)")]
        public KeywordResult SendFile(string filePath, string port)
        {
            filePath = Utils.GetAbsolutePath(filePath, CommonExecutionInfo.ScriptFolder);
            if (!File.Exists(filePath))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"Can not find file : {filePath}";
                Logger.Error($"Can not find file : {filePath}");
                return error;
            }

            if (!int.TryParse(port, out int intPort))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"Port must be a number {port}";
                Logger.Error($"Port must be a number {port}");
                return error;
            }

            FileInfo fileInfo = new FileInfo(filePath);
            return MFPUtils.IpSend(_dut, fileInfo, intPort);
        }

        [KeywordDescription("Send(Print) text to the device with 9100 port")]
        [KeywordDisplayName("Send Text")]
        [KeywordParameters("text", "Text to send to the device")]
        [SampleScript("Fleet.Send Text (Print)")]
        public KeywordResult SendText(string text)
        {
            return SendText(text, "9100");
        }


        [KeywordDescription("Send(Print) text to the device")]
        [KeywordDisplayName("Send Text")]
        [KeywordParameters("text", "Text to send to the device")]
        [KeywordParameters("port", "port of printer (ex. 9100)")]
        [SampleScript("Fleet.Send Text (Print,9100)")]
        public KeywordResult SendText(string text, string port)
        {

            if (!int.TryParse(port, out int intPort))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"Port must be a number {port}";
                Logger.Error($"Port must be a number {port}");
                return error;
            }
            return MFPUtils.IpSend(_dut, text, intPort);
        }
    }
}

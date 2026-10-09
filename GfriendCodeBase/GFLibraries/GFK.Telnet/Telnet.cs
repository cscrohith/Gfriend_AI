using HP.GFriend.GFLogger;
using PrimS.Telnet;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HP.GFriend.Keywords
{
    public class Telnet : IGFLibrary
    {
        private Client _telnetClient = null;
        private DeviceUnderTest _dut;
        private string _outputDir;
        private string _lineFeed = "\n";

        public void Dispose()
        {
            Disconnect();
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
            return "Telnet";
        }

        public void Initialize(DeviceUnderTest dut, string outputDir)
        {
            _dut = dut;
            _outputDir = outputDir;
        }


        private async Task<string> SendCommandAndReadOutput(string command)
        {
            await _telnetClient.WriteLine(command, _lineFeed);
            return await ReadOutput();
            
        }

        private async Task<string> ReadOutput()
        {
            string output = await _telnetClient.ReadAsync();
            return output;
        }

        private async Task<string> ReadUntil(string expect, TimeSpan timeOut)
        {
            string value = string.Empty;
            string output = await _telnetClient.ReadAsync();
            value += output;
            DateTime endTime = DateTime.Now.AddSeconds(timeOut.TotalSeconds);
            while(!output.Contains(expect))
            {
                if(DateTime.Now > endTime)
                {
                    throw new TimeoutException(value);
                }
                output = await _telnetClient.ReadAsync();
                value += output;
            }
            return value;
            
        }


        [KeywordDescription("Connect to telnet with port which described in device information")]
        [KeywordDisplayName("Connect")]
        [SampleScript("Telnet.Connect")]
        public KeywordResult Connect()
        {
            return Connect(_dut.Port.ToString());
   
        }

        [KeywordDescription("Conect to telnet with given port")]
        [KeywordDisplayName("Connect")]
        [KeywordParameters("port", "telnet port to connect")]
        [SampleScript("Telnet.Connect(9114)")]
        public KeywordResult Connect(string port)
        {
            try
            {
                _telnetClient = new Client(_dut.DeviceAddress, int.Parse(port), new System.Threading.CancellationToken());
            }
            catch(Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error connecting telent.";
                error.AdditionalInfo = ex.ToString();
                Logger.Error("Error connecting telnet.", ex);
                return error;
            }
            if(_telnetClient.IsConnected)
            {
                return new KeywordResult(KeywordResults.Pass);
            }
            else
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = "Telnet is not connected";
                return fail;
            }
            
            
        }

        [KeywordDescription("Disconnect telnet")]
        [KeywordDisplayName("Disconnect")]
        [SampleScript("Telnet.Disconnect")]
        public KeywordResult Disconnect()
        {
            if(_telnetClient == null || !_telnetClient.IsConnected)
            {
                return new KeywordResult(KeywordResults.Pass);
            }
            try
            {
                _telnetClient.Dispose();
            }
            catch(Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error during disconnect";
                error.AdditionalInfo = ex.ToString();
                Logger.Error("Error during disconnect", ex);
                return error;
            }
            return new KeywordResult(KeywordResults.Pass);
            
        }

        [KeywordDescription("Set line feed character")]
        [KeywordDisplayName("Set Line Feed")]
        [KeywordParameters("lineFeed", "Line feed character. Default is '\\n'")]
        [SampleScript("Telnet.Set Line Feed (\\r\\n)")]
        public KeywordResult SetLineFeed(string lineFeed)
        {
            KeywordResult pass = new KeywordResult(KeywordResults.Pass);
            _lineFeed = lineFeed.Replace("\\r", "\r").Replace("\\n", "\n");
            pass.Output = $"Changed line feed to {lineFeed}";
            return pass;

        }

        [KeywordDescription("Send command to telent")]
        [KeywordDisplayName("Send")]
        [KeywordParameters("command", "Command to send")]
        [SampleScript("Telnet.Send(read status 004)")]
        public KeywordResult Send(string command)
        {
            try
            {
                Task<string> sendTask = Task.Run(() => SendCommandAndReadOutput(command));
                sendTask.Wait();

                Logger.Trace($"Telnet Send : {command}");
                Logger.Trace($"Telnet Receivce: {Environment.NewLine}{sendTask.Result}");

                KeywordResult pass = new KeywordResult(KeywordResults.Pass);
                pass.Output = sendTask.Result;
                return pass;
            }
            catch(Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error during send command";
                error.AdditionalInfo = ex.ToString();
                Logger.Error("Error during send command", ex);
                return error;
            }
        }

        [GetKeyword]
        [KeywordDescription("Read telnet output")]
        [KeywordDisplayName("Read")]
        [KeywordParameters("to", "Variable for saving output")]
        [SampleScript("Telnet.Read (${buffer})")]
        public KeywordResult Read(string to)
        {
            try
            {
                Task<string> readTask = Task.Run(() => ReadOutput());
                readTask.Wait();

                Logger.Trace($"Telnet Receivce: {Environment.NewLine}{readTask.Result}");
                CommonExecutionInfo.SetVariable(to.Trim(), readTask.Result);
                KeywordResult pass = new KeywordResult(KeywordResults.Pass);
                pass.Output = readTask.Result;
                return pass;
            }
            catch(Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error during read output";
                error.AdditionalInfo = ex.ToString();
                Logger.Error("Error during read output", ex);
                return error;
            }
            

        }

        [GetKeyword]
        [KeywordDescription("Read telnet output until expected output with default timeout of 5 seconds")]
        [KeywordDisplayName("Read Until")]
        [KeywordParameters("to", "Variable for saving output")]
        [KeywordParameters("expect", "expected string to read until")]
        [SampleScript("Telnet.Read Until(${buffer},Password)")]
        public KeywordResult ReadUntil(string to, string expect)
        {
            return ReadUntil(to, expect, "5");
        }

        [GetKeyword]
        [KeywordDescription("Read telnet output until expected output")]
        [KeywordDisplayName("Read Until")]
        [KeywordParameters("to", "Variable for saving output")]
        [KeywordParameters("expect", "expected string to read until")]
        [KeywordParameters("timeOut", "Timeout in seconds")]
        [SampleScript("Telnet.Read Until(${buffer},Password,5)")]
        public KeywordResult ReadUntil(string to, string expect, string timeOut)
        {
            expect = Support.Utils.GetVariablevalueIfExist(expect);
            timeOut = Support.Utils.GetVariablevalueIfExist(timeOut);

            if(!int.TryParse(timeOut.Trim(),out int iTimeOut))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Timeout must be a number.";
                Logger.Error(error.Output);
                return error;
            }

            try
            {
                Task<string> readTask = Task.Run(() => ReadUntil(expect, TimeSpan.FromSeconds(iTimeOut)));
                readTask.Wait();

                Logger.Trace($"Telnet Receivce: {Environment.NewLine}{readTask.Result}");
                
                
                CommonExecutionInfo.SetVariable(to.Trim(), readTask.Result);
                KeywordResult pass = new KeywordResult(KeywordResults.Pass);
                pass.Output = readTask.Result;
                return pass;
            }
            catch (Exception ex)
            {
                if (ex.InnerException is TimeoutException)
                {
                    KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                    fail.Output = $"Can not get expected output.\r\n[Output]\r\n{ex.InnerException.Message}";
                    Logger.Error(fail.Output, ex.InnerException);
                    return fail;
                }
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error during read output";
                error.AdditionalInfo = ex.ToString();
                Logger.Error("Error during read output", ex);
                return error;
            }


        }
    }
}

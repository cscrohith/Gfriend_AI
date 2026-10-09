using HP.GFriend.GFLogger;
using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using WinSCP;

namespace HP.GFriend.Keywords
{
    [LibraryDescription("SSH will connect anything Linux, windows, IOS, Android. <p> Order of the SSH keywords : <p> Ssh.connect(146.204.93.205,22,duneroot,rdl@123), <p> Ssh.Send (systemct1 status sshd),<p> Ssh.Get File (/home/duneroot/newfile.txt,Test.txt),<p> Ssh.Push File (C:\\Users\\MKavyash\\Downloads\\pushfile.txt,/home/duneroot/Desktop/kaa/test2.txt),<p>Ssh.Disconnect")]
    public class Ssh : IGFLibrary
    {
        private DeviceUnderTest _dut;
        private string _outputDir;
        private SshClient _ssh;
        private ConnectionInfo _connectionInfo;
        private string _password;
        private ShellStream _stream;
        private Regex _promptRegex;

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
            return "Ssh";
        }

        public void Initialize(DeviceUnderTest dut, string outputDir)
        {
            _dut = dut;
            _outputDir = outputDir;
        }

      
        [KeywordDescription("Connect to ssh with device address, port, admin id and admin password")]
        [KeywordDisplayName("Connect")]
        [SampleScript("Ssh.Connect")]
        public KeywordResult Connect()
        {
            return Connect(_dut.DeviceAddress, _dut.Port.ToString(), _dut.AdminId, _dut.AdminPassword);
        }

        [KeywordDescription("Connect to ssh")]
        [KeywordDisplayName("Connect")]
        [KeywordParameters("host", "host address to connect")]
        [KeywordParameters("port", "ssh port to connect")]
        [KeywordParameters("userName", "user name of ssh")]
        [KeywordParameters("userPassword", "user password of ssh")]
        [SampleScript("Ssh.connect(146.204.93.205,22,duneroot,rdl@123)")]
        public KeywordResult Connect(string host, string port, string userName, string userPassword)
        {
            host = host.Trim();
            port = port.Trim();
            userName = userName.Trim();
            userPassword = userPassword.Trim();

            try
            {
                PasswordAuthenticationMethod auth = new PasswordAuthenticationMethod(userName, userPassword);
                _connectionInfo = new ConnectionInfo(host, int.Parse(port), userName, auth);
                _password = userPassword;
                _ssh = new SshClient(_connectionInfo);
                _ssh.Connect();
                //_promptRegex = new Regex(userName + @"\@[a-zA-Z0-9~@#$%^&*()_+-=]*\:.[a-zA-Z0-9/~~@#$%^&*()_+-=]*\$", RegexOptions.Compiled);
                _promptRegex = new Regex(@"\@[a-zA-Z0-9~@#$%^&*()_+-=:]*[a-zA-Z0-9/~~@#$%^&*()_+-=:\s~][ %$~ ]*", RegexOptions.Compiled);

                var modes = new Dictionary<Renci.SshNet.Common.TerminalModes, uint>();
                _stream = _ssh.CreateShellStream("xterm", 255, 50, 800, 600, 1024, modes);

                string expect = _stream.Expect(_promptRegex, TimeSpan.FromSeconds(5));
                
                while (_stream.ReadLine(TimeSpan.FromSeconds(1)) != null) ;                
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Connection error";
                error.AdditionalInfo = ex.ToString();
                Logger.Error("Connection error", ex);
                return error;
            }
        }

        [KeywordDescription("Disconnect from ssh")]
        [KeywordDisplayName("Disconnect")]
        [SampleScript("Ssh.Disconnect")]
        public KeywordResult Disconnect()
        {
            if(_ssh == null || !_ssh.IsConnected)
            {
                return new KeywordResult(KeywordResults.Pass);
            }

            try
            {
                _ssh.Disconnect();
                return new KeywordResult(KeywordResults.Pass);
            }
            catch(Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error during disconnect";
                error.AdditionalInfo = ex.ToString();
                Logger.Error("Error during disconnect", ex);
                return error;
            }
        }

        [KeywordDescription("Send exit command from ssh")]
        [KeywordDisplayName("Exit")]
        [SampleScript("Ssh.Exit")]
        public KeywordResult Exit()
        {
            try
            {
                _stream.Write($"exit\n");
                KeywordResult pass = new KeywordResult(KeywordResults.Pass);
                return pass;
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error during send command";
                error.AdditionalInfo = ex.ToString();
                Logger.Error("Error during send command", ex);
                return error;
            }
        }

        [KeywordDescription("Send command via ssh")]
        [KeywordDisplayName("Send")]
        [KeywordParameters("command", "command to send")]
        [SampleScript("Ssh.Send (Is)")]
        public KeywordResult Send(string command)
        {
            try
            {
                _stream.Flush();
                _stream.Write($"{command}\n");
                _stream.ReadLine(TimeSpan.FromSeconds(1));
                string expect = _stream.Expect(_promptRegex, TimeSpan.FromSeconds(5));
                if (string.IsNullOrEmpty(expect))
                {
                    KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                    fail.Output = "Prompt not found after executing command.";
                    return fail;
                }
                string output = expect.ToString();
                KeywordResult pass = new KeywordResult(KeywordResults.Pass);
                output = new Regex(@"\x1B\[[^@-~]*[@-~]").Replace(output, "");
                
                pass.Output = DeleteEndLine(output);
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

        [KeywordDescription("Send command via ssh with sudo")]
        [KeywordDisplayName("Sudo Send")]
        [KeywordParameters("command", "command to send")]
        [SampleScript("Ssh.Sudo Send (systemctl status sshd)")]
        public KeywordResult SudoSend(string command)
        {
            try
            {
                _stream.Flush();
                _stream.Write($"sudo {command}\n");
                _stream.ReadLine(TimeSpan.FromSeconds(1));

                Regex passwordRegex = new Regex("password|Password");
                string expect = _stream.Expect(passwordRegex, TimeSpan.FromSeconds(5));
                
                if (!string.IsNullOrEmpty(expect))
                {
                    _stream.Write($"{_password}\n");
                }

                string output = _stream.Expect(_promptRegex, TimeSpan.FromSeconds(5));

                if (string.IsNullOrEmpty(output))
                {
                    KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                    fail.Output = "Prompt not found after executing command.";
                    return fail;
                }

                output = new Regex(@"\x1B\[[^@-~]*[@-~]").Replace(output, "");
                KeywordResult pass = new KeywordResult(KeywordResults.Pass);
                pass.Output = DeleteEndLine(output);

                return pass;                
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error during sudo send command";
                error.AdditionalInfo = ex.ToString();
                Logger.Error("Error during sudo send command", ex);
                return error;
            }
        }
        
        [KeywordDescription("Port forward with ssh")]
        [KeywordDisplayName("Port Forward")]
        [KeywordParameters("remotePort", "remote port to forward")]
        [KeywordParameters("localPort", "local port to bind")]
        [SampleScript("Ssh.Port Forward (22,13)")]
        public KeywordResult PortForward(string remotePort, string localPort)
        {
            remotePort = remotePort.Trim();
            localPort = localPort.Trim();
            if(!uint.TryParse(remotePort, out uint uiRemotePort) || !uint.TryParse(localPort, out uint uiLocalPort))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Port should be a number";
                return error;
            }
            ForwardedPortLocal ports = new ForwardedPortLocal("127.0.0.1", uiLocalPort, "localhost", uiRemotePort);
            

            try
            {
                _ssh.AddForwardedPort(ports);
                ports.Start();
                return new KeywordResult(KeywordResults.Pass);
            }
            catch(Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error during port forward";
                error.AdditionalInfo = ex.ToString();
                Logger.Error("Error during port forward");
                return error;

            }
        }

        [KeywordDescription("Copy remote file to local")]
        [KeywordDisplayName("Get File")]
        [KeywordParameters("filePathToGet", "Remote file path to get")]
        [KeywordParameters("saveTo", "local file path to save")]
        [SampleScript("Ssh.Get File(/home/duneroot/Desktop/kaa/Test.txt, Downloads\\test1.txt)")]      
        public KeywordResult GetFile(string filePathToGet, string saveTo)
        {
            if(string.IsNullOrEmpty(Path.GetPathRoot(saveTo)))
            {
                saveTo = Path.Combine(_outputDir, saveTo);
            }

            try
            {
                using (ScpClient client = new ScpClient(_connectionInfo))
                {
                    client.Connect();
                    if(!Directory.Exists(Path.GetDirectoryName(saveTo)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(saveTo));
                    }
                    using (FileStream saveFileStream = new FileStream(saveTo, FileMode.Create))
                    {
                        client.Download(filePathToGet, saveFileStream);
                    }
                }
            }
            catch(Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error during get file";
                error.AdditionalInfo = ex.ToString();
                Logger.Error("Error during get file", ex);
                return error;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Copy local file to remote")]
        [KeywordDisplayName("Push File")]
        [KeywordParameters("fileToPush", "local file to copy")]
        [KeywordParameters("saveTo", "remote path to save")]
        [SampleScript("Ssh.Push File (C:\\Users\\MKavyash\\Downloads\\pushfile.txt,/home/duneroot/Desktop/kaa/test2.txt)")]
        public KeywordResult PushFile(string fileToPush, string saveTo)
        {
            fileToPush = Support.Utils.GetAbsolutePath(fileToPush, CommonExecutionInfo.ScriptFolder);
            try
            {
                using (ScpClient client = new ScpClient(_connectionInfo))
                {
                    client.Connect();
                    using (FileStream uploadFileStream = new FileStream(fileToPush, FileMode.Open))
                    {
                        client.Upload(uploadFileStream, saveTo);
                    }
                }
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error during push file";
                error.AdditionalInfo = ex.ToString();
                Logger.Error("Error during push file", ex);
                return error;
            }
            return new KeywordResult(KeywordResults.Pass);
        }

        [KeywordDescription("Copy remote file to local")]
        [KeywordDisplayName("Get Files")]
        [KeywordParameters("remotePath", "Remote file path to get")]
        [KeywordParameters("saveTo", "local file path to save")]
        [SampleScript("/mnt/machinedata/log/,C:\\Users\\XAppanna\\Downloads\\PrinterLogfolder1\\")]
        public KeywordResult GetFiles(string remotePath, string saveTo)
        {
            if (string.IsNullOrEmpty(Path.GetPathRoot(saveTo)))
            {
                saveTo = Path.Combine(_outputDir, saveTo);
            }

            try
            {
                SessionOptions sessionOptions = new SessionOptions
                {
                    Protocol = Protocol.Sftp,
                    HostName = _connectionInfo.Host,
                    UserName = _connectionInfo.Username,
                    Password = _password,
                    PortNumber = _connectionInfo.Port,
                    GiveUpSecurityAndAcceptAnySshHostKey = true
                };

                using (WinSCP.Session session = new WinSCP.Session())
                {
                    session.Open(sessionOptions);

                    TransferOptions transferOptions = new TransferOptions
                    {
                        TransferMode = TransferMode.Binary
                    };

                    if (remotePath.EndsWith("/")) 
                    {
                        TransferOperationResult transferResult = session.GetFiles(remotePath + "*", saveTo, false, transferOptions);
                        transferResult.Check();

                        int total = transferResult.Transfers.Count;

                        for (int i = 0; i < total; i++)
                        {
                            int percent = (int)(((i + 1) / (double)total) * 100);
                            ShowProgressInline(percent);
                        }
                    }
                    else 
                    {
                        string fileName = Path.GetFileName(remotePath);
                        session.GetFiles(remotePath, saveTo, false, transferOptions).Check();
                        ShowProgressInline(100);
                    }
                }
            }
            catch (Exception ex)
            {
                var error = new KeywordResult(KeywordResults.Error)
                {
                    Output = "Error during get file/folder",
                    AdditionalInfo = ex.ToString()
                };
                Logger.Error("Error during get file/folder", ex);
                return error;
            }

            return new KeywordResult(KeywordResults.Pass)
            {
                Output = "File(s) copied successfully."
            };
        }
        private void ShowProgressInline(int percent)
        {
            string message = $"Copying files... {percent}% complete";
            Console.Write("\r" + message.PadRight(80)); 
        }

        private string DeleteEndLine(string output)
        {
            if (!String.IsNullOrEmpty(output))
            {
                string[] lines = output.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                output = "";
                for (int i = 0; i < lines.Length - 1 ; i++)
                {
                    output += lines[i] + System.Environment.NewLine;
                }
            }
            return output;
        }
    }
}

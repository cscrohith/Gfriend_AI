using HP.GFriend.Core.Execution;
using HP.GFriend.GFLogger;
using HP.GFriend.GFWcfService;
using HP.GFriend.Keywords;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HP.GFriend.Core.Remote
{
    public class RemoteExecutor : IDisposable
    {
        public string RemoteId { get; }
        public string SessionId { get; private set; }
        public TestDataManager RemoteTestDataManager { get; set; }
        public List<string> Usings { get; set; }
        public Task<string> RunTask { get; set; }
        
        private IGFRemoteService _channel;
        private string _outputPath;

        public RemoteExecutor(string remoteId)
        {
            RemoteId = remoteId;
            SessionId = Guid.NewGuid().ToString();
            RemoteTestDataManager = new TestDataManager();
            Usings = new List<string>();
        }

        public void Initialize(List<DeviceUnderTest> duts, string tsPath, string outputPath)
        {
            _outputPath = outputPath;
            DeviceUnderTest remote = duts.Where(d => d.DeviceId.Equals(RemoteId)).FirstOrDefault();
            if(remote == null)
            {
                throw new NullReferenceException("Remote executor is not defined in Device list");
            }

            WSHttpBinding binding = new WSHttpBinding(SecurityMode.None);
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            binding.Security.Message.ClientCredentialType = MessageCredentialType.None;
            binding.Security.Message.NegotiateServiceCredential = false;
            binding.Security.Message.EstablishSecurityContext = false;
            binding.MaxReceivedMessageSize = int.MaxValue;
            binding.MaxBufferPoolSize = int.MaxValue;

            ChannelFactory<IGFRemoteService> channelFactory = new ChannelFactory<IGFRemoteService>(binding);
            _channel = channelFactory.CreateChannel(new EndpointAddress(new Uri($"http://{remote.DeviceAddress}:{remote.Port}/GFRemoteService.svc")));
            

            // Check version of agent
            string coreVersion = typeof(Executor).Assembly.GetName().Version.ToString();
            string agentVersion = _channel.GetVersion();
            Logger.Trace($"Core verison : {coreVersion}");
            Logger.Trace($"Agent verison : {agentVersion}");
            
            if (!coreVersion.Equals(agentVersion))
            {
                Console.WriteLine($"Update GFriend on Remote agent : {RemoteId}");
                Logger.Trace("Version mismatch with core and agent. Update agent");
                try
                {
                    _channel.Update("https://8v0mj43oxb.execute-api.ap-northeast-2.amazonaws.com/gfriend");
                }
                catch (Exception) { }

                
                DateTime endTime = DateTime.Now.AddMinutes(2);
                while (true)
                {
                    try
                    {
                        _channel.GetVersion();
                        break;
                    }
                    catch(Exception)
                    {
                        if(DateTime.Now > endTime)
                        {
                            throw new EndpointNotFoundException("Error during agent update");
                        }
                        Thread.Sleep(5000);
                    }
                    
                }
            }


            _channel.StartSession(SessionId);

            foreach (string file in RemoteTestDataManager.FilesToUse)
            {
                Logger.Debug($"Transfer {file} to remote executor");
                string filePath = Path.Combine(Path.GetDirectoryName(tsPath), file);
                byte[] fileByte = File.ReadAllBytes(filePath);
                GFFile toUpload = new GFFile(SessionId, file, fileByte);
                try
                {
                    string output = _channel.SetFilesToUse(toUpload);
                    Logger.Debug(output);
                }
                catch(Exception ex)
                {
                    Logger.Error("Error during set file.", ex);
                }
                
                
            }

            _channel.Initialize(SessionId, duts, Usings);
        }

        public void Run(RemoteBlock block, int repeatCount = -1, int stackLevel = 0, Dictionary<string, string> arguments = null)
        {
            RunTask = Task.Run(() => _channel.RunRemote(SessionId, block.BlockId, block));
        }

        public void Dispose()
        {
            Dispose(false);
        }

        public void Dispose(bool abort)
        {
            if(abort && (!RunTask?.IsCompleted ?? false))
            {
                Logger.Trace($"Force dispose remote task of {RemoteId}");
                try
                {
                    RunTask.Dispose();
                }
                catch (Exception) { }
            }

            if(!RunTask?.IsCompleted ?? false)
            {
                RunTask.Wait();
            }

            if (string.IsNullOrEmpty(SessionId) || _channel == null) return;

            try
            {
                List<string> outputFiles = _channel.GetOutputFileList(SessionId);
                foreach (string outputFile in outputFiles)
                {
                    GFFile file = _channel.GetOutputFile(SessionId, outputFile);
                    string writeTo = Path.Combine(_outputPath, file.FilePath);
                    using (FileStream fileStream = new FileStream(writeTo, FileMode.Create, FileAccess.Write))
                    {
                        fileStream.Write(file.FileByte, 0, file.FileByte.Length);
                    }
                }
                _channel.EndSession(SessionId);
            }
            catch(Exception ex)
            {
                Logger.Error("Error while disposing.", ex);
            }
            SessionId = string.Empty;
        }
    }
}

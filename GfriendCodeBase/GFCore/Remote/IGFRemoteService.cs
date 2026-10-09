using HP.GFriend.Core;
using HP.GFriend.Core.Execution;
using HP.GFriend.Keywords;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace HP.GFriend.GFWcfService
{
    [ServiceContract]
    public interface IGFRemoteService
    {

        [OperationContract]
        string GetVersion();

        [OperationContract]
        bool InUse();

        [OperationContract]
        void ForceReset();

        [OperationContract]
        void Update(string gfSeverEndpoint);

        [OperationContract]
        void StartSession(string sessionId);

        [OperationContract]
        string SetFilesToUse(GFFile file);

        [OperationContract]
        void Initialize(string sessionId, List<DeviceUnderTest> duts, List<string> usings);

        [OperationContract]
        Task<string> RunRemote(string sessionId, string blockId, RemoteBlock remoteBlock, int repeatCount = -1, int stackLevel = 0, Dictionary<string, string> arguments = null);

        [OperationContract]
        List<string> GetOutputFileList(string sessionId);

        [OperationContract]
        GFFile GetOutputFile(string sessionId, string targetFile);

        [OperationContract]
        void EndSession(string sessionId);

        [OperationContract]
        void Dispose(string sessionId);


    }

    [DataContract]
    public class GFFile
    { 
        [DataMember]
        public string SessionId { get; set; }
        
        [DataMember]
        public string FilePath { get; set; }
        
        [DataMember]
        public byte[] FileByte { get; set; }

        public GFFile(string sessionId, string filePath, byte[] fileByte)
        {
            SessionId = sessionId;
            FilePath = filePath;
            FileByte = fileByte;
            
        }
    }

}

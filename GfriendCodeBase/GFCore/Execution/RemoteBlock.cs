using HP.GFriend.Core.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HP.GFriend.Core.Execution
{
    [DataContract]
    public class RemoteBlock : IGFRunnable
    {
        [DataMember]
        public List<Tuple<int,string>> BlockContents;
        
        [DataMember]
        public string SessionId { get; set; }
        
        [DataMember]
        public string RemoteId { get; set; }
        
        [DataMember]
        public string OriginalStatement { get; set; }
        
        [DataMember]
        public string BlockId { get; set; }

        [DataMember]
        public string Result { get; set; }

        [DataMember]
        public bool IsStatement
        {
            get
            {
                return false;
            }
            
            internal set
            {
                value = false;
            }
        }

        [DataMember]
        public bool IsAlwaysPass { get; internal set; }
        public RemoteBlock()
        {
            BlockContents = new List<Tuple<int, string>>();
        }
        public void AddContents(int depth, string line)
        {
            BlockContents.Add(new Tuple<int, string>(depth, line));
        }

        public void Run(TestDataManager testDataManager, int repeatCount = -1, int stackLevel = 0, Dictionary<string, string> arguments = null)
        {
            
            BlockId = Guid.NewGuid().ToString();
            Result = "Running";
            RemoteExecutor remoteExecutor = testDataManager.RemoteExecutors[RemoteId];
            Dictionary<string, string> attributes = new Dictionary<string, string>();
            attributes.Add("SessionID", remoteExecutor.SessionId);
            attributes.Add("BlockID", BlockId);
            attributes.Add("RemoteID", RemoteId);
            Reporter.WriteToOutput("Remote", attributes, false);

            Console.Write("{0}{1}", new string('\t', stackLevel), OriginalStatement);
            Console.WriteLine("{0} :: {1}", new string('\t', stackLevel), Result);
            remoteExecutor.Run(this, repeatCount, stackLevel, arguments);
            Reporter.WriteToOutput("Remote", false);

        }

        public void AddSubBlock(IGFRunnable subBlock)
        {
            throw new NotSupportedException("Sub block is not supported in Remote block");
        }
    }
}

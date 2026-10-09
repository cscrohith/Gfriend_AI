using Newtonsoft.Json.Linq;
using System;

namespace HP.GFriend.Client
{
    public class TestResult : IGFServerData
    {
        public string TestID { get; set; }
        public string ExecutionID { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int TestScriptId { get; set; }
        public int Repetation { get; set; }
        public int Pass { get; set; }
        public int Fail { get; set; }
        public int Error { get; set; }

        public TestResult(string executionID)
        {
            ExecutionID = executionID;
            StartTime = null;
            EndTime = null;
        }

        public TestResult(JObject json)
        {
            TestID = json["id"].ToString();
            ExecutionID = json["executionid"].ToString();
            DateTime tmpDT;
            if (DateTime.TryParse(json["starttime"].ToString(), out tmpDT))
            {
                StartTime = tmpDT.ToLocalTime();
            }
            else
            {
                StartTime = null;
            }

            if (DateTime.TryParse(json["endtime"].ToString(), out tmpDT))
            {
                EndTime = tmpDT.ToLocalTime();
            }
            else
            {
                EndTime = null;
            }
            
            TestScriptId = int.Parse(json["testscriptid"].ToString());
            Repetation = int.Parse(json["repetation"].ToString());
            Pass = int.Parse(json["passcnt"].ToString());
            Fail = int.Parse(json["failcnt"].ToString());
            Error = int.Parse(json["errorcnt"].ToString());
        }

        public string ToJson()
        {
            JObject json = new JObject
            {
                { "id", TestID},
                {"executionid", ExecutionID },
                {"starttime", StartTime },
                {"endtime", EndTime },
                {"testscriptid", TestScriptId },
                {"repetation", Repetation },
                {"passcnt", Pass },
                {"failcnt", Fail },
                {"errorcnt", Error }
            };
            return json.ToString();
        }
    }
}

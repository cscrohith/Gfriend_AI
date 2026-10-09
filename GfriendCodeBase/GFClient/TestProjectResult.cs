using Newtonsoft.Json.Linq;

namespace HP.GFriend.Client
{
    public class TestProjectResult : IGFServerData
    {
        public string ExecutionID { get; }
        public int Pass { get; set; }
        public int Fail { get; set; }
        public int Error { get; set; }


        public TestProjectResult(string executionID)
        {
            ExecutionID = executionID;
            Pass = 0;
            Fail = 0;
            Error = 0;
        }

        public TestProjectResult(string executionID, JObject json)
        {
            ExecutionID = executionID;
            int tmpCnt = 0;
            int.TryParse(json["passcnt__sum"].ToString(), out tmpCnt);
            Pass = tmpCnt;
            int.TryParse(json["failcnt__sum"].ToString(), out tmpCnt);
            Fail = tmpCnt;
            int.TryParse(json["errorcnt__sum"].ToString(), out tmpCnt);
            Error = tmpCnt;
        }

        public string ToJson()
        {
            throw new System.NotImplementedException();
        }
    }
}

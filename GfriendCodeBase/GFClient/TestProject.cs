using HP.GFriend.Keywords;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HP.GFriend.Client
{
    public enum RepeatType
    {
        Duration,
        Count
    }

    public enum ExecutionStatus
    {
        Started,
        Running,
        Completed,
        Error,
        Canceling,
        Canceled,
        Unknown,
        Aborted
    }
    public class TestProject : IGFServerData
    {
        public string ExecutionID { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<int> ScriptsToRun { get; set; }
        public RepeatType RepeatMethod { get { return _repeatType; } }
        public int RepeatCount { get { return _repeatCount; } }
        public TimeSpan RepeatDuration { get { return _repeatDuration; } }
        public DeviceUnderTest DUT { get; set; }
        public string TestReportURL { get; set; }
        public string JenkinsJobURL { get; set; }
        public string EmailTo { get; set; }
        public string Author { get; set; }
        public ExecutionStatus Status { get; set; }


        private RepeatType _repeatType;
        private int _repeatCount;
        private TimeSpan _repeatDuration;

        public TestProject(List<int> scriptToRun, DeviceUnderTest dut)
        {
            ScriptsToRun = scriptToRun;
            DUT = dut;
            StartTime = null;
            EndTime = null;
        }
        public TestProject(JObject json)
        {
            ExecutionID = json["id"].ToString();
            DateTime tmpDT;
            if(DateTime.TryParse(json["starttime"].ToString(), out tmpDT))
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
            
            ScriptsToRun = json["testscripts"].ToString().Split(',').Select(int.Parse).ToList();
            _repeatType = (RepeatType)Enum.Parse(typeof(RepeatType), json["repeattype"].ToString());
            _repeatCount = int.Parse(json["repeatcount"].ToString());
            _repeatDuration = TimeSpan.FromMinutes(int.Parse(json["repeatduration"].ToString()));
            DeserializeDUT(json["deviceinfo"].ToString());
            TestReportURL = json["testreport"].ToString();
            JenkinsJobURL = json["jenkinsurl"].ToString();
            EmailTo = json["emailto"].ToString();
            Author = json["author"].ToString();
            if(!string.IsNullOrEmpty(json["status"].ToString()))
            {
                Status = (ExecutionStatus)Enum.Parse(typeof(ExecutionStatus), json["status"].ToString());
            }
            else
            {
                Status = ExecutionStatus.Unknown;
            }
            

        }

        public void SetRepeat(RepeatType type)
        {
            switch (type)
            {
                case RepeatType.Count:
                    _repeatType = RepeatType.Count;
                    _repeatDuration = TimeSpan.FromSeconds(0);
                    break;
                case RepeatType.Duration:
                    _repeatType = RepeatType.Duration;
                    _repeatCount = -1;
                    break;
            }
        }

        public void SetRepeat(int count)
        {
            _repeatType = RepeatType.Count;
            _repeatCount = count;
            _repeatDuration = TimeSpan.FromSeconds(0);
        }

        public void SetRepeat(TimeSpan duration)
        {
            _repeatType = RepeatType.Duration;
            _repeatDuration = duration;
            _repeatCount = -1;
        }

        public string GetRepeat()
        {
            string repeatStr = _repeatType.ToString();
            switch(_repeatType)
            {
                case RepeatType.Count:
                    repeatStr += $":: {_repeatCount} times ";
                    break;
                case RepeatType.Duration:
                    repeatStr += $":: for {_repeatDuration.ToString()}";
                    break;
            }
            return repeatStr;
        }

        public string ToJson()
        {
            JObject json = new JObject
            {
                { "id", ExecutionID },
                { "starttime", StartTime },
                { "endtime", EndTime },
                { "testscripts", string.Join(",", ScriptsToRun) },
                { "repeattype", RepeatMethod.ToString() },
                { "repeatcount", RepeatCount },
                { "repeatduration", RepeatDuration.TotalMinutes },
                { "deviceinfo", SerializeDUT() },
                { "testreport", TestReportURL },
                { "jenkinsurl", JenkinsJobURL },
                { "emailto",  EmailTo },
                { "author", Author },
                { "status", Status.ToString() }
            };

            return json.ToString();
        }

        private string SerializeDUT()
        {
            JObject json = new JObject
            {
                {"address", DUT.DeviceAddress },
                {"adminid", DUT.AdminId },
                {"adminpw", DUT.AdminPassword },
                {"landebug", DUT.LanDebugAddress }
            };
            return json.ToString();
        }

        private void DeserializeDUT(string jsonStr)
        {
            JObject json = JObject.Parse(jsonStr);
            if(DUT == null)
            {
                DUT = new DeviceUnderTest();
            }
            DUT.DeviceAddress = json["address"].ToString();
            if(json["adminid"] != null)
            {
                DUT.AdminId = json["adminid"].ToString();
            }
            if(json["adminpw"] != null)
            {
                DUT.AdminPassword = json["adminpw"].ToString();
            }
            
            if(json["landebug"] != null)
            {
                DUT.LanDebugAddress = json["landebug"].ToString();
            }

        }
    }
}

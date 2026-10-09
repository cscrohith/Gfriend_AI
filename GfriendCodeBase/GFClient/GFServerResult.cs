using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace HP.GFriend.Client
{
    public enum RequestStatus
    {
        [Description("OK, It's going according to plan")]
        Success,

        [Description("Server Connection Error")]
        ConnectionError,

        [Description("Not enough test executor")]
        ShortOfExecutor,

        [Description("Can not find specified key")]
        KeyError,

        [Description("Ouch, server has some errors")]
        ServerError,

        [Description("Unexpected error")]
        UnExpected,
    }
    public class GFServerResult
    {
        public List<IGFServerData> DataList { get; set; }
        public RequestStatus Status { get; set; }
        public String Message { get; set; }
        public IGFServerData Data { get { return DataList[0]; } }

        public GFServerResult(RequestStatus status)
        {
            DataList = new List<IGFServerData>();
            Status = status;
        }

        public GFServerResult(string message)
        {
            DataList = new List<IGFServerData>();
            Status = RequestStatus.Success;
            Message = message;
        }

        public GFServerResult(RequestStatus status, string message)
        {
            Status = status;
            Message = message;
        }

        public GFServerResult(IGFServerData data, RequestStatus status)
        {
            DataList = new List<IGFServerData>();
            Status = status;
            DataList.Add(data);
        }

        public GFServerResult(IGFServerData data)
        {
            DataList = new List<IGFServerData>();
            DataList.Add(data);
            Status = RequestStatus.Success;
        }

        public GFServerResult(List<IGFServerData> data, RequestStatus status)
        {
            DataList = data;
            Status = status;
            
        }

        public GFServerResult(List<IGFServerData> data)
        {
            DataList = data;            
            Status = RequestStatus.Success;
        }

        public string GetStatusDescription()
        {
            FieldInfo fi = Status.GetType().GetField(Status.ToString());
            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                    typeof(DescriptionAttribute), false);

            if (attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return Status.ToString();
            }
        }

    }
}

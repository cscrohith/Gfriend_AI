using System;

namespace HP.GFriend.Keywords.Event
{
    public class KeywordEventArgs : EventArgs
    {
        public string KeywordName { get; }
        public object EventDetail { get; }
        public DateTime EventTime { get; }

        public KeywordEventArgs(string keywordName, object eventDetail, DateTime eventTime)
        {
            KeywordName = keywordName;
            EventDetail = eventDetail;
            EventTime = eventTime;
        }
    }
}

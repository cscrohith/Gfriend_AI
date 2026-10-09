using System;

namespace HP.GFriend.Keywords.Event
{
    public static class KeywordEvent
    {
        public static event EventHandler<KeywordEventArgs> KeywordExecuted;

        public static void Trigger(string keywordName, object eventDetail)
        {
            KeywordExecuted?.Invoke(null, new KeywordEventArgs(keywordName, eventDetail, DateTime.Now));
        }
    }
}

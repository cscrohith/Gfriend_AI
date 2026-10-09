using System;

namespace HP.GFriend.Event
{
    public class GFEventArgs : EventArgs
    {
        public object EventName { get; }

        public GFEventArgs(object eventName)
        {
            if(eventName == null)
            {
                throw new ArgumentNullException(nameof(eventName), "Event name must be provided");
            }
            EventName = eventName;
        }

        public override string ToString() => EventName.ToString();
    }
}

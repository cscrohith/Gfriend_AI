using System;

namespace HP.GFriend.Event
{
    public static class GFEvent
    {
        public static event EventHandler<GFEventArgs> OnGFEvent;

        public static void Trigger(object eventName)
        {
            OnGFEvent?.Invoke(null, new GFEventArgs(eventName));
        }

    }
}

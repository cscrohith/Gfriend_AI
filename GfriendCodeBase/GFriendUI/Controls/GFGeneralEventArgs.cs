using System;

namespace HP.GFriend.UI.Controls
{
    public class GFGeneralEventArgs : EventArgs
    {
        public GFGeneralEventArgs(string message)
        {
            Message = message;
        }
        public string Message { get; set; }
    }
}

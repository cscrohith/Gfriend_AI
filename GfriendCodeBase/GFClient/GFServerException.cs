using System;

namespace HP.GFriend.Client
{
    public class GFServerException : Exception
    {
        public GFServerException() : base() { }
        public GFServerException(string message) : base(message) { }
    }
}

using System;

namespace HP.GFriend.GFWcfService
{
    public class GFRemoteException : Exception
    {
        public GFRemoteException() : base() { }
        public GFRemoteException(string message) : base(message) { }

    }
}
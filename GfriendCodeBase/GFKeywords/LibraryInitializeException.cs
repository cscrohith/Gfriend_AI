using System;

namespace HP.GFriend.Keywords
{
    public class LibraryInitializeException : Exception
    {
        
        public LibraryInitializeException(string message) : base(message)
        {
        }

        public LibraryInitializeException(string message, Exception ex) : base(message, ex)
        { }
    }
}

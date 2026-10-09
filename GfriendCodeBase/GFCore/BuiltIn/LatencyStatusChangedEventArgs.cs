using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HP.GFriend.Core.BuiltIn
{
    public class LatencyStatusChangedEventArgs : EventArgs
    {
        public bool IsNetworkDown { get; }

        public LatencyStatusChangedEventArgs(bool isNetworkDown)
        {
            IsNetworkDown = isNetworkDown;
        }
    }
}

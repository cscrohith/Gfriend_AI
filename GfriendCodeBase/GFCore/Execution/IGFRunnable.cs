using System.Collections.Generic;

namespace HP.GFriend.Core.Execution
{
    public interface IGFRunnable
    {
        string OriginalStatement { get; }
        string Result { get; }
        bool IsStatement { get; }
        bool IsAlwaysPass { get; }
        void Run(TestDataManager testDataManager, int repeatCount = -1, int stackLevel = 0, Dictionary<string, string> arguments = null);
        void AddSubBlock(IGFRunnable subBlock);
    }
}

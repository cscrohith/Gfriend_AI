using System.Collections.Generic;

namespace HP.GFriend.Keywords
{
    public interface IGFLibrary
    {
        void Initialize(DeviceUnderTest dut, string outputDir);
        void Dispose();
        string GetName();
        List<string> GetDependencies();
        bool DutUsed();
    }
}

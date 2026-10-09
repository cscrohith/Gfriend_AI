using System.ComponentModel;

namespace HP.GFriend.UI
{
    #region TestType
    public enum TestType
    {
        [Description("Test by test suite")]
        FileList = 0,

        [Description("Test all tcs from text editor")]
        AllTC = 1,

        [Description("Test seltected tcs from text editor")]
        PartialTC = 2

    }
    #endregion    
}

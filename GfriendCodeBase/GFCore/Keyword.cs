using System.Reflection;

namespace HP.GFriend.Core
{
    public class Keyword
    {
        public string KeywordName { get; set;}

        public int NumOfArgs { get; set; }
        
        public string Args { get; set; }
        
        public string FunctionName { get; set; }
        
        public string Description { get; set; }
        
        public MethodInfo Method { get; set; }

        public bool IsGetKeyword { get; set; }

        public bool Deprecated { get; set; }

        public string DeprecatedReason { get; set; }

        public string SampleScript { get; set; }
    }
}

using System;
using System.IO;
using System.Text.RegularExpressions;

namespace HP.GFriend.Keywords.Support
{
    public static class Utils
    {
        private const string _varPattern = @"\$\{[0-9a-zA-Z_-]+\}";
        private static Regex varRegex = new Regex(_varPattern);
        public static string GetAbsolutePath(string path, string scriptRoot)
        {
            if (string.IsNullOrEmpty(Path.GetPathRoot(path)))
            {
                if(string.IsNullOrEmpty(scriptRoot))
                {
                    return path;
                }    
                string strOutputPath = Path.Combine(scriptRoot, path);
                Uri uri = new Uri(strOutputPath);
                return Path.GetFullPath(uri.LocalPath);
            }
            return path;
        }

        public static string GetVariablevalueIfExist(string argument)
        {
            if(!varRegex.IsMatch(argument))
            {
                return argument;
            }
            foreach(Match m in varRegex.Matches(argument))
            {
                string value = CommonExecutionInfo.GetVariable(m.Value);
                if(!string.IsNullOrEmpty(value) || m.Value.Equals("${EMPTY}"))
                {
                    argument = argument.Replace(m.Value, value);
                }
            }
            return argument;
        }
    }
}

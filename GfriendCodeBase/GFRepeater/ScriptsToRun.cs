using HP.GFriend.Client;
using System.Collections.Generic;
using System.IO;

namespace HP.GFriend.Repeater
{
    public class ScriptsToRun
    {
        public Dictionary<int, string> Scripts { get; set; }

        private GFServerConnector _gfServerConnector;
        private string _scriptRoot;
        private readonly List<string> ExecuteSupportedExtension = new List<string>(new string[] { ".txt", ".gfscript" });

        public ScriptsToRun(GFServerConnector gfServerConnector, string scriptRoot, List<int> targetScriptIDs)
        {
            Scripts = new Dictionary<int, string>();
            _gfServerConnector = gfServerConnector;
            _scriptRoot = scriptRoot;
            foreach(int scriptId in targetScriptIDs)
            {
                string savedPath = ((Script)(_gfServerConnector.GetScript(scriptId, _scriptRoot).Data)).AbsolutePath;
                string extension = Path.GetExtension(savedPath);
                if(ExecuteSupportedExtension.Contains(extension))
                {
                    Scripts.Add(scriptId, savedPath);
                }
                
            }
        }
    }
}

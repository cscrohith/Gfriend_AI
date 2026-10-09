using System.IO;
using System.Reflection;

namespace HP.GFriend.Client
{
    class GFClientUtil
    {
        private string _settingFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Settings.ini");
        private readonly string ServerInfo = "Server Info";
        private readonly string ScriptServerAddress = "Repository Server Address";
        private string _scriptServerAddress = "http://130.31.193.61:8080";

        #region Script Server info for INI
        public void SaveScriptServerInfoINI(string scriptServerAddress)
        {
            if (!File.Exists(_settingFile))
            {
                FileStream file = File.Create(_settingFile);
                file.Close();
            }

            IniFile appSettingsINI = new IniFile(_settingFile);

            appSettingsINI.SetValue(ServerInfo, ScriptServerAddress, scriptServerAddress);

            appSettingsINI.Flush();
        }

        public string LoadScriptServerInfoINI()
        {
            if (!File.Exists(_settingFile))
            {
                return null;
            }

            IniFile appSettingsINI = new IniFile(_settingFile);

            return appSettingsINI.GetValue(ServerInfo, ScriptServerAddress, _scriptServerAddress);
        }
        #endregion Script Server info for INI
    }
}

using HP.GFriend.GFLogger;
using HP.GFriend.Updater;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HP.GFriend.UI
{
    public partial class AboutForm : Form
    {

        private GFUpdaterOnAws _gfUpdater;
        private string _serverVersion = null;
        private string _localVersion = null;
        private string _hostUrl = Const.DEFAULT_HOST;
        private MainForm _mainForm;
        public AboutForm(MainForm mainForm)
        {
            InitializeComponent();
            //_gfUpdater = new GFUpdaterOnAws(_hostUrl);
            //_mainForm = mainForm;
        }

        private void AboutForm_Load(object sender, EventArgs e)
        {
            // This fanctionality will be implemented later using SharePoint.
            //_serverVersion = _gfUpdater.GetCurrentVersionOnAws(Const.GFRIEND_BIANRY_NAME)?[0]?.Version ?? string.Empty;
            //_localVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            //labelVerion.Text = "Current Version : "+_localVersion;

            //if (!string.IsNullOrEmpty(_serverVersion) && !_serverVersion.Equals(_localVersion))
            //{
            //    labelServerVersion.Text = "A latest version of GFriend is available. Click Update button to update to the latest version : " + _serverVersion;
            //    buttonUpdate.Visible = true;
            //}
            //else
            //{
            //    labelServerVersion.Text = "Already you are using latest version of GFriend. No updates required.";
            //    buttonUpdate.Visible = false;
            //}
            labelVerion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            labelContact.Text = Properties.Resources.Contact;
            linkLabelHomepage.Text = Properties.Resources.Hompage;
        }

        private void linkLabelHomepage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(linkLabelHomepage.Text);
        }

        //private void buttonUpdate_Click(object sender, EventArgs e)
        //{
        //    string iniPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Settings.ini");
        //    if (File.Exists(iniPath))
        //    {
        //        IniFile settings = new IniFile(iniPath);
        //        _hostUrl = settings.GetValue("Server Info", "Release Server Address", _hostUrl);
        //        settings.SetValue("Server Info", "Release Server Address", _hostUrl);
        //    }

        //    Logger.Trace("GFriend host url : " + _hostUrl);
        //    Logger.Trace("GFriend Current Version : " + _localVersion);
        //    Logger.Trace("GFriend Server Version : " + _serverVersion);

        //    Program.UpdateGFriend();
        //}
    }
}

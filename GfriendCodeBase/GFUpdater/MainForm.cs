using HP.GFriend.GFLogger;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HP.GFriend.Updater
{
    public partial class MainForm : Form
    {
        private GFUpdaterOnAws _gfUpdater;
        private string _gfPath;
        private string _targetVersion;
        private Task _updateTask;
        private int _closeWaitSec = 5;
        private bool _updateAgent = false;


        public MainForm()
        {
            InitializeComponent();
        }

        public MainForm(string gfPath, string hostUrl, bool fromAgent)
        {
            Logger.Trace("In Mainform of GFUpdater gfPath=" + gfPath);

            _gfPath = gfPath;
            _updateAgent = fromAgent;
            InitializeComponent();
            textBoxGFBasePath.Text = _gfPath;
            if(string.IsNullOrEmpty(hostUrl))
            {
                hostUrl = Const.DEFAULT_HOST;
            }
            _gfUpdater = new GFUpdaterOnAws(hostUrl);
            timer.Interval = 50;
        }

        public void UpdateGF()
        {
            int _retry = 0;
            string serverVersion = _gfUpdater.GetCurrentVersionOnAws(Const.GFRIEND_BIANRY_NAME)?[0]?.Version ?? string.Empty;
            labelStatus.Text = "Checking version";
            GetReleaseInfo();
            labelStatus.Text = "Download GFriend from server";

            while (_retry < 2) {
                string downloadUrl = _gfUpdater.GetGFBinaryDownloadUrlOnAws(Const.GFRIEND_BIANRY_NAME, _targetVersion);
                string downloadedPath = _gfUpdater.GetGFBinaryOnAws(downloadUrl, Const.GFRIEND_BIANRY_NAME);

                GFLogger.Logger.Trace("Downloaded Path : " + downloadedPath);

                // Check if downloaded file is exist
                string fileToCheck = Path.Combine(downloadedPath, "GFriend2.exe");
                GFLogger.Logger.Trace("UpdateGF->filetocheck=" + fileToCheck);
                if (!File.Exists(fileToCheck))
                {
                    downloadUrl = _gfUpdater.GetGFBinaryDownloadUrlOnAws(Const.GFRIEND_BIANRY_NAME, _targetVersion);
                    downloadedPath = _gfUpdater.GetGFBinaryOnAws(downloadUrl, Const.GFRIEND_BIANRY_NAME);
                }
                labelStatus.Text = "Copying new version of GFriend";
                _gfUpdater.CopyBinary(downloadedPath, _gfPath);

                // Check if version is changed
                Assembly asm = Assembly.LoadFrom(Path.Combine(_gfPath +"\\" +"GFriend2.exe"));
                GFLogger.Logger.Trace(_gfPath + "\\" + "GFriend2.exe");
                string currentVersion = asm.GetName().Version.ToString();
                if (serverVersion.Equals(currentVersion))
                {
                    labelStatus.Text = "Update Done";
                    GFLogger.Logger.Trace("Updated Done to Version:"+serverVersion);
                    return;
                }
                else if (_retry == 1 && !serverVersion.Equals(currentVersion))
                {
                    DialogResult result = MessageBox.Show("GFriend update is currently unavailable. Please try again later", "GFriend Update Alert", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    if (result == DialogResult.OK)
                    {
                        labelStatus.Text = "Update Failed";
                        labelClose.Text = "Update Failed";
                        return;
                    }
                }
                else _retry++;
            }
        }

        public void CloseAndRunGF()
        {
            Process p = new Process();
            ProcessStartInfo info;
            if(_updateAgent)
            {
                info = new ProcessStartInfo(Path.Combine(_gfPath, "GFRemoteAgent.exe"));
                info.Verb = "runas";
            }
            else
            {
                info = new ProcessStartInfo(Path.Combine(_gfPath, "GFriend2.exe"));
            }
            
            p.StartInfo = info;
            p.Start();
            Environment.Exit(0);
        }

        public void GetReleaseInfo()
        {
            GFTools newGF = _gfUpdater.GetCurrentVersionOnAws(Const.GFRIEND_BIANRY_NAME).FirstOrDefault();
            _targetVersion = newGF.Version;
            releseNoteRichTextBox.Text = $"Version {_targetVersion}{Environment.NewLine}{newGF.Description}";
        }


        private void MainForm_Shown(object sender, EventArgs e)
        {
            timer.Start();
            _updateTask = Task.Run(() => { UpdateGF(); });
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            int value = progressBarUpdate.Value + 5;
            
            if(value > 100)
            {
                progressBarUpdate.Value = 0;
            }
            else
            {
                progressBarUpdate.Value = value;
            }
            if(_updateTask?.IsCompleted ?? false)
            {
                progressBarUpdate.Value = 100;
                timer.Stop();
                timerClose.Interval = 1000;
                timerClose.Start();
            }
            
        }

        private void TimerClose_Tick(object sender, EventArgs e)
        {
            panelDone.Visible = true;
            buttonClose.Text = $"Close Updater and Launch GFriend ({_closeWaitSec--})";
            
            if(_closeWaitSec == 0)
            {
                CloseAndRunGF();
            }
        }

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            CloseAndRunGF();
        }
    }
}

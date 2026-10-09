using HP.GFriend.Utils.Web;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace HP.GFriend.Tool
{
    public partial class WebDriverInstallControl : UserControl
    {
        private DriverInfo _driverInfo;
        public WebDriverInstallControl()
        {
            InitializeComponent();
        }

        public WebDriverInstallControl(DriverInfo info)
        {
            InitializeComponent();
            _driverInfo = info;
            Refresh();
            
        }

        public override void Refresh()
        {
            textBoxBrowserName.Text = _driverInfo.Browser.ToString();
            textBoxBrowserVersion.Text = _driverInfo.BrowserVersion;
            textBoxDriverVersion.Text = _driverInfo.DriverVersion;
            labelNote.Text = _driverInfo.Notes;
            switch (_driverInfo.DriverInstalledStatus)
            {
                case DriverStatus.Installed:
                    textBoxDriverVersion.BackColor = Color.FromArgb(192, 255, 192);
                    break;
                case DriverStatus.NotInstalled:
                    textBoxDriverVersion.BackColor = Color.FromArgb(255, 128, 255);
                    break;
                case DriverStatus.UpdateNeeded:
                    textBoxDriverVersion.BackColor = Color.FromArgb(0, 192, 192);
                    break;
                case DriverStatus.NotApplicable:
                    textBoxDriverVersion.BackColor = Color.FromArgb(255, 255, 128);
                    break;
                case DriverStatus.UnKnown:
                    textBoxDriverVersion.BackColor = Color.Gray;
                    break;
            }
            if(!_driverInfo.IsBrowserCompatible)
            {
                textBoxBrowserVersion.BackColor = Color.FromArgb(255, 255, 128);
            }
            if (string.IsNullOrEmpty(_driverInfo.DriverDownloadAddresses))
            {
                buttonDownload.Enabled = false;
            }

            base.Refresh();
        }

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            WebDriverHelper.GetWebDriver(_driverInfo);
            Refresh();
            Cursor = Cursors.Default;
        }
    }
}

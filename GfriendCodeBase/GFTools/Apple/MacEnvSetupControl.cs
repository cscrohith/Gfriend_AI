using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HP.GFriend.Utils.Appium;

namespace HP.GFriend.Tool
{
    
    public partial class MacEnvSetupControl : UserControl
    {
        private PackageInfo _packageInfo;
        public MacEnvSetupControl(PackageInfo info)
        {
            InitializeComponent();
            _packageInfo = info;
            Refresh();
        }

        public override void Refresh()
        {
            textBoxPackageName.Text = _packageInfo.Package.ToString();
            textBoxPackageVersion.Text = _packageInfo.PackageVersion;
            
            switch (_packageInfo.PackageInstalledStatus)
            {
                case PackageStatus.Installed:
                    textBoxPackageVersion.BackColor = Color.FromArgb(192, 255, 192);
                    buttonInstall.Enabled = false;
                    break;
                case PackageStatus.NotInstalled:
                    textBoxPackageVersion.BackColor = Color.FromArgb(255, 128, 255);
                    buttonInstall.Enabled = true;
                    break;
                case PackageStatus.UnKnown:
                    textBoxPackageVersion.BackColor = Color.Gray;
                    buttonInstall.Enabled = true;
                    break;
            }
            base.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            AppiumServerUtils.InstallPackage(_packageInfo);
            Refresh();
            Cursor = Cursors.Default;
        }
    }
}

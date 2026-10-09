using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading.Tasks;
using HP.GFriend.Utils.Appium;
using System.Text;

namespace HP.GFriend.Tool
{
    public partial class MainForm : Form
    {
        private bool _isConnected;
        private int linespacing;

        public MainForm()
        {
            InitializeComponent();
            Console.SetOut(new ControlWriter(textBoxOutput));
            this.Icon = Properties.Resources.GFriend_Icon;
            _isConnected = false;
            linespacing = 5;
        }

        public class ControlWriter : TextWriter
        {
            private TextBox textbox;
            public ControlWriter(TextBox textbox)
            {
                this.textbox = textbox;
            }

            public override void Write(string value)
            {
                WriteImp(value);
            }

            public override void WriteLine(string value)
            {
                WriteImp(value + Environment.NewLine);
            }

            private void WriteImp(string value)
            {
                if (textbox.InvokeRequired)
                    textbox.Invoke(new MethodInvoker(delegate ()
                    {
                        textbox.AppendText(value);
                    }));
                else
                    textbox.AppendText(value);
            }


            public override Encoding Encoding
            {
                get { return Encoding.ASCII; }
            }
        }

        private void buttonConnect_Click(object sender, System.EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxIp.Text) || string.IsNullOrEmpty(textBoxId.Text) ||
                        string.IsNullOrEmpty(textBoxPw.Text))
            {
                MessageBox.Show("Mac address, user id, password should be given.");
                return;
            }

            if (_isConnected)
            {
                if (flowLayoutPanelPackages.Controls.Count != 0)
                {
                    Height -= (flowLayoutPanelPackages.Controls[0].Height + linespacing) * flowLayoutPanelPackages.Controls.Count;
                    flowLayoutPanelPackages.Controls.Clear();
                    flowLayoutPanelPackages.Height = 10;
                }
            }
            
            AppiumServerUtils.Initialize(textBoxIp.Text, 22, textBoxId.Text, textBoxPw.Text);
            string hostname = AppiumServerUtils.CheckPrompt();
            List<PackageInfo> packageInfos = new List<PackageInfo>();

            Task t1 = new Task(() =>
            {
                packageInfos = AppiumServerUtils.GetCurrentStatus();
            });
            Cursor = Cursors.WaitCursor;
            t1.Start();
            t1.Wait();
            Cursor = Cursors.Default;

            foreach (PackageInfo d in packageInfos)
            {
                MacEnvSetupControl wic = new MacEnvSetupControl(d);
                flowLayoutPanelPackages.Controls.Add(wic);
                flowLayoutPanelPackages.Height += wic.Height;
                Height += wic.Height + linespacing;
            }
            _isConnected = true;
            buttonConnect.Text = "Refresh";
            Console.WriteLine($"Connedted to {hostname}");
        }

        private void buttonInstallWDA_Click(object sender, System.EventArgs e)
        {
            if(_isConnected)
            {
                Cursor = Cursors.WaitCursor;
                if (string.IsNullOrEmpty(textBoxDeveloperID.Text))
                {
                    AppiumServerUtils.UninstallWDA();
                    AppiumServerUtils.InstallWDA();
                }
                else
                {
                    string onlyId = textBoxDeveloperID.Text.Split('@')[0];
                    AppiumServerUtils.InstallWDA(onlyId);
                }

                if (flowLayoutPanelPackages.Controls.Count != 0)
                {
                    Height -= (flowLayoutPanelPackages.Controls[0].Height + linespacing) * flowLayoutPanelPackages.Controls.Count;
                    flowLayoutPanelPackages.Controls.Clear();
                    flowLayoutPanelPackages.Height = 10;
                }
                foreach (PackageInfo d in AppiumServerUtils.GetCurrentStatus())
                {
                    MacEnvSetupControl wic = new MacEnvSetupControl(d);
                    flowLayoutPanelPackages.Controls.Add(wic);
                    flowLayoutPanelPackages.Height += wic.Height;
                    Height += wic.Height + linespacing;
                }
                Cursor = Cursors.Default;
            }    
            else
            {
                MessageBox.Show("Is not connected.");
            }

        }

        private void checkBoxPersonal_CheckedChanged(object sender, System.EventArgs e)
        {
            if(checkBoxPersonal.Checked)
            {
                labelDeveloperID.Visible = true;
                textBoxDeveloperID.Visible = true;
            }
            else
            {
                labelDeveloperID.Visible = false;
                textBoxDeveloperID.Visible = false;
                textBoxDeveloperID.Text = string.Empty;
            }
        }
    }
}

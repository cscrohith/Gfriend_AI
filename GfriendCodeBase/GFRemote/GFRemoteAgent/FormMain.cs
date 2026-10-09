using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HP.GFriend.GFWcfService;

namespace HP.GFriend.RemoteAgent
{
    public partial class FormMain : LockNotificationForm
    {
        private ServiceHost _serviceHost = null;
        private bool _isExit = false;
        public FormMain()
        {
            InitializeComponent();
            notifyIconTrayIcon.ContextMenu = contextMenuTray;
        }

        
        private void FormMain_Load(object sender, EventArgs e)
        {
            Start(8000);
        }

        private void buttonRestart_Click(object sender, EventArgs e)
        {
            try
            {
                if (_serviceHost != null)
                {
                    _serviceHost.Close();
                }
            }
            catch (Exception)
            { }
            if(!int.TryParse(textBoxPort.Text, out int port))
            {
                MessageBox.Show("Port should be number.", "Error");
                return;
            }
            Start(port);
        }

        private void Start(int port)
        {
            Uri httpUrl = new Uri($"http://127.0.0.1:{port}/GFRemoteService.svc");
            WSHttpBinding binding = new WSHttpBinding(SecurityMode.None);
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            binding.Security.Message.ClientCredentialType = MessageCredentialType.None;
            binding.Security.Message.NegotiateServiceCredential = false;
            binding.Security.Message.EstablishSecurityContext = false;


            _serviceHost = new ServiceHost(typeof(GFRemoteService), httpUrl);
            _serviceHost.AddServiceEndpoint(typeof(IGFRemoteService), binding, "");
            ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
            smb.HttpGetEnabled = true;
            _serviceHost.Description.Behaviors.Add(smb);
            _serviceHost.Open();

            labelStatus.Text = "Running";
            panelStatus.BackColor = Color.DarkGreen;
        }

        public void ShowWindow()
        {
            WinApi.ShowToFront(this.Handle);
        }
        protected override void WndProc(ref Message message)
        {
            if (message.Msg == SingleInstance.WM_SHOWFIRSTINSTANCE)
            {
                ShowWindow();
            }
            base.WndProc(ref message);
        }

        private void notifyIconTrayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
        }

        private void menuItem1_Click(object sender, EventArgs e)
        {
            Close();
            _isExit = true;
            Application.Exit();
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(!_isExit)
            {
                Hide();
                e.Cancel = true;
            }
            else
            {
                notifyIconTrayIcon.Dispose();
            }
        }
    }
}

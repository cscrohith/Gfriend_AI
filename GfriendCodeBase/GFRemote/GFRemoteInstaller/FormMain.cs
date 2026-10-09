using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Threading;
using System.IO;

namespace HP.GFriend.GFRemoteInstaller
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            AddAvaliableIpAddress();
            CheckInstallStatus();
        }

        private void AddAvaliableIpAddress()
        {
            var IpAddresses = Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(ip => ip.AddressFamily.Equals(AddressFamily.InterNetwork));

            // Loop through all IP addresses and display each 

            foreach (IPAddress ip in IpAddresses)
            {
                
                comboBoxIPAddress.Items.Add(ip.ToString());

            }
            comboBoxIPAddress.SelectedIndex = 0;
        }

        private void CheckInstallStatus()
        {
            textBoxStatus.Text = GFRemoteService.GetServiceStatus();
            if(textBoxStatus.Text.Equals("Not Installed"))
            {
                textBoxStatus.BackColor = Color.Red;
            }
            else
            {
                textBoxStatus.BackColor = Color.Green;
            }
        }

        private void buttonInstall_Click(object sender, EventArgs e)
        {
            using (StreamWriter writer = new StreamWriter("settings", false))
            {
                writer.Write($"http://{comboBoxIPAddress.Text}:{textBoxPort.Text}");
                writer.Flush();
                writer.Close();
            }

            GFRemoteService.StopService();
            GFRemoteService.CopyBinary();
            GFRemoteService.AddService();
            CheckInstallStatus();
            Thread.Sleep(2000);
            CheckInstallStatus();
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            GFRemoteService.StopService();
            CheckInstallStatus();
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            GFRemoteService.DeleteService();
            CheckInstallStatus();
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            CheckInstallStatus();
        }
    }
}

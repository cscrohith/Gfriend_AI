using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HP.GFriend.Utils.Git
{
    public partial class RepoControl : UserControl
    {
        private string _gitConfigFile;
        private GitConfig _gitConfig;
        public event EventHandler<RepoEventArgs> Changed;
        public RepoControl()
        {
            InitializeComponent();
            _gitConfigFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "gfGitConfig.xml");
        }

        private void RepoControl_Load(object sender, EventArgs e)
        {
            if(File.Exists(_gitConfigFile))
            {
                _gitConfig = GitConfig.Load(_gitConfigFile);
                textBoxUserName.Text = _gitConfig.UserName;
                textBoxEmail.Text = _gitConfig.UserEmail;
                textBoxToken.Text = _gitConfig.HttpToken;
            }
            else
            {
                _gitConfig = new GitConfig();
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(textBoxUserName.Text) ||
                string.IsNullOrEmpty(textBoxEmail.Text) ||
                string.IsNullOrEmpty(textBoxToken.Text))
            {
                MessageBox.Show("Please enter change user name, email and http token.", "Caution", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _gitConfig.UserName = textBoxUserName.Text;
            _gitConfig.UserEmail = textBoxEmail.Text;
            _gitConfig.HttpToken = textBoxToken.Text;
            _gitConfig.Save(_gitConfigFile);
            Changed?.Invoke(this, new RepoEventArgs(_gitConfig, null));
        }

        private void pictureBoxClose_Click(object sender, EventArgs e)
        {
            SendToBack();
            Visible = false;
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.SelectedPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrEmpty(fbd.SelectedPath))
                {
                    textBoxLocalPath.Text = fbd.SelectedPath;
                }
            }
        }

        private void buttonClone_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxLocalPath.Text) ||
                string.IsNullOrEmpty(textBoxURL.Text))
            {
                MessageBox.Show("Please enter fill out local path and url.", "Caution", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(textBoxUserName.Text) ||
                string.IsNullOrEmpty(textBoxEmail.Text) ||
                string.IsNullOrEmpty(textBoxToken.Text))
            {
                MessageBox.Show("Please enter change user name, email and http token.", "Caution", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            GitHelper.Clone(textBoxURL.Text, textBoxLocalPath.Text, _gitConfig.UserName, _gitConfig.UserEmail, _gitConfig.HttpToken);
            Changed?.Invoke(this, new RepoEventArgs(_gitConfig, textBoxLocalPath.Text));
        }

        public class RepoEventArgs : EventArgs
        {
            public string UserName { get; set; }
            public string UserEmail { get; set; }
            public string HttpToken { get; set; }
            public string RepoPath { get; set; }

            public RepoEventArgs(GitConfig config, string repoPath)
            {
                UserName = config.UserName;
                UserEmail = config.UserEmail;
                HttpToken = config.HttpToken;
                RepoPath = repoPath;
            }
        }
    }
}

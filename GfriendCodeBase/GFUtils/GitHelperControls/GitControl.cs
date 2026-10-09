using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace HP.GFriend.Utils.Git
{
    public partial class GitControl : UserControl
    {
        public event EventHandler<GitEvent> Changed;

        private const string DEFAULT_MESSAGE = "Enter change description";
        private GitHelper _git;
        private RepoControl _repoControl;
        private string _userName;
        private string _userEmail;
        private string _httpToken;
        private string _gitConfigPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "gfGitConfig.xml");

        public GitControl()
        {
            InitializeComponent();
            textBoxCommitMessage.Text = DEFAULT_MESSAGE;
            _repoControl = new RepoControl();
            _repoControl.Location = new System.Drawing.Point(0, 0);
            _repoControl.Visible = false;
            _repoControl.SendToBack();
            this.Controls.Add(this._repoControl);
            _repoControl.Changed += repoControl_Changed;
            SetControlEnable(false);
            
            string gitdllPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "git2-106a5f2.dll");
            if (!File.Exists(gitdllPath))
            {
                using (BinaryWriter writer = new BinaryWriter(new FileStream(gitdllPath, FileMode.Create)))
                {
                    writer.Write(Properties.Resources.git2_106a5f2);
                }
            }

            if(File.Exists(_gitConfigPath))
            {
                GitConfig config = GitConfig.Load(_gitConfigPath);
                _userName = config.UserName;
                _userEmail = config.UserEmail;
                _httpToken = config.HttpToken;
            }
            
        }

        private void repoControl_Changed(object sender, RepoControl.RepoEventArgs e)
        {
            if(!string.IsNullOrEmpty(e.RepoPath))
            {
                OpenLocalGitRepository(e.RepoPath, e.UserName, e.UserEmail, e.HttpToken);
                Changed?.Invoke(this, new GitEvent($"Repository cloned to {e.RepoPath}",e.RepoPath));
            }
            else 
            {
                _git.ChangeConfig(e.UserName, e.UserEmail, e.HttpToken);
            }
        }

        public void OpenLocalGitRepository(string repoPath, string userName = null, string userEmail = null, string httpToken = null)
        {
            
            if(GitHelper.IsGitRepo(repoPath))
            {
                if (string.IsNullOrEmpty(userName)) userName = _userName;
                if (string.IsNullOrEmpty(userEmail)) userEmail = _userEmail;
                if (string.IsNullOrEmpty(httpToken)) httpToken = _httpToken;

                _git = new GitHelper(repoPath, userName, userEmail, httpToken);
                SetControlEnable(true);
                labelBranchName.Text = _git.Head;
                Changed?.Invoke(this, new GitEvent($"Git repositoy is opend : {repoPath}"));
            }
            else
            {
                SetControlEnable(false);
            }
            
            Reload();
        }

        public void SetControlEnable(bool enable)
        {
            textBoxCommitMessage.Enabled = enable;
            buttonCommitPush.Enabled = enable;
            checkedListBoxFileChanged.Enabled = enable;
            labelBranchName.Enabled = enable;
            pictureBoxPull.Enabled = enable;
            pictureBoxRefresh.Enabled = enable;
        }

        public void Reload()
        {
            if (_git == null) return;
            checkedListBoxFileChanged.Items.Clear();
            if(_git.ChangeExists())
            {
                foreach(Change c in _git.GetChanges())
                {
                 
                    checkedListBoxFileChanged.Items.Add(c);
                }
            }
            textBoxCommitMessage.Text = DEFAULT_MESSAGE;
            labelBranchName.Text = _git.Head;
        }
        

        private void textBoxCommitMessage_Click(object sender, EventArgs e)
        {
            if(textBoxCommitMessage.Text.Equals(DEFAULT_MESSAGE))
            {
                textBoxCommitMessage.Text = string.Empty;
            }
        }

        private void buttonSelectAll_Click(object sender, EventArgs e)
        {
            for(int i=0; i<checkedListBoxFileChanged.Items.Count; i++)
            {
                checkedListBoxFileChanged.SetItemChecked(i, true);
            }
        }

        private void buttonCommitPush_Click(object sender, EventArgs e)
        {
            if(textBoxCommitMessage.Text.Equals(DEFAULT_MESSAGE) || string.IsNullOrEmpty(textBoxCommitMessage.Text))
            {
                MessageBox.Show("Please enter change description.", "Caution", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if(checkedListBoxFileChanged.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select file to upload.", "Caution", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _git.Stage(checkedListBoxFileChanged.CheckedItems.Cast<Change>().ToList());
            _git.Commit(textBoxCommitMessage.Text);
            _git.Push();
            Reload();
            Changed?.Invoke(this, new GitEvent("All changes are uploaded."));
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Reload();
        }

        private void labelBranchName_Click(object sender, EventArgs e)
        {
            BranchControl branchControl = new BranchControl(_git);
            branchControl.Location = labelBranch.Location;
            branchControl.BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(branchControl);
            branchControl.Show();
            branchControl.BringToFront();
            branchControl.Disposed += BranchControl_Disposed;
        }

        private void BranchControl_Disposed(object sender, EventArgs e)
        {
            Reload();
        }

        private void GitControl_Enter(object sender, EventArgs e)
        {
            if(_git != null)
            {
                Reload();
            }
            
        }

        private void pictureBoxPull_Click(object sender, EventArgs e)
        {
            try
            {
                _git.Pull();
                Reload();
                Changed?.Invoke(this, new GitEvent("Sync with Github."));
            }
            catch(Exception)
            {
                MessageBox.Show("Current branch is not exist in the Github");
            }
        }

        private void Icon_MouseOver(object sender, EventArgs e)
        {
            string tooltipText = string.Empty;
            if(sender.Equals(pictureBoxPull))
            {
                tooltipText = "Pull changes from Github";
            }
            else if(sender.Equals(pictureBoxRefresh))
            {
                tooltipText = "Refresh local changes";
            }
            else if(sender.Equals(pictureBoxSelectAll))
            {
                tooltipText = "Select all";
            }
            else if(sender.Equals(pictureBoxSetting))
            {
                tooltipText = "Settings";
            }

            ToolTip toolTip = new ToolTip();
            System.Drawing.Point location = ((Control)sender).Location;
            location.Y = location.Y + ((Control)sender).Height;
            toolTip.Show(tooltipText, this, location, 2000);
        }

        private void pictureBoxSetting_Click(object sender, EventArgs e)
        {
            _repoControl.Dock = DockStyle.Fill;
            _repoControl.BringToFront();
            _repoControl.Show();
        }
    }
}

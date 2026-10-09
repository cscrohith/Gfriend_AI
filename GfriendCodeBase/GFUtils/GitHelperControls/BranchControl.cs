using System;
using System.Windows.Forms;

namespace HP.GFriend.Utils.Git
{
    public partial class BranchControl : UserControl
    {
        private GitHelper _git;
        private TreeNode localBranch = new TreeNode("Local");
        private TreeNode remoteBranch = new TreeNode("Remote");
        public BranchControl()
        {
            InitializeComponent();
        }

        public BranchControl(GitHelper git) : this()
        {
            _git = git;


            foreach(string branch in _git.GetLocalBranches())
            {
                localBranch.Nodes.Add(branch);
            }

            foreach(string branch in _git.GetRemoteBranches())
            {
                remoteBranch.Nodes.Add(branch);
            }
            treeViewBranches.Nodes.Add(localBranch);
            treeViewBranches.Nodes.Add(remoteBranch);
            treeViewBranches.ExpandAll();
        }

        private void treeViewBranches_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Equals(localBranch) || e.Node.Equals(remoteBranch)) return;
            textBoxBranch.Text = e.Node.Text;
        }

        private void Checkout_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(textBoxBranch.Text))
            {
                _git.Checkout(textBoxBranch.Text);
                Dispose();
            }
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            Dispose();
        }
    }
}

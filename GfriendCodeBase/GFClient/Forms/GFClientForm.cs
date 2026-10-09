using System;
using System.Drawing;
using System.Windows.Forms;

namespace HP.GFriend.Client.Forms
{
    public partial class GFClientForm : Form
    {
        private GFServerConnector _gfServerConnector = null;        
        private GFClientUtil _gFClientUtil = new GFClientUtil();
        private string _gFServerAddr = null;

        public GFClientForm(Icon icon)
        {
            Icon = icon;
            InitializeComponent();
            

        }

        public GFClientForm(Icon icon, string localScriptPath)
        {
            Icon = icon;
            InitializeComponent();
            _gFServerAddr = _gFClientUtil.LoadScriptServerInfoINI();

            _gfServerConnector = new GFServerConnector(_gFServerAddr);
            textBoxServerAddr.Text = _gFServerAddr;
            folderViewerGF.GFServer = _gfServerConnector;
            folderViewerGF.LocalScriptDirectory = localScriptPath;
            try
            {
                folderViewerGF.Refresh();
            }
            catch (Exception)
            {
                throw new GFServerException("Server Connection Error");
            }


        }


        private void ButtonConnect_Click(object sender, EventArgs e)
        {
            
            if(!string.IsNullOrEmpty(textBoxServerAddr.Text))
            {
                _gfServerConnector = new GFServerConnector(textBoxServerAddr.Text);
                folderViewerGF.GFServer = _gfServerConnector;
                GFServerResult serverResult = folderViewerGF.RefreshRemote();

                if (serverResult.Status == RequestStatus.Success)
                {
                    _gFClientUtil.SaveScriptServerInfoINI(textBoxServerAddr.Text);
                }
            }
        }


        private void PictureBoxOpen_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.ShowDialog();

            if (!string.IsNullOrEmpty(folderBrowserDialog.SelectedPath))
            {
                folderViewerGF.LocalScriptDirectory = folderBrowserDialog.SelectedPath;
                folderViewerGF.RefreshLocal();
            }
        }
    }
}

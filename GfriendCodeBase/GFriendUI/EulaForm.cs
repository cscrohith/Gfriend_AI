using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace HP.GFriend.UI
{
    public partial class EulaForm : Form
    {
        private const int _eulaVersion = 1;
        private string _eulaFileLocation;

        public EulaForm()
        {
            InitializeComponent();
            DialogResult = DialogResult.Cancel;
            richTextBoxEulaContent.Rtf = Properties.Resources.Eula;
            _eulaFileLocation = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"Eula_v{_eulaVersion}.accepted");
        }

        public static bool IsEulaAccepted()
        {
            string fileToCheck = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"Eula_v{_eulaVersion}.accepted");
            if (File.Exists(fileToCheck))
            {
                return true;
            }
            return false;
        }

        public void AcceptEula()
        {
            using (StreamWriter writer = new StreamWriter(_eulaFileLocation))
            {
                writer.WriteLine($"Date Time : {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                writer.WriteLine($"Login Credential : {System.Security.Principal.WindowsIdentity.GetCurrent().Name}");
            }
        }


        private void checkBoxAccept_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBoxAccept.Checked)
            {
                buttonContinue.Enabled = true;
            }
            else
            {
                buttonContinue.Enabled = false;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonContinue_Click(object sender, EventArgs e)
        {
            AcceptEula();
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}

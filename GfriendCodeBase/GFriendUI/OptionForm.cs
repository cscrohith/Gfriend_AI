
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace HP.GFriend.UI
{
    public partial class OptionForm : Form
    {
        private const string DEFAULT_OVERALL_DESCRIPTION = "<Description></Description>\r\n<Precondition></Precondition>";
        private const string DEFAULT_SCRIPT_DESCRIPTION = "<Description></Description>";
        private const string DEFAULT_LIBRARY_DESCRIPTION = "<Description></Description>\r\n<Parameters></Parameters>";
        public static readonly string DESCRIPTION_SECTION_NAME = "CommentStyle";
        public static readonly string DESCRIPTION_OVERALL_KEY = "Overall";
        public static readonly string DESCRIPTION_SCRIPT_KEY = "Script";
        public static readonly string DESCRIPTION_LIBRARY_KEY = "Library";

        private IniFile _iniFile;

        public OptionForm()
        {
            _iniFile = new IniFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Settings.ini"));
            InitializeComponent();
            LoadOptions();
            
        }
        
        internal OptionForm(IniFile iniFile)
        {
            _iniFile = iniFile;
            InitializeComponent();
            LoadOptions();
        }

        private void LoadOptions()
        {
            string tempStr = _iniFile.GetValue(DESCRIPTION_SECTION_NAME, DESCRIPTION_OVERALL_KEY, DEFAULT_OVERALL_DESCRIPTION);
            tempStr = tempStr.Replace(@"\n", Environment.NewLine);
            textBoxDescriptionOverall.Text = tempStr;

            tempStr = _iniFile.GetValue(DESCRIPTION_SECTION_NAME, DESCRIPTION_SCRIPT_KEY, DEFAULT_SCRIPT_DESCRIPTION);
            tempStr = tempStr.Replace(@"\n", Environment.NewLine);
            textBoxDescriptionTestScript.Text = tempStr;

            tempStr = _iniFile.GetValue(DESCRIPTION_SECTION_NAME, DESCRIPTION_LIBRARY_KEY, DEFAULT_LIBRARY_DESCRIPTION);
            tempStr = tempStr.Replace(@"\n", Environment.NewLine);
            textBoxDescriptionLibrary.Text = tempStr;

            
        }

        private void SaveOptions()
        {
            string tempStr = textBoxDescriptionOverall.Text;
            MainForm.OverallDescriptionTemplate = tempStr.Replace(Environment.NewLine, $"{Environment.NewLine}/// ");
            tempStr = tempStr.Replace(Environment.NewLine, @"\n");
            _iniFile.SetValue(DESCRIPTION_SECTION_NAME, DESCRIPTION_OVERALL_KEY, tempStr);

            tempStr = textBoxDescriptionTestScript.Text;
            MainForm.ScriptDescriptonTemplate = tempStr.Replace(Environment.NewLine , $"{Environment.NewLine}/// ");
            tempStr = tempStr.Replace(Environment.NewLine, @"\n");
            _iniFile.SetValue(DESCRIPTION_SECTION_NAME, DESCRIPTION_SCRIPT_KEY, tempStr);
            

            tempStr = textBoxDescriptionLibrary.Text;
            MainForm.LibraryDescriptionTemplate = tempStr.Replace(Environment.NewLine, $"{Environment.NewLine}/// ");
            tempStr = tempStr.Replace(Environment.NewLine, @"\n");
            _iniFile.SetValue(DESCRIPTION_SECTION_NAME, DESCRIPTION_LIBRARY_KEY, tempStr);
            _iniFile.Flush();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            SaveOptions();
            Close();
        }
    }
}

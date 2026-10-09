using HP.GFriend.Core.Execution;
using System;
using System.IO;
using System.Windows.Forms;

namespace HP.GFriend.Client.Forms
{
    [Docking(DockingBehavior.Ask)]
    public partial class ScriptInfo : UserControl
    {
        private Script _script;
        public string LocalScriptRoot { get; set; }
        public ScriptInfo()
        {
            InitializeComponent();
            LocalScriptRoot = string.Empty;
        }


        
        public void SetData(string scriptPath)
        {
            if (!File.Exists(scriptPath))
            {
                throw new ArgumentException("Script file not found");
            }
            _script = new Script()
            {
                Id = 0,
                ScriptName = Path.GetFileNameWithoutExtension(scriptPath),
                Author = Environment.UserName,
                TcCount = Parser.ParseTestSuite(scriptPath).TargetTestSuite.TcCount,
                Description = "",
                AbsolutePath = scriptPath,
                StoredPath = scriptPath.Replace(LocalScriptRoot, "").TrimStart('\\')
            };
            SetData(_script);
        }

        public void SetData(Script script)
        {
            _script = script;
            textBoxId.Text = _script.Id.ToString();
            textBoxScriptName.Text = _script.ScriptName;
            textBoxAuthor.Text = _script.Author;
            textBoxTcCount.Text = _script.TcCount.ToString();
            textBoxDescription.Text = _script.Description;
            textBoxStoredPath.Text = _script.StoredPath;

        }

        public Script Get()
        {
            _script.ScriptName = textBoxScriptName.Text;
            _script.Author= textBoxAuthor.Text;
            _script.TcCount = int.Parse(textBoxTcCount.Text);
            _script.Description = textBoxDescription.Text;
            _script.StoredPath = textBoxStoredPath.Text;
            return _script;
        }
    }
}

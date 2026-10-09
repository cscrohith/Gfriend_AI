using HP.GFriend.Core.Execution.Spec;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace HP.GFriend.UI.RunSpecWizard
{
    internal partial class FinalStepControl : UserControl, IRunSpecWizardControl
    {
        private RunSpecWizardSequence _sequence;
        public FinalStepControl()
        {
            InitializeComponent();
        }

        public UserControl GetUserControl()
        {
            return this;
        }

        public void SetSequence(RunSpecWizardSequence sequence)
        {
            _sequence = sequence;
            XmlSerializer serializer = new XmlSerializer(typeof(TestRuns));
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, _sequence.TestRunSpec);
                fastColoredTextBoxXML.Text = writer.ToString();
            }
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            using(SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.InitialDirectory = _sequence.TestRunSpec.OutputPath ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                dialog.Filter = "GF Test Run Spec|*.xml";
                dialog.DefaultExt = "xml";
                dialog.ShowDialog();

                if(!string.IsNullOrEmpty(dialog.FileName))
                {
                    WriteToFile(dialog.FileName);
                }
                MessageBox.Show($"Test Run Spec file is saved : {dialog.FileName}");
            }
        }

        private void WriteToFile(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(TestRuns));
            if(!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }
            using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                serializer.Serialize(writer, _sequence.TestRunSpec);
                writer.Flush();
                writer.Close();
            }
            return;
        }

        private void ButtonExecute_Click(object sender, EventArgs e)
        {
            string outputPath = _sequence.TestRunSpec.OutputPath ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if(string.IsNullOrEmpty(Path.GetPathRoot(outputPath)))
            {
                string strOutputPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), outputPath);
                Uri uri = new Uri(strOutputPath);
                outputPath = Path.GetFullPath(uri.LocalPath);
            }

            string tempRunSpec = Path.Combine(outputPath, "GFTestRun.xml");
            WriteToFile(tempRunSpec);

            Process process = new Process();
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "cmd.exe";
            info.Arguments = $"/K {Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "GF_Runner.exe")} -s \"{tempRunSpec}\"";
            process.StartInfo = info;
            process.Start();
        }
    }
}

using HP.GFriend.Core.Execution.Spec;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace HP.GFriend.UI.RunSpecWizard
{
    internal partial class TestRunGlobalSettingControl : UserControl, IRunSpecWizardControl
    {
        private RunSpecWizardSequence _sequence;
        public TestRunGlobalSettingControl()
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

            // Tests
            flowLayoutPanelDetail.Controls.Clear();
            foreach (TestRun testRun in _sequence.TestRunSpec.TestsToRun)
            {
                TestToRunControl tControl = new TestToRunControl(testRun);
                tControl.PreventEditingDefaultValue();
                tControl.ShowDeviceSelection();
                tControl.PreventEditingDeviceSelection();

                flowLayoutPanelDetail.Controls.Add(tControl);
            }
            textBoxOutputPath.Text = _sequence.TestRunSpec.OutputPath;
            numericUpDownRepeat.Value = _sequence.TestRunSpec.RepeatCount;
        }

        private void PictureBoxSelectFolder_Click(object sender, EventArgs e)
        {
            using(FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.SelectedPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                dialog.ShowNewFolderButton = true;
                dialog.ShowDialog();

                if(!string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    textBoxOutputPath.Text = dialog.SelectedPath;
                }
            }
        }

        private void TextBoxOutputPath_TextChanged(object sender, EventArgs e)
        {
            _sequence.TestRunSpec.OutputPath = textBoxOutputPath.Text;
        }

        private void NumericUpDownRepeat_ValueChanged(object sender, EventArgs e)
        {
            _sequence.TestRunSpec.RepeatCount = (int)numericUpDownRepeat.Value;
        }
    }
}

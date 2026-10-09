using HP.GFriend.Core.Execution.Spec;
using System;
using System.Windows.Forms;


namespace HP.GFriend.UI.RunSpecWizard
{
    internal partial class SelectScriptControl : UserControl, IRunSpecWizardControl
    {
        private RunSpecWizardSequence _sequence;
        public SelectScriptControl()
        {
            InitializeComponent();
        }

        public UserControl GetUserControl()
        {
            return this;
        }

        public void SetSequence(RunSpecWizardSequence sqeuence)
        {
            _sequence = sqeuence;
            gfScriptFileList.SetScriptPath(_sequence.ScriptsPath, true, true);
        }

        public FlowLayoutPanel GetTCListPanel()
        {
            return flowLayoutPanelDetail;
        }

        private void GfScriptFileList_OnUpdateSelectedFile(object sender, TreeViewEventArgs e)
        {
            TestRun testRun = new TestRun();
            int separateIndex = e.Node.FullPath.IndexOf(@"\");
            testRun.TestSuitePath =_sequence.ScriptsPath + e.Node.FullPath.Substring(separateIndex);
            TestToRunControl testToRunControl = new TestToRunControl(testRun);
            testToRunControl.CloseClick += TestToRunControl_CloseClick;
            flowLayoutPanelDetail.Controls.Add(testToRunControl);
            _sequence.TestRunSpec.TestsToRun.Add(testRun);
        }

        private void TestToRunControl_CloseClick(object sender, EventArgs e)
        {
            flowLayoutPanelDetail.Controls.Remove((TestToRunControl)sender);
            _sequence.TestRunSpec.TestsToRun.Remove(((TestToRunControl)sender).TargetTest);
        }
    }
}

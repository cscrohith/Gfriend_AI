using System.Windows.Forms;

namespace HP.GFriend.UI.RunSpecWizard
{
    internal interface IRunSpecWizardControl
    {
        void SetSequence(RunSpecWizardSequence sequence);
        UserControl GetUserControl();
    }
}

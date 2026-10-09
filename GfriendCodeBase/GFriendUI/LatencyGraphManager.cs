using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HP.GFriend.UI
{
    public static class LatencyGraphManager
    {
        private static LatencyGraphForm _form;

        public static void ShowGraph(string csvFilePath, bool isTestCaseRunning)
        {
            if (_form == null || _form.IsDisposed)
            {
                _form = new LatencyGraphForm(csvFilePath, isTestCaseRunning);
                _form.Show();
            }
            else
            {
                _form.BringToFront();
            }
        }
    }
}

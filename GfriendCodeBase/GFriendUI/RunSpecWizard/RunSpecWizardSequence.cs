using HP.GFriend.Core.Execution.Spec;
using HP.GFriend.Keywords;
using System.Collections.Generic;
using System.Windows.Forms;

namespace HP.GFriend.UI.RunSpecWizard
{
    internal class RunSpecWizardSequence
    {
        public string ScriptsPath { get; set; }
        public List<DeviceUnderTest> DeviceList { get; set; }
        public List<DeviceUnderTest> DeviceUnderTests { get; set; }

        public IRunSpecWizardControl CurrentControl { get; private set; }
        public int CurrentStep = 1;
        public string Title = string.Empty;
        public bool HaveNext = true;
        public bool HavePrevious = false;

        public TestRuns TestRunSpec { get; set; }

        private int _maxStep = 4;
        private SelectScriptControl step1Control;
        private SelectDeviceControl step2Control;
        private TestRunGlobalSettingControl step3Control;
        private FinalStepControl step4Control;

        public RunSpecWizardSequence(string scriptPath, List<DeviceUnderTest> deviceList)
        {
            ScriptsPath = scriptPath;
            DeviceList = deviceList;
            TestRunSpec = new TestRuns();
            DeviceUnderTests = new List<DeviceUnderTest>();
            CurrentStep = 1;
            SetCurrentControl();
        }
        public UserControl GetCurrentControl()
        {
            return CurrentControl.GetUserControl();
        }
        public void SetCurrentControl()
        {
            switch(CurrentStep)
            {
                case 1:
                    Title = "Step 1: Select test script(s) to run";
                    if (step1Control == null)
                    {
                        step1Control = new SelectScriptControl();
                    }
                    CurrentControl = step1Control;
                    break;
                case 2:
                    Title = "Step 2: Select Device to test";
                    if(step2Control == null)
                    {
                        step2Control = new SelectDeviceControl();
                    }
                    CurrentControl = step2Control;
                    break;
                case 3:
                    Title = "Step 3: Configurate global test run settings";
                    if(step3Control == null)
                    {
                        step3Control = new TestRunGlobalSettingControl();
                    }
                    CurrentControl = step3Control;
                    break;
                case 4:
                    Title = "Save Test Run Spec or Execute now";
                    if(step4Control == null)
                    {
                        step4Control = new FinalStepControl();
                    }
                    CurrentControl = step4Control;
                    break;
                    

            }

            CurrentControl.SetSequence(this);
            
        }

        public void Next()
        {
            CurrentStep++;
            if(CurrentStep > 1)
            {
                HavePrevious = true;
            }
            if(CurrentStep >= _maxStep)
            {
                HaveNext = false;
            }
        }

        public void Previous()
        {
            if(CurrentStep >1)
            {
                CurrentStep--;
            }
            else
            {
                HavePrevious = false;
            }
            if(CurrentStep < _maxStep)
            {
                HaveNext = true;
            }
        }
    }
}

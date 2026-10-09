using System;
using HP.DeviceAutomation;
using HP.DeviceAutomation.Jedi;

namespace HP.GFriend.Keywords
{
    internal class DeviceControl
    {
        private const string _resetButton = "#hpid-button-reset";
        private const string _notificationPanel = "#hpid-notification-panel-notification";
        private const string _homeScreenLogo = "#hpid-homescreen-logo-icon";
        private const string _topViewAttribute = "[hp-global-top-view=true]";
        /// <summary>
        /// Ensures the device is awake and ready for interaction.
        /// </summary>
        private void WakeDevice(JediOmniDevice _device)
        {
            if (_device == null)
            {
                throw new ArgumentNullException(nameof(_device), "Device cannot be null in WakeDevice.");
            }
            try
            {
                // Introduced in Jedi version 24.5
                _device.ControlPanel.SignalUserActivity();
            }
            catch (DeviceInvalidOperationException)
            {
                _device.PowerManagement.Wake();
            }
            catch (DeviceCommunicationException)
            {
                // OXPd service temporarily unavailable (e.g. 503 Server Unavailable); fall back to power management wake
                _device.PowerManagement.Wake();
            }
        }
        /// <summary>
        /// Resets the device, ensuring it is at the home screen and no user is signed in.
        /// </summary>
        public void Reset(JediOmniDevice _device)
        {
            // Return to the home screen and sign out so that the Reset button is available
            WakeDevice(_device);
            
            if (_device.ControlPanel.WaitForState(_resetButton, OmniElementState.Useable, TimeSpan.FromSeconds(5)))
            {
                _device.ControlPanel.Press(_resetButton);
                _device.ControlPanel.WaitForState(_notificationPanel, OmniElementState.VisiblePartially, TimeSpan.FromSeconds(5));
                _device.ControlPanel.WaitForState(_resetButton, OmniElementState.VisibleCompletely, TimeSpan.FromSeconds(5));
            }
        }
        /// <summary>
        /// Navigates to the home screen.  (Does not necessarily sign out.)
        /// </summary>
        private void NavigateHome(JediOmniDevice _device)
        {
            bool homeScreen = false;
            
            if ((homeScreen = IsAtHomeScreen(_device)) == false)
            {
                // Pressing the home button should get us back to the home screen,
                // but if a dialog pops up, we might need to press home again to dismiss it.
                for (int i = 0; i < 10; i++)
                {
                    // Press the home button, then give it a few seconds for the topmost screen to change                    
                    _device.ControlPanel.PressHome();
                    TimeSpan.FromSeconds(3);

                    if (IsAtHomeScreen(_device))
                    {
                        homeScreen = true;
                        break;
                    }
                }
            }
        }
        private string GetTopmostScreen(JediOmniDevice _device)
        {
            if (_device.ControlPanel.GetCount(_topViewAttribute) > 1)
            {
                return "Multiple screens";
            }
            else if (_device.ControlPanel.CheckState($"{_topViewAttribute}[id]", OmniElementState.Exists))
            {
                return _device.ControlPanel.GetValue($"{_topViewAttribute}[id]", "id", OmniPropertyType.Property);
            }
            else if (_device.ControlPanel.CheckState($"{_topViewAttribute}[class]", OmniElementState.Exists))
            {
                return _device.ControlPanel.GetValue($"{_topViewAttribute}[class]", "class", OmniPropertyType.Property);
            }
            else
            {
                return null;
            }
        }
        private bool IsAtHomeScreen(JediOmniDevice _device)
        {
            return _device.ControlPanel.CheckState($".hp-homescreen-folder-view{_topViewAttribute}", OmniElementState.Exists)
                && _device.ControlPanel.CheckState(_homeScreenLogo, OmniElementState.VisibleCompletely);
        }

        public Tuple<int , int> GetResolution(JediOmniDevice _device)
        {
            int x = 0, y = 0;
            try
            {
                x = _device.ControlPanel.GetBoundingBox("body").Right;
                y = _device.ControlPanel.GetBoundingBox("body").Bottom;
                var tuple = new Tuple<int, int>(x, y);
                return tuple;
            }
            catch {
                var tuple = new Tuple<int, int>(0, 0);
                return tuple;
            }
        }

        public void InputString(JediOmniDevice _device, string text)
        {
            try {
                _device.ControlPanel.Type(text);
            }
            catch
            {
                _device.ControlPanel.TypeOnNumericKeypad(text);
            }
        }
    }
}

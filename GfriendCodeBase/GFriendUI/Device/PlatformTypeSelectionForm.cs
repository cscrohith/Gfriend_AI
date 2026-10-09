using HP.GFriend.Core;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace HP.GFriend.UI.Device
{
    /// <summary>
    /// Dialog form for selecting a platform type from available PlatformType enum values.
    /// Features modern design with visual styling and platform descriptions.
    /// </summary>
    public partial class PlatformTypeSelectionForm : Form
    {
        /// <summary>
        /// Gets the selected platform type. Returns null if no selection was made.
        /// </summary>
        public PlatformType? SelectedPlatformType { get; private set; }

        /// <summary>
        /// Initializes a new instance of the PlatformTypeSelectionForm.
        /// </summary>
        public PlatformTypeSelectionForm()
        {
            InitializeComponent();
            InitializePlatformTypes();
            UpdateOkButtonState();
            CustomizeListBoxAppearance();
        }

        /// <summary>
        /// Initializes a new instance of the PlatformTypeSelectionForm with a pre-selected platform type.
        /// </summary>
        /// <param name="currentPlatformType">The current platform type to be selected by default.</param>
        public PlatformTypeSelectionForm(string currentPlatformType) : this()
        {
            if (!string.IsNullOrEmpty(currentPlatformType))
            {
                // Try to parse the current platform type and select it in the list
                if (Enum.TryParse<PlatformType>(currentPlatformType, true, out PlatformType parsedType))
                {
                    for (int i = 0; i < platformTypeListBox.Items.Count; i++)
                    {
                        var item = (PlatformTypeItem)platformTypeListBox.Items[i];
                        if (item.PlatformType == parsedType)
                        {
                            platformTypeListBox.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Customizes the ListBox appearance with custom drawing for better visual experience.
        /// </summary>
        private void CustomizeListBoxAppearance()
        {
            platformTypeListBox.DrawMode = DrawMode.OwnerDrawFixed;
            platformTypeListBox.ItemHeight = 24; // Reduced height since no description
            platformTypeListBox.DrawItem += PlatformTypeListBox_DrawItem;
        }

        /// <summary>
        /// Custom draw handler for ListBox items with color-coded platform types.
        /// Displays only icon and platform name (no descriptions).
        /// </summary>
        private void PlatformTypeListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            var item = (PlatformTypeItem)platformTypeListBox.Items[e.Index];
            e.DrawBackground();

            // Determine colors based on selection and platform type
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color backColor = isSelected ? Color.FromArgb(0, 120, 215) : e.BackColor;
            Color textColor = isSelected ? Color.White : Color.FromArgb(64, 64, 64);
            Color iconColor = GetPlatformColor(item.PlatformType);

            // Draw background
            using (SolidBrush backBrush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(backBrush, e.Bounds);
            }

            // Draw colored indicator bar on the left
            using (SolidBrush iconBrush = new SolidBrush(iconColor))
            {
                Rectangle indicatorRect = new Rectangle(e.Bounds.Left, e.Bounds.Top, 4, e.Bounds.Height);
                e.Graphics.FillRectangle(iconBrush, indicatorRect);
            }

            // Draw platform icon/emoji (centered vertically)
            string platformIcon = GetPlatformIcon(item.PlatformType);
            using (Font iconFont = new Font("Segoe UI Emoji", 11F, FontStyle.Regular))
            {
                e.Graphics.DrawString(platformIcon, iconFont, new SolidBrush(textColor), 
                    new PointF(e.Bounds.Left + 12, e.Bounds.Top + 3));
            }

            // Draw platform name (centered vertically)
            using (Font nameFont = new Font("Segoe UI", 10F, FontStyle.Regular))
            {
                e.Graphics.DrawString(item.PlatformType.ToString(), nameFont, new SolidBrush(textColor),
                    new PointF(e.Bounds.Left + 40, e.Bounds.Top + 4));
            }

            e.DrawFocusRectangle();
        }

        /// <summary>
        /// Gets a color associated with the platform type for visual distinction.
        /// </summary>
        private Color GetPlatformColor(PlatformType platformType)
        {
            switch (platformType)
            {
                case PlatformType.Windows:
                    return Color.FromArgb(0, 120, 212);
                case PlatformType.iOS:
                    return Color.FromArgb(0, 122, 255);
                case PlatformType.Mac:
                    return Color.FromArgb(135, 135, 135);
                case PlatformType.Android:
                case PlatformType.Android2:
                    return Color.FromArgb(61, 220, 132);
                case PlatformType.Web:
                    return Color.FromArgb(255, 127, 39);
                case PlatformType.Jedi:
                    return Color.FromArgb(156, 39, 176);
                case PlatformType.Dune:
                    return Color.FromArgb(255, 193, 7);
                case PlatformType.Ares:
                    return Color.FromArgb(233, 30, 99);
                case PlatformType.Sirius:
                    return Color.FromArgb(0, 188, 212);
                default:
                    return Color.FromArgb(158, 158, 158);
            }
        }

        /// <summary>
        /// Gets an icon/emoji for the platform type.
        /// </summary>
        private string GetPlatformIcon(PlatformType platformType)
        {
            switch (platformType)
            {
                case PlatformType.Windows:
                    return "🪟";
                case PlatformType.iOS:
                    return "📱";
                case PlatformType.Mac:
                    return "💻";
                case PlatformType.Android:
                case PlatformType.Android2:
                    return "🤖";
                case PlatformType.Web:
                    return "🌐";
                case PlatformType.Jedi:
                    return "🖨️";
                case PlatformType.Dune:
                    return "🖨️";
                case PlatformType.Ares:
                    return "🖨️";
                case PlatformType.Sirius:
                    return "🖨️";
                default:
                    return "❓";
            }
        }

        /// <summary>
        /// Populates the ListBox with all available PlatformType enum values.
        /// </summary>
        private void InitializePlatformTypes()
        {
            platformTypeListBox.Items.Clear();

            // Get all PlatformType enum values
            var platformTypes = Enum.GetValues(typeof(PlatformType))
                                    .Cast<PlatformType>()
                                    .OrderBy(pt => pt.ToString());

            foreach (var platformType in platformTypes)
            {
                platformTypeListBox.Items.Add(new PlatformTypeItem(platformType));
            }
        }

        /// <summary>
        /// Updates the OK button state based on whether a platform type is selected.
        /// </summary>
        private void UpdateOkButtonState()
        {
            bool hasSelection = platformTypeListBox.SelectedIndex >= 0;
            okButton.Enabled = hasSelection;

            // Update button appearance based on state
            if (hasSelection)
            {
                okButton.BackColor = Color.FromArgb(0, 120, 215);
                okButton.ForeColor = Color.White;
            }
            else
            {
                okButton.BackColor = Color.FromArgb(200, 200, 200);
                okButton.ForeColor = Color.FromArgb(120, 120, 120);
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event for the platform type ListBox.
        /// Enables the OK button when a selection is made.
        /// </summary>
        private void PlatformTypeListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateOkButtonState();
        }

        /// <summary>
        /// Handles the Click event for the OK button.
        /// Sets the selected platform type and closes the dialog with OK result.
        /// </summary>
        private void OkButton_Click(object sender, EventArgs e)
        {
            if (platformTypeListBox.SelectedIndex >= 0)
            {
                var selectedItem = (PlatformTypeItem)platformTypeListBox.SelectedItem;
                SelectedPlatformType = selectedItem.PlatformType;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        /// <summary>
        /// Handles the Click event for the Cancel button.
        /// Closes the dialog with Cancel result.
        /// </summary>
        private void CancelButton_Click(object sender, EventArgs e)
        {
            SelectedPlatformType = null;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Handles double-click on the ListBox to immediately select and close.
        /// </summary>
        private void PlatformTypeListBox_DoubleClick(object sender, EventArgs e)
        {
            if (platformTypeListBox.SelectedIndex >= 0)
            {
                OkButton_Click(sender, e);
            }
        }

        /// <summary>
        /// Helper class to represent a platform type item in the ListBox.
        /// </summary>
        private class PlatformTypeItem
        {
            public PlatformType PlatformType { get; }

            public PlatformTypeItem(PlatformType platformType)
            {
                PlatformType = platformType;
            }

            public override string ToString()
            {
                return PlatformType.ToString();
            }
        }
    }
}

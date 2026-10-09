using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace HP.GFriend.UI
{
    public partial class WhatsNew : Form
    {
        private Timer marqueeTimer;
        private List<LinkLabel> linkLabels;
        private int scrollSpeed = 1;
        private string xmlFilePath;
        private string marqueeDirection = "Vertical";
        public WhatsNew()
        {
            InitializeComponent();
            this.Text = "What's New";
            this.BackColor = Color.White;
            this.ShowIcon = true;
            this.ControlBox = true;
            this.AutoScroll = true;
            this.AutoSize = false;
            this.Size = new Size(309, 265);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.Resize += SubForm_Resize;
            this.ResizeBegin += SubForm_ResizeBegin;
            this.ResizeEnd += SubForm_ResizeEnd;
            string directoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            xmlFilePath = Path.Combine(directoryPath, "Links.xml");
            LoadMarqueeSettingsFromXml();
            InitializeMarquee();
            PopulateLinks();
        }

        private void LoadMarqueeSettingsFromXml()
        {
            if (!File.Exists(xmlFilePath))
            {
                MessageBox.Show("Links XML file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                XDocument xmlDoc = XDocument.Load(xmlFilePath);
                var settings = xmlDoc.Descendants("Settings").FirstOrDefault();

                if (settings != null)
                {
                    marqueeDirection = settings.Element("marqueedirection")?.Value ?? "Vertical";
                    if (int.TryParse(settings.Element("speed")?.Value, out int speed))
                    {
                        scrollSpeed = speed;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading settings from XML: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void SubForm_ResizeEnd(object sender, EventArgs e)
        {
            this.Size = new Size(309, 265);
            ResetLinkLabelPositions();
            this.ResumeLayout();
        }
        private void InitializeMarquee()
        {
            marqueeTimer = new Timer
            {
                Interval = 60,
            };
            marqueeTimer.Tick += MarqueeTimer_Tick;
            linkLabels = new List<LinkLabel>();
            marqueeTimer.Stop();
        }
        private void PopulateLinks()
        {
            if (!File.Exists(xmlFilePath))
            {
                MessageBox.Show("Links XML file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            foreach (var linkLabel in linkLabels)
            {
                this.Controls.Remove(linkLabel);
            }
            linkLabels.Clear();

            XDocument xmlDoc = XDocument.Load(xmlFilePath);
            var links = xmlDoc.Descendants("Link")
                              .Select(link => new
                              {
                                  Name = link.Element("Name")?.Value,
                                  URL = link.Element("URL")?.Value
                              })
                              .Where(link => !string.IsNullOrEmpty(link.Name) && !string.IsNullOrEmpty(link.URL))
                              .ToList();
            int positionY = 20;
            int maxWidth = this.ClientSize.Width - 25; // Reduce width to prevent horizontal scrolling
            ToolTip toolTip = new ToolTip(); // Tooltip for full text
            foreach (var link in links)
            {
                string displayText = TruncateText(link.Name, maxWidth, new Font("Arial", 12, FontStyle.Regular));
                var linkLabel = new LinkLabel
                {
                    Text = displayText,
                    LinkColor = Color.Blue,
                    Font = new Font("Arial", 12, FontStyle.Regular),
                    Location = new Point(5, positionY),
                    AutoSize = true,
                    Image = Properties.Resources.YellowStarIcon,
                    ImageAlign = ContentAlignment.TopLeft,
                    Tag = link.URL,
                    TextAlign = ContentAlignment.TopLeft,
                    Padding = new Padding(15, 0, 0, 0),
                    LinkBehavior = LinkBehavior.NeverUnderline

                };
                //linkLabel.Tag = link.Name; // Store full text
                if (displayText.EndsWith("..."))
                {
                    toolTip.SetToolTip(linkLabel, link.Name); // Show full text on hover
                }

                linkLabel.Click += (sender, e) =>
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = (string)linkLabel.Tag,
                        UseShellExecute = true
                    });
                };

                linkLabels.Add(linkLabel);
                this.Controls.Add(linkLabel);
                positionY += linkLabel.Height + 20;
            }

            // Enable only vertical scrolling by setting the AutoScrollMinSize
            this.AutoScroll = true;
            this.AutoScrollMinSize = new Size(0, positionY); // No horizontal scrolling
            this.PerformLayout();
        }

        // Method to truncate text if it exceeds max width
        private string TruncateText(string text, int maxWidth, Font font)
        {
            using (Graphics g = this.CreateGraphics())
            {
                if (g.MeasureString(text, font).Width <= maxWidth)
                    return text;

                string ellipsis = "...";
                for (int i = text.Length - 1; i > 0; i--)
                {
                    string truncatedText = text.Substring(0, i) + ellipsis;
                    if (g.MeasureString(truncatedText, font).Width <= maxWidth)
                        return truncatedText;
                }
                return ellipsis; // If the text is too short, just show "..."
            }
        }
        private void MarqueeTimer_Tick(object sender, EventArgs e)
        {
            foreach (var linkLabel in linkLabels)
            {
                if (marqueeDirection == "Vertical")
                {
                    linkLabel.Top -= scrollSpeed;
                    if (linkLabel.Bottom < 0)
                    {
                        linkLabel.Top = this.ClientSize.Height;
                    }
                }
                else if (marqueeDirection == "Horizontal")
                {
                    linkLabel.Left -= scrollSpeed;
                    if (linkLabel.Right < 0)
                    {
                        linkLabel.Left = this.ClientSize.Width;
                    }
                }
            }
        }
        private void SubForm_ResizeBegin(object sender, EventArgs e)
        {
            this.SuspendLayout();
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            this.Size = new Size(370, 255);

            if (File.Exists(xmlFilePath)) // Check if the XML file exists
            {
                PopulateLinks(); // Refresh links to show full text when maximized
            }
        }
        private void SubForm_Resize(object sender, EventArgs e)
        {
            this.PerformLayout(); // Refresh layout
            this.AutoScrollPosition = new Point(0, 0); // Reset scrolling to top
            ResetLinkLabelPositions();
            HideScrollBars(); // Hide the scrollbars            
        }

        private void ResetLinkLabelPositions()
        {
            int positionY = 5;
            foreach (var linkLabel in linkLabels)
            {
                linkLabel.Location = new Point(5, positionY);
                positionY += linkLabel.Height + 20;
            }
        }

        private const int WS_VSCROLL = 0x00200000;
        private const int WS_HSCROLL = 0x00100000;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);

        private const int SB_HORZ = 0; // Hide horizontal scrollbar
        private const int SB_VERT = 1; // Hide vertical scrollbar

        private void HideScrollBars()
        {
            ShowScrollBar(this.Handle, SB_VERT, true); // Hide vertical scrollbar
            ShowScrollBar(this.Handle, SB_HORZ, false); // Hide horizontal scrollbar
        }
    }
}

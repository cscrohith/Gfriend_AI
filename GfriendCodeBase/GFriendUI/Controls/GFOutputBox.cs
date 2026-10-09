using FastColoredTextBoxNS;
using HP.GFriend.GFLogger;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace HP.GFriend.UI.Controls
{
    public partial class GFOutputBox : FastColoredTextBox
    {

        private readonly static Style StyleDefault = new TextStyle(Brushes.Black, null, FontStyle.Regular);

        public GFOutputBox() : base()
        {
            InitializeComponent();
            TextChanged += new EventHandler<TextChangedEventArgs>(TextChanged_On_Output);
            MouseDown += new MouseEventHandler(textOutput_MouseDown);
            MouseMove += new MouseEventHandler(textOutput_MouseMove);
        }

        internal void SetupTextboxOutput(GFOutputBox tb)
        {
            ComponentResourceManager resources = new ComponentResourceManager(typeof(MainForm));
            tb.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tb.AutoCompleteBracketsList = new char[] { '(', ')', '{', '}', '[', ']', '\"', '\"', '\'', '\'' };
            tb.AutoIndent = false;
            tb.AutoIndentChars = false;
            tb.AutoIndentCharsPatterns = "";
            tb.AutoIndentExistingLines = false;
            tb.AutoScrollMargin = new System.Drawing.Size(1, 1);
            tb.AutoScrollMinSize = new System.Drawing.Size(2, 14);
            tb.BackBrush = null;
            tb.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            tb.CharHeight = 14;
            tb.CharWidth = 8;
            tb.Cursor = System.Windows.Forms.Cursors.IBeam;
            tb.DisabledColor = System.Drawing.Color.FromArgb(((int)((byte)(100))), ((int)((byte)(180))), ((int)((byte)(180))), ((int)((byte)(180))));
            tb.IsReplaceMode = false;
            tb.Location = Location;
            tb.Name = "textOutput";
            tb.Paddings = new System.Windows.Forms.Padding(0);
            tb.ReadOnly = true;
            tb.SelectionColor = System.Drawing.Color.FromArgb(((int)((byte)(60))), ((int)((byte)(0))), ((int)((byte)(0))), ((int)((byte)(255))));
            tb.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("textOutput.ServiceColors")));
            tb.ShowLineNumbers = false;
            tb.Size = Size;
            tb.TabIndex = 0;
            tb.Zoom = 100;
            tb.TextChanged += new EventHandler<TextChangedEventArgs>(TextChanged_On_Output);
            tb.MouseDown += new MouseEventHandler(textOutput_MouseDown);
            tb.MouseMove += new MouseEventHandler(textOutput_MouseMove);
        }

        /// <summary>
        /// Text changed event on output field
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        private void TextChanged_On_Output(object sender, TextChangedEventArgs e)
        {
            try
            {
                SelectionStart = Text.Length;
                GoEnd();

                e.ChangedRange.ClearStyle(GFStyles.StylePass);
                e.ChangedRange.ClearStyle(GFStyles.StyleFail);
                e.ChangedRange.ClearStyle(GFStyles.StyleError);
                e.ChangedRange.ClearStyle(GFStyles.StyleHyperLink);

                e.ChangedRange.SetStyle(GFStyles.StylePass, "Pass", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                e.ChangedRange.SetStyle(GFStyles.StyleFail, "Fail", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                e.ChangedRange.SetStyle(GFStyles.StyleError, "Error", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                e.ChangedRange.SetStyle(GFStyles.StyleHyperLink, "[a-zA-Z]:.*output.xml|[a-zA-Z]:.*report.html|[a-zA-Z]:.*log.txt");

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// MouseMove event for text output field
        /// </summary>        
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textOutput_MouseMove(object sender, MouseEventArgs e)
        {
            var p = PointToPlace(e.Location);
            if (CharIsHyperlink(p))
                Cursor = Cursors.Hand;
            else
                Cursor = Cursors.IBeam;
        }

        /// <summary>
        /// MouseDown event for text output field
        /// </summary>        
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textOutput_MouseDown(object sender, MouseEventArgs e)
        {
            var p = PointToPlace(e.Location);
            if (CharIsHyperlink(p))
            {
                string url = GetLine(p.iLine).Text;
                int splitPoint = url.IndexOf(':');
                url = url.Substring(splitPoint + 1).Trim();
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = Path.GetFileName(url);
                psi.WorkingDirectory = Path.GetDirectoryName(url);
                try
                {
                    Process.Start(psi);
                }
                catch (Exception)
                {
                    MessageBox.Show("Can not open file");
                }
            }
        }

        private bool CharIsHyperlink(Place place)
        {
            var mask = GetStyleIndexMask(new Style[] { GFStyles.StyleHyperLink });
            if ((place.iChar < GetLineLength(place.iLine)) && ((this[place].style & mask) != 0))
            {
                return true;
            }
            return false;
        }
    }
}

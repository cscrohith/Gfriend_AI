using FastColoredTextBoxNS;
using System.Drawing;

namespace HP.GFriend.UI.Controls
{
    public static class GFStyles
    {
        public readonly static Style StyleDefault = new TextStyle(Brushes.Black, null, FontStyle.Regular);
        public readonly static Style StyleLibrary = new TextStyle(Brushes.MediumSeaGreen, null, FontStyle.Bold);
        public readonly static Style StyleKeyword = new TextStyle(Brushes.Black, null, FontStyle.Regular);
        public readonly static Style StyleVariable = new TextStyle(Brushes.Olive, null, FontStyle.Regular);
        public readonly static Style StyleControls = new TextStyle(Brushes.MediumBlue, null, FontStyle.Bold);
        public readonly static Style StyleComments = new TextStyle(Brushes.ForestGreen, null, FontStyle.Italic);
        public readonly static Style StyleMetadata = new TextStyle(Brushes.Gray, null, FontStyle.Italic);
        public readonly static Style StylePass = new TextStyle(Brushes.ForestGreen, null, FontStyle.Bold);
        public readonly static Style StyleFail = new TextStyle(Brushes.Red, null, FontStyle.Bold);
        public readonly static Style StyleError = new TextStyle(Brushes.Orange, null, FontStyle.Bold);
        public readonly static Style StyleHyperLink = new TextStyle(Brushes.Blue, null, FontStyle.Underline);

    }
}

using System.Drawing;

namespace HP.GFriend.Utils.Vision
{
    public class ControlRecognitionResult
    {
        public Rectangle Bound { get; private set; }
        public bool Checked { get; private set; }
        public string Description { get; set; }

        public ControlRecognitionResult(Rectangle bound, bool checkStatus)
        {
            Bound = bound;
            Checked = checkStatus;
        }
    }
}

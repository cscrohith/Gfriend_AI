using Newtonsoft.Json;
using System.Drawing;
using Tesseract;

namespace HP.GFriend.Utils.Vision
{
    public class OcrResult
    {
        public Rectangle Bound { get; private set; }
        public string Text { get; private set; }

        public OcrResult(string text, Rect bound)
        {
            Bound = new Rectangle(bound.X1, bound.Y1, bound.Width, bound.Height);
            Text = text;
        }

        public OcrResult(string text, Rectangle bound)
        {
            Text = text;
            Bound = bound;
        }

        [JsonConstructor]
        public OcrResult(Rectangle bound, string text)
        {
            Bound = bound;
            Text = text;
        }

    }
}

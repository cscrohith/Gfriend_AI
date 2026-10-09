using HP.DeviceAutomation.Jedi.OmniUserInteraction;
using System.Drawing;
using System.ServiceModel;

namespace HP.GFriend.Tool
{
    internal class NativeElement
    {
        public string Id { get; set; }
        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }
        
        public NativeElement(string id, ElementPosition position)
        {
            Id = id;
            X = position.Left;
            Y = position.Top;
            Width = position.Width;
            Height = position.Height;
        }

        public NativeElement(string id, int x, int y, int width, int height)
        {
            Id = id;
            X = x;
            Y = y;
            Width = width;
            Height = height;           
        }

        public string GetBoundString()
        {
            return $"[{X},{Y}][{X+Width},{Y+Height}]";
        }

        public int GetSize()
        {
            return Width * Height;
        }

    }
}

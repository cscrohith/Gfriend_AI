using System;
using System.Drawing;

namespace HP.GFriend.Utils.Vision
{
    internal static class VisionUtils
    {
        /// <summary>
        /// Get distance between middle right point of item1 and middle left point of item2
        /// </summary>
        /// <param name="item1">Item 1 for combobox or radio button</param>
        /// <param name="item2">Item 2 for description of combobox or radio button</param>
        /// <returns>distance as double</returns>
        public static double GetDistance(Rectangle item1, Rectangle item2)
        {
            Point pItem1 = new Point(item1.X + item1.Width, item1.Y + item1.Height / 2);
            Point pItem2 = new Point(item2.X, item2.Y + item2.Height / 2);

            int diffX = Math.Abs(pItem1.X - pItem2.X);
            int diffY = Math.Abs(pItem1.Y - pItem2.Y);
            int squareDistance = diffX * diffX + diffY * diffY;
            return Math.Sqrt(squareDistance);
        }

        public static Point GetCenter(this Rectangle item)
        {
            int x = item.X + item.Width / 2;
            int y = item.Y + item.Height / 2;

            return new Point(x, y);
        }

    }
}

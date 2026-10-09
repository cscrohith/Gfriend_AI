using System.Drawing;
using System.IO;

namespace HP.GFriend.Utils.Vision
{
    public static class ImageExtension
    {
        public static byte[] ToByteArray(this Image image)
        {
            using (var ms = new MemoryStream())
            {
                Bitmap newImage = new Bitmap(image);
                newImage.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                return ms.ToArray();
            }
        }

        public static Bitmap ToBitmap(this byte[] image)
        {
            using (var ms = new MemoryStream(image))
            {
                Bitmap bitmap = new Bitmap(ms);
                return bitmap;
            }
        }

    }
}

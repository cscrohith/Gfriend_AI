using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Forms;
using HP.GFriend.GFLogger;
using static HP.GFriend.Keywords.MouseOperations;

namespace HP.GFriend.Keywords
{

    public class WindowsVisionMethods
    {
        public AutomationElement _target;
        private ImageConverter _converter = null;   

        public Windows WindowsSystem { get; set; }

        public byte[] GetScreenShot()
        {
            Rect bound = new Rect();
            if (WindowsSystem._target != null)
            {
                try
                {
                        bound = (Rect)(WindowsSystem._target.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty));
                   
                }
                catch (Exception) { }
            }
            return  WindowsSystem.GetScreenShot(bound);
            //return GetScreenShot(bound);
        }
        public byte[] GetScreenShot(Rect bound)
        {
            try
            {
                Bitmap bitmap = new Bitmap((int)bound.Width, (int)bound.Height);
                Graphics graphics = Graphics.FromImage(bitmap);
                graphics.CopyFromScreen((int)bound.X, (int)bound.Y, 0, 0, new System.Drawing.Size((int)bound.Width, (int)bound.Height));
                return (byte[])_converter.ConvertTo(bitmap, typeof(byte[]));
            }

            catch (Exception ex)
            {
                Logger.Error("Error during capture", ex);
                return null;
            }
        }

        public bool MouseLeftClick(MousePoint pt)
        {
            MouseOperations.SetCursorPosition(pt);
            MouseOperations.MouseEvent(MouseEventFlags.LeftDown);
            Thread.Sleep(100);
            MouseOperations.MouseEvent(MouseEventFlags.LeftUp);
            return true;
        }
    }

}
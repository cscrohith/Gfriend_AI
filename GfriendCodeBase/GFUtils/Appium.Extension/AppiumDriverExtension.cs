using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Mac;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;

namespace OpenQA.Selenium.Appium.Extension
{
    public enum ClickPosition
    {
        Center,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
    public static class AppiumDriverExtension
    {
        public static int GetPixelRatio(this IOSDriver driver)
        {
            try
            {
                var windowSize = driver.Manage().Window.Size;

                int logicalWidth = GetLogicalWidth();

                return windowSize.Width / logicalWidth;
            }
            catch (Exception)
            {
                return 0;
            }
        }
        private static int GetLogicalWidth()
        {
            return 375; // Example for iPhone X size
        }
        public static int GetPixelRatio(this MacDriver driver)
        {
            try
            {
                string sPixelRatio = driver.SessionDetails["pixelRatio"].ToString();
                return int.Parse(sPixelRatio);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static void ClickWithPosition(this MacDriver driver, IWebElement element, ClickPosition position)
        {
            var windowSize = driver.Manage().Window.Size;
            int offsetX = 0;
            int offsetY = 0;

            int width = element.Size.Width;
            int height = element.Size.Height;
            switch (position)
            {
                case ClickPosition.Center:
                    offsetX = 0;
                    offsetY = 0;
                    break;
                case ClickPosition.TopLeft:
                    offsetX = -width / 2 + width / 50;
                    offsetY = -height / 2 + height / 50;
                    break;
                case ClickPosition.TopRight:
                    offsetX = width / 2 - width / 50;
                    offsetY = -height / 2 + height / 50;
                    break;
                case ClickPosition.BottomLeft:
                    offsetX = -width / 2 + width / 50;
                    offsetY = height / 2 - height / 50;
                    break;
                case ClickPosition.BottomRight:
                    offsetX = width / 2 - width / 50;
                    offsetY = height / 2 - height / 50;
                    break;
            }

            var mouse = new PointerInputDevice(PointerKind.Mouse);
            var actionSequence = new ActionSequence(mouse, 0);

            actionSequence.AddAction(mouse.CreatePointerMove(element, offsetX, offsetY, TimeSpan.FromMilliseconds(100)));
            actionSequence.AddAction(mouse.CreatePointerDown(MouseButton.Left));
            actionSequence.AddAction(mouse.CreatePause(TimeSpan.FromMilliseconds(100)));
            actionSequence.AddAction(mouse.CreatePointerUp(MouseButton.Left));

            driver.PerformActions(new List<ActionSequence> { actionSequence });
        }
    }
}

using HP.GFriend.GFLogger;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using Tesseract;

namespace HP.GFriend.Utils.Vision
{
    public class OcrTesseract
    {
        public enum Language
        {
            eng
        }

        public enum ImageOption
        {
            Original,
            BlackAndWhite,
            GrayScale,
        }

        public PageIteratorLevel PageIterationLevel { get; set; }
        public PageSegMode PageSegMode { get; set; }
        public int DefaultThresHold { get; internal set; }
        public float _pixScale { get; set; } = 3.0f;

        private string _dataSetLocation;
        private TesseractEngine _tesseractEngine;

        //TODO : check best options(PageIteratorLevel and PageSegMode) for OCR by testing various images from solutions
        public OcrTesseract() 
            : this(PageIteratorLevel.Block, PageSegMode.SparseText)
        { }

        public OcrTesseract(PageIteratorLevel pageIteratorLevel, PageSegMode pageSegMode) 
            : this(pageIteratorLevel, pageSegMode, "tesseract_data")
        { }

        public OcrTesseract(PageIteratorLevel pageIteratorLevel, PageSegMode pageSegMode, string dataSetPath) 
            : this(pageIteratorLevel, pageSegMode, dataSetPath, Language.eng)
        { }
        
        public OcrTesseract(PageIteratorLevel pageIteratorLevel, PageSegMode pageSegMode, string dataSetPath, Language targetLanguage)
        {
            try
            {
                PageIterationLevel = pageIteratorLevel;
                PageSegMode = pageSegMode;
                _dataSetLocation = dataSetPath;
                string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                _dataSetLocation = Path.Combine(assemblyFolder, "tesseract_data");
                _tesseractEngine = new TesseractEngine(_dataSetLocation, targetLanguage.ToString());
            }
            catch(DllNotFoundException dllException)
            {
                if (dllException.Message.Contains("tesseract"))
                {
                    Console.WriteLine("Please install Microsoft Visual C++ 2015-2022 Redistributable from below link https://support.microsoft.com/en-us/help/2977003/the-latest-supported-visual-c-downloads to fix the issue with tesseract dll");
                    Logger.Trace("Please install Microsoft Visual C++ 2015-2022 Redistributable from below link https://support.microsoft.com/en-us/help/2977003/the-latest-supported-visual-c-downloads to fix the issue with tesseract dll");
                }
            }
            catch(Exception ex)
            {
                Logger.Trace(ex.Message);
            }
        }

        public List<OcrResult> Process(Image image)
        {
            byte[] byteImage = image.ToByteArray();
            return Process(byteImage);
        }

        public List<OcrResult> Process(byte[] image)
        {
            List<OcrResult> result = new List<OcrResult>();
            
            Pix pix = Pix.LoadFromMemory(image);
            
            // TODO : Check various result based on scale arguments
            pix = pix.Scale(_pixScale, _pixScale);
            Page page = _tesseractEngine.Process(pix, PageSegMode);
            
            using (ResultIterator iter = page.GetIterator())
            {
                iter.Begin();
                do
                {
                    string str = iter.GetText(PageIterationLevel);
                    if (!string.IsNullOrEmpty(str) && iter.TryGetBoundingBox(PageIterationLevel, out Rect rect))
                    {
                        // calculate position based on scale
                        rect = new Rect(rect.X1 / (int)_pixScale, rect.Y1 / (int)_pixScale, rect.Width / (int)_pixScale, rect.Height / (int)_pixScale);
                        result.Add(new OcrResult(str, rect));
                    }

                }
                while (iter.Next(PageIterationLevel));
            }
            return result;
        }
    }
}

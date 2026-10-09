using System;
using System.Collections.Generic;
using System.Drawing;
using Tesseract;

namespace HP.GFriend.Utils.Vision
{
    public class GFVision
    {
        private OcrTesseract _ocrEngine;
        private ImageProcessing _visionEngine;
        public int DefaultThresHold = 180;
        private PageIteratorLevel _pageIterationLevel;

        public GFVision()
        {
            // By default iteratorLevel = sparseText and Block
            _ocrEngine = new OcrTesseract(PageIteratorLevel.Block, PageSegMode.SparseText);

        }
        
        public void ChangePageSegMode(PageSegMode segMode)
        {
            _ocrEngine.PageSegMode = segMode;
        }
       
        public void ChangePageIteratorLevel(PageIteratorLevel iterLevel)
        {
            _ocrEngine.PageIterationLevel = iterLevel;
        }

        public void SetPageIterationLevel(string iterationLevel)
        {            
            switch (iterationLevel.ToLower())
            {
                case "word":
                    _ocrEngine.PageIterationLevel = PageIteratorLevel.Word;
                    _pageIterationLevel = PageIteratorLevel.Word;
                    break;

                case "block":
                    _ocrEngine.PageIterationLevel = PageIteratorLevel.Block;
                    _pageIterationLevel = PageIteratorLevel.Block;
                    break;

                default:
                    _ocrEngine.PageIterationLevel = PageIteratorLevel.Block;
                    _pageIterationLevel = PageIteratorLevel.Block;
                    break;
            }
            
        }

        public void ChangeThresholdValue(int thresholdValue)
        {
            _ocrEngine.DefaultThresHold = thresholdValue;
        }

        public void SetPixelScale(float pixelScale)
        {
            _ocrEngine._pixScale = pixelScale;
        }

        /// <summary>
        /// Pre processing images for OCR. Binarize and remove color and boders from button.
        /// </summary>
        /// <param name="image">image as byte array</param>
        private void OCRPreProcess(byte[] image)
        {
            _visionEngine = new ImageProcessing(image.ToBitmap());
            _visionEngine.ToBinary(DefaultThresHold);

            // Invert color filled button
            System.Drawing.Image temp = _visionEngine.ProcessedImage;
            _visionEngine = new ImageProcessing(new Bitmap(temp));
            _visionEngine.ToBinary(DefaultThresHold);
            _visionEngine.Invert();

            temp = _visionEngine.ProcessedImage;
            _visionEngine = new ImageProcessing(new Bitmap(temp));
            _visionEngine.ToBinary(DefaultThresHold);
            _visionEngine.InvertButtons();

            // Convert whilte bordered button to color filled button (to remove borders)
            temp = _visionEngine.ProcessedImage;
            _visionEngine = new ImageProcessing(new Bitmap(temp));
            _visionEngine.ToBinary(DefaultThresHold);
            _visionEngine.Invert();

            temp = _visionEngine.ProcessedImage;
            _visionEngine = new ImageProcessing(new Bitmap(temp));
            _visionEngine.ToBinary(DefaultThresHold);
            _visionEngine.InvertButtons();

            temp = _visionEngine.ProcessedImage;
            _visionEngine = new ImageProcessing(new Bitmap(temp));
            _visionEngine.ToBinary(DefaultThresHold);
            _visionEngine.Invert();

            // Invert color filled button again
            temp = _visionEngine.ProcessedImage;
            _visionEngine = new ImageProcessing(new Bitmap(temp));
            _visionEngine.ToBinary(DefaultThresHold);
            _visionEngine.Invert();            

            temp = _visionEngine.ProcessedImage;
            _visionEngine = new ImageProcessing(new Bitmap(temp));
            _visionEngine.ToBinary(DefaultThresHold);
            _visionEngine.InvertButtons();
        }

        public List<OcrResult> RunOCR(byte[] image)
        {
            _ocrEngine = new OcrTesseract(_pageIterationLevel, PageSegMode.SparseText);            
            OCRPreProcess(image);
            return _ocrEngine.Process(_visionEngine.ProcessedImage);
        }

        public List<ControlRecognitionResult> GetCheckBoxes(byte[] image)
        {
            _visionEngine = new ImageProcessing(image.ToBitmap());
            // TODO : find valid threshold value
            _visionEngine.ToBinary(240);
            _visionEngine.Invert();
            List<ControlRecognitionResult> checkboxes = _visionEngine.FindCheckboxes();
            List<OcrResult> ocrResults = RunOCR(image);
            foreach (ControlRecognitionResult cb in checkboxes)
            {
                double distance = 9999;
                foreach (OcrResult ocrResult in ocrResults)
                {

                    double currentDistance = VisionUtils.GetDistance(cb.Bound, ocrResult.Bound);
                    if(currentDistance < distance)
                    {
                        distance = currentDistance;
                        cb.Description = ocrResult.Text;
                    }
                }
            }
            return checkboxes;
        }

        public List<List<ControlRecognitionResult>> GetRadioButtons(byte[] image, ImageProcessing.RadioButtonAlignment radioButtonAlignment)
        {
            _visionEngine = new ImageProcessing(image.ToBitmap());
            // TODO : find valid threshold value
            _visionEngine.ToBinary(240);
            _visionEngine.Invert();
            List<List<ControlRecognitionResult>> radioButtons = _visionEngine.FindRadioButtons(radioButtonAlignment);
            List<OcrResult> ocrResults = RunOCR(image);
            foreach (List<ControlRecognitionResult> radioButtonGroup in radioButtons)
            {
                foreach(ControlRecognitionResult radio in radioButtonGroup)
                {
                    double distance = 9999;
                    foreach (OcrResult ocrResult in ocrResults)
                    {

                        double currentDistance = VisionUtils.GetDistance(radio.Bound, ocrResult.Bound);
                        if (currentDistance < distance)
                        {
                            distance = currentDistance;
                            radio.Description = ocrResult.Text;
                        }
                    }
                }
            }
            return radioButtons;
        }

        public List<ControlRecognitionResult> FindImage(byte[] originalImage, byte[] findImage, float thresdHold = 0.95f)
        {
            _visionEngine = new ImageProcessing(originalImage.ToBitmap());
            return _visionEngine.FindImage(findImage.ToBitmap(), thresdHold);
        }


    }
}

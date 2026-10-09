
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Accord;
using Accord.Imaging;
using Accord.Imaging.Filters;
using Accord.Math.Geometry;

namespace HP.GFriend.Utils.Vision
{
    public class ImageProcessing
    {
        public enum RadioButtonAlignment
        {
            Vertical,
            Horizontal
        }

        public Bitmap OriginalImage { get; private set; }
        public Bitmap ProcessedImage { get; private set; }

        public ImageProcessing(Bitmap image)
        {
            OriginalImage = image;
            ProcessedImage = image;
        }
        public void Rollback()
        {
            ProcessedImage = OriginalImage;
        }

        public void Save(string filename)
        {
            ProcessedImage.Save(filename);
        }

        public void ToBinary(int threshold=120)
        {
            // TODO: check best threshold value by testing with various images
            Threshold thresholdFilter = new Threshold(threshold);
            Grayscale grayScaleFilter = new Grayscale(0.2125, 0.7154, 0.0721);
            
            Bitmap grayImage = grayScaleFilter.Apply(OriginalImage);

            ProcessedImage = thresholdFilter.Apply(grayImage);
            
            Invert invert = new Invert();
            ProcessedImage = invert.Apply(ProcessedImage);
            
        }

        public void Invert()
        {
            Invert invert = new Invert();
            ProcessedImage = invert.Apply(ProcessedImage);
        }

        public void InvertButtons()
        {
            BlobCounter counter = new BlobCounter();
            counter.MaxHeight = ProcessedImage.Height/2;
            counter.MaxWidth = ProcessedImage.Width/2;
            
            counter.MinHeight = 20;
            counter.MinWidth = 20;
            counter.FilterBlobs = true;

            counter.ProcessImage(ProcessedImage);

           
            Invert invert = new Invert();
            foreach (Blob blob in counter.GetObjectsInformation())
            {
                List<IntPoint> edge = counter.GetBlobsEdgePoints(blob);
                if (edge.Count >=4)
                {

                    Color tl = ProcessedImage.GetPixel((int)(blob.Rectangle.X + blob.Rectangle.Width * 0.10), (int)(blob.Rectangle.Y + blob.Rectangle.Height * 0.10));
                    Color tr = ProcessedImage.GetPixel((int)(blob.Rectangle.X + blob.Rectangle.Width * 0.90), (int)(blob.Rectangle.Y + blob.Rectangle.Height * 0.10));
                    Color bl = ProcessedImage.GetPixel((int)(blob.Rectangle.X + blob.Rectangle.Width * 0.10), (int)(blob.Rectangle.Y + blob.Rectangle.Height * 0.90));
                    Color br = ProcessedImage.GetPixel((int)(blob.Rectangle.X + blob.Rectangle.Width * 0.90), (int)(blob.Rectangle.Y + blob.Rectangle.Height * 0.90));

                    bool isWhite = (tl.R + tl.G + tl.B + tr.R + tr.G + tr.B + bl.R + bl.G + bl.B + br.R + br.G + br.B) >= 3000 ? true : false;

                    if(isWhite)
                    {
                        invert.ApplyInPlace(ProcessedImage, blob.Rectangle);
                    }
                }
            }
        }

        public List<ControlRecognitionResult> FindCheckboxes()
        {
            List<ControlRecognitionResult> result = new List<ControlRecognitionResult>();
            BlobCounter counter = new BlobCounter();

            // TODO : find valid min/max size of checkbox by tesing various input images
            counter.MaxHeight = 20;
            counter.MaxWidth = 20;

            counter.MinHeight = 8;
            counter.MinWidth = 8;
            counter.FilterBlobs = true;

            counter.ProcessImage(ProcessedImage);
        
            foreach (Blob blob in counter.GetObjectsInformation())
            {
                List<IntPoint> edge = counter.GetBlobsEdgePoints(blob);
                if (edge.Count >=4 && 
                    Math.Abs(blob.Rectangle.Width - blob.Rectangle.Height) < 5)
                {

                    bool isChecked = true;
                    double blobArea = blob.Rectangle.Width * blob.Rectangle.Height * 0.90;
                    if(blobArea <= (double)blob.Area)
                    {
                        isChecked = false;
                    }
                    ControlRecognitionResult checkBox = new ControlRecognitionResult(blob.Rectangle, isChecked);
                    result.Add(checkBox);

                }
            }
            return result;
        }

        public List<List<ControlRecognitionResult>> FindRadioButtons(RadioButtonAlignment radioButtonAlignment)
        {
            List<List<ControlRecognitionResult>> result = new List<List<ControlRecognitionResult>>();
            BlobCounter counter = new BlobCounter();
            
            // TODO : find valid min/max size of radio button by tesing various input images
            counter.MaxHeight = 45;
            counter.MaxWidth = 45;

            counter.MinHeight = 15;
            counter.MinWidth = 15;
            counter.FilterBlobs = true;

            counter.ProcessImage(ProcessedImage);
            SimpleShapeChecker shapeChecker = new SimpleShapeChecker();
            List<Blob> circles = new List<Blob>();
            
            // Find all circles
            foreach (Blob blob in counter.GetObjectsInformation())
            {
                List<IntPoint> edge = counter.GetBlobsEdgePoints(blob);

                if (shapeChecker.IsCircle(edge))
                {
                    circles.Add(blob);
                }
            }

            IEnumerable<IGrouping<int, Blob>> radioGroups;
            switch(radioButtonAlignment)
            {
                case RadioButtonAlignment.Horizontal:
                    radioGroups = circles.GroupBy(c => c.Rectangle.GetCenter().Y, new ToleranceEqualityComparer(3));
                    break;
                case RadioButtonAlignment.Vertical:
                    radioGroups = circles.GroupBy(c => c.Rectangle.GetCenter().X, new ToleranceEqualityComparer(3));
                    break;
                default:
                    throw new ArgumentNullException("RadioButtonAlignment should be given");
            }

            foreach (var radioGroup in radioGroups)
            {
                if (radioGroup.Key > 1)
                {
                    List<ControlRecognitionResult> radios = new List<ControlRecognitionResult>();
                    foreach (Blob blob in radioGroup)
                    {
                        bool isChecked = true;
                        if (radioGroup.Count(r => r.Area / 10 * 10 == blob.Area / 10 * 10) > 1)
                        {
                            isChecked = false;
                        }
                        ControlRecognitionResult radio = new ControlRecognitionResult(blob.Rectangle, isChecked);
                        radios.Add(radio);
                    }
                    result.Add(radios);
                }
            }

            return result;
        }

        public List<ControlRecognitionResult> FindImage(System.Drawing.Image imageToFind, float thresdHold = 0.95f)
        {
            List<ControlRecognitionResult> results = new List<ControlRecognitionResult>();
            ExhaustiveTemplateMatching etm = new ExhaustiveTemplateMatching(thresdHold);
            Bitmap target = new Bitmap(ProcessedImage.Width, ProcessedImage.Height, PixelFormat.Format24bppRgb);
            using(Graphics tg = Graphics.FromImage(target))
            {
                tg.DrawImage(ProcessedImage, new Rectangle(0, 0, target.Width, target.Height));
            }
            TemplateMatch[] matchings = etm.ProcessImage(target, imageToFind as Bitmap);

           
            foreach (TemplateMatch matching in matchings)
            {
                ControlRecognitionResult m = new ControlRecognitionResult(matching.Rectangle, true);
                results.Add(m);
            }

            return results;
        }

    }
}

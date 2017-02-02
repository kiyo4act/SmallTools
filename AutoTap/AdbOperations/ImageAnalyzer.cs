using OpenCvSharp;
using System;
using System.Diagnostics;

namespace AdbOperations
{
    public class ImageAnalyzer
    {
        private IplImage Img { get; set; }
        private IplImage Tmp { get; set; }
        private string ScreenShotPath { get; set; }

        public ImageAnalyzer()
        {
            Debug.WriteLine("ImageAnalyzer.ImageAnalyzer()");
        }

        public ImageAnalyzer(string imagePath)
        {
            Debug.WriteLine("ImageAnalyzer.ImageAnalyzer(string)");
            try
            {
                Img = new IplImage(imagePath, LoadMode.Color);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }
        }
        public ImageAnalyzer(string imagePath, string tmpPath)
        {
            Debug.WriteLine("ImageAnalyzer.ImageAnalyzer(string, string)");
            try
            {
                Img = new IplImage(imagePath, LoadMode.Color);
                Tmp = new IplImage(tmpPath, LoadMode.Color);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }
        }

        public IplImage TrimImage(IplImage src, int x, int y, int width, int height)
        {
            Debug.WriteLine("ImageAnalyzer.TrimImage(IplImage, int, int, int, int)");
            IplImage dest = new IplImage(width, height, src.Depth, src.NChannels);
            Cv.SetImageROI(src, new CvRect(x, y, width, height));
            dest = Cv.CloneImage(src);
            Cv.ResetImageROI(src);
            return dest;
        }

        public void TrimInstanceImage(int x, int y, int width, int height)
        {
            Debug.WriteLine("ImageAnalyzer.TrimInstanceImage(int, int, int, int)");
            Img = TrimImage(Img, x, y, width, height);
        }

        public bool IsPatternMatching(IplImage srcImage, IplImage tmpImage, double threshold)
        {
            Debug.WriteLine("ImageAnalyzer.IsPatternMatching(IplImage, IplImage, double)");
            bool fResult = false;

            try
            {
                double result = CheckPatternMatchingScore(srcImage, tmpImage);
                Trace.WriteLine(string.Format("パターンマッチングの評価点数: {0}", result));
                fResult = (result >= threshold);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            
            return fResult;
        }

        public bool IsPatternMatching(double threshold)
        {
            Debug.WriteLine("ImageAnalyzer.IsPatternMatching(double)");
            bool fResult = false;

            try
            {
                double result = CheckPatternMatchingScore();
                Trace.WriteLine(string.Format("パターンマッチングの評価点数: {0}", result));
                fResult = (result >= threshold);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            
            return fResult;
        }

        public double CheckPatternMatchingScore(IplImage srcImage, IplImage tmpImage)
        {
            Debug.WriteLine("ImageAnalyzer.CheckPatternMatchingScore(IplImage, IplImage)");

            CvMat result;
            double minVal = 0;
            double maxVal = 0;

            try
            {
                result = new CvMat(srcImage.Height - tmpImage.Height + 1, srcImage.Width - tmpImage.Width + 1, MatrixType.F32C1);
                Cv.MatchTemplate(srcImage, tmpImage, result, MatchTemplateMethod.CCoeffNormed);
                Cv.MinMaxLoc(result, out minVal, out maxVal);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return maxVal;
        }

        public double CheckPatternMatchingScore()
        {
            Debug.WriteLine("ImageAnalyzer.CheckPatternMatchingScore()");

            CvMat result;
            double minVal = 0;
            double maxVal = 0;

            try
            {
                result = new CvMat(Img.Height - Tmp.Height + 1, Img.Width - Tmp.Width + 1, MatrixType.F32C1);
                Cv.MatchTemplate(Img, Tmp, result, MatchTemplateMethod.CCoeffNormed);
                Cv.MinMaxLoc(result, out minVal, out maxVal);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return maxVal;
        }

        public bool SaveImage(string filePath)
        {
            Debug.WriteLine("ImageAnalyzer.SaveImage(string)");

            bool fResult = true;
            try
            {
                Img.SaveImage(filePath);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                fResult = false;
            }
            return fResult;
        }
    }
}

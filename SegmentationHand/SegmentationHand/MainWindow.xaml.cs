﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
//using System.Windows.Shapes;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure; 
using Emgu.Util;
using System.Drawing;
using System.IO; 

namespace SegmentationHand
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //:::::::::::::::::Variables::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private Seq<System.Drawing.Point> Hull;
        private Seq<MCvConvexityDefect> defects;
        private MCvConvexityDefect[] defectsArray;
        private MCvBox2D box;
        private PointF[] points;
        private double contourArea;
        private double contourPerimeter;
        private double convexHullArea;
        private double convexHullPerimeter;

        //Save the frames to check the noise remove in the roi, also check the bnarization  
        private string path1 = @"C:\CaptureKinect\Binarization\test5\frames\";
        private string path2 = @"C:\CaptureKinect\Binarization\test5\binary\";
        private string path3 = @"C:\CaptureKinect\Binarization\test5\convex\";
        private int numFrames = 1;

        public int numero;
        //:::::::::::::::::fin variables::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        
        public MainWindow()
        {
            InitializeComponent();
        }//end


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Image<Gray, Byte> frameRoi;

            for (int i = 1; i < 49; i++)
            {
                frameRoi = new Image<Gray, Byte>(path1 + i.ToString() + ".png");

                frameRoi = binaryNiBlack(frameRoi);
                frameRoi.Save(path2 + numFrames.ToString() + ".png"); //Borrar ya que funcione
                HandConvexHull(frameRoi); 
                numFrames ++; 
            }
        } //end WindowLoaded

        //:::::::::::::Method for make the image binary::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //the binarization is inspired in NiBlanck banarization, but in this case, we use just the average of the image. 
        //openinOperation() remove the noise of the binarized image, using morphological operation, we use opening. 

        private Image<Gray, Byte> binaryNiBlack(Image<Gray,Byte> handFrame)
        {
            int widthFrame = handFrame.Width; 
            int heigthFrame = handFrame.Height;
            
            int sizeSW = 3; 
            int sizeSW_w = sizeSW; //Size of the slinding window 
            int sizeSW_h = sizeSW; //Size of the slinding window 
            int halfWidth = (int)(Math.Floor((double)sizeSW/2));  
            int halfHeigth = (int)(Math.Floor((double)sizeSW/2));
            int binaryWidth = widthFrame + halfWidth*2;
            int binaryHeigth = heigthFrame + halfHeigth*2; 
            double k = 0.5;


            Image<Gray, Byte> binaryFrameCalculation = new Image<Gray, Byte>(binaryWidth,binaryHeigth);
            binaryFrameCalculation.SetZero(); 
            Rectangle roiHand = new Rectangle(halfWidth, halfHeigth, widthFrame,  heigthFrame);
            binaryFrameCalculation.ROI = roiHand;
            handFrame.CopyTo(binaryFrameCalculation);
            binaryFrameCalculation.ROI = Rectangle.Empty;


            byte[, ,] byteData = handFrame.Data; 
            
            
            for (int i = halfHeigth; i<heigthFrame+halfHeigth; i++)
            {
                for (int j = halfWidth; j < widthFrame+halfWidth; j++)
                {
                    Gray media;
                    MCvScalar desest;
                    MCvScalar mediaValue;
                    double threshold;
                    MCvBox2D roi;          

                    Image<Gray, Byte> imageCalculate = new Image<Gray, Byte>(sizeSW_w, sizeSW_h);
                    roi = new MCvBox2D(new System.Drawing.Point(j, i), new System.Drawing.Size (sizeSW_w, sizeSW_h),0); 

                    imageCalculate = binaryFrameCalculation.Copy(roi);
                    binaryFrameCalculation.ROI = Rectangle.Empty; 
                    imageCalculate.AvgSdv(out media, out desest);
                    mediaValue = media.MCvScalar;
                    threshold = mediaValue.v0 + (k * desest.v0);

                    if (byteData[i-halfHeigth, j-halfWidth, 0] < threshold)
                        byteData[i-halfHeigth, j-halfWidth, 0] = 255;
                    else
                        byteData[i-halfHeigth, j-halfWidth, 0] = 0;
                }
            }

            handFrame.Data = byteData; 
            return handFrame; 
        }

        private Image<Gray, Byte> binaryThresholdNiBlack(Image<Gray, Byte> handImage)
        {
            Gray media;
            MCvScalar desest;
            MCvScalar mediaValue;
            double cv;
            double Kder = 2;
            double Kizq = 0.2;
            double Rizq = 0;
            double Rder = 0;


            handImage.AvgSdv(out media, out desest);
            mediaValue = media.MCvScalar;

            cv = desest.v0 / mediaValue.v0;

            if (mediaValue.v0 < 30.0)
                Kder = 2.3;

            Rizq = mediaValue.v0 - (Kizq * desest.v0);
            Rder = mediaValue.v0 + (Kder * desest.v0);

            //saveStatictics(numFrames, mediaValue.v0, desest.v0, cv, Rizq, Rder); 

            handImage = handImage.InRange(new Gray(Rizq), new Gray(Rder));
            handImage.Save(path2 + numFrames.ToString() + "B.png");
            //handImage = handImage.Not(); 

            //handImage = handImage.ThresholdBinary(media, new Gray(255));
            handImage = openingOperation(handImage);
            handImage.Save(path2 + numFrames.ToString() + "_O.png");
            
            //handImage = closeOperation(handImage);
            //handImage.Save(path2 + numFrames.ToString() + "_C.png");

            return handImage;
        }//end BinaryThresholdNiBlack  


        private Image<Gray, Byte> openingOperation(Image<Gray, Byte> binaryFrame)
        {
            StructuringElementEx SElement;

            SElement = new StructuringElementEx(7,3, 3, 2, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_RECT);

            binaryFrame._MorphologyEx(SElement, Emgu.CV.CvEnum.CV_MORPH_OP.CV_MOP_OPEN, 1);

            return binaryFrame;
        } //end openingOperation()


        private Image<Gray, Byte> closeOperation(Image<Gray, Byte> binaryFrame)
        {
            StructuringElementEx SElement;

            SElement = new StructuringElementEx(11,11, 5, 5, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_RECT);

            binaryFrame._MorphologyEx(SElement, Emgu.CV.CvEnum.CV_MORPH_OP.CV_MOP_CLOSE, 1);

            return binaryFrame;
        }

        //::::::::::::Method to calculate the convex hull:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

        public void HandConvexHull(Image<Gray, Byte> frameRoi)
        {
            List<object> ListReturn = new List<object>();
            //PointF centerPalm; 

            //frameRoi.Save(path1 + "W13_" + numFrames.ToString() + ".png");
            //frameRoi = binaryThresholdNiBlack(frameRoi);
            //frameRoi = binaryNiBlack(frameRoi); 
            //frameRoi.Save(path2 + numFrames.ToString() + "O.png");  

            using (MemStorage storage = new MemStorage())
            {
                Double result1 = 0;
                Double result2 = 0;

                Contour<System.Drawing.Point> contours = frameRoi.FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_LIST, storage);
                Contour<System.Drawing.Point> biggestContour = null;

                while (contours != null)
                {
                    result1 = contours.Area;
                    if (result1 > result2)
                    {
                        result2 = result1;
                        biggestContour = contours;
                    }
                    contours = contours.HNext;
                }

                if (biggestContour != null)
                {
                    Contour<System.Drawing.Point> concurrentContour = biggestContour.ApproxPoly(biggestContour.Perimeter * 0.0025, storage);
                    biggestContour = concurrentContour;

                    Hull = biggestContour.GetConvexHull(Emgu.CV.CvEnum.ORIENTATION.CV_COUNTER_CLOCKWISE);
                    defects = biggestContour.GetConvexityDefacts(storage, Emgu.CV.CvEnum.ORIENTATION.CV_COUNTER_CLOCKWISE);
                    defectsArray = defects.ToArray();

                    box = biggestContour.GetMinAreaRect();
                    points = box.GetVertices();

                    contourArea = result2;
                    contourPerimeter = biggestContour.Perimeter;
                    convexHullArea = Hull.Area;
                    convexHullPerimeter = Hull.Perimeter;

                    frameRoi.Draw(Hull, new Gray(155), 1);
                    frameRoi.Save(path3 + "ConvexHull_" + numFrames.ToString() + ".png");

                    ListReturn = GetFingers(frameRoi);
                    ListReturn.Add(contourPerimeter);
                    ListReturn.Add(contourArea);
                    ListReturn.Add(convexHullPerimeter);
                    ListReturn.Add(convexHullArea);
                }
            }

            //numFrames++;

            //return ListReturn;
        }//end HandConvexHull  


        private List<object> GetFingers(Image<Gray, Byte> HandSegmentation)
        {
            int fingerNum = 0;
            List<object> ListReturn = new List<object>(3); //This list has 
            PointF[] PointsMakeOalmCircle = new PointF[defectsArray.Length];
            PointF[] PositionFingerTips = new PointF[5];
            int[] anglesFingertipsCenter = new int[5];

            for (int i = 0; i < defects.Total; i++)
            {
                PointF startPoint = new PointF((float)defectsArray[i].StartPoint.X, (float)defectsArray[i].StartPoint.Y);
                PointF depthPoint = new PointF((float)defectsArray[i].DepthPoint.X, (float)defectsArray[i].DepthPoint.Y);
                PointF endPoint = new PointF((float)defectsArray[i].EndPoint.X, (float)defectsArray[i].EndPoint.Y);

                //LineSegment2D startDepthLine = new LineSegment2D(defectsArray[i].StartPoint, defectsArray[i].DepthPoint);
                //LineSegment2D depthEndLine = new LineSegment2D(defectsArray[i].DepthPoint, defectsArray[i].EndPoint);

                CircleF startCircle = new CircleF(startPoint, 5f);
                CircleF depthCircle = new CircleF(depthPoint, 5f);
                CircleF endCircle = new CircleF(endPoint, 5f);

                PointsMakeOalmCircle[i] = depthPoint;

                //Custom heuristic based on some experiment, double check it before use
                if ((startCircle.Center.Y < box.center.Y || depthCircle.Center.Y < box.center.Y) && (startCircle.Center.Y < depthCircle.Center.Y) && (Math.Sqrt(Math.Pow(startCircle.Center.X - depthCircle.Center.X, 2) + Math.Pow(startCircle.Center.Y - depthCircle.Center.Y, 2)) > box.size.Height / 6.5))
                {
                    fingerNum++; //Number of the fingers
                    PositionFingerTips[fingerNum - 1] = startPoint;
                }
            }


            CircleF circulito = Emgu.CV.PointCollection.MinEnclosingCircle(PointsMakeOalmCircle); //Circle, represent the palm of the hand
            PointF centro = circulito.Center;  //center of the hand, there is the center of the circle 

            for (int j = 0; j < 5; j++)
            {
                anglesFingertipsCenter[j] = getAngleCenterFinger(PositionFingerTips[j], centro);
            }

            ListReturn.Add(centro);
            ListReturn.Add(fingerNum);
            ListReturn.Add(anglesFingertipsCenter);

            return ListReturn;
        }//end get fingers  

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::


        private int getAngleCenterFinger(PointF fingerPosition, PointF centerHand)
        {
            float yOffset = centerHand.Y - fingerPosition.Y;
            float xOffset = centerHand.X - fingerPosition.X;

            double thethaRadias = Math.Atan2(yOffset, xOffset);
            double angle = thethaRadias * (180 / Math.PI);
            return (int)angle;
        }

        //::::::Method to save de media and the desviation standar, delate when the binarization its over. 

        private void saveStatictics(int frames, double Media, double stdes, double cv, double izq, double der)
        {
            using (StreamWriter file = new StreamWriter(path2 + "Statistics.txt", true))
            {
                file.Write(frames + " ");
                file.Write(Media.ToString() + " ");
                file.Write(stdes.ToString() + " ");
                file.Write(cv.ToString() + " ");
                file.Write(izq.ToString() + " ");
                file.Write(der.ToString() + " ");
                file.Write(Environment.NewLine);
            }
        }

        //end saveStatictics

    }//end class
}//end namespace

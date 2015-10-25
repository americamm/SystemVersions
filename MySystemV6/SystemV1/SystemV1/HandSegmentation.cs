using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.UI; 
using Emgu.CV.Structure;
using Emgu.Util;  
using System.Drawing;
using System.IO; 

namespace SystemV1
{
    public class HandSegmentation
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
        private string path1 = @"C:\SystemTest\V6\Test1\Front\Convex\";
        private string path2 = @"C:\SystemTest\V6\Test1\Side\Convex\";
        private int numFrames = 1; 
                    
        public int numero;
        //:::::::::::::::::fin variables::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::


        //:::::::::::::Method for make the image binary::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //the binarization is inspired in NiBlanck banarization, but in this case, we use just the average of the image. 
        //openinOperation() remove the noise of the binarized image, using morphological operation, we use opening. 

        private Image<Gray, Byte> binaryNiBlack(Image<Gray, Byte> handFrame)
        {
            int widthFrame = handFrame.Width;
            int heigthFrame = handFrame.Height;

            int sizeSW = 3;
            int sizeSW_w = sizeSW; //Size of the slinding window 
            int sizeSW_h = sizeSW; //Size of the slinding window 
            int halfWidth = (int)(Math.Floor((double)sizeSW / 2));
            int halfHeigth = (int)(Math.Floor((double)sizeSW / 2));
            int binaryWidth = widthFrame + halfWidth * 2;
            int binaryHeigth = heigthFrame + halfHeigth * 2;
            double k = 0.5;

            Image<Gray, Byte> binaryFrameCalculation = new Image<Gray, Byte>(binaryWidth, binaryHeigth);
            binaryFrameCalculation.SetZero();
            Rectangle roiHand = new Rectangle(halfWidth, halfHeigth, widthFrame, heigthFrame);
            binaryFrameCalculation.ROI = roiHand;
            handFrame.CopyTo(binaryFrameCalculation);
            binaryFrameCalculation.ROI = Rectangle.Empty;

            byte[, ,] byteData = handFrame.Data;

            for (int i = halfHeigth; i < heigthFrame + halfHeigth; i++)
            {
                for (int j = halfWidth; j < widthFrame + halfWidth; j++)
                {
                    Gray media;
                    MCvScalar desest;
                    MCvScalar mediaValue;
                    double threshold;
                    MCvBox2D roi;

                    Image<Gray, Byte> imageCalculate = new Image<Gray, Byte>(sizeSW_w, sizeSW_h);
                    roi = new MCvBox2D(new System.Drawing.Point(j, i), new System.Drawing.Size(sizeSW_w, sizeSW_h), 0);

                    imageCalculate = binaryFrameCalculation.Copy(roi);
                    binaryFrameCalculation.ROI = Rectangle.Empty;
                    imageCalculate.AvgSdv(out media, out desest);
                    mediaValue = media.MCvScalar;
                    threshold = mediaValue.v0 + (k * desest.v0);

                    if (byteData[i - halfHeigth, j - halfWidth, 0] < threshold)
                        byteData[i - halfHeigth, j - halfWidth, 0] = 255;
                    else
                        byteData[i - halfHeigth, j - halfWidth, 0] = 0;
                }
            }

            handFrame.Data = byteData;
            return handFrame;
        }


        private Image<Gray, Byte> openingOperation(Image<Gray, Byte> binaryFrame)
        {
            StructuringElementEx SElement;

            SElement = new StructuringElementEx(3, 7, 1, 1, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_RECT);

            binaryFrame._MorphologyEx(SElement, Emgu.CV.CvEnum.CV_MORPH_OP.CV_MOP_OPEN, 1);

            return binaryFrame; 
        } //end openingOperation()


        private Image<Gray, Byte> closeOperation(Image<Gray, Byte> binaryFrame)
        {
            StructuringElementEx SElement;

            SElement = new StructuringElementEx(7, 7, 1, 3, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_RECT);

            binaryFrame._MorphologyEx(SElement, Emgu.CV.CvEnum.CV_MORPH_OP.CV_MOP_CLOSE, 1);

            return binaryFrame; 
        }

        //::::::::::::Method to calculate the convex hull:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

        public List<object> HandConvexHull(Image<Gray, Byte> frame, Rectangle Roi)
        {
            List<object> ListReturn = new List<object>(); 
            Image<Gray, Byte> BinaryImage;
            //PointF centerPalm; 

            BinaryImage = frame.Copy(Roi);
            BinaryImage = openingOperation(BinaryImage);
            BinaryImage = closeOperation(BinaryImage);
            BinaryImage = binaryNiBlack(BinaryImage); 
            //naryImage = binaryThresholdNiBlack(BinaryImage);
            //BinaryImage.Save(path2 + numFrames.ToString() + ".png");  

            using (MemStorage storage = new MemStorage())
            {
                Double result1 = 0;
                Double result2 = 0;
                
                Contour<System.Drawing.Point> contours = BinaryImage.FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_LIST, storage);
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

                    BinaryImage.Draw(Hull, new Gray(155), 1);
                    BinaryImage.Save(path1 + "ConvexHull_" + numFrames.ToString() + ".png");

                    ListReturn = GetFingers(BinaryImage);
                    ListReturn.Add(contourPerimeter);
                    ListReturn.Add(contourArea);
                    ListReturn.Add(convexHullPerimeter);
                    ListReturn.Add(convexHullArea);
                }
            }
            
            numFrames++; 

            return ListReturn;
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
                //try 
                if ((startCircle.Center.Y < box.center.Y || depthCircle.Center.Y < box.center.Y) && (startCircle.Center.Y < depthCircle.Center.Y) && (Math.Sqrt(Math.Pow(startCircle.Center.X - depthCircle.Center.X, 2) + Math.Pow(startCircle.Center.Y - depthCircle.Center.Y, 2)) > box.size.Height / 6.5))
                {
                    fingerNum++; //Number of the fingers
                    PositionFingerTips[fingerNum - 1] = startPoint;
                    HandSegmentation.Draw(depthCircle, new Gray(82), 2);
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
            using (StreamWriter file = new StreamWriter(path2+"Statistics.txt", true))
            {
                file.Write(frames + " ");
                file.Write(Media.ToString() + " "); 
                file.Write(stdes.ToString() + " ");
                file.Write(cv.ToString() + " ");
                file.Write(izq.ToString() + " ");
                file.Write(der.ToString() + " "); 
                file.Write(Environment.NewLine); 
            }        
        }//end saveStatictics
        

    }//end class
}//end namespace

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
        private string path1 = @"C:\SystemTest\V8\Test2\Front\Convex\";
        private string path2 = @"C:\SystemTest\V8\Test2\";
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
            int binaryWidth = widthFrame + (halfWidth * 2);         //Size of tyhe image where the calculation is make. 
            int binaryHeigth = heigthFrame + (halfHeigth * 2);
            double k = 0;

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

        private Image<Gray, Byte> binarySauvola(Image<Gray, Byte> handFrame)
        {
            int widthFrame = handFrame.Width;
            int heigthFrame = handFrame.Height;

            int sizeSW = 100;
            int sizeSW_w = sizeSW; //Size of the slinding window 
            int sizeSW_h = sizeSW; //Size of the slinding window 
            int halfWidth = (int)(Math.Floor((double)sizeSW / 2));
            int halfHeigth = (int)(Math.Floor((double)sizeSW / 2));
            int binaryWidth = widthFrame + (halfWidth * 2);
            int binaryHeigth = heigthFrame + (halfHeigth * 2);
            double k = 0.009;
            double R = 128;


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
                    threshold = mediaValue.v0 * (1 + (k * ((desest.v0 / R) - 1)));

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

            SElement = new StructuringElementEx(3, 9, 1, 4, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_RECT);

            binaryFrame._MorphologyEx(SElement, Emgu.CV.CvEnum.CV_MORPH_OP.CV_MOP_OPEN, 1);

            return binaryFrame; 
        } //end openingOperation()


        private Image<Gray, Byte> closeOperation(Image<Gray, Byte> binaryFrame)
        {
            StructuringElementEx SElement;

            SElement = new StructuringElementEx(3, 11, 1, 5, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_RECT);

            binaryFrame._MorphologyEx(SElement, Emgu.CV.CvEnum.CV_MORPH_OP.CV_MOP_CLOSE, 1);

            return binaryFrame; 
        }

        //::::::::::::Method to calculate the convex hull:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

        public List<object> HandConvexHull(Image<Gray, Byte> frame, Rectangle Roi)
        {
            List<object> ListReturn = new List<object>(); 
            Image<Gray, Byte> BinaryImage;
            Image<Gray,Byte> frameImagePtr;  

            BinaryImage = frame.Copy(Roi);
            //BinaryImage.Save(path1 + numFrames.ToString() + ".png");

            frameImagePtr= BinaryImage; 

            IntPtr framePtr = frameImagePtr.Ptr;
            IntPtr binaryPrt = BinaryImage.Ptr; 
            
            CvInvoke.cvThreshold(framePtr, binaryPrt, 0, 255, Emgu.CV.CvEnum.THRESH.CV_THRESH_OTSU);
            BinaryImage.Save(path1 + numFrames.ToString() + "B_Otsu.png");

            BinaryImage = openingOperation(BinaryImage);
            BinaryImage = closeOperation(BinaryImage);
            //BinaryImage.Save(path1 + numFrames.ToString() + "B_OC.png");


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

                    CircleF centerBox = box.center(); 

                    contourArea = result2;
                    contourPerimeter = biggestContour.Perimeter;
                    convexHullArea = Hull.Area;
                    convexHullPerimeter = Hull.Perimeter;

                    BinaryImage.Draw(Hull, new Gray(155), 1);
                    BinaryImage.Draw(box, new Gray(100), 1);
                    BinaryImage.Draw(box.center(), new Gray(200), 1); 
                    BinaryImage.Save(path1 + "ConvexHull_" + numFrames.ToString() + ".png");

                    ListReturn = GetFingersHand(BinaryImage);
                    ListReturn.Add(contourPerimeter);
                    ListReturn.Add(contourArea);
                    ListReturn.Add(convexHullPerimeter);
                    ListReturn.Add(convexHullArea);
                }
            }
            
            numFrames++; 

            return ListReturn;
        }//end HandConvexHull  


        private List<object> GetFingersHand(Image<Gray, Byte> HandSegmentation)
        {
            int fingerNum = 0;
            PointF[] startPoints = new PointF[defectsArray.Length];
            PointF[] depthPoints = new PointF[defectsArray.Length];
            Double[] DistanceDepth = new Double[defectsArray.Length];
            List<object> ListReturn = new List<object>(3); //This list has    
            PointF[] PointsMakeOalmCircle = new PointF[defectsArray.Length];
            PointF[] PositionFingerTips = new PointF[5];
            int[] anglesFingertipsCenter = new int[5];


            for (int i = 0; i < defects.Total; i++)
            {
                startPoints[i] = new PointF((float)defectsArray[i].StartPoint.X, (float)defectsArray[i].StartPoint.Y);
                depthPoints[i] = new PointF((float)defectsArray[i].DepthPoint.X, (float)defectsArray[i].DepthPoint.Y);

                DistanceDepth[i] = defectsArray[i].Depth;

                CircleF startCircle = new CircleF(startPoints[i], 5f);
                CircleF depthCircle = new CircleF(depthPoints[i], 5f);
                HandSegmentation.Draw(startCircle, new Gray(60), 2);
                HandSegmentation.Draw(depthCircle, new Gray(60), 2);
                //HandSegmentation.Save(path3 + "Dedos_Start" + numFrames.ToString() + ".png");
            }

            int elements = DistanceDepth.Length;

            for (int i = 0; i < elements; i++)
            {
                Double minDistance = 20;
                Double maxAngle = 60;
                Double angle;
                int antecesor;
                int sucesor;

                if (DistanceDepth[i] < minDistance)
                    continue;

                if (i == 0)
                    antecesor = elements - 1;
                else
                    antecesor = i - 1;

                if (i == (elements - 1))
                    sucesor = 0;
                else
                    sucesor = i + 1;

                angle = getAngleBetweenStart(startPoints[i], depthPoints[antecesor], depthPoints[sucesor]);

                if (angle >= maxAngle)
                    continue;

                fingerNum++;

                CircleF startCircle = new CircleF(startPoints[i], 3f);
                CircleF depthCircle = new CircleF(depthPoints[i], 3f);
                HandSegmentation.Draw(startCircle, new Gray(120), 2);
                HandSegmentation.Draw(depthCircle, new Gray(120), 2);
                HandSegmentation.Save(path1 + numFrames.ToString() + "Dedos"  + ".png"); 
            }

            CircleF circulito = Emgu.CV.PointCollection.MinEnclosingCircle(depthPoints);
            PointF centro = circulito.Center; 
            //HandSegmentation.Save(path3 + "Dedos" + numFrames.ToString() + ".png");
            using (StreamWriter file = new StreamWriter(path1 + "Dedos.txt", true))
            {
                file.Write(numFrames.ToString() + " ");
                file.Write(fingerNum.ToString() + " ");
                file.Write(Environment.NewLine);
            } 

            for (int j = 0; j < 5; j++)
            {
                anglesFingertipsCenter[j] = getAngleCenterFinger(PositionFingerTips[j], centro);
            }

            ListReturn.Add(centro);
            ListReturn.Add(fingerNum);
            ListReturn.Add(anglesFingertipsCenter);

            return ListReturn;
        }//end GetFingersHand

        //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

        private double getAngleBetweenStart(PointF Pi, PointF Pia, PointF Pis)
        {
            Double angle;
            Double slopeAntecesor;
            Double slopeSucesor;

            slopeAntecesor = slope(Pia, Pi);
            slopeSucesor = slope(Pis, Pi);

            angle = Math.Abs(((slopeSucesor - slopeAntecesor) * 180) / Math.PI);
            //angle= Math.
            return angle;
        }//end  getAngleBetweenStart 


        private double slope(PointF p1, PointF p2)
        {
            double slope;

            slope = Math.Atan2(p1.Y - p2.Y, p1.X - p2.X);

            return slope;
        }//end slope  


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

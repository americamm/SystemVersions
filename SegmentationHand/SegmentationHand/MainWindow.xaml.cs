using System;
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
        private string path1 = @"C:\CaptureKinect\Binarization\test19\frames\";
        private string path2 = @"C:\CaptureKinect\Binarization\test19\binary\";
        private string path3 = @"C:\CaptureKinect\Binarization\test19\convex\";
        private string path4 = @"C:\CaptureKinect\Binarization\test19\opening\";
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
            Image<Gray, Byte> openingImage; 

            for (int i = 1; i < 66; i++)
            {
                frameRoi = new Image<Gray, Byte>(path1 + i.ToString() + ".png");

                openingImage = openingOperation(frameRoi);
                frameRoi.Save(path4 + numFrames.ToString() + "_O.png");
                //frameRoi._Erode(2);
                //openingImage = frameRoi;   
                //frameRoi.Save(path4 + numFrames.ToString() + "E_.png"); 

                openingImage = closeOperation(openingImage);
                //frameRoi.Save(path4 + numFrames.ToString() + "_C.png"); 

                openingImage = binarySauvola(openingImage);
                //openingImage.Save(path2 + numFrames.ToString() + ".png");

                //StructuringElementEx SElement;
                //SElement = new StructuringElementEx(3,3, 1, 1, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_RECT);

                //openingImage = openingImage.SmoothMedian(3);
                //openingImage = openingImage.SmoothMedian(3);
                //openingImage._Dilate(1);
                //openingImage._MorphologyEx(SElement, CV_MORPH_OP.CV_MOP_CLOSE, 1);
                //openingImage.Save(path2 + numFrames.ToString() + "_MF" + ".png");

                HandConvexHull(openingImage); 
                
                //frameRoi = binaryNiBlack(frameRoi);
                //frameRoi.Save(path2 + numFrames.ToString() + ".png"); 

                //HandConvexHull(frameRoi); 
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
            double k = 1;


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

        private Image<Gray, Byte> binarySauvola(Image<Gray, Byte> handFrame)
        {
            int widthFrame = handFrame.Width;
            int heigthFrame = handFrame.Height;

            int sizeSW = 3;
            int sizeSW_w = sizeSW; //Size of the slinding window 
            int sizeSW_h = sizeSW; //Size of the slinding window 
            int halfWidth = (int)(Math.Floor((double)sizeSW / 2));
            int halfHeigth = (int)(Math.Floor((double)sizeSW / 2));
            int binaryWidth = widthFrame + (halfWidth * 2);
            int binaryHeigth = heigthFrame + (halfHeigth * 2);
            double k = .3;
            double R = 256; 


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
                    threshold = mediaValue.v0 *( 1 + ( k*((desest.v0/R) - 1)) );

                    if (byteData[i - halfHeigth, j - halfWidth, 0] < threshold)
                        byteData[i - halfHeigth, j - halfWidth, 0] = 255;
                    else
                        byteData[i - halfHeigth, j - halfWidth, 0] = 0;
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

            handImage = handImage.InRange(new Gray(Rizq), new Gray(Rder));
            //handImage.Save(path2 + numFrames.ToString() + "B.png");

            handImage = openingOperation(handImage);
            //handImage.Save(path2 + numFrames.ToString() + "_O.png");

            return handImage;
        }//end BinaryThresholdNiBlack  


        private Image<Gray, Byte> openingOperation(Image<Gray, Byte> binaryFrame)
        {
            StructuringElementEx SElement;

            SElement = new StructuringElementEx(3,7, 1, 1, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_RECT);

            binaryFrame._MorphologyEx(SElement, Emgu.CV.CvEnum.CV_MORPH_OP.CV_MOP_OPEN, 1);

            return binaryFrame;
        } //end openingOperation()


        private Image<Gray, Byte> closeOperation(Image<Gray, Byte> binaryFrame)
        {
            StructuringElementEx SElement;

            SElement = new StructuringElementEx(7,7, 1, 3, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_RECT);

            binaryFrame._MorphologyEx(SElement, Emgu.CV.CvEnum.CV_MORPH_OP.CV_MOP_CLOSE, 1);

            return binaryFrame;
        }

        //::::::::::::Method to calculate the convex hull:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

        public void HandConvexHull(Image<Gray, Byte> frameRoi)
        {
            List<object> ListReturn = new List<object>();
            //PointF centerPalm; 


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

                    //frameRoi.Draw(Hull, new Gray(155), 1);
                    //frameRoi.Draw(biggestContour, new Gray(100), 1);
                    //frameRoi.Save(path3 + "ConvexHull_" + numFrames.ToString() + ".png");

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
            int numDefects = defectsArray.Length; 
            List<object> ListReturn = new List<object>(3); //This list has 
            PointF[] PositionDepth = new PointF[numDefects];
            PointF[] PositionFingerTips = new PointF[5];
            PointF[] PositionRootFinger = new PointF[5]; 
            int[] anglesFingertipsCenter = new int[5]; 
            int[] indexDepth = new int[5];
            PointF[] positiondepthmid = new PointF[12];
   

            for (int i = 0; i < defects.Total; i++)
            {
                PointF startPoint = new PointF((float)defectsArray[i].StartPoint.X, (float)defectsArray[i].StartPoint.Y);
                PointF depthPoint = new PointF((float)defectsArray[i].DepthPoint.X, (float)defectsArray[i].DepthPoint.Y);
                PointF endPoint = new PointF((float)defectsArray[i].EndPoint.X, (float)defectsArray[i].EndPoint.Y);

                //LineSegment2D startDepthLine = new LineSegment2D(defectsArray[i].StartPoint, defectsArray[i].DepthPoint);
                //LineSegment2D depthEndLine = new LineSegment2D(defectsArray[i].DepthPoint, defectsArray[i].EndPoint);

                CircleF startCircle = new CircleF(startPoint, 3f);
                CircleF depthCircle = new CircleF(depthPoint, 3f);
                CircleF endCircle = new CircleF(endPoint, 3f);

                HandSegmentation.Draw(startCircle, new Gray(40), 4);

                PositionDepth[i] = depthPoint;

                //Custom heuristic based on some experiment, double check it before use
                if ((startCircle.Center.Y < box.center.Y || depthCircle.Center.Y < box.center.Y) && (startCircle.Center.Y < depthCircle.Center.Y) && (Math.Sqrt(Math.Pow(startCircle.Center.X - depthCircle.Center.X, 2) + Math.Pow(startCircle.Center.Y - depthCircle.Center.Y, 2)) > box.size.Height / 6.5))
                { 
                    fingerNum++; //Number of the fingers
                    PositionFingerTips[fingerNum - 1] = startPoint;
                    indexDepth[fingerNum - 1] = i; 
                    HandSegmentation.Draw(depthCircle, new Gray(110), 2);
                    HandSegmentation.Draw(startCircle, new Gray(80), 2);
                    positiondepthmid[fingerNum - 1] = depthPoint; 
                }  
                
            }
 
            using (StreamWriter file = new StreamWriter(path3 + "Dedos.txt", true))
            {
                file.Write(numFrames.ToString() + " ");
                file.Write(fingerNum.ToString() + " ");
                file.Write(Environment.NewLine); 
            } 

            
            HandSegmentation.Draw(box, new Gray(50), 2); 

            CircleF centerBoxDraw = new CircleF(box.center, 4f);
            HandSegmentation.Draw(centerBoxDraw, new Gray(100), 2);
            

            if (fingerNum > 0)
            {  
                int indexActual; 
                int indexPrevous; 
                //Get the features of the fingers:::::::::::::::::::::::::::::::::::::::::::::::::::::::
                for (int j = 0; j < fingerNum; j++)
                {  
                    //Get the angles to the center of the hand::::::::::::::::::::::::::::::::::::::::::
                    anglesFingertipsCenter[j] = getAngleCenterFinger(PositionFingerTips[j], box.center); 

                    //Get the root of the fingers:::::::::::::::::::::::::::
                    if (j == 0 && indexDepth[j] == 0) 
                    {
                        indexActual = 0;
                        indexPrevous = numDefects - 1;
                    }
                    else
                    {
                        indexActual = indexDepth[j];
                        indexPrevous = indexActual - 1;
                    } 

                    PositionRootFinger[j] = getFingerRoot(PositionDepth[indexActual], PositionDepth[indexPrevous]);

                    CircleF midPoint = new CircleF(PositionRootFinger[j], 3f);
                    HandSegmentation.Draw(midPoint, new Gray(60), 2); 
                }
            }

            ListReturn.Add(box.center);
            ListReturn.Add(fingerNum);
            ListReturn.Add(anglesFingertipsCenter);
            
            HandSegmentation.Save(path3 + "Dedos_" + numFrames.ToString() + ".png");
            
            return ListReturn;
        }//end get fingers  

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

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
                HandSegmentation.Save(path3 + "Dedos_Start" + numFrames.ToString() + ".png"); 
            } 

            int elements = DistanceDepth.Length; 
            
            for (int i = 0; i < elements; i++)
            {
                Double minDistance = 18;
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
            }

            HandSegmentation.Save(path3 + "Dedos" + numFrames.ToString()+".png");

            using (StreamWriter file = new StreamWriter(path3 + "Dedos.txt", true))
            {
                file.Write(numFrames.ToString() + " ");
                file.Write(fingerNum.ToString() + " ");
                file.Write(Environment.NewLine);
            }

            return ListReturn; 
        }//end GetFingersHand

        private double getAngleBetweenStart(PointF Pi, PointF Pia, PointF Pis)
        {  
            Double angle;  
            Double slopeAntecesor; 
            Double slopeSucesor;

            slopeAntecesor = slope(Pia, Pi);
            slopeSucesor = slope(Pis, Pi);

            angle = Math.Abs(((slopeSucesor - slopeAntecesor)* 180) / Math.PI);  
            //angle= Math.
            return angle;  
        }//end  getAngleBetweenStart 

        private double slope(PointF  p1, PointF p2)
        {
            double slope;

            slope = Math.Atan2(p1.Y - p2.Y, p1.X - p2.X); 

            return slope; 
        }//end slope  

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

        private int getAngleCenterFinger(PointF fingerPosition, PointF centerHand)
        {   
            float yOffset = Math.Abs( fingerPosition.Y - centerHand.Y);
            float xOffset = Math.Abs( fingerPosition.X- centerHand.X);
              
            double thethaRadias = Math.Atan2(yOffset, xOffset);
            double angle = 90 - (thethaRadias * (180 / Math.PI));
            return (int)angle;
        }

        private PointF getFingerRoot(PointF depthActual, PointF depthPrevious)
        {
            PointF root = new PointF();

            root.X = (depthActual.X + depthPrevious.X) / 2;
            root.Y = (depthActual.Y + depthPrevious.Y) / 2;

            return root;
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

﻿using System;
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

        //Save the frames to check the noise remove in the roi, also check the bnarization 
        private string path1 = @"C:\Gestures\90L\G1\Convex\";
        
        private int numFrames = 1;            
        public int numero;
        //:::::::::::::::::fin variables::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::


        //:::::::::::::Morphological operations ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

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

            //---------------Binarization using Otsu algortihm------------------------------------- 
            frameImagePtr= BinaryImage; 

            IntPtr framePtr = frameImagePtr.Ptr;
            IntPtr binaryPrt = BinaryImage.Ptr; 
            
            CvInvoke.cvThreshold(framePtr, binaryPrt, 0, 255, Emgu.CV.CvEnum.THRESH.CV_THRESH_OTSU);
            BinaryImage.Save(path1 + numFrames.ToString() + "B_Otsu.png");
            //--------------------------------------------------------------------------------------
            
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

                    BinaryImage.Draw(Hull, new Gray(155), 1);
                    BinaryImage.Draw(box, new Gray(100), 1);
                    BinaryImage.Save(path1 + "ConvexHull_" + numFrames.ToString() + ".png");

                    ListReturn = GetFingers(BinaryImage);  
                }
            }//end using
            
            numFrames++; 

            return ListReturn;
        }//end HandConvexHull  


        private List<object> GetFingers(Image<Gray, Byte> HandSegmentation)
        {
            int fingerNum = 0;
            int numDefects = defectsArray.Length; 
            List<object> ListReturn = new List<object>(5); //This list has th features
            PointF[] PositionDepth = new PointF[numDefects];
            PointF[] PositionFingerTips = new PointF[5];
            PointF[] PositionRootFinger = new PointF[5]; 
            double[] distanceCenterRoots = new double[5]; 
            int[] anglesFingerTipsCenter = new int[5];
            int[] anglesFingerRootCenter = new int[5];
            int[] indexDepth = new int[5];
            Rectangle PalmArea;
            float cPalmX;
            float cPalmY;
            PointF cPalm; 

            int indexActual;
            int indexPrevous;
            //------------------------------------------------------

            for (int i = 0; i < defects.Total; i++)
            {                
                PointF startPoint = new PointF((float)defectsArray[i].StartPoint.X, (float)defectsArray[i].StartPoint.Y);
                PointF depthPoint = new PointF((float)defectsArray[i].DepthPoint.X, (float)defectsArray[i].DepthPoint.Y);
                PointF endPoint = new PointF((float)defectsArray[i].EndPoint.X, (float)defectsArray[i].EndPoint.Y);

                CircleF startCircle = new CircleF(startPoint, 3f);
                CircleF depthCircle = new CircleF(depthPoint, 3f);
                CircleF endCircle = new CircleF(endPoint, 3f);

                PositionDepth[i] = depthPoint;

                //Count Fingers raised
                if ((startCircle.Center.Y < box.center.Y || depthCircle.Center.Y < box.center.Y) && (startCircle.Center.Y < depthCircle.Center.Y) && (Math.Sqrt(Math.Pow(startCircle.Center.X - depthCircle.Center.X, 2) + Math.Pow(startCircle.Center.Y - depthCircle.Center.Y, 2)) > (box.size.Height / 6.5))) 
                {
                    fingerNum++; //Number of the fingers
                    
                    PositionFingerTips[fingerNum - 1] = startPoint;
                    indexDepth[fingerNum - 1] = i;
                    
                    HandSegmentation.Draw(startCircle, new Gray(82), 2); 
                    HandSegmentation.Draw(depthCircle, new Gray(82), 2); 
                }
            }

            //Get tha palm area with the depth points
            PalmArea = Emgu.CV.PointCollection.BoundingRectangle(PositionDepth);
            cPalmX = PalmArea.X + (PalmArea.Width / 2);
            cPalmY = PalmArea.Y + (PalmArea.Height / 2);
            cPalm = new PointF(cPalmX, cPalmY);

            if (fingerNum > 0)
            {
                //::Get the features of the fingers
                for (int j = 0; j < fingerNum; j++)
                {
                    //:::::Get the angles to the center of the hand
                    anglesFingerTipsCenter[j] = getAngleCenterFinger(PositionFingerTips[j], cPalm);

                    //:::Get the root of the fingers
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
                    anglesFingerRootCenter[j] = getAngleCenterFinger(PositionRootFinger[j], cPalm);
                    distanceCenterRoots[j] = DistanceCenterPalmRoots(PositionRootFinger[j], cPalm, PalmArea.Height); 
                }
            }
            else
            {
                for (int j = 0; j < numDefects && j < 5; j++)
                {
                    //:::Get the root of the fingers
                    if (j == 0)
                    {
                        indexActual = 0;
                        indexPrevous = numDefects - 1;
                    }
                    else
                    {
                        indexActual = j;
                        indexPrevous = indexActual - 1;
                    }

                    PositionRootFinger[j] = getFingerRoot(PositionDepth[indexActual], PositionDepth[indexPrevous]);
                    anglesFingerRootCenter[j] = getAngleCenterFinger(PositionRootFinger[j], cPalm);
                    distanceCenterRoots[j] = DistanceCenterPalmRoots(PositionRootFinger[j], cPalm, PalmArea.Height); 
                }
            }

           
            CircleF centerP = new CircleF(cPalm, 2f);
            HandSegmentation.Draw(centerP, new Gray(30), 2);
            HandSegmentation.Draw(PalmArea, new Gray(10), 2); 

            HandSegmentation.Save(path1 + numFrames.ToString() + "_Dedos.png");
            
            ListReturn.Add(cPalm);
            ListReturn.Add(fingerNum);
            ListReturn.Add(anglesFingerTipsCenter);
            ListReturn.Add(anglesFingerRootCenter);
            ListReturn.Add(distanceCenterRoots); 

            using (StreamWriter file = new StreamWriter(path1 + "Dedos.txt", true))
            {
                file.Write(numFrames.ToString() + " ");
                file.Write(fingerNum.ToString() + " ");
                file.Write(Environment.NewLine);
            } 

            return ListReturn;
        }//end get fingers  

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::: 
        //::::Angle to the line of teh fingertip to teh center of the hand::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

        private int getAngleCenterFinger(PointF fingerPosition, PointF centerHand)
        {
            float yOffset = Math.Abs(fingerPosition.Y - centerHand.Y);
            float xOffset = Math.Abs(fingerPosition.X - centerHand.X);

            double thethaRadias = Math.Atan2(yOffset, xOffset);
            double angle = 90 - (thethaRadias * (180 / Math.PI));
            return (int)angle;
        }

        //::::Get the root of the fingers:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

        private PointF getFingerRoot(PointF depthActual, PointF depthPrevious)
        {
            PointF root = new PointF();

            root.X = (depthActual.X + depthPrevious.X) / 2;
            root.Y = (depthActual.Y + depthPrevious.Y) / 2;

            return root;
        }

        //::::Distance to the center box to depth:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

        private double DistanceCenterPalmRoots(PointF center, PointF depth, double heigthPalm)
        {
            double distance; 

            distance = Math.Sqrt(Math.Pow(center.X - depth.X, 2) + Math.Pow(center.Y - depth.Y, 2));

            return distance/heigthPalm; 
        }




        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

 

    }//end class
}//end namespace

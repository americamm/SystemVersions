﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO; 
using Emgu.CV; 
using System.Drawing; 

namespace SystemV1
{ 
    public class SaveFeaturesTxt 
    { 
        //::::::::Variables:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

        //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::


        public void SaveFeaturesTraining(string assignedClass, List<object> Features, string path) 
        {  
            //Cast every element to his time of data; 
            int Fingers = (int) Features[1];
            int[] ArrayAngles = (int[]) Features[2];
            double contourPerimeter = (double) Features[3];
            double contourArea = (double)Features[4];
            double convexhullArea = (double)Features[5];
            double convexhullPerimeter = (double)Features[6];
           

            using (StreamWriter file = new StreamWriter(path, true))
            {
                file.Write(assignedClass + " ");
                file.Write("1" + ":" + Fingers + " ");
                
                int featureNumber=2; // 

                //write the angles;  
                foreach (int element in ArrayAngles)
                {
                    file.Write(featureNumber.ToString() + ":" + element.ToString() + " ");
                    featureNumber++; 
                } 

                file.Write(featureNumber.ToString() + ":" + contourPerimeter.ToString() + " ");
                featureNumber++;

                file.Write(featureNumber.ToString() + ":" + contourArea.ToString() + " ");
                featureNumber++;

                file.Write(featureNumber.ToString() + ":" + convexhullArea.ToString() + " ");
                featureNumber++;

                file.Write(featureNumber.ToString() + ":" + convexhullPerimeter.ToString() + " ");
                featureNumber++;
                
                file.Write(Environment.NewLine); 
            }
        }


        public void SaveFeaturesTest(int numFrames, List<object> Features, string path)
        {
            //Cast every element to his time of data; 
            int Fingers = (int)Features[1];
            int[] ArrayAngles = (int[])Features[2];
            double contourPerimeter = (double)Features[3];
            double contourArea = (double)Features[4];
            double convexhullArea = (double)Features[5];
            double convexhullPerimeter = (double)Features[6];


            using (StreamWriter file = new StreamWriter(path, true))
            {
                file.Write(numFrames.ToString() + " ");
                file.Write("1" + ":" + Fingers + " ");

                int featureNumber = 2; // 

                //write the angles;  
                foreach (int element in ArrayAngles)
                {
                    file.Write(featureNumber.ToString() + ":" + element.ToString() + " ");
                    featureNumber++;
                }

                file.Write(featureNumber.ToString() + ":" + contourPerimeter.ToString() + " ");
                featureNumber++;

                file.Write(featureNumber.ToString() + ":" + contourArea.ToString() + " ");
                featureNumber++;

                file.Write(featureNumber.ToString() + ":" + convexhullArea.ToString() + " ");
                featureNumber++;

                file.Write(featureNumber.ToString() + ":" + convexhullPerimeter.ToString() + " ");
                featureNumber++;
                 
                file.Write(Environment.NewLine);
            }
        }


        public void FeaturesTwoKinects(string assignedClass, List<object> FrontK, List<object> SideK, string path)
        { 
            //cast the FrontK
            int FingersFrontK = (int)FrontK[1];
            int[] ArrayAnglesFrontK = (int[])FrontK[2];
            double contourPerimeterFrontK = (double)FrontK[3];
            double contourAreaFrontK = (double)FrontK[4];
            double convexhullAreaFrontK = (double)FrontK[5];
            double convexhullPerimeterFrontK = (double)FrontK[6]; 
            //cast the SideK
            int FingersSideK = (int)SideK[1];
            int[] ArrayAnglesSideK = (int[])SideK[2];
            double contourPerimeterSideK = (double)SideK[3];
            double contourAreaSideK = (double)SideK[4];
            double convexhullAreaSideK = (double)SideK[5];
            double convexhullPerimeterSideK = (double)SideK[6];

            using (StreamWriter file = new StreamWriter(path, true))
            {
                file.Write(assignedClass + " ");
                file.Write("1" + ":" + FingersFrontK + " ");

                int featureNumber = 2; // 

                //write the angles;  
                foreach (int element in ArrayAnglesFrontK)
                {
                    file.Write(featureNumber.ToString() + ":" + element.ToString() + " ");
                    featureNumber++;
                }

                file.Write(featureNumber.ToString() + ":" + contourPerimeterFrontK.ToString() + " ");
                featureNumber++;

                file.Write(featureNumber.ToString() + ":" + contourAreaFrontK.ToString() + " ");
                featureNumber++;

                file.Write(featureNumber.ToString() + ":" + convexhullAreaFrontK.ToString() + " ");
                featureNumber++;

                file.Write(featureNumber.ToString() + ":" + convexhullPerimeterFrontK.ToString() + " ");
                featureNumber++;

                file.Write(featureNumber.ToString() + ":" + FingersSideK + " ");
                featureNumber++;

                //write the angles;  
                foreach (int element in ArrayAnglesSideK)
                {
                    file.Write(featureNumber.ToString() + ":" + element.ToString() + " ");
                    featureNumber++;
                }

                file.Write(featureNumber.ToString() + ":" + contourPerimeterSideK.ToString() + " ");
                featureNumber++;

                file.Write(featureNumber.ToString() + ":" + contourAreaSideK.ToString() + " ");
                featureNumber++;

                file.Write(featureNumber.ToString() + ":" + convexhullAreaSideK.ToString() + " ");
                featureNumber++;

                file.Write(featureNumber.ToString() + ":" + convexhullPerimeterSideK.ToString() + " ");
                featureNumber++;

                file.Write(Environment.NewLine);
            }

        }

        public void FeaturesTwoKinectTest(int numFrame, List<object> FrontK, List<object> SideK, string path)
        {
            //cast the FrontK
            int FingersFrontK = (int)FrontK[1];
            int[] ArrayAnglesFrontK = (int[])FrontK[2];
            double contourPerimeterFrontK = (double)FrontK[3];
            double contourAreaFrontK = (double)FrontK[4];
            double convexhullAreaFrontK = (double)FrontK[5];
            double convexhullPerimeterFrontK = (double)FrontK[6];
            //cast the SideK
            int FingersSideK = (int)SideK[1];
            int[] ArrayAnglesSideK = (int[])SideK[2];
            double contourPerimeterSideK = (double)SideK[3];
            double contourAreaSideK = (double)SideK[4];
            double convexhullAreaSideK = (double)SideK[5];
            double convexhullPerimeterSideK = (double)SideK[6];

            using (StreamWriter file = new StreamWriter(path, true))
            {
                file.Write(numFrame.ToString() + " ");
                file.Write("1" + ":" + FingersFrontK + " ");

                int featureNumber = 2; // 

                //write the angles;  
                foreach (int element in ArrayAnglesFrontK)
                {
                    file.Write(featureNumber.ToString() + ":" + element.ToString() + " ");
                    featureNumber++;
                }

                file.Write(featureNumber.ToString() + ":" + contourPerimeterFrontK.ToString() + " ");
                featureNumber++;

                file.Write(featureNumber.ToString() + ":" + contourAreaFrontK.ToString() + " ");
                featureNumber++;

                file.Write(featureNumber.ToString() + ":" + convexhullAreaFrontK.ToString() + " ");
                featureNumber++;

                file.Write(featureNumber.ToString() + ":" + convexhullPerimeterFrontK.ToString() + " ");
                featureNumber++;

                file.Write(featureNumber.ToString() + ":" + FingersSideK + " ");
                featureNumber++;

                //write the angles;  
                foreach (int element in ArrayAnglesSideK)
                {
                    file.Write(featureNumber.ToString() + ":" + element.ToString() + " ");
                    featureNumber++;
                }

                file.Write(featureNumber.ToString() + ":" + contourPerimeterSideK.ToString() + " ");
                featureNumber++;

                file.Write(featureNumber.ToString() + ":" + contourAreaSideK.ToString() + " ");
                featureNumber++;

                file.Write(featureNumber.ToString() + ":" + convexhullAreaSideK.ToString() + " ");
                featureNumber++;

                file.Write(featureNumber.ToString() + ":" + convexhullPerimeterSideK.ToString() + " ");
                featureNumber++;

                file.Write(Environment.NewLine);
            }
        } 

        //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //::::Intento:::::::::::::::::::::::::::::::::::::

        public void FeaturesTwoKTest(int numFrame, List<object> FrontK, List<object> SideK, string path)
        { 
            //Declare////////////////////////// 
            int featureNumber = 0; 

            int FingersFrontK = 0 ;
            int[] ArrayAnglesFrontK = new int[5];
            double contourPerimeterFrontK = 0;
            double contourAreaFrontK = 0;
            double convexhullAreaFrontK = 0;
            double convexhullPerimeterFrontK = 0;

            int FingersSideK =0;
            int[] ArrayAnglesSideK = new int[5];
            double contourPerimeterSideK = 0;
            double contourAreaSideK = 0;
            double convexhullAreaSideK = 0;
            double convexhullPerimeterSideK = 0;

            bool flagFront = false;
            bool flagSide = false; 
 
            ////////////////////////////////
            
            if (FrontK != null)
            {   
                //cast the FrontK
                FingersFrontK = (int)FrontK[1];
                ArrayAnglesFrontK = (int[])FrontK[2];
                contourPerimeterFrontK = (double)FrontK[3];
                contourAreaFrontK = (double)FrontK[4];
                convexhullAreaFrontK = (double)FrontK[5];
                convexhullPerimeterFrontK = (double)FrontK[6];

                flagFront = true; 
            }

            if (SideK != null)
            {
                //cast the SideK
                FingersSideK = (int)SideK[1];
                ArrayAnglesSideK = (int[])SideK[2];
                contourPerimeterSideK = (double)SideK[3];
                contourAreaSideK = (double)SideK[4];
                convexhullAreaSideK = (double)SideK[5];
                convexhullPerimeterSideK = (double)SideK[6];

                flagSide = true; 
            }


            using (StreamWriter file = new StreamWriter(path, true))
            {
                if (flagFront)
                {
                    file.Write(numFrame.ToString() + " ");
                    file.Write("1" + ":" + FingersFrontK + " ");

                    featureNumber = 2; // 

                    //write the angles;  
                    foreach (int element in ArrayAnglesFrontK)
                    {
                        file.Write(featureNumber.ToString() + ":" + element.ToString() + " ");
                        featureNumber++;
                    }

                    file.Write(featureNumber.ToString() + ":" + contourPerimeterFrontK.ToString() + " ");
                    featureNumber++;

                    file.Write(featureNumber.ToString() + ":" + contourAreaFrontK.ToString() + " ");
                    featureNumber++;

                    file.Write(featureNumber.ToString() + ":" + convexhullAreaFrontK.ToString() + " ");
                    featureNumber++;

                    file.Write(featureNumber.ToString() + ":" + convexhullPerimeterFrontK.ToString() + " ");
                    featureNumber++;
                }

                if (flagSide)
                {
                    if (flagFront != null)
                    {
                        featureNumber = 11; 
                    }

                    file.Write(featureNumber.ToString() + ":" + FingersSideK + " ");
                    featureNumber++;

                    //write the angles;  
                    foreach (int element in ArrayAnglesSideK)
                    {
                        file.Write(featureNumber.ToString() + ":" + element.ToString() + " ");
                        featureNumber++;
                    }

                    file.Write(featureNumber.ToString() + ":" + contourPerimeterSideK.ToString() + " ");
                    featureNumber++;

                    file.Write(featureNumber.ToString() + ":" + contourAreaSideK.ToString() + " ");
                    featureNumber++;

                    file.Write(featureNumber.ToString() + ":" + convexhullAreaSideK.ToString() + " ");
                    featureNumber++;

                    file.Write(featureNumber.ToString() + ":" + convexhullPerimeterSideK.ToString() + " ");
                    featureNumber++;
                }

                file.Write(Environment.NewLine);
            }
        }//end method

    }//end class
}//end namespace

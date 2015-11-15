using System;
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
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

        public void FeaturesTwoKTest(int numFrame, List<object> FrontK, List<object> SideK, string path)
        {   
            //Declare////////////////////////// 
            int featureNumber = 0; 

            int FingersFrontK = 0 ;
            int[] ArrayAnglesTipsFrontK = new int[5];
            int[] ArrayAnglesRootsFrontK = new int[5];
            double[] ArrayDistaceRootsFrontK = new double[5]; 

            int FingersSideK =0;
            int[] ArrayAnglesTipsSideK = new int[5];
            int[] ArrayAnglesRootsSideK = new int[5];
            double[] ArrayDistanceRootsSideK = new double[5]; 

            bool flagFront = false;
            bool flagSide = false; 
            ////////////////////////////////
            
            if (FrontK.Count != 0)
            {   
                //cast the FrontK
                FingersFrontK = (int)FrontK[1];
                ArrayAnglesTipsFrontK = (int[])FrontK[2];
                ArrayAnglesRootsFrontK = (int[])FrontK[3];
                ArrayDistaceRootsFrontK = (double[])FrontK[4];

                flagFront = true; 
            }

            if (SideK.Count != 0)
            {
                //cast the SideK
                FingersSideK = (int)SideK[1];
                ArrayAnglesTipsSideK = (int[])SideK[2];
                ArrayAnglesRootsSideK = (int[])SideK[3];
                ArrayDistanceRootsSideK = (double[])SideK[4];

                flagSide = true; 
            }


            using (StreamWriter file = new StreamWriter(path, true))
            {
                file.Write(numFrame.ToString() + " ");
                
                if (flagFront)
                {
                    
                    file.Write("1" + ":" + FingersFrontK + " ");

                    featureNumber = 2; // 

                    //write the angles tips;  
                    foreach (int element in ArrayAnglesTipsFrontK)
                    {
                        file.Write(featureNumber.ToString() + ":" + element.ToString() + " ");
                        featureNumber++;
                    }

                    //write the angles roots;  
                    foreach (int element in ArrayAnglesRootsFrontK)
                    {
                        file.Write(featureNumber.ToString() + ":" + element.ToString() + " ");
                        featureNumber++;
                    }

                    //write the distance roots;  
                    foreach (double element in ArrayDistaceRootsFrontK)
                    {
                        file.Write(featureNumber.ToString() + ":" + element.ToString() + " ");
                        featureNumber++;
                    }
                }

                if (flagSide)
                {
                    if (flagFront != null)
                    {
                        featureNumber = 17; 
                    }

                    file.Write(featureNumber.ToString() + ":" + FingersSideK + " ");
                    featureNumber++;

                    //write the angles;  
                    foreach (int element in ArrayAnglesTipsSideK)
                    {
                        file.Write(featureNumber.ToString() + ":" + element.ToString() + " ");
                        featureNumber++;
                    }

                    //write the angles roots;  
                    foreach (int element in ArrayAnglesRootsSideK)
                    {
                        file.Write(featureNumber.ToString() + ":" + element.ToString() + " ");
                        featureNumber++;
                    }

                    //write the distance roots;  
                    foreach (double element in ArrayDistanceRootsSideK)
                    {
                        file.Write(featureNumber.ToString() + ":" + element.ToString() + " ");
                        featureNumber++;
                    }
                }

                file.Write(Environment.NewLine);
            }
        }//end method


        public void FeaturesOneTest(int numFrame, List<object> FrontK, string path)
        {
            //Declare////////////////////////// 
            int featureNumber = 0;

            int FingersFrontK = 0;
            int[] ArrayAnglesTipsFrontK = new int[5];
            int[] ArrayAnglesRootsFrontK = new int[5];
            double[] ArrayDistaceRootsFrontK = new double[5];

            bool flagFront = false;
            ////////////////////////////////

            if (FrontK.Count != 0)
            {
                //cast the FrontK
                FingersFrontK = (int)FrontK[1];
                ArrayAnglesTipsFrontK = (int[])FrontK[2];
                ArrayAnglesRootsFrontK = (int[])FrontK[3];
                ArrayDistaceRootsFrontK = (double[])FrontK[4];

                flagFront = true;
            }

            using (StreamWriter file = new StreamWriter(path, true))
            {
                file.Write(numFrame.ToString() + " ");

                if (flagFront)
                {

                    file.Write("1" + ":" + FingersFrontK + " ");

                    featureNumber = 2; // 

                    //write the angles tips;  
                    foreach (int element in ArrayAnglesTipsFrontK)
                    {
                        file.Write(featureNumber.ToString() + ":" + element.ToString() + " ");
                        featureNumber++;
                    }

                    //write the angles roots;  
                    foreach (int element in ArrayAnglesRootsFrontK)
                    {
                        file.Write(featureNumber.ToString() + ":" + element.ToString() + " ");
                        featureNumber++;
                    }

                    //write the distance roots;  
                    foreach (double element in ArrayDistaceRootsFrontK)
                    {
                        file.Write(featureNumber.ToString() + ":" + element.ToString() + " ");
                        featureNumber++;
                    }
                }

                file.Write(Environment.NewLine);
            }
        }//end method 

    }//end class
}//end namespace

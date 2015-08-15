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
        //::::::::Variables:::::::::::::::::::::::::::::::::::::::::
        //private string path= @"C:\MyRecognition\5to\WriteTest1.txt"; 
 
        //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::


        public void SaveFeaturesTraining(string assignedClass, List<object> Features, string path) 
        {  
            //Cast every element to his time of data; 
            int Fingers = (int) Features[1];
            int[] ArrayAngles = (int[]) Features[2];
            double contourArea = (double)Features[3];  
            double convexhullArea = (double)Features[4];
           

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

                file.Write(featureNumber.ToString() + ":" + contourArea.ToString() + " ");
                featureNumber++;

                file.Write(featureNumber.ToString() + ":" + convexhullArea.ToString() + " ");
                featureNumber++;
                
                file.Write(Environment.NewLine); 
            }
        }

        public void SaveFeaturesTest(List<object> Features, string path)
        {
            //Cast every element to his time of data; 
            int Fingers = (int)Features[1];
            int[] ArrayAngles = (int[])Features[2];
            double contourArea = (double)Features[3];
            double convexhullArea = (double)Features[4];


            using (StreamWriter file = new StreamWriter(path, true))
            {
                file.Write("1" + ":" + Fingers + " ");

                int featureNumber = 2; // 

                //write the angles;  
                foreach (int element in ArrayAngles)
                {
                    file.Write(featureNumber.ToString() + ":" + element.ToString() + " ");
                    featureNumber++;
                }

                file.Write(featureNumber.ToString() + ":" + contourArea.ToString() + " ");
                featureNumber++;

                file.Write(featureNumber.ToString() + ":" + convexhullArea.ToString() + " ");
                featureNumber++;

                file.Write(Environment.NewLine);
            }
        }

    }//end class
}//end namespace

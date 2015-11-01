using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing; 
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.Util; 



namespace SystemV1
{
    public class HandDetector
    { 
        //:::::::::::::::::Variables:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private CascadeClassifier haar;   
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::


        //::::::::::::Detection of the hand in a gray image::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        public List<Object> Detection(Image<Gray, Byte> frame)
        {   
            List<Object> listReturn = new List<object>(2);
            haar = new CascadeClassifier(@"C:\Users\America\Documents\MySystemV1\classifier\cascade.xml");
            

            if (frame != null)
            {
                System.Drawing.Rectangle[] hands = haar.DetectMultiScale(frame, 1.07, 2, new System.Drawing.Size(frame.Width / 8, frame.Height / 8), new System.Drawing.Size(frame.Width / 3, frame.Height / 3));

                //Draw the inflated teh rectagle and draw it. 
                for (int i = 0; i < hands.Length; i++)
                {
                    hands[i].Inflate(-15, 26);
                    //frame.Draw(hands[i], new Gray(255), 1);
                }

                //Check if the rois are intersected with others and then merge. 
                if (hands.Length > 1)
                { 
                    //Call the function to check the intersection 
                    hands = MergeRois(hands); 
                }

                if (hands.Count() == 0)
                {
                    Rectangle[] noDetection = new Rectangle[] { };
                    listReturn.Add(noDetection);
                }
                else
                {
                    for (int i = 0; i < hands.Length; i++)
                    {
                        frame.Draw(hands[i], new Gray(255), 1);
                    }
                    listReturn.Add(hands);
                }
                
            }

            listReturn.Add(frame);

            return listReturn;
            //Regresa los dos valores si el frame es diferente de null, lo cual se supone que siempre es cierto, por que eso se toma en cuenta desde data poll
        }//finaliza detection()   
        
        //::::::::Merge the intersected rois, when his intersection is more then  50% of the area::::::::::::::::::::::::::::::::::::::::::::::::::: 
        private System.Drawing.Rectangle[] MergeRois(System.Drawing.Rectangle[] detectedRois)
        {  
            int index = 0;
            System.Drawing.Rectangle[] returnMergeRois = new Rectangle[detectedRois.Length];  
            
            for (int i = 1; i < detectedRois.Length; i++)
            {
                if (detectedRois[i - 1].IntersectsWith(detectedRois[i]))
                {
                    double areaRect1; //area of the first rectangle 
                    double areaRect2; //area of the intecsection of the rectangles. 
                    Rectangle intersection;

                    intersection = Rectangle.Intersect(detectedRois[i - 1], detectedRois[i]); 
                    areaRect1 = detectedRois[i - 1].Width * detectedRois[i-1].Height;
                    areaRect2 = intersection.Width * intersection.Height; 
                    
                    if ((areaRect2/areaRect1) > 0.6)
                    {
                        if (index < 1) 
                            returnMergeRois[0] = Rectangle.Union(detectedRois[i - 1], detectedRois[i]);
                        
                        index++;
                    }
                } 
            }

            if (index == 0)
            {
                return detectedRois; 
            }

            return returnMergeRois; 
        }//end MergeRois

    }//end class
}//end namespace


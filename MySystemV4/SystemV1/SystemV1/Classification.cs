using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO; 
using LibSVMsharp.Extensions;
using LibSVMsharp.Helpers;
using LibSVMsharp.Core; 
using LibSVMsharp; 


namespace SystemV1
{
    public class Classification
    {
        private SVMProblem testProblem;
        private SVMModel model;
        
        public void ClassifiGesture(string pathModel, string pathTest, string pathResult)
        { 
            testProblem = SVMProblemHelper.Load(pathTest);
            model = SVM.LoadModel(pathModel);

            double[] testResults = testProblem.Predict(model);

            using (StreamWriter file = new StreamWriter(pathResult, true))
            {
                foreach (double element in testResults)
                {
                    file.Write(element.ToString()); 
                }
            }
        }//end ClassifyGesture 

    }//end class
}//end namespace

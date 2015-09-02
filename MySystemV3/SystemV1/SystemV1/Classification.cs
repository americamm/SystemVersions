using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        
        public void ClassifiGesture(string pathModel, string pathTest)
        {
            model = SVM.LoadModel(pathModel);
            




         
            
            
        }




    }
}

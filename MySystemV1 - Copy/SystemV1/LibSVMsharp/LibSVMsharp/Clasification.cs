﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibSVMsharp.Helpers;
using LibSVMsharp.Extensions;
using LibSVMsharp.Core;
using System.Runtime.InteropServices;
using System.Threading.Tasks; 

namespace LibSVMsharp
{
   public class Clasification
    {
         private SVMProblem problem;
        private SVMProblem testProblem;
        private SVMParameter parameter;

        public Clasification()
        {
            problem = SVMProblemHelper.Load(@"C:\Training.txt");
            testProblem = SVMProblemHelper.Load(@"C:\Training.txt");
        }

        /*public Classification(string training, string test)
        {
            problem = SVMProblemHelper.Load(training);
            testProblem = SVMProblemHelper.Load(test);
        }*/

        public void ClasificationHand()
        {

            parameter = new SVMParameter();

            parameter.Type = SVMType.C_SVC;
            parameter.Kernel = SVMKernelType.RBF;
            parameter.C = 1;
            parameter.Gamma = 1;


           // System.Windows.MessageBox.Show("Algo real");
            SVMModel model = SVM.Train(problem, parameter);
            //System.Windows.MessageBox.Show("Oscar");

            double[] target;
            target = new double[testProblem.Length];
            int a = 3;
            for (int i = 0; i < testProblem.Length; i++)
                target[i] = SVM.Predict(model, testProblem.X[i]);

            double accuracy = SVMHelper.EvaluateClassificationProblem(testProblem, target);
        }
    }
    }


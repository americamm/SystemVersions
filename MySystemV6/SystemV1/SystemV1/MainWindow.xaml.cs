﻿using System;
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
using System.Windows.Shapes;
using Emgu.CV;
using Emgu.CV.UI; 
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.Util; 
using System.Drawing;
using System.IO;


namespace SystemV1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary> 
 
    public partial class MainWindow : Window
    {
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //----------Declare Class---------------------------
        public GetKinectData GettingKinectData;
        public HandDetector HandDetection;
        public HandSegmentation GettingSegmentationK1;
        public HandSegmentation GettingSegmentationK2;
        public SaveFeaturesTxt SaveFeaturesText;
        public Classification Classifier; 
        
        //----------Variables-------------------------------
        private int FrameWidth = 640;
        private int FrameHeigth = 480;

        private WriteableBitmap ImagenWriteablebitmap; 
        private Int32Rect WriteablebitmapRect;
        private int WriteablebitmapStride;

        private System.Drawing.Rectangle[] RectArrayPrev; 

        //para contar los cuadros por segundo 
        public int fps = 1;
        public int sec = 1;
        public int numFrames = 1;
        private int numFrameHandDetected = 1;
        //Escribir el archivito de las caracteristicas.
        private string pathFront = @"C:\SystemTest\V6\Test2\Front\Input\";
        private string pathSide = @"C:\SystemTest\V6\Test2\Side\Input\"; 
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        
        
        //::::::Constructor::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        public MainWindow()
        {
            InitializeComponent();
            //GettingKinectData = new GetKinectData();
            HandDetection = new HandDetector();
            GettingSegmentationK1 = new HandSegmentation();
            GettingSegmentationK2 = new HandSegmentation();
            SaveFeaturesText = new SaveFeaturesTxt();
            //Classifier = new Classification(); 
        } 
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::


        //::::::Call methods:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //GettingKinectData.FindKinect();  
            //Comentado hasta ver el tiempo que tarda esto. 
            //Classifier.ClassifiGesture(@"C:\SystemTest\test1\TrainingTest1\2Classes_Model1.txt", @"C:\SystemTest\test1\TrainingTest1\Front\Test1.txt", @"C:\SystemTest\test1\Results1.txt");
            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
        }
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::


        //:::::::Display the stuff:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            //---------------------------------------------------------------
            List<Object> returnHandDetectorFrontActual = new List<object>(2);
            List<Object> returnHandDetectorSideActual = new List<object>(2);
            System.Drawing.Rectangle[] RoiKinectFrontActual;
            System.Drawing.Rectangle[] RoiKinectSideActual;
            Image<Gray, Byte> imagenKinectGray1;
            Image<Gray, Byte> imagenKinectGray2;
            Image<Gray, Byte> imageRoi1 = new Image<Gray,Byte>(200, 200);
            Image<Gray, Byte> imageRoi2 = new Image<Gray, Byte>(200, 200);
            
            List<Object> returnGettingSegK1 = new List<object>(); 
            List<Object> returnGettingSegK2 = new List<object>(); 
            
            bool noPositiveFalses;
            bool flag_execute = false;
            bool FrameHandDetected = false;
            bool detectRoiFront = false;
            bool detectRoiSide = false;

            //to get the position of the hand 
            PointF centerHandFront; 
            PointF positionRoi1Front;  
            PointF positionCenterHandF = new PointF();     
            PointF centerHandSide; 
            PointF positionRoi1Side;  
            PointF positionCenterHandS = new PointF();
           
            //-------------------------------------------------------------------------------

            //-------------------------------------------------------------------------------
            //Get actual frame and the next frame. To delate the frames with positive falses.  
            imagenKinectGray1 = new Image<Gray, byte>(pathFront + "Front" + "_" + numFrames.ToString() + "_1_1" + ".png"); //Poner el folder
            imagenKinectGray2 = new Image<Gray, byte>(pathSide + "Side" + "_" + numFrames.ToString() + "_1_1" + ".png"); 

            returnHandDetectorFrontActual = HandDetection.Detection(imagenKinectGray1);
            returnHandDetectorSideActual = HandDetection.Detection(imagenKinectGray2);

            //Cast the return of each frame                                                      MODIFICAR
            RoiKinectFrontActual = (System.Drawing.Rectangle[])returnHandDetectorFrontActual[0]; //Poner todos los rectangulos que regresen
            RoiKinectSideActual = (System.Drawing.Rectangle[])returnHandDetectorSideActual[0];

            //Set the bool variables,
            if (RoiKinectFrontActual.Length != 0)
                detectRoiFront = true;
            if (RoiKinectSideActual.Length != 0)
                detectRoiSide = true; 

            //To compare two consecitives frames. 
            if ( (numFrames == 1) || (FrameHandDetected == false) ) 
            {   
                RectArrayPrev = RoiKinectFrontActual; 
            }

            //Guardar las imagenes para ver cuantas detecta   
            //{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{ 
            int numHsFront = RoiKinectFrontActual.Length;
            int numHsSide = RoiKinectSideActual.Length;

            imagenKinectGray1.Save(pathFront + "DFront_" + numFrames.ToString() + ".png");
            imagenKinectGray2.Save(pathSide + "DSide_" + numFrames.ToString() + ".png"); 

            switch (numHsFront)
            {
                case 0:
                    imagenKinectGray1.Save(pathFront + @"CeroH\" + numFrames.ToString() + "_" + sec.ToString() + "_" + fps.ToString() + ".png");
                    break;
                case 1:
                    imagenKinectGray1.Save(pathFront + @"OneH\" + numFrames.ToString() + "_" + sec.ToString() + "_" + fps.ToString() + ".png");
                    break;
                default:
                    imagenKinectGray1.Save(pathFront + @"TwoH\" + numFrames.ToString() + "_" + sec.ToString() + "_" + fps.ToString() + ".png");
                    break;
            }

            switch (numHsSide)
            {
                case 0:
                    imagenKinectGray2.Save(pathSide + @"CeroH\" + numFrames.ToString() + "_" + sec.ToString() + "_" + fps.ToString() + ".png");
                    break;
                case 1:
                    imagenKinectGray2.Save(pathSide + @"OneH\" + numFrames.ToString() + "_" + sec.ToString() + "_" + fps.ToString() + ".png");
                    break;
                default:
                    imagenKinectGray2.Save(pathSide + @"TwoH\" + numFrames.ToString() + "_" + sec.ToString() + "_" + fps.ToString() + ".png");
                    break;
            }
            //}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}


            if ( (detectRoiFront) || (detectRoiSide) )//Esto se realiza si el cuadro del kinect es detectado. 
            {
                if (detectRoiFront)
                {
                    returnGettingSegK1 = GettingSegmentationK1.HandConvexHull(imagenKinectGray1, RoiKinectFrontActual[0]);
                    
                    if (returnGettingSegK1 != null)
                    {
                        centerHandFront = (PointF)returnGettingSegK1[0];                //Meter en una funcion 

                        positionRoi1Front = RoiKinectFrontActual[0].Location;
                        positionCenterHandF.X = positionRoi1Front.X + centerHandFront.X;
                        positionCenterHandF.Y = positionRoi1Front.Y + centerHandFront.Y;

                        CircleF centrillo = new CircleF(positionCenterHandF, 5f);

                        imagenKinectGray1.Draw(centrillo, new Gray(150), 3);

                        //SaveFeaturesText.SaveFeaturesTraining("2", returnGettingSegK1, pathFront+"TrainingFront2.txt"); 
                        SaveFeaturesText.SaveFeaturesTest(numFrames, returnGettingSegK1, pathFront + "Test1Front.txt");
                    }  
                }

                if (detectRoiSide)
                {
                    returnGettingSegK2 = GettingSegmentationK2.HandConvexHull(imagenKinectGray2, RoiKinectSideActual[0]);

                    if (returnGettingSegK2 != null)
                    {
                        centerHandSide = (PointF)returnGettingSegK2[0];                //Meter en una funcion 

                        positionRoi1Side = RoiKinectSideActual[0].Location;
                        positionCenterHandS.X = positionRoi1Side.X + centerHandSide.X;
                        positionCenterHandS.Y = positionRoi1Side.Y + centerHandSide.Y;

                        CircleF centrillo = new CircleF(positionCenterHandS, 5f);

                        imagenKinectGray2.Draw(centrillo, new Gray(150), 3);

                        //SaveFeaturesText.SaveFeaturesTraining("2", returnGettingSegK1, pathFront+"TrainingFront2.txt"); 
                        SaveFeaturesText.SaveFeaturesTest(numFrames, returnGettingSegK2, pathSide + "Test1Side.txt");
                    } 
                }

                if ((RoiKinectFrontActual.Length == 1) && (RoiKinectSideActual.Length == 1)) //Para saber cuantas detecta al mismo tiempo
                {
                    //SaveFeaturesText.SaveFeaturesTraining("2", returnGettingSegK1, pathFront+"TrainingFront2.txt"); 
                    SaveFeaturesText.FeaturesTwoKinectTest(numFrames, returnGettingSegK1, returnGettingSegK2, pathFront + "Test1.txt"); 

                    //Save the images
                    imagenKinectGray1.Save(pathFront + "F_" + numFrames.ToString() + "_"+ ".png");
                    imagenKinectGray2.Save(pathSide + "S_" + numFrames.ToString() + "_"+ ".png");
                }
            } 

            DepthImageK1.Source = imagetoWriteablebitmap(imagenKinectGray1);
            DepthImageK2.Source = imagetoWriteablebitmap(imagenKinectGray2); 

            if (fps == 30)
            {
                fps = 1;
                sec++;
             }
            fps++;
            numFrames++; 

        } //end CompositionTarget 
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::


        //:::::::::::::Method to convert a byte[] of the gray image to a writeablebitmap:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private WriteableBitmap imagetoWriteablebitmap(Image<Gray, Byte> frameHand)
        {
            Image<Bgra, Byte> frameBGR = new Image<Bgra, Byte>(FrameWidth, FrameHeigth);
            byte[] imagenPixels = new byte[FrameWidth * FrameHeigth];

            this.ImagenWriteablebitmap = new WriteableBitmap(FrameWidth, FrameHeigth, 96, 96, PixelFormats.Bgr32, null);
            this.WriteablebitmapRect = new Int32Rect(0, 0, FrameWidth, FrameHeigth);
            this.WriteablebitmapStride = FrameWidth * 4;

            frameBGR = frameHand.Convert<Bgra, Byte>();
            imagenPixels = frameBGR.Bytes;

            ImagenWriteablebitmap.WritePixels(WriteablebitmapRect, imagenPixels, WriteablebitmapStride, 0);

            return ImagenWriteablebitmap;
        }  
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        

        //:::::::::::::::Stop the kinect stream::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            GettingKinectData.StopKinect();
        }
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
    }//end class
}// end namespace

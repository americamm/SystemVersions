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
using System.Windows.Shapes;
using Emgu.CV;
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
        private string pathFront = @"C:\SystemTest\test9\Front\";
        private string pathSide = @"C:\SystemTest\test9\Side\"; 
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        
        
        //::::::Constructor::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        public MainWindow()
        {
            InitializeComponent();
            GettingKinectData = new GetKinectData();
            HandDetection = new HandDetector();
            GettingSegmentationK1 = new HandSegmentation();
            GettingSegmentationK2 = new HandSegmentation();
            SaveFeaturesText = new SaveFeaturesTxt();
            Classifier = new Classification(); 
        } 
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::


        //::::::Call methods:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GettingKinectData.FindKinect();  
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

            //to get teh position of the hand 
            PointF centerHand; 
            PointF positionRoi1;  
            PointF positionCenterHand = new PointF();         
            //-------------------------------------------------------------------------------

            //-------------------------------------------------------------------------------
            //Get actual frame and the next frame. To delate the frames with positive falses.  
            imagenKinectGray1 = GettingKinectData.PollDepth(0);
            imagenKinectGray2 = GettingKinectData.PollDepth(1);

            returnHandDetectorFrontActual = HandDetection.Detection(imagenKinectGray1);
            returnHandDetectorSideActual = HandDetection.Detection(imagenKinectGray2);


            //Cast the return of each frame
            RoiKinectFrontActual = (System.Drawing.Rectangle[])returnHandDetectorFrontActual[0];
            RoiKinectSideActual = (System.Drawing.Rectangle[])returnHandDetectorSideActual[0];


            if ( (numFrames == 1) || (FrameHandDetected == false) ) 
            {   
                RectArrayPrev = RoiKinectFrontActual; 
            }



            //Guardar las imagenes para ver cuantas detecta   
            //{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{ 
            int numHsFront = RoiKinectFrontActual.Length;
            int numHsSide = RoiKinectSideActual.Length;

            imagenKinectGray1.Save(pathFront + "Front_" + numFrames.ToString() + "_" + sec.ToString() + "_" + fps.ToString() + ".png");
            imagenKinectGray2.Save(pathSide + "Side_" + numFrames.ToString() + "_" + sec.ToString() + "_" + fps.ToString() + ".png"); 

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


            if (RoiKinectFrontActual.Length != 0) //Esto se realiza si el cuadro del kinect es detectado. 
            {
                //check if the roi in the previous frame intesect with the roi in the actual frame 
                noPositiveFalses = RectArrayPrev[0].IntersectsWith(RoiKinectFrontActual[0]);

                if (noPositiveFalses)
                {
                    RectArrayPrev = RoiKinectFrontActual;
                    flag_execute = true;
                    numFrameHandDetected++; 
                }


                if (flag_execute)
                {
                    if ((RoiKinectFrontActual.Length == 1) && (RoiKinectSideActual.Length == 1)) //Para saber cuantas detecta al mismo tiempo
                    {
                        //Comentado hasta que quede el despliege de los datos 
                        returnGettingSegK1 = GettingSegmentationK1.HandConvexHull(imagenKinectGray1, RoiKinectFrontActual[0]); 
                        //if (RoiKinectSideActual.Length != 0)
                        //    returnGettingSegK2 = GettingSegmentationK2.HandConvexHull(imagenKinectGray2, RoiKinectSideActual[0]);
                        
                        if (returnGettingSegK1 != null) 
                        {
                            centerHand = (PointF)returnGettingSegK1[0]; 

                            positionRoi1 = RoiKinectFrontActual[0].Location; 
                            positionCenterHand.X = positionRoi1.X + centerHand.X; 
                            positionCenterHand.Y = positionRoi1.Y + centerHand.Y; 

                            CircleF centrillo = new CircleF(positionCenterHand, 5f);

                            imagenKinectGray1.Draw(centrillo, new Gray(150), 3);

                            //SaveFeaturesText.SaveFeaturesTraining("2", returnGettingSegK1, pathFront+"TrainingFront2.txt"); 
                            SaveFeaturesText.SaveFeaturesTest(numFrames, returnGettingSegK1, pathFront + "Test1.txt"); 

                            //This thing is for count the frames where one hand is detected in every kinect.

                            imagenKinectGray1.Save(pathFront + "F_" + numFrames.ToString() + "_"+ sec.ToString() + "_" + fps.ToString() + ".png");
                            imagenKinectGray2.Save(pathSide + "S_" + numFrames.ToString() + "_"+ sec.ToString() + "_" + fps.ToString() + ".png");
                        } 
                    }
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

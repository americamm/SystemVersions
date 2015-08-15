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
        
        //:::::Declare Class::::::::::::::::::::::::::::::::
        public GetKinectData GettingKinectData;
        public HandDetector HandDetection;
        public HandSegmentation GettingSegmentationK1;
        public HandSegmentation GettingSegmentationK2;
        public SaveFeaturesTxt SaveFeaturesText;
        public Classification Classifier; 
        
        //:::::Variables::::::::::::::::::::::::::::::::::::
        private int FrameWidth = 640;
        private int FrameHeigth = 480;

        private WriteableBitmap ImagenWriteablebitmap;
        private Int32Rect WriteablebitmapRect;
        private int WriteablebitmapStride; 
        
        //para contar los cuadros por segundo 
        private int fps = 1;
        private int sec = 1;  
        //Escribir el archivito.
        //private StreamWriter file = new StreamWriter(@"C:\MyRecognition\3ro\test.txt", true);
        private string path = @"C:\MyRecognition\Corridas\";
        //private string path = 

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
            Classifier.ClasificationHand(); 
            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
        }
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::



        //:::::::Display the stuff:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            List<Object> returnHandDetectorK1 = new List<object>(2);
            List<Object> returnHandDetectorK2 = new List<object>(2);
            System.Drawing.Rectangle[] RoiKinect1;
            System.Drawing.Rectangle[] RoiKinect2;
            Image<Gray, Byte> imagenKinectGray1;
            Image<Gray, Byte> imagenKinectGray2;
            Image<Gray, Byte> imageRoi1 = new Image<Gray,Byte>(200,200);
            Image<Gray, Byte> imageRoi2 = new Image<Gray, Byte>(200, 200);
            
            List<Object> returnGettingSegK1 = new List<object>(); 
            List<Object> returnGettingSegk2 = new List<object>();


            PointF centerHand; 
            PointF positionRoi1;  
            PointF positionCenterHand = new PointF();         


            imagenKinectGray1 = GettingKinectData.PollDepth(0); 
            imagenKinectGray2 = GettingKinectData.PollDepth(1);
            
            returnHandDetectorK1 = HandDetection.Detection(imagenKinectGray1);
            returnHandDetectorK2 = HandDetection.Detection(imagenKinectGray2);

            //cast the return
            //imagenKinectGray1 = (Image<Gray,Byte>)returnHandDetectorK1[1];
            //imagenKinectGray2 = (Image<Gray, Byte>)returnHandDetectorK2[1];
            RoiKinect1 = (System.Drawing.Rectangle[])returnHandDetectorK1[0];
            RoiKinect2 = (System.Drawing.Rectangle[])returnHandDetectorK2[0]; 


            //if (RoiKinect1 != System.Drawing.Rectangle.Empty)//&& RoiKinect2 != System.Drawing.Rectangle.Empty)
            //{
                /*Comentado hasta que quede el despliege de los datos
                centerHand = GettingSegmentation.HandConvexHull(imagenKinectGray1, RoiKinect1);

                positionRoi1 = RoiKinect1.Location; 
                positionCenterHand.X = positionRoi1.X + centerHand.X; 
                positionCenterHand.Y = positionRoi1.Y + centerHand.Y; 

                CircleF centrillo = new CircleF(positionCenterHand, 5f);

                imagenKinectGray1.Draw(centrillo, new Gray(150), 3); 
          

                //imageRoi2 = GettingSegmentation.HandConvexHull(imagenKinectGray2, RoiKinect2);

                */

            //This thing is for count the frames where one hand is detected in every kinect.
            //Save the images and a txt.
            
            
            if ((RoiKinect1.Length == 1) && (RoiKinect2.Length == 1))
            {  
                returnGettingSegK1 = GettingSegmentationK1.HandConvexHull(imagenKinectGray1, RoiKinect1[0]);
                returnGettingSegk2 = GettingSegmentationK2.HandConvexHull(imagenKinectGray2, RoiKinect2[0]);

                SaveFeaturesText.SaveFeaturesTest(returnGettingSegK1, path+"Test.txt"); 

                imagenKinectGray1.Save(path + "front" + sec.ToString() +"_"+ fps.ToString() + ".png");
                //imagenKinectGray2.Save(path + "side" + sec.ToString() +"_"+ fps.ToString() + ".png");
            } 
            if (fps == 30)
            {
                    fps = 1;
                    sec++; 
            } 

                DepthImageK1.Source = imagetoWriteablebitmap(imagenKinectGray1);
                DepthImageK2.Source = imagetoWriteablebitmap(imagenKinectGray2);  


            //} 
            fps++;
        }
        //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::



        //:::::::::::::Method to convert a byte[] of the gray image to a writeablebitmap::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
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
        //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::


        //:::::::::::::::Metodo para escribir cuantos frames con las manos detecta:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //private void WriteTxt()
        //{ 
            
        //}


        //:::::::::::::::Stop the kinect stream::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            GettingKinectData.StopKinect();
            //file.Close(); 
        }
        //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::



    }//end class
}// end namespace

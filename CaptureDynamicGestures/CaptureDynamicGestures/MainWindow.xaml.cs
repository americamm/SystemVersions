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
using Microsoft.Kinect;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.Util;
using System.IO; 



namespace CaptureDynamicGestures
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //::::::::::::::Variables:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private KinectSensor Kinect;
        private KinectSensorCollection Sensores = KinectSensor.KinectSensors;
        private List<KinectSensor> Sensor = new List<KinectSensor>();

        private DepthImagePixel[] DepthPixels;
        private DepthImageStream DepthStream;
        private byte[] DepthImagenPixeles;
        private Image<Gray, Byte> depthFrameKinect;

        private WriteableBitmap ImagenWriteablebitmap;
        private Int32Rect WriteablebitmapRect;
        private int WriteablebitmapStride; 

        private int minDepth = 400;
        private int maxDepth = 2000;

        private int fps = 1;
        private int sec = 1;
        private int numFrames = 1;

        private int FrameWidth = 640;
        private int FrameHeigth = 480;

        private bool capture;

        private string path = @"C:\CaptureGestures\";
        private string pathSave;
        private string testP;
        private string subjectP;
        private string gestureP; 

        //:::::::::::::end variables:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

        public MainWindow()
        {
            InitializeComponent();
            FindKinect(); 
        } 

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
        }  
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            //---------------------------------------------------------------
            Image<Gray, Byte> imagenKinectGray1; //Front kinect
            Image<Gray, Byte> imagenKinectGray2; //Side Kinect 
            //--------------------------------------------------------------- 

            imagenKinectGray1 = PollDepth(0);
            imagenKinectGray2 = PollDepth(1);

            imageKinectFront.Source = imagetoWriteablebitmap(imagenKinectGray1);
            imageKinectSide.Source = imagetoWriteablebitmap(imagenKinectGray2);

            if (capture)
            {  
              pathSave = @
            }
        }//end CompositionTarget_rendering()

        //:::::::::::::Find all the sensors kinect, and inicialites the depth streams::::::::::::::::::::::::::::::::::::::::::::::::::::::
        public void FindKinect()
        {
            foreach (KinectSensor Kinect in Sensores)
            {
                if (Kinect.Status == KinectStatus.Connected)
                {
                    Sensor.Add(Kinect);
                }
            }

            try
            {
                for (int i = 0; i < Sensor.Count; i++)
                {
                    Kinect = Sensor[i];
                    if (this.Kinect != null)
                    {
                        this.Kinect.DepthStream.Enable();
                        this.Kinect.DepthStream.Range = DepthRange.Near;
                        this.Kinect.Start();
                    }
                }
            }
            catch
            {
                MessageBox.Show("El dispositivo Kinect no se encuentra conectado", "Error Kinect");
            }
        }
        //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        
        //:::::::Return the depth image from the sensor, the image is a Emgu type. The image is noise free with a median filter:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        public Image<Gray, Byte> PollDepth(int numKinect)
        {
            Image<Bgra, Byte> depthFrameKinectBGR = new Image<Bgra, Byte>(FrameWidth, FrameHeigth);
            Kinect = Sensor[numKinect];

            if (this.Kinect != null)
            {
                this.DepthStream = this.Kinect.DepthStream;
                this.DepthPixels = new DepthImagePixel[DepthStream.FramePixelDataLength];
                this.DepthImagenPixeles = new byte[DepthStream.FramePixelDataLength * 4];
                this.depthFrameKinect = new Image<Gray, Byte>(DepthStream.FrameWidth, DepthStream.FrameHeight);

                Array.Clear(DepthImagenPixeles, 0, DepthImagenPixeles.Length);

                try
                {
                    using (DepthImageFrame frame = this.Kinect.DepthStream.OpenNextFrame(100))
                    {
                        if (frame != null)
                        {
                            frame.CopyDepthImagePixelDataTo(this.DepthPixels);


                            int index = 0;
                            for (int i = 0; i < DepthPixels.Length; ++i)
                            {
                                short depth = DepthPixels[i].Depth;

                                byte intensity = (byte)((depth >= minDepth) && (depth <= maxDepth) ? depth : 0);

                                DepthImagenPixeles[index++] = intensity;
                                DepthImagenPixeles[index++] = intensity;
                                DepthImagenPixeles[index++] = intensity;

                                ++index;
                            }

                            depthFrameKinectBGR.Bytes = DepthImagenPixeles; //The bytes are converted to a Imagen(Emgu). This to work with the functions of opencv. 
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("No se pueden leer los datos del sensor", "Error");
                }
            }

            depthFrameKinect = depthFrameKinectBGR.Convert<Gray, Byte>();
            depthFrameKinect = removeNoise(depthFrameKinect, 13);

            numFrames++;

            return depthFrameKinect;
        }//fin PollDepth() 
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

        //::::::::::::Remove the noise ina a gray image:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        public Image<Gray, Byte> removeNoise(Image<Gray, Byte> imagenKinet, int sizeWindow)
        {
            Image<Gray, Byte> imagenSinRuido;

            imagenSinRuido = imagenKinet.SmoothMedian(sizeWindow);

            return imagenSinRuido;
        }//endremoveNoise  
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

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


        //:::::::Capture, save and store de gestures; 
        private void CaptureGesture_Click(object sender, RoutedEventArgs e)
        {
            capture = true;
            StopCapture.IsEnabled = true; 
        }

        private void StopCapture_Click(object sender, RoutedEventArgs e)
        { 
            capture = false; 
        }

        //::::::::::Unload the window::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            StopKinect(); 
        }        
        //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //::::::::::::Close the stream of the sensors:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        public void StopKinect()
        {
            foreach (KinectSensor Kinect in Sensores)
            {
                if (Kinect.Status == KinectStatus.Connected)
                {
                    Kinect.Stop();
                }
            }
        }

        private void Test_TextChanged(object sender, TextChangedEventArgs e)
        {
            testP = Test.Text; 
        }

        private void Subject_TextChanged(object sender, TextChangedEventArgs e)
        {
            subjectP = Subject.Text; 
        }

        private void Gesture_TextChanged(object sender, TextChangedEventArgs e)
        {
            gestureP = Gesture.Text; 
        }
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::


    } //end class
}//end namespace

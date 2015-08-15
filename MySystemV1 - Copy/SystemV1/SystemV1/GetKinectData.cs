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

namespace SystemV1
{
    
    
    public class GetKinectData
    {
        //::::::::::::::Variables:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private KinectSensor Kinect;
        private KinectSensorCollection Sensores = KinectSensor.KinectSensors;
        private List<KinectSensor> Sensor = new List<KinectSensor>();


        private DepthImagePixel[] DepthPixels;
        private DepthImageStream DepthStream;
        private byte[] DepthImagenPixeles;
        private Image<Gray, Byte> depthFrameKinect;

        private int minDepth = 400;
        private int maxDepth = 2000;
        //:::::::::::::fin variables::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::


        //:::::::::::::Find all the sensors kinect, and inicialites the depth streams:::::::::::::::::::::::::::::::::::::::::::::
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
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::



        //:::::::Return the depth image from the sensor, the image is a Emgu type. The image is noise free with a median filter:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        public Image<Gray, Byte> PollDepth(int numKinect)
        {
            Image<Bgra, Byte> depthFrameKinectBGR = new Image<Bgra, Byte>(640, 480);
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

            return depthFrameKinect;
        }//fin PollDepth() 
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::



        //::::::::::::Remove the noise ina a gray image:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private Image<Gray, Byte> removeNoise(Image<Gray, Byte> imagenKinet, int sizeWindow)
        {
            Image<Gray, Byte> imagenSinRuido;

            imagenSinRuido = imagenKinet.SmoothMedian(sizeWindow);

            return imagenSinRuido;
        }//endremoveNoise  
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::



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
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::



    }//end class
}//end namespace 

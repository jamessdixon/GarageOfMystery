using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

namespace ChickenSoftware.Halloween.UI
{
    public partial class MainWindow : Window
    {
        Int32 _numberOfSkeletonHits = 0;
        Int32 _numberOfHitsForAlarm = 100;
        SentientGarage _garage = null;
        KinectSensor _kinectSensor = null;
        Int32 _lightSensorThreshold = 0;
        Boolean _sensor0Tripped = false;
        Boolean _sensor1Tripped = false;
        Boolean _sensor2Tripped = false;
        Boolean _sensor3Tripped = false;
        Int32 _numberOfSensorsForAlarm = 2;
        SkeletonPoint _skeletonPoint = new SkeletonPoint();
        float _zDeltaThreshold = .0050F;
        Int32 _openPosition = 40;

        public MainWindow()
        {
            InitializeComponent();
            Start();
        }

        public void Start()
        {
            _kinectSensor = KinectSensor.KinectSensors[0];

            _garage = new SentientGarage();
            _garage.ColorDataReady += garage_ColorDataReady;
            _garage.SkeletonChanged += garage_SkeletonChanged;
            _garage.initializeKinect();

            _garage.initializeController();
            _garage.moveController(110);
            _garage.LightSensorChange += garage_LightSensorChange;
            _garage.initializeInterfaceKit();
        }

        void garage_ColorDataReady(object sender, ColorDataReadyEventArgs args)
        {
            kinectVideo.Source = BitmapSource.Create(
                args.Width,args.Height,96,96,PixelFormats.Bgr32,
                null,args.ColorData,args.Stride);
        }

        void garage_LightSensorChange(object sender, LightSensorChangeEventArgs args)
        {
            switch (args.SensorIndex)
            {
                case 0:
                    if (args.SensorIndex == 0 && args.LightAmount < _lightSensorThreshold)
                    {
                        _sensor0Tripped = true;
                        sensor0Rectange.Dispatcher.Invoke(new Action(()=>sensor0Rectange.Fill = new SolidColorBrush(Colors.Red)));
                    }
                    break;
                case 1:

                    if (args.SensorIndex == 1 && args.LightAmount < _lightSensorThreshold)
                    {
                        _sensor1Tripped = true;
                        sensor1Rectange.Dispatcher.Invoke(new Action(() => sensor1Rectange.Fill = new SolidColorBrush(Colors.Red)));
                    }
                    break;
                case 2:
                    if (args.SensorIndex == 2 && args.LightAmount < _lightSensorThreshold)
                    {
                        _sensor2Tripped = true;
                        sensor2Rectange.Dispatcher.Invoke(new Action(() => sensor2Rectange.Fill = new SolidColorBrush(Colors.Red)));
                    }
                    break;
                case 3:
                    if (args.SensorIndex == 3 && args.LightAmount < _lightSensorThreshold)
                    {
                        _sensor3Tripped = true;
                        sensor3Rectange.Dispatcher.Invoke(new Action(() => sensor3Rectange.Fill = new SolidColorBrush(Colors.Red)));
                    }
                    break;
            }
            CheckForIntruder();
        }

        void garage_SkeletonChanged(object sender, Skeleton skeleton)
        {
            if(_skeletonPoint.Z > 0)
            {
                float zDelta = _skeletonPoint.Z - skeleton.Position.Z;
                if (zDelta >= _zDeltaThreshold)
                {
                    _numberOfSkeletonHits += 1;
                    skeletonChangedProgressBar.Dispatcher.Invoke(new Action(() => skeletonChangedProgressBar.Value = _numberOfSkeletonHits));

                }
                if(_numberOfSkeletonHits >= _numberOfHitsForAlarm)
                {
                    _garage.moveController(_openPosition);
                }

                skeletonCanvas.Children.Clear();
                drawSkelton(skeleton);
            }
            _skeletonPoint = skeleton.Position;
        }

        


        void drawSkelton(Skeleton skeleton)
        {
            addLine(skeleton.Joints[JointType.Head], skeleton.Joints[JointType.ShoulderCenter]);
            addLine(skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.Spine]);
            addLine(skeleton.Joints[JointType.Spine], skeleton.Joints[JointType.HipCenter]);
            addLine(skeleton.Joints[JointType.HipCenter], skeleton.Joints[JointType.HipLeft]);
            addLine(skeleton.Joints[JointType.HipLeft], skeleton.Joints[JointType.KneeLeft]);
            addLine(skeleton.Joints[JointType.KneeLeft], skeleton.Joints[JointType.AnkleLeft]);
            addLine(skeleton.Joints[JointType.AnkleLeft], skeleton.Joints[JointType.FootLeft]);
            addLine(skeleton.Joints[JointType.HipCenter], skeleton.Joints[JointType.HipRight]);
            addLine(skeleton.Joints[JointType.HipRight], skeleton.Joints[JointType.KneeRight]);
            addLine(skeleton.Joints[JointType.KneeRight], skeleton.Joints[JointType.AnkleRight]);
            addLine(skeleton.Joints[JointType.AnkleRight], skeleton.Joints[JointType.FootRight]);
            addLine(skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.ShoulderLeft]);
            addLine(skeleton.Joints[JointType.ShoulderLeft], skeleton.Joints[JointType.ElbowLeft]);
            addLine(skeleton.Joints[JointType.ElbowLeft], skeleton.Joints[JointType.WristLeft]);
            addLine(skeleton.Joints[JointType.WristLeft], skeleton.Joints[JointType.HandLeft]);
            addLine(skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.ShoulderRight]);
            addLine(skeleton.Joints[JointType.ShoulderRight], skeleton.Joints[JointType.ElbowRight]);
            addLine(skeleton.Joints[JointType.ElbowRight], skeleton.Joints[JointType.WristRight]);
            addLine(skeleton.Joints[JointType.WristRight], skeleton.Joints[JointType.HandRight]);
        }

        void addLine(Joint joint0, Joint joint1)
        {
            Line boneLine = new Line();
            var _skeletonBrush = new SolidColorBrush(Colors.Red);
            boneLine.Stroke = _skeletonBrush;
            boneLine.StrokeThickness = 5;

            ColorImagePoint jointPoint0 = _kinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(joint0.Position, ColorImageFormat.RgbResolution640x480Fps30);
            boneLine.X1 = jointPoint0.X;
            boneLine.Y1 = jointPoint0.Y;

            ColorImagePoint jointPoint1 = _kinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(joint1.Position, ColorImageFormat.RgbResolution640x480Fps30);
            boneLine.X2 = jointPoint1.X;
            boneLine.Y2 = jointPoint1.Y;

            skeletonCanvas.Children.Add(boneLine);
        }

        private void CheckForIntruder()
        {
            Int32 numberOfSensorsTripped = 0;

            if (_sensor0Tripped == true) 
                numberOfSensorsTripped += 1;
            if (_sensor1Tripped == true)
                numberOfSensorsTripped += 1;
            if (_sensor2Tripped == true)
                numberOfSensorsTripped += 1;
            if (_sensor3Tripped == true)
                numberOfSensorsTripped += 1;
            if (numberOfSensorsTripped >= _numberOfSensorsForAlarm )
                _garage.moveController(0);

        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            skeletonCanvas.Children.Clear();
            _numberOfSkeletonHits = 0;
            skeletonChangedProgressBar.Value = 0;
            _sensor0Tripped = false;
            _sensor1Tripped = false;
            _sensor2Tripped = false;
            _sensor3Tripped = false;
            _garage.moveController(110);

        }

        private void EjectButton_Click(object sender, RoutedEventArgs e)
        {
            _garage.moveController(_openPosition);

        }

    }
}

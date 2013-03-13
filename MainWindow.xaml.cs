using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace Kmouse
{

    enum CurrenState
    {
        WaitForPPT,
        PlayingPPT,
        WaitForMaya,
        PlayingMaya,
    };
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor kinect;
        private Skeleton[] _FrameSkeletons;
        private SwipeDetect _SwipeGesture;
        private SeparateDetect _SeparateGesture;
        private HoldRotate _HoldRotate;

        private CurrenState _cs;

        public static bool IfRotating;

        public KinectSensor Kinect
        {
            get { return this.kinect; }
            set
            {
                //如果带赋值的传感器和目前的不一样
                if (this.kinect != value)
                {
                    //如果当前的传感对象不为null
                    if (this.kinect != null)
                    {
                        UninitializeKinectSensor(this.kinect);
                        //uninitailize当前对象
                        this.kinect = null;
                    }
                    //如果传入的对象不为空，且状态为连接状态
                    if (value != null && value.Status == KinectStatus.Connected)
                    {
                        this.kinect = value;
                        InitializeKinectSensor(this.kinect);
                    }
                }
            }
        }

        private void InitializeKinectSensor(KinectSensor kinectSensor)
        {

            if (kinectSensor != null)
            {
                SkeletonStream skeleStream = kinectSensor.SkeletonStream;

                skeleStream.Enable();//骨骼追踪声明
                _cs = CurrenState.WaitForPPT;
                IfRotating = false;
                _SeparateGesture.GesturePointTrackingInitialize();
                _SeparateGesture.GesturePointTrackingStart();

                kinectSensor.SkeletonFrameReady += kinectSensor_SkeletonFrameReady;
                kinectSensor.Start();
            }
        }

        void kinectSensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    this._FrameSkeletons = new Skeleton[kinect.SkeletonStream.FrameSkeletonArrayLength];
                    frame.CopySkeletonDataTo(this._FrameSkeletons);


                    if (_cs == CurrenState.WaitForPPT){
                            _SeparateGesture.Update(this._FrameSkeletons, frame.Timestamp);
                    }
                    else if (_cs == CurrenState.PlayingPPT)
                    {
                        _SwipeGesture.Update(this._FrameSkeletons, frame.Timestamp);
                    }
                    else if (_cs == CurrenState.PlayingMaya)
                    {
                        _HoldRotate.Update(this._FrameSkeletons, frame.Timestamp);
                    }
                   
                }
            }
            
        }



        private void UninitializeKinectSensor(KinectSensor kinectSensor)
        {
            throw new NotImplementedException();
        }

        public MainWindow()
        {
            this._SwipeGesture = new SwipeDetect();
            this._SwipeGesture.GestureDetectRight += _SwipeGesture_GestureDetectRight;
            this._SwipeGesture.GestureDetectLeft += _SwipeGesture_GestureDetectLeft;

            this._SeparateGesture = new SeparateDetect();
            this._SeparateGesture.GestureDetect += _SeparateGesture_GestureDetect;

            this._HoldRotate = new HoldRotate();
            this._HoldRotate.HoldDetectLeft += _HoldRotate_HoldDetectLeft;
            this._HoldRotate.HoldDetectRight += _HoldRotate_HoldDetectRight;
            
            this.kinect = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
            InitializeKinectSensor(kinect);
            InitializeComponent();    
        }

        void _HoldRotate_HoldDetectRight(object sender, EventArgs e)
        {
            while (IfRotating)
            {
                System.Windows.Forms.SendKeys.SendWait("{%Left}");
            }
        }

        void _HoldRotate_HoldDetectLeft(object sender, EventArgs e)
        {
            while (IfRotating)
            {
                System.Windows.Forms.SendKeys.SendWait("{%Left}");
            }
        }

        void _SeparateGesture_GestureDetect(object sender, EventArgs e)
        {
            _cs = CurrenState.PlayingPPT;
            _SwipeGesture.GesturePointTrackingInitialize();
            _SwipeGesture.GesturePointTrackingStart();
            System.Windows.Forms.SendKeys.SendWait("{F5}");
        }

        void _SwipeGesture_GestureDetectLeft(object sender, EventArgs e)
        {
            System.Windows.Forms.SendKeys.SendWait("{Left}");
        }

        void _SwipeGesture_GestureDetectRight(object sender, EventArgs e)
        {
            System.Windows.Forms.SendKeys.SendWait("{Right}");
        }

    }
}

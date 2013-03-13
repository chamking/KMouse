using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using System.Windows;

namespace Kmouse
{
    class SeparateDetect
    {
        private List<GesturePoint> gesturePoints = new List<GesturePoint>();
        private bool gesturePointTrackingEnabled;
        private int SeparateTime;
        private double separateLength, separateDeviation;
        public event EventHandler GestureDetect;
        private double xOutOfBoundsLength;
        private static double initialSwipeX;




        public void GesturePointTrackingInitialize()
        {
            this.separateLength = 40; 
            this.SeparateTime = 1500;
            this.xOutOfBoundsLength = 1000;
        }

        public void GesturePointTrackingStart()
        {
            if (separateLength + separateDeviation + SeparateTime == 0)
                throw new InvalidOperationException("挥动手势识别参数没有初始化！");
            gesturePointTrackingEnabled = true;
        }

        public void GesturePointTrackingStop()
        {
            xOutOfBoundsLength = 0;
            gesturePointTrackingEnabled = false;
            gesturePoints.Clear();
        }

        public bool GesturePointTrackingEnabled
        {
            get { return gesturePointTrackingEnabled; }
        }

        private void ResetGesturePoint(GesturePoint point)
        {
            bool startRemoving = false;
            for (int i = gesturePoints.Count; i >= 0; i--)
            {
                if (startRemoving)
                    gesturePoints.RemoveAt(i);
                else
                    if (gesturePoints[i].Equals(point))
                        startRemoving = true;
            }
        }

        private void ResetGesturePoint(int point)
        {
            if (point < 1)
                return;
            for (int i = point - 1; i >= 0; i--)
            {
                gesturePoints.RemoveAt(i);
            }
        }



        public void Update(Skeleton[] skeletons, long frameTimestamp)
        {
            if (skeletons != null)
            {
                Skeleton skeleton;

                for (int i = 0; i < skeletons.Length; i++)
                {
                    skeleton = skeletons[i];

                    if (skeleton.TrackingState != SkeletonTrackingState.NotTracked)
                    {
                        Joint Rhand = skeleton.Joints[JointType.HandRight];
                        Joint Lhand = skeleton.Joints[JointType.HandLeft];

                        Joint center = skeleton.Joints[JointType.HipCenter];

                        if (Rhand.Position.Y > center.Position.Y && Lhand.Position.Y >  center.Position.Y)
                        {

                            HandleGestureTracking(Rhand.Position.X * 100, Rhand.Position.Y * 100, Rhand.Position.Z * 100, frameTimestamp,
                                                  Lhand.Position.X * 100, Lhand.Position.Y* 100,Lhand.Position.Z * 100);
                        }
                        else
                        {
                            gesturePoints.Clear();
                        }
                    }
                }
            }

        }
        private void HandleGestureTracking(float x, float y, float z, long TimeStamp,float _x,float _y,float _z)
        {
            if (!gesturePointTrackingEnabled)
                return;
            // check to see if xOutOfBounds is being used
            if (xOutOfBoundsLength != 0 && initialSwipeX == 0)
            {
                initialSwipeX = x;
            }

            GesturePoint newPoint = new GesturePoint() { X = x, Y = y, Z = z, T = TimeStamp, _X = _x, _Y = _y, _Z = _z };
            newPoint.MathDis();

            gesturePoints.Add(newPoint);

            GesturePoint startPoint = gesturePoints[0];
            var point = new Point(x, y);


            //check for deviation
            
            if (newPoint.T - startPoint.T > SeparateTime) //check time
            {
                gesturePoints.RemoveAt(0);
                startPoint = gesturePoints[0];
            }
            if (newPoint.Dis - startPoint.Dis > separateLength) // check to see if distance has been achieved swipe right
            {
                //throw local event
                    if (GestureDetect != null)
                        GestureDetect(this, new EventArgs());
                    gesturePoints.Clear();
                    return;
            }

        }

    }
}

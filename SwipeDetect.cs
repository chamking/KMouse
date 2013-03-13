using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using System.Windows;

namespace Kmouse
{
    public struct GesturePoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public long T { get; set; }


        public double _X { get; set; }
        public double _Y { get; set; }
        public double _Z { get; set; }

        public double Dis { get; set; }

        public void MathDis()
        {
            Dis = Math.Sqrt((X - _X) * (X - _X) + (Y - _Y) * (Y - _Y));
        }
        public override bool Equals(object obj)
        {
            var o = (GesturePoint)obj;
            return (X == o.X) && (Y == o.Y) && (Z == o.Z) && (T == o.T) && (_X == o._X) && (_Y == o._Y) && (_Z == o._Z) && (Dis == o.Dis);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    class SwipeDetect
    {
        private List<GesturePoint> gesturePoints = new List<GesturePoint>();
        private bool gesturePointTrackingEnabled; 
        private int swipeTime;
        private double swipeLength, swipeDeviation;
        public event EventHandler GestureDetectRight;
        public event EventHandler GestureDetectLeft;
        private double xOutOfBoundsLength;
        private static double initialSwipeX;


        public void GesturePointTrackingInitialize()
        {
            this.swipeLength = 30; this.swipeDeviation = 30;
            this.swipeTime = 600;
            this.xOutOfBoundsLength =1000;
        }

        public void GesturePointTrackingStart()
        {
            if (swipeLength + swipeDeviation + swipeTime == 0)
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
                        Joint hand = skeleton.Joints[JointType.HandRight];
                        Joint center = skeleton.Joints[JointType.HipCenter];

                        if (hand.Position.Y > center.Position.Y)
                        {

                            HandleGestureTracking(hand.Position.X * 100, hand.Position.Y * 100, hand.Position.Z * 100, frameTimestamp);
                        }
                        else
                        {
                            gesturePoints.Clear();
                        }
                    }
                }
            }

        }
        private void HandleGestureTracking(float x, float y, float z, long TimeStamp)
        {
            if (!gesturePointTrackingEnabled)
                return;
            // check to see if xOutOfBounds is being used
            if (xOutOfBoundsLength != 0 && initialSwipeX == 0)
            {
                initialSwipeX = x;
            }

            GesturePoint newPoint = new GesturePoint() { X = x, Y = y, Z = z, T = TimeStamp };

            gesturePoints.Add(newPoint);

            GesturePoint startPoint = gesturePoints[0];
            var point = new Point(x, y);


            //check for deviation
            if (Math.Abs(newPoint.Y - startPoint.Y) > swipeDeviation)
            {
                //Debug.WriteLine("Y out of bounds");
                
                ResetGesturePoint(gesturePoints.Count);
                return;
            }
            if ( newPoint.T - startPoint.T > swipeTime) //check time
            {
                gesturePoints.RemoveAt(0);
                startPoint = gesturePoints[0];
            }
            if ( Math.Abs(newPoint.X - startPoint.X) > swipeLength) // check to see if distance has been achieved swipe right
            {
                

                //throw local event
                if ((newPoint.X - startPoint.X) > 0)
                {
                    if (GestureDetectRight != null)
                        GestureDetectRight(this, new EventArgs());
                    gesturePoints.Clear();
                    return;
                }
                else
                {
                    if (GestureDetectLeft != null)
                        GestureDetectLeft(this, new EventArgs());
                    gesturePoints.Clear();
                    return;
                }
            }

        }

    }
}

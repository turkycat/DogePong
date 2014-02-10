using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.GamerServices;

namespace DogePong.Controllers
{
    using Microsoft.Kinect;
    using System.IO;

    public class Kinect : Controller
    {
        private KinectSensor _sensor;
        private Player blue;
        private Player red;

        public Kinect( Player blue, Player red )
        {
            this.blue = blue;
            this.red = red;
        }


        public void init()
        {
            foreach ( var potentialSensor in KinectSensor.KinectSensors )
            {
                if ( potentialSensor.Status == KinectStatus.Connected )
                {
                    this._sensor = potentialSensor;
                    break;
                }
            }

            if ( this._sensor != null )
            {
                // Turn on the skeleton stream to receive skeleton frames
                this._sensor.SkeletonStream.Enable();

                //we only care about the arms and above
                this._sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;

                // Add an event handler to be called whenever there is new color frame data
                this._sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;

                // Start the sensor!
                try
                {
                    this._sensor.Start();
                    GameState.Instance.KinectReady = true;
                }
                catch ( IOException )
                {
                    this._sensor = null;
                }
            }

            if ( this._sensor == null )
            {
                GameState.Instance.KinectReady = false;
            }
        }

        private void SensorSkeletonFrameReady( object sender, SkeletonFrameReadyEventArgs e )
        {
            Skeleton[] skeletons = new Skeleton[0];

            using ( SkeletonFrame skeletonFrame = e.OpenSkeletonFrame() )
            {
                if ( skeletonFrame != null )
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo( skeletons );
                }

                if ( skeletons.Length != 0 && skeletons[0] != null )
                {
                    Skeleton first = null;
                    Skeleton second = null;
                    foreach ( Skeleton skel in skeletons )
                    {
                        if ( skel.TrackingState == SkeletonTrackingState.Tracked )
                        {
                            if ( first == null )
                            {
                                first = skel;
                            }
                            else if ( second == null )
                            {
                                second = skel;
                            }

                            Joint active = skel.Joints[JointType.HandRight];
                            if ( active.TrackingState == JointTrackingState.NotTracked && skel.Joints[JointType.HandLeft].TrackingState == JointTrackingState.Tracked )
                            {
                                active = skel.Joints[JointType.HandLeft];
                            }

                            //remember, the lowest possible place physically differs here. On screen, it represents the highest Y value, but Kinect represents it as the lowest.
                            //also, screen position uses ints, but the range of values on Kinect is -1.0 to 1.0. The most valuable space is the ~1 range between -0.5 and 0.5
                            float lowestPoint = GameState.Instance.GameHeight - DogePong.BOUNDARY_DENSITY - GameState.Instance.getTexture( "bluepaddle" ).Height;
                            float highestPoint = DogePong.BOUNDARY_DENSITY;
                            float playableAreaHeight = lowestPoint - DogePong.BOUNDARY_DENSITY;
                            float activeHandHeight = -1f * MathHelper.Clamp( active.Position.Y, -.5f, .5f );
                            //the area we care about is 0.5 to -0.5 in the screen, so for the highest screen value to be represented by 0.5, we add .5 and inverse
                            activeHandHeight += 0.5f;
                            float yPosition = ( ( (float) playableAreaHeight ) * activeHandHeight ) + highestPoint;

                            if ( skel == first )
                            {
                                blue.paddle.trajectory.currentPosition = new Vector2( blue.paddle.trajectory.currentPosition.X, yPosition );
                            }
                            else if( skel == second )
                            {
                                red.paddle.trajectory.currentPosition = new Vector2( red.paddle.trajectory.currentPosition.X, yPosition );
                            }
                        }

                    }
                }
            }
        }

        public override void ProcessMove( Paddle paddle )
        {
            return;
        }
    }
}

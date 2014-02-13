using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace DogePong.Controllers
{
    using Microsoft.Kinect;
    using System.IO;
    using Microsoft.Speech.Recognition;
    using Microsoft.Speech.AudioFormat;

    public class Kinect : Microsoft.Xna.Framework.GameComponent
    {
        private KinectSensor _sensor;
        private SpeechRecognitionEngine speechEngine;
        private Player blue;
        private Player red;

        public Kinect( Game game, Player blue, Player red ) : base( game )
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



                //speech recognizer
                RecognizerInfo ri = GetKinectRecognizer();

                if ( ri != null )
                {
                    this.speechEngine = new SpeechRecognitionEngine( ri.Id );

                    var speech = new Choices();
                    speech.Add( new SemanticResultValue( "begin", "START" ) );
                    speech.Add( new SemanticResultValue( "start", "START" ) );
                    speech.Add( new SemanticResultValue( "reset", "RESET" ) );

                    speech.Add( new SemanticResultValue( "pause", "PAUSE" ) );
                    speech.Add( new SemanticResultValue( "resume", "UNPAUSE" ) );
                    speech.Add( new SemanticResultValue( "unpause", "UNPAUSE" ) );

                    speech.Add( new SemanticResultValue( "single player", "ONEPLAYER" ) );
                    speech.Add( new SemanticResultValue( "two player", "TWOPLAYERS" ) );
                    speech.Add( new SemanticResultValue( "uno dos", "ONEPLAYER" ) );
                    speech.Add( new SemanticResultValue( "uno doges", "ONEPLAYER" ) );
                    speech.Add( new SemanticResultValue( "dos doges", "TWOPLAYERS" ) );
                    speech.Add( new SemanticResultValue( "dos dos", "TWOPLAYERS" ) );

                    speech.Add( new SemanticResultValue( "disable", "DISABLE" ) );
                    speech.Add( new SemanticResultValue( "kinect disable", "DISABLE" ) );
                    speech.Add( new SemanticResultValue( "connect disable", "DISABLE" ) );
                    speech.Add( new SemanticResultValue( "enable", "ENABLE" ) );
                    speech.Add( new SemanticResultValue( "kinect enable", "ENABLE" ) );
                    speech.Add( new SemanticResultValue( "connect enable", "ENABLE" ) );

                    speech.Add( new SemanticResultValue( "exit", "QUIT" ) );
                    speech.Add( new SemanticResultValue( "quit", "QUIT" ) );

                    var gb = new GrammarBuilder { Culture = ri.Culture };
                    gb.Append( speech );

                    var g = new Grammar( gb );

                    speechEngine.LoadGrammar( g );
                    speechEngine.SpeechRecognized += SpeechRecognized;

                    // I don't need to do anything when speech is rejected
                    //speechEngine.SpeechRecognitionRejected += SpeechRejected;
                }
                else
                {

                }



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



                speechEngine.SetInputToAudioStream(
                    _sensor.AudioSource.Start(), new SpeechAudioFormatInfo( EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null ) );
                speechEngine.RecognizeAsync( RecognizeMode.Multiple );
            }
            //if we have no sensor, signal the GameState singleton that Kinect is not enabled.
            if ( this._sensor == null )
            {
                GameState.Instance.KinectReady = false;
            }
        }

        #region Voice Recognition


        /**
         * looks through the available speech recognizers for en-US and returns if found
         */
        private static RecognizerInfo GetKinectRecognizer()
        {
            foreach ( RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers() )
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue( "Kinect", out value );
                if ( "True".Equals( value, StringComparison.OrdinalIgnoreCase ) && "en-US".Equals( recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase ) )
                {
                    return recognizer;
                }
            }

            return null;
        }





        /// <summary>
        /// Handler for recognized speech events.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void SpeechRecognized( object sender, SpeechRecognizedEventArgs e )
        {
            // Speech utterance confidence below which we treat speech as if it hadn't been heard
            const double ConfidenceThreshold = 0.3;


            if ( e.Result.Confidence >= ConfidenceThreshold )
            {
                switch ( e.Result.Semantics.Value.ToString() )
                {
                    case "START":
                        if ( GameState.Instance.State == State.MENU && GameState.Instance.KinectEnabled )
                            GameState.Instance.Begin();
                        break;

                    case "ENABLE":
                        if ( GameState.Instance.State == State.MENU )
                        {
                            GameState.Instance.KinectEnabled = true;
                        }
                        break;

                    case "DISABLE":
                        if ( GameState.Instance.State == State.MENU )
                        {
                            GameState.Instance.KinectEnabled = false;
                        }
                        break;

                    case "PAUSE":
                        if ( GameState.Instance.KinectEnabled && GameState.Instance.State == State.PLAYING )
                        {
                            GameState.Instance.State = State.PAUSED;
                        }
                        break;

                    case "UNPAUSE":
                        if ( GameState.Instance.KinectEnabled && GameState.Instance.State == State.PAUSED )
                        {
                            GameState.Instance.State = State.PLAYING;
                        }
                        break;

                    case "ONEPLAYER":
                        GameState.Instance.players = 1;
                        GameState.Instance.selectedMenuItem = MenuItem.SINGLE;
                        break;

                    case "TWOPLAYERS":
                        GameState.Instance.players = 2;
                        GameState.Instance.selectedMenuItem = MenuItem.MULTI;
                        break;

                    case "RESET":
                        if ( GameState.Instance.State == State.END )
                            GameState.Instance.reset();
                        break;

                    case "QUIT":
                        if( GameState.Instance.State != State.PLAYING )
                            base.Game.Exit();
                        break;
                }
            }
        }

        /**
         * Speech rejected callback method
         */
        private void SpeechRejected( object sender, SpeechRecognitionRejectedEventArgs e )
        {
            //do nothing
        }

        #endregion


        #region Skeleton Frame Callback

        private void SensorSkeletonFrameReady( object sender, SkeletonFrameReadyEventArgs e )
        {
            if ( !GameState.Instance.KinectEnabled ) return;
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


        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

/**
 * Don't judge me too harshly on the design pattern... Let's just say there are a few things I would do differently if I wrote this again,
 *  and a few things I did do differently on my 2nd XNA project. 
 *                   either way, hopefully you'll find this useful:
 * 
 *                                                      DogePong                                                                    "is a" relationships
 *                                                          |                                                                          V V V V V V V
 *   "has a" relationships -->               -----------------------------------------------------------------                     ---------------------
 *                                           |                                   |                           |                     |    IController    | (i)
 *                                           |                                 Player                        |                     ---------------------
 *                                           |                                   |                           |                                |
 *                                           |        --------------------------------------------           |                                V
 *                                           |        |          |                                |          |                     ----------------------
 *                                           |        |        Paddle                         Controller    Kinect                 |     Controller     |
 *                                       Colliders    |          |                                                                 ----------------------
 *                                           |------  |  ---------                                                                            |     
 *                                                 |  |  |                                                                                    V
 *                                                 |  |  |                                                              ----------------------------------------------
 *                                                Trajectory                                                            |                                            |
 *                                                                                                                      V                                            V
 *                                                                                                            ---------------------                           ----------------
 *                                                                                                            | GamePadController |                            |  Computer    |
 *                                                                                                            ---------------------                           -----------------
 *                                                                                                            
 * 
 *                                          ---------------------                                                                 
 *       more "is a"                        |    ICollidable    | (i)
 *                                          ---------------------
 *                                                    |
 *                                                    |---------------------------
 *                                                    |                          |
 *                                                    V                          V
 *                                          ----------------------      -----------------------
 *                                     (ac) | SphericalCollider  |      |      Boundary        |
 *                                          ----------------------      -----------------------                                                                       |     
 *                                                    |
 *                                                    V
 *                                   ---------------------------------------
 *                                   |                                     |
 *                                   V                                     V
 *                        ---------------------               -----------------------
 *                        |      DogeBall     |               |      BlackHole      |
 *                        ---------------------               -------------------------
 *                         
 * 
 * Kinect support automatically allows you to use up to two players, but an Kinect logo image will appear on the main screen confirming that the Kinect is ready
 * otherwise, you can play with any standard GamePad (tested with Xbox360 usb controller)
 */

namespace DogePong
{
    using Controllers;
    using Colliders;

    public enum MenuItem
    {
        SINGLE,
        MULTI
    };


    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class DogePong : Microsoft.Xna.Framework.Game
    {
        public static int MAX_BALLS = 10;
        public static int POINTS_TO_WIN = 7;
        public static float BOUNDARY_DENSITY = 75.0f;

        static GraphicsDeviceManager graphics;
        static SpriteBatch spriteBatch;
        
        //player paddles
        Paddle bluePaddle;
        Paddle redPaddle;

        TextItem blueScore;
        TextItem redScore;

        //a list of the text sprites
        List<TextItem> textList;

        //collision calculator used to handle the correct collision timings.
        private CollisionCalculator calculator;

        //dynamic game properties
        private long elapsedTime;
        private Color currentBackgroundColor;

        public DogePong()
        {
            graphics = new GraphicsDeviceManager( this );
            Content.RootDirectory = "Content";


            //set window preferences
            graphics.PreferredBackBufferWidth = 1600;
            graphics.PreferredBackBufferHeight = 900;

            /*
            if ( !graphics.IsFullScreen )
            {
                graphics.IsFullScreen = true;
            }
            */

            graphics.ApplyChanges();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch( GraphicsDevice );

            //initialize fonts and textures (again, credit to using the singleton class as a texture dictionary goes to James Boddie)
            Texture2D blueTexture = Content.Load<Texture2D>( "dogepaddle_blue" );
            Texture2D redTexture = Content.Load<Texture2D>( "dogepaddle_red" );
            GameState.Instance.addTexture( "bluepaddle", blueTexture );
            GameState.Instance.addTexture( "redpaddle", redTexture );
            GameState.Instance.addTexture( "border", Content.Load<Texture2D>( "border" ) );
            GameState.Instance.addTexture( "dogehead", Content.Load<Texture2D>( "dogehead" ) );
            GameState.Instance.addTexture( "dogeball", Content.Load<Texture2D>( "dogeball" ) );
            GameState.Instance.addTexture( "blackhole", Content.Load<Texture2D>( "blackhole" ) );
            GameState.Instance.addTexture( "kinect", Content.Load<Texture2D>( "Kinect" ) );
            GameState.Instance.addTexture( "kinectready", Content.Load<Texture2D>( "KinectReady" ) );
            GameState.Instance.addTexture( "kinectdisable", Content.Load<Texture2D>( "KinectDisable" ) );


            GameState.Instance.addFont( "small", Content.Load<SpriteFont>( "ComicSansSmall" ) );
            GameState.Instance.addFont( "regular", Content.Load<SpriteFont>( "ComicSans" ) );
            GameState.Instance.addFont( "large", Content.Load<SpriteFont>( "ComicSansLarge" ) );


            GameState.Instance.addSound( "boing", Content.Load<SoundEffect>( @"Sounds/boing" ) );
            GameState.Instance.addSound( "point", Content.Load<SoundEffect>( @"Sounds/point" ) );
            GameState.Instance.addSound( "boop0", Content.Load<SoundEffect>( @"Sounds/boop0" ) );
            GameState.Instance.addSound( "boop1", Content.Load<SoundEffect>( @"Sounds/boop1" ) );
            GameState.Instance.addSound( "boop2", Content.Load<SoundEffect>( @"Sounds/boop2" ) );
            GameState.Instance.addSound( "boop3", Content.Load<SoundEffect>( @"Sounds/boop3" ) );
            GameState.Instance.addSound( "port0", Content.Load<SoundEffect>( @"Sounds/port0" ) );
            GameState.Instance.addSound( "port1", Content.Load<SoundEffect>( @"Sounds/port1" ) );
            GameState.Instance.addSound( "port2", Content.Load<SoundEffect>( @"Sounds/port2" ) );
            GameState.Instance.addSound( "pew", Content.Load<SoundEffect>( @"Sounds/pew" ) );

            textList = new List<TextItem>();
            blueScore = new TextItem( "0", GameState.Instance.getFont( "regular" ), new Vector2( 680f, 15f ), Color.White, 0 ); ;
            redScore = new TextItem( "0", GameState.Instance.getFont( "regular" ), new Vector2( 900f, 15f ), Color.White, 0 );
            textList.Add( blueScore );
            textList.Add( redScore );

            //choose an initial background
            randomizeBackground();

            int areaHeight = graphics.GraphicsDevice.Viewport.Height;
            int areaWidth = graphics.GraphicsDevice.Viewport.Width;
            GameState.Instance.GameHeight = areaHeight;
            GameState.Instance.GameWidth = areaWidth;

            Boundary gameBoundary = new Boundary( BOUNDARY_DENSITY, areaHeight, areaWidth );

            //--------------initialize paddles & their players

            //initialize player paddle positions to the middle of the screen
            Vector2 bluePosition = new Vector2( BOUNDARY_DENSITY + blueTexture.Width, ( areaHeight - blueTexture.Height ) / 2.0f );
            Vector2 redPosition = new Vector2( areaWidth - BOUNDARY_DENSITY - redTexture.Width, ( graphics.GraphicsDevice.Viewport.Height - blueTexture.Height ) / 2.0f );

            //initialize paddle Trajectory objects using their initial positions
            Trajectory blueTrajectory = new Trajectory( bluePosition );
            Trajectory redTrajectory = new Trajectory( redPosition );

            //initialize paddles
            bluePaddle = new Paddle( blueTrajectory, blueTexture );
            redPaddle = new Paddle( redTrajectory, redTexture );

            //initialize a player and a computer player for starters
            Controller blueController = new GamePadController( PlayerIndex.One );
            //Controller redController = new GamePadController( PlayerIndex.Two );
            Controller redController = new ComputerController();

            //initialize players
            Player blue = new Player( blueController, PlayerIndex.One, bluePaddle, blueScore );
            Player red = new Player( redController, PlayerIndex.Two, redPaddle, redScore );
            GameState.Instance.blue = blue;
            GameState.Instance.red = red;
            //red = new Player( PlayerIndex.One, redPaddle, redScore );

            //initialize collision engine
            calculator = new CollisionCalculator( gameBoundary, blue, red );

            //--------------initialize a ball

            //initialize first ball with a position directly in the middle of the screen
            float centerBallWidth = ( graphics.GraphicsDevice.Viewport.Width - GameState.Instance.getTexture( "dogeball" ).Width ) / 2.0f;
            float centerBallHeight = ( graphics.GraphicsDevice.Viewport.Height - GameState.Instance.getTexture( "dogeball" ).Height ) / 2.0f;
            GameState.Instance.neutralSpawningPoint = new Vector2( centerBallWidth, centerBallHeight );


            Kinect kin = new Kinect( this, blue, red );
            kin.init();
            this.Components.Add( kin );
        }





        /**
         * I should probably put some code here :-/
         */
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }









        /**
         * updates! my favorite thing!
         */
        protected override void Update( GameTime gameTime )
        {
            GameState.Instance.totalMillis = gameTime.TotalGameTime.TotalMilliseconds;

            if ( GameState.Instance.State == State.PLAYING )
            {
                randomlyCreateEvents( gameTime );
            }

            GamePadState playerOne = GamePad.GetState( PlayerIndex.One );
            GamePadState playerTwo = GamePad.GetState( PlayerIndex.Two );

            // Allows the game to pause
            if (playerOne.Buttons.Start == ButtonState.Pressed || (playerTwo.IsConnected && playerTwo.Buttons.Start == ButtonState.Pressed))
            {
                GameState.Instance.handlePauseEvent(gameTime);
            }
            else
            {
                GameState.Instance.pauseButtonState = ButtonState.Released;
            }

            // handle events that occur in the menu state
            if ( GameState.Instance.State == State.MENU )
            {
                //primitive menu item selection, but works for now... while I only have two menu items
                if ( playerOne.ThumbSticks.Left.Y < 0f ) GameState.Instance.selectedMenuItem = MenuItem.MULTI;
                if ( playerOne.ThumbSticks.Left.Y > 0f ) GameState.Instance.selectedMenuItem = MenuItem.SINGLE;
                if ( playerOne.DPad.Down == ButtonState.Pressed ) GameState.Instance.selectedMenuItem = MenuItem.MULTI;
                if ( playerOne.DPad.Up == ButtonState.Pressed ) GameState.Instance.selectedMenuItem = MenuItem.SINGLE;

                if ( playerOne.Buttons.A == ButtonState.Pressed )
                {
                    if ( GameState.Instance.selectedMenuItem == MenuItem.MULTI )
                    {
                        if ( playerTwo.IsConnected || ( GameState.Instance.KinectReady && GameState.Instance.KinectEnabled ) )
                        {
                            GameState.Instance.players = 2;
                            GameState.Instance.Begin();
                        }
                    }
                    else
                    {
                        GameState.Instance.players = 1;
                        GameState.Instance.Begin();
                    }
                }
            }

            else if ( GameState.Instance.State == State.PLAYING )
            {

                GameState.Instance.blue.calculatePlayerMovement();
                GameState.Instance.red.calculatePlayerMovement();

                calculator.calculate();

                if ( GameState.Instance.blue.Score() >= POINTS_TO_WIN || GameState.Instance.red.Score() >= POINTS_TO_WIN )
                {
                    GameState.Instance.State = State.END;
                }
                else if (GameState.Instance.NumberOfBalls() == 0)
                {
                    GameState.Instance.spawnBall( null );
                }
            }

            base.Update( gameTime );
        }





         /**
          * This is called when the game should draw itself.
          */
        protected override void Draw( GameTime gameTime )
        {
            int elapsedMillis = gameTime.ElapsedGameTime.Milliseconds;

            GraphicsDevice.Clear( currentBackgroundColor );

            spriteBatch.Begin( SpriteSortMode.BackToFront, BlendState.AlphaBlend );


            //---------------------------------draw main elements of frame


            spriteBatch.Draw( GameState.Instance.getTexture( "border" ), Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f );
            //spriteBatch.Draw( border, new Vector2( 0, 0 ), Color.White );

            BlackHole[] holes = GameState.Instance.getBlackHoles();
            spriteBatch.Draw( GameState.Instance.getTexture( "blackhole" ), holes[0].trajectory.currentPosition, null, Color.White, holes[0].rotation, new Vector2( holes[0].radius, holes[0].radius ), 1f, SpriteEffects.None, 0.5f );
            spriteBatch.Draw( GameState.Instance.getTexture( "blackhole" ), holes[1].trajectory.currentPosition, null, Color.White, holes[1].rotation, new Vector2( holes[1].radius, holes[1].radius ), 1f, SpriteEffects.None, 0.5f );

            if ( GameState.Instance.State == State.MENU )
            {
                drawMenu( spriteBatch );
            }
            

            //we will still continue to process events and draw paddles & text items while the game is in any other state besides menu
            else
            {
                //draw paddles
                spriteBatch.Draw( bluePaddle.texture, bluePaddle.trajectory.currentPosition, null, Color.White, 0f, new Vector2( bluePaddle.texture.Width, 0f ), 1.0f, SpriteEffects.None, 0f );
                spriteBatch.Draw( redPaddle.texture, redPaddle.trajectory.currentPosition, null, Color.White, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0f );

                for ( int i = 0; i < textList.Count; ++i )
                {
                    TextItem text = textList.ElementAt( i );
                    if ( text.elapseMillis( elapsedMillis ) ) textList.RemoveAt( i );
                    
                    //draw the text items almost at the front, but allow paddles to overlap
                    else spriteBatch.DrawString( text.font, text.sequence, text.position, text.color, text.rotation, new Vector2(), text.scale, SpriteEffects.None, .1f );
                }
            }




            //----------------------draw special menu events

            //we have additional special draw commands that need to be handled when the game is over or paused
            if ( GameState.Instance.State == State.END )
            {
                drawGameOver();
            }
            else if (GameState.Instance.State == State.PAUSED)
            {
                spriteBatch.DrawString( GameState.Instance.getFont( "large" ), "so paus", new Vector2( 300, 300 ), Color.Snow, -.3f, Vector2.Zero, 1f, SpriteEffects.None, 0f );
                spriteBatch.DrawString( GameState.Instance.getFont( "large" ), "wow", new Vector2( 1000, 250 ), Color.Snow, .6f, Vector2.Zero, 1f, SpriteEffects.None, 0f );
                spriteBatch.Draw( GameState.Instance.getTexture( "dogehead" ), new Vector2( 640, 250 ), null, Color.White, .6f, Vector2.Zero, 1.0f, SpriteEffects.None, 0f );
            }



            //----------------------------draw the balls


            //only draw the balls if the game is playing
            if ( GameState.Instance.State == State.PLAYING )
            {
                //draw all of the active balls
                Texture2D texture = GameState.Instance.getTexture( "dogeball" );
                for ( int i = 0; i < GameState.Instance.NumberOfBalls(); ++i )
                {
                    DogeBall current = GameState.Instance.GetBall( i );
                    Vector2 relativeMidpoint = new Vector2( current.radius, current.radius );
                    spriteBatch.Draw( texture, current.trajectory.currentPosition + relativeMidpoint, null, Color.White, current.rotation, relativeMidpoint, 1.0f, SpriteEffects.None, 0f );
                }
            }
            spriteBatch.End();

            base.Draw( gameTime );
        }




        /**
         * game over
         */
        private void drawGameOver()
        {
            spriteBatch.Draw( GameState.Instance.getTexture( "dogehead" ), new Vector2( 700, 200 ), null, Color.White, 0.5f, Vector2.Zero, 1.0f, SpriteEffects.None, 0f );
            spriteBatch.DrawString( GameState.Instance.getFont( "large" ), "wow", new Vector2( 300, 300 ), Color.Red, -0.3f, Vector2.Zero, 1f, SpriteEffects.None, 0f );
            spriteBatch.DrawString( GameState.Instance.getFont( "large" ), "so bad", new Vector2( 1100, 300 ), Color.White, .2f, Vector2.Zero, 1f, SpriteEffects.None, 0f );
            spriteBatch.DrawString( GameState.Instance.getFont( "large" ), "much shame", new Vector2( 900, 700 ), Color.LawnGreen, -.6f, Vector2.Zero, 1f, SpriteEffects.None, 0f );
        }






        /**
         * draws the game menu while the game is in Menu state
         */
        private void drawMenu( SpriteBatch spriteBatch )
        {
            Color first = currentBackgroundColor == Color.Firebrick ? Color.Snow : Color.Firebrick;
            Color second = currentBackgroundColor == Color.Indigo ? Color.Snow : Color.Indigo;
            spriteBatch.DrawString( GameState.Instance.getFont("large"), "uno doge", new Vector2( 620, 350 ), first, 0f, new Vector2(), 1f, SpriteEffects.None, .1f );
            spriteBatch.DrawString( GameState.Instance.getFont( "large" ), "dos doges", new Vector2( 600, 450 ), second, 0f, new Vector2(), 1f, SpriteEffects.None, .1f );

            if ( !GameState.Instance.KinectEnabled )
            {
                spriteBatch.Draw( GameState.Instance.getTexture( "kinectdisable" ), new Vector2( 1200, 600 ), null, Color.White, 0f, new Vector2(), 1.0f, SpriteEffects.None, 0f );
            }
            else if ( GameState.Instance.KinectReady )
            {
                spriteBatch.Draw( GameState.Instance.getTexture( "kinectready" ), new Vector2( 1200, 600 ), null, Color.White, 0f, new Vector2(), 1.0f, SpriteEffects.None, 0f );
            }
            else
            {
                spriteBatch.Draw( GameState.Instance.getTexture( "kinect" ), new Vector2( 1200, 600 ), null, Color.White, 0f, new Vector2(), 1.0f, SpriteEffects.None, 0f );
            }

            if ( GameState.Instance.selectedMenuItem == MenuItem.MULTI && !GameState.Instance.red.isGamePadConnected() && ( !GameState.Instance.KinectEnabled || !GameState.Instance.KinectReady ) )
            {
                spriteBatch.DrawString( GameState.Instance.getFont( "large" ), "no wai", new Vector2( 650, 520 ), Color.Ivory, -.6f, new Vector2(), 1f, SpriteEffects.None, 0f );
            }

            Vector2 firstPosition = new Vector2( 530, 385 );
            Vector2 secondPosition = new Vector2( 530, 485 );
            spriteBatch.Draw( GameState.Instance.getTexture( "dogeball" ), ( GameState.Instance.selectedMenuItem == MenuItem.SINGLE ? firstPosition : secondPosition ), Color.White );

            spriteBatch.Draw( GameState.Instance.getTexture( "dogehead" ), new Vector2( 840, 250 ), null, Color.White, -.6f, new Vector2(), 1.0f, SpriteEffects.None, 0f );
            spriteBatch.DrawString( GameState.Instance.getFont( "large" ), "wow", new Vector2( 300, 200 ), Color.Snow, -.4f, new Vector2(), 1f, SpriteEffects.None, 0f );
            spriteBatch.DrawString( GameState.Instance.getFont( "large" ), "such choice", new Vector2( 400, 500 ), Color.Red, .6f, new Vector2(), 1f, SpriteEffects.None, 0f );
        }





        /**
         * adds a text item to the list of text items that are drawn each round
         */
        private void generateTextItem( String sequence )
        {
            Random rnd = new Random();
            int height = (int) graphics.GraphicsDevice.Viewport.Height;
            int width = (int) graphics.GraphicsDevice.Viewport.Width;
            int randy = 0;

            String s = sequence;
            if ( s == null )
            {
                randy = rnd.Next( 19 );
                switch ( randy )
                {
                    case 0: s = "wow";
                        break;
                    case 1: s = "very excite";
                        break;
                    case 2: s = "such game";
                        break;
                    case 3: s = "so clos";
                        break;
                    case 4: s = "much fun";
                        break;
                    case 5: s = "many bounce";
                        break;
                    case 6: s = "so pong";
                        break;
                    case 7: s = "game";
                        break;
                    case 8: s = "hi";
                        break;
                    case 9: s = "don't read this";
                        break;
                    case 10: s = "words";
                        break;
                    case 11: s = "such program";
                        break;
                    case 12: s = "very suspense";
                        break;
                    case 13: s = "omg";
                        break;
                    case 14: s = "much graphx";
                        break;
                    case 15: s = "so fetch";
                        break;
                    case 16: s = "lulz";
                        break;
                    case 17: s = "whee";
                        break;
                    case 18: s = "dis gaem liek whoe";
                        break;
                }
            }

            SpriteFont font = GameState.Instance.getFont( "large" );
            //randy = rnd.Next( 3 );
            //switch( randy )
            //{
            //    case 0: font = GameState.Instance.getFont( "small" );
            //        break;
            //    case 1: font = GameState.Instance.getFont( "regular" );
            //        break;
            //    case 2: font = GameState.Instance.getFont( "large" );
            //        break;
            //}

            Vector2 pos = new Vector2( rnd.Next( width + 100 ) - 100, rnd.Next( height + 30 ) - 30 );
            int millis = rnd.Next( 10001 );
            float rotation = (float) rnd.NextDouble() * 5;
            float scale = (float) rnd.NextDouble() * 2;
            Color color = getRandomColor();

            TextItem item = new TextItem( s, font, pos, scale, color, millis, rotation );
            textList.Add( item );
        }




        /**
         * handles the creation of random events within the game
         */
        public void randomlyCreateEvents( GameTime gameTime )
        {
            long currentSeconds = (long) gameTime.TotalGameTime.TotalSeconds;
            if ( GameState.Instance.State == State.MENU || GameState.Instance.State == State.END ) return;
            if ( currentSeconds > elapsedTime )
            {
                elapsedTime = currentSeconds;

                //randomize the background color every 3 seconds
                if ( elapsedTime % 3 == 0 )
                {
                    randomizeBackground();
                }

                //create a new ball every 5 seconds
                if ( elapsedTime % 5 == 0 )
                {
                    GameState.Instance.spawnBall( null );
                }

                //create a new text item every second
                //if ( elapsedTime % 1 == 0 )
                {
                    generateTextItem( null );
                }
            }
        }


        /**
         * sets the background to a new random color
         */
        private void randomizeBackground()
        {
            Random rnd = new Random();
            bool picked = false;
            while ( !picked )
            {
                Color selected = getRandomColor();
                if ( !selected.Equals( currentBackgroundColor ) )
                {
                    picked = true;
                    currentBackgroundColor = selected;
                }
            }
        }



        /**
         * returns a random color
         */
        private Color getRandomColor()
        {
            Random rnd = new Random();
            int randy = rnd.Next( 8 );
            switch ( randy )
            {
                case 0:
                    return Color.DarkSlateBlue;
                case 1:
                    return Color.Firebrick;
                case 2:
                    return Color.Indigo;
                case 3:
                    return Color.DarkRed;
                case 4:
                    return Color.Pink;
                case 5:
                    return Color.PowderBlue;
                case 6:
                    return Color.Plum;
                case 7:
                    return Color.DarkOrange;
                case 8:
                    return Color.WhiteSmoke;
            }

            return Color.White;
        }
    }





}

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

namespace DogePong
{
    public enum State
    {
        MENU,
        PLAYING,
        PAUSED,
        END
    };

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

        public static float northBoundary;
        public static float southBoundary;

        //private bool gameOver = false;
        private State state = State.MENU;
        private MenuItem selected;

        static GraphicsDeviceManager graphics;
        static SpriteBatch spriteBatch;

        //font used for writing
        static SpriteFont largefont;
        static SpriteFont smallfont;
        static SpriteFont regularfont;

        //basic game textures
        static Texture2D border;
        static Texture2D ball_texture;
        static Texture2D doge_head;

        //players
        Player blue;
        Player red;
        
        //player paddles
        Paddle bluePaddle;
        Paddle redPaddle;

        TextItem blueScore;
        TextItem redScore;

        //a list of the text sprites
        List<TextItem> textList;

        //array for balls
        private DogeBall[] dogeBalls;

        //collision calculator used to handle the correct collision timings.
        private CollisionCalculator calculator;

        //dynamic game properties
        private ButtonState pauseButtonState = ButtonState.Released;
        private int players = 1;
        private Vector2 neutralSpawningPoint;
        private int activeBalls = 0;
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

            //initialize font
            smallfont = Content.Load<SpriteFont>( "ComicSansSmall" );
            regularfont = Content.Load<SpriteFont>( "ComicSans" );
            largefont = Content.Load<SpriteFont>( "ComicSansLarge" );

            textList = new List<TextItem>();
            blueScore = new TextItem( "0", regularfont, new Vector2( 680f, 15f ), Color.White, 0 ); ;
            redScore = new TextItem( "0", regularfont, new Vector2( 900f, 15f ), Color.White, 0 );
            textList.Add( blueScore );
            textList.Add( redScore );

            //initialize border Texture
            border = Content.Load<Texture2D>( "border" );
            doge_head = Content.Load<Texture2D>( "dogehead" );
            randomizeBackground();

            float areaHeight = graphics.GraphicsDevice.Viewport.Height;
            float areaWidth = graphics.GraphicsDevice.Viewport.Width;

            Boundary gameBoundary = new Boundary( BOUNDARY_DENSITY, areaHeight, areaWidth );

            //--------------initialize paddles & their players

            //initialize paddle Textures
            Texture2D blueTexture = Content.Load<Texture2D>( "dogepaddle_blue" );
            Texture2D redTexture = Content.Load<Texture2D>( "dogepaddle_red" );

            //initialize player paddle positions to the middle of the screen
            Vector2 bluePosition = new Vector2( BOUNDARY_DENSITY + blueTexture.Width, ( areaHeight - blueTexture.Height ) / 2.0f );
            Vector2 redPosition = new Vector2( areaWidth - BOUNDARY_DENSITY - redTexture.Width, ( graphics.GraphicsDevice.Viewport.Height - blueTexture.Height ) / 2.0f );

            //initialize paddle Trajectory objects using their initial positions
            Trajectory blueTrajectory = new Trajectory( bluePosition );
            Trajectory redTrajectory = new Trajectory( redPosition );

            //initialize paddles
            bluePaddle = new Paddle( blueTrajectory, blueTexture );
            redPaddle = new Paddle( redTrajectory, redTexture );

            //initialize players
            blue = new Player( PlayerIndex.One, bluePaddle, blueScore );
            red = new Player( PlayerIndex.Two, redPaddle, redScore );
            //red = new Player( PlayerIndex.One, redPaddle, redScore );

            //initialize collision engine
            calculator = new CollisionCalculator( gameBoundary, blue, red );

            //--------------initialize balls

            //initialize ball texture
            ball_texture = Content.Load<Texture2D>( "dogeball" );

            //initialize ball array
            dogeBalls = new DogeBall[ MAX_BALLS ];

            //initialize first ball with a position directly in the middle of the screen
            float centerBallWidth = ( graphics.GraphicsDevice.Viewport.Width - ball_texture.Width ) / 2.0f;
            float centerBallHeight = ( graphics.GraphicsDevice.Viewport.Height - ball_texture.Height ) / 2.0f;
            neutralSpawningPoint = new Vector2( centerBallWidth, centerBallHeight );

            //spawn the initial ball
            spawnBall( neutralSpawningPoint );
            
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }









        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update( GameTime gameTime )
        {
            //define the north and south walls, for more sensible collision detection code
            northBoundary = BOUNDARY_DENSITY;
            southBoundary = ( graphics.GraphicsDevice.Viewport.Height - BOUNDARY_DENSITY );

            GamePadState playerOne = GamePad.GetState( PlayerIndex.One );
            GamePadState playerTwo = GamePad.GetState( PlayerIndex.Two );

            // Allows the game to pause
            if (playerOne.Buttons.Start == ButtonState.Pressed || (playerTwo.IsConnected && playerTwo.Buttons.Start == ButtonState.Pressed))
            {
                handlePauseEvent(gameTime);
            }
            else
            {
                pauseButtonState = ButtonState.Released;
            }

            // handle events that occur in the menu state
            if ( state == State.MENU )
            {
                //primitive menu item selection, but works for now... while I only have two menu items
                if ( playerOne.ThumbSticks.Left.Y < 0f ) selected = MenuItem.MULTI;
                if ( playerOne.ThumbSticks.Left.Y > 0f ) selected = MenuItem.SINGLE;
                if ( playerOne.DPad.Down == ButtonState.Pressed ) selected = MenuItem.MULTI;
                if ( playerOne.DPad.Up == ButtonState.Pressed ) selected = MenuItem.SINGLE;

                if ( playerOne.Buttons.A == ButtonState.Pressed )
                {
                    if ( selected == MenuItem.MULTI )
                    {
                        if (playerTwo.IsConnected)
                        {
                            players = 2;
                            state = State.PLAYING;
                        }
                    }
                    else
                    {
                        state = State.PLAYING;
                        players = 1;
                        red = new ComputerPlayer( dogeBalls, PlayerIndex.Two, red.paddle, red.scoreText );
                    }
                }
            }

            else if (state == State.PLAYING)
            {

                blue.calculatePlayerMovement();
                red.calculatePlayerMovement();

                activeBalls = calculator.calculate(dogeBalls, activeBalls);

                if (blue.Score() >= POINTS_TO_WIN || red.Score() >= POINTS_TO_WIN)
                {
                    state = State.END;
                }
                else if (activeBalls == 0)
                {
                    spawnBall(neutralSpawningPoint);
                }
            }

            base.Update( gameTime );
        }





        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw( GameTime gameTime )
        {
            int elapsedMillis = gameTime.ElapsedGameTime.Milliseconds;

            GraphicsDevice.Clear( currentBackgroundColor );

            spriteBatch.Begin( SpriteSortMode.BackToFront, BlendState.AlphaBlend );

            spriteBatch.Draw(border, new Vector2(), null, Color.White, 0f, new Vector2(), 1f, SpriteEffects.None, 1f);
            //spriteBatch.Draw( border, new Vector2( 0, 0 ), Color.White );

            if ( state == State.MENU )
            {
                drawMenu( spriteBatch );
            }
            
            //we will still continue to process events and draw paddles & text items while the game is in any other state besides menu
            else
            {
                randomlyCreateEvents( gameTime );

                //draw paddles
                spriteBatch.Draw( bluePaddle.texture, bluePaddle.trajectory.currentPosition, null, Color.White, 0f, new Vector2( bluePaddle.texture.Width, 0f ), 1.0f, SpriteEffects.None, 0f );
                spriteBatch.Draw( redPaddle.texture, redPaddle.trajectory.currentPosition, null, Color.White, 0f, new Vector2( 0f, 0f ), 1.0f, SpriteEffects.None, 0f );

                for ( int i = 0; i < textList.Count; ++i )
                {
                    TextItem text = textList.ElementAt( i );
                    if ( text.elapseMillis( elapsedMillis ) ) textList.RemoveAt( i );
                    
                    //draw the text items almost at the front, but allow paddles to overlap
                    else spriteBatch.DrawString( text.font, text.sequence, text.position, text.color, text.rotation, new Vector2(), text.scale, SpriteEffects.None, .1f );
                }
            }

            //we have additional special draw commands that need to be handled when the game is over or paused
            if ( state == State.END )
            {
                spriteBatch.DrawString( largefont, "so game over", new Vector2( 700, 300 ), Color.White );
            }
            else if (state == State.PAUSED)
            {
                spriteBatch.DrawString(largefont, "so paus", new Vector2(300, 300), Color.Snow, -.3f, new Vector2(), 1f, SpriteEffects.None, 0f);
                spriteBatch.DrawString(largefont, "wow", new Vector2(1000, 250), Color.Snow, .6f, new Vector2(), 1f, SpriteEffects.None, 0f);
                spriteBatch.Draw(doge_head, new Vector2(640, 250), null, Color.White, .6f, new Vector2(), 1.0f, SpriteEffects.None, 0f);
            }

            //only draw the balls if the game is playing
            if ( state == State.PLAYING )
            {
                //draw all of the active balls
                float radius = DogeBall.texture.Width / 2f;
                Vector2 midpoint = new Vector2( radius, radius );
                for ( int i = 0; i < activeBalls; ++i )
                {
                    //float rotation = gameTime.TotalGameTime.Milliseconds / 500;
                    //rotation = rotation % ( MathHelper.Pi * 2 );
                    spriteBatch.Draw( DogeBall.texture, dogeBalls[i].trajectory.currentPosition + midpoint, null, Color.White, dogeBalls[i].rotation, midpoint, 1.0f, SpriteEffects.None, 0f );
                }
            }
            spriteBatch.End();

            base.Draw( gameTime );
        }









        /**
         * handles toggling the paused state of the game
         */
        private void handlePauseEvent(GameTime gameTime)
        {
            //only allow the event to be processed once for each individual button press (avoids lightning-fast pause toggle due to update loop speed)
            if (pauseButtonState == ButtonState.Released)
            {
                pauseButtonState = ButtonState.Pressed;
                if (state == State.PLAYING)
                {
                    state = State.PAUSED;
                }
                else if (state == State.PAUSED)
                {
                    state = State.PLAYING;
                    long currentSeconds = (long)gameTime.TotalGameTime.TotalSeconds;
                }
            }
        }






        /**
         * draws the game menu while the game is in Menu state
         */
        private void drawMenu( SpriteBatch spriteBatch )
        {
            Color first = currentBackgroundColor == Color.Firebrick ? Color.Snow : Color.Firebrick;
            Color second = currentBackgroundColor == Color.Indigo ? Color.Snow : Color.Indigo;
            spriteBatch.DrawString(largefont, "uno doge", new Vector2(620, 350), first, 0f, new Vector2(), 1f, SpriteEffects.None, .1f );
            spriteBatch.DrawString( largefont, "dos doges", new Vector2( 600, 450 ), second, 0f, new Vector2(), 1f, SpriteEffects.None, .1f );

            if (selected == MenuItem.MULTI && !red.isConnected())
            {
                spriteBatch.DrawString(largefont, "no wai", new Vector2(650, 520), Color.Ivory, -.6f, new Vector2(), 1f, SpriteEffects.None, 0f);
            }

            Vector2 firstPosition = new Vector2( 530, 385 );
            Vector2 secondPosition = new Vector2( 530, 485 );
            spriteBatch.Draw( DogeBall.texture, ( selected == MenuItem.SINGLE ? firstPosition : secondPosition ), Color.White );

            spriteBatch.Draw( doge_head, new Vector2( 840, 250 ), null, Color.White, -.6f, new Vector2(), 1.0f, SpriteEffects.None, 0f );
            spriteBatch.DrawString( largefont, "wow", new Vector2( 300, 200 ), Color.Snow, -.4f, new Vector2(), 1f, SpriteEffects.None, 0f );
            spriteBatch.DrawString( largefont, "such choice", new Vector2( 400, 500 ), Color.Red, .6f, new Vector2(), 1f, SpriteEffects.None, 0f );
        }




        /**
         * spawns a ball with an initial position and velocity
         */
        public void spawnBall( Vector2 position )
        {
            if ( activeBalls == MAX_BALLS ) return;
            Random rnd = new Random();
            int coin = rnd.Next() % 2;
            Vector2 ballVelocity = ( coin == 1 ? new Vector2( -5.0f, 0.0f ) : new Vector2( 5.0f, 0.0f ) );
            dogeBalls[activeBalls++] = new DogeBall( ball_texture, new Trajectory( position, ballVelocity ) );
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
                    case 15: s = "so fetch.";
                        break;
                    case 16: s = "lulz";
                        break;
                    case 17: s = "whee";
                        break;
                    case 18: s = "dis gaem liek whoe";
                        break;
                }
            }

            SpriteFont font = regularfont;
            randy = rnd.Next( 3 );
            switch( randy )
            {
                case 0: font = smallfont;
                    break;
                case 1: font = regularfont;
                    break;
                case 2: font = largefont;
                    break;
            }

            Vector2 pos = new Vector2( rnd.Next( width + 100 ) - 100, rnd.Next( height + 30 ) - 30 );
            int millis = rnd.Next( 10001 );
            float rotation = (float) rnd.NextDouble() * 5;
            float scale = (float) rnd.NextDouble() * 3;
            Color color = getRandomColor();

            TextItem item = new TextItem( s, font, pos, scale, color, millis, rotation );
            textList.Add( item );
        }




        /**
         * handles the creation of random events within the game
         */
        public void randomlyCreateEvents( GameTime gameTime )
        {
            if ( state == State.MENU || state == State.END ) return;
            long currentSeconds = (long) gameTime.TotalGameTime.TotalSeconds;
            if ( currentSeconds > elapsedTime )
            {
                elapsedTime = currentSeconds;

                //randomize the background color every 10 seconds
                if ( elapsedTime % 10 == 0 )
                {
                    randomizeBackground();
                }

                //create a new ball every 8 seconds
                if ( elapsedTime % 8 == 0 )
                {
                    spawnBall( neutralSpawningPoint );
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

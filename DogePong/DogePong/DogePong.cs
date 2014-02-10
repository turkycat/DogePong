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
    using Controllers;
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
        private MenuItem selected;

        static GraphicsDeviceManager graphics;
        static SpriteBatch spriteBatch;

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

        //collision calculator used to handle the correct collision timings.
        private CollisionCalculator calculator;

        //dynamic game properties
        private ButtonState pauseButtonState = ButtonState.Released;
        private int players = 1;
        private Vector2 neutralSpawningPoint;
        //private int activeBalls = 0;
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


            GameState.Instance.addFont( "small", Content.Load<SpriteFont>( "ComicSansSmall" ) );
            GameState.Instance.addFont( "regular", Content.Load<SpriteFont>( "ComicSans" ) );
            GameState.Instance.addFont( "large", Content.Load<SpriteFont>( "ComicSansLarge" ) );

            textList = new List<TextItem>();
            blueScore = new TextItem( "0", GameState.Instance.getFont( "regular" ), new Vector2( 680f, 15f ), Color.White, 0 ); ;
            redScore = new TextItem( "0", GameState.Instance.getFont( "regular" ), new Vector2( 900f, 15f ), Color.White, 0 );
            textList.Add( blueScore );
            textList.Add( redScore );

            //choose an initial background
            randomizeBackground();

            float areaHeight = graphics.GraphicsDevice.Viewport.Height;
            float areaWidth = graphics.GraphicsDevice.Viewport.Width;

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
            Controller redController = new ComputerController();

            //initialize players
            blue = new Player( blueController, PlayerIndex.One, bluePaddle, blueScore );
            red = new Player( redController, PlayerIndex.Two, redPaddle, redScore );
            //red = new Player( PlayerIndex.One, redPaddle, redScore );

            //initialize collision engine
            calculator = new CollisionCalculator( gameBoundary, blue, red );

            //--------------initialize a ball

            //initialize first ball with a position directly in the middle of the screen
            float centerBallWidth = ( graphics.GraphicsDevice.Viewport.Width - GameState.Instance.getTexture( "dogeball" ).Width ) / 2.0f;
            float centerBallHeight = ( graphics.GraphicsDevice.Viewport.Height - GameState.Instance.getTexture( "dogeball" ).Height ) / 2.0f;
            neutralSpawningPoint = new Vector2( centerBallWidth, centerBallHeight );

            //spawn the initial ball
            GameState.Instance.spawnBall( neutralSpawningPoint );
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
            if ( GameState.Instance.State == State.MENU )
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
                            red.setController( new GamePadController( PlayerIndex.Two ) );
                            GameState.Instance.State = State.PLAYING;
                        }
                    }
                    else
                    {
                        GameState.Instance.State = State.PLAYING;
                        players = 1;
                        //red = new ComputerPlayer( PlayerIndex.Two, red.paddle, red.scoreText );
                    }
                }
            }

            else if ( GameState.Instance.State == State.PLAYING )
            {

                blue.calculatePlayerMovement();
                red.calculatePlayerMovement();

                calculator.calculate();

                if (blue.Score() >= POINTS_TO_WIN || red.Score() >= POINTS_TO_WIN)
                {
                    GameState.Instance.State = State.END;
                }
                else if (GameState.Instance.NumberOfBalls() == 0)
                {
                    GameState.Instance.spawnBall( neutralSpawningPoint );
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

            spriteBatch.Draw( GameState.Instance.getTexture( "border" ), new Vector2(), null, Color.White, 0f, new Vector2(), 1f, SpriteEffects.None, 1f );
            //spriteBatch.Draw( border, new Vector2( 0, 0 ), Color.White );

            if ( GameState.Instance.State == State.MENU )
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
            if ( GameState.Instance.State == State.END )
            {
                spriteBatch.DrawString( GameState.Instance.getFont( "large" ), "so game over", new Vector2( 700, 300 ), Color.White );
            }
            else if (GameState.Instance.State == State.PAUSED)
            {
                spriteBatch.DrawString( GameState.Instance.getFont( "large" ), "so paus", new Vector2( 300, 300 ), Color.Snow, -.3f, new Vector2(), 1f, SpriteEffects.None, 0f );
                spriteBatch.DrawString( GameState.Instance.getFont( "large" ), "wow", new Vector2( 1000, 250 ), Color.Snow, .6f, new Vector2(), 1f, SpriteEffects.None, 0f );
                spriteBatch.Draw(GameState.Instance.getTexture( "dogehead" ), new Vector2(640, 250), null, Color.White, .6f, new Vector2(), 1.0f, SpriteEffects.None, 0f);
            }

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
         * handles toggling the paused state of the game
         */
        private void handlePauseEvent(GameTime gameTime)
        {
            //only allow the event to be processed once for each individual button press (avoids lightning-fast pause toggle due to update loop speed)
            if (pauseButtonState == ButtonState.Released)
            {
                pauseButtonState = ButtonState.Pressed;
                if ( GameState.Instance.State == State.PLAYING )
                {
                    GameState.Instance.State = State.PAUSED;
                }
                else if ( GameState.Instance.State == State.PAUSED )
                {
                    GameState.Instance.State = State.PLAYING;
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
            spriteBatch.DrawString( GameState.Instance.getFont("large"), "uno doge", new Vector2( 620, 350 ), first, 0f, new Vector2(), 1f, SpriteEffects.None, .1f );
            spriteBatch.DrawString( GameState.Instance.getFont( "large" ), "dos doges", new Vector2( 600, 450 ), second, 0f, new Vector2(), 1f, SpriteEffects.None, .1f );

            if (selected == MenuItem.MULTI && !red.isGamePadConnected())
            {
                spriteBatch.DrawString( GameState.Instance.getFont( "large" ), "no wai", new Vector2( 650, 520 ), Color.Ivory, -.6f, new Vector2(), 1f, SpriteEffects.None, 0f );
            }

            Vector2 firstPosition = new Vector2( 530, 385 );
            Vector2 secondPosition = new Vector2( 530, 485 );
            spriteBatch.Draw( GameState.Instance.getTexture("dogeball"), ( selected == MenuItem.SINGLE ? firstPosition : secondPosition ), Color.White );

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

            SpriteFont font = GameState.Instance.getFont( "regular" );
            randy = rnd.Next( 3 );
            switch( randy )
            {
                case 0: font = GameState.Instance.getFont( "small" );
                    break;
                case 1: font = GameState.Instance.getFont( "regular" );
                    break;
                case 2: font = GameState.Instance.getFont( "large" );
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
            if ( GameState.Instance.State == State.MENU || GameState.Instance.State == State.END ) return;
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
                    GameState.Instance.spawnBall( neutralSpawningPoint );
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

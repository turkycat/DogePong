using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using Microsoft.Xna.Framework.Media;

namespace DogePong
{
    public enum State
    {
        MENU,
        PLAYING,
        PAUSED,
        END
    };

    /**
     * this is a Singleton class, containing vital game information which can be dynamically changed and accessed from anywhere within namespace DogePong
     */
    public class GameState
    {
        private static GameState properties;

        private Dictionary<string, Texture2D> textures;
        private Dictionary<string, SpriteFont> fonts;

        private DogeBall[] balls;
        private int activeBalls;

        public Random randy;
        public State State { get; set; }
        public int GameHeight;
        public int GameWidth;
        public bool KinectReady;

        public double totalMillis;

        private BlackHole[] blackHoles;


        private GameState()
        {
            this.textures = new Dictionary<string, Texture2D>();
            this.fonts = new Dictionary<string, SpriteFont>();
            this.State = State.MENU;
            this.randy = new Random();
            this.balls = new DogeBall[ DogePong.MAX_BALLS ];
            this.activeBalls = 0;
            this.KinectReady = false;
        }

        //the idea of using the singleton class as a texture reference goes to James Boddie

        #region Texture related Methods

        public void addTexture( string key, Texture2D texture )
        {
            textures.Add( key, texture );
        }

        public Texture2D getTexture( string key )
        {
            if ( !textures.ContainsKey( key ) ) return null;
            return textures[key];
        }

        public void addFont( string key, SpriteFont texture )
        {
            fonts.Add( key, texture );
        }

        public SpriteFont getFont( string key )
        {
            if ( !fonts.ContainsKey( key ) ) return null;
            return fonts[key];
        }

        #endregion


        #region Ball Related Methods

        //----------public

        //public void SpawnBall( Vector2 position )
        //{
        //    if( activeBalls == DogePong.MAX_BALLS ) return;
        //    balls[activeBalls++] = new DogeBall( new Trajectory( position ) );
        //}





        /**
         * spawns a ball with an initial position and velocity
         */
        public void spawnBall( Vector2 position )
        {
            if ( activeBalls == DogePong.MAX_BALLS ) return;
            Vector2 ballVelocity = generateRandomVelocity() * 10;
            balls[activeBalls++] = new DogeBall( new Trajectory( position, ballVelocity ) );
        }

        public int NumberOfBalls()
        {
            return activeBalls;
        }

        /**
         * returns a ball
         */
        public DogeBall GetBall( int i )
        {
            if( i >= activeBalls ) return null;
            return balls[i];
        }

        public bool RemoveBall( int i )
        {
            if( i >= activeBalls )
            {
                return false;
            }

            swap( i, activeBalls - 1 );
            --activeBalls;
            return true;
        }


        // a couple methods for the black holes

        public BlackHole[] getBlackHoles()
        {
            if ( blackHoles == null )
            {
                blackHoles = new BlackHole[2];
                float radius = getTexture( "blackhole" ).Width;
                blackHoles[0] = new BlackHole( new Trajectory( generateRandomLocation( radius * 2, radius * 2 ), generateRandomVelocity() ) );
                blackHoles[1] = new BlackHole( new Trajectory( generateRandomLocation( radius * 2, radius * 2 ), generateRandomVelocity() ) );
            }
            return blackHoles;
        }


        
        //--------private ball related methods

        private void swap( int i, int j )
        {
            DogeBall temp = balls[i];
            balls[i] = balls[j];
            balls[j] = temp;
        }
        
    #endregion


        #region Utility Methods

        /**
         * generates a valid position for a sprite of given size, use 0f for any valid point in bounds
         */
        public Vector2 generateRandomLocation( float width, float height )
        {
            return new Vector2( randy.Next( (int) DogePong.BOUNDARY_DENSITY, (int) ( GameWidth - DogePong.BOUNDARY_DENSITY - width ) ), randy.Next( (int) ( GameHeight - DogePong.BOUNDARY_DENSITY - height ) ) );
        }


        /**
         * generates a valid position for a sprite of given size, use 0f for any valid point in bounds
         */
        public Vector2 generateRandomVelocity()
        {
            float x = (float) ( randy.NextDouble() - 0.5 );
            float y = (float) ( randy.NextDouble() - 0.5 );
            return new Vector2( x, y );
        }



    #endregion


        public static GameState Instance
        {
            get
            {
                if ( properties == null )
                {
                    properties = new GameState();
                }
                return properties;
            }
        }
    }
}

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
        private Random randy;

        public State State { get; set; }

        private Dictionary<string, Texture2D> textures;
        private Dictionary<string, SpriteFont> fonts;

        private DogeBall[] balls;
        private int activeBalls;


        private GameState()
        {
            State = State.MENU;
            randy = new Random();
            this.balls = new DogeBall[ DogePong.MAX_BALLS ];
            this.activeBalls = 0;
            textures = new Dictionary<string,Texture2D>();
            fonts = new Dictionary<string, SpriteFont>();
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

        public void SpawnBall( Vector2 position )
        {
            if( activeBalls == DogePong.MAX_BALLS ) return;
            balls[activeBalls++] = new DogeBall( new Trajectory( position ) );
        }





        /**
         * spawns a ball with an initial position and velocity
         */
        public void spawnBall( Vector2 position )
        {
            if ( activeBalls == DogePong.MAX_BALLS ) return;
            int coin = randy.Next() % 2;
            Vector2 ballVelocity = ( coin == 1 ? new Vector2( -5.0f, 0.0f ) : new Vector2( 5.0f, 0.0f ) );
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


        
        //--------private ball related methods

        private void swap( int i, int j )
        {
            DogeBall temp = balls[i];
            balls[i] = balls[j];
            balls[j] = temp;
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

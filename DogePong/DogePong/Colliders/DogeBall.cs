using DogePong.Colliders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DogePong
{
    public class DogeBall : SphericalCollider
    {
        public double lastTeleport;
        public double lastCollision;

#region Constructors

        //public DogeBall( Texture2D image, Vector2 position ) : this( image, position, new Vector2( 0.0f, 0.0f ) ) { }

        public DogeBall( Trajectory trajectory ) : base( trajectory )
        {
            this.elapsedTime = 0f;
            this.radius = GameState.Instance.getTexture( "dogeball" ).Width / 2f;
            this.lastTeleport = 0;
            this.lastCollision = 0;
        }

#endregion


        /**
         * teleport the ball to the given location, as long as it has passed a certain amount of "grace period" since the last teleport
         */
        public void teleport( Vector2 newPosition )
        {
            if ( GameState.Instance.totalMillis - lastTeleport > 500 )
            {
                playTeleportSound();
                lastTeleport = GameState.Instance.totalMillis;
                trajectory.currentPosition = new Vector2( newPosition.X, newPosition.Y );
                trajectory.nextPosition = new Vector2( newPosition.X, newPosition.Y );
                //choose a random velocity
                trajectory.currentVelocity = GameState.Instance.generateRandomVelocity() * 30;
            }
        }


        /**
         * updates both ICollidable objects to the collision point, calculates the new velocity based on reflection
         *  and recalculates their next position given the new velocity
         */
        public override void collide( ICollidable other, float collisionTime )
        {
            playCollisionSound();
            //we can assume that collisions will be called in order of earliest to latest, but safe > sorry
            if ( elapsedTime < collisionTime )
            {
                this.weightedMovement( collisionTime - elapsedTime );
                elapsedTime = collisionTime;
            }
        }




        private void playCollisionSound()
        {
            //we will only make a sound once per every set amount of time
            if ( GameState.Instance.totalMillis - lastCollision > 100 )
            {
                lastCollision = GameState.Instance.totalMillis;
                switch ( GameState.Instance.randy.Next( 4 ) )
                {
                    case 0:
                        GameState.Instance.getSound( "boop0" ).Play();
                        break;
                    case 1:
                        GameState.Instance.getSound( "boop1" ).Play();
                        break;
                    case 2:
                        GameState.Instance.getSound( "boop2" ).Play();
                        break;
                    case 3:
                        GameState.Instance.getSound( "boop3" ).Play();
                        break;
                }
            }
        }



        private void playTeleportSound()
        {
            switch ( GameState.Instance.randy.Next( 3 ) )
            {
                case 0:
                    GameState.Instance.getSound( "port0" ).Play();
                    break;
                case 1:
                    GameState.Instance.getSound( "port1" ).Play();
                    break;
                case 2:
                    GameState.Instance.getSound( "port2" ).Play();
                    break;
            }
        }


        /**
         * reflects the current velocity across the given normal and sets this value as the new velocity.
         *  - also recomputes the next potential position given the remaining velocity magnitude
         */
        public override void reflect( Vector2 normal )
        {
            Vector2 oldVelocity = new Vector2( trajectory.currentVelocity.X, trajectory.currentVelocity.Y );
            trajectory.currentVelocity = Vector2.Reflect( trajectory.currentVelocity, normal );

            if ( oldVelocity.Equals( trajectory.currentVelocity ) )
            {
                trajectory.currentVelocity = new Vector2( -trajectory.currentVelocity.X, trajectory.currentVelocity.Y );
            }

            trajectory.calculateNextPosition( getRemainingTime() );
        }
    }
}

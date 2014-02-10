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

#region Constructors

        //public DogeBall( Texture2D image, Vector2 position ) : this( image, position, new Vector2( 0.0f, 0.0f ) ) { }

        public DogeBall( Trajectory trajectory ) : base( trajectory )
        {
            this.elapsedTime = 0f;
            this.radius = GameState.Instance.getTexture("dogeball").Width / 2f;
        }

#endregion

        public void teleport( Vector2 newPosition )
        {
            if ( GameState.Instance.totalMillis - lastTeleport > 500 )
            {
                lastTeleport = GameState.Instance.totalMillis;
                trajectory.currentPosition = new Vector2( newPosition.X, newPosition.Y );
                trajectory.nextPosition = new Vector2( newPosition.X, newPosition.Y );
                trajectory.currentVelocity = GameState.Instance.generateRandomVelocity() * 10;
            }
        }


        /**
         * updates both ICollidable objects to the collision point, calculates the new velocity based on reflection
         *  and recalculates their next position given the new velocity
         */
        public override void collide( ICollidable other, float collisionTime )
        {
            this.weightedMovement( collisionTime );

            //we can assume that collisions will be called in order of earliest to latest, but safe > sorry
            if ( elapsedTime < collisionTime )
            {
                elapsedTime = collisionTime;
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

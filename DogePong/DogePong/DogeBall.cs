using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DogePong
{
    public class DogeBall : ICollidable
    {
        //public static Texture2D texture { get; set; }
        public Trajectory trajectory;

        private float elapsedTime;

        public float rotation { get; set; }
        public float rotationVelocity { get; set; }

        public Vector2 midpoint
        {
            get
            {
                return new Vector2( trajectory.currentPosition.X + radius, trajectory.currentPosition.Y + radius );
            }
        }
        public Vector2 nextMidpoint
        {
            get
            {
                return new Vector2( trajectory.nextPosition.X + radius, trajectory.nextPosition.Y + radius );
            }
        }

        public float radius { get; set; }



#region Constructors

        //public DogeBall( Texture2D image, Vector2 position ) : this( image, position, new Vector2( 0.0f, 0.0f ) ) { }

        public DogeBall( Trajectory trajectory )
        {
            this.elapsedTime = 0f;
            this.trajectory = ( trajectory == null ? new Trajectory() : trajectory );
            this.rotation = 0f;
            this.rotationVelocity = 0f;
            this.radius = GameState.Instance.getTexture("dogeball").Width / 2f;
        }

#endregion


        /**
         * sets the ball's current position to it's next position and resets the elapsed time for next round
         */
        public void applyNextPosition()
        {
            this.trajectory.applyNextPosition();
            this.elapsedTime = 0f;
        }


        /**
         * updates both ICollidable objects to the collision point, calculates the new velocity based on reflection
         *  and recalculates their next position given the new velocity
         */
        public void collide( ICollidable other, float collisionTime )
        {
            this.weightedMovement( collisionTime );
            other.weightedMovement( collisionTime );

            float remainingTime = 1f - collisionTime;

            this.reflect( other.getNormal( this ) );
            other.reflect( this.getNormal( other ) );
        }


        /**
         * modifies the current position of this item to the current position + ( current velocity * weight )
         */
        public void weightedMovement( float time )
        {
            time = Math.Min( time, 1f );
            float weight = time - this.elapsedTime;
            trajectory.currentPosition = trajectory.currentPosition + ( trajectory.currentVelocity * weight );
            this.elapsedTime = time;
        }


        /**
         * reflects the current velocity across the given normal and sets this value as the new velocity.
         *  - also recomputes the next potential position given the remaining velocity magnitude
         */
        public void reflect( Vector2 normal )
        {
            Vector2 oldVelocity = new Vector2( trajectory.currentVelocity.X, trajectory.currentVelocity.Y );
            trajectory.currentVelocity = Vector2.Reflect( trajectory.currentVelocity, normal );

            if ( oldVelocity.Equals( trajectory.currentVelocity ) )
            {
                trajectory.currentVelocity = new Vector2( -trajectory.currentVelocity.X, trajectory.currentVelocity.Y );
            }

            trajectory.calculateNextPosition( getRemainingTime() );
        }


        /**
         * returns the normal vector computed as the perpendicular
         */
        public Vector2 getNormal( ICollidable other )
        {
            Trajectory otherTrajectory = other.getTrajectory();
            //float magnitude = Vector2.Distance( otherTrajectory.currentPosition, trajectory.currentPosition );

            Vector2 toOther = otherTrajectory.currentPosition - trajectory.currentPosition;
            Vector2 normal = new Vector2( -toOther.Y, toOther.X );
            Vector2 normalized = Vector2.Normalize( normal );

            return normalized;
        }


        /**
         * returns the trajectory associated with this object
         */
        public Trajectory getTrajectory()
        {
            return trajectory;
        }


        /**
         * returns the remaining fraction of velocity that this object is allowed to travel this turn
         */
        public float getRemainingTime()
        {
            return 1f - this.elapsedTime;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DogePong.Colliders
{
    public abstract class SphericalCollider : ICollidable
    {
        public Trajectory trajectory;
        public float radius { get; protected set; }
        public float rotation { get; set; }
        public float rotationVelocity { get; set; }
        protected float elapsedTime = 0f;

        public SphericalCollider( Trajectory trajectory )
        {
            this.trajectory = ( trajectory == null ? new Trajectory() : trajectory );
            this.rotation = 0f;
            this.rotationVelocity = 0f;
            this.elapsedTime = 0f;
        }

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


        /**
         * sets the ball's current position to it's next position and resets the elapsed time for next round
         */
        public virtual void applyNextPosition()
        {
            this.trajectory.applyNextPosition();
            this.elapsedTime = 0f;
        }


        /**
         * returns the normal vector computed as the perpendicular
         */
        public virtual Vector2 getNormal( ICollidable other )
        {
            Trajectory otherTrajectory = other.getTrajectory();
            //float magnitude = Vector2.Distance( otherTrajectory.currentPosition, trajectory.currentPosition );

            Vector2 toOther = otherTrajectory.currentPosition - this.trajectory.currentPosition;

            //if ( other is SphericalCollider )
            //{
            //    SphericalCollider collider = other as SphericalCollider;
            //    toOther = collider.midpoint - this.midpoint;
            //}

            Vector2 normal = new Vector2( -toOther.Y, toOther.X );
            Vector2 normalized = Vector2.Normalize( normal );

            return normalized;
        }


        /**
         * modifies the current position of this item to the current position + ( current velocity * weight )
         */
        public virtual void weightedMovement( float time )
        {
            time = Math.Min( time, 1f );
            float weight = time - this.elapsedTime;
            trajectory.currentPosition = trajectory.currentPosition + ( trajectory.currentVelocity * weight );
            this.elapsedTime = time;
        }

        public virtual Trajectory getTrajectory()
        {
            return trajectory;
        }

        public virtual float getRemainingTime()
        {
            return 1 - elapsedTime;
        }

        public virtual void calculateMovementPotential()
        {
            if ( trajectory.currentVelocity.Length() > 7 )
            {
                trajectory.currentVelocity = new Vector2( MathHelper.Clamp( trajectory.currentVelocity.X, -6f, 6f ), MathHelper.Clamp( trajectory.currentVelocity.Y, -6f, 6f ) );
            }
            trajectory.calculateNextPosition( 1f - elapsedTime );
        }

        public abstract void collide( ICollidable other, float collisionTime );

        public abstract void reflect( Vector2 normal );
    }
}

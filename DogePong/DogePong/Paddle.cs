using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DogePong
{
    public class Paddle : ICollidable
    {
        public Trajectory trajectory;
        public Texture2D texture;

        public Paddle( Trajectory trajectory, Texture2D texture )
        {
            this.trajectory = trajectory;
            this.texture = texture;
        }






        /**
         * calculates the movement of a given paddle texture with a given trajectory.
         *   the paddle bounces off the bumpers as necessary, reversing it's velocity
         */
        public void applyMovement()
        {
            float northBoundary = DogePong.northBoundary;
            float southBoundary = DogePong.southBoundary;

            //borrow a reference to the paddle's trajectory
            float textureHeight = texture.Height;

            //define the new position of the paddle, check against walls, apply whats necessary.
            float currentY = trajectory.currentPosition.Y;
            Vector2 yVelocity = trajectory.currentVelocity;
            float newY = currentY + yVelocity.Y;

            //check for north bumper collision
            if ( newY < northBoundary )
            {
                float collisionTime = ( currentY - northBoundary ) / yVelocity.Y;

                trajectory.nextVelocity = Vector2.Reflect( trajectory.currentVelocity, new Vector2( 0f, 1f ) );
                trajectory.ApplyWeightedVelocities( collisionTime );
            }

            //check for south bumper collision
            else if ( ( newY + textureHeight ) > southBoundary )
            {
                float collisionTime = ( ( currentY + textureHeight ) - southBoundary ) / yVelocity.Y;

                trajectory.nextVelocity = Vector2.Reflect( trajectory.currentVelocity, new Vector2( 0f, 1f ) );
                trajectory.ApplyWeightedVelocities( collisionTime );
            }
            else
                trajectory.currentPosition = new Vector2( trajectory.currentPosition.X, newY );
        }



        //paddles don't collide, other things collide with paddles.
        public void collide( ICollidable other, float collisionTime )
        {
            if ( other == null ) return;
            other.collide( this, collisionTime );
        }

        public void weightedMovement( float collisionTime )
        {
            //paddles don't move when they are collided with.
        }

        public void reflect( Vector2 normal )
        {
            //paddles don't move when collided with, but we'll set the next position to be equal to the current position in the event that trajectory.applyNextPosition() is mistakenly called.
            this.trajectory.nextPosition = new Vector2( this.trajectory.currentPosition.X, this.trajectory.currentPosition.Y );
        }

        public Vector2 getNormal( ICollidable other )
        {

            //return a vector based on the location that the paddle was hit
            Trajectory otherTrajectory = other.getTrajectory();

            if ( other is DogeBall )
            {
                DogeBall ball = other as DogeBall;

                //scale the velocity to add 5% speed
                Vector2 velocity = ball.trajectory.currentVelocity;
                float magnitude = velocity.Length();
                velocity.Normalize();
                ball.trajectory.currentVelocity = velocity * ( magnitude * 1.1f );

                Vector2 ballMidpoint = ball.midpoint;
                Vector2 nextMidpoint = ball.nextMidpoint;
                Vector2 centerOfPaddle = new Vector2( trajectory.currentPosition.X, trajectory.currentPosition.Y + ( texture.Height / 2 ) );

                float bot = trajectory.currentPosition.Y + texture.Height;
                float top = trajectory.currentPosition.Y;
                float radius = ball.radius;

                //check if the ball collided with the top or bottom
                if( ( Math.Abs( top - ( ballMidpoint.Y + radius ) ) < 1f ) ||
                        ( Math.Abs( bot - ( ballMidpoint.Y - radius ) ) < 1f ) ||
                        ( Math.Abs( top - ( nextMidpoint.Y + radius ) ) < 1f ) || 
                        ( Math.Abs( bot - ( nextMidpoint.Y + radius ) ) < 1f ) )
                {
                    return new Vector2( 0f, 1f );
                }

                //the paddle bounces
                if ( Math.Abs( ballMidpoint.Y - centerOfPaddle.Y )  < 30 ) return new Vector2( 1f, 0f );

                Vector2 toCenter = centerOfPaddle - ballMidpoint;

                //set a reflection vector inversely x-proportional to the center of the paddle
                Vector2 normal = new Vector2( -toCenter.Y, -toCenter.X );
                Vector2 normalized = Vector2.Normalize( normal );

                return normalized;
            }



            //unknown things reflect directly off the paddle
            return new Vector2( 1f, 0f );
        }

        public Trajectory getTrajectory()
        {
            return trajectory;
        }

        /**
         * we don't care about time constraints in this class
         */
        public float getRemainingTime()
        {
            return 1f;
        }
    }
}

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DogePong
{
    public class Trajectory
    {
        public Vector2 currentPosition { get; set; }
        public Vector2 currentVelocity { get; set; }

        //public Vector2 nextPosition;
        public Vector2 nextPosition { get; set; }
        public Vector2 nextVelocity { get; set; }

        /**
         * a default constructor
         */
        public Trajectory() : this( new Vector2(), new Vector2() ) { }

        /**
         * a constructor which accepts only the initial position. Sets velocity to 0,
         */
        public Trajectory( Vector2 currentPosition ) : this( currentPosition, new Vector2() ) { }


        /**
         * a constructor initializing current position & velocity, sets default next velocity
         */
        public Trajectory( Vector2 currentPosition, Vector2 initialVelocity )
        {
            this.currentPosition = new Vector2( currentPosition.X, currentPosition.Y );
            this.nextPosition = new Vector2( currentPosition.X, currentPosition.Y );
            this.currentVelocity = new Vector2( initialVelocity.X, initialVelocity.Y );
            //this.nextVelocity = new Vector2( initialVelocity.X, initialVelocity.Y );
        }




        /**
         * sets the next position given the total current velocity
         */
        public void calculateNextPosition( float weight )
        {
            this.nextPosition = currentPosition + ( currentVelocity * weight );
        }


        /**
         * sets the current position to be equivalent to it's next position.
         */
        public void applyNextPosition()
        {
            this.currentPosition = new Vector2( nextPosition.X, nextPosition.Y ) ;
        }



        /**
         * called when a collision occurs.
         * 
         * updates the movement of the ball, adjustable based on a portion of the time passed before the trajectory changes
         * @param - collisionTime, a value [ 0, 1 ] that determines how much of each velocity to apply
         *          - a value of 0 represents an immediate collision, where 100% of the next velocity should be applied.
         *          - a value of 1 represents no collosion, where 100% of the current velocity should be applied.
         */
        public void ApplyWeightedVelocities( float collisionTime )
        {
            //bound [ 0, 1 ]
            collisionTime = Math.Max( 0f, Math.Min( 1f, collisionTime ) );

            float xPos = currentPosition.X + ( currentVelocity.X * collisionTime ) + ( nextVelocity.X * ( 1f - collisionTime ) );
            float yPos = currentPosition.Y + ( currentVelocity.Y * collisionTime ) + ( nextVelocity.Y * ( 1f - collisionTime ) );
            currentPosition = new Vector2( currentPosition.X, currentPosition.Y );
            currentVelocity = nextVelocity;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Microsoft.Xna.Framework;

namespace DogePong
{
/**
 * wow such comment
 */
    class CollisionCalculator
    {
        //a collection of CollisionEvent objects to sort by time and process
        private ArrayList collisions;
        private ArrayList completed;
        private Boundary boundary;
        private Player blue;
        private Player red;

        public CollisionCalculator( Boundary boundary, Player blue, Player red )
        {
            this.collisions = new ArrayList();
            this.completed = new ArrayList();
            this.boundary = boundary;
            this.blue = blue;
            this.red = red;
        }


        /**
         * the main collision handling function. Will compute the next potential position of each ball, detect & process collisions based on the time of collision [ 0, 1 )
         */
        public void calculate()
        {
            BlackHole[] holes = GameState.Instance.getBlackHoles();
            holes[0].calculateMovementPotential();
            holes[1].calculateMovementPotential();

            //borrow references to paddles
            Paddle bluePaddle = blue.paddle;
            Paddle redPaddle = red.paddle;

            //borrow references to the trajectory
            Trajectory blueTrajectory = bluePaddle.trajectory;
            Trajectory redTrajectory = redPaddle.trajectory;
            
            //first, iterate through the balls, remove any deleted/invalid balls, and calculate their potential next positions.
            for ( int i = 0; i < GameState.Instance.NumberOfBalls(); ++i )
            {
                DogeBall current = GameState.Instance.GetBall( i );
                float radius = current.radius;
                float diameter = radius * 2f;

                if ( holes[0].gravity( current ) )
                {
                    current.teleport( new Vector2( holes[1].getTrajectory().currentPosition.X - current.radius, holes[1].getTrajectory().currentPosition.Y - current.radius ) );
                    continue;
                }
                if( holes[1].gravity( current ) )
                {
                    current.teleport( new Vector2( holes[0].midpoint.X - current.radius, holes[0].midpoint.Y - current.radius ) );
                    continue;
                }

                //calculate the next position in the current ball's trajectory object using 100% of the current velocity.
                else current.calculateMovementPotential();

                if ( !isValid( current ) )
                {
                    //if the ball has gone out of bounds
                    GameState.Instance.RemoveBall( i );
                    --i;
                    continue;
                }
            }

            int iterationCount = 0;
            float totalTime = 0f;
            detectCollisions();
            while ( totalTime < 1f && collisions.Count > 0 )
            {
                iterationCount++;
                totalTime = processCollisions();
                detectCollisions();
                if ( iterationCount > 5 ) break;
            }

            //all collisions should be processed, and each ball's next position/velocity will have been properly set.
            for ( int i = 0; i < GameState.Instance.NumberOfBalls(); ++i )
            {
                GameState.Instance.GetBall( i ).applyNextPosition();
            }

            holes[0].applyNextPosition();
            holes[1].applyNextPosition();
        }



        /**
         * determines if the given ball is in valid range. If not, the appropriate player is awarded a point.
         */
        private bool isValid( DogeBall dogeBall )
        {
            Vector2 nextPosition = dogeBall.trajectory.nextPosition;
            float width = dogeBall.radius * 2;

            if ( nextPosition.X + width < 0 )
            {
                red.Point();
                return false;
            }

            if ( nextPosition.X > boundary.width )
            {
                blue.Point();
                return false;
            }

            return true;
        }






        /**
         * detects and creates collision events between all collidable objects
         */
        public void detectCollisions()
        {
            //iterate through and detect collisions. There will never be a very large active number of balls, so an n^2 loop should suffice.
            for ( int i = 0; i < GameState.Instance.NumberOfBalls(); ++i )
            {
                DogeBall first = GameState.Instance.GetBall( i );
                detectWallAndPaddleCollisions( first );

                for ( int j = 0; j < GameState.Instance.NumberOfBalls(); ++j )
                {
                    if ( i == j ) continue;

                    DogeBall second = GameState.Instance.GetBall( j );

                    Vector2 movement = second.trajectory.currentVelocity - first.trajectory.currentVelocity;

                    Vector2 toOtherMidpoint = second.trajectory.currentPosition - first.trajectory.currentPosition;
                    //Vector2 toOtherMidpoint = second.midpoint - first.midpoint;
                    float distance = Vector2.Distance( first.trajectory.currentPosition, second.trajectory.currentPosition );

                    //if the combined movement vector's magnitude isn't great enough to cover the gap between the balls, they can't collide.
                    float diameter = first.radius * 2;
                    float mag = movement.Length();
                    if ( movement.Length() < distance - diameter )
                    {
                        continue;
                    }

                    Vector2 normalVelocity = Vector2.Normalize( movement );
                    float dot = -Vector2.Dot( toOtherMidpoint, normalVelocity );

                    // if the current ball is not moving towards the other ball, no collision can occur
                    if ( dot <= 0f )
                    {
                        continue;
                    }

                    //determine the difference between the squares of the dot product and the distance, this will tell us the squared length of the line perpendicular to our movement
                    float squaredLength = ( distance * distance ) - ( dot * dot );

                    //if the length of the perpendicular line is greater than the diameter of the balls, they won't collide
                    float squaredDiameter = diameter * diameter;
                    if ( squaredLength >= squaredDiameter )
                    {
                        continue;
                    }

                    //use these to create the missing vector
                    float third = squaredDiameter - squaredLength;

                    //safety check, if negative can't collide anyway...
                    if ( third < 0f )
                    {
                        continue;
                    }

                    //determine the appropriate magnitide for our combined movement
                    float newDistance = dot - (float) Math.Sqrt( third );

                    //one last safety check
                    if ( newDistance > distance )
                    {
                        continue;
                    }

                    float collisionTime = newDistance / ( distance - diameter );
                    collisionTime = MathHelper.Clamp( collisionTime, 0.0f, 1.0f );
                    CollisionEvent eve = new CollisionEvent( first, second, collisionTime );
                    if( !collisions.Contains( eve ) )
                        collisions.Add( eve );
                }
            }
        }





        /**
         * process all current collisions.
         *   returns 1f if all collisions have been processed
         *   otherwise, returns the largest fraction of time that a collision occurred
         */
        private float processCollisions()
        {
            if ( collisions.Count == 0 ) return 1f;

            //CollisionTime implements IComparable, and will be sorted by earliest event
            collisions.Sort();
            float runningTime = 0f;

            foreach ( CollisionEvent collision in collisions )
            {
                runningTime = collision.processCollision();
                completed.Add( collision );
            }

            collisions.Clear();

            return runningTime;
        }



        /**
         * this method will create and compare bounds for many objects. A CollisionEvent object will be created for
         *  every collision that is detected. Resolution of these events should result in the object's placement directly at the collision point
         *  with its new velocity and potential next position computed. For this reason, we define a collision strictly:
         *         [ ( currentPosition < bound ) && ( nextPosition > bound ) ] || [ ( currentPosition > bound ) && ( nextPosition < bound ) ]  iff ( A collision must be processed ).
         *         for any arbitrary currentPosition, nextPosition, & bound
         */
        private void detectWallAndPaddleCollisions( DogeBall dogeBall )
        {
            Paddle bluePaddle = blue.paddle;
            Paddle redPaddle = red.paddle; 

            Vector2 nextMidpoint = dogeBall.nextMidpoint;
            Vector2 midpoint = dogeBall.midpoint;
            float radius = dogeBall.radius;
            float remainingTime = dogeBall.getRemainingTime();

            //borrow references to the trajectories
            Trajectory blueTrajectory = bluePaddle.trajectory;
            Trajectory redTrajectory = redPaddle.trajectory;
            Trajectory ballTrajectory = dogeBall.trajectory;

            //----------------define bounds for each paddle

            float blueXPos = blueTrajectory.currentPosition.X;
            float redXPos = redTrajectory.currentPosition.X;

            //defines right bounds of blue (left) paddle for knockback
            float blueTop = blueTrajectory.currentPosition.Y;
            float blueBot = blueTrajectory.currentPosition.Y + bluePaddle.texture.Height;
            float blueMid = ( blueTop + blueBot ) / 2.0f;

            //defines left bounds of red (right) paddle
            float redTop = redTrajectory.currentPosition.Y;
            float redBot = redTrajectory.currentPosition.Y + redPaddle.texture.Height;
            float redMid = ( redTop + redBot ) / 2.0f;

            //----------------define simple bounds for the balls (since paddles are always vertical, we can get away with this here)

            //determine left and right bounds of the given ball
            float left = midpoint.X - radius;
            float right = midpoint.X + radius;
            float nextLeft = nextMidpoint.X - radius;
            float nextRight = nextMidpoint.X + radius;

            //get top and bottom bounds
            float bot = midpoint.Y + radius;
            float top = midpoint.Y - radius;
            float nextBot = nextMidpoint.Y + radius;
            float nextTop = nextMidpoint.Y - radius;

            float northBoundary = boundary.top;
            float southBoundary = boundary.bot;

            //check ball collision with north wall
            if ( nextTop < northBoundary )
            {
                float currentTop = dogeBall.midpoint.Y - radius;
                float collisionTime = Math.Abs( ( currentTop - northBoundary ) / ballTrajectory.currentVelocity.Y );
                if ( dogeBall.getRemainingTime() - collisionTime > 0f )
                {
                    this.collisions.Add( new CollisionEvent( dogeBall, boundary, collisionTime ) );
                }
            }


            //check ball collision with south wall
            else if ( nextBot > southBoundary )
            {
                float currentBot = dogeBall.midpoint.Y + radius;
                float collisionTime = Math.Abs( ( currentBot - southBoundary ) / ballTrajectory.currentVelocity.Y );
                if ( dogeBall.getRemainingTime() - collisionTime > 0f )
                {
                    this.collisions.Add( new CollisionEvent( dogeBall, boundary, collisionTime ) );
                }
            }


            //--------------------------------------------------------paddle collisions


            //detect ball collision with blue paddle
            if ( nextLeft <= blueXPos )
            {
                if ( nextRight < blueXPos - bluePaddle.texture.Width ) { }   //do nothing if the ball has crossed the width of the paddle completely

                //collision if the bottom of the ball is lower than the top of the paddle & the top of the ball higher then the bottom of the paddle. create an event
                else if ( ( nextBot >= blueTop ) && ( nextTop <= blueBot ) )
                {
                    float collisionTime = 0f;

                    //if the midpoint of the ball will also be across the blue paddle's x position, then it must be a vertical collision
                    if ( nextMidpoint.X <= blueXPos )
                    {
                        //determine distance to paddle  ==  Min(              dist to top            ,                   dist to bot         )
                        float remaining = Math.Min( Math.Abs( ( nextMidpoint.Y + radius ) - blueTop ), Math.Abs( ( nextMidpoint.Y - radius ) - blueBot ) );
                        collisionTime = remaining / ballTrajectory.currentVelocity.Y;
                    }
                    else
                    {
                        collisionTime = Math.Abs( ( left - blueXPos ) / ballTrajectory.currentVelocity.X );
                    }

                    if ( dogeBall.getRemainingTime() - collisionTime > 0f )
                    {
                        collisions.Add( new CollisionEvent( dogeBall, bluePaddle, collisionTime ) );
                    }
                }
            }

            //detect ball collision with red paddle
            else if ( nextRight >= redXPos )
            {
                if ( nextLeft > redXPos + bluePaddle.texture.Width ) { }   //do nothing if the ball has crossed the width of the paddle completely

                //collision. create an event
                else if ( ( nextBot >= redTop ) && ( nextTop <= redBot ) )
                {
                    float collisionTime = 0f;

                    //if the midpoint of the ball will also be across the blue paddle's x position, then it must be a vertical collision
                    if ( nextMidpoint.X > redXPos )
                    {
                        //determine distance to paddle  ==  Min(              dist to top            ,                   dist to bot         )
                        float remaining = Math.Min( Math.Abs( ( nextMidpoint.Y + radius ) - redTop ), Math.Abs( ( nextMidpoint.Y - radius ) - redBot ) );
                        collisionTime = remaining / ballTrajectory.currentVelocity.Y;
                    }
                    else
                    {
                        collisionTime = Math.Abs( ( right - redXPos ) / ballTrajectory.currentVelocity.X );
                    }

                    if ( dogeBall.getRemainingTime() - collisionTime > 0f )
                    {
                        collisions.Add( new CollisionEvent( dogeBall, redPaddle, collisionTime ) );
                    }
                }

            }
        }




        private class CollisionEvent : IComparable
        {
            ICollidable first;
            ICollidable second;
            float collisionTime;

            public CollisionEvent( ICollidable one, ICollidable two, float collisionTime )
            {
                this.first = one;
                this.second = two;
                this.collisionTime = collisionTime;
            }

            public float processCollision()
            {
                if ( first == null || second == null ) return 0f;

                //first, instruct both objects to bring themselves up to the point of collision
                first.collide( second, collisionTime );
                second.collide( first, collisionTime );

                first.reflect( second.getNormal( first ) );
                second.reflect( first.getNormal( second ) );

                return collisionTime;
            }

            public int CompareTo( object obj )
            {
                if ( obj == null ) return 1;

                CollisionEvent other = obj as CollisionEvent;
                if ( other != null )
                    return (int) ( this.collisionTime - other.collisionTime ) * 1000;
                else
                    throw new ArgumentException();
            }

            public override bool Equals( object obj )
            {
                if ( obj == null ) return false;

                if ( obj is CollisionEvent )
                {
                    CollisionEvent other = obj as CollisionEvent;
                    if ( ( first == other.first && second == other.second ) ||
                        ( first == other.second && second == other.first ) )
                        return true;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DogePong
{
    public interface ICollidable
    {
        /**
         * process the collision for both ICollidable objects
         */
        void collide( ICollidable other, float collisionTime );

        /**
         * move the given object by a fraction of it's current velocity
         */
        void weightedMovement( float collisionTime );

        /**
         * reflect the object against given normal & calculate the next velocity and position given the remaining fraction of time in this update
         */
        void reflect( Vector2 normal);

        ///**
        // * sets the remaining fraction of velocity that this object is allowed to travel this turn
        // */
        //void setRemainingTime( float remainingTime );

        /**
         * returns the remaining fraction of velocity that this object is allowed to travel this turn
         */
        float getRemainingTime();

        /**
         * return the normal for which this the other object will collide with this object. 
         */
        Vector2 getNormal( ICollidable other );

        /**
         * return a reference to this object's Trajectory which should represent the object's current state.
         */
        Trajectory getTrajectory();
    }
}

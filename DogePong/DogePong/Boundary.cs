using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DogePong
{
    class Boundary : ICollidable
    {
        public readonly float top;
        public readonly float bot;
        public readonly float left;
        public readonly float right;
        public readonly float width;
        public readonly float height;

        public Boundary( float boundaryDensity, float height, float width )
        {
            this.height = height;
            this.width = width;
            this.top = boundaryDensity;
            this.bot = height - boundaryDensity;
            this.left = boundaryDensity;
            this.right = width - boundaryDensity;
        }

        //walls don't collide with things, other things collide with them.
        public void collide( ICollidable other, float collisionTime )
        {
            if ( other == null ) return;
            other.collide( this, collisionTime );
        }



        public void weightedMovement( float collisionTime )
        {
            //walls don't move when collided with
        }

        public void reflect( Vector2 normal )
        {
            //walls don't move when collided with
        }


        public Vector2 getNormal( ICollidable other )
        {
            return new Vector2( 0f, 1f );
        }


        public Trajectory getTrajectory()
        {
            return new Trajectory();
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

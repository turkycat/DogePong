using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DogePong
{
    using Colliders;
    public class BlackHole : SphericalCollider
    {
        public float gravityRadius;
        private double nextSpawn;

        public BlackHole( Trajectory trajectory ) : base( trajectory )
        {
            this.radius = GameState.Instance.getTexture( "blackhole" ).Width / 2f;
            this.gravityRadius = 60;
            this.rotationVelocity = 0.03f;
            nextSpawn = GameState.Instance.totalMillis + ( GameState.Instance.randy.NextDouble() * 10000.0 );
        }

        public override void calculateMovementPotential()
        {
            if ( GameState.Instance.totalMillis > nextSpawn )
            {
                nextSpawn = GameState.Instance.totalMillis + 5000 + ( GameState.Instance.randy.NextDouble() * 10000.0 );
                trajectory.nextPosition = GameState.Instance.generateRandomLocation( gravityRadius * 8, gravityRadius * 8 );
            }
            else
            {
                base.calculateMovementPotential();
                rotation = ( rotation + rotationVelocity );
                float bottom = GameState.Instance.GameHeight - DogePong.BOUNDARY_DENSITY - ( gravityRadius * 2 );
                float right = GameState.Instance.GameWidth - DogePong.BOUNDARY_DENSITY - ( gravityRadius * 2 );
                if ( trajectory.currentPosition.Y + gravityRadius > bottom )
                {
                    trajectory.nextPosition = new Vector2( trajectory.currentPosition.X, bottom );
                    this.trajectory.currentVelocity = Vector2.Reflect( this.trajectory.currentVelocity, new Vector2( 0f, 1f ) );
                }
                else if ( trajectory.currentPosition.Y - gravityRadius < DogePong.BOUNDARY_DENSITY )
                {
                    trajectory.nextPosition = new Vector2( trajectory.currentPosition.X, trajectory.currentPosition.Y + DogePong.BOUNDARY_DENSITY + gravityRadius );
                    this.trajectory.currentVelocity = Vector2.Reflect( this.trajectory.currentVelocity, new Vector2( 0f, 1f ) );
                }
                if ( trajectory.currentPosition.X - gravityRadius < DogePong.BOUNDARY_DENSITY )
                {
                    trajectory.nextPosition = new Vector2( trajectory.currentPosition.X + DogePong.BOUNDARY_DENSITY, trajectory.currentPosition.Y + gravityRadius );
                    this.trajectory.currentVelocity = Vector2.Reflect( this.trajectory.currentVelocity, new Vector2( 1f, 0f ) );
                }
                else if ( trajectory.currentPosition.X + gravityRadius > right )
                {
                    trajectory.nextPosition = new Vector2( right, trajectory.currentPosition.Y );
                    this.trajectory.currentVelocity = Vector2.Reflect( this.trajectory.currentVelocity, new Vector2( 1f, 0f ) );
                }
                //trajectory.currentVelocity = new Vector2( trajectory.currentVelocity.X + (float) ( ( GameState.Instance.randy.NextDouble() - 0.5 ) * 0.1 ), trajectory.currentVelocity.X + (float) ( ( GameState.Instance.randy.NextDouble() - 0.5 ) * 0.1 ) );
            }
        }

        public bool gravity( DogeBall ball )
        {
            Vector2 pull = this.trajectory.currentPosition - ball.midpoint;
            if ( pull.Length() > 100 ) return false;
            else if ( pull.Length() < 25f )
            {
                if ( GameState.Instance.totalMillis - ball.lastTeleport < 1000 ) return false;
                return true;
            }

            else
            {
                Vector2 normalPull = Vector2.Normalize( pull );
                float scale = pull.Length() / 150f;
                Vector2 scaled = normalPull * scale;
                ball.trajectory.currentVelocity = ball.trajectory.currentVelocity + scaled;
            }
            return false;
        }

        public override void collide( ICollidable other, float collisionTime )
        {
            //do nothing
        }

        public override void reflect( Vector2 normal )
        {
            //do nothing
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DogePong.Controllers
{
    public class ComputerController : Controller
    {
        //public ComputerController( PlayerIndex index ) : base( index ) { }

        public override void ProcessMove( Paddle paddle )
        {
            Trajectory trajectory = paddle.trajectory;

            float smallestDistance = float.MaxValue;
            Vector2 toNearestBall = new Vector2();
            Vector2 paddleMidpoint = new Vector2( trajectory.currentPosition.X, trajectory.currentPosition.Y + ( paddle.texture.Height / 2f ) );
            for (int i = 0; i < GameState.Instance.NumberOfBalls(); ++i)
            {
                DogeBall current = GameState.Instance.GetBall( i );
                if (current == null) continue;
                Vector2 toCurrentBall = current.midpoint - paddleMidpoint;
                Vector2 correctedVector = new Vector2(toCurrentBall.X, 0f);
                //the dot product of these two normalized vectors = cos( angle between them )
                float cos = Vector2.Dot(Vector2.Normalize(correctedVector), Vector2.Normalize(paddleMidpoint));

                if (cos > 0) continue;

                float xDist = toCurrentBall.X;
                if (xDist < smallestDistance) toNearestBall = toCurrentBall;
                //float distance = toCurrentBall.Length();
                //if (distance < smallestDistance) toNearestBall = toCurrentBall;
            }

            //we don't care about the distance to the ball in the x-direction
            float yDist = toNearestBall.Y;

            //if we're within 50 pixels
            if ( Math.Abs( yDist ) < 50f)
            {
                if (trajectory.currentVelocity.Length() > 0f)
                {
                    trajectory.currentVelocity = Vector2.Normalize(trajectory.currentVelocity) * (trajectory.currentVelocity.Length() * .95f);
                }
                else
                {
                    trajectory.currentVelocity = new Vector2();
                }
            }
            else
            {
                //clamp velocity [ -12, 12 ]
                float yVelocity = Math.Max(-12f, Math.Min(12f, trajectory.currentVelocity.Y + ( yDist > 0 ? .3f : -.3f ) ) );
                trajectory.currentVelocity = new Vector2(0f, yVelocity);
            }
        }
    }
}

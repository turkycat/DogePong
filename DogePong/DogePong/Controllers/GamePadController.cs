using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.GamerServices;

namespace DogePong.Controllers
{
    public class GamePadController : Controller
    {
        PlayerIndex index;

        public GamePadController( PlayerIndex index )
        {
            this.index = index;
        }

        public override void ProcessMove( Paddle paddle )
        {
            GamePadState state = GamePad.GetState( index );
            Trajectory trajectory = paddle.trajectory;

            //allow the player to use either sticks.
            float thumbstickValueLeft = -state.ThumbSticks.Left.Y;
            float thumbstickValueRight = -state.ThumbSticks.Right.Y;
            float thumbstickValue = ( thumbstickValueLeft == 0f ? thumbstickValueRight : thumbstickValueLeft );

            //trajectory.currentVelocity.n
            //if (thumbstickValue == 0f) trajectory.currentVelocity = new Vector2(trajectory.currentVelocity.X * 0.95f, trajectory.currentVelocity.Y * 0.95f);
            float magnitude = trajectory.currentVelocity.Length();
            if ( thumbstickValue == 0f )
            {
                if ( magnitude != 0 )
                {
                    trajectory.currentVelocity = Vector2.Normalize( trajectory.currentVelocity ) * ( magnitude * .95f );
                }
            }
            else
            {
                //clamp velocity [ -12, 12 ]
                float yVelocity = Math.Max( -12f, Math.Min( 12f, trajectory.currentVelocity.Y + ( thumbstickValue * 0.3f ) ) );
                trajectory.currentVelocity = new Vector2( 0f, yVelocity );
            }
        }
    }
}

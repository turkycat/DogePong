using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.GamerServices;

namespace DogePong
{
    public class Player
    {
        public PlayerIndex index { get; private set; }
        public TextItem scoreText;
        public Paddle paddle;
        public Controllers.Controller controller;
        private int points;

        public Player( Controllers.Controller controller, PlayerIndex index, Paddle paddle, TextItem scoreText )
        {
            this.points = 0;
            this.paddle = paddle;
            this.scoreText = scoreText;
            this.controller = controller;
            this.index = index;
        }



        /**
         * calculates and processes the player's movement for this update
         */
        public void calculatePlayerMovement()
        {
            controller.ProcessMove( paddle );
            paddle.applyMovement();
        }


        /**
         * sets the controller for this player, which will be used when calculating input
         */
        public void setController( Controllers.Controller controller )
        {
            this.controller = controller;
        }


        /**
         * determines if the input device is connected
         */
        public bool isGamePadConnected()
        {
            GamePadState state = GamePad.GetState( index );
            return state.IsConnected;
        }



        public int Score()
        {
            return points;
        }

        public void Point()
        {
            ++points;
            scoreText.sequence = points.ToString();
            GameState.Instance.getSound( "point" ).Play();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.GamerServices;

namespace DogePong.Controllers
{
    public abstract class Controller : IController
    {
        //protected PlayerIndex index;

        //public Controller( PlayerIndex index )
        //{
        //    this.index = index;
        //}

        public abstract void ProcessMove( Paddle paddle );

        //public PlayerIndex getPlayerIndex()
        //{
        //    return index;
        //}
    }
}

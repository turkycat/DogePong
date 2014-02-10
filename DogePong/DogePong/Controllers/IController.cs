using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.GamerServices;

namespace DogePong
{
    public interface IController
    {
        void ProcessMove( Paddle paddle );
        //PlayerIndex getPlayerIndex();
    }
}

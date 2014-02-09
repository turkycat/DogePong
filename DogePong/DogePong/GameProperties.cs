using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DogePong
{
    /**
     * this is a Singleton class, containing vital game information which can be dynamically changed and accessed from anywhere within namespace DogePong
     */
    class GameProperties
    {
        private static GameProperties properties;













        public static GameProperties Instance()
        {
            if ( properties == null )
            {
                properties = new GameProperties();
            }
            return properties;
        }
    }
}

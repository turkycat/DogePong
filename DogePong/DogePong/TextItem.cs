using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DogePong
{
    /**
     * private class to wrap properties of a text item to the screen
     */
    public class TextItem
    {
        public bool permanent;
        public String sequence;
        public SpriteFont font;
        public Vector2 position;
        public float scale;
        public Color color;
        public float rotation;
        public bool expired
        {
            get
            {
                return !permanent && duration < 0;
            }
        }
        private long duration;

        public TextItem( String sequence, SpriteFont font, Vector2 position, Color color, int duration ) : this( sequence, font, position, 1f, color, duration, 0f ) { }

        public TextItem( String sequence, SpriteFont font, Vector2 position, float scale, Color color, int duration, float rotation )
        {
            this.position = position;
            this.sequence = sequence;
            this.permanent = duration <= 0 ? true : false;
            if ( !permanent ) this.duration = duration;
            this.font = font;
            this.color = color;
            this.rotation = rotation;
            this.scale = scale;
        }



        public void setPermanent()
        {
            this.permanent = true;
        }


        public bool elapseMillis( int millis )
        {
            this.duration -= millis;
            return expired;
        }
    }

}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace MoleMash
{
    public class Ghost
    {
        public Vector2 Position { get; set; }
        public Texture2D Texture { get; set; }
        public float TimeAlive { get; set; }

        public Rectangle Rectangle => new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);

        public Ghost(Vector2 position, Texture2D texture, float timeAlive)
        {
            Position = position;
            Texture = texture;
            TimeAlive = timeAlive;
        }
    }
}

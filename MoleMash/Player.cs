using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace MoleMash
{
    public class Player
    {
        private int _health;
        private Game1 _game;

        public Player(Game1 game)
        {
            _game = game;
        }

        public int Health
        {
            get { return _health; }
            set
            {
                _health = value;

                if (_health <= 0)
                {
                    //_game.Exit();
                }
            }
        }
        public int score { get; set; }
        public string username {  get; set; }

        public bool bHasImmunity { get; set; } = false;
        public bool bSlowerTime { get; set; } = false;
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngineApp.GameObjects
{
    class Enemy : Player
    {
        public Enemy(string tag = "default", int unique = 0) : base(tag, unique)
        {
        }

        public override void Update(int delta)
        {
        }
    }
}

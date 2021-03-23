using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngineApp.GameObjects
{
    class Panel : GameObject
    {
        int right_width_ = 0;
        public Panel(string tag = "default", int right_width = 22) : base(tag)
        {
            right_width_ = right_width;
        }

        public int RightBound() { 
            return GameEngine.GetInstance().GetWidth() - right_width_;
        }

        public override ScreenPoint[] GetScreenPoints()
        {
            int max_row = GameEngine.GetInstance().GetHeight();

            ScreenPoint[] sps = new ScreenPoint[max_row];

            for (int i = 0; i < max_row; ++i) {
                sps[i] = new ScreenPoint();
                sps[i].col = GameEngine.GetInstance().GetWidth() - right_width_;
                sps[i].row = i;
                sps[i].symbol = '@';
            }
            return sps;
        }
    }
}

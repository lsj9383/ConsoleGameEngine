using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngineApp.GameObjects
{
    class Player : GameObject
    {
        int col_ = 0;
        int row_ = 0;

        public Player(string tag = "default") : base(tag)
        {
        }

        public override ScreenPoint[] GetScreenPoints()
        {
            ScreenPoint[] sps = new ScreenPoint[1];
            sps[0] = new ScreenPoint();
            sps[0].col = col_;
            sps[0].row = row_;
            return sps;
        }

        public override void Update(int delta)
        {
            if (!Console.KeyAvailable) {
                return;
            }

            ConsoleKeyInfo cki = Console.ReadKey(true);
            if (cki.Key == ConsoleKey.W)
            {
                row_ -= 1;
            }

            if (cki.Key == ConsoleKey.S)
            {
                row_ += 1;
            }

            if (cki.Key == ConsoleKey.A)
            {
                col_ -= 1;
            }

            if (cki.Key == ConsoleKey.D)
            {
                col_ += 1;
            }

            int top_bound = 0;
            int bottom_bound = GameEngine.GetInstance().GetHeight() - 1;
            int left_bound = 0;
            int right_bound = GameEngine.GetInstance().GetWidth() - 2;

            List<GameObject> game_objects = GameEngine.GetInstance().QueryGameObjectsByTag("Panel");
            if (game_objects != null || game_objects.Count == 1) {
                Panel p = (Panel)game_objects[0];
                right_bound = p.RightBound() - 1;
            }

            row_ = Math.Min(Math.Max(row_, top_bound), bottom_bound);
            col_ = Math.Min(Math.Max(col_, left_bound), right_bound);
        }
    }
}

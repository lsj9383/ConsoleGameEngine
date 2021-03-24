using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngineApp.GameObjects
{
    class Player : GameObject
    {
        public int level = 0;

        int col_ = 0;
        int row_ = 0;
        char symbol_ = '*';

        int top_bound_ = 0;
        int bottom_bound_ = 0;
        int left_bound_ = 0;
        int right_bound_ = 0;

        int unique_ = 0;

        Random rd = new Random();

        public Player(string tag = "default", int unique = 0) : base(tag)
        {
            string symbols = "!@#$%^&*()_+1234567890-=";
            symbol_ = symbols[rd.Next(0, symbols.Length)];

            if (unique == 0)
            {
                unique_ = rd.Next(0, int.MaxValue);
            }
            else
            {
                unique_ = unique;
            }
        }

        public int GetUnique() {
            return unique_;
        }

        public override void Start()
        {
            top_bound_ = 0;
            bottom_bound_ = GameEngine.GetInstance().GetHeight() - 1;
            left_bound_ = 0;
            right_bound_ = GameEngine.GetInstance().GetWidth() - 2;

            List<GameObject> game_objects = GameEngine.GetInstance().QueryGameObjectsByTag("Panel");
            if (game_objects != null || game_objects.Count == 1)
            {
                Panel p = (Panel)game_objects[0];
                right_bound_ = p.RightBound() - 1;
            }

            col_ = rd.Next(left_bound_, right_bound_);
            row_ = rd.Next(top_bound_, bottom_bound_);
        }


        public override ScreenPoint[] GetScreenPoints()
        {
            ScreenPoint[] sps = new ScreenPoint[1];
            sps[0] = new ScreenPoint();
            sps[0].col = col_;
            sps[0].row = row_;
            sps[0].symbol = symbol_;
            sps[0].level = level;
            return sps;
        }

        public override void Update(int delta)
        {
            List<GameObject> game_objects = GameEngine.GetInstance().QueryGameObjectsByTag("Root");
            if (game_objects != null || game_objects.Count == 1)
            {
                Root r = (Root)game_objects[0];
                proto.StateProto state = new proto.StateProto();
                state.id = unique_;
                state.col = col_;
                state.row = row_;
                state.symbol = symbol_;
                r.SendStateProto(state);
            }

            if (!Console.KeyAvailable) {
                return;
            }

            char key = ' ';
            ConsoleKeyInfo cki = Console.ReadKey(true);
            if (cki.Key == ConsoleKey.W)
            {
                key = 'W';
            }
            else if (cki.Key == ConsoleKey.S)
            {
                key = 'S';
            }
            else if (cki.Key == ConsoleKey.A)
            {
                key = 'A';
            }
            else if (cki.Key == ConsoleKey.D)
            {
                key = 'D';
            }

            game_objects = GameEngine.GetInstance().QueryGameObjectsByTag("Root");
            if (game_objects != null || game_objects.Count == 1)
            {
                Root r = (Root)game_objects[0];
                proto.ActionProto action = new proto.ActionProto();
                action.id = unique_;
                action.key = key;
                r.SendActionProto(action);
            }
        }

        public void ApplayAction(proto.ActionProto action) {
            if (action.id != unique_) {
                return;
            }

            if (action.key == 'W')
            {
                row_ -= 1;
            }

            if (action.key == 'S')
            {
                row_ += 1;
            }

            if (action.key == 'A')
            {
                col_ -= 1;
            }

            if (action.key == 'D')
            {
                col_ += 1;
            }

            row_ = Math.Min(Math.Max(row_, top_bound_), bottom_bound_);
            col_ = Math.Min(Math.Max(col_, left_bound_), right_bound_);
        }

        public void MoveTo(int row, int col) {
            row_ = row;
            col_ = col;
        }

        public void SetSymbol(char s) {
            symbol_ = s;
        }
    }
}

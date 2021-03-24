using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GameEngineApp
{
    class GameEngine
    {
        int rate_ = 100;
        int width_ = 0;
        int height_ = 0;
        List<char[]> video_memory = new List<char[]>();
        Dictionary<int, GameObject> game_objects = new Dictionary<int, GameObject>();
        Dictionary<string, List<GameObject>> tag_game_objects = new Dictionary<string, List<GameObject>>();
        List<GameObject> wait_add_game_objects = new List<GameObject>();

        bool exit_ = false;
        bool run_ = false;


        static GameEngine instance = new GameEngine();

        GameEngine() {
            width_ = Console.LargestWindowWidth / 2;
            height_ = Console.LargestWindowHeight / 2;
        }

        public static GameEngine GetInstance()
        {
            return instance;
        }

        public int GetWidth() {
            return width_;
        }

        public int GetHeight() {
            return height_;
        }

        public GameObject QueryGameObject(int game_object_id)
        {
            if (!game_objects.ContainsKey(game_object_id))
            {
                return null;
            }
            return game_objects[game_object_id];
        }

        public List<GameObject> QueryGameObjectsByTag(string tag)
        {
            if (!tag_game_objects.ContainsKey(tag))
            {
                return null;
            }
            return tag_game_objects[tag];
        }

        public GameEngine SetWidth(int width)
        {
            if (width > 0 && width <= Console.LargestWindowWidth)
            {
                width_ = width;
            }
            return this;
        }

        public GameEngine SetHeight(int height)
        {
            if (height > 0 && height <= Console.LargestWindowHeight)
            {
                height_ = height;
            }
            return this;
        }

        public GameEngine SetUpdateRate(int rate)
        {
            rate_ = rate;
            return this;
        }

        public GameEngine AddGameObject(GameObject go, bool force = false)
        {
            if (run_ && !force)
            {
                wait_add_game_objects.Add(go);
                return this;
            }
            game_objects.Add(go.GetID(), go);
            if (!tag_game_objects.ContainsKey(go.GetTag()))
            {
                tag_game_objects.Add(go.GetTag(), new List<GameObject>());
            }
            tag_game_objects[go.GetTag()].Add(go);
            return this;
        }

        public void Run()
        {
            
            Console.SetWindowSize(width_, height_);
            Console.SetBufferSize(width_, height_);
            Console.CursorVisible = false;

            for (int i = 0; i <= height_; ++i)
            {
                video_memory.Add(new char[width_ + 1]);
            }

            foreach (KeyValuePair<int, GameObject> kv in game_objects)
            {
                kv.Value.Start();
            }

            run_ = true;

            int now = DateTime.Now.Millisecond;


            while (!exit_)
            {
                int last = now;
                now = DateTime.Now.Millisecond;
                Update(now - last);
                Draw();

                foreach (GameObject go in wait_add_game_objects) {
                    AddGameObject(go, true);
                }
                wait_add_game_objects.Clear();

                Thread.Sleep(1000 / rate_);
            }

            OnApplicationQuit();
        }

        void ClearVideoMemory()
        {
            for (int i = 0; i < height_; ++i)
            {
                for (int j = 0; j < width_; ++j)
                {
                    video_memory[i][j] = ' ';
                }
            }
        }

        void Draw()
        {
            foreach (KeyValuePair<int, GameObject> kv in game_objects)
            {
                ScreenPoint[] screen_points = kv.Value.GetScreenPoints();
                if (screen_points == null)
                {
                    continue;
                }
                foreach (ScreenPoint sp in screen_points)
                {
                    if (sp.row < 0 || sp.col < 0 || sp.row >= height_ || sp.col >= width_)
                    {
                        continue;
                    }
                    video_memory[sp.row][sp.col] = sp.symbol;
                }
            }

            Console.SetCursorPosition(0, 0);
            for (int i = 0; i < height_; ++i)
            {
                Console.Write(video_memory[i]);
            }
            Console.SetCursorPosition(0, 0);
            ClearVideoMemory();
        }

        void Update(int delta)
        {
            foreach (KeyValuePair<int, GameObject> kv in game_objects)
            {
                kv.Value.Update(delta);
            }
        }

        void OnApplicationQuit() {
            foreach (KeyValuePair<int, GameObject> kv in game_objects)
            {
                kv.Value.OnApplicationQuit();
            }
        }

        public void Exit()
        {
            exit_ = true;
        }

        public bool IsExit()
        {
            return exit_;
        }
    }
}
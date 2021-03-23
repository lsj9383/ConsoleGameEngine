using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngineApp
{
    class GameObject
    {
        static int id_gen_ = 0;
        int id_ = 0;
        string tag_;

        public GameObject(string tag = "default")
        {
            id_ = id_gen_;
            id_gen_ += 1;
            tag_ = tag;
        }

        public string GetTag() {
            return tag_;
        }

        public int GetID()
        {
            return id_;
        }

        public virtual void Update(int delta) { }
        public virtual void Start() { }
        public virtual void Awake() { }
        public virtual ScreenPoint[] GetScreenPoints() { return null; }
    }
}
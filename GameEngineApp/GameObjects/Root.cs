using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngineApp.GameObjects
{
    enum NetworkProtocolType
    {
        STATE = 1,
        ACTION
    }

    class Root : GameObject
    {
        JsonCodecs codecs = new JsonCodecs();

        string ip = "9.134.9.104";
        int port = 12345;
        Network _net = new Network(new TCPTransport());

        Dictionary<int, Player> enemy_dict = new Dictionary<int, Player>();

        public Root(string tag = "default") : base(tag)
        {
            _net.AddCallback((int)NetworkProtocolType.STATE, StateHandler);
            _net.AddCallback((int)NetworkProtocolType.ACTION, ActionHandler);
            _net.Connect(ip, port);
        }

        void StateHandler(byte[] data)
        {
            proto.StateProto proto = codecs.Decode<proto.StateProto>(data);

            // 是自己的状态 直接忽略
            List<GameObject> game_objects = GameEngine.GetInstance().QueryGameObjectsByTag("Player");
            Player p = (Player)game_objects[0];
            if (proto.id == p.GetUnique())
            {
                return;
            }

            // 不存在其他玩家，添加敌人
            if (!enemy_dict.ContainsKey(proto.id))
            {
                Player enemy = new Enemy("Enemy", proto.id);
                enemy_dict[proto.id] = enemy;
                GameEngine.GetInstance().AddGameObject(enemy);
            }

            // 更新其他玩家的状态
            enemy_dict[proto.id].MoveTo(proto.row, proto.col);
            enemy_dict[proto.id].SetSymbol(proto.symbol);
        }

        void ActionHandler(byte[] data)
        {
            proto.ActionProto proto = codecs.Decode<proto.ActionProto>(data);

            // 是自己的状态 才进行更新
            List<GameObject> game_objects = GameEngine.GetInstance().QueryGameObjectsByTag("Player");
            Player p = (Player)game_objects[0];
            if (proto.id == p.GetUnique())
            {
                p.ApplayAction(proto);
            }
        }

        public override void Update(int delta)
        {
            if (_net != null)
            {
                _net.Update();
            }
        }

        public void SendStateProto(proto.StateProto state)
        {
            _net.Send((int)NetworkProtocolType.STATE, codecs.Encode<proto.StateProto>(state));
        }

        public void SendActionProto(proto.ActionProto action)
        {
            _net.Send((int)NetworkProtocolType.ACTION, codecs.Encode<proto.ActionProto>(action));
        }

        public override void OnApplicationQuit() {
            if (_net != null) {
                _net.Disconnect();
            }
        }
    }
}

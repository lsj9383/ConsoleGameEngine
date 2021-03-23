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

        public Root(string tag = "default") : base(tag)
        {
            _net.AddCallback((int)NetworkProtocolType.STATE, StateHandler);
            _net.AddCallback((int)NetworkProtocolType.ACTION, ActionHandler);
            _net.Connect(ip, port);
        }

        void StateHandler(byte[] data)
        {
            proto.StateProto proto = codecs.Decode<proto.StateProto>(data);
        }

        void ActionHandler(byte[] data)
        {
            proto.ActionProto proto = codecs.Decode<proto.ActionProto>(data);
        }

        public override void Update(int delta)
        {
            if (_net != null)
            {
                _net.Update();
            }
        }
    }
}

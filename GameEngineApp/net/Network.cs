using System;
using System.Collections;
using System.Collections.Generic;

public class Network
{
    private const int _processMessageOnce = 128;
    private Events _events = new Events();
    private Transport _transport;

    public Network(Transport transport=null, Codecs cs=null) {
        _transport = transport == null ? new TCPTransport() : transport;
    }

    public void AddCallback(int type, Events.Callback cb) {
        _events.AddCallback(type, cb);
    }

    public bool IsConnected() {
        return _transport.IsConnected();
    }

    public bool Connect(string ip, int port) {
        return _transport.Connect(ip, port);
    }

    public void Disconnect() {
        _transport.Disconnect();
    }

    public bool Send(byte[] data) {
        _transport.Send(data);
        return true;
    }

    public bool Send(int type, byte[] data) {
        _transport.Send(type, data);
        return true;
    }

    public void Update() {
        int process_count = 0;
        byte[] bytes;

        _transport.Update();

        while (process_count < _processMessageOnce) {
            process_count++;
            if (!_transport.Receive(out bytes)) {
                continue;
            }

            NetworkPackage p = NetworkPackage.Decode(bytes);
            _events.Invoke(p.type, p.data);
        }
    }
}

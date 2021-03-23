using System;

public abstract class Transport
{
    public abstract bool IsConnected();

    public abstract bool Connect(string ip, int port);
    public abstract void Disconnect();
    public abstract void Send(byte[] data);
    public abstract void Send(int type, byte[] data);
    public abstract bool Receive(out byte[] data);
    public abstract int PeekMessageNumber();
    public virtual void Update() {}

    public virtual bool Connect(string ip, int port, uint conv) {
        return Connect(ip, port);
    }
}

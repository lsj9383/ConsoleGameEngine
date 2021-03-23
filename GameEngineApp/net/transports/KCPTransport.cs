using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class KCPTransport : Transport
{
    private Socket _socket;
    private KCP _kcp;

    private bool _connected = false;
    private uint _conv = 0;

    private readonly Queue<byte[]> _queue = new Queue<byte[]>();
    private readonly Queue<byte[]> _kcpQueue = new Queue<byte[]>();
    private readonly Queue<KCPDatagram> _cmdQueue = new Queue<KCPDatagram>();

    private const int _maxBufferSize = 65535;
    private byte[] _buffer = new byte[_maxBufferSize];

    public override bool IsConnected() {
        return _connected;
    }

    public override bool Connect(string ip, int port) {
        var random = new System.Random();
        int number = random.Next(Int32.MinValue, Int32.MaxValue);
        uint conv = (uint)(number + (uint)Int32.MaxValue);
        Console.WriteLine($"new connect conv: {conv}");
        return Connect(ip, port, conv);
    }

    public override bool Connect(string ip, int port, uint conv) {
        try
        {
            _connected = false;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _socket.Connect(ip, port);
            _kcp = new KCP(conv, HandleKcpSend);
            _kcp.SetMtu(KCP.IKCP_MTU_DEF - KCPDatagram.CMD_SIZE);
            _kcp.SetInterval(1);
            _kcp.WndSize(512, 512);
            _kcp.NoDelay(2, 10, 2, 1);
            _connected = true;
            _conv = conv;
            _socket.BeginReceive(_buffer, 0, _maxBufferSize, SocketFlags.None,
                                 new AsyncCallback(OnReceive), _socket);
        }
        catch (Exception e)
        {
            Console.WriteLine("connect failed: " + e);
            _connected = false;
        }

        // clear message queue
        lock (_queue) {
            _queue.Clear();
        }

        return _connected;
    }

    private void HandleKcpSend(byte[] buff, int size) {
        if (_socket != null && _connected) {
            // Debug.Log($">>>> Send time {GetMSClock()}, size: {size}");
            byte[] payload = KCPDatagram.BuildKCP(buff, size);
            _socket.Send(payload, 0, payload.Length, SocketFlags.None);
            // _socket.Send(buff, 0, size, SocketFlags.None);
        }
    }

    void OnReceive(IAsyncResult ar)
    {
        try
        {
            int count = _socket.EndReceive(ar);
            // Debug.Log($"<<<< OnReceive {GetMSClock()}, size: {count}");
            ProcessReceiveData(count);
            // byte[] messageBytes = new byte[count];
            // Array.Copy(_buffer, 0, messageBytes, 0, count);
            // lock (_kcpQueue)
            // {
            //     _kcpQueue.Enqueue(messageBytes);
            // }
            _socket.BeginReceive(_buffer, 0, _maxBufferSize, SocketFlags.None,
                                 new AsyncCallback(OnReceive), _socket);
        }
        catch (Exception e)
        {
            Console.WriteLine("receive failed: " + e);
            _connected = false;
        }
    }

    void ProcessReceiveData(int count) {
        KCPDatagram.Command cmd = KCPDatagram.GetCMD(_buffer);

        uint conv = KCPDatagram.GetConv(_buffer);
        KCPDatagram kcpCmd = new KCPDatagram();
        kcpCmd.cmd = cmd;
        lock (_cmdQueue)
        {
            _cmdQueue.Enqueue(kcpCmd);
        }
        
        if (_connected && cmd == KCPDatagram.Command.KCP) {
            byte[] messageBytes = KCPDatagram.GetData(_buffer, count);
            lock (_kcpQueue)
            {
                _kcpQueue.Enqueue(messageBytes);
            }
            return;
        }
    }

    public override void Disconnect() {
        if (!_connected) {
            return;
        }

        byte[] payload = KCPDatagram.BuildRST(_conv);
        _socket.Send(payload, 0, payload.Length, SocketFlags.None);
        _socket.Close();
        _connected = false;
    }

    void Close() {
        if (!_connected) {
            return;
        }
        _socket.Close();
        _connected = false;
    }

    public override void Send(byte[] data) {
        if (data.Length <= 0 || !_connected) {
            return;
        }
        _kcp.Send(data, data.Length);
    }

    public override void Send(int type, byte[] data) {
        if (data.Length <= 0 || !_connected) {
            return;
        }

        NetworkPackage pack = new NetworkPackage();
        pack.type = type;
        pack.data = data;

        const bool ignoreLength = true;
        Send(pack.Encode(ignoreLength));
    }

    public override int PeekMessageNumber() {
        return _queue.Count;
    }

    public override bool Receive(out byte[] data) {
        data = default;
        if (_queue.Count > 0) {
            data = _queue.Dequeue();
            return true;
        }
        return false;
    }

    public override void Update() {
        HandleCmdQueue();

        if (_connected) {
            HandleRecvQueue();

            uint current = GetClockMS();
            _kcp.Update(current);
        }
    }

    void HandleRecvQueue() {
        if (_kcpQueue.Count <= 0) {
            return;
        }

        List<byte[]> kcpData = new List<byte[]>();
        lock (_kcpQueue) {
            while (_kcpQueue.Count > 0) {
                kcpData.Add(_kcpQueue.Dequeue());
            }
        }
        foreach (byte[] data in kcpData) {
            int ret = _kcp.Input(data);
            // invalid kcp package
            if (ret < 0) {
                continue;
            }
            for (int size = _kcp.PeekSize(); size > 0; size = _kcp.PeekSize()) {
                var recvBuffer = new byte[size];
                if (_kcp.Recv(recvBuffer) > 0) {
                    _queue.Enqueue(recvBuffer);
                }
            }
        }

    }

    void HandleCmdQueue() {
        if (_cmdQueue.Count <= 0) {
            return;
        }

        List<KCPDatagram> kcpCommands = new List<KCPDatagram>();
        lock (_cmdQueue) {
            while (_cmdQueue.Count > 0) {
                kcpCommands.Add(_cmdQueue.Dequeue());
            }
        }

        foreach (KCPDatagram kcpCmd in kcpCommands) {
            // 未连接 但是收到了数据
            if (!_connected) {
                Disconnect();
                continue;
            }

            // 收到 RST 直接关闭连接
            if (kcpCmd.cmd == KCPDatagram.Command.RST) {
                Close();
                continue;
            }

            // 目前不支持 SYN
            if (kcpCmd.cmd == KCPDatagram.Command.SYN) {
                Disconnect();
                continue;
            }
        }
    }

    private static readonly DateTime UTCTimeBegin = new DateTime(1970, 1, 1);
    static UInt32 GetClockMS() {
        return (UInt32)(Convert.ToInt64(DateTime.UtcNow.Subtract(UTCTimeBegin).TotalMilliseconds) & 0xffffffff);
    }

    long GetMSClock() {
        long currentTicks = DateTime.Now.Ticks;
        DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        long currentMillis = (currentTicks - dtFrom.Ticks) / 10000;
        return currentMillis - 28800000;
    }
}

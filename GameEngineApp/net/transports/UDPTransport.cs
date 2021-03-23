using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;


public class UDPTransport : Transport
{
    private Socket _socket;
    private readonly Queue<byte[]> _queue = new Queue<byte[]>();
    private bool _connected = false;

    private const int _maxBufferSize = 65535;
    private byte[] _buffer = new byte[_maxBufferSize];


    public override bool IsConnected() {
        return _connected;
    }

    public override bool Connect(string ip, int port) {
        try
        {
            _connected = false;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _socket.Connect(ip, port);
            _connected = true;
            _socket.BeginReceive(_buffer, 0, _maxBufferSize, SocketFlags.None, new AsyncCallback(OnReceive), _socket);
        }
        catch (Exception e)
        {
            Console.WriteLine("connect failed: " + e.Message);
            _connected = false;
        }

        // clear message queue
        lock (_queue)
        {
            _queue.Clear();
        }

        return _connected;
    }

    /*
    对接收 Server Message 的支持与限制:
    1. 一个 UDP 报文中可以有多个 Message
    2. 单个 Message 的大小不允许超过 65535(最佳实践是不应超过 MTU)
    3. Message 不允许跨报文存在
    */
    void OnReceive(IAsyncResult ar)
    {
        try
        {
            int count = _socket.EndReceive(ar);
            ProcessReceiveData(count);
            _socket.BeginReceive(_buffer, 0, _maxBufferSize, SocketFlags.None, new AsyncCallback(OnReceive), _socket);
        }
        catch (Exception e)
        {
            Console.WriteLine("receive failed: " + e);
            _connected = false;
        }
    }

    void ProcessReceiveData(int receiveSize) {
        int offset = 0;

        while (offset < receiveSize) {
            byte[] lenBytes = new byte[sizeof(int)];
            Array.Copy(_buffer, offset, lenBytes, 0, sizeof(int));
            // converter big-endian to littleEndian
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lenBytes);
            }
            int messageLength = BitConverter.ToInt32(lenBytes, 0);

            // Message 跨报文，直接放弃处理
            if (offset + sizeof(int) + messageLength > receiveSize) {
                break;
            }

            byte[] messageBytes = new byte[messageLength];
            Array.Copy(_buffer, offset + sizeof(int), messageBytes, 0, messageLength);
            lock (_queue)
            {
                _queue.Enqueue(messageBytes);
            }

            offset += (sizeof(int) + messageLength);
        }
    }

    public override void Disconnect() {
        if (_socket != null)
        {
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception e)
            {
                Console.WriteLine("Disconnect failed: " + e);
            }
            finally
            {
                _socket.Close();
            }
        }

        _connected = false;
    }

    public override void Send(byte[] data) {
        if (data.Length <= 0 || !_connected) {
            return;
        }
        _socket.BeginSend(data, 0, data.Length, 0, new AsyncCallback(OnSend), _socket);
    }

    public override void Send(int type, byte[] data) {
        if (data.Length <= 0 || !_connected) {
            return;
        }
        NetworkPackage pack = new NetworkPackage();
        pack.type = type;
        pack.data = data;
        Send(pack.Encode());
    }

    void OnSend(IAsyncResult result)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket socket = (Socket)result.AsyncState;

            // Complete sending the data to the remote device.  
            int bytesSent = socket.EndSend(result);
        }
        catch (Exception e)
        {
            Console.WriteLine("Send failed: " + e);
        }
    }

    public override int PeekMessageNumber() {
        return _queue.Count;
    }

    public override bool Receive(out byte[] data) {
        lock (_queue)
        {
            data = default;
            if (_queue.Count > 0)
            {
                data = _queue.Dequeue();
                return true;
            }
        }
        return false;
    }

}

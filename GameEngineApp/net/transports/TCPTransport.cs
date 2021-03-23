using System;
using System.Collections.Generic;
using System.Net.Sockets;

public class TCPTransport : Transport
{
    private Socket _socket;
    private readonly Queue<byte[]> _queue = new Queue<byte[]>();
    private bool _connected = false;
    private byte[] _buffer;
    private int _bytesNeed;
    private int _bytesRead;
    private bool _isReadDataSize;

    public override bool IsConnected() {
        return _connected;
    }

    public override bool Connect(string ip, int port) {
        try
        {
            _connected = false;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(ip, port);
            _connected = true;
            // receive data size at first
            _bytesNeed = sizeof(int);
            _bytesRead = 0;
            _isReadDataSize = true;
            _buffer = new byte[_bytesNeed];
            _socket.BeginReceive(_buffer, _bytesRead, _bytesNeed, 0, new AsyncCallback(OnReceive), _socket);
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

    void OnReceive(IAsyncResult ar)
    {
        if (!_connected) {
            return;
        }

        try
        {
            int count = _socket.EndReceive(ar);
            if (count <= 0) {
                return;
            }

            _bytesRead += count;
            if (_bytesRead == _bytesNeed) {
                ProcessReceiveData();
            }

            _socket.BeginReceive(_buffer, _bytesRead, _bytesNeed - _bytesRead, 0, new AsyncCallback(OnReceive), _socket);              
        }
        catch (Exception e)
        {
            Console.WriteLine("receive failed: " + e);
            _connected = false;
        }
    }

    void ProcessReceiveData() {
        if (_isReadDataSize)
        {
            // converter big-endian to littleEndian
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(_buffer);
            }
            _bytesNeed = BitConverter.ToInt32(_buffer, 0);
            if (_bytesNeed <= 0) {
                return;
            }
            _isReadDataSize = false;
            _bytesRead = 0;
            _buffer = new byte[_bytesNeed];
        } else {
            lock (_queue)
            {
                _queue.Enqueue(_buffer);
            }

            _isReadDataSize = true;
            _bytesNeed = sizeof(int);
            _bytesRead = 0;
            _buffer = new byte[_bytesNeed];
        }
    }

    public override void Disconnect() {
        if (_socket != null)
        {
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception)
            {
                // log error
            }
            finally
            {
                _socket.Close();
            }
        }
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
        catch (Exception)
        {
            // log error
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

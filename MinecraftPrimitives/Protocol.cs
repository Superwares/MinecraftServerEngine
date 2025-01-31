
using Common;
using Containers;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace MinecraftPrimitives
{
    internal static class SocketMethods
    {
        public static int GetLocalPort(System.Net.Sockets.Socket socket)
        {
            System.Diagnostics.Debug.Assert(socket != null);

            System.Net.IPEndPoint localEndPoint = (System.Net.IPEndPoint)socket.LocalEndPoint;
            System.Diagnostics.Debug.Assert(localEndPoint != null);
            return localEndPoint.Port;
        }

        public static System.Net.Sockets.Socket Establish(ushort port)
        {
            System.Net.Sockets.Socket socket = new(
                System.Net.Sockets.SocketType.Stream,
                System.Net.Sockets.ProtocolType.Tcp);

            System.Net.IPEndPoint localEndPoint = new(System.Net.IPAddress.Any, port);
            socket.Bind(localEndPoint);
            socket.Listen();

            return socket;
        }

        /// <exception cref="TryAgainException"></exception>
        public static System.Net.Sockets.Socket Accept(System.Net.Sockets.Socket socket)
        {
            System.Diagnostics.Debug.Assert(socket != null);

            try
            {
                System.Net.Sockets.Socket newSocket = socket.Accept();

                return newSocket;
            }
            catch (System.Net.Sockets.SocketException e)
            {
                if (e.SocketErrorCode == System.Net.Sockets.SocketError.WouldBlock)
                {
                    throw new TryAgainException();
                }

                throw;
            }

            /*System.Diagnostics.Debug.Assert(false);*/
        }

        public static bool Poll(System.Net.Sockets.Socket socket, System.TimeSpan span)
        {
            System.Diagnostics.Debug.Assert(socket != null);

            // TODO: check the socket is binding and listening.

            if (IsBlocking(socket) &&
                socket.Poll(span, System.Net.Sockets.SelectMode.SelectRead) == false)
            {
                return false;
            }

            return true;
        }

        public static void SetBlocking(
            System.Net.Sockets.Socket socket, bool f)
        {
            System.Diagnostics.Debug.Assert(socket != null);

            socket.Blocking = f;
        }

        public static bool IsBlocking(
            System.Net.Sockets.Socket socket)
        {
            System.Diagnostics.Debug.Assert(socket != null);

            return socket.Blocking;
        }

        /// <exception cref="DisconnectedClientException"></exception>
        /// <exception cref="TryAgainException"></exception>
        public static int RecvBytes(
            System.Net.Sockets.Socket socket,
            byte[] data, int offset, int size)
        {
            System.Diagnostics.Debug.Assert(socket != null);

            if (data == null || data.Length == 0)
            {
                System.Diagnostics.Debug.Assert(offset == 0);
                System.Diagnostics.Debug.Assert(size == 0);
                return 0;
            }

            System.Diagnostics.Debug.Assert(offset <= data.Length);
            System.Diagnostics.Debug.Assert(offset >= 0);
            System.Diagnostics.Debug.Assert(offset + size <= data.Length);
            System.Diagnostics.Debug.Assert(size >= 0);
            if (size == 0)
            {
                return 0;
            }

            System.Diagnostics.Debug.Assert(data != null);
            System.Diagnostics.Debug.Assert(data.Length > 0);

            System.Diagnostics.Debug.Assert(offset <= data.Length);
            System.Diagnostics.Debug.Assert(offset >= 0);

            System.Diagnostics.Debug.Assert(offset + size <= data.Length);
            System.Diagnostics.Debug.Assert(size >= 0);

            try
            {
                int n = socket.Receive(data, offset, size, System.Net.Sockets.SocketFlags.None);
                if (n == 0)
                {
                    throw new DisconnectedClientException();
                }


                System.Diagnostics.Debug.Assert(n <= size);
                return n;
            }
            catch (System.Net.Sockets.SocketException e)
            when (e.SocketErrorCode == System.Net.Sockets.SocketError.WouldBlock)
            {
                //throw new TryAgainException();
                System.Diagnostics.Debug.Assert(size == 1);
                return 0;
            }
            catch (System.Net.Sockets.SocketException e)
            when (e.SocketErrorCode == System.Net.Sockets.SocketError.ConnectionAborted)
            {
                throw new DisconnectedClientException();
            }
            catch (System.Net.Sockets.SocketException e)
            when (e.SocketErrorCode == System.Net.Sockets.SocketError.ConnectionReset)
            {
                throw new DisconnectedClientException();
            }
            catch (System.Net.Sockets.SocketException)
            {
                throw;
            }

        }

        /// <exception cref="DisconnectedClientException"></exception>
        /// <exception cref="TryAgainException"></exception>
        //public static byte RecvByte(System.Net.Sockets.Socket socket)
        //{
        //    System.Diagnostics.Debug.Assert(socket != null);

        //    byte[] buffer = new byte[1];

        //    int n = RecvBytes(socket, buffer, 0, 1);
        //    System.Diagnostics.Debug.Assert(n == 1);

        //    return buffer[0];
        //}

        /// <exception cref="DisconnectedClientException"></exception>
        /// <exception cref="TryAgainException"></exception>
        public static int SendBytes(
            System.Net.Sockets.Socket socket,
            byte[] data, int offset, int size)
        {
            System.Diagnostics.Debug.Assert(socket != null);

            if (data == null || data.Length == 0)
            {
                System.Diagnostics.Debug.Assert(offset == 0);
                System.Diagnostics.Debug.Assert(size == 0);
                return 0;
            }

            System.Diagnostics.Debug.Assert(data != null);

            System.Diagnostics.Debug.Assert(offset <= data.Length);
            System.Diagnostics.Debug.Assert(offset >= 0);

            System.Diagnostics.Debug.Assert(offset + size == data.Length);
            System.Diagnostics.Debug.Assert(size >= 0);

            try
            {
                System.Diagnostics.Debug.Assert(data != null);
                int n = socket.Send(data, offset, size, System.Net.Sockets.SocketFlags.None);

                System.Diagnostics.Debug.Assert(n >= 0);
                System.Diagnostics.Debug.Assert(n == data.Length);
                return n;
            }
            catch (System.Net.Sockets.SocketException e)
            when (e.SocketErrorCode == System.Net.Sockets.SocketError.WouldBlock)
            {
                //throw new TryAgainException();
                System.Diagnostics.Debug.Assert(data != null);
                System.Diagnostics.Debug.Assert(data.Length == 1);
                return 0;
            }
            catch (System.Net.Sockets.SocketException e)
            when (e.SocketErrorCode == System.Net.Sockets.SocketError.ConnectionAborted)
            {
                throw new DisconnectedClientException();
            }
            catch (System.Net.Sockets.SocketException e)
            when (e.SocketErrorCode == System.Net.Sockets.SocketError.ConnectionReset)
            {
                throw new DisconnectedClientException();
            }
            catch (System.Net.Sockets.SocketException)
            {
                throw;
            }

        }

        /// <exception cref="DisconnectedClientException"></exception>
        /// <exception cref="TryAgainException"></exception>
        //public static int SendByte(System.Net.Sockets.Socket socket, byte v)
        //{
        //    System.Diagnostics.Debug.Assert(socket != null);

        //    return SendBytes(socket, [v]);
        //}

    }

    public sealed class MinecraftClient : System.IDisposable
    {
        private bool _disposed = false;

        private const int Timeout = 100;
        private int _tryAgainHitCount = 0;

        private const byte SegmentBits = 0x7F;
        private const byte ContinueBit = 0x80;

        private int _sizeRecv = 0, _offsetRecv = 0;
        private byte[] _dataRecv = null;

        private int _presizeSend = 0, _sizeSend = 0, _offsetSend = 0;
        private byte[] _dataSend = null;

        private readonly System.Net.Sockets.Socket Socket;

        public int LocalPort => SocketMethods.GetLocalPort(Socket);

        /// <exception cref="TryAgainException"></exception>
        internal static MinecraftClient Accept(System.Net.Sockets.Socket socket)
        {
            //TODO: Check the socket is Binding and listening correctly.

            System.Net.Sockets.Socket newSocket = SocketMethods.Accept(socket);
            SocketMethods.SetBlocking(newSocket, false);

            /*Console.WriteLine($"socket: {socket.LocalEndPoint}");*/


            return new(newSocket);
        }

        private MinecraftClient(System.Net.Sockets.Socket socket)
        {
            System.Diagnostics.Debug.Assert(SocketMethods.IsBlocking(socket) == false);
            Socket = socket;
        }

        ~MinecraftClient()
        {
            System.Diagnostics.Debug.Assert(false);

            //Dispose(false);
        }



        /// <exception cref="UnexpectedClientBehaviorExecption"></exception>
        /// <exception cref="DisconnectedClientException"></exception>
        /// <exception cref="TryAgainException"></exception>
        private int RecvSize()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(SocketMethods.IsBlocking(Socket) == false);

            System.Diagnostics.Debug.Assert(_sizeRecv >= 0);
            System.Diagnostics.Debug.Assert(_offsetRecv >= 0);

            byte v;
            int n;
            byte[] data = new byte[1];

            while (true)
            {
                n = SocketMethods.RecvBytes(Socket, data, 0, 1);
                System.Diagnostics.Debug.Assert(n <= 1);

                if (n == 0)
                {
                    throw new TryAgainException();
                }

                v = data[0];  // TODO: Refactoring: only using byte v and its pointer to pass to the RecvBytes by array...

                _sizeRecv |= (v & SegmentBits) << _offsetRecv;
                if ((v & ContinueBit) == 0)
                {
                    break;
                }

                _offsetRecv += 7;

                if (_offsetRecv >= 32)
                {
                    throw new InvalidEncodingException();
                }

                System.Diagnostics.Debug.Assert(_offsetRecv > 0);
            }


            return _sizeRecv;
        }

        /// <exception cref="DisconnectedClientException"></exception>
        /// <exception cref="TryAgainException"></exception>
        private void SendSize(int size)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            _presizeSend = size;

            byte value;
            int n;

            while (true)
            {
                if ((_presizeSend & ~SegmentBits) == 0)
                {
                    value = (byte)(_presizeSend & SegmentBits);
                    n = SocketMethods.SendBytes(Socket, [value], 0, 1);

                    System.Diagnostics.Debug.Assert(n <= 1);
                    if (n == 0)
                    {
                        throw new TryAgainException();
                    }

                    break;
                }

                value = (byte)((_presizeSend & SegmentBits) | ContinueBit);
                System.Diagnostics.Debug.Assert(((uint)255 ^ (byte)0b_11111111U) == 0);  // TODO: ?

                n = SocketMethods.SendBytes(Socket, [value], 0, 1);

                System.Diagnostics.Debug.Assert(n <= 1);
                if (n == 0)
                {
                    throw new TryAgainException();
                }

                _presizeSend >>= 7;
            }

            System.Diagnostics.Debug.Assert(_presizeSend >= 0);
            _presizeSend = 0;
        }

        /// <exception cref="UnexpectedClientBehaviorExecption"></exception>
        /// <exception cref="DisconnectedClientException"></exception>
        /// <exception cref="TryAgainException"></exception>
        public void Recv(MinecraftProtocolDataStream buffer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(SocketMethods.IsBlocking(Socket) == false);

            try
            {
                if (_dataRecv == null)
                {
                    _sizeRecv = RecvSize();
                    _offsetRecv = 0;

                    if (_sizeRecv == 0)
                    {
                        return;
                    }

                    System.Diagnostics.Debug.Assert(_dataRecv == null);
                    System.Diagnostics.Debug.Assert(_sizeRecv > 0);

                    // TODO: Pooling: Instead of dynamically allocating new memory, pre-allocate a fixed amount and reuse it.
                    _dataRecv = new byte[_sizeRecv];

                }

                int n = SocketMethods.RecvBytes(Socket, _dataRecv, _offsetRecv, _sizeRecv);
                System.Diagnostics.Debug.Assert(n <= _sizeRecv);

                if (n < _sizeRecv)
                {
                    _sizeRecv -= n;
                    _offsetRecv += n;

                    throw new TryAgainException();
                }

                buffer.WriteData(_dataRecv);

                _sizeRecv = 0;
                _offsetRecv = 0;
                _dataRecv = null;

                _tryAgainHitCount = 0;
            }
            catch (TryAgainException)
            {
                /*Console.WriteLine($"_tryAgainHitCount: {_tryAgainHitCount}");*/
                if (Timeout < ++_tryAgainHitCount)
                {
                    throw new DataRecvTimeoutException();
                }

                throw;
            }

        }

        /// <exception cref="DisconnectedClientException"></exception>
        /// <exception cref="TryAgainException"></exception>
        public void Send(MinecraftProtocolDataStream buffer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            int n;

            try
            {
                if (_dataSend != null)
                {
                    if (_dataSend.Length > 0)
                    {
                        System.Diagnostics.Debug.Assert(_presizeSend >= 0);
                        if (_presizeSend > 0)
                        {
                            SendSize(_presizeSend);

                            System.Diagnostics.Debug.Assert(_presizeSend == 0);
                            System.Diagnostics.Debug.Assert(_sizeSend == _dataSend.Length);
                            System.Diagnostics.Debug.Assert(_offsetSend == 0);
                        }

                        n = SocketMethods.SendBytes(Socket, _dataSend, _offsetSend, _sizeSend);

                        if (n < _sizeSend)
                        {
                            _sizeSend -= n;
                            _offsetSend += n;

                            throw new TryAgainException();
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(_presizeSend == 0);
                        System.Diagnostics.Debug.Assert(_sizeSend == 0);
                        System.Diagnostics.Debug.Assert(_offsetSend == 0);
                        _dataSend = null;
                    }




                }

                _dataSend = buffer.ReadData();

                System.Diagnostics.Debug.Assert(_dataSend != null);
                if (_dataSend.Length == 0)
                {
                    _presizeSend = 0;
                    _sizeSend = 0;
                    _offsetSend = 0;
                    _dataSend = null;

                    return;
                }

                _sizeSend = _dataSend.Length;
                _offsetSend = 0;

                SendSize(_dataSend.Length);

                System.Diagnostics.Debug.Assert(_presizeSend == 0);

                n = SocketMethods.SendBytes(Socket, _dataSend, 0, _sizeSend);

                System.Diagnostics.Debug.Assert(n <= _sizeSend);
                if (n < _sizeSend)
                {
                    System.Diagnostics.Debug.Assert(_presizeSend == 0);
                    _sizeSend -= n;
                    _offsetSend += n;

                    throw new TryAgainException();
                }

                _presizeSend = 0;
                _sizeSend = 0;
                _offsetSend = 0;
                _dataSend = null;

                _tryAgainHitCount = 0;
            }
            catch (TryAgainException)
            {
                /*Console.WriteLine($"_tryAgainHitCount: {_tryAgainHitCount}");*/
                if (Timeout < ++_tryAgainHitCount)
                {
                    throw new DataRecvTimeoutException();
                }

                throw;
            }
        }

        public void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.
            Socket.Dispose();
            _dataRecv = null;

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }

    public readonly struct UserProperty
    {
        public readonly string Name;
        public readonly string Value;
        public readonly string Signature;

        internal UserProperty(string name, string value, string signature)
        {
            System.Diagnostics.Debug.Assert(name != null);
            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(name) == false);
            System.Diagnostics.Debug.Assert(value != null);
            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(value) == false);

            Name = name;
            Value = value;
            Signature = signature;
        }
    }

    public readonly struct User
    {
        public readonly MinecraftClient Client;
        public readonly UserId Id;
        public readonly string Username;

        public readonly UserProperty[] Properties;

        internal User(
            MinecraftClient client,
            System.Guid userId, string username,
            params UserProperty[] properties)
        {
            System.Diagnostics.Debug.Assert(client != null);
            System.Diagnostics.Debug.Assert(userId != System.Guid.Empty);
            System.Diagnostics.Debug.Assert(username != null);
            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(username) == false);

            if (properties == null)
            {
                properties = [];
            }

            Client = client;
            Id = new UserId(userId);
            Username = username;

            Properties = new UserProperty[properties.Length];
            System.Array.Copy(properties, Properties, properties.Length);
        }

    }

    public interface IConnectionListener : System.IDisposable
    {

        public void AddUser(User user);

    }

    public sealed class ClientListener : System.IDisposable
    {
        private static readonly System.TimeSpan PendingTimeout = System.TimeSpan.FromSeconds(1);

        private bool _disposed = false;

        private readonly IConnectionListener _ConnectionListener;


        private readonly System.Net.Sockets.Socket Socket;  // Disposable

        private readonly Queue<MinecraftClient> Visitors;  // Disposable

        /*
         * 0: Handshake
         * 1: Request
         * 2: Ping
         * 3: Start Login
         */
        private readonly Queue<int> LevelQueue;  // Disposable


        public ClientListener(IConnectionListener connListener, ushort port)
        {
            _ConnectionListener = connListener;

            Socket = SocketMethods.Establish(port);
            Visitors = new();
            LevelQueue = new();

            SocketMethods.SetBlocking(Socket, true);
        }

        ~ClientListener() => System.Diagnostics.Debug.Assert(false);

        private int HandleVisitors()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            int count = Visitors.Count;
            System.Diagnostics.Debug.Assert(count == LevelQueue.Count);
            if (count == 0) return 0;

            bool close, success;

            for (; count > 0; --count)
            {
                close = success = false;

                MinecraftClient client = Visitors.Dequeue();
                int level = LevelQueue.Dequeue();

                /*Console.WriteLine($"count: {count}, level: {level}");*/

                System.Diagnostics.Debug.Assert(level >= 0);
                using MinecraftProtocolDataStream buffer = new();

                try
                {
                    if (level == 0)
                    {
                        /*Console.WriteLine("Handshake!");*/
                        client.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundHandshakingPacket.SetProtocolPacketId != packetId)
                        {
                            throw new UnexpectedPacketException();
                        }

                        SetProtocolPacket packet = SetProtocolPacket.Read(buffer);

                        if (!buffer.Empty)
                        {
                            throw new BufferOverflowException();
                        }

                        switch (packet.NextState)
                        {
                            default:
                                throw new InvalidEncodingException();
                            case Packet.States.Status:
                                level = 1;
                                break;
                            case Packet.States.Login:
                                level = 3;
                                break;
                        }

                    }

                    if (level == 1)  // Request
                    {
                        /*Console.WriteLine("Request!");*/
                        client.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundStatusPacket.RequestPacketId != packetId)
                        {
                            throw new UnexpectedPacketException();
                        }

                        RequestPacket requestPacket = RequestPacket.Read(buffer);

                        if (!buffer.Empty)
                        {
                            throw new BufferOverflowException();
                        }

                        // TODO
                        ResponsePacket responsePacket = new(100, 10, "Hello, World!");
                        responsePacket.Write(buffer);

                        level = 4;
                        client.Send(buffer);

                        level = 2;
                    }

                    if (level == 4)
                    {
                        client.Send(buffer);

                        level = 2;
                    }

                    if (level == 2)  // Ping
                    {
                        /*Console.WriteLine("Ping!");*/
                        client.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundStatusPacket.PingPacketId != packetId)
                        {
                            throw new UnexpectedPacketException();
                        }

                        PingPacket inPacket = PingPacket.Read(buffer);

                        if (!buffer.Empty)
                        {
                            throw new BufferOverflowException();
                        }

                        PongPacket outPacket = new(inPacket.Payload);
                        outPacket.Write(buffer);

                        level = 5;
                        client.Send(buffer);
                    }

                    if (level == 3)  // Start Login
                    {
                        client.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundLoginPacket.StartLoginPacketId != packetId)
                        {
                            throw new UnexpectedPacketException();
                        }

                        StartLoginPacket inPacket = StartLoginPacket.Read(buffer);

                        if (!buffer.Empty)
                        {
                            throw new BufferOverflowException();
                        }

                        // TODO: Check username is empty or invalid.

                        System.Guid userId = System.Guid.Empty;
                        System.Diagnostics.Debug.Assert(inPacket.Username != null);
                        System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(inPacket.Username) == false);
                        string username = inPacket.Username;

                        try
                        {
                            // TODO: Refactoring

                            // TODO: Use own http client in common library.
                            using System.Net.Http.HttpClient httpClient = new();
                            string url = string.Format(
                                "https://api.mojang.com/users/profiles/minecraft/{0}",
                                username);

                            using System.Net.Http.HttpRequestMessage request = new(System.Net.Http.HttpMethod.Get, url);

                            // TODO: handle HttpRequestException
                            using System.Net.Http.HttpResponseMessage response = httpClient.Send(request);

                            using System.IO.Stream stream = response.Content.ReadAsStream();
                            using System.IO.StreamReader reader = new(stream);
                            string str = reader.ReadToEnd();
                            System.Collections.Generic.Dictionary<string, string> dictionary = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, string>>(str);
                            System.Diagnostics.Debug.Assert(dictionary != null);

                            userId = System.Guid.Parse(dictionary["id"]);

                            //System.Diagnostics.Debug.Assert(string.Equals(dictionary["name"], username) == true);
                            username = dictionary["name"];  // TODO: check username is valid

                            // TODO: Handle to throw exception
                            /*System.Diagnostics.Debug.Assert(inPacket.Username == username);*/
                        }
                        catch (System.Exception e)
                        {
                            MyConsole.Warn(e.Message);

                            userId = System.Guid.NewGuid();
                        }

                        //System.Diagnostics.Debug.Assert(userId != System.Guid.Empty);
                        //System.Diagnostics.Debug.Assert(username != null);
                        //System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(username) == false);
                        //LoginSuccessPacket outPacket1 = new(userId, username);
                        //outPacket1.Write(buffer);

                        //client.Send(buffer);

                        UserProperty[] properties = null;

                        try
                        {
                            // TODO: Refactoring

                            using System.Net.Http.HttpClient httpClient = new();
                            string url = string.Format(
                                "https://sessionserver.mojang.com/session/minecraft/profile/{0}?unsigned=false",
                                userId.ToString());

                            using System.Net.Http.HttpRequestMessage request = new(System.Net.Http.HttpMethod.Get, url);

                            // TODO: handle HttpRequestException
                            using System.Net.Http.HttpResponseMessage response = httpClient.Send(request);

                            using System.IO.Stream stream = response.Content.ReadAsStream();
                            using System.IO.StreamReader reader = new(stream);
                            string str = reader.ReadToEnd();
                            var jsonResponse = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(str);

                            var propertiesArray = jsonResponse.GetProperty("properties").EnumerateArray();
                            properties = System.Linq.Enumerable.ToArray(
                                System.Linq.Enumerable.Select(propertiesArray,
                                prop => new UserProperty(
                                    prop.GetProperty("name").GetString(),
                                    prop.GetProperty("value").GetString(),
                                    prop.GetProperty("signature").GetString()
                                )));

                        }
                        catch (System.Exception e)
                        {
                            MyConsole.Warn(e.Message);
                        }

                        User user = new(client, userId, username, properties);

                        // TODO: Must dealloc id when connection is disposed.
                        _ConnectionListener.AddUser(user);

                        success = true;
                    }

                    if (level == 5)
                    {
                        client.Send(buffer);
                    }

                    System.Diagnostics.Debug.Assert(buffer.Size == 0);

                    if (success == false)
                    {
                        close = true;
                    }

                }
                catch (TryAgainException)
                {
                    System.Diagnostics.Debug.Assert(success == false);
                    System.Diagnostics.Debug.Assert(close == false);

                }
                catch (UnexpectedClientBehaviorExecption)
                {
                    System.Diagnostics.Debug.Assert(success == false);
                    System.Diagnostics.Debug.Assert(close == false);

                    close = true;

                    buffer.Flush();

                    if (level >= 3)  // >= Start Login level
                    {
                        System.Diagnostics.Debug.Assert(false);
                        // TODO: Handle send Disconnect packet with reason.
                    }

                }
                catch (DisconnectedClientException)
                {
                    System.Diagnostics.Debug.Assert(success == false);
                    System.Diagnostics.Debug.Assert(close == false);
                    close = true;

                    /*Console.Write("~");*/

                    buffer.Flush();

                    /*Console.Write($"EndofFileException");*/
                }

                System.Diagnostics.Debug.Assert(buffer.Empty);

                if (!success)
                {
                    if (close == false)
                    {
                        Visitors.Enqueue(client);
                        LevelQueue.Enqueue(level);
                    }
                    else
                    {
                        client.Dispose();
                    }

                    continue;
                }

                System.Diagnostics.Debug.Assert(close == false);

            }

            return Visitors.Count;
        }

        public void StartRoutine()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            /*Console.Print(">");*/

            try
            {
                if (!SocketMethods.IsBlocking(Socket) &&
                    HandleVisitors() == 0)
                {
                    SocketMethods.SetBlocking(Socket, true);
                }

                if (SocketMethods.Poll(Socket, PendingTimeout))
                {
                    MinecraftClient client = MinecraftClient.Accept(Socket);
                    Visitors.Enqueue(client);
                    LevelQueue.Enqueue(0);

                    SocketMethods.SetBlocking(Socket, false);
                }
            }
            catch (TryAgainException)
            {
                /*Console.WriteLine("TryAgainException!");*/
            }


        }

        public void Flush()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // TODO: Handle client, send Disconnet Packet if the client's step is after StartLogin;
        }

        public void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Visitors.Empty);
            System.Diagnostics.Debug.Assert(LevelQueue.Empty);

            // Release resources.
            Socket.Dispose();
            Visitors.Dispose();
            LevelQueue.Dispose();

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }


}

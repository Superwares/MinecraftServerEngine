
using Common;
using Containers;


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
            byte[] buffer, int offset, int size)
        {
            System.Diagnostics.Debug.Assert(socket != null);

            System.Diagnostics.Debug.Assert(buffer != null);

            System.Diagnostics.Debug.Assert(offset <= buffer.Length);
            System.Diagnostics.Debug.Assert(offset >= 0);

            System.Diagnostics.Debug.Assert(offset + size == buffer.Length);
            System.Diagnostics.Debug.Assert(size >= 0);

            try
            {
                int n = socket.Receive(buffer, offset, size, System.Net.Sockets.SocketFlags.None);
                if (n == 0)
                {
                    throw new DisconnectedClientException();
                }

                System.Diagnostics.Debug.Assert(n <= size);

                return n;
            }
            catch (System.Net.Sockets.SocketException e)
            {
                if (e.SocketErrorCode == System.Net.Sockets.SocketError.WouldBlock)
                {
                    throw new TryAgainException();
                }
                if (e.SocketErrorCode == System.Net.Sockets.SocketError.ConnectionAborted)
                {
                    throw new DisconnectedClientException();
                }
                if (e.SocketErrorCode == System.Net.Sockets.SocketError.ConnectionReset)
                {
                    throw new DisconnectedClientException();
                }

                throw;
            }

        }

        /// <exception cref="DisconnectedClientException"></exception>
        /// <exception cref="TryAgainException"></exception>
        public static byte RecvByte(System.Net.Sockets.Socket socket)
        {
            System.Diagnostics.Debug.Assert(socket != null);

            byte[] buffer = new byte[1];

            int n = RecvBytes(socket, buffer, 0, 1);
            System.Diagnostics.Debug.Assert(n == 1);

            return buffer[0];
        }

        /// <exception cref="DisconnectedClientException"></exception>
        /// <exception cref="TryAgainException"></exception>
        public static void SendBytes(System.Net.Sockets.Socket socket, byte[] data)
        {
            System.Diagnostics.Debug.Assert(socket != null);
            System.Diagnostics.Debug.Assert(data != null);

            try
            {
                System.Diagnostics.Debug.Assert(data != null);
                int n = socket.Send(data);
                System.Diagnostics.Debug.Assert(n >= 0);
                System.Diagnostics.Debug.Assert(n == data.Length);
            }
            catch (System.Net.Sockets.SocketException e)
            {
                if (e.SocketErrorCode == System.Net.Sockets.SocketError.WouldBlock)
                {
                    throw new TryAgainException();
                }
                if (e.SocketErrorCode == System.Net.Sockets.SocketError.ConnectionAborted)
                {
                    throw new DisconnectedClientException();
                }

                throw;
            }

        }

        /// <exception cref="DisconnectedClientException"></exception>
        /// <exception cref="TryAgainException"></exception>
        public static void SendByte(System.Net.Sockets.Socket socket, byte v)
        {
            System.Diagnostics.Debug.Assert(socket != null);

            SendBytes(socket, [v]);
        }

    }

    public sealed class MinecraftClient : System.IDisposable
    {
        private bool _disposed = false;

        private const int Timeout = 100;
        private int _tryAgainCount = 0;

        private const byte SegmentBits = 0x7F;
        private const byte ContinueBit = 0x80;

        private int _x = 0, _y = 0;
        private byte[] _data = null;

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

        ~MinecraftClient() => System.Diagnostics.Debug.Assert(false);


        /// <exception cref="UnexpectedClientBehaviorExecption"></exception>
        /// <exception cref="DisconnectedClientException"></exception>
        /// <exception cref="TryAgainException"></exception>
        private int RecvSize()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(SocketMethods.IsBlocking(Socket) == false);

            int size = _x;
            int position = _y;
            System.Diagnostics.Debug.Assert(size >= 0);
            System.Diagnostics.Debug.Assert(position >= 0);

            try
            {
                while (true)
                {
                    byte v = SocketMethods.RecvByte(Socket);

                    size |= (v & SegmentBits) << position;
                    if ((v & ContinueBit) == 0)
                    {
                        break;
                    }

                    position += 7;

                    if (position >= 32)
                    {
                        throw new InvalidEncodingException();
                    }

                    System.Diagnostics.Debug.Assert(position > 0);
                }

            }
            finally
            {
                _x = size;
                _y = position;
                System.Diagnostics.Debug.Assert(_data == null);
            }

            return size;
        }

        /// <exception cref="DisconnectedClientException"></exception>
        /// <exception cref="TryAgainException"></exception>
        private void SendSize(int size)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            while (true)
            {
                if ((size & ~SegmentBits) == 0)
                {
                    System.Diagnostics.Debug.Assert(size <= 0b_00000000_00000000_00000000_11111111);
                    SocketMethods.SendByte(Socket, (byte)size);
                    break;
                }

                int v = (size & SegmentBits) | ContinueBit;
                System.Diagnostics.Debug.Assert(v <= 0b_00000000_00000000_00000000_11111111);
                System.Diagnostics.Debug.Assert(((uint)255 ^ (byte)0b_11111111U) == 0);
                SocketMethods.SendByte(Socket, (byte)v);

                size >>= 7;
            }

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
                if (_data == null)
                {
                    int size = RecvSize();
                    _x = size;
                    _y = 0;

                    if (size == 0) return;

                    System.Diagnostics.Debug.Assert(_data == null);
                    System.Diagnostics.Debug.Assert(size > 0);
                    _data = new byte[size];
                }

                int availSize = _x, offset = _y;

                do
                {
                    try
                    {
                        int n = SocketMethods.RecvBytes(Socket, _data, offset, availSize);
                        System.Diagnostics.Debug.Assert(n <= availSize);

                        availSize -= n;
                        offset += n;
                    }
                    finally
                    {
                        _x = availSize;
                        _y = offset;
                    }

                } while (availSize > 0);

                buffer.WriteData(_data);

                _x = 0;
                _y = 0;
                _data = null;

                _tryAgainCount = 0;
            }
            catch (TryAgainException)
            {
                /*Console.WriteLine($"count: {_count}");*/
                if (Timeout < _tryAgainCount++)
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

            byte[] data = buffer.ReadData();
            SendSize(data.Length);
            SocketMethods.SendBytes(Socket, data);
        }

        public void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.
            Socket.Dispose();
            _data = null;

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }

    public readonly struct User(MinecraftClient client, System.Guid userId, string username)
    {
        public readonly MinecraftClient Client = client;
        public readonly UserId Id = new(userId);
        public readonly string Username = username;
    }

    public interface IConnectionListener : System.IDisposable
    {

        public void AddUser(MinecraftClient client, System.Guid userId, string username);

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

                        MyConsole.Printl("Start http request!");

                        // TODO: Use own http client in common library.
                        using System.Net.Http.HttpClient httpClient = new();
                        string url = string.Format("https://api.mojang.com/users/profiles/minecraft/{0}", inPacket.Username);
                        /*Console.Printll(inPacket.Username);*/
                        /*Console.Printl($"url: {url}");*/
                        using System.Net.Http.HttpRequestMessage request = new(System.Net.Http.HttpMethod.Get, url);

                        // TODO: handle HttpRequestException
                        using System.Net.Http.HttpResponseMessage response = httpClient.Send(request);

                        using System.IO.Stream stream = response.Content.ReadAsStream();
                        using System.IO.StreamReader reader = new(stream);
                        string str = reader.ReadToEnd();
                        System.Collections.Generic.Dictionary<string, string> dictionary = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, string>>(str);
                        System.Diagnostics.Debug.Assert(dictionary != null);

                        System.Guid userId = System.Guid.Parse(dictionary["id"]);
                        string username = dictionary["name"];  // TODO: check username is valid
                        /*Console.Printll($"userId: {userId}");
                        Console.Printll($"username: {username}");*/

                        // TODO: Handle to throw exception
                        /*System.Diagnostics.Debug.Assert(inPacket.Username == username);*/

                        MyConsole.Printl("Finish http request!");

                        LoginSuccessPacket outPacket1 = new(userId, username);
                        outPacket1.Write(buffer);
                        client.Send(buffer);

                        // TODO: Must dealloc id when connection is disposed.
                        _ConnectionListener.AddUser(client, userId, username);

                        success = true;
                    }


                    System.Diagnostics.Debug.Assert(buffer.Size == 0);

                    if (!success) close = true;

                }
                catch (TryAgainException)
                {
                    System.Diagnostics.Debug.Assert(success == false);
                    System.Diagnostics.Debug.Assert(close == false);

                    /*Console.Write($"TryAgain!");*/
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

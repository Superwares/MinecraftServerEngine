using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;  // TODO: Use custom socket object in common library.
using System.Xml;
using Applications;
using Containers;

namespace Protocol
{
    internal static class SocketMethods
    {
        public static Socket Establish(ushort port)
        {
            Socket socket = new(SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint localEndPoint = new(IPAddress.Any, port);
            socket.Bind(localEndPoint);
            socket.Listen();

            return socket;
        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <param name="socket">TODO: Add description.</param>
        /// <returns>TODO: Add description.</returns>
        /// <exception cref="TryAgainException">TODO: Why it's thrown.</exception>
        public static Socket Accept(Socket socket)
        {
            try
            {
                Socket newSocket = socket.Accept();

                return newSocket;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.WouldBlock)
                    throw new TryAgainException();

                throw;
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <param name="socket">TODO: Add description.</param>
        /// <param name="span">TODO: Add description.</param>
        public static bool Poll(Socket socket, TimeSpan span)  // TODO: Use own TimeSpan in common library.
        {
            // TODO: check the socket is binding and listening.

            if (IsBlocking(socket) &&
                socket.Poll(span, SelectMode.SelectRead) == false)
            {
                return false;
            }

            return true;
        }

        public static void SetBlocking(Socket socket, bool f)
        {
            socket.Blocking = f;
        }

        public static bool IsBlocking(Socket socket)
        {
            return socket.Blocking;
        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <param name="socket">TODO: Add description.</param>
        /// <param name="buffer">TODO: Add description.</param>
        /// <param name="offset">TODO: Add description.</param>
        /// <param name="size">TODO: Add description.</param>
        /// <returns></returns>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
        /// <exception cref="TryAgainException">TODO: Why it's thrown.</exception>
        public static int RecvBytes(
            Socket socket, byte[] buffer, int offset, int size)
        {
            try
            {
                int n = socket.Receive(buffer, offset, size, SocketFlags.None);
                if (n == 0)
                    throw new DisconnectedClientException();

                Debug.Assert(n <= size);

                return n;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.WouldBlock)
                    throw new TryAgainException();

                throw;
            }

        }

        /// <summary>
        /// TODO: Add description..
        /// </summary>
        /// <param name="socket">TODO: Add description..</param>
        /// <returns>TODO: Add description..</returns>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
        /// <exception cref="TryAgainException">TODO: Why it's thrown.</exception>
        public static byte RecvByte(Socket socket)
        {
            byte[] buffer = new byte[1];

            int n = RecvBytes(socket, buffer, 0, 1);
            Debug.Assert(n == 1);

            return buffer[0];
        }

        /// <summary>
        /// TODO: Add description..
        /// </summary>
        /// <param name="socket">TODO: Add description..</param>
        /// <param name="data">TODO: Add description..</param>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
        public static void SendBytes(Socket socket, byte[] data)
        {
            try
            {
                Debug.Assert(data != null);
                int n = socket.Send(data);
                Debug.Assert(n == data.Length);
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.ConnectionAborted)
                    throw new DisconnectedClientException();

                throw;
            }

        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <param name="socket">TODO: Add description.</param>
        /// <param name="v">TODO: Add description.</param>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
        public static void SendByte(Socket socket, byte v)
        {
            SendBytes(socket, [v]);
        }

    }

    internal sealed class Client : IDisposable
    {
        private bool _isDisposed = false;

        private const int _TimeoutLimit = 100;
        private int _tryAgainCount = 0;

        private const byte _SegmentBits = 0x7F;
        private const byte _ContinueBit = 0x80;

        private int _x = 0, _y = 0;
        private byte[]? _data = null;

        private Socket _socket;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <param name="socket">TODO: Add description.</param>
        /// <returns>TODO: Add description.</returns>
        /// <exception cref="TryAgainException">TODO: Why it's thrown.</exception>
        internal static Client Accept(Socket socket)
        {
            //TODO: Check the socket is Binding and listening correctly.

            Socket newSocket = SocketMethods.Accept(socket);
            SocketMethods.SetBlocking(newSocket, false);

            /*Console.WriteLine($"socket: {socket.LocalEndPoint}");*/


            return new(newSocket);
        }

        private Client(Socket socket)
        {
            Debug.Assert(SocketMethods.IsBlocking(socket) == false);
            _socket = socket;
        }

        ~Client()
        {
            Dispose(false);
        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <returns>TODO: Add description.</returns>
        /// <exception cref="UnexpectedClientBehaviorExecption">TODO: Why it's thrown.</exception>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
        /// <exception cref="TryAgainException">TODO: Why it's thrown.</exception>
        private int RecvSize()
        {
            Debug.Assert(!_isDisposed);

            Debug.Assert(SocketMethods.IsBlocking(_socket) == false);

            uint uvalue = (uint)_x;
            int position = _y;

            try
            {
                while (true)
                {
                    byte v = SocketMethods.RecvByte(_socket);

                    uvalue |= (uint)(v & _SegmentBits) << position;
                    if ((v & _ContinueBit) == 0)
                        break;

                    position += 7;

                    if (position >= 32)
                        throw new InvalidEncodingException();

                    Debug.Assert(position > 0);
                }

            }
            finally
            {
                _x = (int)uvalue;
                _y = position;
                Debug.Assert(_data == null);
            }

            return (int)uvalue;
        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <param name="size">TODO: Add description.</param>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
        private void SendSize(int size)
        {
            Debug.Assert(!_isDisposed);

            uint uvalue = (uint)size;

            while (true)
            {
                if ((uvalue & ~_SegmentBits) == 0)
                {
                    SocketMethods.SendByte(_socket, (byte)uvalue);
                    break;
                }

                SocketMethods.SendByte(
                    _socket, (byte)((uvalue & _SegmentBits) | _ContinueBit));
                uvalue >>= 7;
            }

        }

        /*public void A()
        {
            SocketMethods.SetBlocking(_socket, true);
        }*/

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <param name="buffer">TODO: Add description.</param>
        /// <exception cref="UnexpectedClientBehaviorExecption">TODO: Why it's thrown.</exception>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
        /// <exception cref="TryAgainException">TODO: Why it's thrown.</exception>
        public void Recv(Buffer buffer)
        {
            Debug.Assert(!_isDisposed);

            Debug.Assert(SocketMethods.IsBlocking(_socket) == false);

            try
            {
                if (_data == null)
                {
                    int size = RecvSize();
                    _x = size;
                    _y = 0;

                    if (size == 0) return;

                    Debug.Assert(_data == null);
                    _data = new byte[size];
                    Debug.Assert(size > 0);
                }

                int availSize = _x, offset = _y;

                do
                {
                    try
                    {
                        int n = SocketMethods.RecvBytes(_socket, _data, offset, availSize);
                        Debug.Assert(n <= availSize);

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
                if (_TimeoutLimit < _tryAgainCount++)
                {
                    throw new DataRecvTimeoutException();
                }

                throw;
            }

        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <param name="buffer">TODO: Add description.</param>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
        public void Send(Buffer buffer)
        {
            Debug.Assert(!_isDisposed);

            byte[] data = buffer.ReadData();
            SendSize(data.Length);
            SocketMethods.SendBytes(_socket, data);
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing == true)
            {
                // Release managed resources.
                _socket.Dispose();
                _data = null;
            }

            // Release unmanaged resources.

            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Close()
        {
            Dispose();
        }

    }

    public class ConnectionListener
    {
        private readonly ConcurrentQueue<(Client, Guid, string)> _clients = new();

        internal void Add(Client client, Guid userId, string username)
        {
            _clients.Enqueue((client, userId, username));
        }

        public void Accept(World world, Queue<(Connection, Player)> connections)
        {
            for (int i = 0; i < _clients.Count; ++i)
            {
                (Client client, System.Guid uniqueId, string username) = _clients.Dequeue();

                if (!world.CanSpawnOrConnectPlayer(uniqueId))
                {
                    _clients.Enqueue((client, uniqueId, username));
                    continue;
                }

                Player player = world.SpawnOrConnectPlayer(username, uniqueId);
                Connection conn = new(client);
                connections.Enqueue((conn, player));
            }

        }

    }

    public class GlobalListener
    {
        private static readonly TimeSpan _PendingTimeout = TimeSpan.FromSeconds(1);

        private readonly ConnectionListener _connListener;

        public GlobalListener(ConnectionListener connListener)
        {
            _connListener = connListener;
        }

        private int HandleVisitors(
            Queue<Client> visitors, Queue<int> levelQueue)
        {
            /*Console.Write(".");*/

            int count = visitors.Count;
            Debug.Assert(count == levelQueue.Count);
            if (count == 0) return 0;

            bool close, success;

            for (; count > 0; --count)
            {
                close = success = false;

                Client client = visitors.Dequeue();
                int level = levelQueue.Dequeue();

                /*Console.WriteLine($"count: {count}, level: {level}");*/

                Debug.Assert(level >= 0);
                using Buffer buffer = new();

                try
                {
                    if (level == 0)
                    {
                        /*Console.WriteLine("Handshake!");*/
                        client.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundHandshakingPacket.SetProtocolPacketId != packetId)
                            throw new UnexpectedPacketException();

                        SetProtocolPacket packet = SetProtocolPacket.Read(buffer);

                        if (!buffer.Empty)
                            throw new BufferOverflowException();

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
                            throw new UnexpectedPacketException();

                        RequestPacket requestPacket = RequestPacket.Read(buffer);

                        if (!buffer.Empty)
                            throw new BufferOverflowException();

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
                            throw new UnexpectedPacketException();

                        PingPacket inPacket = PingPacket.Read(buffer);

                        if (!buffer.Empty)
                            throw new BufferOverflowException();

                        PongPacket outPacket = new(inPacket.Payload);
                        outPacket.Write(buffer);
                        client.Send(buffer);
                    }

                    if (level == 3)  // Start Login
                    {
                        client.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundLoginPacket.StartLoginPacketId != packetId)
                            throw new UnexpectedPacketException();

                        StartLoginPacket inPacket = StartLoginPacket.Read(buffer);

                        if (!buffer.Empty)
                            throw new BufferOverflowException();

                        // TODO: Check username is empty or invalid.

                        /*Console.Write("Start http request!");

                        // TODO: Use own http client in common library.
                        using HttpClient httpClient = new();
                        string url = string.Format("https://api.mojang.com/users/profiles/minecraft/{0}", inPacket.Username);
                        *//*Console.WriteLine(inPacket.Username);
                        Console.WriteLine($"url: {url}");*//*
                        using HttpRequestMessage request = new(HttpMethod.Get, url);

                        // TODO: handle HttpRequestException
                        using HttpResponseMessage response = httpClient.Send(request);

                        using Stream stream = response.Content.ReadAsStream();
                        using StreamReader reader = new(stream);
                        string str = reader.ReadToEnd();
                        System.Collections.Generic.Dictionary<string, string>? dictionary =
                            JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, string>>(str);
                        Debug.Assert(dictionary != null);

                        Guid userId = Guid.Parse(dictionary["id"]);
                        string username = dictionary["name"];  // TODO: check username is valid
                        *//*Console.WriteLine($"userId: {userId}");
                        Console.WriteLine($"username: {username}");*//*

                        // TODO: Handle to throw exception
                        Debug.Assert(inPacket.Username == username);

                        Console.Write("Finish http request!");*/

                        Guid userId = Guid.NewGuid();
                        string username = inPacket.Username;

                        LoginSuccessPacket outPacket1 = new(userId, username);
                        outPacket1.Write(buffer);
                        client.Send(buffer);

                        // TODO: Must dealloc id when connection is disposed.
                        _connListener.Add(client, userId, username);

                        success = true;
                    }


                    Debug.Assert(buffer.Size == 0);

                    if (!success) close = true;

                }
                catch (TryAgainException)
                {
                    Debug.Assert(success == false);
                    Debug.Assert(close == false);

                    /*Console.Write($"TryAgain!");*/
                }
                catch (UnexpectedClientBehaviorExecption)
                {
                    Debug.Assert(success == false);
                    Debug.Assert(close == false);

                    close = true;

                    buffer.Flush();

                    if (level >= 3)  // >= Start Login level
                    {
                        Debug.Assert(false);
                        // TODO: Handle send Disconnect packet with reason.
                    }

                }
                catch (DisconnectedClientException)
                {
                    Debug.Assert(success == false);
                    Debug.Assert(close == false);
                    close = true;

                    /*Console.Write("~");*/

                    buffer.Flush();

                    /*Console.Write($"EndofFileException");*/
                }

                Debug.Assert(buffer.Empty);

                if (!success)
                {
                    if (close == false)
                    {
                        visitors.Enqueue(client);
                        levelQueue.Enqueue(level);
                    }
                    else
                    {
                        client.Close();
                    }

                    continue;
                }
                    
                Debug.Assert(close == false);

            }

            return visitors.Count;
        }

        public void StartRoutine(ConsoleApplication app, ushort port)
        {
            using Socket socket = SocketMethods.Establish(port);

            using Queue<Client> visitors = new();
            /*
             * 0: Handshake
             * 1: Request
             * 2: Ping
             * 3: Start Login
             */
            using Queue<int> levelQueue = new();

            SocketMethods.SetBlocking(socket, true);

            while (app.Running)
            {
                Console.Write(">");

                try
                {
                    if (!SocketMethods.IsBlocking(socket) &&
                        HandleVisitors(visitors, levelQueue) == 0)
                    {
                        SocketMethods.SetBlocking(socket, true);
                    }

                    if (SocketMethods.Poll(socket, _PendingTimeout))
                    {
                        Client client = Client.Accept(socket);
                        visitors.Enqueue(client);
                        levelQueue.Enqueue(0);

                        SocketMethods.SetBlocking(socket, false);
                    }
                }
                catch (TryAgainException)
                {
                    /*Console.WriteLine("TryAgainException!");*/
                    continue;
                }

            }

            // TODO: Handle client, send Disconnet Packet if the client's step is after StartLogin;
        }

    }


}

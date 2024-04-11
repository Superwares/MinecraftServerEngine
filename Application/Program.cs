using Protocol;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;

namespace Application
{


    // TODO: Change to correct name.
    public class PacketTable<T> : IDisposable where T : PlayingPacket
    {
        private bool _disposed = false;

        private readonly Table<int, Queue<T>> _data = new();

        public PacketTable() { }

        ~PacketTable()
        {
            Dispose(false);
        }

        public void Init(int key)
        {
            Debug.Assert(!_disposed);

            Debug.Assert(!_data.Contains(key));

            Queue<T> queue = new();
            _data.Insert(key, queue);
        }

        public void Close(int key)
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_data.Contains(key));

            Queue<T> queue = _data.Extract(key);
            Debug.Assert(queue.Count == 0);
        }

        public void Enqueue(int key, T value)
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_data.Contains(key));

            Queue<T> queue = _data.Lookup(key);
            Debug.Assert(queue != null);

            queue.Enqueue(value);
        }

        public T Dequeue(int key)
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_data.Contains(key));

            Queue<T> queue = _data.Lookup(key);
            Debug.Assert(queue != null);

            return queue.Dequeue();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing == true)
            {
                // managed objects
                _data.Dispose();
            }

            // unmanaged objects

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }

    public sealed class Application : IDisposable
    {
        public delegate void StartRoutine();

        private static readonly Thread _MainThread = Thread.CurrentThread;
        private static readonly int _MainId = _MainThread.ManagedThreadId;

        private bool _disposed = false;

        private readonly object _SharedObject = new();

        private bool _closed = false;
        public bool Closed => _closed;
        public bool Running => !_closed;


        private readonly Queue<Thread> _threads = new();

        private readonly NumList _idList = new();

        private readonly Table<(int, int), Chunk> _chunks = new();

        private readonly ConcurrentQueue<(Connection, Player.ClientsideSettings)> 
            _newJoinedConnections = new();
        private readonly Queue<Connection> _connections = new();

        private readonly Queue<Player> _newJoinedPlayers = new();
        private readonly Queue<Player> _players = new();

        private readonly PacketTable<ClientboundPlayingPacket> 
            _outPackets = new(), _inPackets = new();


        private void Close()
        {
            Debug.Assert(Thread.CurrentThread.ManagedThreadId != _MainId);

            _closed = true;

            while (_threads.Count > 0)
            {
                Thread t = _threads.Dequeue();
                t.Join();
            }

            /*Thread.Sleep(1000 * 5);*/

            Console.WriteLine("Close!");
            lock (_SharedObject) 
                Monitor.Pulse(_SharedObject);
        }

        private Application()
        {
            Debug.Assert(Thread.CurrentThread.ManagedThreadId == _MainId);

            Console.CancelKeyPress += (sender, e) => Close();
        }

        ~Application()
        {
            /*Dispose(false);*/
            Debug.Assert(false);
        }

        private void Run(StartRoutine f)
        {
            Debug.Assert(Closed == false);

            Thread thread = new(new ThreadStart(f));
            thread.Start();

            Debug.Assert(thread.ManagedThreadId != _MainId);

            _threads.Enqueue(thread);
        }

 /*       private void InitOutPackets(int id)
        {
            Debug.Assert(_outPackets.ContainsKey(id) == false);
            _outPackets.Add(id, new());
        }*/

        private int HandleVisitors(
            Queue<Client> visitors, Queue<int> levelQueue,
            Queue<Player.ClientsideSettings> settingsQueue)
        {
            Console.Write(".");

            int count = visitors.Count;
            Debug.Assert(count == levelQueue.Count);
            if (count == 0) return 0;

            bool close, success;

            for (; count > 0; --count)
            {
                close = success = false;

                Client visitor = visitors.Dequeue();
                int level = levelQueue.Dequeue();
                Player.ClientsideSettings? settings = 
                    (level >= 5) ? settingsQueue.Dequeue() : null;

                /*Console.WriteLine($"count: {count}, level: {level}");*/

                Debug.Assert(level >= 0);
                using Buffer buffer = new();

                try
                {
                    if (level == 0)
                    {
                        /*Console.WriteLine("Handshake!");*/
                        visitor.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundHandshakingPacket.SetProtocolPacketId != packetId)
                            throw new UnexpectedPacketException();

                        SetProtocolPacket packet = SetProtocolPacket.Read(buffer);

                        if (buffer.Size > 0)
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
                        visitor.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundStatusPacket.RequestPacketId != packetId)
                            throw new UnexpectedPacketException();

                        RequestPacket requestPacket = RequestPacket.Read(buffer);

                        if (buffer.Size > 0)
                            throw new BufferOverflowException();

                        // TODO
                        ResponsePacket responsePacket = new(100, 10, "Hello, World!");
                        responsePacket.Write(buffer);
                        visitor.Send(buffer);

                        level = 2;
                    }

                    if (level == 2)  // Ping
                    {
                        /*Console.WriteLine("Ping!");*/
                        visitor.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundStatusPacket.PingPacketId != packetId)
                            throw new UnexpectedPacketException();

                        PingPacket inPacket = PingPacket.Read(buffer);

                        if (buffer.Size > 0)
                            throw new BufferOverflowException();

                        PongPacket outPacket = new(inPacket.Payload);
                        outPacket.Write(buffer);
                        visitor.Send(buffer);
                    }

                    if (level == 3)  // Start Login
                    {
                        visitor.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundLoginPacket.StartLoginPacketId != packetId)
                            throw new UnexpectedPacketException();

                        StartLoginPacket inPacket = StartLoginPacket.Read(buffer);

                        if (buffer.Size > 0)
                            throw new BufferOverflowException();

                        // TODO: Check username is empty or invalid.

                        HttpClient httpClient = new();
                        string url = string.Format("https://api.mojang.com/users/profiles/minecraft/{0}", inPacket.Username);
                        /*Console.WriteLine(inPacket.Username);
                        Console.WriteLine($"url: {url}");*/
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
                        /*Console.WriteLine($"userId: {userId}");
                        Console.WriteLine($"username: {username}");*/

                        // TODO: Handle to throw exception
                        Debug.Assert(inPacket.Username == username);

                        LoginSuccessPacket outPacket1 = new(userId, username);
                        outPacket1.Write(buffer);
                        visitor.Send(buffer);

                        int id = _idList.Alloc();
                        JoinGamePacket outPacket2 = new(id, 1, 0, 0, "default", false);
                        outPacket2.Write(buffer);
                        visitor.Send(buffer);

                        /*  // TODO: Must dealloc id when connection is disposed.
                        

                        /*success = true;*/

                        level = 4;
                    }

                    if (level == 4)  // Client Settings
                    {
                        visitor.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundPlayingPacket.ClientSettingsPacketId != packetId)
                            throw new UnexpectedPacketException();

                        // TODO: Handle this packet. (Client Settings)
                        ClientSettingsPacket packet = ClientSettingsPacket.Read(buffer);

                        if (buffer.Size > 0)
                            throw new BufferOverflowException();

                        settings = new(packet.RenderDistance);

                        level = 5;
                    }

                    if (level == 5)  // Plugin Message
                    {
                        Debug.Assert(settings != null);

                        visitor.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (packetId != 0x09)
                            throw new UnexpectedPacketException();

                        // TODO: Handle this packet. (Plugin Message)

                        buffer.Flush();



                        /*Connection conn = new(id, visitor, userId, username);
                        _newJoinedConnections.Enqueue(conn);*/

                        success = true;
                    }


                    Debug.Assert(buffer.Size == 0);

                    close = true;

                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode == SocketError.ConnectionAborted ||
                        false)  // Add other Exceptions here!
                        close = true;
                    else if (e.SocketErrorCode != SocketError.WouldBlock)
                        Debug.Assert(false);

                    Debug.Assert(
                        (e.SocketErrorCode == SocketError.WouldBlock) ? 
                        close == false : true);
                    /*Console.WriteLine($"SocketError.WouldBlock!");*/
                }
                catch (UnexpectedDataException)
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
                catch (EndofFileException)
                {
                    Debug.Assert(success == false);
                    Debug.Assert(close == false);
                    close = true;

                    /*Console.Write("~");*/

                    buffer.Flush();
                }

                if (!success)
                {
                    if (close == false)
                    {
                        
                        visitors.Enqueue(visitor);
                        levelQueue.Enqueue(level);

                        if (level >= 5)
                        {
                            Debug.Assert(settings != null);
                            settingsQueue.Enqueue(settings);
                        }
                        else
                            Debug.Assert(settings == null);

                    }
                    else
                    {
                        visitor.Close();
                    }
                }

            }

            return visitors.Count;
        }

        private void StartListenerRoutine()
        {
            using Socket socket = new(SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint localEndPoint = new(IPAddress.Any, Port);
            socket.Bind(localEndPoint);
            socket.Listen();

            Queue<Client> visitors = new();
            /*
             * 0: Handshake
             * 1: Request
             * 2: Ping
             * 3: Start Login
             * 4: Client Settings
             * 5: Plugin Message
             */
            Queue<int> levelQueue = new();
            Queue<Player.ClientsideSettings> settingsQueue = new();

            socket.Blocking = true;

            while (Running)
            {
                try
                {
                    if (!socket.Blocking &&
                        HandleVisitors(visitors, levelQueue, settingsQueue) == 0)
                    {
                        socket.Blocking = true;
                    }
                    
                    if (socket.Blocking &&
                        socket.Poll(PendingTimeout, SelectMode.SelectRead) == false)
                    {
                        throw new TimeoutException();
                    }

                    Client client = Client.Accept(socket);
                    visitors.Enqueue(client);
                    levelQueue.Enqueue(0);

                    socket.Blocking = false;

                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode != SocketError.WouldBlock)
                        throw new NotImplementedException();
                }
                catch (TimeoutException) 
                {
                    Console.WriteLine("!");
                }

            }

            // TODO: Handle client, send Disconnet Packet if the client's step is after StartLogin;
        }

        private void StartCoreRoutine()
        {
            while (Running)
            {
                // recv packets using connections

                // Barrier

                int count = _newJoinedConnections.Count;
                for (int i = 0; i < count; ++i)
                {
                    Connection conn = _newJoinedConnections.Dequeue();


                    int id = conn.Id;

                    // TODO
                    Player player = new(id, new(0, 60, 0));
                    _newJoinedPlayers.Enqueue(player);

                    _inPackets.Init(id);
                    _outPackets.Init(id);

                    _connections.Enqueue(conn);
                }

                // Barrier

                // handle players

                // Barrier

                while (!_newJoinedPlayers.Empty)
                {
                    Player player = _newJoinedPlayers.Dequeue();

                    // load chunks
                    

                    _players.Enqueue(player);
                }

                // Barrier

                while (_connections.Count > 0)
                {
                    Connection conn = _connections.Dequeue();

                    // send packets
                    _connections.Enqueue(conn);
                }
            }
        }

        private void Dispose(bool disposing)
        {
            Debug.Assert(Thread.CurrentThread.ManagedThreadId == _MainId);
            Debug.Assert(Closed == true);

            lock (_SharedObject)
                Monitor.Wait(_SharedObject);

            if (_disposed) return;

            Debug.Assert(_threads.Count == 0);
            /*Debug.Assert(_chunks.Count == 0);*/
            // TODO: Check the data structures must be empty.
             
            if (disposing == true)
            {
                // Release managed resources.
                _idList.Dispose();
                _newJoinedConnections.Dispose();
                // TODO: Add data structures.
            }

            // Release unmanaged resources.

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public static void Main()
        {
            Debug.Assert(Thread.CurrentThread.ManagedThreadId == _MainId);

            ushort port = 25565;

            using Application app = new();


            Console.WriteLine("Hello, World!");
            
            app.Run(() => listener.StartRoutine(port));

            while (app.Running)
            {
                Thread.Sleep(1000);
            }


        }
    }

}


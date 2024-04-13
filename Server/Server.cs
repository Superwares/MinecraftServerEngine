using Applications;
using Containers;
using Protocol;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;

namespace Application
{
   

    public sealed class Server : ConsoleApplication
    {
        private bool _disposed = false;

        private readonly ConcurrentNumList _idList = new();

        private readonly ConcurrentQueue<(Connection, Player.ClientsideSettings?)> 
            _newConnections = new();
        private readonly Queue<Connection> _connections = new();

        private readonly Queue<Player> _newPlayers = new();
        private readonly Queue<Player> _players = new();

        private readonly Table<Chunk.Position, Chunk> _chunks = new();

        private readonly Table<int, Queue<ServerboundPlayingPacket>> _inPacketTable = new();
        private readonly Table<int, Queue<ClientboundPlayingPacket>> _outPacketTable = new();

        private Server() { }

        ~Server() => Dispose(false);

        /*       private void InitOutPackets(int id)
        {
            Debug.Assert(_outPackets.ContainsKey(id) == false);
            _outPackets.Add(id, new());
        }*/

        private void HandleNewConnections()
        {
            bool start, close;

            int count = _newConnections.Count;
            for (int i = 0; i < count; ++i)
            {
                (Connection conn, Player.ClientsideSettings? settings)
                    = _newConnections.Dequeue();

                start = close = false;

                try
                {
                    conn.HandleSetupProcess(ref settings);

                    Debug.Assert(!start);
                    Debug.Assert(!close);
                    Debug.Assert(settings != null);

                    start = true;
                }
                catch (TryAgainException)
                {
                    Debug.Assert(!start);
                    Debug.Assert(!close);

                    /*Console.WriteLine("TryAgainException!");*/
                }
                catch (DataReadTimeoutException)
                {
                    Debug.Assert(!start);
                    Debug.Assert(!close);

                    close = true;

                    /*Console.WriteLine("DataReadTimeoutException!");*/
                }
                catch (UnexpectedDataException)
                {
                    Debug.Assert(!start);
                    Debug.Assert(!close);

                    close = true;

                    // TODO: Send packets with reason.

                    /*Console.WriteLine("UnexpectedDataException!");*/
                }
                catch (DisconnectedException)
                {
                    Debug.Assert(!start);
                    Debug.Assert(!close);

                    close = true;

                    /*Console.WriteLine("DisconnectedException!");*/
                }

                if (!start)
                {
                    if (!close)
                    {
                        _newConnections.Enqueue((conn, settings));
                    }
                    else
                    {
                        conn.Close();
                    }
                }
                else
                {
                    Debug.Assert(!close);
                    /*Console.WriteLine("Start Game!");*/

                    _connections.Enqueue(conn);

                    int id = conn.Id;

                    Debug.Assert(settings != null);
                    Player player = new(id, new(0, 61, 0), settings);
                    _newPlayers.Enqueue(player);

                    _inPacketTable.Insert(id, new());
                    _outPacketTable.Insert(id, new());
                }

            }

        }

        private void StartCoreRoutine()
        {
            while (Running)
            {
                // recv packets using connections

                // Barrier

                HandleNewConnections();

                // Barrier

                // handle players

                // Barrier

                {
                    int count = _newPlayers.Count;
                    for (int i = 0; i < count; ++i)
                    {
                        Player player = _newPlayers.Dequeue();

                        int id = player.Id;

                        Queue<ClientboundPlayingPacket> outPackets = _outPacketTable.Lookup(id);

                        // load chunks
                        Chunk.Position pChunk = player.PosChunk;
                        int d = player.Settings.renderDistance;
                        foreach (var p in Chunk.Position.GenerateGridAroundCenter(pChunk, d))
                        {
                            bool continuous;
                            int mask;
                            byte[] data;
                            try
                            {
                                Chunk chunk = _chunks.Lookup(p);
                                (continuous, mask, data) = Chunk.Write(chunk);
                            }
                            catch(NotFoundException)
                            {
                                (continuous, mask, data) = Chunk.Write();
                            }

                            LoadChunk packet = new(p.X, p.Z, continuous, mask, data);
                            outPackets.Enqueue(packet);
                        }

                        _players.Enqueue(player);
                    }
                }

                // Barrier

                {
                    int count = _connections.Count;
                    for (int i = 0; i < count; ++i)
                    {
                        Connection conn = _connections.Dequeue();

                        int id = conn.Id;

                        Queue<ClientboundPlayingPacket> outPackets = _outPacketTable.Lookup(id);

                        try
                        {
                            while (!outPackets.Empty)
                            {
                                ClientboundPlayingPacket packet = outPackets.Dequeue();
                                conn.Send(packet);
                            }

                            Debug.Assert(outPackets.Empty);
                        }
                        catch (DisconnectedException)
                        {
                            TODO: handle disconnected connection
                        }

                        /*_connections.Enqueue(conn);*/
                    }
                }

            }

        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing == true)
                {
                    // Release managed resources.
                    _idList.Dispose();
                    _newConnections.Dispose();
                    _chunks.Dispose();
                    _inPacketTable.Dispose();
                    _outPacketTable.Dispose();
                }

                // Release unmanaged resources.

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        public static void Main()
        {
            Console.WriteLine("Hello, World!");

            ushort port = 25565;

            using Server app = new();

            app.Run(app.StartCoreRoutine);

            Listener listener = new();
            app.Run(() => 
                listener.StartRoutine(app, port, app._idList, app._newConnections));

            while (app.Running)
            {

                // Handle Barriers
                Thread.Sleep(1000);
            }


        }

    }

}


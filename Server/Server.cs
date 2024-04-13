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

        private readonly Table<int, Queue<TeleportRecord>> _teleportRecords = new();

        private readonly ConcurrentQueue<(Connection, Player.ClientsideSettings?)> 
            _newConnections = new();
        private readonly Queue<Connection> _abortedConnections = new();
        private readonly Queue<Connection> _unexpectedConnections = new();
        private readonly Queue<Connection> _connections = new();

        private readonly Queue<Player> _spawnedPlayers = new();
        private readonly Table<int, Player> _playerTable = new();
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

        private void StartGameRoutine()
        {
            // recv packets using connections

            // Barrier

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
                        Debug.Assert(player.connected == true);

                        _playerTable.Insert(id, player);
                        _spawnedPlayers.Enqueue(player);

                        _inPacketTable.Insert(id, new());
                        _outPacketTable.Insert(id, new());
                    }

                }
            }

            // Barrier

            // handle players
            {
                int count = _players.Count;
                for (int i = 0; i < count; ++i)
                {
                    Player player = _players.Dequeue();
                    int id = player.Id;

                    if (player.connected == false)
                    {
                        // Release resources of players.
                        Debug.Assert(!_inPacketTable.Contains(id));
                        Debug.Assert(!_outPacketTable.Contains(id));

                        _playerTable.Extract(id);
                        continue;
                    }

                    ServerboundPlayingPacket[] inPackets = _inPacketTable.Lookup(id).Flush();

                    foreach (var _p in inPackets)
                    {
                        throw new NotImplementedException();
                    }

                    _players.Enqueue(player);
                }
            }

            // Barrier

            // Handle entities.

            // Barrier

            {
                
                int count = _spawnedPlayers.Count;
                for (int i = 0; i < count; ++i)
                {

                    Player player = _spawnedPlayers.Dequeue();

                    int id = player.Id;

                    Queue<ClientboundPlayingPacket> outPackets = _outPacketTable.Lookup(id);

                    {
                        SetAbilitiesPacket packet = new(true, true, true, true, 1, 0);
                        outPackets.Enqueue(packet);
                    }

                    // load chunks
                    Chunk.Position pChunk = player.PosChunk;
                    int d = player.Settings.renderDistance;
                    Chunk.Position[] positions = Chunk.Position.GenerateGridAroundCenter(pChunk, d);
                    /*Console.WriteLine(positions.Length);*/
                    foreach (var p in positions)
                    {
                        bool continuous;
                        int mask;
                        byte[] data;
                        if (_chunks.Contains(p))
                        {
                            Chunk chunk = _chunks.Lookup(p);
                            (continuous, mask, data) = Chunk.Write(chunk);
                        }
                        else
                            (continuous, mask, data) = Chunk.Write();

                        Debug.Assert(continuous);
                        LoadChunk packet = new(p.X, p.Z, continuous, mask, data);
                        outPackets.Enqueue(packet);
                    }

                    {
                        // teleport
                        TeleportRecord record = new();

                        // enqueue set player position and look packet
                        TeleportPacket packet = new(
                            player.Pos.X, player.Pos.Y, player.Pos.Z,
                            0, 0,  // TODO
                            false, false, false, false, false,
                            record.Payload);
                        outPackets.Enqueue(packet);

                        if (!_teleportRecords.Contains(id))
                            _teleportRecords.Insert(id, new());

                        _teleportRecords.Lookup(id).Enqueue(record);
                    }

                    _players.Enqueue(player);

                }
            }

            // Barrier

            {

                bool close;
                int count = _connections.Count;
                for (int i = 0; i < count; ++i)
                {
                    close = false;

                    Connection conn = _connections.Dequeue();

                    int id = conn.Id;

                    ClientboundPlayingPacket[] outPackets = _outPacketTable.Lookup(id).Flush();

                    try
                    {
                        foreach (var p in outPackets) conn.Send(p);
                    }
                    catch (DisconnectedException)
                    {
                        Debug.Assert(close == false);
                        close = true;

                    }

                    if (!close)
                    {
                        _connections.Enqueue(conn);
                        continue;
                    }

                    _abortedConnections.Enqueue(conn);

                    Player player = _playerTable.Lookup(id);
                    Debug.Assert(player.connected == true);
                    player.connected = false;

                }
            }

            // Barrier

            {
                int count = _abortedConnections.Count;
                for (int i = 0; i < count; ++i)
                {
                    Connection conn = _abortedConnections.Dequeue();

                    int id = conn.Id;

                    _inPacketTable.Extract(id).Close();
                    _outPacketTable.Extract(id).Close();
                }
            }

        }

        private static ulong GetCurrentTime()
        {
            return (ulong)(DateTime.Now.Ticks / TimeSpan.TicksPerMicrosecond);
        }

        private void StartCoreRoutine()
        {
            ulong interval, total, start, end, elapsed;

            interval = total = (ulong)TimeSpan.FromMilliseconds(50).TotalMicroseconds;
            start = GetCurrentTime();

            while(Running)
            {
                if (total >= interval)
                {
                    total -= interval;

                    StartGameRoutine();
                }

                end = GetCurrentTime();
                elapsed = end - start;
                total += elapsed;
                start = end;

                if (elapsed > interval)
                {
                    Console.WriteLine($"elapsed: {elapsed}");
                    Console.WriteLine("The task is taking longer than expected.");
                }
                /*else
                {
                    int a = (int)((interval - elapsed) * 0.9 / 1000);
                    if (a > 0)
                        Thread.Sleep(a);


                }*/
            }

            // Handle close routine...

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
                listener.StartRoutine(
                    app, port, app._idList, app._newConnections));

            while (app.Running)
            {

                // Handle Barriers
                Thread.Sleep(1000);
            }


        }

    }

}


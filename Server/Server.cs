using Applications;
using Containers;
using Protocol;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Security.AccessControl;
using System.Threading;

namespace Application
{
    

    public sealed class Server : ConsoleApplication
    {
        private bool _disposed = false;

        private readonly ConcurrentNumList _idList = new();

        private readonly ConcurrentQueue<(Connection, Player.ClientsideSettings?)> 
            _newConnections = new();
        private readonly Queue<Connection> _abortedConnections = new();
        private readonly Queue<Connection> _unexpectedConnections = new();
        private readonly Queue<Connection> _connections = new();

        private readonly Table<int, Player> _playerTable = new();
        private readonly Queue<Player> _spawnedPlayers = new();
        private readonly Queue<Player> _players = new();

        private readonly TeleportManager _teleportManager = new();

        private readonly Table<int, Queue<Control>> _controlsTable = new();
        private readonly Table<int, Queue<Report>> _reportsTable = new();

        private readonly Table<Chunk.Position, Chunk> _chunks = new();

        private Server() { }

        ~Server() => Dispose(false);

        private void RecvData()
        {
            if (_connections.Empty) return;

            bool close;

            int count = _connections.Count;
            Debug.Assert(count > 0);
            for (int i = 0; i < count; ++i)
            {
                close = false;

                Connection conn = _connections.Dequeue();
                int id = conn.PlayerId;

                Queue<Control> controls = _controlsTable.Lookup(id);
                using Queue<Confirm> confirms = new();

                try
                {
                    conn.Recv(controls, confirms);

                    while (!confirms.Empty)
                    {
                        
                        Confirm _c = confirms.Dequeue();
                        if (_c is TeleportConfirm teleportConfirm)
                        {
                            _teleportManager.Confirm(id, teleportConfirm.Payload);
                        }
                        else
                            throw new NotImplementedException();
                    }

                    Debug.Assert(confirms.Empty);

                    _teleportManager.Update(id);
                }
                catch (UnexpectedClientBehaviorExecption)
                {
                    Debug.Assert(close == false);

                    _unexpectedConnections.Enqueue(conn);

                    close = true;
                }
                catch (DisconnectedClientException)
                {
                    Debug.Assert(close == false);

                    _abortedConnections.Enqueue(conn);

                    close = true;
                }

                if (close)
                {
                    confirms.Flush();

                    Player player = _playerTable.Lookup(id);
                    Debug.Assert(player.connected == true);
                    player.connected = false;

                    continue;
                }

                _connections.Enqueue(conn);
            }

        }

        private void HandleNewConnections()
        {
            if (_newConnections.Empty) return;

            bool start, close;

            int count = _newConnections.Count;
            Debug.Assert(count > 0);
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
                catch (UnexpectedClientBehaviorExecption)
                {
                    Debug.Assert(!start);
                    Debug.Assert(!close);

                    close = true;

                    // TODO: Send why disconnected...

                    /*Console.WriteLine("UnexpectedBehaviorExecption!");*/
                }
                catch (DisconnectedClientException)
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

                    int id = conn.PlayerId;

                    Debug.Assert(settings != null);
                    Player player = new(id, new(0, 61, 0), new(0, 0), false, settings);
                    Debug.Assert(player.connected == true);

                    _playerTable.Insert(id, player);
                    _spawnedPlayers.Enqueue(player);

                    _teleportManager.Init(id);

                    _controlsTable.Insert(id, new());
                    _reportsTable.Insert(id, new());
                }

            }

        }

        private void HandlePlayers()
        {
            if (_players.Empty) return;

            int count = _players.Count;
            Debug.Assert(count > 0);
            for (int i = 0; i < count; ++i)
            {
                Player player = _players.Dequeue();

                if (!player.connected)
                {
                    // TODO: Add logic to determine the player is appear or disappear when disconnected.
                    continue;
                }

                int id = player.Id;

                Queue<Control> controls = _controlsTable.Lookup(id);
                
                while (!controls.Empty)
                {
                    Control _c = controls.Dequeue();

                    if (_c is ClientSettingsControl clientSettingsControl)
                    {
                        player.Settings.renderDistance = clientSettingsControl.settings.renderDistance;
                    }
                    else if (_c is PlayerOnGroundControl playerOnGroundControl)
                    {
                        player.onGround = playerOnGroundControl.OnGround;
                    }
                    else if (_c is PlayerPositionControl playerMovementControl)
                    {
                        player.posPrev = player.pos;
                        player.posChunkPrev = player.posChunk;
                        
                        player.pos = playerMovementControl.Pos;
                        player.posChunk = Chunk.Position.Convert(playerMovementControl.Pos);

                    }
                    else if (_c is PlayerLookControl playerLookControl)
                    {
                        player.look = playerLookControl.Look;

                    }
                    else
                        throw new NotImplementedException();

                }

                // TODO: load/unload chunks.

                _players.Enqueue(player);

            }

        }

        private void HandleSpawnedPlayers()
        {
            if (_spawnedPlayers.Empty) return;

            int count = _spawnedPlayers.Count;
            Debug.Assert(count > 0);
            for (int i = 0; i < count; ++i)
            {

                Player player = _spawnedPlayers.Dequeue();

                int id = player.Id;

                Queue<Report> reports = _reportsTable.Lookup(id);

                {
                    SetPlayerAbilitiesReport report = new(true, true, true, true, 1, 0);
                    reports.Enqueue(report);
                }

                {
                    Report? report = null;

                    // load chunks
                    Chunk.Position c = player.posChunk;
                    int d = player.Settings.renderDistance;
                    (Chunk.Position pMax, Chunk.Position pMin) = Chunk.Position.GenerateGridAround(c, d);
                    for (int z = pMin.z; z <= pMax.z; ++z)
                    {
                        for (int x = pMin.x; x <= pMax.x; ++x)
                        {
                            Chunk.Position p = new(x, z);

                            if (_chunks.Contains(p))
                            {
                                Chunk chunk = _chunks.Lookup(p);
                                report = new LoadChunkReport(chunk);
                            }
                            else
                                report = new LoadEmptyChunkReport(p);

                            Debug.Assert(report != null);
                            reports.Enqueue(report);
                        }
                    }

                }

                {
                    // teleport
                    int payload = _teleportManager.Teleport(id);

                    AbsoluteTeleportReport report = new(player.pos, player.look, payload);
                    reports.Enqueue(report);
                }

                _players.Enqueue(player);

            }
        }

        private void HandleEntities()
        {
            throw new NotImplementedException();
        }

        private void SendData()
        {
            if (_connections.Empty) return;

            bool close;
            int count = _connections.Count;
            Debug.Assert(count > 0);
            for (int i = 0; i < count; ++i)
            {
                close = false;

                Connection conn = _connections.Dequeue();

                int id = conn.PlayerId;

                Queue<Report> reports = _reportsTable.Lookup(id);

                try
                {
                    conn.Send(reports);
                }
                catch (DisconnectedClientException)
                {
                    Debug.Assert(close == false);

                    _abortedConnections.Enqueue(conn);

                    close = true;
                }

                if (close)
                {
                    Player player = _playerTable.Lookup(id);
                    Debug.Assert(player.connected == true);
                    player.connected = false;

                    continue;
                }

                _connections.Enqueue(conn);

            }
        }

        private void HandleAbortedConnections()
        {
            if (_abortedConnections.Empty) return;

            int count = _abortedConnections.Count;
            Debug.Assert(count > 0);
            for (int i = 0; i < count; ++i)
            {
                Connection conn = _abortedConnections.Dequeue();

                int id = conn.PlayerId;

                _teleportManager.Close(id);

                Queue<Control> controls = _controlsTable.Extract(id);
                Queue<Report> reports = _reportsTable.Extract(id);

                // TODO: Handle flush and release garbage.                
                controls.Flush();
                reports.Flush();

                controls.Close();
                reports.Close();

            }
        }

        private void StartGameRoutine()
        {
            RecvData();

            // Barrier

            HandleNewConnections();

            // Barrier

            HandlePlayers();

            // Barrier

            HandleSpawnedPlayers();

            // Barrier

            SendData();

            // Barrier

            HandleAbortedConnections();

            // Barrier

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
                    _abortedConnections.Dequeue();
                    _connections.Dispose();

                    _playerTable.Dispose();
                    _spawnedPlayers.Dispose();
                    _players.Dispose();

                    _teleportManager.Dispose();

                    _controlsTable.Dispose();
                    _reportsTable.Dispose();

                    _chunks.Dispose();
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


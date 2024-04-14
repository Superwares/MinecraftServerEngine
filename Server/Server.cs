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

        private readonly Table<int, Queue<TeleportRecord>> _teleportRecordsTable = new();

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
                int id = conn.Id;

                Queue<Control> controls = _controlsTable.Lookup(id);
                using Queue<Confirm> confirms = new();

                Queue<TeleportRecord> records = _teleportRecordsTable.Lookup(id);

                try
                {
                    conn.Recv(controls, confirms);

                    while (!confirms.Empty)
                    {
                        Confirm _c = confirms.Dequeue();
                        if (_c is TeleportConfirm teleportConfirm)
                        {
                            // TODO: Handle this logic in the Protocol library.
                            if (records.Empty)
                                throw new UnknownTeleportConfirmException();  

                            var record = records.Dequeue();
                            if (record.Payload != teleportConfirm.Payload)
                                throw new InvalidTeleportConfirmPayloadException();

                        }
                        else if (_c is ChangeClientSettingsConfirm changeClientSettingsConfirm)
                        {
                            throw new NotImplementedException();
                        }
                    }

                    Debug.Assert(confirms.Empty);

                }
                catch (UnexpectedBehaviorExecption)
                {
                    Debug.Assert(close == false);

                    _unexpectedConnections.Enqueue(conn);

                    close = true;
                }
                catch (DisconnectedException)
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
                catch (UnexpectedBehaviorExecption)
                {
                    Debug.Assert(!start);
                    Debug.Assert(!close);

                    close = true;

                    // TODO: Send why disconnected...

                    /*Console.WriteLine("UnexpectedBehaviorExecption!");*/
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

                    _teleportRecordsTable.Insert(id, new());

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
                if (controls.Empty) continue;

                throw new NotImplementedException();

                // TODO: load/unload chunks.
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
                    Chunk.Position c = player.p_chunk;
                    int d = player.Settings.renderDistance;
                    Chunk.Position[] P = Chunk.Position.GenerateGridAroundCenter(c, d);
                    foreach (var p in P)
                    {
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

                {
                    // teleport
                    TeleportRecord record = new();

                    // enqueue set player position and look packet
                    AbsoluteTeleportReport report = new(
                        player.p.X, player.p.Y, player.p.Z,
                        0, 0,  // TODO: Set yaw and pitch.
                        record.Payload);
                    reports.Enqueue(report);

                    Debug.Assert(_teleportRecordsTable.Contains(id));

                    _teleportRecordsTable.Lookup(id).Enqueue(record);
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

                int id = conn.Id;

                Queue<Report> reports = _reportsTable.Lookup(id);

                try
                {
                    conn.Send(reports);
                }
                catch (DisconnectedException)
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

                int id = conn.Id;

                Queue<TeleportRecord> teleportRecords = _teleportRecordsTable.Extract(id);

                Queue<Control> controls = _controlsTable.Extract(id);
                Queue<Report> reports = _reportsTable.Extract(id);

                // TODO: Handle flush and release garbage.
                teleportRecords.Flush();
                controls.Flush();
                reports.Flush();

                teleportRecords.Close();
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

                    _teleportRecordsTable.Dispose();

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


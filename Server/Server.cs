using Applications;
using Containers;
using Protocol;
using System;
using System.Diagnostics;
using System.Threading;

namespace Application
{
    

    public sealed class Server : ConsoleApplication
    {
        private bool _isDisposed = false;

        private readonly ConcurrentNumList _idList = new();  // Disposable

        private readonly Queue<(Connection, Player)> _connections = new();  // Disposable

        private readonly PlayerList _playerList = new();  // Disposable
        private readonly PlayerSearchTable _playerSearchTable = new();  // Disposable
        private readonly Queue<Player> _players = new();  // Disposable

        private readonly Table<Chunk.Vector, Chunk> _chunks = new();  // Disposable

        private Server() { }

        ~Server() => Dispose(false);

        private void HandleConnectionControls(ulong serverTicks)
        {
            if (_connections.Empty) return;

            bool close;

            string? msg;
            int count = _connections.Count;
            Debug.Assert(count > 0);
            for (int i = 0; i < count; ++i)
            {
                msg = null;
                close = false;

                (Connection conn, Player player) = _connections.Dequeue();

                try
                {
                    conn.Control(player, serverTicks, _playerSearchTable);
                }
                catch (DisconnectedClientException)
                {
                    conn.Close();

                    Debug.Assert(!close);
                    close = true;
                }

                if (close) continue;

                _connections.Enqueue((conn, player));
            }

        }

        private void HandleEntityRoutines()
        {
            if (_players.Empty) return;

            int count = _players.Count;
            Debug.Assert(count > 0);
            for (int i = 0; i < count; ++i)
            {
                Player player = _players.Dequeue();

                if (!player.isConnected)
                {
                    // TODO: Release resources of player object.

                    _playerList.Remove(player.UniqueId);
                    _playerSearchTable.Close(player.Id);
                    _idList.Dealloc(player.Id);
                    /*Console.WriteLine("Disconnected!");*/
                    continue;

                }

                _players.Enqueue(player);
            }

        }

        private void Render()
        {
            if (_connections.Empty) return;

            int count = _connections.Count;
            for (int i = 0; i < count; ++i)
            {
                (Connection conn, Player player) = _connections.Dequeue();

                conn.UpdatePlayerList(_playerList);
                conn.RenterChunks(_chunks, player);
                conn.RenderEntities(player, _playerSearchTable);

                _connections.Enqueue((conn, player));
            }
        }

        private void SendData()
        {
            if (_connections.Empty) return;

            /*Console.Write("Start send data!");*/

            bool close;
            int count = _connections.Count;
            Debug.Assert(count > 0);
            for (int i = 0; i < count; ++i)
            {
                close = false;

                (Connection conn, Player player) = _connections.Dequeue();

                try
                {
                    conn.SendData(player);
                }
                catch (DisconnectedClientException)
                {
                    conn.Close();

                    Debug.Assert(!close);
                    close = true;
                }

                if (close) continue;

                _connections.Enqueue((conn, player));

            }

            /*Console.Write("Finish send data!");*/
        }

        private void Reset()
        {
            if (_players.Empty) return;

            _playerList.Reset();

            foreach (Player player in _players.GetValues())
            {
                player.Reset();
            }
        }

        private void StartGameRoutine(
            ulong serverTicks,
            ConnectionListener connListener)
        {
            Console.Write(".");
            /*Console.Write($"{ticks}");*/

            connListener.Accept(
                _idList,
                _connections, _players,
                _playerSearchTable,
                _playerList,
                _chunks,
                new(0, 60, 0), new(0, 0));

            // Barrier

            /*Physics();*/

            // Barrier

            HandleEntityRoutines();  // reset actions...

            // Barrier

            HandleConnectionControls(serverTicks);

            //

            /*UpdateEntityMovements();*/

            // Barrier

            Render();

            // Barrier

            SendData();

            // Barrier

            Reset();

            // Barrier

        }

        private static ulong GetCurrentTime()
        {
            return (ulong)(DateTime.Now.Ticks / TimeSpan.TicksPerMicrosecond);
        }

        private void StartCoreRoutine(ConnectionListener connListener)
        {
            ulong interval, total, start, end, elapsed, serverTicks;

            serverTicks = 0;
            interval = total = (ulong)TimeSpan.FromMilliseconds(50).TotalMicroseconds;
            start = GetCurrentTime();

            while (Running)
            {
                if (total >= interval)
                {
                    total -= interval;

                    StartGameRoutine(serverTicks++, connListener);
                }

                end = GetCurrentTime();
                elapsed = end - start;
                total += elapsed;
                start = end;

                if (elapsed > interval)
                {
                    Console.WriteLine();
                    Console.WriteLine($"The task is taking longer than expected. Elapsed Time: {elapsed}.");
                }
            }

            // Handle close routine...

        }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {

                if (disposing == true)
                {
                    // Release managed resources.
                    _idList.Dispose();

                    _connections.Dispose();

                    _playerList.Dispose();
                    _playerSearchTable.Dispose();
                    _players.Dispose();

                    _chunks.Dispose();
                }

                // Release unmanaged resources.

                _isDisposed = true;
            }

            base.Dispose(disposing);
        }

        public static void Main()
        {
            Console.WriteLine("Hello, World!");

            ushort port = 25565;

            using Server app = new();

            ConnectionListener connListener = new();

            app.Run(() => app.StartCoreRoutine(connListener));

            GlobalListener listener = new(connListener);
            app.Run(() => 
                listener.StartRoutine(app, port));

            while (app.Running)
            {
                // Handle Barriers
                Thread.Sleep(1000);
            }

        }

    }

}


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
        private bool _isDisposed = false;

        private readonly EntityIdList _entityIdList = new();  // Disposable

        private readonly Queue<(Connection, Player)> _connections = new();  // Disposable
        private readonly Queue<Connection> _disconnections = new();  // Disposable

        private readonly PlayerList _playerList = new();  // Disposable

        private readonly EntityRenderingTable _entityRenderingTable = new();  // Disposable
        private readonly Queue<Entity> _entities = new();  // Disposable

        private readonly Table<Chunk.Vector, Chunk> _chunks = new();  // Disposable

        private Server() { }

        ~Server() => Dispose(false);

        private void HandleEntities()
        {
            if (_entities.Empty) return;

            for (int i = 0; i < _entities.Count; ++i)
            {
                Entity entity = _entities.Dequeue();

                if (entity is Player player)
                {
                    if (!player.IsConnected)
                    {
                        // TODO: Release resources of player object.

                        /*Console.WriteLine("Disconnected!");*/
                        entity.Close();
                        continue;
                    }

                }
                else
                {
                    throw new NotImplementedException();
                }


                _entities.Enqueue(entity);
            }

        }

        private void ControlPlayers(ulong serverTicks)
        {
            if (_connections.Empty) return;

            for (int i = 0; i < _connections.Count; ++i)
            {
                (Connection conn, Player player) = _connections.Dequeue();

                try
                {
                    conn.Control(serverTicks, player);
                }
                catch (DisconnectedClientException)
                {
                    _disconnections.Enqueue(conn);

                    continue;
                }
                
                _connections.Enqueue((conn, player));
            }

        }

        private void MoveEntities()
        {
            if (_entities.Empty) return;

            for (int i = 0; i < _entities.Count; ++i)
            {
                Entity entity = _entities.Dequeue();

                entity.Move();

                _entities.Enqueue(entity);
            }
        }

        private void Render()
        {
            for (int i = 0; i < _connections.Count; ++i)
            {
                (Connection conn, Player player) = _connections.Dequeue();

                conn.UpdatePlayerList(_playerList);
                conn.RenderChunks(_chunks, player);

                _connections.Enqueue((conn, player));
            }

            _entityIdList.Reset();
            _playerList.Reset();

            // reset entities
            foreach (Entity entity in _entities.GetValues())
                entity.Reset();

            for (int i = 0; i < _connections.Count; ++i)
            {
                (Connection conn, Player player) = _connections.Dequeue();

                conn.RenderEntities(player, _entityRenderingTable);

                _connections.Enqueue((conn, player));
            }
        }

        private void SendData()
        {
            if (_connections.Empty) return;

            /*Console.Write("Start send data!");*/

            for (int i = 0; i < _connections.Count; ++i)
            {

                (Connection conn, Player player) = _connections.Dequeue();

                try
                {
                    conn.SendData(player);
                }
                catch (DisconnectedClientException)
                {
                    _disconnections.Enqueue(conn);

                    continue;
                }

                _connections.Enqueue((conn, player));

            }

            /*Console.Write("Finish send data!");*/
        }

        private void HandleDisconnections()
        {

            if (_disconnections.Empty) return;


            for (int i = 0; i < _disconnections.Count; ++i)
            {
                Connection conn = _disconnections.Dequeue();

                conn.Close();
            }

        }
        

        private void StartGameRoutine(
            ulong serverTicks, ConnectionListener connListener)
        {
            Console.Write(".");

            /*Physics();*/

            // Barrier

            HandleEntities();

            // Barrier

            ControlPlayers(serverTicks);

            // Barrier

            connListener.Accept(
                _entityIdList,
                _connections, _entities,
                _entityRenderingTable,
                _playerList,
                new(0, 60, 0), new(0, 0));

            // Barrier

            MoveEntities();

            // Barrier

            Render();

            // Barrier

            SendData();

            // Barrier

            HandleDisconnections();

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
                    _entityIdList.Dispose();

                    _connections.Dispose();

                    _playerList.Dispose();

                    _entityRenderingTable.Dispose();
                    _entities.Dispose();

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


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

        private readonly Queue<(Connection, World, Player)> _connections = new();  // Disposable
        private readonly Queue<Connection> _disconnections = new();  // Disposable

        private readonly PlayerList _playerList = new();  // Disposable

        private readonly Queue<(Entity, World)> _entities = new();  // Disposable
        private readonly Queue<Entity> _despawnedEntities = new();  // Disposable

        private readonly Table<System.Guid, int> _userIdToWorldId = new();

        private readonly Table<int, World> _worldTale;
        private readonly Queue<World> _worlds;


        private Server() { }

        ~Server() => Dispose(false);

        private void HandleEntities()
        {
            if (_entities.Empty) return;

            for (int i = 0; i < _entities.Count; ++i)
            {
                (Entity entity, World world) = _entities.Dequeue();

                if (world.DetermineDespawningEntity(entity))
                {
                    _despawnedEntities.Enqueue(entity);
                    continue;
                }

                entity.Handle(world);

                _entities.Enqueue(entity);
            }

        }

        private void ControlPlayers(ulong serverTicks)
        {
            if (_connections.Empty) return;

            for (int i = 0; i < _connections.Count; ++i)
            {
                (Connection conn, World world, Player player) = _connections.Dequeue();

                try
                {
                    conn.Control(serverTicks, world, player);
                }
                catch (DisconnectedClientException)
                {
                    _disconnections.Enqueue(conn);

                    continue;
                }
                
                _connections.Enqueue((conn, world, player));
            }

        }

        private void MoveEntities()
        {
            if (_entities.Empty) return;

            for (int i = 0; i < _entities.Count; ++i)
            {
                (Entity entity, World world) = _entities.Dequeue();

                entity.Move(world);

                _entities.Enqueue((entity, world));
            }
        }

        private void Render()
        {
            for (int i = 0; i < _connections.Count; ++i)
            {
                (Connection conn, World world, Player player) = _connections.Dequeue();

                conn.UpdatePlayerList(_playerList);

                conn.Render(world, player);

                _connections.Enqueue((conn, world, player));
            }
        }

        private void SendData()
        {
            if (_connections.Empty) return;

            /*Console.Write("Start send data!");*/

            for (int i = 0; i < _connections.Count; ++i)
            {

                (Connection conn, World world, Player player) = _connections.Dequeue();

                try
                {
                    conn.SendData(player);
                }
                catch (DisconnectedClientException)
                {
                    _disconnections.Enqueue(conn);

                    continue;
                }

                _connections.Enqueue((conn, world, player));

            }

            /*Console.Write("Finish send data!");*/
        }

        private void Reset()
        {

            foreach (Entity entity in _entities.GetValues())
                entity.Reset();

            foreach (World world in worlds)
                world.Reset();

            foreach (Entity entity in _despawnedEntities)
                throw new NotImplementedException();

            _playerList.Reset();

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

            connListener.Accept(
                _entityIdList,
                _connections, _entities,
                _userIdToWorldId,
                _worldTablse,
                _playerList,
                new(0, 60, 0), new(0, 0));

            // Barrier

            HandleWorlds();

            // Barrier

            HandleEntities();

            // Barrier

            ControlPlayers(serverTicks);

            // Barrier

            MoveEntities();

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


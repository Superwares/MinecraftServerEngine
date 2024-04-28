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

        private readonly WorldManager _worldManager;
        private readonly Queue<World> _worlds;

        private readonly Queue<(World, Entity)> _entities = new();  // Disposable

        private Server() { }

        ~Server() => Dispose(false);

        /*private void HandleEntities()
        {
            if (_entities.Empty) return;

            for (int i = 0; i < _entities.Count; ++i)
            {
                (Entity entity, World world) = _entities.Dequeue();

                if (world.DetermineAndDespawnEntity(entity))
                {
                    _despawnedEntities.Enqueue(entity);
                    continue;
                }

                entity.Handle(world);

                _entities.Enqueue(entity);
            }

        }

        private void ControlPlayers(long serverTicks)
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

        }*/

        private void DespawnEntities()
        {
            for (int i = 0; i < _entities.Count; ++i)
            {
                (World world, Entity entity) = _entities.Dequeue();

                if (world.DetermineAndDespawnEntity(entity))
                    continue;

                _entities.Enqueue((world, entity));
            }
        }

        private void MoveEntities()
        {
            for (int i = 0; i < _entities.Count; ++i)
            {
                (World world, Entity entity) = _entities.Dequeue();

                entity.Move(world);

                _entities.Enqueue((world, entity));
            }
        }

        private void SpawnEntities()
        {
            for (int i = 0; i < _worlds.Count; ++i)
            {
                World world = _worlds.Dequeue();

                using Queue<Entity> entities = world.SpawnEntities();
                while (!entities.Empty)
                {
                    Entity entity = entities.Dequeue();

                    _entities.Enqueue((world, entity));
                }
                System.Diagnostics.Debug.Assert(entities.Empty);

                _worlds.Enqueue(world);
            }
        }

        private void StartEntityRoutines(long serverTicks)
        {
            for (int i = 0; i < _entities.Count; ++i)
            {
                (World world, Entity entity) = _entities.Dequeue();

                entity.StartRoutine(serverTicks, world);

                _entities.Enqueue((world, entity));
            }
        }

        private void StartWorldRoutines(long serverTicks)
        {
            for (int i = 0; i < _worlds.Count; ++i)
            {
                World world = _worlds.Dequeue();

                world.StartRoutine(serverTicks);

                _worlds.Enqueue(world);
            }
        }

        /*private void Render()
        {
            for (int i = 0; i < _connections.Count; ++i)
            {
                (Connection conn, World world, Player player) = _connections.Dequeue();

                conn.UpdatePlayerList(_playerList);

                conn.Render(world, player);

                _connections.Enqueue((conn, world, player));
            }
        }*/

        /*private void SendData()
        {
            if (_connections.Empty) return;

            *//*Console.Write("Start send data!");*//*

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

            *//*Console.Write("Finish send data!");*//*
        }*/

        private void Reset()
        {
            for (int i = 0; i < _entities.Count; ++i)
            {
                (World world, Entity entity) = _entities.Dequeue();

                entity.Reset();

                _entities.Enqueue((world, entity));
            }

            for (int i = 0; i < _worlds.Count; ++i)
            {
                World world = _worlds.Dequeue();

                world.Reset();

                _worlds.Enqueue(world);
            }

        }

        private void StartGameRoutine(
            long serverTicks, ConnectionListener connListener)
        {
            Console.Write(".");

            connListener.Accept(_worldManager);

            // Barrier

            DespawnEntities();

            // Barrier

            MoveEntities();

            // Barrier

            SpawnEntities();

            // Barrier

            StartEntityRoutines(serverTicks);

            // Barrier

            StartWorldRoutines(serverTicks);

            // Barrier

            Reset();

            // Barrier

        }

        private static long GetCurrentTime()
        {
            return (DateTime.Now.Ticks / TimeSpan.TicksPerMicrosecond);
        }

        private void StartCoreRoutine(ConnectionListener connListener)
        {
            long interval, total, start, end, elapsed;

            long serverTicks = 0;

            interval = total = (long)TimeSpan.FromMilliseconds(50).TotalMicroseconds;
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


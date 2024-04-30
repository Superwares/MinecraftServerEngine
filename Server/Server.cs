using Applications;
using Containers;
using Protocol;
using Server;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;

namespace Application
{
    public sealed class Server : ConsoleApplication
    {
        private bool _isDisposed = false;

        private readonly World _World;

        private readonly Queue<(Connection, Player)> _Connections = new();

        private readonly Queue<Entity> _Entities = new();  // Disposable

        private Server() 
        {
            _World = new SuperWorld();
        }

        ~Server() => Dispose(false);

        private void Control(long serverTicks)
        {
            for (int i = 0; i < _Connections.Count; ++i)
            {
                (Connection conn, Player player) = _Connections.Dequeue();

                try
                {
                    conn.Control(serverTicks, _World, player);
                }
                catch (DisconnectedClientException)
                {
                    conn.Flush();
                    conn.Close();
                    continue;
                }

                _Connections.Enqueue((conn, player));
            }
        }

        private void MoveEntities()
        {
            for (int i = 0; i < _Entities.Count; ++i)
            {
                Entity entity = _Entities.Dequeue();

                _World.MoveEntity(entity);

                _Entities.Enqueue(entity);
            }
        }

        private void Reset()
        {
            for (int i = 0; i < _Entities.Count; ++i)
            {
                Entity entity = _Entities.Dequeue();

                entity.Reset();

                _Entities.Enqueue(entity);
            }
        }

        private void Render(long serverTicks)
        {
            for (int i = 0; i < _Connections.Count; ++i)
            {
                (Connection conn, Player player) = _Connections.Dequeue();

                try
                {
                    conn.Render(_World, player);
                }
                catch (DisconnectedClientException)
                {
                    conn.Flush();
                    conn.Close();
                    continue;
                }

                _Connections.Enqueue((conn, player));
            }
        }

        private void StartEntityRoutines(long serverTicks)
        {
            for (int i = 0; i < _Entities.Count; ++i)
            {
                Entity entity = _Entities.Dequeue();

                if (_World.StartEntitRoutine(serverTicks, entity))
                {
                    continue;
                }

                _Entities.Enqueue(entity);
            }
        }
        
        private void StartGameRoutine(
            long serverTicks, ConnectionListener connListener)
        {
            Console.Write(".");

            Control(serverTicks);

            // Barrier

            _World.DespawnEntities();

            // Barrier

            MoveEntities();

            // Barrier

            _World.SpawnEntities(_Entities);

            // Barrier

            Reset();

            // Barrier

            Render(serverTicks);

            // Barrier

            _World.StartRoutine(serverTicks);

            // Barrier

            StartEntityRoutines(serverTicks);

            // Barrier

            connListener.Accept(_World, _Connections);

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

                System.Diagnostics.Debug.Assert(_Connections.Empty);
                System.Diagnostics.Debug.Assert(_Entities.Empty);

                if (disposing == true)
                {
                    // Release managed resources.
                    _World.Dispose();
                    _Connections.Dispose();
                    _Entities.Dispose();
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

            using ConnectionListener connListener = new();

            app.Run(() => app.StartCoreRoutine(connListener));

            ClientListener listener = new(connListener);
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


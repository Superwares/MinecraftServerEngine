using Applications;
using Containers;
using Protocol;
using Server;

namespace Application
{
    public sealed class Server : ConsoleApplication
    {
        private bool _disposed = false;

        private readonly World _WORLD;

        private readonly Queue<(Connection, Player)> _CONNECTIONS = new();  // Disposable

        private readonly Queue<Entity> _ENTITIES = new();  // Disposable

        private Server() 
        {
            _WORLD = new SuperWorld();
        }

        ~Server() => System.Diagnostics.Debug.Assert(false);

        private void Control(long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            for (int i = 0; i < _CONNECTIONS.Count; ++i)
            {
                (Connection conn, Player player) = _CONNECTIONS.Dequeue();

                try
                {
                    conn.Control(serverTicks, _WORLD, player);
                }
                catch (DisconnectedClientException)
                {
                    conn.Flush(_WORLD);
                    conn.Dispose();

                    continue;
                }

                _CONNECTIONS.Enqueue((conn, player));
            }
        }

        private void HandleEntities()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            for (int i = 0; i < _ENTITIES.Count; ++i)
            {
                Entity entity = _ENTITIES.Dequeue();

                if (_WORLD.HandleEntity(entity))
                {
                    continue;
                }

                _ENTITIES.Enqueue(entity);
            }
        }

        private void ReleaseResources()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _WORLD.ReleaseResources();
        }

        private void Render(long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            for (int i = 0; i < _CONNECTIONS.Count; ++i)
            {
                (Connection conn, Player player) = _CONNECTIONS.Dequeue();

                try
                {
                    conn.Render(_WORLD, player);
                }
                catch (DisconnectedClientException)
                {
                    conn.Flush(_WORLD);
                    conn.Dispose();

                    continue;
                }

                _CONNECTIONS.Enqueue((conn, player));
            }
        }

        private void StartEntityRoutines(long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            for (int i = 0; i < _ENTITIES.Count; ++i)
            {
                Entity entity = _ENTITIES.Dequeue();

                _WORLD.StartEntitRoutine(serverTicks, entity);

                _ENTITIES.Enqueue(entity);
            }
        }
        
        private void StartGameRoutine(long serverTicks, ConnectionListener connListener)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Console.Write(".");

            Control(serverTicks);

            // Barrier

            HandleEntities();

            // Barrier

            _WORLD.SpawnEntities(_ENTITIES);

            // Barrier

            Render(serverTicks);

            // Barrier

            ReleaseResources();

            // Barrier

            _WORLD.StartRoutine(serverTicks);

            // Barrier

            StartEntityRoutines(serverTicks);

            // Barrier

            connListener.Accept(_WORLD, _CONNECTIONS);

            // Barrier

        }

        private static long GetCurrentTime()
        {
            return (System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMicrosecond);
        }

        private void StartCoreRoutine(ConnectionListener connListener)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            long interval, total, start, end, elapsed;

            long serverTicks = 0;

            interval = total = (long)System.TimeSpan.FromMilliseconds(50).TotalMicroseconds;
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
                    System.Console.WriteLine();
                    System.Console.WriteLine($"The task is taking longer than expected. Elapsed Time: {elapsed}.");
                }
            }

            // Handle close routine...

        }

        public override void Dispose()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // Assertiong
            System.Diagnostics.Debug.Assert(_CONNECTIONS.Empty);

            System.Diagnostics.Debug.Assert(_ENTITIES.Empty);

            // Release resources.
            _WORLD.Dispose();

            _CONNECTIONS.Dispose();

            _ENTITIES.Dispose();

            // Finish
            base.Dispose();
            _disposed = true;
        }

        public static void Main()
        {
            System.Console.WriteLine("Hello, World!");

            ushort port = 25565;

            using Server app = new();

            using ConnectionListener connListener = new();

            app.Run(() => app.StartCoreRoutine(connListener));

            ClientListener listener = new(connListener);
            app.Run(() => listener.StartRoutine(app, port));

            while (app.Running)
            {
                // Handle Barriers
                System.Threading.Thread.Sleep(1000);
            }

        }

    }

}


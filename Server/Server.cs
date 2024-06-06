using Applications;
using Protocol;
using Server;
using Threading;

namespace Application
{
    public sealed class Server : ConsoleApplication
    {
        private bool _disposed = false;

        private readonly World _WORLD;

        private long _ticks = 0;

        private Server() 
        {
            _WORLD = new SuperWorld();
        }

        ~Server() => System.Diagnostics.Debug.Assert(false);

        private void CountTicks()
        {
            System.Diagnostics.Debug.Assert(_ticks >= 0);
            System.Diagnostics.Debug.Assert(_ticks <= long.MaxValue);

            ++_ticks;
        }

        /*private void HandleControls()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Connection? conn = null;
            while (_CONNECTIONS.Dequeue(ref conn))
            {
                System.Diagnostics.Debug.Assert(conn != null);

                try
                {
                    conn.Control(_ticks, _WORLD);
                }
                catch (DisconnectedClientException)
                {
                    _DISCONNECTIONS.Enqueue(conn);

                    continue;
                }

                _CONNECTIONS.Enqueue(conn);
            }

            System.Diagnostics.Debug.Assert(_CONNECTIONS.Empty);
        }

        private void HandleDisconnections()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Connection? conn = null;
            while (_DISCONNECTIONS.Dequeue(ref conn))
            {
                System.Diagnostics.Debug.Assert(conn != null);

                conn = _DISCONNECTIONS.Dequeue();

                conn.Flush(_WORLD);
                conn.Dispose();
            }

            System.Diagnostics.Debug.Assert(_DISCONNECTIONS.Empty);
        }

        private void HandleRenders()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Connection? conn = null;
            while (_CONNECTIONS.Dequeue(ref conn))
            {
                System.Diagnostics.Debug.Assert(conn != null);

                conn.Render(_WORLD);

                _CONNECTIONS.Enqueue(conn);
            }

            System.Diagnostics.Debug.Assert(_CONNECTIONS.Empty);
        }*/

        private void StartCoreRoutine(Barrier barrier, ConnectionListener connListener)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_ticks >= 0);

            System.Console.Write(".");

            barrier.Hold();

            _WORLD.StartPlayerRoutines(_ticks);

            barrier.Hold();

            _WORLD.HandlePlayerConnections(_ticks);

            barrier.Hold();

            _WORLD.DespawnEntities();

            barrier.Hold();

            _WORLD.DespawnPlayers();

            barrier.Hold();

            _WORLD.MoveEntities();

            barrier.Hold();

            connListener.Accept(_WORLD);

            barrier.Hold();

            _WORLD.SpawnEntities();

            barrier.Hold();

            _WORLD.HandlePlayerRenders();

            barrier.Hold();

            _WORLD.ReleaseResources();

            barrier.Hold();

            _WORLD.StartRoutine(_ticks);

            barrier.Hold();

            _WORLD.StartEntityRoutines(_ticks);

        }

        public override void Dispose()
        {
            // Assertiong
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.
            _WORLD.Dispose();

            // Finish
            base.Dispose();
            _disposed = true;
        }

        private static long GetCurrentMicroseconds()
        {
            return (System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMicrosecond);
        }

        private static void StartMainRoutine(Barrier barrier, Server server)
        {
            barrier.Wait();

            server._CONNECTIONS.Switch();

            barrier.Start();

            // Handle controls.

            barrier.Wait();

            server._DISCONNECTIONS.Switch();

            barrier.Start();

            // Handle disconnections.

            barrier.Wait();

            server._WORLD._ENTITIES.Switch();

            barrier.Start();

            // Despawn entities.

            barrier.Wait();

            server._WORLD._ENTITIES.Switch();

            barrier.Start();

            // Move entities.

            barrier.Wait();

            barrier.Start();

            // Accept new connections.

            barrier.Wait();

            barrier.Start();

            // Spawn entities.

            barrier.Wait();

            server._CONNECTIONS.Switch();

            barrier.Start();

            // Handle renders.

            barrier.Wait();

            barrier.Start();

            // Start world routine

            barrier.Wait();

            server._WORLD._ENTITIES.Switch();

            barrier.Start();

            // Start entity routines

            barrier.Wait();

            barrier.Start();

            // Release resources.
        }

        public static void Main()
        {
            System.Console.WriteLine("Hello, World!");

            ushort port = 25565;

            using Server server = new();

            int n = 2;

            using Barrier barrier = new(n);
            using ConnectionListener connListener = new();

            for (int i = 0; i < n; ++i)
            {
                server.Run(() =>
                {
                    server.StartCoreRoutine(barrier, connListener);
                });
            }

            ClientListener listener = new(connListener);
            server.Run(() =>
            {
                listener.StartRoutine(server, port);
            });

            long interval, total, start, end, elapsed;

            interval = total = 50L * 1000L;  // 50 milliseconds
            start = GetCurrentMicroseconds();

            while (server.Running)
            {
                if (total >= interval)
                {
                    total -= interval;

                    StartMainRoutine(barrier, server);
                    server.CountTicks();
                }

                end = GetCurrentMicroseconds();
                elapsed = end - start;
                total += elapsed;
                start = end;

                if (elapsed > interval)
                {
                    System.Console.WriteLine();
                    System.Console.WriteLine($"The task is taking longer than expected, ElapsedTime: {elapsed}.");
                }
            }

            // Handle close routine...

        }

    }

}


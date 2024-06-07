
using Applications;
using Threading;

namespace MinecraftServerEngine
{
    public class ServerFramework : ConsoleApplication
    {
        private bool _disposed = false;

        private readonly World _WORLD;

        private long _ticks = 0;

        public ServerFramework(World world)
        {
            _WORLD = world;
        }

        ~ServerFramework() => System.Diagnostics.Debug.Assert(false);

        private void CountTicks()
        {
            System.Diagnostics.Debug.Assert(_ticks >= 0);
            System.Diagnostics.Debug.Assert(_ticks <= long.MaxValue);

            ++_ticks;
        }

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

            _WORLD.DestroyEntities();

            barrier.Hold();

            _WORLD.DestroyPlayers();

            barrier.Hold();

            _WORLD.MoveEntities();

            barrier.Hold();

            _WORLD.CreateEntities();

            barrier.Hold();

            connListener.Accept(_WORLD);

            barrier.Hold();

            _WORLD.HandlePlayerRenders();

            barrier.Hold();

            _WORLD.StartRoutine(_ticks);

            barrier.Hold();

            _WORLD.StartEntityRoutines(_ticks);

        }

        private static long GetCurrentMicroseconds()
        {
            return (System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMicrosecond);
        }

        private void StartMainRoutine(Barrier barrier)
        {
            barrier.Wait();

            _WORLD._PLAYERS.Switch();

            barrier.Start();

            // Start player routines.

            barrier.Wait();

            _WORLD._PLAYERS.Switch();

            barrier.Start();

            // Handle player connections.

            barrier.Wait();

            _WORLD._ENTITIES.Switch();

            barrier.Start();

            // Destroy entities.

            barrier.Wait();

            _WORLD._PLAYERS.Switch();

            barrier.Start();

            // Destroy players.

            barrier.Wait();

            _WORLD._ENTITIES.Switch();

            barrier.Start();

            // Move entities.

            barrier.Wait();

            barrier.Start();

            // Create entities.

            barrier.Wait();

            barrier.Start();

            // Create or connect players.

            barrier.Wait();

            _WORLD._PLAYERS.Switch();

            barrier.Start();

            // Handle player renders.

            barrier.Wait();

            barrier.Start();

            // Start world routine.

            barrier.Wait();

            _WORLD._ENTITIES.Switch();

            barrier.Start();

            // Start entity routines.
        }

        public void Run()
        {
            ushort port = 25565;

            int n = 2;

            using Barrier barrier = new(n);
            using ConnectionListener connListener = new();

            for (int i = 0; i < n; ++i)
            {
                Run(() =>
                {
                    while (Running)
                    {
                        StartCoreRoutine(barrier, connListener);
                    }
                });
            }


            Run(() =>
            {
                using ClientListener clientListener = new(connListener, port);

                while (Running)
                {
                    clientListener.StartRoutine();
                }

                clientListener.Flush();
            });

            long interval, total, start, end, elapsed;

            interval = total = 50L * 1000L;  // 50 milliseconds
            start = GetCurrentMicroseconds();

            while (Running)
            {
                if (total >= interval)
                {
                    total -= interval;

                    StartMainRoutine(barrier);
                    CountTicks();
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

    }
}

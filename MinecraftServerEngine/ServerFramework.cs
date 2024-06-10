
using Threading;
using Containers;

namespace MinecraftServerEngine
{
    public class ServerFramework : System.IDisposable
    {
        private delegate void StartRoutine();


        private bool _disposed = false;



        private bool _running = true;
        public bool Running => _running;

        private readonly Queue<System.Threading.Thread> _THREADS = new();  // Disposable


        private readonly World _WORLD;

        private long _ticks = 0;

        private void Cancel(object? sender, System.ConsoleCancelEventArgs e)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            lock (_SHARED_OBJECT)
            {
                _running = false;

                while (_THREADS.Count > 0)
                {
                    System.Threading.Thread t = _THREADS.Dequeue();
                    t.Join();
                }

                System.Threading.Monitor.Wait(_SHARED_OBJECT);
            }

        }

        public ServerFramework(World world)
        {
            _WORLD = world;

            System.Console.CancelKeyPress += Cancel;
        }

        ~ServerFramework() => System.Diagnostics.Debug.Assert(false);

        private void NewThread(StartRoutine startRoutine)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_running);

            System.Threading.Thread thread = new(new System.Threading.ThreadStart(startRoutine));
            thread.Start();

            _THREADS.Enqueue(thread);
        }

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

            barrier.ReachAndWait();

            _WORLD.StartPlayerRoutines(_ticks);

            barrier.ReachAndWait();

            _WORLD.HandlePlayerConnections(_ticks);

            barrier.ReachAndWait();

            _WORLD.DestroyEntities();

            barrier.ReachAndWait();

            _WORLD.DestroyPlayers();

            barrier.ReachAndWait();

            _WORLD.MoveEntities();

            barrier.ReachAndWait();

            _WORLD.CreateEntities();

            barrier.ReachAndWait();

            connListener.Accept(_WORLD);

            barrier.ReachAndWait();

            _WORLD.HandlePlayerRenders();

            barrier.ReachAndWait();

            _WORLD.StartRoutine(_ticks);

            barrier.ReachAndWait();

            _WORLD.StartEntityRoutines(_ticks);

        }

        private static long GetCurrentMicroseconds()
        {
            return (System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMicrosecond);
        }

        private void StartMainRoutine(Barrier barrier)
        {
            barrier.WaitAllReaching();

            _WORLD._PLAYERS.Switch();

            barrier.Broadcast();

            // Start player routines.

            barrier.WaitAllReaching();

            _WORLD._PLAYERS.Switch();

            barrier.Broadcast();

            // Handle player connections.

            barrier.WaitAllReaching();

            _WORLD._ENTITIES.Switch();

            barrier.Broadcast();

            // Destroy entities.

            barrier.WaitAllReaching();

            _WORLD._PLAYERS.Switch();

            barrier.Broadcast();

            // Destroy players.

            barrier.WaitAllReaching();

            _WORLD._ENTITIES.Switch();

            barrier.Broadcast();

            // Move entities.

            barrier.WaitAllReaching();

            barrier.Broadcast();

            // Create entities.

            barrier.WaitAllReaching();

            barrier.Broadcast();

            // Create or connect players.

            barrier.WaitAllReaching();

            _WORLD._PLAYERS.Switch();

            barrier.Broadcast();

            // Handle player renders.

            barrier.WaitAllReaching();

            barrier.Broadcast();

            // Start world routine.

            barrier.WaitAllReaching();

            _WORLD._ENTITIES.Switch();

            barrier.Broadcast();

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
                NewThread(() =>
                {
                    while (Running)
                    {
                        StartCoreRoutine(barrier, connListener);
                    }
                });
            }


            NewThread(() =>
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

                    System.Diagnostics.Debug.Assert(_ticks >= 0);
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

        public void Dispose()
        {
            // Assertiong
            System.Diagnostics.Debug.Assert(!_disposed);

            lock (_SHARED_OBJECT)
            {
                System.Threading.Monitor.Pulse(_SHARED_OBJECT);
            }

            System.Diagnostics.Debug.Assert(!_running);
            System.Diagnostics.Debug.Assert(_THREADS.Empty);

            // Release resources.
            _THREADS.Dispose();
            _WORLD.Dispose();

            // Finish
            System.GC.SuppressFinalize(this);
            _disposed = true;

            System.Console.Write("Cancel!");
        }

    }
}

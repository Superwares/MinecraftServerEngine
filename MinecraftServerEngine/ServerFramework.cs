
using Common;
using Sync;
using Containers;

namespace MinecraftServerEngine
{
    public class ServerFramework : System.IDisposable
    {

        private bool _disposed = false;


        private bool _running = true;

        private static System.Threading.Thread MainThread = null;
        private readonly Queue<Thread> Threads = new();  // Disposable


        private readonly World _WORLD;

        private long _ticks = 0;


        public ServerFramework(World world)
        {
            _WORLD = world;
        }

        ~ServerFramework() => System.Diagnostics.Debug.Assert(false);

        private void CountTicks()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_ticks >= 0);
            System.Diagnostics.Debug.Assert(_ticks <= long.MaxValue);

            ++_ticks;
        }

        private void StartCoreRoutine(Barrier barrier, ConnectionListener connListener)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_ticks >= 0);

            Console.Print(".");


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

            _WORLD.MovePlayers();


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

        private void StartMainRoutine(Barrier barrier)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            barrier.WaitAllReach();

            _WORLD.Players.Swap();

            barrier.Broadcast();

            // Start player routines.


            barrier.WaitAllReach();

            _WORLD.Players.Swap();

            barrier.Broadcast();

            // Handle player connections.


            barrier.WaitAllReach();

            _WORLD.Entities.Swap();

            barrier.Broadcast();

            // Destroy entities.


            barrier.WaitAllReach();

            _WORLD.Players.Swap();

            barrier.Broadcast();

            // Destroy players.


            barrier.WaitAllReach();

            _WORLD.Entities.Swap();

            barrier.Broadcast();

            // Move entities.


            barrier.WaitAllReach();

            _WORLD.Players.Swap();

            barrier.Broadcast();

            // Move players.


            barrier.WaitAllReach();

            barrier.Broadcast();

            // Create entities.


            barrier.WaitAllReach();

            barrier.Broadcast();

            // Create or connect players.


            barrier.WaitAllReach();

            _WORLD.Players.Swap();

            barrier.Broadcast();

            // Handle player renders.


            barrier.WaitAllReach();

            barrier.Broadcast();

            // Start world routine.


            barrier.WaitAllReach();

            _WORLD.Entities.Swap();

            barrier.Broadcast();

            // Start entity routines.

        }

        private void StartCancelRoutine()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _running = false;
        }

        public void Run()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            MainThread = System.Threading.Thread.CurrentThread;
            Console.HandleTerminatin(() =>
            {
                _running = false;

                System.Diagnostics.Debug.Assert(MainThread != null);
                MainThread.Join();
            });

            ushort port = 25565;

            int n = 1;  // TODO: Determine using number of processor.

            using Barrier barrier = new(n);
            using ConnectionListener connListener = new();

            for (int i = 0; i < n; ++i)
            {
                var coreThread = Thread.New(() =>
                {
                    while (_running)
                    {
                        StartCoreRoutine(barrier, connListener);
                    }
                });

                Threads.Enqueue(coreThread);
            }


            var subThread1 = Thread.New(() =>
            {
                using ClientListener clientListener = new(connListener, port);

                while (_running)
                {
                    clientListener.StartRoutine();
                }

                clientListener.Flush();
            });

            Threads.Enqueue(subThread1);

            {
                Time interval, accumulated, start, end, elapsed;

                interval = accumulated = Time.FromMilliseconds(50);
                start = Time.Now();

                while (_running)
                {
                    if (accumulated >= interval)
                    {
                        accumulated -= interval;

                        System.Diagnostics.Debug.Assert(_ticks >= 0);
                        StartMainRoutine(barrier);
                        CountTicks();
                    }

                    end = Time.Now();
                    elapsed = end - start;
                    start = end;
                    accumulated += elapsed;

                    if (elapsed > interval)
                    {
                        Console.NewLine();
                        Console.Printl($"The task is taking longer than expected, ElapsedTime: {elapsed}.");
                    }
                }
            }

            {
                // Handle close routine.
                while (!Threads.Empty)
                {
                    Thread t = Threads.Dequeue();
                    t.Join();
                }

                System.Diagnostics.Debug.Assert(!_running);
            }

            Console.Print("Finish!!");

        }

        public void Dispose()
        {
            // Assertiong
            System.Diagnostics.Debug.Assert(!_disposed);
            System.Diagnostics.Debug.Assert(!_running);
            System.Diagnostics.Debug.Assert(Threads.Empty);

            // Release resources.
            Threads.Dispose();
            _WORLD.Dispose();

            // Finish
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }
}

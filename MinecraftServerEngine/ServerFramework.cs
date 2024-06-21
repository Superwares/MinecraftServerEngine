
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

        private void StartCoreRoutine(
            Locker locker, Cond cond, 
            Barrier barrier, ConnectionListener connListener)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_ticks >= 0);

            Console.Print(".");

            _WORLD.StartPlayerRoutines(locker, cond, barrier, _ticks);

            _WORLD.HandlePlayerConnections(locker, cond, barrier, _ticks);

            _WORLD.DestroyEntities(locker, cond, barrier);

            _WORLD.DestroyPlayers(locker, cond, barrier);

            _WORLD.MoveEntities(locker, cond, barrier);

            _WORLD.MovePlayers(locker, cond, barrier);

            _WORLD.CreateEntities(locker, cond, barrier);
                                   
            connListener.Accept(locker, cond, barrier, _WORLD);
                                   
            _WORLD.HandlePlayerRenders(locker, cond, barrier);

            _WORLD.StartRoutine(locker, cond, barrier, _ticks);

            _WORLD.StartEntityRoutines(locker, cond, barrier, _ticks);
        }        
        
        private void StartMainRoutine(Locker locker, Cond cond, Barrier barrier)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _WORLD.Players.Swap();

            locker.Hold();
            cond.Broadcast();
            locker.Release();

            // Start player routines.

            barrier.SignalAndWait();
            barrier.Reset();

            _WORLD.Players.Swap();

            locker.Hold();
            cond.Broadcast();
            locker.Release();

            // Handle player connections.

            barrier.SignalAndWait();
            barrier.Reset();

            _WORLD.Entities.Swap();

            locker.Hold();
            cond.Broadcast();
            locker.Release();

            // Destroy entities.

            barrier.SignalAndWait();
            barrier.Reset();

            _WORLD.Players.Swap();

            locker.Hold();
            cond.Broadcast();
            locker.Release();

            // Destroy players.

            barrier.SignalAndWait();
            barrier.Reset();

            _WORLD.Entities.Swap();

            locker.Hold();
            cond.Broadcast();
            locker.Release();

            // Move entities.

            barrier.SignalAndWait();
            barrier.Reset();

            _WORLD.Players.Swap();

            locker.Hold();
            cond.Broadcast();
            locker.Release();

            // Move players.

            barrier.SignalAndWait();
            barrier.Reset();

            locker.Hold();
            cond.Broadcast();
            locker.Release();

            // Create entities.

            barrier.SignalAndWait();
            barrier.Reset();

            locker.Hold();
            cond.Broadcast();
            locker.Release();

            // Create or connect players.

            barrier.SignalAndWait();
            barrier.Reset();

            _WORLD.Players.Swap();

            locker.Hold();
            cond.Broadcast();
            locker.Release();

            // Handle player renders.

            barrier.SignalAndWait();
            barrier.Reset();

            locker.Hold();
            cond.Broadcast();
            locker.Release();

            // Start world routine.

            barrier.SignalAndWait();
            barrier.Reset();

            _WORLD.Entities.Swap();

            locker.Hold();
            cond.Broadcast();
            locker.Release();

            // Start entity routines.

            barrier.SignalAndWait();
            barrier.Reset();
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

            using Locker locker = new();
            using Cond cond = new(locker);
            using Barrier barrier = new(n);
            using ConnectionListener connListener = new();

            for (int i = 0; i < n; ++i)
            {
                var coreThread = Thread.New(() =>
                {
                    while (_running)
                    {
                        StartCoreRoutine(locker, cond, barrier, connListener);
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
                        StartMainRoutine(locker, cond, barrier);
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

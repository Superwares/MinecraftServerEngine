
using Common;
using Sync;
using Containers;

namespace MinecraftServerEngine
{
    public class ServerFramework : System.IDisposable
    {

        private bool _disposed = false;


        private bool _running = true;

        private static Thread MainThread = null;
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
            Barrier barrier, ConnectionListener connListener)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_ticks >= 0);

            Console.Print(".");

            /*Console.Printl("StartPlayerRoutines!");*/
            _WORLD.StartPlayerRoutines(barrier, _ticks);

            /*Console.Printl("HandlePlayerConnections!");*/
            _WORLD.HandlePlayerConnections(barrier, _ticks);

            /*Console.Printl("DestroyEntities!");*/
            _WORLD.DestroyEntities(barrier);

            /*Console.Printl("DestroyPlayers!");*/
            _WORLD.DestroyPlayers(barrier);

            /*Console.Printl("MoveEntities!");*/
            _WORLD.MoveEntities(barrier);

            /*Console.Printl("MovePlayers");*/
            _WORLD.MovePlayers(barrier);

            /*Console.Printl("CreateEntities!");*/
            _WORLD.CreateEntities(barrier);

            /*Console.Printl("Create or Connect Players!");*/
            connListener.Accept(barrier, _WORLD);
                   
            /*Console.Printl("HandlePlayerRenders!");*/
            _WORLD.HandlePlayerRenders(barrier);

            /*Console.Printl("StartRoutine!");*/
            _WORLD.StartRoutine(barrier, _ticks);

            /*Console.Printl("StartEntityRoutines!");*/
            _WORLD.StartEntityRoutines(barrier, _ticks);
        }        
        
        private void StartMainRoutine(Barrier barrier)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _WORLD.Players.Swap();

            barrier.SignalAndWait();

            // Start player routines.

            barrier.SignalAndWait();

            _WORLD.Players.Swap();

            barrier.SignalAndWait();

            // Handle player connections.

            barrier.SignalAndWait();

            _WORLD.Entities.Swap();

            barrier.SignalAndWait();

            // Destroy entities.

            barrier.SignalAndWait();

            _WORLD.Players.Swap();

            barrier.SignalAndWait();

            // Destroy players.

            barrier.SignalAndWait();

            _WORLD.Entities.Swap();

            barrier.SignalAndWait();

            // Move entities.

            barrier.SignalAndWait();

            _WORLD.Players.Swap();

            barrier.SignalAndWait();

            // Move players.

            barrier.SignalAndWait();

            barrier.SignalAndWait();

            // Create entities.

            barrier.SignalAndWait();

            barrier.SignalAndWait();

            // Create or connect players.

            barrier.SignalAndWait();

            _WORLD.Players.Swap();

            barrier.SignalAndWait();

            // Handle player renders.

            barrier.SignalAndWait();

            barrier.SignalAndWait();

            // Start world routine.

            barrier.SignalAndWait();

            _WORLD.Entities.Swap();

            barrier.SignalAndWait();

            // Start entity routines.

            barrier.SignalAndWait();

        }

        public void Run()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            using ReadLocker rLocker = new();

            MainThread = Thread.GetCurrent();
            Console.HandleTerminatin(() =>
            {
                {
                    rLocker.Hold();
                    /*Console.Printl("Stop Running!");*/
                    _running = false;
                    rLocker.Release();
                }

                System.Diagnostics.Debug.Assert(MainThread != null);
                MainThread.Join();
            });

            ushort port = 25565;

            int n = 2;  // TODO: Determine using number of processor.

            using Barrier barrier = new(n);
            using ConnectionListener connListener = new();

            System.Diagnostics.Debug.Assert(n > 1);
            for (int i = 0; i < n - 1; ++i)
            {
                var coreThread = Thread.New(() =>
                {
                    do
                    {
                        barrier.SignalAndWait();

                        rLocker.Read();

                        try
                        {
                            if (!_running)
                            {
                                break;
                            }

                            StartCoreRoutine(barrier, connListener);
                        }
                        finally
                        {
                            rLocker.Release();
                        }
                    } while(true);
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

                do
                {

                    if (accumulated >= interval)
                    {
                        accumulated -= interval;

                        System.Diagnostics.Debug.Assert(_ticks >= 0);

                        barrier.SignalAndWait();

                        rLocker.Read();

                        try
                        {
                            if (!_running)
                            {
                                break;
                            }

                            StartMainRoutine(barrier);
                            CountTicks();
                        }
                        finally
                        {
                            rLocker.Release();
                        }
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

                } while (true);
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

            Console.Print("Finish!");

        }

        public void Dispose()
        {
            // Assertiong
            System.Diagnostics.Debug.Assert(!_disposed);
            System.Diagnostics.Debug.Assert(!_running);
            System.Diagnostics.Debug.Assert(Threads.Empty);

            // Release resources.
            Threads.Dispose();

            // Finish
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }
}

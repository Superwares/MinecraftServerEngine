
using Common;
using Sync;
using Containers;

namespace MinecraftServerEngine
{
    public class ServerFramework : System.IDisposable
    {

        private bool _disposed = false;


        private bool _running = true;

        private static Thread CurrentRunningThread = null;
        private readonly Queue<Thread> Threads = new();  // Disposable


        private readonly World _WORLD;

        private long _ticks = 0;


        public ServerFramework(World world)
        {
            System.Diagnostics.Debug.Assert(world != null);
            _WORLD = world;
        }

        ~ServerFramework() => System.Diagnostics.Debug.Assert(false);

        private void NewThread(VoidMethod startRoutine)
        {
            System.Diagnostics.Debug.Assert(startRoutine != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            Threads.Enqueue(Thread.New(startRoutine));
        }

        private void CountTicks()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_ticks >= 0);
            System.Diagnostics.Debug.Assert(_ticks <= long.MaxValue);

            ++_ticks;
        }   

        private void Parallel(int i, int n, VoidMethod startRoutine)
        {
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(n > 0);
            System.Diagnostics.Debug.Assert(startRoutine != null);

            System.Threading.Tasks.ParallelLoopResult result;

            Time start = Time.Now(), end;

            result = System.Threading.Tasks.Parallel.For(0, n, (_) => startRoutine());
            System.Diagnostics.Debug.Assert(result.IsCompleted);

            end = Time.Now();
            Console.Print($"{i}: {end - start}, ");
        }

        public void Run(ushort port)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            using ReadLocker rLocker = new();

            CurrentRunningThread = Thread.GetCurrent();
            Console.HandleTerminatin(() =>
            {
                {
                    rLocker.Hold();

                    /*Console.Print("Cancel Running!");*/
                    _running = false;

                    rLocker.Release();
                }

                System.Diagnostics.Debug.Assert(CurrentRunningThread != null);
                CurrentRunningThread.Join();
            });

            using ConnectionListener connListener = new();

            NewThread(() =>
            {
                using ClientListener clientListener = new(connListener, port);

                while (_running)
                {
                    clientListener.StartRoutine();
                }

                clientListener.Flush();
            });

            {
                int n = System.Environment.ProcessorCount;

                using Barrier barrier = new(n);

                int i;

                Time interval, accumulated, start, end, elapsed;

                interval = accumulated = Time.FromMilliseconds(50);
                start = Time.Now();

                while (_running)
                {
                    i = 0;
                    if (accumulated >= interval)
                    {
                        accumulated -= interval;

                        System.Diagnostics.Debug.Assert(_ticks >= 0);

                        Console.Print(".");

                        Parallel(i++, n, () =>
                        {
                            _WORLD.StartPlayerControls(barrier, _ticks);
                        });

                        Parallel(i++, n, () =>
                        {
                            _WORLD.DestroyEntities(barrier);
                        });

                        Parallel(i++, n, () =>
                        {
                            _WORLD.MoveEntities(barrier);
                        });

                        Parallel(i++, n, () =>
                        {
                            _WORLD.CreateEntities(barrier);
                        });

                        Parallel(i++, n, () =>
                        {
                            connListener.Accept(barrier, _WORLD);
                        });

                        Parallel(i++, n, () =>
                        {
                            _WORLD.HandlePlayerRenders(barrier);
                        });

                        Parallel(i++, n, () =>
                        {
                            _WORLD.StartRoutine(barrier, _ticks);
                        });

                        Parallel(i++, n, () =>
                        {
                            _WORLD.StartEntityRoutines(barrier, _ticks);
                        });

                        CountTicks();

                        Console.NewLine();
                    }

                    end = Time.Now();
                    elapsed = end - start;
                    start = end;
                    accumulated += elapsed;

                    if (elapsed > interval)
                    {
                        Console.NewLine();
                        Console.Print($"The task is taking longer, Elapsed: {elapsed}!");
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

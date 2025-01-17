
using Common;
using Sync;
using Containers;

namespace MinecraftServerEngine
{
    public sealed class ServerFramework : System.IDisposable
    {
        internal sealed class PerformanceMonitor
        {
            private readonly int N;
            private int _i = 0;

            private int _count = 0;
            private Time[] _totalTimes;

            public PerformanceMonitor(int n)
            {
                System.Diagnostics.Debug.Assert(n > 0);
                N = n;
                _totalTimes = new Time[N + 1];

                for (int i = 0; i <= N; ++i)
                {
                    _totalTimes[i] = Time.Zero;
                }
            }

            public void Record(Time elapsed)
            {
                System.Diagnostics.Debug.Assert(_i >= 0);
                System.Diagnostics.Debug.Assert(_i <= N);

                System.Diagnostics.Debug.Assert(_count >= 0);
                if (_i == 0)
                {
                    ++_count;
                }

                /*Console.Printl($"i: {_i}");*/
                _totalTimes[_i++] += elapsed;

                if (_i > N)
                {
                    _i = 0;
                }
            }

            public void Print()
            {
                System.Diagnostics.Debug.Assert(_i == 0);
                System.Diagnostics.Debug.Assert(_count > 0);

                string msg = "";
                msg += "[Performance] ";

                Time average;
                for (int i = 0; i < N; ++i)
                {
                    average = _totalTimes[i] / _count;
                    msg += $"{i}:{average}, ";

                    _totalTimes[i] = Time.Zero;
                }

                {
                    average = _totalTimes[N] / _count;
                    msg += $"T:{average}";

                    _totalTimes[N] = Time.Zero;
                }

                MyConsole.Info(msg);

                _count = 0;
            }
        }

        private sealed class Task
        {
            internal readonly bool Parallel;

            private readonly VoidMethod InitRoutine = null;
            private readonly VoidMethod StartRoutine = null;

            public Task(VoidMethod initRoutine, VoidMethod startRoutine)
            {
                System.Diagnostics.Debug.Assert(initRoutine != null);
                System.Diagnostics.Debug.Assert(startRoutine != null);

                InitRoutine = initRoutine; StartRoutine = startRoutine;
            }

            public Task(VoidMethod startRoutine)
            {
                System.Diagnostics.Debug.Assert(startRoutine != null);

                StartRoutine = startRoutine;

                Parallel = true;
            }

            public Task(VoidMethod startRoutine, bool parallel)
            {
                System.Diagnostics.Debug.Assert(startRoutine != null);

                StartRoutine = startRoutine;

                Parallel = parallel;
            }

            public void Init()
            {
                if (InitRoutine == null)
                {
                    return;
                }

                InitRoutine();
            }

            public void Start()
            {
                System.Diagnostics.Debug.Assert(StartRoutine != null);

                StartRoutine();
            }
        }

        private sealed class TaskManager
        {
            private readonly int ProcessorCount = System.Environment.ProcessorCount;
            /*private readonly int ProcessorCount = 1;*/

            public readonly int TotalTaskCount;
            private readonly Task[] Tasks;

            public TaskManager(params Task[] tasks)
            {
                System.Diagnostics.Debug.Assert(tasks != null);

                TotalTaskCount = tasks.Length;
                Tasks = tasks;

                /*Console.Printl($"{System.Environment.ProcessorCount}");*/
            }

            public void Start(PerformanceMonitor sys)
            {
                System.Diagnostics.Debug.Assert(sys != null);

                System.Diagnostics.Debug.Assert(Tasks != null);

                System.Threading.Tasks.ParallelLoopResult result;
                for (int i = 0; i < TotalTaskCount; ++i)
                {
                    Task task = Tasks[i];

                    Time start = Time.Now(), end;

                    task.Init();

                    if (task.Parallel)
                    {
                        result = System.Threading.Tasks.Parallel.For(
                            0, ProcessorCount, (_) => task.Start());
                        System.Diagnostics.Debug.Assert(result.IsCompleted);
                    }
                    else
                    {
                        task.Start();
                    }

                    end = Time.Now();
                    sys.Record(end - start);

                }
            }
        }

        private bool _disposed = false;


        private bool _running = true;

        private static Thread CurrentRunningThread = null;
        private readonly Queue<Thread> Threads = new();  // Disposable


        private readonly World World;


        public ServerFramework(World world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            World = world;
        }

        ~ServerFramework()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        private void NewThread(VoidMethod startRoutine)
        {
            System.Diagnostics.Debug.Assert(startRoutine != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(Threads != null);
            Threads.Enqueue(Thread.New(startRoutine));
        }

        public void Run(ushort port)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            CurrentRunningThread = Thread.GetCurrent();
            MyConsole.HandleTerminatin(() =>
            {
                MyConsole.Info("Server shutdown initiated.");
                _running = false;

                System.Diagnostics.Debug.Assert(CurrentRunningThread != null);
                CurrentRunningThread.Join();
            });

            using ConnectionListener connListener = new();

            NewThread(() =>
            {
                using ClientListener clntListener = new(connListener, port);

                while (_running)
                {
                    clntListener.StartRoutine();
                }

                clntListener.Flush();
            });

            TaskManager manager = new(
                new Task(  // 0
                    () => World.SwapObjectQueue(),
                    () => World.ControlPlayers()),
                new Task(  // 1
                    () => World.SwapObjectQueue(),
                    () => World.HandleDeathEvents()),
                new Task(  // 2
                    () => World.SwapObjectQueue(),
                    () => World.HandleDisconnections()),
                new Task(  // 3
                    () => World.SwapObjectQueue(),
                    () => World.DestroyObjects()),
                new Task(  // 4
                    () => World.SwapObjectQueue(),
                    () => World.MoveObjects()),
                new Task(  // 5
                    () => World.CreateObjects()),
                new Task(  // 6
                    () => connListener.Accept(World)),
                new Task(  // 7
                    () => World.SwapObjectQueue(),
                    () => World.LoadAndSendData()),
                new Task(  // 8
                    () => World.StartRoutine(), false),
                new Task(  // 9
                    () => World.SwapObjectQueue(),
                    () => World.StartObjectRoutines()));

            PerformanceMonitor sys = new(manager.TotalTaskCount);

            bool f;

            ulong ticks = 0;

            Time total, interval, accumulated, start, end, elapsed;

            total = Time.Zero;
            interval = accumulated = Time.FromMilliseconds(50);
            start = Time.Now();

            while (_running)
            {
                f = (accumulated >= interval);
                if (f == true)
                {
                    /*System.Diagnostics.Debug.Assert(_ticks >= 0);*/

                    manager.Start(sys);
                }

                end = Time.Now();
                elapsed = end - start;

                start = end;
                accumulated += elapsed;
                total += elapsed;

                if (elapsed > interval)
                {
                    MyConsole.Warn($"The task is taking longer, Elapsed: {elapsed}!");
                }

                if (f)
                {
                    accumulated -= interval;
                    ++ticks;

                    sys.Record(elapsed);

                    /*Console.Printl($"total % Time.FromSeconds(5): {total % Time.FromSeconds(5)}");*/
                    if (ticks % (20 * 5) == 0)
                    {
                        sys.Print();
                    }
                }

            }

            {
                Thread t;
                while (!Threads.Empty)
                {
                    t = Threads.Dequeue();

                    System.Diagnostics.Debug.Assert(t != null);
                    t.Join();
                }

                System.Diagnostics.Debug.Assert(!_running);
            }

            MyConsole.Info("Finish!");
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed == false)
            {
                System.Diagnostics.Debug.Assert(_running == false);
                System.Diagnostics.Debug.Assert(Threads.Empty == true);

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing == true)
                {
                    Threads.Dispose();
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                //CloseHandle(handle);
                //handle = IntPtr.Zero;

                // Note disposing has been done.
                _disposed = true;
            }
        }


    }
}

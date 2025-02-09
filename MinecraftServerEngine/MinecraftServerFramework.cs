
using Common;
using Containers;
using Sync;

using MinecraftServerEngine.Protocols;

namespace MinecraftServerEngine
{

    public sealed class MinecraftServerFramework : System.IDisposable
    {
        internal sealed class PerformanceMonitor
        {
            private readonly int N;
            private int _i = 0;

            public int CurrentTaskIndex => _i;

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

                //MyConsole.Debug($"Record: i: {_i}");
                _totalTimes[_i++] += elapsed;

                if (_i > N)
                {
                    _i = 0;
                }
            }

            public void Pass()
            {
                System.Diagnostics.Debug.Assert(_i >= 0);
                System.Diagnostics.Debug.Assert(_i <= N);

                System.Diagnostics.Debug.Assert(_count >= 0);
                if (_i == 0)
                {
                    ++_count;
                }

                //MyConsole.Debug($"Pass: i: {_i}");
                ++_i;

                if (_i > N)
                {
                    _i = 0;
                }
            }

            public void Print(Time interval)
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

                if (average > interval)
                {
                    MyConsole.Warn($"The task is taking longer!");
                }

                _count = 0;
            }
        }

        private sealed class Task
        {
            internal readonly bool Parallel;

            public readonly bool EnsureOneTick;

            private readonly VoidMethod _Start = null;

            public Task(
                bool ensureOneTick, bool parallel,
                VoidMethod start
                )
            {
                System.Diagnostics.Debug.Assert(start != null);

                EnsureOneTick = ensureOneTick;
                Parallel = parallel;

                _Start = start;
            }


            public void Start()
            {
                System.Diagnostics.Debug.Assert(_Start != null);
                _Start();
            }
        }

        private sealed class TaskManager
        {
            private readonly int ProcessorCount = System.Environment.ProcessorCount;
            /*private readonly int ProcessorCount = 1;*/

            public readonly int TotalTaskCount;
            private readonly Task[] Tasks;

            private readonly VoidMethod _Initialize = null;
            private readonly VoidMethod _Terminate = null;

            public TaskManager(
                VoidMethod initialize,
                VoidMethod terminate,
                params Task[] tasks
                )
            {
                System.Diagnostics.Debug.Assert(initialize != null);
                System.Diagnostics.Debug.Assert(terminate != null);

                System.Diagnostics.Debug.Assert(tasks != null);

                _Initialize = initialize;
                _Terminate = terminate;

                TotalTaskCount = tasks.Length;
                Tasks = tasks;

                /*Console.Printl($"{System.Environment.ProcessorCount}");*/
            }

            public void Start(bool shouldExecuteOneTick, PerformanceMonitor sys)
            {
                System.Diagnostics.Debug.Assert(sys != null);

                System.Diagnostics.Debug.Assert(Tasks != null);

                System.Threading.Tasks.ParallelLoopResult result;
                for (int i = 0; i < TotalTaskCount; ++i)
                {
                    Task task = Tasks[i];

                    System.Diagnostics.Debug.Assert(task != null);
                    if (shouldExecuteOneTick == false && task.EnsureOneTick == true)
                    {
                        System.Diagnostics.Debug.Assert(sys != null);
                        sys.Pass();

                        continue;
                    }

                    Time start = Time.Now();

                    System.Diagnostics.Debug.Assert(_Initialize != null);
                    _Initialize();

                    try
                    {
                        System.Diagnostics.Debug.Assert(task != null);
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

                    }
                    finally
                    {
                        System.Diagnostics.Debug.Assert(sys != null);
                        sys.Record(Time.Now() - start);

                        System.Diagnostics.Debug.Assert(_Terminate != null);
                        _Terminate();
                    }
                }

                //MyConsole.Debug($"Current task index: {sys.CurrentTaskIndex}");
            }
        }

        private bool _disposed = false;


        private bool _running = true;

        private static Thread CurrentRunningThread = null;
        private readonly Queue<Thread> Threads = new();  // Disposable


        private readonly World World;


        public MinecraftServerFramework(World world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            World = world;
        }

        ~MinecraftServerFramework()
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

                MyConsole.Info($"Server is now listening on port {port}");

                while (_running)
                {
                    clntListener.StartRoutine();
                }

                clntListener.Flush();
            });



            TaskManager manager = new(
                World.StartTask,
                World.EndTask,
                [
                    new Task(  // 0
                        false,  // EnsureOneTick
                        true,  // Parallel
                        () => World.HandlePlayerDisconnections()),
                    new Task(  // 1
                        false,  // EnsureOneTick
                        true,  // Parallel
                        () => World.HandleLivingEntityDamageEvents()),
                    new Task(  // 2
                        false,  // EnsureOneTick
                        true,  // Parallel
                        () => World.HandleDespawning()),
                    new Task(  // 3
                        false,  // EnsureOneTick
                        true,  // Parallel
                        () => World.DestroyObjects()),

                    new Task(  // 4
                        false,  // EnsureOneTick
                        true,  // Parallel
                        () => World.PreMoveObjects()),
                    new Task(  // 4
                        true,  // EnsureOneTick
                        true,  // Parallel
                        () => World.MoveObjects()),

                    new Task(  // 5
                        true,  // EnsureOneTick
                        true,  // Parallel
                        () => connListener.Accept(World)),
                    new Task(  // 6
                        false,  // EnsureOneTick
                        true,  // Parallel
                        () => World.CreateObjects()),

                    new Task(  // 7
                        true,  // EnsureOneTick
                        true,  // Parallel
                        () => World.LoadWorld()),
                    new Task(  // 8
                        false,  // EnsureOneTick
                        true,  // Parallel
                        () => World.SendData()),

                    new Task(  // 9
                        false,  // EnsureOneTick
                        false,  // Parallel
                        () => World._StartRoutine()),
                    new Task(  // 10
                        false,  // EnsureOneTick
                        true,  // Parallel
                        () => World.StartObjectRoutines()),
                    new Task(  // 11
                        true,  // EnsureOneTick
                        true,  // Parallel
                        () => World.ControlPlayers()),
                ]);

            PerformanceMonitor sys = new(manager.TotalTaskCount);

            bool shouldExecuteOneTick;

            ulong ticks = 0;

            Time total, interval, accumulated, start, end, elapsed;

            total = Time.Zero;

            interval = accumulated = MinecraftTimes.TimePerTick;
            start = Time.Now();

            while (_running)
            {
                shouldExecuteOneTick = accumulated >= interval;

                manager.Start(shouldExecuteOneTick, sys);

                end = Time.Now();
                elapsed = end - start;

                start = end;
                accumulated += elapsed;
                total += elapsed;

                //if (elapsed > interval)
                //{
                //    MyConsole.Warn($"The task is taking longer, Elapsed: {elapsed}!");
                //}

                if (shouldExecuteOneTick == true)
                {
                    accumulated -= interval;
                    ++ticks;

                    sys.Record(elapsed);

                    /*Console.Printl($"total % Time.FromSeconds(5): {total % Time.FromSeconds(5)}");*/
                    if (ticks % (MinecraftTimes.TicksPerSeconds * 5) == 0)
                    {
                        sys.Print(interval);
                    }
                }
                else
                {
                    sys.Pass();
                }

            }

            {
                Thread t;
                while (Threads.Empty == false)
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
                System.Diagnostics.Debug.Assert(Threads != null);
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

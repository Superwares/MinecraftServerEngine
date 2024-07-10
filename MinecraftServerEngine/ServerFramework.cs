
using Common;
using Sync;
using Containers;

namespace MinecraftServerEngine
{
    public class ServerFramework : System.IDisposable
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
                    _totalTimes[i] = Time.Zero();
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

                    _totalTimes[i] = Time.Zero();
                }

                {
                    average = _totalTimes[N] / _count;
                    msg += $"T:{average}";

                    _totalTimes[N] = Time.Zero();
                }

                Console.Printl(msg);

                _count = 0;
            }
        }

        private sealed class Task
        {
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

            public readonly int TotalTaskCount;
            private readonly Task[] Tasks;

            public TaskManager(params Task[] tasks)
            {
                System.Diagnostics.Debug.Assert(tasks != null);

                TotalTaskCount = tasks.Length;
                Tasks = tasks;
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

                    result = System.Threading.Tasks.Parallel.For(
                        0, ProcessorCount, (_) => task.Start());

                    end = Time.Now();
                    sys.Record(end - start);

                    System.Diagnostics.Debug.Assert(result.IsCompleted);
                }
            }
        }

        private bool _disposed = false;


        private bool _running = true;

        private static Thread CurrentRunningThread = null;
        private readonly Queue<Thread> Threads = new();  // Disposable


        private readonly World World;

        private long _ticks = 0;


        public ServerFramework(World world)
        {
            System.Diagnostics.Debug.Assert(world != null);
            World = world;
        }

        ~ServerFramework() => System.Diagnostics.Debug.Assert(false);

        private void NewThread(VoidMethod startRoutine)
        {
            System.Diagnostics.Debug.Assert(startRoutine != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Threads != null);
            Threads.Enqueue(Thread.New(startRoutine));
        }

        private void CountTicks()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_ticks >= 0);
            System.Diagnostics.Debug.Assert(_ticks < long.MaxValue);

            ++_ticks;
        }

        public void Run(ushort port)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            CurrentRunningThread = Thread.GetCurrent();
            Console.HandleTerminatin(() =>
            {
                /*Console.Print("Cancel Running!");*/
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
                    () => World.SwapQueues(),
                    () => World.ControlPlayers(_ticks)),
                new Task(  // 1
                    () => World.SwapQueues(),
                    () => World.DestroyEntities()),
                new Task(  // 2
                    () => World.SwapQueues(),
                    () => World.IntegrateEntity()),
                new Task(  // 3
                    () => World.SwapQueues(),
                    () => World.MoveEntities()),
                new Task(  // 4
                    () => World.CreateEntities()),
                new Task(  // 5
                    () => connListener.Accept(World)),
                new Task(  // 6
                    () => World.SwapQueues(),
                    () => World.RenderPlayers()),
                new Task(  // 7
                    () => World.StartRoutine(_ticks)),
                new Task(  // 8
                    () => World.SwapQueues(),
                    () => World.StartEntityRoutines(_ticks)));

            PerformanceMonitor sys = new(manager.TotalTaskCount);

            bool f;

            Time interval, accumulated, start, end, elapsed;

            interval = accumulated = Time.FromMilliseconds(50);
            start = Time.Now();

            while (_running)
            {
                f = accumulated >= interval;
                if (f)
                {
                    System.Diagnostics.Debug.Assert(_ticks >= 0);

                    manager.Start(sys);
                }

                end = Time.Now();
                elapsed = end - start;

                start = end;
                accumulated += elapsed;

                if (elapsed > interval)
                {
                    Console.Printl($"[Warning] The task is taking longer, Elapsed: {elapsed}!");
                }

                if (f)
                {
                    accumulated -= interval;
                    CountTicks();

                    sys.Record(elapsed);

                    if (_ticks % (20 * 5) == 0)
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

            Console.Printl("Finish!");
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

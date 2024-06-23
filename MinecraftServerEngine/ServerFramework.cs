﻿
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

        private void StartServerRoutine(
            Barrier barrier, ConnectionListener connListener)
        {
            System.Diagnostics.Debug.Assert(barrier != null);
            System.Diagnostics.Debug.Assert(connListener != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_ticks >= 0);

            Console.Print(".");

            /*Console.Printl("StartPlayerRoutines!");*/
            _WORLD.StartPlayerControls(barrier, _ticks);

            /*Console.Printl("DestroyEntities!");*/
            _WORLD.DestroyEntities(barrier);

            /*Console.Printl("MoveEntities!");*/
            _WORLD.MoveEntities(barrier);

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


            int n = 2;  // TODO: Determine using number of processor.

            using Barrier barrier = new(n);

            System.Diagnostics.Debug.Assert(n > 1);
            for (int i = 0; i < n - 1; ++i)
            {
                NewThread(() =>
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

                            StartServerRoutine(barrier, connListener);
                        }
                        finally
                        {
                            rLocker.Release();
                        }
                    } while (true);
                });

            }

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

                            StartServerRoutine(barrier, connListener);
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
                        Console.Print($"The task is taking longer, Elapsed: {elapsed}!");
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

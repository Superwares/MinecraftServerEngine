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
            Locker locker, Cond cond, 
            Barrier barrier, ConnectionListener connListener)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_ticks >= 0);

            Console.Print(".");

            /*Console.Printl("StartPlayerRoutines!");*/
            _WORLD.StartPlayerRoutines(locker, cond, barrier, _ticks);

            /*Console.Printl("HandlePlayerConnections!");*/
            _WORLD.HandlePlayerConnections(locker, cond, barrier, _ticks);

            /*Console.Printl("DestroyEntities!");*/
            _WORLD.DestroyEntities(locker, cond, barrier);

            /*Console.Printl("DestroyPlayers!");*/
            _WORLD.DestroyPlayers(locker, cond, barrier);

            /*Console.Printl("MoveEntities!");*/
            _WORLD.MoveEntities(locker, cond, barrier);

            /*Console.Printl("MovePlayers");*/
            _WORLD.MovePlayers(locker, cond, barrier);

            /*Console.Printl("CreateEntities!");*/
            _WORLD.CreateEntities(locker, cond, barrier);

            /*Console.Printl("Create or Connect Players!");*/
            connListener.Accept(locker, cond, barrier, _WORLD);
                   
            /*Console.Printl("HandlePlayerRenders!");*/
            _WORLD.HandlePlayerRenders(locker, cond, barrier);

            /*Console.Printl("StartRoutine!");*/
            _WORLD.StartRoutine(locker, cond, barrier, _ticks);

            /*Console.Printl("StartEntityRoutines!");*/
            _WORLD.StartEntityRoutines(locker, cond, barrier, _ticks);
        }        
        
        private void StartMainRoutine(Locker locker, Cond cond, Barrier barrier)
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

        private void StartCancelRoutine()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _running = false;
        }

        public void Run()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            MainThread = Thread.GetCurrent();
            Console.HandleTerminatin(() =>
            {
                _running = false;

                System.Diagnostics.Debug.Assert(MainThread != null);
                MainThread.Join();
            });

            ushort port = 25565;

            int n = 2;  // TODO: Determine using number of processor.

            using Locker locker = new();
            using Cond cond = new(locker);
            using Barrier barrier = new(n);
            using ConnectionListener connListener = new();

            System.Diagnostics.Debug.Assert(n > 1);
            for (int i = 0; i < n - 1; ++i)
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

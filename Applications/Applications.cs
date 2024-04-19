using Containers;
using System;
using System.Diagnostics;
using System.Threading;

namespace Applications
{
    /*public abstract class Application : IDisposable
    {

    }*/

    public class ConsoleApplication : IDisposable/*, Application*/
    {
        public delegate void StartRoutine();

        private static ulong GetCurrentTimeInMicroseconds()
        {
            return (ulong)(DateTime.Now.Ticks / TimeSpan.TicksPerMicrosecond);
        }

        private readonly object _SharedObject = new();

        private bool _running = true;
        public bool Running => _running;

        private readonly Queue<Thread> _threads = new();

        private bool _disposed = false;

        private void Cancel()
        {
            Debug.Assert(!_disposed);

            lock (_SharedObject)
            {
                _running = false;

                while (_threads.Count > 0)
                {
                    Thread t = _threads.Dequeue();
                    t.Join();
                }

                Monitor.Wait(_SharedObject);
            }
        }

        public ConsoleApplication()
        {
            Console.CancelKeyPress += (sender, e) => Cancel();
        }

        ~ConsoleApplication()
        {
            Debug.Assert(false);
        }

        protected void Run(StartRoutine startRoutine)
        {
            Debug.Assert(!_disposed);
            Debug.Assert(_running);

            Thread thread = new(new ThreadStart(startRoutine));
            thread.Start();

            _threads.Enqueue(thread);
        }

        protected virtual void Dispose(bool disposing)
        {
            Debug.Assert(!_running);
            
            lock (_SharedObject) 
                Monitor.Pulse(_SharedObject);

            if (_disposed) return;

            // Assertion.
            Debug.Assert(_threads.Count == 0);

            if (disposing == true)
            {
                // Release managed resources.
                _threads.Dispose();
            }

            // Release unmanaged resources.

            _disposed = true;

            Console.WriteLine("Close!");
        }

        public void Dispose()
        {
            Console.WriteLine("Dispose!");
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /*public void Close() => Dispose();*/

    }

}

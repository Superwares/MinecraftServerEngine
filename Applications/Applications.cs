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

        private readonly object _SharedObject = new();

        private bool _closed = false;
        public bool Closed => _closed;
        public bool Running => !_closed;

        
        private readonly Queue<Thread> _threads = new();


        private bool _disposed = false;

        private void Close()
        {
            Debug.Assert(!_disposed);

            _closed = true;

            while (_threads.Count > 0)
            {
                Thread t = _threads.Dequeue();
                t.Join();
            }

            /*Thread.Sleep(1000 * 5);*/

            Console.WriteLine("Close!");
            lock (_SharedObject)
                Monitor.Pulse(_SharedObject);
        }

        public ConsoleApplication()
        {
            Console.CancelKeyPress += (sender, e) => Close();
        }

        ~ConsoleApplication() => Dispose(false);

        protected void Run(StartRoutine f)
        {
            Debug.Assert(!_disposed);
            Debug.Assert(Closed == false);

            Thread thread = new(new ThreadStart(f));
            thread.Start();

            _threads.Enqueue(thread);
        }

        protected virtual void Dispose(bool disposing)
        {
            Debug.Assert(Closed == true);

            lock (_SharedObject)
                Monitor.Wait(_SharedObject);

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
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }

}

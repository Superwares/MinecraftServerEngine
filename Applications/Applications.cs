using Containers;

namespace Applications
{
    public class ConsoleApplication : System.IDisposable/*, Application*/
    {
        public delegate void StartRoutine();

        private readonly object _SharedObject = new();

        private bool _running = true;
        public bool Running => _running;

        private readonly Queue<System.Threading.Thread> _threads = new();

        private bool _disposed = false;

        private void Cancel()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            lock (_SharedObject)
            {
                _running = false;

                while (_threads.Count > 0)
                {
                    System.Threading.Thread t = _threads.Dequeue();
                    t.Join();
                }

                System.Threading.Monitor.Wait(_SharedObject);
            }
        }

        public ConsoleApplication()
        {
            System.Console.CancelKeyPress += (sender, e) => Cancel();
        }

        ~ConsoleApplication() => System.Diagnostics.Debug.Assert(false);

        protected void Run(StartRoutine startRoutine)
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            System.Diagnostics.Debug.Assert(_running);

            System.Threading.Thread thread = new(new System.Threading.ThreadStart(startRoutine));
            thread.Start();

            _threads.Enqueue(thread);
        }

        protected virtual void Dispose(bool disposing)
        {
            System.Diagnostics.Debug.Assert(!_running);
            
            lock (_SharedObject)
            {
                System.Threading.Monitor.Pulse(_SharedObject);
            }

            if (_disposed) return;

            // Assertion.
            System.Diagnostics.Debug.Assert(_threads.Count == 0);

            if (disposing == true)
            {
                // Release managed resources.
                _threads.Dispose();
            }

            // Release unmanaged resources.

            _disposed = true;

            System.Console.WriteLine("Close!");
        }

        public void Dispose()
        {
            /*Console.WriteLine("Dispose!");*/
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        /*public void Close() => Dispose();*/

    }

}

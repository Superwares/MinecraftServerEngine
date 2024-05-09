using Containers;

namespace Applications
{
    public class ConsoleApplication : System.IDisposable/*, Application*/
    {
        private bool _disposed = false;

        public delegate void StartRoutine();

        private readonly object _SHARED_OBJECT = new();

        private bool _running = true;
        public bool Running => _running;

        private readonly Queue<System.Threading.Thread> _THREADS = new();

        private void Cancel()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            lock (_SHARED_OBJECT)
            {
                _running = false;

                while (_THREADS.Count > 0)
                {
                    System.Threading.Thread t = _THREADS.Dequeue();
                    t.Join();
                }

                System.Threading.Monitor.Wait(_SHARED_OBJECT);
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

            _THREADS.Enqueue(thread);
        }

        public virtual void Dispose()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_running);
            
            lock (_SHARED_OBJECT)
            {
                System.Threading.Monitor.Pulse(_SHARED_OBJECT);
            }

            // Assertion.
            System.Diagnostics.Debug.Assert(_THREADS.Empty);

            // Release resources.
            _THREADS.Dispose();

            // Finish
            System.GC.SuppressFinalize(this);
            _disposed = true;

            System.Console.Write("Cancel!");
        }


    }

}

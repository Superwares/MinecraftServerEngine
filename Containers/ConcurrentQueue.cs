
using Sync;

namespace Containers
{
    public sealed class ConcurrentQueue<T> : Queue<T>
    {
        private bool _disposed = false;

        private readonly ReadLocker Locker = new();

        public ConcurrentQueue() { }

        ~ConcurrentQueue()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        public override void Enqueue(T value)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            Locker.Hold();

            try
            {
                base.Enqueue(value);

            }
            finally
            {
                Locker.Release();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="EmptyContainerException">The Queue<T> is empty.</exception>
        public override T Dequeue()
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            Locker.Hold();

            try
            {
                return base.Dequeue();
            }
            finally
            {
                Locker.Release();
            }
        }

        public override bool Dequeue(out T value)
        {
            Locker.Hold();

            try
            {
                return base.Dequeue(out value);
            }
            finally
            {
                Locker.Release();
            }
        }

        public override T[] Flush()
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            Locker.Hold();

            try
            {
                return base.Flush();
            }
            finally
            {
                Locker.Release();
            }

        }

        public new System.Collections.Generic.IEnumerable<T> GetValues()
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            Locker.Read();

            try
            {
                if (Empty == false)
                {
                    System.Diagnostics.Debug.Assert(_inNode != null);
                    System.Diagnostics.Debug.Assert(_outNode != null);

                    Node current = _outNode;
                    do
                    {
                        System.Diagnostics.Debug.Assert(current != null);

                        yield return current.Value;

                        current = current.NextNode;
                    } while (current != null);
                }
            }
            finally
            {
                Locker.Release();
            }
        }

        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed == false)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing == true)
                {
                    // Dispose managed resources.
                    Locker.Dispose();
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

            base.Dispose(disposing);
        }

    }

}


using Sync;

namespace Containers
{
    public sealed class ConcurrentQueue<T> : Queue<T>
    {
        private bool _disposed = false;

        private readonly ReadLocker ReadLocker = new();

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

            System.Diagnostics.Debug.Assert(ReadLocker != null);
            ReadLocker.Hold();

            try
            {
                base.Enqueue(value);

            }
            finally
            {
                System.Diagnostics.Debug.Assert(ReadLocker != null);
                ReadLocker.Release();
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

            System.Diagnostics.Debug.Assert(ReadLocker != null);
            ReadLocker.Hold();

            try
            {
                return base.Dequeue();
            }
            finally
            {
                System.Diagnostics.Debug.Assert(ReadLocker != null);
                ReadLocker.Release();
            }
        }

        public override bool Dequeue(out T value)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(ReadLocker != null);
            ReadLocker.Hold();

            try
            {
                return base.Dequeue(out value);
            }
            finally
            {
                System.Diagnostics.Debug.Assert(ReadLocker != null);
                ReadLocker.Release();
            }
        }

        public override void Dequeue(out T value, T defaultValue)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(ReadLocker != null);
            ReadLocker.Hold();

            try
            {
                base.Dequeue(out value, defaultValue);
            }
            finally
            {
                System.Diagnostics.Debug.Assert(ReadLocker != null);
                ReadLocker.Release();
            }
        }

        public override T[] Flush()
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(ReadLocker != null);
            ReadLocker.Hold();

            try
            {
                return base.Flush();
            }
            finally
            {
                System.Diagnostics.Debug.Assert(ReadLocker != null);
                ReadLocker.Release();
            }

        }

        public new System.Collections.Generic.IEnumerable<T> GetValues()
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(ReadLocker != null);
            ReadLocker.Read();

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
                System.Diagnostics.Debug.Assert(ReadLocker != null);
                ReadLocker.Release();
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
                    ReadLocker.Dispose();
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

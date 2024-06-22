
using Containers;
using Sync;

namespace MinecraftServerEngine
{
    internal class SwapQueue<T> : System.IDisposable
    {
        private bool _disposed = false;

        private readonly Locker Locker = new();  // Disposable

        private int dequeue = 1;
        private readonly ConcurrentQueue<T> Queue1 = new();  // Disposable
        private readonly ConcurrentQueue<T> Queue2 = new();  // Disposable

        private ConcurrentQueue<T> GetQueueForDequeue()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (dequeue == 1)
            {
                return Queue1;
            }
            else if (dequeue == 2)
            {
                return Queue2;
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }

            throw new System.NotImplementedException();
        }

        private ConcurrentQueue<T> GetQueueForEnqueue()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (dequeue == 1)
            {
                return Queue2;
            }
            else if (dequeue == 2)
            {
                return Queue1;
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }

            throw new System.NotImplementedException();
        }

        public int Count
        {
            get
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                ConcurrentQueue<T> queue = GetQueueForDequeue();
                return queue.Count;
            }
        }
        public bool Empty => (Count == 0);

        public SwapQueue() { }

        ~SwapQueue() => System.Diagnostics.Debug.Assert(false);

        public virtual void Swap()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Locker.Hold();

            ConcurrentQueue<T> queue = GetQueueForDequeue();

            if (queue.Empty)
            {
                if (dequeue == 1)
                {
                    System.Diagnostics.Debug.Assert(Queue1.Empty);

                    dequeue = 2;
                }
                else if (dequeue == 2)
                {
                    System.Diagnostics.Debug.Assert(Queue2.Empty);

                    dequeue = 1;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                }
            }

            Locker.Release();
        }

        public virtual void Enqueue(T value)
        {
            System.Diagnostics.Debug.Assert(value != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            ConcurrentQueue<T> queue = GetQueueForEnqueue();
            queue.Enqueue(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="EmptyContainerException"></exception>
        public virtual T Dequeue()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            ConcurrentQueue<T> queue = GetQueueForDequeue();
            T value = queue.Dequeue();

            return value;
        }

        public virtual void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.
            Locker.Dispose();

            Queue1.Dispose();
            Queue2.Dispose();

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }
    }
}

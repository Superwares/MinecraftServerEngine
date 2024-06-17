

using Sync;

namespace Containers
{
    public class SwapQueue<T> : System.IDisposable
    {
        private bool _disposed = false;

        private int dequeue = 1;
        private readonly Queue<T> _QUEUE1 = new();
        private readonly Queue<T> _QUEUE2 = new();

        private Queue<T> GetQueueForDequeue()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (dequeue == 1)
            {
                return _QUEUE1;
            }
            else if (dequeue == 2)
            {
                return _QUEUE2;
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }

            throw new System.NotImplementedException();
        }

        private Queue<T> GetQueueForEnqueue()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (dequeue == 1)
            {
                return _QUEUE2;
            }
            else if (dequeue == 2)
            {
                return _QUEUE1;
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

                Queue<T> queue = GetQueueForDequeue();
                return queue.Count;
            }
        }
        public bool Empty => (Count == 0);

        public SwapQueue() { }

        ~SwapQueue() => System.Diagnostics.Debug.Assert(false);

        public virtual void Swap()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (dequeue == 1)
            {
                System.Diagnostics.Debug.Assert(_QUEUE1.Empty);

                dequeue = 2;
            }
            else if (dequeue == 2)
            {
                System.Diagnostics.Debug.Assert(_QUEUE2.Empty);

                dequeue = 1;
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }
        }

        public virtual void Enqueue(T value)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Queue<T> queue = GetQueueForEnqueue();
            queue.Enqueue(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="EmptyContainerException">The DualQueue<T> is empty.</exception>
        public virtual T Dequeue()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Queue<T> queue = GetQueueForDequeue();
            T value = queue.Dequeue();

            return value;
        }

        public virtual void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.
            _QUEUE1.Dispose();
            _QUEUE2.Dispose();

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }
    }

    public sealed class ConcurrentSwapQueue<T> : SwapQueue<T>
    {
        private bool _disposed = false;

        private readonly Lock _MUTEX = new();

        public ConcurrentSwapQueue() { }

        ~ConcurrentSwapQueue() => System.Diagnostics.Debug.Assert(false);

        public override void Swap()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _MUTEX.Hold();

            base.Swap();

            _MUTEX.Release();
        }

        public override void Enqueue(T value)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _MUTEX.Hold();

            base.Enqueue(value);

            _MUTEX.Release();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="EmptyContainerException">The DualQueue<T> is empty.</exception>
        public override T Dequeue()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _MUTEX.Hold();

            T value = base.Dequeue();

            _MUTEX.Release();

            return value;
        }

        public override void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.
            _MUTEX.Dispose();

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }
}

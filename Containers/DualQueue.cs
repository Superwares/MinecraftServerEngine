

namespace Containers
{
    public sealed class DualQueue<T> : System.IDisposable
        where T : class
    {
        private bool _disposed = false;

        private readonly object _SHARED_OBJECT1 = new();
        private readonly object _SHARED_OBJECT2 = new();

        private bool _flag = true;
        private readonly Queue<T> _FIRST_QUEUE = new();  // Disposable
        private readonly Queue<T> _SECOND_QUEUE = new();  // Disposable

        private int _count = 0;
        public int Count
        {
            get
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                return _count;
            }
        }
        public bool Empty => Count == 0;

        public DualQueue()
        {

        }

        ~DualQueue() => System.Diagnostics.Debug.Assert(false);

        public void Switch()
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            
            // TODO: Check whether the locking is necessary or not.
            lock (_SHARED_OBJECT1) lock (_SHARED_OBJECT2)
            {
                _flag = !_flag;
            }
        }

        public T? Dequeue()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            lock (_SHARED_OBJECT1)
            {
                
                Queue<T> queue;
                if (_flag)
                {
                    queue = _FIRST_QUEUE;
                }
                else
                {
                    queue = _SECOND_QUEUE;
                }

                if (queue.Empty)
                {
                    return null;
                }
                else
                {
                    --_count;
                    return queue.Dequeue();
                }
                
            }
        }

        public void Enqueue(T value)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            lock (_SHARED_OBJECT2)
            {
                Queue<T> queue;
                if (_flag)
                {
                    queue = _SECOND_QUEUE;
                }
                else
                {
                    queue = _FIRST_QUEUE;
                }

                queue.Enqueue(value);

                ++_count;
            }
        }

        
        public void Dispose()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // Assertion

            // Release resources.
            _FIRST_QUEUE.Dispose();
            _SECOND_QUEUE.Dispose();

            // Finish
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }
    }
}

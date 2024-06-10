

using Threading;

namespace Containers
{
    public sealed class DualQueue<T> : System.IDisposable
    {
        private bool _disposed = false;

        private readonly Mutex _MUTEX = new();



        public DualQueue() { }

        ~DualQueue() => System.Diagnostics.Debug.Assert(false);

        public void Switch()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // 한쪽이 모두 비어있음을 확인
            throw new System.NotImplementedException();
        }

        public void Enqueue(T v)
        {
            System.Diagnostics.Debug.Assert(!_disposed);


            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="EmptyContainerException">The DualQueue<T> is empty.</exception>
        public T Dequeue()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.
            _MUTEX.Dispose();

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }
    }
}

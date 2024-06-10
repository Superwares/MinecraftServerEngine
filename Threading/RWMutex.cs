
namespace Threading
{
    public sealed class RWMutex : System.IDisposable
    {
        private bool _disposed = false;

        public void Lock()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            throw new System.NotImplementedException();
        }

        public void Rlock()
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            throw new System.NotImplementedException();
        }

        public void Unlock()
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release reousrces.

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }
}

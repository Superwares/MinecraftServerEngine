
namespace Threading
{
    public sealed class Mutex : System.IDisposable
    {
        private bool _disposed = false;

        public Mutex() { }

        ~Mutex() => System.Diagnostics.Debug.Assert(false):

        public void Lock()
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

            // Release resources.

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }
}

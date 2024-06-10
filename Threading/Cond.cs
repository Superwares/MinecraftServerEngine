
namespace Threading
{
    public sealed class Cond : System.IDisposable
    {
        private bool _disposed = false;

        public void Wait()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            throw new System.NotImplementedException();
        }

        public void Signal()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            throw new System.NotImplementedException();
        }

        public void Broadcast()
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

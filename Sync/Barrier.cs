
namespace Sync
{
    public sealed class Barrier : System.IDisposable
    {
        private bool _disposed = false;

        public Barrier(int n) 
        {
            throw new System.NotImplementedException();
        }

        ~Barrier() => System.Diagnostics.Debug.Assert(false);

        public void ReachAndWait()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            throw new System.NotImplementedException();
        }

        public void WaitAllReach()
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
            System.Diagnostics.Debug.Assert(false);

            // Release resources.

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }
}

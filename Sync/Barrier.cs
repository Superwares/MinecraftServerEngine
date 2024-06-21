
namespace Sync
{
    public sealed class Barrier : System.IDisposable
    {
        private bool _disposed = false;

        public readonly int Count;

        public Barrier(int n)
        {
            Count = n;
        }

        ~Barrier() => System.Diagnostics.Debug.Assert(false);

        public void SignalAndWait()
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

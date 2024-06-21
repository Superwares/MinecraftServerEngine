
namespace Sync
{
    public sealed class Cond : System.IDisposable
    {
        private bool _disposed = false;

        private readonly Locker Locker;

        public Cond(Locker locker)
        {
            Locker = locker;
        }

        ~Cond() => System.Diagnostics.Debug.Assert(false);

        public void Wait()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Locker.Hold();
            System.Threading.Monitor.Wait(Locker.Object);
            Locker.Release();
        }

        public void Signal()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Locker.Hold();
            System.Threading.Monitor.Pulse(Locker.Object);
            Locker.Release();
        }

        public void Broadcast()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Locker.Hold();
            System.Threading.Monitor.PulseAll(Locker.Object);
            Locker.Release();
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

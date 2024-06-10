
namespace Threading
{
    public class Lock : System.IDisposable
    {
        private bool _disposed = false;

        public Lock() { }

        ~Lock() => System.Diagnostics.Debug.Assert(false);

        public virtual void Hold()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            throw new System.NotImplementedException();
        }

        public virtual void Release()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            throw new System.NotImplementedException();
        }

        public virtual void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }

    public sealed class RecurLock : Lock
    {
        private bool _disposed = false;

        public RecurLock() { }

        ~RecurLock() => System.Diagnostics.Debug.Assert(false);

        public override void Hold()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            throw new System.NotImplementedException();
        }

        public override void Release()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            throw new System.NotImplementedException();
        }

        public override void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.

            // Finish.
            base.Dispose();
            _disposed = true;
        }
    }

    public sealed class RWLock : Lock
    {
        private bool _disposed = false;

        public RWLock() { }

        ~RWLock() => System.Diagnostics.Debug.Assert(false);

        public override void Hold()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            throw new System.NotImplementedException();
        }

        public void HoldForRead()
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            throw new System.NotImplementedException();
        }

        public override void Release()
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            throw new System.NotImplementedException();
        }

        public override void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release reousrces.

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

}

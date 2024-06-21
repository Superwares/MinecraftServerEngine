
namespace Sync
{
    public class Locker : System.IDisposable
    {
        private bool _disposed = false;

        internal readonly object Object = new();  // Disposable

        public Locker() { }

        ~Locker() => System.Diagnostics.Debug.Assert(false);

        public virtual void Hold()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Threading.Monitor.Enter(Object);
        }

        public virtual void Release()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Threading.Monitor.Exit(Object);
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

    // SpinLocker

    public sealed class RecurLocker : Locker
    {
        private bool _disposed = false;

        public RecurLocker() { }

        ~RecurLocker() => System.Diagnostics.Debug.Assert(false);

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

    public sealed class ReadLocker : Locker
    {
        private bool _disposed = false;

        public ReadLocker() { }

        ~ReadLocker() => System.Diagnostics.Debug.Assert(false);

        public override void Hold()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            throw new System.NotImplementedException();
        }

        public void Read()
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

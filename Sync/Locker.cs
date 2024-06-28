
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

    public sealed class ReadLocker : Locker
    {
        private bool _disposed = false;

        private readonly Locker Locker;
        private readonly Cond Cond;

        private int _read = 0;
        private int _write = 0;

        public ReadLocker() 
        {
            Locker = new Locker();
            Cond = new Cond(Locker);
        }

        ~ReadLocker() => System.Diagnostics.Debug.Assert(false);

        public override void Hold()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Locker.Hold();

            while (_read > 0 || _write > 0)
            {
                Cond.Wait();
            }

            System.Diagnostics.Debug.Assert(_read == 0);
            System.Diagnostics.Debug.Assert(_write == 0);

            ++_write;

            Locker.Release();
        }

        public void Read()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Locker.Hold();

            while (_write > 0)
            {
                Cond.Wait();
            }

            System.Diagnostics.Debug.Assert(_write == 0);

            ++_read;

            Locker.Release();
        }

        public override void Release()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Locker.Hold();

            if (_write > 0)
            {
                System.Diagnostics.Debug.Assert(_read == 0);

                --_write;

                System.Diagnostics.Debug.Assert(_write == 0);
            }
            else if (_read > 0)
            {
                System.Diagnostics.Debug.Assert(_read >= 0);
                System.Diagnostics.Debug.Assert(_write == 0);

                --_read;
            }

            Cond.Broadcast();

            Locker.Release();
        }

        public override void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release reousrces.
            Locker.Dispose();
            Cond.Dispose();

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

}

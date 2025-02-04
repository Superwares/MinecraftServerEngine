using Containers;
using Sync;

namespace MinecraftServerEngine.Entities
{

    internal static class EntityIdAllocator
    {
    
        // TODO: make static destructor to dispose.
        private readonly static Locker _Locker = new();  // Disposable
        private readonly static NumberList _NumberList = new();  // Disposable

        public static int Allocate()
        {
            System.Diagnostics.Debug.Assert(_Locker != null);
            _Locker.Hold();

            try
            {
                System.Diagnostics.Debug.Assert(_NumberList != null);
                return _NumberList.Allocate();
            }
            finally
            {
                System.Diagnostics.Debug.Assert(_Locker != null);
                _Locker.Release();
            }
        }

        public static void Deallocate(int id)
        {
            System.Diagnostics.Debug.Assert(_Locker != null);
            _Locker.Hold();

            try
            {
                System.Diagnostics.Debug.Assert(_NumberList != null);
                _NumberList.Deallocate(id);
            }
            finally
            {
                System.Diagnostics.Debug.Assert(_Locker != null);
                _Locker.Release();
            }
        }

    }
}

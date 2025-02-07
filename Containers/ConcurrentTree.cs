
using Sync;

namespace Containers
{
    public sealed class ConcurrentTree<K> : Tree<K>
        where K : notnull
    {
        private bool _disposed = false;

        private readonly ReadLocker Locker = new();

        public ConcurrentTree() { }

        ~ConcurrentTree()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <exception cref="DuplicateKeyException"></exception>
        public override void Insert(K key)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            Locker.Hold();

            try
            {
                base.Insert(key);
            }
            finally
            {
                Locker.Release();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public override void Extract(K key)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            Locker.Hold();

            try
            {
                base.Extract(key);
            }
            finally
            {
                Locker.Release();
            }
        }

        public override bool Contains(K key)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(Locker != null);
            Locker.Read();

            try
            {
                return base.Contains(key);
            } 
            finally
            {
                System.Diagnostics.Debug.Assert(Locker != null);
                Locker.Release();
            }
        }

        public override K[] Flush()
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            Locker.Hold();

            K[] keys = base.Flush();

            Locker.Release();

            return keys;
        }

        public new System.Collections.Generic.IEnumerable<K> GetKeys()
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(_flags.Length >= MinActualLength);
            System.Diagnostics.Debug.Assert(_keys.Length >= MinActualLength);
            System.Diagnostics.Debug.Assert(_length >= MinActualLength);
            System.Diagnostics.Debug.Assert(_count >= 0);

            Locker.Read();

            if (!Empty)
            {
                int i = 0;
                for (int j = 0; j < _length; ++j)
                {
                    if (!_flags[j])
                    {
                        continue;
                    }

                    yield return _keys[j];

                    if (++i == _count)
                    {
                        break;
                    }
                }
            }

            Locker.Release();
        }

        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed == false)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing == true)
                {
                    // Dispose managed resources.
                    Locker.Dispose();
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                //CloseHandle(handle);
                //handle = IntPtr.Zero;

                // Note disposing has been done.
                _disposed = true;
            }

            base.Dispose(disposing);
        }


    }

}

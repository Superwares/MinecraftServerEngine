

using Sync;

namespace Containers
{

    public sealed class ConcurrentTable<K, T> : Table<K, T>
        where K : notnull
    {
        private bool _disposed = false;

        private readonly ReadLocker Locker = new();

        public ConcurrentTable() { }

        ~ConcurrentTable()
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
        public override void Insert(K key, T value)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(Locker != null);
            Locker.Hold();

            try
            {
                base.Insert(key, value);
            }
            finally
            {
                System.Diagnostics.Debug.Assert(Locker != null);
                Locker.Release();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public override T Extract(K key)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(Locker != null);
            Locker.Hold();

            try
            {
                return base.Extract(key);
            }
            finally
            {
                System.Diagnostics.Debug.Assert(Locker != null);
                Locker.Release();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public override T Lookup(K key)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(Locker != null);
            Locker.Read();

            try
            {
                return base.Lookup(key);
            }
            finally
            {
                System.Diagnostics.Debug.Assert(Locker != null);
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

        public override (K, T)[] Flush()
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(Locker != null);
            Locker.Hold();

            try
            {

                return base.Flush();
            }
            finally
            {
                System.Diagnostics.Debug.Assert(Locker != null);
                Locker.Release();
            }
        }

        public new System.Collections.Generic.IEnumerable<(K, T)> GetElements()
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(_flags.Length >= MinLength);
            System.Diagnostics.Debug.Assert(_keys.Length >= MinLength);
            System.Diagnostics.Debug.Assert(_values.Length >= MinLength);
            System.Diagnostics.Debug.Assert(_length >= MinLength);
            System.Diagnostics.Debug.Assert(_count >= 0);

            System.Diagnostics.Debug.Assert(Locker != null);
            Locker.Read();

            try
            {
                if (!Empty)
                {
                    int i = 0;
                    for (int j = 0; j < _length; ++j)
                    {
                        if (!_flags[j])
                        {
                            continue;
                        }

                        yield return (_keys[j], _values[j]);

                        if (++i == _count)
                        {
                            break;
                        }
                    }
                }

            }
            finally
            {
                System.Diagnostics.Debug.Assert(Locker != null);
                Locker.Release();
            }
        }

        public new System.Collections.Generic.IEnumerable<K> GetKeys()
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(_flags.Length >= MinLength);
            System.Diagnostics.Debug.Assert(_keys.Length >= MinLength);
            System.Diagnostics.Debug.Assert(_values.Length >= MinLength);
            System.Diagnostics.Debug.Assert(_length >= MinLength);
            System.Diagnostics.Debug.Assert(_count >= 0);

            System.Diagnostics.Debug.Assert(Locker != null);
            Locker.Read();

            try
            {
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

            }
            finally
            {
                System.Diagnostics.Debug.Assert(Locker != null);
                Locker.Release();
            }
        }

        public new System.Collections.Generic.IEnumerable<T> GetValues()
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(_flags.Length >= MinLength);
            System.Diagnostics.Debug.Assert(_keys.Length >= MinLength);
            System.Diagnostics.Debug.Assert(_values.Length >= MinLength);
            System.Diagnostics.Debug.Assert(_length >= MinLength);
            System.Diagnostics.Debug.Assert(_count >= 0);

            System.Diagnostics.Debug.Assert(Locker != null);
            Locker.Read();

            try
            {
                if (!Empty)
                {
                    int i = 0;
                    for (int j = 0; j < _length; ++j)
                    {
                        if (!_flags[j])
                        {
                            continue;
                        }

                        yield return _values[j];

                        if (++i == _count)
                        {
                            break;
                        }
                    }
                }

            }
            finally
            {
                System.Diagnostics.Debug.Assert(Locker != null);
                Locker.Release();
            }
        }

        public override Table<K, T> Clone()
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(Locker != null);
            Locker.Read();

            try
            {
                return base.Clone();
            }
            finally
            {
                System.Diagnostics.Debug.Assert(Locker != null);
                Locker.Release();
            }
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

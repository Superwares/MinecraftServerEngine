
using Sync;

namespace Containers
{

    public class Table<K, T> : System.IDisposable
        where K : notnull
    {
        private bool _disposed = false;

        protected const int MinLength = 16;
        protected const int ExpansionFactor = 2;
        protected const float LoadFactor = 0.75F;
        protected const int Constant = 5;

        private readonly System.Collections.Generic.IEqualityComparer<K> Comparer;

        protected bool[] _flags = new bool[MinLength];
        protected K[] _keys = new K[MinLength];
        protected T[] _values = new T[MinLength];
        protected int _length = MinLength;
        protected int _count = 0;

        public int Count
        {
            get
            {
                if (_disposed == true)
                {
                    throw new System.ObjectDisposedException(GetType().Name);
                }
                return _count;
            }
        }

        public bool Empty => (Count == 0);

        public Table()
        {
            Comparer = System.Collections.Generic.EqualityComparer<K>.Default;
        }

        public Table(System.Collections.Generic.IEqualityComparer<K> comparer)
        {
            Comparer = comparer;
        }

        private Table(
            System.Collections.Generic.IEqualityComparer<K> comparer,
            bool[] flags,
            K[] keys,
            T[] values,
            int length,
            int count
            )
        {
            System.Diagnostics.Debug.Assert(comparer != null);
            Comparer = comparer;

            System.Diagnostics.Debug.Assert(flags != null);
            System.Diagnostics.Debug.Assert(keys != null);
            System.Diagnostics.Debug.Assert(values != null);
            System.Diagnostics.Debug.Assert(length >= MinLength);
            System.Diagnostics.Debug.Assert(count >= 0);
            _flags = flags;
            _keys = keys;
            _values = values;
            _length = length;
            _count = count;

        }

        ~Table()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        private int Hash(K key)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            return System.Math.Abs(key.GetHashCode() * Constant);
        }

        private void Resize(int newLength)
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

            bool[] oldFlags = _flags;
            K[] oldKeys = _keys;
            T[] oldValues = _values;
            int oldLength = _length;

            bool[] newFlags = new bool[newLength];
            K[] newKeys = new K[newLength];
            T[] newValues = new T[newLength];

            for (int i = 0; i < oldLength; ++i)
            {
                if (!oldFlags[i])
                {
                    continue;
                }

                K key = oldKeys[i];
                int hash = Hash(key);
                for (int j = 0; j < newLength; ++j)
                {
                    int index = (hash + j) % newLength;

                    if (newFlags[index])
                    {
                        continue;
                    }

                    newFlags[index] = true;
                    newKeys[index] = key;
                    newValues[index] = oldValues[i];

                    break;
                }

            }

            _flags = newFlags;
            _keys = newKeys;
            _values = newValues;
            _length = newLength;

            oldFlags = null;
            oldKeys = null;
            oldValues = null;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <exception cref="DuplicateKeyException"></exception>
        public virtual void Insert(K key, T value)
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

            int index = -1;

            int hash = Hash(key);
            for (int i = 0; i < _length; ++i)
            {
                index = (hash + i) % _length;

                if (_flags[index])
                {
                    if (Comparer.Equals(_keys[index], key))
                    {
                        throw new DuplicateKeyException();
                    }

                    continue;
                }

                break;
            }

            System.Diagnostics.Debug.Assert(index >= 0);
            _flags[index] = true;
            _keys[index] = key;
            _values[index] = value;
            _count++;

            float factor = (float)_count / (float)_length;
            if (factor >= LoadFactor)
            {
                Resize(_length * ExpansionFactor);
            }
        }

        private static bool CanShift(int indexTarget, int indexCurrent, int indexOrigin)
        {
            return (indexTarget < indexCurrent && indexCurrent < indexOrigin) ||
                (indexOrigin < indexTarget && indexTarget < indexCurrent) ||
                (indexCurrent < indexOrigin && indexOrigin < indexTarget) ||
                (indexOrigin == indexTarget);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public virtual T Extract(K key)
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

            if (_count == 0)
            {
                throw new KeyNotFoundException();
            }

            int index = -1;

            int hash = Hash(key);

            int i;
            for (i = 0; i < _length; ++i)
            {
                index = (hash + i) % _length;

                if (!_flags[index])
                {
                    throw new KeyNotFoundException();
                }

                if (!Comparer.Equals(_keys[index], key))
                {
                    continue;
                }

                break;
            }

            --_count;

            /*if (MinLength < _length)
            {
                int lenReduced = _length / ExpansionFactor;
                float factor = (float)_count / (float)lenReduced;
                if (factor < LoadFactor)
                {
                    System.Diagnostics.Debug.Assert(index >= 0);
                    T v = _values[index];
                    _flags[index] = false;

                    Resize(lenReduced);

                    return v;
                }
            }*/

            {
                System.Diagnostics.Debug.Assert(index >= 0);
                int indexTarget = index, indexOrigin;

                T v = _values[indexTarget];
                K keyShift;

                for (++i; i < _length; ++i)
                {
                    index = (hash + i) % _length;

                    if (!_flags[index])
                    {
                        break;
                    }

                    keyShift = _keys[index];
                    indexOrigin = Hash(keyShift) % _length;
                    if (!CanShift(indexTarget, index, indexOrigin))
                    {
                        continue;
                    }

                    _keys[indexTarget] = keyShift;
                    _values[indexTarget] = _values[index];

                    indexTarget = index;
                }

                _flags[indexTarget] = false;

                return v;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public virtual T Lookup(K key)
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

            if (_count == 0)
            {
                throw new KeyNotFoundException();
            }

            int index = -1;

            int hash = Hash(key);
            for (int i = 0; i < _length; ++i)
            {
                index = (hash + i) % _length;

                if (!_flags[index])
                {
                    throw new KeyNotFoundException();
                }

                if (!Comparer.Equals(_keys[index], key))
                {
                    continue;
                }

                break;
            }

            System.Diagnostics.Debug.Assert(index >= 0);
            return _values[index];
        }

        public virtual bool Contains(K key)
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

            if (_count == 0)
            {
                return false;
            }

            int index;

            int hash = Hash(key);
            for (int i = 0; i < _length; ++i)
            {
                index = (hash + i) % _length;

                if (!_flags[index])
                {
                    return false;
                }

                if (!Comparer.Equals(_keys[index], key))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        public virtual (K, T)[] Flush()
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

            if (_count == 0)
            {
                return [];
            }

            var elements = new (K, T)[_count];

            int i = 0;
            for (int j = 0; j < _length; ++j)
            {
                if (!_flags[j])
                {
                    continue;
                }

                elements[i++] = (_keys[j], _values[j]);

                if (i == _count)
                {
                    break;
                }
            }

            _flags = new bool[MinLength];
            _keys = new K[MinLength];
            _values = new T[MinLength];
            _length = MinLength;
            _count = 0;

            return elements;
        }

        public System.Collections.Generic.IEnumerable<(K, T)> GetElements()
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

            if (_count == 0)
            {
                yield break;
            }

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

        public System.Collections.Generic.IEnumerable<K> GetKeys()
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

            if (_count == 0)
            {
                yield break;
            }

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

        public System.Collections.Generic.IEnumerable<T> GetValues()
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

            if (_count == 0)
            {
                yield break;
            }

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

        public virtual Table<K, T> Clone()
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(_flags != null);
            System.Diagnostics.Debug.Assert(_keys != null);
            System.Diagnostics.Debug.Assert(_values != null);
            System.Diagnostics.Debug.Assert(_length >= MinLength);
            System.Diagnostics.Debug.Assert(_count >= 0);
            bool[] flags = new bool[_flags.Length];
            K[] keys = new K[_keys.Length];
            T[] values = new T[_values.Length];
            int length = _length;
            int count = _count;

            Table<K, T> clone = new(
                Comparer,
                flags,
                keys,
                values,
                length,
                count
                );

            return clone;
        }


        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed == false)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing == true)
                {
                    // Dispose managed resources.
                    _flags = null;
                    _keys = null;
                    _values = null;
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
        }

    }

}

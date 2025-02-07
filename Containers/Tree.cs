
using Common;
using Sync;

namespace Containers
{

    public class Tree<K> : System.IDisposable
        where K : notnull
    {
        private bool _disposed = false;

        protected const int MinActualLength = 16;
        protected const int ExpansionFactor = 2;
        protected const float LoadFactor = 0.75F;
        protected const int Constant = 5;

        private readonly System.Collections.Generic.IEqualityComparer<K> Comparer;

        protected bool[] _flags = new bool[MinActualLength];
        protected K[] _keys = new K[MinActualLength];
        protected int _length = MinActualLength;
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

        public Tree()
        {
            Comparer = System.Collections.Generic.EqualityComparer<K>.Default;
        }

        public Tree(System.Collections.Generic.IEqualityComparer<K> comparer)
        {
            Comparer = comparer;
        }

        ~Tree()
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

            /*Debug.Assert(key != null);*/
            int hashCode = key.GetHashCode();
            return System.Math.Abs(hashCode * Constant);
        }

        private void Resize(int newLength)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(_flags.Length >= MinActualLength);
            System.Diagnostics.Debug.Assert(_keys.Length >= MinActualLength);
            System.Diagnostics.Debug.Assert(_length >= MinActualLength);
            System.Diagnostics.Debug.Assert(_count >= 0);

            bool[] oldFlags = _flags;
            K[] oldKeys = _keys;
            int oldLength = _length;

            bool[] newFlags = new bool[newLength];
            K[] newKeys = new K[newLength];

            for (int i = 0; i < oldLength; ++i)
            {
                if (!oldFlags[i])
                    continue;

                K key = oldKeys[i];
                int hash = Hash(key);
                for (int j = 0; j < newLength; ++j)
                {
                    int index = (hash + j) % newLength;

                    if (newFlags[index])
                        continue;

                    newFlags[index] = true;
                    newKeys[index] = key;

                    break;
                }

            }

            _flags = newFlags;
            _keys = newKeys;
            _length = newLength;

            oldFlags = null;
            oldKeys = null;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <exception cref="DuplicateKeyException"></exception>
        public virtual void Insert(K key)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(_flags.Length >= MinActualLength);
            System.Diagnostics.Debug.Assert(_keys.Length >= MinActualLength);
            System.Diagnostics.Debug.Assert(_length >= MinActualLength);
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
            ++_count;

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
        public virtual void Extract(K key)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(_flags.Length >= MinActualLength);
            System.Diagnostics.Debug.Assert(_keys.Length >= MinActualLength);
            System.Diagnostics.Debug.Assert(_length >= MinActualLength);
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
                    _flags[index] = false;

                    Resize(lenReduced);

                    return;
                }
            }*/

            {
                System.Diagnostics.Debug.Assert(index >= 0);
                int indexTarget = index, indexOrigin;

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

                    indexTarget = index;
                }

                _flags[indexTarget] = false;

                return;
            }
        }

        public virtual bool Contains(K key)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(_flags.Length >= MinActualLength);
            System.Diagnostics.Debug.Assert(_keys.Length >= MinActualLength);
            System.Diagnostics.Debug.Assert(_length >= MinActualLength);
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

        public virtual K[] Flush()
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(_flags.Length >= MinActualLength);
            System.Diagnostics.Debug.Assert(_keys.Length >= MinActualLength);
            System.Diagnostics.Debug.Assert(_length >= MinActualLength);
            System.Diagnostics.Debug.Assert(_count >= 0);

            if (_count == 0)
            {
                return [];
            }

            var keys = new K[_count];

            int i = 0;
            for (int j = 0; j < _length; ++j)
            {
                if (!_flags[j])
                {
                    continue;
                }

                keys[i++] = _keys[j];

                if (i == _count)
                {
                    break;
                }
            }

            _flags = new bool[MinActualLength];
            _keys = new K[MinActualLength];
            _length = MinActualLength;
            _count = 0;

            return keys;
        }

        public System.Collections.Generic.IEnumerable<K> GetKeys()
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(_flags.Length >= MinActualLength);
            System.Diagnostics.Debug.Assert(_keys.Length >= MinActualLength);
            System.Diagnostics.Debug.Assert(_length >= MinActualLength);
            System.Diagnostics.Debug.Assert(_count >= 0);

            if (Empty)
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

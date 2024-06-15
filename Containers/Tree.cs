
using Sync;

namespace Containers
{

    public class Tree<K> : System.IDisposable where K : notnull
    {
        private bool _disposed = false;

        protected const int _MIN_LENGTH = 16;
        protected const int _EXPANSION_FACTOR = 2;
        protected const float _LOAD_FACTOR = 0.75F;
        protected const int _C = 5;

        protected bool[] _flags = new bool[_MIN_LENGTH];
        protected K[] _keys = new K[_MIN_LENGTH];
        protected int _length = _MIN_LENGTH;
        protected int _count = 0;

        public int Count
        {
            get
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                return _count;
            }
        }
        public bool Empty => (Count == 0);

        public Tree() { }

        ~Tree() => System.Diagnostics.Debug.Assert(false);

        private int Hash(K key)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            /*Debug.Assert(key != null);*/
            return System.Math.Abs(key.GetHashCode() * _C);
        }

        private void Resize(int newLength)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_length >= _MIN_LENGTH);
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
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_count >= 0);

            int j = -1;

            int hash = Hash(key);
            for (int i = 0; i < _length; ++i)
            {
                int k = (hash + i) % _length;

                if (_flags[k])
                {
                    if (_keys[k].Equals(key))
                    {
                        throw new DuplicateKeyException();
                    }

                    continue;
                }

                j = k;

                break;
            }

            System.Diagnostics.Debug.Assert(j >= 0);
            _flags[j] = true;
            _keys[j] = key;
            _count++;

            float factor = (float)_count / (float)_length;
            if (factor >= _LOAD_FACTOR)
            {
                Resize(_length * _EXPANSION_FACTOR);
            }
        }

        private bool CanShift(int indexTarget, int indexCurrent, int indexOrigin)
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
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_count >= 0);

            if (_count == 0)
            {
                throw new KeyNotFoundException();
            }

            int j = -1;

            int hash = Hash(key);

            int index = -1;
            int i;
            for (i = 0; i < _length; ++i)
            {
                index = (hash + i) % _length;

                if (!_flags[index])
                {
                    throw new KeyNotFoundException();
                }

                if (!_keys[index].Equals(key))
                {
                    continue;
                }

                j = index;

                break;
            }

            --_count;

            if (_MIN_LENGTH < _length)
            {
                int lenReduced = _length / _EXPANSION_FACTOR;
                float factor = (float)_count / (float)lenReduced;
                if (factor < _LOAD_FACTOR)
                {
                    System.Diagnostics.Debug.Assert(j >= 0);
                    _flags[j] = false;

                    Resize(lenReduced);

                    return;
                }
            }

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
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_count >= 0);
            if (_count == 0)
            {
                return false;
            }

            int hash = Hash(key);
            for (int i = 0; i < _length; ++i)
            {
                int index = (hash + i) % _length;

                if (!_flags[index])
                {
                    return false;
                }

                if (!_keys[index].Equals(key))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        public virtual K[] Flush()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_length >= _MIN_LENGTH);
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

            _flags = new bool[_MIN_LENGTH];
            _keys = new K[_MIN_LENGTH];
            _length = _MIN_LENGTH;
            _count = 0;

            return keys;
        }

        public System.Collections.Generic.IEnumerable<K> GetKeys()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_length >= _MIN_LENGTH);
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

        public virtual void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release reousrces.
            _flags = null;
            _keys = null;

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }

    public sealed class ConcurrentTree<K> : Tree<K>
        where K : System.IEquatable<K>
    {
        private bool _disposed = false;

        private readonly RWLock _MUTEX = new();

        public ConcurrentTree() { }

        ~ConcurrentTree() => System.Diagnostics.Debug.Assert(false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <exception cref="DuplicateKeyException"></exception>
        public override void Insert(K key)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _MUTEX.Hold();

            base.Insert(key);

            _MUTEX.Release();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public override void Extract(K key)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _MUTEX.Hold();

            base.Extract(key);

            _MUTEX.Release();
        }

        public override bool Contains(K key)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _MUTEX.HoldForRead();

            bool f = base.Contains(key);

            _MUTEX.Release();

            return f;
        }

        public override K[] Flush()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _MUTEX.Hold();

            K[] keys = base.Flush();

            _MUTEX.Release();

            return keys;
        }

        public new System.Collections.Generic.IEnumerable<K> GetKeys()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_count >= 0);

            _MUTEX.HoldForRead();

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

            _MUTEX.Release();
        }

        public override void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.
            _MUTEX.Dispose();

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

}

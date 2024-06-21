
using Sync;

namespace Containers
{

    public class Tree<K> : System.IDisposable where K : notnull
    {
        private bool _disposed = false;

        protected const int MinLength = 16;
        protected const int ExpansionFactor = 2;
        protected const float LoadFactor = 0.75F;
        protected const int Constant = 5;

        private readonly System.Collections.Generic.IEqualityComparer<K> Comparer;

        protected bool[] _flags = new bool[MinLength];
        protected K[] _keys = new K[MinLength];
        protected int _length = MinLength;
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

        public Tree()
        {
            Comparer = System.Collections.Generic.EqualityComparer<K>.Default;
        }

        public Tree(System.Collections.Generic.IEqualityComparer<K> comparer)
        {
            Comparer = comparer;
        }

        ~Tree() => System.Diagnostics.Debug.Assert(false);

        private int Hash(K key)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            /*Debug.Assert(key != null);*/
            return System.Math.Abs(key.GetHashCode() * Constant);
        }

        private void Resize(int newLength)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= MinLength);
            System.Diagnostics.Debug.Assert(_keys.Length >= MinLength);
            System.Diagnostics.Debug.Assert(_length >= MinLength);
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

            System.Diagnostics.Debug.Assert(_flags.Length >= MinLength);
            System.Diagnostics.Debug.Assert(_keys.Length >= MinLength);
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
        public virtual void Extract(K key)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= MinLength);
            System.Diagnostics.Debug.Assert(_keys.Length >= MinLength);
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

            if (MinLength < _length)
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

            System.Diagnostics.Debug.Assert(_flags.Length >= MinLength);
            System.Diagnostics.Debug.Assert(_keys.Length >= MinLength);
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

        public virtual K[] Flush()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= MinLength);
            System.Diagnostics.Debug.Assert(_keys.Length >= MinLength);
            System.Diagnostics.Debug.Assert(_length >= MinLength);
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

            _flags = new bool[MinLength];
            _keys = new K[MinLength];
            _length = MinLength;
            _count = 0;

            return keys;
        }

        public System.Collections.Generic.IEnumerable<K> GetKeys()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= MinLength);
            System.Diagnostics.Debug.Assert(_keys.Length >= MinLength);
            System.Diagnostics.Debug.Assert(_length >= MinLength);
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

        private readonly ReadLocker Locker = new();

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
            System.Diagnostics.Debug.Assert(!_disposed);

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
            System.Diagnostics.Debug.Assert(!_disposed);

            Locker.Read();

            bool f = base.Contains(key);

            Locker.Release();

            return f;
        }

        public override K[] Flush()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Locker.Hold();

            K[] keys = base.Flush();

            Locker.Release();

            return keys;
        }

        public new System.Collections.Generic.IEnumerable<K> GetKeys()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= MinLength);
            System.Diagnostics.Debug.Assert(_keys.Length >= MinLength);
            System.Diagnostics.Debug.Assert(_length >= MinLength);
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

        public override void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.
            Locker.Dispose();

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

}

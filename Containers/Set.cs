
using Threading;

namespace Containers
{

    public class Set<K> : System.IDisposable
        where K : System.IEquatable<K>
    {
        private bool _disposed = false;

        protected const int _MIN_LENGTH = 16;
        protected const int _EXPENSION_FACTOR = 2;
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

        public Set() { }

        ~Set() => System.Diagnostics.Debug.Assert(false);

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

        public virtual void Insert(K key)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_count >= 0);

            int hash = Hash(key);
            for (int i = 0; i < _length; ++i)
            {
                int index = (hash + i) % _length;

                if (_flags[index])
                {
                    if (_keys[index].Equals(key))
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }

                    continue;
                }

                _flags[index] = true;
                _keys[index] = key;
                _count++;

                float factor = (float)_count / (float)_length;
                if (factor >= _LOAD_FACTOR)
                {
                    Resize(_length * _EXPENSION_FACTOR);
                }

                break;
            }

        }

        private bool CanShift(int targetIndex, int currentIndex, int originIndex)
        {
            return (targetIndex < currentIndex && currentIndex < originIndex) ||
                (originIndex < targetIndex && targetIndex < currentIndex) ||
                (currentIndex < originIndex && originIndex < targetIndex) ||
                (originIndex == targetIndex);
        }

        public virtual void Extract(K key)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_count >= 0);

            if (_count == 0)
            {
                System.Diagnostics.Debug.Assert(false);
            }

            int targetIndex = -1, nextI = -1;
            int hash = Hash(key);
            for (int i = 0; i < _length; ++i)
            {
                int index = (hash + i) % _length;

                if (!_flags[index])
                {
                    System.Diagnostics.Debug.Assert(false);
                }

                if (!_keys[index].Equals(key))
                {
                    continue;
                }

                _count--;

                if (_MIN_LENGTH < _length)
                {
                    int reducedLength = _length / _EXPENSION_FACTOR;
                    float factor = (float)_count / (float)reducedLength;
                    if (factor < _LOAD_FACTOR)
                    {
                        _flags[index] = false;
                        Resize(reducedLength);

                        return;
                    }
                }

                targetIndex = index;
                nextI = i + 1;

                break;
            }

            System.Diagnostics.Debug.Assert(targetIndex >= 0);
            System.Diagnostics.Debug.Assert(nextI > 0);
            for (int i = nextI; i < _length; ++i)
            {
                int index = (hash + i) % _length;

                if (!_flags[index])
                {
                    break;
                }

                K shiftedKey = _keys[index];
                int originIndex = Hash(shiftedKey) % _length;
                if (!CanShift(targetIndex, index, originIndex))
                {
                    continue;
                }

                _keys[targetIndex] = shiftedKey;

                targetIndex = index;
            }

            _flags[targetIndex] = false;

            return;
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

    public sealed class ConcurrentSet<K> : Set<K>
        where K : System.IEquatable<K>
    {
        private bool _disposed = false;

        private readonly RWMutex _MUTEX = new();

        public ConcurrentSet() { }

        ~ConcurrentSet() => System.Diagnostics.Debug.Assert(false);

        public override void Insert(K key)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _MUTEX.Lock();

            base.Insert(key);

            _MUTEX.Unlock();
        }

        public override void Extract(K key)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _MUTEX.Lock();

            base.Extract(key);

            _MUTEX.Unlock();
        }

        public override bool Contains(K key)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _MUTEX.Rlock();

            bool f = base.Contains(key);

            _MUTEX.Unlock();

            return f;
        }

        public override K[] Flush()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _MUTEX.Lock();

            K[] keys = base.Flush();

            _MUTEX.Unlock();

            return keys;
        }

        public new System.Collections.Generic.IEnumerable<K> GetKeys()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_count >= 0);

            _MUTEX.Rlock();

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

            _MUTEX.Unlock();
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

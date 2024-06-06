
namespace Containers
{

    public class Map<K, T> : System.IDisposable
        where K : System.IEquatable<K>
    {
        private bool _disposed = false;

        protected const int _MIN_LENGTH = 16;
        protected const int _EXPANSION_FACTOR = 2;
        protected const float _LOAD_FACTOR = 0.75F;
        protected const int _C = 5;

        protected bool[] _flags = new bool[_MIN_LENGTH];
        protected K[] _keys = new K[_MIN_LENGTH];
        protected T[] _values = new T[_MIN_LENGTH];
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

        public Map() { }

        ~Map() => System.Diagnostics.Debug.Assert(false);

        private int Hash(K key)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return System.Math.Abs(key.GetHashCode() * _C);
        }

        private void Resize(int newLength)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_values.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_length >= _MIN_LENGTH);
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

        public virtual void Insert(K key, T value)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_values.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_count >= 0);

            int hash = Hash(key);
            for (int i = 0; i < _length; ++i)
            {
                int index = (hash + i) % _length;

                if (_flags[index])
                {
                    /*Console.WriteLine($"_keys[index]: {_keys[index]}, _key: {key} ");*/
                    if (_keys[index].Equals(key))
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }

                    continue;
                }

                _flags[index] = true;
                _keys[index] = key;
                _values[index] = value;
                _count++;

                float factor = (float)_count / (float)_length;
                if (factor >= _LOAD_FACTOR)
                {
                    Resize(_length * _EXPANSION_FACTOR);
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

        public virtual T Extract(K key)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_values.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_count >= 0);

            if (_count == 0)
            {
                System.Diagnostics.Debug.Assert(false);
            }

            T? value = default;

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

                value = _values[index];

                _count--;

                if (_MIN_LENGTH < _length)
                {
                    int reducedLength = _length / _EXPANSION_FACTOR;
                    float factor = (float)_count / (float)reducedLength;
                    if (factor < _LOAD_FACTOR)
                    {
                        _flags[index] = false;
                        Resize(reducedLength);

                        System.Diagnostics.Debug.Assert(value != null);
                        return value;
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
                System.Diagnostics.Debug.Assert(_values[index] != null);
                _values[targetIndex] = _values[index];

                targetIndex = index;
            }

            _flags[targetIndex] = false;

            System.Diagnostics.Debug.Assert(value != null);
            return value;
        }

        public virtual T Lookup(K key)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_values.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_count >= 0);

            if (_count == 0)
            {
                System.Diagnostics.Debug.Assert(false);
            }

            T? value = default;

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

                value = _values[index];
                break;
            }

            System.Diagnostics.Debug.Assert(value != null);
            return value;
        }

        public virtual bool Contains(K key)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_values.Length >= _MIN_LENGTH);
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

        public virtual (K, T)[] Flush()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_values.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_length >= _MIN_LENGTH);
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

            _flags = new bool[_MIN_LENGTH];
            _keys = new K[_MIN_LENGTH];
            _values = new T[_MIN_LENGTH];
            _length = _MIN_LENGTH;
            _count = 0;

            return elements;
        }

        public System.Collections.Generic.IEnumerable<(K, T)> GetElements()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_values.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_length >= _MIN_LENGTH);
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
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_values.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_length >= _MIN_LENGTH);
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
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_values.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_length >= _MIN_LENGTH);
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

        public virtual void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.
            _flags = null;
            _keys = null;
            _values = null;

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }

    public sealed class ConcurrentMap<K, T> : Map<K, T>
        where K : System.IEquatable<K>
    {
        private bool _disposed = false;

        private readonly RWMutex _MUTEX = new();

        public ConcurrentMap() { }

        ~ConcurrentMap() => System.Diagnostics.Debug.Assert(false);

        public override void Insert(K key, T value)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _MUTEX.Lock();

            base.Insert(key, value);

            _MUTEX.Unlock();
        }

        public override T Extract(K key)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _MUTEX.Lock();

            T v = base.Extract(key);

            _MUTEX.Unlock();

            return v;
        }

        public override T Lookup(K key)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _MUTEX.Rlock();

            T v = base.Lookup(key);

            _MUTEX.Unlock();

            return v;
        }

        public override bool Contains(K key)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _MUTEX.Rlock();

            bool f = base.Contains(key);

            _MUTEX.Unlock();

            return f;
        }

        public override (K, T)[] Flush()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _MUTEX.Lock();

            var ret = base.Flush();

            _MUTEX.Unlock();

            return ret;
        }

        public new System.Collections.Generic.IEnumerable<(K, T)> GetElements()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_values.Length >= _MIN_LENGTH);
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

                    yield return (_keys[j], _values[j]);

                    if (++i == _count)
                    {
                        break;
                    }
                }
            }

            _MUTEX.Unlock();
        }

        public new System.Collections.Generic.IEnumerable<K> GetKeys()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_values.Length >= _MIN_LENGTH);
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

        public new System.Collections.Generic.IEnumerable<T> GetValues()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MIN_LENGTH);
            System.Diagnostics.Debug.Assert(_values.Length >= _MIN_LENGTH);
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

                    yield return _values[j];

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

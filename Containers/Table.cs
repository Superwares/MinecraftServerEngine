
namespace Containers
{

    public interface IReadOnlyTable<K, V> where K : struct, System.IEquatable<K>
    {
        public int Count { get; }
        public bool Empty { get; }

        public V Lookup(K key);
        public bool Contains(K key);

        public System.Collections.Generic.IEnumerable<K> GetKeys();
        public System.Collections.Generic.IEnumerable<V> GetValues();

    }

    public class Table<K, V> : System.IDisposable, IReadOnlyTable<K, V>
        where K : struct, System.IEquatable<K>
    {
        private bool _isDisposed = false;

        private const int _MinLength = 16;
        private const int _ExpansionFactor = 2;
        private const float _LoadFactor = 0.75F;
        private const int _C = 5;

        private bool[] _flags = new bool[_MinLength];
        private K[] _keys = new K[_MinLength];
        private V[] _values = new V[_MinLength];
        private int _length = _MinLength;
        private int _count = 0;

        public int Count
        {
            get
            {
                System.Diagnostics.Debug.Assert(!_isDisposed);
                return _count;
            }
        }
        public bool Empty => (Count == 0);

        public Table() 
        { 
            
        }

        ~Table() => System.Diagnostics.Debug.Assert(false);

        private int Hash(K key)
        {
            System.Diagnostics.Debug.Assert(!_isDisposed);

            /*Debug.Assert(key != null);*/
            return System.Math.Abs(key.GetHashCode() * _C);
        }

        private void Resize(int newLength)
        {
            System.Diagnostics.Debug.Assert(!_isDisposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MinLength);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MinLength);
            System.Diagnostics.Debug.Assert(_values.Length >= _MinLength);
            System.Diagnostics.Debug.Assert(_length >= _MinLength);
            System.Diagnostics.Debug.Assert(_count >= 0);

            bool[] oldFlags = _flags;
            K[] oldKeys = _keys;
            V[] oldValues = _values;
            int oldLength = _length;

            bool[] newFlags = new bool[newLength];
            K[] newKeys = new K[newLength];
            V[] newValues = new V[newLength];

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

        public virtual void Insert(K key, V value)
        {
            System.Diagnostics.Debug.Assert(!_isDisposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MinLength);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MinLength);
            System.Diagnostics.Debug.Assert(_values.Length >= _MinLength);
            System.Diagnostics.Debug.Assert(_length >= _MinLength);
            System.Diagnostics.Debug.Assert(_count >= 0);

            int hash = Hash(key);
            for (int i = 0; i < _length; ++i)
            {
                int index = (hash + i) % _length;

                if (_flags[index])
                {
                    /*Console.WriteLine($"_keys[index]: {_keys[index]}, _key: {key} ");*/
                    if (_keys[index].Equals(key))
                        throw new DuplicateKeyException();

                    continue;
                }

                _flags[index] = true;
                _keys[index] = key;
                _values[index] = value;
                _count++;

                float factor = (float)_count / (float)_length;
                if (factor < _LoadFactor)
                    return;

                Resize(_length * _ExpansionFactor);

                return;
            }

            throw new System.NotImplementedException();
        }

        private bool CanShift(int targetIndex, int currentIndex, int originIndex)
        {
            return (targetIndex < currentIndex && currentIndex < originIndex) ||
                (originIndex < targetIndex && targetIndex < currentIndex) ||
                (currentIndex < originIndex && originIndex < targetIndex) ||
                (originIndex == targetIndex);
        }

        public virtual V Extract(K key)
        {
            System.Diagnostics.Debug.Assert(!_isDisposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MinLength);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MinLength);
            System.Diagnostics.Debug.Assert(_values.Length >= _MinLength);
            System.Diagnostics.Debug.Assert(_length >= _MinLength);
            System.Diagnostics.Debug.Assert(_count >= 0);

            if (_count == 0)
                throw new NotFoundException();

            V? value = default;

            int targetIndex = -1, nextI = -1;
            int hash = Hash(key);
            for (int i = 0; i < _length; ++i)
            {
                int index = (hash + i) % _length;

                if (!_flags[index])
                    throw new NotFoundException();

                if (!_keys[index].Equals(key))
                    continue;

                value = _values[index];

                _count--;

                if (_MinLength < _length)
                {
                    int reducedLength = _length / _ExpansionFactor;
                    float factor = (float)_count / (float)reducedLength;
                    if (factor < _LoadFactor)
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

                if (!_flags[index]) break;

                K shiftedKey = _keys[index];
                int originIndex = Hash(shiftedKey) % _length;
                if (!CanShift(targetIndex, index, originIndex))
                    continue;

                _keys[targetIndex] = shiftedKey;
                System.Diagnostics.Debug.Assert(_values[index] != null);
                _values[targetIndex] = _values[index];

                targetIndex = index;
            }

            _flags[targetIndex] = false;

            System.Diagnostics.Debug.Assert(value != null);
            return value;
        }

        public virtual V Lookup(K key)
        {
            System.Diagnostics.Debug.Assert(!_isDisposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MinLength);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MinLength);
            System.Diagnostics.Debug.Assert(_values.Length >= _MinLength);
            System.Diagnostics.Debug.Assert(_length >= _MinLength);
            System.Diagnostics.Debug.Assert(_count >= 0);

            if (_count == 0)
                throw new NotFoundException();

            V? value = default;

            int hash = Hash(key);
            for (int i = 0; i < _length; ++i)
            {
                int index = (hash + i) % _length;

                if (!_flags[index])
                    throw new NotFoundException();

                if (!_keys[index].Equals(key))
                    continue;

                value = _values[index];
                break;
            }

            System.Diagnostics.Debug.Assert(value != null);
            return value;
        }

        public virtual bool Contains(K key)
        {
            System.Diagnostics.Debug.Assert(!_isDisposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MinLength);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MinLength);
            System.Diagnostics.Debug.Assert(_values.Length >= _MinLength);
            System.Diagnostics.Debug.Assert(_length >= _MinLength);
            System.Diagnostics.Debug.Assert(_count >= 0);
            if (_count == 0)
                return false;

            int hash = Hash(key);
            for (int i = 0; i < _length; ++i)
            {
                int index = (hash + i) % _length;

                if (!_flags[index])
                    return false;

                if (!_keys[index].Equals(key))
                    continue;

                return true;
            }

            return false;
        }

        public virtual (K, V)[] Flush()
        {
            System.Diagnostics.Debug.Assert(!_isDisposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MinLength);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MinLength);
            System.Diagnostics.Debug.Assert(_values.Length >= _MinLength);
            System.Diagnostics.Debug.Assert(_length >= _MinLength);
            System.Diagnostics.Debug.Assert(_count >= 0);

            if (_count == 0)
                return [];

            var elements = new (K, V)[_count];

            int i = 0;
            for (int j = 0; j < _length; ++j)
            {
                if (!_flags[j]) continue;

                elements[i++] = (_keys[j], _values[j]);

                if (i == _count) break;
            }

            _flags = new bool[_MinLength];
            _keys = new K[_MinLength];
            _values = new V[_MinLength];
            _length = _MinLength;
            _count = 0;

            return elements;
        }

        public virtual System.Collections.Generic.IEnumerable<(K, V)> GetElements()
        {
            System.Diagnostics.Debug.Assert(!_isDisposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MinLength);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MinLength);
            System.Diagnostics.Debug.Assert(_values.Length >= _MinLength);
            System.Diagnostics.Debug.Assert(_length >= _MinLength);
            System.Diagnostics.Debug.Assert(_count >= 0);

            if (_count == 0)
                yield break;

            int i = 0;
            for (int j = 0; j < _length; ++j)
            {
                if (!_flags[j]) continue;

                yield return (_keys[j], _values[j]);

                if (++i == _count) break;
            }

        }


        public virtual System.Collections.Generic.IEnumerable<K> GetKeys()
        {
            System.Diagnostics.Debug.Assert(!_isDisposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MinLength);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MinLength);
            System.Diagnostics.Debug.Assert(_values.Length >= _MinLength);
            System.Diagnostics.Debug.Assert(_length >= _MinLength);
            System.Diagnostics.Debug.Assert(_count >= 0);

            if (_count == 0)
                yield break;

            int i = 0;
            for (int j = 0; j < _length; ++j)
            {
                if (!_flags[j]) continue;

                yield return _keys[j];

                if (++i == _count) break;
            }

        }

        public virtual System.Collections.Generic.IEnumerable<V> GetValues()
        {
            System.Diagnostics.Debug.Assert(!_isDisposed);

            System.Diagnostics.Debug.Assert(_flags.Length >= _MinLength);
            System.Diagnostics.Debug.Assert(_keys.Length >= _MinLength);
            System.Diagnostics.Debug.Assert(_values.Length >= _MinLength);
            System.Diagnostics.Debug.Assert(_length >= _MinLength);
            System.Diagnostics.Debug.Assert(_count >= 0);

            if (_count == 0)
                yield break;

            int i = 0;
            for (int j = 0; j < _length; ++j)
            {
                if (!_flags[j]) continue;

                yield return _values[j];

                if (++i == _count) break;
            }

        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing == true)
            {
                // Release managed resources.
                _flags = null;
                _keys = null;
                _values = null;
            }

            // Release unmanaged resources.

            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

    }
}

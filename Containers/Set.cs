using System;
using System.Diagnostics;

namespace Containers
{

    public interface IReadOnlySet<K> where K : struct, IEquatable<K>
    {
        public bool Contains(K key);

        public System.Collections.Generic.IEnumerable<K> GetKeys();

    }

    public class Set<K> : IDisposable, IReadOnlySet<K>
        where K : struct, IEquatable<K>
    {
        private bool _isDisposed = false;

        private const int _MinLength = 16;
        private const int _ExpansionFactor = 2;
        private const float _LoadFactor = 0.75F;
        private const int _C = 5;

        private bool[] _flags = new bool[_MinLength];
        private K[] _keys = new K[_MinLength];
        private int _length = _MinLength;
        private int _count = 0;

        public int Count
        {
            get
            {
                Debug.Assert(!_isDisposed);
                return _count;
            }
        }
        public bool Empty => (Count == 0);

        public Set() { }

        ~Set() => Dispose(false);

        private int Hash(K key)
        {
            Debug.Assert(!_isDisposed);

            /*Debug.Assert(key != null);*/
            return Math.Abs(key.GetHashCode() * _C);
        }

        private void Resize(int newLength)
        {
            Debug.Assert(!_isDisposed);

            Debug.Assert(_flags.Length >= _MinLength);
            Debug.Assert(_keys.Length >= _MinLength);
            Debug.Assert(_length >= _MinLength);
            Debug.Assert(_count >= 0);

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
            Debug.Assert(!_isDisposed);

            Debug.Assert(_flags.Length >= _MinLength);
            Debug.Assert(_keys.Length >= _MinLength);
            Debug.Assert(_length >= _MinLength);
            Debug.Assert(_count >= 0);

            int hash = Hash(key);
            for (int i = 0; i < _length; ++i)
            {
                int index = (hash + i) % _length;
                K currentKey = _keys[index];

                if (_flags[index])
                {
                    if (key.Equals(currentKey))
                        throw new DuplicateKeyException();

                    continue;
                }

                _flags[index] = true;
                _keys[index] = key;
                _count++;

                float factor = (float)_count / (float)_length;
                if (factor < _LoadFactor)
                    return;

                Resize(_length * _ExpansionFactor);

                return;
            }

            throw new NotImplementedException();
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
            Debug.Assert(!_isDisposed);

            Debug.Assert(_flags.Length >= _MinLength);
            Debug.Assert(_keys.Length >= _MinLength);
            Debug.Assert(_length >= _MinLength);
            Debug.Assert(_count >= 0);

            if (_count == 0)
                throw new NotFoundException();

            int targetIndex = -1, nextI = -1;
            int hash = Hash(key);
            for (int i = 0; i < _length; ++i)
            {
                int index = (hash + i) % _length;

                if (!_flags[index])
                    throw new NotFoundException();

                if (!_keys[index].Equals(key))
                    continue;

                _count--;

                if (_MinLength < _length)
                {
                    int reducedLength = _length / _ExpansionFactor;
                    float factor = (float)_count / (float)reducedLength;
                    if (factor < _LoadFactor)
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

            Debug.Assert(targetIndex >= 0);
            Debug.Assert(nextI > 0);
            for (int i = nextI; i < _length; ++i)
            {
                int index = (hash + i) % _length;

                if (!_flags[index]) break;

                K shiftedKey = _keys[index];
                int originIndex = Hash(shiftedKey) % _length;
                if (!CanShift(targetIndex, index, originIndex))
                    continue;

                _keys[targetIndex] = shiftedKey;

                targetIndex = index;
            }

            _flags[targetIndex] = false;

            return;
        }

        public virtual bool Contains(K key)
        {
            Debug.Assert(!_isDisposed);

            Debug.Assert(_flags.Length >= _MinLength);
            Debug.Assert(_keys.Length >= _MinLength);
            Debug.Assert(_length >= _MinLength);
            Debug.Assert(_count >= 0);
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

        public virtual K[] Flush()
        {
            Debug.Assert(!_isDisposed);

            Debug.Assert(_flags.Length >= _MinLength);
            Debug.Assert(_keys.Length >= _MinLength);
            Debug.Assert(_length >= _MinLength);
            Debug.Assert(_count >= 0);

            if (_count == 0)
                return [];

            var keys = new K[_count];

            int i = 0;
            for (int j = 0; j < _length; ++j)
            {
                if (!_flags[j]) continue;

                keys[i++] = _keys[j];

                if (i == _count) break;
            }

            _flags = new bool[_MinLength];
            _keys = new K[_MinLength];
            _length = _MinLength;
            _count = 0;

            return keys;
        }

        public virtual System.Collections.Generic.IEnumerable<K> GetKeys()
        {
            Debug.Assert(!_isDisposed);

            Debug.Assert(_flags.Length >= _MinLength);
            Debug.Assert(_keys.Length >= _MinLength);
            Debug.Assert(_length >= _MinLength);
            Debug.Assert(_count >= 0);

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

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            Debug.Assert(_flags.Length == _MinLength);
            Debug.Assert(_keys.Length == _MinLength);
            Debug.Assert(_length == _MinLength);
            Debug.Assert(_count == 0);

            if (disposing == true)
            {
                // Release managed resources.
                _flags = null;
                _keys = null;
            }

            // Release unmanaged resources.

            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Close() => Dispose();

    }
}

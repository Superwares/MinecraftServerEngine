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
        private bool _disposed = false;

        private const int _MIN_LENGTH = 16;
        private const int _EXPENSION_FACTOR = 2;
        private const float _LOAD_FACTOR = 0.75F;
        private const int _C = 5;

        private bool[] _flags = new bool[_MIN_LENGTH];
        private K[] _keys = new K[_MIN_LENGTH];
        private int _length = _MIN_LENGTH;
        private int _count = 0;

        public int Count
        {
            get
            {
                Debug.Assert(!_disposed);
                return _count;
            }
        }
        public bool Empty => (Count == 0);

        public Set() { }

        ~Set() => Dispose(false);

        private int Hash(K key)
        {
            Debug.Assert(!_disposed);

            /*Debug.Assert(key != null);*/
            return Math.Abs(key.GetHashCode() * _C);
        }

        private void Resize(int newLength)
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_flags.Length >= _MIN_LENGTH);
            Debug.Assert(_keys.Length >= _MIN_LENGTH);
            Debug.Assert(_length >= _MIN_LENGTH);
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
            Debug.Assert(!_disposed);

            Debug.Assert(_flags.Length >= _MIN_LENGTH);
            Debug.Assert(_keys.Length >= _MIN_LENGTH);
            Debug.Assert(_length >= _MIN_LENGTH);
            Debug.Assert(_count >= 0);

            int hash = Hash(key);
            for (int i = 0; i < _length; ++i)
            {
                int index = (hash + i) % _length;

                if (_flags[index])
                {
                    if (_keys[index].Equals(key))
                        throw new DuplicateKeyException();

                    continue;
                }

                _flags[index] = true;
                _keys[index] = key;
                _count++;

                float factor = (float)_count / (float)_length;
                if (factor < _LOAD_FACTOR)
                    return;

                Resize(_length * _EXPENSION_FACTOR);

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
            Debug.Assert(!_disposed);

            Debug.Assert(_flags.Length >= _MIN_LENGTH);
            Debug.Assert(_keys.Length >= _MIN_LENGTH);
            Debug.Assert(_length >= _MIN_LENGTH);
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
            Debug.Assert(!_disposed);

            Debug.Assert(_flags.Length >= _MIN_LENGTH);
            Debug.Assert(_keys.Length >= _MIN_LENGTH);
            Debug.Assert(_length >= _MIN_LENGTH);
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

        public void Reset()
        {
            Debug.Assert(!_disposed);

            _flags = new bool[_MIN_LENGTH];
            _keys = new K[_MIN_LENGTH];
            _length = _MIN_LENGTH;
            _count = 0;
        }

        public virtual K[] Flush()
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_flags.Length >= _MIN_LENGTH);
            Debug.Assert(_keys.Length >= _MIN_LENGTH);
            Debug.Assert(_length >= _MIN_LENGTH);
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

            _flags = new bool[_MIN_LENGTH];
            _keys = new K[_MIN_LENGTH];
            _length = _MIN_LENGTH;
            _count = 0;

            return keys;
        }

        public virtual System.Collections.Generic.IEnumerable<K> GetKeys()
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_flags.Length >= _MIN_LENGTH);
            Debug.Assert(_keys.Length >= _MIN_LENGTH);
            Debug.Assert(_length >= _MIN_LENGTH);
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
            if (_disposed) return;

            Debug.Assert(_flags.Length == _MIN_LENGTH);
            Debug.Assert(_keys.Length == _MIN_LENGTH);
            Debug.Assert(_length == _MIN_LENGTH);
            Debug.Assert(_count == 0);

            if (disposing == true)
            {
                // Release managed resources.
                _flags = null;
                _keys = null;
            }

            // Release unmanaged resources.

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Close() => Dispose();

    }
}

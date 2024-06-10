﻿
using Threading;

namespace Containers
{

    public class Table<K, T> : System.IDisposable
        where K : notnull, System.IEquatable<K>
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

        public Table() { }

        ~Table() => System.Diagnostics.Debug.Assert(false);

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

            int j = -1;

            int hash = Hash(key);
            for (int i = 0; i < _length; ++i)
            {
                int k = (hash + i) % _length;

                if (_flags[k])
                {
                    /*Console.WriteLine($"_keys[index]: {_keys[index]}, _key: {key} ");*/
                    if (_keys[k].Equals(key))
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }

                    continue;
                }

                j = k;

                break;
            }

            System.Diagnostics.Debug.Assert(j >= 0);
            _flags[j] = true;
            _keys[j] = key;
            _values[j] = value;
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

            int j = -1;

            int hash = Hash(key);

            int index = -1;
            int i;
            for (i = 0; i < _length; ++i)
            {
                index = (hash + i) % _length;

                if (!_flags[index])
                {
                    System.Diagnostics.Debug.Assert(false);
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
                    T v = _values[j];
                    _flags[j] = false;

                    Resize(lenReduced);

                    return v;
                }
            }

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

            int j = -1;

            int hash = Hash(key);
            for (int i = 0; i < _length; ++i)
            {
                int k = (hash + i) % _length;

                if (!_flags[k])
                {
                    System.Diagnostics.Debug.Assert(false);
                }

                if (!_keys[k].Equals(key))
                {
                    continue;
                }

                j = k;

                break;
            }

            System.Diagnostics.Debug.Assert(j >= 0);
            return _values[j];
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

    public sealed class ConcurrentTable<K, T> : Table<K, T> 
        where K : System.IEquatable<K>
    {
        private bool _disposed = false;

        private readonly RWMutex _MUTEX = new();

        public ConcurrentTable() { }

        ~ConcurrentTable() => System.Diagnostics.Debug.Assert(false);

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

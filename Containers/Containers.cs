using System;
using System.Diagnostics;

namespace Containers
{
    public class ContainerException : Exception
    {
        public ContainerException(string message) : base(message) { }

    }

    public class EmptyQueueException : ContainerException
    {
        public EmptyQueueException() : base("No elements in the queue.") { }

    }

    public class DuplicateKeyException : ContainerException
    {
        // TODO: Set Message
        public DuplicateKeyException() : base("") { }
    }

    public class NotFoundException : ContainerException
    {
        // TODO: Set Message
        public NotFoundException() : base("") { }
    }

    public sealed class NumList : IDisposable
    {
        private bool _disposed = false;

        private readonly int _MIN = 0;
        private readonly int _MAX = int.MaxValue;

        private class Node(int from, int to)
        {
            public int from = from, to = to;
            public Node? next = null;

        }

        private Node _first;

        public NumList()
        {
            _first = new(_MIN, _MAX);
        }

        ~NumList()
        {
            Dispose(false);
        }

        public int Alloc()
        {
            Debug.Assert(!_disposed);

            int from = _first.from, to = _first.to;
            Debug.Assert(from <= to);

            int number;

            if (from < to)
            {
                number = from++;
                _first.from = from;
            }
            else
            {
                Debug.Assert(from == to);

                number = from;
                Node? next = _first.next;
                Debug.Assert(next != null);
                _first = next;
            }

            return number;
        }

        public void Dealloc(int number)
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_first != null);

            Node? prev;
            Node? current = _first;

            int from = current.from,
                to = current.to;
            Debug.Assert(from <= to);
            Debug.Assert(!(from <= number && number <= to));

            if (number < from)
            {
                if (from > 0)
                {
                    if (number == (from - 1))
                    {
                        current.from--;
                    }
                    else
                    {
                        prev = new(number, number);
                        prev.next = current;
                        _first = prev;
                    }
                }
                else
                    Debug.Assert(false);
            }
            else
            {
                do
                {
                    Debug.Assert(current.from <= current.to);
                    Debug.Assert(!(current.from <= number && number <= current.to));
                    Debug.Assert(current.to < number);

                    prev = current;
                    current = prev.next;
                    Debug.Assert(current != null);
                }
                while (!(prev.to < number && number < current.from));

                to = prev.to;
                from = current.from;

                if ((to + 1) == (from - 1))
                {
                    Debug.Assert((to + 1) == number);
                    prev.to = current.to;
                    prev.next = current.next;
                }
                else if ((to + 1) < number && number < (from - 1))
                {
                    Node between = new(number, number);
                    between.next = current;
                    prev.next = between;
                }
                else if ((to + 1) == number)
                {
                    Debug.Assert((to + 1) + 1 < from);
                    prev.to++;
                }
                else
                {
                    Debug.Assert(to < (from - 1) - 1);
                    current.from--;
                }
            }

        }

        private void Dispose(bool disposing)
        {
            if (_disposed == true) return;

            Debug.Assert(_first != null);
            Debug.Assert(_first.next == null);
            Debug.Assert(_first.from == _MIN);
            Debug.Assert(_first.to == _MAX);

            if (disposing == true)
            {
                // managed objects
                _first = null;
            }

            // Release unmanaged objects

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Close()
        {
            Dispose();
        }
    }

    public class Queue<T> : IDisposable
    {
        private class Node(T value)
        {
            private T _value = value;
            public T Value => _value;

            public Node? NextNode = null;

        }

        private bool _disposed = false;

        private Node? _outNode = null, _inNode = null;

        private int _count = 0;
        public int Count
        {
            get
            {
                Debug.Assert(!_disposed);

                return _count;
            }
        }

        public bool Empty => (_count == 0);

        public Queue() { }

        ~Queue()
        {
            Dispose(false);
        }

        public void Enqueue(T value)
        {
            Debug.Assert(!_disposed);

            Node newNode = new(value);

            if (_count == 0)
            {
                Debug.Assert(_outNode == null);
                Debug.Assert(_inNode == null);

                _outNode = _inNode = newNode;
            }
            else
            {
                Debug.Assert(_outNode != null);
                Debug.Assert(_inNode != null);

                _inNode.NextNode = newNode;
                _inNode = newNode;
            }

            _count++;
        }

        public T Dequeue()
        {
            Debug.Assert(!_disposed);

            if (_count == 0)
                throw new EmptyQueueException();

            Debug.Assert(_inNode != null);
            Debug.Assert(_outNode != null);
            T value = _outNode.Value;

            if (_count == 1)
            {
                _inNode = _outNode = null;
            }
            else
            {
                Debug.Assert(_count > 1);
                _outNode = _outNode.NextNode;
            }

            _count--;

            return value;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            Debug.Assert(Count == 0);
            Debug.Assert(_outNode == null);
            Debug.Assert(_inNode == null);

            if (disposing == true)
            {
                // Release managed resources.
                _outNode = _inNode = null;
            }

            // Release unmanaged resources.

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }

    public sealed class ConcurrentQueue<T> : Queue<T>
    {
        private readonly object _SharedResource = new();

        private bool _disposed = false;

        public ConcurrentQueue() { }

        ~ConcurrentQueue() => Dispose(false);

        public new void Enqueue(T value)
        {
            Debug.Assert(!_disposed);

            lock (_SharedResource)
            {
                base.Enqueue(value);
            }
        }

        public new T Dequeue()
        {
            Debug.Assert(!_disposed);

            lock (_SharedResource)
            {
                return base.Dequeue();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing == true)
                {
                    // Release managed resources.
                }

                // Release unmanaged resources.

                _disposed = true;
            }

            base.Dispose(disposing);
        }

    }

    public class Table<K, V> : IDisposable where K : struct, IEquatable<K>
    {
        private static readonly int _MinLength = 16;
        private static readonly int _ExpansionFactor = 2;
        private static readonly float _LoadFactor = 0.75F;
        private static readonly int _C = 5;

        private bool[] _flags = new bool[_MinLength];
        private K[] _keys = new K[_MinLength];
        private V[] _values = new V[_MinLength];
        private int _length = _MinLength;
        private int _count = 0;
        public int Count
        {
            get
            {
                Debug.Assert(!_disposed);

                return _count;
            }
        }

        private bool _disposed = false;

        public Table()
        {

        }

        ~Table() => Dispose(false);

        private int Hash(K key)
        {
            Debug.Assert(!_disposed);

            /*Debug.Assert(key != null);*/
            return key.GetHashCode() * _C;
        }

        private void Resize(int newLength)
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_flags.Length >= _MinLength);
            Debug.Assert(_keys.Length >= _MinLength);
            Debug.Assert(_values.Length >= _MinLength);
            Debug.Assert(_length >= _MinLength);
            Debug.Assert(_count > 0);

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

        public void Insert(K key, V value)
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_flags.Length >= _MinLength);
            Debug.Assert(_keys.Length >= _MinLength);
            Debug.Assert(_values.Length >= _MinLength);
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
                _values[index] = value;
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

        public V Extract(K key)
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_flags.Length >= _MinLength);
            Debug.Assert(_keys.Length >= _MinLength);
            Debug.Assert(_values.Length >= _MinLength);
            Debug.Assert(_length >= _MinLength);
            Debug.Assert(_count > 0);

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

                        Debug.Assert(value != null);
                        return value;
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
                Debug.Assert(_values[index] != null);
                _values[targetIndex] = _values[index];

                targetIndex = index;
            }

            _flags[targetIndex] = false;

            Debug.Assert(value != null);
            return value;
        }

        public V Lookup(K key)
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_flags.Length >= _MinLength);
            Debug.Assert(_keys.Length >= _MinLength);
            Debug.Assert(_values.Length >= _MinLength);
            Debug.Assert(_length >= _MinLength);
            Debug.Assert(_count > 0);

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

            Debug.Assert(value != null);
            return value;
        }

        public bool Contains(K key)
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_flags.Length >= _MinLength);
            Debug.Assert(_keys.Length >= _MinLength);
            Debug.Assert(_values.Length >= _MinLength);
            Debug.Assert(_length >= _MinLength);
            Debug.Assert(_count > 0);

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

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            Debug.Assert(_flags.Length == _MinLength);
            Debug.Assert(_keys.Length == _MinLength);
            Debug.Assert(_values.Length == _MinLength);
            Debug.Assert(_length == _MinLength);
            Debug.Assert(_count == 0);

            if (disposing == true)
            {
                // Release managed resources.
                _flags = null;
                _keys = null;
                _values = null;
            }

            // Release unmanaged resources.

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }

}

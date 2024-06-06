
using Threading;

namespace Containers
{

    public class Queue<T> : System.IDisposable
    {
        protected class Node(T value)
        {
            private T _value = value;
            public T Value => _value;

            public Node? NextNode = null;

        }

        private bool _disposed = false;

        protected Node? _outNode = null, _inNode = null;

        private int _count = 0;
        public int Count
        {
            get
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                return _count;
            }
        }

        public bool Empty => (_count == 0);

        public Queue() { }

        ~Queue() => System.Diagnostics.Debug.Assert(false);

        public virtual void Enqueue(T value)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Node newNode = new(value);

            if (_count == 0)
            {
                System.Diagnostics.Debug.Assert(_outNode == null);
                System.Diagnostics.Debug.Assert(_inNode == null);

                _outNode = _inNode = newNode;
            }
            else
            {
                System.Diagnostics.Debug.Assert(_outNode != null);
                System.Diagnostics.Debug.Assert(_inNode != null);

                _inNode.NextNode = newNode;
                _inNode = newNode;
            }

            _count++;
        }

        public virtual T Dequeue()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (_count == 0)
            {
                System.Diagnostics.Debug.Assert(false);
            }

            System.Diagnostics.Debug.Assert(_inNode != null);
            System.Diagnostics.Debug.Assert(_outNode != null);
            T value = _outNode.Value;

            if (_count == 1)
            {
                _inNode = _outNode = null;
            }
            else
            {
                System.Diagnostics.Debug.Assert(_count > 1);
                _outNode = _outNode.NextNode;
            }

            _count--;

            return value;
        }

        public virtual T[] Flush()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (_count == 0)
            {
                return [];
            }

            System.Diagnostics.Debug.Assert(_inNode != null);
            System.Diagnostics.Debug.Assert(_outNode != null);

            T[] values = new T[_count];

            Node? node = _outNode;

            for (int i = 0; i < _count; ++i)
            {
                System.Diagnostics.Debug.Assert(node != null);
                values[i] = node.Value;
                node = node.NextNode;
            }
            System.Diagnostics.Debug.Assert(node == null);

            _inNode = null;
            _outNode = null;
            _count = 0;

            return values;
        }

        public System.Collections.Generic.IEnumerable<T> GetValues()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (_count == 0)
            {
                yield break;
            }

            System.Diagnostics.Debug.Assert(_inNode != null);
            System.Diagnostics.Debug.Assert(_outNode != null);

            Node? current = _outNode;
            do
            {
                System.Diagnostics.Debug.Assert(current != null);

                yield return current.Value;

                current = current.NextNode;
            } while (current != null);

        }

        public virtual void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.
            _outNode = _inNode = null;

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }

    public sealed class ConcurrentQueue<T> : Queue<T>
    {
        private bool _disposed = false;

        private readonly RWMutex _MUTEX = new();

        public ConcurrentQueue() { }

        ~ConcurrentQueue() => System.Diagnostics.Debug.Assert(false);

        public override void Enqueue(T value)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _MUTEX.Lock();

            base.Enqueue(value);

            _MUTEX.Unlock();
        }

        public override T Dequeue()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _MUTEX.Lock();

            T v = base.Dequeue();

            _MUTEX.Unlock();

            return v;
        }

        public bool Dequeue(ref T? v)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            bool f;

            _MUTEX.Lock();

            if (Empty)
            {
                f = false;
            }
            else
            {
                v = base.Dequeue();

                f = true;
            }

            _MUTEX.Unlock();

            return f;
        }

        public override T[] Flush()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _MUTEX.Lock();

            T[] arr = base.Flush();

            _MUTEX.Unlock();

            return arr;
        }

        public new System.Collections.Generic.IEnumerable<T> GetValues()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _MUTEX.Rlock();

            if (!Empty)
            {
                System.Diagnostics.Debug.Assert(_inNode != null);
                System.Diagnostics.Debug.Assert(_outNode != null);

                Node? current = _outNode;
                do
                {
                    System.Diagnostics.Debug.Assert(current != null);

                    yield return current.Value;

                    current = current.NextNode;
                } while (current != null);
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

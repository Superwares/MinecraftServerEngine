using System;
using System.Diagnostics;

namespace Containers
{
    public interface IReadOnlyQueue<T>
    {
        public System.Collections.Generic.IEnumerable<T> GetValues();
    }

    public class Queue<T> : IDisposable, IReadOnlyQueue<T>
    {
        private class Node(T value)
        {
            private T _value = value;
            public T Value => _value;

            public Node? NextNode = null;

        }

        private bool _isDisposed = false;

        private Node? _outNode = null, _inNode = null;

        private int _count = 0;
        public int Count
        {
            get
            {
                Debug.Assert(!_isDisposed);

                return _count;
            }
        }

        public bool Empty => (_count == 0);

        public Queue() { }

        ~Queue()
        {
            Dispose(false);
        }

        public virtual void Enqueue(T value)
        {
            Debug.Assert(!_isDisposed);

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

        public virtual T Dequeue()
        {
            Debug.Assert(!_isDisposed);

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

        public virtual T[] Flush()
        {
            Debug.Assert(!_isDisposed);

            if (_count == 0)
                return [];

            Debug.Assert(_inNode != null);
            Debug.Assert(_outNode != null);

            T[] values = new T[_count];

            Node? node = _outNode;

            for (int i = 0; i < _count; ++i)
            {
                Debug.Assert(node != null);
                values[i] = node.Value;
                node = node.NextNode;
            }
            Debug.Assert(node == null);

            _inNode = null;
            _outNode = null;
            _count = 0;

            return values;
        }

        public virtual System.Collections.Generic.IEnumerable<T> GetValues()
        {
            Debug.Assert(!_isDisposed);

            if (_count == 0)
                yield break;

            Debug.Assert(_inNode != null);
            Debug.Assert(_outNode != null);

            Node? current = _outNode;
            do
            {
                Debug.Assert(current != null);

                yield return current.Value;
                current = current.NextNode;
            } while (current != null);

        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            Debug.Assert(Count == 0);
            Debug.Assert(_outNode == null);
            Debug.Assert(_inNode == null);

            if (disposing == true)
            {
                // Release managed resources.
                _outNode = _inNode = null;
            }

            // Release unmanaged resources.

            _isDisposed = true;
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

    // TODO: Make concurrency mechanisms using rwmutex.
    public sealed class ConcurrentQueue<T> : Queue<T>
    {
        private readonly object _SharedResource = new();

        private bool _isDisposed = false;

        public ConcurrentQueue() { }

        ~ConcurrentQueue() => Dispose(false);

        public override void Enqueue(T value)
        {
            Debug.Assert(!_isDisposed);

            lock (_SharedResource)
            {
                base.Enqueue(value);
            }
        }

        public override T Dequeue()
        {
            Debug.Assert(!_isDisposed);

            lock (_SharedResource)
            {
                return base.Dequeue();
            }
        }

        public override T[] Flush()
        {
            Debug.Assert(!_isDisposed);

            lock (_SharedResource)
            {
                return base.Flush();
            }
        }

        public override System.Collections.Generic.IEnumerable<T> GetValues()
        {
            Debug.Assert(!_isDisposed);


            lock (_SharedResource)
            {
                return base.GetValues();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing == true)
                {
                    // Release managed resources.
                }

                // Release unmanaged resources.

                _isDisposed = true;
            }

            base.Dispose(disposing);
        }

    }
}

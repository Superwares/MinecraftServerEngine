namespace Containers
{
    public interface IReadOnlyQueue<T>
    {
        public System.Collections.Generic.IEnumerable<T> GetValues();
    }

    public class Queue<T> : System.IDisposable, IReadOnlyQueue<T>
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
                throw new EmptyQueueException();

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
                return [];

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

        public virtual System.Collections.Generic.IEnumerable<T> GetValues()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (_count == 0)
                yield break;

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

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

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
            System.GC.SuppressFinalize(this);
        }

    }

    // TODO: Make concurrency mechanisms using rwmutex.
    public sealed class ConcurrentQueue<T> : Queue<T>
    {
        private readonly object _SharedResource = new();

        private bool _disposed = false;

        public ConcurrentQueue() { }

        ~ConcurrentQueue() => Dispose(false);

        public override void Enqueue(T value)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            lock (_SharedResource)
            {
                base.Enqueue(value);
            }
        }

        public override T Dequeue()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            lock (_SharedResource)
            {
                return base.Dequeue();
            }
        }

        public override T[] Flush()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            lock (_SharedResource)
            {
                return base.Flush();
            }
        }

        public override System.Collections.Generic.IEnumerable<T> GetValues()
        {
            System.Diagnostics.Debug.Assert(!_disposed);


            lock (_SharedResource)
            {
                return base.GetValues();
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
}

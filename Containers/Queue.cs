
using Sync;

namespace Containers
{

    public class Queue<T> : System.IDisposable
    {
        protected class Node(T value)
        {
            private T _value = value;
            public T Value => _value;

            public Node NextNode = null;

        }

        private bool _disposed = false;

        protected Node _outNode = null, _inNode = null;

        private int _count = 0;
        public int Count
        {
            get
            {
                if (_disposed == true)
                {
                    throw new System.ObjectDisposedException(GetType().Name);
                }

                return _count;
            }
        }

        public bool Empty => (_count == 0);

        public Queue() { }

        ~Queue()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        public virtual void Enqueue(T value)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            if (value == null)
            {
                throw new System.ArgumentNullException(nameof(value));
            }

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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="EmptyContainerException">The Queue<T> is empty.</exception>
        public virtual T Dequeue()
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            if (_count == 0)
            {
                throw new EmptyContainerException();
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

            --_count;

            System.Diagnostics.Debug.Assert(value != null);
            return value;
        }

        public virtual bool Dequeue(out T value)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            if (_count == 0)
            {
                value = default;
                return false;
            }

            System.Diagnostics.Debug.Assert(_inNode != null);
            System.Diagnostics.Debug.Assert(_outNode != null);
            value = _outNode.Value;

            if (_count == 1)
            {
                _inNode = _outNode = null;
            }
            else
            {
                System.Diagnostics.Debug.Assert(_count > 1);
                _outNode = _outNode.NextNode;
            }

            --_count;

            System.Diagnostics.Debug.Assert(value != null);
            return true;
        }

        public virtual T[] Flush()
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            if (_count == 0)
            {
                return [];
            }

            System.Diagnostics.Debug.Assert(_inNode != null);
            System.Diagnostics.Debug.Assert(_outNode != null);

            T[] values = new T[_count];

            Node node = _outNode;

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
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            if (_count == 0)
            {
                yield break;
            }

            System.Diagnostics.Debug.Assert(_inNode != null);
            System.Diagnostics.Debug.Assert(_outNode != null);

            Node current = _outNode;
            do
            {
                System.Diagnostics.Debug.Assert(current != null);

                yield return current.Value;

                current = current.NextNode;
            } while (current != null);

        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed == false)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing == true)
                {
                    // Dispose managed resources.
                    _outNode = _inNode = null;
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                //CloseHandle(handle);
                //handle = IntPtr.Zero;

                // Note disposing has been done.
                _disposed = true;
            }
        }

    }


}

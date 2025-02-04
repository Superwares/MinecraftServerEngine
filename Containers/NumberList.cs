

namespace Containers
{
    // TODO: Make more generic for numeric data types... int, long, ...
    public class NumberList : System.IDisposable
    {
        private const int Min = 0;
        private const int Max = int.MaxValue;

        private bool _disposed = false;

        private class Node(int from, int to)
        {
            public int from = from, to = to;
            public Node next = null;

        }

        private Node _first = new(Min, Max);



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



        public NumberList() { }

        ~NumberList()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        public virtual int Allocate()
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            int from = _first.from, to = _first.to;
            System.Diagnostics.Debug.Assert(from <= to);

            int num;

            if (from < to)
            {
                num = from++;
                _first.from = from;
            }
            else
            {
                System.Diagnostics.Debug.Assert(from == to);

                num = from;
                Node next = _first.next;
                System.Diagnostics.Debug.Assert(next != null);
                _first = next;
            }

            _count++;

            return num;
        }

        public virtual void Deallocate(int n)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(_first != null);

            Node prev;
            Node current = _first;

            int from = current.from,
                to = current.to;
            System.Diagnostics.Debug.Assert(from <= to);
            System.Diagnostics.Debug.Assert(!(from <= n && n <= to));

            if (n < from)
            {
                if (from > 0)
                {
                    if (n == (from - 1))
                    {
                        current.from--;
                    }
                    else
                    {
                        prev = new Node(n, n);
                        prev.next = current;
                        _first = prev;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                }
            }
            else
            {
                do
                {
                    System.Diagnostics.Debug.Assert(current.from <= current.to);
                    System.Diagnostics.Debug.Assert(!(current.from <= n && n <= current.to));
                    System.Diagnostics.Debug.Assert(current.to < n);

                    prev = current;
                    current = prev.next;
                    System.Diagnostics.Debug.Assert(current != null);
                }
                while (!(prev.to < n && n < current.from));

                to = prev.to;
                from = current.from;

                if ((to + 1) == (from - 1))
                {
                    System.Diagnostics.Debug.Assert((to + 1) == n);
                    prev.to = current.to;
                    prev.next = current.next;
                }
                else if ((to + 1) < n && n < (from - 1))
                {
                    Node between = new(n, n);
                    between.next = current;
                    prev.next = between;
                }
                else if ((to + 1) == n)
                {
                    System.Diagnostics.Debug.Assert((to + 1) + 1 < from);
                    prev.to++;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(to < (from - 1) - 1);
                    current.from--;
                }
            }

            _count--;

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
                    _first = null;
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

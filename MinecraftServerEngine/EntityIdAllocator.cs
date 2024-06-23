using Containers;
using Sync;


namespace MinecraftServerEngine
{

    internal static class EntityIdAllocator
    {
        private class Numlist : System.IDisposable
        {
            private bool _disposed = false;

            private const int Min = 0;
            private const int Max = int.MaxValue;

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
                    System.Diagnostics.Debug.Assert(!_disposed);

                    return _count;
                }
            }
            public bool Empty => (_count == 0);

            public Numlist() { }

            ~Numlist() => System.Diagnostics.Debug.Assert(false);

            public virtual int Alloc()
            {
                System.Diagnostics.Debug.Assert(!_disposed);

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

            public virtual void Dealloc(int n)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

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

            public virtual void Dispose()
            {
                // Assertions.
                System.Diagnostics.Debug.Assert(!_disposed);

                // Release resources.
                _first = null;

                // Finish.
                System.GC.SuppressFinalize(this);
                _disposed = true;
            }
        }

        // TODO: make static destructor to dispose.
        private readonly static Locker Locker = new();  // Disposable
        private readonly static Numlist IdList = new();  // Disposable

        public static int Alloc()
        {
            Locker.Hold();

            int id = IdList.Alloc();

            Locker.Release();

            return id;
        }

        public static void Dealloc(int id)
        {
            Locker.Hold();

            IdList.Dealloc(id);

            Locker.Release();
        }

    }
}

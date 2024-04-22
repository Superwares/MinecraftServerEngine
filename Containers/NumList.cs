using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Containers
{
    public class NumList : IDisposable
    {
        private bool _isDisposed = false;

        private const int _MinNum = 0;
        private const int _MaxNum = int.MaxValue;

        private class Node(int from, int to)
        {
            public int from = from, to = to;
            public Node? next = null;

        }

        private Node _first;

        private int _count = 0;
        public int Count => _count;
        public bool Empty => (_count == 0);

        public NumList()
        {
            _first = new(_MinNum, _MaxNum);
        }

        ~NumList() => Dispose(false);

        public int Alloc()
        {
            Debug.Assert(!_isDisposed);

            int from = _first.from, to = _first.to;
            Debug.Assert(from <= to);

            int num;

            if (from < to)
            {
                num = from++;
                _first.from = from;
            }
            else
            {
                Debug.Assert(from == to);

                num = from;
                Node? next = _first.next;
                Debug.Assert(next != null);
                _first = next;
            }

            _count++;

            return num;
        }

        public void Dealloc(int num)
        {
            Debug.Assert(!_isDisposed);

            Debug.Assert(_first != null);

            Node? prev;
            Node? current = _first;

            int from = current.from,
                to = current.to;
            Debug.Assert(from <= to);
            Debug.Assert(!(from <= num && num <= to));

            if (num < from)
            {
                if (from > 0)
                {
                    if (num == (from - 1))
                    {
                        current.from--;
                    }
                    else
                    {
                        prev = new(num, num);
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
                    Debug.Assert(!(current.from <= num && num <= current.to));
                    Debug.Assert(current.to < num);

                    prev = current;
                    current = prev.next;
                    Debug.Assert(current != null);
                }
                while (!(prev.to < num && num < current.from));

                to = prev.to;
                from = current.from;

                if ((to + 1) == (from - 1))
                {
                    Debug.Assert((to + 1) == num);
                    prev.to = current.to;
                    prev.next = current.next;
                }
                else if ((to + 1) < num && num < (from - 1))
                {
                    Node between = new(num, num);
                    between.next = current;
                    prev.next = between;
                }
                else if ((to + 1) == num)
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

            _count--;

        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed == true) return;

            Debug.Assert(_count == 0);
            Debug.Assert(_first != null);
            Debug.Assert(_first.next == null);
            Debug.Assert(_first.from == _MinNum);
            Debug.Assert(_first.to == _MaxNum);

            if (disposing == true)
            {
                // managed objects
                _first = null;
            }

            // Release unmanaged objects

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

    public class ConcurrentNumList : NumList
    {
        private readonly object _SharedResource = new();

        private bool _isDisposed = false;

        ~ConcurrentNumList() => Dispose(false);

        public new int Alloc()
        {
            Debug.Assert(!_isDisposed);

            lock (_SharedResource)
            {
                return base.Alloc();
            }
        }

        public new void Dealloc(int num)
        {
            Debug.Assert(!_isDisposed);

            lock (_SharedResource)
            {
                base.Dealloc(num);
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

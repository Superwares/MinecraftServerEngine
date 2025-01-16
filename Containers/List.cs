using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Containers
{

    // TODO: Modify to use an array that is allocated and deallocated
    // according to a factor,
    // like a hash table, instead of using a simple array.
    public class List<T> : IEnumerable<T>, System.IDisposable

    {
        private bool _disposed = false;

        private T[] _items = [];

        public List()
        {
        }

        ~List() => System.Diagnostics.Debug.Assert(false);

        public void Append(T item)
        {
            _items = _items.Concat(new T[] { item }).ToArray();
        }

        public T[] ToArray()
        {
            return _items;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (T value in _items)
            {
                yield return value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public virtual void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(_disposed == false);

            // Release resources.
            _items = null;

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }
    }
}

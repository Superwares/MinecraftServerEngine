using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Containers
{

    // TODO: Modify to use an array that is allocated and deallocated
    // according to a factor,
    // like a hash table, instead of using a simple array.
    public class List<T> : IEnumerable<T>
    {
        private T[] _items = [];

        public List()
        {
        }

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



    }
}

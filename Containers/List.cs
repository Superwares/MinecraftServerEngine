
namespace Containers
{

    // TODO: Modify to use an array that is allocated and deallocated
    // according to a factor,
    // like a hash table, instead of using a simple array.
    public class List<T> : IReadOnlyList<T>, System.IDisposable

    {
        private bool _disposed = false;

        private T[] _items = [];

        public int Count
        {
            get
            {
                if (_disposed == true)
                {
                    throw new System.ObjectDisposedException(GetType().Name);
                }

                return _items.Length;
            }
        }

        public T this[int index]
        {
            get
            {
                if (_disposed == true)
                {
                    throw new System.ObjectDisposedException(GetType().Name);
                }

                return _items[index];
            }

            set
            {
                if (_disposed == true)
                {
                    throw new System.ObjectDisposedException(GetType().Name);
                }

                _items[index] = value;
            }
        }

        public List()
        {
        }

        ~List() => System.Diagnostics.Debug.Assert(false);


        public void Append(T item)
        {
            _items = System.Linq.Enumerable.ToArray(
                System.Linq.Enumerable.Concat(_items, new T[] { item }));
        }

        //public T Find(System.Func<T, bool> predicate, T defaultValue = default(T))
        public T Find(System.Func<T, bool> predicate, T defaultValue)
        {
            foreach (T item in _items)
            {
                if (predicate(item))
                {
                    return item;
                }
            }

            return defaultValue;
        }

        public T Extract(System.Func<T, bool> predicate, T defaultValue)
        {
            for (int i = 0; i < _items.Length; i++)
            {
                if (predicate(_items[i]))
                {
                    T item = _items[i];
                    _items = System.Linq.Enumerable.ToArray(
                        System.Linq.Enumerable.Where(_items, (_, index) => index != i));
                    return item;
                }
            }
            return defaultValue;
        }

        public T[] ToArray()
        {
            return _items;
        }

        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            foreach (T value in _items)
            {
                yield return value;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T[] Flush()
        {
            try
            {
                return _items;
            }
            finally
            {
                _items = [];
            }
        }

        public virtual void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_items.Length == 0);

            // Release resources.
            _items = null;

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }


    }
}

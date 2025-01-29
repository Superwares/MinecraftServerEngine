

namespace Containers
{
    public interface IReadOnlyMap<K, T> where K : notnull
    {
        public int Count { get; }
        public bool Empty { get; }

        public T Lookup(K key);
        public bool Contains(K key);

        public System.Collections.Generic.IEnumerable<(K, T)> GetElements();
        public System.Collections.Generic.IEnumerable<K> GetKeys();
        public System.Collections.Generic.IEnumerable<T> GetValues();
    }
}



namespace Containers
{
    public interface IReadOnlyList<T> : System.Collections.Generic.IEnumerable<T>
    {
        public int Length { get; }
        public T this[int index] { get; }

        public T Find(System.Func<T, bool> predicate, T defaultValue);

        public List<T> Clone();
    }
}

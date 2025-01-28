

namespace Containers
{
    public interface IReadOnlyList<T> : System.Collections.Generic.IEnumerable<T>
    {
        public T Find(System.Func<T, bool> predicate, T defaultValue);

    }
}

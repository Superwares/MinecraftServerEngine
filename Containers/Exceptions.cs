using Common;

namespace Containers
{
    public abstract class ContainerException : CommonException
    {
        public ContainerException(string msg) : base(msg) { }
    }

    public sealed class EmptyContainerException : ContainerException
    {
        public EmptyContainerException() : base("The container is empty.") { }
    }

    public sealed class KeyNotFoundException : ContainerException
    {
        public KeyNotFoundException() : base("The key does not exist in the container.") { }
    }

    public sealed class DuplicateKeyException : ContainerException
    {
        public DuplicateKeyException() : base("An element with the same key already exists in container.") { }
    }

}

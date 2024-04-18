using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Containers
{
    public abstract class ContainerException : Exception
    {
        public ContainerException(string message) : base(message) { }

    }

    public class EmptyQueueException : ContainerException
    {
        public EmptyQueueException() : base("No elements in the queue.") { }

    }

    public class DuplicateKeyException : ContainerException
    {
        // TODO: Set Message
        public DuplicateKeyException() : base("") { }
    }

    public class NotFoundException : ContainerException
    {
        // TODO: Set Message
        public NotFoundException() : base("") { }
    }

}

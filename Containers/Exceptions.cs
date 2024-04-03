using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Containers
{
    public class ContainerException : Exception
    {
        public ContainerException(string message) : base(message) { }

    }

    public class EmptyQueueException : ContainerException
    {
        public EmptyQueueException() : base("No elements in the queue.") { }

    }

    public class TimeoutException : ContainerException
    {
        // TODO
        public TimeoutException() : base("TimeoutException") { }

    }

}

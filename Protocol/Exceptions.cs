using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public abstract class ProtocolException : Exception
    {
        public ProtocolException(string message) : base(message) { }
    }

    public abstract class UnexpectedDataException : ProtocolException
    {
        public UnexpectedDataException(string message) : base(message) { }
    }

    public class UnexpectedPacketException : UnexpectedDataException
    {
        public UnexpectedPacketException() : base("Encountered an unexpected packet.") { }
    }

    public class InvalidEncodingException : UnexpectedDataException
    {
        public InvalidEncodingException() : base("Failed to decode the data due to invalid encoding.") { }
    }

    public class BufferOverflowException : UnexpectedDataException
    {
        public BufferOverflowException() : base("Unexpected buffer overflow occurred due to excessive data.") { }
    }

    public class EmptyBufferException : UnexpectedDataException
    {
        public EmptyBufferException() : base("Attempting to read from an empty buffer.") { }
    }

    public class DisconnectedException : ProtocolException
    {
        public DisconnectedException() : base("The connection with the client has been terminated.") { }
    }

    // TODO: It needs the corrent name and message.
    internal class PendingTimeoutException : ProtocolException
    {
        public PendingTimeoutException() : base("Connections are not pending.") { }
    }

    public class DataReadTimeoutException : ProtocolException
    {
        public DataReadTimeoutException() : base("A timeout occurred while attempting to read data.") { }
    }

    public class TryAgainException : ProtocolException
    {
        public TryAgainException() : base("No data is waiting to be received.") { }
    }

}

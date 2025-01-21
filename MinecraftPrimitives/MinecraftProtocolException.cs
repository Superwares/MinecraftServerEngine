using Common;
namespace MinecraftPrimitives
{
    public class MinecraftProtocolException : MinecraftCommonException
    {
        internal MinecraftProtocolException(string msg) : base(msg)
        {
        }
    }

    public class UnexpectedClientBehaviorExecption : MinecraftProtocolException
    {
        public UnexpectedClientBehaviorExecption(string msg) : base(msg) { }
    }

    public class DataRecvTimeoutException : UnexpectedClientBehaviorExecption
    {
        public DataRecvTimeoutException() : base("A timeout occurred while attempting to recv data.") { }
    }

    public class UnexpectedValueException : UnexpectedClientBehaviorExecption
    {
        public UnexpectedValueException(string name) : base($"Value {name} is out of range.") { }
    }

    public class UnexpectedPacketException : UnexpectedClientBehaviorExecption
    {
        public UnexpectedPacketException() : base("Encountered an unexpected packet.") { }
    }

    public class InvalidDecodingException : UnexpectedClientBehaviorExecption
    {
        public InvalidDecodingException() : base("Failed to decode the data due to invalid decoding.") { }

        public InvalidDecodingException(string message) : base(message) { }
    }

    public class InvalidEncodingException : UnexpectedClientBehaviorExecption
    {
        public InvalidEncodingException() : base("Failed to decode the data due to invalid encoding.") { }
    }

    public class BufferOverflowException : UnexpectedClientBehaviorExecption
    {
        public BufferOverflowException() : base("Unexpected buffer overflow occurred due to excessive data.") { }
    }

    public class EmptyBufferException : UnexpectedClientBehaviorExecption
    {
        public EmptyBufferException() : base("Attempting to read from an empty buffer.") { }
    }

    public class TeleportationConfirmTimeoutException : UnexpectedClientBehaviorExecption
    {
        // TODO: Add description.
        public TeleportationConfirmTimeoutException() : base("TeleportationConfirmTimeoutException") { }
    }

    public class KeepAliveTimeoutException : UnexpectedClientBehaviorExecption
    {
        // TODO: Add description.
        public KeepAliveTimeoutException() : base("KeepAliveTimeoutException") { }
    }

    public class DisconnectedClientException : MinecraftProtocolException
    {
        public DisconnectedClientException() : base("The connection with the client has been terminated.") { }
    }

    public class TryAgainException : MinecraftProtocolException
    {
        public TryAgainException() : base("No data is waiting to be received.") { }
    }

}

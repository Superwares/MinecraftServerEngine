using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using Applications;
using Containers;

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

    public class TryAgainException : ProtocolException
    {
        public TryAgainException() : base("No data is waiting to be received.") { }
    }

    internal static class SocketMethods
    {
        public static Socket Establish(ushort port)
        {
            Socket socket = new(SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint localEndPoint = new(IPAddress.Any, port);
            socket.Bind(localEndPoint);
            socket.Listen();

            return socket;
        }

        public static Socket Accept(Socket socket)
        {
            try
            {
                Socket newSocket = socket.Accept();

                return newSocket;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.WouldBlock)
                    throw new NotImplementedException($"Must hanle this exception: {e}");
                else
                    throw new TryAgainException();
            }

            throw new NotImplementedException();
        }

        public static void Poll(Socket socket, TimeSpan span)
        {
            // TODO: check the socket is binding and listening.

            if (IsBlocking(socket) &&
                socket.Poll(span, SelectMode.SelectRead) == false)
            {
                throw new PendingTimeoutException();
            }
        }

        public static void SetBlocking(Socket socket, bool f)
        {
            socket.Blocking = f;
        }

        public static bool IsBlocking(Socket socket)
        {
            return socket.Blocking;
        }

        public static int RecvBytes(
            Socket socket, byte[] buffer, int offset, int size)
        {
            try
            {
                int n = socket.Receive(buffer, offset, size, SocketFlags.None);
                if (n == 0)
                    throw new DisconnectedException();

                Debug.Assert(n <= size);

                return n;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.WouldBlock)
                    throw new TryAgainException();
                else if (e.SocketErrorCode == SocketError.ConnectionAborted ||
                    false)  // Add other Exceptions here!
                    throw new DisconnectedException();
                else
                    throw new NotImplementedException($"Must handle this exception: {e}");

            }

        }

        public static byte RecvByte(Socket socket)
        {
            byte[] buffer = new byte[1];

            int n = RecvBytes(socket, buffer, 0, 1);
            Debug.Assert(n == 1);

            return buffer[0];
        }

        public static void SendByte(Socket socket, byte v)
        {
            int n = socket.Send([v]);
            Debug.Assert(n == 1);
        }

        public static void SendBytes(Socket socket, byte[] data)
        {
            Debug.Assert(data != null);
            int n = socket.Send(data);
            Debug.Assert(n == data.Length);
        }

    }

    // TODO: Check system is little- or big-endian.
    internal sealed class Buffer : IDisposable
    {
        private static readonly int _EXPANSION_FACTOR = 2;
        private static readonly float _LOAD_FACTOR = 0.7F;

        private static readonly byte _SEGMENT_BITS = 0x7F;
        private static readonly byte _CONTINUE_BIT = 0x80;

        /*private static readonly int _BOOL_DATATYPE_SIZE = 1;
        private static readonly int _SBYTE_DATATYPE_SIZE = 1;
        private static readonly int _BYTE_DATATYPE_SIZE = 1;*/
        private static readonly int _SHORT_DATATYPE_SIZE = 2;
        private static readonly int _USHORT_DATATYPE_SIZE = 2;
        private static readonly int _INT_DATATYPE_SIZE = 4;
        private static readonly int _LONG_DATATYPE_SIZE = 8;
        private static readonly int _FLOAT_DATATYPE_SIZE = 4;
        private static readonly int _DOUBLE_DATATYPE_SIZE = 8;
        private static readonly int _GUID_DATATYPE_SIZE = 16;

        private const int _InitDatasize = 16;

        private bool _disposed = false;

        private int _dataSize;
        private byte[] _data;

        private int _first = 0, _last = 0;
        public int Size
        {
            get
            {
                Debug.Assert(_first >= 0);
                Debug.Assert(_last >= _first);
                return _last - _first;
            }
        }

        public bool Empty => (Size == 0);

        public Buffer()
        {
            _dataSize = _InitDatasize;
            _data = new byte[_InitDatasize];
        }

        ~Buffer()
        {
            Dispose(false);
        }

        public bool IsEmpty()
        {
            Debug.Assert(Size >= 0);
            return (Size == 0);
        }

        private byte ExtractByte()
        {
            Debug.Assert(_last >= _first);
            if (_first == _last)
                throw new EmptyBufferException();

            return _data[_first++];
        }

        private byte[] ExtractBytes(int size)
        {
            Debug.Assert(size >= 0);
            if (size == 0)
            {
                return [];
            }
            else if (size == 1)
            {
                return [ExtractByte()];
            }

            Debug.Assert(_last >= _first);

            if (_first + size > _last)
                throw new EmptyBufferException();

            byte[] data = new byte[size];
            Array.Copy(_data, _first, data, 0, size);
            _first += size;

            return data;
        }

        private void ExpandData(int addedSize)
        {
            Debug.Assert(addedSize >= 0);
            if (addedSize == 0)
                return;

            int prevSize = _dataSize,
                newSize = prevSize,
                requiredSize = _last + addedSize;

            if (addedSize > 1)
                while (((float)requiredSize / (float)newSize) >= _LOAD_FACTOR)
                    newSize *= _EXPANSION_FACTOR;
            else
                if (((float)requiredSize / (float)newSize) >= _LOAD_FACTOR)
                newSize *= _EXPANSION_FACTOR;

            Debug.Assert(prevSize <= newSize);

            _dataSize = newSize;

            var newData = new byte[newSize];
            if (Size > 0)
                Array.Copy(_data, _first, newData, _first, Size);
            _data = newData;
        }

        private void InsertByte(byte data)
        {
            Debug.Assert(_last >= _first);

            ExpandData(1);
            _data[_last++] = data;
        }

        private void InsertBytes(byte[] data)
        {
            Debug.Assert(_last >= _first);

            int size = data.Length;
            ExpandData(size);
            Array.Copy(data, 0, _data, _last, size);
            _last += size;
        }

        public bool ReadBool()
        {
            byte data = ExtractByte();
            Debug.Assert(data != 0x01 | data != 0x00);
            return (data > 0x00);
        }

        public sbyte ReadSbyte()
        {
            return (sbyte)ExtractByte();
        }

        public byte ReadByte()
        {
            return (byte)ExtractByte();
        }

        public short ReadShort()
        {
            byte[] data = ExtractBytes(_SHORT_DATATYPE_SIZE);
            Debug.Assert(data.Length == _SHORT_DATATYPE_SIZE);

            return (short)(
                ((short)data[0] << 8) |
                ((short)data[1] << 0));
        }

        public ushort ReadUshort()
        {
            byte[] data = ExtractBytes(_USHORT_DATATYPE_SIZE);
            Debug.Assert(data.Length == _USHORT_DATATYPE_SIZE);

            return (ushort)(
                ((ushort)data[0] << 8) |
                ((ushort)data[1] << 0));
        }

        public int ReadInt()
        {
            byte[] data = ExtractBytes(_INT_DATATYPE_SIZE);
            Debug.Assert(data.Length == _INT_DATATYPE_SIZE);

            return (int)(
                ((int)data[0] << 24) |
                ((int)data[1] << 16) |
                ((int)data[2] << 8) |
                ((int)data[3] << 0));
        }

        public long ReadLong()
        {
            byte[] data = ExtractBytes(_LONG_DATATYPE_SIZE);
            Debug.Assert(data.Length == _LONG_DATATYPE_SIZE);

            return (long)(
                ((long)data[0] << 56) |
                ((long)data[1] << 48) |
                ((long)data[2] << 40) |
                ((long)data[3] << 32) |
                ((long)data[4] << 24) |
                ((long)data[5] << 16) |
                ((long)data[6] << 8) |
                ((long)data[7] << 0));
        }

        public float ReadFLoat()
        {
            byte[] data = ExtractBytes(_FLOAT_DATATYPE_SIZE);
            Debug.Assert(data.Length == _FLOAT_DATATYPE_SIZE);
            Array.Reverse(data);
            return BitConverter.ToSingle(data);
        }

        public double ReadDouble()
        {
            byte[] data = ExtractBytes(_DOUBLE_DATATYPE_SIZE);
            Debug.Assert(data.Length == _DOUBLE_DATATYPE_SIZE);
            Array.Reverse(data);
            return BitConverter.ToDouble(data);
        }

        public int ReadInt(bool decode)
        {
            if (decode == false)
                return ReadInt();

            uint uvalue = 0;
            int position = 0;

            while (true)
            {
                byte data = ExtractByte();

                uvalue |= (uint)(data & _SEGMENT_BITS) << position;
                if ((data & _CONTINUE_BIT) == 0)
                    break;

                position += 7;

                if (position >= 32)
                    throw new InvalidEncodingException();

                Debug.Assert(position > 0);
            }

            return (int)uvalue;
        }

        public long ReadLong(bool decode)
        {
            if (decode == false)
                return ReadLong();

            ulong uvalue = 0;
            int position = 0;

            while (true)
            {
                byte data = ExtractByte();

                uvalue |= (ulong)(data & _SEGMENT_BITS) << position;
                if ((data & _CONTINUE_BIT) == 0)
                    break;

                position += 7;

                if (position >= 64)
                    throw new InvalidEncodingException();

                Debug.Assert(position > 0);
            }

            return (long)uvalue;
        }

        public string ReadString()
        {
            int size = ReadInt(true);
            Debug.Assert(size >= 0);

            byte[] data = ExtractBytes(size);
            return Encoding.UTF8.GetString(data);
        }

        public Guid ReadGuid()
        {
            byte[] data = ExtractBytes(_GUID_DATATYPE_SIZE);
            Debug.Assert(data.Length == _GUID_DATATYPE_SIZE);

            return new Guid(data);
        }

        public void WriteBool(bool value)
        {
            if (value == true)
                InsertByte(1);
            else
                InsertByte(0);
        }

        public void WriteSbyte(sbyte value)
        {
            InsertByte((byte)value);
        }

        public void WriteByte(byte value)
        {
            InsertByte((byte)value);
        }

        public void WriteShort(short value)
        {
            byte[] data = BitConverter.GetBytes(value);
            Debug.Assert(data.Length == _SHORT_DATATYPE_SIZE);
            Array.Reverse(data);
            InsertBytes(data);
        }

        public void WriteUshort(ushort value)
        {
            byte[] data = BitConverter.GetBytes(value);
            Debug.Assert(data.Length == _USHORT_DATATYPE_SIZE);
            Array.Reverse(data);
            InsertBytes(data);
        }

        public void WriteInt(int value)
        {
            byte[] data = BitConverter.GetBytes(value);
            Debug.Assert(data.Length == _INT_DATATYPE_SIZE);
            Array.Reverse(data);
            InsertBytes(data);
        }

        public void WriteLong(long value)
        {
            byte[] data = BitConverter.GetBytes(value);
            Debug.Assert(data.Length == _LONG_DATATYPE_SIZE);
            Array.Reverse(data);
            InsertBytes(data);
        }

        public void WriteFloat(float value)
        {
            byte[] data = BitConverter.GetBytes(value);
            Debug.Assert(data.Length == _FLOAT_DATATYPE_SIZE);
            Array.Reverse(data);
            InsertBytes(data);
        }

        public void WriteDouble(double value)
        {
            byte[] data = BitConverter.GetBytes(value);
            Debug.Assert(data.Length == _DOUBLE_DATATYPE_SIZE);
            Array.Reverse(data);
            InsertBytes(data);
        }

        public void WriteInt(int value, bool encode)
        {
            if (encode == false)
            {
                WriteInt(value);
                return;
            }

            uint uvalue = (uint)value;

            while (true)
            {
                if ((uvalue & ~_SEGMENT_BITS) == 0)
                {
                    InsertByte((byte)uvalue);
                    break;
                }

                InsertByte((byte)((uvalue & _SEGMENT_BITS) | _CONTINUE_BIT));
                uvalue >>= 7;
            }

        }

        public void WriteLong(long value, bool encode)
        {
            if (encode == false)
            {
                WriteLong(value);
                return;
            }

            uint uvalue = (uint)value;
            while (true)
            {
                if ((uvalue & ~_SEGMENT_BITS) == 0)
                {
                    InsertByte((byte)uvalue);
                    break;
                }

                InsertByte((byte)((uvalue & _SEGMENT_BITS) | _CONTINUE_BIT));
                uvalue >>= 7;
            }
        }

        public void WriteString(string value)
        {
            int size = value.Length;
            WriteInt(size, true);

            byte[] data = Encoding.UTF8.GetBytes(value);
            InsertBytes(data);
        }

        public void WriteGuid(Guid value)
        {
            byte[] data = value.ToByteArray();
            Debug.Assert(data.Length == _GUID_DATATYPE_SIZE);
            InsertBytes(data);
        }

        internal byte[] ReadData()
        {
            return ExtractBytes(Size);
        }

        internal void WriteData(byte[] data)
        {
            InsertBytes(data);
        }

        public void Flush()
        {
            Debug.Assert(_dataSize >= _InitDatasize);
            if (Size == 0)
                return;

            Debug.Assert(_last >= _first);
            _first = _last;
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            Debug.Assert(Size == 0);

            if (disposing)
            {
                // Release managed resources.
                _data = null;
            }

            // Release unmanaged resources.

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }

    public abstract class Packet
    {
        protected static readonly string _MinecraftVersion = "1.12.2";
        protected static readonly int _ProtocolVersion = 340;

        public enum States
        {
            Handshaking,
            Status,
            Login,
            Playing,
        }

        public enum WhereBound
        {
            Serverbound,
            Clientbound,
        }

        public abstract States State { get; }
        public abstract WhereBound BoundTo { get; }

        public readonly int Id;

        public Packet(int id) { Id = id; }

        internal abstract void WriteData(Buffer buffer);

        internal void Write(Buffer buffer)
        {
            buffer.WriteInt(Id, true);
            WriteData(buffer);
        }

    }

    internal abstract class HandshakingPacket(int id) : Packet(id)
    {
        public override States State => States.Handshaking;

    }

    internal abstract class StatusPacket(int id) : Packet(id)
    {
        public override States State => States.Status;

    }

    internal abstract class LoginPacket(int id) : Packet(id)
    {
        public override States State => States.Login;

    }

    public abstract class PlayingPacket(int id) : Packet(id)
    {
        public override States State => States.Playing;
    }

    internal abstract class ServerboundHandshakingPacket(int id) : HandshakingPacket(id)
    {
        public static readonly int SetProtocolPacketId = 0x00;

        public override WhereBound BoundTo { get { return WhereBound.Serverbound; } }

    }

    internal abstract class ClientboundStatusPacket(int id) : StatusPacket(id)
    {
        public static readonly int ResponsePacketId = 0x00;
        public static readonly int PongPacketId = 0x01;

        public override WhereBound BoundTo { get { return WhereBound.Clientbound; } }

    }

    internal abstract class ServerboundStatusPacket(int id) : StatusPacket(id)
    {
        public static readonly int RequestPacketId = 0x00;
        public static readonly int PingPacketId = 0x01;

        public override WhereBound BoundTo { get { return WhereBound.Serverbound; } }

    }

    internal abstract class ClientboundLoginPacket(int id) : LoginPacket(id)
    {
        public static readonly int DisconnectPacketId = 0x00;
        public static readonly int EncryptionRequestPacketId = 0x01;
        public static readonly int LoginSuccessPacketId = 0x02;
        public static readonly int SetCompressionPacketId = 0x03;

        public override WhereBound BoundTo => WhereBound.Clientbound;

    }

    internal abstract class ServerboundLoginPacket(int id) : LoginPacket(id)
    {
        public static readonly int StartLoginPacketId = 0x00;
        public static readonly int EncryptionResponsePacketId = 0x01;

        public override WhereBound BoundTo => WhereBound.Clientbound;

    }

    public abstract class ClientboundPlayingPacket(int id) : PlayingPacket(id)
    {
        public static readonly int LoadChunkPacketId = 0x20;
        public static readonly int UnloadChunkPacketId = 0x1D;
        public static readonly int JoinGamePacketId = 0x23;
        public static readonly int SetPlayerAbilitiesId = 0x2C;

        public override WhereBound BoundTo => WhereBound.Clientbound;

    }

    public abstract class ServerboundPlayingPacket(int id) : PlayingPacket(id)
    {
        public static readonly int ClientSettingsPacketId = 0x04;

        public override WhereBound BoundTo => WhereBound.Serverbound;

    }

    internal class SetProtocolPacket : ServerboundHandshakingPacket
    {
        public readonly int Version;
        public readonly string Hostname;
        public readonly ushort Port;
        public readonly States NextState;

        private static States ReadNextState(Buffer buffer)
        {
            int a = buffer.ReadInt(true);
            States nextState = (a == 1 ? States.Status : States.Login);
            if (!(a == 1 || a == 2))
                throw new InvalidEncodingException();

            return nextState;
        }

        internal static SetProtocolPacket Read(Buffer buffer)
        {
            return new SetProtocolPacket(
                buffer.ReadInt(true),
                buffer.ReadString(), buffer.ReadUshort(),
                ReadNextState(buffer));
        }

        private SetProtocolPacket(
            int version,
            string hostname, ushort port,
            States nextState) : base(SetProtocolPacketId)
        {
            Debug.Assert(version == _ProtocolVersion);
            Debug.Assert(port > 0);
            Debug.Assert(
                nextState == States.Status ||
                nextState == States.Login);

            Version = version;
            Hostname = hostname;
            Port = port;
            NextState = nextState;
        }

        public SetProtocolPacket(string hostname, ushort port, States nextState)
            : this(_ProtocolVersion, hostname, port, nextState) { }

        internal override void WriteData(Buffer buffer)
        {
            Debug.Assert(Id == SetProtocolPacketId);
            Debug.Assert(
                NextState == States.Status ||
                NextState == States.Login);

            int a = NextState == States.Status ? 1 : 2;
            buffer.WriteInt(Version, true);
            buffer.WriteString(Hostname);
            buffer.WriteUshort(Port);
            buffer.WriteInt(a, true);
        }

    }

    internal class ResponsePacket : ClientboundStatusPacket
    {

        public readonly int MaxPlayers;
        public readonly int OnlinePlayers;
        public readonly string Description;

        internal static ResponsePacket Read(Buffer buffer)
        {
            /*string jsonString = buffer.ReadString();*/
            // TODO
            throw new NotImplementedException();
        }

        public ResponsePacket(int maxPlayers, int onlinePlayers, string description)
            : base(ResponsePacketId)
        {
            Debug.Assert(maxPlayers >= onlinePlayers);

            MaxPlayers = maxPlayers;
            OnlinePlayers = onlinePlayers;
            Description = description;
        }

        internal override void WriteData(Buffer buffer)
        {
            // TODO
            string jsonString = "{\"version\":{\"name\":\"1.12.2\",\"protocol\":340},\"players\":{\"max\":100,\"online\":0,\"sample\":[]},\"description\":{\"text\":\"Hello, World!\"},\"favicon\":\"data:image/png;base64,<data>\",\"enforcesSecureChat\":true,\"previewsChat\":true}";

            buffer.WriteString(jsonString);
        }

    }

    internal class PongPacket : ClientboundStatusPacket
    {
        public readonly long Payload;

        internal static PongPacket Read(Buffer buffer)
        {
            return new(buffer.ReadLong());
        }

        public PongPacket(long payload) : base(PongPacketId)
        {
            Payload = payload;
        }

        internal override void WriteData(Buffer buffer)
        {
            buffer.WriteLong(Payload);
        }

    }

    internal class RequestPacket : ServerboundStatusPacket
    {
        internal static RequestPacket Read(Buffer buffer)
        {
            return new();
        }

        public RequestPacket() : base(RequestPacketId) { }

        internal override void WriteData(Buffer buffer) { }

    }

    internal class PingPacket : ServerboundStatusPacket
    {
        public readonly long Payload;

        internal static PingPacket Read(Buffer buffer)
        {
            return new(buffer.ReadLong());
        }

        private PingPacket(long payload) : base(PingPacketId)
        {
            Payload = payload;
        }

        public PingPacket() : this(DateTime.Now.Ticks) { }

        internal override void WriteData(Buffer buffer)
        {
            buffer.WriteLong(Payload);
        }

    }

    internal class DisconnectPacket : ClientboundLoginPacket
    {
        public readonly string Reason;

        internal static DisconnectPacket Read(Buffer buffer)
        {
            return new(buffer.ReadString());
        }

        public DisconnectPacket(string reason) : base(DisconnectPacketId)
        {
            Reason = reason;
        }

        internal override void WriteData(Buffer buffer)
        {
            buffer.WriteString(Reason);
        }

    }

    internal class EncryptionRequestPacket : ClientboundLoginPacket
    {
        internal static EncryptionRequestPacket Read(Buffer buffer)
        {
            throw new NotImplementedException();
        }

        private EncryptionRequestPacket() : base(EncryptionRequestPacketId)
        {
            throw new NotImplementedException();
        }

        internal override void WriteData(Buffer buffer)
        {
            throw new NotImplementedException();
        }

    }

    internal class LoginSuccessPacket : ClientboundLoginPacket
    {
        public readonly Guid UserId;
        public readonly string Username;

        internal static LoginSuccessPacket Read(Buffer buffer)
        {
            return new(
                Guid.Parse(buffer.ReadString()),
                buffer.ReadString());
        }

        public LoginSuccessPacket(Guid userId, string username)
            : base(LoginSuccessPacketId)
        {
            UserId = userId;
            Username = username;
        }

        internal override void WriteData(Buffer buffer)
        {
            buffer.WriteString(UserId.ToString());
            buffer.WriteString(Username);
        }

    }

    internal class SetCompressionPacket : ClientboundLoginPacket
    {
        public readonly int Threshold;

        internal static SetCompressionPacket Read(Buffer buffer)
        {
            return new(buffer.ReadInt(true));
        }

        public SetCompressionPacket(int threshold)
            : base(SetCompressionPacketId)
        {
            Threshold = threshold;
        }

        internal override void WriteData(Buffer buffer)
        {
            buffer.WriteInt(Threshold, true);
        }

    }

    internal class StartLoginPacket : ServerboundLoginPacket
    {
        public readonly string Username;

        internal static StartLoginPacket Read(Buffer buffer)
        {
            return new(buffer.ReadString());
        }

        public StartLoginPacket(string username) : base(StartLoginPacketId)
        {
            Username = username;
        }

        internal override void WriteData(Buffer buffer)
        {
            buffer.WriteString(Username);
        }
    }

    internal class EncryptionResponsePacket : ServerboundLoginPacket
    {
        internal static EncryptionResponsePacket Read(Buffer buffer)
        {
            throw new NotImplementedException();
        }

        public EncryptionResponsePacket() : base(EncryptionResponsePacketId)
        {
            throw new NotImplementedException();
        }

        internal override void WriteData(Buffer buffer)
        {
            throw new NotImplementedException();
        }
    }

    public class LoadChunk : ClientboundPlayingPacket
    {
        public readonly int XChunk, ZChunk;
        public readonly bool Continuous;
        public readonly int Mask;
        public readonly byte[] Data;

        internal static LoadChunk Read(Buffer buffer)
        {
            throw new NotImplementedException();
        }

        public LoadChunk(
            int xChunk, int zChunk,
            bool continuous, int mask, byte[] data)
            : base(LoadChunkPacketId)
        {
            XChunk = xChunk;
            ZChunk = zChunk;
            Continuous = continuous;
            Mask = mask;
            Data = data;  // TODO: move semantics
        }

        internal override void WriteData(Buffer buffer)
        {
            buffer.WriteInt(XChunk);
            buffer.WriteInt(ZChunk);
            buffer.WriteBool(Continuous);
            buffer.WriteInt(Mask, true);
            buffer.WriteInt(Data.Length, true);
            buffer.WriteData(Data);
            buffer.WriteInt(0, true);  // TODO: Block entities
        }

    }

    public class UnloadChunk : ClientboundPlayingPacket
    {
        public readonly int XChunk, ZChunk;

        internal static UnloadChunk Read(Buffer buffer)
        {
            throw new NotImplementedException();
        }

        public UnloadChunk(int xChunk, int zChunk) : base(UnloadChunkPacketId)
        {
            XChunk = xChunk;
            ZChunk = zChunk;
        }

        internal override void WriteData(Buffer buffer)
        {
            buffer.WriteInt(XChunk);
            buffer.WriteInt(ZChunk);
        }

    }

    public class JoinGamePacket : ClientboundPlayingPacket
    {
        private readonly int _entityId;
        private readonly byte _gamemode;
        private readonly int _dimension;
        private readonly byte _difficulty;
        private readonly string _levelType;
        private readonly bool _reducedDebugInfo;

        internal static JoinGamePacket Read(Buffer buffer)
        {
            throw new NotImplementedException();
        }

        public JoinGamePacket(
            int entityId,
            byte gamemode,
            int dimension,
            byte difficulty,
            string levelType,
            bool reducedDebugInfo) : base(JoinGamePacketId)
        {
            _entityId = entityId;
            _gamemode = gamemode;
            _dimension = dimension;
            _difficulty = difficulty;
            _levelType = levelType;
            _reducedDebugInfo = reducedDebugInfo;

        }

        internal override void WriteData(Buffer buffer)
        {
            buffer.WriteInt(_entityId);
            buffer.WriteByte(_gamemode);
            buffer.WriteInt(_dimension);
            buffer.WriteByte(_difficulty);
            buffer.WriteByte(0);
            buffer.WriteString(_levelType);
            buffer.WriteBool(_reducedDebugInfo);
        }
    }

    public class SetAbilitiesPacket : ClientboundPlayingPacket
    {
        private readonly byte _flags;
        private readonly float _flyingSpeed;
        private readonly float _fovModifier;  // fov * _fovModifier;

        internal static SetAbilitiesPacket Read(Buffer buffer)
        {
            throw new NotImplementedException();
        }

        public SetAbilitiesPacket(
            bool invulnerable, bool flying, bool allowFlying, bool creativeMode,
            float flyingSpeed, float fovModifier) : base(SetPlayerAbilitiesId)
        {
            byte flags = 0;
            if (invulnerable)
                flags |= 0x01;
            if (flying)
                flags |= 0x02;
            if (allowFlying)
                flags |= 0x04;
            if (creativeMode)
                flags |= 0x08;

            _flags = flags;
            _flyingSpeed = flyingSpeed;
            _fovModifier = fovModifier;
        }

        internal override void WriteData(Buffer buffer)
        {
            buffer.WriteByte(_flags);
            buffer.WriteFloat(_flyingSpeed);
            buffer.WriteFloat(_fovModifier);
        }

    }

    public class ClientSettingsPacket : ServerboundPlayingPacket
    {
        public readonly byte RenderDistance;

        internal static ClientSettingsPacket Read(Buffer buffer)
        {
            buffer.ReadString();
            byte renderDistance = buffer.ReadByte();
            buffer.ReadInt(true);
            buffer.ReadBool();
            buffer.ReadSbyte();
            buffer.ReadInt(true);

            return new(renderDistance);
        }

        public ClientSettingsPacket(byte renderDistance)
            : base(ClientSettingsPacketId)
        {
            RenderDistance = renderDistance;
        }

        internal override void WriteData(Buffer buffer)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class Block
    {
        public enum Ids : ushort
        {
            Air = 0,
            Stone,
            GrassBlock,
            Dirt,
        }

        private readonly Ids _id;  // 9 bits
        public Ids Id => _id;

        public Block(Ids id)
        {
            _id = id;
        }

        protected abstract byte GetMetadata();

        public ulong GetGlobalPaletteID()
        {
            byte metadata = GetMetadata();
            Debug.Assert((metadata & 0b_11110000) == 0);  // metadata is 4 bits

            ushort id = (ushort)_id;
            Debug.Assert((id & 0b_11111110_00000000) == 0);  // id is 9 bits
            return (ulong)(id << 4 | metadata);  // 13 bits
        }


    }

    public class Air : Block
    {

        public Air() : base(Ids.Air) { }

        protected override byte GetMetadata()
        {
            return 0;
        }
    }

    public class Stone : Block
    {
        public enum Types : byte
        {
            Normal = 0,
            Granite,
            PolishedGranite,
            Diorite,
            PolishedDiorite,
            Andesite,
            PolishedAndesite,
        }

        private readonly Types _type;  // 4 bits
        public Types Type => _type;

        public Stone(Types type) : base(Ids.Stone)
        {
            _type = type;
        }

        protected override byte GetMetadata()
        {
            return (byte)_type;
        }

    }

    public class GrassBlock : Block
    {
        public GrassBlock() : base(Ids.GrassBlock) { }

        protected override byte GetMetadata()
        {
            return 0;
        }
    }

    public class Chunk
    {
        public static readonly int Width = 16;
        public static readonly int Height = 16 * 16;

        public struct Position(int x, int z)
        {
            public int X = x, Z = z;

            public static Position Convert(Entity.Position p)
            {
                return new(
                    (p.X >= 0) ? ((int)p.X / Width) : (((int)p.X / (Width + 1)) - 1),
                    (p.Z >= 0) ? ((int)p.Z / Width) : (((int)p.Z / (Width + 1)) - 1));
            }

        }

        private class Section
        {
            public static readonly int Width = Chunk.Width;
            public static readonly int Height = Chunk.Height / Width;

            public static readonly int BlockTotalCount = Width * Width * Height;
            // (0, 0, 0) to (16, 16, 16)
            private Block?[] _blocks = new Block?[BlockTotalCount];

            public static void Write(Buffer buffer, Section section)
            {
                int blockBitCount = 13;
                buffer.WriteByte((byte)blockBitCount);
                buffer.WriteInt(0, true);  // Write pallete as globally

                int blockBitTotalCount = (BlockTotalCount) * blockBitCount,
                    ulongBitCount = (sizeof(ulong) * 8);  // TODO: Make as constants
                int dataLength = blockBitTotalCount / ulongBitCount;
                Debug.Assert(blockBitTotalCount % ulongBitCount == 0);
                ulong[] data = new ulong[dataLength];

                for (int y = 0; y < Height; ++y)
                {
                    for (int z = 0; z < Width; ++z)
                    {
                        for (int x = 0; x < Width; ++x)
                        {
                            int i = (((y * Height) + z) * Width) + x;

                            int start = (i * blockBitCount) / ulongBitCount,
                                offset = (i * blockBitCount) % ulongBitCount,
                                end = (((i + 1) * blockBitCount) - 1) / ulongBitCount;

                            Block? block = section._blocks[i];
                            if (block == null)
                                block = new Air();

                            ulong id = block.GetGlobalPaletteID();
                            Debug.Assert((id >> blockBitCount) == 0);

                            data[start] |= (id << offset);

                            if (start != end)
                            {
                                data[end] = (id >> (ulongBitCount - offset));
                            }

                        }
                    }
                }

                Debug.Assert(unchecked((long)ulong.MaxValue) == -1);
                buffer.WriteInt(dataLength, true);
                for (int i = 0; i < dataLength; ++i)
                {
                    buffer.WriteLong((long)data[i]);  // TODO
                }

                // TODO
                for (int y = 0; y < Height; ++y)
                {
                    for (int z = 0; z < Width; ++z)
                    {
                        for (int x = 0; x < Width; x += 2)
                        {
                            buffer.WriteByte(byte.MaxValue / 2);

                        }
                    }
                }

                // TODO
                for (int y = 0; y < Height; ++y)
                {
                    for (int z = 0; z < Width; ++z)
                    {
                        for (int x = 0; x < Width; x += 2)
                        {
                            buffer.WriteByte(byte.MaxValue / 2);

                        }
                    }
                }


            }

        }

        public static readonly int SectionTotalCount = Height / Section.Height;
        // bottom to top
        private Section?[] _sections = new Section?[SectionTotalCount];

        public static (bool, int, byte[]) Write(Chunk chunk)
        {
            Buffer buffer = new();

            buffer.WriteBool(true);
            int mask = 0;
            Debug.Assert(SectionTotalCount == 16);
            for (int i = 0; i < SectionTotalCount; ++i)
            {
                Section? section = chunk._sections[i];
                if (section == null) continue;

                mask |= (1 << i);  // TODO;
                Section.Write(buffer, section);
            }

            // TODO
            for (int z = 0; z < Width; ++z)
            {
                for (int x = 0; x < Width; ++x)
                {
                    buffer.WriteByte(0);
                }
            }

            return (true, mask, buffer.ReadData());
        }

        public readonly Position Pos;

        public Chunk(Position p)
        {
            Pos = p;
        }

    }

    public abstract class Entity
    {
        public struct Position(float x, float y, float z)
        {
            public float X = x, Y = y, Z = z;

            public static Position Convert(Chunk.Position p)
            {
                throw new NotImplementedException();
            }

        }

        public readonly int Id;

        public Position PosPrev, Pos;
        public Chunk.Position PosChunkPrev, PosChunk;

        internal Entity(int id, Position p)
        {
            Id = id;
            Pos = PosPrev = p;
            PosChunk = PosChunkPrev = Chunk.Position.Convert(p);
        }

    }

    public abstract class LivingEntity : Entity
    {
        /*private int _health;*/

        public LivingEntity(int id, Position p) : base(id, p) { }

    }

    public class Player : LivingEntity
    {
        public sealed class ClientsideSettings(int renderDistance)
        {
            public int renderDistance = renderDistance;
        }

        public readonly ClientsideSettings Settings;

        public Player(
            int id,
            Position p,
            ClientsideSettings settings) : base(id, p)
        {
            Settings = settings;
        }

    }

    internal sealed class Client : IDisposable
    {
        private static readonly byte _SEGMENT_BITS = 0x7F;
        private static readonly byte _CONTINUE_BIT = 0x80;

        private bool _disposed = false;

        private int _x = 0, _y = 0;
        private byte[]? _data = null;

        private Socket _socket;

        internal static Client Accept(Socket socket)
        {
            //TODO: Check the socket is Binding and listening correctly.
            Debug.Assert(socket.IsBound == true);

            Socket newSocket = SocketMethods.Accept(socket);
            SocketMethods.SetBlocking(newSocket, false);

            /*Console.WriteLine($"socket: {socket.LocalEndPoint}");*/


            return new(newSocket);
        }

        private Client(Socket socket)
        {
            Debug.Assert(SocketMethods.IsBlocking(socket) == false);
            _socket = socket;
        }

        ~Client()
        {
            Dispose(false);
        }

        private int RecvSize()
        {
            Debug.Assert(!_disposed);

            Debug.Assert(SocketMethods.IsBlocking(_socket) == false);

            uint uvalue = (uint)_x;
            int position = _y;

            try
            {
                while (true)
                {
                    byte b = SocketMethods.RecvByte(_socket);

                    uvalue |= (uint)(b & _SEGMENT_BITS) << position;
                    if ((b & _CONTINUE_BIT) == 0)
                        break;

                    position += 7;

                    if (position >= 32)
                        throw new InvalidEncodingException();

                    Debug.Assert(position > 0);
                }

            }
            finally
            {
                _x = (int)uvalue;
                _y = position;
                Debug.Assert(_data == null);
            }

            return (int)uvalue;
        }

        private void SendSize(int size)
        {
            Debug.Assert(!_disposed);

            uint uvalue = (uint)size;

            while (true)
            {
                if ((uvalue & ~_SEGMENT_BITS) == 0)
                {
                    SocketMethods.SendByte(_socket, (byte)uvalue);
                    break;
                }

                SocketMethods.SendByte(
                    _socket, (byte)((uvalue & _SEGMENT_BITS) | _CONTINUE_BIT));
                uvalue >>= 7;
            }

        }

        /*public void A()
        {
            SocketMethods.SetBlocking(_socket, true);
        }*/

        public void Recv(Buffer buffer)
        {
            Debug.Assert(!_disposed);

            Debug.Assert(SocketMethods.IsBlocking(_socket) == false);

            if (_data == null)
            {
                int size = RecvSize();
                _x = size;
                Debug.Assert(_y == 0);

                if (size == 0)
                    return;  // TODO: make EmptyBuffer and return it.

                Debug.Assert(_data == null);
                _data = new byte[size];
                Debug.Assert(size > 0);
            }

            int availSize = _x, offset = _y;

            do
            {
                try
                {
                    int n = SocketMethods.RecvBytes(_socket, _data, offset, availSize);
                    Debug.Assert(n <= availSize);

                    availSize -= n;
                    offset += n;
                }
                finally
                {
                    _x = availSize;
                    _y = offset;
                }
            } while (availSize > 0);

            buffer.WriteData(_data);

            _x = 0;
            _y = 0;
            _data = null;

        }

        public void Send(Buffer buffer)
        {
            Debug.Assert(!_disposed);

            byte[] data = buffer.ReadData();
            SendSize(data.Length);
            SocketMethods.SendBytes(_socket, data);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing == true)
            {
                // Release managed resources.
                _socket.Dispose();
                _data = null;
            }

            // Release unmanaged resources.

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Close()
        {
            Dispose();
        }

    }

    public class Connection : IDisposable
    {
        public readonly int Id;

        private Client _client;

        public readonly Guid UserId;
        public readonly string Username;

        private bool _disposed = false;

        internal Connection(
            int id,
            Client client,
            Guid userId, string username)
        {
            Id = id;

            _client = client;

            UserId = userId;
            Username = username;
        }

        ~Connection()
        {
            Dispose(false);
        }

        public ServerboundPlayingPacket RecvPacket()
        {
            Debug.Assert(!_disposed);

            throw new NotImplementedException();
        }

        public void SendPacket(ClientboundPlayingPacket packet)
        {
            Debug.Assert(!_disposed);

            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing == true)
            {
                // managed objects
                /*Username.Dispose();*/  // TODO
                _client.Dispose();
            }

            // unmanaged objects

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Close()
        {
            _client.Dispose();
        }

    }

    /*public sealed class PlayerList
    {
        private readonly NumList _idList;

        public PlayerList(NumList idList)
        {
            _idList = idList;
        }

        public int Alloc(Guid userId)
        {
            throw new NotImplementedException();
        }

        public void Dealloc(int id)
        {
            throw new NotImplementedException();
        }

    }*/

    public class Listener
    {
        private static readonly TimeSpan PendingTimeout = TimeSpan.FromSeconds(1);

        public Listener() { }

        private int HandleVisitors(Queue<Client> visitors, Queue<int> levelQueue,
            NumList idList, ConcurrentQueue<Connection> connections)
        {
            Console.Write(".");

            int count = visitors.Count;
            Debug.Assert(count == levelQueue.Count);
            if (count == 0) return 0;

            bool close, success;

            for (; count > 0; --count)
            {
                close = success = false;

                Client visitor = visitors.Dequeue();
                int level = levelQueue.Dequeue();

                /*Console.WriteLine($"count: {count}, level: {level}");*/

                Debug.Assert(level >= 0);
                using Buffer buffer = new();

                try
                {
                    if (level == 0)
                    {
                        /*Console.WriteLine("Handshake!");*/
                        visitor.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundHandshakingPacket.SetProtocolPacketId != packetId)
                            throw new UnexpectedPacketException();

                        SetProtocolPacket packet = SetProtocolPacket.Read(buffer);

                        if (buffer.Size > 0)
                            throw new BufferOverflowException();

                        switch (packet.NextState)
                        {
                            default:
                                throw new InvalidEncodingException();
                            case Packet.States.Status:
                                level = 1;
                                break;
                            case Packet.States.Login:
                                level = 3;
                                break;
                        }

                    }

                    if (level == 1)  // Request
                    {
                        /*Console.WriteLine("Request!");*/
                        visitor.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundStatusPacket.RequestPacketId != packetId)
                            throw new UnexpectedPacketException();

                        RequestPacket requestPacket = RequestPacket.Read(buffer);

                        if (buffer.Size > 0)
                            throw new BufferOverflowException();

                        // TODO
                        ResponsePacket responsePacket = new(100, 10, "Hello, World!");
                        responsePacket.Write(buffer);
                        visitor.Send(buffer);

                        level = 2;
                    }

                    if (level == 2)  // Ping
                    {
                        /*Console.WriteLine("Ping!");*/
                        visitor.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundStatusPacket.PingPacketId != packetId)
                            throw new UnexpectedPacketException();

                        PingPacket inPacket = PingPacket.Read(buffer);

                        if (buffer.Size > 0)
                            throw new BufferOverflowException();

                        PongPacket outPacket = new(inPacket.Payload);
                        outPacket.Write(buffer);
                        visitor.Send(buffer);
                    }

                    if (level == 3)  // Start Login
                    {
                        visitor.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundLoginPacket.StartLoginPacketId != packetId)
                            throw new UnexpectedPacketException();

                        StartLoginPacket inPacket = StartLoginPacket.Read(buffer);

                        if (buffer.Size > 0)
                            throw new BufferOverflowException();

                        // TODO: Check username is empty or invalid.

                        HttpClient httpClient = new();
                        string url = string.Format("https://api.mojang.com/users/profiles/minecraft/{0}", inPacket.Username);
                        /*Console.WriteLine(inPacket.Username);
                        Console.WriteLine($"url: {url}");*/
                        using HttpRequestMessage request = new(HttpMethod.Get, url);

                        // TODO: handle HttpRequestException
                        using HttpResponseMessage response = httpClient.Send(request);

                        using Stream stream = response.Content.ReadAsStream();
                        using StreamReader reader = new(stream);
                        string str = reader.ReadToEnd();
                        System.Collections.Generic.Dictionary<string, string>? dictionary =
                            JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, string>>(str);
                        Debug.Assert(dictionary != null);

                        Guid userId = Guid.Parse(dictionary["id"]);
                        string username = dictionary["name"];  // TODO: check username is valid
                        /*Console.WriteLine($"userId: {userId}");
                        Console.WriteLine($"username: {username}");*/

                        // TODO: Handle to throw exception
                        Debug.Assert(inPacket.Username == username);

                        LoginSuccessPacket outPacket1 = new(userId, username);
                        outPacket1.Write(buffer);
                        visitor.Send(buffer);

                        // TODO: Must dealloc id when connection is disposed.
                        int id = idList.Alloc();

                        Connection conn = new(id, visitor, userId, username);
                        connections.Enqueue(conn);

                        success = true;
                    }


                    Debug.Assert(buffer.Size == 0);

                    if (!success) close = true;

                }
                catch (SocketException e)
                {
                    asdfasdf
                    // TODO: try again
                    if (e.SocketErrorCode == SocketError.ConnectionAborted ||
                        false)  // Add other Exceptions here!
                        close = true;
                    else if (e.SocketErrorCode != SocketError.WouldBlock)
                        Debug.Assert(false);

                    Debug.Assert(
                        (e.SocketErrorCode == SocketError.WouldBlock) ?
                        close == false : true);

                    Console.Write($"SocketError.WouldBlock!");
                }
                catch (UnexpectedDataException)
                {
                    Debug.Assert(success == false);
                    Debug.Assert(close == false);
                    close = true;

                    buffer.Flush();

                    if (level >= 3)  // >= Start Login level
                    {
                        Debug.Assert(false);
                        // TODO: Handle send Disconnect packet with reason.
                    }

                    /*Console.Write($"UnexpectedDataException");*/
                }
                catch (DisconnectedException)
                {
                    Debug.Assert(success == false);
                    Debug.Assert(close == false);
                    close = true;

                    /*Console.Write("~");*/

                    buffer.Flush();

                    /*Console.Write($"EndofFileException");*/
                }

                if (!success)
                {
                    if (close == false)
                    {
                        visitors.Enqueue(visitor);
                        levelQueue.Enqueue(level);
                    }
                    else
                    {
                        visitor.Close();
                    }
                }
                else
                    Debug.Assert(close == false);


            }

            return visitors.Count;
        }

        public void StartRoutine(ConsoleApplication app, ushort port,
            NumList idList, ConcurrentQueue<Connection> connections)
        {
            using Socket socket = SocketMethods.Establish(port);

            using Queue<Client> visitors = new();
            /*
             * 0: Handshake
             * 1: Request
             * 2: Ping
             * 3: Start Login
             */
            using Queue<int> levelQueue = new();

            SocketMethods.SetBlocking(socket, true);

            while (app.Running)
            {
                try
                {
                    if (!SocketMethods.IsBlocking(socket) &&
                        HandleVisitors(visitors, levelQueue,
                            idList, connections) == 0)
                    {
                        SocketMethods.SetBlocking(socket, true);
                    }

                    SocketMethods.Poll(socket, PendingTimeout);

                    Client client = Client.Accept(socket);
                    visitors.Enqueue(client);
                    levelQueue.Enqueue(0);

                    SocketMethods.SetBlocking(socket, false);

                }
                catch (TryAgainException)
                {
                    continue;
                }
                catch (PendingTimeoutException)
                {
                    Console.WriteLine("!");
                }

            }

            // TODO: Handle client, send Disconnet Packet if the client's step is after StartLogin;
        }

    }

}

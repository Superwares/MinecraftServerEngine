using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Application
{

    public abstract class ProtocolException : Exception
    {
        public ProtocolException(string message) : base(message) { }
    }

    public class EndofFileException : ProtocolException
    {
        public EndofFileException() : base("EOF reached unexpectedly.") { }
    }

    public class EmptyBufferException : ProtocolException
    {
        public EmptyBufferException() : base("Attempting to read from an empty buffer.") { }
    }

    public class UnexpectedPacketException : ProtocolException
    {
        public UnexpectedPacketException() : base("Encountered an unexpected packet.") { }
    }

    public class InvalidEncodingException : ProtocolException
    {
        public InvalidEncodingException() : base("Failed to decode the data due to invalid encoding.") { }
    }

    // TODO: It needs the corrent name and message.
    public class TimeoutException : ProtocolException
    {
        public TimeoutException() : base("Connections are not pending.") { }
    }

    internal static class SocketMethods
    {
        public static byte RecvByte(Socket socket)
        {
            byte[] buffer = new byte[1];

            int n = socket.Receive(buffer);
            if (n == 0)
                throw new EndofFileException();

            Debug.Assert(n == 1);

            return buffer[0];
        }

        public static int RecvBytes(
            Socket socket, byte[] buffer, int offset, int size)
        {
            int n = socket.Receive(buffer, offset, size, SocketFlags.None);
            if (n == 0)
                throw new EndofFileException();

            Debug.Assert(n <= size);

            return n;
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
    public class Buffer
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

        private const int _INITIAL_DATA_SIZE = 16;

        private int _size;
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

        public Buffer()
        {
            _size = _INITIAL_DATA_SIZE;
            _data = new byte[_INITIAL_DATA_SIZE];
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

            int prevSize = _size,
                newSize = prevSize,
                requiredSize = _last + addedSize;

            if (addedSize > 1)
                while (((float)requiredSize / (float)newSize) >= _LOAD_FACTOR)
                    newSize *= _EXPANSION_FACTOR;
            else
                if (((float)requiredSize / (float)newSize) >= _LOAD_FACTOR)
                newSize *= _EXPANSION_FACTOR;

            Debug.Assert(prevSize <= newSize);

            _size = newSize;

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
            return BitConverter.ToString(data);
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

        public Packet() { }

        public abstract void Write(Buffer buffer);

    }

    public abstract class HandshakingPacket : Packet
    {
        public override States State { get { return States.Handshaking; } }

    }

    public abstract class StatusPacket : Packet
    {
        public override States State { get { return States.Status; } }

    }

    public abstract class LoginPacket : Packet
    {
        public override States State { get { return States.Login; } }

    }

    public abstract class PlayingPacket : Packet
    {
        public override States State => States.Playing;
    }

    public abstract class ServerboundHandshakingPacket : HandshakingPacket
    {
        public enum Ids
        {
            HandshakePacketId = 0x00,
        }

        protected readonly Ids _Id;
        public Ids Id { get { return _Id; } }
        public override WhereBound BoundTo { get { return WhereBound.Serverbound; } }

        protected ServerboundHandshakingPacket(Ids id)
        {
            _Id = id;
        }

    }
    public abstract class ClientboundStatusPacket : StatusPacket
    {
        public enum Ids
        {
            ResponsePacketId = 0x00,
            PongPacketId = 0x01,
        }

        protected readonly Ids _Id;
        public Ids Id { get { return _Id; } }
        public override WhereBound BoundTo { get { return WhereBound.Clientbound; } }

        protected ClientboundStatusPacket(Ids id)
        {
            _Id = id;
        }

    }

    public abstract class ServerboundStatusPacket : StatusPacket
    {
        public enum Ids
        {
            RequestPacketId = 0x00,
            PingPacketId = 0x01,
        }

        protected readonly Ids _Id;
        public Ids Id { get { return _Id; } }
        public override WhereBound BoundTo { get { return WhereBound.Serverbound; } }

        protected ServerboundStatusPacket(Ids id)
        {
            _Id = id;
        }

    }

    public abstract class ClientboundLoginPacket : LoginPacket
    {
        public enum Ids
        {
            DisconnectPacketId = 0x00,
            EncryptionRequestPacketId = 0x01,
            LoginSuccessPacketId = 0x02,
            SetCompressionPacketId = 0x03,
        }

        protected readonly Ids _Id;
        public Ids Id => _Id;
        public override WhereBound BoundTo => WhereBound.Clientbound;

        protected ClientboundLoginPacket(Ids id)
        {
            _Id = id;
        }

    }

    public abstract class ServerboundLoginPacket : LoginPacket
    {
        public enum Ids
        {
            StartLoginPacketId = 0x00,
            EncryptionResponsePacketId = 0x01,
        }

        protected readonly Ids _Id;
        public Ids Id => _Id;
        public override WhereBound BoundTo => WhereBound.Clientbound;

        protected ServerboundLoginPacket(Ids id)
        {
            _Id = id;
        }

    }

    public abstract class ClientboundPlayingPacket : PlayingPacket
    {
        public enum Ids
        {

        }

        protected readonly Ids _Id;
        public Ids Id => _Id;
        public override WhereBound BoundTo => WhereBound.Clientbound;

        protected ClientboundPlayingPacket(Ids id)
        {
            _Id = id;
        }
    }

    public abstract class ServerboundPlayingPacket : PlayingPacket
    {
        public enum Ids
        {

        }

        protected readonly Ids _Id;
        public Ids Id => _Id;
        public override WhereBound BoundTo => WhereBound.Serverbound;

        protected ServerboundPlayingPacket(Ids id)
        {
            _Id = id;
        }
    }

    public class SetProtocolPacket : ServerboundHandshakingPacket
    {
        public readonly int Version;
        public readonly string Hostname;
        public readonly ushort Port;
        public readonly States NextState;

        public static SetProtocolPacket Read(Buffer buffer)
        {
            return new SetProtocolPacket(
                buffer.ReadInt(true),
                buffer.ReadString(), buffer.ReadUshort(),
                buffer.ReadInt(true) == 1 ? States.Status : States.Login);
        }

        private SetProtocolPacket(
            int version,
            string hostname, ushort port,
            States nextState)
            : base(Ids.HandshakePacketId)
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
            : this(_ProtocolVersion, hostname, port, nextState)
        { }

        public override void Write(Buffer buffer)
        {
            Debug.Assert(_Id == Ids.HandshakePacketId);
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

    public class ResponsePacket : ClientboundStatusPacket
    {

        public readonly int MaxPlayers;
        public readonly int OnlinePlayers;
        public readonly string Description;

        public static ResponsePacket Read(Buffer buffer)
        {
            string jsonString = buffer.ReadString();
            // TODO
            throw new NotImplementedException();
        }

        public ResponsePacket(
            int maxPlayers, int onlinePlayers, string description)
            : base(Ids.ResponsePacketId)
        {
            Debug.Assert(maxPlayers >= onlinePlayers);

            MaxPlayers = maxPlayers;
            OnlinePlayers = onlinePlayers;
            Description = description;
        }

        public override void Write(Buffer buffer)
        {
            string jsonString = "{\"version\":{\"name\":\"1.12.2\",\"protocol\":340},\"players\":{\"max\":100,\"online\":0,\"sample\":[]},\"description\":{\"text\":\"Hello, World!\"},\"favicon\":\"data:image/png;base64,<data>\",\"enforcesSecureChat\":true,\"previewsChat\":true}";

            buffer.WriteString(jsonString);
        }

    }

    public class PongPacket : ClientboundStatusPacket
    {
        public readonly long Payload;

        public static PongPacket Read(Buffer buffer)
        {
            return new(buffer.ReadLong());
        }

        public PongPacket(long payload) : base(Ids.PongPacketId)
        {
            Payload = payload;
        }

        public override void Write(Buffer buffer)
        {
            buffer.WriteLong(Payload);
        }

    }

    public class RequestPacket : ServerboundStatusPacket
    {
        public static RequestPacket Read(Buffer buffer)
        {
            return new();
        }

        public RequestPacket() : base(Ids.RequestPacketId) { }

        public override void Write(Buffer buffer) { }

    }

    public class PingPacket : ServerboundStatusPacket
    {
        public readonly long Payload;

        public static PingPacket Read(Buffer buffer)
        {
            return new(buffer.ReadLong());
        }

        private PingPacket(long payload) : base(Ids.PingPacketId)
        {
            Payload = payload;
        }

        public PingPacket() : this(DateTime.Now.Ticks) { }

        public override void Write(Buffer buffer)
        {
            buffer.WriteLong(Payload);
        }

    }

    public class DisconnectPacket : ClientboundLoginPacket
    {
        public readonly string Reason;

        public static DisconnectPacket Read(Buffer buffer)
        {
            return new(buffer.ReadString());
        }

        public DisconnectPacket(string reason) : base(Ids.DisconnectPacketId)
        {
            Reason = reason;
        }

        public override void Write(Buffer buffer)
        {
            buffer.WriteString(Reason);
        }

    }

    public class EncryptionRequestPacket : ClientboundLoginPacket
    {
        public static EncryptionRequestPacket Read(Buffer buffer)
        {
            throw new NotImplementedException();
        }

        private EncryptionRequestPacket() : base(Ids.EncryptionRequestPacketId)
        {
            throw new NotImplementedException();
        }

        public override void Write(Buffer buffer)
        {
            throw new NotImplementedException();
        }

    }

    public class LoginSuccessPacket : ClientboundLoginPacket
    {
        public readonly Guid UserId;
        public readonly string Username;

        public static LoginSuccessPacket Read(Buffer buffer)
        {
            return new(
                Guid.Parse(buffer.ReadString()),
                buffer.ReadString());
        }

        public LoginSuccessPacket(Guid userId, string username)
            : base(Ids.LoginSuccessPacketId)
        {
            UserId = userId;
            Username = username;
        }

        public override void Write(Buffer buffer)
        {
            buffer.WriteString(UserId.ToString());
            buffer.WriteString(Username);
        }

    }

    public class SetCompressionPacket : ClientboundLoginPacket
    {
        public readonly int Threshold;

        public static SetCompressionPacket Read(Buffer buffer)
        {
            return new(buffer.ReadInt(true));
        }

        public SetCompressionPacket(int threshold)
            : base(Ids.SetCompressionPacketId)
        {
            Threshold = threshold;
        }

        public override void Write(Buffer buffer)
        {
            buffer.WriteInt(Threshold, true);
        }

    }

    public class StartLoginPacket : ServerboundLoginPacket
    {
        public readonly string Username;

        public static StartLoginPacket Read(Buffer buffer)
        {
            return new(buffer.ReadString());
        }

        public StartLoginPacket(string username) : base(Ids.StartLoginPacketId)
        {
            Username = username;
        }

        public override void Write(Buffer buffer)
        {
            buffer.WriteString(Username);
        }
    }

    public class EncryptionResponsePacket : ServerboundLoginPacket
    {
        public static EncryptionResponsePacket Read(Buffer buffer)
        {
            throw new NotImplementedException();
        }

        public EncryptionResponsePacket() : base(Ids.EncryptionResponsePacketId)
        {
            throw new NotImplementedException();
        }

        public override void Write(Buffer buffer)
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

        public ushort GetGlobalPaletteID()
        {
            byte metadata = GetMetadata();
            Debug.Assert((metadata & 0b_11110000) == 0);  // metadata is 4 bits

            ushort id = (ushort)_id;
            Debug.Assert((id & 0b_11111110_00000000) == 0);  // id is 9 bits
            return (ushort)(id << 4 | metadata);  // 13 bits
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

    public class CheckSection
    {
        private static readonly int _BlocksNumber = 16 * 16 * 16;

        private Block?[] _blocks = new Block?[_BlocksNumber];



    }

    internal class Client : IDisposable
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

            Socket newSocket = socket.Accept();
            newSocket.Blocking = false;

            /*Console.WriteLine($"socket: {socket.LocalEndPoint}");*/

            
            return new(newSocket);
        }

        private Client(Socket socket)
        {
            Debug.Assert(socket.Blocking == false);
            _socket = socket;
        }

        ~Client()
        {
            Dispose(false);
        }

        private int RecvSize()
        {
            if (_disposed) throw new ObjectDisposedException(this.GetType().Name);

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
            if (_disposed) throw new ObjectDisposedException(this.GetType().Name);

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

        public Buffer RecvBuffer()
        {
            if (_disposed) throw new ObjectDisposedException(this.GetType().Name);

            if (_data == null)
            {
                int size = RecvSize();
                _x = size;
                Debug.Assert(_y == 0);

                if (size == 0)
                    return new();  // TODO: make EmptyBuffer and return it.

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

            Buffer result = new();
            result.WriteData(_data);

            _x = 0;
            _y = 0;
            _data = null;


            return result;
        }

        public void SendBuffer(Buffer buffer)
        {
            if (_disposed) throw new ObjectDisposedException(this.GetType().Name);

            byte[] data = buffer.ReadData();
            SendSize(data.Length);
            SocketMethods.SendBytes(_socket, data);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed == false)
            {
                if (disposing == true)
                {
                    // managed objects
                    _data = null;
                }

                // unmanaged objects
                _socket.Dispose();

                _disposed = true;
            }
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

    public class Connection
    {
        private Client _client;

        internal Connection(Client client)
        {
            _client = client;
        }

        public PlayingPacket RecvPacket()
        {
            throw new NotImplementedException();
        }

        public void SendPacket(PlayingPacket packet)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            _client.Close();
        }
    }

    public class Listener
    {
        public enum Steps
        {
            Handshake,
            Request,
            Ping,
            StartLogin,
            JoinGame,
            //Playing,
        }

        private static readonly TimeSpan PENDING_TIMEOUT = TimeSpan.FromSeconds(1);

        private Containers.Queue<Client> _clientQueue = new();
        private Containers.Queue<Steps> _stepQueue = new();

        public Listener() { }

        private void HandleNonplayers()
        {
            /*Console.WriteLine($"Call HandleNonplayers.");*/

            int count = _clientQueue.Count;
            if (count == 0)
                return;

            bool close = false;

            for (; count > 0; --count)
            {
                close = false;

                Client client = _clientQueue.Dequeue();
                Steps step = _stepQueue.Dequeue();

                try
                {
                    if (step == Steps.Handshake)
                    {
                        /*Console.WriteLine("Handshake!");*/
                        Buffer buffer = client.RecvBuffer();

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundHandshakingPacket.Ids.HandshakePacketId !=
                            (ServerboundHandshakingPacket.Ids)packetId)
                            throw new UnexpectedPacketException();

                        SetProtocolPacket packet = SetProtocolPacket.Read(buffer);
                        Packet.States state = packet.NextState;

                        if (state == Packet.States.Status)
                            step = Steps.Request;
                        else if (state == Packet.States.Login)
                            step = Steps.StartLogin;
                        else
                            throw new NotImplementedException();
                    }

                    if (step == Steps.Request)
                    {
                        /*Console.WriteLine("Request!");*/
                        Buffer buffer = client.RecvBuffer();

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundStatusPacket.Ids.RequestPacketId !=
                            (ServerboundStatusPacket.Ids)packetId)
                            throw new UnexpectedPacketException();

                        RequestPacket requestPacket = RequestPacket.Read(buffer);

                        // TODO
                        ResponsePacket responsePacket = new(100, 10, "Hello, World!");
                        buffer.WriteInt((int)responsePacket.Id, true);
                        responsePacket.Write(buffer);
                        client.SendBuffer(buffer);

                        step = Steps.Ping;
                    }

                    if (step == Steps.Ping)
                    {
                        /*Console.WriteLine("Ping!");*/
                        Buffer buffer = client.RecvBuffer();

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundStatusPacket.Ids.PingPacketId !=
                            (ServerboundStatusPacket.Ids)packetId)
                            throw new UnexpectedPacketException();

                        PingPacket inPacket = PingPacket.Read(buffer);

                        PongPacket pongPacket = new(inPacket.Payload);
                        buffer.WriteInt((int)pongPacket.Id, true);
                        pongPacket.Write(buffer);
                        client.SendBuffer(buffer);
                    }

                    if (step == Steps.StartLogin)
                    {
                        throw new NotImplementedException();
                    }

                    close = true;

                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode != SocketError.WouldBlock)
                        throw new NotImplementedException();

                    /*Console.WriteLine($"SocketError.WouldBlock!");*/

                }
                catch (ProtocolException)
                {
                    close = true;

                    if (step >= Steps.StartLogin)
                    {
                        throw new NotImplementedException();
                        // TODO: Handle send Disconnect packet.
                    }
                }

                if (close == false)
                {
                    _clientQueue.Enqueue(client);
                    _stepQueue.Enqueue(step);
                }
                else
                    client.Close();

            }

        }

        public void Accept(ushort port)
        {
            using Socket socket = new(SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint localEndPoint = new(IPAddress.Any, port);
            socket.Bind(localEndPoint);
            socket.Listen();

            while (Application.Running)
            {
                
                try
                {
                    /*Console.WriteLine();*/

                    HandleNonplayers();

                    if (_clientQueue.Count == 0)
                        socket.Blocking = true;

                    /*Console.WriteLine($"_clientQueue.Count: {_clientQueue.Count}");*/

                    if (socket.Blocking == true && 
                        socket.Poll(PENDING_TIMEOUT, SelectMode.SelectRead) == false)
                        throw new TimeoutException();

                    Client client = Client.Accept(socket);

                    _clientQueue.Enqueue(client);
                    _stepQueue.Enqueue(Steps.Handshake);

                    socket.Blocking = false;


                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode != SocketError.WouldBlock)
                        throw new NotImplementedException();

                    /*Console.WriteLine(DateTime.Now);*/
                }
                catch (TimeoutException)
                {
                    /*Console.WriteLine($"Timeout! {DateTime.Now}");*/

                }

            }

            // TODO: Handle client, send Disconnet Packet if the client's step is after StartLogin;


        }

    }
    
    public sealed class Application
    {
        public delegate void StartRoutine();
        public delegate void StartRoutineWithArg<T>(T arg);

        private interface IThreadContext
        {
            void Call();
        }

        private readonly struct ThreadContext(StartRoutine f) : IThreadContext
        {
            private readonly StartRoutine _f = f;

            public void Call() { _f(); }
        }

        private readonly struct ThreadContextWithArg<T>(StartRoutineWithArg<T> f, T arg) : IThreadContext
        {
            private readonly StartRoutineWithArg<T> _f = f;
            private readonly T _arg = arg;

            public void Call() { _f(_arg); }
        }

        private static readonly Thread _MainThread = Thread.CurrentThread;
        private static readonly int _MainId = _MainThread.ManagedThreadId;
        private static readonly Application Instance = new();

        private readonly object _SharedObject = new();

        private bool _closed = false;
        public static bool Closed => Instance._closed;
        public static bool Running => !Closed;

        private Containers.Queue<Thread> _threadQueue = new();

        private void Close()
        {
            Debug.Assert(Thread.CurrentThread.ManagedThreadId != _MainId);

            _closed = true;

            foreach (Thread t in _threadQueue)
                t.Join();

            /*Thread.Sleep(1000 * 5);*/

            Console.WriteLine("Close!");
            lock (_SharedObject) Monitor.Pulse(_SharedObject);
        }

        private Application()
        {
            Console.CancelKeyPress += (sender, e) => Close();
        }

        private void HandleContext(object? obj)
        {
            Debug.Assert(obj != null);
            IThreadContext ctx = (IThreadContext)obj;
            ctx.Call();

            Debug.Assert(Closed == true);
        }

        private void _Run(IThreadContext ctx)
        {
            Thread thread = new(HandleContext);
            thread.Start(ctx);

            Debug.Assert(thread.ManagedThreadId != _MainId);

            _threadQueue.Enqueue(thread);
        }

        private void Run(StartRoutine f)
        {
            Debug.Assert(Closed == false);

            IThreadContext ctx = new ThreadContext(f);

            _Run(ctx);
        }

        private void Run<T>(StartRoutineWithArg<T> f, T arg)
        {
            Debug.Assert(Closed == false);

            IThreadContext ctx = new ThreadContextWithArg<T>(f, arg);

            _Run(ctx);
        }

        private void FinishMainFunction()
        {
            Debug.Assert(Thread.CurrentThread.ManagedThreadId == _MainId);
            Debug.Assert(Closed == true);

            lock (_SharedObject) Monitor.Wait(_SharedObject);
            
        }

        private static void StartCoreRoutine()
        {
            
        }

        public static void Main()
        {
            Debug.Assert(Thread.CurrentThread.ManagedThreadId == _MainId);

            Console.WriteLine("Hello, World!");

            

            ushort port = 25565;

            Listener listener = new();
            Instance.Run(listener.Accept, port);

            while (Running)
            {
                Thread.Sleep(1000);
            }

            Instance.FinishMainFunction();
        }
    }
}
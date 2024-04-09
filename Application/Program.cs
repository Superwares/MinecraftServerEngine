using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        protected abstract void WriteData(Buffer buffer);

        public void Write(Buffer buffer)
        {
            buffer.WriteInt(Id, true);
            WriteData(buffer);
        }

    }

    public abstract class HandshakingPacket(int id) : Packet(id)
    {
        public override States State => States.Handshaking;

    }

    public abstract class StatusPacket(int id) : Packet(id)
    {
        public override States State => States.Status;

    }

    public abstract class LoginPacket(int id) : Packet(id)
    {
        public override States State => States.Login;

    }

    public abstract class PlayingPacket(int id) : Packet(id)
    {
        public override States State => States.Playing;
    }

    public abstract class ServerboundHandshakingPacket(int id) : HandshakingPacket(id)
    {
        public static readonly int SetProtocolPacketId = 0x00;

        public override WhereBound BoundTo { get { return WhereBound.Serverbound; } }

    }

    public abstract class ClientboundStatusPacket(int id) : StatusPacket(id)
    {
        public static readonly int ResponsePacketId = 0x00;
        public static readonly int PongPacketId = 0x01;

        public override WhereBound BoundTo { get { return WhereBound.Clientbound; } }

    }

    public abstract class ServerboundStatusPacket(int id) : StatusPacket(id)
    {
        public static readonly int RequestPacketId = 0x00;
        public static readonly int PingPacketId = 0x01;

        public override WhereBound BoundTo { get { return WhereBound.Serverbound; } }

    }

    public abstract class ClientboundLoginPacket(int id) : LoginPacket(id)
    {
        public static readonly int DisconnectPacketId = 0x00;
        public static readonly int EncryptionRequestPacketId = 0x01;
        public static readonly int LoginSuccessPacketId = 0x02;
        public static readonly int SetCompressionPacketId = 0x03;

        public override WhereBound BoundTo => WhereBound.Clientbound;

    }

    public abstract class ServerboundLoginPacket(int id) : LoginPacket(id)
    {
        public static readonly int StartLoginPacketId = 0x00;
        public static readonly int EncryptionResponsePacketId = 0x01;

        public override WhereBound BoundTo => WhereBound.Clientbound;

    }

    public abstract class ClientboundPlayingPacket(int id) : PlayingPacket(id)
    {
        public static readonly int LoadChunkPacketId = 0x20;
        public static readonly int UnloadChunkPacketId = 0x1D;
        public static readonly int SetPlayerAbilitiesId = 0x2C;

        public override WhereBound BoundTo => WhereBound.Clientbound;

    }

    public abstract class ServerboundPlayingPacket(int id) : PlayingPacket(id)
    {
        public override WhereBound BoundTo => WhereBound.Serverbound;

    }

    public class SetProtocolPacket : ServerboundHandshakingPacket
    {
        public readonly int Version;
        public readonly string Hostname;
        public readonly ushort Port;
        public readonly States NextState;

        public static States ReadNextState(Buffer buffer)
        {
            int a = buffer.ReadInt(true);
            States nextState = (a == 1 ? States.Status : States.Login);
            if (!(a == 1 || a == 2))
                throw new InvalidEncodingException();

            return nextState;
        }

        public static SetProtocolPacket Read(Buffer buffer)
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

        protected override void WriteData(Buffer buffer)
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

    public class ResponsePacket : ClientboundStatusPacket
    {

        public readonly int MaxPlayers;
        public readonly int OnlinePlayers;
        public readonly string Description;

        public static ResponsePacket Read(Buffer buffer)
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

        protected override void WriteData(Buffer buffer)
        {
            // TODO
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

        public PongPacket(long payload) : base(PongPacketId)
        {
            Payload = payload;
        }

        protected override void WriteData(Buffer buffer)
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

        public RequestPacket() : base(RequestPacketId) { }

        protected override void WriteData(Buffer buffer) { }

    }

    public class PingPacket : ServerboundStatusPacket
    {
        public readonly long Payload;

        public static PingPacket Read(Buffer buffer)
        {
            return new(buffer.ReadLong());
        }

        private PingPacket(long payload) : base(PingPacketId)
        {
            Payload = payload;
        }

        public PingPacket() : this(DateTime.Now.Ticks) { }

        protected override void WriteData(Buffer buffer)
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

        public DisconnectPacket(string reason) : base(DisconnectPacketId)
        {
            Reason = reason;
        }

        protected override void WriteData(Buffer buffer)
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

        private EncryptionRequestPacket() : base(EncryptionRequestPacketId)
        {
            throw new NotImplementedException();
        }

        protected override void WriteData(Buffer buffer)
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
            : base(LoginSuccessPacketId)
        {
            UserId = userId;
            Username = username;
        }

        protected override void WriteData(Buffer buffer)
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
            : base(SetCompressionPacketId)
        {
            Threshold = threshold;
        }

        protected override void WriteData(Buffer buffer)
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

        public StartLoginPacket(string username) : base(StartLoginPacketId)
        {
            Username = username;
        }

        protected override void WriteData(Buffer buffer)
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

        public EncryptionResponsePacket() : base(EncryptionResponsePacketId)
        {
            throw new NotImplementedException();
        }

        protected override void WriteData(Buffer buffer)
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

        public static LoadChunk Read(Buffer buffer)
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

        protected override void WriteData(Buffer buffer)
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

        public static UnloadChunk Read(Buffer buffer)
        {
            throw new NotImplementedException();
        }

        public UnloadChunk(int xChunk, int zChunk) : base(UnloadChunkPacketId)
        {
            XChunk = xChunk;
            ZChunk = zChunk;
        }

        protected override void WriteData(Buffer buffer)
        {
            buffer.WriteInt(XChunk);
            buffer.WriteInt(ZChunk);
        }

    }

    public class SetPlayerAbilities : ClientboundPlayingPacket
    {
        private readonly byte _flags;
        private readonly float _flyingSpeed;
        private readonly float _fovModifier;  // fov * _fovModifier;

        public static SetPlayerAbilities Read(Buffer buffer)
        {
            throw new NotImplementedException();
        }

        public SetPlayerAbilities(
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

        protected override void WriteData(Buffer buffer)
        {
            buffer.WriteByte(_flags);
            buffer.WriteFloat(_flyingSpeed);
            buffer.WriteFloat(_fovModifier);
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

        public readonly int X;
        public readonly int Z;

        public Chunk(int x, int z)
        {
            X = x;
            Z = z;
        }

    }

    public abstract class Entity
    {
        private Vector3 _p;
        public Vector3 Position => _p;

        internal Entity(Vector3 p)
        {
            _p = p;
                
        }
    }

    public abstract class LivingEntity : Entity
    {
        /*private int _health;*/

        public LivingEntity(Vector3 p) : base(p) { }
    }

    public class Player : LivingEntity
    {

        public Player(Vector3 p) : base(p) { }
    }

    public class NumberList
    {
        private readonly int _MIN = 0;
        private readonly int _MAX = int.MaxValue;

        private class Node(int from, int to)
        {
            public int from = from, to = to;
            public Node? next = null;

        }

        private Node _first;

        public NumberList()
        {
            _first = new(_MIN, _MAX);
        }

        ~NumberList()
        {
            Debug.Assert(_first != null);
            Debug.Assert(_first.next == null);
            Debug.Assert(_first.from == _MIN);
            Debug.Assert(_first.to == _MAX);
        }

        public int Alloc()
        {
            int from = _first.from, to = _first.to;
            Debug.Assert(from <= to);

            int number;

            if (from < to)
            {
                number = from++;
                _first.from = from;
            }
            else
            {
                Debug.Assert(from == to);

                number = from;
                Node? next = _first.next;
                Debug.Assert(next != null);
                _first = next;
            }

            return number;
        }

        public void Dealloc(int number)
        {
            Debug.Assert(_first != null);

            Node? prev;
            Node? current = _first;

            int from = current.from,
                to = current.to;
            Debug.Assert(from <= to);
            Debug.Assert(!(from <= number && number <= to));

            if (number < from)
            {
                if (from > 0)
                {
                    if (number == (from - 1))
                    {
                        current.from--;
                    }
                    else
                    {
                        prev = new(number, number);
                        prev.next = current;
                        _first = prev;
                    }
                }
                else
                    Debug.Assert(false);
            }
            else
            {
                do
                {
                    Debug.Assert(current.from <= current.to);
                    Debug.Assert(!(current.from <= number && number <= current.to));
                    Debug.Assert(current.to < number);

                    prev = current;
                    current = prev.next;
                    Debug.Assert(current != null);
                }
                while (!(prev.to < number && number < current.from));

                to = prev.to;
                from = current.from;

                if ((to + 1) == (from - 1))
                {
                    Debug.Assert((to + 1) == number);
                    prev.to = current.to;
                    prev.next = current.next;
                }
                else if ((to + 1) < number && number < (from - 1))
                {
                    Node between = new(number, number);
                    between.next = current;
                    prev.next = between;
                }
                else if ((to + 1) == number)
                {
                    Debug.Assert((to + 1) + 1 < from);
                    prev.to++;
                }
                else
                {
                    Debug.Assert(to < (from - 1) - 1);
                    current.from--;
                }
            }

        }


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

    public class Connection : IDisposable
    {
        private bool _disposed = false;

        public readonly int Id;

        private Client _client;

        public readonly Guid UserId;
        public readonly string Username;

        internal Connection(int id, Client client, Guid userId, string username)
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
            throw new NotImplementedException();
        }

        public void SendPacket(ClientboundPlayingPacket packet)
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed == false)
            {
                if (disposing == true)
                {
                    // managed objects
                }

                // unmanaged objects
                _client.Dispose();

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
            _client.Dispose();
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

        private readonly struct ListenerContext(ushort port)
        {
            public readonly ushort Port = port;
        }

        private static readonly TimeSpan PendingTimeout = TimeSpan.FromSeconds(1);

        private static readonly Thread _MainThread = Thread.CurrentThread;
        private static readonly int _MainId = _MainThread.ManagedThreadId;
        private static readonly Application Instance = new();

        private readonly NumberList numberList = new();

        private readonly object _SharedObject = new();

        private bool _closed = false;
        public static bool Closed => Instance._closed;
        public static bool Running => !Closed;

        private Queue<Thread> _threads = new();

        private Dictionary<(int, int), Chunk> _chunks = new();

        private Queue<Connection> _newJoinedConnections = new();
        private Queue<Connection> _connections = new();

        private void Close()
        {
            Debug.Assert(Thread.CurrentThread.ManagedThreadId != _MainId);

            _closed = true;

            foreach (Thread t in _threads)
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

            _threads.Enqueue(thread);
        }

        private static void Run(StartRoutine f)
        {
            Debug.Assert(Closed == false);

            IThreadContext ctx = new ThreadContext(f);

            Instance._Run(ctx);
        }

        private static void Run<T>(StartRoutineWithArg<T> f, T arg)
        {
            Debug.Assert(Closed == false);

            IThreadContext ctx = new ThreadContextWithArg<T>(f, arg);

            Instance._Run(ctx);
        }

        private void FinishMainFunction()
        {
            Debug.Assert(Thread.CurrentThread.ManagedThreadId == _MainId);
            Debug.Assert(Closed == true);

            lock (_SharedObject) Monitor.Wait(_SharedObject);
            
        }

        private int HandleVisitors(Queue<Client> visitors, Queue<int> levelQueue)
        {
            int count = visitors.Count;
            Debug.Assert(count == levelQueue.Count);
            if (count == 0) return 0;

            bool close;
            bool loginSuccess;

            for (; count > 0; --count)
            {
                close = false;
                loginSuccess = false;

                Client visitor = visitors.Dequeue();
                int level = levelQueue.Dequeue();
                Debug.Assert(level >= 0);

                try
                {
                    if (level == 0)
                    {
                        /*Console.WriteLine("Handshake!");*/
                        Buffer buffer = visitor.RecvBuffer();

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundHandshakingPacket.SetProtocolPacketId != packetId)
                            throw new UnexpectedPacketException();

                        SetProtocolPacket packet = SetProtocolPacket.Read(buffer);

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
                        Buffer buffer = visitor.RecvBuffer();

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundStatusPacket.RequestPacketId != packetId)
                            throw new UnexpectedPacketException();

                        RequestPacket requestPacket = RequestPacket.Read(buffer);

                        // TODO
                        ResponsePacket responsePacket = new(100, 10, "Hello, World!");
                        responsePacket.Write(buffer);
                        visitor.SendBuffer(buffer);

                        level = 2;
                    }

                    if (level == 2)  // Ping
                    {
                        /*Console.WriteLine("Ping!");*/
                        Buffer buffer = visitor.RecvBuffer();

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundStatusPacket.PingPacketId != packetId)
                            throw new UnexpectedPacketException();

                        PingPacket inPacket = PingPacket.Read(buffer);

                        PongPacket outPacket = new(inPacket.Payload);
                        outPacket.Write(buffer);
                        visitor.SendBuffer(buffer);
                    }

                    if (level == 3)  // Start Login
                    {
                        Buffer buffer = visitor.RecvBuffer();

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundLoginPacket.StartLoginPacketId != packetId)
                            throw new UnexpectedPacketException();

                        StartLoginPacket inPacket = StartLoginPacket.Read(buffer);

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
                        Dictionary<string, string>? dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(str);
                        Debug.Assert(dictionary != null);

                        Guid userId = Guid.Parse(dictionary["id"]);
                        string username = dictionary["name"];  // TODO: check username is valid
                        /*Console.WriteLine($"userId: {userId}");
                        Console.WriteLine($"username: {username}");*/

                        // TODO: Handle to throw exception
                        Debug.Assert(inPacket.Username == username);

                        LoginSuccessPacket outPacket = new(userId, username);
                        outPacket.Write(buffer);
                        visitor.SendBuffer(buffer);

                        int id = numberList.Alloc();  // TODO: Must dealloc id when connection is disposed.
                        Connection conn = new(id, visitor, userId, username);
                        _newJoinedConnections.Enqueue(conn);

                        loginSuccess = true;
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
                    Debug.Assert(loginSuccess == false);
                    close = true;

                    if (level >= 3)
                    {
                        throw new NotImplementedException();
                        // TODO: Handle send Disconnect packet.
                    }
                }

                if (loginSuccess == true)
                    continue;

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

            return visitors.Count;
        }

        private void _StartListenerRoutine(ListenerContext _ctx)
        {
            ushort port = _ctx.Port;
            using Socket socket = new(SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint localEndPoint = new(IPAddress.Any, port);
            socket.Bind(localEndPoint);
            socket.Listen();

            Queue<Client> visitors = new();
            // 0: Handshake, 1: Request, 2: Ping, 3: StartLogin
            Queue<int> levelQueue = new();

            while (Running)
            {
                try
                {
                    if (HandleVisitors(visitors, levelQueue) == 0)
                    {
                        socket.Blocking = true;
                    }

                    if (socket.Blocking == true &&
                        socket.Poll(PendingTimeout, SelectMode.SelectRead) == false)
                    {
                        throw new TimeoutException();
                    }

                    Client client = Client.Accept(socket);
                    visitors.Enqueue(client);
                    levelQueue.Enqueue(0);

                    socket.Blocking = false;

                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode != SocketError.WouldBlock)
                        throw new NotImplementedException();
                }
                catch (TimeoutException) { }

            }

            // TODO: Handle client, send Disconnet Packet if the client's step is after StartLogin;
        }

        private static void StartListenerRoutine(ListenerContext _ctx)
        {
            Instance._StartListenerRoutine(_ctx);
        }

        public static void Main()
        {
            Debug.Assert(Thread.CurrentThread.ManagedThreadId == _MainId);

            Console.WriteLine("Hello, World!");
            
            ushort port = 25565;
            Run(StartListenerRoutine, new ListenerContext(port));

            while (Running)
            {
                Thread.Sleep(1000);
            }

            Instance.FinishMainFunction();
        }
    }
}
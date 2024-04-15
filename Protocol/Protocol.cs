using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;  // TODO: Use custom socket object in common library.
using System.Text;
using System.Text.Json;
using System.Threading;
using Applications;
using Containers;

namespace Protocol
{
    

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

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <param name="socket">TODO: Add description.</param>
        /// <returns>TODO: Add description.</returns>
        /// <exception cref="TryAgainException">TODO: Why it's thrown.</exception>
        public static Socket Accept(Socket socket)
        {
            try
            {
                Socket newSocket = socket.Accept();

                return newSocket;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.WouldBlock)
                    throw new TryAgainException();

                throw;
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <param name="socket">TODO: Add description.</param>
        /// <param name="span">TODO: Add description.</param>
        public static bool Poll(Socket socket, TimeSpan span)  // TODO: Use own TimeSpan in common library.
        {
            // TODO: check the socket is binding and listening.

            if (IsBlocking(socket) &&
                socket.Poll(span, SelectMode.SelectRead) == false)
            {
                return false;
            }

            return true;
        }

        public static void SetBlocking(Socket socket, bool f)
        {
            socket.Blocking = f;
        }

        public static bool IsBlocking(Socket socket)
        {
            return socket.Blocking;
        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <param name="socket">TODO: Add description.</param>
        /// <param name="buffer">TODO: Add description.</param>
        /// <param name="offset">TODO: Add description.</param>
        /// <param name="size">TODO: Add description.</param>
        /// <returns></returns>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
        /// <exception cref="TryAgainException">TODO: Why it's thrown.</exception>
        public static int RecvBytes(
            Socket socket, byte[] buffer, int offset, int size)
        {
            try
            {
                int n = socket.Receive(buffer, offset, size, SocketFlags.None);
                if (n == 0)
                    throw new DisconnectedClientException();

                Debug.Assert(n <= size);

                return n;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.WouldBlock)
                    throw new TryAgainException();

                throw;
            }

        }

        /// <summary>
        /// TODO: Add description..
        /// </summary>
        /// <param name="socket">TODO: Add description..</param>
        /// <returns>TODO: Add description..</returns>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
        /// <exception cref="TryAgainException">TODO: Why it's thrown.</exception>
        public static byte RecvByte(Socket socket)
        {
            byte[] buffer = new byte[1];

            int n = RecvBytes(socket, buffer, 0, 1);
            Debug.Assert(n == 1);

            return buffer[0];
        }

        /// <summary>
        /// TODO: Add description..
        /// </summary>
        /// <param name="socket">TODO: Add description..</param>
        /// <param name="data">TODO: Add description..</param>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
        public static void SendBytes(Socket socket, byte[] data)
        {
            try
            {
                Debug.Assert(data != null);
                int n = socket.Send(data);
                Debug.Assert(n == data.Length);
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.ConnectionAborted)
                    throw new DisconnectedClientException();

                throw;
            }

        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <param name="socket">TODO: Add description.</param>
        /// <param name="v">TODO: Add description.</param>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
        public static void SendByte(Socket socket, byte v)
        {
            SendBytes(socket, [v]);
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

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <returns>TODO: Add description.</returns>
        /// <exception cref="UnexpectedClientBehaviorExecption">TODO: WHy it's thrown.</exception>
        private byte ExtractByte()
        {
            Debug.Assert(_last >= _first);
            if (_first == _last)
                throw new EmptyBufferException();

            return _data[_first++];
        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <param name="size">TODO: Add description.</param>
        /// <returns>TODO: Add description.</returns>
        /// <exception cref="UnexpectedClientBehaviorExecption">TODO: WHy it's thrown.</exception>
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

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedClientBehaviorExecption">TODO: Why it's thrown.</exception>
        public bool ReadBool()
        {
            byte data = ExtractByte();
            Debug.Assert(data != 0x01 | data != 0x00);
            return (data > 0x00);
        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedClientBehaviorExecption">TODO: Why it's thrown.</exception>
        public sbyte ReadSbyte()
        {
            return (sbyte)ExtractByte();
        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedClientBehaviorExecption">TODO: Why it's thrown.</exception>
        public byte ReadByte()
        {
            return (byte)ExtractByte();
        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedClientBehaviorExecption">TODO: Why it's thrown.</exception>
        public short ReadShort()
        {
            byte[] data = ExtractBytes(_SHORT_DATATYPE_SIZE);
            Debug.Assert(data.Length == _SHORT_DATATYPE_SIZE);

            return (short)(
                ((short)data[0] << 8) |
                ((short)data[1] << 0));
        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedClientBehaviorExecption">TODO: Why it's thrown.</exception>
        public ushort ReadUshort()
        {
            byte[] data = ExtractBytes(_USHORT_DATATYPE_SIZE);
            Debug.Assert(data.Length == _USHORT_DATATYPE_SIZE);

            return (ushort)(
                ((ushort)data[0] << 8) |
                ((ushort)data[1] << 0));
        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedClientBehaviorExecption">TODO: Why it's thrown.</exception>
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

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedClientBehaviorExecption">TODO: Why it's thrown.</exception>
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

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedClientBehaviorExecption">TODO: Why it's thrown.</exception>
        public float ReadFloat()
        {
            byte[] data = ExtractBytes(_FLOAT_DATATYPE_SIZE);
            Debug.Assert(data.Length == _FLOAT_DATATYPE_SIZE);
            Array.Reverse(data);
            return BitConverter.ToSingle(data);
        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedClientBehaviorExecption">TODO: Why it's thrown.</exception>
        public double ReadDouble()
        {
            byte[] data = ExtractBytes(_DOUBLE_DATATYPE_SIZE);
            Debug.Assert(data.Length == _DOUBLE_DATATYPE_SIZE);
            Array.Reverse(data);
            return BitConverter.ToDouble(data);
        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <param name="decode">TODO: Add description.</param>
        /// <returns>TODO: Add description.</returns>
        /// <exception cref="UnexpectedClientBehaviorExecption">TODO: Why it's thrown.</exception>
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

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <param name="decode">TODO: Add description.</param>
        /// <returns>TODO: Add description.</returns>
        /// <exception cref="UnexpectedClientBehaviorExecption">TODO: Why it's thrown.</exception>
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

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedClientBehaviorExecption">TODO: Why it's thrown.</exception>
        public string ReadString()
        {
            int size = ReadInt(true);
            Debug.Assert(size >= 0);

            byte[] data = ExtractBytes(size);
            return Encoding.UTF8.GetString(data);
        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedClientBehaviorExecption">TODO: Why it's thrown.</exception>
        public Guid ReadGuid()
        {
            byte[] data = ExtractBytes(_GUID_DATATYPE_SIZE);
            Debug.Assert(data.Length == _GUID_DATATYPE_SIZE);

            return new Guid(data);
        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        internal byte[] ReadData()
        {
            // UnexpectedBehaviorExecption is not handled by try/catch.
            return ExtractBytes(Size);
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

        public struct Position : IEquatable<Position>
        {
            public int x, z;

            public Position(int x, int z)
            {
                this.x = x; this.z = z;
            }

            public static Position Convert(Entity.Position pos)
            {
                return new(
                    (pos.x >= 0) ? ((int)pos.x / Width) : (((int)pos.x / (Width + 1)) - 1),
                    (pos.z >= 0) ? ((int)pos.z / Width) : (((int)pos.z / (Width + 1)) - 1));
            }

            public static (Position, Position) GenerateGridAround(Position center, int d)
            {
                Debug.Assert(d >= 0);
                if (d == 0)
                    return (center, center);

                int xMax = center.x + d, zMax = center.z + d,
                    xMin = center.x - d, zMin = center.z - d;

                /*int a = (2 * d) + 1;
                int length = a * a;
                Position[] positions = new Position[length];

                int i = 0;
                for (int z = zMin; z <= zMax; ++z)
                {
                    for (int x = xMin; x <= xMax; ++x)
                    {
                        positions[i++] = new(x, z);
                    }
                }
                Debug.Assert(i == length);*/

                Debug.Assert(xMax > xMin);
                Debug.Assert(zMax > zMin);
                return (new(xMax, zMax), new(xMin, zMin));
            }

            public static (Position, Position) GenerateGridBetween(
                Position max1, Position min1, Position max2, Position min2)
            {
                Position max3 = new(Math.Min(max1.x, max2.x), Math.Min(max1.z, max2.z)),
                    min3 = new(Math.Max(min1.x, min2.x), Math.Max(min1.z, min2.z));
                if (max3.x < min3.x)
                    (max3.x, min3.x) = (min3.x, max3.x);
                if (max3.z < min3.z)
                    (max3.z, min3.z) = (min3.z, max3.z);

                return (max3, min3);
            }

            public bool Equals(Position other)
            {
                return (x == other.x) && (z == other.z);
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

        internal static (int, byte[]) Write(Chunk chunk)
        {
            Buffer buffer = new();

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
                    buffer.WriteByte(127);  // Void Biome
                }
            }

            return (mask, buffer.ReadData());
        }

        internal static (int, byte[]) Write()
        {
            Buffer buffer = new();

            int mask = 0;
            Debug.Assert(SectionTotalCount == 16);

            // TODO: biomes
            for (int z = 0; z < Width; ++z)
            {
                for (int x = 0; x < Width; ++x)
                {
                    buffer.WriteByte(127);  // Void Biome
                }
            }

            return (mask, buffer.ReadData());
        }

        public readonly Position p;

        public Chunk(Position p)
        {
            this.p = p;
        }

    }

    public abstract class Entity
    {
        public struct Position : IEquatable<Position>
        {
            public double x, y, z;

            public Position(double x, double y, double z)
            {
                this.x = x; this.y = y; this.z = z;
            }

            public static Position Convert(Chunk.Position p)
            {
                throw new NotImplementedException();
            }

            public bool Equals(Position other)
            {
                return (x == other.x) && (y == other.y) && (z == other.z);

            }
        }

        public struct Look
        {
            public const float MaxYaw = 180, MinYaw = -180;
            public const float MaxPitch = 90, MinPitch = -90;

            public float yaw, pitch;

            public Look(float yaw, float pitch)
            {
                this.yaw = yaw;
                this.pitch = pitch;
            }

        }


        public readonly int Id;

        public Position posPrev, pos;
        public Look look;
        public bool onGround;

        internal Entity(int id, Position pos, Look look, bool onGround)
        {
            Id = id;
            this.pos = posPrev = pos;
            this.look = look;
            this.onGround = onGround;
        }

    }

    public abstract class LivingEntity : Entity
    {
        /*private int _health;*/

        public LivingEntity(int id, Position pos, Look look, bool onGround) 
            : base(id, pos, look, onGround) { }

    }

    public class Player : LivingEntity
    {
        public bool connected = true;

        public Player(
            int id, Position pos, Look look, bool onGround) 
            : base(id, pos, look, onGround) { }

    }

    internal sealed class Client : IDisposable
    {
        private static readonly int _TimeoutCount = 100;

        private static readonly byte _SEGMENT_BITS = 0x7F;
        private static readonly byte _CONTINUE_BIT = 0x80;

        private bool _disposed = false;

        private int _x = 0, _y = 0;
        private byte[]? _data = null;

        private int _count = 0;

        private Socket _socket;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <param name="socket">TODO: Add description.</param>
        /// <returns>TODO: Add description.</returns>
        /// <exception cref="TryAgainException">TODO: Why it's thrown.</exception>
        internal static Client Accept(Socket socket)
        {
            //TODO: Check the socket is Binding and listening correctly.

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

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <returns>TODO: Add description.</returns>
        /// <exception cref="UnexpectedClientBehaviorExecption">TODO: Why it's thrown.</exception>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
        /// <exception cref="TryAgainException">TODO: Why it's thrown.</exception>
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
                    byte v = SocketMethods.RecvByte(_socket);

                    uvalue |= (uint)(v & _SEGMENT_BITS) << position;
                    if ((v & _CONTINUE_BIT) == 0)
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

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <param name="size">TODO: Add description.</param>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
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

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <param name="buffer">TODO: Add description.</param>
        /// <exception cref="UnexpectedClientBehaviorExecption">TODO: Why it's thrown.</exception>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
        /// <exception cref="TryAgainException">TODO: Why it's thrown.</exception>
        public void Recv(Buffer buffer)
        {
            Debug.Assert(!_disposed);

            Debug.Assert(SocketMethods.IsBlocking(_socket) == false);

            try
            {
                if (_data == null)
                {
                    int size = RecvSize();
                    _x = size;
                    _y = 0;

                    if (size == 0) return;

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

                _count = 0;
            }
            catch (TryAgainException)
            {
                /*Console.WriteLine($"count: {_count}");*/
                if (_TimeoutCount <= _count++)
                {
                    
                    throw new DataRecvTimeoutException();
                }
                
                throw;
            }

        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <param name="buffer">TODO: Add description.</param>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
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

    public sealed class TeleportRecord
    {
        private const int MaxTicks = 20;  // 1 seconds

        public readonly int Payload;
        private int _ticks = 0;

        public TeleportRecord(int payload)
        {
            Payload = payload;
        }

        public void Update()
        {
            Debug.Assert(_ticks >= 0);

            if (++_ticks > MaxTicks)
            {
                throw new TeleportConfirmTimeoutException();
            }

        }

    }

    public class Connection : IDisposable
    {
        public sealed class ClientsideSettings(byte renderDistance)
        {
            public const int MinRenderDistance = 2, MaxRenderDistance = 32;

            public int renderDistance = renderDistance;
        }

        private Client _client;

        public readonly int Id;

        public readonly Guid UserId;
        public readonly string Username;

        public readonly ClientsideSettings Settings;

        Queue<Report> _reports;

        private (Chunk.Position, Chunk.Position) _loadedChunkGrid;

        Queue<TeleportRecord> _teleportRecords = new();

        private bool _disposed = false;

        internal Connection(
            int id,
            Client client,
            Guid userId, string username,
            ClientsideSettings settings,
            Queue<Report> reports,
            (Chunk.Position, Chunk.Position) loadedChunkGrid)
        {
            Id = id;

            _client = client;

            UserId = userId;
            Username = username;

            Settings = settings;

            _reports = reports;

            _loadedChunkGrid = loadedChunkGrid;
        }

        ~Connection()
        {
            Dispose(false);
        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <returns>TODO: Add description.</returns>
        /// <exception cref="UnexpectedClientBehaviorExecption">TODO: Why it's thrown.</exception>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
        public void Recv(Player player)
        {
            Debug.Assert(!_disposed);

            using Queue<Control> controls = new();
            using Queue<Confirm> confirms = new();
            
            try
            {
                Buffer buffer = new();

                while (true)
                {
                    _client.Recv(buffer);

                    int packetId = buffer.ReadInt(true);
                    switch (packetId)
                    {
                        default:
                            Console.WriteLine($"packetId: 0x{packetId:X}");
                            throw new NotImplementedException();
                        case ServerboundPlayingPacket.ConfirmTeleportPacketId:
                            {
                                ConfirmTeleportPacket packet = ConfirmTeleportPacket.Read(buffer);
                                confirms.Enqueue(new TeleportConfirm(packet.Payload));
                            }
                            break;
                        case ServerboundPlayingPacket.ClientSettingsPacketId:
                            {
                                ClientSettingsPacket packet = ClientSettingsPacket.Read(buffer);
                                throw new NotImplementedException();
                            }
                            break;
                        case ServerboundPlayingPacket.PlayerPacketId:
                            {
                                PlayerPacket packet = PlayerPacket.Read(buffer);
                                controls.Enqueue(new PlayerOnGroundControl(packet.OnGround));
                            }
                            break;
                        case ServerboundPlayingPacket.PlayerPositionPacketId:
                            {
                                PlayerPositionPacket packet = PlayerPositionPacket.Read(buffer);
                                controls.Enqueue(new PlayerPositionControl(packet.X, packet.Y, packet.Z));
                                controls.Enqueue(new PlayerOnGroundControl(packet.OnGround));
                            }
                            break;
                        case ServerboundPlayingPacket.PlayerPosAndLookPacketId:
                            {
                                PlayerPosAndLookPacket packet = PlayerPosAndLookPacket.Read(buffer);
                                controls.Enqueue(new PlayerPositionControl(packet.X, packet.Y, packet.Z));
                                controls.Enqueue(new PlayerLookControl(packet.Yaw, packet.Pitch));
                                controls.Enqueue(new PlayerOnGroundControl(packet.OnGround));
                            }
                            break;
                        case ServerboundPlayingPacket.PlayerLookPacketId:
                            {
                                PlayerLookPacket packet = PlayerLookPacket.Read(buffer);
                                controls.Enqueue(new PlayerLookControl(packet.Yaw, packet.Pitch));
                                controls.Enqueue(new PlayerOnGroundControl(packet.OnGround));
                            }
                            break;
                    }

                    if (!buffer.Empty)
                        throw new BufferOverflowException();
                }
            }
            catch (TryAgainException) 
            {

            }

            while (!confirms.Empty)
            {
                Confirm _confirm = confirms.Dequeue();
            
                switch (_confirm)
                {
                    case TeleportConfirm teleportConfirm:
                        {
                            if (_teleportRecords.Empty)
                                throw new UnexpectedPacketException();

                            TeleportRecord record = _teleportRecords.Dequeue();
                            if (record.Payload != teleportConfirm.Payload)
                                throw new UnexpectedValueException("TeleportPayload");
                        }
                        break;
                    case ClientSettingsConfirm:
                        throw new NotImplementedException();
                        break;
                }
            }

            while (!controls.Empty)
            {
                if (!_teleportRecords.Empty)
                {
                    Console.Write("Ignore Any Controls");
                    break;
                }

                Control _control = controls.Dequeue();

                switch (_control)
                {
                    case PlayerOnGroundControl playerOnGroundControl:
                        {
                            player.onGround = playerOnGroundControl.OnGround;
                        }
                        break;
                    case PlayerPositionControl playerPositionControl:
                        {
                            player.posPrev = player.pos;
                            player.pos = playerPositionControl.Pos;

                            // load/unload chunks
                        }
                        break;
                    case PlayerLookControl playerLookControl:
                        {
                            player.look = playerLookControl.Look;
                        }
                        break;
                }

            }

            controls.Flush();

            if (!_teleportRecords.Empty)
            {
                int count = _teleportRecords.Count;
                for (int i = 0; i < count; ++i)
                {
                    TeleportRecord record = _teleportRecords.Dequeue();
                    record.Update();
                    _teleportRecords.Enqueue(record);
                }
            }

        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <param name="packet">TODO: Add description.</param>
        public void Send()
        {
            Debug.Assert(!_disposed);

            using Buffer buffer = new();

            while (!_reports.Empty)
            {
                Report report = _reports.Dequeue();
                
                if (report is TeleportReport teleportReport)
                {
                    TeleportRecord record = new(teleportReport.Payload);
                    _teleportRecords.Enqueue(record);
                }

                report.Write(buffer);
                _client.Send(buffer);

                Debug.Assert(buffer.Empty);
            }

            Debug.Assert(_reports.Empty);
        }

        // TODO: Make chunks to readonly using interface? in this function.
        public void UpdateChunks(Table<Chunk.Position, Chunk> chunks, Player player)  
        {
            Console.WriteLine("UpdateChunks!");
            Chunk.Position pChunkCenter = Chunk.Position.Convert(player.pos);
            int d = Settings.renderDistance;
            Debug.Assert(d >= ClientsideSettings.MinRenderDistance);
            Debug.Assert(d <= ClientsideSettings.MaxRenderDistance);

            (Chunk.Position pChunkMax, Chunk.Position pChunkMin) = 
                Chunk.Position.GenerateGridAround(pChunkCenter, d);
            Debug.Assert(pChunkMax.x > pChunkMin.x);
            Debug.Assert(pChunkMax.z > pChunkMin.z);

            (Chunk.Position pChunkPrevMax, Chunk.Position pChunkPrevMin) = _loadedChunkGrid;
            Debug.Assert(pChunkPrevMax.x > pChunkPrevMin.x);
            Debug.Assert(pChunkPrevMax.z > pChunkPrevMin.z);

            if (pChunkMax.Equals(pChunkPrevMax) && pChunkMin.Equals(pChunkPrevMin)) 
                return;

            (Chunk.Position pChunkBetweenMax, Chunk.Position pChunkBetweenMin)
                = Chunk.Position.GenerateGridBetween(pChunkMax, pChunkMin, pChunkPrevMax, pChunkPrevMin);
            Debug.Assert(pChunkBetweenMax.x > pChunkBetweenMin.x);
            Debug.Assert(pChunkBetweenMax.z > pChunkBetweenMin.z);

            Report? report = null;

            // Load Chunks
            for (int z = pChunkMin.z; z <= pChunkMax.z; ++z)
            {
                for (int x = pChunkMin.x; x <= pChunkMax.x; ++x)
                {
                    if (x <= pChunkBetweenMax.x && x >= pChunkBetweenMin.x &&
                        z <= pChunkBetweenMax.z && z >= pChunkBetweenMin.x)
                        continue;

                    Chunk.Position pChunkLoad = new(x, z);

                    if (chunks.Contains(pChunkLoad))
                    {
                        Chunk chunk = chunks.Lookup(pChunkLoad);
                        report = new LoadChunkReport(chunk);
                    }
                    else
                        report = new LoadEmptyChunkReport(pChunkLoad);

                    Debug.Assert(report != null);
                    _reports.Enqueue(report);
                }
            }

            // Unload Chunks
            for (int z = pChunkPrevMin.z; z <= pChunkPrevMax.z; ++z)
            {
                for (int x = pChunkPrevMin.x; x <= pChunkPrevMax.x; ++x)
                {
                    if (x <= pChunkBetweenMax.x && x >= pChunkBetweenMin.x &&
                        z <= pChunkBetweenMax.z && z >= pChunkBetweenMin.x)
                        continue;

                    Chunk.Position pChunkUnload = new(x, z);

                    report = new UnloadChunkReport(pChunkUnload);
                    _reports.Enqueue(report);
                }
            }

        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing == true)
            {
                // managed objects
                _client.Dispose();
                _reports.Dispose();
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

    

    public class ConnectionListener
    {
        private enum SetupSteps
        {
            JoinGame = 0,
            ClientSettings,
            PluginMessage,
            StartPlay,
        }

        private readonly ConcurrentQueue<
            (Client, Guid, string, int, Connection.ClientsideSettings?, SetupSteps)
            > _clients = new();

        internal void Add(Client client, Guid userId, string username)
        {
            _clients.Enqueue((client, userId, username, -1, null, SetupSteps.JoinGame));
        }

        public void Accept(
            NumList idList,
            Queue<(Connection, Player)> connections, Queue<Player> players,
            Table<int, Queue<Report>> reportsTable,
            Table<Chunk.Position, Chunk> _chunks,  // TODO: readonly
            Entity.Position posInitial, Entity.Look lookInitial)
        {
            if (_clients.Empty) return;

            bool start, close;

            int count = _clients.Count;
            for (int i = 0; i < count; ++i)
            {
                using Buffer buffer = new();

                (Client client, 
                    Guid userId, 
                    string username, 
                    int entityId,
                    Connection.ClientsideSettings? settings,
                    SetupSteps step) = 
                    _clients.Dequeue();
                start = close = false;

                try
                {
                    if (step == SetupSteps.JoinGame)
                    {
                        /*Console.WriteLine("JoinGame!");*/

                        Debug.Assert(settings == null);
                        Debug.Assert(entityId == -1);

                        // TODO: If already player exists, use id of that player object, not new alloc id.
                        entityId = idList.Alloc();

                        JoinGamePacket packet = new(entityId, 1, 0, 0, "default", false);  // TODO
                        packet.Write(buffer);
                        client.Send(buffer);

                        step = SetupSteps.ClientSettings;
                    }

                    if (step == SetupSteps.ClientSettings)
                    {
                        /*Console.WriteLine("ClientSettings!");*/

                        Debug.Assert(settings == null);
                        Debug.Assert(entityId >= 0);

                        client.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundPlayingPacket.ClientSettingsPacketId != packetId)
                            throw new UnexpectedPacketException();

                        ClientSettingsPacket packet = ClientSettingsPacket.Read(buffer);

                        if (buffer.Size > 0)
                            throw new BufferOverflowException();

                        settings = new(packet.RenderDistance);
    
                        step = SetupSteps.PluginMessage;
                    }

                    if (step == SetupSteps.PluginMessage)
                    {
                        /*Console.WriteLine("PluginMessage!");*/

                        Debug.Assert(settings != null);
                        Debug.Assert(entityId >= 0);

                        client.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (0x09 != packetId)
                            throw new UnexpectedPacketException();

                        buffer.Flush();

                        if (buffer.Size > 0)
                            throw new BufferOverflowException();

                        step = SetupSteps.StartPlay;
                    }

                    Debug.Assert(!start);
                    Debug.Assert(!close);

                    start = true;
                }
                catch (TryAgainException)
                {
                    Debug.Assert(!start);
                    Debug.Assert(!close);

                    /*Console.WriteLine("TryAgainException!");*/
                }
                catch (UnexpectedClientBehaviorExecption)
                {
                    Debug.Assert(!start);
                    Debug.Assert(!close);

                    buffer.Flush();

                    close = true;

                    // TODO: Send why disconnected...

                    /*Console.WriteLine("UnexpectedBehaviorExecption!");*/
                }
                catch (DisconnectedClientException)
                {
                    Debug.Assert(!start);
                    Debug.Assert(!close);

                    buffer.Flush();

                    close = true;

                    /*Console.WriteLine("DisconnectedException!");*/
                }

                if (!start)
                {
                    if (!close)
                    {
                        Debug.Assert(step >= SetupSteps.JoinGame ? entityId >= 0 : true);
                        Debug.Assert(step >= SetupSteps.PluginMessage ? settings != null: true);

                        _clients.Enqueue((
                            client, 
                            userId, username, 
                            entityId, 
                            settings,
                            step));
                    }
                    else
                    {
                        if (step >= SetupSteps.JoinGame)
                            idList.Dealloc(entityId);

                        client.Close();
                    }

                    continue;
                }



                Debug.Assert(step == SetupSteps.StartPlay);
                Debug.Assert(!close);
                Console.Write("Start init connection!");

                Debug.Assert(settings != null);
                Player player = new(entityId, posInitial, lookInitial, false);

                Queue<Report> reports = new();
                (Chunk.Position, Chunk.Position) loadedChunkGrid;

                {
                    SetPlayerAbilitiesReport report = new(true, true, true, true, 1, 0);
                    reports.Enqueue(report);
                }

                {
                    Report? report = null;

                    // load chunks
                    Chunk.Position c = Chunk.Position.Convert(posInitial);
                    int d = settings.renderDistance;
                    (Chunk.Position pMax, Chunk.Position pMin) = Chunk.Position.GenerateGridAround(c, d);
                    for (int z = pMin.z; z <= pMax.z; ++z)
                    {
                        for (int x = pMin.x; x <= pMax.x; ++x)
                        {
                            Chunk.Position p = new(x, z);

                            if (_chunks.Contains(p))
                            {
                                Chunk chunk = _chunks.Lookup(p);
                                report = new LoadChunkReport(chunk);
                            }
                            else
                                report = new LoadEmptyChunkReport(p);

                            Debug.Assert(report != null);
                            reports.Enqueue(report);
                        }
                    }

                    loadedChunkGrid = (pMax, pMin);

                }

                {
                    // teleport
                    AbsoluteTeleportReport report = new(player.pos, player.look);
                    reports.Enqueue(report);
                }


                Connection conn = new(
                    entityId,
                    client,
                    userId, username,
                    settings, 
                    reports,
                    loadedChunkGrid);

                connections.Enqueue((conn, player));

                // TODO: when player is exists in the world, doesn't enqueue player.
                players.Enqueue(player);

                reportsTable.Insert(entityId, reports);

                Console.Write("Finish init connection!");

            }

        }

    }

    public class GlobalListener
    {
        private static readonly TimeSpan _PendingTimeout = TimeSpan.FromSeconds(1);

        private readonly ConnectionListener _connListener;

        public GlobalListener(ConnectionListener connListener)
        {
            _connListener = connListener;
        }

        private int HandleVisitors(
            Queue<Client> visitors, Queue<int> levelQueue)
        {
            /*Console.Write(".");*/

            int count = visitors.Count;
            Debug.Assert(count == levelQueue.Count);
            if (count == 0) return 0;

            bool close, success;

            for (; count > 0; --count)
            {
                close = success = false;

                Client client = visitors.Dequeue();
                int level = levelQueue.Dequeue();

                /*Console.WriteLine($"count: {count}, level: {level}");*/

                Debug.Assert(level >= 0);
                using Buffer buffer = new();

                try
                {
                    if (level == 0)
                    {
                        /*Console.WriteLine("Handshake!");*/
                        client.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundHandshakingPacket.SetProtocolPacketId != packetId)
                            throw new UnexpectedPacketException();

                        SetProtocolPacket packet = SetProtocolPacket.Read(buffer);

                        if (!buffer.Empty)
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
                        client.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundStatusPacket.RequestPacketId != packetId)
                            throw new UnexpectedPacketException();

                        RequestPacket requestPacket = RequestPacket.Read(buffer);

                        if (!buffer.Empty)
                            throw new BufferOverflowException();

                        // TODO
                        ResponsePacket responsePacket = new(100, 10, "Hello, World!");
                        responsePacket.Write(buffer);
                        client.Send(buffer);

                        level = 2;
                    }

                    if (level == 2)  // Ping
                    {
                        /*Console.WriteLine("Ping!");*/
                        client.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundStatusPacket.PingPacketId != packetId)
                            throw new UnexpectedPacketException();

                        PingPacket inPacket = PingPacket.Read(buffer);

                        if (!buffer.Empty)
                            throw new BufferOverflowException();

                        PongPacket outPacket = new(inPacket.Payload);
                        outPacket.Write(buffer);
                        client.Send(buffer);
                    }

                    if (level == 3)  // Start Login
                    {
                        client.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundLoginPacket.StartLoginPacketId != packetId)
                            throw new UnexpectedPacketException();

                        StartLoginPacket inPacket = StartLoginPacket.Read(buffer);

                        if (!buffer.Empty)
                            throw new BufferOverflowException();

                        // TODO: Check username is empty or invalid.

                        Console.Write("Start http request!");

                        // TODO: Use own http client in common library.
                        using HttpClient httpClient = new();
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

                        Console.Write("Finish http request!");

                        // TODO: Handle to throw exception
                        Debug.Assert(inPacket.Username == username);

                        LoginSuccessPacket outPacket1 = new(userId, username);
                        outPacket1.Write(buffer);
                        client.Send(buffer);

                        // TODO: Must dealloc id when connection is disposed.
                        _connListener.Add(client, userId, username);

                        success = true;
                    }


                    Debug.Assert(buffer.Size == 0);

                    if (!success) close = true;

                }
                catch (TryAgainException)
                {
                    Debug.Assert(success == false);
                    Debug.Assert(close == false);

                    /*Console.Write($"TryAgain!");*/
                }
                catch (UnexpectedClientBehaviorExecption)
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

                }
                catch (DisconnectedClientException)
                {
                    Debug.Assert(success == false);
                    Debug.Assert(close == false);
                    close = true;

                    /*Console.Write("~");*/

                    buffer.Flush();

                    /*Console.Write($"EndofFileException");*/
                }

                Debug.Assert(buffer.Empty);

                if (!success)
                {
                    if (close == false)
                    {
                        visitors.Enqueue(client);
                        levelQueue.Enqueue(level);
                    }
                    else
                    {
                        client.Close();
                    }

                    continue;
                }
                    
                Debug.Assert(close == false);

            }

            return visitors.Count;
        }

        public void StartRoutine(ConsoleApplication app, ushort port)
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
                Console.Write(">");

                try
                {
                    if (!SocketMethods.IsBlocking(socket) &&
                        HandleVisitors(visitors, levelQueue) == 0)
                    {
                        SocketMethods.SetBlocking(socket, true);
                    }

                    if (SocketMethods.Poll(socket, _PendingTimeout))
                    {
                        Client client = Client.Accept(socket);
                        visitors.Enqueue(client);
                        levelQueue.Enqueue(0);

                        SocketMethods.SetBlocking(socket, false);
                    }
                }
                catch (TryAgainException)
                {
                    /*Console.WriteLine("TryAgainException!");*/
                    continue;
                }

            }

            // TODO: Handle client, send Disconnet Packet if the client's step is after StartLogin;
        }

    }


}

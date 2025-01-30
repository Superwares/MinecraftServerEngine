
using Common;

namespace MinecraftPrimitives
{

    // TODO: Check system is little- or big-endian.
    public class MinecraftProtocolDataStream : System.IDisposable
    {
        private const int _EXPANSION_FACTOR = 2;
        private const float _LOAD_FACTOR = 0.7F;

        private const byte _SEGMENT_BITS = 0x7F;
        private const byte _CONTINUE_BIT = 0x80;

        private const int _BOOL_DATATYPE_SIZE = 1;
        private const int _SBYTE_DATATYPE_SIZE = 1;
        private const int _BYTE_DATATYPE_SIZE = 1;
        private const int _SHORT_DATATYPE_SIZE = 2;
        private const int _USHORT_DATATYPE_SIZE = 2;
        private const int _INT_DATATYPE_SIZE = 4;
        private const int _LONG_DATATYPE_SIZE = 8;
        private const int _FLOAT_DATATYPE_SIZE = 4;
        private const int _DOUBLE_DATATYPE_SIZE = 8;
        private const int _UUID_DATATYPE_SIZE = 16;

        private const int _INIT_DATASIZE = 16;

        private bool _disposed = false;

        private int _size;
        private byte[] _data;

        private int _first = 0, _last = 0;
        public int Size
        {
            get
            {
                System.Diagnostics.Debug.Assert(_disposed == false);

                System.Diagnostics.Debug.Assert(_first >= 0);
                System.Diagnostics.Debug.Assert(_last >= _first);
                return _last - _first;
            }
        }

        public bool Empty => (Size == 0);

        public MinecraftProtocolDataStream()
        {
            _size = _INIT_DATASIZE;
            _data = new byte[_INIT_DATASIZE];
        }

        ~MinecraftProtocolDataStream()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        public bool IsEmpty()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(Size >= 0);
            return (Size == 0);
        }

        /// <exception cref="UnexpectedClientBehaviorExecption"></exception>
        private byte ExtractByte()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_last >= _first);
            if (_first == _last)
            {
                throw new EmptyBufferException();
            }

            return _data[_first++];
        }

        /// <exception cref="UnexpectedClientBehaviorExecption"></exception>
        private byte[] ExtractBytes(int size)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(size >= 0);
            if (size == 0)
            {
                return [];
            }
            else if (size == 1)
            {
                return [ExtractByte()];
            }

            System.Diagnostics.Debug.Assert(_last >= _first);

            if (_first + size > _last)
            {
                throw new EmptyBufferException();
            }

            byte[] data = new byte[size];
            System.Array.Copy(_data, _first, data, 0, size);
            _first += size;

            return data;
        }

        private void ExpandData(int addedSize)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(addedSize >= 0);
            if (addedSize == 0)
            {
                return;
            }

            int prevSize = _size,
                newSize = prevSize,

                requiredSize = _last + addedSize;

            if (addedSize > 1)
            {
                while (((float)requiredSize / (float)newSize) >= _LOAD_FACTOR)
                {
                    newSize *= _EXPANSION_FACTOR;
                }
            }
            else
            {
                System.Diagnostics.Debug.Assert(addedSize == 1);

                if (((float)requiredSize / (float)newSize) >= _LOAD_FACTOR)
                {
                    newSize *= _EXPANSION_FACTOR;
                }
            }

            System.Diagnostics.Debug.Assert(prevSize <= newSize);

            _size = newSize;

            var newData = new byte[newSize];
            if (Size > 0)
            {
                System.Array.Copy(_data, _first, newData, _first, Size);
            }
            _data = newData;
        }

        private void InsertByte(byte data)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_last >= _first);

            ExpandData(1);
            _data[_last++] = data;
        }

        private void InsertBytes(byte[] data)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_last >= _first);

            int size = data.Length;
            ExpandData(size);
            System.Array.Copy(data, 0, _data, _last, size);
            _last += size;
        }

        /// <exception cref="UnexpectedClientBehaviorExecption"></exception>
        public byte[] ReadData()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            return ExtractBytes(Size);
        }

        /// <exception cref="UnexpectedClientBehaviorExecption"></exception>
        public byte[] ReadData(int size)
        {
            System.Diagnostics.Debug.Assert(size >= 0);

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (size == 0)
            {
                return [];
            }

            return ExtractBytes(size);
        }

        /// <exception cref="UnexpectedClientBehaviorExecption"></exception>
        public bool ReadBool()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            byte data = ExtractByte();
            System.Diagnostics.Debug.Assert(data != 0x01 | data != 0x00);
            return (data > 0x00);
        }

        /// <exception cref="UnexpectedClientBehaviorExecption"></exception>
        public sbyte ReadSbyte()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            return (sbyte)ExtractByte();
        }

        /// <exception cref="UnexpectedClientBehaviorExecption"></exception>
        public byte ReadByte()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            return (byte)ExtractByte();
        }

        /// <exception cref="UnexpectedClientBehaviorExecption"></exception>
        public short ReadShort()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            byte[] data = ExtractBytes(_SHORT_DATATYPE_SIZE);
            System.Diagnostics.Debug.Assert(data.Length == _SHORT_DATATYPE_SIZE);

            return (short)(
                ((short)data[0] << 8) |
                ((short)data[1] << 0));
        }

        /// <exception cref="UnexpectedClientBehaviorExecption"></exception>
        public ushort ReadUshort()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            byte[] data = ExtractBytes(_USHORT_DATATYPE_SIZE);
            System.Diagnostics.Debug.Assert(data.Length == _USHORT_DATATYPE_SIZE);

            return (ushort)(
                ((ushort)data[0] << 8) |
                ((ushort)data[1] << 0));
        }

        /// <exception cref="UnexpectedClientBehaviorExecption"></exception>
        public int ReadInt()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            byte[] data = ExtractBytes(_INT_DATATYPE_SIZE);
            System.Diagnostics.Debug.Assert(data.Length == _INT_DATATYPE_SIZE);

            return (int)(
                ((int)data[0] << 24) |
                ((int)data[1] << 16) |
                ((int)data[2] << 8) |
                ((int)data[3] << 0));
        }

        /// <exception cref="UnexpectedClientBehaviorExecption"></exception>
        public long ReadLong()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            byte[] data = ExtractBytes(_LONG_DATATYPE_SIZE);
            System.Diagnostics.Debug.Assert(data.Length == _LONG_DATATYPE_SIZE);

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

        /// <exception cref="UnexpectedClientBehaviorExecption"></exception>
        public float ReadFloat()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            byte[] data = ExtractBytes(_FLOAT_DATATYPE_SIZE);
            System.Diagnostics.Debug.Assert(data.Length == _FLOAT_DATATYPE_SIZE);
            System.Array.Reverse(data);
            return System.BitConverter.ToSingle(data);
        }

        /// <exception cref="UnexpectedClientBehaviorExecption"></exception>
        public double ReadDouble()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            byte[] data = ExtractBytes(_DOUBLE_DATATYPE_SIZE);
            System.Diagnostics.Debug.Assert(data.Length == _DOUBLE_DATATYPE_SIZE);
            System.Array.Reverse(data);
            return System.BitConverter.ToDouble(data);
        }

        /// <exception cref="UnexpectedClientBehaviorExecption"></exception>
        public int ReadInt(bool decode)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            if (decode == false)
            {
                return ReadInt();
            }

            uint uvalue = 0;
            int position = 0;

            while (true)
            {
                byte data = ExtractByte();

                uvalue |= (uint)(data & _SEGMENT_BITS) << position;
                if ((data & _CONTINUE_BIT) == 0)
                {
                    break;
                }

                position += 7;

                if (position >= 32)
                {
                    throw new InvalidEncodingException();
                }

                System.Diagnostics.Debug.Assert(position > 0);
            }

            return (int)uvalue;
        }

        /// <exception cref="UnexpectedClientBehaviorExecption"></exception>
        public long ReadLong(bool decode)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            if (decode == false)
            {
                return ReadLong();
            }

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
                {
                    throw new InvalidEncodingException();
                }

                System.Diagnostics.Debug.Assert(position > 0);
            }

            return (long)uvalue;
        }


        /**
          * Size:
          * ≥ 1 
          * ≤ (n×3) + 3
          * 
          * Encodes:
          * A sequence of Unicode scalar values
          * 
          * UTF-8 string prefixed with its size in bytes as a VarInt. Maximum length of n characters, which varies b
          * context. The encoding used on the wire is regular UTF-8, not Java's "slight modification". However, the
          * length of the string for purposes of the length limit is its number of UTF-16 code units, that is, scala
          * values > U+FFFF are counted as two. Up to n × 3 bytes can be used to encode a UTF-8 string comprising n
          * code units when converted to UTF-16, and both of those limits are checked. Maximum n value is 32767. 
          * The + 3 is due to the max size of a valid length VarInt.
          * 
          * <exception cref="UnexpectedClientBehaviorExecption"></exception>
          */
        public string ReadString()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);


            int size = ReadInt(true);
            System.Diagnostics.Debug.Assert(size >= 0);

            byte[] data = ExtractBytes(size);
            return System.Text.Encoding.UTF8.GetString(data);
        }

        /**
         * Modified UTF-8
         * 
         * References:
         * https://docs.oracle.com/en/java/javase/18/docs/api/java.base/java/io/DataInput.html#modified-utf-8
         */
        public string ReadModifiedString()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            // 1. Read 2 bytes indicating the length of the string to be read (Big-Endian)
            int utfLength = ReadByte() << 8 | ReadByte();

            // 2. Initialize StringBuilder to hold the string
            System.Text.StringBuilder result = new(utfLength);

            // 3. Decode as UTF-8
            int bytesRead = 0;
            while (bytesRead < utfLength)
            {
                // Read the first byte
                byte a = ReadByte();
                bytesRead++;

                if ((a & 0x80) == 0)
                {
                    // 1-byte character (0xxxxxxx)
                    result.Append((char)a);
                }
                else if ((a & 0xE0) == 0xC0)
                {
                    // 2-byte character (110xxxxx 10xxxxxx)
                    byte b = ReadByte();
                    bytesRead++;

                    if ((b & 0xC0) != 0x80)
                    {
                        throw new InvalidDecodingException("Invalid UTF-8 sequence");
                    }

                    char decodedChar = (char)(((a & 0x1F) << 6) | (b & 0x3F));
                    result.Append(decodedChar);
                }
                else if ((a & 0xF0) == 0xE0)
                {
                    // 3-byte character (1110xxxx 10xxxxxx 10xxxxxx)
                    byte b = ReadByte();
                    byte c = ReadByte();
                    bytesRead += 2;

                    if ((b & 0xC0) != 0x80 || (c & 0xC0) != 0x80)
                    {
                        throw new InvalidDecodingException("Invalid UTF-8 sequence");
                    }

                    char decodedChar = (char)(((a & 0x0F) << 12) | ((b & 0x3F) << 6) | (c & 0x3F));
                    result.Append(decodedChar);
                }
                else
                {
                    // Invalid byte
                    throw new InvalidDecodingException("Invalid UTF-8 sequence");
                }
            }

            return result.ToString();
        }

        /// <exception cref="UnexpectedClientBehaviorExecption"></exception>
        public System.Guid ReadGuid()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            byte[] data = ExtractBytes(_UUID_DATATYPE_SIZE);
            System.Diagnostics.Debug.Assert(data.Length == _UUID_DATATYPE_SIZE);


            return new System.Guid(data);
        }

        public void WriteData(byte[] data)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            if (data == null)
            {
                return;
            }

            InsertBytes(data);
        }

        public void WriteBool(bool value)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            if (value == true)
            {
                InsertByte(1);
            }
            else
            {
                InsertByte(0);
            }
        }

        public void WriteSbyte(sbyte value)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            InsertByte((byte)value);
        }

        public void WriteByte(byte value)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            InsertByte((byte)value);
        }

        public void WriteShort(short value)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            byte[] data = System.BitConverter.GetBytes(value);
            System.Diagnostics.Debug.Assert(data.Length == _SHORT_DATATYPE_SIZE);
            System.Array.Reverse(data);
            InsertBytes(data);
        }

        public void WriteUshort(ushort value)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            byte[] data = System.BitConverter.GetBytes(value);
            System.Diagnostics.Debug.Assert(data.Length == _USHORT_DATATYPE_SIZE);
            System.Array.Reverse(data);
            InsertBytes(data);
        }

        public void WriteInt(int value)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            byte[] data = System.BitConverter.GetBytes(value);
            System.Diagnostics.Debug.Assert(data.Length == _INT_DATATYPE_SIZE);
            System.Array.Reverse(data);
            InsertBytes(data);
        }

        public void WriteLong(long value)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            byte[] data = System.BitConverter.GetBytes(value);
            System.Diagnostics.Debug.Assert(data.Length == _LONG_DATATYPE_SIZE);
            System.Array.Reverse(data);
            InsertBytes(data);
        }

        public void WriteFloat(float value)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            byte[] data = System.BitConverter.GetBytes(value);
            System.Diagnostics.Debug.Assert(data.Length == _FLOAT_DATATYPE_SIZE);
            System.Array.Reverse(data);
            InsertBytes(data);
        }

        public void WriteDouble(double value)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            byte[] data = System.BitConverter.GetBytes(value);
            System.Diagnostics.Debug.Assert(data.Length == _DOUBLE_DATATYPE_SIZE);
            System.Array.Reverse(data);
            InsertBytes(data);
        }

        public void WriteInt(int value, bool encode)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

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
            System.Diagnostics.Debug.Assert(_disposed == false);

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

        /**
         * Size:
         * ≥ 1 
         * ≤ (n×3) + 3
         * 
         * Encodes:
         * A sequence of Unicode scalar values
         * 
         * UTF-8 string prefixed with its size in bytes as a VarInt. Maximum length of n characters, which varies b
         * context. The encoding used on the wire is regular UTF-8, not Java's "slight modification". However, the
         * length of the string for purposes of the length limit is its number of UTF-16 code units, that is, scala
         * values > U+FFFF are counted as two. Up to n × 3 bytes can be used to encode a UTF-8 string comprising n
         * code units when converted to UTF-16, and both of those limits are checked. Maximum n value is 32767. 
         * The + 3 is due to the max size of a valid length VarInt.
         */
        public void WriteString(string value)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            if (value == null)
            {
                return;
            }

            byte[] data = System.Text.Encoding.UTF8.GetBytes(value);
            WriteInt(data.Length, true);
            InsertBytes(data);
        }

        public void WriteModifiedString(string value)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            if (value == null)
            {
                value = "";
            }

            if (value.Length > 0xFFFF)
            {
                throw new System.ArgumentOutOfRangeException(nameof(value), "String is too long.");
            }

            byte[] data = System.Text.Encoding.UTF8.GetBytes(value);
            WriteShort((short)data.Length);
            InsertBytes(data);
        }

        public void WritePosition(int x, int y, int z)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            if (x >= 1 << 25)
            {
                x -= 1 << 26;
            }
            if (y >= 1 << 11)
            {
                y -= 1 << 12;
            }
            if (z >= 1 << 25)
            {
                z -= 1 << 26;
            }

            //long value = (((long)x & 0x3FFFFFF) << 38) | (((long)z & 0x3FFFFFF) << 12) | ((long)y & 0xFFF);
            long value = (((long)x & 0x3FFFFFF) << 38) | (((long)y & 0xFFF) << 26) | ((long)z & 0x3FFFFFF);

            //MyConsole.Debug($"x: {x}, y: {y}, z: {z}, value: {value}");

            WriteLong(value);
        }

        public void WriteGuid(System.Guid value)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            byte[] _data = value.ToByteArray();
            System.Diagnostics.Debug.Assert(_data.Length == _UUID_DATATYPE_SIZE);

            byte[] data = new byte[16];
            data[0] = _data[3];
            data[1] = _data[2];
            data[2] = _data[1];
            data[3] = _data[0];

            data[4] = _data[5];
            data[5] = _data[4];

            data[6] = _data[7];
            data[7] = _data[6];

            data[8] = _data[8];
            data[9] = _data[9];
            data[10] = _data[10];
            data[11] = _data[11];
            data[12] = _data[12];
            data[13] = _data[13];
            data[14] = _data[14];
            data[15] = _data[15];

            // c8a169ec-895c-426b-a0c4-2d359dd59a26
            //byte[] data = { 0xc8, 0xa1, 0x69, 0xec, 0x89, 0x5c, 0x42, 0x6b, 0xa0, 0xc4, 0x2d, 0x35, 0x9d, 0xd5, 0x9a, 0x26 };
            InsertBytes(data);
        }

        public void Flush()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_size >= _INIT_DATASIZE);
            if (Size == 0)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(_last >= _first);
            _first = _last;
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed == false)
            {

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing == true)
                {
                    // Dispose managed resources.
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                //CloseHandle(handle);
                //handle = IntPtr.Zero;

                // Note disposing has been done.
                _disposed = true;
            }

        }


    }

}



using System;
using System.Diagnostics;
using System.Text;

// TODO
public class E1 : Exception
{
    public E1() : base("No data to read.") { }
}

// TODO
public class E2 : Exception
{
    public E2() : base("Unexpected Data.") { }
}

// TODO: Check system is little- or big-endian.
public class Buffer
{
    private static readonly int _EXPANSION_FACTOR = 2;
    private static readonly float _LOAD_FACTOR = 0.7F;

    private static readonly byte _SEGMENT_BITS = 0x7F;
    private static readonly byte _CONTINUE_BIT = 0x80;

    private static readonly int _BOOL_DATATYPE_SIZE   = 1;
    private static readonly int _SBYTE_DATATYPE_SIZE  = 1;
    private static readonly int _BYTE_DATATYPE_SIZE   = 1;
    private static readonly int _SHORT_DATATYPE_SIZE  = 2;
    private static readonly int _USHORT_DATATYPE_SIZE = 2;
    private static readonly int _INT_DATATYPE_SIZE    = 4;
    private static readonly int _LONG_DATATYPE_SIZE   = 8;
    private static readonly int _FLOAT_DATATYPE_SIZE  = 4;
    private static readonly int _DOUBLE_DATATYPE_SIZE = 8;
    private static readonly int _GUID_DATATYPE_SIZE   = 16;

    private const int _INITIAL_DATA_SIZE = 16;

    private int _size = _INITIAL_DATA_SIZE;
    private byte[] _data = new byte[_INITIAL_DATA_SIZE];

    private int _first = 0, _last = 0;
    public int Size {
        get
        {
            Debug.Assert(_first >= 0);
            Debug.Assert(_last >= _first);
            return _last - _first;
        }
    }
    public Buffer() { }

    public bool IsEmpty()
    {
        Debug.Assert(Size >= 0);
        return (Size == 0);
    }

    private byte ExtractByte()
    {
        Debug.Assert(_last >= _first);
        if (_first == _last)
            throw new E1();

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
            throw new E1();

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
            if(((float)requiredSize / (float)newSize) >= _LOAD_FACTOR)
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
            ((int)data[2] <<  8) | 
            ((int)data[3] <<  0));
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
            ((long)data[6] <<  8) |
            ((long)data[7] <<  0));
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
                throw new E2();
            
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
                throw new E2();

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

}

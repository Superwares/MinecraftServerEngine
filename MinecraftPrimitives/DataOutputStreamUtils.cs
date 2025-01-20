
namespace MinecraftPrimitives
{
    internal static class DataOutputStreamUtils
    {
        public static void WriteByte(System.IO.Stream s, int value)
        {
            System.Diagnostics.Debug.Assert(s != null);

            if (value < 0 || value > 255)
            {
                throw new System.ArgumentOutOfRangeException(nameof(value), "Value must be between 0 and 255.");
            }
            s.WriteByte((byte)value);
        }

        public static void WriteShort(System.IO.Stream s, int value)
        {
            System.Diagnostics.Debug.Assert(s != null);

            if (value < short.MinValue || value > short.MaxValue)
            {
                throw new System.ArgumentOutOfRangeException(nameof(value), "Value must be a valid 16-bit integer.");
            }

            s.WriteByte((byte)((value >> 8) & 0xFF));
            s.WriteByte((byte)(value & 0xFF));
        }

        public static void WriteInt(System.IO.Stream s, int value)
        {
            System.Diagnostics.Debug.Assert(s != null);

            s.WriteByte((byte)((value >> 24) & 0xFF));
            s.WriteByte((byte)((value >> 16) & 0xFF));
            s.WriteByte((byte)((value >> 8) & 0xFF));
            s.WriteByte((byte)(value & 0xFF));
        }

        public static void WriteLong(System.IO.Stream s, long value)
        {
            System.Diagnostics.Debug.Assert(s != null);

            for (int i = 7; i >= 0; i--)
            {
                s.WriteByte((byte)((value >> (i * 8)) & 0xFF));
            }
        }

        public static void WriteFloat(System.IO.Stream s, float value)
        {
            System.Diagnostics.Debug.Assert(s != null);

            byte[] bytes = System.BitConverter.GetBytes(value);
            if (System.BitConverter.IsLittleEndian)
            {
                System.Array.Reverse(bytes);
            }
            s.Write(bytes, 0, bytes.Length);
        }

        public static void WriteDouble(System.IO.Stream s, double value)
        {
            System.Diagnostics.Debug.Assert(s != null);

            byte[] bytes = System.BitConverter.GetBytes(value);
            if (System.BitConverter.IsLittleEndian)
            {
                System.Array.Reverse(bytes);
            }
            s.Write(bytes, 0, bytes.Length);
        }

        public static void WriteModifiedUtf8String(System.IO.Stream s, string value)
        {
            System.Diagnostics.Debug.Assert(s != null);

            if (value == null) throw new System.ArgumentNullException(nameof(value));

            byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(value);
            if (utf8Bytes.Length > ushort.MaxValue)
            {
                throw new System.ArgumentException("String is too long for UTF-8 encoding.");
            }

            // Write length
            WriteShort(s, utf8Bytes.Length);

            // Write string bytes
            s.Write(utf8Bytes, 0, utf8Bytes.Length);
        }

        public static void Write(System.IO.Stream s, byte[] bytes)
        {
            System.Diagnostics.Debug.Assert(s != null);

            if (bytes == null)
            {
                return;
            }

            s.Write(bytes, 0, bytes.Length);
        }
    }
}

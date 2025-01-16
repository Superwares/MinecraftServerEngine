using System;
using System.Text;
using System.IO;

namespace MinecraftPrimitives
{
    internal static class DataInputStreamUtils
    {
        public static int ReadByte(Stream s)
        {
            int value = s.ReadByte();
            if (value < 0)
            {
                throw new Exception("EOF");
            }

            return value;
        }

        public static int ReadShort(Stream s)
        {
            int b0 = s.ReadByte();
            int b1 = s.ReadByte();
            if ((b0 | b1) < 0)
            {
                throw new Exception("EOF");
            }
            return (b0 << 8) + (b1 << 0);
        }

        public static int ReadInt(Stream s)
        {
            int b0 = s.ReadByte();
            int b1 = s.ReadByte();
            int b2 = s.ReadByte();
            int b3 = s.ReadByte();
            if ((b0 | b1 | b2 | b3) < 0)
            {
                throw new Exception("EOF");
            }

            return (b0 << 24) + (b1 << 16) + (b2 << 8) + (b3 << 0);
        }

        public static long ReadLong(Stream s)
        {
            int b0 = s.ReadByte();
            int b1 = s.ReadByte();
            int b2 = s.ReadByte();
            int b3 = s.ReadByte();
            int b4 = s.ReadByte();
            int b5 = s.ReadByte();
            int b6 = s.ReadByte();
            int b7 = s.ReadByte();
            if ((b0 | b1 | b2 | b3 | b4 | b5 | b6 | b7) < 0)
            {
                throw new Exception("EOF");
            }

            return ((long)(b0 & 0xff) << 56)
                + ((long)(b1 & 0xff) << 48)
                + ((long)(b2 & 0xff) << 40)
                + ((long)(b3 & 0xff) << 32)
                + ((long)(b4 & 0xff) << 24)
                + ((long)(b5 & 0xff) << 16)
                + ((long)(b6 & 0xff) << 8)
                + ((long)(b7 & 0xff) << 0);
        }

        public static float ReadFloat(Stream s)
        {
            int b0 = s.ReadByte();
            int b1 = s.ReadByte();
            int b2 = s.ReadByte();
            int b3 = s.ReadByte();
            if ((b0 | b1 | b2 | b3) < 0)
            {
                throw new Exception("EOF");
            }

            /**
             * Reads four input bytes and returns a float value. 
             * It does this by first constructing an int value 
             * in exactly the manner of the readInt method, 
             * then converting this int value to a float in exactly 
             * the manner of the method Float. intBitsToFloat. 
             * This method is suitable for reading bytes written by 
             * the writeFloat method of interface DataOutput.
             * 
             * Returns: the float value read.
             */
            return BitConverter.ToSingle(new byte[] {
                (byte)b0, (byte)b1, (byte)b2, (byte)b3,
            }, 0);
        }

        public static double ReadDouble(Stream s)
        {
            int b0 = s.ReadByte();
            int b1 = s.ReadByte();
            int b2 = s.ReadByte();
            int b3 = s.ReadByte();
            int b4 = s.ReadByte();
            int b5 = s.ReadByte();
            int b6 = s.ReadByte();
            int b7 = s.ReadByte();
            if ((b0 | b1 | b2 | b3 | b4 | b5 | b6 | b7) < 0)
            {
                throw new Exception("EOF");
            }

            /**
             * Reads eight input bytes and returns a double value. 
             * It does this by first constructing a long value 
             * in exactly the manner of the readLong method, 
             * then converting this long value to a double in exactly 
             * the manner of the method Double. longBitsToDouble. 
             * This method is suitable for reading bytes written by 
             * the writeDouble method of interface DataOutput.
             * 
             * Returns: the double value read.
             */
            return BitConverter.ToDouble(new byte[] {
                (byte)b0, (byte)b1, (byte)b2, (byte)b3,
                (byte)b4, (byte)b5, (byte)b6, (byte)b7,
            }, 0);
        }

        public static string ReadModifiedUtf8String(Stream s)
        {

            // 1. Read 2 bytes indicating the length of the string to be read (Big-Endian)
            int utfLength = ReadByte(s) << 8 | ReadByte(s);

            // 2. Initialize StringBuilder to hold the string
            StringBuilder result = new(utfLength);

            // 3. Decode as UTF-8
            int bytesRead = 0;
            while (bytesRead < utfLength)
            {
                // Read the first byte
                byte a = (byte)ReadByte(s);
                bytesRead++;

                if ((a & 0x80) == 0)
                {
                    // 1-byte character (0xxxxxxx)
                    result.Append((char)a);
                }
                else if ((a & 0xE0) == 0xC0)
                {
                    // 2-byte character (110xxxxx 10xxxxxx)
                    byte b = (byte)ReadByte(s);
                    bytesRead++;

                    if ((b & 0xC0) != 0x80)
                    {
                        throw new FormatException("Invalid UTF-8 sequence");
                    }

                    char decodedChar = (char)(((a & 0x1F) << 6) | (b & 0x3F));
                    result.Append(decodedChar);
                }
                else if ((a & 0xF0) == 0xE0)
                {
                    // 3-byte character (1110xxxx 10xxxxxx 10xxxxxx)
                    byte b = (byte)ReadByte(s);
                    byte c = (byte)ReadByte(s);
                    bytesRead += 2;

                    if ((b & 0xC0) != 0x80 || (c & 0xC0) != 0x80)
                    {
                        throw new FormatException("Invalid UTF-8 sequence");
                    }

                    char decodedChar = (char)(((a & 0x0F) << 12) | ((b & 0x3F) << 6) | (c & 0x3F));
                    result.Append(decodedChar);
                }
                else
                {
                    // Invalid byte
                    throw new FormatException("Invalid UTF-8 sequence");
                }
            }

            return result.ToString();
        }

        public static void Read(Stream s, byte[] bytes)
        {

            /**
             * Returns the number of bytes read. 
             * This can be less than the size of the buffer 
             * if that many bytes are not currently available, or zero (0) 
             * if the buffer's length is zero or the end of the stream has been reached.
             */
            int read = s.Read(bytes, 0, bytes.Length);

            if (read < bytes.Length)
            {
                throw new Exception("EOF");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MinecraftPrimitives
{
    public sealed class NBTCompressedStreamTools
    {
        static string ReadModifiedUtf8String(Stream s)
        {
            var reader = new BinaryReader(s);

            // 1. Read 2 bytes indicating the length of the string to be read (Big-Endian)
            int utfLength = reader.ReadByte() << 8 | reader.ReadByte();

            // 2. Initialize StringBuilder to hold the string
            StringBuilder result = new StringBuilder(utfLength);

            // 3. Decode as UTF-8
            int bytesRead = 0;
            while (bytesRead < utfLength)
            {
                // Read the first byte
                byte a = reader.ReadByte();
                bytesRead++;

                if ((a & 0x80) == 0)
                {
                    // 1-byte character (0xxxxxxx)
                    result.Append((char)a);
                }
                else if ((a & 0xE0) == 0xC0)
                {
                    // 2-byte character (110xxxxx 10xxxxxx)
                    byte b = reader.ReadByte();
                    bytesRead++;

                    if ((b & 0xC0) != 0x80)
                        throw new FormatException("Invalid UTF-8 sequence");

                    char decodedChar = (char)(((a & 0x1F) << 6) | (b & 0x3F));
                    result.Append(decodedChar);
                }
                else if ((a & 0xF0) == 0xE0)
                {
                    // 3-byte character (1110xxxx 10xxxxxx 10xxxxxx)
                    byte b = reader.ReadByte();
                    byte c = reader.ReadByte();
                    bytesRead += 2;

                    if ((b & 0xC0) != 0x80 || (c & 0xC0) != 0x80)
                        throw new FormatException("Invalid UTF-8 sequence");

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

        public static NBTTagCompound Load(Stream s)
        {
            int id = s.ReadByte();
            if (NBTTagCompound.TypeId != id)
            {
                throw new InvalidDataException("Root tag must be a named compound tag");
            }

            ReadModifiedUtf8String(s);

            return NBTTagCompound.Read(s, 0);
        }
    }
}

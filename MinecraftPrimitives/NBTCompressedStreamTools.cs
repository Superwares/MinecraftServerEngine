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

            // 1. 읽을 문자열의 길이를 나타내는 2바이트를 읽음 (Big-Endian)
            int utfLength = reader.ReadByte() << 8 | reader.ReadByte();

            // 2. 문자열을 담을 StringBuilder 초기화
            StringBuilder result = new StringBuilder(utfLength);

            // 3. UTF-8로 디코딩
            int bytesRead = 0;
            while (bytesRead < utfLength)
            {
                // 첫 번째 바이트 읽기
                byte a = reader.ReadByte();
                bytesRead++;

                if ((a & 0x80) == 0)
                {
                    // 1바이트 문자 (0xxxxxxx)
                    result.Append((char)a);
                }
                else if ((a & 0xE0) == 0xC0)
                {
                    // 2바이트 문자 (110xxxxx 10xxxxxx)
                    byte b = reader.ReadByte();
                    bytesRead++;

                    if ((b & 0xC0) != 0x80)
                        throw new FormatException("Invalid UTF-8 sequence");

                    char decodedChar = (char)(((a & 0x1F) << 6) | (b & 0x3F));
                    result.Append(decodedChar);
                }
                else if ((a & 0xF0) == 0xE0)
                {
                    // 3바이트 문자 (1110xxxx 10xxxxxx 10xxxxxx)
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
                    // 잘못된 바이트
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

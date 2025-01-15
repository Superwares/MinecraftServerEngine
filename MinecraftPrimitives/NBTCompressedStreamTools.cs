using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
{
    public sealed class NBTCompressedStreamTools
    {
        static string ReadModifiedUtf8String(Stream stream)
        {
            using (MemoryStream buffer = new MemoryStream())
            {
                int b;
                while ((b = stream.ReadByte()) != -1)
                {
                    if (b == 0xC0) // 수정된 Null 문자의 첫 번째 바이트
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte == 0x80)
                        {
                            buffer.WriteByte(0x00); // Null 문자로 복원
                        }
                        else
                        {
                            throw new InvalidDataException("Invalid modified UTF-8 sequence.");
                        }
                    }
                    else
                    {
                        buffer.WriteByte((byte)b);
                    }
                }

                // UTF-8로 디코딩
                return Encoding.UTF8.GetString(buffer.ToArray());
            }
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

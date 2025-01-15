using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
{
    public sealed class NBTTagString : NBTBase
    {
        public const int TypeId = 8;

        private readonly string value;

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

        public static NBTTagString Read(Stream s, int depth)
        {
            string value = ReadModifiedUtf8String(s);
            return new NBTTagString(value);
        }

        private NBTTagString(string value)
        {
            this.value = value;
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }
    }
}

using Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
{
    public sealed class NBTTagCompound : NBTBase
    {
        public const int TypeId = 10;

        private Table<string, NBTBase> data;

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

        public static NBTTagCompound Read(Stream s, int depth)
        {
            //Console.WriteLine($"Reading stream with depth {depth}");
            System.Diagnostics.Debug.Assert(s != null);
            System.Diagnostics.Debug.Assert(depth >= 0);
            if (depth > 512)
            {
                throw new Exception("Tried to read NBT tag with too high complexity, depth > 512");
            }

            Table<string, NBTBase> data = new Table<string, NBTBase>();

            int id = -1;

            while (true)
            {
                id = s.ReadByte();
                if (NBTTagEnd.TypeId == id)
                {
                    break;
                }

                string key = ReadModifiedUtf8String(s);
                NBTBase value = null;

                switch (id)
                {
                    default:
                        throw new Exception("Unknown type Id");
                    case NBTTagByte.TypeId:
                        value = NBTTagByte.Read(s, depth + 1);
                        break;
                    case NBTTagCompound.TypeId:
                        value = NBTTagCompound.Read(s, depth + 1);
                        break;
                }

                if (data.Contains(key) == true)
                {
                    throw new Exception("Duplicate item in NBT compound");
                }

                data.Insert(key, value);
            }

            return new NBTTagCompound(data);
        }

        private NBTTagCompound(Table<string, NBTBase> data)
        {
            this.data = data;
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }
    }
}

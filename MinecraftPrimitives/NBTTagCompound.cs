using Containers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
{
    public sealed class NBTTagCompound : NBTBase
    {
        public const int TypeId = 10;

        private Table<string, NBTBase> data;

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

            int id;

            while (true)
            {
                id = s.ReadByte();
                if (NBTTagEnd.TypeId == id)
                {
                    break;
                }

                string key = ReadModifiedUtf8String(s);
                NBTBase value;

                switch (id)
                {
                    default:
                        throw new Exception("Unknown type Id");
                    case NBTTagEnd.TypeId:
                        value = NBTTagEnd.Read(s, depth + 1);
                        break;
                    case NBTTagByte.TypeId:
                        value = NBTTagByte.Read(s, depth + 1);
                        break;
                    case NBTTagShort.TypeId:
                        value = NBTTagShort.Read(s, depth + 1);
                        break;
                    case NBTTagInt.TypeId:
                        value = NBTTagInt.Read(s, depth + 1);
                        break;
                    case NBTTagLong.TypeId:
                        value = NBTTagLong.Read(s, depth + 1);
                        break;
                    case NBTTagFloat.TypeId:
                        value = NBTTagFloat.Read(s, depth + 1);
                        break;
                    case NBTTagDouble.TypeId:
                        value = NBTTagDouble.Read(s, depth + 1);
                        break;
                    case NBTTagByteArray.TypeId:
                        value = NBTTagByteArray.Read(s, depth + 1);
                        break;
                    case NBTTagString.TypeId:
                        value = NBTTagString.Read(s, depth + 1);
                        break;
                    case NBTTagList.TypeId:
                        value = NBTTagList.Read(s, depth + 1);
                        break;
                    case NBTTagCompound.TypeId:
                        value = NBTTagCompound.Read(s, depth + 1);
                        break;
                    case NBTTagIntArray.TypeId:
                        value = NBTTagIntArray.Read(s, depth + 1);
                        break;
                    case NBTTagLongArray.TypeId:
                        value = NBTTagLongArray.Read(s, depth + 1);
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

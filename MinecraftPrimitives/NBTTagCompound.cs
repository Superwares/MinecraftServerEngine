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

        public static NBTTagCompound Read(Stream s, int depth)
        {
            //Console.WriteLine($"Reading stream with depth {depth}");
            System.Diagnostics.Debug.Assert(s != null);
            System.Diagnostics.Debug.Assert(depth >= 0);
            if (depth > 512)
            {
                throw new Exception("Tried to read NBT tag with too high complexity, depth > 512");
            }

            Table<string, NBTBase> data = new();

            int typeId;

            while (true)
            {
                typeId = DataInputStreamUtils.ReadByte(s);
                if (NBTTagEnd.TypeId == typeId)
                {
                    break;
                }

                string key = DataInputStreamUtils.ReadModifiedUtf8String(s);
                NBTBase value;

                switch (typeId)
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
                    case NBTTagListBase.TypeId:
                        int _typeId = DataInputStreamUtils.ReadByte(s);
                        switch(_typeId)
                        {
                            case NBTTagEnd.TypeId:
                                value = NBTTagList<NBTTagEnd>.ReadEndArray(s, depth + 1);
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
                            default:
                                throw new Exception("Unknown type Id");

                        }
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

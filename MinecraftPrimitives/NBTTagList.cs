using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
{
    public abstract class NBTTagListBase : NBTBase
    {
        public const int TypeId = 9;

        public static NBTTagList<NBTTagEnd> ReadEndArray(Stream s, int depth)
        {
            System.Diagnostics.Debug.Assert(s != null);
            System.Diagnostics.Debug.Assert(depth >= 0);
            if (depth > 512)
            {
                throw new Exception("Tried to read NBT tag with too high complexity, depth > 512");
            }

            int length = DataInputStreamUtils.ReadInt(s);

            if (length > 0)
            {
                throw new Exception("Missing type on ListTag");
            }

            NBTTagEnd[] list = new NBTTagEnd[length];

            for (int i = 0; i < length; i++)
            {
                list[i] = NBTTagEnd.Read(s, depth + 1);
            }

            return new NBTTagList<NBTTagEnd>(list);
        }

        public static NBTTagList<NBTTagEnd> ReadByteArray(Stream s, int depth)
        {
            System.Diagnostics.Debug.Assert(s != null);
            System.Diagnostics.Debug.Assert(depth >= 0);
            if (depth > 512)
            {
                throw new Exception("Tried to read NBT tag with too high complexity, depth > 512");
            }

            int typeId = DataInputStreamUtils.ReadByte(s);
            int length = DataInputStreamUtils.ReadInt(s);

            if (typeId == 0 && length > 0)
            {
                throw new Exception("Missing type on ListTag");
            }

            NBTBase[] list = new NBTBase[length];

            for (int i = 0; i < length; i++)
            {
                switch (typeId)
                {
                    default:
                        throw new Exception("Unknown type Id");
                    case NBTTagEnd.TypeId:
                        list[i] = NBTTagEnd.Read(s, depth + 1);
                        break;
                    case NBTTagByte.TypeId:
                        list[i] = NBTTagByte.Read(s, depth + 1);
                        break;
                    case NBTTagShort.TypeId:
                        list[i] = NBTTagShort.Read(s, depth + 1);
                        break;
                    case NBTTagInt.TypeId:
                        list[i] = NBTTagInt.Read(s, depth + 1);
                        break;
                    case NBTTagLong.TypeId:
                        list[i] = NBTTagLong.Read(s, depth + 1);
                        break;
                    case NBTTagFloat.TypeId:
                        list[i] = NBTTagFloat.Read(s, depth + 1);
                        break;
                    case NBTTagDouble.TypeId:
                        list[i] = NBTTagDouble.Read(s, depth + 1);
                        break;
                    case NBTTagByteArray.TypeId:
                        list[i] = NBTTagByteArray.Read(s, depth + 1);
                        break;
                    case NBTTagString.TypeId:
                        list[i] = NBTTagString.Read(s, depth + 1);
                        break;
                    case NBTTagList.TypeId:
                        list[i] = NBTTagList.Read(s, depth + 1);
                        break;
                    case NBTTagCompound.TypeId:
                        list[i] = NBTTagCompound.Read(s, depth + 1);
                        break;
                    case NBTTagIntArray.TypeId:
                        list[i] = NBTTagIntArray.Read(s, depth + 1);
                        break;
                    case NBTTagLongArray.TypeId:
                        list[i] = NBTTagLongArray.Read(s, depth + 1);
                        break;
                }
            }

            return new NBTTagList(typeId, list);
        }

    }


    public sealed class NBTTagList<T> : NBTTagListBase
        where T : NBTBase
    {

        private readonly T[] value;

        public static NBTTagList<T> Read(Stream s, int depth)
        {
            if (
            typeof(T).Name == "NBTTagEnd")
            {

            }
            System.Diagnostics.Debug.Assert(s != null);
            System.Diagnostics.Debug.Assert(depth >= 0);
            if (depth > 512)
            {
                throw new Exception("Tried to read NBT tag with too high complexity, depth > 512");
            }

            int length = DataInputStreamUtils.ReadInt(s);

            if (length > 0)
            {
                throw new Exception("Missing type on ListTag");
            }

            T[] list = new T[length];

            for (int i = 0; i < length; i++)
            {
                list[i] = NBTTagEnd.Read(s, depth + 1);
            }

            return new NBTTagList<T>(list);
        }


        public NBTTagList(T[] value)
        {
            this.value = value;
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }
    }
}

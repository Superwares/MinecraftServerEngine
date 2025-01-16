using System;
using System.IO;
using System.Linq;

using Containers;

namespace MinecraftPrimitives
{
    public sealed class NBTTagCompound : NBTTagBase, IReadableNBTTag<NBTTagCompound>
    {
        private readonly struct TagListItem(string key, NBTTagBase value)
        {
            public readonly string Key = key;
            public readonly NBTTagBase Value = value;
        }

        public const int TypeId = 10;

        private readonly Table<string, NBTTagBase> _tagTable;
        private readonly List<TagListItem> _tagList;

        private static NBTTagBase ReadNBTTagList(Stream s, int depth)
        {
            System.Diagnostics.Debug.Assert(s != null);
            System.Diagnostics.Debug.Assert(depth >= 0);

            int id = DataInputStreamUtils.ReadByte(s);
            switch (id)
            {
                case NBTTagEnd.TypeId:
                    return NBTTagList<NBTTagEnd>.Read(s, depth + 1);
                case NBTTagByte.TypeId:
                    return NBTTagList<NBTTagByte>.Read(s, depth + 1);
                case NBTTagShort.TypeId:
                    return NBTTagList<NBTTagShort>.Read(s, depth + 1);
                case NBTTagInt.TypeId:
                    return NBTTagList<NBTTagInt>.Read(s, depth + 1);
                case NBTTagLong.TypeId:
                    return NBTTagList<NBTTagLong>.Read(s, depth + 1);
                case NBTTagFloat.TypeId:
                    return NBTTagList<NBTTagFloat>.Read(s, depth + 1);
                case NBTTagDouble.TypeId:
                    return NBTTagList<NBTTagDouble>.Read(s, depth + 1);
                case NBTTagByteArray.TypeId:
                    return NBTTagList<NBTTagByteArray>.Read(s, depth + 1);
                case NBTTagString.TypeId:
                    return NBTTagList<NBTTagString>.Read(s, depth + 1);
                case NBTTagListBase.TypeId:
                    throw new NotImplementedException();
                case NBTTagCompound.TypeId:
                    return NBTTagList<NBTTagCompound>.Read(s, depth + 1);
                case NBTTagIntArray.TypeId:
                    return NBTTagList<NBTTagIntArray>.Read(s, depth + 1);
                case NBTTagLongArray.TypeId:
                    return NBTTagList<NBTTagLongArray>.Read(s, depth + 1);
                default:
                    throw new Exception("Unknown type Id");

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

            Table<string, NBTTagBase> tagTable = new();
            List<TagListItem> tagList = new();

            int id;

            while (true)
            {
                id = DataInputStreamUtils.ReadByte(s);
                if (NBTTagEnd.TypeId == id)
                {
                    break;
                }

                string key = DataInputStreamUtils.ReadModifiedUtf8String(s);
                NBTTagBase value;

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
                    case NBTTagListBase.TypeId:
                        value = ReadNBTTagList(s, depth + 1);
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

                if (tagTable.Contains(key) == true)
                {
                    throw new Exception("Duplicate item in NBT compound");
                }

                tagTable.Insert(key, value);
                tagList.Append(new TagListItem(key, value));
            }

            return new NBTTagCompound(tagTable, tagList);
        }

        private NBTTagCompound(
            Table<string, NBTTagBase> tagTable,
            List<TagListItem> tagList)
        {
            _tagTable = tagTable;
            _tagList = tagList;
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }

        public T GetNBTTag<T>(string key) where T : NBTTagBase
        {
            try
            {
                NBTTagBase nbtBase = _tagTable.Lookup(key);

                return (T)nbtBase;
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
            catch (InvalidCastException)
            {
                return null;
            }

        }

        public override string ToString()
        {
            string str = "";

            foreach (var item in _tagList)
            {
                string _str = item.Value.ToString();

                string indentedStr = string.Join("\n\t",
                    _str.Split('\n', StringSplitOptions.None));

                //Console.WriteLine($"{item.Key} : {item.Value}");

                str += $"{item.Key}:\n\t{indentedStr}\n";
            }

            return str;
        }
    }
}

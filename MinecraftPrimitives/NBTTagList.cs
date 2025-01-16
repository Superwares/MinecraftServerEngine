using System;
using System.IO;

namespace MinecraftPrimitives
{
    public sealed class NBTTagList<T> : NBTTagListBase, IReadableNBTTag<NBTTagList<T>>
        where T : NBTTagBase, IReadableNBTTag<T>
    {

        private readonly T[] value;

        public static NBTTagList<T> Read(Stream s, int depth)
        {
            System.Diagnostics.Debug.Assert(s != null);
            System.Diagnostics.Debug.Assert(depth >= 0);
            if (depth > 512)
            {
                throw new Exception("Tried to read NBT tag with too high complexity, depth > 512");
            }

            int length = DataInputStreamUtils.ReadInt(s);

            T[] value = new T[length];

            for (int i = 0; i < length; i++)
            {
                value[i] = T.Read(s, depth + 1);
            }

            return new NBTTagList<T>(value);
        }


        public NBTTagList(T[] value)
        {
            this.value = value;
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            string tab = "    ";

            string str = $"NBTTagList<{typeof(T).Name}>({value.Length})";

            str += "[";
            if (value.Length > 0)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    string _str = value[i].ToString();
                    string indentedStr = string.Join($"\n{tab}",
                        _str.Split('\n', StringSplitOptions.RemoveEmptyEntries));

                    str += $"\n{tab}{indentedStr}";

                    if (i < value.Length - 1)
                    {
                        str += $", ";
                    }
                }

                str += "\n";
            }
            str += "]";

            return str;
        }
    }

}

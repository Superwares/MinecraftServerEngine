using System;
using System.Diagnostics;
using System.IO;

namespace MinecraftPrimitives
{
    public sealed class NBTTagList<T> : NBTTagListBase, IReadableNBTTag<NBTTagList<T>>
        where T : NBTTagBase, IReadableNBTTag<T>
    {
        private bool _disposed = false;

        public readonly T[] Data;

        public static NBTTagList<T> Read(Stream s, int depth)
        {
            System.Diagnostics.Debug.Assert(s != null);
            System.Diagnostics.Debug.Assert(depth >= 0);
            if (depth > 512)
            {
                throw new Exception("Tried to read NBT tag with too high complexity, depth > 512");
            }

            int length = DataInputStreamUtils.ReadInt(s);

            T[] data = new T[length];

            for (int i = 0; i < length; i++)
            {
                data[i] = T.Read(s, depth + 1);
            }

            return new NBTTagList<T>(data);
        }


        public NBTTagList(T[] data)
        {
            Data = data;
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            string tab = "    ";

            string str = $"NBTTagList<{typeof(T).Name}>({Data.Length})";

            str += "[";
            if (Data.Length > 0)
            {
                for (int i = 0; i < Data.Length; i++)
                {
                    string _str = Data[i].ToString();
                    string indentedStr = string.Join($"\n{tab}",
                        _str.Split('\n', StringSplitOptions.RemoveEmptyEntries));

                    str += $"\n{tab}{indentedStr}";

                    if (i < Data.Length - 1)
                    {
                        str += $", ";
                    }
                }

                str += "\n";
            }
            str += "]";

            return str;
        }

        public override void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(_disposed == false);

            // Release resources
            foreach (T item in Data)
            {
                item.Dispose();
            }

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

}

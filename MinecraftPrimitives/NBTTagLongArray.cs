using System;
using System.IO;

namespace MinecraftPrimitives
{
    public sealed class NBTTagLongArray : NBTTagBase, IReadableNBTTag<NBTTagLongArray>
    {
        public const int TypeId = 12;

        public readonly long[] Data;

        public static NBTTagLongArray Read(Stream s, int depth)
        {
            int length = DataInputStreamUtils.ReadInt(s);

            long[] data = new long[length];

            for (int i = 0; i < length; ++i)
            {
                
                data[i] = DataInputStreamUtils.ReadLong(s);
            }

            return new NBTTagLongArray(data);
        }

        private NBTTagLongArray(long[] data)
        {
            Data = data;
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}({Data.Length})";
        }
    }
}

using System;
using System.IO;

namespace MinecraftPrimitives
{
    public sealed class NBTTagIntArray : NBTTagBase, IReadableNBTTag<NBTTagIntArray>
    {
        public const int TypeId = 11;

        public readonly int[] Data;

        public static NBTTagIntArray Read(Stream s, int depth)
        {
            int length = DataInputStreamUtils.ReadInt(s);

            int[] data = new int[length];
            
            for (int i = 0; i < length; ++i)
            {
                data[i] = DataInputStreamUtils.ReadInt(s);
            }

            return new NBTTagIntArray(data);
        }

        private NBTTagIntArray(int[] data)
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

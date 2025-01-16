using System;
using System.IO;

namespace MinecraftPrimitives
{
    public sealed class NBTTagByteArray : NBTTagBase, IReadableNBTTag<NBTTagByteArray>
    {
        public const int TypeId = 7;

        public readonly byte[] Data;

        public static NBTTagByteArray Read(Stream s, int depth)
        {
            int length = DataInputStreamUtils.ReadInt(s);

            byte[] data = new byte[length];
            DataInputStreamUtils.Read(s, data);

            return new NBTTagByteArray(data);
        }

        private NBTTagByteArray(byte[] data)
        {
            Data = data;
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"NBTTagByteArray({Data.Length})";
        }
    }
}

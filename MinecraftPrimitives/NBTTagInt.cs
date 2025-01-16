using System;
using System.IO;

namespace MinecraftPrimitives
{
    public sealed class NBTTagInt : NBTTagBase, IReadableNBTTag<NBTTagInt>
    {
        public const int TypeId = 3;

        public readonly int Value;

        public static NBTTagInt Read(Stream s, int depth)
        {
            int value = DataInputStreamUtils.ReadInt(s);
            return new NBTTagInt(value);
        }

        private NBTTagInt(int value)
        {
            Value = value;
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"NBTTagInt({Value})";
        }
    }
}

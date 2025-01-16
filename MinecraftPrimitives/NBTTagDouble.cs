using System;
using System.IO;

namespace MinecraftPrimitives
{
    public sealed class NBTTagDouble : NBTTagBase, IReadableNBTTag<NBTTagDouble>
    {
        public const int TypeId = 6;

        public readonly double Value;

        public static NBTTagDouble Read(Stream s, int depth)
        {
            double value = DataInputStreamUtils.ReadDouble(s);
            return new NBTTagDouble(value);
        }

        private NBTTagDouble(double value)
        {
            Value = value;
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"NBTTagDouble({Value})";
        }
    }
}

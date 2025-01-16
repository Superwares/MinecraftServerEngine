using System;
using System.IO;

namespace MinecraftPrimitives
{
    public sealed class NBTTagLong : NBTTagBase, IReadableNBTTag<NBTTagLong>
    {
        public const int TypeId = 4;

        public readonly long Value;

        public static NBTTagLong Read(Stream s, int depth)
        {
            long value = DataInputStreamUtils.ReadLong(s);
            return new NBTTagLong(value);
        }

        private NBTTagLong(long value)
        {
            Value = value;
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"NBTTagLong({Value})";
        }
    }
}

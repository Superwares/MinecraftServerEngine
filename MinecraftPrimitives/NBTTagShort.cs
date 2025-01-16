using System;
using System.IO;

namespace MinecraftPrimitives
{
    public sealed class NBTTagShort : NBTTagBase, IReadableNBTTag<NBTTagShort>
    {
        public const int TypeId = 2;

        public readonly int Value;

        public static NBTTagShort Read(Stream s, int depth)
        {
            int value = DataInputStreamUtils.ReadShort(s);
            return new NBTTagShort(value);
        }

        private NBTTagShort(int value)
        {
            Value = value;
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"NBTTagShort({Value})";
        }
    }
}

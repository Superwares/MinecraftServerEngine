using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MinecraftPrimitives
{
    public sealed class NBTTagByte : NBTTagBase, IReadableNBTTag<NBTTagByte>
    {
        public const int TypeId = 1;

        public readonly int Value;

        public static NBTTagByte Read(Stream s, int depth)
        {
            int value = DataInputStreamUtils.ReadByte(s);
            return new NBTTagByte(value);
        }

        private NBTTagByte(int value)
        {
            Value = value;
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"NBTTagByte({Value})";
        }
    }
}

using System;
using System.IO;

namespace MinecraftPrimitives
{
    public sealed class NBTTagFloat : NBTTagBase, IReadableNBTTag<NBTTagFloat>
    {
        public const int TypeId = 5;

        readonly float Value;

        public static NBTTagFloat Read(Stream s, int depth)
        {
            float value = DataInputStreamUtils.ReadFloat(s);
            return new NBTTagFloat(value);
        }

        private NBTTagFloat(float value)
        {
            Value = value;
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"NBTTagFloat({Value})";
        }
    }
}

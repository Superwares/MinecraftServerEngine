using System;
using System.IO;

namespace MinecraftPrimitives
{
    public sealed class NBTTagString : NBTTagBase, IReadableNBTTag<NBTTagString>
    {
        public const int TypeId = 8;

        public readonly string Value;

        public static NBTTagString Read(Stream s, int depth)
        {
            string value = DataInputStreamUtils.ReadModifiedUtf8String(s);
            return new NBTTagString(value);
        }

        private NBTTagString(string value)
        {
            Value = value;
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}({Value})";
        }
    }
}

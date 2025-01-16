using System;
using System.IO;

namespace MinecraftPrimitives
{
    public sealed class NBTTagEnd : NBTTagBase, IReadableNBTTag<NBTTagEnd>
    {
        public const int TypeId = 0;


        public static NBTTagEnd Read(Stream s, int depth)
        {
            return new NBTTagEnd();
        }

        private NBTTagEnd()
        {
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }
    }
}

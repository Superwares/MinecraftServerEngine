using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
{
    public sealed class NBTTagEnd : NBTBase, IReadableNBTTag<NBTTagEnd>
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
{
    public sealed class NBTTagDouble : NBTBase, IReadableNBTTag<NBTTagDouble>
    {
        public const int TypeId = 6;

        private readonly double value;

        public static NBTTagDouble Read(Stream s, int depth)
        {
            double value = DataInputStreamUtils.ReadDouble(s);
            return new NBTTagDouble(value);
        }

        private NBTTagDouble(double value)
        {
            this.value = value;
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }
    }
}

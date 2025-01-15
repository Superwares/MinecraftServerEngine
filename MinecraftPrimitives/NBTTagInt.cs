using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
{
    public sealed class NBTTagInt : NBTBase
    {
        public const int TypeId = 3;

        private readonly int value;

        public static NBTTagInt Read(Stream s, int depth)
        {
            int value = DataInputStreamUtils.ReadInt(s);
            return new NBTTagInt(value);
        }

        private NBTTagInt(int value)
        {
            this.value = value;
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
{
    public sealed class NBTTagShort : NBTBase
    {
        public const int TypeId = 2;

        private readonly int value;

        public static NBTTagShort Read(Stream s, int depth)
        {
            int b0 = s.ReadByte();
            int b1 = s.ReadByte();
            int value = ((b0 << 8) | (b1 & 0xff));
            return new NBTTagShort(value);
        }

        private NBTTagShort(int value)
        {
            this.value = value;
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }
    }
}

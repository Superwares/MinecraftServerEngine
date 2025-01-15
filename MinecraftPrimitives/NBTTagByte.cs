using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
{
    public sealed class NBTTagByte : NBTBase
    {
        public const int TypeId = 1;

        private readonly int value;

        public static NBTTagByte Read(Stream s, int depth)
        {
            int value = DataInputStreamUtils.ReadByte(s);
            return new NBTTagByte(value);
        }

        private NBTTagByte(int value)
        {
            this.value = value;
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
{
    public sealed class NBTTagLong : NBTBase
    {
        public const int TypeId = 4;

        private readonly long value;

        public static NBTTagLong Read(Stream s, int depth)
        {
            long value = DataInputStreamUtils.ReadLong(s);
            return new NBTTagLong(value);
        }

        private NBTTagLong(long value)
        {
            this.value = value;
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }
    }
}

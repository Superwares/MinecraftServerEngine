using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
{
    public sealed class NBTTagFloat : NBTBase
    {
        public const int TypeId = 5;

        private readonly float value;

        public static NBTTagFloat Read(Stream s, int depth)
        {
            float value = DataInputStreamUtils.ReadFloat(s);
            return new NBTTagFloat(value);
        }

        private NBTTagFloat(float value)
        {
            this.value = value;
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }
    }
}

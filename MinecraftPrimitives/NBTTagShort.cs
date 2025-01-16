using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
{
    public sealed class NBTTagShort : NBTBase, IReadableNBTTag<NBTTagShort>
    {
        public const int TypeId = 2;

        private readonly int value;

        public static NBTTagShort Read(Stream s, int depth)
        {
            int value = DataInputStreamUtils.ReadShort(s);
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

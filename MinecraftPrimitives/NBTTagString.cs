using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
{
    public sealed class NBTTagString : NBTBase, IReadableNBTTag<NBTTagString>
    {
        public const int TypeId = 8;

        private readonly string value;

        public static NBTTagString Read(Stream s, int depth)
        {
            string value = DataInputStreamUtils.ReadModifiedUtf8String(s);
            return new NBTTagString(value);
        }

        private NBTTagString(string value)
        {
            this.value = value;
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }
    }
}

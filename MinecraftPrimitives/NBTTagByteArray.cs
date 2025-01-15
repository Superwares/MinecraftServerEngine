using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
{
    public sealed class NBTTagByteArray : NBTBase
    {
        public const int TypeId = 7;

        private readonly byte[] value;

        public static NBTTagByteArray Read(Stream s, int depth)
        {
            int b0 = s.ReadByte();
            int b1 = s.ReadByte();
            int b2 = s.ReadByte();
            int b3 = s.ReadByte();
            int length =
                ((b0 & 0xff) << 24)
                | ((b1 & 0xff) << 16)
                | ((b2 & 0xff) << 8)
                | ((b3 & 0xff) << 0);

            byte[] value = new byte[length];
            s.Read(value, 0, length);

            return new NBTTagByteArray(value);
        }

        private NBTTagByteArray(byte[] value)
        {
            this.value = value;
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }
    }
}

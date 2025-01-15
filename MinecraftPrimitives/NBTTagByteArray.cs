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

        private readonly byte[] arr;

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

            byte[] arr = new byte[length];
            s.Read(arr, 0, length);

            return new NBTTagByteArray(arr);
        }

        private NBTTagByteArray(byte[] arr)
        {
            this.arr = arr;
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }
    }
}

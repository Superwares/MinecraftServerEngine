using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
{
    public sealed class NBTTagLongArray : NBTBase
    {
        public const int TypeId = 12;

        private readonly long[] arr;

        public static NBTTagLongArray Read(Stream s, int depth)
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

            long[] arr = new long[length];

            for (int i = 0; i < length; ++i)
            {
                int b4 = s.ReadByte();
                int b5 = s.ReadByte();
                int b6 = s.ReadByte();
                int b7 = s.ReadByte();
                int b8 = s.ReadByte();
                int b9 = s.ReadByte();
                int b10 = s.ReadByte();
                int b11 = s.ReadByte();

                long value =
                    ((long)(b0 & 0xff) << 56)
                    | ((long)(b1 & 0xff) << 48)
                    | ((long)(b2 & 0xff) << 40)
                    | ((long)(b3 & 0xff) << 32)
                    | ((long)(b4 & 0xff) << 24)
                    | ((long)(b5 & 0xff) << 16)
                    | ((long)(b6 & 0xff) << 8)
                    | ((long)(b7 & 0xff) << 0);
                arr[i] = value;
            }

            return new NBTTagLongArray(arr);
        }

        private NBTTagLongArray(long[] arr)
        {
            this.arr = arr;
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
{
    public sealed class NBTTagIntArray : NBTBase
    {
        public const int TypeId = 11;

        private readonly int[] arr;

        public static NBTTagIntArray Read(Stream s, int depth)
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

            int[] arr = new int[length];
            
            for (int i = 0; i < length; ++i)
            {
                int b4 = s.ReadByte();
                int b5 = s.ReadByte();
                int b6 = s.ReadByte();
                int b7 = s.ReadByte();
                int value =
                    ((b4 & 0xff) << 24)
                    | ((b5 & 0xff) << 16)
                    | ((b6 & 0xff) << 8)
                    | ((b7 & 0xff) << 0);
                arr[i] = value;
            }

            return new NBTTagIntArray(arr);
        }

        private NBTTagIntArray(int[] arr)
        {
            this.arr = arr;
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }
    }
}

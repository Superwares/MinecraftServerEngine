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
            int length = DataInputStreamUtils.ReadInt(s);

            long[] arr = new long[length];

            for (int i = 0; i < length; ++i)
            {
                
                arr[i] = DataInputStreamUtils.ReadLong(s);
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

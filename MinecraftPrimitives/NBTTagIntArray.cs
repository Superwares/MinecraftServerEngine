using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
{
    public sealed class NBTTagIntArray : NBTBase, IReadableNBTTag<NBTTagIntArray>
    {
        public const int TypeId = 11;

        private readonly int[] arr;

        public static NBTTagIntArray Read(Stream s, int depth)
        {
            int length = DataInputStreamUtils.ReadInt(s);

            int[] arr = new int[length];
            
            for (int i = 0; i < length; ++i)
            {
                arr[i] = DataInputStreamUtils.ReadInt(s);
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

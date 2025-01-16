using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
{
    public sealed class NBTTagByteArray : NBTBase, IReadableNBTTag<NBTTagByteArray>
    {
        public const int TypeId = 7;

        private readonly byte[] arr;

        public static NBTTagByteArray Read(Stream s, int depth)
        {
            int length = DataInputStreamUtils.ReadInt(s);

            byte[] arr = new byte[length];
            DataInputStreamUtils.Read(s, arr);

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

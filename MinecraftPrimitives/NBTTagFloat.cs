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
            int b0 = s.ReadByte();
            int b1 = s.ReadByte();
            int b2 = s.ReadByte();
            int b3 = s.ReadByte();

            /**
             * Reads four input bytes and returns a float value. 
             * It does this by first constructing an int value 
             * in exactly the manner of the readInt method, 
             * then converting this int value to a float in exactly 
             * the manner of the method Float. intBitsToFloat. 
             * This method is suitable for reading bytes written by 
             * the writeFloat method of interface DataOutput.
             * 
             * Returns: the float value read.
             */
            float value = BitConverter.ToSingle(new byte[] {
                (byte)b0, (byte)b1, (byte)b2, (byte)b3,
            }, 0);
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
{
    public sealed class NBTTagDouble : NBTBase
    {
        public const int TypeId = 6;

        private readonly double value;

        public static NBTTagDouble Read(Stream s, int depth)
        {
            int b0 = s.ReadByte();
            int b1 = s.ReadByte();
            int b2 = s.ReadByte();
            int b3 = s.ReadByte();
            int b4 = s.ReadByte();
            int b5 = s.ReadByte();
            int b6 = s.ReadByte();
            int b7 = s.ReadByte();

            /**
             * Reads eight input bytes and returns a double value. 
             * It does this by first constructing a long value 
             * in exactly the manner of the readLong method, 
             * then converting this long value to a double in exactly 
             * the manner of the method Double. longBitsToDouble. 
             * This method is suitable for reading bytes written by 
             * the writeDouble method of interface DataOutput.
             * 
             * Returns: the double value read.
             */
            double value = BitConverter.ToDouble(new byte[] {
                (byte)b0, (byte)b1, (byte)b2, (byte)b3,
                (byte)b4, (byte)b5, (byte)b6, (byte)b7,
            }, 0);
            return new NBTTagDouble(value);
        }

        private NBTTagDouble(double value)
        {
            this.value = value;
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }
    }
}

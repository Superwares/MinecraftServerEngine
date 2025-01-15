using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
{
    public sealed class NBTTagCompound : NBTBase
    {
        public override byte TypeId => 10;

        public static NBTTagCompound Read(Stream s, int depth)
        {
            //Console.WriteLine($"Reading stream with depth {depth}");
            System.Diagnostics.Debug.Assert(s != null);
            System.Diagnostics.Debug.Assert(depth >= 0);
            if (depth > 512)
            {
                throw new Exception("Tried to read NBT tag with too high complexity, depth > 512");
            }

            throw new NotImplementedException();
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
{
    internal class NBTTagByte : NBTBase
    {
        public override byte TypeId => 1;

        public static NBTTagByte Read(Stream s, int depth)
        {
            Console.WriteLine($"Reading stream with depth {depth}");

            throw new NotImplementedException();
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }
    }
}

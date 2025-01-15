using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
{
    internal class NBTTagEnd : NBTBase
    {
        public const int TypeId = 1;

        public static NBTTagEnd Read(Stream s, int depth)
        {
            
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }
    }
}

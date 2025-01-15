using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
{

    public abstract class NBTBase
    {
        public abstract byte TypeId
        {
            get;
        }

        public abstract void Write(Stream s);
        

    }
}

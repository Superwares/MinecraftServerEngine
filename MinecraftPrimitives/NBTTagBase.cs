using System;
using System.IO;

namespace MinecraftPrimitives
{

    public abstract class NBTTagBase
    {


        public abstract void Write(Stream s);

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
{
    public static class NBTTagCompoundLoader
    {
        public static NBTTagCompound Load(FileInfo fileInfo, int chunkX, int chunkY)
        {
            RegionFile file = new(fileInfo);

            using (Stream s = file.ReadChunk(chunkX & 31, chunkY & 31))
            {
                if (s == null)
                {
                    return null;
                }
            }


            throw new NotImplementedException();
        }
    }
}

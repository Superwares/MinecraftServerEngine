using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
{
    public static class NBTTagRootCompoundLoader
    {
        public static NBTTagCompound Load(FileInfo fileInfo, int chunkX, int chunkY)
        {
            using RegionFile file = new(fileInfo);

            using Stream s = file.ReadChunk(chunkX & 31, chunkY & 31);

            if (s == null)
            {
                return null;
            }

            int id = s.ReadByte();
            if (NBTTagCompound.TypeId != id)
            {
                throw new InvalidDataException("Root tag must be a named compound tag");
            }

            DataInputStreamUtils.ReadModifiedUtf8String(s);

            return NBTTagCompound.Read(s, 0);
        }
    }
}

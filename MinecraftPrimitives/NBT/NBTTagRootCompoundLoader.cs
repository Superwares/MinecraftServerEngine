namespace MinecraftPrimitives.NBT
{
    public static class NBTTagRootCompoundLoader
    {
        public static NBTTagCompound Load(System.IO.FileInfo fileInfo, int chunkX, int chunkY)
        {
            using RegionFile file = new(fileInfo);

            using System.IO.Stream s = file.ReadChunk(chunkX & 31, chunkY & 31);

            if (s == null)
            {
                return null;
            }

            int id = s.ReadByte();
            if (NBTTagCompound.TypeId != id)
            {
                throw new NBTTagException("Root tag must be a named compound tag");
            }

            DataInputStreamUtils.ReadModifiedUtf8String(s);

            return NBTTagCompound.Read(s, 0);
        }
    }
}

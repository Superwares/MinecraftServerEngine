using System.IO;

namespace MinecraftPrimitives
{
    public interface IReadableNBTTag<T> where T : IReadableNBTTag<T>
    {
        public static abstract T Read(Stream s, int depth);
    }
}

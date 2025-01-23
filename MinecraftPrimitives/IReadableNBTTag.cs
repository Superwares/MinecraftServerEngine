using System.IO;

namespace MinecraftPrimitives
{
    public interface IReadableNBTTag<T> where T : IReadableNBTTag<T>
    {
        static abstract byte GetTypeId();

        public static abstract T Read(Stream s, int depth);
    }
}

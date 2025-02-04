
namespace MinecraftServerEngine.NBT
{
    public interface IReadableNBTTag<T> where T : IReadableNBTTag<T>
    {
        static abstract byte GetTypeId();

        public static abstract T Read(System.IO.Stream s, int depth);
    }
}

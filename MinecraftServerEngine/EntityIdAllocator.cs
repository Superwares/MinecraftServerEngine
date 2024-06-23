using Containers;

namespace MinecraftServerEngine
{
    internal static class EntityIdAllocator
    {
        // TODO: make static destructor to dispose.
        private readonly static ConcurrentNumList IdList = new();  // Disposable

        public static int Alloc()
        {
            return IdList.Alloc();
        }

        public static void Dealloc(int id)
        {
            IdList.Dealloc(id);
        }

    }
}

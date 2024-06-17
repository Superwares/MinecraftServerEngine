using Containers;

namespace MinecraftServerEngine
{
    internal static class EntityIdAllocator
    {
        // TODO: make static destructor to dispose.
        private readonly static Numlist _ENTITY_ID_LIST = new();  // Disposable

        public static int Alloc()
        {
            return _ENTITY_ID_LIST.Alloc();
        }

        public static void Dealloc(int id)
        {
            _ENTITY_ID_LIST.Dealloc(id);
        }

    }
}

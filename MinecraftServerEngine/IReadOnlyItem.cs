

namespace MinecraftServerEngine
{
    public interface IReadOnlyItem : System.IEquatable<IReadOnlyItem>
    {
        public ItemType Type { get; }

        public int MaxCount { get; }

        public string Name { get; }
        public string[] Lore { get; }


        public int MaxDurability { get; }
        public int CurrentDurability { get; }
        public bool IsBreakable { get; }
        public bool IsBreaked { get; }

        public byte[] Hash { get; }
    }
}

using System.Diagnostics;

namespace Protocol
{
    public abstract class Block
    {
        private readonly int _id;
        public int Id => _id;

        private readonly int _metadate;
        public int Metadata => _metadate;

        public Block(int id, int metadata)
        {
            _id = id;
            _metadate = metadata;
        }

        public ulong GetGlobalPaletteID()
        {
            byte metadata = (byte)_metadate;
            Debug.Assert((metadata & 0b_11110000) == 0);  // metadata is 4 bits

            ushort id = (ushort)_id;
            Debug.Assert((id & 0b_11111110_00000000) == 0);  // id is 9 bits
            return (ulong)(id << 4 | metadata);  // 13 bits
        }

    }

    public class Air() : Block(0, 0)
    {
        
    }

    public class Stone() : Block(1, 0)
    {
        
    }

    public class Granite() : Block(1, 1)
    {
        
    }

    public class PolishedGranite() : Block(1, 2)
    {
        
    }

}

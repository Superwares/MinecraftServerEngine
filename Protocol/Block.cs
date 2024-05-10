using System;
using System.Diagnostics;

namespace Protocol
{
    public sealed class Block : IEquatable<Block>
    {
        public enum Types : uint
        {
            Air              = 0,
            Stone            = 1,
            Grass            = 2,
            Dirt             = 3,
        }

        private readonly Types _type;
        public Types Type => _type;

        private readonly uint _metadata;
        public uint Metadata => _metadata;

        public ulong GlobalPaletteId
        {
            get
            {
                byte metadata = (byte)_metadata;
                Debug.Assert((metadata & 0b_11110000) == 0);  // metadata is 4 bits

                ushort id = (ushort)_type;
                Debug.Assert((id & 0b_11111110_00000000) == 0);  // id is 9 bits
                return (ulong)(id << 4 | metadata);  // 13 bits
            }
        }

        public Block(Types type, uint metadata)
        {
            _type = type;
            _metadata = metadata;
        }

        public bool Equals(Block? other)
        {
            if (other == null)
            {
                return false;
            }

            return (_type == other._type) && (_metadata == other._metadata);
        }

    }

}

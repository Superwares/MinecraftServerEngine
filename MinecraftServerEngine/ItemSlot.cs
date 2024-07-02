

using Containers;

namespace MinecraftServerEngine
{
    

    internal sealed class ItemSlot
    {

        
        
        public readonly ItemType Item;

        private int _count;
        public int Count => _count;
        public int MinCount => 1;
        public readonly int MaxCount;
        public int RemainingCount => MaxCount - Count;


        public ItemSlot(ItemType item, int count)
        {
            Item = item;
            _count = count;

            MaxCount = item.GetMaxCount();
            System.Diagnostics.Debug.Assert(MaxCount >= MinCount);
        }

        public int Stack(int count)
        {
            System.Diagnostics.Debug.Assert(count >= 0);

            System.Diagnostics.Debug.Assert(_count >= MinCount);
            System.Diagnostics.Debug.Assert(_count <= MaxCount);

            if (count == 0)
            {
                return 0;
            }

            int rest;
            _count += count;

            if (_count > MaxCount)
            {
                rest = _count - MaxCount;
                _count = MaxCount;
            }
            else
            {
                rest = 0;
            }

            return count - rest;  // spend
        }

        public void Spend(int count)
        {
            System.Diagnostics.Debug.Assert(count >= 0);
            System.Diagnostics.Debug.Assert(count < MaxCount);

            System.Diagnostics.Debug.Assert(_count >= MinCount);
            System.Diagnostics.Debug.Assert(_count <= MaxCount);

            if (count == 0)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(count < _count);
            _count -= count;
            System.Diagnostics.Debug.Assert(_count > 0);
        }

        public ItemSlot DivideHalf()
        {
            System.Diagnostics.Debug.Assert(_count >= MinCount);
            System.Diagnostics.Debug.Assert(_count <= MaxCount);

            int count = (_count / 2) + (_count % 2);
            _count /= 2;

            System.Diagnostics.Debug.Assert(count >= MinCount);
            System.Diagnostics.Debug.Assert(count <= MaxCount);
            System.Diagnostics.Debug.Assert(_count >= MinCount);
            System.Diagnostics.Debug.Assert(_count <= MaxCount);

            return new ItemSlot(Item, count);
        }

        public ItemSlot DivideOne()
        {
            System.Diagnostics.Debug.Assert(_count >= MinCount);
            System.Diagnostics.Debug.Assert(_count <= MaxCount);

            --_count;

            System.Diagnostics.Debug.Assert(_count >= MinCount);

            return new ItemSlot(Item, 1);
        }

        public void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            System.Diagnostics.Debug.Assert(_count >= MinCount);
            System.Diagnostics.Debug.Assert(_count <= MaxCount);

            int id = Item.GetId();

            System.Diagnostics.Debug.Assert(id >= short.MinValue);
            System.Diagnostics.Debug.Assert(id <= short.MaxValue);
            buffer.WriteShort((short)id);

            System.Diagnostics.Debug.Assert(_count >= byte.MinValue);
            System.Diagnostics.Debug.Assert(_count <= byte.MaxValue);
            buffer.WriteByte((byte)_count);

            buffer.WriteShort(0);  // damage
            buffer.WriteByte(0x00);  // no NBT
        }

        /*public bool CompareWithProtocolFormat(SlotData slotData)
        {
            System.Diagnostics.Debug.Assert(_count >= MinCount);
            System.Diagnostics.Debug.Assert(_count <= MaxCount);

            if (slotData.Id == -1)
            {
                return false;
            }

            int id = _ITEM_ENUM_TO_ID_MAP.Lookup(Item);
            if (slotData.Id != id)
            {
                return false;
            }

            if (slotData.Count != _count)
            {
                return false;
            }

            return true;
        }*/

    }
}



using Containers;

namespace MinecraftServerEngine
{
    public sealed class PlayerInventory : Inventory
    {


        internal const int TotalSlotCount = 46;

        internal const int CraftingOutputSlotIndex = 0;
        internal const int CraftingInputSlotsOffset = 1;
        internal const int HelmetSlotIndex = 5;
        internal const int ChestplateSlotIndex = 6;
        internal const int LeggingsSlotIndex = 7;
        internal const int BootsSlotIndex = 8;
        internal const int ArmorSlotsOffset = 5;
        internal const int PrimarySlotsOffset = 9;
        internal const int MainSlotsOffset = 9;
        internal const int HotbarSlotsOffset = 36;
        internal const int OffHandSlotIndex = 45;

        internal const int CraftingInputSlotCount = 4;
        internal const int ArmorSlotCount = 4;
        internal const int PrimarySlotCount = 36;
        internal const int MainSlotCount = 27;
        internal const int HotbarSlotCount = 9;


        private bool _disposed = false;

        private int _indexMainHandSlot = 0;  // 0-8


        internal PlayerInventory() : base(TotalSlotCount)
        {
            GiveFromLeftInPrimary(new ItemStack(ItemType.Stick, "Stick!"));
            GiveFromLeftInPrimary(new ItemStack(ItemType.Stick, "Stick!!!"));
            GiveFromLeftInPrimary(new ItemStack(ItemType.DiamondSword, "Hello Sword!"));
            /*GiveFromLeftInPrimary(new ItemStack(ItemType.Snowball));*/
        }

        ~PlayerInventory()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        internal System.Collections.Generic.IEnumerable<InventorySlot> GetCraftingInputSlots()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            for (int i = 0; i < CraftingInputSlotCount; ++i)
            {
                yield return Slots[i + CraftingInputSlotsOffset];
            }
        }
        internal System.Collections.Generic.IEnumerable<InventorySlot> GetArmorSlots()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            for (int i = 0; i < ArmorSlotCount; ++i)
            {
                yield return Slots[i + ArmorSlotsOffset];
            }
        }
        internal System.Collections.Generic.IEnumerable<InventorySlot> GetPrimarySlots()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            for (int i = 0; i < PrimarySlotCount; ++i)
            {
                yield return Slots[i + PrimarySlotsOffset];
            }
        }
        private System.Collections.Generic.IEnumerable<InventorySlot> GetMainSlots()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            for (int i = 0; i < MainSlotCount; ++i)
            {
                yield return Slots[i + MainSlotsOffset];
            }
        }
        private System.Collections.Generic.IEnumerable<InventorySlot> GetHotbarSlots()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            for (int i = 0; i < HotbarSlotCount; ++i)
            {
                yield return Slots[i + HotbarSlotsOffset];
            }
        }

        internal InventorySlot GetCraftingOutputSlot()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return Slots[CraftingOutputSlotIndex];
        }
        internal InventorySlot GetHelmetSlot()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return Slots[HelmetSlotIndex];
        }
        internal InventorySlot GetChestplateSlot()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return Slots[ChestplateSlotIndex];
        }
        internal InventorySlot GetLeggingsSlot()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return Slots[LeggingsSlotIndex];
        }
        internal InventorySlot GetBootsSlot()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return Slots[BootsSlotIndex];
        }
        internal InventorySlot GetArmorSlot(int i)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < ArmorSlotCount);

            return Slots[i + ArmorSlotsOffset];
        }
        internal InventorySlot GetPrimarySlot(int i)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < PrimarySlotCount);

            return Slots[i + PrimarySlotsOffset];
        }
        internal void SetPrimarySlot(int i, InventorySlot slot)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < PrimarySlotCount);

            Slots[i + PrimarySlotsOffset] = slot;
        }
        internal InventorySlot GetMainSlot(int i)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < MainSlotCount);

            System.Diagnostics.Debug.Assert(MainSlotsOffset >= 0);
            System.Diagnostics.Debug.Assert(i + MainSlotsOffset < TotalSlotCount);
            return Slots[i + MainSlotsOffset];
        }
        internal InventorySlot GetHotbarSlot(int i)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < HotbarSlotCount);

            System.Diagnostics.Debug.Assert(HotbarSlotsOffset >= 0);
            System.Diagnostics.Debug.Assert(i + HotbarSlotsOffset < TotalSlotCount);
            return Slots[i + HotbarSlotsOffset];
        }
        internal InventorySlot GetOffHandSlot()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return Slots[OffHandSlotIndex];
        }

        internal void ChangeMainHand(int index)
        {
            System.Diagnostics.Debug.Assert(index >= 0);
            System.Diagnostics.Debug.Assert(index < HotbarSlotCount);
            _indexMainHandSlot = index;
        }
        internal InventorySlot GetMainHandSlot()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_indexMainHandSlot >= 0);
            System.Diagnostics.Debug.Assert(_indexMainHandSlot < HotbarSlotCount);

            return Slots[_indexMainHandSlot + HotbarSlotsOffset];
        }

        internal (byte[], byte[]) GetEquipmentsData()
        {
            InventorySlot
                slotMainHand = GetMainHandSlot(),
                slotOffHand = GetOffHandSlot();
            System.Diagnostics.Debug.Assert(slotMainHand != null);
            System.Diagnostics.Debug.Assert(slotOffHand != null);
            return (slotMainHand.WriteData(), slotOffHand.WriteData());
        }


        /*internal virtual void TakeHalf(
            int i, ref ItemSlot cursor, WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount);
            System.Diagnostics.Debug.Assert(cursor == null);
            System.Diagnostics.Debug.Assert(renderer != null);

            ref ItemSlot slot = ref Slots[i];

            if (slot == null)
            {
                System.Diagnostics.Debug.Assert(cursor == null);

            }
            else
            {
                System.Diagnostics.Debug.Assert(!ReferenceEquals(slot, cursor));

                if (slot.Count == 1)
                {
                    --_count;
                    System.Diagnostics.Debug.Assert(_count >= 0);
                    System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

                    cursor = slot;
                    slot = null;
                    System.Diagnostics.Debug.Assert(cursor.Count == 1);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(slot.Count > 1);

                    cursor = slot.DivideHalf();
                    System.Diagnostics.Debug.Assert(slot.Count > 0);
                    System.Diagnostics.Debug.Assert(cursor != null);
                }
            }

            Refresh(renderer);
        }

        internal virtual void PutOne(
            int i, ref ItemSlot cursor, WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);
            System.Diagnostics.Debug.Assert(renderer != null);

            ref ItemSlot slot = ref Slots[i];
            System.Diagnostics.Debug.Assert(!ReferenceEquals(slot, cursor));

            if (slot != null)
            {
                if (cursor.Item == slot.Item)
                {
                    if (cursor.Count == 1)
                    {
                        int spend = slot.Stack(1);
                        if (spend == 1)
                        {
                            cursor = null;
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(cursor.Count == 1);
                            System.Diagnostics.Debug.Assert(slot.Count == slot.MaxCount);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(cursor.Count > 1);

                        int spend = slot.Stack(1);
                        if (spend == 1)
                        {
                            cursor.Spend(1);
                        }
                    }

                }
                else
                {
                    // Swap
                    ItemSlot temp = cursor;
                    cursor = slot;
                    slot = temp;
                }

            }
            else
            {
                if (cursor.Count > 1)
                {
                    slot = cursor.DivideOne();
                    System.Diagnostics.Debug.Assert(cursor.Count >= cursor.MinCount);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(cursor.Count == 1);
                    System.Diagnostics.Debug.Assert(slot == null);

                    slot = cursor;
                    cursor = null;
                }

                ++_count;
                System.Diagnostics.Debug.Assert(_count >= 0);
                System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            }

            Refresh(renderer);
        }*/

        internal void LeftClick(InventorySlot slot, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(slot != null);
            System.Diagnostics.Debug.Assert(cursor != null);

            slot.LeftClick(cursor);
        }

        internal void LeftClickInPrimary(int i, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < PrimarySlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);

            InventorySlot slot = GetPrimarySlot(i);
            System.Diagnostics.Debug.Assert(slot != null);

            LeftClick(slot, cursor);
        }

        internal void LeftClick(int i, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);

            if (i >= CraftingOutputSlotIndex && i < PrimarySlotsOffset)
            {
                return;
            }

            InventorySlot slot = Slots[i];
            System.Diagnostics.Debug.Assert(slot != null);

            LeftClick(slot, cursor);
        }

        private void RightClick(InventorySlot slot, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(slot != null);
            System.Diagnostics.Debug.Assert(cursor != null);

            slot.RightClick(cursor);
        }

        internal void RightClickInPrimary(int i, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);

            InventorySlot slot = GetPrimarySlot(i);
            System.Diagnostics.Debug.Assert(slot != null);

            RightClick(slot, cursor);
        }

        internal void RightClick(int i, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);

            if (i >= CraftingOutputSlotIndex && i < PrimarySlotsOffset)
            {
                return;
            }

            InventorySlot slot = Slots[i];
            System.Diagnostics.Debug.Assert(slot != null);

            RightClick(slot, cursor);
        }

        internal bool GiveFromLeftInPrimary(ItemStack stack)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (stack == null)
            {
                return true;
            }

            using Queue<InventorySlot> slots = new();

            int j = -1;
            int prevCount = stack.Count;

            InventorySlot slotInside;
            for (int i = 0; i < PrimarySlotCount; ++i)
            {
                slotInside = GetPrimarySlot(i);
                System.Diagnostics.Debug.Assert(slotInside != null);

                if (slotInside.Empty)
                {
                    //prevCount = 0;
                    //slots.Enqueue(slotInside);
                    //break;

                    if (j < 0)
                    {
                        j = i;
                    }

                    continue;
                }

                int restCount = slotInside.PreMove(stack.Type, stack.Name, prevCount);

                if (prevCount == restCount)
                {
                    continue;
                }

                prevCount = restCount;
                slots.Enqueue(slotInside);

                if (prevCount == 0)
                {
                    break;
                }

            }

            if (prevCount > 0 && j < 0)
            {
                return false;
            }

            while (slots.Empty == false)
            {
                slotInside = slots.Dequeue();

                slotInside.Move(ref stack);

                if (stack == null)
                {
                    break;
                }
            }

            if (j >= 0)
            {
                slotInside = GetPrimarySlot(j);

                System.Diagnostics.Debug.Assert(slotInside != null);
                System.Diagnostics.Debug.Assert(slotInside.Empty == true);

                slotInside.Move(ref stack);

                System.Diagnostics.Debug.Assert(stack == null);
            }

            slots.Flush();

            return true;
        }

        public bool GiveItem(ItemStack stack)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            if (stack == null)
            {
                return true;
            }

            return GiveFromLeftInPrimary(stack);
        }

        public ItemStack[] TakeItemsInPrimary(ItemType itemType, string name, int count)
        {
            if (name == null || string.IsNullOrEmpty(name))
            {
                throw new System.ArgumentNullException(nameof(name));
            }

            if (count < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(count));
            }

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (count == 0)
            {
                return [];
            }

            System.Diagnostics.Debug.Assert(itemType.GetMaxCount() > 0);
            int minLength = (int)System.Math.Ceiling((double)count / (double)itemType.GetMaxCount());
            ItemStack[] itemStacks = new ItemStack[minLength];

            int leftCount = count;

            InventorySlot invSlot;
            ItemStack targetItemStack;

            for (int i = 0; i < PrimarySlotCount && leftCount > 0; ++i)
            {
                invSlot = GetPrimarySlot(i);
                System.Diagnostics.Debug.Assert(invSlot != null);

                if (invSlot.Empty == true)
                {
                    continue;
                }

                targetItemStack = invSlot.Stack;

                if (targetItemStack.Type == itemType && targetItemStack.Name == name)
                {
                    int takedCount = invSlot.PreTake(leftCount);

                    System.Diagnostics.Debug.Assert(takedCount >= 0);
                    System.Diagnostics.Debug.Assert(takedCount <= count);

                    if (takedCount > 0)
                    {
                        leftCount -= takedCount;
                    }
                }

            }

            if (leftCount > 0)
            {
                return [];
            }

            leftCount = count;
            int k = 0;

            for (int i = 0; i < PrimarySlotCount && leftCount > 0; ++i)
            {
                invSlot = GetPrimarySlot(i);
                System.Diagnostics.Debug.Assert(invSlot != null);

                if (invSlot.Empty == true)
                {
                    continue;
                }

                targetItemStack = invSlot.Stack;

                if (targetItemStack.Type == itemType && targetItemStack.Name == name)
                {
                    int takedCount = invSlot.Take(out ItemStack takedItemStack, leftCount);

                    System.Diagnostics.Debug.Assert(takedCount >= 0);
                    System.Diagnostics.Debug.Assert(takedCount <= count);

                    if (takedCount > 0)
                    {
                        System.Diagnostics.Debug.Assert(takedItemStack != null);

                        itemStacks[k++] = takedItemStack;

                        leftCount -= takedCount;
                    }
                }

            }

            System.Diagnostics.Debug.Assert(leftCount == 0);

            return itemStacks;
        }

        internal void QuickMoveFromLeftInPrimary(InventorySlot slot)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(slot != null);

            if (slot.Empty)
            {
                return;
            }

            int j = -1;

            InventorySlot slotInside;
            for (int i = 0; i < PrimarySlotCount; ++i)
            {
                System.Diagnostics.Debug.Assert(slot.Empty == false);

                slotInside = GetPrimarySlot(i);

                System.Diagnostics.Debug.Assert(slotInside != null);
                System.Diagnostics.Debug.Assert(ReferenceEquals(slotInside, slot) == false);

                if (slotInside.Empty == true)
                {
                    j = (j < 0) ? i : j;

                    continue;
                }

                slotInside.Move(slot);

                if (slot.Empty == true)
                {
                    break;
                }
            }

            System.Diagnostics.Debug.Assert(j <= TotalSlotCount);
            if (slot.Empty == false && j >= 0)
            {
                slotInside = GetPrimarySlot(j);

                System.Diagnostics.Debug.Assert(slotInside != null);
                System.Diagnostics.Debug.Assert(slotInside.Empty == true);
                slotInside.Move(slot);
            }
        }

        internal void QuickMoveFromRightInPrimary(InventorySlot slot)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(slot != null);

            if (slot.Empty)
            {
                return;
            }

            int j = -1;

            InventorySlot slotInside;
            for (int i = 0; i < PrimarySlotCount; ++i)
            {
                int k = PrimarySlotCount - i - 1;

                System.Diagnostics.Debug.Assert(slot.Empty == false);

                slotInside = GetPrimarySlot(k);

                System.Diagnostics.Debug.Assert(slotInside != null);
                System.Diagnostics.Debug.Assert(ReferenceEquals(slotInside, slot) == false);

                if (slotInside.Empty == true)
                {
                    j = (j < 0) ? k : j;

                    continue;
                }

                slotInside.Move(slot);

                if (slot.Empty == true)
                {
                    break;
                }
            }

            System.Diagnostics.Debug.Assert(j <= TotalSlotCount);
            if (slot.Empty == false && j >= 0)
            {
                slotInside = GetPrimarySlot(j);

                System.Diagnostics.Debug.Assert(slotInside != null);
                System.Diagnostics.Debug.Assert(slotInside.Empty == true);
                slotInside.Move(slot);
            }
        }

        private void QuickMoveFromLeftInMain(InventorySlot slot)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(slot != null);

            if (slot.Empty)
            {
                return;
            }

            int j = -1;

            InventorySlot slotInside;
            for (int i = 0; i < MainSlotCount; ++i)
            {
                System.Diagnostics.Debug.Assert(slot.Empty == false);

                slotInside = GetMainSlot(i);

                System.Diagnostics.Debug.Assert(slotInside != null);
                System.Diagnostics.Debug.Assert(ReferenceEquals(slotInside, slot) == false);

                if (slotInside.Empty == true)
                {
                    j = (j < 0) ? i : j;

                    continue;
                }

                slotInside.Move(slot);

                if (slot.Empty == true)
                {
                    break;
                }
            }

            System.Diagnostics.Debug.Assert(j <= TotalSlotCount);
            if (slot.Empty == false && j >= 0)
            {
                slotInside = GetMainSlot(j);

                System.Diagnostics.Debug.Assert(slotInside != null);
                System.Diagnostics.Debug.Assert(slotInside.Empty == true);
                slotInside.Move(slot);
            }

        }

        private void QuickMoveFromLeftInHotbar(InventorySlot slot)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (slot.Empty)
            {
                return;
            }

            int j = -1;

            InventorySlot slotInside;
            for (int i = 0; i < HotbarSlotCount; ++i)
            {
                System.Diagnostics.Debug.Assert(slot.Empty == false);

                slotInside = GetHotbarSlot(i);

                System.Diagnostics.Debug.Assert(slotInside != null);
                System.Diagnostics.Debug.Assert(ReferenceEquals(slotInside, slot) == false);

                if (slotInside.Empty == true)
                {
                    j = (j < 0) ? i : j;

                    continue;
                }

                slotInside.Move(slot);

                if (slot.Empty == true)
                {
                    break;
                }
            }

            System.Diagnostics.Debug.Assert(j <= TotalSlotCount);
            if (slot.Empty == false && j >= 0)
            {
                slotInside = GetHotbarSlot(j);

                System.Diagnostics.Debug.Assert(slotInside != null);
                System.Diagnostics.Debug.Assert(slotInside.Empty == true);
                slotInside.Move(slot);
            }

        }

        // Search from right bottom to left top.
        internal void QuickMove(int i)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount);

            if (i >= CraftingOutputSlotIndex && i < PrimarySlotsOffset)
            {
                return;
            }

            InventorySlot slot = Slots[i];
            System.Diagnostics.Debug.Assert(slot != null);

            if (i >= MainSlotsOffset && i < HotbarSlotsOffset)
            {
                /*QuickMoveFromLeftInArmor(slot);*/
                QuickMoveFromLeftInHotbar(slot);
            }
            else if (i >= HotbarSlotsOffset && i < OffHandSlotIndex)
            {
                /*QuickMoveFromLeftInArmor(slot);*/
                QuickMoveFromLeftInMain(slot);
            }
            else
            {
                System.Diagnostics.Debug.Assert(i == OffHandSlotIndex);
                /*QuickMoveFromLeftInArmor(slot);*/
                QuickMoveFromLeftInPrimary(slot);
            }
        }

        internal void SwapItems(int i, int j)
        {
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount);
            System.Diagnostics.Debug.Assert(j >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount);

            if (i == j)
            {
                return;
            }

            InventorySlot slot_i = Slots[i], slot_j = Slots[j];

            Slots[i] = slot_j;
            Slots[j] = slot_i;
        }

        internal void SwapItemsWithHotbarSlot(int i, int j)
        {
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount);
            System.Diagnostics.Debug.Assert(j >= 0);
            System.Diagnostics.Debug.Assert(j < HotbarSlotCount);

            SwapItems(i, HotbarSlotsOffset + j);
        }

        internal ItemStack DropSingle(int i)
        {
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount);

            InventorySlot slot = Slots[i];

            return slot.DropSingle();
        }

        internal ItemStack DropFull(int i)
        {
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount);

            InventorySlot slot = Slots[i];

            return slot.DropFull();
        }

        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed == false)
            {

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing == true)
                {
                    // Dispose managed resources.
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                //CloseHandle(handle);
                //handle = IntPtr.Zero;

                // Note disposing has been done.
                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }

}

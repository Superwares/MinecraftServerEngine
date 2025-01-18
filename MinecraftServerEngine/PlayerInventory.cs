

namespace MinecraftServerEngine
{
    public sealed class PlayerInventory : Inventory
    {
        private const int CraftingOutputSlotIndex = 0;
        private const int CraftingInputSlotsOffset = 1;
        private const int HelmetSlotIndex = 5;
        private const int ChestplateSlotIndex = 6;
        private const int LeggingsSlotIndex = 7;
        private const int BootsSlotIndex = 8;
        private const int ArmorSlotsOffset = 5;
        internal const int PrimarySlotsOffset = 9;
        private const int MainSlotsOffset = 9;
        private const int HotbarSlotsOffset = 36;
        private const int OffHandSlotIndex = 45;

        private const int CraftingInputSlotCount = 4;
        private const int ArmorSlotCount = 4;
        internal const int PrimarySlotCount = 36;
        private const int MainSlotCount = 27;
        internal const int HotbarSlotCount = 9;

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

        private InventorySlot GetCraftingOutputSlot()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return Slots[CraftingOutputSlotIndex];
        }
        private InventorySlot GetHelmetSlot()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return Slots[HelmetSlotIndex];
        }
        private InventorySlot GetChestplateSlot()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return Slots[ChestplateSlotIndex];
        }
        private InventorySlot GetLeggingsSlot()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return Slots[LeggingsSlotIndex];
        }
        private InventorySlot GetBootsSlot()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return Slots[BootsSlotIndex];
        }
        private InventorySlot GetArmorSlot(int i)
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
        private InventorySlot GetMainSlot(int i)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < MainSlotCount);

            System.Diagnostics.Debug.Assert(MainSlotsOffset >= 0);
            System.Diagnostics.Debug.Assert(i + MainSlotsOffset < TotalSlotCount);
            return Slots[i + MainSlotsOffset];
        }
        private InventorySlot GetHotbarSlot(int i)
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

        private int _indexMainHandSlot = 0;  // 0-8
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

        private bool _disposed = false;

        internal PlayerInventory() : base(46)
        {
            GiveFromLeftInPrimary(new ItemStack(ItemType.Stick));
            GiveFromLeftInPrimary(new ItemStack(ItemType.DiamondSword));
            /*GiveFromLeftInPrimary(new ItemStack(ItemType.Snowball));*/
        }

        ~PlayerInventory()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
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

        private void LeftClick(InventorySlot slot, InventorySlot cursor)
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

            System.Diagnostics.Debug.Assert(stack != null);

            int j = -1;

            InventorySlot slotInside;
            for (int i = 0; i < PrimarySlotCount; ++i)
            {
                slotInside = GetPrimarySlot(i);
                System.Diagnostics.Debug.Assert(slotInside != null);

                if (slotInside.Empty)
                {
                    j = i;
                    break;
                }
            }

            System.Diagnostics.Debug.Assert(j <= TotalSlotCount);
            if (j >= 0)
            {
                slotInside = GetPrimarySlot(j);
                System.Diagnostics.Debug.Assert(slotInside != null);
                System.Diagnostics.Debug.Assert(slotInside.Empty);

                slotInside.Give(stack);
            }

            return j >= 0;
        }

        internal void QuickMoveFromLeftInPrimary(InventorySlot slot)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            throw new System.NotImplementedException();
        }

        internal void QuickMoveFromRightInPrimary(InventorySlot slot)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            throw new System.NotImplementedException();
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
                System.Diagnostics.Debug.Assert(!slot.Empty);

                slotInside = GetMainSlot(i);
                System.Diagnostics.Debug.Assert(slotInside != null);
                System.Diagnostics.Debug.Assert(!ReferenceEquals(slotInside, slot));

                if (slotInside.Empty)
                {
                    j = (j < 0) ? i : j;

                    continue;
                }

                slotInside.Move(slot);

                if (slot.Empty)
                {
                    break;
                }
            }

            System.Diagnostics.Debug.Assert(j <= TotalSlotCount);
            if (!slot.Empty && j >= 0)
            {
                slotInside = GetMainSlot(j);
                System.Diagnostics.Debug.Assert(slotInside != null);
                System.Diagnostics.Debug.Assert(slotInside.Empty);

                slotInside.Move(slot);
            }

        }

        private void QuickMoveFromLeftInHotbar(InventorySlot slot)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            throw new System.NotImplementedException();
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

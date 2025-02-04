

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
        internal int IndexMainHandSlot => _indexMainHandSlot;


        internal PlayerInventory() : base(TotalSlotCount)
        {
            //GiveFromLeftInPrimary(new ItemStack(ItemType.Stick, "Stick!"));
            //GiveFromLeftInPrimary(new ItemStack(ItemType.Stick, "Stick!!!"));
            //GiveFromLeftInPrimary(new ItemStack(ItemType.DiamondSword, "Hello Sword!"));
            //GiveFromLeftInPrimary(new ItemStack(ItemType.Snowball));
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

        internal (ItemStack, bool) HandleMainHandSlot()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            InventorySlot mainSlot = GetMainHandSlot();

            if (mainSlot.Empty == false && mainSlot.Stack.IsBreaked == true)
            {
                mainSlot.Reset(null);
                return (null, true);
            }

            return (mainSlot.Stack, false);

            //else
            //{

            //    return (null, true);
            //}

            //if (mainSlot.Stack.IsBreaked == true)
            //{
            //    mainSlot.Reset(null);
            //    return (null, true);
            //}

            //throw new System.NotImplementedException();
        }

        internal InventorySlot HandleMainHandSlot2()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            InventorySlot mainSlot = GetMainHandSlot();

            if (mainSlot.Empty == false && mainSlot.Stack.IsBreaked == true)
            {
                mainSlot.Reset(null);
            }

            return mainSlot;
        }

        internal (byte[] helmet, byte[] mainHand, byte[] offHand) GetEquipmentsData()
        {
            InventorySlot slotHelmet = GetArmorSlot(0);
            InventorySlot slotMainHand = GetMainHandSlot();
            InventorySlot slotOffHand = GetOffHandSlot();
            System.Diagnostics.Debug.Assert(slotMainHand != null);
            System.Diagnostics.Debug.Assert(slotOffHand != null);
            return (slotHelmet.WriteData(), slotMainHand.WriteData(), slotOffHand.WriteData());
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

        internal void SetHelmet(ItemStack itemStack)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            InventorySlot slot = GetArmorSlot(0);

            System.Diagnostics.Debug.Assert(slot != null);
            slot.Reset(itemStack);
        }

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

        public void GiveItemStackFromLeftInPrimary(ref ItemStack itemStack)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(!_disposed);

            if (itemStack == null)
            {
                return;
            }

            using Queue<InventorySlot> slots = new();

            int _preMovedCount;
            int _preRemainingCount = itemStack.Count;

            InventorySlot slot;
            for (int i = 0; i < PrimarySlotCount; ++i)
            {
                slot = GetPrimarySlot(i);
                System.Diagnostics.Debug.Assert(slot != null);

                _preMovedCount = slot.PreMove(itemStack, _preRemainingCount);

                if (_preRemainingCount == _preMovedCount)
                {
                    continue;
                }

                _preRemainingCount = _preMovedCount;
                slots.Enqueue(slot);


                if (_preRemainingCount == 0)
                {
                    break;
                }
            }

            if (_preRemainingCount > 0)
            {
                return;
            }

            while (slots.Empty == false)
            {
                slot = slots.Dequeue();

                slot.Move(ref itemStack);

                if (itemStack == null)
                {
                    break;
                }
            }

            return;
        }

        public bool GiveItemStacksFromLeftInPrimary(IReadOnlyItem item, int count)
        {
            if (item == null)
            {
                throw new System.ArgumentNullException(nameof(item));
            }

            if (count < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(count));
            }

            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(count >= 0);
            if (count == 0)
            {
                return true;
            }

            using Queue<InventorySlot> slots = new();

            int _preMovedCount;
            int _preRemainingCount = count;

            InventorySlot slot;
            for (int i = 0; i < PrimarySlotCount; ++i)
            {
                slot = GetPrimarySlot(i);
                System.Diagnostics.Debug.Assert(slot != null);

                _preMovedCount = slot.PreMove(item, _preRemainingCount);

                if (_preRemainingCount == _preMovedCount)
                {
                    continue;
                }

                _preRemainingCount = _preMovedCount;
                slots.Enqueue(slot);


                if (_preRemainingCount == 0)
                {
                    break;
                }
            }

            if (_preRemainingCount > 0)
            {
                return false;
            }

            while (slots.Empty == false)
            {
                slot = slots.Dequeue();

                count = slot.Move(item, count);

                System.Diagnostics.Debug.Assert(count >= 0);
                if (count == 0)
                {
                    break;
                }
            }

            System.Diagnostics.Debug.Assert(count == 0);

            return true;
        }

        public ItemStack[] TakeItemStacksInPrimary(IReadOnlyItem item, int count)
        {
            if (item == null)
            {
                throw new System.ArgumentNullException(nameof(item));
            }

            if (count < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(count));
            }

            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (count == 0)
            {
                return [];
            }

            System.Diagnostics.Debug.Assert(item.Type.GetMaxCount() > 0);
            int minLength = (int)System.Math.Ceiling((double)count / (double)item.Type.GetMaxCount());
            ItemStack[] itemStacks = new ItemStack[minLength];

            int leftCount = count;

            InventorySlot slot;
            ItemStack targetItemStack;

            for (int i = 0; i < PrimarySlotCount && leftCount > 0; ++i)
            {
                slot = GetPrimarySlot(i);
                System.Diagnostics.Debug.Assert(slot != null);

                if (slot.Empty == true)
                {
                    continue;
                }

                targetItemStack = slot.Stack;

                if (targetItemStack.Equals(item) == true)
                {
                    int takedCount = slot.PreTake(leftCount);

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
                return null;
            }

            leftCount = count;
            int k = 0;

            ItemStack takedItemStack = null;

            for (int i = 0; i < PrimarySlotCount && leftCount > 0; ++i)
            {
                slot = GetPrimarySlot(i);
                System.Diagnostics.Debug.Assert(slot != null);

                if (slot.Empty == true)
                {
                    continue;
                }

                targetItemStack = slot.Stack;

                if (targetItemStack.Equals(item) == true)
                {
                    int takedCount = takedItemStack == null
                        ? slot.Take(out takedItemStack, leftCount)
                        : takedItemStack.Count;

                    System.Diagnostics.Debug.Assert(takedCount >= 0);
                    System.Diagnostics.Debug.Assert(takedCount <= count);

                    if (takedCount > 0)
                    {
                        System.Diagnostics.Debug.Assert(takedItemStack != null);

                        if (itemStacks[k] == null)
                        {
                            itemStacks[k] = takedItemStack;
                        }
                        else
                        {
                            itemStacks[k].Move(ref takedItemStack);

                            if (takedItemStack != null)
                            {
                                itemStacks[++k] = takedItemStack;
                            }
                        }

                        leftCount -= takedCount;
                        takedItemStack = null;

                        System.Diagnostics.Debug.Assert(itemStacks[k].Count <= itemStacks[k].MaxCount);
                        if (itemStacks[k].Count == itemStacks[k].MaxCount)
                        {
                            ++k;
                        }


                    }
                }

            }

            System.Diagnostics.Debug.Assert(leftCount == 0);

            return itemStacks;
        }

        public ItemStack[] GiveAndTakeItemStacksFromLeftInPrimary(
            IReadOnlyItem giveItem, int giveCount,
            IReadOnlyItem takeItem, int takeCount)
        {
            if (giveItem == null)
            {
                throw new System.ArgumentNullException(nameof(giveItem));
            }

            if (giveCount < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(giveCount));
            }

            if (takeItem == null)
            {
                throw new System.ArgumentNullException(nameof(takeItem));
            }

            if (takeCount < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(takeCount));
            }

            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(_disposed == false);

            using Queue<InventorySlot> slots = new();

            InventorySlot slot;

            if (giveCount > 0)
            {
                int _preMovedCount;
                int _preRemainingCount = giveCount;

                for (int i = 0; i < PrimarySlotCount; ++i)
                {
                    slot = GetPrimarySlot(i);
                    System.Diagnostics.Debug.Assert(slot != null);

                    _preMovedCount = slot.PreMove(giveItem, _preRemainingCount);

                    if (_preRemainingCount == _preMovedCount)
                    {
                        continue;
                    }

                    _preRemainingCount = _preMovedCount;
                    slots.Enqueue(slot);


                    if (_preRemainingCount == 0)
                    {
                        break;
                    }
                }

                if (_preRemainingCount > 0)
                {
                    return null;
                }

            }

            if (takeCount > 0)
            {
                ItemStack targetItemStack;
                int leftCount = takeCount;

                for (int i = 0; i < PrimarySlotCount && leftCount > 0; ++i)
                {
                    slot = GetPrimarySlot(i);
                    System.Diagnostics.Debug.Assert(slot != null);

                    if (slot.Empty == true)
                    {
                        continue;
                    }

                    targetItemStack = slot.Stack;

                    if (targetItemStack.Equals(takeItem) == true)
                    {
                        int takedCount = slot.PreTake(leftCount);

                        System.Diagnostics.Debug.Assert(takedCount >= 0);
                        System.Diagnostics.Debug.Assert(takedCount <= takeCount);

                        if (takedCount > 0)
                        {
                            leftCount -= takedCount;
                        }
                    }

                }

                if (leftCount > 0)
                {
                    return null;
                }
            }

            if (giveCount > 0)
            {
                while (slots.Empty == false)
                {
                    slot = slots.Dequeue();

                    giveCount = slot.Move(giveItem, giveCount);

                    System.Diagnostics.Debug.Assert(giveCount >= 0);
                    if (giveCount == 0)
                    {
                        break;
                    }
                }

                System.Diagnostics.Debug.Assert(giveCount == 0);

            }

            if (takeCount > 0)
            {
                System.Diagnostics.Debug.Assert(takeItem.MaxCount > 0);
                int minLength = (int)System.Math.Ceiling((double)takeCount / (double)takeItem.MaxCount);
                ItemStack[] itemStacks = new ItemStack[minLength];

                ItemStack targetItemStack;
                ItemStack takedItemStack = null;

                int leftCount = takeCount;
                int k = 0;

                for (int i = 0; i < PrimarySlotCount && leftCount > 0; ++i)
                {
                    slot = GetPrimarySlot(i);
                    System.Diagnostics.Debug.Assert(slot != null);

                    if (slot.Empty == true)
                    {
                        continue;
                    }

                    targetItemStack = slot.Stack;

                    if (targetItemStack.Equals(takeItem) == true)
                    {
                        int takedCount = takedItemStack == null
                            ? slot.Take(out takedItemStack, leftCount)
                            : takedItemStack.Count;

                        System.Diagnostics.Debug.Assert(takedCount >= 0);
                        System.Diagnostics.Debug.Assert(takedCount <= takeCount);

                        if (takedCount > 0)
                        {
                            System.Diagnostics.Debug.Assert(takedItemStack != null);

                            if (itemStacks[k] == null)
                            {
                                itemStacks[k] = takedItemStack;
                            }
                            else
                            {
                                itemStacks[k].Move(ref takedItemStack);

                                if (takedItemStack != null)
                                {
                                    itemStacks[++k] = takedItemStack;
                                }
                            }

                            leftCount -= takedCount;
                            takedItemStack = null;

                            System.Diagnostics.Debug.Assert(itemStacks[k].Count <= itemStacks[k].MaxCount);
                            if (itemStacks[k].Count == itemStacks[k].MaxCount)
                            {
                                ++k;
                            }


                        }
                    }

                }

                System.Diagnostics.Debug.Assert(leftCount == 0);

                return itemStacks;
            }

            return [];
        }


        public bool GiveItemStacks(IReadOnlyItem item, int count)
        {
            if (item == null)
            {
                throw new System.ArgumentNullException(nameof(item));
            }

            if (count < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(count));
            }

            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(count >= 0);
            if (count == 0)
            {
                return true;
            }

            return GiveItemStacksFromLeftInPrimary(item, count);
        }

        public void GiveItemStack(ref ItemStack itemStack)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            if (itemStack == null)
            {
                return;
            }

            GiveItemStackFromLeftInPrimary(ref itemStack);
        }

        public ItemStack[] TakeItemStacks(IReadOnlyItem item, int count)
        {
            if (item == null)
            {
                throw new System.ArgumentNullException(nameof(item));
            }

            if (count < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(count));
            }

            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(_disposed == false);

            return TakeItemStacksInPrimary(item, count);
        }

        public ItemStack[] GiveAndTakeItemStacks(
            IReadOnlyItem giveItem, int giveCount,
            IReadOnlyItem takeItem, int takeCount)
        {
            if (giveItem == null)
            {
                throw new System.ArgumentNullException(nameof(giveItem));
            }

            if (giveCount < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(giveCount));
            }

            if (takeItem == null)
            {
                throw new System.ArgumentNullException(nameof(takeItem));
            }

            if (takeCount < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(takeCount));
            }

            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(_disposed == false);

            return GiveAndTakeItemStacksFromLeftInPrimary(
                giveItem, giveCount,
                takeItem, takeCount);
        }

        private ItemStack[] GiveAndTakeItemStacks(
            ref ItemStack itemStack,
            IReadOnlyItem takeItem, int takeCount)
        {
            if (takeItem == null)
            {
                throw new System.ArgumentNullException(nameof(takeItem));
            }

            if (takeCount < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(takeCount));
            }

            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(_disposed == false);

            throw new System.NotImplementedException();
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

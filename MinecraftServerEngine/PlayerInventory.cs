

using Containers;

using MinecraftPrimitives;
using static System.Reflection.Metadata.BlobBuilder;

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

        private int _activeMainHandIndex = 0;  // 0-8
        internal int ActiveMainHandIndex
        {
            get
            {
                System.Diagnostics.Debug.Assert(_disposed == false);
                return _activeMainHandIndex;
            }
        }


        internal PlayerInventory() : base(TotalSlotCount)
        {
        }

        ~PlayerInventory()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        internal System.Collections.Generic.IEnumerable<InventorySlot> GetCraftingInputSlots()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            for (int i = 0; i < CraftingInputSlotCount; ++i)
            {
                yield return Slots[i + CraftingInputSlotsOffset];
            }
        }
        internal System.Collections.Generic.IEnumerable<InventorySlot> GetArmorSlots()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            for (int i = 0; i < ArmorSlotCount; ++i)
            {
                yield return Slots[i + ArmorSlotsOffset];
            }
        }
        internal System.Collections.Generic.IEnumerable<InventorySlot> GetPrimarySlots()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            for (int i = 0; i < PrimarySlotCount; ++i)
            {
                yield return Slots[i + PrimarySlotsOffset];
            }
        }
        private System.Collections.Generic.IEnumerable<InventorySlot> GetMainSlots()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            for (int i = 0; i < MainSlotCount; ++i)
            {
                yield return Slots[i + MainSlotsOffset];
            }
        }
        private System.Collections.Generic.IEnumerable<InventorySlot> GetHotbarSlots()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            for (int i = 0; i < HotbarSlotCount; ++i)
            {
                yield return Slots[i + HotbarSlotsOffset];
            }
        }

        internal InventorySlot GetCraftingOutputSlot()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            return Slots[CraftingOutputSlotIndex];
        }
        internal InventorySlot GetHelmetSlot()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            return Slots[HelmetSlotIndex];
        }
        internal InventorySlot GetChestplateSlot()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            return Slots[ChestplateSlotIndex];
        }
        internal InventorySlot GetLeggingsSlot()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            return Slots[LeggingsSlotIndex];
        }
        internal InventorySlot GetBootsSlot()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            return Slots[BootsSlotIndex];
        }
        internal InventorySlot GetArmorSlot(int i)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < ArmorSlotCount);

            return Slots[i + ArmorSlotsOffset];
        }
        internal InventorySlot GetPrimarySlot(int i)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < PrimarySlotCount);

            return Slots[i + PrimarySlotsOffset];
        }
        internal void SetPrimarySlot(int i, InventorySlot slot)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < PrimarySlotCount);

            Slots[i + PrimarySlotsOffset] = slot;
        }
        internal InventorySlot GetMainSlot(int i)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < MainSlotCount);

            System.Diagnostics.Debug.Assert(MainSlotsOffset >= 0);
            System.Diagnostics.Debug.Assert(i + MainSlotsOffset < TotalSlotCount);
            return Slots[i + MainSlotsOffset];
        }
        internal InventorySlot GetHotbarSlot(int i)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < HotbarSlotCount);

            System.Diagnostics.Debug.Assert(HotbarSlotsOffset >= 0);
            System.Diagnostics.Debug.Assert(i + HotbarSlotsOffset < TotalSlotCount);
            return Slots[i + HotbarSlotsOffset];
        }
        internal InventorySlot GetOffHandSlot()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            return Slots[OffHandSlotIndex];
        }

        internal void ChangeActiveMainHandIndex(int index)
        {
            System.Diagnostics.Debug.Assert(index >= 0);
            System.Diagnostics.Debug.Assert(index < HotbarSlotCount);
            _activeMainHandIndex = index;
        }

        internal InventorySlot GetMainHandSlot()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_activeMainHandIndex >= 0);
            System.Diagnostics.Debug.Assert(_activeMainHandIndex < HotbarSlotCount);

            return Slots[_activeMainHandIndex + HotbarSlotsOffset];
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


        internal void SetHelmet(ItemStack itemStack)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            InventorySlot slot = GetArmorSlot(0);

            System.Diagnostics.Debug.Assert(slot != null);
            slot.Reset(itemStack);
        }

        internal void LeftClick(InventorySlot slot, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(slot != null);
            System.Diagnostics.Debug.Assert(cursor != null);

            slot.LeftClick(cursor);
        }

        internal void LeftClickInPrimary(int i, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < PrimarySlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);

            InventorySlot slot = GetPrimarySlot(i);
            System.Diagnostics.Debug.Assert(slot != null);

            LeftClick(slot, cursor);
        }

        internal void LeftClick(int i, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

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
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(slot != null);
            System.Diagnostics.Debug.Assert(cursor != null);

            slot.RightClick(cursor);
        }

        internal void RightClickInPrimary(int i, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);

            InventorySlot slot = GetPrimarySlot(i);
            System.Diagnostics.Debug.Assert(slot != null);

            RightClick(slot, cursor);
        }

        internal void RightClick(int i, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

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

        private bool CanGiveItemStacksFromLeft(
            int slotCount, System.Func<int, InventorySlot> getSlot,
            IReadOnlyItem item, int count,
            Queue<InventorySlot> slots,
            Queue<InventorySlot> emptySlots
            )
        {
            System.Diagnostics.Debug.Assert(slotCount > 0);
            System.Diagnostics.Debug.Assert(getSlot != null);

            System.Diagnostics.Debug.Assert(item != null);
            System.Diagnostics.Debug.Assert(count >= 0);

            System.Diagnostics.Debug.Assert(slots != null);
            System.Diagnostics.Debug.Assert(emptySlots != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(count >= 0);
            if (count == 0)
            {
                return true;
            }

            InventorySlot slot;

            int _preMovedCount;
            int _preRemainingCount = count;

            for (int i = 0; i < slotCount; ++i)
            {
                System.Diagnostics.Debug.Assert(_preRemainingCount > 0);

                slot = getSlot(i);
                System.Diagnostics.Debug.Assert(slot != null);

                if (slot.Empty == true)
                {
                    emptySlots.Enqueue(slot);
                    continue;
                }

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

            System.Diagnostics.Debug.Assert(item.MaxCount >= Item.MinCount);
            if (
                _preRemainingCount > 0 &&
                _preRemainingCount > (emptySlots.Length * item.MaxCount)
                )
            {
                return false;
            }

            return true;
        }

        private void GiveItemStacksFromLeft(
            IReadOnlyItem item, int count,
            Queue<InventorySlot> slots,
            Queue<InventorySlot> emptySlots
            )
        {
            System.Diagnostics.Debug.Assert(item != null);
            System.Diagnostics.Debug.Assert(count >= 0);

            System.Diagnostics.Debug.Assert(slots != null);
            System.Diagnostics.Debug.Assert(emptySlots != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(count >= 0);
            if (count == 0)
            {
                return;
            }

            InventorySlot slot;

            System.Diagnostics.Debug.Assert(slots != null);
            while (slots.Empty == false)
            {
                System.Diagnostics.Debug.Assert(count > 0);

                slot = slots.Dequeue();

                count = slot.Move(item, count);

                System.Diagnostics.Debug.Assert(count >= 0);
                if (count == 0)
                {
                    break;
                }
            }

            System.Diagnostics.Debug.Assert(emptySlots != null);
            while (emptySlots.Empty == false && count > 0)
            {
                slot = emptySlots.Dequeue();

                count = slot.Move(item, count);

                System.Diagnostics.Debug.Assert(count >= 0);
                if (count == 0)
                {
                    break;
                }
            }

            System.Diagnostics.Debug.Assert(count == 0);
        }

        private bool CanTakeItemStacks(
            int slotCount, System.Func<int, InventorySlot> getSlot,
            IReadOnlyItem item, int count
            )
        {
            System.Diagnostics.Debug.Assert(slotCount > 0);
            System.Diagnostics.Debug.Assert(getSlot != null);

            System.Diagnostics.Debug.Assert(item != null);
            System.Diagnostics.Debug.Assert(count >= 0);

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (count == 0)
            {
                return true;
            }

            InventorySlot slot;

            for (int i = 0; i < slotCount && count > 0; ++i)
            {
                slot = getSlot(i);
                System.Diagnostics.Debug.Assert(slot != null);

                if (slot.Empty == true)
                {
                    continue;
                }

                int takedCount = slot.PreTake(item, count);

                System.Diagnostics.Debug.Assert(takedCount >= 0);
                System.Diagnostics.Debug.Assert(takedCount <= count);

                if (takedCount > 0)
                {
                    count -= takedCount;
                }

            }

            System.Diagnostics.Debug.Assert(count >= 0);
            return count == 0;
        }

        private ItemStack[] _TakeItemStacks(
            int slotCount, System.Func<int, InventorySlot> getSlot,
            IReadOnlyItem item, int count
            )
        {
            System.Diagnostics.Debug.Assert(item != null);
            System.Diagnostics.Debug.Assert(count >= 0);

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (count == 0)
            {
                return [];
            }

            InventorySlot slot;

            int k = 0;

            System.Diagnostics.Debug.Assert(item.Type.GetMaxCount() > 0);
            int minLength = (int)System.Math.Ceiling((double)count / (double)item.Type.GetMaxCount());
            ItemStack[] itemStacks = new ItemStack[minLength];

            ItemStack takedItemStack;

            for (int i = 0; i < slotCount && count > 0; ++i)
            {
                slot = getSlot(i);
                System.Diagnostics.Debug.Assert(slot != null);

                if (slot.Empty == true)
                {
                    continue;
                }

                int takedCount = slot.Take(out takedItemStack, item, count);

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

                    count -= takedCount;
                    takedItemStack = null;

                    System.Diagnostics.Debug.Assert(itemStacks[k].Count <= itemStacks[k].MaxCount);
                    if (itemStacks[k].Count == itemStacks[k].MaxCount)
                    {
                        ++k;
                    }


                }

            }

            System.Diagnostics.Debug.Assert(count == 0);

            return itemStacks;
        }


        public bool GiveItemStackFromLeftInPrimary(ref ItemStack itemStack)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (itemStack == null)
            {
                return true;
            }

            InventorySlot slot;

            using Queue<InventorySlot> slots = new();
            InventorySlot emptySlot = null;

            try
            {
                int _preMovedCount;
                int _preRemainingCount = itemStack.Count;

                for (int i = 0; i < PrimarySlotCount; ++i)
                {
                    System.Diagnostics.Debug.Assert(_preRemainingCount > 0);

                    slot = GetPrimarySlot(i);
                    System.Diagnostics.Debug.Assert(slot != null);

                    if (slot.Empty == true)
                    {
                        if (emptySlot == null)
                        {
                            emptySlot = slot;
                        }

                        continue;
                    }

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


                if (
                    _preRemainingCount > 0 &&
                    emptySlot == null
                    )
                {
                    return false;
                }
                System.Diagnostics.Debug.Assert(itemStack.MaxCount >= Item.MinCount);
                System.Diagnostics.Debug.Assert(_preRemainingCount <= itemStack.MaxCount);

                System.Diagnostics.Debug.Assert(slots != null);
                while (slots.Length > 0)
                {
                    System.Diagnostics.Debug.Assert(itemStack != null);

                    slot = slots.Dequeue();

                    slot.Move(ref itemStack);

                    if (slot.MoveAll(ref itemStack) == true)
                    {
                        break;
                    }
                }

                if (itemStack != null)
                {
                    System.Diagnostics.Debug.Assert(emptySlot != null);
                    System.Diagnostics.Debug.Assert(emptySlot.Empty == true);
                    emptySlot.MoveAll(ref itemStack);
                }

                return true;
            }
            finally
            {
                System.Diagnostics.Debug.Assert(slots != null);
                slots.Flush();
            }
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
            using Queue<InventorySlot> emptySlots = new();

            try
            {
                if (
                    CanGiveItemStacksFromLeft(
                        MainSlotCount, GetMainSlot,
                        item, count,
                        slots, emptySlots) == false
                    )
                {
                    return false;
                }

                GiveItemStacksFromLeft(
                    item, count,
                    slots, emptySlots);

                return true;
            }

            finally
            {
                System.Diagnostics.Debug.Assert(slots != null);
                slots.Flush();
                System.Diagnostics.Debug.Assert(emptySlots != null);
                emptySlots.Flush();
            }
            

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

            if (
                CanTakeItemStacks(
                    PrimarySlotCount, GetPrimarySlot,
                    item, count) == false
                )
            {
                return null;
            }

            return _TakeItemStacks(PrimarySlotCount, GetPrimarySlot, item, count);
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

            using Queue<InventorySlot> giveSlots = new();
            using Queue<InventorySlot> giveEmptySlots = new();

            try
            {
                if (
                    CanGiveItemStacksFromLeft(
                        MainSlotCount, GetMainSlot,
                        giveItem, giveCount,
                        giveSlots, giveEmptySlots) == false
                    )
                {
                    return null;
                }

                if (CanTakeItemStacks(
                    PrimarySlotCount, GetPrimarySlot,
                    takeItem, takeCount) == false)
                {
                    return null;
                }

                GiveItemStacksFromLeft(
                    giveItem, giveCount,
                    giveSlots, giveEmptySlots);

                return _TakeItemStacks(
                    PrimarySlotCount, GetPrimarySlot, 
                    takeItem, takeCount
                    );
            } 
            finally
            {
                System.Diagnostics.Debug.Assert(giveSlots != null);
                giveSlots.Flush();
                System.Diagnostics.Debug.Assert(giveEmptySlots != null);
                giveEmptySlots.Flush();
            }
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

        public bool GiveItemStack(ref ItemStack itemStack)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            if (itemStack == null)
            {
                return true;
            }

            return GiveItemStackFromLeftInPrimary(ref itemStack);
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


        internal void QuickMoveFromLeftInPrimary(InventorySlot slot)
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
            System.Diagnostics.Debug.Assert(_disposed == false);

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
            System.Diagnostics.Debug.Assert(_disposed == false);

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
            System.Diagnostics.Debug.Assert(_disposed == false);

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
                QuickMoveFromLeftInHotbar(slot);
            }
            else if (i >= HotbarSlotsOffset && i < OffHandSlotIndex)
            {
                QuickMoveFromLeftInMain(slot);
            }
            else
            {
                System.Diagnostics.Debug.Assert(i == OffHandSlotIndex);
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


        internal void WriteMainHandData(MinecraftProtocolDataStream buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            InventorySlot slot = GetMainHandSlot();

            System.Diagnostics.Debug.Assert(slot != null);
            slot.WriteData(buffer);
        }

        internal void WriteOffHandData(MinecraftProtocolDataStream buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            InventorySlot slot = GetOffHandSlot();

            System.Diagnostics.Debug.Assert(slot != null);
            slot.WriteData(buffer);
        }

        internal void WriteHelmetData(MinecraftProtocolDataStream buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            InventorySlot slot = GetArmorSlot(0);

            System.Diagnostics.Debug.Assert(slot != null);
            slot.WriteData(buffer);
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

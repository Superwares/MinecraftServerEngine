using Common;
using Containers;
using Sync;

namespace MinecraftServerEngine
{
    
    public abstract class Inventory : System.IDisposable
    {
        internal const int SlotsPerLine = 9;

        private bool _disposed = false;

        public readonly int TotalSlotCount;

        internal readonly Slot[] Slots;

        internal Inventory(int n)
        {
            System.Diagnostics.Debug.Assert(n > 0);
            TotalSlotCount = n;

            System.Diagnostics.Debug.Assert(TotalSlotCount > 0);

            Slots = new Slot[TotalSlotCount];
            for (int i = 0; i < TotalSlotCount; ++i)
            {
                Slots[i] = new Slot();
            }
        }

        ~Inventory() => System.Diagnostics.Debug.Assert(false);

        public void Print()
        {
            Console.Printl($"Inventory: ");
            for (int i = 0; i < TotalSlotCount; ++i)
            {
                if (i % SlotsPerLine == 0)
                {
                    Console.NewLine();
                    Console.NewTab();
                }

                Slot slot = Slots[i];
                System.Diagnostics.Debug.Assert(slot != null);

                Console.Print($"{i}:D2:[{slot}]");
            }
            Console.NewLine();
        }

        public virtual void Dispose()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // Assertion.

            // Release resources.

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }

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
        private const int HotbarSlotCount = 9;

        internal System.Collections.Generic.IEnumerable<Slot> GetCraftingInputSlots()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            for (int i = 0; i < CraftingInputSlotCount; ++i)
            {
                yield return Slots[i + CraftingInputSlotsOffset];
            }
        }
        internal System.Collections.Generic.IEnumerable<Slot> GetArmorSlots()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            for (int i = 0; i < ArmorSlotCount; ++i)
            {
                yield return Slots[i + ArmorSlotsOffset];
            }
        }
        internal System.Collections.Generic.IEnumerable<Slot> GetPrimarySlots()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            for (int i = 0; i < PrimarySlotCount; ++i)
            {
                yield return Slots[i + PrimarySlotsOffset];
            }
        }
        private System.Collections.Generic.IEnumerable<Slot> GetMainSlots()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            for (int i = 0; i < MainSlotCount; ++i)
            {
                yield return Slots[i + MainSlotsOffset];
            }
        }
        private System.Collections.Generic.IEnumerable<Slot> GetHotbarSlots()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            for (int i = 0; i < HotbarSlotCount; ++i)
            {
                yield return Slots[i + HotbarSlotsOffset];
            }
        }

        private Slot GetCraftingOutputSlot()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return Slots[CraftingOutputSlotIndex];
        }
        private Slot GetHelmetSlot()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return Slots[HelmetSlotIndex];
        }
        private Slot GetChestplateSlot()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return Slots[ChestplateSlotIndex];
        }
        private Slot GetLeggingsSlot()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return Slots[LeggingsSlotIndex];
        }
        private Slot GetBootsSlot()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return Slots[BootsSlotIndex];
        }
        private Slot GetArmorSlot(int i)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < ArmorSlotCount);

            return Slots[i + ArmorSlotsOffset];
        }
        internal Slot GetPrimarySlot(int i)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < PrimarySlotCount);

            return Slots[i + PrimarySlotsOffset];
        }
        private Slot GetMainSlot(int i)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < MainSlotCount);

            System.Diagnostics.Debug.Assert(MainSlotsOffset >= 0);
            System.Diagnostics.Debug.Assert(i + MainSlotsOffset < TotalSlotCount);
            return Slots[i + MainSlotsOffset];
        }
        private Slot GetHotbarSlot(int i)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < HotbarSlotCount);

            System.Diagnostics.Debug.Assert(HotbarSlotsOffset >= 0);
            System.Diagnostics.Debug.Assert(i + HotbarSlotsOffset < TotalSlotCount);
            return Slots[i + HotbarSlotsOffset];
        }
        private Slot GetOffHandSlot()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return Slots[OffHandSlotIndex];
        }

        private int _indexMainHandSlot = 0;  // 0-8
        private Slot GetMainHandSlot()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_indexMainHandSlot >= 0);
            System.Diagnostics.Debug.Assert(_indexMainHandSlot < HotbarSlotCount);

            return Slots[_indexMainHandSlot];
        }

        private bool _disposed = false;

        internal PlayerInventory() : base(46) 
        {
            GiveFromLeftInMain(ItemType.Stick, 100);
            GiveFromLeftInMain(ItemType.Snowball, 100);
        }

        ~PlayerInventory() => System.Diagnostics.Debug.Assert(false);

        /*internal virtual void TakeAll(
            int i, ref ItemSlot cursor, WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount);
            System.Diagnostics.Debug.Assert(cursor == null);
            System.Diagnostics.Debug.Assert(renderer != null);

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            ref ItemSlot slot = ref Slots[i];

            if (slot != null)
            {
                System.Diagnostics.Debug.Assert(!ReferenceEquals(slot, cursor));
                cursor = slot;
                slot = null;

                --_count;
                System.Diagnostics.Debug.Assert(_count >= 0);
            }
            else
            {
                System.Diagnostics.Debug.Assert(cursor == null);
            }

            Refresh(renderer);
        }

        internal virtual void PutAll(
            int i, ref ItemSlot cursor, WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);
            System.Diagnostics.Debug.Assert(renderer != null);

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);
            
            ref ItemSlot slot = ref Slots[i];
            System.Diagnostics.Debug.Assert(!ReferenceEquals(slot, cursor));

            if (slot != null)
            {
                if (cursor.Item == slot.Item)
                {
                    int spend = slot.Stack(cursor.Count);
                    System.Diagnostics.Debug.Assert(spend <= cursor.Count);
                    if (spend == cursor.Count)
                    {
                        cursor = null;
                    }
                    else
                    {
                        cursor.Spend(spend);
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
                slot = cursor;
                cursor = null;

                ++_count;
                System.Diagnostics.Debug.Assert(_count >= 0);
                System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            }

            Refresh(renderer);
        }

        internal virtual void TakeHalf(
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

        internal void LeftClick(int i, Slot cursor)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);

            Slot slot = Slots[i];
            System.Diagnostics.Debug.Assert(slot != null);

            slot.LeftClick(cursor);
        }

        internal void RightClick(int i, Slot cursor)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);

            Slot slot = Slots[i];
            System.Diagnostics.Debug.Assert(slot != null);

            slot.RightClick(cursor);
        }

        internal bool GiveFromLeftInPrimary(ItemStack stack)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(stack != null);

            int j = -1;

            Slot slotInside;
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

            if (j < 0)
            {
                return false;
            }

            System.Diagnostics.Debug.Assert(j <= TotalSlotCount);
            if (j >= 0)
            {
                slotInside = GetPrimarySlot(j);
                System.Diagnostics.Debug.Assert(slotInside != null);
                System.Diagnostics.Debug.Assert(slotInside.Empty);

                slotInside.Give(stack);
            }

            return true;
        }

        internal void QuickMoveFromLeftInPrimary(Slot slot)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            throw new System.NotImplementedException();
        }

        internal void QuickMoveFromRightInPrimary(Slot slot)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            throw new System.NotImplementedException();
        }

        private void QuickMoveFromLeftInMain(Slot slot)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(slot != null);

            if (slot.Empty)
            {
                return;
            }

            int j = -1;

            Slot slotInside;
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

        private void QuickMoveFromLeftInHotbar(Slot slot)
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

            Slot slot = Slots[i];
            System.Diagnostics.Debug.Assert(slot != null);

            if (i == CraftingOutputSlotIndex)
            {
                QuickMoveFromRightInPrimary(slot);
            }
            else if (i >= CraftingInputSlotsOffset && i < ArmorSlotsOffset)
            {
                QuickMoveFromLeftInPrimary(slot);
            }
            else if (i >= ArmorSlotsOffset && < MainSlotsOffset)
            {
                QuickMoveFromLeftInPrimary(slot);
            }
            else if (i >= MainSlotsOffset && i < HotbarSlotsOffset)
            {
                QuickMoveFromLeftInArmor(slot);
                QuickMoveFromLeftInHotbar(slot);
            }
            else if (i >= HotbarSlotsOffset && i < OffHandSlotIndex)
            {
                QuickMoveFromLeftInArmor(slot);
                QuickMoveFromLeftInMain(slot);
            }
            else
            {
                System.Diagnostics.Debug.Assert(i == OffHandSlotIndex);
                QuickMoveFromLeftInArmor(slot);
                QuickMoveFromLeftInPrimary(slot);
            }
        }

        public override void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

    public abstract class PublicInventory : Inventory
    {
        private bool _disposed = false;

        protected abstract string Title { get; }

        private protected readonly Locker Locker = new();  // Disposable

        private readonly Map<PlayerInventory, PublicInventoryRenderer> Renderers = new();

        internal PublicInventory(int totalSlotCount) : base(totalSlotCount) { }

        ~PublicInventory() => System.Diagnostics.Debug.Assert(false);

        private protected void Update()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Locker.Hold();

            using Buffer buffer = new();

            System.Diagnostics.Debug.Assert(Slots != null);
            foreach (Slot slot in Slots)
            {
                System.Diagnostics.Debug.Assert(slot != null);
                slot.WriteData(buffer);
            }

            byte[] data = buffer.ReadData();

            System.Diagnostics.Debug.Assert(TotalSlotCount > 0);
            System.Diagnostics.Debug.Assert(Renderers != null);
            foreach (PublicInventoryRenderer renderer in Renderers.GetValues())
            {
                renderer.Update(TotalSlotCount, data);
            }

            Locker.Release();
        }

        internal void Open(
            PlayerInventory invPrivate, 
            ConcurrentQueue<ClientboundPlayingPacket> outPackets)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(outPackets != null);

            PublicInventoryRenderer renderer = new(outPackets);

            Locker.Hold();

            renderer.Open(Title, Slots);

            System.Diagnostics.Debug.Assert(!Renderers.Contains(invPrivate));
            Renderers.Insert(invPrivate, renderer);

            Locker.Release();
        }

        internal void Close(PlayerInventory invPrivate)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(invPrivate != null);

            Locker.Hold();

            System.Diagnostics.Debug.Assert(Renderers.Contains(invPrivate));
            Renderers.Extract(invPrivate);

            Locker.Release();
        }

        /*private ref ItemSlot GetSlot(
            PrivateInventory invPrivate, int i, bool isPublic)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(invPrivate != null);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + invPrivate.PrimarySlotCount);

            return ref isPublic ? ref Slots[i] : ref invPrivate.GetPrimarySlot(i);
        }*/

        /*private void SetSlot(
            PrivateInventory invPrivate, int i, ItemSlot slot, bool isPublic)
        {
            System.Diagnostics.Debug.Assert(invPrivate != null);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + invPrivate.PrimarySlotCount);

            System.Diagnostics.Debug.Assert(slot != null);

            if (isPublic)
            {
                Slots[i] = slot;
            }
            else
            {
                invPrivate.SetPrimarySlot(i, slot);
            }

        }

        private void EmptySlot(
            PrivateInventory invPrivate, int i, bool isPublic)
        {
            System.Diagnostics.Debug.Assert(invPrivate != null);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + invPrivate.PrimarySlotCount);

            if (isPublic)
            {
                Slots[i] = null;
            }
            else
            {
                invPrivate.EmptyPrimarySlot(i);
            }

        }*/

        /*internal virtual void TakeAll(
            PrivateInventory invPrivate, int i, ref ItemSlot cursor,
            WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + invPrivate.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(cursor == null);
            System.Diagnostics.Debug.Assert(renderer != null);

            System.Diagnostics.Debug.Assert(Renderers.Contains(renderer));

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            bool isPublic = (i < TotalSlotCount);
            int j = isPublic ? i : i - TotalSlotCount;

            Locker.Hold();

            ref ItemSlot slot = ref GetSlot(invPrivate, j, isPublic);
            
            if (slot != null)
            {
                System.Diagnostics.Debug.Assert(!ReferenceEquals(slot, cursor));

                cursor = slot;
                slot = null;

                --_count;
                System.Diagnostics.Debug.Assert(_count >= 0);
            }
            else
            {
                System.Diagnostics.Debug.Assert(cursor == null);

            }

            Update(invPrivate, renderer);

            if (isPublic)
            {
                Broadcast(renderer);
            }

            Locker.Release();
        }

        internal virtual void PutAll(
            PrivateInventory invPrivate, int i, ref ItemSlot cursor,
            WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + invPrivate.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);
            System.Diagnostics.Debug.Assert(renderer != null);

            System.Diagnostics.Debug.Assert(Renderers.Contains(renderer));

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            bool isPublic = (i < TotalSlotCount);
            int j = isPublic ? i : i - TotalSlotCount;

            Locker.Hold();
            
            ref ItemSlot slot = ref GetSlot(invPrivate, j, isPublic);
            System.Diagnostics.Debug.Assert(!ReferenceEquals(slot, cursor));

            if (slot != null)
            {
                if (cursor.Item == slot.Item)
                {
                    int spend = slot.Stack(cursor.Count);
                    System.Diagnostics.Debug.Assert(spend <= cursor.Count);
                    if (spend == cursor.Count)
                    {
                        cursor = null;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(spend < cursor.Count);
                        cursor.Spend(spend);
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
                slot = cursor;
                cursor = null;

                ++_count;
                System.Diagnostics.Debug.Assert(_count >= 0);
                System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            }

            Update(invPrivate, renderer);

            if (isPublic)
            {
                Broadcast(renderer);
            }

            Locker.Release();

        }

        internal virtual void TakeHalf(
            PrivateInventory invPrivate, int i, ref ItemSlot cursor,
            WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + invPrivate.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(cursor == null);
            System.Diagnostics.Debug.Assert(renderer != null);

            System.Diagnostics.Debug.Assert(Renderers.Contains(renderer));

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            bool isPublic = (i < TotalSlotCount);
            int j = isPublic ? i : i - TotalSlotCount;

            Locker.Hold();

            ref ItemSlot slot = ref GetSlot(invPrivate, j, isPublic);

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

            Update(invPrivate, renderer);

            if (isPublic)
            {
                Broadcast(renderer);
            }

            Locker.Release();

        }

        internal virtual void PutOne(
            PrivateInventory invPrivate, int i, ref ItemSlot cursor, 
            WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + invPrivate.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);
            System.Diagnostics.Debug.Assert(renderer != null);

            System.Diagnostics.Debug.Assert(Renderers.Contains(renderer));

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            bool isPublic = (i < TotalSlotCount);
            int j = isPublic ? i : i - TotalSlotCount;

            Locker.Hold();

            ref ItemSlot slot = ref GetSlot(invPrivate, j, isPublic);
            System.Diagnostics.Debug.Assert(!ReferenceEquals(slot, cursor));

            if (slot != null)
            {
                if (cursor.Item == slot.Item)
                {
                    int spend = slot.Stack(1);
                    System.Diagnostics.Debug.Assert(spend <= 1);
                    if (spend == cursor.Count)
                    {
                        cursor = null;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(spend < cursor.Count);
                        cursor.Spend(spend);
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

            Update(invPrivate, renderer);

            if (isPublic)
            {
                Broadcast(renderer);
            }
            
            Locker.Release();
        }*/

        internal virtual void LeftClick(
            World world, Player player,
            int i, Slot cursor, PlayerInventory invPrivate)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(player != null);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + invPrivate.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);
            System.Diagnostics.Debug.Assert(invPrivate != null);

            Locker.Hold();

            System.Diagnostics.Debug.Assert(Renderers.Contains(invPrivate));

            if (i < TotalSlotCount)
            {
                Slot slot = Slots[i];
                System.Diagnostics.Debug.Assert(slot != null);

                slot.LeftClick(cursor);

                Update();
            }
            else
            {
                invPrivate.LeftClick(world, player, i, cursor);
            }

            Locker.Release();
        }

        internal virtual void RightClick(
            World world, Player player,
            int i, Slot cursor, PlayerInventory invPrivate)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(player != null);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + invPrivate.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);
            System.Diagnostics.Debug.Assert(invPrivate != null);

            Locker.Hold();

            System.Diagnostics.Debug.Assert(Renderers.Contains(invPrivate));

            if (i < TotalSlotCount)
            {
                Slot slot = Slots[i];
                System.Diagnostics.Debug.Assert(slot != null);

                slot.LeftClick(cursor);

                Update();
            }
            else
            {
                invPrivate.RightClick(world, player, i, cursor);
            }

            Locker.Release();
        }

        private void QuickMoveFromLeft(Slot slot)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(slot != null);

            if (slot.Empty)
            {
                return;
            }

            int j = -1;

            Slot slotInside;
            for (int i = 0; i < TotalSlotCount; ++i)
            {
                System.Diagnostics.Debug.Assert(!slot.Empty);

                slotInside = Slots[i];
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
                slotInside = Slots[j];
                System.Diagnostics.Debug.Assert(slotInside != null);
                System.Diagnostics.Debug.Assert(slotInside.Empty);

                slotInside.Move(slot);
            }
        }

        internal virtual void QuickMove(
            World world, Player player,
            PlayerInventory invPrivate, int i)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(player != null);

            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + invPrivate.PrimarySlotCount);

            Locker.Hold();

            if (i < TotalSlotCount)
            {
                Slot slot = Slots[i];
                System.Diagnostics.Debug.Assert(slot != null);

                invPrivate.QuickMoveFromRightInPrimary(slot);
            }
            else
            {
                int j = i - TotalSlotCount;
                System.Diagnostics.Debug.Assert(j >= 0);
                Slot slot = invPrivate.GetPrimarySlot(j);
                System.Diagnostics.Debug.Assert(slot != null);

                QuickMoveFromLeft(slot);
            }

            Update();

            Locker.Release();
        }

        public override void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Renderers.Empty);

            // Release resources.
            Locker.Dispose();
            Renderers.Dispose();

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

    public sealed class Chest : PublicInventory
    {
        private bool _disposed = false;

        protected override string Title => "Chest";

        public Chest(int line) : base(3 * SlotsPerLine) { }

        ~Chest() => System.Diagnostics.Debug.Assert(false);

        public override void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.
            

            // Finish.
            base.Dispose();
            _disposed = true;
        }
    }

    /*public abstract class ItemInterfaceInventory : PublicInventory
    {
        protected abstract class ClickContext
        {
            private readonly PrivateInventory Inventory;

            internal ClickContext(PrivateInventory inv)
            {
                System.Diagnostics.Debug.Assert(inv != null);
                Inventory = inv;
            }

            public bool Give(ItemType item, int count)
            {
                System.Diagnostics.Debug.Assert(count >= 0);

                System.Diagnostics.Debug.Assert(Inventory != null);
                return Inventory.GiveFromLeftInMain(item, count);
            }
        }

        protected sealed class LeftClickContext : ClickContext
        {
            internal LeftClickContext(PrivateInventory inv) : base(inv)
            {
                System.Diagnostics.Debug.Assert(inv != null);
            }
        }

        protected sealed class RightClickContext : ClickContext
        {
            internal RightClickContext(PrivateInventory inv) : base(inv)
            {
                System.Diagnostics.Debug.Assert(inv != null);
            }
        }

        protected sealed class MiddleClickContext : ClickContext
        {
            internal MiddleClickContext(PrivateInventory inv) : base(inv)
            {
                System.Diagnostics.Debug.Assert(inv != null);
            }
        }

        private const int MaxLineCount = 6;

        private bool _disposed = false;


        public ItemInterfaceInventory(int line) : base(line * SlotsPerLine)
        {
            System.Diagnostics.Debug.Assert(line > 0);
            System.Diagnostics.Debug.Assert(line <= MaxLineCount);
        }

        ~ItemInterfaceInventory() => System.Diagnostics.Debug.Assert(false);

        */
    /*internal override void TakeAll(
            PrivateInventory invPrivate, int i, ref ItemSlot cursor, 
            WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + invPrivate.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(cursor == null);
            System.Diagnostics.Debug.Assert(renderer != null);

            Locker.Hold();
                
            LeftClickContext ctx = new(invPrivate);
            LeftClick(ctx, i);

            Refresh(invPrivate, renderer);

            Locker.Release();
        }

        internal override void PutAll(
            PrivateInventory invPrivate, int i, ref ItemSlot cursor,  
            WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + invPrivate.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);
            System.Diagnostics.Debug.Assert(renderer != null);

            Locker.Hold();

            LeftClickContext ctx = new(invPrivate);
            LeftClick(ctx, i);

            Refresh(invPrivate, renderer);

            Locker.Release();
        }

        internal override void TakeHalf(
            PrivateInventory invPrivate, int i, ref ItemSlot cursor, 
            WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + invPrivate.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(cursor == null);
            System.Diagnostics.Debug.Assert(renderer != null);

            Locker.Hold();

            RightClickContext ctx = new(invPrivate);
            RightClick(ctx, i);

            Refresh(invPrivate, renderer);

            Locker.Release();
        }

        internal override void PutOne(
            PrivateInventory invPrivate, int i, ref ItemSlot cursor, 
            WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + invPrivate.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);
            System.Diagnostics.Debug.Assert(renderer != null);

            Locker.Hold();
            
            RightClickContext ctx = new(invPrivate);
            RightClick(ctx, i);

            Refresh(invPrivate, renderer);

            Locker.Release();
        }*//*

        protected abstract void LeftClick(LeftClickContext ctx, int i);

        protected abstract void RightClick(RightClickContext ctx, int i);

        public override void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.

            // Finish.
            base.Dispose();
            _disposed = true;
        }
    }*/

}

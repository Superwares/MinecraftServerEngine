﻿using Common;
using Containers;
using Sync;



namespace MinecraftServerEngine
{
    using Inventories;
    using Items;
    using Renderers;
    using Entities;
    using Protocols;

    internal sealed class Window : System.IDisposable
    {
        private bool _disposed = false;

        private readonly Locker _Locker = new();  // Disposable

        private readonly WindowRenderer _Renderer;
        private readonly InventorySlot _Cursor = new();


        private SharedInventory _sharedInventory = null;

        public Window(
            ConcurrentQueue<ClientboundPlayingPacket> outPackets,
            PlayerInventory invPlayer)
        {
            System.Diagnostics.Debug.Assert(outPackets != null);
            System.Diagnostics.Debug.Assert(invPlayer != null);

            _Renderer = new WindowRenderer(outPackets, invPlayer, _Cursor);
        }

        ~Window()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        internal bool Open(
            UserId userId,
            ConcurrentQueue<ClientboundPlayingPacket> outPackets,
            PlayerInventory playerInventory, SharedInventory sharedInventory)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(outPackets != null);
            System.Diagnostics.Debug.Assert(playerInventory != null);
            System.Diagnostics.Debug.Assert(sharedInventory != null);

            _Locker.Hold();
            sharedInventory.Locker.Hold();

            try
            {
                if (_sharedInventory != null)
                {
                    return false;
                }


                sharedInventory.Open(userId, playerInventory, outPackets);
                _Renderer.Open(sharedInventory, playerInventory, _Cursor);

                _sharedInventory = sharedInventory;

                return true;
            }
            finally
            {
                sharedInventory.Locker.Release();
                _Locker.Release();
            }
        }

        internal void Handle(
            UserId userId,
            World world, AbstractPlayer player,
            PlayerInventory playerInventory,
            int mode, int button, int i)
        {
            System.Diagnostics.Debug.Assert(userId != UserId.Null);
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(player != null);
            System.Diagnostics.Debug.Assert(playerInventory != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_Renderer != null);
            System.Diagnostics.Debug.Assert(_Cursor != null);

        /*    if (_sharedInventory != null)
            {
                System.Diagnostics.Debug.Assert(_sharedInventory.Locker != null);
                _sharedInventory.Locker.Hold();
            }*/

            ItemStack droppedItemStack = null;

            try
            {
                switch (mode)
                {
                    default:
                        throw new UnexpectedValueException($"Invalid mode number: {mode}");
                    case 0:
                        if (i < 0)
                        {
                            if (i == -999)
                            {
                                switch (button)
                                {
                                    default:
                                        throw new UnexpectedValueException($"Invalid button number: {button}, in mode {mode}");
                                    case 0:
                                        droppedItemStack = _Cursor.DropFull();
                                        break;
                                    case 1:
                                        droppedItemStack = _Cursor.DropSingle();
                                        break;

                                }
                            }

                            break;
                        }
                        switch (button)
                        {
                            default:
                                throw new UnexpectedValueException($"Invalid button number: {button}, in mode {mode}");
                            case 0:
                                if (_sharedInventory == null)
                                {
                                    playerInventory.LeftClick(i, _Cursor);
                                }
                                else
                                {
                                    System.Diagnostics.Debug.Assert(world != null);
                                    System.Diagnostics.Debug.Assert(player != null);
                                    System.Diagnostics.Debug.Assert(_Cursor != null);
                                    _sharedInventory.LeftClick(world, userId, player, i, _Cursor);
                                }
                                break;
                            case 1:
                                if (_sharedInventory == null)
                                {
                                    playerInventory.RightClick(i, _Cursor);
                                }
                                else
                                {
                                    System.Diagnostics.Debug.Assert(world != null);
                                    System.Diagnostics.Debug.Assert(player != null);
                                    System.Diagnostics.Debug.Assert(_Cursor != null);
                                    _sharedInventory.RightClick(world, userId, player, i, _Cursor);
                                }
                                break;
                        }
                        break;
                    case 1:
                        if (i < 0)
                        {
                            break;
                        }
                        switch (button)
                        {
                            default:
                                throw new UnexpectedValueException($"Invalid button number: {button}, in mode {mode}");
                            case 0:
                                if (_sharedInventory == null)
                                {
                                    playerInventory.QuickMove(i);
                                }
                                else
                                {
                                    _sharedInventory.QuickMove(userId, playerInventory, i);
                                }
                                break;
                            case 1:
                                if (_sharedInventory == null)
                                {
                                    playerInventory.QuickMove(i);
                                }
                                else
                                {
                                    _sharedInventory.QuickMove(userId, playerInventory, i);
                                }
                                break;
                        }
                        break;
                    case 2:
                        if (_sharedInventory == null)
                        {
                            playerInventory.SwapItemsWithHotbarSlot(i, button);
                        }
                        else
                        {
                            _sharedInventory.SwapItemsWithHotbarSlot(userId, playerInventory, i, button);
                        }
                        break;
                    case 3:
                        break;
                    case 4:
                        if (i < 0)
                        {
                            break;
                        }
                        switch (button)
                        {
                            default:
                                throw new UnexpectedValueException($"Invalid button number: {button}, in mode {mode}");
                            case 0:
                                if (_sharedInventory == null)
                                {
                                    droppedItemStack = playerInventory.DropSingle(i);
                                }
                                else
                                {
                                    droppedItemStack = _sharedInventory.DropSingle(userId, playerInventory, i);
                                }
                                break;
                            case 1:
                                if (_sharedInventory == null)
                                {
                                    droppedItemStack = playerInventory.DropFull(i);
                                }
                                else
                                {
                                    droppedItemStack = _sharedInventory.DropFull(userId, playerInventory, i);
                                }
                                break;
                        }
                        break;
                    case 5:
                        break;
                    case 6:
                        break;
                }

                if (droppedItemStack != null)
                {
                    System.Diagnostics.Debug.Assert(world != null);
                    world.SpawnObject(new ItemEntity(droppedItemStack, player.Position));
                }

                player.UpdateEquipmentsData();

                _Renderer.Update(_sharedInventory, playerInventory, _Cursor);

                //{
                //    if (_sharedInventory == null)
                //    {
                //        playerInventory.Print();
                //    }

                //    MyConsole.Debug($"Cursor: {_Cursor}");
                //}
            }
            finally
            {
                //if (_sharedInventory != null)
                
                //    System.Diagnostics.Debug.Assert(_sharedInventory.Locker != null);
                //    _sharedInventory.Locker.Release();
                //}

                
            }

        }

        internal void Handle(
            UserId id,
            World world, AbstractPlayer player,
            PlayerInventory invPlayer,
            int idWindow, int mode, int button, int i)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(id != UserId.Null);
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(invPlayer != null);

            System.Diagnostics.Debug.Assert(_Locker != null);
            _Locker.Hold();

            try
            {
                if (idWindow < 0 || idWindow > 1)
                {
                    throw new UnexpectedClientBehaviorExecption("Invalid window id: <0 || >1");
                }

                //if (i < 0)
                //{
                //    throw new UnexpectedClientBehaviorExecption("Negative slot index");
                //}

                if (_sharedInventory == null)
                {
                    if (i >= invPlayer.GetTotalSlotCount())
                    {
                        throw new UnexpectedClientBehaviorExecption("Slot index is out of the valid range for player inventory");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.Assert(_sharedInventory != null);

                    if (i >= _sharedInventory.GetTotalSlotCount() + PlayerInventory.PrimarySlotCount)
                    {
                        throw new UnexpectedClientBehaviorExecption("Slot index is out of the valid range for public and player primary inventory");
                    }
                }

                if (_sharedInventory == null)
                {
                    if (idWindow == 1)
                    {
                        throw new UnexpectedClientBehaviorExecption("Invalid window id: ==1");
                    }

                    System.Diagnostics.Debug.Assert(idWindow == 0);
                }
                else
                {

                    if (idWindow == 0)
                    {
                        throw new UnexpectedClientBehaviorExecption("Invalid window Id");
                    }

                    System.Diagnostics.Debug.Assert(idWindow == 1);
                }

                Handle(id, world, player, invPlayer, mode, button, i);

            }
            finally
            {
                System.Diagnostics.Debug.Assert(_Locker != null);
                _Locker.Release();
            }
        }

        internal ItemStack HandleMainHandSlot(PlayerInventory playerInventory)
        {
            System.Diagnostics.Debug.Assert(playerInventory != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_Locker != null);
            _Locker.Hold();

            try
            {
                (ItemStack itemStack, bool changed) = playerInventory.HandleMainHandSlot();

                if (changed == true)
                {
                    _Renderer.HandleMainHandSlot(playerInventory);
                }

                return itemStack;
            }
            finally
            {
                System.Diagnostics.Debug.Assert(_Locker != null);
                _Locker.Release();
            }
        }

        internal void SetHelmet(PlayerInventory playerInventory, ItemStack itemStack)
        {
            System.Diagnostics.Debug.Assert(playerInventory != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_Locker != null);
            _Locker.Hold();

            try
            {
                playerInventory.SetHelmet(itemStack);
            }
            finally
            {
                System.Diagnostics.Debug.Assert(_Renderer != null);
                _Renderer.Update(_sharedInventory, playerInventory, _Cursor);

                System.Diagnostics.Debug.Assert(_Locker != null);
                _Locker.Release();
            }
        }

        internal void UpdateMainHandSlot(PlayerInventory playerInventory)
        {
            System.Diagnostics.Debug.Assert(playerInventory != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_Locker != null);
            _Locker.Hold();

            try
            {
                _Renderer.HandleMainHandSlot(playerInventory);
            }
            finally
            {
                System.Diagnostics.Debug.Assert(_Locker != null);
                _Locker.Release();
            }
        }

        internal bool GiveItemStacks(PlayerInventory playerInventory, IReadOnlyItem item, int count)
        {
            System.Diagnostics.Debug.Assert(playerInventory != null);
            System.Diagnostics.Debug.Assert(item != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (count == 0)
            {
                return true;
            }

            System.Diagnostics.Debug.Assert(_Locker != null);
            _Locker.Hold();

            try
            {
                return playerInventory.GiveItemStacks(item, count);
            }
            finally
            {
                System.Diagnostics.Debug.Assert(_Renderer != null);
                _Renderer.Update(_sharedInventory, playerInventory, _Cursor);

                System.Diagnostics.Debug.Assert(_Locker != null);
                _Locker.Release();
            }
        }

        internal bool GiveItemStack(PlayerInventory playerInventory, ref ItemStack itemStack)
        {
            System.Diagnostics.Debug.Assert(playerInventory != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (itemStack == null)
            {
                return true;
            }

            System.Diagnostics.Debug.Assert(_Locker != null);
            _Locker.Hold();

            try
            {
                return playerInventory.GiveItemStack(ref itemStack);
            }
            finally
            {
                System.Diagnostics.Debug.Assert(_Renderer != null);
                _Renderer.Update(_sharedInventory, playerInventory, _Cursor);

                System.Diagnostics.Debug.Assert(_Locker != null);
                _Locker.Release();
            }
        }

        internal ItemStack[] TakeItemStacks(
            PlayerInventory playerInventory,
            IReadOnlyItem item, int count)
        {
            System.Diagnostics.Debug.Assert(playerInventory != null);
            System.Diagnostics.Debug.Assert(item != null);
            System.Diagnostics.Debug.Assert(count >= 0);

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (count == 0)
            {
                return [];
            }

            System.Diagnostics.Debug.Assert(_Locker != null);
            _Locker.Hold();

            try
            {
                return playerInventory.TakeItemStacks(item, count);
            }
            finally
            {
                System.Diagnostics.Debug.Assert(_Renderer != null);
                _Renderer.Update(_sharedInventory, playerInventory, _Cursor);

                System.Diagnostics.Debug.Assert(_Locker != null);
                _Locker.Release();
            }
        }

        internal ItemStack[] GiveAndTakeItemStacks(
            PlayerInventory playerInventory,
            IReadOnlyItem giveItem, int giveCount,
            IReadOnlyItem takeItem, int takeCount)
        {
            System.Diagnostics.Debug.Assert(playerInventory != null);

            System.Diagnostics.Debug.Assert(giveItem != null);
            System.Diagnostics.Debug.Assert(giveCount >= 0);
            System.Diagnostics.Debug.Assert(takeItem != null);
            System.Diagnostics.Debug.Assert(takeCount >= 0);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_Locker != null);
            _Locker.Hold();

            try
            {
                return playerInventory.GiveAndTakeItemStacks(
                    giveItem, giveCount, 
                    takeItem, takeCount
                    );
            }
            finally
            {
                System.Diagnostics.Debug.Assert(_Renderer != null);
                _Renderer.Update(_sharedInventory, playerInventory, _Cursor);

                System.Diagnostics.Debug.Assert(_Locker != null);
                _Locker.Release();
            }
        }

        internal void FlushItems(PlayerInventory playerInventory)
        {
            System.Diagnostics.Debug.Assert(playerInventory != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_Locker != null);
            _Locker.Hold();

            try
            {
                System.Diagnostics.Debug.Assert(playerInventory != null);
                playerInventory.FlushItems();
            }
            finally
            {
                System.Diagnostics.Debug.Assert(_Renderer != null);
                _Renderer.Update(_sharedInventory, playerInventory, _Cursor);

                System.Diagnostics.Debug.Assert(_Locker != null);
                _Locker.Release();
            }
        }

        internal void Reset(
            World world, AbstractPlayer player,
            int idWindow, PlayerInventory playerInventory)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(playerInventory != null);

            System.Diagnostics.Debug.Assert(_Locker != null);
            _Locker.Hold();

            try
            {
                if (idWindow < 0 || idWindow > 1)
                {
                    throw new UnexpectedValueException($"Invalid window Id: {idWindow}<0 || {idWindow}>1");
                }

                if (_sharedInventory != null)
                {
                    _sharedInventory.Close(playerInventory);
                    _sharedInventory = null;
                }

                ItemStack droppedItemStack = _Cursor.DropFull();

                if (droppedItemStack != null)
                {
                    System.Diagnostics.Debug.Assert(world != null);
                    System.Diagnostics.Debug.Assert(droppedItemStack != null);
                    player.OnItemDrop(world, droppedItemStack);
                }

            }
            finally
            {
                System.Diagnostics.Debug.Assert(_Renderer != null);
                System.Diagnostics.Debug.Assert(_Cursor != null);
                _Renderer.Reset(playerInventory, _Cursor);

                System.Diagnostics.Debug.Assert(_Locker != null);
                _Locker.Release();
            }
        }


        internal ItemStack Close(PlayerInventory playerInventory)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(playerInventory != null);

            

            System.Diagnostics.Debug.Assert(_Locker != null);
            _Locker.Hold();
     
            int windowId = _sharedInventory != null ? 1 : 0;

            try
            {
                if (_sharedInventory != null)
                {
                    System.Diagnostics.Debug.Assert(playerInventory != null);
                    _sharedInventory.Close(playerInventory);
                    _sharedInventory = null;
                }

                System.Diagnostics.Debug.Assert(_Cursor != null);
                return _Cursor.DropFull();
            }
            finally
            {

                System.Diagnostics.Debug.Assert(_Renderer != null);
                System.Diagnostics.Debug.Assert(playerInventory != null);
                System.Diagnostics.Debug.Assert(_Cursor != null);
                _Renderer.Reset2(windowId, playerInventory, _Cursor);
                
                System.Diagnostics.Debug.Assert(_Locker != null);
                _Locker.Release();
            }
        }

        public void Flush(World world, PlayerInventory invPlayer)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(invPlayer != null);

            if (_sharedInventory != null)
            {
                _sharedInventory.Close(invPlayer);
                _sharedInventory = null;
            }

            if (_Cursor.Empty == false)
            {
                // TODO: Drop Item.

                throw new System.NotImplementedException();
            }

        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed == false)
            {
                System.Diagnostics.Debug.Assert(_sharedInventory == null);
                System.Diagnostics.Debug.Assert(_Cursor.Empty);

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing == true)
                {
                    // Dispose managed resources.
                    _Locker.Dispose();
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

        }

    }
}

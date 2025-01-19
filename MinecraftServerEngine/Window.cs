using Common;
using Containers;
using Sync;

namespace MinecraftServerEngine
{
    internal sealed class Window : System.IDisposable
    {
        private bool _disposed = false;

        private readonly Locker Locker = new();  // Disposable

        private readonly WindowRenderer Renderer;
        private readonly InventorySlot Cursor = new();

        private SharedInventory _sharedInventory = null;


        public Window(
            ConcurrentQueue<ClientboundPlayingPacket> outPackets,
            PlayerInventory invPlayer)
        {
            System.Diagnostics.Debug.Assert(outPackets != null);
            System.Diagnostics.Debug.Assert(invPlayer != null);

            Renderer = new WindowRenderer(outPackets, invPlayer, Cursor);
        }

        ~Window()
        {
            System.Diagnostics.Debug.Assert(false);

            //Dispose(false);
        }

        internal bool Open(
            UserId userId,
            ConcurrentQueue<ClientboundPlayingPacket> outPackets,
            PlayerInventory playerInventory, SharedInventory sharedInventory)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(outPackets != null);
            System.Diagnostics.Debug.Assert(playerInventory != null);
            System.Diagnostics.Debug.Assert(sharedInventory != null);

            Locker.Hold();
            sharedInventory.Locker.Hold();

            try
            {
                if (_sharedInventory != null)
                {
                    return false;
                }


                sharedInventory.Open(userId, playerInventory, outPackets);
                Renderer.Open(sharedInventory, playerInventory, Cursor);

                _sharedInventory = sharedInventory;

                return true;
            }
            finally
            {
                sharedInventory.Locker.Release();
                Locker.Release();
            }
        }

        internal void Handle(
            UserId id,
            World world, AbstractPlayer player,
            PlayerInventory playerInventory,
            int mode, int button, int i)
        {
            System.Diagnostics.Debug.Assert(id != UserId.Null);
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(player != null);
            System.Diagnostics.Debug.Assert(playerInventory != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Renderer != null);
            System.Diagnostics.Debug.Assert(Cursor != null);

            if (_sharedInventory != null)
            {
                _sharedInventory.Locker.Hold();
            }

            try
            {
                switch (mode)
                {
                    default:
                        throw new UnexpectedValueException($"Invalid mode number: {mode}");
                    case 0:
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
                                    playerInventory.LeftClick(i, Cursor);
                                }
                                else
                                {
                                    _sharedInventory.LeftClick(id, playerInventory, i, Cursor);
                                }
                                break;
                            case 1:
                                if (_sharedInventory == null)
                                {
                                    playerInventory.RightClick(i, Cursor);
                                }
                                else
                                {
                                    _sharedInventory.RightClick(id, playerInventory, i, Cursor);
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
                                    _sharedInventory.QuickMove(id, playerInventory, i);
                                }
                                break;
                            case 1:
                                if (_sharedInventory == null)
                                {
                                    playerInventory.QuickMove(i);
                                }
                                else
                                {
                                    _sharedInventory.QuickMove(id, playerInventory, i);
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
                            _sharedInventory.SwapItemsWithHotbarSlot(id, playerInventory, i, button);
                        }
                        break;
                    case 3:
                        break;
                    case 4:
                        break;
                    case 5:
                        break;
                    case 6:
                        break;
                }

                player.UpdateEntityEquipmentsData(playerInventory.GetEquipmentsData());

                Renderer.Update(_sharedInventory, playerInventory, Cursor);

                {
                    if (_sharedInventory == null)
                    {
                        playerInventory.Print();
                    }

                    MyConsole.Debug($"Cursor: {Cursor}");
                }
            }
            finally
            {
                if (_sharedInventory != null)
                {
                    _sharedInventory.Locker.Release();
                }
            }

        }

        internal void Handle(
            UserId id,
            World world, AbstractPlayer player,
            PlayerInventory invPlayer,
            int idWindow, int mode, int button, int i)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(id != UserId.Null);
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(invPlayer != null);

            Locker.Hold();

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
                Locker.Release();
            }
        }


        internal bool GiveItem(PlayerInventory playerInventory, ItemStack stack)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            if (stack == null)
            {
                return true;
            }

            Locker.Hold();
            if (_sharedInventory != null)
            {
                _sharedInventory.Locker.Hold();
            }

            try
            {
                bool f = playerInventory.GiveItem(stack);

                Renderer.Update(_sharedInventory, playerInventory, Cursor);

                return f;
            }
            finally
            {
                if (_sharedInventory != null)
                {
                    _sharedInventory.Locker.Release();
                }
                Locker.Release();
            }
        }

        internal void Reset(
            ConcurrentQueue<ClientboundPlayingPacket> outPackets,
            World world, AbstractPlayer player,
            int idWindow, PlayerInventory invPrivate)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(outPackets != null);
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(invPrivate != null);

            SharedInventory sharedInventory = _sharedInventory;

            Locker.Hold();
            if (sharedInventory != null)
            {
                sharedInventory.Locker.Hold();
            }

            try
            {
                if (idWindow < 0 || idWindow > 1)
                {
                    throw new UnexpectedValueException($"Invalid window Id: {idWindow}<0 || {idWindow}>1");
                }

                if (_sharedInventory == null)
                {

                    if (idWindow == 0)
                    {
                    }
                    else if (idWindow == 1)
                    {
                        throw new UnexpectedValueException($"Invalid window Id: {idWindow}==1");
                    }

                }
                else
                {
                    if (idWindow == 0)
                    {
                        throw new UnexpectedValueException($"Invalid window Id: {idWindow}==0");
                    }

                    System.Diagnostics.Debug.Assert(idWindow == 1);

                    _sharedInventory.Close(invPrivate);
                }

                ItemStack dropItem = Cursor.Drop();

                if (dropItem != null)
                {
                    world.SpawnObject(new ItemEntity(dropItem, player.GetEyeOrigin()));
                }

                Renderer.Reset(invPrivate, Cursor);
                _sharedInventory = null;

            }
            finally
            {
                if (sharedInventory != null)
                {
                    sharedInventory.Locker.Release();
                }
                Locker.Release();
            }
        }


        public void Flush(World world, PlayerInventory invPlayer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(invPlayer != null);

            if (_sharedInventory != null)
            {
                _sharedInventory.Close(invPlayer);
                _sharedInventory = null;
            }

            if (!Cursor.Empty)
            {
                // TODO: Drop Item.

                throw new System.NotImplementedException();
            }

        }

        public void Dispose()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // Assertion.
            System.Diagnostics.Debug.Assert(_sharedInventory == null);
            System.Diagnostics.Debug.Assert(Cursor.Empty);

            // Release Resources.
            Locker.Dispose();

            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }
}

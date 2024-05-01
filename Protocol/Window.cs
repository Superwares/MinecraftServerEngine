
using Containers;
using System;
using System.Reflection;

namespace Protocol
{
    internal sealed class Window : System.IDisposable
    {
        private bool _disposed = false;

        private bool _ambiguous = false;

        /**
         *  -1: Window is closed.
         *   0: Window is opened with only self inventory.
         * > 0: Window is opened with self and public inventory.
         */
        private int _windowId = -1;
        private int _id = -1;

        private PublicInventory? _publicInventory = null;

        private Item? _itemCursor = null;

        private bool _startDrag = false;
        private Queue<int> _dragedIndices = new();  // Disposable

        public Window(
            Queue<ClientboundPlayingPacket> outPackets,
            SelfInventory selfInventory)
        {
            _windowId = 0;

            {
                int i = 0, n = selfInventory.TotalSlotCount;
                var arr = new SlotData[n];

                foreach (Item? item in selfInventory.Items)
                {
                    if (item == null)
                    {
                        arr[i++] = new();
                        continue;
                    }

                    arr[i++] = item.ConventToPacketFormat();
                }

                System.Diagnostics.Debug.Assert(_windowId >= byte.MinValue);
                System.Diagnostics.Debug.Assert(_windowId <= byte.MaxValue);
                outPackets.Enqueue(new SetWindowItemsPacket((byte)_windowId, arr));

                System.Diagnostics.Debug.Assert(_itemCursor == null);
                outPackets.Enqueue(new SetSlotPacket(-1, 0, new()));

                System.Diagnostics.Debug.Assert(i == n);
            }

        }

        ~Window() => System.Diagnostics.Debug.Assert(false);

        /*public int GetWindowId()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_windowId >= 0);
            System.Diagnostics.Debug.Assert(_windowId > 0 ?
                _publicInventory != null : _publicInventory == null);

            return _windowId;
        }*/

        public void OpenWindowWithPublicInventory(
            Queue<ClientboundPlayingPacket> outPackets,
            SelfInventory selfInventory,
            PublicInventory publicInventory)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _ambiguous = true;

            System.Diagnostics.Debug.Assert(_windowId == 0);
            System.Diagnostics.Debug.Assert(_id == -1);
            System.Diagnostics.Debug.Assert(_publicInventory == null);

            _windowId = (new System.Random().Next() % 100) + 1;

            _id = publicInventory.Open(_windowId, outPackets, selfInventory);
            if (_itemCursor != null)
            {
                outPackets.Enqueue(new SetSlotPacket(-1, 0, _itemCursor.ConventToPacketFormat()));
            }

            _publicInventory = publicInventory;
        }

        public void ResetWindow(
            int windowId, Queue<ClientboundPlayingPacket> outPackets)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (windowId != _windowId)
            {
                if (_ambiguous)
                {
                    _ambiguous = false;
                    return;
                }
                else
                {
                    throw new UnexpectedValueException("ClickWindowPacket.WindowId");
                }
            }

            _ambiguous = false;

            if (_windowId == 0)
            {
                System.Diagnostics.Debug.Assert(_id == -1);
                System.Diagnostics.Debug.Assert(_publicInventory == null);

            }
            else
            {
                System.Diagnostics.Debug.Assert(_windowId > 0);
                System.Diagnostics.Debug.Assert(_id >= 0);
                System.Diagnostics.Debug.Assert(_publicInventory != null);

                _publicInventory.Close(_id, _windowId);

                _windowId = 0;
                _id = -1;
                _publicInventory = null;
            }

            /*if (_itemCursor != null)
            {
                // TODO: Drop item if _iremCursor is not null.
                _itemCursor = null;
            }*/

            System.Diagnostics.Debug.Assert(_itemCursor == null);
        }

        internal void ResetWindowForcibly(
            SelfInventory selfInventory, Queue<ClientboundPlayingPacket> outPackets)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _ambiguous = true;

            _windowId = 0;
            _id = -1;
            _publicInventory = null;

            {
                int count = selfInventory.TotalSlotCount;
                int i = 0;
                var arr = new SlotData[count];

                foreach (Item? item in selfInventory.Items)
                {
                    if (item == null)
                    {
                        arr[i++] = new();
                        continue;
                    }

                    arr[i++] = item.ConventToPacketFormat();
                }

                System.Diagnostics.Debug.Assert(_windowId == 0);
                System.Diagnostics.Debug.Assert(_windowId >= byte.MinValue);
                System.Diagnostics.Debug.Assert(_windowId <= byte.MaxValue);
                outPackets.Enqueue(new SetWindowItemsPacket((byte)_windowId, arr));

                System.Diagnostics.Debug.Assert(i == count);
            }

            /*if (_itemCursor != null)
            {
                // TODO: Drop item if _iremCursor is not null.
                _itemCursor = null;
            }*/

            System.Diagnostics.Debug.Assert(_itemCursor == null);
        }

        private void ClickLeftMouseButton(
            SelfInventory selfInventory, int index, SlotData slotData)
        {
            if (index >= selfInventory.TotalSlotCount)
            {
                throw new UnexpectedValueException("ClickWindowPacket.SlotNumber");
            }

            bool f;

            if (_itemCursor == null)
            {
                (f, _itemCursor) = selfInventory.TakeItem(index, slotData);
            }
            else
            {
                (f, _itemCursor) = selfInventory.PutItem(index, _itemCursor, slotData);
            }

            if (!f)
            {
                /*SlotData slotDataInCursor = _itemCursor.ConventToPacketFormat();

                outPackets.Enqueue(new SetSlotPacket(-1, 0, slotDataInCursor));*/

                throw new UnexpectedValueException("ClickWindowPacket.SLOT_DATA");
            }
        }

        private void ClickLeftMouseButtonWithPublicInventory(
            SelfInventory selfInventory, int index, SlotData slotData,
            Queue<ClientboundPlayingPacket> outPackets)
        {
            System.Diagnostics.Debug.Assert(_windowId > 0);
            System.Diagnostics.Debug.Assert(_publicInventory != null);

            bool f;

            if (_itemCursor == null)
            {
                if (index >= 0 && index < _publicInventory.TotalSlotCount)
                {
                    (f, _itemCursor) = _publicInventory.TakeItem(index, slotData);
                }
                else if (
                    index >= _publicInventory.TotalSlotCount &&
                    index < _publicInventory.TotalSlotCount + selfInventory.PrimarySlotCount)
                {
                    int j = index + 9 - _publicInventory.TotalSlotCount;
                    (f, _itemCursor) = selfInventory.TakeItem(j, slotData);
                }
                else
                {
                    throw new UnexpectedValueException("ClickWindowPacket.SlotNumber");
                }
            }
            else
            {
                if (index >= 0 && index < _publicInventory.TotalSlotCount)
                {
                    (f, _itemCursor) = _publicInventory.PutItem(index, _itemCursor, slotData);
                }
                else if (
                    index >= _publicInventory.TotalSlotCount &&
                    index < _publicInventory.TotalSlotCount + selfInventory.PrimarySlotCount)
                {
                    int j = index + 9 - _publicInventory.TotalSlotCount;
                    (f, _itemCursor) = selfInventory.PutItem(j, _itemCursor, slotData);
                }
                else
                {
                    throw new UnexpectedValueException("ClickWindowPacket.SlotNumber");
                }
            }


            if (!f)
            {
                if (_itemCursor == null)
                {
                    if (index >= 0 && index < _publicInventory.TotalSlotCount)
                    {
                        outPackets.Enqueue(new SetSlotPacket(-1, 0, new()));
                    }
                    else if (
                        index >= _publicInventory.TotalSlotCount &&
                        index < _publicInventory.TotalSlotCount + selfInventory.PrimarySlotCount)
                    {
                        throw new UnexpectedValueException("ClickWindowPacket.SLOT_DATA");
                    }
                }
                else
                {
                    if (index >= 0 && index < _publicInventory.TotalSlotCount)
                    {
                        outPackets.Enqueue(new SetSlotPacket(-1, 0, _itemCursor.ConventToPacketFormat()));
                    }
                    else if (
                        index >= _publicInventory.TotalSlotCount &&
                        index < _publicInventory.TotalSlotCount + selfInventory.PrimarySlotCount)
                    {
                        throw new UnexpectedValueException("ClickWindowPacket.SLOT_DATA");
                    }
                }
                
                
            }

        }


        public void Handle(
            SelfInventory selfInventory,
            int windowId, int mode, int button, int index, SlotData slotData,
            Queue<ClientboundPlayingPacket> outPackets)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_windowId >= 0);

            if (windowId != _windowId)
            {
                if (_ambiguous)
                {
                    return;
                }
                else
                {
                    throw new UnexpectedValueException("ClickWindowPacket.WindowId");
                }
            }

            _ambiguous = false;

            if (index < 0)
            {
                throw new UnexpectedValueException("ClickWindowPacket.SlotNumber");
            }

            switch (mode)
            {
                default:
                    throw new UnexpectedValueException("ClickWindowPacket.ModeNumber");
                case 0:
                    switch (button)
                    {
                        default:
                            throw new UnexpectedValueException("ClickWindowPacket.ButtonNumber");
                        case 0:
                            if (_windowId == 0)
                            {
                                ClickLeftMouseButton(selfInventory, index, slotData);
                            }
                            else
                            {
                                ClickLeftMouseButtonWithPublicInventory(
                                    selfInventory, index, slotData,
                                    outPackets);
                            }
                            break;
                    }
                    break;
            }

            if (_windowId == 0)
            {
                selfInventory.Print();
            }
            else
            {
                System.Diagnostics.Debug.Assert(_publicInventory != null);
                _publicInventory.Print();
                selfInventory.Print();
            }

        }
        
        public void Flush()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (_windowId > 0)
            {
                System.Diagnostics.Debug.Assert(_windowId > 0);
                System.Diagnostics.Debug.Assert(_id >= 0);
                System.Diagnostics.Debug.Assert(_publicInventory != null);

                _publicInventory.Close(_id, _windowId);

                _windowId = 0;
                _id = -1;
                _publicInventory = null;
            }

            /*if (_itemCursor != null)
            {
                // TODO: Drop item if _iremCursor is not null.
                _itemCursor = null;
            }*/

            System.Diagnostics.Debug.Assert(_itemCursor == null);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            // Assertion.

            if (disposing == true)
            {
                // Release managed resources.

            }

            // Release unmanaged resources.

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        public void Close() => Dispose();

    }
}

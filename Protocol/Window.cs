using System;
using System.Diagnostics;
using Containers;

namespace Protocol
{
    internal sealed class Window : IDisposable
    {
        private bool _disposed = false;

        /**
         *  -1: Window is closed.
         *   0: Window is opened with only self inventory.
         * > 0: Window is opened with self and other inventory.
         */
        private int _windowId = -1;

        private PlayerInventory _inventorySelf;
        private PublicInventory? _inventoryOther = null;

        private Item? _itemCursor = null;

        private bool _startDrag = false;
        private Queue<SlotData> _slotDataQueue = new();  // Disposable

        public Window(
            Queue<ClientboundPlayingPacket> outPackets,
            PlayerInventory inventorySelf)
        {
            _windowId = 0;

            int i = 0, n = inventorySelf.Count;
            var arr = new SlotData[n];

            foreach (Item? item in inventorySelf.Items)
            {
                if (item == null)
                {
                    arr[i++] = new();
                    continue;
                }

                Debug.Assert(item.Id >= short.MinValue);
                Debug.Assert(item.Id <= short.MaxValue);
                Debug.Assert(item.Count >= byte.MinValue);
                Debug.Assert(item.Count <= byte.MaxValue);
                arr[i++] = new((short)item.Id, (byte)item.Count);
            }

            Debug.Assert(_windowId >= byte.MinValue);
            Debug.Assert(_windowId <= byte.MaxValue);
            outPackets.Enqueue(new SetWindowItemsPacket((byte)_windowId, arr));

            Debug.Assert(_itemCursor == null);
            outPackets.Enqueue(new SetSlotPacket(-1, 0, new()));

            Debug.Assert(i == n);

            _inventorySelf = inventorySelf;
        }

        ~Window() => Debug.Assert(false);

        public int GetWindowId()
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_windowId >= 0);
            Debug.Assert(_windowId > 0 ?
                _inventoryOther != null : _inventoryOther == null);

            return _windowId;
        }

        public void ReopenWindowWithOtherInventory(
            int idConn, Queue<ClientboundPlayingPacket> outPackets,
            PublicInventory inventoryOther)
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_windowId == 0);
            Debug.Assert(_inventoryOther == null);

            _windowId = (new Random().Next() % 100) + 1;  // TODO

            int n = inventoryOther.Count;
            Debug.Assert(n % 9 == 0);

            Debug.Assert(_windowId >= byte.MinValue);
            Debug.Assert(_windowId <= byte.MaxValue);
            Debug.Assert(n >= byte.MinValue);
            Debug.Assert(n <= byte.MaxValue);
            outPackets.Enqueue(new OpenWindowPacket(
                (byte)_windowId, inventoryOther.WindowType, "", (byte)n));

            lock (inventoryOther._SharedObject)
            {

                inventoryOther.AddRenderer(idConn, _windowId, outPackets);

                int i = 0;
                var arr = new SlotData[n];

                foreach (Item? item in inventoryOther.Items)
                {
                    if (item == null)
                    {
                        arr[i++] = new();
                        continue;
                    }

                    Debug.Assert(item.Id >= short.MinValue);
                    Debug.Assert(item.Id <= short.MaxValue);
                    Debug.Assert(item.Count >= byte.MinValue);
                    Debug.Assert(item.Count <= byte.MaxValue);
                    arr[i++] = new((short)item.Id, (byte)item.Count);
                }

                Debug.Assert(_windowId >= byte.MinValue);
                Debug.Assert(_windowId <= byte.MaxValue);
                outPackets.Enqueue(new SetWindowItemsPacket((byte)_windowId, arr));

                Debug.Assert(_itemCursor == null);
                outPackets.Enqueue(new SetSlotPacket(-1, 0, new()));

                Debug.Assert(i == n);

            }

            _inventoryOther = inventoryOther;
        }

        public void ResetWindow(
            int idConn, Queue<ClientboundPlayingPacket> outPackets)
        {
            Debug.Assert(!_disposed);

            if (_windowId == 0)
            {
                // TODO: if window closed that has zero id and
                // a item in cursor slot is not empty, the item must be droped.
                Debug.Assert(_itemCursor == null);
                return;
            }

            Debug.Assert(_inventoryOther != null);
            Debug.Assert(_windowId > 0);

            _windowId = 0;

            lock (_inventoryOther._SharedObject)
            {
                _inventoryOther.RemoveRenderer(idConn);
            }

            _inventoryOther = null;

            /*int i = 0, n = 9;
            var arr = new SlotData[n];*/

            /*foreach (Item? item in _inventorySelf.Items)
            {
                if (item == null)
                {
                    arr[i++] = new();
                    continue;
                }

                Debug.Assert(item.Id >= short.MinValue);
                Debug.Assert(item.Id <= short.MaxValue);
                Debug.Assert(item.Count >= byte.MinValue);
                Debug.Assert(item.Count <= byte.MaxValue);
                arr[i++] = new((short)item.Id, (byte)item.Count);

                if (i == n)
                    break;
            }*/

            /*Debug.Assert(_windowId >= byte.MinValue);
            Debug.Assert(_windowId <= byte.MaxValue);
            outPackets.Enqueue(new SetWindowItemsPacket((byte)_windowId, arr));*/

            {
                Item? item = _inventorySelf.OffhandItem;
                SlotData slotData;
                if (item == null)
                {
                    slotData = new();
                }
                else
                {
                    Debug.Assert(item.Id >= short.MinValue);
                    Debug.Assert(item.Id <= short.MaxValue);
                    Debug.Assert(item.Count >= byte.MinValue);
                    Debug.Assert(item.Count <= byte.MaxValue);
                    slotData = new((short)item.Id, (byte)item.Count);
                }
                outPackets.Enqueue(new SetSlotPacket((sbyte)_windowId, 45, slotData));
            }

            /*if (_itemCursor != null)
            {
                // TODO: Drop item if _iremCursor is not null.
                _itemCursor = null;
            }*/

            Debug.Assert(_itemCursor == null);
            outPackets.Enqueue(new SetSlotPacket(-1, 0, new()));
        }

        public void Handle(
            int windowId, int mode, int button, int index, SlotData slotData, 
            Queue<ClientboundPlayingPacket> outPackets)
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_windowId >= 0);

            if (windowId < 0)
                throw new UnexpectedValueException($"ClickWindowPacket.WindowId {windowId}");

            int totalSlotCount;

            if (_windowId == 0)
            {
                Debug.Assert(_inventoryOther == null);

                totalSlotCount = _inventorySelf.Count;
            }
            else
            {
                Debug.Assert(_inventoryOther != null);

                totalSlotCount = 36 + _inventoryOther.Count;
            }

            switch (mode)
            {
                default:
                    throw new UnexpectedValueException($"ClickWindowPacket.ModeNumber {mode}");
                case 0:
                    switch (button)
                    {
                        default:
                            throw new UnexpectedValueException($"ClickWindowPacket.ButtonNumber {button}");
                        case 0:
                            if (index < 0 || index >= totalSlotCount)
                                throw new UnexpectedValueException($"ClickWindowPacket.SlotNumber {index}");

                            if (_windowId == 0)
                            {
                                Debug.Assert(_inventoryOther == null);

                                _inventorySelf.ClickLeftMouseButton(index, ref _itemCursor);
                            }
                            else
                            {
                                Debug.Assert(_inventoryOther != null);

                                if (index > _inventoryOther.Count)
                                {
                                    _inventoryOther.ClickLeftMouseButton(index, ref _itemCursor);
                                }
                                else
                                {
                                    _inventorySelf.ClickLeftMouseButton(
                                        index - _inventoryOther.Count, 
                                        ref _itemCursor);
                                }
                            }
                            break;
                        case 1:
                            if (index < 0 || index >= totalSlotCount)
                                throw new UnexpectedValueException($"ClickWindowPacket.SlotNumber {index}");

                            if (_windowId == 0)
                            {
                                Debug.Assert(_inventoryOther == null);

                                _inventorySelf.ClickRightMouseButton(index, ref _itemCursor);
                            }
                            else
                            {
                                Debug.Assert(_inventoryOther != null);

                                if (index > _inventoryOther.Count)
                                {
                                    _inventoryOther.ClickRightMouseButton(index, ref _itemCursor);
                                }
                                else
                                {
                                    _inventorySelf.ClickRightMouseButton(
                                        index - _inventoryOther.Count,
                                        ref _itemCursor);
                                }
                            }
                            break;
                    }
                    break;
            }

            _inventorySelf.Print();

            if (_itemCursor != null && _itemCursor.Count == 0)
            {
                _itemCursor = null;
            }

            if (_itemCursor == null)
            {
                outPackets.Enqueue(new SetSlotPacket(-1, 0, new()));
            }
            else
            {
                Debug.Assert(_itemCursor.Id >= short.MinValue);
                Debug.Assert(_itemCursor.Id <= short.MaxValue);
                Debug.Assert(_itemCursor.Count >= byte.MinValue);
                Debug.Assert(_itemCursor.Count <= byte.MaxValue);
                outPackets.Enqueue(new SetSlotPacket(
                    -1, 0, 
                    new((short)_itemCursor.Id, (byte)_itemCursor.Count)));
            }

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
            _inventoryOther.RemoveRenderer()


            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Close(int id)
        {
            /*if (_opened)
                CloseWindow(id, _idWindow);*/

            Dispose();
        }

    }
}

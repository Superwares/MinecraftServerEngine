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
         * > 0: Window is opened with self and public inventory.
         */
        private int _windowId = -1;
        private int _id = -1;

        private SelfInventory _selfInventory;
        private PublicInventory? _publicInventory = null;

        private Item? _itemCursor = null;

        private bool _startDrag = false;
        private Queue<int> _dragedIndices = new();  // Disposable

        public Window(
            Queue<ClientboundPlayingPacket> outPackets,
            SelfInventory selfInventory)
        {
            _windowId = 0;

            int i = 0, n = selfInventory.Count;
            var arr = new SlotData[n];

            foreach (Item? item in selfInventory.Items)
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

            {
                Debug.Assert(_windowId >= byte.MinValue);
                Debug.Assert(_windowId <= byte.MaxValue);
                outPackets.Enqueue(new SetWindowItemsPacket((byte)_windowId, arr));

                Debug.Assert(_itemCursor == null);
                outPackets.Enqueue(new SetSlotPacket(-1, 0, new()));
            }

            Debug.Assert(i == n);

            _selfInventory = selfInventory;
        }

        ~Window() => Debug.Assert(false);

        public int GetWindowId()
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_windowId >= 0);
            Debug.Assert(_windowId > 0 ?
                _publicInventory != null : _publicInventory == null);

            return _windowId;
        }

        public void OpenWindowWithPublicInventory(
            Queue<ClientboundPlayingPacket> outPackets,
            PublicInventory publicInventory)
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_windowId == 0);
            System.Diagnostics.Debug.Assert(_id == -1);
            Debug.Assert(_publicInventory == null);

            _windowId = (new Random().Next() % 100) + 1;  // TODO

            int n = publicInventory.Count;
            Debug.Assert(n % 9 == 0);

            _id = publicInventory.Open(_windowId, outPackets);

            _publicInventory = publicInventory;
        }

        public void ResetWindow(Queue<ClientboundPlayingPacket> outPackets)
        {
            Debug.Assert(!_disposed);

            if (_windowId == 0)
            {
                System.Diagnostics.Debug.Assert(_id == -1);
                Debug.Assert(_publicInventory == null);

            }
            else
            {
                Debug.Assert(_windowId > 0);
                System.Diagnostics.Debug.Assert(_id >= 0);
                Debug.Assert(_publicInventory != null);

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

            Debug.Assert(_itemCursor == null);
            outPackets.Enqueue(new SetSlotPacket(-1, 0, new()));
        }

        internal void ResetWindowForcibly(Queue<ClientboundPlayingPacket> outPackets)
        {
            _windowId = 0;
            _id = -1;
            _publicInventory = null;

            /*if (_itemCursor != null)
            {
                // TODO: Drop item if _iremCursor is not null.
                _itemCursor = null;
            }*/

            Debug.Assert(_itemCursor == null);
            outPackets.Enqueue(new SetSlotPacket(-1, 0, new()));
        }

        public void Handle(
            int windowId, int mode, int button, int index,
            Queue<ClientboundPlayingPacket> outPackets)
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_windowId >= 0);

            /*int totalSlotCount;

            if (_windowId == 0)
            {
                Debug.Assert(_publicInventory == null);

                totalSlotCount = _selfInventory.Count;
            }
            else
            {
                Debug.Assert(_publicInventory != null);

                totalSlotCount = 36 + _publicInventory.Count;
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
                                Debug.Assert(_publicInventory == null);

                                _selfInventory.ClickLeftMouseButton(index, ref _itemCursor);
                            }
                            else
                            {
                                Debug.Assert(_publicInventory != null);

                                if (index > _publicInventory.Count)
                                {
                                    _publicInventory.ClickLeftMouseButton(index, ref _itemCursor);
                                }
                                else
                                {
                                    _selfInventory.ClickLeftMouseButton(
                                        index - _publicInventory.Count, 
                                        ref _itemCursor);
                                }
                            }
                            break;
                        case 1:
                            if (index < 0 || index >= totalSlotCount)
                                throw new UnexpectedValueException($"ClickWindowPacket.SlotNumber {index}");

                            if (_windowId == 0)
                            {
                                Debug.Assert(_publicInventory == null);

                                _selfInventory.ClickRightMouseButton(index, ref _itemCursor);
                            }
                            else
                            {
                                Debug.Assert(_publicInventory != null);

                                if (index > _publicInventory.Count)
                                {
                                    _publicInventory.ClickRightMouseButton(index, ref _itemCursor);
                                }
                                else
                                {
                                    _selfInventory.ClickRightMouseButton(
                                        index - _publicInventory.Count,
                                        ref _itemCursor);
                                }
                            }
                            break;
                    }
                    break;
            }

            _selfInventory.Print();

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
*/
        }
        
        public void Flush()
        {
            Debug.Assert(!_disposed);

            if (_windowId > 0)
            {
                Debug.Assert(_windowId > 0);
                System.Diagnostics.Debug.Assert(_id >= 0);
                Debug.Assert(_publicInventory != null);

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

            Debug.Assert(_itemCursor == null);
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
            GC.SuppressFinalize(this);
        }

        public void Close() => Dispose();

    }
}

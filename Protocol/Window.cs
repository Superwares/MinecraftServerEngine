using System;
using System.Diagnostics;
using Containers;

namespace Protocol
{
    internal sealed class Window : IDisposable
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

                Debug.Assert(_windowId >= byte.MinValue);
                Debug.Assert(_windowId <= byte.MaxValue);
                outPackets.Enqueue(new SetWindowItemsPacket((byte)_windowId, arr));

                Debug.Assert(_itemCursor == null);
                outPackets.Enqueue(new SetSlotPacket(-1, 0, new()));

                Debug.Assert(i == n);
            }

        }

        ~Window() => Debug.Assert(false);

        /*public int GetWindowId()
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_windowId >= 0);
            Debug.Assert(_windowId > 0 ?
                _publicInventory != null : _publicInventory == null);

            return _windowId;
        }*/

        public void OpenWindowWithPublicInventory(
            Queue<ClientboundPlayingPacket> outPackets,
            SelfInventory selfInventory,
            PublicInventory publicInventory)
        {
            Debug.Assert(!_disposed);

            _ambiguous = true;

            Debug.Assert(_windowId == 0);
            System.Diagnostics.Debug.Assert(_id == -1);
            Debug.Assert(_publicInventory == null);

            _windowId = (new Random().Next() % 100) + 1;

            _id = publicInventory.Open(_windowId, outPackets, selfInventory);

            _publicInventory = publicInventory;

            /*if (_itemCursor != null)
            {
                // TODO: Drop item if _iremCursor is not null.
                _itemCursor = null;
            }*/

            Debug.Assert(_itemCursor == null);
            outPackets.Enqueue(new SetSlotPacket(-1, 0, new()));
        }

        public void ResetWindow(
            int windowId, Queue<ClientboundPlayingPacket> outPackets)
        {
            Debug.Assert(!_disposed);

            if (windowId != _windowId)
            {
                if (_ambiguous)
                    return;
                else
                    throw new UnexpectedValueException("ClickWindowPacket.WindowId");
            }

            _ambiguous = false;

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

        internal void ResetWindowForcibly(
            SelfInventory selfInventory, Queue<ClientboundPlayingPacket> outPackets)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _ambiguous = true;

            _windowId = 0;
            _id = -1;
            _publicInventory = null;

            {
                int count = selfInventory.Count;
                int i = 0;
                var arr = new SlotData[count];

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

                Debug.Assert(_windowId == 0);
                Debug.Assert(_windowId >= byte.MinValue);
                Debug.Assert(_windowId <= byte.MaxValue);
                outPackets.Enqueue(new SetWindowItemsPacket((byte)_windowId, arr));

                Debug.Assert(i == count);
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
            SelfInventory selfInventory,
            int windowId, int mode, int button, int index,
            Queue<ClientboundPlayingPacket> outPackets)
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_windowId >= 0);

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

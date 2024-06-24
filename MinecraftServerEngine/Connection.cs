using Common;
using Containers;
using MinecraftServerEngine.PhysicsEngine;
using Sync;

namespace MinecraftServerEngine
{
    public sealed class Connection : System.IDisposable
    {
        private sealed class Window : System.IDisposable
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

            private ItemSlot _cursor = null;

            public Window(
                ConcurrentQueue<ClientboundPlayingPacket> outPackets,
                PlayerInventory inv)
            {
                _windowId = 0;

                System.Diagnostics.Debug.Assert(_cursor == null);
                outPackets.Enqueue(new SetSlotPacket(-1, 0, new()));

                int i = 0;
                var arr = new SlotData[inv.TotalSlotCount];

                foreach (ItemSlot slot in inv.AllSlots)
                {
                    if (slot == null)
                    {
                        arr[i++] = new SlotData();
                        continue;
                    }

                    arr[i++] = slot.ConventToProtocolFormat();
                }
                System.Diagnostics.Debug.Assert(i == inv.TotalSlotCount);

                System.Diagnostics.Debug.Assert(_windowId >= byte.MinValue);
                System.Diagnostics.Debug.Assert(_windowId <= byte.MaxValue);
                outPackets.Enqueue(new SetWindowItemsPacket((byte)_windowId, arr));

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

            /*public bool IsOpenedWithPublicInventory()
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                return _windowId > 0;
            }*/

            /*public void OpenWindowWithPublicInventory(
                ConcurrentQueue<ClientboundPlayingPacket> outPackets,
                PlayerInventory selfInventory,
                PublicInventory publicInventory)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                _ambiguous = true;

                System.Diagnostics.Debug.Assert(_windowId == 0);
                System.Diagnostics.Debug.Assert(_id == -1);
                System.Diagnostics.Debug.Assert(_publicInventory == null);

                _windowId = (Random.NextInt() % 100) + 1;

                _id = publicInventory.Open(_windowId, outPackets, selfInventory);
                if (_itemCursor != null)
                {
                    outPackets.Enqueue(new SetSlotPacket(-1, 0, _itemCursor.ConventToPacketFormat()));
                }

                _publicInventory = publicInventory;
            }*/

            public void ResetWindow(
                World world,
                int windowId, ConcurrentQueue<ClientboundPlayingPacket> outPackets)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(_windowId >= 0);

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
                    /*System.Diagnostics.Debug.Assert(_publicInventory == null);*/

                }
                else
                {
                    throw new System.NotImplementedException();

                    /*System.Diagnostics.Debug.Assert(_windowId > 0);
                    System.Diagnostics.Debug.Assert(_id >= 0);
                    System.Diagnostics.Debug.Assert(_publicInventory != null);

                    _publicInventory.Close(_id, _windowId);

                    _windowId = 0;
                    _id = -1;
                    _publicInventory = null;*/


                }

                if (_cursor != null)
                {
                    throw new System.NotImplementedException();

                    // TODO: Drop item if _iremCursor is not null.
                    /*_cursor = null;

                    outPackets.Enqueue(new SetSlotPacket(-1, 0, new()));*/
                }

                System.Diagnostics.Debug.Assert(_cursor == null);
            }

            /*internal void ResetWindowForcibly(
                PlayerInventory selfInventory, 
                ConcurrentQueue<ClientboundPlayingPacket> outPackets, bool f)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                if (f)
                {
                    System.Diagnostics.Debug.Assert(_windowId >= byte.MinValue);
                    System.Diagnostics.Debug.Assert(_windowId <= byte.MaxValue);
                    outPackets.Enqueue(new ClientboundCloseWindowPacket((byte)_windowId));
                }

                _ambiguous = true;

                _windowId = 0;
                _id = -1;
                _publicInventory = null;

                {
                    int count = selfInventory.TOTAL_SLOT_COUNT;
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

                *//*if (_itemCursor != null)
                {
                    // TODO: Drop item if _iremCursor is not null.
                    _itemCursor = null;

                    outPackets.Enqueue(new SetSlotPacket(-1, 0, new()));
                }*//*

                System.Diagnostics.Debug.Assert(_itemCursor == null);
            }*/

            private void ClickLeftMouseButton(
                PlayerInventory selfInventory, int index, SlotData slotData)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                if (index >= selfInventory.TotalSlotCount)
                {
                    throw new UnexpectedValueException("ClickWindowPacket.SlotNumber");
                }

                bool f;

                if (_cursor == null)
                {
                    f = selfInventory.TakeAll(index, ref _cursor, slotData);
                }
                else
                {
                    f = selfInventory.PutAll(index, ref _cursor, slotData);
                }

                if (!f)
                {
                    /*SlotData slotDataInCursor = _itemCursor.ConventToPacketFormat();

                    outPackets.Enqueue(new SetSlotPacket(-1, 0, slotDataInCursor));*/

                    throw new UnexpectedValueException("ClickWindowPacket.SLOT_DATA");
                }
            }

            /*private void ClickLeftMouseButtonWithPublicInventory(
                PlayerInventory selfInventory, int index, SlotData slotData,
                ConcurrentQueue<ClientboundPlayingPacket> outPackets)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(_windowId > 0);
                System.Diagnostics.Debug.Assert(_publicInventory != null);

                bool f;

                if (_itemCursor == null)
                {
                    if (index >= 0 && index < _publicInventory.TOTAL_SLOT_COUNT)
                    {
                        (f, _itemCursor) = _publicInventory.TakeAll(index, slotData);
                    }
                    else if (
                        index >= _publicInventory.TOTAL_SLOT_COUNT &&
                        index < _publicInventory.TOTAL_SLOT_COUNT + selfInventory.PrimarySlotCount)
                    {
                        int j = index + 9 - _publicInventory.TOTAL_SLOT_COUNT;
                        (f, _itemCursor) = selfInventory.TakeAll(j, slotData);
                    }
                    else
                    {
                        throw new UnexpectedValueException("ClickWindowPacket.SlotNumber");
                    }
                }
                else
                {
                    if (index >= 0 && index < _publicInventory.TOTAL_SLOT_COUNT)
                    {
                        (f, _itemCursor) = _publicInventory.PutAll(index, _itemCursor, slotData);
                    }
                    else if (
                        index >= _publicInventory.TOTAL_SLOT_COUNT &&
                        index < _publicInventory.TOTAL_SLOT_COUNT + selfInventory.PrimarySlotCount)
                    {
                        int j = index + 9 - _publicInventory.TOTAL_SLOT_COUNT;
                        (f, _itemCursor) = selfInventory.PutAll(j, _itemCursor, slotData);
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
                        if (index >= 0 && index < _publicInventory.TOTAL_SLOT_COUNT)
                        {
                            outPackets.Enqueue(new SetSlotPacket(-1, 0, new()));
                        }
                        else if (
                            index >= _publicInventory.TOTAL_SLOT_COUNT &&
                            index < _publicInventory.TOTAL_SLOT_COUNT + selfInventory.PrimarySlotCount)
                        {
                            throw new UnexpectedValueException("ClickWindowPacket.SLOT_DATA");
                        }
                        else
                        {
                            throw new System.NotImplementedException();
                        }
                    }
                    else
                    {
                        if (index >= 0 && index < _publicInventory.TOTAL_SLOT_COUNT)
                        {
                            outPackets.Enqueue(new SetSlotPacket(-1, 0, _itemCursor.ConventToPacketFormat()));
                        }
                        else if (
                            index >= _publicInventory.TOTAL_SLOT_COUNT &&
                            index < _publicInventory.TOTAL_SLOT_COUNT + selfInventory.PrimarySlotCount)
                        {
                            throw new UnexpectedValueException("ClickWindowPacket.SLOT_DATA");
                        }
                        else
                        {
                            throw new System.NotImplementedException();
                        }
                    }
                }

            }*/

            private void ClickRightMouseButton(
                PlayerInventory selfInventory, int index, SlotData slotData)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                if (index >= selfInventory.TotalSlotCount)
                {
                    throw new UnexpectedValueException("ClickWindowPacket.SlotNumber");
                }

                bool f;

                if (_cursor == null)
                {
                    f = selfInventory.TakeHalf(index, ref _cursor, slotData);
                }
                else
                {
                    f = selfInventory.PutOne(index, ref _cursor, slotData);
                }

                if (!f)
                {
                    /*SlotData slotDataInCursor = _itemCursor.ConventToPacketFormat();

                    outPackets.Enqueue(new SetSlotPacket(-1, 0, slotDataInCursor));*/

                    throw new UnexpectedValueException("ClickWindowPacket.SLOT_DATA");
                }
            }

            /*private void ClickRightMouseButtonWithPublicInventory(
                PlayerInventory selfInventory, int index, SlotData slotData,
                ConcurrentQueue<ClientboundPlayingPacket> outPackets)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(_windowId > 0);
                System.Diagnostics.Debug.Assert(_publicInventory != null);

                bool f;

                if (_itemCursor == null)
                {
                    if (index >= 0 && index < _publicInventory.TOTAL_SLOT_COUNT)
                    {
                        (f, _itemCursor) = _publicInventory.TakeHalf(index, slotData);
                    }
                    else if (
                        index >= _publicInventory.TOTAL_SLOT_COUNT &&
                        index < _publicInventory.TOTAL_SLOT_COUNT + selfInventory.PrimarySlotCount)
                    {
                        int j = index + 9 - _publicInventory.TOTAL_SLOT_COUNT;
                        (f, _itemCursor) = selfInventory.TakeHalf(j, slotData);
                    }
                    else
                    {
                        throw new UnexpectedValueException("ClickWindowPacket.SlotNumber");
                    }
                }
                else
                {
                    if (index >= 0 && index < _publicInventory.TOTAL_SLOT_COUNT)
                    {
                        (f, _itemCursor) = _publicInventory.PutOne(index, _itemCursor, slotData);
                    }
                    else if (
                        index >= _publicInventory.TOTAL_SLOT_COUNT &&
                        index < _publicInventory.TOTAL_SLOT_COUNT + selfInventory.PrimarySlotCount)
                    {
                        int j = index + 9 - _publicInventory.TOTAL_SLOT_COUNT;
                        (f, _itemCursor) = selfInventory.PutOne(j, _itemCursor, slotData);
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
                        if (index >= 0 && index < _publicInventory.TOTAL_SLOT_COUNT)
                        {
                            outPackets.Enqueue(new SetSlotPacket(-1, 0, new()));
                        }
                        else if (
                            index >= _publicInventory.TOTAL_SLOT_COUNT &&
                            index < _publicInventory.TOTAL_SLOT_COUNT + selfInventory.PrimarySlotCount)
                        {
                            throw new UnexpectedValueException("ClickWindowPacket.SLOT_DATA");
                        }
                        else
                        {
                            throw new System.NotImplementedException();
                        }
                    }
                    else
                    {
                        if (index >= 0 && index < _publicInventory.TOTAL_SLOT_COUNT)
                        {
                            outPackets.Enqueue(new SetSlotPacket(-1, 0, _itemCursor.ConventToPacketFormat()));
                        }
                        else if (
                            index >= _publicInventory.TOTAL_SLOT_COUNT &&
                            index < _publicInventory.TOTAL_SLOT_COUNT + selfInventory.PrimarySlotCount)
                        {
                            throw new UnexpectedValueException("ClickWindowPacket.SLOT_DATA");
                        }
                        else
                        {
                            throw new System.NotImplementedException();
                        }
                    }
                }

            }*/

            public void Handle(
                World world,
                PlayerInventory inv,
                int windowId, int mode, int button, int index, SlotData slotData,
                ConcurrentQueue<ClientboundPlayingPacket> outPackets)   
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(_windowId >= 0);

                if (_windowId == 0)
                {
                    if (windowId > 0)
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
                }
                else
                {
                    System.Diagnostics.Debug.Assert(_windowId > 0);
                    if (windowId == 0)
                    {
                        if (_ambiguous)
                        {
                            // Ignored...
                            return;
                        }
                        else
                        {
                            throw new UnexpectedValueException("ClickWindowPacket.WindowId");
                        }
                    }
                    else if (_windowId != windowId)  // TODO: Check it is correct condition.
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
                                    ClickLeftMouseButton(inv, index, slotData);
                                }
                                else
                                {
                                    throw new System.NotImplementedException();
                                    /*ClickLeftMouseButtonWithPublicInventory(
                                        inv, index, slotData,
                                        outPackets);*/
                                }
                                break;
                            case 1:
                                if (_windowId == 0)
                                {
                                    ClickRightMouseButton(inv, index, slotData);
                                }
                                else
                                {
                                    throw new System.NotImplementedException();
                                    /*ClickRightMouseButtonWithPublicInventory(
                                        inv, index, slotData,
                                        outPackets);*/
                                }
                                break;
                        }
                        break;
                    case 1:
                        throw new System.NotImplementedException();
                    case 2:
                        throw new System.NotImplementedException();
                    case 3:
                        throw new System.NotImplementedException();
                    case 4:
                        throw new System.NotImplementedException();
                    case 5:
                        throw new System.NotImplementedException();
                    case 6:
                        throw new System.NotImplementedException();
                }

                {
                    if (_windowId == 0)
                    {
                        inv.Print();
                    }
                    else
                    {
                        throw new System.NotImplementedException();

                        /*System.Diagnostics.Debug.Assert(_publicInventory != null);
                        _publicInventory.Print();
                        inv.Print();*/
                    }

                    if (_cursor != null)
                    {
                        Console.Printl($"Cursor: {_cursor}");
                    }
                }

            }

            public void Flush(World world)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                if (_cursor != null)
                {
                    // TODO: Drop Item.
                    throw new System.NotImplementedException();

                    /*_cursor = null;*/
                }

                /*if (_publicInventory != null)
                {
                    System.Diagnostics.Debug.Assert(_windowId > 0);
                    System.Diagnostics.Debug.Assert(_id >= 0);

                    _publicInventory.Close(_id, _windowId);

                    _publicInventory = null;
                }*/

                _windowId = 0;
                _id = -1;

            }

            public void Dispose()
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                // Assertion.
                System.Diagnostics.Debug.Assert(_windowId == 0);
                System.Diagnostics.Debug.Assert(_id == -1);
                /*System.Diagnostics.Debug.Assert(_publicInventory == null);*/
                System.Diagnostics.Debug.Assert(_cursor == null);

                // Release Resources.

                System.GC.SuppressFinalize(this);
                _disposed = true;
            }

        }
        private sealed class ChunkingHelper : System.IDisposable
        {
            private const int MaxLoadCount = 7;
            private const int MaxSearchCount = 103;

            private bool _disposed = false;

            private readonly Set<ChunkLocation> _LOADED_CHUNKS = new();  // Disposable

            private ChunkGrid _grid;
            private ChunkLocation _loc;
            private int _d;

            private int _x, _z, _layer, _n;

            public ChunkingHelper(ChunkLocation loc, int d) 
            {
                _loc = loc;
                _d = d;
                _grid = ChunkGrid.Generate(loc, d);

                _layer = 0;
                _n = 0;
            }

            ~ChunkingHelper() => System.Diagnostics.Debug.Assert(false);

            public void Load(
                Queue<ChunkLocation> newChunks, Queue<ChunkLocation> outOfRangeChunks, 
                ChunkLocation loc, int d)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(d > 0);
                System.Diagnostics.Debug.Assert(MaxLoadCount > 0);
                System.Diagnostics.Debug.Assert(MaxSearchCount >= MaxLoadCount);

                if (!loc.Equals(_loc) || (d != _d))
                {
                    ChunkGrid grid = ChunkGrid.Generate(loc, d);

                    System.Diagnostics.Debug.Assert(!_grid.Equals(grid));

                    // TODO: Refactoring without brute-force approch.
                    foreach (ChunkLocation locPrev in _grid.GetLocations())
                    {
                        if (grid.Contains(locPrev))
                        {
                            continue;
                        }

                        if (_LOADED_CHUNKS.Contains(locPrev))
                        {
                            _LOADED_CHUNKS.Extract(locPrev);
                            outOfRangeChunks.Enqueue(locPrev);
                        }
                    }

                    _loc = loc;
                    _d = d;
                    _grid = grid;

                    _layer = 0;
                    _n = 0;
                }

                if (_layer > d)
                {
                    return;
                }

                {
                    int i = 0, j = 0;

                    int w;
                    ChunkLocation locTarget;

                    do
                    {
                        w = _layer * 2;

                        System.Diagnostics.Debug.Assert(_layer >= 0);

                        if (_layer == 0)
                        {
                            locTarget = loc;
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(_n >= 0);
                            if (_n < w)
                            {
                                locTarget = new(--_x, _z);
                            }
                            else if (_n < w * 2)
                            {
                                locTarget = new(_x, --_z);
                            }
                            else if (_n < w * 3)
                            {
                                locTarget = new(++_x, _z);
                            }
                            else if (_n < w * 4)
                            {
                                locTarget = new(_x, ++_z);
                            }
                            else
                            {
                                throw new System.NotImplementedException();
                            }

                            ++_n;
                        }

                        if (!_LOADED_CHUNKS.Contains(locTarget))
                        {
                            newChunks.Enqueue(locTarget);
                            _LOADED_CHUNKS.Insert(locTarget);
                            ++i;
                        }

                        System.Diagnostics.Debug.Assert(_n <= w * 4);
                        if (_n == w * 4)
                        {
                            ++_layer;
                            _n = 0;

                            _x = loc.X + _layer; _z = loc.Z + _layer;
                        }

                        if (_layer > d)
                        {
                            break;
                        }

                        System.Diagnostics.Debug.Assert(j <= MaxSearchCount);
                        if (++j == MaxSearchCount)
                        {
                            break;
                        }

                    } while (i < MaxLoadCount);
                }

                System.Diagnostics.Debug.Assert(_LOADED_CHUNKS.Count <= (d + d + 1) * (d + d + 1));
            }

            public void Dispose()
            {
                // Assertions.
                System.Diagnostics.Debug.Assert(!_disposed);

                // Release resources.
                _LOADED_CHUNKS.Dispose();

                // Finish.
                System.GC.SuppressFinalize(this);
                _disposed = true;
            }

        }

        private sealed class TeleportationRecord
        {
            private const long Timeout = 20 * 10;  // 10 seconds, 20 * 10 ticks

            public readonly int _payload;
            private long _ticks = 0;

            private readonly Vector _p;
            public Vector Position => _p;

            public TeleportationRecord(int payload, Vector p)
            {
                _payload = payload;
                _p = p;
            }

            public void Confirm(int payload)
            {
                if (payload != _payload)
                {
                    throw new UnexpectedValueException("ConfirmSelfPlayerTeleportationPacket.Payload");
                }
            }

            public void Update()
            {
                System.Diagnostics.Debug.Assert(_ticks >= 0);

                if (++_ticks > Timeout)
                {
                    throw new TeleportationConfirmTimeoutException();
                }

            }

        }

        private sealed class KeepAliveRecord
        {
            private const long Timeout = 20 * 30;  // 30 seconds, 20 * 30 ticks

            private long _payload;
            private long _ticks = -1;

            public KeepAliveRecord() { }

            public long Confirm(long payload)
            {
                if (_ticks == -1)
                {
                    throw new UnexpectedPacketException();
                }
                System.Diagnostics.Debug.Assert(_ticks >= 0);

                if (payload != _payload)
                {
                    throw new UnexpectedValueException("ResponseKeepAlivePacketId.Payload");
                }

                long ticks = _ticks;
                _ticks = -1;

                return ticks;
            }

            public void Update(ConcurrentQueue<ClientboundPlayingPacket> renderer)
            {
                if (_ticks == -1)
                {
                    long payload = Random.NextLong();
                    renderer.Enqueue(new RequestKeepAlivePacket(payload));
                    _payload = payload;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(_ticks >= 0);
                }

                System.Diagnostics.Debug.Assert(_ticks < long.MaxValue);
                if (++_ticks > Timeout)
                {
                    throw new ResponseKeepAliveTimeoutException();
                }

            }

        }

        private bool _disposed = false;

        private readonly Client Client;  // Dispoasble

        
        private bool _init = false;


        private bool _disconnected = false;
        public bool Disconnected => _disconnected;


        private readonly Queue<LoadChunkPacket> LoadChunkPackets = new();  // Dispoasble
        private readonly ConcurrentQueue<ClientboundPlayingPacket> OutPackets = new();  // Dispoasble


        private const int MAxEntityRanderDistance = 7;
        private const int MinRenderDistance = 2, MaxRenderDistance = 32;
        private int _dEntityRendering = MinRenderDistance;
        private int _dChunkRendering = MinRenderDistance;


        private readonly EntityRenderer EntityRenderer;


        private readonly ChunkingHelper ChunkingHelprt;  // Dispoasble
        private readonly Queue<TeleportationRecord> TeleportationRecords = new();  // Dispoasble
        private KeepAliveRecord _keepAliveRecord = new();  // Disposable


        private Window _window;  // Disposable

        internal Connection(
            Client client, 
            World world, 
            int id, System.Guid userId, 
            Vector p, 
            PlayerInventory inv)
        {
            Client = client;

            System.Diagnostics.Debug.Assert(MAxEntityRanderDistance >= MinRenderDistance);
            System.Diagnostics.Debug.Assert(MaxRenderDistance >= MAxEntityRanderDistance);

            ChunkLocation loc = ChunkLocation.Generate(p);
            EntityRenderer = new EntityRenderer(OutPackets, id, loc, _dEntityRendering);

            ChunkingHelprt = new ChunkingHelper(loc, _dChunkRendering);

            _window = new Window(OutPackets, inv);

            PlayerListRenderer plRenderer = new(OutPackets);
            world.PlayerList.Connect(userId, plRenderer);
        }

        ~Connection() => System.Diagnostics.Debug.Assert(false);

        private void RecvDataAndHandle(
            Queue<Control> controls,
            Buffer buffer, 
            World world, 
            System.Guid userId, 
            bool sneaking, bool sprinting,
            PlayerInventory inv)
        {
            System.Diagnostics.Debug.Assert(controls != null);
            System.Diagnostics.Debug.Assert(buffer != null);
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(inv != null);

            if (_disconnected)
            {
                throw new DisconnectedClientException();
            }

            Client.Recv(buffer);

            int packetId = buffer.ReadInt(true);
            switch (packetId)
            {
                default:
                    Console.Printl($"packetId: 0x{packetId:X}");
                    /*throw new NotImplementedException();*/
                    buffer.Flush();
                    break;
                case ServerboundPlayingPacket.ConfirmSelfPlayerTeleportationPacketId:
                    {
                        ConfirmSelfPlayerTeleportationPacket packet = ConfirmSelfPlayerTeleportationPacket.Read(buffer);

                        if (TeleportationRecords.Empty)
                        {
                            throw new UnexpectedPacketException();
                        }

                        TeleportationRecord record = TeleportationRecords.Dequeue();
                        record.Confirm(packet.Payload);

                        Vector p = new(record.Position.X, record.Position.Y, record.Position.Z);
                        ChunkLocation locChunk = ChunkLocation.Generate(p);
                        EntityRenderer.Update(locChunk);
                    }
                    break;
                case ServerboundPlayingPacket.SetClientSettingsPacketId:
                    {
                        SetClientSettingsPacket packet = SetClientSettingsPacket.Read(buffer);

                        int d = packet.RenderDistance;
                        
                        if (d < MinRenderDistance || d > MaxRenderDistance)
                        {
                            throw new UnexpectedValueException("SetClientSettingsPacket.RenderDistance");
                        }

                        _dChunkRendering = d;
                        _dEntityRendering = System.Math.Min(d, MAxEntityRanderDistance);

                        EntityRenderer.Update(_dEntityRendering);
                    }
                    break;
                case ServerboundPlayingPacket.ServerboundConfirmTransactionPacketId:
                    {
                        ServerboundConfirmTransactionPacket packet =
                            ServerboundConfirmTransactionPacket.Read(buffer);

                        Console.Printl(
                            $"WindowId: {packet.WindowId}, " +
                            $"ActionNumber: {packet.ActionNumber}, " +
                            $"Accepted: {packet.Accepted}, ");

                        throw new UnexpectedPacketException();
                    }
                    /*break;*/
                case ServerboundPlayingPacket.ClickWindowPacketId:
                    {
                        ClickWindowPacket packet = ClickWindowPacket.Read(buffer);

                        {
                            Console.NewLine();
                            Console.Printl(
                                $"WindowId: {packet.WINDOW_ID}, " +
                                $"SlotNumber: {packet.SLOT}, " +
                                $"ButtonNumber: {packet.BUTTON}, " +
                                $"ActionNumber: {packet.ACTION}, " +
                                $"ModeNumber: {packet.MODE}, " +
                                $"SlotData.Id: {packet.SLOT_DATA.Id}, " +
                                $"SlotData.Count: {packet.SLOT_DATA.Count}, ");
                        }

                        System.Diagnostics.Debug.Assert(_window != null);
                        _window.Handle(
                            world,
                            inv,
                            packet.WINDOW_ID,
                            packet.MODE,
                            packet.BUTTON,
                            packet.SLOT,
                            packet.SLOT_DATA,
                            OutPackets);

                        OutPackets.Enqueue(new ClientboundConfirmTransactionPacket(
                                (sbyte)packet.WINDOW_ID, packet.ACTION, true));
                    }
                    break;
                case ServerboundPlayingPacket.ServerboundCloseWindowPacketId:
                    {
                        ServerboundCloseWindowPacket packet =
                            ServerboundCloseWindowPacket.Read(buffer);

                        if (packet.WindowId < 0)
                        {
                            throw new UnexpectedValueException($"ClickWindowPacket.WindowId");
                        }

                        System.Diagnostics.Debug.Assert(_window != null);
                        _window.ResetWindow(world, packet.WindowId, OutPackets);
                    }
                    break;
                case 0x09:
                    {
                        buffer.Flush();
                    }
                    break;
                case ServerboundPlayingPacket.ResponseKeepAlivePacketId:
                    {
                        ResponseKeepAlivePacket packet = ResponseKeepAlivePacket.Read(buffer);

                        if (_keepAliveRecord == null)
                        {
                            throw new UnexpectedPacketException();
                        }

                        long ticks = _keepAliveRecord.Confirm(packet.Payload);
                        world.PlayerList.UpdateLaytency(userId, ticks);
                    }
                    break;
                case ServerboundPlayingPacket.PlayerPacketId:
                    {
                        PlayerPacket packet = PlayerPacket.Read(buffer);

                        if (TeleportationRecords.Empty)
                        {
                            StandControl control = new(packet.OnGround);
                            controls.Enqueue(control);
                        }

                    }
                    break;
                case ServerboundPlayingPacket.PlayerPositionPacketId:
                    {
                        PlayerPositionPacket packet = PlayerPositionPacket.Read(buffer);
                
                        if (TeleportationRecords.Empty)
                        {
                            Vector p = new(packet.X, packet.Y, packet.Z);

                            controls.Enqueue(new MoveControl(p));
                            controls.Enqueue(new StandControl(packet.OnGround));

                            ChunkLocation locChunk = ChunkLocation.Generate(p);
                            EntityRenderer.Update(locChunk);
                        }

                    }
                    break;
                case ServerboundPlayingPacket.PlayerPosAndLookPacketId:
                    {
                        PlayerPosAndLookPacket packet = PlayerPosAndLookPacket.Read(buffer);

                        if (TeleportationRecords.Empty)
                        {
                            Vector p = new(packet.X, packet.Y, packet.Z);
                            Look look = new(packet.Yaw, packet.Pitch);

                            controls.Enqueue(new MoveControl(p));
                            controls.Enqueue(new RotControl(look));
                            controls.Enqueue(new StandControl(packet.OnGround));

                            ChunkLocation locChunk = ChunkLocation.Generate(p);
                            EntityRenderer.Update(locChunk);
                        }

                    }
                    break;
                case ServerboundPlayingPacket.PlayerLookPacketId:
                    {
                        PlayerLookPacket packet = PlayerLookPacket.Read(buffer);

                        if (TeleportationRecords.Empty)
                        {
                            Look look = new(packet.Yaw, packet.Pitch);

                            controls.Enqueue(new RotControl(look));
                            controls.Enqueue(new StandControl(packet.OnGround));
                        }

                    }
                    break;
                case ServerboundPlayingPacket.EntityActionPacketId:
                    {
                        EntityActionPacket packet = EntityActionPacket.Read(buffer);

                        switch (packet.ActionId)
                        {
                            default:
                                throw new UnexpectedValueException("EntityAction.ActoinId");
                            case 0:
                                /*Console.Print("Seanking!");*/
                                if (sneaking)
                                {
                                    throw new UnexpectedValueException("EntityActionPacket.ActionId");
                                }

                                controls.Enqueue(new SneakControl());

                                sneaking = true;
                                break;
                            case 1:
                                /*Console.Print("Unseanking!");*/
                                if (!sneaking)
                                {
                                    throw new UnexpectedValueException("EntityActionPacket.ActionId");
                                }

                                controls.Enqueue(new UnsneakControl());

                                sneaking = false;
                                break;
                            case 3:
                                if (sprinting)
                                {
                                    throw new UnexpectedValueException("EntityActionPacket.ActionId");
                                }

                                controls.Enqueue(new SprintControl());

                                sprinting = true;
                                break;
                            case 4:
                                if (!sprinting)
                                {
                                    throw new UnexpectedValueException("EntityActionPacket.ActionId");
                                }

                                controls.Enqueue(new UnsprintControl());

                                sprinting = false;
                                break;
                        }

                        if (packet.JumpBoost > 0)
                        {
                            throw new UnexpectedValueException("EntityActionPacket.JumpBoost");
                        }

                    }
                    break;
            }

        }

        internal void Control(
            Queue<Control> controls,
            World world,
            System.Guid userId,
            bool sneaking, bool sprinting,
            PlayerInventory inv)
        {
            System.Diagnostics.Debug.Assert(controls != null);
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(inv != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            /*if (serverTicks == 200)  // 10 seconds
            {
                System.Diagnostics.Debug.Assert(_window != null);
                _window.OpenWindowWithPublicInventory(
                    _OUT_PACKETS,
                    player._selfInventory,
                    world._Inventory);
            }*/

            using Buffer buffer = new();

            try
            {
                try
                {
                    try
                    {
                        while (true)
                        {
                            RecvDataAndHandle(
                                controls,
                                buffer, 
                                world,
                                userId,
                                sneaking, sprinting,
                                inv);
                        }
                    }
                    catch (TryAgainException)
                    {

                    }

                    foreach (TeleportationRecord record in TeleportationRecords.GetValues())
                    {
                        record.Update();
                    }

                    _keepAliveRecord.Update(OutPackets);

                }
                catch (UnexpectedClientBehaviorExecption e)
                {
                    // TODO: send disconnected message to client.

                    Console.Printl($"UnexpectedClientBehavior: {e.Message}!");

                    throw new DisconnectedClientException();
                }

            }
            catch (DisconnectedClientException)
            {
                buffer.Flush();

                _disconnected = true;
                /*Console.Print("Disconnect!");*/
            }
            
        }

        private void LoadWorld(World world, Vector p)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_disconnected);

            System.Diagnostics.Debug.Assert(_dChunkRendering >= MinRenderDistance);
            System.Diagnostics.Debug.Assert(_dChunkRendering <= MaxRenderDistance);
            System.Diagnostics.Debug.Assert(_dEntityRendering >= MinRenderDistance);
            System.Diagnostics.Debug.Assert(_dEntityRendering <= MAxEntityRanderDistance);

            ChunkLocation loc = ChunkLocation.Generate(p);

            /*Console.Printl($"_dEntityRendering: {_dEntityRendering}");*/
            ChunkGrid grid = ChunkGrid.Generate(loc, _dEntityRendering);
            /*Console.Printl($"grid: {grid}");*/

            AxisAlignedBoundingBox aabbTotal = grid.GetMinBoundingBox();
            foreach (PhysicsObject obj in world.GetObjects(aabbTotal))
            {
                switch (obj)
                {
                    default:
                        throw new System.NotImplementedException();
                    case Entity entity:
                        entity.ApplyRenderer(EntityRenderer);
                        break;
                }
            }

            using Queue<ChunkLocation> newChunkPositions = new();
            using Queue<ChunkLocation> outOfRangeChunks = new();

            ChunkingHelprt.Load(newChunkPositions, outOfRangeChunks, loc, _dChunkRendering);

            int mask; byte[] data;
            while (!newChunkPositions.Empty)
            {
                ChunkLocation locChunk = newChunkPositions.Dequeue();

                (mask, data) = world.BlockContext.GetChunkData(locChunk);

                LoadChunkPackets.Enqueue(new LoadChunkPacket(
                    locChunk.X, locChunk.Z, true, mask, data));
            }

            while (!outOfRangeChunks.Empty)
            {
                ChunkLocation locOut = outOfRangeChunks.Dequeue();

                OutPackets.Enqueue(new UnloadChunkPacket(locOut.X, locOut.Z));
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="packet"></param>
        /// <exception cref="DisconnectedClientException"></exception>
        /// <exception cref="TryAgainException"></exception>
        private void SendPacket(Buffer buffer, ClientboundPlayingPacket packet)
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            System.Diagnostics.Debug.Assert(!_disconnected);

            packet.Write(buffer);
            Client.Send(buffer);

            System.Diagnostics.Debug.Assert(buffer.Empty);
        }

        public void ApplyVelocity(int id, Vector v)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (_disconnected)
            {
                return;
            }

            EntityVelocityPacket packet = new(
                id,
                (short)(v.X * 8000),
                (short)(v.Y * 8000),
                (short)(v.Z * 8000));

            using Buffer buffer = new();

            bool tryAgain;

            do
            {
                tryAgain = false;

                try
                {
                    SendPacket(buffer, packet);
                }
                catch (DisconnectedClientException)
                {
                    buffer.Flush();

                    _disconnected = true;

                    /*Console.Print("Disconnect!");*/

                    System.Diagnostics.Debug.Assert(!tryAgain);
                }
                catch (TryAgainException)
                {
                    tryAgain = true;
                }

            } while (tryAgain);

            System.Diagnostics.Debug.Assert(buffer.Empty);
        }

        public void Teleport(Vector p, Look look)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (_disconnected)
            {
                return;
            }

            int payload = Random.NextInt();
            TeleportSelfPlayerPacket packet = new(
                p.X, p.Y, p.Z, look.Yaw, look.Pitch,
                false, false, false, false, false,
                payload);

            using Buffer buffer = new();

            bool tryAgain;

            do
            {
                tryAgain = false;

                try
                {
                    SendPacket(buffer, packet);

                    TeleportationRecord report = new(payload, p);
                    TeleportationRecords.Enqueue(report);
                }
                catch (DisconnectedClientException)
                {
                    buffer.Flush();

                    _disconnected = true;
                    /*Console.Print("Disconnect!");*/

                    System.Diagnostics.Debug.Assert(!tryAgain);
                }
                catch (TryAgainException)
                {
                    tryAgain = true;
                }

            } while (tryAgain);

            System.Diagnostics.Debug.Assert(buffer.Empty);
        }

        internal void Render(
            World world, 
            int id,
            Vector p, Look look, 
            PlayerInventory inv)
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            System.Diagnostics.Debug.Assert(!_disconnected);

            using Buffer buffer = new();

            try
            {
                if (!_init)
                {

                    JoinGamePacket packet = new(id, 0, 0, 0, "default", false);
                    SendPacket(buffer, packet);

                    int payload = Random.NextInt();
                    OutPackets.Enqueue(new TeleportSelfPlayerPacket(
                        p.X, p.Y, p.Z,
                        look.Yaw, look.Pitch,
                        false, false, false, false, false,
                        payload));

                    TeleportationRecord report = new(payload, p);
                    TeleportationRecords.Enqueue(report);

                    OutPackets.Enqueue(new SetPlayerAbilitiesPacket(
                            false, false, true, false, 0.1F, 0.0F));

                    _init = true;
                }

                LoadWorld(world, p);

                while (!LoadChunkPackets.Empty)
                {
                    LoadChunkPacket packet = LoadChunkPackets.Dequeue();
                    SendPacket(buffer, packet);

                    System.Diagnostics.Debug.Assert(buffer.Empty);
                }

                while (!OutPackets.Empty)
                {
                    ClientboundPlayingPacket packet = OutPackets.Dequeue();

                    /*if (packet is TeleportSelfPlayerPacket teleportPacket)
                    {
                        
                    }*/
                    /*else if (packet is ClientboundCloseWindowPacket)
                    {
                        throw new System.NotImplementedException();
                        System.Diagnostics.Debug.Assert(_window != null);
                        _window.ResetWindowForcibly(inv, _OUT_PACKETS, false);
                    }*/

                    SendPacket(buffer, packet);

                    System.Diagnostics.Debug.Assert(buffer.Empty);
                }

                System.Diagnostics.Debug.Assert(LoadChunkPackets.Empty);
                System.Diagnostics.Debug.Assert(OutPackets.Empty);
            }
            catch (DisconnectedClientException)
            {
                buffer.Flush();

                _disconnected = true;
                /*Console.Print("Disconnect!");*/
            }

        }

        public void Flush(
            World world, 
            System.Guid userId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            System.Diagnostics.Debug.Assert(_disconnected);

            EntityRenderer.Disconnect();

            _window.Flush(world);

            world.PlayerList.Disconnect(userId);
        }

        public void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);
            System.Diagnostics.Debug.Assert(_disconnected);

            // Release Resources.
            Client.Dispose();

            LoadChunkPackets.Dispose();
            OutPackets.Dispose();

            ChunkingHelprt.Dispose();
            TeleportationRecords.Dispose();

            _window.Dispose();

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }
}

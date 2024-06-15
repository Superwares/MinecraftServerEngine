
using Containers;
using PhysicsEngine;

namespace MinecraftServerEngine
{
    public sealed class Connection : System.IDisposable
    {

        private sealed class LoadingHelper : System.IDisposable
        {
            private const int _MAX_LOAD_COUNT = 7;
            private const int _MAX_SEARCH_COUNT = 103;

            private bool _disposed = false;

            private readonly Set<ChunkLocation> _LOADED_CHUNKS = new();  // Disposable

            private ChunkGrid _grid;
            private ChunkLocation _loc;
            private int _d;

            private int _x, _z, _layer, _n;

            public LoadingHelper(ChunkLocation loc, int d) 
            {
                _loc = loc;
                _d = d;
                _grid = ChunkGrid.Generate(loc, d);

                _layer = 0;
                _n = 0;
            }

            ~LoadingHelper() => System.Diagnostics.Debug.Assert(false);

            public void Load(
                Queue<ChunkLocation> newChunks, Queue<ChunkLocation> outOfRangeChunks, 
                ChunkLocation loc, int d)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(d > 0);
                System.Diagnostics.Debug.Assert(_MAX_LOAD_COUNT > 0);
                System.Diagnostics.Debug.Assert(_MAX_SEARCH_COUNT >= _MAX_LOAD_COUNT);

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

                        System.Diagnostics.Debug.Assert(j <= _MAX_SEARCH_COUNT);
                        if (++j == _MAX_SEARCH_COUNT)
                        {
                            break;
                        }

                    } while (i < _MAX_LOAD_COUNT);
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
            private const long _MAX_TICKS = 20;  // 1 seconds, 20 ticks

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

                if (++_ticks > _MAX_TICKS)
                {
                    throw new TeleportationConfirmTimeoutException();
                }

            }

        }

        private sealed class KeepAliveRecord
        {
            private const long _MAX_TICKS = 20 * 30;  // 30 seconds, 20 * 30 ticks

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
                    long payload = new System.Random().NextInt64();
                    renderer.Enqueue(new RequestKeepAlivePacket(payload));
                    _payload = payload;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(_ticks >= 0);
                }

                System.Diagnostics.Debug.Assert(_ticks < long.MaxValue);
                if (++_ticks > _MAX_TICKS)
                {
                    throw new ResponseKeepAliveTimeoutException();
                }

            }

        }

        private bool _disposed = false;

        private readonly Client _CLIENT;  // Dispoasble

        
        private bool _init = false;


        private bool _disconnected = false;
        public bool Disconnected => _disconnected;


        private readonly Queue<LoadChunkPacket> _CHUNK_LOAD_PACKETS = new();  // Dispoasble
        private readonly ConcurrentQueue<ClientboundPlayingPacket> _OUT_PACKETS = new();  // Dispoasble


        private const int _MAX_ENTITY_RENDER_DISTANCE = 7;
        private const int _MIN_RENDER_DISTANCE = 2, _MAX_RENDER_DISTANCE = 32;
        private int _dEntityRendering = _MIN_RENDER_DISTANCE;
        private int _dChunkRendering = _MIN_RENDER_DISTANCE;


        private readonly EntityRenderer _ENTITY_RENDERER;


        private readonly LoadingHelper _LOADING_HELPER;  // Dispoasble
        private readonly Queue<TeleportationRecord> _TELEPORTATION_RECORDS = new();  // Dispoasble
        private KeepAliveRecord _keepAliveRecord = new();  // Disposable


        private Window _window;  // Disposable

        internal Connection(
            Client client, 
            World world, 
            int id, System.Guid userId, 
            Vector p, 
            SelfInventory inv)
        {
            _CLIENT = client;


            System.Diagnostics.Debug.Assert(_MAX_ENTITY_RENDER_DISTANCE >= _MIN_RENDER_DISTANCE);
            System.Diagnostics.Debug.Assert(_MAX_RENDER_DISTANCE >= _MAX_ENTITY_RENDER_DISTANCE);

            ChunkLocation loc = ChunkLocation.Generate(p);
            _ENTITY_RENDERER = new EntityRenderer(_OUT_PACKETS, id, loc, _dEntityRendering);

            _LOADING_HELPER = new LoadingHelper(loc, _dChunkRendering);

            _window = new Window(_OUT_PACKETS, inv);

            PlayerListRenderer plRenderer = new(_OUT_PACKETS);
            world._PLAYER_LIST.Connect(userId, plRenderer);
        }

        ~Connection() => System.Diagnostics.Debug.Assert(false);

        private void RecvDataAndHandle(
            Queue<ServerboundPlayingPacket> controls,
            Buffer buffer, 
            World world, 
            System.Guid userId, 
            bool sneaking, bool sprinting,
            SelfInventory inv)
        {
            if (_disconnected)
            {
                throw new DisconnectedClientException();
            }

            _CLIENT.Recv(buffer);

            int packetId = buffer.ReadInt(true);
            switch (packetId)
            {
                default:
                    System.Console.WriteLine($"packetId: 0x{packetId:X}");
                    /*throw new NotImplementedException();*/
                    buffer.Flush();
                    break;
                case ServerboundPlayingPacket.ConfirmSelfPlayerTeleportationPacketId:
                    {
                        ConfirmSelfPlayerTeleportationPacket packet = ConfirmSelfPlayerTeleportationPacket.Read(buffer);

                        if (_TELEPORTATION_RECORDS.Empty)
                        {
                            throw new UnexpectedPacketException();
                        }

                        TeleportationRecord record = _TELEPORTATION_RECORDS.Dequeue();
                        record.Confirm(packet.Payload);

                        Vector p = new(record.Position.X, record.Position.Y, record.Position.Z);
                        ChunkLocation locChunk = ChunkLocation.Generate(p);
                        _ENTITY_RENDERER.Update(locChunk);
                    }
                    break;
                case ServerboundPlayingPacket.SetClientSettingsPacketId:
                    {
                        SetClientSettingsPacket packet = SetClientSettingsPacket.Read(buffer);

                        int d = packet.RenderDistance;
                        
                        if (d < _MIN_RENDER_DISTANCE || d > _MAX_RENDER_DISTANCE)
                        {
                            throw new UnexpectedValueException("SetClientSettingsPacket.RenderDistance");
                        }

                        _dChunkRendering = d;
                        _dEntityRendering = System.Math.Min(d, _MAX_ENTITY_RENDER_DISTANCE);

                        _ENTITY_RENDERER.Update(_dEntityRendering);
                    }
                    break;
                case ServerboundPlayingPacket.ServerboundConfirmTransactionPacketId:
                    {
                        ServerboundConfirmTransactionPacket packet =
                            ServerboundConfirmTransactionPacket.Read(buffer);

                        System.Console.WriteLine(
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
                            System.Console.WriteLine();
                            System.Console.WriteLine(
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
                            inv,
                            packet.WINDOW_ID,
                            packet.MODE,
                            packet.BUTTON,
                            packet.SLOT,
                            packet.SLOT_DATA,
                            _OUT_PACKETS);

                        _OUT_PACKETS.Enqueue(new ClientboundConfirmTransactionPacket(
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
                        _window.ResetWindow(packet.WindowId, _OUT_PACKETS);
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
                        world._PLAYER_LIST.UpdateLaytency(userId, ticks);
                    }
                    break;
                case ServerboundPlayingPacket.PlayerPacketId:
                    {
                        PlayerPacket packet = PlayerPacket.Read(buffer);

                        if (_TELEPORTATION_RECORDS.Empty)
                        {
                            controls.Enqueue(packet);
                            /*_PLAYER.Control(packet.OnGround);*/
                        }

                    }
                    break;
                case ServerboundPlayingPacket.PlayerPositionPacketId:
                    {
                        PlayerPositionPacket packet = PlayerPositionPacket.Read(buffer);
                
                        if (_TELEPORTATION_RECORDS.Empty)
                        {
                            Vector p = new(packet.X, packet.Y, packet.Z);

                            controls.Enqueue(packet);
                            /*_PLAYER.Control(p, packet.OnGround);*/

                            ChunkLocation locChunk = ChunkLocation.Generate(p);
                            _ENTITY_RENDERER.Update(locChunk);
                        }

                    }
                    break;
                case ServerboundPlayingPacket.PlayerPosAndLookPacketId:
                    {
                        PlayerPosAndLookPacket packet = PlayerPosAndLookPacket.Read(buffer);

                        if (_TELEPORTATION_RECORDS.Empty)
                        {
                            Vector p = new(packet.X, packet.Y, packet.Z);

                            controls.Enqueue(packet);
                            /*
                            _PLAYER.Control(p, packet.OnGround);
                            _PLAYER.Rotate(new(packet.Yaw, packet.Pitch));*/

                            ChunkLocation locChunk = ChunkLocation.Generate(p);
                            _ENTITY_RENDERER.Update(locChunk);
                        }

                    }
                    break;
                case ServerboundPlayingPacket.PlayerLookPacketId:
                    {
                        PlayerLookPacket packet = PlayerLookPacket.Read(buffer);

                        if (_TELEPORTATION_RECORDS.Empty)
                        {
                            controls.Enqueue(packet);
                            /*_PLAYER.Control(packet.OnGround);
                            _PLAYER.Rotate(new(packet.Yaw, packet.Pitch));*/
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
                                /*System.Console.Write("Seanking!");*/
                                if (sneaking)
                                {
                                    throw new UnexpectedValueException("EntityActionPacket.ActionId");
                                }

                                sneaking = true;
                                break;
                            case 1:
                                /*System.Console.Write("Unseanking!");*/
                                if (!sneaking)
                                {
                                    throw new UnexpectedValueException("EntityActionPacket.ActionId");
                                }

                                sneaking = false;
                                break;
                            case 3:
                                if (sprinting)
                                {
                                    throw new UnexpectedValueException("EntityActionPacket.ActionId");
                                }

                                sprinting = true;
                                break;
                            case 4:
                                if (!sprinting)
                                {
                                    throw new UnexpectedValueException("EntityActionPacket.ActionId");
                                }

                                sprinting = false;
                                break;
                        }

                        if (packet.JumpBoost > 0)
                        {
                            throw new UnexpectedValueException("EntityActionPacket.JumpBoost");
                        }

                        controls.Enqueue(packet);

                    }
                    break;
            }

        }

        internal void Control(
            Queue<ServerboundPlayingPacket> controls,
            World world,
            System.Guid id,
            bool sneaking, bool sprinting,
            SelfInventory inv)
        {
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
                                id,
                                sneaking, sprinting,
                                inv);
                        }
                    }
                    catch (TryAgainException)
                    {

                    }

                    foreach (TeleportationRecord record in _TELEPORTATION_RECORDS.GetValues())
                    {
                        record.Update();
                    }

                    _keepAliveRecord.Update(_OUT_PACKETS);

                }
                catch (UnexpectedClientBehaviorExecption e)
                {
                    // TODO: send disconnected message to client.

                    System.Console.WriteLine(e.Message);

                    throw new DisconnectedClientException();
                }
                
            }
            catch (DisconnectedClientException)
            {
                buffer.Flush();

                _disconnected = true;
            }
            
        }

        private void LoadWorld(World world, Vector p)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_disconnected);

            System.Diagnostics.Debug.Assert(_dChunkRendering >= _MIN_RENDER_DISTANCE);
            System.Diagnostics.Debug.Assert(_dChunkRendering <= _MAX_RENDER_DISTANCE);
            System.Diagnostics.Debug.Assert(_dEntityRendering >= _MIN_RENDER_DISTANCE);
            System.Diagnostics.Debug.Assert(_dEntityRendering <= _MAX_ENTITY_RENDER_DISTANCE);

            ChunkLocation locCenter = ChunkLocation.Generate(p);

            ChunkGrid grid = ChunkGrid.Generate(locCenter, _dEntityRendering);
            foreach (ChunkLocation loc in grid.GetLocations())
            {
                foreach (Entity entity in world._ENTITY_CTX.GetEntities(loc))
                {
                    entity.ApplyRenderer(_ENTITY_RENDERER);
                }
            }

            using Queue<ChunkLocation> newChunkPositions = new();
            using Queue<ChunkLocation> outOfRangeChunks = new();

            _LOADING_HELPER.Load(newChunkPositions, outOfRangeChunks, locCenter, _dChunkRendering);

            int mask; byte[] data;
            while (!newChunkPositions.Empty)
            {
                ChunkLocation locChunk = newChunkPositions.Dequeue();

                (mask, data) = world._BLOCK_CTX.GetChunkData(locChunk);

                _CHUNK_LOAD_PACKETS.Enqueue(new LoadChunkPacket(
                    locChunk.X, locChunk.Z, true, mask, data));
            }

            while (!outOfRangeChunks.Empty)
            {
                ChunkLocation loc = outOfRangeChunks.Dequeue();

                _OUT_PACKETS.Enqueue(new UnloadChunkPacket(loc.X, loc.Z));
            }

        }
        
        private void SendPacket(Buffer buffer, ClientboundPlayingPacket packet)
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            System.Diagnostics.Debug.Assert(!_disconnected);

            packet.Write(buffer);
            _CLIENT.Send(buffer);

            System.Diagnostics.Debug.Assert(buffer.Empty);
        }

        private void SendPacket(ClientboundPlayingPacket packet)
        {
            using Buffer buffer = new();

            SendPacket(buffer, packet);
        }

        public void ApplyVelocity(int id, Vector v)
        {
            EntityVelocityPacket packet = new(
                id,
                (short)(v.X * 8000),
                (short)(v.Y * 8000),
                (short)(v.Z * 8000));

            SendPacket(packet);
        }

        internal void Render(
            World world, 
            int id,
            Vector p, Entity.Angles look, 
            SelfInventory inv)
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

                    _OUT_PACKETS.Enqueue(new TeleportSelfPlayerPacket(
                        p.X, p.Y, p.Z,
                        look.Yaw, look.Pitch,
                        false, false, false, false, false,
                        new System.Random().Next()));

                    _OUT_PACKETS.Enqueue(new SetPlayerAbilitiesPacket(
                            false, false, true, false, 0.1F, 0.0F));

                    _init = true;
                }

                LoadWorld(world, p);

                while (!_CHUNK_LOAD_PACKETS.Empty)
                {
                    LoadChunkPacket packet = _CHUNK_LOAD_PACKETS.Dequeue();
                    SendPacket(buffer, packet);

                    System.Diagnostics.Debug.Assert(buffer.Empty);
                }

                while (!_OUT_PACKETS.Empty)
                {
                    ClientboundPlayingPacket packet = _OUT_PACKETS.Dequeue();

                    if (packet is TeleportSelfPlayerPacket teleportPacket)
                    {
                        Vector pTeleport = new(teleportPacket.X, teleportPacket.Y, teleportPacket.Z);

                        TeleportationRecord report = new(teleportPacket.Payload, pTeleport);
                        _TELEPORTATION_RECORDS.Enqueue(report);
                    }
                    else if (packet is ClientboundCloseWindowPacket)
                    {
                        System.Diagnostics.Debug.Assert(_window != null);
                        _window.ResetWindowForcibly(inv, _OUT_PACKETS, false);
                    }

                    SendPacket(buffer, packet);

                    System.Diagnostics.Debug.Assert(buffer.Empty);
                }

                System.Diagnostics.Debug.Assert(_CHUNK_LOAD_PACKETS.Empty);
                System.Diagnostics.Debug.Assert(_OUT_PACKETS.Empty);
            }
            catch (DisconnectedClientException)
            {
                buffer.Flush();

                _disconnected = true;
            }

        }

        public void Flush(
            World world, 
            System.Guid userId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            System.Diagnostics.Debug.Assert(_disconnected);

            _ENTITY_RENDERER.Disconnect();

            _window.Flush(world);

            world._PLAYER_LIST.Disconnect(userId);
        }

        public void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);
            System.Diagnostics.Debug.Assert(_disconnected);

            // Release Resources.
            _CLIENT.Dispose();

            _CHUNK_LOAD_PACKETS.Dispose();
            _OUT_PACKETS.Dispose();

            _LOADING_HELPER.Dispose();
            _TELEPORTATION_RECORDS.Dispose();

            _window.Dispose();

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }
}

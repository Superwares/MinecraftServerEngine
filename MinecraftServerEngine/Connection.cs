using Common;
using Containers;
using Sync;

namespace MinecraftServerEngine
{
    using PhysicsEngine;

    internal sealed class Connection : System.IDisposable
    {
        private sealed class Window : System.IDisposable
        {
            private bool _disposed = false;

            private readonly Locker Locker = new();  // Disposable

            private readonly WindowRenderer Renderer;
            private readonly InventorySlot Cursor = new();

            private PublicInventory _invPublic = null;

            private bool _ambiguous = false;


            public Window(
                ConcurrentQueue<ClientboundPlayingPacket> outPackets,
                PlayerInventory invPlayer)
            {
                System.Diagnostics.Debug.Assert(outPackets != null);
                System.Diagnostics.Debug.Assert(invPlayer != null);

                Renderer = new WindowRenderer(outPackets, invPlayer, Cursor);
            }

            ~Window() => System.Diagnostics.Debug.Assert(false);

            internal bool Open(
                ConcurrentQueue<ClientboundPlayingPacket> outPackets,
                PlayerInventory invPrivate, PublicInventory invPublic)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(outPackets != null);
                System.Diagnostics.Debug.Assert(invPrivate != null);
                System.Diagnostics.Debug.Assert(invPublic != null);

                Locker.Hold();

                try
                {
                    if (_invPublic != null)
                    {
                        return false;
                    }

                    invPublic.Open(invPrivate, outPackets);
                    Renderer.Open(invPrivate, Cursor, invPublic.GetTotalSlotCount());

                    _invPublic = invPublic;

                    _ambiguous = true;

                    return true;
                }
                finally
                {
                    Locker.Release();
                }
            }

            internal void Reset(
                ConcurrentQueue<ClientboundPlayingPacket> outPackets,
                World world,
                int idWindow, PlayerInventory invPrivate)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(outPackets != null);
                System.Diagnostics.Debug.Assert(world != null);
                System.Diagnostics.Debug.Assert(invPrivate != null);

                Locker.Hold();

                try
                {
                    if (idWindow < 0 || idWindow > 1)
                    {
                        throw new UnexpectedValueException("ServerboundCloseWindowPacket.WindowId");
                    }

                    if (_invPublic == null)
                    {
                        System.Diagnostics.Debug.Assert(!_ambiguous);

                        if (idWindow == 0)
                        {
                            return;
                        }
                        else if (idWindow == 1)
                        {
                            throw new UnexpectedValueException("ServerboundCloseWindowPacket.WindowId");
                        }

                        System.Diagnostics.Debug.Assert(false);
                    }
                    else
                    {
                        if (idWindow == 0)
                        {
                            if (!_ambiguous)
                            {
                                throw new UnexpectedValueException("ServerboundCloseWindowPacket.WindowId");
                            }
                            else
                            {
                                return;
                            }
                        }

                        System.Diagnostics.Debug.Assert(idWindow == 1);
                    }

                    System.Diagnostics.Debug.Assert(idWindow == 1);

                    _invPublic.Close(invPrivate);

                    if (!Cursor.Empty)
                    {
                        throw new System.NotImplementedException();

                        // TODO: Drop item stack.
                    }
                    
                    Renderer.Reset(invPrivate, Cursor);
                    _invPublic = null;

                    _ambiguous = false;
                }
                finally
                {
                    Locker.Release();
                }
            }

            internal void Handle(
                UserId id,
                World world, AbstractPlayer player,
                PlayerInventory invPlayer,
                int mode, int button, int i)
            {
                System.Diagnostics.Debug.Assert(id != UserId.Null);
                System.Diagnostics.Debug.Assert(world != null);
                System.Diagnostics.Debug.Assert(player != null);
                System.Diagnostics.Debug.Assert(invPlayer != null);

                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(Renderer != null);
                System.Diagnostics.Debug.Assert(Cursor != null);
                System.Diagnostics.Debug.Assert(!_ambiguous);

                switch (mode)
                {
                    default:
                        throw new UnexpectedValueException("ClickWindowPacket.ModeNumber");
                    case 0:
                        if (i < 0)
                        {
                            break;
                        }
                        switch (button)
                        {
                            default:
                                throw new UnexpectedValueException("ClickWindowPacket.ButtonNumber");
                            case 0:
                                if (_invPublic == null)
                                {
                                    invPlayer.LeftClick(i, Cursor);
                                }
                                else
                                {
                                    _invPublic.LeftClick(id, invPlayer, i, Cursor);
                                }
                                break;
                            case 1:
                                if (_invPublic == null)
                                {
                                    invPlayer.RightClick(i, Cursor);
                                }
                                else
                                {
                                    _invPublic.RightClick(id, invPlayer, i, Cursor);
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
                                throw new UnexpectedValueException("ClickWindowPacket.ButtonNumber");
                            case 0:
                                if (_invPublic == null)
                                {
                                    invPlayer.QuickMove(i);
                                }
                                else
                                {
                                    _invPublic.QuickMove(id, invPlayer, i);
                                }
                                break;
                            case 1:
                                if (_invPublic == null)
                                {
                                    invPlayer.QuickMove(i);
                                }
                                else
                                {
                                    _invPublic.QuickMove(id, invPlayer, i);
                                }
                                break;
                        }
                        break;
                    case 2:
                        if (_invPublic == null)
                        {
                            invPlayer.SwapItemsWithHotbarSlot(i, button);
                        }
                        else
                        {
                            _invPublic.SwapItemsWithHotbarSlot(id, invPlayer, i, button);
                        }
                        break;
                    case 3:
                        //throw new System.NotImplementedException();
                        break;
                    case 4:
                        //throw new System.NotImplementedException();
                        break;
                    case 5:
                        //throw new System.NotImplementedException();
                        break;
                    case 6:
                        //throw new System.NotImplementedException();
                        break;
                }

                player.UpdateEntityEquipmentsData(invPlayer.GetEquipmentsData());

                int offset = _invPublic == null ? 0 : _invPublic.GetTotalSlotCount();
                System.Diagnostics.Debug.Assert(offset >= 0);
                Renderer.Update(invPlayer, Cursor, offset);

                {
                    if (_invPublic == null)
                    {
                        invPlayer.Print();
                    }

                    MyConsole.Debug($"Cursor: {Cursor}");
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

                    if (_invPublic == null)
                    {
                        if (i >= invPlayer.GetTotalSlotCount())
                        {
                            throw new UnexpectedClientBehaviorExecption("Slot index is out of the valid range for player inventory");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(_invPublic != null);

                        if (i >= _invPublic.GetTotalSlotCount() + PlayerInventory.PrimarySlotCount)
                        {
                            throw new UnexpectedClientBehaviorExecption("Slot index is out of the valid range for public and player primary inventory");
                        }
                    }

                    if (_invPublic == null)
                    {
                        System.Diagnostics.Debug.Assert(!_ambiguous);
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
                            if (_ambiguous == false)
                            {
                                throw new UnexpectedClientBehaviorExecption("Not ambiguous!");
                            }
                            else
                            {
                                return;
                            }
                        }

                        System.Diagnostics.Debug.Assert(idWindow == 1);
                    }

                    _ambiguous = false;

                    Handle(id, world, player, invPlayer, mode, button, i);

                }
                finally
                {
                    Locker.Release();
                }
            }

            public void Flush(World world, PlayerInventory invPlayer)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(world != null);
                System.Diagnostics.Debug.Assert(invPlayer != null);

                if (_invPublic != null)
                {
                    _invPublic.Close(invPlayer);
                    _invPublic = null;
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
                System.Diagnostics.Debug.Assert(_invPublic == null);
                System.Diagnostics.Debug.Assert(Cursor.Empty);

                // Release Resources.
                Locker.Dispose();

                System.GC.SuppressFinalize(this);
                _disposed = true;
            }

        }
        private sealed class ChunkingHelper : System.IDisposable
        {
            private const int MaxLoadCount = 7;
            private const int MaxSearchCount = 103;

            private bool _disposed = false;

            private readonly Set<ChunkLocation> LoadedChunks = new();  // Disposable

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

                        if (LoadedChunks.Contains(locPrev))
                        {
                            LoadedChunks.Extract(locPrev);
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

                        if (!LoadedChunks.Contains(locTarget))
                        {
                            newChunks.Enqueue(locTarget);
                            LoadedChunks.Insert(locTarget);
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

                System.Diagnostics.Debug.Assert(LoadedChunks.Count <= (d + d + 1) * (d + d + 1));
            }

            public void Dispose()
            {
                // Assertions.
                System.Diagnostics.Debug.Assert(!_disposed);

                // Release resources.
                LoadedChunks.Dispose();

                // Finish.
                System.GC.SuppressFinalize(this);
                _disposed = true;
            }

        }

        private sealed class TeleportRecord
        {
            private const long Timeout = 20 * 10;  // 10 seconds, 20 * 10 ticks

            public readonly int _payload;
            private long _ticks = 0;

            private readonly Vector _p;
            public Vector Position => _p;

            public TeleportRecord(int payload, Vector p)
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
                    renderer.Enqueue(new ClientboundKeepAlivePacket(payload));
                    _payload = payload;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(_ticks >= 0);
                }

                if (++_ticks > Timeout)
                {
                    throw new KeepAliveTimeoutException();
                }

            }

        }

        private bool _disposed = false;

        private readonly UserId Id;

        private readonly Client Client;  // Dispoasble

        

        private bool _disconnected = false;
        public bool Disconnected => _disconnected;


        private JoinGamePacket JoinGamePacket = null;
        private readonly Queue<LoadChunkPacket> LoadChunkPackets = new();  // Dispoasble
        private readonly ConcurrentQueue<ClientboundPlayingPacket> OutPackets = new();  // Dispoasble


        private const int MAxEntityRanderDistance = 7;
        private const int MinRenderDistance = 2, MaxRenderDistance = 32;
        private int _dEntityRendering = MinRenderDistance;
        private int _dChunkRendering = MinRenderDistance;


        private readonly EntityRenderer EntityRenderer;
        private readonly ParticleObjectRenderer ParticleObjectRenderer;


        private readonly ChunkingHelper ChunkingHelprt;  // Dispoasble
        private readonly Queue<TeleportRecord> TeleportRecords = new();  // Dispoasble
        private KeepAliveRecord _KeepAliveRecord = new();  // Disposable


        private bool _startDigging = false;
        private bool _attackWhenDigging = false;


        private readonly Window _Window;  // Disposable

        internal Connection(
            UserId id,
            Client client, 
            World world, 
            int idEntity,
            float health,
            Vector p, Angles look,
            PlayerInventory invPlayer,
            Gamemode gamemode)
        {
            System.Diagnostics.Debug.Assert(id != UserId.Null);
            System.Diagnostics.Debug.Assert(client != null);
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(invPlayer != null);

            Id = id;

            Client = client;

            System.Diagnostics.Debug.Assert(MAxEntityRanderDistance >= MinRenderDistance);
            System.Diagnostics.Debug.Assert(MaxRenderDistance >= MAxEntityRanderDistance);

            ChunkLocation loc = ChunkLocation.Generate(p);
            EntityRenderer = new EntityRenderer(OutPackets, loc, _dEntityRendering);
            ParticleObjectRenderer = new ParticleObjectRenderer(OutPackets, loc, _dEntityRendering);

            ChunkingHelprt = new ChunkingHelper(loc, _dChunkRendering);

            _Window = new Window(OutPackets, invPlayer);

            world.Connect(Id, OutPackets);

            System.Diagnostics.Debug.Assert(JoinGamePacket == null);
            JoinGamePacket = new JoinGamePacket(idEntity, 2, 0, 2, "default", false);

            int payload = Random.NextInt();
            OutPackets.Enqueue(new TeleportPacket(
                p.X, p.Y, p.Z,
                look.Yaw, look.Pitch,
                false, false, false, false, false,
                payload));

            TeleportRecord report = new(payload, p);
            TeleportRecords.Enqueue(report);

            UpdateHealth(health);
            Set(idEntity, gamemode);

        }

        ~Connection() => System.Diagnostics.Debug.Assert(false);

        private void RecvDataAndHandle(
            Buffer buffer, 
            World world, AbstractPlayer player, PlayerInventory invPlayer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(player != null);
            System.Diagnostics.Debug.Assert(invPlayer != null);

            if (_disconnected)
            {
                throw new DisconnectedClientException();
            }

            Client.Recv(buffer);

            int packetId = buffer.ReadInt(true);
            switch (packetId)
            {
                default:
                    MyConsole.Debug($"Received packet Id: 0x{packetId:X}");
                    /*throw new NotImplementedException();*/
                    buffer.Flush();
                    break;
                case ServerboundPlayingPacket.TeleportAcceptPacketd:
                    {
                        TeleportAcceptPacket packet = TeleportAcceptPacket.Read(buffer);

                        if (TeleportRecords.Empty)
                        {
                            throw new UnexpectedPacketException();
                        }

                        TeleportRecord record = TeleportRecords.Dequeue();
                        record.Confirm(packet.Payload);

                        Vector p = new(record.Position.X, record.Position.Y, record.Position.Z);
                        ChunkLocation locChunk = ChunkLocation.Generate(p);
                        EntityRenderer.Update(locChunk);
                        ParticleObjectRenderer.Update(locChunk);

                        if (player.Sneaking)
                        {
                            player.Unsneak(world);
                        }

                        if (player.Sprinting)
                        {
                            player.Unsprint(world);
                        }
                    }
                    break;
                case ServerboundPlayingPacket.SettingsPacketId:
                    {
                        SettingsPacket packet = SettingsPacket.Read(buffer);

                        int d = packet.RenderDistance;
                        
                        if (d < MinRenderDistance || d > MaxRenderDistance)
                        {
                            throw new UnexpectedValueException("SetClientSettingsPacket.RenderDistance");
                        }

                        _dChunkRendering = d;
                        _dEntityRendering = System.Math.Min(d, MAxEntityRanderDistance);

                        EntityRenderer.Update(_dEntityRendering);
                        ParticleObjectRenderer.Update(_dEntityRendering);
                    }
                    break;
                case ServerboundPlayingPacket.ServerboundConfirmTransactionPacketId:
                    {
                        ServerboundConfirmTransactionPacket packet =
                            ServerboundConfirmTransactionPacket.Read(buffer);

                        /*Console.Printl(
                            $"WindowId: {packet.WindowId}, " +  
                            $"ActionNumber: {packet.ActionNumber}, " +
                            $"Accepted: {packet.Accepted}, ");*/

                        throw new UnexpectedPacketException();
                    }
                case ServerboundPlayingPacket.ClickWindowPacketId:
                    {
                        ClickWindowPacket packet = ClickWindowPacket.Read(buffer);

                        {
                            MyConsole.NewLine();
                            MyConsole.Printl(
                                $"WindowId: {packet.WindowId}, " +
                                $"SlotNumber: {packet.Slot}, " +
                                $"ButtonNumber: {packet.Button}, " +
                                $"ActionNumber: {packet.Action}, " +
                                $"ModeNumber: {packet.Mode}");
                        }

                        System.Diagnostics.Debug.Assert(_Window != null);
                        _Window.Handle(
                            Id, world, player, invPlayer,
                            packet.WindowId, packet.Mode, packet.Button, packet.Slot);

                        OutPackets.Enqueue(new ClientboundConfirmTransactionPacket(
                                (sbyte)packet.WindowId, packet.Action, true));
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

                        System.Diagnostics.Debug.Assert(_Window != null);
                        _Window.Reset(OutPackets, world, packet.WindowId, invPlayer);
                    }
                    break;
                case ServerboundPlayingPacket.ServerboundCustomPayloadPacketId:
                    {
                        buffer.Flush();
                    }
                    break;
                case ServerboundPlayingPacket.UseEntityPacketId:
                    {
                        var packet = UseEntityPacket.Read(buffer);

                        /*Console.Printl("UseEntityPacket!");
                        Console.Printl($"\tEntityId: {packet.EntityId}");
                        Console.Printl($"\tType: {packet.Type}");
                        Console.Printl($"\tHand: {packet.Hand}");*/

                        if (packet.Type == 2 && packet.Hand == 0)
                        {
                            /*Console.Printl("UseEntity!");*/

                            int id = packet.EntityId;

                            if (!world.EntitiesById.Contains(id))
                            {
                                throw new UnexpectedValueException("UseEntityPacket.EntityId");
                            }

                            player.OnUseEntity(world, world.EntitiesById.Lookup(id));
                        }
                    }
                    break;
                case ServerboundPlayingPacket.ServerboundKeepAlivePacketId:
                    {
                        ServerboundKeepAlivePacket packet = ServerboundKeepAlivePacket.Read(buffer);

                        long ticks = _KeepAliveRecord.Confirm(packet.Payload);
                        world.PlayerList.UpdateLaytency(Id, ticks);
                    }
                    break;
                case ServerboundPlayingPacket.PlayerPacketId:
                    {
                        PlayerPacket packet = PlayerPacket.Read(buffer);

                        if (!TeleportRecords.Empty)
                        {
                            break;
                        }

                        /*player.ControlStanding(packet.OnGround);*/
                    }
                    break;
                case ServerboundPlayingPacket.PlayerPositionPacketId:
                    {
                        PlayerPositionPacket packet = PlayerPositionPacket.Read(buffer);
                
                        if (!TeleportRecords.Empty)
                        {
                            break;
                        }

                        Vector p = new(packet.X, packet.Y, packet.Z);

                        player.ControlMovement(p);
                        /*player.ControlStanding(packet.OnGround);*/

                        ChunkLocation locChunk = ChunkLocation.Generate(p);
                        EntityRenderer.Update(locChunk);
                        ParticleObjectRenderer.Update(locChunk);

                    }
                    break;
                case ServerboundPlayingPacket.PlayerPosAndLookPacketId:
                    {
                        PlayerPosAndLookPacket packet = PlayerPosAndLookPacket.Read(buffer);

                        if (!TeleportRecords.Empty)
                        {
                            break;
                        }

                        Vector p = new(packet.X, packet.Y, packet.Z);
                        Angles look = new(packet.Yaw, packet.Pitch);

                        player.ControlMovement(p);
                        player.Rotate(look);
                        /*player.ControlStanding(packet.OnGround);*/

                        ChunkLocation locChunk = ChunkLocation.Generate(p);
                        EntityRenderer.Update(locChunk);
                        ParticleObjectRenderer.Update(locChunk);
                    }
                    break;
                case ServerboundPlayingPacket.PlayerLookPacketId:
                    {
                        PlayerLookPacket packet = PlayerLookPacket.Read(buffer);

                        if (!TeleportRecords.Empty)
                        {
                            break;
                        }

                        Angles look = new(packet.Yaw, packet.Pitch);

                        player.Rotate(look);
                        /*player.ControlStanding(packet.OnGround);*/
                    }
                    break;
                case ServerboundPlayingPacket.PlayerDigPacketId:
                    {
                        throw new UnexpectedPacketException();

                        var packet = PlayerDigPacket.Read(buffer);

                        /*Console.Printl("PlayerDigPacket!");
                        Console.Printl($"\tStatus: {packet.Status}");*/
                        switch (packet.Status)
                        {
                            default:
                                throw new System.NotImplementedException();
                            case 0:  // Started digging
                                if (_startDigging)
                                {
                                    throw new UnexpectedValueException("PlayerDigPacket.Status");
                                }

                                _startDigging = true;
                                System.Diagnostics.Debug.Assert(!_attackWhenDigging);
                                break;
                            case 1:  // Cancelled digging

                                _startDigging = false;
                                _attackWhenDigging = false;
                                break;
                            case 2:  // Finished digging

                                // TODO: Send Block Change Packet, 0x0B.

                                _startDigging = false;
                                _attackWhenDigging = false;
                                break;
                        }
                    }
                    break;
                case ServerboundPlayingPacket.EntityActionPacketId:
                    {
                        EntityActionPacket packet = EntityActionPacket.Read(buffer);

                        if (!TeleportRecords.Empty)
                        {
                            break;
                        }

                        switch (packet.ActionId)
                        {
                            default:
                                /*Console.Printl($"ActionId: {packet.ActionId}");*/
                                throw new UnexpectedValueException("EntityAction.ActoinId");
                            case 0:
                                /*Console.Print("Seanking!");*/
                                if (player.Sneaking)
                                {
                                    throw new UnexpectedValueException("EntityActionPacket.ActionId");
                                }

                                player.Sneak(world);

                                break;
                            case 1:
                                /*Console.Print("Unseanking!");*/
                                if (!player.Sneaking)
                                {
                                    throw new UnexpectedValueException("EntityActionPacket.ActionId");
                                }

                                player.Unsneak(world);

                                break;
                            case 3:
                                if (player.Sprinting)
                                {
                                    throw new UnexpectedValueException("EntityActionPacket.ActionId");
                                }

                                player.Sprint(world);

                                break;
                            case 4:
                                if (!player.Sprinting)
                                {
                                    throw new UnexpectedValueException("EntityActionPacket.ActionId");
                                }

                                player.Unsprint(world);

                                break;
                        }

                        if (packet.JumpBoost > 0)
                        {
                            throw new UnexpectedValueException("EntityActionPacket.JumpBoost");
                        }

                    }
                    break;
                case ServerboundPlayingPacket.ServerboundHeldItemSlotPacketId:
                    {
                        var packet = ServerboundHeldItemSlotPacket.Read(buffer);

                        if (packet.Slot < 0 || packet.Slot >= PlayerInventory.HotbarSlotCount)
                        {
                            throw new UnexpectedValueException("ServerboundHeldItemSlotPacket.Slot");
                        }

                        invPlayer.ChangeMainHand(packet.Slot);
                        player.UpdateEntityEquipmentsData(invPlayer.GetEquipmentsData());
                    }
                    break;
                case ServerboundPlayingPacket.AnimationPacketId:
                    {
                        var packet = AnimationPacket.Read(buffer);

                        /*Console.Printl("AnimationPacket!");
                        Console.Printl($"\tHand: {packet.Hand}");*/

                        if (packet.Hand == 0)
                        {
                            if (_startDigging)
                            {
                                if (!_attackWhenDigging)
                                {
                                    // Attack!
                                    /*Console.Printl("Attack!");*/

                                    ItemStack stack = invPlayer.GetMainHandSlot().Stack;

                                    if (stack != null)
                                    {
                                        player.OnAttack(world, stack);
                                    }
                                    else
                                    {
                                        player.OnAttack(world);
                                    }

                                    _attackWhenDigging = true;
                                }
                            }
                            else
                            {
                                // Attack!
                                /*Console.Printl("Attack!");*/

                                ItemStack stack = invPlayer.GetMainHandSlot().Stack;

                                if (stack != null)
                                {
                                    player.OnAttack(world, stack);
                                }
                                else
                                {
                                    player.OnAttack(world);
                                }
                            }
                        }
                        else if (packet.Hand == 1)  // offhand
                        {
                            // TOOD: Check it is correct behavior.
                            throw new UnexpectedValueException("AnimationPacket.Hand");
                        }
                        else
                        {
                            throw new UnexpectedValueException("AnimationPacket.Hand");
                        }
                        
                    }
                    break;
                case ServerboundPlayingPacket.BlockPlacementPacketId:
                    {
                        buffer.Flush();
                    }
                    break;
                case ServerboundPlayingPacket.UseItemPacketId:
                    {
                        var packet = UseItemPacket.Read(buffer);

                        /*Console.Printl("UseItemPacket!");
                        Console.Printl($"\tHand: {packet.Hand}");*/

                        if (packet.Hand == 0)
                        {
                            /*Console.Printl("UseItem!");*/

                            ItemStack stack = invPlayer.GetMainHandSlot().Stack;

                            player.OnUseItem(world, stack);
                        }
                        else if (packet.Hand == 1)
                        {
                            /*Console.Printl("UseItem!");*/

                            ItemStack stack = invPlayer.GetOffHandSlot().Stack;

                            player.OnUseItem(world, stack);
                        }
                        else
                        {
                            throw new UnexpectedValueException("UseItemPacket.Hand");
                        }
                        
                    }
                    break;
                case 0x03:
                    /*Console.Printl("Respawn!");
                    OutPackets.Enqueue(new RespawnPacket(
                        0, 2, 2, "default"));*/

                    /*player.Teleport(new Vector(2.0D, 110.0D, 2.0D), new Look(30.0F, 20.0F));*/

                    buffer.Flush();
                    break;
            }

        }

        long ticks = 0;

        internal void Control(
            World world, 
            AbstractPlayer player, PlayerInventory invPlayer)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(player != null);
            System.Diagnostics.Debug.Assert(invPlayer != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            /*if (serverTicks == 200)  // 10 seconds
            {
                System.Diagnostics.Debug.Assert(_window != null);
                _window.OpenWindowWithPublicInventory(
                    _OUT_PACKETS,
                    player._selfInventory,
                    world._Inventory);
            }*/

            /*{
                using Buffer buffer2 = new();

                ParticlesPacket packet2 = new(
                    30, true,
                    0.0F, 102.0F, 0.0F,
                    0.001F, 0.999F, 0.999F,
                    1.0F,
                    0);

                SendPacket(buffer2, packet2);
            }*/

            ++ticks;

            /*if (ticks == 20)
            {
                using Buffer buffer2 = new();

                UpdateHealthPacket packet = new(0.00001F, 20, 0.0F);
                SendPacket(buffer2, packet);
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
                                buffer, 
                                world, player, invPlayer);
                        }
                    }
                    catch (TryAgainException)
                    {

                    }

                    foreach (TeleportRecord record in TeleportRecords.GetValues())
                    {
                        record.Update();
                    }

                    _KeepAliveRecord.Update(OutPackets);

                }
                catch (UnexpectedClientBehaviorExecption e)
                {
                    // TODO: send disconnected message to client.

                    MyConsole.Printl($"UnexpectedClientBehavior: {e.Message}!");

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

        private void LoadWorld(int idEntitySelf, World world, Vector p)
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
            using Tree<PhysicsObject> objs = new();
            world.SearchObjects(objs, aabbTotal);
            /*Console.Printl($"count: {objs.Count}");*/
            foreach (PhysicsObject obj in objs.GetKeys())
            {
                switch (obj)
                {
                    default:
                        throw new System.NotImplementedException();
                    case ParticleObject particleObj:
                        particleObj.ApplyRenderer(ParticleObjectRenderer);
                        break;
                    case Entity entity:
                        if (entity.Id != idEntitySelf)
                        {
                            entity.ApplyRenderer(EntityRenderer);
                        }
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
            System.Diagnostics.Debug.Assert(buffer != null);
            System.Diagnostics.Debug.Assert(packet != null);

            System.Diagnostics.Debug.Assert(!_disposed);
            System.Diagnostics.Debug.Assert(!_disconnected);

            packet.Write(buffer);
            Client.Send(buffer);

            System.Diagnostics.Debug.Assert(buffer.Empty);
        }

        internal void ApplyVelocity(int id, Vector v)
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

        internal void Teleport(Vector p, Angles look)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (_disconnected)
            {
                return;
            }

            int payload = Random.NextInt();
            TeleportPacket packet = new(
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

                    TeleportRecord report = new(payload, p);
                    TeleportRecords.Enqueue(report);
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

        internal void UpdateHealth(float health)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (_disconnected)
            {
                return;
            }

            OutPackets.Enqueue(new UpdateHealthPacket(health, 20, 5.0F));
        }

        internal void Set(int idEntity, Gamemode gamemode)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (_disconnected)
            {
                return;
            }

            bool canFly = false;

            using EntityMetadata metadata = new();

            if (gamemode == Gamemode.Spectator)
            {   
                metadata.AddByte(0, 0x20);

                canFly = true;
            }
            else
            {
                metadata.AddByte(0, 0x00);
            }

            OutPackets.Enqueue(new EntityMetadataPacket(idEntity, metadata.WriteData()));
            OutPackets.Enqueue(new AbilitiesPacket(
                    false, canFly, canFly, false, 0.1F, 0.0F));
        }

        internal bool Open(PlayerInventory invPri, PublicInventory invPub)
        {
            System.Diagnostics.Debug.Assert(invPri != null);
            System.Diagnostics.Debug.Assert(invPub != null);

            if (_disconnected)
            {
                return false;
            }

            return _Window.Open(OutPackets, invPri, invPub);
        }

        internal void LoadAndSendData(
            World world, 
            int idEntitySelf,
            Vector p, Angles look)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(!_disposed);
            System.Diagnostics.Debug.Assert(!_disconnected);

            using Buffer buffer = new();

            try
            {
                if (JoinGamePacket != null)
                {

                    // The difficulty must be normal,
                    // because the hunger bar increases on its own in easy difficulty.
                    SendPacket(buffer, JoinGamePacket);

                    JoinGamePacket = null;
                }

                System.Diagnostics.Debug.Assert(JoinGamePacket == null);

                LoadWorld(idEntitySelf, world, p);

                while (!LoadChunkPackets.Empty)
                {
                    LoadChunkPacket packet = LoadChunkPackets.Dequeue();
                    SendPacket(buffer, packet);

                    System.Diagnostics.Debug.Assert(buffer.Empty);
                }

                /*Console.Printl($"Packets Length: {OutPackets.Count}");*/
                while (!OutPackets.Empty)
                {
                    ClientboundPlayingPacket packet = OutPackets.Dequeue();
                    System.Diagnostics.Debug.Assert(packet != null);

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

        internal void Flush(
            out UserId id,
            World world, 
            PlayerInventory invPlayer)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(invPlayer != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_disconnected);

            System.Diagnostics.Debug.Assert(Id != UserId.Null);
            id = Id;

            EntityRenderer.Disconnect();
            ParticleObjectRenderer.Disconnect();

            _Window.Flush(world, invPlayer);

            world.Disconnect(Id);
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
            TeleportRecords.Dispose();

            _Window.Dispose();

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }
}

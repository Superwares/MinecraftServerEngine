using Common;
using Containers;
using Sync;

namespace MinecraftServerEngine
{
    using PhysicsEngine;

    internal sealed class Connection : System.IDisposable
    {

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

            ~ChunkingHelper()
            {
                System.Diagnostics.Debug.Assert(false);

                //Dispose(false);
            }

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

            private readonly static Time CHECK_INTERVAL = Time.FromSeconds(5);

            private long _payload;
            private long _ticks = -1;

            private Time _startTime = Time.Now();

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

                _startTime = Time.Now();

                return ticks;
            }

            public void Update(ConcurrentQueue<ClientboundPlayingPacket> renderer)
            {
                Time endTime = Time.Now();
                Time intervalTime = endTime - _startTime;

                if (intervalTime < CHECK_INTERVAL)
                {
                    return;
                }

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


        internal readonly Window Window;  // Disposable

        internal Connection(
            UserId id,
            Client client,
            World world,
            int idEntity,
            float health,
            Vector p, Angles look,
            PlayerInventory playerInventory,
            Gamemode gamemode)
        {
            System.Diagnostics.Debug.Assert(id != UserId.Null);
            System.Diagnostics.Debug.Assert(client != null);
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(playerInventory != null);

            Id = id;

            Client = client;

            System.Diagnostics.Debug.Assert(MAxEntityRanderDistance >= MinRenderDistance);
            System.Diagnostics.Debug.Assert(MaxRenderDistance >= MAxEntityRanderDistance);

            ChunkLocation loc = ChunkLocation.Generate(p);
            EntityRenderer = new EntityRenderer(OutPackets, loc, _dEntityRendering);
            ParticleObjectRenderer = new ParticleObjectRenderer(OutPackets, loc, _dEntityRendering);

            ChunkingHelprt = new ChunkingHelper(loc, _dChunkRendering);

            Window = new Window(OutPackets, playerInventory);

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

        ~Connection()
        {
            System.Diagnostics.Debug.Assert(false);

            //Dispose(false);
        }

        private string HandleCommandLineText(string text)
        {
            if (text == null || string.IsNullOrEmpty(text))
            {
                return null;
            }

            string[] args = text.Split(' ', System.StringSplitOptions.TrimEntries | System.StringSplitOptions.RemoveEmptyEntries);

            if (args.Length == 0)
            {
                return null;
            }

            string command = args[0];

            switch (command)
            {
                default:
                    return $"Error: Unknown command '{command}'!";
                case "teleport":
                case "tp":
                    {
                        const string usage =
"""
Usage:
/teleport <x> <y> <z>
    Teleports the command issuer (you) to the specified coordinates <x>, <y>, and <z>.

/teleport <x> <y> <z> <player's username>
    Teleports the specified player to the coordinates <x>, <y>, and <z>.

/teleport <from player's username> <to player's username>
    Teleports the player specified as <from player's username> to the location of the player specified as <to player's username>.
""";

                        if (args.Length == 4)
                        {
                            if (float.TryParse(args[1], out float x) &&
                                float.TryParse(args[2], out float y) &&
                                float.TryParse(args[3], out float z))
                            {
                                throw new System.NotImplementedException();
                            }
                            else
                            {
                                return $"Error: Invalid arguments!\n {usage}";
                            }
                        }
                        else if (args.Length == 5)
                        {
                            if (float.TryParse(args[1], out float x) &&
                                float.TryParse(args[2], out float y) &&
                                float.TryParse(args[3], out float z) &&
                                args[4] != null && string.IsNullOrEmpty(args[4]))
                            {
                                string username = args[4];

                                throw new System.NotImplementedException();
                            }
                            else
                            {
                                return $"Error: Invalid arguments!\n {usage}";
                            }
                        }
                        else if (args.Length == 3)
                        {
                            if (args[1] != null && string.IsNullOrEmpty(args[1]) &&
                                args[2] != null && string.IsNullOrEmpty(args[2]))
                            {
                                string fromUsername = args[1];
                                string toUsername = args[2];

                                throw new System.NotImplementedException();
                            }
                            else
                            {
                                return $"Error: Invalid arguments!\n {usage}";
                            }
                        }
                        else
                        {
                            return $"Error: Invalid arguments!\n {usage}";
                        }
                    }

                    return null;
                case "gamemode":
                case "gm":
                    throw new System.NotImplementedException();
            }
        }

        private void RecvDataAndHandle(
            Buffer buffer,
            World world, AbstractPlayer player, PlayerInventory playerInventory)
        {
            System.Diagnostics.Debug.Assert(buffer != null);
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(player != null);
            System.Diagnostics.Debug.Assert(playerInventory != null);

            if (_disconnected)
            {
                throw new DisconnectedClientException();
            }

            Client.Recv(buffer);

            int packetId = buffer.ReadInt(true);

            //MyConsole.Debug($"Received packet Id: 0x{packetId:X}");

            switch (packetId)
            {
                default:

                    /*throw new NotImplementedException();*/
                    buffer.Flush();
                    break;
                case ServerboundPlayingPacket.TeleportAcceptPacketId:
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
                case ServerboundPlayingPacket.ServerboundChatMessagePacketId:
                    {
                        ServerboundChatMessagePacket packet = ServerboundChatMessagePacket.Read(buffer);

                        string text = packet.Text;
                        if (text.StartsWith('/') == true)
                        {
                            text = text.Substring(1);
                            HandleCommandLineText(text);
                        }
                        else
                        {
                            MyConsole.Warn("Handling of normal chat messages is not implemented yet...");
                        }

                        throw new System.NotImplementedException();
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

                        //double x = 10.0D, y = 0.0D, z = 10.0D;

                        //OutPackets.Enqueue(new NamedSoundEffectPacket(
                        //    "entity.player.attack.strong",
                        //    7,
                        //    (int)(x * 8), (int)(y * 8), (int)(z * 8),
                        //    0.5F,
                        //    2.0F));

                        {
                            MyConsole.NewLine();
                            MyConsole.Printl(
                                $"WindowId: {packet.WindowId}, " +
                                $"SlotNumber: {packet.Slot}, " +
                                $"ButtonNumber: {packet.Button}, " +
                                $"ActionNumber: {packet.Action}, " +
                                $"ModeNumber: {packet.Mode}");
                        }

                        System.Diagnostics.Debug.Assert(Window != null);
                        Window.Handle(
                            Id, world, player, playerInventory,
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

                        System.Diagnostics.Debug.Assert(Window != null);
                        Window.Reset(OutPackets, world, player, packet.WindowId, playerInventory);
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

                            if (world.EntitiesById.Contains(id) == false)
                            {
                                throw new UnexpectedValueException("Invalid entity Id");
                            }

                            // TODO: Check the relationship between the curent player and entitty physically.

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

                        playerInventory.ChangeMainHand(packet.Slot);
                        player.UpdateEntityEquipmentsData(playerInventory.GetEquipmentsData());
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

                                    ItemStack stack = playerInventory.GetMainHandSlot().Stack;

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

                                ItemStack stack = playerInventory.GetMainHandSlot().Stack;

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

                        Vector eyeOrigin = player.GetEyeOrigin();
                        Vector d = player.Look.GetUnitVector();
                        Vector scaled_d = d * player.GetEyeHeight();

                        PhysicsObject obj = world.SearchClosestObject(eyeOrigin, scaled_d, player);

                        if (obj is ItemEntity itemEntity)
                        {
                            itemEntity.PickUp(player);
                        }
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

                            ItemStack stack = playerInventory.GetMainHandSlot().Stack;

                            player.OnUseItem(world, stack);
                        }
                        else if (packet.Hand == 1)
                        {
                            /*Console.Printl("UseItem!");*/

                            ItemStack stack = playerInventory.GetOffHandSlot().Stack;

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

        internal bool Open(PlayerInventory invPri, SharedInventory invPub)
        {
            System.Diagnostics.Debug.Assert(invPri != null);
            System.Diagnostics.Debug.Assert(invPub != null);

            if (_disconnected == true)
            {
                return false;
            }

            return Window.Open(Id, OutPackets, invPri, invPub);
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

            Window.Flush(world, invPlayer);

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

            Window.Dispose();

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }
}

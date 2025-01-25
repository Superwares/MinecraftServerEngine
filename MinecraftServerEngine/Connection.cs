using Common;
using Containers;
using Sync;

using MinecraftPrimitives;

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

        private readonly MinecraftClient Client;  // Dispoasble



        private bool _disconnected = false;
        public bool Disconnected => _disconnected;


        private JoinGamePacket JoinGamePacket = null;
        private readonly Queue<LoadChunkPacket> LoadChunkPackets = new();  // Dispoasble
        internal readonly ConcurrentQueue<ClientboundPlayingPacket> OutPackets = new();  // Dispoasble


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
            MinecraftClient client,
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
            
            OutPackets.Enqueue(new EntityPropertiesPacket(
                idEntity,
                [
                    ("generic.attackSpeed", 4.0F),   // 5 ticks, 0.25 seconds
                ]));
        }

        ~Connection()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        private string HandleCommandLineText(
            string text,
            World world, AbstractPlayer player)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(player != null);

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
                    return $"Error: Unknown command \"{command}\"!";
                case "item-types":
                    {
                        string output = "Item types: [\n";

                        foreach (ItemType item in System.Enum.GetValues(typeof(ItemType)))
                        {
                            output += $"{(int)item}: {item},\n";
                        }

                        output += "]";
                        return output;
                    }
                case "blocks":
                    {
                        string output = "Blocks: [\n";

                        foreach (Block item in System.Enum.GetValues(typeof(Block)))
                        {
                            output += $"{(int)item}: {item},\n";
                        }

                        output += "]";
                        return output;
                    }
                case "gamemodes":
                    {
                        string output = "Gamemodes: [\n";

                        foreach (Gamemode item in System.Enum.GetValues(typeof(Gamemode)))
                        {
                            output += $"{(int)item}: {item},\n";
                        }

                        output += "]";
                        return output;
                    }
                case "teleport":
                case "tp":
                    {
                        const string usage = "\n" +
"Usage:\n" +
"\n" +
"/teleport <x> <y> <z> <yaw> <pitch> \n" +
"\n" +
"* Teleports the command issuer (you) to the specified coordinates <x>, <y>, <z>, <yaw>, and <pitch>. \n" +
"\n" +
"/teleport <x> <y> <z> <yaw> <pitch> <username> \n" +
"\n" +
"* Teleports the specified player to the coordinates <x>, <y>, <z>, <yaw>, and <pitch>. \n" +
"\n" +
"/teleport <from username> <to username> \n" +
"\n" +
"* Teleports the player specified as <from username> to the location of the player specified as <to username>. \n";
                        if (args.Length == 6)
                        {
                            if (Vector.TryParse(args[1], args[2], args[3], out Vector v) == true &&
                                Angles.TryParse(args[4], args[5], out Angles angles) == true)
                            {
                                player.Teleport(v, angles);
                            }
                            else
                            {
                                return $"Error: Invalid arguments!\n {usage}";
                            }
                        }
                        else if (args.Length == 7)
                        {
                            if (Vector.TryParse(args[1], args[2], args[3], out Vector v) == true &&
                                Angles.TryParse(args[4], args[5], out Angles angles) == true &&
                                args[6] != null && string.IsNullOrEmpty(args[6]) == false)
                            {
                                string username = args[6];

                                try
                                {
                                    AbstractPlayer targetPlayer = world.PlayersByUsername.Lookup(username);
                                    targetPlayer.Teleport(v, angles);
                                }
                                catch (KeyNotFoundException)
                                {
                                    return $"Error: Player \"{username}\" not found!\n {usage}";
                                }

                            }
                            else
                            {
                                return $"Error: Invalid arguments!\n {usage}";
                            }
                        }
                        else if (args.Length == 3)
                        {
                            if (args[1] != null && string.IsNullOrEmpty(args[1]) == false &&
                                args[2] != null && string.IsNullOrEmpty(args[2]) == false)
                            {
                                string fromUsername = args[1];
                                string toUsername = args[2];

                                try
                                {
                                    AbstractPlayer fromPlayer = world.PlayersByUsername.Lookup(fromUsername);
                                    AbstractPlayer toPlayer = world.PlayersByUsername.Lookup(toUsername);
                                    fromPlayer.Teleport(toPlayer.Position, toPlayer.Look);
                                }
                                catch (KeyNotFoundException)
                                {
                                    return $"Error: Player \"{fromUsername}\" or \"{toUsername}\" not found!\n {usage}";
                                }

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

                    break;
                case "gamemode":
                case "gm":
                    {
                        const string usage = "\n" +
"Usage:\n" +
"\n" +
"/gamemode <Adventure|Spectator> \n" +
"\n" +
"* Changes the game mode of the command issuer (you) to the specified mode.\n" +
"- Adventure: Sets your game mode to adventure mode, where you can interact with objects but cannot break or place blocks.\n" +
"- Spectator: Sets your game mode to spectator mode, where you can fly around and observe the world without interacting with it.\n";

                        if (args.Length == 2)
                        {
                            if (
                                System.Enum.TryParse(args[1], out Gamemode gamemode) == true &&
                                System.Enum.IsDefined(typeof(Gamemode), gamemode) == true)
                            {
                                player.SwitchGamemode(gamemode);
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
                    break;
                case "give":
                    {
                        const string usage = "\n" +
"Usage:\n" +
"\n" +
"/give <item-type> <name> <amount> [username] \n" +
"\n" +
"* Gives the specified item to the command issuer (you) or to another player if a username is specified.\n" +
"- <item-type>: The name of the item type you want to receive.\n" +
"  Example: 'DiamondSword', 'Stick', 'Snowball'.\n" +
"- <name>: An optional custom name for the item.\n" +
"  Example: 'Excalibur', 'Magic Wand'.\n" +
"- <amount>: The number of items to give.\n" +
"  Example: 1, 32, 64.\n" +
"  Note: Each item has a predefined minimum and maximum amount. If the specified amount is outside this range, it will be adjusted to the nearest valid value.\n" +
"- [username]: An optional username of the player to receive the item. If specified, the item will be given to the specified player instead of you.\n";

                        if (args.Length >= 4)
                        {
                            if (
                                System.Enum.TryParse(args[1], out ItemType itemType) == true &&
                                System.Enum.IsDefined(typeof(ItemType), itemType) == true &&
                                args[2] != null && string.IsNullOrEmpty(args[2]) == false &&
                                int.TryParse(args[3], out int amount) == true)
                            {
                                string name = args[2];

                                if (amount > itemType.GetMaxStackCount())
                                {
                                    amount = itemType.GetMaxStackCount();
                                }

                                if (amount < itemType.GetMinStackCount())
                                {
                                    amount = itemType.GetMinStackCount();
                                }

                                string username = args.Length >= 5 ? args[4] : null;

                                try
                                {
                                    if (username == null)
                                    {
                                        player.GiveItem(new ItemStack(itemType, name, amount));
                                    }
                                    else
                                    {
                                        AbstractPlayer targetPlayer = world.PlayersByUsername.Lookup(username);
                                        targetPlayer.GiveItem(new ItemStack(itemType, name, amount));
                                    }

                                }
                                catch (KeyNotFoundException)
                                {
                                    return $"Error: Player \"{username}\" not found!\n {usage}";
                                }
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
                    break;
            }

            return null;
        }

        private void RecvDataAndHandle(
            MinecraftDataStream buffer,
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

                        string text = packet.Text, output = null;
                        if (text.StartsWith('/') == true)
                        {
                            text = text.Substring(1);
                            output = HandleCommandLineText(text, world, player).TrimEnd('\n');
                        }
                        else
                        {
                            MyConsole.Warn("Handling of normal chat messages is not implemented yet...");
                        }

                        if (output != null)
                        {
                            var data = new
                            {
                                text = output,
                            };

                            string jsonString = System.Text.Json.JsonSerializer.Serialize(data);

                            OutPackets.Enqueue(new ClientboundChatmessagePacket(
                                jsonString, 0));
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

                        //{
                        //    MyConsole.NewLine();
                        //    MyConsole.Printl(
                        //        $"WindowId: {packet.WindowId}, " +
                        //        $"SlotNumber: {packet.Slot}, " +
                        //        $"ButtonNumber: {packet.Button}, " +
                        //        $"ActionNumber: {packet.Action}, " +
                        //        $"ModeNumber: {packet.Mode}");
                        //}

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
                    throw new UnexpectedPacketException();
                //{

                //    var packet = PlayerDigPacket.Read(buffer);

                //    Console.Printl("PlayerDigPacket!");
                //    Console.Printl($"\tStatus: {packet.Status}");
                //    switch (packet.Status)
                //    {
                //        default:
                //            throw new System.NotImplementedException();
                //        case 0:  // Started digging
                //            if (_startDigging)
                //            {
                //                throw new UnexpectedValueException("PlayerDigPacket.Status");
                //            }

                //            _startDigging = true;
                //            System.Diagnostics.Debug.Assert(!_attackWhenDigging);
                //            break;
                //        case 1:  // Cancelled digging

                //            _startDigging = false;
                //            _attackWhenDigging = false;
                //            break;
                //        case 2:  // Finished digging

                //            // TODO: Send Block Change Packet, 0x0B.

                //            _startDigging = false;
                //            _attackWhenDigging = false;
                //            break;
                //    }
                //}
                //break;
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
                case ServerboundPlayingPacket.ServerboundAnimationPacketId:
                    {
                        var packet = ServerboundAnimationPacket.Read(buffer);

                        /*Console.Printl("AnimationPacket!");
                        Console.Printl($"\tHand: {packet.Hand}");*/

                        if (packet.Hand == 0)
                        {
                            ItemStack mainHandItemStack = null;
                            byte[] mainHandItemStackHash = null;

                            if (_startDigging)
                            {
                                if (!_attackWhenDigging)
                                {
                                    // Attack!
                                    /*Console.Printl("Attack!");*/

                                    /**
                                     * Handle breakable of item before attacks. 
                                     * Because this item can be broken by another places (ex. Inventory interface).
                                     */
                                    mainHandItemStack = Window.HandleMainHandSlot(playerInventory);

                                    if (mainHandItemStack != null)
                                    {
                                        mainHandItemStackHash = mainHandItemStack.Hash;
                                        player._Attack(world, mainHandItemStack);
                                    }
                                    else
                                    {
                                        player._Attack(world);
                                    }

                                    _attackWhenDigging = true;
                                }
                            }
                            else
                            {
                                // Attack!
                                /*Console.Printl("Attack!");*/

                                /**
                                 * Handle breakable of item before attacks. 
                                 * Because this item can be broken by another places (ex. Inventory interface).
                                 */
                                mainHandItemStack = Window.HandleMainHandSlot(playerInventory);

                                if (mainHandItemStack != null)
                                {
                                    mainHandItemStackHash = mainHandItemStack.Hash;
                                    player._Attack(world, mainHandItemStack);
                                }
                                else
                                {
                                    player._Attack(world);
                                }
                            }

                            if (
                                mainHandItemStack != null &&
                                mainHandItemStack.IsBreaked == true)
                            {

                                Window.UpdateMainHandSlot(playerInventory);

                                player._ItemBreak(world, mainHandItemStack);

                                player.UpdateEntityEquipmentsData(playerInventory.GetEquipmentsData());

                                //MyConsole.Debug("Item break!");
                            }
                            else if (
                                 mainHandItemStack != null &&
                                 mainHandItemStackHash != null &&
                                 mainHandItemStack.CheckHash(mainHandItemStackHash) == false)
                            {
                                MyConsole.Debug("Different status of prev and current item!");
                                Window.UpdateMainHandSlot(playerInventory);

                                player.UpdateEntityEquipmentsData(playerInventory.GetEquipmentsData());
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

            using MinecraftDataStream buffer = new();

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
        private void SendPacket(MinecraftDataStream buffer, ClientboundPlayingPacket packet)
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

            using MinecraftDataStream buffer = new();

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

            using MinecraftDataStream buffer = new();

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

            if (_disconnected == true)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(OutPackets != null);
            OutPackets.Enqueue(new UpdateHealthPacket(health, 20, 5.0F));
        }

        internal void Animate(int entityId, EntityAnimation animation)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            if (_disconnected == true)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(OutPackets != null);
            OutPackets.Enqueue(new ClientboundAnimationPacket(entityId, (byte)animation));
        }

        internal void AddEffect(
            int entityId, byte effectId, byte amplifier, int duration, byte flags)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);
            if (_disconnected == true)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(OutPackets != null);
            OutPackets.Enqueue(new EntityEffectPacket(entityId, effectId, amplifier, duration, flags));
        }

        internal void PlaySound(
            string name, int category, Vector p, float volume, float pitch)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);
            if (_disconnected == true)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(name != null);
            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(name) == false);
            System.Diagnostics.Debug.Assert(volume >= 0.0F);
            System.Diagnostics.Debug.Assert(volume <= 1.0F);
            System.Diagnostics.Debug.Assert(pitch >= 0.5F);
            System.Diagnostics.Debug.Assert(pitch <= 2.0F);

            System.Diagnostics.Debug.Assert(OutPackets != null);
            OutPackets.Enqueue(new NamedSoundEffectPacket(
                //"entity.player.attack.strong", 7,
                name, category,
                (int)(p.X * 8), (int)(p.Y * 8), (int)(p.Z * 8),
                volume, pitch));
        }

        internal void EmitParticles(
            Particle particle, Vector v,
            float speed, int count,
            float r, float g, float b)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);
            if (_disconnected == true)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(OutPackets != null);
            OutPackets.Enqueue(new ParticlesPacket(
                (int)particle, true,
                (float)v.X, (float)v.Y, (float)v.Z,
                r, g, b,
                speed, count));
        }

        internal void SetExperience(float ratio, int level)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(ratio >= 0 && ratio <= 1);
            System.Diagnostics.Debug.Assert(level >= 0);


            if (_disconnected == true)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(OutPackets != null);
            OutPackets.Enqueue(new SetExperiencePacket(ratio, level, 0));
        }

        internal void Set(int idEntity, Gamemode gamemode)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (_disconnected == true)
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

            System.Diagnostics.Debug.Assert(OutPackets != null);
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

            using MinecraftDataStream buffer = new();

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
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed == false)
            {
                System.Diagnostics.Debug.Assert(_disconnected == true);

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing == true)
                {
                    // Dispose managed resources.
                    Client.Dispose();

                    LoadChunkPackets.Dispose();
                    OutPackets.Dispose();

                    ChunkingHelprt.Dispose();
                    TeleportRecords.Dispose();

                    Window.Dispose();
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

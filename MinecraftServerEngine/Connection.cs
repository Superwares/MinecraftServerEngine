﻿using Common;
using Containers;
using Sync;


namespace MinecraftServerEngine
{
    using Blocks;
    using Inventories;
    using Entities;
    using Protocols;
    using Items;
    using Renderers;
    using Physics;
    using Physics.BoundingVolumes;
    using Particles;
    using ShapeObjects;

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
                System.Diagnostics.Debug.Assert(_disposed == false);

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
                System.Diagnostics.Debug.Assert(_disposed == false);

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



        private readonly UserId UserId;

        private readonly MinecraftClient Client;  // Dispoasble



        private bool _disconnected = false;
        internal bool IsDisconnected => _disconnected;


        private LoginSuccessPacket _LoginSuccessPacket = null;
        private JoinGamePacket _JoinGamePacket = null;
        private readonly Queue<LoadChunkPacket> LoadChunkPackets = new();  // Dispoasble
        internal readonly ConcurrentQueue<ClientboundPlayingPacket> OutPackets = new();  // Dispoasble


        private const int MAxEntityRanderDistance = 7;
        private const int MinRenderDistance = 2, MaxRenderDistance = 32;
        private int _dEntityRendering = MinRenderDistance;
        private int _dChunkRendering = MinRenderDistance;


        private readonly EntityRenderer _EntityRenderer;
        private readonly ParticleObjectRenderer _ParticleObjectRenderer;
        private readonly ShapeObjectRenderer _ShapeObjectRenderer;


        private readonly ChunkingHelper _ChunkingHelprt;  // Dispoasble
        private readonly Queue<TeleportRecord> _TeleportRecords = new();  // Dispoasble
        private KeepAliveRecord _KeepAliveRecord = new();  // Disposable


        private bool _startDigging = false;
        private bool _attackWhenDigging = false;


        internal readonly Window Window;  // Disposable



        internal Connection(
            UserId userId, string username,
            MinecraftClient client,
            World world,
            int entityId,
            double additionalHealth, double maxHealth, double health,
            double movementSpeed,
            Vector p, EntityAngles look,
            bool blindness,
            PlayerInventory playerInventory,
            Gamemode gamemode)
        {
            System.Diagnostics.Debug.Assert(userId != UserId.Null);
            System.Diagnostics.Debug.Assert(username != null);
            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(username) == false);
            System.Diagnostics.Debug.Assert(client != null);
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(playerInventory != null);

            UserId = userId;

            Client = client;

            System.Diagnostics.Debug.Assert(MAxEntityRanderDistance >= MinRenderDistance);
            System.Diagnostics.Debug.Assert(MaxRenderDistance >= MAxEntityRanderDistance);

            ChunkLocation loc = ChunkLocation.Generate(p);
            _EntityRenderer = new EntityRenderer(OutPackets, loc, _dEntityRendering, blindness);
            _ParticleObjectRenderer = new ParticleObjectRenderer(OutPackets, loc, _dEntityRendering);
            _ShapeObjectRenderer = new ShapeObjectRenderer(OutPackets, loc, _dEntityRendering);

            _ChunkingHelprt = new ChunkingHelper(loc, _dChunkRendering);

            Window = new Window(OutPackets, playerInventory);

            world.Connect(UserId, OutPackets);

            System.Diagnostics.Debug.Assert(userId != UserId.Null);
            System.Diagnostics.Debug.Assert(username != null);
            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(username) == false);
            System.Diagnostics.Debug.Assert(_LoginSuccessPacket == null);
            _LoginSuccessPacket = new LoginSuccessPacket(userId.Value, username);

            System.Diagnostics.Debug.Assert(_JoinGamePacket == null);
            _JoinGamePacket = new JoinGamePacket(entityId, 2, 0, 2, "default", false);

            int payload = Random.NextInt();
            OutPackets.Enqueue(new TeleportPacket(
                p.X, p.Y, p.Z,
                (float)look.Yaw, (float)look.Pitch,
                false, false, false, false, false,
                payload));

            TeleportRecord report = new(payload, p);
            _TeleportRecords.Enqueue(report);

            System.Diagnostics.Debug.Assert(additionalHealth >= 0.0);
            UpdateAdditionalHealth(entityId, additionalHealth);

            System.Diagnostics.Debug.Assert(maxHealth >= health);
            System.Diagnostics.Debug.Assert(maxHealth > 0.0);
            System.Diagnostics.Debug.Assert(health >= 0.0);
            UpdateMaxHealth(entityId, maxHealth);
            UpdateHealth(health);

            UpdateMovementSpeed(entityId, movementSpeed);

            SetGamemode(entityId, gamemode);

            {
                OutPackets.Enqueue(new EntityPropertiesPacket(
                entityId,
                [
                    ("generic.attackSpeed", 4.0),   // 5 ticks, 0.25 seconds
                    //("generic.attackSpeed", 1.0),   // 20 ticks, 1 seconds

                    //("generic.movementSpeed", 0.699999988079071),  // default movement speed
                ]));
            }

            {
                using MinecraftProtocolDataStream stream = new();

                using EntityMetadata metadata = new();

                metadata.AddByte(13, 0xFF);  // The Displayed Skin Parts bit mask
                metadata.WriteData(stream);

                byte[] data = stream.ReadData();

                OutPackets.Enqueue(new EntityMetadataPacket(entityId, data));
            }

            //{
            //    using MinecraftProtocolDataStream s = new();

            //    ItemStack itemStack = new ItemStack(ItemType.RedstoneOre, "Hello");

            //    itemStack.WriteData(s);

            //    OutPackets.Enqueue(new EntityEquipmentPacket(idEntity, 5, s.ReadData()));
            //}

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
                //case "item-types":
                //    {
                //        string output = "Item types: [\n";

                //        foreach (ItemType item in System.Enum.GetValues(typeof(ItemType)))
                //        {
                //            output += $"{(int)item}: {item},\n";
                //        }

                //        output += "]";
                //        return output;
                //    }
                //case "blocks":
                //    {
                //        string output = "Blocks: [\n";

                //        foreach (Block item in System.Enum.GetValues(typeof(Block)))
                //        {
                //            output += $"{(int)item}: {item},\n";
                //        }

                //        output += "]";
                //        return output;
                //    }
                //case "gamemodes":
                //    {
                //        string output = "Gamemodes: [\n";

                //        foreach (Gamemode item in System.Enum.GetValues(typeof(Gamemode)))
                //        {
                //            output += $"{(int)item}: {item},\n";
                //        }

                //        output += "]";
                //        return output;
                //    }
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
                                EntityAngles.TryParse(args[4], args[5], out EntityAngles angles) == true)
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
                                EntityAngles.TryParse(args[4], args[5], out EntityAngles angles) == true &&
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

                                if (amount > itemType.GetMaxCount())
                                {
                                    amount = itemType.GetMaxCount();
                                }

                                if (amount < Item.MinCount)
                                {
                                    amount = Item.MinCount;
                                }

                                string username = args.Length >= 5 ? args[4] : null;

                                try
                                {
                                    ItemStack itemStack = new(itemType, name, amount);

                                    if (username == null)
                                    {
                                        player.GiveItemStack(ref itemStack);
                                    }
                                    else
                                    {
                                        AbstractPlayer targetPlayer = world.PlayersByUsername.Lookup(username);
                                        targetPlayer.GiveItemStack(ref itemStack);
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
                case "world-time":
                case "wt":
                    {
                        const string usage = "\n" +
"Usage:\n" +
"\n" +
"/world-time next <ticks> <transition seconds> \n" +
"\n" +
"/world-time add <ticks> <transition seconds>\n";

                        if (args.Length == 4)
                        {
                            if (args[1] == "next")
                            {
                                if (
                                    int.TryParse(args[2], out int ticks) == true &&
                                    int.TryParse(args[3], out int seconds) == true
                                    )
                                {
                                    Time worldTime = MinecraftTimes.TimePerTick * ticks;
                                    Time transitionTime = Time.FromSeconds(seconds);

                                    System.Diagnostics.Debug.Assert(world != null);
                                    world.ChangeWorldTimeToNextDay(worldTime, transitionTime);
                                }
                                else
                                {
                                    return $"Error: Invalid arguments!\n {usage}";
                                }
                            }
                            else if (args[1] == "add")
                            {
                                if (
                                    int.TryParse(args[2], out int ticks) == true &&
                                    int.TryParse(args[3], out int seconds) == true
                                    )
                                {
                                    Time addingWorldTime = MinecraftTimes.TimePerTick * ticks;
                                    Time transitionTime = Time.FromSeconds(seconds);

                                    System.Diagnostics.Debug.Assert(world != null);
                                    world.AddTimeToWorld(addingWorldTime, transitionTime);
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
                        else
                        {
                            return $"Error: Invalid arguments!\n {usage}";
                        }
                    }
                    break;
                case "world-border":
                case "wb":
                    {
                        const string usage = "\n" +
"Usage:\n" +
"\n" +
"/world-border <radius meters> <transition ms per meter> \n";

                        if (args.Length == 3)
                        {
                            if (
                                    double.TryParse(args[1], out double radiusInMeters) == true &&
                                    int.TryParse(args[2], out int transitionMsPerMeter) == true &&
                                    transitionMsPerMeter >= 0
                                    )
                            {
                                System.Diagnostics.Debug.Assert(transitionMsPerMeter >= 0);
                                Time transitionTimePerMeter = Time.FromMilliseconds(transitionMsPerMeter);

                                System.Diagnostics.Debug.Assert(world != null);
                                world.ChangeWorldBorderSize(radiusInMeters, transitionTimePerMeter);
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
            MinecraftProtocolDataStream buffer,
            World world, AbstractPlayer player)
        {
            System.Diagnostics.Debug.Assert(buffer != null);
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(player != null);

            if (_disconnected == true)
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

                        if (_TeleportRecords.Empty)
                        {
                            throw new UnexpectedPacketException();
                        }

                        TeleportRecord record = _TeleportRecords.Dequeue();
                        record.Confirm(packet.Payload);

                        Vector p = new(record.Position.X, record.Position.Y, record.Position.Z);
                        ChunkLocation locChunk = ChunkLocation.Generate(p);
                        _EntityRenderer.Update(locChunk);
                        _ParticleObjectRenderer.Update(locChunk);
                        _ShapeObjectRenderer.Update(locChunk);

                        //if (player.Sneaking)
                        //{
                        //    player.Unsneak(world);
                        //}

                        //if (player.Sprinting)
                        //{
                        //    player.Unsprint(world);
                        //}
                    }
                    break;
                case ServerboundPlayingPacket.ServerboundChatMessagePacketId:
                    {
                        ServerboundChatMessagePacket packet = ServerboundChatMessagePacket.Read(buffer);

                        string text = packet.Text, output = null;
                        if (text.StartsWith('/') == true)
                        {
                            text = text.Substring(1);
                            output = HandleCommandLineText(text, world, player);
                        }
                        else
                        {
                            MyConsole.Warn("Handling of normal chat messages is not implemented yet...");
                        }

                        if (output != null)
                        {
                            //var data = new
                            //{
                            //    text = output.TrimEnd('\n'),
                            //};

                            //string jsonString = System.Text.Json.JsonSerializer.Serialize(data);

                            string jsonString = $"\"{{\"text\":\"{output.TrimEnd('\n')}\"}}\"";

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

                        _EntityRenderer.Update(_dEntityRendering);
                        _ParticleObjectRenderer.Update(_dEntityRendering);
                        _ShapeObjectRenderer.Update(_dEntityRendering);
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
                            UserId, world, player, player.Inventory,
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
                        Window.Reset(world, player, packet.WindowId, player.Inventory);
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
                        world.PlayerList.UpdateLaytency(UserId, ticks);
                    }
                    break;
                case ServerboundPlayingPacket.PlayerPacketId:
                    {
                        PlayerPacket packet = PlayerPacket.Read(buffer);

                        if (!_TeleportRecords.Empty)
                        {
                            break;
                        }

                        /*player.ControlStanding(packet.OnGround);*/
                    }
                    break;
                case ServerboundPlayingPacket.PlayerPositionPacketId:
                    {
                        PlayerPositionPacket packet = PlayerPositionPacket.Read(buffer);

                        if (!_TeleportRecords.Empty)
                        {
                            break;
                        }

                        Vector p = new(packet.X, packet.Y, packet.Z);

                        player.ControlMovement(p);
                        /*player.ControlStanding(packet.OnGround);*/

                        ChunkLocation locChunk = ChunkLocation.Generate(p);
                        _EntityRenderer.Update(locChunk);
                        _ParticleObjectRenderer.Update(locChunk);
                        _ShapeObjectRenderer.Update(locChunk);
                    }
                    break;
                case ServerboundPlayingPacket.PlayerPosAndLookPacketId:
                    {
                        PlayerPosAndLookPacket packet = PlayerPosAndLookPacket.Read(buffer);

                        if (!_TeleportRecords.Empty)
                        {
                            break;
                        }

                        Vector p = new(packet.X, packet.Y, packet.Z);
                        EntityAngles look = new(packet.Yaw, packet.Pitch);

                        player.ControlMovement(p);
                        player.Rotate(look);
                        /*player.ControlStanding(packet.OnGround);*/

                        ChunkLocation locChunk = ChunkLocation.Generate(p);
                        _EntityRenderer.Update(locChunk);
                        _ParticleObjectRenderer.Update(locChunk);
                        _ShapeObjectRenderer.Update(locChunk);
                    }
                    break;
                case ServerboundPlayingPacket.PlayerLookPacketId:
                    {
                        PlayerLookPacket packet = PlayerLookPacket.Read(buffer);

                        if (!_TeleportRecords.Empty)
                        {
                            break;
                        }

                        EntityAngles look = new(packet.Yaw, packet.Pitch);

                        player.Rotate(look);
                        /*player.ControlStanding(packet.OnGround);*/
                    }
                    break;
                case ServerboundPlayingPacket.PlayerDigPacketId:
                    {
                        PlayerDigPacket packet = PlayerDigPacket.Read(buffer);

                        //MyConsole.Debug("PlayerDigPacket!");
                        //MyConsole.Debug($"\tStatus: {packet.Status}");
                        switch (packet.Status)
                        {
                            default:
                                break;
                            case 0:  // Started digging
                                if (_startDigging == true)
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
                            case 6:
                                System.Diagnostics.Debug.Assert(player != null);
                                player.OnPressHandSwapButton(world);
                                break;
                        }
                    }
                    break;
                case ServerboundPlayingPacket.EntityActionPacketId:
                    {
                        EntityActionPacket packet = EntityActionPacket.Read(buffer);

                        //if (!_TeleportRecords.Empty)
                        //{
                        //    break;
                        //}

                        switch (packet.ActionId)
                        {
                            default:
                                /*Console.Printl($"ActionId: {packet.ActionId}");*/
                                throw new UnexpectedValueException($"Entity action id ({packet.ActionId})");
                            case 0:
                                if (player.IsSneaking == true)
                                {
                                    throw new UnexpectedValueException($"Entity action id ({packet.ActionId})");
                                }

                                player.Sneak(world);
                                break;
                            case 1:
                                if (player.IsSneaking == false)
                                {
                                    throw new UnexpectedValueException($"Entity action id ({packet.ActionId})");
                                }

                                player.Unsneak(world);
                                break;
                            case 3:
                                if (player.IsSprinting == true)
                                {
                                    throw new UnexpectedValueException($"Entity action id ({packet.ActionId})");
                                }

                                player.Sprint(world);
                                break;
                            case 4:
                                if (player.IsSprinting == false)
                                {
                                    throw new UnexpectedValueException($"Entity action id ({packet.ActionId})");
                                }

                                player.Unsprint(world);
                                break;
                        }

                        if (packet.JumpBoost > 0)
                        {
                            throw new UnexpectedValueException($"Invalid jump boost: {packet.JumpBoost}");
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

                        player.Inventory.ChangeActiveMainHandIndex(packet.Slot);
                        player.UpdateEquipmentsData();
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
                                    mainHandItemStack = Window.HandleMainHandSlot(player.Inventory);

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
                                mainHandItemStack = Window.HandleMainHandSlot(player.Inventory);

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

                                Window.UpdateMainHandSlot(player.Inventory);

                                player._ItemBreak(world, mainHandItemStack);

                                player.UpdateEquipmentsData();

                                //MyConsole.Debug("Item break!");
                            }
                            else if (
                                 mainHandItemStack != null &&
                                 mainHandItemStackHash != null &&
                                 mainHandItemStack.CheckHash(mainHandItemStackHash) == false)
                            {
                                //MyConsole.Debug("Different status of prev and current item!");
                                Window.UpdateMainHandSlot(player.Inventory);

                                player.UpdateEquipmentsData();
                            }


                        }
                        else if (packet.Hand == 1)  // offhand
                        {
                            // TOOD: Check it is correct behavior.
                            buffer.Flush();
                            //throw new UnexpectedValueException("AnimationPacket.Hand");
                        }
                        else
                        {
                            throw new UnexpectedValueException("Invalid hand animation");
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

                        ItemStack mainHandItemStack = null;
                        byte[] mainHandItemStackHash = null;

                        if (packet.Hand == 0)  // main hand
                        {
                            /*Console.Printl("UseItem!");*/

                            mainHandItemStack = player.Inventory.GetMainHandSlot().Stack;

                            if (mainHandItemStack != null)
                            {
                                mainHandItemStackHash = mainHandItemStack.Hash;
                                player.OnUseItem(world, mainHandItemStack);
                            }
                        }
                        else if (packet.Hand == 1)  // off hand
                        {
                            /*Console.Printl("UseItem!");*/

                            mainHandItemStack = player.Inventory.GetOffHandSlot().Stack;

                            if (mainHandItemStack != null)
                            {
                                mainHandItemStackHash = mainHandItemStack.Hash;
                                player.OnUseItem(world, mainHandItemStack);
                            }
                        }
                        else
                        {
                            throw new UnexpectedValueException("UseItemPacket.Hand");
                        }

                        if (
                            mainHandItemStack != null &&
                            mainHandItemStack.IsBreaked == true
                            )
                        {

                            Window.UpdateMainHandSlot(player.Inventory);

                            player._ItemBreak(world, mainHandItemStack);

                            player.UpdateEquipmentsData();

                            //MyConsole.Debug("Item break!");
                        }
                        else if (
                             mainHandItemStack != null &&
                             mainHandItemStackHash != null &&
                             mainHandItemStack.CheckHash(mainHandItemStackHash) == false
                             )
                        {
                            //MyConsole.Debug("Different status of prev and current item!");
                            Window.UpdateMainHandSlot(player.Inventory);

                            player.UpdateEquipmentsData();
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

        internal void Control(
            World world,
            AbstractPlayer player)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(player != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            using MinecraftProtocolDataStream buffer = new();

            try
            {
                try
                {
                    try
                    {
                        while (true)
                        {
                            RecvDataAndHandle(buffer, world, player);
                        }
                    }
                    catch (TryAgainException)
                    {

                    }

                    foreach (TeleportRecord record in _TeleportRecords.GetValues())
                    {
                        record.Update();
                    }

                    _KeepAliveRecord.Update(OutPackets);

                }
                catch (UnexpectedClientBehaviorExecption e)
                {
                    // TODO: send disconnected message to client.

                    MyConsole.Warn($"UnexpectedClientBehavior: {e.Message}!");

                    throw new DisconnectedClientException();
                }

            }
            catch (DisconnectedClientException)
            {
                System.Diagnostics.Debug.Assert(buffer != null);
                buffer.Flush();

                _disconnected = true;
                /*Console.Print("Disconnect!");*/
            }

        }

        internal void LoadWorld(int selfEntityId, World world, Vector p, bool blindness)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_disconnected == false);

            System.Diagnostics.Debug.Assert(_dChunkRendering >= MinRenderDistance);
            System.Diagnostics.Debug.Assert(_dChunkRendering <= MaxRenderDistance);
            System.Diagnostics.Debug.Assert(_dEntityRendering >= MinRenderDistance);
            System.Diagnostics.Debug.Assert(_dEntityRendering <= MAxEntityRanderDistance);

            ChunkLocation loc = ChunkLocation.Generate(p);

            if (blindness == false)
            {

                /*Console.Printl($"_dEntityRendering: {_dEntityRendering}");*/
                ChunkGrid grid = ChunkGrid.Generate(loc, _dEntityRendering);


                AxisAlignedBoundingBox aabbTotal = grid.GetMinBoundingBox();
                using Tree<PhysicsObject> objs = new();
                world.SearchObjects(objs, aabbTotal);

                //if (idEntitySelf == 0)
                //{
                //MyConsole.Debug($"grid: {grid}");
                //MyConsole.Debug($"aabbTotal: {aabbTotal}");
                //MyConsole.Debug($"idEntitySelf: {idEntitySelf}, count: {objs.Count}");
                //}

                foreach (PhysicsObject obj in objs.GetKeys())
                {
                    System.Diagnostics.Debug.Assert(obj != null);
                    switch (obj)
                    {
                        default:
                            throw new System.NotImplementedException();
                        case Entity entity:
                            if (entity.Id != selfEntityId)
                            {
                                entity.ApplyRenderer(_EntityRenderer);
                            }
                            break;
                        case ParticleObject particleObj:
                            particleObj.ApplyRenderer(_ParticleObjectRenderer);
                            break;
                        case ShapeObject shapeObj:
                            shapeObj.ApplyRenderer(_ShapeObjectRenderer);
                            break;

                    }
                }
            }


            using Queue<ChunkLocation> newChunkPositions = new();
            using Queue<ChunkLocation> outOfRangeChunks = new();

            System.Diagnostics.Debug.Assert(_ChunkingHelprt != null);
            _ChunkingHelprt.Load(newChunkPositions, outOfRangeChunks, loc, _dChunkRendering);

            int mask; byte[] data;
            while (newChunkPositions.Empty == false)
            {
                ChunkLocation locChunk = newChunkPositions.Dequeue();

                (mask, data) = world.BlockContext.GetChunkData(locChunk);

                LoadChunkPackets.Enqueue(new LoadChunkPacket(
                    locChunk.X, locChunk.Z, true, mask, data));
            }

            while (outOfRangeChunks.Empty == false)
            {
                ChunkLocation locOut = outOfRangeChunks.Dequeue();

                OutPackets.Enqueue(new UnloadChunkPacket(locOut.X, locOut.Z));
            }

        }


        internal void ApplyVelocity(int id, Vector v)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            if (_disconnected == true)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(v.X <= MinecraftPhysics.MaxVelocity);
            System.Diagnostics.Debug.Assert(v.Y <= MinecraftPhysics.MaxVelocity);
            System.Diagnostics.Debug.Assert(v.Z <= MinecraftPhysics.MaxVelocity);
            System.Diagnostics.Debug.Assert(v.X >= MinecraftPhysics.MinVelocity);
            System.Diagnostics.Debug.Assert(v.Y >= MinecraftPhysics.MinVelocity);
            System.Diagnostics.Debug.Assert(v.Z >= MinecraftPhysics.MinVelocity);

            EntityVelocityPacket packet = new(
                id,
                (short)(v.X * MinecraftPhysics.VelocityScaleFactorForClient),
                (short)(v.Y * MinecraftPhysics.VelocityScaleFactorForClient),
                (short)(v.Z * MinecraftPhysics.VelocityScaleFactorForClient));

            using MinecraftProtocolDataStream buffer = new();

            bool tryAgain;

            int hit = 0;

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

                System.Diagnostics.Debug.Assert(hit++ < 100);

            } while (tryAgain == true);

            System.Diagnostics.Debug.Assert(buffer.Empty);
        }

        internal void ApplyBilndness(bool f)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            _EntityRenderer.ApplyBlindness(f);
        }

        internal void Teleport(Vector p, EntityAngles look)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            if (_disconnected)
            {
                return;
            }

            int payload = Random.NextInt();
            TeleportPacket packet = new(
                p.X, p.Y, p.Z, (float)look.Yaw, (float)look.Pitch,
                false, false, false, false, false,
                payload);

            using MinecraftProtocolDataStream buffer = new();

            bool tryAgain;

            do
            {
                tryAgain = false;

                try
                {
                    SendPacket(buffer, packet);

                    TeleportRecord report = new(payload, p);
                    _TeleportRecords.Enqueue(report);
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

        internal void UpdateAdditionalHealth(int entityId, double health)
        {
            System.Diagnostics.Debug.Assert(health >= 0.0);

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (_disconnected == true)
            {
                return;
            }

            using MinecraftProtocolDataStream stream = new();

            {
                using EntityMetadata metadata = new();

                //MyConsole.Debug($"UpdateAdditionalHealth's health: {health}");
                metadata.AddFloat(11, (float)health);
                metadata.WriteData(stream);
            }

            byte[] data = stream.ReadData();

            System.Diagnostics.Debug.Assert(OutPackets != null);
            OutPackets.Enqueue(new EntityMetadataPacket(entityId, data));
        }

        internal void UpdateMaxHealth(int entityId, double maxHealth)
        {
            System.Diagnostics.Debug.Assert(maxHealth > 0.0);

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (_disconnected == true)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(OutPackets != null);
            OutPackets.Enqueue(new EntityPropertiesPacket(
                entityId,
                [
                    ("generic.maxHealth", maxHealth),
                ]));
        }

        internal void UpdateHealth(double health)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            if (_disconnected == true)
            {
                return;
            }

            health += 0.00001;

            System.Diagnostics.Debug.Assert(OutPackets != null);
            OutPackets.Enqueue(new UpdateHealthPacket((float)health, 20, 5.0F));
        }

        internal void UpdateMovementSpeed(int entityId, double amount)
        {
            System.Diagnostics.Debug.Assert(amount >= 0.0);
            System.Diagnostics.Debug.Assert(amount <= 1024.0);

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (_disconnected == true)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(OutPackets != null);
            OutPackets.Enqueue(new EntityPropertiesPacket(
                entityId,
                [
                    ("generic.movementSpeed", amount),
                ]));
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
            string name, int category, 
            Vector p, 
            double volume, double pitch)
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
                (float)volume, (float)pitch));
        }

        internal void EmitParticles(
            Particle particle, Vector v,
            double extra, int count,
            double offsetX, double offsetY, double offsetZ)
        {
            System.Diagnostics.Debug.Assert(offsetX >= 0.0D);
            System.Diagnostics.Debug.Assert(offsetX <= 1.0D);
            System.Diagnostics.Debug.Assert(offsetY >= 0.0D);
            System.Diagnostics.Debug.Assert(offsetY <= 1.0D);
            System.Diagnostics.Debug.Assert(offsetZ >= 0.0D);
            System.Diagnostics.Debug.Assert(offsetZ <= 1.0D);
            System.Diagnostics.Debug.Assert(extra >= 0.0);
            System.Diagnostics.Debug.Assert(extra <= 1.0);
            System.Diagnostics.Debug.Assert(count >= 0);

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (_disconnected == true)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(OutPackets != null);
            OutPackets.Enqueue(new ParticlesPacket(
                (int)particle, true,
                (float)v.X, (float)v.Y, (float)v.Z,
                (float)offsetX, (float)offsetY, (float)offsetZ,
                (float)extra, count));
        }

        internal void SetExperience(double ratio, int level)
        {
            System.Diagnostics.Debug.Assert(ratio >= 0 && ratio <= 1);
            System.Diagnostics.Debug.Assert(level >= 0);

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (_disconnected == true)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(OutPackets != null);
            OutPackets.Enqueue(new SetExperiencePacket((float)ratio, level, 0));
        }

        internal void SetGamemode(int entityId, Gamemode gamemode)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            if (_disconnected == true)
            {
                return;
            }

            bool canFly = false;

            using MinecraftProtocolDataStream stream = new();
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

            metadata.WriteData(stream);

            byte[] metadataData = stream.ReadData();

            System.Diagnostics.Debug.Assert(OutPackets != null);
            OutPackets.Enqueue(new EntityMetadataPacket(entityId, metadataData));
            OutPackets.Enqueue(new AbilitiesPacket(
                    false, canFly, canFly, false, 0.1F, 0.0F));
        }

        internal bool Open(PlayerInventory playerInventory, SharedInventory sharedInventory)
        {
            System.Diagnostics.Debug.Assert(playerInventory != null);
            System.Diagnostics.Debug.Assert(sharedInventory != null);

            if (_disconnected == true)
            {
                return false;
            }

            return Window.Open(UserId, OutPackets, playerInventory, sharedInventory);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="packet"></param>
        /// <exception cref="DisconnectedClientException"></exception>
        /// <exception cref="TryAgainException"></exception>
        private void SendPacket(MinecraftProtocolDataStream buffer, ClientboundLoginPacket packet)
        {
            System.Diagnostics.Debug.Assert(buffer != null);
            System.Diagnostics.Debug.Assert(packet != null);

            System.Diagnostics.Debug.Assert(_disposed == false);
            System.Diagnostics.Debug.Assert(!_disconnected);

            packet.Write(buffer);
            Client.Send(buffer);

            System.Diagnostics.Debug.Assert(buffer.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="packet"></param>
        /// <exception cref="DisconnectedClientException"></exception>
        /// <exception cref="TryAgainException"></exception>
        private void SendPacket(MinecraftProtocolDataStream buffer, ClientboundPlayingPacket packet)
        {
            System.Diagnostics.Debug.Assert(buffer != null);
            System.Diagnostics.Debug.Assert(packet != null);

            System.Diagnostics.Debug.Assert(_disposed == false);
            System.Diagnostics.Debug.Assert(!_disconnected);

            packet.Write(buffer);
            Client.Send(buffer);

            System.Diagnostics.Debug.Assert(buffer.Empty);
        }


        internal void SendData()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_disconnected == false);

            using MinecraftProtocolDataStream buffer = new();

            try
            {
                if (_LoginSuccessPacket != null)
                {
                    try
                    {
                        SendPacket(buffer, _LoginSuccessPacket);
                    }
                    finally
                    {
                        _LoginSuccessPacket = null;
                    }

                }

                if (_JoinGamePacket != null)
                {
                    try
                    {
                        // The difficulty must be normal,
                        // because the hunger bar increases on its own in easy difficulty.
                        SendPacket(buffer, _JoinGamePacket);
                    }
                    finally
                    {
                        _JoinGamePacket = null;
                    }

                }

                System.Diagnostics.Debug.Assert(_LoginSuccessPacket == null);
                System.Diagnostics.Debug.Assert(_JoinGamePacket == null);

                //LoadWorld(idEntitySelf, world, p, blindness);

                while (LoadChunkPackets.Empty == false)
                {
                    LoadChunkPacket packet = LoadChunkPackets.Dequeue();
                    SendPacket(buffer, packet);

                    System.Diagnostics.Debug.Assert(buffer.Empty);
                }

                /*Console.Printl($"Packets Length: {OutPackets.Count}");*/
                while (OutPackets.Empty == false)
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
            catch (TryAgainException)
            { }

        }

        internal void Flush(
            out UserId id,
            World world,
            PlayerInventory invPlayer)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(invPlayer != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_disconnected);

            System.Diagnostics.Debug.Assert(UserId != UserId.Null);
            id = UserId;

            System.Diagnostics.Debug.Assert(_EntityRenderer != null);
            _EntityRenderer.Disconnect();
            System.Diagnostics.Debug.Assert(_ParticleObjectRenderer != null);
            _ParticleObjectRenderer.Disconnect();
            System.Diagnostics.Debug.Assert(_ShapeObjectRenderer != null);
            _ShapeObjectRenderer.Disconnect();

            System.Diagnostics.Debug.Assert(Window != null);
            Window.Flush(world, invPlayer);

            System.Diagnostics.Debug.Assert(world != null);
            world.Disconnect(UserId);

            //MyConsole.Debug("Flush Connection!");
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

                    _ChunkingHelprt.Dispose();
                    _TeleportRecords.Dispose();

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

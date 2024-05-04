
using Containers;

namespace Protocol
{
    public sealed class Connection : System.IDisposable
    {
        private sealed class TeleportationRecord
        {
            private const long TickLimit = 20;  // 1 seconds, 20 ticks

            public readonly int _payload;
            private long _ticks = 0;

            public TeleportationRecord(int payload)
            {
                _payload = payload;
            }

            public void Confirm(int payload)
            {
                if (payload != _payload)
                    throw new UnexpectedValueException("ConfirmSelfPlayerTeleportationPacket.Payload");
            }

            public void Update()
            {
                System.Diagnostics.Debug.Assert(_ticks >= 0);

                if (_ticks++ > TickLimit)
                {
                    throw new TeleportationConfirmTimeoutException();
                }

            }

        }

        private sealed class KeepAliveRecord
        {
            private const long TickLimit = 20 * 30;  // 30 seconds, 20 * 30 ticks

            public readonly long _payload;
            private long _ticks = 0;

            public KeepAliveRecord(long payload)
            {
                _payload = payload;
            }

            public void Confirm(long payload)
            {
                if (payload != _payload)
                    throw new UnexpectedValueException("ResponseKeepAlivePacketId.Payload");
            }

            public void Update()
            {
                System.Diagnostics.Debug.Assert(_ticks >= 0);

                if (_ticks++ > TickLimit)
                {
                    throw new ResponseKeepAliveTimeoutException();
                }

            }

        }

        private bool _disposed = false;

        private readonly int Id;

        private readonly Client _CLIENT;  // dispoasble

        private bool _init = false;
        private int _initStep = 0;

        private const int _MIN_RENDER_DISTANCE = 2, _MAX_RENDER_DISTANCE = 32;
        private int _renderDistance = -1;

        private const int _MAX_LOAD_CHUNK_COUNT = 5;
        private Set<Chunk.Vector> _loadedChunkPositions = new();  // Disposable
        private Table<int, EntityRendererManager> _loadedEntityRendererManagers = new();  // Disposable

        private readonly Queue<TeleportationRecord> _TELEPORTATION_RECORDS = new();  // dispoasble
        private KeepAliveRecord? _keepAliveRecord = null;

        private readonly Queue<LoadChunkPacket> _LOAD_CHUNK_PACKETS = new();  // dispoasble
        private readonly Queue<ClientboundPlayingPacket> _OUT_PACKETS = new();  // dispoasble

        private Window? _window = null;  // disposable

        internal Connection(Client client)
        {
            Id = client.LocalPort;

            _CLIENT = client;
        }

        ~Connection() => System.Diagnostics.Debug.Assert(false);
        
        private void StartInitProcess(Buffer buffer, World world, Player player)
        {
            if (_initStep == 0)
            {
                /*Console.WriteLine("JoinGame!");*/

                player.Connect(_OUT_PACKETS);
                world.PlayerListConnect(player.UniqueId, _OUT_PACKETS);

                System.Diagnostics.Debug.Assert(_renderDistance == -1);

                // TODO: If already player exists, use id of that player object, not new alloc id.
                JoinGamePacket packet = new(player.Id, 0, 0, 0, "default", false);  // TODO
                packet.Write(buffer);
                _CLIENT.Send(buffer);

                _initStep++;
            }

            if (_initStep == 1)
            {
                /*Console.WriteLine("ClientSettings!");*/

                System.Diagnostics.Debug.Assert(_renderDistance == -1);

                _CLIENT.Recv(buffer);

                int packetId = buffer.ReadInt(true);
                if (ServerboundPlayingPacket.SetClientSettingsPacketId != packetId)
                    throw new UnexpectedPacketException();

                SetClientSettingsPacket packet = SetClientSettingsPacket.Read(buffer);

                if (buffer.Size > 0)
                    throw new BufferOverflowException();

                _renderDistance = packet.RenderDistance;
                if (_renderDistance < _MIN_RENDER_DISTANCE ||
                    _renderDistance > _MAX_RENDER_DISTANCE)
                    throw new UnexpectedValueException("SetClientSettingsPacket.RenderDistance");

                _initStep++;
            }

            if (_initStep == 2)
            {
                /*Console.WriteLine("PluginMessage!");*/

                System.Diagnostics.Debug.Assert(_renderDistance >= _MIN_RENDER_DISTANCE);
                System.Diagnostics.Debug.Assert(_renderDistance <= _MAX_RENDER_DISTANCE);

                _CLIENT.Recv(buffer);

                int packetId = buffer.ReadInt(true);
                if (0x09 != packetId)
                    throw new UnexpectedPacketException();

                buffer.Flush();

                if (buffer.Size > 0)
                    throw new BufferOverflowException();

                _initStep++;
            }

            System.Diagnostics.Debug.Assert(_initStep == 3);
            _init = true;

            System.Diagnostics.Debug.Assert(_window == null);
            _window = new(_OUT_PACKETS, player._selfInventory);
        }

        private void RecvDataAndHandle(
            Buffer buffer, long serverTicks, World world, Player player)
        {
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
                    }
                    break;
                case ServerboundPlayingPacket.SetClientSettingsPacketId:
                    {
                        SetClientSettingsPacket packet = SetClientSettingsPacket.Read(buffer);

                        throw new System.NotImplementedException();
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

                        throw new System.NotImplementedException();
                    }
                    break;
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
                            player._selfInventory,
                            packet.WINDOW_ID,
                            packet.MODE,
                            packet.BUTTON,
                            packet.SLOT,
                            packet.SLOT_DATA,
                            _OUT_PACKETS);

                        _OUT_PACKETS.Enqueue(new ClientboundConfirmTransactionPacket(
                                (sbyte)packet.WINDOW_ID, packet.ACTION, true));

                        /*{
                            SlotData slotData = new(280, 1, 0);
                            _outPackets.Enqueue(new SetSlotPacket(0, 27, slotData.WriteData()));
                        }
                        {
                            SlotData slotData = new();
                            _outPackets.Enqueue(new SetSlotPacket(-1, 27, slotData.WriteData()));
                        }*/
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

                        /*_outPackets.Enqueue(new SetSlotPacket(-1, 0, new(280, 10)));*/

                        System.Diagnostics.Debug.Assert(_window != null);
                        _window.ResetWindow(packet.WindowId, _OUT_PACKETS);
                    }
                    break;
                case ServerboundPlayingPacket.ResponseKeepAlivePacketId:
                    {
                        ResponseKeepAlivePacket packet = ResponseKeepAlivePacket.Read(buffer);

                        if (_keepAliveRecord == null)
                        {
                            throw new UnexpectedPacketException();
                        }

                        _keepAliveRecord.Confirm(packet.Payload);
                        _keepAliveRecord = null;

                        world.PlayerListKeepAlive(serverTicks, player.UniqueId);

                    }
                    break;
                case ServerboundPlayingPacket.PlayerPacketId:
                    {
                        PlayerPacket packet = PlayerPacket.Read(buffer);

                        player.Stand(packet.OnGround);
                    }
                    break;
                case ServerboundPlayingPacket.PlayerPositionPacketId:
                    {
                        PlayerPositionPacket packet = PlayerPositionPacket.Read(buffer);

                        if (!_TELEPORTATION_RECORDS.Empty)
                            throw new System.NotImplementedException();

                        if (_TELEPORTATION_RECORDS.Empty)
                        {
                            player.Control(new(packet.X, packet.Y, packet.Z));
                        }

                        player.Stand(packet.OnGround);
                    }
                    break;
                case ServerboundPlayingPacket.PlayerPosAndLookPacketId:
                    {
                        PlayerPosAndLookPacket packet = PlayerPosAndLookPacket.Read(buffer);

                        if (_TELEPORTATION_RECORDS.Empty)
                        {
                            player.Control(new(packet.X, packet.Y, packet.Z));
                        }

                        player.Rotate(new(packet.Yaw, packet.Pitch));
                        player.Stand(packet.OnGround);
                    }
                    break;
                case ServerboundPlayingPacket.PlayerLookPacketId:
                    {
                        PlayerLookPacket packet = PlayerLookPacket.Read(buffer);

                        player.Rotate(new(packet.Yaw, packet.Pitch));
                        player.Stand(packet.OnGround);
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
                                if (player.IsSneaking)
                                {
                                    throw new UnexpectedValueException("Entity.Sneaking");
                                }

                                player.Sneak();
                                break;
                            case 1:
                                /*System.Console.Write("Unseanking!");*/
                                if (!player.IsSneaking)
                                {
                                    throw new UnexpectedValueException("Entity.Sneaking");
                                }

                                player.Unsneak();
                                break;
                            case 3:
                                if (player.IsSprinting)
                                {
                                    throw new UnexpectedValueException("Entity.Sprinting");
                                }

                                player.Sprint();
                                break;
                            case 4:
                                if (!player.IsSprinting)
                                {
                                    throw new UnexpectedValueException("Entity.Sprinting");
                                }

                                player.Unsprint();
                                break;
                        }

                        if (packet.JumpBoost > 0)
                        {
                            throw new UnexpectedValueException("EntityAction.JumpBoost");
                        }

                    }
                    break;
            }

            if (!buffer.Empty)
                throw new BufferOverflowException();
        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <returns>TODO: Add description.</returns>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
        public void Control(
            long serverTicks, World world, Player player)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            using Buffer buffer = new();

            try
            {
                try
                {
                    if (!_init)
                    {
                        StartInitProcess(buffer, world, player);
                    }
                    else
                    {
                        /*if (serverTicks == 200)  // 10 seconds
                        {
                            System.Diagnostics.Debug.Assert(_window != null);
                            _window.OpenWindowWithPublicInventory(
                                _OUT_PACKETS,
                                player._selfInventory,
                                world._Inventory);
                        }*/

                        while (true)
                        {
                            RecvDataAndHandle(buffer, serverTicks, world, player);
                        }

                    }
                }
                catch (UnexpectedClientBehaviorExecption e)
                {
                    // TODO: send disconnected message to client.

                    System.Console.WriteLine(e.Message);

                    throw new DisconnectedClientException();
                }
            }
            catch (TryAgainException)
            {

            }
            catch (DisconnectedClientException)
            {
                buffer.Flush();
                player.Disconnect();
                world.PlayerListDisconnect(player.UniqueId);

                throw;
            }

            if (_init)
            {
                foreach (TeleportationRecord record in _TELEPORTATION_RECORDS.GetValues())
                {
                    record.Update();
                }

                if (_keepAliveRecord != null)
                {
                    _keepAliveRecord.Update();
                }
            }

        }

        private void LoadWorld(World world, Player player)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            using Set<Chunk.Vector> prevLoadedChunkPositions = _loadedChunkPositions;
            Set<Chunk.Vector> loadedChunkPositions = new();

            using Queue<Entity> newEntities = new();

            using Table<int, EntityRendererManager> prevRendererManagers = _loadedEntityRendererManagers;
            Table<int, EntityRendererManager> rendererManagers = new();

            {
                Chunk.Vector pCenter = Chunk.Vector.Convert(player.Position);
                Chunk.Grid grid = Chunk.Grid.Generate(pCenter, _renderDistance);

                int n = 0;

                int mask; byte[] data;

                foreach (Chunk.Vector p in grid.GetVectorsInSpiral())
                {
                    if (world.ContainsEntities(p))
                    {
                        foreach (Entity entity in world.GetEntities(p))
                        {
                            if (entity.Id == player.Id) continue;

                            if (rendererManagers.Contains(entity.Id)) continue;

                            EntityRendererManager rendererManager = entity._RendererManager;

                            if (prevRendererManagers.Contains(entity.Id))
                            {
                                prevRendererManagers.Extract(entity.Id);
                            }
                            else
                            {
                                newEntities.Enqueue(entity);
                                var renderer = new EntityRenderer(Id, _OUT_PACKETS);
                                rendererManager.AddRenderer(Id, renderer);
                            }

                            System.Diagnostics.Debug.Assert(rendererManager.ContainsRenderer(Id));
                            rendererManagers.Insert(entity.Id, rendererManager);
                        }
                    }


                    loadedChunkPositions.Insert(p);

                    if (prevLoadedChunkPositions.Contains(p))
                    {
                        prevLoadedChunkPositions.Extract(p);
                    }
                    else
                    {
                        /*if (world.ContainsChunk(o))
                            {
                            Chunk chunk = world.GetChunk(p);
                            (mask, data) = Chunk.Write(chunk);
                        }
                            else
                        {
                            (mask, data) = Chunk.Write();
                        }*/

                        (mask, data) = Chunk.Write2();
                        _LOAD_CHUNK_PACKETS.Enqueue(new LoadChunkPacket(p.X, p.Z, true, mask, data));

                        if (++n == _MAX_LOAD_CHUNK_COUNT)
                        {
                            break;
                        }
                    }
                    
                }

                System.Diagnostics.Debug.Assert(n <= _MAX_LOAD_CHUNK_COUNT);
            }

            if (!prevRendererManagers.Empty)
            {
                int i = 0;
                var despawnedEntityIds = new int[prevRendererManagers.Count];
                foreach ((int entityId, var rendererManager) in prevRendererManagers.Flush())
                {
                    System.Diagnostics.Debug.Assert(rendererManager.ContainsRenderer(Id));
                    rendererManager.RemoveRenderer(Id);
                    despawnedEntityIds[i++] = entityId;
                }

                _OUT_PACKETS.Enqueue(new DestroyEntitiesPacket(despawnedEntityIds));
            }

            while (!newEntities.Empty)
            {
                Entity entity = newEntities.Dequeue();
                System.Diagnostics.Debug.Assert(entity.Id != player.Id);

                switch (entity)
                {
                    default:
                        throw new System.NotImplementedException();
                    case Player spawnedPlayer:
                        {
                            byte flags = 0x00;

                            if (spawnedPlayer.IsSneaking)
                                flags |= 0x02;
                            if (spawnedPlayer.IsSprinting)
                                flags |= 0x08;

                            using EntityMetadata metadata = new();
                            metadata.AddByte(0, flags);

                            (byte x, byte y) = spawnedPlayer.Look.ConvertToPacketFormat();
                            _OUT_PACKETS.Enqueue(new SpawnNamedEntityPacket(
                                spawnedPlayer.Id,
                                spawnedPlayer.UniqueId,
                                spawnedPlayer.Position.X, spawnedPlayer.Position.Y, spawnedPlayer.Position.Z,
                                x, y,  // TODO: Convert yaw and pitch to angles of minecraft protocol.
                                metadata.WriteData()));
                        }
                        break;
                }
            }

            {
                Chunk.Vector[] positions = prevLoadedChunkPositions.Flush();
                for (int i = 0; i < positions.Length; ++i)
                {
                    Chunk.Vector p = positions[i];
                    _OUT_PACKETS.Enqueue(new UnloadChunkPacket(p.X, p.Z));
                }
            }
            System.Diagnostics.Debug.Assert(newEntities.Empty);

            _loadedChunkPositions = loadedChunkPositions;
            _loadedEntityRendererManagers = rendererManagers;
        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
        public void Render(World world, Player player)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (!_init)
                return;

            System.Diagnostics.Debug.Assert(_renderDistance >= _MIN_RENDER_DISTANCE);
            System.Diagnostics.Debug.Assert(_renderDistance <= _MAX_RENDER_DISTANCE);
            LoadWorld(world, player);

            if (_OUT_PACKETS.Empty) return;

            using Buffer buffer = new();

            try
            {
                while (!_LOAD_CHUNK_PACKETS.Empty)
                {
                    LoadChunkPacket packet = _LOAD_CHUNK_PACKETS.Dequeue();

                    packet.Write(buffer);
                    _CLIENT.Send(buffer);

                    System.Diagnostics.Debug.Assert(buffer.Empty);
                }

                while (!_OUT_PACKETS.Empty)
                {
                    ClientboundPlayingPacket packet = _OUT_PACKETS.Dequeue();

                    if (packet is TeleportSelfPlayerPacket teleportPacket)
                    {
                        TeleportationRecord report = new(teleportPacket.Payload);
                        _TELEPORTATION_RECORDS.Enqueue(report);
                    }
                    else if (packet is ClientboundCloseWindowPacket)
                    {
                        System.Diagnostics.Debug.Assert(_window != null);
                        _window.ResetWindowForcibly(player._selfInventory, _OUT_PACKETS, false);
                    }
                    else if (packet is RequestKeepAlivePacket requestKeepAlivePacket)
                    {
                        // TODO: save payload and check when recived.
                        System.Diagnostics.Debug.Assert(_keepAliveRecord == null);
                        _keepAliveRecord = new(requestKeepAlivePacket.Payload);
                    }

                    packet.Write(buffer);
                    _CLIENT.Send(buffer);

                    System.Diagnostics.Debug.Assert(buffer.Empty);
                }

            }
            catch (DisconnectedClientException)
            {
                buffer.Flush();
                player.Disconnect();
                world.PlayerListDisconnect(player.UniqueId);

                throw;
            }

            System.Diagnostics.Debug.Assert(_OUT_PACKETS.Empty);
        }

        public void Flush()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // TODO: Release resources corrently for no garbage.
            _loadedChunkPositions.Flush();
            foreach ((int entityId, var rendererManager) in _loadedEntityRendererManagers.Flush())
            {
                rendererManager.RemoveRenderer(Id);
            }

            _TELEPORTATION_RECORDS.Flush();

            _LOAD_CHUNK_PACKETS.Flush();
            _OUT_PACKETS.Flush();

            if (_window != null)
            {
                _window.CloseWindow();
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            // Assertion
            System.Diagnostics.Debug.Assert(_loadedChunkPositions.Empty);
            System.Diagnostics.Debug.Assert(_loadedEntityRendererManagers.Empty);

            System.Diagnostics.Debug.Assert(_TELEPORTATION_RECORDS.Empty);

            System.Diagnostics.Debug.Assert(_LOAD_CHUNK_PACKETS.Empty);
            System.Diagnostics.Debug.Assert(_OUT_PACKETS.Empty);

            if (disposing == true)
            {
                // managed objects
                _CLIENT.Dispose();

                _loadedChunkPositions.Dispose();
                _loadedEntityRendererManagers.Dispose();

                _TELEPORTATION_RECORDS.Dispose();

                _LOAD_CHUNK_PACKETS.Dispose();
                _OUT_PACKETS.Dispose();

                if (_window != null)
                {
                    _window.Dispose();
                }

            }

            // unmanaged objects

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

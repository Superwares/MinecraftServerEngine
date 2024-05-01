
using Containers;

namespace Protocol
{
    public sealed class Connection : System.IDisposable
    {

        private sealed class TeleportationRecord
        {
            private const ulong TickLimit = 20;  // 1 seconds, 20 ticks

            public readonly int _payload;
            private ulong _ticks = 0;

            public TeleportationRecord(int payload)
            {
                _payload = payload;
            }

            public void Confirm(int payload)
            {
                if (payload != _payload)
                    throw new UnexpectedValueException("TeleportationPayload");
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

        private bool _disposed = false;

        private readonly Client _Client;  // dispoasble

        private bool _init = false;
        private int _initStep = 0;

        private const int _MinRenderDistance = 2, _MaxRenderDistance = 32;
        private int _renderDistance = -1;

        private Chunk.Grid? _renderedChunkGrid = null;
        private Table<int, EntityRenderer>? _renderers = null;  // Disposable

        private readonly Queue<TeleportationRecord> _teleportationRecords = new();  // dispoasble

        private readonly Queue<LoadChunkPacket> _loadChunkPackets = new();  // dispoasble
        private readonly Queue<ClientboundPlayingPacket> _outPackets = new();  // dispoasble
        internal Queue<ClientboundPlayingPacket> Renderer => _outPackets;

        private Window? _window = null;  // disposable

        internal Connection(Client client)
        {
            _Client = client;
        }

        ~Connection() => System.Diagnostics.Debug.Assert(false);
        
        private void StartInitProcess(Buffer buffer, World world, Player player)
        {
            if (_initStep == 0)
            {
                /*Console.WriteLine("JoinGame!");*/

                player.Connect(_outPackets);
                world.PlayerListConnect(player.UniqueId, _outPackets);

                System.Diagnostics.Debug.Assert(_renderDistance == -1);

                // TODO: If already player exists, use id of that player object, not new alloc id.
                JoinGamePacket packet = new(player.Id, 0, 0, 0, "default", false);  // TODO
                packet.Write(buffer);
                _Client.Send(buffer);

                _initStep++;
            }

            if (_initStep == 1)
            {
                /*Console.WriteLine("ClientSettings!");*/

                System.Diagnostics.Debug.Assert(_renderDistance == -1);

                _Client.Recv(buffer);

                int packetId = buffer.ReadInt(true);
                if (ServerboundPlayingPacket.SetClientSettingsPacketId != packetId)
                    throw new UnexpectedPacketException();

                SetClientSettingsPacket packet = SetClientSettingsPacket.Read(buffer);

                if (buffer.Size > 0)
                    throw new BufferOverflowException();

                _renderDistance = packet.RenderDistance;
                if (_renderDistance < _MinRenderDistance ||
                    _renderDistance > _MaxRenderDistance)
                    throw new UnexpectedValueException("SetClientSettingsPacket.RenderDistance");

                _initStep++;
            }

            if (_initStep == 2)
            {
                /*Console.WriteLine("PluginMessage!");*/

                System.Diagnostics.Debug.Assert(_renderDistance >= _MinRenderDistance);
                System.Diagnostics.Debug.Assert(_renderDistance <= _MaxRenderDistance);

                _Client.Recv(buffer);

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
            _window = new(_outPackets, player._selfInventory);
        }

        private void RecvDataAndHandle(
            Buffer buffer, long serverTicks, World world, Player player)
        {
            _Client.Recv(buffer);

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

                        if (_teleportationRecords.Empty)
                            throw new UnexpectedPacketException();

                        TeleportationRecord record = _teleportationRecords.Dequeue();
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
                            _outPackets);

                        _outPackets.Enqueue(new ClientboundConfirmTransactionPacket(
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
                            throw new UnexpectedValueException($"ClickWindowPacket.WindowId");

                        /*_outPackets.Enqueue(new SetSlotPacket(-1, 0, new(280, 10)));*/

                        System.Diagnostics.Debug.Assert(_window != null);
                        _window.ResetWindow(packet.WindowId, _outPackets);
                    }
                    break;
                case ServerboundPlayingPacket.ResponseKeepAlivePacketId:
                    {
                        ResponseKeepAlivePacket packet = ResponseKeepAlivePacket.Read(buffer);

                        // TODO: Check payload with cache. If not corrent, throw unexpected client behavior exception.
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

                        if (!_teleportationRecords.Empty)
                            throw new System.NotImplementedException();

                        if (_teleportationRecords.Empty)
                        {
                            player.Control(new(packet.X, packet.Y, packet.Z));
                        }

                        player.Stand(packet.OnGround);
                    }
                    break;
                case ServerboundPlayingPacket.PlayerPosAndLookPacketId:
                    {
                        PlayerPosAndLookPacket packet = PlayerPosAndLookPacket.Read(buffer);

                        if (_teleportationRecords.Empty)
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
                        /*if (serverTicks == 100)  // 5 seconds
                        {
                            System.Diagnostics.Debug.Assert(_window != null);
                            _window.OpenWindowWithPublicInventory(
                                _outPackets,
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
                foreach (TeleportationRecord record in _teleportationRecords.GetValues())
                {
                    record.Update();
                }
            }
        }

        // TODO: Make chunks to readonly using interface? in this function.
        private void RenderChunks(World world, Entity.Vector pos)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Chunk.Vector pChunkCenter = Chunk.Vector.Convert(pos);
            int d = _renderDistance;
            System.Diagnostics.Debug.Assert(d >= _MinRenderDistance);
            System.Diagnostics.Debug.Assert(d <= _MaxRenderDistance);

            Chunk.Grid grid = Chunk.Grid.Generate(pChunkCenter, d);

            if (_renderedChunkGrid == null)
            {
                int mask; byte[] data;

                foreach (Chunk.Vector pChunk in grid.GetVectors())
                {
                    /*if (world.ContainsChunk(pChunk))
                    {
                        Chunk chunk = world.GetChunk(pChunk);
                        (mask, data) = Chunk.Write(chunk);
                    }
                    else
                    {
                        (mask, data) = Chunk.Write();
                    }*/
                    (mask, data) = Chunk.Write2();

                    _loadChunkPackets.Enqueue(new LoadChunkPacket(
                        pChunk.X, pChunk.Z, true, mask, data));
                }

            }
            else
            {
                Chunk.Grid gridPrev = _renderedChunkGrid;

                if (gridPrev.Equals(grid))
                    return;

                Chunk.Grid gridBetween = Chunk.Grid.Generate(grid, gridPrev);

                foreach (Chunk.Vector pChunk in grid.GetVectors())
                {
                    if (gridBetween.Contains(pChunk))
                        continue;

                    int mask; byte[] data;

                    /*if (world.ContainsChunk(pChunk))
                    {
                        Chunk chunk = world.GetChunk(pChunk);
                        (mask, data) = Chunk.Write(chunk);
                    }
                    else
                    {
                        (mask, data) = Chunk.Write();
                    }*/

                    (mask, data) = Chunk.Write2();

                    _loadChunkPackets.Enqueue(new LoadChunkPacket(
                        pChunk.X, pChunk.Z, true, mask, data));
                }

                foreach (Chunk.Vector pChunk in gridPrev.GetVectors())
                {
                    if (gridBetween.Contains(pChunk))
                        continue;

                    _outPackets.Enqueue(new UnloadChunkPacket(pChunk.X, pChunk.Z));
                }

            }

            _renderedChunkGrid = grid;
        }

        private void RenderEntities(World world, int selfEntityId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            using Queue<Entity> newEntities = new();
            Table<int, EntityRenderer> renderers = new();

            if (_renderers == null)
            {
                System.Diagnostics.Debug.Assert(_renderedChunkGrid != null);
                foreach (Chunk.Vector pChunk in _renderedChunkGrid.GetVectors())
                {
                    if (!world.ContainsEntities(pChunk))
                        continue;

                    foreach (Entity entity in world.GetEntities(pChunk))
                    {
                        if (entity.Id == selfEntityId) continue;

                        EntityRenderer renderer = entity._Renderer;

                        newEntities.Enqueue(entity);
                        renderer.Add(selfEntityId, _outPackets);

                        renderers.Insert(entity.Id, renderer);
                    }
                }

            }
            else
            {
                System.Diagnostics.Debug.Assert(_renderers != null);
                using Table<int, EntityRenderer> prevRenderers = _renderers;

                System.Diagnostics.Debug.Assert(_renderedChunkGrid != null);
                foreach (Chunk.Vector pChunk in _renderedChunkGrid.GetVectors())
                {
                    if (!world.ContainsEntities(pChunk))
                        continue;

                    foreach (Entity entity in world.GetEntities(pChunk))
                    {
                        if (entity.Id == selfEntityId) continue;

                        EntityRenderer renderer = entity._Renderer;

                        if (prevRenderers.Contains(entity.Id))
                            prevRenderers.Extract(entity.Id);
                        else
                        {
                            newEntities.Enqueue(entity);
                            renderer.Add(selfEntityId, _outPackets);
                        }

                        renderers.Insert(entity.Id, renderer);
                    }
                }

                int i = 0;
                var despawnedEntityIds = new int[prevRenderers.Count];
                foreach ((int entityId, EntityRenderer renderer) in prevRenderers.GetElements())
                {
                    renderer.Remove(selfEntityId);
                    despawnedEntityIds[i++] = entityId;
                }

                _outPackets.Enqueue(new DestroyEntitiesPacket(despawnedEntityIds));

            }

            while (!newEntities.Empty)
            {
                Entity entity = newEntities.Dequeue();
                System.Diagnostics.Debug.Assert(entity.Id != selfEntityId);

                switch (entity)
                {
                    default:
                        throw new System.NotImplementedException();
                    case Player player:
                        {
                            byte flags = 0x00;

                            if (player.IsSneaking)
                                flags |= 0x02;
                            if (player.IsSprinting)
                                flags |= 0x08;

                            using EntityMetadata metadata = new();
                            metadata.AddByte(0, flags);

                            (byte x, byte y) = player.Look.ConvertToPacketFormat();
                            _outPackets.Enqueue(new SpawnNamedEntityPacket(
                                player.Id,
                                player.UniqueId,
                                player.Position.X, player.Position.Y, player.Position.Z,
                                x, y,  // TODO: Convert yaw and pitch to angles of minecraft protocol.
                                metadata.WriteData()));
                        }
                        break;
                }

            }

            System.Diagnostics.Debug.Assert(newEntities.Empty);

            _renderers = renderers;
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

            System.Diagnostics.Debug.Assert(_renderDistance >= _MinRenderDistance);
            System.Diagnostics.Debug.Assert(_renderDistance <= _MaxRenderDistance);
            RenderChunks(world, player.Position);
            RenderEntities(world, player.Id);

            if (_outPackets.Empty) return;

            using Buffer buffer = new();

            try
            {
                while (!_loadChunkPackets.Empty)
                {
                    LoadChunkPacket packet = _loadChunkPackets.Dequeue();

                    packet.Write(buffer);
                    _Client.Send(buffer);

                    System.Diagnostics.Debug.Assert(buffer.Empty);
                }

                while (!_outPackets.Empty)
                {
                    ClientboundPlayingPacket packet = _outPackets.Dequeue();

                    if (packet is TeleportSelfPlayerPacket teleportPacket)
                    {
                        TeleportationRecord report = new(teleportPacket.Payload);
                        _teleportationRecords.Enqueue(report);
                    }
                    else if (packet is ClientboundCloseWindowPacket)
                    {
                        System.Diagnostics.Debug.Assert(_window != null);
                        _window.ResetWindowForcibly(player._selfInventory, _outPackets, false);
                    }
                    else if (packet is RequestKeepAlivePacket)
                    {
                        // TODO: save payload and check when recived.
                    }

                    packet.Write(buffer);
                    _Client.Send(buffer);

                    System.Diagnostics.Debug.Assert(buffer.Empty);
                }

            }
            catch (DisconnectedClientException)
            {
                buffer.Flush();

                throw;
            }

            System.Diagnostics.Debug.Assert(_outPackets.Empty);
        }

        public void Flush()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // TODO: Release resources corrently for no garbage.
            System.Diagnostics.Debug.Assert(_renderers != null);
            _renderers.Flush();
            _teleportationRecords.Flush();
            _loadChunkPackets.Flush();
            _outPackets.Flush();

            if (_window != null)
            {
                if (_window.IsOpenedWithPublicInventory())
                {
                    _window.CloseWindow();
                }
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            System.Diagnostics.Debug.Assert(_renderers != null);
            System.Diagnostics.Debug.Assert(_renderers.Empty);
            System.Diagnostics.Debug.Assert(_teleportationRecords.Empty);
            System.Diagnostics.Debug.Assert(_loadChunkPackets.Empty);
            System.Diagnostics.Debug.Assert(_outPackets.Empty);

            if (disposing == true)
            {
                // managed objects
                _Client.Dispose();

                _renderers.Dispose();
                _teleportationRecords.Dispose();
                _loadChunkPackets.Dispose();
                _outPackets.Dispose();

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

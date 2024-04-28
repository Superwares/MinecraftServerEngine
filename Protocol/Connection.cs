
using Containers;
using System.Diagnostics;
using System.Xml;

namespace Protocol
{
    internal sealed class Connection : System.IDisposable
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

        private Client _client;  // dispoasble

        private const int _MinRenderDistance = 2, _MaxRenderDistance = 32;
        private int _renderDistance;

        private Chunk.Grid? _renderedChunkGrid = null;
        private Table<int, EntityRenderer>? _renderers = null;  // Disposable

        private readonly Queue<TeleportationRecord> _teleportationRecords = new();  // dispoasble

        private readonly Queue<LoadChunkPacket> _loadChunkPackets = new();  // dispoasble
        private readonly Queue<ClientboundPlayingPacket> _outPackets = new();  // dispoasble
        internal Queue<ClientboundPlayingPacket> Renderer => _outPackets;

        private readonly Window _window;  // disposable

        private bool _disposed = false;

        internal Connection(
            Client client,
            int renderDistance)
        {
            _client = client;

            _renderDistance = renderDistance;

            _window = new(_outPackets, new SelfInventory());

            /*{
                SlotData slotData = new(280, 64);
                _outPackets.Enqueue(new SetSlotPacket(0, 27, slotData.WriteData()));
            }*/
        }

        ~Connection() => System.Diagnostics.Debug.Assert(false);

        public void Render(ClientboundPlayingPacket packet)
        {
            _outPackets.Enqueue(packet);
        }

        internal void OpenPublicInvenrory(PublicInventory inventory)
        {
            System.Diagnostics.Debug.Assert(false);
        }

        internal void ClosePublicInvenrory()
        {
            System.Diagnostics.Debug.Assert(false);
        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <returns>TODO: Add description.</returns>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
        public void Control(
            long serverTicks, World world, PlayerList playerList, Player player)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            /*if (serverTicks == 100)
            {
                SlotData slotData = new(426, 64);

                _outPackets.Enqueue(new OpenWindowPacket(1, "minecraft:chest", "WindowTitle!", 27));
                _outPackets.Enqueue(new SetSlotPacket(1, 30, slotData));
                
            }*/

            /*if (serverTicks == 100)
            {
                _windowHelper.ReopenWindowWithOtherInventory(Id, _outPackets, new PlayerInventory());
            }*/

            try
            {
                Buffer buffer = new();

                while (true)
                {
                    _client.Recv(buffer);

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
                            }
                            break;
                        case ServerboundPlayingPacket.ClickWindowPacketId:
                            {
                                ClickWindowPacket packet = ClickWindowPacket.Read(buffer);

                                {
                                    System.Console.WriteLine();
                                    System.Console.WriteLine(
                                        $"WindowId: {packet.WindowId}, " +
                                        $"SlotNumber: {packet.SlotNumber}, " +
                                        $"ButtonNumber: {packet.ButtonNumber}, " +
                                        $"ActionNumber: {packet.ActionNumber}, " +
                                        $"ModeNumber: {packet.ModeNumber}, " +
                                        $"SlotData.Id: {packet.Data.Id}, " +
                                        $"SlotData.Count: {packet.Data.Count}, ");
                                }

                                if (_window.GetWindowId() != packet.WindowId)
                                    throw new System.NotImplementedException();

                                _window.Handle(
                                    packet.WindowId,
                                    packet.ModeNumber,
                                    packet.ButtonNumber,
                                    packet.SlotNumber,
                                    _outPackets);

                                _outPackets.Enqueue(new ClientboundConfirmTransactionPacket(
                                        (sbyte)packet.WindowId, packet.ActionNumber, true));

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
                                    throw new UnexpectedValueException($"ClickWindowPacket.WindowId {packet.WindowId}");

                                if (_window.GetWindowId() != packet.WindowId)
                                    throw new System.NotImplementedException();

                                _window.ResetWindow(_outPackets);
                            }
                            break;
                        case ServerboundPlayingPacket.ResponseKeepAlivePacketId:
                            {
                                ResponseKeepAlivePacket packet = ResponseKeepAlivePacket.Read(buffer);

                                // TODO: Check payload with cache. If not corrent, throw unexpected client behavior exception.
                                playerList.KeepAlive(serverTicks, player.UniqueId);
                            }
                            break;
                        case ServerboundPlayingPacket.PlayerPacketId:
                            {
                                PlayerPacket packet = PlayerPacket.Read(buffer);

                                if (!_teleportationRecords.Empty)
                                    throw new System.NotImplementedException();

                                player.Stand(packet.OnGround);
                            }
                            break;
                        case ServerboundPlayingPacket.PlayerPositionPacketId:
                            {
                                PlayerPositionPacket packet = PlayerPositionPacket.Read(buffer);

                                if (!_teleportationRecords.Empty)
                                    throw new System.NotImplementedException();

                                player.Control(new(packet.X, packet.Y, packet.Z));
                                player.Stand(packet.OnGround);
                            }
                            break;
                        case ServerboundPlayingPacket.PlayerPosAndLookPacketId:
                            {
                                PlayerPosAndLookPacket packet = PlayerPosAndLookPacket.Read(buffer);

                                if (!_teleportationRecords.Empty)
                                    throw new System.NotImplementedException();

                                player.Control(new(packet.X, packet.Y, packet.Z));
                                player.Rotate(new(packet.Yaw, packet.Pitch));
                                player.Stand(packet.OnGround);
                            }
                            break;
                        case ServerboundPlayingPacket.PlayerLookPacketId:
                            {
                                PlayerLookPacket packet = PlayerLookPacket.Read(buffer);

                                if (!_teleportationRecords.Empty)
                                    throw new System.NotImplementedException();

                                player.Rotate(new(packet.Yaw, packet.Pitch));
                                player.Stand(packet.OnGround);
                            }
                            break;
                        case ServerboundPlayingPacket.EntityActionPacketId:
                            {
                                EntityActionPacket packet = EntityActionPacket.Read(buffer);

                                if (!_teleportationRecords.Empty)
                                    throw new System.NotImplementedException();

                                switch (packet.ActionId)
                                {
                                    default:
                                        throw new UnexpectedValueException("EntityAction.ActoinId");
                                    case 0:
                                        if (player.IsSneaking)
                                            throw new UnexpectedValueException("Entity.Sneaking");
                                        /*System.Console.Write("Seanking!");*/
                                        player.Sneak();
                                        break;
                                    case 1:
                                        if (!player.IsSneaking)
                                            throw new UnexpectedValueException("Entity.Sneaking");
                                        /*System.Console.Write("Unseanking!");*/
                                        player.Unsneak();
                                        break;
                                    case 3:
                                        if (player.IsSprinting)
                                            throw new UnexpectedValueException("Entity.Sprinting");
                                        player.Sprint();
                                        break;
                                    case 4:
                                        if (!player.IsSprinting)
                                            throw new UnexpectedValueException("Entity.Sprinting");
                                        player.Unsprint();
                                        break;
                                }

                                if (packet.JumpBoost > 0)
                                    throw new UnexpectedValueException("EntityAction.JumpBoost");

                            }
                            break;
                    }

                    if (!buffer.Empty)
                        throw new BufferOverflowException();
                }
            }
            catch (UnexpectedClientBehaviorExecption e)
            {
                // TODO: send disconnected message to client.

                System.Console.WriteLine(e.Message);

                throw new DisconnectedClientException();
            }
            catch (DisconnectedClientException)
            {
                throw;
            }
            catch (TryAgainException)
            {

            }

            foreach (TeleportationRecord record in _teleportationRecords.GetValues())
                record.Update();
        }

        // TODO: Make chunks to readonly using interface? in this function.
        internal void RenderChunks(World world, Entity.Vector pos)
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

        internal void RenderEntities(World world, int selfEntityId)
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

                            (byte x, byte y) = player.Look.ConvertToProtocolFormat();
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
        public void SendData(Player player)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (_outPackets.Empty) return;

            using Buffer buffer = new();

            try
            {
                while (!_loadChunkPackets.Empty)
                {
                    LoadChunkPacket packet = _loadChunkPackets.Dequeue();

                    packet.Write(buffer);
                    _client.Send(buffer);

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
                        _window.ResetWindowForcibly(_outPackets);
                    }
                    else if (packet is RequestKeepAlivePacket)
                    {
                        // save payload and check when recived.
                    }

                    packet.Write(buffer);
                    _client.Send(buffer);

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
            Debug.Assert(!_disposed);

            // TODO: Release resources corrently for no garbage.
            System.Diagnostics.Debug.Assert(_renderers != null);
            _renderers.Flush();
            _teleportationRecords.Flush();
            _loadChunkPackets.Flush();
            _outPackets.Flush();
            _window.Flush();
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
                _client.Dispose();

                _renderers.Dispose();
                _teleportationRecords.Dispose();
                _loadChunkPackets.Dispose();
                _outPackets.Dispose();
                _window.Dispose();

                _window.Dispose();
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

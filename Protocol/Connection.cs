
using Containers;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml;

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

        private Client _client;  // dispoasble

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

        private readonly Window _window;  // disposable

        private bool _disposed = false;

        internal Connection(Client client)
        {
            _client = client;

            /*_renderDistance = renderDistance;

            _window = new(_outPackets, selfInventory);*/

            System.Diagnostics.Debug.Assert(!_init);

            /*{
                SlotData slotData = new(280, 64);
                _outPackets.Enqueue(new SetSlotPacket(0, 27, slotData.WriteData()));
            }*/
        }

        ~Connection() => System.Diagnostics.Debug.Assert(false);
        
        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <returns>TODO: Add description.</returns>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
        public void InitOrControl(
            long serverTicks, World world, Player player)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            using Buffer buffer = new();

            if (!_init)
            {
                try
                {
                    if (_initStep == 0)
                    {
                        /*Console.WriteLine("JoinGame!");*/

                        {
                            int payload = new Random().Next();
                            _outPackets.Enqueue(new TeleportSelfPlayerPacket(
                                player.Position.X, player.Position.Y, player.Position.Z,
                                player.Look.Yaw, player.Look.Pitch,
                                false, false, false, false, false,
                                payload));
                        }

                        {
                            _outPackets.Enqueue(new SetPlayerAbilitiesPacket(
                                false, false, false, false, 0, 0));
                        }

                        player.Connect(_outPackets);

                        Debug.Assert(_renderDistance == -1);

                        // TODO: If already player exists, use id of that player object, not new alloc id.
                        JoinGamePacket packet = new(player.Id, 0, 0, 0, "default", true);  // TODO
                        packet.Write(buffer);
                        _client.Send(buffer);

                        _initStep++;
                    }

                    if (_initStep == 1)
                    {
                        /*Console.WriteLine("ClientSettings!");*/

                        Debug.Assert(_renderDistance == -1);

                        _client.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundPlayingPacket.SetClientSettingsPacketId != packetId)
                            throw new UnexpectedPacketException();

                        SetClientSettingsPacket packet = SetClientSettingsPacket.Read(buffer);

                        if (buffer.Size > 0)
                            throw new BufferOverflowException();

                        _renderDistance = packet.RenderDistance;
                        if (_renderDistance < _MinRenderDistance ||
                            _renderDistance > _MaxRenderDistance)
                            throw new UnexpectedValueException("invalid render distance");  // TODO

                        _initStep++;
                    }

                    if (_initStep == 2)
                    {
                        /*Console.WriteLine("PluginMessage!");*/

                        Debug.Assert(_renderDistance >= _MinRenderDistance);
                        Debug.Assert(_renderDistance <= _MaxRenderDistance);

                        _client.Recv(buffer);

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

                }
                catch (TryAgainException)
                {
                    /*Console.WriteLine("TryAgainException!");*/
                }
                catch (UnexpectedClientBehaviorExecption)
                {
                    /*Console.WriteLine("UnexpectedBehaviorExecption!");*/

                    buffer.Flush();

                    // TODO: Send why disconnected...

                    throw new DisconnectedClientException();
                }
                catch (DisconnectedClientException)
                {
                    /*Console.WriteLine("DisconnectedException!");*/

                    buffer.Flush();

                    throw;
                }

            }
            else
            {
                try
                {
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

                                    throw new System.NotImplementedException();
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

                                    _window.Handle(
                                        player._selfInventory,
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

                                    _window.ResetWindow(packet.WindowId, _outPackets);
                                }
                                break;
                            case ServerboundPlayingPacket.ResponseKeepAlivePacketId:
                                {
                                    ResponseKeepAlivePacket packet = ResponseKeepAlivePacket.Read(buffer);

                                    // TODO: Check payload with cache. If not corrent, throw unexpected client behavior exception.
                                    world.KeepAliveConnectedPlayer(serverTicks, player.UniqueId);
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

                    player.Disconnect();

                    throw new DisconnectedClientException();
                }
                catch (DisconnectedClientException)
                {
                    player.Disconnect();

                    throw;
                }
                catch (TryAgainException)
                {

                }

                foreach (TeleportationRecord record in _teleportationRecords.GetValues())
                    record.Update();
            }

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
        public void Render(World world, Player player)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (!_init)
                return;

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
                        _window.ResetWindowForcibly(player._selfInventory, _outPackets);
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

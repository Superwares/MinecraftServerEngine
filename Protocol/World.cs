﻿using Common;
using Containers;

namespace Protocol
{
    public abstract class World : System.IDisposable
    {
        private bool _disposed = false;

        
        private readonly PlayerList _PLAYER_LIST = new();  // Disposable

        private readonly NumList _ENTITY_ID_LIST = new();  // Disposable

        private readonly Vector _P_SPAWE;
        private readonly Entity.Angles _LOOK_SPAWE;

        private readonly ConcurrentQueue<Entity> _ENTITY_SPAWNING_POOL = new();  // Disposable
        private readonly ConcurrentQueue<Entity> _ENTITIES1 = new();  // Disposable
        private readonly ConcurrentQueue<Entity> _ENTITIES2 = new();  // Disposable
        private readonly ConcurrentQueue<Entity> _DESPAWNED_ENTITIES = new();  // Disposable

        private readonly Table<System.Guid, Player> _DISCONNECTED_PLAYERS = new(); // Disposable

        private readonly BlockContext _BLOCK_CTX = new();  // Disposable
        
        private readonly Table<ChunkLocation, Table<int, Entity>> _CHUNK_TO_ENTITIES = new();
        private readonly Table<int, ChunkGrid> _ENTITY_TO_CHUNKS = new();  // Disposable

        /*internal PublicInventory _Inventory = new ChestInventory();*/

        public World(Vector pSpawe, Entity.Angles lookSpawe)
        {
            _P_SPAWE = pSpawe; _LOOK_SPAWE = lookSpawe;

            {
                // Dummy code.
                for (int z = -10; z <= 10; ++z)
                {
                    for (int x = -10; x <= 10; ++x)
                    {
                        BlockLocation loc = new(x, 100, z);

                        _BLOCK_CTX.SetBlock(loc, Blocks.Stone);
                    }
                }
                
            }
        }

        ~World() => System.Diagnostics.Debug.Assert(false);

        internal void ConnectPlayer(Player player, Queue<ClientboundPlayingPacket> renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            /*System.Console.WriteLine($"player.UniqueId: {player.UniqueId}");*/
            System.Diagnostics.Debug.Assert(_DISCONNECTED_PLAYERS.Contains(player.UniqueId));
            Player playerExtracted = _DISCONNECTED_PLAYERS.Extract(player.UniqueId);
            System.Diagnostics.Debug.Assert(ReferenceEquals(player, playerExtracted));

            _PLAYER_LIST.Connect(player.UniqueId, renderer);
        }

        internal void DisconnectPlayer(Player player)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _PLAYER_LIST.Disconnect(player.UniqueId);

            System.Diagnostics.Debug.Assert(!_DISCONNECTED_PLAYERS.Contains(player.UniqueId));
            _DISCONNECTED_PLAYERS.Insert(player.UniqueId, player);

            player.Disconnect();
        }

        internal void KeepAlivePlayer(long serverTicks, System.Guid uniqueId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _PLAYER_LIST.KeepAlive(serverTicks, uniqueId);
        }

        public abstract bool CanJoinWorld();
        
        internal Player SpawnOrFindPlayer(string username, System.Guid uniqueId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Player player;

            if (_DISCONNECTED_PLAYERS.Contains(uniqueId))
            {
                player = _DISCONNECTED_PLAYERS.Lookup(uniqueId);
            }
            else
            {
                int id = _ENTITY_ID_LIST.Alloc();
                player = new(
                    id,
                    uniqueId,
                    _P_SPAWE, _LOOK_SPAWE,
                    username);

                _ENTITY_SPAWNING_POOL.Enqueue(player);
            }

            return player;
        }

        internal ItemEntity SpawnItemEntity()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            int id = _ENTITY_ID_LIST.Alloc();
            ItemEntity itemEntity = new(id, _P_SPAWE, _LOOK_SPAWE);

            _ENTITY_SPAWNING_POOL.Enqueue(itemEntity);

            return itemEntity;
        }

        protected abstract bool DetermineToDespawnPlayerOnDisconnect();

        private void DespawnEntity(Entity entity)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (entity is Player player)
            {
                _PLAYER_LIST.ClosePlayer(player.UniqueId);

                Player playerExtracted = _DISCONNECTED_PLAYERS.Extract(player.UniqueId);
                System.Diagnostics.Debug.Assert(ReferenceEquals(playerExtracted, player));
            }

            CloseEntityChunkMapping(entity);

            entity.Flush();
        }

        private (Directions, bool, int) DetermineStairsBlockShape(BlockLocation loc, Blocks block)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(block.IsStairsBlock());

            Directions d = block.GetStairsDirection();
            bool bottom = block.IsBottomStairsBlock();

            Blocks block2 = _BLOCK_CTX.GetBlock(loc.Shift(d, 1));
            if (block2.IsStairsBlock() &&
                bottom == block2.IsBottomStairsBlock())
            {
                if (block2.IsVerticalStairsBlock() != block.IsVerticalStairsBlock())
                {
                    Directions d2 = block2.GetStairsDirection();
                    Blocks block3 = _BLOCK_CTX.GetBlock(loc.Shift(d2.GetOpposite(), 1));
                    if (!block3.IsStairsBlock() ||
                        block3.GetStairsDirection() != d ||
                        block3.IsBottomStairsBlock() != bottom)
                    {
                        if (d2 == d.GetCCW())
                        {
                            // outer left
                            return (d, bottom, 1);
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(d2 == d.GetCW());
                            // outer right
                            return (d, bottom, 2);
                        }
                    }
                }

            }

            Blocks block4 = _BLOCK_CTX.GetBlock(loc.Shift(d.GetOpposite(), 1));
            if (block4.IsStairsBlock() &&
                bottom == block4.IsBottomStairsBlock())
            {
                if (block4.IsVerticalStairsBlock() != block.IsVerticalStairsBlock())
                {
                    Directions d4 = block4.GetStairsDirection();
                    Blocks block5 = _BLOCK_CTX.GetBlock(loc.Shift(d4, 1));
                    if (!block5.IsStairsBlock() ||
                        block5.GetStairsDirection() != d ||
                        block5.IsBottomStairsBlock() != bottom)
                    {
                        if (d4 == d.GetCCW())
                        {
                            // inner left
                            return (d, bottom, 3);
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(d4 == d.GetCW());
                            // inner right
                            return (d, bottom, 4);
                        }
                    }
                }
            }


            // straight
            return (d, bottom, 0);
        }

        private BoundingShape GetBlockBoundingShape(BlockLocation loc)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Blocks block = _BLOCK_CTX.GetBlock(loc);

            /**
             * 0: None (Air)
             * 1: Cube
             * 2: Slab
             * 3: Stairs
             */
            int a = 0;

            switch (block)
            {
                default:
                    throw new System.NotImplementedException();
                case Blocks.Air:
                    a = 0;
                    break;
                case Blocks.Stone:
                    a = 1;
                    break;
                case Blocks.Granite:
                    a = 1;
                    break;
                case Blocks.PolishedGranite:
                    a = 1;
                    break;
                case Blocks.EastBottomOakWoodStairs:
                    a = 3;
                    break;
            }

            switch (a)
            {
                default:
                    throw new System.NotImplementedException();
                case 0:
                    return BoundingShapeGenerator.Make();
                case 1:
                    return BoundingShapeGenerator.Make(loc);
                case 3:
                    {
                        (Directions d, bool bottom, int b) = DetermineStairsBlockShape(loc, block);
                        return BoundingShapeGenerator.Make(loc, d, bottom, b);
                    }
            }

        }

        private BoundingShape[] GetBlockBoundingShapes(BoundingBox bb)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            BlockGrid grid = BlockGrid.Generate(bb);

            int count = grid.GetCount(), i = 0;
            var shapes = new BoundingShape[count];

            for (int y = grid.Min.Y; y <= grid.Max.Y; ++y)
            {
                for (int z = grid.Min.Z; z <= grid.Max.Z; ++z)
                {
                    for (int x = grid.Min.X; x <= grid.Max.X; ++x)
                    {
                        BlockLocation loc = new(x, y, z);
                        BoundingShape shape = GetBlockBoundingShape(loc);
                        shapes[i++] = shape;
                    }
                }
            }
            System.Diagnostics.Debug.Assert(i == count);

            return shapes;
        }

        private void MoveEntity(Entity entity)
        {
            (BoundingBox bb, Vector v) = entity.Integrate();
            /*System.Console.WriteLine($"bb: {bb}");*/

            BoundingBox bbTotal = bb.Extend(v);
            BoundingShape[] shapes = GetBlockBoundingShapes(bbTotal);

            bool f, onGround;
            double vx = v.X, vy = v.Y, vz = v.Z;

            for (int i = 0; i < shapes.Length; ++i)
            {
                BoundingShape shape = shapes[i];
                vy = shape.AdjustY(bb, vy);
            }
            if (!Comparing.IsEqualTo(vy, 0.0D))
            {
                bb = bb.MoveY(vy);
            }

            f = !Comparing.IsEqualTo(vy, v.Y);
            onGround = Comparing.IsLessThan(v.Y, 0.0D) && f;
            if (f)
            {
                vy = 0.0D;
            }

            for (int i = 0; i < shapes.Length; ++i)
            {
                BoundingShape shape = shapes[i];
                vx = shape.AdjustX(bb, vx);
            }
            if (!Comparing.IsEqualTo(vx, 0.0D))
            {
                bb = bb.MoveX(vx);
            }

            f = !Comparing.IsEqualTo(vx, v.X);
            if (f)
            {
                vx = 0.0D;
            }

            for (int i = 0; i < shapes.Length; ++i)
            {
                BoundingShape shape = shapes[i];
                vz = shape.AdjustZ(bb, vz);
            }
            if (!Comparing.IsEqualTo(vz, 0.0D))
            {
                bb = bb.MoveZ(vz);
            }

            f = !Comparing.IsEqualTo(vz, v.Z);
            if (f)
            {
                vz = 0.0D;
            }

            v = new(vx, vy, vz);
            entity.Move(bb, v, onGround);

            UpdateEntityChunkMapping(entity);
        }

        public void HandleEntities()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Entity? entity = null;
            while (true)
            {
                entity = _ENTITIES2.Dequeue();
                if (entity == null) break;

                bool despawn = false;

                if (entity is Player player)
                {
                    if (!player.IsConnected)
                    {
                        System.Diagnostics.Debug.Assert(!despawn);
                        System.Diagnostics.Debug.Assert(
                            _DISCONNECTED_PLAYERS.Contains(player.UniqueId));

                        despawn = DetermineToDespawnPlayerOnDisconnect();
                    }

                }
                else if (entity.IsDead())
                {
                    System.Diagnostics.Debug.Assert(entity is not Player);
                    System.Diagnostics.Debug.Assert(!despawn);

                    despawn = true;
                }

                if (despawn)
                {
                    DespawnEntity(entity);

                    _DESPAWNED_ENTITIES.Enqueue(entity);
                }
                else
                {
                    MoveEntity(entity);

                    _ENTITIES1.Enqueue(entity);
                }
                
            }

        }

        public void SpawnEntities()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Entity? entity = null;
            while (true)
            {
                entity = _ENTITY_SPAWNING_POOL.Dequeue();
                if (entity == null) break;

                if (entity is Player player)
                {
                    _PLAYER_LIST.InitPlayer(player.UniqueId, player.Username);
                    /*System.Console.WriteLine($"player.UniqueId: {player.UniqueId}");*/
                    _DISCONNECTED_PLAYERS.Insert(player.UniqueId, player);
                }

                InitEntityChunkMapping(entity);

                _ENTITIES1.Enqueue(entity);
            }
        }

        public void ReleaseResources()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            while (true)
            {
                Entity? entity = _DESPAWNED_ENTITIES.Dequeue();
                if (entity == null) break;

                _ENTITY_ID_LIST.Dealloc(entity.Id);
                entity.Dispose();
            }

        }

        public void StartEntitRoutines(long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Entity? entity;
            while (true)
            {
                entity = _ENTITIES1.Dequeue();
                if (entity == null) break;

                // TODO: Resolve Collisions with other entities.
                // TODO: Add Global Forces with OnGround flag. (Gravity, Damping Force, ...)
                {
                    entity.ApplyGlobalForce(
                            -1.0D * new Vector(1.0D - 0.91D, 1.0D - 0.9800000190734863D, 1.0D - 0.91D) *
                            entity.Velocity);  // Damping Force
                    entity.ApplyGlobalForce(entity.GetMass() * 0.08D * new Vector(0.0D, -1.0D, 0.0D));  // Gravity

                    /*entity.ApplyForce(entity.GetMass() * 0.001D * new Entity.Vector(0, -1, 0));  // Gravity*/
                }

                entity.StartRoutine(serverTicks, this);

                /*if (entity is Player player)
                {
                    StartPlayerRoutine(serverTicks, player);
                }*/

                _ENTITIES2.Enqueue(entity);

            }
        }

        public void StartRoutine(long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _PLAYER_LIST.StartRoutine(serverTicks);

            /*if (serverTicks == 20 * 5)
            {
                SpawnItemEntity();
            }*/

        }

        private void InitEntityChunkMapping(Entity entity)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            ChunkGrid grid = ChunkGrid.Generate(entity.BB);
            foreach (ChunkLocation loc in grid.GetLocations())
            {
                Table<int, Entity> entities;
                if (!_CHUNK_TO_ENTITIES.Contains(loc))
                {
                    entities = new();
                    _CHUNK_TO_ENTITIES.Insert(loc, entities);
                }
                else
                {
                    entities = _CHUNK_TO_ENTITIES.Lookup(loc);
                }
                
                entities.Insert(entity.Id, entity);
            }

            System.Diagnostics.Debug.Assert(!_ENTITY_TO_CHUNKS.Contains(entity.Id));
            _ENTITY_TO_CHUNKS.Insert(entity.Id, grid);
        }

        private void CloseEntityChunkMapping(Entity entity)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_ENTITY_TO_CHUNKS.Contains(entity.Id));
            ChunkGrid grid = _ENTITY_TO_CHUNKS.Extract(entity.Id);

            foreach (ChunkLocation loc in grid.GetLocations())
            {
                System.Diagnostics.Debug.Assert(_CHUNK_TO_ENTITIES.Contains(loc));
                Table<int, Entity> entities = _CHUNK_TO_ENTITIES.Lookup(loc);

                Entity entityInChunk = entities.Extract(entity.Id);
                System.Diagnostics.Debug.Assert(ReferenceEquals(entityInChunk, entity));

                if (entities.Empty)
                {
                    _CHUNK_TO_ENTITIES.Extract(loc);
                    entities.Dispose();
                }
            }

        }

        private void UpdateEntityChunkMapping(Entity entity)
        {
            System.Diagnostics.Debug.Assert(_ENTITY_TO_CHUNKS.Contains(entity.Id));
            ChunkGrid gridPrev = _ENTITY_TO_CHUNKS.Extract(entity.Id);
            ChunkGrid grid = ChunkGrid.Generate(entity.BB);

            if (!gridPrev.Equals(grid))
            {
                ChunkGrid? gridChunkBetween = ChunkGrid.Generate(grid, gridPrev);

                foreach (ChunkLocation loc in gridPrev.GetLocations())
                {
                    if (gridChunkBetween != null && gridChunkBetween.Contains(loc))
                    {
                        continue;
                    }

                    System.Diagnostics.Debug.Assert(_CHUNK_TO_ENTITIES.Contains(loc));
                    Table<int, Entity> entities = _CHUNK_TO_ENTITIES.Lookup(loc);

                    Entity entityInChunk = entities.Extract(entity.Id);
                    System.Diagnostics.Debug.Assert(ReferenceEquals(entityInChunk, entity));

                    if (entities.Empty)
                    {
                        _CHUNK_TO_ENTITIES.Extract(loc);
                        entities.Dispose();
                    }
                }

                foreach (ChunkLocation loc in grid.GetLocations())
                {
                    if (gridChunkBetween != null && gridChunkBetween.Contains(loc))
                    {
                        continue;
                    }

                    Table<int, Entity> entities;

                    if (!_CHUNK_TO_ENTITIES.Contains(loc))
                    {
                        entities = new();
                        _CHUNK_TO_ENTITIES.Insert(loc, entities);
                    }
                    else
                    {
                        entities = _CHUNK_TO_ENTITIES.Lookup(loc);
                    }

                    entities.Insert(entity.Id, entity);
                }

            }

            _ENTITY_TO_CHUNKS.Insert(entity.Id, grid);
        }

        internal (int, byte[]) GetChunkData(ChunkLocation loc)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return _BLOCK_CTX.GetChunkData(loc);
        }

        internal System.Collections.Generic.IEnumerable<Entity> GetEntities(ChunkLocation loc)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (!_CHUNK_TO_ENTITIES.Contains(loc))
            {
                return [];
            }

            Table<int, Entity> entities = _CHUNK_TO_ENTITIES.Lookup(loc);
            return entities.GetValues();
        }

        /*public Entity RaycastClosestEntity(Entity.Vector p, Entity.Vector u, int d)
        {
            throw new System.NotImplementedException();
        }

        public Entity[] RaycastEntities(Entity.Vector p, Entity.Vector u, int d)
        {
            throw new System.NotImplementedException();
        }

        public Entity[] GetCollidedEntities(bool resolve)
        {
            throw new System.NotImplementedException();
        }*/

        public virtual void Dispose()
        {
            // Assertion.
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_ENTITY_ID_LIST.Empty);

            System.Diagnostics.Debug.Assert(_ENTITY_SPAWNING_POOL.Empty);
            System.Diagnostics.Debug.Assert(_ENTITIES1.Empty);
            System.Diagnostics.Debug.Assert(_ENTITIES2.Empty);
            System.Diagnostics.Debug.Assert(_DESPAWNED_ENTITIES.Empty);

            System.Diagnostics.Debug.Assert(_DISCONNECTED_PLAYERS.Empty);

            System.Diagnostics.Debug.Assert(_CHUNK_TO_ENTITIES.Empty);
            System.Diagnostics.Debug.Assert(_ENTITY_TO_CHUNKS.Empty);


            // Release resources.
            _PLAYER_LIST.Dispose();

            _ENTITY_ID_LIST.Dispose();

            _ENTITY_SPAWNING_POOL.Dispose();
            _ENTITIES1.Dispose();
            _ENTITIES2.Dispose();
            _DESPAWNED_ENTITIES.Dispose();

            _DISCONNECTED_PLAYERS.Dispose();

            _BLOCK_CTX.Dispose();

            _CHUNK_TO_ENTITIES.Dispose();
            _ENTITY_TO_CHUNKS.Dispose();

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }
       

    }


}

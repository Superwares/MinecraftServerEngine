using Containers;

namespace Protocol
{
    public abstract class World : System.IDisposable
    {
        private bool _disposed = false;

        private readonly object _SHARED_OBJECT = new();

        private readonly PlayerList _PLAYER_LIST = new();  // Disposable

        private readonly NumList _ENTITY_ID_LIST = new();  // Disposable

        private readonly Entity.Vector _POS_SPAWNING;
        private readonly Entity.Angles _LOOK_SPAWNING;

        private readonly Table<Chunk.Vector, Chunk> _CHUNKS = new();  // Disposable

        private readonly DualQueue<Entity> _ENTITY_SPAWNING_POOL = new();  // Disposable
        private readonly DualQueue<Entity> _ENTITIES = new();  // Disposable
        private readonly ConcurrentQueue<Entity> _DESPAWNED_ENTITIES = new();  // Disposable

        private readonly Table<System.Guid, Player> _DISCONNECTED_PLAYERS = new(); // Disposable

        private readonly 
            Table<Chunk.Vector, Table<int, Entity>> _CHUNK_TO_ENTITIES = new();  // Disposable
        private readonly Table<int, Chunk.Grid> _ENTITY_TO_CHUNKS = new();  // Disposable


        /*internal PublicInventory _Inventory = new ChestInventory();*/

        public World(Entity.Vector posSpawning, Entity.Angles lookSpawning)
        {
            _POS_SPAWNING = posSpawning; _LOOK_SPAWNING = lookSpawning;

            {
                // Dummy code.
                for (int z = -10; z <= 10; ++z)
                {
                    for (int x = -10; x <= 10; ++x)
                    {
                        Chunk.Vector p = new(x, z);
                        Chunk c = new Chunk(p);
                        _CHUNKS.Insert(p, c);
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

        protected abstract bool DetermineNewPlayerCanJoinWorld();

        internal bool CanJoinWorld(System.Guid uniqueId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (_DISCONNECTED_PLAYERS.Contains(uniqueId))
            {
                return true;
            }

            return DetermineNewPlayerCanJoinWorld();
        }

        /*internal bool CanSpawnOrGetPlayer(System.Guid uniqueId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (_DISCONNECTED_PLAYERS.Contains(uniqueId))
            {
                System.Diagnostics.Debug.Assert(!_DESPAWNING_PLAYER_IDS.Contains(uniqueId));
                return true;
            }

            return !_DESPAWNING_PLAYER_IDS.Contains(uniqueId);
        }*/

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
                    _POS_SPAWNING, _LOOK_SPAWNING,
                    username);

                _ENTITY_SPAWNING_POOL.Enqueue(player);
            }

            return player;
        }

        internal ItemEntity SpawnItemEntity()
        {
            int id = _ENTITY_ID_LIST.Alloc();
            ItemEntity itemEntity = new(id, _POS_SPAWNING, _LOOK_SPAWNING);

            _ENTITY_SPAWNING_POOL.Enqueue(itemEntity);

            System.Console.WriteLine("SpawnItemEntity!");

            return itemEntity;
        }

        public Block GetBlock(Block.Vector p)
        {
            Chunk.Vector pChunk = Chunk.Vector.Convert(p);
            if (!_CHUNKS.Contains(pChunk))
            {
                return new Block(Block.Types.Air);
            }

            Chunk c = _CHUNKS.Lookup(pChunk);
            return c.GetBlock(p);
        }

        private (Entity.Vector, bool) AdjustPosition(
            Entity.Vector pEntity, Entity.BoundingBox bbEntity)
        {
            Block.Grid grid = Block.Grid.Generate(pEntity, bbEntity);

            bool onGround = false;

            foreach (Block.Vector pBlock in grid.GetVectors())
            {
                Block block = GetBlock(pBlock);
                if (block.Type == Block.Types.Air)
                {
                    continue;
                }

                Entity.BoundingBox bbBlock = Entity.BoundingBox.GetBlockBB();
                Entity.Grid gridEntity = Entity.Grid.Generate(pEntity, bbEntity),
                            gridBlock = Entity.Grid.Generate(pBlock, bbBlock);

                Entity.Vector centerBlock = gridBlock.GetCenter(),
                       centerEntity = gridEntity.GetCenter();

                if (gridBlock.Contains(centerEntity) || gridEntity.Contains(centerBlock))
                {
                    // inside
                    System.Diagnostics.Debug.Assert(false);
                }

                Entity.Grid? gridOverlapped = Entity.Grid.Generate(gridEntity, gridBlock);
                if (gridOverlapped == null)
                {
                    // outside
                    System.Diagnostics.Debug.Assert(false);
                }

                if (centerEntity.Y > gridBlock.MAX.Y)
                {
                    // top
                    double h1 = (bbEntity.Height + bbBlock.Height) / 2;
                    System.Diagnostics.Debug.Assert(h1 > 0);

                    double h2 = centerEntity.Y - centerBlock.Y;
                    System.Diagnostics.Debug.Assert(h2 > 0);

                    double h3 = h1 - h2;
                    System.Diagnostics.Debug.Assert(h3 > 0);

                    pEntity = new Entity.Vector(pEntity.X, pEntity.Y + h3, pEntity.Z);
                }
                else if (centerEntity.Y < gridBlock.MIN.Y)
                {
                    // bottom
                    // top
                    double h1 = (bbEntity.Height + bbBlock.Height) / 2;
                    System.Diagnostics.Debug.Assert(h1 > 0);

                    double h2 = centerBlock.Y - centerEntity.Y;
                    System.Diagnostics.Debug.Assert(h2 > 0);

                    double h3 = h1 - h2;
                    System.Diagnostics.Debug.Assert(h3 > 0);

                    pEntity = new Entity.Vector(pEntity.X, pEntity.Y - h3, pEntity.Z);
                }
                else
                {
                    bool f1 = centerEntity.Z > (centerEntity.X - centerBlock.X) + centerBlock.Z,
                     f2 = centerEntity.Z > -(centerEntity.X - centerBlock.X) + centerBlock.Z;
                    if (f1 && f2)
                    {
                        // front
                        throw new System.NotFiniteNumberException();
                    }
                    else if (f1)
                    {
                        System.Diagnostics.Debug.Assert(!f2);
                        // left
                        throw new System.NotFiniteNumberException();
                    }
                    else if (f2)
                    {
                        System.Diagnostics.Debug.Assert(!f1);
                        // right
                        throw new System.NotFiniteNumberException();
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(!f1 && !f2);
                        // bottom
                        throw new System.NotFiniteNumberException();
                    }
                }


            }

            return (pEntity, onGround);
        }

        private void DespawnEntity(Entity entity)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            CloseEntityRendering(entity);

            entity.Flush();

            _DESPAWNED_ENTITIES.Enqueue(entity);
        }

        public void HandleEntities()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Entity? entity = null;
            while (true)
            {
                entity = _ENTITIES.Dequeue();
                if (entity == null)
                {
                    break;
                }

                bool despawn = false;

                if (entity is Player player)
                {
                    if (!player.IsConnected)
                    {
                        System.Diagnostics.Debug.Assert(
                            _DISCONNECTED_PLAYERS.Contains(player.UniqueId));

                        if (DetermineToDespawnPlayerOnDisconnect())
                        {
                            Player playerExtracted =
                                    _DISCONNECTED_PLAYERS.Extract(player.UniqueId);
                            System.Diagnostics.Debug.Assert(
                                ReferenceEquals(playerExtracted, player));

                            _PLAYER_LIST.ClosePlayer(player.UniqueId);

                            despawn = true;
                        }
                        else
                        {
                            
                        }
                    }
                    else
                    {
                        if (entity.IsDead())
                        {
                            System.Diagnostics.Debug.Assert(!despawn);
                            despawn = true;
                        }
                    }
                }

                if (despawn)
                {
                    DespawnEntity(entity);
                    continue;
                }

                {
                    Entity.Vector p = entity.Integrate();

                    (Entity.Vector pAdjusted, bool onGround) = 
                        AdjustPosition(p, entity.GetBoundingBox());

                    entity.Move(pAdjusted, onGround);

                    UpdateEntityRendering(entity);
                }


                _ENTITIES.Enqueue(entity);
            }

        }

        public void SpawnEntities()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Entity? entity = null;
            while (true)
            {
                entity = _ENTITY_SPAWNING_POOL.Dequeue();
                if (entity == null)
                {
                    break;
                }

                if (entity is Player player)
                {
                    _PLAYER_LIST.InitPlayer(player.UniqueId, player.Username);
                    /*System.Console.WriteLine($"player.UniqueId: {player.UniqueId}");*/
                    _DISCONNECTED_PLAYERS.Insert(player.UniqueId, player);
                }

                (Entity.Vector p, bool onGround) =
                        AdjustPosition(entity.Position, entity.GetBoundingBox());

                entity.Spawn(p, onGround);

                InitEntityRendering(entity);

                _ENTITIES.Enqueue(entity);
            }
        }

        public void ReleaseResources()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            while (true)
            {
                Entity? entity = _DESPAWNED_ENTITIES.Dequeue();
                if (entity == null)
                {
                    break;
                }

                _ENTITY_ID_LIST.Dealloc(entity.Id);
                entity.Dispose();
            }

        }

        public void Reset()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _ENTITY_SPAWNING_POOL.Switch();
            _ENTITIES.Switch();
        }

        protected abstract bool DetermineToDespawnPlayerOnDisconnect();

        public void StartEntitRoutines(long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Entity? entity;
            while (true)
            {
                entity = _ENTITIES.Dequeue();
                if (entity == null)
                {
                    break;
                }

                // TODO: Resolve Collisions with other entities.
                // TODO: Add Global Forces with OnGround flag. (Gravity, Damping Force, ...)
                {
                    entity.ApplyGlobalForce(
                            new Entity.Vector(-(1.0D - 0.91D), -(1.0D - 0.9800000190734863D), -(1.0D - 0.91D)) *
                            entity.Velocity);  // Damping Force
                    entity.ApplyGlobalForce(entity.GetMass() * 0.08D * new Entity.Vector(0, -1, 0));  // Gravity
                }

                entity.StartRoutine(serverTicks, this);

                /*if (entity is Player player)
                {
                    StartPlayerRoutine(serverTicks, player);
                }*/

            }
        }

        protected abstract void StartPlayerRoutine(long serverTicks, Player player);

        protected abstract void StartSubRoutine(long serverTicks);

        public void StartRoutine(long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _PLAYER_LIST.StartRoutine(serverTicks);

            /*if (serverTicks == 20 * 5)
            {
                SpawnItemEntity();
            }*/

            StartSubRoutine(serverTicks);
        }

        private void InitEntityRendering(Entity entity)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Chunk.Grid grid = Chunk.Grid.Generate(entity.Position, entity.GetBoundingBox());
            foreach (Chunk.Vector p in grid.GetVectors())
            {
                if (!_CHUNK_TO_ENTITIES.Contains(p))
                {
                    _CHUNK_TO_ENTITIES.Insert(p, new());
                }

                Table<int, Entity> entities = _CHUNK_TO_ENTITIES.Lookup(p);
                entities.Insert(entity.Id, entity);
            }

            System.Diagnostics.Debug.Assert(!_ENTITY_TO_CHUNKS.Contains(entity.Id));
            _ENTITY_TO_CHUNKS.Insert(entity.Id, grid);
        }

        private void CloseEntityRendering(Entity entity)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_ENTITY_TO_CHUNKS.Contains(entity.Id));
            Chunk.Grid grid = _ENTITY_TO_CHUNKS.Extract(entity.Id);

            foreach (Chunk.Vector p in grid.GetVectors())
            {
                Table<int, Entity> entities = _CHUNK_TO_ENTITIES.Lookup(p);

                Entity entityInChunk = entities.Extract(entity.Id);
                System.Diagnostics.Debug.Assert(ReferenceEquals(entityInChunk, entity));

                if (entities.Empty)
                {
                    _CHUNK_TO_ENTITIES.Extract(p);
                    entities.Dispose();
                }
            }

        }

        internal bool ContainsChunk(Chunk.Vector p)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return _CHUNKS.Contains(p);
        }

        internal Chunk GetChunk(Chunk.Vector p)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return _CHUNKS.Lookup(p);
        }

        internal bool ContainsEntities(Chunk.Vector p)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return _CHUNK_TO_ENTITIES.Contains(p);
        }

        internal System.Collections.Generic.IEnumerable<Entity> GetEntities(Chunk.Vector p)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Table<int, Entity> entities = _CHUNK_TO_ENTITIES.Lookup(p);
            foreach (Entity entity in entities.GetValues())
            {
                yield return entity;
            }
        }

        internal void UpdateEntityRendering(Entity entity)
        {
            System.Diagnostics.Debug.Assert(_ENTITY_TO_CHUNKS.Contains(entity.Id));
            Chunk.Grid gridPrev = _ENTITY_TO_CHUNKS.Extract(entity.Id);
            Chunk.Grid grid = Chunk.Grid.Generate(entity.Position, entity.GetBoundingBox());

            /*System.Console.WriteLine();*/
            /*System.Console.Write("gridPrev");
            gridPrev.Print();*/
            /*System.Console.Write($"grid: {grid}");*/

            if (!gridPrev.Equals(grid))
            {
                Chunk.Grid? gridBetween = Chunk.Grid.Generate(grid, gridPrev);
                /*System.Console.Write("gridBetween");
                gridBetween.Print();*/

                foreach (Chunk.Vector pChunk in gridPrev.GetVectors())
                {
                    if (gridBetween != null && gridBetween.Contains(pChunk))
                    {
                        continue;
                    }

                    /*System.Console.WriteLine($"pChunk: ({pChunk.X}, {pChunk.Z})");*/
                    Table<int, Entity> entities = _CHUNK_TO_ENTITIES.Lookup(pChunk);

                    Entity entityInChunk = entities.Extract(entity.Id);
                    System.Diagnostics.Debug.Assert(ReferenceEquals(entityInChunk, entity));

                    if (entities.Empty)
                    {
                        _CHUNK_TO_ENTITIES.Extract(pChunk);
                        entities.Dispose();
                    }
                }

                foreach (Chunk.Vector pChunk in grid.GetVectors())
                {
                    if (gridBetween != null && gridBetween.Contains(pChunk))
                    {
                        continue;
                    }

                    if (!_CHUNK_TO_ENTITIES.Contains(pChunk))
                    {
                        _CHUNK_TO_ENTITIES.Insert(pChunk, new());
                    }

                    Table<int, Entity> entities = _CHUNK_TO_ENTITIES.Lookup(pChunk);
                    entities.Insert(entity.Id, entity);
                }

            }

            _ENTITY_TO_CHUNKS.Insert(entity.Id, grid);
        }

        public Entity RaycastClosestEntity(Entity.Vector p, Entity.Vector u, int d)
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
        }

        public virtual void Dispose()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // Assertion.
            System.Diagnostics.Debug.Assert(_ENTITY_ID_LIST.Empty);

            System.Diagnostics.Debug.Assert(_CHUNKS.Empty);

            System.Diagnostics.Debug.Assert(_ENTITY_SPAWNING_POOL.Empty);

            System.Diagnostics.Debug.Assert(_DISCONNECTED_PLAYERS.Empty);

            System.Diagnostics.Debug.Assert(_CHUNK_TO_ENTITIES.Empty);
            System.Diagnostics.Debug.Assert(_ENTITY_TO_CHUNKS.Empty);

            System.Diagnostics.Debug.Assert(_DESPAWNED_ENTITIES.Empty);

            // Release resources.
            _PLAYER_LIST.Dispose();

            _ENTITY_ID_LIST.Dispose();

            _CHUNKS.Dispose();

            _ENTITY_SPAWNING_POOL.Dispose();

            _DISCONNECTED_PLAYERS.Dispose();

            _CHUNK_TO_ENTITIES.Dispose();
            _ENTITY_TO_CHUNKS.Dispose();

            _DESPAWNED_ENTITIES.Dispose();

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }
       

    }


}

using Containers;
using System;

namespace Protocol
{
    public abstract class World : System.IDisposable
    {
        private bool _disposed = false;

        private readonly PlayerList _PLAYER_LIST = new();  // Disposable

        private readonly NumList _ENTITY_ID_LIST = new();  // Disposable

        private readonly Entity.Vector _POS_SPAWNING;
        private readonly Entity.Angles _LOOK_SPAWNING;

        private readonly Table<Chunk.Vector, Chunk> _CHUNKS = new();  // Disposable

        private readonly Queue<Entity> _ENTITY_SPAWNING_POOL = new();  // Disposable

        private readonly Table<System.Guid, Player> _DISCONNECTED_PLAYERS = new(); // Disposable

        private readonly 
            Table<Chunk.Vector, Table<int, Entity>> _CHUNK_TO_ENTITIES = new();  // Disposable
        private readonly Table<int, Chunk.Grid> _ENTITY_TO_CHUNKS = new();  // Disposable

        private readonly Queue<Entity> _DESPAWNED_ENTITIES = new();  // Disposable

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

        public void F(Entity.Vector pEntity, BoundingBox boundingBox)
        {
            Block.Grid grid = Block.Grid.Generate(pEntity, boundingBox);
            foreach (Block.Vector pBlock in grid.GetVectors())
            {

            }
        }

        public bool HandleEntity(Entity entity)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            {
                bool despawnEntity = false;

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

                            despawnEntity = true;
                        }
                        else
                        {
                            
                        }
                    }
                    else
                    {
                        if (entity.IsDead())
                        {
                            System.Diagnostics.Debug.Assert(!despawnEntity);
                            despawnEntity = true;
                        }
                    }
                }

                if (despawnEntity)
                {
                    CloseEntityRendering(entity);

                    entity.Flush();

                    _DESPAWNED_ENTITIES.Enqueue(entity);

                    return true;
                }

                
            }

            {
                entity.ApplyBaseForce(
                        new Entity.Vector(-(1.0D - 0.91D), -(1.0D - 0.9800000190734863D), -(1.0D - 0.91D)) *
                        entity.Velocity);  // Damping Force
                entity.ApplyBaseForce(entity.GetMass() * 0.08D * new Entity.Vector(0, -1, 0));  // Gravity
            }


            {

                Entity.Vector p = entity.Integrate();

                // Test collide with blocks and adjust position.
                {
                    int xMax, yMax, zMax,
                        xMin, yMin, zMin;
                    


                }

                entity.UpdateMovement(p, false);

                UpdateEntityRendering(entity);
            }


            return false;
        }

        public void SpawnEntities(Queue<Entity> entities)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            while (!_ENTITY_SPAWNING_POOL.Empty)
            {
                Entity entity = _ENTITY_SPAWNING_POOL.Dequeue();

                if (entity is Player player)
                {
                    _PLAYER_LIST.InitPlayer(player.UniqueId, player.Username);
                    /*System.Console.WriteLine($"player.UniqueId: {player.UniqueId}");*/
                    _DISCONNECTED_PLAYERS.Insert(player.UniqueId, player);
                }

                InitEntityRendering(entity);

                entities.Enqueue(entity);
            }
        }

        public void ReleaseResources()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            while (!_DESPAWNED_ENTITIES.Empty)
            {
                Entity entity = _DESPAWNED_ENTITIES.Dequeue();

                _ENTITY_ID_LIST.Dealloc(entity.Id);
                entity.Dispose();
            }
        }

        protected abstract bool DetermineToDespawnPlayerOnDisconnect();

        public void StartEntitRoutine(long serverTicks, Entity entity)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            entity.StartRoutine(serverTicks, this);

            /*if (entity is Player player)
            {
                StartPlayerRoutine(serverTicks, player);
            }*/

            return;
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

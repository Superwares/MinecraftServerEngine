using Common;
using Containers;
using Threading;

namespace Protocol
{
    public abstract class World : System.IDisposable
    {
        private bool _disposed = false;

        private readonly Vector _P_SPAWE;
        private readonly Entity.Angles _LOOK_SPAWE;

        internal readonly PlayerList _PLAYER_LIST = new();  // Disposable

        private readonly ConcurrentNumList _ENTITY_ID_LIST = new();  // Disposable

        private readonly ConcurrentQueue<Entity> _ENTITY_SPAWNING_POOL = new();  // Disposable
        private readonly ParallelQueue<Entity> _ENTITIES = new();  // Disposable
        private readonly ParallelQueue<Player> _PLAYERS = new();
        private readonly ConcurrentQueue<Entity> _DESPAWNED_ENTITIES = new();  // Disposable

        private readonly ConcurrentTable<System.Guid, Player> _DISCONNECTED_PLAYERS = new(); // Disposable

        internal readonly BlockContext _BLOCK_CTX = new();  // Disposable

        internal readonly EntityContext _ENTITY_CTX = new();  // Disposable

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

        public abstract bool CanJoinWorld();

        internal ItemEntity SpawnItemEntity()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            int id = _ENTITY_ID_LIST.Alloc();
            ItemEntity itemEntity = new(id, _P_SPAWE, _LOOK_SPAWE);

            _ENTITY_SPAWNING_POOL.Enqueue(itemEntity);

            return itemEntity;
        }

        protected abstract bool DetermineToDespawnPlayerOnDisconnect();

        public void StartRoutine(long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            /*if (serverTicks == 20 * 5)
            {
                SpawnItemEntity();
            }*/
        }

        public void StartEntityRoutine(long serverTicks, Entity entity)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            entity.StartRoutine(serverTicks, this);
        }

        public void StartEntityRoutines(long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Entity? entity = null;
            while (_ENTITIES.Dequeue(ref entity))
            {
                System.Diagnostics.Debug.Assert(entity != null);

                System.Diagnostics.Debug.Assert(entity is not Player);

                // TODO: Resolve Collisions with other entities.
                // TODO: Add Global Forces with OnGround flag. (Gravity, Damping Force, ...)
                {
                    entity.ApplyGlobalForce(
                            -1.0D * new Vector(1.0D - 0.91D, 1.0D - 0.9800000190734863D, 1.0D - 0.91D) *
                            entity.Velocity);  // Damping Force
                    entity.ApplyGlobalForce(entity.GetMass() * 0.08D * new Vector(0.0D, -1.0D, 0.0D));  // Gravity

                    /*entity.ApplyForce(entity.GetMass() * 0.001D * new Entity.Vector(0, -1, 0));  // Gravity*/
                }

                StartEntityRoutine(serverTicks, entity);

                _ENTITIES.Enqueue(entity);

            }
        }

        public void StartPlayerRoutines(long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Player? player = null;
            while (_PLAYERS.Dequeue(ref player))
            {
                System.Diagnostics.Debug.Assert(player != null);

                StartEntityRoutine(serverTicks, player);

                _PLAYERS.Enqueue(player);
            }
        }

        public void HandlePlayerConnections(long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Player? player = null;
            while (_PLAYERS.Dequeue(ref player))
            {
                System.Diagnostics.Debug.Assert(player != null);

                if (player.HandlePlayerConnection(this))
                {
                    System.Diagnostics.Debug.Assert(!_DISCONNECTED_PLAYERS.Contains(userId));
                    _DISCONNECTED_PLAYERS.Insert(userId, player);
                    
                }

                _PLAYERS.Enqueue(player);

            }
        }

        private void DespawnEntity(Entity entity)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (entity is Player player)
            {
                _PLAYER_LIST.Remove(player.UniqueId);

                Player extracted = _DISCONNECTED_PLAYERS.Extract(player.UniqueId);
                System.Diagnostics.Debug.Assert(ReferenceEquals(extracted, player));
            }

            CloseEntityChunkMapping(entity);

            entity.Flush();

            _DESPAWNED_ENTITIES.Enqueue(entity);
        }

        public void DespawnEntities()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Entity? entity = null;
            while (_ENTITIES.Dequeue(ref entity))
            {
                System.Diagnostics.Debug.Assert(entity != null);

                System.Diagnostics.Debug.Assert(entity is not Player);

                if (entity.IsDead())
                {
                    DespawnEntity(entity);
                }
                else
                {
                    _ENTITIES.Enqueue(entity);
                }

            }
        }

        public void DespawnPlayers()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Player? player = null;
            while (_PLAYERS.Dequeue(ref player))
            {
                System.Diagnostics.Debug.Assert(player != null);

                if (!player.Connected)
                {
                    System.Diagnostics.Debug.Assert(
                        _DISCONNECTED_PLAYERS.Contains(player.UniqueId));

                    if (DetermineToDespawnPlayerOnDisconnect())
                    {
                        DespawnEntity(player);
                    }
                }
                else
                {
                    _PLAYERS.Enqueue(player);
                }
                
            }
        }

        private void MoveEntity(Entity entity)
        {
            (BoundingBox bb, Vector v) = entity.Integrate();

            BoundingBox bbTotal = bb.Extend(v);
            BoundingShape[] shapes = _BLOCK_CTX.GetBlockShapes(bbTotal);

            bool f, onGround;
            double vx = v.X, vy = v.Y, vz = v.Z;

            for (int i = 0; i < shapes.Length; ++i)
            {
                BoundingShape shape = shapes[i];
                vy = shape.AdjustY(bb, vy);
            }
            if (vy != 0.0D)
            {
                bb = bb.MoveY(vy);
            }

            f = vy != v.Y;
            onGround = (v.Y < 0.0D) && f;
            if (f)
            {
                vy = 0.0D;
            }

            for (int i = 0; i < shapes.Length; ++i)
            {
                BoundingShape shape = shapes[i];
                vx = shape.AdjustX(bb, vx);
            }
            if (vx != 0.0D)
            {
                bb = bb.MoveX(vx);
            }

            f = vx != v.X;
            if (f)
            {
                vx = 0.0D;
            }

            for (int i = 0; i < shapes.Length; ++i)
            {
                BoundingShape shape = shapes[i];
                vz = shape.AdjustZ(bb, vz);
            }
            if (vz != 0.0D)
            {
                bb = bb.MoveZ(vz);
            }

            f = vz != v.Z;
            if (f)
            {
                vz = 0.0D;
            }

            v = new(vx, vy, vz);
            entity.Move(bb, v, onGround);

            UpdateEntityChunkMapping(entity);
        }

        public void MoveEntities()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Entity? entity = null;
            while (_ENTITIES.Dequeue(ref entity))
            {
                System.Diagnostics.Debug.Assert(entity != null);

                MoveEntity(entity);

                _ENTITIES.Enqueue(entity);
            }

        }

        public void SpawnEntities()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Entity? entity = null;
            while (_ENTITY_SPAWNING_POOL.Dequeue(ref entity))
            {
                System.Diagnostics.Debug.Assert(entity != null);

                System.Diagnostics.Debug.Assert(entity is not Player);

                InitEntityChunkMapping(entity);

                _ENTITIES.Enqueue(entity);
            }
        }

        internal void SpawnOrConnectPlayer(Client client, string username, System.Guid userId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Player player;

            if (_DISCONNECTED_PLAYERS.Contains(userId))
            {
                player = _DISCONNECTED_PLAYERS.Extract(userId);
            }
            else
            {
                int id = _ENTITY_ID_LIST.Alloc();
                player = new(
                    id,
                    userId,
                    _P_SPAWE, _LOOK_SPAWE,
                    username);

                _PLAYER_LIST.Add(player.UniqueId, player.Username);
            }

            
            player.Connect(client, this);

            _PLAYERS.Enqueue(player);
        }

        public void HandlePlayerRenders()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Player? player = null;
            while (_PLAYERS.Dequeue(ref player))
            {
                System.Diagnostics.Debug.Assert(player != null);

                player.Render(this);

                _PLAYERS.Enqueue(player);
            }
        }

        public void ReleaseResources()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Entity? entity = null;
            while (_DESPAWNED_ENTITIES.Dequeue(ref entity))
            {
                System.Diagnostics.Debug.Assert(entity != null);

                _ENTITY_ID_LIST.Dealloc(entity.Id);

                entity.Dispose();
            }

        }

        public virtual void Dispose()
        {
            // Assertion.
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_ENTITY_ID_LIST.Empty);

            System.Diagnostics.Debug.Assert(_ENTITY_SPAWNING_POOL.Empty);
            System.Diagnostics.Debug.Assert(_ENTITIES.Empty);
            System.Diagnostics.Debug.Assert(_DESPAWNED_ENTITIES.Empty);

            System.Diagnostics.Debug.Assert(_DISCONNECTED_PLAYERS.Empty);

            System.Diagnostics.Debug.Assert(_CHUNK_TO_ENTITIES.Empty);
            System.Diagnostics.Debug.Assert(_ENTITY_TO_CHUNKS.Empty);

            // Release resources.
            _PLAYER_LIST.Dispose();

            _ENTITY_ID_LIST.Dispose();

            _ENTITY_SPAWNING_POOL.Dispose();
            _ENTITIES.Dispose();
            _DESPAWNED_ENTITIES.Dispose();

            _DISCONNECTED_PLAYERS.Dispose();

            _BLOCK_CTX.Dispose();

            _MUTEX.Dispose();
            _CHUNK_TO_ENTITIES.Dispose();
            _ENTITY_TO_CHUNKS.Dispose();

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }
       

    }


}

using Common;
using Containers;
using PhysicsEngine;

namespace MinecraftServerEngine
{
    public abstract class World : PhysicsWorld
    {
        private bool _disposed = false;

        internal readonly PlayerList PlayerList = new();  // Disposable

        private readonly Queue<Entity> EntitySpawningPool = new();  // Disposable
        internal readonly SwapQueue<Entity> Entities = new();  // Disposable
        internal readonly SwapQueue<Player> Players = new();

        private readonly Table<System.Guid, Player> DisconnectedPlayers = new(); // Disposable

        internal readonly BlockContext BlockContext = new();  // Disposable

        /*internal PublicInventory _Inventory = new ChestInventory();*/

        public World()
        {
            // Dummy code.
            for (int z = -10; z <= 10; ++z)
            {
                for (int x = -10; x <= 10; ++x)
                {
                    BlockLocation loc = new(x, 100, z);

                    BlockContext.SetBlock(loc, Blocks.Stone);
                }
            }

        }

        ~World() => System.Diagnostics.Debug.Assert(false);

        public override IBoundingVolume[] GetTerrainBoundingVolumes(IBoundingVolume volume)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            switch (volume)
            {
                default:
                    throw new System.NotImplementedException();
                case AxisAlignedBoundingBox aabb:
                    return BlockContext.GetBlockBoundingVolumes(aabb.Max, aabb.Min);
            }
        }

        public abstract bool CanJoinWorld();

        public void SpawnEntity(Entity entity)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            EntitySpawningPool.Enqueue(entity);

            throw new System.NotImplementedException();
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

        public void StartEntityRoutine(long serverTicks, Entity entity, bool serversidePhysics)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // TODO: Resolve Collisions with other entities.
            // TODO: Add Global Forces with OnGround flag. (Gravity, Damping Force, ...)
            if (serversidePhysics) 
            {
                entity.ApplyForce(
                        -1.0D * new Vector(1.0D - 0.91D, 1.0D - 0.9800000190734863D, 1.0D - 0.91D) *
                        entity.Velocity);  // Damping Force
                entity.ApplyForce(entity.GetMass() * 0.08D * new Vector(0.0D, -1.0D, 0.0D));  // Gravity

                /*entity.ApplyForce(entity.GetMass() * 0.001D * new Entity.Vector(0, -1, 0));  // Gravity*/
            }

            entity.StartInternalRoutine(serverTicks, this);
            entity.StartRoutine(serverTicks, this);
        }

        public void StartEntityRoutines(long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            try
            {
                do
                {
                    Entity entity = Entities.Dequeue();

                    System.Diagnostics.Debug.Assert(entity is not Player);

                    StartEntityRoutine(serverTicks, entity, true);

                    Entities.Enqueue(entity);
                } while (true);
            }
            catch (EmptyContainerException)
            {

            }

            System.Diagnostics.Debug.Assert(Entities.Empty);
        }

        public void StartPlayerRoutines(long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            try
            {
                do
                {
                    Player player = Players.Dequeue();

                    StartEntityRoutine(serverTicks, player, !player.Connected);

                    Players.Enqueue(player);
                } while (true);

            }
            catch (EmptyContainerException)
            {

            }

            System.Diagnostics.Debug.Assert(Players.Empty);
        }

        public void HandlePlayerConnections(long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            try
            {
                do
                {
                    Player player = Players.Dequeue();

                    if (player.HandlePlayerConnection(this))
                    {
                        System.Guid userId = player.UniqueId;

                        System.Diagnostics.Debug.Assert(!DisconnectedPlayers.Contains(userId));
                        DisconnectedPlayers.Insert(userId, player);

                    }

                    Players.Enqueue(player);
                } while (true);

            }
            catch (EmptyContainerException)
            {

            }

            System.Diagnostics.Debug.Assert(Players.Empty);
        }

        private void DestroyEntity(Entity entity)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (entity is Player player)
            {
                PlayerList.Remove(player.UniqueId);

                Player playerExtracted = DisconnectedPlayers.Extract(player.UniqueId);
                System.Diagnostics.Debug.Assert(ReferenceEquals(playerExtracted, player));
            }

            CloseObjectMapping(entity);

            entity.Flush();
            entity.Dispose();
        }

        public void DestroyEntities()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            try
            {
                do
                {
                    Entity entity = Entities.Dequeue();

                    System.Diagnostics.Debug.Assert(entity is not Player);

                    if (entity.IsDead())
                    {
                        DestroyEntity(entity);
                    }
                    else
                    {
                        Entities.Enqueue(entity);
                    }

                } while (true);
            }
            catch (EmptyContainerException)
            {

            }

            System.Diagnostics.Debug.Assert(Entities.Empty);
        }

        public void DestroyPlayers()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            try
            {
                do
                {
                    Player player = Players.Dequeue();

                    if (!player.Connected)
                    {
                        System.Diagnostics.Debug.Assert(
                            DisconnectedPlayers.Contains(player.UniqueId));

                        if (DetermineToDespawnPlayerOnDisconnect())
                        {
                            DestroyEntity(player);
                        }
                    }
                    else
                    {
                        Players.Enqueue(player);
                    }
                } while (true);

            }
            catch (EmptyContainerException)
            {

            }

            System.Diagnostics.Debug.Assert(Players.Empty);
        }

        public void MoveEntity(Entity entity)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            MoveObject(entity);
            UpdateObjectMapping(entity);
        }

        public void MoveEntities()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            try
            {
                do
                {
                    Entity entity = Entities.Dequeue();

                    MoveEntity(entity);

                    Entities.Enqueue(entity);
                } while (true);
            }
            catch (EmptyContainerException)
            {

            }

            System.Diagnostics.Debug.Assert(Entities.Empty);
        }

        public void MovePlayers()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            try
            {
                do
                {
                    Player player = Players.Dequeue();

                    MoveEntity(player);

                    Players.Enqueue(player);
                } while (true);

            }
            catch (EmptyContainerException)
            {

            }

            System.Diagnostics.Debug.Assert(Players.Empty);
        }

        public void CreateEntities()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            try
            {
                do
                {
                    Entity entity = EntitySpawningPool.Dequeue();

                    System.Diagnostics.Debug.Assert(entity is not Player);

                    InitObjectMapping(entity);

                    Entities.Enqueue(entity);
                } while (true);
            }
            catch (EmptyContainerException)
            {

            }

            System.Diagnostics.Debug.Assert(EntitySpawningPool.Empty);
        }

        protected abstract Player CreatePlayer();

        internal void CreateOrConnectPlayer(Client client, string username, System.Guid userId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Player player;

            if (DisconnectedPlayers.Contains(userId))
            {
                player = DisconnectedPlayers.Extract(userId);
            }
            else
            {
                player = CreatePlayer();

                InitObjectMapping(player);

                PlayerList.Add(userId, username);
            }
            
            player.Connect(client, this, userId);

            Players.Enqueue(player);
        }

        public void HandlePlayerRenders()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            try
            {
                do
                {
                    Player player = Players.Dequeue();

                    player.Render(this);

                    Players.Enqueue(player);
                } while (true);

            }
            catch (EmptyContainerException)
            {

            }
        }

        public override void Dispose()
        {
            // Assertion.
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(EntitySpawningPool.Empty);
            System.Diagnostics.Debug.Assert(Entities.Empty);
            System.Diagnostics.Debug.Assert(Players.Empty);

            System.Diagnostics.Debug.Assert(DisconnectedPlayers.Empty);

            // Release resources.
            PlayerList.Dispose();

            EntitySpawningPool.Dispose();
            Entities.Dispose();
            Players.Dispose();

            DisconnectedPlayers.Dispose();

            BlockContext.Dispose();

            // Finish.
            base.Dispose();
            _disposed = true;
        }
       

    }


}

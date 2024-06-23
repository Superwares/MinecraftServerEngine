using Common;
using Containers;
using MinecraftServerEngine.PhysicsEngine;
using Sync;

namespace MinecraftServerEngine
{
    public abstract class World : PhysicsWorld
    {
        private readonly struct IntegrationResult
        {
            public readonly BoundingVolume Volume;
            public readonly Vector Velocity;
            public readonly bool OnGround;

            public IntegrationResult(
                BoundingVolume volume, Vector v, bool onGround)
            {
                Volume = volume;
                Velocity = v;
                OnGround = onGround;
            }
        }

        private bool _disposed = false;

        internal readonly PlayerList PlayerList = new();  // Disposable

        private readonly ConcurrentQueue<Entity> EntitySpawningPool = new();  // Disposable
        internal readonly SwapQueue<Entity> Entities = new();  // Disposable
        internal readonly SwapQueue<Player> Players = new();

        private readonly ConcurrentTable<System.Guid, Player> DisconnectedPlayers = new(); // Disposable

        internal readonly BlockContext BlockContext = new();  // Disposable

        private readonly ConcurrentTable<Entity, IntegrationResult> IntegrationResults = new();  // Disposable

        /*internal PublicInventory _Inventory = new ChestInventory();*/

        public World() { }

        ~World() => System.Diagnostics.Debug.Assert(false);

        public abstract bool CanJoinWorld();

        public void SpawnEntity(Entity entity)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            EntitySpawningPool.Enqueue(entity);

            throw new System.NotImplementedException();
        }

        protected abstract bool DetermineToDespawnPlayerOnDisconnect();

        public void StartRoutine(Barrier barrier, long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            /*if (serverTicks == 20 * 5)
            {
                SpawnItemEntity();
            }*/

            barrier.SignalAndWait();
        }

        public void StartEntityRoutines(Barrier barrier, long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Entities.Swap();

            barrier.SignalAndWait();

            try
            {
                do
                {
                    Entity entity = Entities.Dequeue();

                    System.Diagnostics.Debug.Assert(entity is not Player);

                    entity.StartRoutine(serverTicks, this);

                    Entities.Enqueue(entity);
                } while (true);
            }
            catch (EmptyContainerException) { }

            System.Diagnostics.Debug.Assert(Entities.Empty);

            barrier.SignalAndWait();
        }

        public void StartPlayerRoutines(Barrier barrier, long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Players.Swap();

            barrier.SignalAndWait();

            try
            {
                do
                {
                    Player player = Players.Dequeue();
                    System.Diagnostics.Debug.Assert(player != null);

                    player.StartRoutine(serverTicks, this);

                    Players.Enqueue(player);
                } while (true);

            }
            catch (EmptyContainerException) { }

            System.Diagnostics.Debug.Assert(Players.Empty);

            barrier.SignalAndWait();
        }

        private void DestroyEntity(Entity entity)
        {
            System.Diagnostics.Debug.Assert(entity != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            CloseObjectMapping(entity);

            entity.Flush();
            entity.Dispose();
        }

        public void DestroyEntities(Barrier barrier)
        {
            System.Diagnostics.Debug.Assert(barrier != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            Entities.Swap();

            barrier.SignalAndWait();

            try
            {
                do
                {
                    Entity entity = Entities.Dequeue();

                    System.Diagnostics.Debug.Assert(entity is not Player);

                    if (entity.IsDead())
                    {
                        DestroyEntity(entity);

                        continue;
                    }

                    Entities.Enqueue(entity);

                } while (true);
            }
            catch (EmptyContainerException) { }

            System.Diagnostics.Debug.Assert(Entities.Empty);

            Players.Swap();

            barrier.SignalAndWait();

            try
            {
                do
                {
                    Player player = Players.Dequeue();
                    System.Diagnostics.Debug.Assert(player != null);

                    if (player.HandleConnection(this))
                    {
                        System.Guid userId = player.UniqueId;

                        System.Diagnostics.Debug.Assert(!DisconnectedPlayers.Contains(userId));
                        DisconnectedPlayers.Insert(userId, player);

                        System.Diagnostics.Debug.Assert(
                            DisconnectedPlayers.Contains(player.UniqueId));

                        if (DetermineToDespawnPlayerOnDisconnect())
                        {
                            PlayerList.Remove(player.UniqueId);

                            Player playerExtracted = DisconnectedPlayers.Extract(player.UniqueId);
                            System.Diagnostics.Debug.Assert(ReferenceEquals(playerExtracted, player));

                            DestroyEntity(player);

                            continue;
                        }
                    }

                    Players.Enqueue(player);
                } while (true);

            }
            catch (EmptyContainerException) { }

            System.Diagnostics.Debug.Assert(Players.Empty);

            barrier.SignalAndWait();
        }

        public void MoveEntities(Barrier barrier)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Entities.Swap();

            barrier.SignalAndWait();

            try
            {
                do
                {
                    Entity entity = Entities.Dequeue();

                    (BoundingVolume volume, Vector v, bool f) =
                        IntegrateObject(BlockContext, entity);
                    IntegrationResults.Insert(entity, new IntegrationResult(volume, v, f));

                    Entities.Enqueue(entity);
                } while (true);
            }
            catch (EmptyContainerException) { }

            System.Diagnostics.Debug.Assert(Entities.Empty);

            Players.Swap();

            barrier.SignalAndWait();

            try
            {
                do
                {
                    Player player = Players.Dequeue();

                    (BoundingVolume volume, Vector v, bool f) =
                        IntegrateObject(BlockContext, player);
                    IntegrationResults.Insert(player, new IntegrationResult(volume, v, f));

                    Players.Enqueue(player);
                } while (true);

            }
            catch (EmptyContainerException) { }

            System.Diagnostics.Debug.Assert(Players.Empty);

            Entities.Swap();

            barrier.SignalAndWait();

            try
            {
                do
                {
                    Entity entity = Entities.Dequeue();

                    IntegrationResult result = IntegrationResults.Extract(entity);

                    entity.Move(result.Volume, result.Velocity, result.OnGround);

                    UpdateObjectMapping(entity);

                    Entities.Enqueue(entity);
                } while (true);
            }
            catch (EmptyContainerException) { }

            System.Diagnostics.Debug.Assert(Entities.Empty);

            Players.Swap();

            barrier.SignalAndWait();

            try
            {
                do
                {
                    Player player = Players.Dequeue();

                    IntegrationResult result = IntegrationResults.Extract(player);

                    player.Move(result.Volume, result.Velocity, result.OnGround);

                    UpdateObjectMapping(player);

                    Players.Enqueue(player);
                } while (true);

            }
            catch (EmptyContainerException) { }

            System.Diagnostics.Debug.Assert(Players.Empty);

            barrier.SignalAndWait();
        }

        public void CreateEntities(Barrier barrier)
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
            catch (EmptyContainerException) { }

            System.Diagnostics.Debug.Assert(EntitySpawningPool.Empty);

            barrier.SignalAndWait();
        }

        protected abstract Player CreatePlayer(System.Guid userId);

        internal void CreateOrConnectPlayer(Client client, string username, System.Guid userId)
        {
            System.Diagnostics.Debug.Assert(client != null);
            System.Diagnostics.Debug.Assert(username != "");
            System.Diagnostics.Debug.Assert(userId != System.Guid.Empty);

            System.Diagnostics.Debug.Assert(!_disposed);

            Player player;

            if (DisconnectedPlayers.Contains(userId))
            {
                player = DisconnectedPlayers.Extract(userId);
                System.Diagnostics.Debug.Assert(player != null);
            }
            else
            {
                player = CreatePlayer(userId);
                System.Diagnostics.Debug.Assert(player != null);

                InitObjectMapping(player);

                PlayerList.Add(userId, username);

                Players.Enqueue(player);
            }
            
            player.Connect(client, this, userId);

        }

        public void HandlePlayerRenders(Barrier barrier)
        {
            System.Diagnostics.Debug.Assert(barrier != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            Players.Swap();

            barrier.SignalAndWait();

            try
            {
                do
                {
                    Player player = Players.Dequeue();

                    player.Render(this);

                    Players.Enqueue(player);
                } while (true);

            }
            catch (EmptyContainerException) { }

            barrier.SignalAndWait();
        }

        public override void Dispose()
        {
            // Assertion.
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(EntitySpawningPool.Empty);
            System.Diagnostics.Debug.Assert(Entities.Empty);
            System.Diagnostics.Debug.Assert(Players.Empty);

            System.Diagnostics.Debug.Assert(DisconnectedPlayers.Empty);

            System.Diagnostics.Debug.Assert(IntegrationResults.Empty);

            // Release resources.
            PlayerList.Dispose();

            EntitySpawningPool.Dispose();
            Entities.Dispose();
            Players.Dispose();

            DisconnectedPlayers.Dispose();

            BlockContext.Dispose();

            IntegrationResults.Dispose();

            // Finish.
            base.Dispose();
            _disposed = true;
        }
       

    }


}

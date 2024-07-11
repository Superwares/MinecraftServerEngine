using Common;
using Containers;
using Sync;

namespace MinecraftServerEngine
{
    using PhysicsEngine;

    public abstract class World : PhysicsWorld
    {
        private sealed class EntityQueue : System.IDisposable
        {
            private class SwapQueue<T> : System.IDisposable
            {
                private bool _disposed = false;

                private int dequeue = 1;
                private readonly ConcurrentQueue<T> Queue1 = new();  // Disposable
                private readonly ConcurrentQueue<T> Queue2 = new();  // Disposable

                private ConcurrentQueue<T> GetQueueForDequeue()
                {
                    System.Diagnostics.Debug.Assert(!_disposed);

                    if (dequeue == 1)
                    {
                        return Queue1;
                    }
                    else if (dequeue == 2)
                    {
                        return Queue2;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }

                    throw new System.NotImplementedException();
                }

                private ConcurrentQueue<T> GetQueueForEnqueue()
                {
                    System.Diagnostics.Debug.Assert(!_disposed);

                    if (dequeue == 1)
                    {
                        return Queue2;
                    }
                    else if (dequeue == 2)
                    {
                        return Queue1;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }

                    throw new System.NotImplementedException();
                }

                public int Count
                {
                    get
                    {
                        System.Diagnostics.Debug.Assert(!_disposed);

                        ConcurrentQueue<T> queue = GetQueueForDequeue();
                        return queue.Count;
                    }
                }
                public bool Empty => (Count == 0);

                public SwapQueue() { }

                ~SwapQueue() => System.Diagnostics.Debug.Assert(false);

                public void Swap()
                {
                    System.Diagnostics.Debug.Assert(!_disposed);

                    ConcurrentQueue<T> queue = GetQueueForDequeue();

                    if (queue.Empty)
                    {
                        if (dequeue == 1)
                        {
                            System.Diagnostics.Debug.Assert(Queue1.Empty);

                            dequeue = 2;
                        }
                        else if (dequeue == 2)
                        {
                            System.Diagnostics.Debug.Assert(Queue2.Empty);

                            dequeue = 1;
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false);
                        }
                    }
                }

                public bool Dequeue(out T value)
                {
                    System.Diagnostics.Debug.Assert(!_disposed);

                    ConcurrentQueue<T> queue = GetQueueForDequeue();

                    return queue.Dequeue(out value);
                }

                public void Enqueue(T value)
                {
                    System.Diagnostics.Debug.Assert(value != null);

                    System.Diagnostics.Debug.Assert(!_disposed);

                    ConcurrentQueue<T> queue = GetQueueForEnqueue();
                    queue.Enqueue(value);
                }

                public void Dispose()
                {
                    // Assertions.
                    System.Diagnostics.Debug.Assert(!_disposed);

                    // Release resources.
                    Queue1.Dispose();
                    Queue2.Dispose();

                    // Finish.
                    System.GC.SuppressFinalize(this);
                    _disposed = true;
                }
            }

            private bool _disposed = false;

            private readonly Locker Locker = new();  // Disposable

            private readonly SwapQueue<Entity> Entities = new();  // Disposable
            private readonly SwapQueue<AbstractPlayer> Players = new();

            ~EntityQueue() => System.Diagnostics.Debug.Assert(false);
            
            public void Swap()
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                Entities.Swap();
                Players.Swap();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            /// <exception cref="EmptyContainerException" />
            public bool Dequeue(out Entity entity)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                Locker.Hold();

                bool f;

                if (!Entities.Empty)
                {
                    f = Entities.Dequeue(out entity);
                }
                else
                {
                    AbstractPlayer player;
                    f = Players.Dequeue(out player);
                    entity = player;
                }

                Locker.Release();

                return f;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            /// <exception cref="EmptyContainerException" />
            public bool DequeuePlayer(out AbstractPlayer player)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                return Players.Dequeue(out player);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            /// <exception cref="EmptyContainerException" />
            public bool DequeueNonPlayer(out Entity entity)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                return Entities.Dequeue(out entity);
            }

            public void Enqueue(Entity entity)
            {
                System.Diagnostics.Debug.Assert(entity != null);

                System.Diagnostics.Debug.Assert(!_disposed);

                if (entity is AbstractPlayer player)
                {
                    Players.Enqueue(player);
                }
                else
                {
                    Entities.Enqueue(entity);
                }
            }

            public void Dispose()
            {
                // Assertions.
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(Entities.Empty);
                System.Diagnostics.Debug.Assert(Players.Empty);

                // Release resources.
                Locker.Dispose();

                Entities.Dispose();
                Players.Dispose();

                // Finish.
                System.GC.SuppressFinalize(this);
                _disposed = true;
            }

        }

        /*private readonly struct IntegrationResult
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
        }*/

        private bool _disposed = false;

        internal readonly PlayerList PlayerList = new();  // Disposable

        private readonly ConcurrentQueue<Entity> EntitySpawningPool = new();  // Disposable

        private readonly EntityQueue Entities = new();  // Disposable

        private readonly ConcurrentTable<UserId, AbstractPlayer> DisconnectedPlayers = new(); // Disposable

        internal readonly BlockContext BlockContext = new();  // Disposable

        /*private readonly ConcurrentTable<Entity, IntegrationResult> IntegrationResults = new();  // Disposable*/

        /*internal PublicInventory _Inventory = new ChestInventory();*/

        public World() { }

        ~World() => System.Diagnostics.Debug.Assert(false);

        public abstract bool CanJoinWorld();

        public void SpawnEntity(Entity entity)
        {
            System.Diagnostics.Debug.Assert(entity is not AbstractPlayer);

            System.Diagnostics.Debug.Assert(!_disposed);

            EntitySpawningPool.Enqueue(entity);
        }

        protected abstract bool DetermineToDespawnPlayerOnDisconnect();

        internal void SwapEntityQueue()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Entities != null);
            Entities.Swap();
        }

        internal void StartRoutine(long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (serverTicks == 20 * 5)
            {
                SpawnEntity(new ItemEntity(new ItemStack(ItemType.Stick, 30), new Vector(0.0D, 120.0D, 0.0D)));
            }
        }

        internal void StartEntityRoutines(long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            while (Entities.Dequeue(out Entity entity))
            {
                System.Diagnostics.Debug.Assert(entity != null);

                entity.StartRoutine(serverTicks, this);

                Entities.Enqueue(entity);
            }
        }

        internal void ControlPlayers(long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            while (Entities.DequeuePlayer(out AbstractPlayer player))
            {
                System.Diagnostics.Debug.Assert(player != null);

                player.Control(serverTicks, this);

                Entities.Enqueue(player);
            }
        }

        private void DestroyEntity(Entity entity)
        {
            System.Diagnostics.Debug.Assert(entity != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            CloseObjectMapping(entity);

            entity.Flush();
            entity.Dispose();
        }

        internal void DestroyEntities()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Entity entity;
            while (Entities.Dequeue(out entity))
            {
                System.Diagnostics.Debug.Assert(entity != null);

                if (entity is AbstractPlayer player && 
                    player.HandleConnection(out UserId id, this))
                {
                    System.Diagnostics.Debug.Assert(id != UserId.Null);
                    System.Diagnostics.Debug.Assert(!DisconnectedPlayers.Contains(id));
                    DisconnectedPlayers.Insert(id, player);

                    if (DetermineToDespawnPlayerOnDisconnect())
                    {
                        PlayerList.Remove(id);

                        AbstractPlayer playerExtracted = DisconnectedPlayers.Extract(id);
                        System.Diagnostics.Debug.Assert(ReferenceEquals(playerExtracted, player));

                        DestroyEntity(player);

                        continue;
                    }
                }
                else
                {
                    if (entity.IsDead())
                    {
                        DestroyEntity(entity);

                        continue;
                    }
                }

                Entities.Enqueue(entity);

            }
        }

        internal void MoveEntities()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Entity entity;
            while (Entities.Dequeue(out entity))
            {
                System.Diagnostics.Debug.Assert(entity != null);

                (BoundingVolume volume, Vector v) = 
                    IntegrateObject(BlockContext, entity);

                entity.Move(volume, v);

                UpdateObjectMapping(entity);

                Entities.Enqueue(entity);
            }

        }

        internal void CreateEntities()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            while (EntitySpawningPool.Dequeue(out Entity entity))
            {
                System.Diagnostics.Debug.Assert(entity != null);
                System.Diagnostics.Debug.Assert(entity is not AbstractPlayer);

                InitObjectMapping(entity);

                Entities.Enqueue(entity);
            }

            System.Diagnostics.Debug.Assert(EntitySpawningPool.Empty);
        }

        protected abstract AbstractPlayer CreatePlayer(UserId id);

        internal void CreateOrConnectPlayer(Client client, string username, UserId id)
        {
            System.Diagnostics.Debug.Assert(client != null);
            System.Diagnostics.Debug.Assert(username != "");
            System.Diagnostics.Debug.Assert(id != UserId.Null);

            System.Diagnostics.Debug.Assert(!_disposed);

            AbstractPlayer player;

            if (DisconnectedPlayers.Contains(id))
            {
                player = DisconnectedPlayers.Extract(id);
                System.Diagnostics.Debug.Assert(player != null);
            }
            else
            {
                player = CreatePlayer(id);
                System.Diagnostics.Debug.Assert(player != null);

                InitObjectMapping(player);

                PlayerList.Add(id, username);

                Entities.Enqueue(player);
            }
            
            player.Connect(client, this, id);

        }

        internal void RenderPlayers()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            AbstractPlayer player;
            while (Entities.DequeuePlayer(out player))
            {
                System.Diagnostics.Debug.Assert(player != null);

                player.Render(this);

                Entities.Enqueue(player);
            }

        }

        public override void Dispose()
        {
            // Assertion.
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(EntitySpawningPool.Empty);

            System.Diagnostics.Debug.Assert(DisconnectedPlayers.Empty);

            /*System.Diagnostics.Debug.Assert(IntegrationResults.Empty);*/

            // Release resources.
            PlayerList.Dispose();

            EntitySpawningPool.Dispose();

            Entities.Dispose();

            DisconnectedPlayers.Dispose();

            BlockContext.Dispose();

            /*IntegrationResults.Dispose();*/

            // Finish.
            base.Dispose();
            _disposed = true;
        }
       
    }


}

using Common;
using Containers;
using Sync;

namespace MinecraftServerEngine
{
    using Protocols;
    using Text;
    using Blocks;
    using Entities;
    using Renderers;
    using Physics;

    public abstract class World : PhysicsWorld
    {
        private sealed class ObjectQueue : System.IDisposable
        {
            private class SwapQueue<T> : System.IDisposable
            {
                private bool _disposed = false;

                private int dequeue = 0;

                // TODO: Implement using array
                private readonly Queue<T> Queue0 = new();  // Disposable
                private readonly Queue<T> Queue1 = new();  // Disposable

                private Queue<T> GetQueueForDequeue()
                {
                    System.Diagnostics.Debug.Assert(_disposed == false);

                    if (dequeue == 0)
                    {
                        System.Diagnostics.Debug.Assert(Queue0 != null);
                        return Queue0;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(Queue1 != null);
                        return Queue1;
                    }
                }

                private Queue<T> GetQueueForEnqueue()
                {
                    System.Diagnostics.Debug.Assert(_disposed == false);

                    if (dequeue == 0)
                    {
                        System.Diagnostics.Debug.Assert(Queue1 != null);
                        return Queue1;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(Queue0 != null);
                        return Queue0;
                    }
                }

                public int CountForDequeue
                {
                    get
                    {
                        System.Diagnostics.Debug.Assert(_disposed == false);

                        Queue<T> queue = GetQueueForDequeue();

                        System.Diagnostics.Debug.Assert(queue != null);
                        return queue.Length;
                    }
                }
                public bool EmptyForDequeue => (CountForDequeue == 0);

                public SwapQueue() { }

                ~SwapQueue() => System.Diagnostics.Debug.Assert(false);

                public void Swap()
                {
                    System.Diagnostics.Debug.Assert(_disposed == false);

                    if (dequeue == 0)
                    {
                        dequeue = 1;
                    }
                    else
                    {
                        dequeue = 0;
                    }
                }

                public bool Dequeue(out T value)
                {
                    System.Diagnostics.Debug.Assert(_disposed == false);

                    Queue<T> queue = GetQueueForDequeue();

                    System.Diagnostics.Debug.Assert(queue != null);
                    return queue.Dequeue(out value);
                }


                public void Enqueue(T value)
                {
                    System.Diagnostics.Debug.Assert(value != null);

                    System.Diagnostics.Debug.Assert(_disposed == false);

                    Queue<T> queue = GetQueueForEnqueue();

                    System.Diagnostics.Debug.Assert(queue != null);
                    queue.Enqueue(value);
                }

                public void Dispose()
                {
                    // Assertions.
                    System.Diagnostics.Debug.Assert(_disposed == false);

                    // Release resources.
                    Queue0.Dispose();
                    Queue1.Dispose();

                    // Finish.
                    System.GC.SuppressFinalize(this);
                    _disposed = true;
                }
            }

            private bool _disposed = false;

            private readonly Locker Locker = new();  // Disposable

            private bool _serving = false;

            private readonly SwapQueue<PhysicsObject> _Objects = new();  // Disposable
            private readonly SwapQueue<LivingEntity> _LivingEntities = new();  // Disposable
            private readonly SwapQueue<AbstractPlayer> _Players = new();  // Disposable

            private bool IsAllQueueEmptyForDequeue
            {
                get
                {
                    System.Diagnostics.Debug.Assert(_disposed == false);
                    return _Objects.EmptyForDequeue == true && _Players.EmptyForDequeue == true;
                }
            }

            ~ObjectQueue() => System.Diagnostics.Debug.Assert(false);

            internal void StartServing()
            {
                System.Diagnostics.Debug.Assert(_disposed == false);

                System.Diagnostics.Debug.Assert(_serving == false);
                _serving = true;
            }

            internal bool Dequeue(out PhysicsObject obj)
            {
                System.Diagnostics.Debug.Assert(_disposed == false);

                System.Diagnostics.Debug.Assert(Locker != null);
                Locker.Hold();

                System.Diagnostics.Debug.Assert(_serving == true);

                try
                {

                    if (_Objects.EmptyForDequeue == false)
                    {
                        return _Objects.Dequeue(out obj);
                    }
                    else if (_LivingEntities.EmptyForDequeue == false)
                    {
                        System.Diagnostics.Debug.Assert(_LivingEntities != null);
                        bool success = _LivingEntities.Dequeue(out LivingEntity livingEntity);
                        obj = livingEntity;

                        return success;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(_Players != null);
                        bool success = _Players.Dequeue(out AbstractPlayer player);
                        obj = player;

                        return success;
                    }
                }
                finally
                {
                    System.Diagnostics.Debug.Assert(Locker != null);
                    Locker.Release();
                }


            }

            internal bool DequeueNonPlayer(out PhysicsObject obj)
            {
                System.Diagnostics.Debug.Assert(_disposed == false);

                System.Diagnostics.Debug.Assert(Locker != null);
                Locker.Hold();

                System.Diagnostics.Debug.Assert(_serving == true);

                try
                {
                    if (_Objects.EmptyForDequeue == false)
                    {
                        return _Objects.Dequeue(out obj);
                    }
                    else
                    {
                        bool success = _LivingEntities.Dequeue(out LivingEntity livingEntity);
                        obj = livingEntity;

                        return success;
                    }
                }
                finally
                {
                    System.Diagnostics.Debug.Assert(Locker != null);
                    Locker.Release();
                }
            }

            internal bool DequeueLivingEntity(out LivingEntity livingEntity)
            {
                System.Diagnostics.Debug.Assert(_disposed == false);

                System.Diagnostics.Debug.Assert(Locker != null);
                Locker.Hold();

                System.Diagnostics.Debug.Assert(_serving == true);

                try
                {
                    if (_LivingEntities.EmptyForDequeue == false)
                    {
                        return _LivingEntities.Dequeue(out livingEntity);
                    }
                    else
                    {
                        bool success = _Players.Dequeue(out AbstractPlayer player);
                        livingEntity = player;

                        return success;
                    }
                }
                finally
                {
                    System.Diagnostics.Debug.Assert(Locker != null);
                    Locker.Release();
                }
            }

            internal bool DequeuePlayer(out AbstractPlayer player)
            {
                System.Diagnostics.Debug.Assert(_disposed == false);

                System.Diagnostics.Debug.Assert(Locker != null);
                Locker.Hold();

                System.Diagnostics.Debug.Assert(_serving == true);

                try
                {
                    return _Players.Dequeue(out player);
                }
                finally
                {
                    System.Diagnostics.Debug.Assert(Locker != null);
                    Locker.Release();
                }
            }

            internal void Enqueue(PhysicsObject obj)
            {
                System.Diagnostics.Debug.Assert(obj != null);

                System.Diagnostics.Debug.Assert(_disposed == false);

                System.Diagnostics.Debug.Assert(Locker != null);
                Locker.Hold();

                System.Diagnostics.Debug.Assert(_serving == true);

                try
                {

                    if (obj is AbstractPlayer player)
                    {
                        System.Diagnostics.Debug.Assert(_Players != null);
                        _Players.Enqueue(player);
                    }
                    else if (obj is LivingEntity livingEntity)
                    {
                        System.Diagnostics.Debug.Assert(_LivingEntities != null);
                        _LivingEntities.Enqueue(livingEntity);
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(_Objects != null);
                        _Objects.Enqueue(obj);
                    }
                }
                finally
                {
                    System.Diagnostics.Debug.Assert(Locker != null);
                    Locker.Release();
                }
            }

            internal void EndServing()
            {
                System.Diagnostics.Debug.Assert(_disposed == false);

                System.Diagnostics.Debug.Assert(_serving == true);
                _serving = false;

                System.Diagnostics.Debug.Assert(_Objects != null);
                if (_Objects.EmptyForDequeue == true)
                {
                    _Objects.Swap();
                }

                System.Diagnostics.Debug.Assert(_LivingEntities != null);
                if (_LivingEntities.EmptyForDequeue == true)
                {
                    _LivingEntities.Swap();
                }

                System.Diagnostics.Debug.Assert(_Players != null);
                if (_Players.EmptyForDequeue == true)
                {
                    _Players.Swap();
                }
            }

            public void Dispose()
            {
                // Assertions.
                System.Diagnostics.Debug.Assert(_disposed == false);

                System.Diagnostics.Debug.Assert(_serving == false);

                System.Diagnostics.Debug.Assert(_Objects.EmptyForDequeue);
                System.Diagnostics.Debug.Assert(_LivingEntities.EmptyForDequeue);
                System.Diagnostics.Debug.Assert(_Players.EmptyForDequeue);

                // Release resources.
                Locker.Dispose();

                _Objects.Dispose();
                _LivingEntities.Dispose();
                _Players.Dispose();

                // Finish.
                System.GC.SuppressFinalize(this);
                _disposed = true;
            }

        }

        private bool _disposed = false;

        internal readonly PlayerList PlayerList = new();  // Disposable

        private readonly ConcurrentQueue<PhysicsObject> ObjectSpawningPool = new();  // Disposable
        private readonly ConcurrentQueue<PhysicsObject> ObjectDespawningPool = new();  // Disposable

        private readonly ObjectQueue _ObjectQueue = new();  // Disposable

        internal readonly ConcurrentTable<int, Entity> EntitiesById = new();  // Disposable
        internal readonly ConcurrentTable<UserId, AbstractPlayer> PlayersByUserId = new();  // Disposable
        public int AllPlayers
        {
            get
            {
                if (_disposed == true)
                {
                    throw new System.ObjectDisposedException(GetType().Name);
                }

                System.Diagnostics.Debug.Assert(PlayersByUserId != null);
                return PlayersByUserId.Count;
            }
        }
        public System.Collections.Generic.IEnumerable<AbstractPlayer> Players
        {
            get
            {
                if (_disposed == true)
                {
                    throw new System.ObjectDisposedException(GetType().Name);
                }

                System.Diagnostics.Debug.Assert(PlayersByUserId != null);
                return PlayersByUserId.GetValues();
            }
        }


        internal readonly ConcurrentTable<string, AbstractPlayer> PlayersByUsername = new();  // Disposable

        internal readonly ConcurrentMap<UserId, WorldRenderer> WorldRenderersByUserId = new();  // Disposable

        private readonly ConcurrentTable<UserId, AbstractPlayer> DisconnectedPlayers = new(); // Disposable

        public readonly BlockContext BlockContext;  // Disposable


        private readonly ReadLocker LockerProgressBars = new();  // Disposable
        private readonly Map<System.Guid, ProgressBar> ProgressBars = new();  // Disposable


        public const double MaxWorldBorderRadiusInMeters = 999999.0;

        private readonly ReadLocker LockerWorldBorder = new();  // Disposable
        private double _worldBorder_centerX;
        private double _worldBorder_centerZ;
        private double _worldBorder_currentRadiusInMeters;
        private double _worldBorder_targetRadiusInMeters;
        private Time _worldBorder_transitionStartTime = Time.Zero;
        private Time _worldBorder_transitionTimePerMeter = Time.Zero;


        private readonly ReadLocker LockerWorldTime = new();  // Disposable
        // TODO: Add world age
        private Time _currentWorldTime = MinecraftTimes.DaytimeMid;
        private Time _targetWorldTime = MinecraftTimes.DaytimeMid;
        private Time _worldTime_transitionStartTime = Time.Zero;
        private Time _worldTime_transitionTime = Time.Zero;


        public World(
            double centerX, double centerZ,
            double worldBorderRadiusInMeters)
        {
            if (worldBorderRadiusInMeters <= 0)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(worldBorderRadiusInMeters),
                    "World border radius must be greater than 0.");
            }
            if (worldBorderRadiusInMeters > MaxWorldBorderRadiusInMeters)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(worldBorderRadiusInMeters),
                    $"World border radius must be less than or equal to {MaxWorldBorderRadiusInMeters}.");
            }

            _worldBorder_centerX = centerX;
            _worldBorder_centerZ = centerZ;
            _worldBorder_currentRadiusInMeters = worldBorderRadiusInMeters;
            _worldBorder_targetRadiusInMeters = worldBorderRadiusInMeters;

            BlockContext = BlockContext.LoadWithRegionFiles(@"region");
        }

        public World(
            double centerX, double centerZ,
            double worldBorderRadiusInMeters,
            Time worldTime)
            : this(centerX, centerZ, worldBorderRadiusInMeters)
        {
            //Time currentWorldTime = new(Math.Normalize(worldTime.Amount, MinecraftTimes.OneDay.Amount));
            _currentWorldTime = worldTime;
            _targetWorldTime = worldTime;
            _worldTime_transitionStartTime = Time.Zero;
            _worldTime_transitionTime = Time.Zero;

        }

        ~World()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }


        internal void Connect(UserId id, ConcurrentQueue<ClientboundPlayingPacket> OutPackets)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            PlayerListRenderer plRenderer = new(OutPackets);
            PlayerList.Connect(id, plRenderer);

            WorldRenderer renderer = new(OutPackets);
            WorldRenderersByUserId.Insert(id, renderer);


            System.Diagnostics.Debug.Assert(LockerProgressBars != null);
            LockerProgressBars.Read();

            try
            {
                System.Diagnostics.Debug.Assert(ProgressBars != null);
                foreach (ProgressBar progressBar in ProgressBars.GetValues())
                {
                    renderer.OpenBossBar(
                        progressBar.Id, progressBar.TitleData,
                        progressBar.Health, progressBar.Color, progressBar.Division);
                }


            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerProgressBars != null);
                LockerProgressBars.Release();
            }

            System.Diagnostics.Debug.Assert(LockerWorldBorder != null);
            LockerWorldBorder.Read();
            try
            {
                System.Diagnostics.Debug.Assert(_worldBorder_currentRadiusInMeters > 0);
                System.Diagnostics.Debug.Assert(_worldBorder_currentRadiusInMeters <= MaxWorldBorderRadiusInMeters);
                System.Diagnostics.Debug.Assert(_worldBorder_targetRadiusInMeters > 0);
                System.Diagnostics.Debug.Assert(_worldBorder_targetRadiusInMeters <= MaxWorldBorderRadiusInMeters);
                double remainingDistanceInMeters =
                        _worldBorder_targetRadiusInMeters - _worldBorder_currentRadiusInMeters;
                Time remainingTransitionTime =
                    _worldBorder_transitionTimePerMeter * Math.Abs(remainingDistanceInMeters);
                renderer.InitWorldBorder(
                    _worldBorder_centerX, _worldBorder_centerZ,
                    _worldBorder_currentRadiusInMeters, _worldBorder_targetRadiusInMeters,
                    remainingTransitionTime);
            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerWorldBorder != null);
                LockerWorldBorder.Release();
            }

            System.Diagnostics.Debug.Assert(LockerWorldTime != null);
            LockerWorldTime.Read();

            try
            {
                Time currentWorldTime = new(
                    Math.Normalize(_currentWorldTime.Amount, MinecraftTimes.OneDay.Amount)
                    );
                System.Diagnostics.Debug.Assert(renderer != null);
                renderer.UpdateWorldTime(currentWorldTime);

            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerWorldTime != null);
                LockerWorldTime.Release();
            }
        }

        public void PlaySound(string name, int category, Vector p, double volume, double pitch)
        {
            if (volume < 0.0 || volume > 1.0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(volume));
            }
            if (pitch < 0.5 || pitch > 2.0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(pitch));
            }

            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(WorldRenderersByUserId != null);
            foreach (WorldRenderer renderer in WorldRenderersByUserId.GetValues())
            {
                System.Diagnostics.Debug.Assert(renderer != null);
                renderer.PlaySound(name, category, p, volume, pitch);
            }
        }

        public void DisplayTitle(
            Time fadeIn, Time stay, Time fadeOut,
            params TextComponent[] components)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            if (components == null || components.Length == 0)
            {
                return;
            }

            var extra = new object[components.Length];

            for (int i = 0; i < components.Length; ++i)
            {
                TextComponent component = components[i];

                extra[i] = new
                {
                    text = component.Text,
                    color = component.Color.GetName(),
                };
            }

            var chat = new
            {
                text = "",
                extra = extra,
            };

            string data = System.Text.Json.JsonSerializer.Serialize(chat);

            // TODO: 1tick = 50ms, make this variable to single constant.

            foreach (WorldRenderer renderer in WorldRenderersByUserId.GetValues())
            {
                System.Diagnostics.Debug.Assert(renderer != null);
                renderer.DisplayTitle(
                    (int)(fadeIn.Amount / MinecraftTimes.TimePerTick.Amount),
                    (int)(stay.Amount / MinecraftTimes.TimePerTick.Amount),
                    (int)(fadeOut.Amount / MinecraftTimes.TimePerTick.Amount),
                    data);
            }

        }

        public System.Guid OpenProgressBar(
            TextComponent[] title, double health,
            ProgressBarColor color, ProgressBarDivision division)
        {
            if (health < 0.0 || health > 1.0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(health));
            }

            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            if (title == null)
            {
                title = [];
            }

            ProgressBar progressBar = new(title, health, color, division);

            System.Diagnostics.Debug.Assert(LockerProgressBars != null);
            LockerProgressBars.Hold();

            try
            {
                System.Diagnostics.Debug.Assert(WorldRenderersByUserId != null);
                foreach (WorldRenderer renderer in WorldRenderersByUserId.GetValues())
                {
                    System.Diagnostics.Debug.Assert(renderer != null);
                    renderer.OpenBossBar(
                        progressBar.Id, progressBar.TitleData, progressBar.Health,
                        progressBar.Color, progressBar.Division);
                }

                System.Diagnostics.Debug.Assert(ProgressBars != null);
                ProgressBars.Insert(progressBar.Id, progressBar);

                return progressBar.Id;
            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerProgressBars != null);
                LockerProgressBars.Release();
            }


        }

        public void UpdateProgressBarHealth(System.Guid id, double health)
        {
            if (id == System.Guid.Empty)
            {
                throw new System.ArgumentNullException(nameof(id));
            }
            if (health < 0.0 || health > 1.0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(health));
            }

            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(LockerProgressBars != null);
            LockerProgressBars.Hold();

            try
            {
                System.Diagnostics.Debug.Assert(ProgressBars != null);
                ProgressBar progressBar = ProgressBars.Lookup(id);

                progressBar.UpdateHealth(health);

                System.Diagnostics.Debug.Assert(WorldRenderersByUserId != null);
                foreach (WorldRenderer renderer in WorldRenderersByUserId.GetValues())
                {
                    System.Diagnostics.Debug.Assert(renderer != null);
                    renderer.UpdateBossBarHealth(progressBar.Id, progressBar.Health);
                }

            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerProgressBars != null);
                LockerProgressBars.Release();
            }
        }

        public void CloseProgressBar(System.Guid id)
        {
            if (id == System.Guid.Empty)
            {
                throw new System.ArgumentNullException(nameof(id));
            }

            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(LockerProgressBars != null);
            LockerProgressBars.Hold();

            try
            {
                System.Diagnostics.Debug.Assert(ProgressBars != null);
                ProgressBar progressBar = ProgressBars.Extract(id);

                System.Diagnostics.Debug.Assert(WorldRenderersByUserId != null);
                foreach (WorldRenderer renderer in WorldRenderersByUserId.GetValues())
                {
                    System.Diagnostics.Debug.Assert(renderer != null);
                    renderer.CloseBossBar(progressBar.Id);
                }

            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerProgressBars != null);
                LockerProgressBars.Release();
            }
        }

        public void WriteMessageInChatBox(params TextComponent[] components)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            if (components == null)
            {
                components = [];
            }

            var extra = new object[components.Length];

            for (int i = 0; i < components.Length; ++i)
            {
                TextComponent component = components[i];

                extra[i] = new
                {
                    text = component.Text,
                    color = component.Color.GetName(),
                };
            }

            var chat = new
            {
                text = "",
                extra = extra,
            };

            string data = System.Text.Json.JsonSerializer.Serialize(chat);

            System.Diagnostics.Debug.Assert(WorldRenderersByUserId != null);
            foreach (WorldRenderer renderer in WorldRenderersByUserId.GetValues())
            {
                System.Diagnostics.Debug.Assert(renderer != null);
                renderer.WriteChatInChatBox(data);
            }

        }

        public bool IsOutsideOfWorldBorder(Vector p)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(LockerWorldBorder != null);
            LockerWorldBorder.Read();

            try
            {
                double dx = Math.Abs(p.X - _worldBorder_centerX);
                double dz = Math.Abs(p.Z - _worldBorder_centerZ);

                System.Diagnostics.Debug.Assert(_worldBorder_currentRadiusInMeters > 0.0);
                System.Diagnostics.Debug.Assert(_worldBorder_currentRadiusInMeters <= MaxWorldBorderRadiusInMeters);
                return (dx > _worldBorder_currentRadiusInMeters) || (dz > _worldBorder_currentRadiusInMeters);
            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerWorldBorder != null);
                LockerWorldBorder.Release();
            }
        }

        public void ChangeWorldBorderSize(double radiusInMeters, Time transitionTimePerMeter)
        {
            if (radiusInMeters <= 0)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(radiusInMeters),
                    "World border radius must be greater than 0.");
            }
            if (radiusInMeters > MaxWorldBorderRadiusInMeters)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(radiusInMeters),
                    $"World border radius must be less than or equal to {MaxWorldBorderRadiusInMeters}.");
            }

            if (transitionTimePerMeter < Time.Zero)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(transitionTimePerMeter),
                    "World border transition time must be greater than or equal to 0.");
            }

            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(LockerWorldBorder != null);
            LockerWorldBorder.Hold();

            try
            {
                if (Math.AreDoublesEqual(_worldBorder_targetRadiusInMeters, radiusInMeters) == false)
                {
                    _worldBorder_targetRadiusInMeters = radiusInMeters;
                    _worldBorder_transitionTimePerMeter = transitionTimePerMeter;

                    _worldBorder_transitionStartTime = Time.Now();

                    System.Diagnostics.Debug.Assert(_worldBorder_currentRadiusInMeters > 0.0);
                    System.Diagnostics.Debug.Assert(_worldBorder_currentRadiusInMeters <= MaxWorldBorderRadiusInMeters);
                    double remainingDistanceInMeters =
                        _worldBorder_targetRadiusInMeters - _worldBorder_currentRadiusInMeters;
                    Time remainingTransitionTime =
                        _worldBorder_transitionTimePerMeter * Math.Abs(remainingDistanceInMeters);

                    System.Diagnostics.Debug.Assert(WorldRenderersByUserId != null);
                    foreach (WorldRenderer renderer in WorldRenderersByUserId.GetValues())
                    {
                        System.Diagnostics.Debug.Assert(renderer != null);
                        System.Diagnostics.Debug.Assert(_worldBorder_currentRadiusInMeters > 0.0);
                        System.Diagnostics.Debug.Assert(_worldBorder_currentRadiusInMeters <= MaxWorldBorderRadiusInMeters);
                        renderer.InitWorldBorder(_worldBorder_centerX, _worldBorder_centerZ,
                            _worldBorder_currentRadiusInMeters, _worldBorder_targetRadiusInMeters,
                            remainingTransitionTime);
                    }
                }
            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerWorldBorder != null);
                LockerWorldBorder.Release();
            }
        }

        public void ChangeWorldTimeToNextDay(Time worldTime, Time transitionTime)
        {
            if (transitionTime < Time.Zero)
            {
                throw new System.ArgumentOutOfRangeException(nameof(transitionTime));
            }

            System.Diagnostics.Debug.Assert(LockerWorldTime != null);
            LockerWorldTime.Hold();

            try
            {
                System.Diagnostics.Debug.Assert(MinecraftTimes.OneDay.Amount > 0);
                int days = (int)System.Math.Ceiling(
                    (double)_targetWorldTime.Amount / (double)MinecraftTimes.OneDay.Amount
                    );

                _targetWorldTime = (MinecraftTimes.OneDay * days) + worldTime;

                _worldTime_transitionStartTime = Time.Now();
                _worldTime_transitionTime = transitionTime;

            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerWorldTime != null);
                LockerWorldTime.Release();
            }
        }

        public void ChangeWorldTimeOfDay(Time worldTime, Time transitionTime)
        {
            if (transitionTime < Time.Zero)
            {
                throw new System.ArgumentOutOfRangeException(nameof(transitionTime));
            }

            System.Diagnostics.Debug.Assert(LockerWorldTime != null);
            LockerWorldTime.Hold();

            try
            {
                System.Diagnostics.Debug.Assert(MinecraftTimes.OneDay.Amount > 0);
                int days = (int)System.Math.Floor(
                    (double)_targetWorldTime.Amount / (double)MinecraftTimes.OneDay.Amount
                    );

                _targetWorldTime = (MinecraftTimes.OneDay * days) + worldTime;

                _worldTime_transitionStartTime = Time.Now();
                _worldTime_transitionTime = transitionTime;

            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerWorldTime != null);
                LockerWorldTime.Release();
            }
        }

        public void ChangeWorldTimeToPrevDayTo(Time worldTime, Time transitionTime)
        {
            if (transitionTime < Time.Zero)
            {
                throw new System.ArgumentOutOfRangeException(nameof(transitionTime));
            }

            System.Diagnostics.Debug.Assert(LockerWorldTime != null);
            LockerWorldTime.Hold();

            try
            {

                throw new System.NotImplementedException();

            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerWorldTime != null);
                LockerWorldTime.Release();
            }
        }

        public void AddTimeToWorld(Time addTime, Time transitionTime)
        {
            if (transitionTime < Time.Zero)
            {
                throw new System.ArgumentOutOfRangeException(nameof(transitionTime));
            }

            System.Diagnostics.Debug.Assert(LockerWorldTime != null);
            LockerWorldTime.Hold();

            try
            {

                _targetWorldTime = _targetWorldTime + addTime;
                _worldTime_transitionStartTime = Time.Now();
                _worldTime_transitionTime = transitionTime;

            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerWorldTime != null);
                LockerWorldTime.Release();
            }
        }

        internal void Disconnect(UserId id)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            PlayerList.Disconnect(id);

            WorldRenderersByUserId.Extract(id);

            // TODO: world border
            // TODO: Boss Bar
        }

        public abstract bool CanJoinWorld();

        protected abstract bool CanDespawnPlayerOnDisconnect();

        public void SpawnObject(PhysicsObject obj)
        {
            if (obj is AbstractPlayer)
            {
                throw new System.ArgumentException("Cannot spawn an AbstractPlayer object.");
            }

            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(obj is not AbstractPlayer);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(ObjectSpawningPool != null);
            ObjectSpawningPool.Enqueue(obj);
        }



        internal void StartTask()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_ObjectQueue != null);
            _ObjectQueue.StartServing();
        }

        internal void ControlPlayers()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            while (_ObjectQueue.DequeuePlayer(out AbstractPlayer player))
            {
                System.Diagnostics.Debug.Assert(player != null);

                player.Control(this);

                _ObjectQueue.Enqueue(player);
            }
        }

        internal void HandlePlayerDisconnections()
        {
            bool despawned = CanDespawnPlayerOnDisconnect();
            bool cleanup;

            while (_ObjectQueue.DequeuePlayer(out AbstractPlayer player))
            {
                System.Diagnostics.Debug.Assert(player != null);

                if (despawned == false)
                {
                    if (player.HandleDisconnection(out UserId userId, this) == true)
                    {
                        System.Diagnostics.Debug.Assert(userId != UserId.Null);

                        DisconnectedPlayers.Insert(userId, player);
                    }
                }
                else
                {
                    cleanup = false;

                    if (DisconnectedPlayers.Contains(player.UserId) == true)
                    {
                        DisconnectedPlayers.Extract(player.UserId);

                        cleanup = true;
                    }
                    else if (player.HandleDisconnection(out UserId userId, this) == true)
                    {
                        cleanup = true;
                    }

                    if (cleanup == true)
                    {
                        PlayerList.Remove(player.UserId);

                        PlayersByUserId.Extract(player.UserId);
                        PlayersByUsername.Extract(player.Username);

                        ObjectDespawningPool.Enqueue(player);

                        System.Diagnostics.Debug.Assert(player != null);
                        player.OnDespawn(this);

                        //player.OnDisconnected();

                        continue;
                    }


                }


                _ObjectQueue.Enqueue(player);
            }

        }

        internal void HandleDespawning()
        {
            while (_ObjectQueue.DequeueNonPlayer(out PhysicsObject obj))
            {
                System.Diagnostics.Debug.Assert(obj != null);

                System.Diagnostics.Debug.Assert(obj != null);
                if (obj.HandleDespawning() == true)
                {
                    System.Diagnostics.Debug.Assert(obj is not AbstractPlayer);

                    System.Diagnostics.Debug.Assert(ObjectDespawningPool != null);
                    ObjectDespawningPool.Enqueue(obj);

                    System.Diagnostics.Debug.Assert(obj != null);
                    obj.OnDespawn(this);

                    continue;
                }

                _ObjectQueue.Enqueue(obj);
            }
        }

        internal void DestroyObjects()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            while (ObjectDespawningPool.Dequeue(out PhysicsObject obj))
            {
                System.Diagnostics.Debug.Assert(obj != null);

                CloseObjectMapping(obj);

                obj.Flush(this);
                obj.Dispose();

                if (obj is Entity entity)
                {
                    Entity entityExtracted = EntitiesById.Extract(entity.Id);
                    System.Diagnostics.Debug.Assert(ReferenceEquals(entity, entityExtracted));
                }
            }
        }

        internal void MoveObjects()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            while (_ObjectQueue.Dequeue(out PhysicsObject obj))
            {
                System.Diagnostics.Debug.Assert(obj != null);

                (BoundingVolume volume, Vector v) = IntegrateObject(BlockContext, obj);

                obj.Move(this, volume, v);

                UpdateObjectMapping(obj);

                _ObjectQueue.Enqueue(obj);
            }

        }

        protected abstract AbstractPlayer CreatePlayer(UserId userId, string username);

        internal void ConnectPlayer(User user)
        {

            System.Diagnostics.Debug.Assert(_disposed == false);

            AbstractPlayer player;

            if (DisconnectedPlayers.Contains(user.Id) == true)
            {
                player = DisconnectedPlayers.Extract(user.Id);
                System.Diagnostics.Debug.Assert(player != null);
            }
            else
            {
                player = CreatePlayer(user.Id, user.Username);
                System.Diagnostics.Debug.Assert(player != null);

                ObjectSpawningPool.Enqueue(player);

                PlayerList.Add(user.Id, user.Username, user.Properties);

                PlayersByUserId.Insert(user.Id, player);
                PlayersByUsername.Insert(user.Username, player);
            }

            player.Connect(user.Client, this, user.Id);

        }

        internal void CreateObjects()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            while (ObjectSpawningPool.Dequeue(out PhysicsObject obj) == true)
            {
                System.Diagnostics.Debug.Assert(obj != null);

                InitObjectMapping(obj);

                _ObjectQueue.Enqueue(obj);

                if (obj is Entity entity)
                {
                    EntitiesById.Insert(entity.Id, entity);
                }

            }

            System.Diagnostics.Debug.Assert(ObjectSpawningPool.Empty);
        }

        internal void LoadWorld()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_ObjectQueue != null);
            while (_ObjectQueue.DequeuePlayer(out AbstractPlayer player))
            {
                System.Diagnostics.Debug.Assert(player != null);
                player.LoadWorld(this);

                _ObjectQueue.Enqueue(player);
            }

        }

        internal void SendData()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_ObjectQueue != null);
            while (_ObjectQueue.DequeuePlayer(out AbstractPlayer player))
            {
                System.Diagnostics.Debug.Assert(player != null);
                player.SendData();

                _ObjectQueue.Enqueue(player);
            }

        }

        protected internal override void _StartRoutine()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            if (_worldBorder_transitionStartTime > Time.Zero)
            {
                System.Diagnostics.Debug.Assert(LockerWorldBorder != null);
                LockerWorldBorder.Hold();

                try
                {
                    System.Diagnostics.Debug.Assert(
                        Math.AreDoublesEqual(
                            _worldBorder_currentRadiusInMeters,
                            _worldBorder_targetRadiusInMeters) == false
                            );
                    System.Diagnostics.Debug.Assert(_worldBorder_transitionStartTime != Time.Zero);
                    Time elapsedTime = Time.Now() - _worldBorder_transitionStartTime;

                    System.Diagnostics.Debug.Assert(_worldBorder_targetRadiusInMeters > 0.0);
                    System.Diagnostics.Debug.Assert(_worldBorder_currentRadiusInMeters > 0.0);
                    double remainingDistanceInMeters =
                        _worldBorder_targetRadiusInMeters - _worldBorder_currentRadiusInMeters;
                    Time remainingTransitionTime =
                        _worldBorder_transitionTimePerMeter * Math.Abs(remainingDistanceInMeters);

                    if (elapsedTime >= remainingTransitionTime)
                    {
                        System.Diagnostics.Debug.Assert(_worldBorder_targetRadiusInMeters <= MaxWorldBorderRadiusInMeters);
                        _worldBorder_currentRadiusInMeters = _worldBorder_targetRadiusInMeters;

                        _worldBorder_transitionStartTime = Time.Zero;
                        _worldBorder_transitionTimePerMeter = Time.Zero;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(remainingTransitionTime > elapsedTime);
                        // Calculate Transition Progress:
                        double transitionProgress =
                            (double)elapsedTime.Amount / (double)remainingTransitionTime.Amount;

                        // Ensure transition progress does not exceed 1.0
                        transitionProgress = System.Math.Min(transitionProgress, 1.0);

                        // Update Old Radius:
                        _worldBorder_currentRadiusInMeters = _worldBorder_currentRadiusInMeters +
                            (remainingDistanceInMeters * transitionProgress);
                        System.Diagnostics.Debug.Assert(_worldBorder_currentRadiusInMeters <= MaxWorldBorderRadiusInMeters);

                        // Update Transition Start Time
                        _worldBorder_transitionStartTime = Time.Now();
                    }



                }
                finally
                {
                    System.Diagnostics.Debug.Assert(LockerWorldBorder != null);
                    LockerWorldBorder.Release();
                }
            }

            if (_worldTime_transitionStartTime > Time.Zero)
            {
                System.Diagnostics.Debug.Assert(LockerWorldTime != null);
                LockerWorldTime.Hold();

                try
                {

                    System.Diagnostics.Debug.Assert(_worldTime_transitionStartTime != Time.Zero);
                    Time elapsedTime = Time.Now() - _worldTime_transitionStartTime;

                    System.Diagnostics.Debug.Assert(elapsedTime >= Time.Zero);
                    System.Diagnostics.Debug.Assert(_worldTime_transitionTime >= Time.Zero);
                    if (elapsedTime >= _worldTime_transitionTime)
                    {
                        _currentWorldTime = _targetWorldTime;
                        //_targetWorldTime = _targetWorldTime;
                        _worldTime_transitionStartTime = Time.Zero;
                        _worldTime_transitionTime = Time.Zero;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(elapsedTime.Amount < _worldTime_transitionTime.Amount);
                        double transitionProgress =
                                (double)elapsedTime.Amount / (double)_worldTime_transitionTime.Amount;

                        Time remaningWorldTime = _targetWorldTime - _currentWorldTime;

                        _currentWorldTime = _currentWorldTime + (remaningWorldTime * transitionProgress);

                        _worldTime_transitionTime = _worldTime_transitionTime - elapsedTime;
                        _worldTime_transitionStartTime = Time.Now();
                    }

                    {
                        Time currentWorldTime = new(
                            Math.Normalize(_currentWorldTime.Amount, MinecraftTimes.OneDay.Amount)
                            );

                        System.Diagnostics.Debug.Assert(WorldRenderersByUserId != null);
                        foreach (WorldRenderer renderer in WorldRenderersByUserId.GetValues())
                        {
                            System.Diagnostics.Debug.Assert(renderer != null);
                            renderer.UpdateWorldTime(currentWorldTime);
                        }
                    }

                }
                finally
                {
                    System.Diagnostics.Debug.Assert(LockerWorldTime != null);
                    LockerWorldTime.Release();
                }
            }

            base._StartRoutine();
        }

        internal void StartObjectRoutines()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            while (_ObjectQueue.Dequeue(out PhysicsObject obj) == true)
            {
                System.Diagnostics.Debug.Assert(obj != null);

                obj.StartRoutine(this);

                _ObjectQueue.Enqueue(obj);
            }
        }

        internal void HandleLivingEntityDamageEvents()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            while (_ObjectQueue.DequeueLivingEntity(out LivingEntity livingEntity) == true)
            {
                System.Diagnostics.Debug.Assert(livingEntity != null);
                livingEntity.HandleDamageEvents(this);

                _ObjectQueue.Enqueue(livingEntity);
            }
        }

        internal void EndTask()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_ObjectQueue != null);
            _ObjectQueue.EndServing();
        }

        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed == false)
            {
                System.Diagnostics.Debug.Assert(ObjectSpawningPool.Empty == true);
                System.Diagnostics.Debug.Assert(ObjectDespawningPool.Empty == true);

                System.Diagnostics.Debug.Assert(EntitiesById.Empty == true);

                System.Diagnostics.Debug.Assert(DisconnectedPlayers.Empty == true);

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing == true)
                {
                    PlayerList.Dispose();

                    ObjectSpawningPool.Dispose();
                    ObjectDespawningPool.Dispose();

                    _ObjectQueue.Dispose();

                    EntitiesById.Dispose();
                    PlayersByUserId.Dispose();
                    PlayersByUsername.Dispose();

                    WorldRenderersByUserId.Dispose();

                    DisconnectedPlayers.Dispose();

                    BlockContext.Dispose();

                    LockerProgressBars.Dispose();
                    ProgressBars.Dispose();

                    LockerWorldBorder.Dispose();

                    LockerWorldTime.Dispose();
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                //CloseHandle(handle);
                //handle = IntPtr.Zero;

                // Note disposing has been done.
                _disposed = true;
            }

            base.Dispose(disposing);
        }


    }


}

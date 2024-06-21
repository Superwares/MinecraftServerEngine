
using Common;
using Containers;
using MinecraftPhysicsEngine;

namespace MinecraftServerEngine
{
    public abstract class Entity : PhysicsObject
    {

        private protected readonly struct Hitbox : System.IEquatable<Hitbox>
        {
            private readonly double Width, Height;
            /*public readonly double EyeHeight, MaxStepHeight;*/

            public Hitbox(double w, double h)
            {
                Width = w;
                Height = h;
            }

            public readonly BoundingVolume Convert(Vector p)
            {
                System.Diagnostics.Debug.Assert(Width > 0.0D);
                System.Diagnostics.Debug.Assert(Height > 0.0D);

                double w = Width / 2.0D;

                Vector max = new(p.X + w, p.Y + Height, p.Z + w),
                       min = new(p.X - w, p.Y, p.Z - w);
                return new AxisAlignedBoundingBox(max, min);
            }

            public readonly override string ToString()
            {
                throw new System.NotImplementedException();
            }

            public readonly bool Equals(Hitbox other)
            {
                throw new System.NotImplementedException();
            }
        }

        private bool _disposed = false;

        internal readonly int Id;
        internal readonly System.Guid UniqueId;

        private Vector _p;
        internal Vector Position => _p;

        private bool _rotated = false;
        private Look _look;
        internal Look LOOK => _look;

        protected bool _sneaking, _sprinting = false;
        public bool Sneaking => _sneaking;
        public bool Sprinting => _sprinting;


        private protected bool _teleported = false;
        private protected Vector _pTeleport;
        private protected Look _lookTeleport;


        private EntityRendererManager _MANAGER;  // Disposable


        private protected Entity(
            System.Guid uniqueId,
            Vector p, Look look,
            Hitbox hitbox,
            double m)
            : base(hitbox.Convert(p), m)
        {
            Id = EntityIdAllocator.Alloc();
            UniqueId = uniqueId;


            System.Diagnostics.Debug.Assert(!_rotated);
            _look = look;

            System.Diagnostics.Debug.Assert(!_sneaking);
            System.Diagnostics.Debug.Assert(!_sprinting);

            _MANAGER = new(Id);
        }

        ~Entity() => System.Diagnostics.Debug.Assert(false);

        private protected abstract Hitbox GetHitbox();

        private protected abstract void RenderSpawning(EntityRenderer renderer);

        internal void ApplyRenderer(EntityRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_MANAGER != null);
            if (_MANAGER.Apply(renderer))
            {
                RenderSpawning(renderer);
            }
        }

        public virtual bool IsDead()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return false;
        }

        public virtual void StartInternalRoutine(long serverTicks, World world) { }

        public virtual void StartRoutine(long serverTicks, World world) { }

        protected override BoundingVolume GenerateBoundingVolume()
        {
            Hitbox hitbox = GetHitbox();

            return _teleported ? hitbox.Convert(_pTeleport) : hitbox.Convert(_p);
        }

        public override void Move(BoundingVolume volume, Vector v, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (_teleported)
            {
                _MANAGER.Teleport(_pTeleport, _lookTeleport, false);

                _p = _pTeleport;

                if (!_rotated)
                {
                    _look = _lookTeleport;
                }

                _teleported = false;
            }

            Vector p = volume.GetBottomCenter();

            System.Diagnostics.Debug.Assert(_MANAGER != null);
            _MANAGER.HandleRendering(p);

            bool moved = !p.Equals(_p);  // TODO: Compare with machine epsilon.
            if (moved && _rotated)
            {
                _MANAGER.MoveAndRotate(p, _p, _look, onGround);
            }
            else if (moved)
            {
                System.Diagnostics.Debug.Assert(!_rotated);

                _MANAGER.Move(p, _p, onGround);
            }
            else if (_rotated)
            {
                System.Diagnostics.Debug.Assert(!moved);

                _MANAGER.Rotate(_look, onGround);
            }
            else
            {
                System.Diagnostics.Debug.Assert(!moved);
                System.Diagnostics.Debug.Assert(!_rotated);

                _MANAGER.Stand();
            }

            _MANAGER.FinishMovementRenderring();

            base.Move(volume, v, onGround);

            _p = p;
            _rotated = false;
        }

        public virtual void Teleport(Vector p, Look look)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _rotated = false;

            _teleported = true;
            _pTeleport = p;
            _lookTeleport = look;
        }

        public void Rotate(Look look)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _rotated = true;
            _look = look;
        }

        private void RanderFormChanging()
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            
            _MANAGER.ChangeForms(_sneaking, _sprinting);
        }

        public void Sneak()
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            
            _sneaking = true;

            RanderFormChanging();
        }

        public void Unsneak()
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            
            _sneaking = false;

            RanderFormChanging();
        }

        public void Sprint()
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            
            _sprinting = true;

            RanderFormChanging();
        }

        public void Unsprint()
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            
            _sprinting = false;

            RanderFormChanging();
        }
        
        public virtual void Flush()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_MANAGER != null);
            _MANAGER.Flush();
        }

        public override void Dispose()
        {
            // Assertion
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.
            EntityIdAllocator.Dealloc(Id);
            _MANAGER.Dispose();

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

    public abstract class ItemEntity : Entity
    {
        private bool _disposed = false;
        
        public const double MASS = 0.1D;

        private static readonly Hitbox HITBOX = new(0.25D, 0.25D);

        public ItemEntity(Vector p, Look look)
            : base(System.Guid.NewGuid(), p, look, HITBOX, MASS) 
        { }

        ~ItemEntity() => System.Diagnostics.Debug.Assert(false);

        private protected override void RenderSpawning(EntityRenderer renderer)
        {
            /*(byte x, byte y) = Look.ConvertToPacketFormat();
            outPackets.Enqueue(new SpawnObjectPacket(
                Id, UniqueId, 2,
                Position.X, Position.Y, Position.Z,
                x, y,
                1, 0, 0, 0));

            using EntityMetadata metadata = new();
            metadata.AddBool(5, true);
            metadata.AddSlotData(6, new SlotData(280, 1));
            outPackets.Enqueue(new EntityMetadataPacket(
                Id, metadata.WriteData()));*/

            renderer.SpawnItemEntity(Id);
        }

        public override void Dispose()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // Assertion.

            // Release resources.

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

    public abstract class LivingEntity : Entity
    {
        private bool _disposed = false;

        private protected LivingEntity(System.Guid uniqueId,
            Vector p, Look look,
            Hitbox hitbox,
            double m) : base(uniqueId, p, look, hitbox, m) { }

        ~LivingEntity() => System.Diagnostics.Debug.Assert(false);

        public override void Dispose()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // Assertion.

            // Release resources.

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

    public abstract class Player : LivingEntity
    {
        private bool _disposed = false;

        public const double MASS = 1.0D;

        internal readonly PlayerInventory _selfInventory = new();

        private Connection _CONN;
        public bool Connected => (_CONN != null) || !_CONN.Disconnected;

        private bool _controled = false;
        private Vector _pControl;
        private bool _onGroundControl;

        private static Hitbox GetHitbox(bool sneaking)
        {
            double w = 0.6D, h;
            if (sneaking)
            {
                h = 1.65D;
            }
            else
            {
                h = 1.8D;
            }

            return new Hitbox(w, h);
        }

        public Player(System.Guid userId, Vector p, Look look) 
            : base(userId, p, look, GetHitbox(false), MASS) 
        {
            System.Diagnostics.Debug.Assert(!Sneaking);
            System.Diagnostics.Debug.Assert(!Sprinting);
        }

        ~Player() => System.Diagnostics.Debug.Assert(false);

        private protected override Hitbox GetHitbox()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return GetHitbox(Sneaking);
        }

        private protected override void RenderSpawning(EntityRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            renderer.SpawnPlayer(
                Id, UniqueId,
                Position, LOOK,
                Sneaking, Sprinting);
        }

        internal void Connect(Client client, World world, System.Guid userId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _pControl = Position;
            _onGroundControl = OnGround;

            _CONN = new Connection(client, world, Id, userId, _pControl, _selfInventory);
        }

        public override void ApplyForce(Vector force)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (Connected)
            {
                System.Diagnostics.Debug.Assert(_CONN != null);
                Vector v = force / MASS;
                _CONN.ApplyVelocity(Id, v);
            }

            base.ApplyForce(force);
        }

        public override void Move(BoundingVolume volume, Vector v, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (_controled)
            {
                // TODO: Check the difference between _p and p. and predict movement....
                /*Console.Printl($"p: {p}, _p: {_p}, ");
                Console.Printl($"Length: {Vector.GetLength(p, _p)}");
                if (Vector.GetLength(p, _p) > k)
                {
                }
                */

                /*Vector v1 = bb.GetBottomCenter(), v2 = _p;
                double length = Vector.GetLength(v1, v2);
                Console.Printl($"length: {length}");*/

                volume = GetHitbox().Convert(_pControl);
                onGround = _onGroundControl;

                _controled = false;
            }
            
            base.Move(volume, v, onGround);
        }

        public override void Teleport(Vector p, Look look)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (Connected)
            {
                _CONN.Teleport(p, look);

                _controled = true;
                _pControl = p;
                _onGroundControl = false;
            }

            base.Teleport(p, look);
        }

        internal void Control(Vector p, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_teleported);

            _controled = true;
            _pControl = p;
            _onGroundControl = onGround;

        }

        internal void Control(bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_teleported);

            _controled = true;
            _onGroundControl = onGround;

        }

        private void HandleControl(ServerboundPlayingPacket control)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            switch (control)
            {
                default:
                    throw new System.NotImplementedException();
                case PlayerPacket playerPacket:
                    {
                        Control(playerPacket.OnGround);
                    }
                    break;
                case PlayerPositionPacket playerPositionPacket:
                    {
                        Vector p = new(
                            playerPositionPacket.X, 
                            playerPositionPacket.Y, 
                            playerPositionPacket.Z);
                        Control(p, playerPositionPacket.OnGround);
                    }
                    break;
                case PlayerPosAndLookPacket playerPosAndLookPacket:
                    {
                        Vector p = new(
                            playerPosAndLookPacket.X,
                            playerPosAndLookPacket.Y,
                            playerPosAndLookPacket.Z);
                        Look look = new Look(playerPosAndLookPacket.Yaw, playerPosAndLookPacket.Pitch);

                        Control(p, playerPosAndLookPacket.OnGround);
                        Rotate(look);
                    }
                    break;
                case PlayerLookPacket playerLookPacket:
                    {
                        Look look = new Look(playerLookPacket.Yaw, playerLookPacket.Pitch);
                        
                        Control(playerLookPacket.OnGround);
                        Rotate(look);
                    }
                    break;
                case EntityActionPacket entityActionPacket:
                    switch (entityActionPacket.ActionId)
                    {
                        default:
                            throw new System.NotImplementedException();
                        case 0:
                            Sneak();
                            break;
                        case 1:
                            Unsneak();
                            break;
                        case 3:
                            Sprint();
                            break;
                        case 4:
                            Unsprint();
                            break;
                    }
                    break;
            }
        }

        public override void StartInternalRoutine(long serverTicks, World world)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (Connected)
            {
                System.Diagnostics.Debug.Assert(_CONN != null);

                using Queue<ServerboundPlayingPacket> controls = new();

                _CONN.Control(
                    controls, 
                    world, 
                    UniqueId, Sneaking, Sprinting, _selfInventory);

                while (!controls.Empty)
                {
                    ServerboundPlayingPacket control = controls.Dequeue();
                    HandleControl(control);
                }
            }

            base.StartInternalRoutine(serverTicks, world);
        }

        public bool HandlePlayerConnection(World world)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (_CONN == null)
            {
                return false;
            }

            System.Diagnostics.Debug.Assert(_CONN != null);
            if (_CONN.Disconnected)
            {
                _CONN.Flush(world, UniqueId);
                _CONN.Dispose();

                _CONN = null;

                return true;
            }

            return false;
        }

        public void Render(World world)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (!Connected)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(_CONN != null);
            _CONN.Render(
                world, 
                Id, 
                Position, LOOK, 
                _selfInventory);
        }
        
        public override void Dispose()
        {
            // Assertion.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.

            // Finish.
            base.Dispose();
            _disposed = true;
        }


    }

}

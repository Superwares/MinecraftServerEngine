
using Common;
using Sync;
using Containers;

using MinecraftServerEngine.Blocks;
using MinecraftServerEngine.Renderers;
using MinecraftServerEngine.Physics;
using MinecraftServerEngine.Physics.BoundingVolumes;
using MinecraftServerEngine.Particles;

namespace MinecraftServerEngine.Entities
{

    public abstract class Entity : PhysicsObject
    {
        private bool _disposed = false;


        public readonly int Id;
        public readonly System.Guid UniqueId;



        protected Vector _p;
        public Vector Position => _p;


        private readonly Locker _LockerRotate = new();
        private bool _rotated = false;
        private EntityAngles _look;
        public EntityAngles Look => _look;


        protected bool _sneaking = false, _sprinting = false;
        public bool IsSneaking => _sneaking;
        public bool IsSprinting => _sprinting;


        protected readonly Locker LockerTeleport = new();
        private bool _teleported = false;
        private Vector _pTeleport;

        private bool _prevFakeBlockApplied = false, _fakeBlockApplied = false;
        private Block _prevFakeBlock, _fakeBlock;



        private readonly bool _NoGravity;


        private bool _canHaveMovement = false;

        internal readonly ConcurrentTree<EntityRenderer> Renderers = new();  // Disposable


        private protected Entity(
            System.Guid uniqueId,
            Vector p, EntityAngles look,
            bool noGravity,
            EntityHitbox hitbox,
            double mass, double maxStepHeight)
            : base(mass, hitbox.Convert(p), new StepableMovement(maxStepHeight))
        {
            Id = EntityIdAllocator.Allocate();
            UniqueId = uniqueId;

            _p = p;

            System.Diagnostics.Debug.Assert(!_rotated);
            _look = look;

            System.Diagnostics.Debug.Assert(!_sneaking);
            System.Diagnostics.Debug.Assert(!_sprinting);

            _NoGravity = noGravity;
        }

        ~Entity()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        private protected abstract EntityHitbox GetHitbox();

        protected override (BoundingVolume, bool noGravity) GetCurrentStatus()
        {
            EntityHitbox hitbox = GetHitbox();

            BoundingVolume volume = _teleported ? hitbox.Convert(_pTeleport) : hitbox.Convert(_p);
            return (volume, _NoGravity || volume is EmptyBoundingVolume);
        }

        private protected abstract void RenderSpawning(EntityRenderer renderer);

        protected virtual void OnMove(PhysicsWorld world) { }
        protected virtual void OnSneak(World world, bool f) { }
        protected virtual void OnSprint(World world, bool f) { }


        //protected (Vector, Vector) GetRay()
        //{
        //    throw new System.NotImplementedException();
        //    /*return (origin, u);*/
        //}

        internal void ApplyRenderer(EntityRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(renderer != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (BoundingVolume is EmptyBoundingVolume)
            {
                System.Diagnostics.Debug.Assert(Renderers.Empty);
                return;
            }

            if (Renderers.Contains(renderer) == true)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(Renderers != null);
            Renderers.Insert(renderer);

            if (_prevFakeBlockApplied == true)
            {
                BlockLocation loc = BlockLocation.Generate(_p);
                renderer.SetBlockAppearance(_prevFakeBlock, loc);
            }
            else
            {
                RenderSpawning(renderer);
            }
        }

        private bool HandleRendering(
            PhysicsWorld _world,
            BlockLocation prevBlockLocation,
            BoundingVolume volume, out Vector p)
        {
            System.Diagnostics.Debug.Assert(_world != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            p = volume.GetBottomCenter();

            if (volume is not EmptyBoundingVolume)
            {
                System.Diagnostics.Debug.Assert(_canHaveMovement == false);

                using Queue<EntityRenderer> queue = new();

                System.Diagnostics.Debug.Assert(Renderers != null);
                foreach (EntityRenderer renderer in Renderers.GetKeys())
                {
                    System.Diagnostics.Debug.Assert(renderer != null);

                    if (renderer.CanRender(p) == true)
                    {
                        continue;
                    }

                    queue.Enqueue(renderer);
                }

                if (_world is World world)
                {
                    System.Diagnostics.Debug.Assert(world.BlockContext != null);
                    Block prevBlock = world.BlockContext.GetBlock(prevBlockLocation);

                    if (_prevFakeBlockApplied == true)
                    {

                        while (queue.Empty == false)
                        {
                            EntityRenderer renderer = queue.Dequeue();

                            if (renderer.IsDisconnected == false)
                            {
                                renderer.SetBlockAppearance(prevBlock, prevBlockLocation);
                            }

                            Renderers.Extract(renderer);
                        }
                    }
                    else
                    {
                        while (queue.Empty == false)
                        {
                            EntityRenderer renderer = queue.Dequeue();

                            if (renderer.IsDisconnected == false)
                            {
                                renderer.DestroyEntity(Id);
                            }

                            Renderers.Extract(renderer);
                        }
                    }

                    if (_prevFakeBlockApplied != _fakeBlockApplied)
                    {
                        if (_fakeBlockApplied == false)
                        {
                            System.Diagnostics.Debug.Assert(_prevFakeBlockApplied == true);
                            System.Diagnostics.Debug.Assert(_fakeBlockApplied == false);

                            System.Diagnostics.Debug.Assert(Renderers != null);
                            foreach (EntityRenderer renderer in Renderers.GetKeys())
                            {
                                System.Diagnostics.Debug.Assert(renderer != null);
                                renderer.SetBlockAppearance(prevBlock, prevBlockLocation);

                                RenderSpawning(renderer);
                            }

                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(_prevFakeBlockApplied == false);
                            System.Diagnostics.Debug.Assert(_fakeBlockApplied == true);

                            System.Diagnostics.Debug.Assert(Renderers != null);
                            foreach (EntityRenderer renderer in Renderers.GetKeys())
                            {
                                System.Diagnostics.Debug.Assert(renderer != null);
                                renderer.DestroyEntity(Id);
                            }

                        }
                    }
                }
                else
                {
                    while (queue.Empty == false)
                    {
                        EntityRenderer renderer = queue.Dequeue();

                        if (renderer.IsDisconnected == false)
                        {
                            renderer.DestroyEntity(Id);
                        }

                        Renderers.Extract(renderer);
                    }
                }


                System.Diagnostics.Debug.Assert(_canHaveMovement == false);
                _canHaveMovement = true;
                return true;
            }
            else
            {
                System.Diagnostics.Debug.Assert(volume is EmptyBoundingVolume);

                EntityRenderer[] renderers = Renderers.Flush();

                if (_world is World world && _prevFakeBlockApplied == true)
                {
                    System.Diagnostics.Debug.Assert(world.BlockContext != null);
                    Block prevBlock = world.BlockContext.GetBlock(prevBlockLocation);

                    foreach (EntityRenderer renderer in renderers)
                    {
                        System.Diagnostics.Debug.Assert(renderer != null);
                        if (renderer.IsDisconnected == true)
                        {
                            continue;
                        }

                        System.Diagnostics.Debug.Assert(renderer != null);
                        renderer.SetBlockAppearance(prevBlock, prevBlockLocation);
                    }
                }
                else
                {
                    foreach (EntityRenderer renderer in renderers)
                    {
                        System.Diagnostics.Debug.Assert(renderer != null);
                        if (renderer.IsDisconnected == true)
                        {
                            continue;
                        }

                        System.Diagnostics.Debug.Assert(renderer != null);
                        renderer.DestroyEntity(Id);
                    }
                }

                System.Diagnostics.Debug.Assert(_canHaveMovement == false);
                _canHaveMovement = false;
                return false;
            }

        }

        internal override void Move(PhysicsWorld _world, BoundingVolume volume, Vector v)
        {
            System.Diagnostics.Debug.Assert(_world != null);
            System.Diagnostics.Debug.Assert(volume != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            bool isNoneToBlockAppearanceApplie = _prevFakeBlockApplied == false && _fakeBlockApplied == true;

            BlockLocation prevBlockLocation = BlockLocation.Generate(_p);
            Vector prevPosition = _p;

            bool shouldMovementRendering = HandleRendering(
                _world,
                prevBlockLocation,
                volume, out Vector p);

            BlockLocation blockLocatioin = BlockLocation.Generate(p);

            bool moved = p.Equals(_p) == false;  // TODO: Compare with machine epsilon.
            bool blockMoved = prevBlockLocation.Equals(blockLocatioin) == false;

            if (shouldMovementRendering == true)
            {
                System.Diagnostics.Debug.Assert(_canHaveMovement == true);

                if (_teleported == true)
                {
                    if (_fakeBlockApplied == false && Renderers.Empty == false)
                    {
                        System.Diagnostics.Debug.Assert(Renderers != null);
                        foreach (EntityRenderer renderer in Renderers.GetKeys())
                        {
                            System.Diagnostics.Debug.Assert(renderer != null);
                            renderer.Teleport(Id, _pTeleport, _look, false);
                        }
                    }

                    _p = _pTeleport;

                    _rotated = false;
                    _teleported = false;
                }

                if (_world is World world && _fakeBlockApplied == true)
                {
                    System.Diagnostics.Debug.Assert(world.BlockContext != null);
                    Block prevBlock = world.BlockContext.GetBlock(prevBlockLocation);

                    if (blockMoved == true)
                    {
                        foreach (EntityRenderer renderer in Renderers.GetKeys())
                        {
                            System.Diagnostics.Debug.Assert(renderer != null);
                            renderer.SetBlockAppearance(prevBlock, prevBlockLocation);
                        }
                    }

                }
                else if (moved == true && _rotated == true)
                {
                    double dx = (p.X - _p.X) * (32 * 128);
                    double dy = (p.Y - _p.Y) * (32 * 128);
                    double dz = (p.Z - _p.Z) * (32 * 128);

                    if (
                        dx < short.MinValue || dx > short.MaxValue ||
                        dy < short.MinValue || dy > short.MaxValue ||
                        dz < short.MinValue || dz > short.MaxValue
                        )
                    {
                        System.Diagnostics.Debug.Assert(Renderers != null);
                        foreach (EntityRenderer renderer in Renderers.GetKeys())
                        {
                            System.Diagnostics.Debug.Assert(renderer != null);
                            renderer.Teleport(Id, p, _look, false);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(Renderers != null);
                        foreach (EntityRenderer renderer in Renderers.GetKeys())
                        {
                            System.Diagnostics.Debug.Assert(renderer != null);
                            renderer.RelativeMoveAndRotate(Id, dx, dy, dz, _look);
                        }
                    }
                    
                }
                else if (moved == true)
                {
                    System.Diagnostics.Debug.Assert(_rotated == false);

                    double dx = (p.X - _p.X) * (32 * 128);
                    double dy = (p.Y - _p.Y) * (32 * 128);
                    double dz = (p.Z - _p.Z) * (32 * 128);

                    if (
                        dx < short.MinValue || dx > short.MaxValue ||
                        dy < short.MinValue || dy > short.MaxValue ||
                        dz < short.MinValue || dz > short.MaxValue
                        )
                    {
                        System.Diagnostics.Debug.Assert(Renderers != null);
                        foreach (EntityRenderer renderer in Renderers.GetKeys())
                        {
                            System.Diagnostics.Debug.Assert(renderer != null);
                            renderer.Teleport(Id, p, _look, false);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(Renderers != null);
                        foreach (EntityRenderer renderer in Renderers.GetKeys())
                        {
                            System.Diagnostics.Debug.Assert(renderer != null);
                            renderer.RelativeMove(Id, dx, dy, dz);
                        }
                    }
                }
                else if (_rotated == true)
                {
                    System.Diagnostics.Debug.Assert(moved == false);

                    System.Diagnostics.Debug.Assert(Renderers != null);
                    foreach (EntityRenderer renderer in Renderers.GetKeys())
                    {
                        System.Diagnostics.Debug.Assert(renderer != null);
                        renderer.Rotate(Id, _look);
                    }

                }
                else
                {
                    System.Diagnostics.Debug.Assert(moved == false);
                    System.Diagnostics.Debug.Assert(_rotated == false);

                    System.Diagnostics.Debug.Assert(Renderers != null);
                    foreach (EntityRenderer renderer in Renderers.GetKeys())
                    {
                        System.Diagnostics.Debug.Assert(renderer != null);
                        renderer.Stand(Id);
                    }
                }

                _canHaveMovement = false;

            }
            else if (_teleported == true)
            {
                _p = _pTeleport;

                _rotated = false;
                _teleported = false;
            }

            if (_fakeBlockApplied == true)
            {
                foreach (EntityRenderer renderer in Renderers.GetKeys())
                {
                    System.Diagnostics.Debug.Assert(renderer != null);
                    renderer.SetBlockAppearance(_fakeBlock, blockLocatioin);
                }
            }

            _p = p;
            _rotated = false;

            _prevFakeBlockApplied = _fakeBlockApplied;
            _prevFakeBlock = _fakeBlock;

            base.Move(_world, volume, v);

            OnMove(_world);
        }

        internal void SetEntityStatus(byte v)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(Renderers != null);
            if (Renderers.Empty == true)
            {
                return;
            }

            foreach (EntityRenderer renderer in Renderers.GetKeys())
            {
                System.Diagnostics.Debug.Assert(renderer != null);
                renderer.SetEntityStatus(Id, v);
            }
        }

        internal void Rotate(EntityAngles look)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(LockerTeleport != null);
            _LockerRotate.Hold();

            try
            {
                _rotated = true;
                _look = look;


            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerTeleport != null);
                _LockerRotate.Release();
            }
        }


        private void ChangeForms(bool sneaking, bool sprinting)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(Renderers != null);
            if (Renderers.Empty == true)
            {
                return;
            }

            foreach (EntityRenderer renderer in Renderers.GetKeys())
            {
                System.Diagnostics.Debug.Assert(renderer != null);
                renderer.ChangeForms(Id, sneaking, sprinting);
            }
        }

        // Entity's forms was reset after teleported in minecraft client...
        private void ResetForms()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            _sneaking = false;
            _sprinting = false;

            System.Diagnostics.Debug.Assert(Renderers != null);
            if (Renderers.Empty == true)
            {
                return;
            }

            foreach (EntityRenderer renderer in Renderers.GetKeys())
            {
                System.Diagnostics.Debug.Assert(renderer != null);
                renderer.ChangeForms(Id, _sneaking, _sprinting);
            }
        }

        internal void Sneak(World world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(!IsSneaking);

            if (_sneaking == true)
            {
                return;
            }

            _sneaking = true;

            OnSneak(world, _sneaking);
            
            ChangeForms(_sneaking, _sprinting);
        }

        internal void Unsneak(World world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(IsSneaking);

            if (_sneaking == false)
            {
                return;
            }
                
            _sneaking = false;

            OnSneak(world, _sneaking);

            ChangeForms(_sneaking, _sprinting);
        }

        internal void Sprint(World world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(!IsSprinting);

            if (_sprinting == true)
            {
                return;
            }

            _sprinting = true;

            OnSprint(world, _sprinting);

            ChangeForms(_sneaking, _sprinting);
        }

        internal void Unsprint(World world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(IsSprinting);

            if (_sprinting == false)
            {
                return;
            }

            _sprinting = false;

            OnSprint(world, _sprinting);

            ChangeForms(_sneaking, _sprinting);
        }

        public virtual void Teleport(Vector p, EntityAngles look)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(LockerTeleport != null);
            LockerTeleport.Hold();

            try
            {
                _teleported = true;
                _pTeleport = p;

                Rotate(look);

            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerTeleport != null);
                LockerTeleport.Release();
            }
        }
        
        internal virtual void _Animate(EntityAnimation animation)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(Renderers != null);
            if (Renderers.Empty == true)
            {
                return;
            }

            foreach (EntityRenderer renderer in Renderers.GetKeys())
            {
                System.Diagnostics.Debug.Assert(renderer != null);
                renderer.Animate(Id, animation);
            }
        }

        public void Animate(EntityAnimation animation)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            _Animate(animation);
        }

        internal virtual void _AddEffect(
            byte effectId,
            byte amplifier, int duration, byte flags)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(Renderers != null);
            if (Renderers.Empty == true)
            {
                return;
            }

            foreach (EntityRenderer renderer in Renderers.GetKeys())
            {
                System.Diagnostics.Debug.Assert(renderer != null);
                renderer.AddEffect(Id, effectId, amplifier, duration, flags);
            }
        }

        internal virtual void _EmitParticles(
            Particle particle, Vector v,
            double speed, int count,
            double offsetX, double offsetY, double offsetZ)
        {
            System.Diagnostics.Debug.Assert(offsetX >= 0.0);
            System.Diagnostics.Debug.Assert(offsetX <= 1.0);
            System.Diagnostics.Debug.Assert(offsetY >= 0.0);
            System.Diagnostics.Debug.Assert(offsetY <= 1.0);
            System.Diagnostics.Debug.Assert(offsetZ >= 0.0);
            System.Diagnostics.Debug.Assert(offsetZ <= 1.0);
            System.Diagnostics.Debug.Assert(speed >= 0.0);
            System.Diagnostics.Debug.Assert(speed <= 1.0);
            System.Diagnostics.Debug.Assert(count >= 0);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(Renderers != null);
            if (Renderers.Empty == true)
            {
                return;
            }

            foreach (EntityRenderer renderer in Renderers.GetKeys())
            {
                System.Diagnostics.Debug.Assert(renderer != null);
                renderer.ShowParticles(particle, v, (float)speed, count, (float)offsetX, (float)offsetY, (float)offsetZ);
            }
        }

        public void EmitParticles(
            Particle particle, Vector offset,
            double speed, int count,
            double offsetX, double offsetY, double offsetZ)
        {
            if (speed < 0.0 || speed > 1.0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(speed));
            }
            if (count < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(count));
            }

            if (offsetX < 0.0 || offsetX > 1.0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(offsetX));
            }
            if (offsetY < 0.0 || offsetY > 1.0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(offsetY));
            }
            if (offsetZ < 0.0 || offsetZ > 1.0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(offsetZ));
            }

            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(_disposed == false);

            _EmitParticles(particle, Position + offset, speed, count, offsetX, offsetY, offsetZ);
        }

        public void EmitParticles(
            Particle particle, Vector offset,
            double speed, int count)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            EmitParticles(particle, offset, speed, count, 0.0F, 0.0F, 0.0F);
        }

        public void EmitRgbParticle(
            Vector offset,
            double red, double green, double blue)
        {
            if (red < 0.0 || red > 1.0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(red));
            }
            if (green < 0.0 || green > 1.0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(green));
            }
            if (blue < 0.0 || blue > 1.0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(blue));
            }

            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (red == 0.0)
            {
                red = 0.00001;
            }
            if (green == 0.0)
            {
                green = 0.00001;
            }
            if (blue == 0.0)
            {
                blue = 0.00001;
            }

            _EmitParticles(Particle.Reddust, Position + offset, 1.0, 0, red, green, blue);
        }

        public void EmitParticles(
            Particle particle,
            double speed, int count,
            double red, double green, double blue)
        {
            if (speed < 0.0F || speed > 1.0F)
            {
                throw new System.ArgumentOutOfRangeException(nameof(speed));
            }
            if (count < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(count));
            }

            if (red < 0.0D || red > 1.0D)
            {
                throw new System.ArgumentOutOfRangeException(nameof(red));
            }
            if (green < 0.0D || green > 1.0D)
            {
                throw new System.ArgumentOutOfRangeException(nameof(green));
            }
            if (blue < 0.0D || blue > 1.0D)
            {
                throw new System.ArgumentOutOfRangeException(nameof(blue));
            }

            System.Diagnostics.Debug.Assert(_disposed == false);

            _EmitParticles(particle, Position, speed, count, red, green, blue);
        }

        public void EmitParticles(
            Particle particle,
            double speed, int count)
        {
            EmitParticles(particle, speed, count, 0.0F, 0.0F, 0.0F);
        }

        public void ApplyBlockAppearance(Block block)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            _fakeBlockApplied = true;
            _fakeBlock = block;
        }

        public void ResetBlockAppearance()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            _fakeBlockApplied = false;
            _fakeBlock = Block.Air;
        }

        protected internal virtual void OnPressHandSwapButton(World world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(_disposed == false);
        }

        internal override void Flush(PhysicsWorld _world)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(Renderers != null);
            if (Renderers.Empty == false)
            {
                System.Diagnostics.Debug.Assert(Renderers != null);
                EntityRenderer[] renderers = Renderers.Flush();

                if (_world is World world && _prevFakeBlockApplied == true)
                {
                    BlockLocation prevBlockLocation = BlockLocation.Generate(_p);

                    System.Diagnostics.Debug.Assert(world.BlockContext != null);
                    Block prevBlock = world.BlockContext.GetBlock(prevBlockLocation);

                    foreach (EntityRenderer renderer in renderers)
                    {
                        System.Diagnostics.Debug.Assert(renderer != null);
                        if (renderer.IsDisconnected == true)
                        {
                            continue;
                        }

                        renderer.SetBlockAppearance(prevBlock, prevBlockLocation);
                    }

                }
                else
                {
                    foreach (EntityRenderer renderer in renderers)
                    {
                        System.Diagnostics.Debug.Assert(renderer != null);
                        if (renderer.IsDisconnected == true)
                        {
                            continue;
                        }

                        renderer.DestroyEntity(Id);
                    }
                }

            }



            base.Flush(_world);
        }

        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed == false)
            {
                System.Diagnostics.Debug.Assert(_canHaveMovement == false);

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing == true)
                {
                    // Dispose managed resources.
                    EntityIdAllocator.Deallocate(Id);
                    Renderers.Dispose();

                    _LockerRotate.Dispose();
                    LockerTeleport.Dispose();
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

    //public abstract class BlockEntity : Entity
    //{
    //    private static readonly Hitbox DefaultHitbox = new(1.0D, 1.0D);
    //    public const double DefaultMass = 1.0D;
    //    public const double DefaultMaxStepLevel = 0.0D;


    //    private bool _disposed = false;


    //    internal BlockEntity(Vector p, Look look, bool noGravity) :
    //        base(System.Guid.NewGuid(), p, look, noGravity,
    //            DefaultHitbox,
    //            DefaultMass, DefaultMaxStepLevel)
    //    {
    //    }

    //    public override void Dispose()
    //    {
    //        // Assertions.
    //        System.Diagnostics.Debug.Assert(_disposed == false);

    //        // Release resources.

    //        // Finish.
    //        base.Dispose();
    //        _disposed = true;
    //    }
    //}

    //public abstract class FixedBlockEntity : BlockEntity
    //{
    //    private bool _disposed = false;



    //    public FixedBlockEntity(Vector p) : base(p, new Look(), true)
    //    {

    //    }

    //    private protected override void RenderSpawning(
    //        EntityRenderer renderer)
    //    {
    //        System.Diagnostics.Debug.Assert(renderer != null);

    //        throw new System.NotImplementedException();
    //    }

    //    public override void Dispose()
    //    {
    //        // Assertions.
    //        System.Diagnostics.Debug.Assert(_disposed == false);

    //        // Release resources.

    //        // Finish.
    //        base.Dispose();
    //        _disposed = true;
    //    }

    //}

    /*public sealed class MovableBlockEntity : BlockEntity
    {

    }*/



}

﻿
using Common;
using Sync;
using Containers;

using MinecraftServerEngine.Blocks;
using MinecraftServerEngine.Renderers;
using MinecraftServerEngine.Physics;

namespace MinecraftServerEngine.Entities
{

    public abstract class Entity : PhysicsObject
    {
        private protected readonly struct Hitbox : System.IEquatable<Hitbox>
        {
            internal static Hitbox Empty = new(0.0D, 0.0D);

            private readonly double Width, Height;
            /*public readonly double EyeHeight, MaxStepHeight;*/

            internal Hitbox(double w, double h)
            {
                System.Diagnostics.Debug.Assert(w >= 0.0D);
                System.Diagnostics.Debug.Assert(h >= 0.0D);

                Width = w;
                Height = h;
            }

            internal readonly BoundingVolume Convert(Vector p)
            {
                if (Width == 0.0D || Height == 0.0D)
                {
                    System.Diagnostics.Debug.Assert(Width == 0.0D);
                    System.Diagnostics.Debug.Assert(Height == 0.0D);

                    return new EmptyBoundingVolume(p);
                }

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


        public readonly int Id;
        public readonly System.Guid UniqueId;

        private bool _hasMovement = false;

        internal readonly ConcurrentTree<EntityRenderer> Renderers = new();  // Disposable


        private readonly Locker LockerRotate = new();
        private bool _rotated = false;
        private EntityAngles _look;
        public EntityAngles Look => _look;


        protected bool _sneaking = false, _sprinting = false;
        public bool Sneaking => _sneaking;
        public bool Sprinting => _sprinting;


        protected readonly Locker LockerTeleport = new();
        private bool _teleported = false;
        private Vector _pTeleport;

        private bool _prevFakeBlockApplied = false, _fakeBlockApplied = false;
        private Block _prevFakeBlock, _fakeBlock;

        // ApplyBlockAppearance
        // TransformAppearance

        private readonly bool NoGravity;


        private protected Entity(
            System.Guid uniqueId,
            Vector p, EntityAngles look,
            bool noGravity,
            Hitbox hitbox,
            double mass, double maxStepHeight)
            : base(p, mass, hitbox.Convert(p), new StepableMovement(maxStepHeight))
        {
            Id = EntityIdAllocator.Allocate();
            UniqueId = uniqueId;

            System.Diagnostics.Debug.Assert(!_rotated);
            _look = look;

            System.Diagnostics.Debug.Assert(!_sneaking);
            System.Diagnostics.Debug.Assert(!_sprinting);

            NoGravity = noGravity;
        }

        ~Entity()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        private protected abstract Hitbox GetHitbox();

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

            System.Diagnostics.Debug.Assert(!_disposed);

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

        protected override (BoundingVolume, bool noGravity) GetCurrentStatus()
        {
            Hitbox hitbox = GetHitbox();

            Forces.Enqueue(
                -1.0D *
                new Vector(1.0D - 0.91D, 1.0D - 0.9800000190734863D, 1.0D - 0.91D) *
                Velocity);  // Damping Force

            BoundingVolume volume = _teleported ? hitbox.Convert(_pTeleport) : hitbox.Convert(_p);
            return (volume, NoGravity || volume is EmptyBoundingVolume);
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
                System.Diagnostics.Debug.Assert(!_hasMovement);

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

                            if (renderer.Disconnected == false)
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

                            if (renderer.Disconnected == false)
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

                        if (renderer.Disconnected == false)
                        {
                            renderer.DestroyEntity(Id);
                        }

                        Renderers.Extract(renderer);
                    }
                }


                System.Diagnostics.Debug.Assert(_hasMovement == false);
                _hasMovement = true;
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
                        if (renderer.Disconnected == true)
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
                        if (renderer.Disconnected == true)
                        {
                            continue;
                        }

                        System.Diagnostics.Debug.Assert(renderer != null);
                        renderer.DestroyEntity(Id);
                    }
                }

                System.Diagnostics.Debug.Assert(_hasMovement == false);
                _hasMovement = false;
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
                if (_teleported == true)
                {
                    System.Diagnostics.Debug.Assert(Renderers.Empty == false);
                    if (_fakeBlockApplied == false)
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

                System.Diagnostics.Debug.Assert(_hasMovement == true);

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
                    System.Diagnostics.Debug.Assert(Renderers != null);
                    foreach (EntityRenderer renderer in Renderers.GetKeys())
                    {
                        System.Diagnostics.Debug.Assert(renderer != null);
                        renderer.RelMoveAndRotate(Id, p, _p, _look);
                    }
                }
                else if (moved == true)
                {
                    System.Diagnostics.Debug.Assert(!_rotated);

                    System.Diagnostics.Debug.Assert(Renderers != null);
                    foreach (EntityRenderer renderer in Renderers.GetKeys())
                    {
                        System.Diagnostics.Debug.Assert(renderer != null);
                        renderer.RelMove(Id, p, _p);
                    }
                }
                else if (_rotated == true)
                {
                    System.Diagnostics.Debug.Assert(!moved);

                    System.Diagnostics.Debug.Assert(Renderers != null);
                    foreach (EntityRenderer renderer in Renderers.GetKeys())
                    {
                        System.Diagnostics.Debug.Assert(renderer != null);
                        renderer.Rotate(Id, _look);
                    }

                }
                else
                {
                    System.Diagnostics.Debug.Assert(!moved);
                    System.Diagnostics.Debug.Assert(!_rotated);

                    System.Diagnostics.Debug.Assert(Renderers != null);
                    foreach (EntityRenderer renderer in Renderers.GetKeys())
                    {
                        System.Diagnostics.Debug.Assert(renderer != null);
                        renderer.Stand(Id);
                    }
                }

                _hasMovement = false;

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
            System.Diagnostics.Debug.Assert(!_disposed);

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
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(LockerTeleport != null);
            LockerRotate.Hold();

            try
            {
                _rotated = true;
                _look = look;


            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerTeleport != null);
                LockerRotate.Release();
            }
        }





        private void ChangeForms(bool sneaking, bool sprinting)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

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

            System.Diagnostics.Debug.Assert(!_disposed);

            //System.Diagnostics.Debug.Assert(!Sneaking);

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

            System.Diagnostics.Debug.Assert(!_disposed);

            //System.Diagnostics.Debug.Assert(Sneaking);

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

            System.Diagnostics.Debug.Assert(!_disposed);

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

            System.Diagnostics.Debug.Assert(!_disposed);

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
            double r, double g, double b)
        {
            System.Diagnostics.Debug.Assert(r >= 0.0D);
            System.Diagnostics.Debug.Assert(r <= 1.0D);
            System.Diagnostics.Debug.Assert(g >= 0.0D);
            System.Diagnostics.Debug.Assert(g <= 1.0D);
            System.Diagnostics.Debug.Assert(b >= 0.0D);
            System.Diagnostics.Debug.Assert(b <= 1.0D);
            System.Diagnostics.Debug.Assert(speed >= 0.0F);
            System.Diagnostics.Debug.Assert(speed <= 1.0F);
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
                renderer.ShowParticles(particle, v, (float)speed, count, (float)r, (float)g, (float)b);
            }
        }

        public void EmitParticles(
            Particle particle, Vector offset,
            double speed, int count,
            double red, double green, double blue)
        {
            if (speed < 0.0 || speed > 1.0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(speed));
            }
            if (count < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(count));
            }

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

            System.Diagnostics.Debug.Assert(_disposed == false);

            _EmitParticles(particle, Position + offset, speed, count, red, green, blue);
        }

        public void EmitParticles(
            Particle particle, Vector offset,
            double speed, int count)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            EmitParticles(particle, offset, speed, count, 0.0F, 0.0F, 0.0F);
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
            System.Diagnostics.Debug.Assert(_disposed == false);

            EmitParticles(particle, speed, count, 0.0F, 0.0F, 0.0F);
        }

        public void ResetBlockAppearance()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            _fakeBlockApplied = false;
            _fakeBlock = Block.Air;
        }

        public void ApplyBlockAppearance(Block block)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            _fakeBlockApplied = true;
            _fakeBlock = block;
        }
        
        protected internal virtual void OnPressHandSwapButton(World world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(_disposed == false);
        }

        internal override void Flush(PhysicsWorld _world)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

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
                        if (renderer.Disconnected == true)
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
                        if (renderer.Disconnected == true)
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
                System.Diagnostics.Debug.Assert(_hasMovement == false);

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing == true)
                {
                    // Dispose managed resources.
                    EntityIdAllocator.Deallocate(Id);
                    Renderers.Dispose();

                    LockerRotate.Dispose();
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
    //        System.Diagnostics.Debug.Assert(!_disposed);

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
    //        System.Diagnostics.Debug.Assert(!_disposed);

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

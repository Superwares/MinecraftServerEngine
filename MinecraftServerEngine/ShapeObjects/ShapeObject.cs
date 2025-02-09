
using Common;
using Containers;

using MinecraftServerEngine.Renderers;
using MinecraftServerEngine.Physics;
using MinecraftServerEngine.Physics.BoundingVolumes;
using MinecraftServerEngine.Blocks;
using MinecraftServerEngine.Particles;

namespace MinecraftServerEngine.ShapeObjects
{
    public abstract class ShapeObject : PhysicsObject
    {
        private bool _disposed = false;


        private Vector _center;
        public Vector Center => _center;


        private BoundingVolume _bv;


        internal readonly ConcurrentTree<ShapeObjectRenderer> Renderers = new();  // Disposable


        private protected ShapeObject(BoundingVolume bv) : base(0.0, bv, new EmptyMovement())
        {
            _bv = bv;

            _center = bv.GetCenter();
        }


        internal abstract double _GetArea();

        public double GetArea()
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            double area = _GetArea();

            System.Diagnostics.Debug.Assert(area >= 0.0);
            return area;
        }

        protected override (BoundingVolume, bool noGravity) GetCurrentStatus()
        {
            System.Diagnostics.Debug.Assert(_bv != null);
            return (_bv, false);
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

            foreach (ShapeObjectRenderer renderer in Renderers.GetKeys())
            {
                System.Diagnostics.Debug.Assert(renderer != null);
                renderer.ShowParticles(particle, v, (float)speed, count, (float)r, (float)g, (float)b);
            }
        }

        public void EmitParticles(
            Particle particle, Vector v,
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

            System.Diagnostics.Debug.Assert(Renderers != null);
            if (Renderers.Empty == true)
            {
                return;
            }

            _EmitParticles(particle, v, speed, count, offsetX, offsetY, offsetZ);
        }

        public void EmitRgbParticle(
            Vector v,
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

            _EmitParticles(Particle.Reddust, v, 1.0, 0, red, green, blue);
        }

        internal void ApplyRenderer(ShapeObjectRenderer renderer)
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
        }

        private protected override void _ApplyForce(Vector v)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);
        }

        internal override void Move(PhysicsWorld _world, BoundingVolume volume, Vector v)
        {
            System.Diagnostics.Debug.Assert(_world != null);
            System.Diagnostics.Debug.Assert(volume != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            Vector p = volume.GetCenter();

            if (volume is not EmptyBoundingVolume)
            {
                using Queue<ShapeObjectRenderer> queue = new();

                System.Diagnostics.Debug.Assert(Renderers != null);
                foreach (ShapeObjectRenderer renderer in Renderers.GetKeys())
                {
                    System.Diagnostics.Debug.Assert(renderer != null);

                    if (renderer.CanRender(p) == true)
                    {
                        continue;
                    }

                    queue.Enqueue(renderer);
                }

                while (queue.Empty == false)
                {
                    ShapeObjectRenderer renderer = queue.Dequeue();

                    if (renderer.IsDisconnected == false)
                    {
                    }

                    Renderers.Extract(renderer);

                    //MyConsole.Debug("Extract Renderer!");
                }


            }
            else
            {
                System.Diagnostics.Debug.Assert(volume is EmptyBoundingVolume);

                System.Diagnostics.Debug.Assert(Renderers != null);
                ShapeObjectRenderer[] renderers = Renderers.Flush();

            }

            System.Diagnostics.Debug.Assert(v == Vector.Zero);
            base.Move(_world, volume, v);
        }

        internal override void Flush(PhysicsWorld _world)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(Renderers != null);
            if (Renderers.Empty == false)
            {
                System.Diagnostics.Debug.Assert(Renderers != null);
                ShapeObjectRenderer[] renderers = Renderers.Flush();

                //foreach (ShapeObjectRenderer renderer in renderers)
                //{
                //    System.Diagnostics.Debug.Assert(renderer != null);
                //    if (renderer.Disconnected == true)
                //    {
                //        continue;
                //    }

                    
                //}

            }

            base.Flush(_world);
        }

        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed == false)
            {

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing == true)
                {
                    // Dispose managed resources.
                    System.Diagnostics.Debug.Assert(Renderers != null);
                    Renderers.Dispose();

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

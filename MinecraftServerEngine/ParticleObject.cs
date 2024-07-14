
using Containers;

namespace MinecraftServerEngine
{
    using PhysicsEngine;

    public abstract class ParticleObject : PhysicsObject
    {
        private sealed class RendererManager : System.IDisposable
        {
            private bool _disposed = false;

            private readonly ConcurrentTree<ParticleObjectRenderer> Renderers = new();  // Disposable

            public RendererManager()
            {
            }

            ~RendererManager() => System.Diagnostics.Debug.Assert(false);

            internal bool Apply(ParticleObjectRenderer renderer)
            {
                System.Diagnostics.Debug.Assert(renderer != null);

                System.Diagnostics.Debug.Assert(!_disposed);

                if (Renderers.Contains(renderer))
                {
                    return false;
                }

                System.Diagnostics.Debug.Assert(Renderers != null);
                Renderers.Insert(renderer);

                return true;
            }

            internal void HandleRendering(Vector p)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                using Queue<ParticleObjectRenderer> queue = new();

                System.Diagnostics.Debug.Assert(Renderers != null);
                foreach (ParticleObjectRenderer renderer in Renderers.GetKeys())
                {
                    System.Diagnostics.Debug.Assert(renderer != null);

                    if (renderer.CanRender(p))
                    {
                        continue;
                    }

                    queue.Enqueue(renderer);
                }

                while (!queue.Empty)
                {
                    ParticleObjectRenderer renderer = queue.Dequeue();

                    Renderers.Extract(renderer);
                }

            }


            internal void Move(Vector p, float r, float g, float b)
            {
                System.Diagnostics.Debug.Assert(r > 0.0D);
                System.Diagnostics.Debug.Assert(r <= 1.0D);
                System.Diagnostics.Debug.Assert(g > 0.0D);
                System.Diagnostics.Debug.Assert(g <= 1.0D);
                System.Diagnostics.Debug.Assert(b > 0.0D);
                System.Diagnostics.Debug.Assert(b <= 1.0D);

                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(Renderers != null);
                foreach (ParticleObjectRenderer renderer in Renderers.GetKeys())
                {
                    System.Diagnostics.Debug.Assert(renderer != null);
                    renderer.Move(p, r, g, b);
                }
            
            }

            public void Dispose()
            {
                // Assertions.
                System.Diagnostics.Debug.Assert(!_disposed);

                // Release  resources.
                Renderers.Dispose();

                // Finish
                System.GC.SuppressFinalize(this);
                _disposed = true;
            }

        }

        private const double Radius = 0.25D;

        private bool _disposed = false;

        private readonly float Red, Green, Blue;

        private RendererManager Manager = new();  // Disposable

        private static float Normalize(byte v)
        {
            return (v > 0) ? ((float)v / (float)byte.MaxValue) : 0.0001F;
        }

        internal ParticleObject(Vector p, double m, byte r, byte g, byte b, Movement movement)
            : base(m, AxisAlignedBoundingBox.Generate(p, Radius), movement)
        {
            System.Diagnostics.Debug.Assert(movement != null);

            Red = Normalize(r);
            Green = Normalize(g);
            Blue = Normalize(b);
        }

        internal void ApplyRenderer(ParticleObjectRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(renderer != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Manager != null);
            Manager.Apply(renderer);
        }

        protected override BoundingVolume GenerateBoundingVolume()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // TODO: Apply force based on volume size.
            Forces.Enqueue(
                -1.0D *
                new Vector(1.0D - 0.9999d, 1.0D - 0.9999d, 1.0D - 0.9999d) *
                Velocity);  // Damping Force

            return BoundingVolume;
        }

        internal override void Move(BoundingVolume volume, Vector v)
        {
            System.Diagnostics.Debug.Assert(volume != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            Vector p = volume.GetCenter();

            System.Diagnostics.Debug.Assert(Manager != null);
            Manager.HandleRendering(p);
            Manager.Move(p, Red, Green, Blue);

            base.Move(volume, v);
        }

        // TODO: set color

        public override void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release Resources.
            Manager.Dispose();

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

    public abstract class FreeParticle : ParticleObject
    {
        protected FreeParticle(Vector p, double m, byte r, byte g, byte b)
            : base(p, m, r, g, b, new WallPharsing())
        {

        }
    }

    public abstract class SimpleParticle : ParticleObject
    {
        protected SimpleParticle(Vector p, double m, byte r, byte g, byte b)
            : base(p, m, r, g, b, new SimpleMovement())
        {

        }
    }

    public abstract class SmoothParticle : ParticleObject
    {
        protected SmoothParticle(Vector p, double m, byte r, byte g, byte b)
            : base(p, m, r, g, b, new SmoothMovement())
        {

        }
    }

}

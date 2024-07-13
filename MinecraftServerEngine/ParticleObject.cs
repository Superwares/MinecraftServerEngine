

namespace MinecraftServerEngine
{
    using PhysicsEngine;

    public abstract class ParticleObject : PhysicsObject
    {


        private const double Radius = 0.25D;

        private bool _disposed = false;

        private readonly float Red, Green, Blue;

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

            return BoundingVolume;
        }

        internal override void Move(BoundingVolume volume, Vector v)
        {
            System.Diagnostics.Debug.Assert(volume != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            Vector p = volume.GetCenter();
            Manager.Move(p, Red, Green, Blue);

            base.Move(volume, v);
        }

        public override void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release Resources.

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

    public abstract class FreeParticle : ParticleObject
    {
        internal FreeParticle(Vector p, double m, byte r, byte g, byte b)
            : base(p, m, r, g, b, new WallPharsing())
        {

        }
    }

    public abstract class SimpleParticle : ParticleObject
    {
        internal SimpleParticle(Vector p, double m, byte r, byte g, byte b)
            : base(p, m, r, g, b, new SimpleMovement())
        {

        }
    }

    public abstract class SmoothParticle : ParticleObject
    {
        internal SmoothParticle(Vector p, double m, byte r, byte g, byte b)
            : base(p, m, r, g, b, new SmoothMovement())
        {

        }
    }

}

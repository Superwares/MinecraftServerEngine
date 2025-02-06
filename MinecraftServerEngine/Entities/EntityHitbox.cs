


using MinecraftServerEngine.Physics;
using MinecraftServerEngine.Physics.BoundingVolumes;

namespace MinecraftServerEngine.Entities
{
    public readonly struct EntityHitbox
    {
        internal static EntityHitbox Empty = new(0.0D, 0.0D);

        private readonly double Width, Height;
        /*public readonly double EyeHeight, MaxStepHeight;*/

        internal EntityHitbox(double w, double h)
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

        public readonly bool Equals(EntityHitbox other)
        {
            throw new System.NotImplementedException();
        }
    }
}

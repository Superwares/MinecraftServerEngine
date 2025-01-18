using Common;

namespace MinecraftServerEngine.PhysicsEngine
{
    public abstract class BoundingVolume
    {
        internal abstract void Extend(Vector v);
        internal abstract void Move(Vector v);
        internal abstract bool Intersects(Vector o, Vector d);

        internal abstract Vector GetCenter();
        internal abstract Vector GetBottomCenter();

        internal abstract double GetHeight();

        internal abstract AxisAlignedBoundingBox GetMinBoundingBox();

        internal abstract bool TestIntersection(BoundingVolume volume);
        /*public abstract bool TestIntersection(BoundingVolume volume, Vector v);*/

        /*public override string ToString()
        {
            return base.ToString();
        }*/
    }

    public class EmptyBoundingVolume : BoundingVolume
    {
        private Vector _p;
        public Vector Position => _p;

        internal EmptyBoundingVolume(Vector p)
        {
            _p = p;
        }

        internal override void Extend(Vector v)
        {

        }

        internal override void Move(Vector v)
        {
            _p += v;
        }

        internal override bool Intersects(Vector o, Vector d)
        {
            return false;
        }

        internal override Vector GetCenter()
        {
            return _p;
        }

        internal override Vector GetBottomCenter()
        {
            return _p;
        }

        internal override double GetHeight()
        {
            return 0.0D;
        }

        internal override AxisAlignedBoundingBox GetMinBoundingBox()
        {
            return new(_p, _p);
        }

        internal override bool TestIntersection(BoundingVolume volume)
        {
            throw new System.NotImplementedException();
        }


    }

    public class AxisAlignedBoundingBox : BoundingVolume
    {
        internal static AxisAlignedBoundingBox Generate(Vector p, double r)
        {
            System.Diagnostics.Debug.Assert(r > 0.0D);

            Vector max = new(p.X + r, p.Y + r, p.Z + r),
                min = new(p.X - r, p.Y - r, p.Z - r);
            return new AxisAlignedBoundingBox(max, min);
        }

        public static AxisAlignedBoundingBox Generate(Vector p, Vector d)
        {
            // Calculate min and max based on p (origin) and d (direction)
            Vector min = new Vector(
                System.Math.Min(p.X, p.X + d.X),
                System.Math.Min(p.Y, p.Y + d.Y),
                System.Math.Min(p.Z, p.Z + d.Z)
            );

            Vector max = new Vector(
                System.Math.Max(p.X, p.X + d.X),
                System.Math.Max(p.Y, p.Y + d.Y),
                System.Math.Max(p.Z, p.Z + d.Z)
            );

            // Return the axis-aligned bounding box
            return new AxisAlignedBoundingBox(max, min);
        }


        private Vector _max, _min;
        public Vector Max => _max;
        public Vector Min => _min;

        internal AxisAlignedBoundingBox(Vector max, Vector min)
        {
            System.Diagnostics.Debug.Assert(max.X >= min.X);
            System.Diagnostics.Debug.Assert(max.Y >= min.Y);
            System.Diagnostics.Debug.Assert(max.Z >= min.Z);

            _max = max;
            _min = min;
        }

        internal override void Extend(Vector v)
        {
            System.Diagnostics.Debug.Assert(Max.X >= Min.X);
            System.Diagnostics.Debug.Assert(Max.Y >= Min.Y);
            System.Diagnostics.Debug.Assert(Max.Z >= Min.Z);

            double xMax = Max.X, yMax = Max.Y, zMax = Max.Z,
                xMin = Min.X, yMin = Min.Y, zMin = Min.Z;
            if (v.X > 0.0D)
            {
                xMax += v.X;
            }
            else if (v.X < 0.0D)
            {
                xMin += v.X;
            }

            if (v.Y > 0.0D)
            {
                yMax += v.Y;
            }
            else if (v.Y < 0.0D)
            {
                yMin += v.Y;
            }

            if (v.Z > 0.0D)
            {
                zMax += v.Z;
            }
            else if (v.Z < 0.0D)
            {
                zMin += v.Z;
            }

            _max = new Vector(xMax, yMax, zMax);
            _min = new Vector(xMin, yMin, zMin);

            System.Diagnostics.Debug.Assert(Max.X >= Min.X);
            System.Diagnostics.Debug.Assert(Max.Y >= Min.Y);
            System.Diagnostics.Debug.Assert(Max.Z >= Min.Z);
        }

        internal override void Move(Vector v)
        {
            System.Diagnostics.Debug.Assert(Max.X >= Min.X);
            System.Diagnostics.Debug.Assert(Max.Y >= Min.Y);
            System.Diagnostics.Debug.Assert(Max.Z >= Min.Z);

            _max = _max + v; _min = _min + v;
        }
        internal override bool Intersects(Vector o, Vector d)
        {
            double tMin, tMax, tyMin, tyMax, tzMin, tzMax;

            // X-axis
            if (d.X != 0)
            {
                tMin = (Min.X - o.X) / d.X;
                tMax = (Max.X - o.X) / d.X;
                if (tMin > tMax) (tMin, tMax) = (tMax, tMin); // Swap if needed
            }
            else
            {
                // Ray parallel to X-axis, check if origin is within X bounds
                if (o.X < Min.X || o.X > Max.X) return false;
                tMin = double.NegativeInfinity;
                tMax = double.PositiveInfinity;
            }

            // Y-axis
            if (d.Y != 0)
            {
                tyMin = (Min.Y - o.Y) / d.Y;
                tyMax = (Max.Y - o.Y) / d.Y;
                if (tyMin > tyMax) (tyMin, tyMax) = (tyMax, tyMin); // Swap if needed
            }
            else
            {
                // Ray parallel to Y-axis, check if origin is within Y bounds
                if (o.Y < Min.Y || o.Y > Max.Y) return false;
                tyMin = double.NegativeInfinity;
                tyMax = double.PositiveInfinity;
            }

            // Merge X and Y slabs
            if ((tMin > tyMax) || (tyMin > tMax)) return false;
            tMin = System.Math.Max(tMin, tyMin);
            tMax = System.Math.Min(tMax, tyMax);

            // Z-axis
            if (d.Z != 0)
            {
                tzMin = (Min.Z - o.Z) / d.Z;
                tzMax = (Max.Z - o.Z) / d.Z;
                if (tzMin > tzMax) (tzMin, tzMax) = (tzMax, tzMin); // Swap if needed
            }
            else
            {
                // Ray parallel to Z-axis, check if origin is within Z bounds
                if (o.Z < Min.Z || o.Z > Max.Z) return false;
                tzMin = double.NegativeInfinity;
                tzMax = double.PositiveInfinity;
            }

            // Final merge
            if ((tMin > tzMax) || (tzMin > tMax)) return false;

            return true; // Ray intersects AABB
        }


        internal override Vector GetCenter()
        {
            System.Diagnostics.Debug.Assert(Max.X >= Min.X);
            System.Diagnostics.Debug.Assert(Max.Y >= Min.Y);
            System.Diagnostics.Debug.Assert(Max.Z >= Min.Z);

            double x = (Max.X + Min.X) / 2.0D,
                y = (Max.Y + Min.Y) / 2.0D,
                z = (Max.Z + Min.Z) / 2.0D;
            return new(x, y, z);
        }

        internal override Vector GetBottomCenter()
        {
            System.Diagnostics.Debug.Assert(Max.X >= Min.X);
            System.Diagnostics.Debug.Assert(Max.Y >= Min.Y);
            System.Diagnostics.Debug.Assert(Max.Z >= Min.Z);

            double x = (Max.X + Min.X) / 2.0D,
                y = Min.Y,
                z = (Max.Z + Min.Z) / 2.0D;
            return new(x, y, z);
        }

        internal override double GetHeight()
        {
            System.Diagnostics.Debug.Assert(Max.X >= Min.X);
            System.Diagnostics.Debug.Assert(Max.Y >= Min.Y);
            System.Diagnostics.Debug.Assert(Max.Z >= Min.Z);

            return (Max.Y - Min.Y);
        }

        internal override AxisAlignedBoundingBox GetMinBoundingBox()
        {
            System.Diagnostics.Debug.Assert(Max.X >= Min.X);
            System.Diagnostics.Debug.Assert(Max.Y >= Min.Y);
            System.Diagnostics.Debug.Assert(Max.Z >= Min.Z);

            return new(Max, Min);
        }

        internal override bool TestIntersection(BoundingVolume volume)
        {
            if (volume is AxisAlignedBoundingBox aabb)
            {
                return IntersectionTests.TestFixedAndFixed(this, aabb);
            }
            else if (volume is CompoundBoundingVolume cbv)
            {
                return IntersectionTests.TestFixedAndFixed(this, cbv);
            }

            throw new System.NotImplementedException();
        }

        /*public override bool TestIntersection(BoundingVolume volume, Vector v)
        {
            if (volume is AxisAlignedBoundingBox aabb)
            {
                return IntersectionTests.TestFixedAndMoving(this, aabb, v);
            }
            else if (volume is CompoundBoundingVolume cbv)
            {
                return IntersectionTests.TestFixedAndMoving(this, cbv, v);
            }

            throw new System.NotImplementedException();
        }*/

        internal (int axis, double t) ResolveCollision(BoundingVolume volume, Vector v)
        {
            if (volume is EmptyBoundingVolume)
            {
                return (-1, 0.0D);
            }
            else if (volume is AxisAlignedBoundingBox aabb)
            {
                return Collisions.Resolve(this, aabb, v);
            }
            else if (volume is CompoundBoundingVolume cbv)
            {
                return Collisions.Resolve(this, cbv, v);
            }

            throw new System.NotImplementedException();
        }

        /*public override string ToString()
        {
            return $"";
        }*/

    }

    /*public sealed class OrientedBoundingBox : BoundingVolume
    {

    }*/

    /*public sealed class BoundingSphere : BoundingVolume
    {

    }*/

    public sealed class CompoundBoundingVolume : BoundingVolume
    {
        public readonly BoundingVolume[] Volumes;

        public CompoundBoundingVolume(params BoundingVolume[] volumes)
        {
            // TODO: Assert all volumes are attached.
            Volumes = volumes;
        }

        internal override void Extend(Vector v)
        {
            throw new System.NotImplementedException();
        }

        internal override void Move(Vector v)
        {
            throw new System.NotImplementedException();
        }
        internal override bool Intersects(Vector o, Vector d)
        {
            throw new System.NotImplementedException();
        }

        internal override Vector GetCenter()
        {
            throw new System.NotImplementedException();
        }

        internal override Vector GetBottomCenter()
        {
            throw new System.NotImplementedException();
        }

        internal override double GetHeight()
        {
            throw new System.NotImplementedException();
        }

        internal override AxisAlignedBoundingBox GetMinBoundingBox()
        {
            throw new System.NotImplementedException();
        }

        internal override bool TestIntersection(BoundingVolume volume)
        {
            if (volume is AxisAlignedBoundingBox aabb)
            {
                return IntersectionTests.TestFixedAndFixed(this, aabb);
            }
            else if (volume is CompoundBoundingVolume cbv)
            {
                return IntersectionTests.TestFixedAndFixed(this, cbv);
            }

            throw new System.NotImplementedException();
        }

        /*public override bool TestIntersection(BoundingVolume volume, Vector v)
        {
            if (volume is AxisAlignedBoundingBox aabb)
            {
                return IntersectionTests.TestFixedAndMoving(this, aabb, v);
            }
            else if (volume is CompoundBoundingVolume cbv)
            {
                return IntersectionTests.TestFixedAndMoving(this, cbv, v);
            }

            throw new System.NotImplementedException();
        }*/

    }

}

using Common;

namespace MinecraftServerEngine.PhysicsEngine
{
    public abstract class BoundingVolume
    {
        public abstract void Extend(Vector v);
        public abstract void Move(Vector v);

        public abstract Vector GetCenter();
        public abstract Vector GetBottomCenter();

        public abstract AxisAlignedBoundingBox GetMinBoundingBox();

        public abstract bool TestIntersection(BoundingVolume volume);
        /*public abstract bool TestIntersection(BoundingVolume volume, Vector v);*/

        /*public override string ToString()
        {
            return base.ToString();
        }*/
    }

    public sealed class AxisAlignedBoundingBox : BoundingVolume
    {
        private Vector _max, _min;
        public Vector Max => _max;
        public Vector Min => _min;

        public AxisAlignedBoundingBox(Vector max, Vector min)
        {
            System.Diagnostics.Debug.Assert(max.X >= min.X);
            System.Diagnostics.Debug.Assert(max.Y >= min.Y);
            System.Diagnostics.Debug.Assert(max.Z >= min.Z);

            _max = max;
            _min = min;
        }

        public override void Extend(Vector v)
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

        public override void Move(Vector v)
        {
            System.Diagnostics.Debug.Assert(Max.X >= Min.X);
            System.Diagnostics.Debug.Assert(Max.Y >= Min.Y);
            System.Diagnostics.Debug.Assert(Max.Z >= Min.Z);

            _max = _max + v; _min = _min + v;
        }

        public override Vector GetCenter()
        {
            System.Diagnostics.Debug.Assert(Max.X >= Min.X);
            System.Diagnostics.Debug.Assert(Max.Y >= Min.Y);
            System.Diagnostics.Debug.Assert(Max.Z >= Min.Z);

            double x = (Max.X + Min.X) / 2.0D,
                y = (Max.Y + Min.Y) / 2.0D,
                z = (Max.Z + Min.Z) / 2.0D;
            return new(x, y, z);
        }

        public override Vector GetBottomCenter()
        {
            System.Diagnostics.Debug.Assert(Max.X >= Min.X);
            System.Diagnostics.Debug.Assert(Max.Y >= Min.Y);
            System.Diagnostics.Debug.Assert(Max.Z >= Min.Z);

            double x = (Max.X + Min.X) / 2.0D,
                y = Min.Y,
                z = (Max.Z + Min.Z) / 2.0D;
            return new(x, y, z);
        }

        public override AxisAlignedBoundingBox GetMinBoundingBox()
        {
            System.Diagnostics.Debug.Assert(Max.X >= Min.X);
            System.Diagnostics.Debug.Assert(Max.Y >= Min.Y);
            System.Diagnostics.Debug.Assert(Max.Z >= Min.Z);

            return new(Max, Min);
        }

        public override bool TestIntersection(BoundingVolume volume)
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

        internal (int, double) ResolveCollision(BoundingVolume volume, Vector v)
        {
            if (volume is AxisAlignedBoundingBox aabb)
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

        public override void Extend(Vector v)
        {
            throw new System.NotImplementedException();
        }

        public override void Move(Vector v)
        {
            throw new System.NotImplementedException();
        }

        public override  Vector GetCenter()
        {
            throw new System.NotImplementedException();
        }

        public override  Vector GetBottomCenter()
        {
            throw new System.NotImplementedException();
        }

        public override AxisAlignedBoundingBox GetMinBoundingBox()
        {
            throw new System.NotImplementedException();
        }

        public override  bool TestIntersection(BoundingVolume volume)
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

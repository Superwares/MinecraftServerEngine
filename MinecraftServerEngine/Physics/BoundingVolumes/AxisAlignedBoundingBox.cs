

namespace MinecraftServerEngine.Physics.BoundingVolumes
{
    public sealed class AxisAlignedBoundingBox : BoundingVolume
    {
        private const int _VertexCount = 8;

        //internal static AxisAlignedBoundingBox Generate(Vector p, double r)
        //{
        //    System.Diagnostics.Debug.Assert(r > 0.0D);

        //    Vector max = new(p.X + r, p.Y + r, p.Z + r),
        //        min = new(p.X - r, p.Y - r, p.Z - r);
        //    return new AxisAlignedBoundingBox(max, min);
        //}

        public readonly static Vector[] Axes = [new(1.0, 0.0, 0.0), new(0.0, 1.0, 0.0), new(0.0, 0.0, 1.0)];

        

        private Vector _max, _min;
        public Vector MaxVector => _max;
        public Vector MinVector => _min;


        internal readonly Vector[] Vertices = new Vector[_VertexCount];


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

        public static AxisAlignedBoundingBox Generate(Vector p, double r)
        {
            Vector min = new Vector(
                System.Math.Min(p.X + r, p.X - r),
                System.Math.Min(p.Y + r, p.Y - r),
                System.Math.Min(p.Z + r, p.Z - r)
            );

            Vector max = new Vector(
                System.Math.Max(p.X + r, p.X - r),
                System.Math.Max(p.Y + r, p.Y - r),
                System.Math.Max(p.Z + r, p.Z - r)
            );

            // Return the axis-aligned bounding box
            return new AxisAlignedBoundingBox(max, min);
        }

        private static void FindVertices(Vector[] vertices, Vector max, Vector min)
        {
            System.Diagnostics.Debug.Assert(vertices.Length == _VertexCount);

            System.Diagnostics.Debug.Assert(max.X >= min.X);
            System.Diagnostics.Debug.Assert(max.Y >= min.Y);
            System.Diagnostics.Debug.Assert(max.Z >= min.Z);

            vertices[0] = min;
            vertices[1] = new Vector(max.X, min.Y, min.Z);
            vertices[2] = new Vector(min.X, max.Y, min.Z);
            vertices[3] = new Vector(max.X, max.Y, min.Z);
            vertices[4] = new Vector(min.X, min.Y, max.Z);
            vertices[5] = new Vector(max.X, min.Y, max.Z);
            vertices[6] = new Vector(min.X, max.Y, max.Z);
            vertices[7] = max;
        }

        internal AxisAlignedBoundingBox(Vector max, Vector min)
        {
            System.Diagnostics.Debug.Assert(max.X >= min.X);
            System.Diagnostics.Debug.Assert(max.Y >= min.Y);
            System.Diagnostics.Debug.Assert(max.Z >= min.Z);

            _max = max;
            _min = min;

            FindVertices(Vertices, _max, _min);
        }



        internal override Vector GetCenter()
        {
            System.Diagnostics.Debug.Assert(MaxVector.X >= MinVector.X);
            System.Diagnostics.Debug.Assert(MaxVector.Y >= MinVector.Y);
            System.Diagnostics.Debug.Assert(MaxVector.Z >= MinVector.Z);

            double x = (MaxVector.X + MinVector.X) / 2.0D,
                y = (MaxVector.Y + MinVector.Y) / 2.0D,
                z = (MaxVector.Z + MinVector.Z) / 2.0D;
            return new(x, y, z);
        }

        internal override Vector GetBottomCenter()
        {
            System.Diagnostics.Debug.Assert(MaxVector.X >= MinVector.X);
            System.Diagnostics.Debug.Assert(MaxVector.Y >= MinVector.Y);
            System.Diagnostics.Debug.Assert(MaxVector.Z >= MinVector.Z);

            double x = (MaxVector.X + MinVector.X) / 2.0D,
                y = MinVector.Y,
                z = (MaxVector.Z + MinVector.Z) / 2.0D;
            return new(x, y, z);
        }

        internal override double GetHeight()
        {
            System.Diagnostics.Debug.Assert(MaxVector.X >= MinVector.X);
            System.Diagnostics.Debug.Assert(MaxVector.Y >= MinVector.Y);
            System.Diagnostics.Debug.Assert(MaxVector.Z >= MinVector.Z);

            return (MaxVector.Y - MinVector.Y);
        }

        public override AxisAlignedBoundingBox GetMinBoundingBox()
        {
            System.Diagnostics.Debug.Assert(MaxVector.X >= MinVector.X);
            System.Diagnostics.Debug.Assert(MaxVector.Y >= MinVector.Y);
            System.Diagnostics.Debug.Assert(MaxVector.Z >= MinVector.Z);

            return new(MaxVector, MinVector);
        }

        internal override void Extend(Vector extents)
        {
            System.Diagnostics.Debug.Assert(MaxVector.X >= MinVector.X);
            System.Diagnostics.Debug.Assert(MaxVector.Y >= MinVector.Y);
            System.Diagnostics.Debug.Assert(MaxVector.Z >= MinVector.Z);

            double xMax = MaxVector.X, yMax = MaxVector.Y, zMax = MaxVector.Z,
                xMin = MinVector.X, yMin = MinVector.Y, zMin = MinVector.Z;
            if (extents.X > 0.0D)
            {
                xMax += extents.X;
            }
            else if (extents.X < 0.0D)
            {
                xMin += extents.X;
            }

            if (extents.Y > 0.0D)
            {
                yMax += extents.Y;
            }
            else if (extents.Y < 0.0D)
            {
                yMin += extents.Y;
            }

            if (extents.Z > 0.0D)
            {
                zMax += extents.Z;
            }
            else if (extents.Z < 0.0D)
            {
                zMin += extents.Z;
            }

            _max = new Vector(xMax, yMax, zMax);
            _min = new Vector(xMin, yMin, zMin);

            System.Diagnostics.Debug.Assert(MaxVector.X >= MinVector.X);
            System.Diagnostics.Debug.Assert(MaxVector.Y >= MinVector.Y);
            System.Diagnostics.Debug.Assert(MaxVector.Z >= MinVector.Z);

            FindVertices(Vertices, _max, _min);
        }

        internal override void Move(Vector v)
        {
            System.Diagnostics.Debug.Assert(MaxVector.X >= MinVector.X);
            System.Diagnostics.Debug.Assert(MaxVector.Y >= MinVector.Y);
            System.Diagnostics.Debug.Assert(MaxVector.Z >= MinVector.Z);

            _max = _max + v; _min = _min + v;

            FindVertices(Vertices, _max, _min);
        }

        internal override void ExtendAndMove(Vector extents, Vector v)
        {
            Extend(extents);
            Move(v);

            FindVertices(Vertices, _max, _min);
        }


        internal override double TestIntersection(Vector o, Vector d)
        {
            double tMin, tMax, tyMin, tyMax, tzMin, tzMax;

            // X-axis
            if (d.X != 0)
            {
                tMin = (MinVector.X - o.X) / d.X;
                tMax = (MaxVector.X - o.X) / d.X;
                if (tMin > tMax) (tMin, tMax) = (tMax, tMin); // Swap if needed
            }
            else
            {
                // Ray parallel to X-axis, check if origin is within X bounds
                if (o.X < MinVector.X || o.X > MaxVector.X)

                {
                    return -1;
                }
                tMin = double.NegativeInfinity;
                tMax = double.PositiveInfinity;
            }

            // Y-axis
            if (d.Y != 0)
            {
                tyMin = (MinVector.Y - o.Y) / d.Y;
                tyMax = (MaxVector.Y - o.Y) / d.Y;
                if (tyMin > tyMax) (tyMin, tyMax) = (tyMax, tyMin); // Swap if needed
            }
            else
            {
                // Ray parallel to Y-axis, check if origin is within Y bounds
                if (o.Y < MinVector.Y || o.Y > MaxVector.Y)
                {
                    return -1;
                }
                tyMin = double.NegativeInfinity;
                tyMax = double.PositiveInfinity;
            }

            // Merge X and Y slabs
            if ((tMin > tyMax) || (tyMin > tMax))
            {
                return -1;
            }
            tMin = System.Math.Max(tMin, tyMin);
            tMax = System.Math.Min(tMax, tyMax);

            // Z-axis
            if (d.Z != 0)
            {
                tzMin = (MinVector.Z - o.Z) / d.Z;
                tzMax = (MaxVector.Z - o.Z) / d.Z;
                if (tzMin > tzMax) (tzMin, tzMax) = (tzMax, tzMin); // Swap if needed
            }
            else
            {
                // Ray parallel to Z-axis, check if origin is within Z bounds
                if (o.Z < MinVector.Z || o.Z > MaxVector.Z)
                {
                    return -1;
                }
                tzMin = double.NegativeInfinity;
                tzMax = double.PositiveInfinity;
            }

            // Final merge
            if ((tMin > tzMax) || (tzMin > tMax))
            {
                return -1;
            }
            tMin = System.Math.Max(tMin, tzMin);
            tMax = System.Math.Min(tMax, tzMax);

            if (tMin <= 1 && tMax >= 0)  // Ensure the intersection is within the segment [o, o + d]
            {
                System.Diagnostics.Debug.Assert(tMin <= tMax);
                return tMin;
            }

            return -1;

        }

        internal override bool TestIntersection(BoundingVolume volume)
        {
            switch (volume)
            {
                default:
                    throw new System.NotImplementedException();
                case AxisAlignedBoundingBox aabb:
                    return IntersectionTests.TestFixedAndFixed(this, aabb);
                case OrientedBoundingBox obb:
                    return IntersectionTests.TestFixedAndFixed(this, obb);
                case CompoundBoundingVolume cbv:
                    return IntersectionTests.TestFixedAndFixed(this, cbv);
            }
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
            switch (volume)
            {
                default:
                    throw new System.NotImplementedException();
                case EmptyBoundingVolume:
                    return (-1, 0.0);
                case AxisAlignedBoundingBox aabb:
                    return Collisions.Resolve(this, aabb, v);
                case CompoundBoundingVolume cbv:
                    return Collisions.Resolve(this, cbv, v);
            }
        }

        public override string ToString()
        {
            return $"( Max: ({MaxVector.X}, {MaxVector.Y}, {MaxVector.Z}), Min: ({MinVector.X}, {MinVector.Y}, {MinVector.Z}) )";
        }


    }

}

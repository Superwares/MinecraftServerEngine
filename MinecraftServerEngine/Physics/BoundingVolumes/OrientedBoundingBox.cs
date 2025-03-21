﻿
namespace MinecraftServerEngine.Physics.BoundingVolumes
{
    public sealed class OrientedBoundingBox : BoundingVolume
    {
        private const int _VertexCount = 8;
        private readonly static double[] _Signs = [-1.0, 1.0];

        private Vector _center;
        public Vector Center => _center;

        private Vector _extents;
        public Vector Extents => _extents;

        private Angles _angles;
        public Angles Angles => _angles;

        public readonly Vector[] Axes = new Vector[Vector.Dimension];
        public readonly Vector[] Vertices = new Vector[_VertexCount];

        private static void FindVertices(Vector[] vertices, Vector[] axes, Vector extents, Vector c)
        {
            System.Diagnostics.Debug.Assert(vertices.Length == _VertexCount);
            System.Diagnostics.Debug.Assert(axes.Length == Vector.Dimension);

            System.Diagnostics.Debug.Assert(extents.X > 0.0);
            System.Diagnostics.Debug.Assert(extents.Y > 0.0);
            System.Diagnostics.Debug.Assert(extents.Z > 0.0);

            int i = 0;
            foreach (double dx in _Signs)
            {
                foreach (double dy in _Signs)
                {
                    foreach (double dz in _Signs)
                    {
                        vertices[i++] = c
                            + (dx * extents.X * axes[0])
                            + (dy * extents.Y * axes[1])
                            + (dz * extents.Z * axes[2]);
                    }
                }
            }
        }


        internal OrientedBoundingBox(Vector center, Vector extents, Angles angles)
        {
            _center = center;
            _extents = extents.Abs();
            _angles = angles;

            // Rotate the default axes
            Axes[0] = new Vector(1, 0, 0);
            Axes[1] = new Vector(0, 1, 0);
            Axes[2] = new Vector(0, 0, 1);

            Vector.Rotate(_angles, Axes);

            FindVertices(Vertices, Axes, _extents, _center);

        }


        public override AxisAlignedBoundingBox GetMinBoundingBox()
        {
            double maxX = double.NegativeInfinity;
            double maxY = double.NegativeInfinity;
            double maxZ = double.NegativeInfinity;

            double minX = double.PositiveInfinity;
            double minY = double.PositiveInfinity;
            double minZ = double.PositiveInfinity;


            foreach (Vector vertex in Vertices)
            {
                if (vertex.X > maxX) { maxX = vertex.X; }
                if (vertex.Y > maxY) { maxY = vertex.Y; }
                if (vertex.Z > maxZ) { maxZ = vertex.Z; }

                if (vertex.X < minX) { minX = vertex.X; }
                if (vertex.Y < minY) { minY = vertex.Y; }
                if (vertex.Z < minZ) { minZ = vertex.Z; }

            }

            return new AxisAlignedBoundingBox(
                new Vector(maxX, maxY, maxZ),
                new Vector(minX, minY, minZ)
                );
        }

        internal override Vector GetBottomCenter()
        {
            throw new System.NotImplementedException();
        }

        internal override Vector GetCenter()
        {
            return _center;
        }

        internal override double GetHeight()
        {
            throw new System.NotImplementedException();
        }

        internal void SetAngles(Angles angles)
        {
            _angles += angles;

            // Rotate the default axes
            Axes[0] = new Vector(1, 0, 0);
            Axes[1] = new Vector(0, 1, 0);
            Axes[2] = new Vector(0, 0, 1);

            Vector.Rotate(_angles, Axes);

            FindVertices(Vertices, Axes, _extents, _center);
        }

        internal override void Extend(Vector extents)
        {
            _extents += extents.Abs();

            FindVertices(Vertices, Axes, _extents, _center);
        }

        internal override void Move(Vector v)
        {
            _center += v;

            FindVertices(Vertices, Axes, _extents, _center);
        }

        internal void Rotate(Angles angles)
        {
            _angles += angles;

            // Rotate the default axes
            Axes[0] = new Vector(1, 0, 0);
            Axes[1] = new Vector(0, 1, 0);
            Axes[2] = new Vector(0, 0, 1);

            Vector.Rotate(_angles, Axes);

            FindVertices(Vertices, Axes, _extents, _center);
        }

        internal override void ExtendAndMove(Vector extents, Vector v)
        {
            _extents += extents.Abs();
            _center += v;

            FindVertices(Vertices, Axes, _extents, _center);
        }

        internal void ExtendAndRotate(Vector extents, Angles angles)
        {
            _extents += extents.Abs();
            _angles += angles;

            // Rotate the default axes
            Axes[0] = new Vector(1, 0, 0);
            Axes[1] = new Vector(0, 1, 0);
            Axes[2] = new Vector(0, 0, 1);

            Vector.Rotate(_angles, Axes);

            FindVertices(Vertices, Axes, _extents, _center);
        }

        internal void MoveAndRotate(Vector v, Angles angles)
        {
            _center += v;
            _angles += angles;

            // Rotate the default axes
            Axes[0] = new Vector(1, 0, 0);
            Axes[1] = new Vector(0, 1, 0);
            Axes[2] = new Vector(0, 0, 1);

            Vector.Rotate(_angles, Axes);

            FindVertices(Vertices, Axes, _extents, _center);
        }

        internal void ExtendAndMoveAndRotate(Vector extents, Vector v, Angles angles)
        {
            _extents += extents.Abs();
            _center += v;
            _angles += angles;

            // Rotate the default axes
            Axes[0] = new Vector(1, 0, 0);
            Axes[1] = new Vector(0, 1, 0);
            Axes[2] = new Vector(0, 0, 1);

            Vector.Rotate(_angles, Axes);

            FindVertices(Vertices, Axes, _extents, _center);
        }

        internal override double TestIntersection(Vector o, Vector d)
        {
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
                    throw new System.NotImplementedException();
            }
        }


    }
}

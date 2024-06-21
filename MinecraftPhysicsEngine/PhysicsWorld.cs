

using Containers;
using MinecraftPhysicsEngine;
using Sync;

namespace MinecraftPhysicsEngine
{
    public abstract class PhysicsWorld : System.IDisposable
    {

        private readonly struct Cell : System.IEquatable<Cell>
        {
            public const double WIDTH = 5.0D;

            public static Cell Generate(Vector p)
            {
                // TODO: Assertion
                int x = (int)(p.X / WIDTH),
                    z = (int)(p.Z / WIDTH);

                double r1 = p.X % WIDTH,
                       r3 = p.Z % WIDTH;
                if (r1 < 0.0D)
                {
                    --x;
                }
                if (r3 < 0.0D)
                {
                    --z;
                }

                return new(x, z);

            }

            public readonly int X, Z;

            public Cell(int x, int z)
            {
                X = x; Z = z;
            }

            public override string ToString()
            {
                return $"( X: {X}, Z: {Z} )";
            }

            public readonly bool Equals(Cell other)
            {
                return other.X == X && other.Z == Z;
            }

            public readonly override bool Equals(object obj)
            {
                return (obj is Cell other) && Equals(other);
            }

            public readonly override int GetHashCode()
            {
                return base.GetHashCode();
            }

        }

        private readonly struct Grid : System.IEquatable<Grid>
        {
            private static Grid Generate(AxisAlignedBoundingBox aabb)
            {
                Cell max = Cell.Generate(aabb.Max),
                     min = Cell.Generate(aabb.Min);

                return new(max, min);
            }

            public static Grid Generate(BoundingVolume volume)
            {
                switch (volume)
                {
                    default:
                        throw new System.NotImplementedException();
                    case AxisAlignedBoundingBox aabb:
                        return Generate(aabb);
                }

            }

            public static Grid Generate(Grid g1, Grid g2)
            {
                int temp;

                int xMax = System.Math.Min(g1._MAX.X, g2._MAX.X),
                    xMin = System.Math.Max(g1._MIN.X, g2._MIN.X);

                if (xMax < xMin)
                {
                    temp = xMax;
                    xMax = --xMin;
                    xMin = ++temp;
                }

                int zMin = System.Math.Max(g1._MIN.Z, g2._MIN.Z),
                    zMax = System.Math.Min(g1._MAX.Z, g2._MAX.Z);

                if (zMax < zMin)
                {
                    temp = zMax;
                    zMax = --zMin;
                    zMin = ++temp;
                }

                return new(new(xMax, zMax), new(xMin, zMin));
            }

            private readonly Cell _MAX, _MIN;
            public Cell Max => _MAX;
            public Cell Min => _MIN;

            public Grid(Cell max, Cell min)
            {
                System.Diagnostics.Debug.Assert(max.X >= min.X);
                System.Diagnostics.Debug.Assert(max.Z >= min.Z);

                _MAX = max; _MIN = min;
            }

            public bool Contains(Cell cell)
            {
                return 
                    cell.X <= _MAX.X && cell.X >= _MIN.X &&
                    cell.Z <= _MAX.Z && cell.Z >= _MIN.Z;
            }

            public System.Collections.Generic.IEnumerable<Cell> GetCells()
            {
                if (_MAX.X == _MIN.X && _MAX.Z == _MIN.Z)
                {
                    yield return new(_MAX.X, _MIN.Z);
                }
                else
                {
                    for (int z = _MIN.Z; z <= _MAX.Z; ++z)
                    {
                        for (int x = _MIN.X; x <= _MAX.X; ++x)
                        {
                            yield return new(x, z);
                        }
                    }
                }

            }

            public override string ToString()
            {
                return $"( Max: {_MAX}, Min: {_MIN} )";
            }

            public readonly bool Equals(Grid other)
            {
                return _MAX.Equals(other._MAX) && _MIN.Equals(other._MIN);
            }

            public readonly override bool Equals(object obj)
            {
                return (obj is Grid other) && Equals(other);
            }

            public readonly override int GetHashCode()
            {
                return base.GetHashCode();
            }

        }

        private bool _disposed = false;

        private readonly Table<Cell, Tree<PhysicsObject>> CellToObjects = new();  // Disposable
        private readonly Table<PhysicsObject, Grid> ObjectToGrid = new();  // Disposable

        public PhysicsWorld() { }

        ~PhysicsWorld() => System.Diagnostics.Debug.Assert(false);

        public Tree<PhysicsObject> GetPhysicsObjects(BoundingVolume volume)
        {
            throw new System.NotImplementedException();
        }

        private void InsertObjectToCell(Cell cell, PhysicsObject obj)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Tree<PhysicsObject> objs;
            if (!CellToObjects.Contains(cell))
            {
                objs = new();
                CellToObjects.Insert(cell, objs);
            }
            else
            {
                objs = CellToObjects.Lookup(cell);
            }

            objs.Insert(obj);
        }

        private void ExtractObjectToCell(Cell cell, PhysicsObject obj)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(CellToObjects.Contains(cell));
            Tree<PhysicsObject> objs = CellToObjects.Lookup(cell);

            objs.Extract(obj);

            if (objs.Empty)
            {
                CellToObjects.Extract(cell);
                objs.Dispose();
            }
        }

        private void InitObjectMapping(PhysicsObject obj)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Grid grid = Grid.Generate(obj.BoundingVolume);
            foreach (Cell cell in grid.GetCells())
            {
                InsertObjectToCell(cell, obj);
            }

            System.Diagnostics.Debug.Assert(!ObjectToGrid.Contains(obj));
            ObjectToGrid.Insert(obj, grid);
        }

        private void CloseObjectMapping(PhysicsObject obj)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(ObjectToGrid.Contains(obj));
            Grid grid = ObjectToGrid.Extract(obj);

            foreach (Cell cell in grid.GetCells())
            {
                ExtractObjectToCell(cell, obj);
            }
        }

        private void UpdateObjectMapping(PhysicsObject obj)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(ObjectToGrid.Contains(obj));
            Grid gridPrev = ObjectToGrid.Extract(obj);
            Grid grid = Grid.Generate(obj.BoundingVolume);

            if (!gridPrev.Equals(grid))
            {
                Grid gridBetween = Grid.Generate(grid, gridPrev);

                foreach (Cell cell in gridPrev.GetCells())
                {
                    if (gridBetween.Contains(cell))
                    {
                        continue;
                    }

                    ExtractObjectToCell(cell, obj);

                }

                foreach (Cell cell in grid.GetCells())
                {
                    if (gridBetween.Contains(cell))
                    {
                        continue;
                    }

                    InsertObjectToCell(cell, obj);
                }
            }

            ObjectToGrid.Insert(obj, grid);
        }

        public void InitObject(PhysicsObject obj)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            InitObjectMapping(obj);
        }

        public void CloseObject(PhysicsObject obj)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            CloseObjectMapping(obj);
        }

        public void MoveObject(Terrain terrain, PhysicsObject obj)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            (BoundingVolume volume, Vector v) = obj.Integrate();

            AxisAlignedBoundingBox minBoundingBox = volume.GetMinBoundingBox();
            minBoundingBox.Extend(v);

            (v, bool onGround) = terrain.ResolveCollisions(volume, v);

            obj.Move(volume, v, onGround);

            UpdateObjectMapping(obj);
        }

        public virtual void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }
}

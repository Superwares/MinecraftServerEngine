

using Containers;
using MinecraftPhysicsEngine;
using Sync;

namespace MinecraftPhysicsEngine
{
    public abstract class PhysicsWorld : System.IDisposable
    {

        private readonly struct Cell : System.IEquatable<Cell>
        {
            public const double Width = 5.0D;

            public static Cell Generate(Vector p)
            {
                // TODO: Assertion
                int x = (int)(p.X / Width),
                    z = (int)(p.Z / Width);

                double r1 = p.X % Width,
                       r3 = p.Z % Width;
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

        private class Grid : System.IEquatable<Grid>
        {
            private static Grid Generate(AxisAlignedBoundingBox aabb)
            {
                System.Diagnostics.Debug.Assert(aabb != null);

                Cell max = Cell.Generate(aabb.Max),
                     min = Cell.Generate(aabb.Min);

                return new(max, min);
            }

            public static Grid Generate(BoundingVolume volume)
            {
                System.Diagnostics.Debug.Assert(volume != null);

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
                if (g1 == null)
                {
                    return null;
                }    
                if (g2 == null)
                {
                    return null;
                }

                int xMax = System.Math.Min(g1._MAX.X, g2._MAX.X),
                    xMin = System.Math.Max(g1._MIN.X, g2._MIN.X);

                if (xMax < xMin)
                {
                    return null;
                }

                int zMax = System.Math.Min(g1._MAX.Z, g2._MAX.Z),
                    zMin = System.Math.Max(g1._MIN.Z, g2._MIN.Z);

                if (zMax < zMin)
                {
                    return null;
                }

                return new Grid(new Cell(xMax, zMax), new Cell(xMin, zMin));
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

            public int GetCount()
            {
                System.Diagnostics.Debug.Assert(Max.X >= Min.X);
                System.Diagnostics.Debug.Assert(Max.Z >= Min.Z);

                int l1 = (Max.X - Min.X) + 1,
                    l3 = (Max.Z - Min.Z) + 1;
                return l1 * l3;
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

            public bool Equals(Grid other)
            {
                if (other == null)
                {
                    return false;
                }

                return _MAX.Equals(other._MAX) && _MIN.Equals(other._MIN);
            }

            public override bool Equals(object obj)
            {
                return (obj is Grid other) && Equals(other);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

        }

        private bool _disposed = false;

        private readonly Locker Locker = new();  // Disposable
        private readonly Table<Cell, Tree<PhysicsObject>> CellToObjects = new();  // Disposable

        private readonly ConcurrentTable<PhysicsObject, Grid> ObjectToGrid = new();  // Disposable

        public PhysicsWorld() { }

        ~PhysicsWorld() => System.Diagnostics.Debug.Assert(false);

        public void GetObjects(
            Queue<PhysicsObject> objects, AxisAlignedBoundingBox minBoundingBox)
        {
            System.Diagnostics.Debug.Assert(objects != null);
            System.Diagnostics.Debug.Assert(minBoundingBox != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            Grid grid = Grid.Generate(minBoundingBox);

            foreach (Cell cell in grid.GetCells())
            {
                if (!CellToObjects.Contains(cell))
                {
                    continue;
                }

                Tree<PhysicsObject> objectsInCell = CellToObjects.Lookup(cell);
                foreach (PhysicsObject objInCell in objectsInCell.GetKeys())
                {
                    objects.Enqueue(objInCell);
                }
            }

        }

        private void InsertObjectToCell(Cell cell, PhysicsObject obj)
        {
            System.Diagnostics.Debug.Assert(obj != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            Locker.Hold();

            Tree<PhysicsObject> objs;
            if (!CellToObjects.Contains(cell))
            {
                objs = new Tree<PhysicsObject>();
                CellToObjects.Insert(cell, objs);
            }
            else
            {
                objs = CellToObjects.Lookup(cell);
            }

            objs.Insert(obj);

            Locker.Release();
        }

        private void ExtractObjectToCell(Cell cell, PhysicsObject obj)
        {
            System.Diagnostics.Debug.Assert(obj != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            Locker.Hold();

            System.Diagnostics.Debug.Assert(CellToObjects.Contains(cell));
            Tree<PhysicsObject> objs = CellToObjects.Lookup(cell);

            objs.Extract(obj);

            if (objs.Empty)
            {
                CellToObjects.Extract(cell);
                objs.Dispose();
            }

            Locker.Release();
        }

        public void InitObjectMapping(PhysicsObject obj)
        {
            System.Diagnostics.Debug.Assert(obj != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            Grid grid = Grid.Generate(obj.BoundingVolume);
            foreach (Cell cell in grid.GetCells())
            {
                InsertObjectToCell(cell, obj);
            }

            System.Diagnostics.Debug.Assert(!ObjectToGrid.Contains(obj));
            ObjectToGrid.Insert(obj, grid);
        }

        public void CloseObjectMapping(PhysicsObject obj)
        {
            System.Diagnostics.Debug.Assert(obj != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(ObjectToGrid.Contains(obj));
            Grid grid = ObjectToGrid.Extract(obj);

            foreach (Cell cell in grid.GetCells())
            {
                ExtractObjectToCell(cell, obj);
            }
        }

        public void UpdateObjectMapping(PhysicsObject obj)
        {
            System.Diagnostics.Debug.Assert(obj != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(ObjectToGrid.Contains(obj));
            Grid gridPrev = ObjectToGrid.Extract(obj);
            Grid grid = Grid.Generate(obj.BoundingVolume);

            System.Diagnostics.Debug.Assert(gridPrev != null);
            System.Diagnostics.Debug.Assert(grid != null);

            if (!gridPrev.Equals(grid))
            {
                Grid gridBetween = Grid.Generate(grid, gridPrev);

                foreach (Cell cell in gridPrev.GetCells())
                {
                    if (gridBetween != null && gridBetween.Contains(cell))
                    {
                        continue;
                    }

                    ExtractObjectToCell(cell, obj);

                }

                foreach (Cell cell in grid.GetCells())
                {
                    if (gridBetween != null && gridBetween.Contains(cell))
                    {
                        continue;
                    }

                    InsertObjectToCell(cell, obj);
                }
            }

            ObjectToGrid.Insert(obj, grid);
        }

        public (BoundingVolume, Vector, bool) IntegrateObject(
            Terrain terrain, PhysicsObject obj)
        {
            System.Diagnostics.Debug.Assert(terrain != null);
            System.Diagnostics.Debug.Assert(obj != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            (BoundingVolume volumeObject, Vector v) = obj.Integrate();

            AxisAlignedBoundingBox volumeTotal = volumeObject.GetMinBoundingBox();
            volumeTotal.Extend(v);

            (v, bool onGround) = terrain.ResolveCollisions(volumeTotal, volumeObject, v);

            return (volumeObject, v, onGround);
        }

        public virtual void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.
            Locker.Dispose();

            CellToObjects.Dispose();
            ObjectToGrid.Dispose();

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }
}

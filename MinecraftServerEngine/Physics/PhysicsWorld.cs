

using Common;
using Containers;
using Sync;


namespace MinecraftServerEngine.Physics
{
    using BoundingVolumes;

    public abstract class PhysicsWorld : System.IDisposable
    {

        private readonly struct Cell : System.IEquatable<Cell>
        {
            // It must be the same as the chunk width because the client uses the chunk width as a unit when rendering entities based on the render distance.
            // TODO: Integrate with chunk width constant
            public const double Width = 16.0D;

            public static Cell GenerateForMax(Vector p)
            {
                // TODO: Assertion
                int x = (int)(p.X / Width),
                    z = (int)(p.Z / Width);

                double r1 = p.X % Width,
                       r3 = p.Z % Width;
                if (r1 < 0.0 || (p.X >= 0.0 && Math.AreDoublesEqual(r1, 0.0)))
                {
                    --x;
                }
                if (r3 < 0.0 || (p.Z >= 0.0 && Math.AreDoublesEqual(r3, 0.0)))
                {
                    --z;
                }

                return new(x, z);

            }

            public static Cell GenerateForMin(Vector p)
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
                return $"[{X},{Z}]";
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

                Cell max = Cell.GenerateForMax(aabb.MaxVector),
                     min = Cell.GenerateForMin(aabb.MinVector);

                return new(max, min);
            }

            private static Grid Generate(OrientedBoundingBox obb)
            {
                System.Diagnostics.Debug.Assert(obb != null);

                AxisAlignedBoundingBox minAABB = obb.GetMinBoundingBox();

                Cell max = Cell.GenerateForMax(minAABB.MaxVector),
                     min = Cell.GenerateForMin(minAABB.MinVector);

                return new(max, min);
            }

            public static Grid Generate(BoundingVolume volume)
            {
                System.Diagnostics.Debug.Assert(volume != null);

                switch (volume)
                {
                    default:
                        throw new System.NotImplementedException();
                    case EmptyBoundingVolume:
                        return null;
                    case AxisAlignedBoundingBox aabb:
                        return Generate(aabb);
                    case OrientedBoundingBox obb:
                        return Generate(obb);
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

        private readonly ReadLocker ReadLocker = new();  // Disposable
        private readonly Table<Cell, ConcurrentTree<PhysicsObject>> _CellToObjects = new();  // Disposable
        private readonly ConcurrentTable<PhysicsObject, Grid> _ObjectToGrid = new();  // Disposable

        public PhysicsWorld() { }

        ~PhysicsWorld()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        protected virtual void StartRoutine()
        {
        }

        protected internal virtual void _StartRoutine()
        {
            StartRoutine();
        }


        public void SearchObjects(
            Tree<PhysicsObject> objs, BoundingVolume bv,
            bool strict, PhysicsObject exceptObj = null)
        {
            System.Diagnostics.Debug.Assert(bv != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            Grid grid = Grid.Generate(bv);

            foreach (Cell cell in grid.GetCells())
            {
                if (!_CellToObjects.Contains(cell))
                {
                    continue;
                }

                Tree<PhysicsObject> objectsInCell = _CellToObjects.Lookup(cell);
                System.Diagnostics.Debug.Assert(objectsInCell != null);
                foreach (PhysicsObject obj in objectsInCell.GetKeys())
                {
                    if (ReferenceEquals(obj, exceptObj) == true)
                    {
                        continue;
                    }

                    System.Diagnostics.Debug.Assert(obj != null);
                    if (objs.Contains(obj))
                    {
                        continue;
                    }

                    if (strict == true)
                    {
                        if (obj.BoundingVolume.TestIntersection(bv) == true)
                        {
                            objs.Insert(obj);
                        }
                    }
                    else
                    {
                        objs.Insert(obj);
                    }
                }
            }
        }

        public void SearchObjects(Tree<PhysicsObject> objs, AxisAlignedBoundingBox minBoundingBox)
        {
            System.Diagnostics.Debug.Assert(minBoundingBox != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            SearchObjects(objs, minBoundingBox, false);
        }

        public void SearchObjects(Tree<PhysicsObject> objs, Vector o, Vector d)
        {
            System.Diagnostics.Debug.Assert(objs != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            AxisAlignedBoundingBox aabb = AxisAlignedBoundingBox.Generate(o, d);

            double t;

            Grid grid = Grid.Generate(aabb);

            foreach (Cell cell in grid.GetCells())
            {
                if (_CellToObjects.Contains(cell) == false)
                {
                    continue;
                }

                Tree<PhysicsObject> objectsInCell = _CellToObjects.Lookup(cell);
                System.Diagnostics.Debug.Assert(objectsInCell != null);
                foreach (PhysicsObject obj in objectsInCell.GetKeys())
                {
                    System.Diagnostics.Debug.Assert(obj != null);
                    if (objs.Contains(obj) == true)
                    {
                        continue;
                    }

                    //objs.Insert(objInCell);

                    //AxisAlignedBoundingBox minAABB = objInCell.BoundingVolume.GetMinBoundingBox();

                    //minAABB.Move(-1 * origin);

                    t = obj.BoundingVolume.TestIntersection(o, d);
                    if (t >= 0.0)
                    {
                        objs.Insert(obj);
                    }

                    //if (obj.BoundingVolume.Intersects(o, d) == true)
                    //{
                    //    objs.Insert(obj);
                    //}

                    //if (minAABB.Intersects(o, d) == true)
                    //{
                    //    objs.Insert(objInCell);
                    //}

                }
            }
        }

        public PhysicsObject SearchClosestObject(Vector o, Vector d, PhysicsObject exceptObj = null)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            AxisAlignedBoundingBox aabb = AxisAlignedBoundingBox.Generate(o, d);

            Grid grid = Grid.Generate(aabb);

            double t_min = double.PositiveInfinity;
            double t;

            PhysicsObject obj_min = null;

            foreach (Cell cell in grid.GetCells())
            {
                if (_CellToObjects.Contains(cell) == false)
                {
                    continue;
                }

                Tree<PhysicsObject> objectsInCell = _CellToObjects.Lookup(cell);
                System.Diagnostics.Debug.Assert(objectsInCell != null);
                foreach (PhysicsObject obj in objectsInCell.GetKeys())
                {
                    System.Diagnostics.Debug.Assert(obj != null);
                    if (ReferenceEquals(obj, exceptObj) == true)
                    {
                        continue;
                    }

                    //objs.Insert(objInCell);

                    //AxisAlignedBoundingBox minAABB = objInCell.BoundingVolume.GetMinBoundingBox();

                    //minAABB.Move(-1 * origin);

                    t = obj.BoundingVolume.TestIntersection(o, d);
                    if (t >= 0.0 && t < t_min)
                    {
                        t_min = t;
                        obj_min = obj;
                    }

                    //if (
                    //    minAABB.Intersects(o, d) == true
                    //    && a > Vector.GetLengthSquared(objInCell.Position, o))
                    //{
                    //    objFound = objInCell;
                    //}

                }
            }

            return obj_min;
        }

        private void InsertObjectToCell(Cell cell, PhysicsObject obj)
        {
            System.Diagnostics.Debug.Assert(obj != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            ReadLocker.Hold();

            ConcurrentTree<PhysicsObject> objs;
            if (!_CellToObjects.Contains(cell))
            {
                objs = new ConcurrentTree<PhysicsObject>();
                _CellToObjects.Insert(cell, objs);
            }
            else
            {
                objs = _CellToObjects.Lookup(cell);
            }

            ReadLocker.Release();

            System.Diagnostics.Debug.Assert(objs != null);
            objs.Insert(obj);
        }

        private void ExtractObjectToCell(Cell cell, PhysicsObject obj)
        {
            System.Diagnostics.Debug.Assert(obj != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_CellToObjects.Contains(cell));
            Tree<PhysicsObject> objs = _CellToObjects.Lookup(cell);

            System.Diagnostics.Debug.Assert(objs != null);
            objs.Extract(obj);

            /*if (objs.Empty)
            {
                CellToObjects.Extract(cell);
                objs.Dispose();
            }*/
        }

        private protected void InitObjectMapping(PhysicsObject obj)
        {
            System.Diagnostics.Debug.Assert(obj != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            Grid grid = Grid.Generate(obj.BoundingVolume);
            if (grid != null)
            {
                foreach (Cell cell in grid.GetCells())
                {
                    InsertObjectToCell(cell, obj);
                }
            }

            System.Diagnostics.Debug.Assert(!_ObjectToGrid.Contains(obj));
            _ObjectToGrid.Insert(obj, grid);
        }

        private protected void CloseObjectMapping(PhysicsObject obj)
        {
            System.Diagnostics.Debug.Assert(obj != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_ObjectToGrid.Contains(obj));
            Grid grid = _ObjectToGrid.Extract(obj);

            if (grid != null)
            {
                foreach (Cell cell in grid.GetCells())
                {
                    ExtractObjectToCell(cell, obj);
                }
            }
        }

        private protected void UpdateObjectMapping(PhysicsObject obj)
        {
            System.Diagnostics.Debug.Assert(obj != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            BoundingVolume volume = obj.BoundingVolume;

            System.Diagnostics.Debug.Assert(_ObjectToGrid.Contains(obj));
            Grid gridPrev = _ObjectToGrid.Extract(obj);
            Grid grid = Grid.Generate(volume);

            if (grid != null)
            {
                System.Diagnostics.Debug.Assert(grid != null);

                if (gridPrev != null)
                {
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
                }
                else
                {
                    foreach (Cell cell in grid.GetCells())
                    {
                        InsertObjectToCell(cell, obj);
                    }
                }

            }
            else
            {
                if (gridPrev != null)
                {
                    foreach (Cell cell in gridPrev.GetCells())
                    {
                        ExtractObjectToCell(cell, obj);
                    }
                }
            }

            _ObjectToGrid.Insert(obj, grid);
        }

        private protected (BoundingVolume, Vector) IntegrateObject(
            Time dt, Terrain terrain, PhysicsObject obj)
        {
            System.Diagnostics.Debug.Assert(terrain != null);
            System.Diagnostics.Debug.Assert(obj != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            return obj.Integrate(dt, terrain);
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed == false)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing == true)
                {
                    // Dispose managed resources.
                    ReadLocker.Dispose();

                    _CellToObjects.Dispose();
                    _ObjectToGrid.Dispose();
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
        }

    }
}

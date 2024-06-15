

using Containers;
using Sync;

namespace PhysicsEngine
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

            public bool Equals(Cell other)
            {
                return (other.X == X && other.Z == Z);
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

            public Grid(Cell max, Cell min)
            {
                System.Diagnostics.Debug.Assert(max.X >= min.X);
                System.Diagnostics.Debug.Assert(max.Y >= min.Y);
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

            public bool Equals(Grid other)
            {
                return (_MAX.Equals(other._MAX) && _MIN.Equals(other._MIN));
            }

        }


        private bool _disposed = false;

        private readonly Table<Cell, Tree<PhysicsObject>> _CELL_TO_OBJECTS = new();  // Disposable
        private readonly Table<PhysicsObject, Grid> _OBJECT_TO_GRID = new();  // Disposable
        
        public abstract BoundingVolume[] GetTerrainBoundingVolumes(Vector max, Vector min);

        public BoundingVolume[] GetTerrainBoundingVolumes(BoundingVolume volume)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            switch (volume)
            {
                default:
                    throw new System.NotImplementedException();
                case AxisAlignedBoundingBox aabb:
                    return GetTerrainBoundingVolumes(aabb.Max, aabb.Min);
            }
            
        }

        public BoundingVolume[] GetTerrainBoundingVolumes(BoundingVolume volume, Vector v)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            BoundingVolume volumeTotal = volume.GetMinBoundingVolume(v);

            return GetTerrainBoundingVolumes(volumeTotal);
        }

        public Tree<PhysicsObject> GetPhysicsObjects(BoundingVolume volume)
        {
            throw new System.NotImplementedException();
        }

        private void InsertObjectToCell(Cell cell, PhysicsObject obj)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Tree<PhysicsObject> objs;
            if (!_CELL_TO_OBJECTS.Contains(cell))
            {
                objs = new();
                _CELL_TO_OBJECTS.Insert(cell, objs);
            }
            else
            {
                objs = _CELL_TO_OBJECTS.Lookup(cell);
            }

            objs.Insert(obj);
        }

        private void ExtractObjectToCell(Cell cell, PhysicsObject obj)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_CELL_TO_OBJECTS.Contains(cell));
            Tree<PhysicsObject> objs = _CELL_TO_OBJECTS.Lookup(cell);

            objs.Extract(obj);

            if (objs.Empty)
            {
                _CELL_TO_OBJECTS.Extract(cell);
                objs.Dispose();
            }
        }

        public void InitObjectMapping(PhysicsObject obj)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Grid grid = Grid.Generate(obj.BOUNDING_VOLUME);
            foreach (Cell cell in grid.GetCells())
            {
                InsertObjectToCell(cell, obj);
            }

            System.Diagnostics.Debug.Assert(!_OBJECT_TO_GRID.Contains(obj));
            _OBJECT_TO_GRID.Insert(obj, grid);
        }

        public void CloseObjectMapping(PhysicsObject obj)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_OBJECT_TO_GRID.Contains(obj));
            Grid grid = _OBJECT_TO_GRID.Extract(obj);

            foreach (Cell cell in grid.GetCells())
            {
                ExtractObjectToCell(cell, obj);
            }
        }

        private void UpdateObjectMapping(PhysicsObject obj)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_OBJECT_TO_GRID.Contains(obj));
            Grid gridPrev = _OBJECT_TO_GRID.Extract(obj);
            Grid grid = Grid.Generate(obj.BOUNDING_VOLUME);

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

            _OBJECT_TO_GRID.Insert(obj, grid);
        }

        public void MoveObject(PhysicsObject obj)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            (BoundingVolume volumeMoving, Vector v) = obj.Integrate();

            BoundingVolume[] fixedVolumes = GetTerrainBoundingVolumes(volumeMoving, v);

            int i;

            Vector vPrime1 = v;
            {
                for (i = 0; i < fixedVolumes.Length; ++i)
                {
                    BoundingVolume volumeFixed = fixedVolumes[i];
                    vPrime1 = volumeFixed.AdjustMovingVolumeSideToSide(volumeMoving, vPrime1);
                }

                for (i = 0; i < fixedVolumes.Length; ++i)
                {
                    BoundingVolume volumeFixed = fixedVolumes[i];
                    vPrime1 = volumeFixed.AdjustMovingVolumeUpAndDown(volumeMoving, vPrime1);
                }
            }

            Vector vPrime2 = v;
            {
                for (i = 0; i < fixedVolumes.Length; ++i)
                {
                    BoundingVolume volumeFixed = fixedVolumes[i];
                    vPrime2 = volumeFixed.AdjustMovingVolumeUpAndDown(volumeMoving, vPrime2);
                }

                for (i = 0; i < fixedVolumes.Length; ++i)
                {
                    BoundingVolume volumeFixed = fixedVolumes[i];
                    vPrime2 = volumeFixed.AdjustMovingVolumeSideToSide(volumeMoving, vPrime2);
                }
            }

            Vector vPrime3 = new(vPrime1.X, 0.0D, vPrime1.Z), 
                   vPrime4 = new(vPrime2.X, 0.0D, vPrime2.Z);

            double lenSquared3 = vPrime3.GetLengthSquared(), lenSquared4 = vPrime4.GetLengthSquared();
            System.Diagnostics.Debug.Assert(lenSquared3 <= lenSquared4);
            Vector vPrime5 = (lenSquared3 == lenSquared4) ? vPrime1 : vPrime2;

            bool movedUpAndDown = vPrime5.Y != v.Y;
            bool onGround = v.Y < 0.0D && movedUpAndDown;

            volumeMoving = volumeMoving.Move(vPrime5);

            v = new(
                vPrime5.X != v.X ? 0.0D : vPrime5.X,
                movedUpAndDown ? 0.0D : vPrime5.Y,
                vPrime5.Z != v.Z ? 0.0D : vPrime5.Z);

            obj.Move(volumeMoving, v, onGround);

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

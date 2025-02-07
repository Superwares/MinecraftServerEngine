
using Common;
using Containers;

using MinecraftServerEngine.Renderers;
using MinecraftServerEngine.Physics;
using MinecraftServerEngine.Physics.BoundingVolumes;

namespace MinecraftServerEngine.ShapeObjects
{
    //internal abstract class ShapeObject : PhysicsObject
    //{
    //    private bool _disposed = false;


    //    private Vector _center;
    //    public Vector Center
    //    {
    //        get
    //        {
    //            return _center;
    //        }
    //    }


    //    private bool _hasMovement = false;

    //    internal readonly ConcurrentTree<EntityRenderer> Renderers = new();  // Disposable

    //    private protected ShapeObject(
    //        double mass,
    //        BoundingVolume bv)
    //        : base(mass, bv, new WallPharsing())
    //    {

    //        _center = bv.GetCenter();
    //    }


    //    internal abstract double _GetArea();

    //    public double GetArea()
    //    {
    //        if (_disposed == true)
    //        {
    //            throw new System.ObjectDisposedException(GetType().Name);
    //        }

    //        double area = _GetArea();

    //        System.Diagnostics.Debug.Assert(area >= 0.0);
    //        return area;
    //    }
    //}
}

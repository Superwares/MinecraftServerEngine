
using MinecraftServerEngine.Physics;
using MinecraftServerEngine.Physics.BoundingVolumes;

namespace MinecraftServerEngine.ShapeObjects
{
    public abstract class CylinderObject : ShapeObject
    {


        public CylinderObject(Vector center, Vector extents, Angles angles) 
            : base(new OrientedBoundingBox(center, extents, angles))
        {
        }

        internal override double _GetArea()
        {
            throw new System.NotImplementedException();
        }


    }
}

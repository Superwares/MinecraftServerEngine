

namespace Protocol
{
    internal static class BoundingShapeGenerator
    {
        public static BoundingShape Make()
        {
            return new BoundingShape();
        }

        public static BoundingShape Make(BlockLocation loc)
        {
            Vector min = loc.Convert(),
                   max = new(min.X + 1.0D, min.Y + 1.0D, min.Z + 1.0D);
            BoundingBox bb = new(max, min);
            return new BoundingShape(bb);
        }

        /**
         * b=0: straight
         * b=1: outer left
         * b=2: outer right
         * b=3: inner left
         * b=4: inner right
         */
        public static BoundingShape Make(BlockLocation loc, Directions d, bool bottom, int b)
        {
            throw new System.NotImplementedException();
        }

    }
}



namespace Protocol
{
    internal static class DirectionExtensions
    {
        public static Directions GetCCW(this Directions d)
        {
            switch (d)
            {
                default:
                    throw new System.NotImplementedException();
                case Directions.NORTH:
                    return Directions.WEST;
                case Directions.EAST:
                    return Directions.NORTH;
                case Directions.SOUTH:
                    return Directions.EAST;
                case Directions.WEST:
                    return Directions.SOUTH;
            }
        }

        public static Directions GetCW(this Directions d)
        {
            switch (d)
            {
                default:
                    throw new System.NotImplementedException();
                case Directions.NORTH:
                    return Directions.EAST;
                case Directions.EAST:
                    return Directions.SOUTH;
                case Directions.SOUTH:
                    return Directions.WEST;
                case Directions.WEST:
                    return Directions.NORTH;
            }
        }

        public static Directions GetOpposite(this Directions d)
        {
            switch (d)
            {
                default:
                    throw new System.NotImplementedException();
                case Directions.UP:
                    return Directions.DOWN;
                case Directions.DOWN:
                    return Directions.UP;
                case Directions.NORTH:
                    return Directions.SOUTH;
                case Directions.EAST:
                    return Directions.WEST;
                case Directions.SOUTH:
                    return Directions.NORTH;
                case Directions.WEST:
                    return Directions.EAST;
            }
        }

    }
}

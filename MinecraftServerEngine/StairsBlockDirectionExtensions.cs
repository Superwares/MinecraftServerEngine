using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftServerEngine
{
    internal static class StairsBlockDirectionExtensions
    {
        public static StairsBlockDirection RotateCCW(this StairsBlockDirection d)
        {
            switch (d)
            {
                default:
                    throw new System.NotImplementedException($"Unknown stairs block direction");
                case StairsBlockDirection.NORTH:
                    return StairsBlockDirection.WEST;
                case StairsBlockDirection.EAST:
                    return StairsBlockDirection.NORTH;
                case StairsBlockDirection.SOUTH:
                    return StairsBlockDirection.EAST;
                case StairsBlockDirection.WEST:
                    return StairsBlockDirection.SOUTH;
            }
        }

        public static StairsBlockDirection RotateCW(this StairsBlockDirection d)
        {
            switch (d)
            {
                default:
                    throw new System.NotImplementedException();
                case StairsBlockDirection.NORTH:
                    return StairsBlockDirection.EAST;
                case StairsBlockDirection.EAST:
                    return StairsBlockDirection.SOUTH;
                case StairsBlockDirection.SOUTH:
                    return StairsBlockDirection.WEST;
                case StairsBlockDirection.WEST:
                    return StairsBlockDirection.NORTH;
            }
        }

        public static StairsBlockDirection GetOpposite(this StairsBlockDirection d)
        {
            switch (d)
            {
                default:
                    throw new System.NotImplementedException();
                case StairsBlockDirection.UP:
                    return StairsBlockDirection.DOWN;
                case StairsBlockDirection.DOWN:
                    return StairsBlockDirection.UP;
                case StairsBlockDirection.NORTH:
                    return StairsBlockDirection.SOUTH;
                case StairsBlockDirection.EAST:
                    return StairsBlockDirection.WEST;
                case StairsBlockDirection.SOUTH:
                    return StairsBlockDirection.NORTH;
                case StairsBlockDirection.WEST:
                    return StairsBlockDirection.EAST;
            }
        }
    }
}

namespace MinecraftServerEngine.Blocks
{
    internal static class StairsBlockDirectionExtensions
    {
        public static BlockDirection RotateCCW(this BlockDirection d)
        {
            switch (d)
            {
                default:
                    throw new System.NotImplementedException($"Unknown stairs block direction");
                case BlockDirection.Front:
                    return BlockDirection.Left;
                case BlockDirection.Right:
                    return BlockDirection.Front;
                case BlockDirection.Back:
                    return BlockDirection.Right;
                case BlockDirection.Left:
                    return BlockDirection.Back;
            }
        }

        public static BlockDirection RotateCW(this BlockDirection d)
        {
            switch (d)
            {
                default:
                    throw new System.NotImplementedException();
                case BlockDirection.Front:
                    return BlockDirection.Right;
                case BlockDirection.Right:
                    return BlockDirection.Back;
                case BlockDirection.Back:
                    return BlockDirection.Left;
                case BlockDirection.Left:
                    return BlockDirection.Front;
            }
        }

        public static BlockDirection GetOpposite(this BlockDirection d)
        {
            switch (d)
            {
                default:
                    throw new System.NotImplementedException();
                case BlockDirection.UP:
                    return BlockDirection.DOWN;
                case BlockDirection.DOWN:
                    return BlockDirection.UP;
                case BlockDirection.Front:
                    return BlockDirection.Back;
                case BlockDirection.Right:
                    return BlockDirection.Left;
                case BlockDirection.Back:
                    return BlockDirection.Front;
                case BlockDirection.Left:
                    return BlockDirection.Right;
            }
        }
    }
}

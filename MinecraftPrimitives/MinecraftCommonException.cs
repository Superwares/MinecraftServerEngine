

using Common;

namespace MinecraftPrimitives
{
    public abstract class MinecraftCommonException : CommonException
    {
        public MinecraftCommonException(string msg) : base(msg)
        {
        }
    }
}

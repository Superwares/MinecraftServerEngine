

using Common;

namespace MinecraftServerEngine
{
    public abstract class MinecraftCommonException : CommonException
    {
        public MinecraftCommonException(string msg) : base(msg)
        {
        }
    }
}

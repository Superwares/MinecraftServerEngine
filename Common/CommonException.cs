
namespace Common
{
    public abstract class CommonException : System.Exception
    {
        public CommonException(string msg) : base(msg) { }
    }
}

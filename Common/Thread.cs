
namespace Common
{
    public sealed class Thread
    {
        public static Thread GetCurrent()
        {
            return new(System.Threading.Thread.CurrentThread);
        }

        public static Thread New(VoidMethod startRoutine)
        {
            System.Threading.Thread t = new(new System.Threading.ThreadStart(startRoutine));
            t.Start();

            return new(t);
        }

        private readonly System.Threading.Thread SystemThread = null;

        private Thread(System.Threading.Thread t) 
        {
            SystemThread = t;
        }

        public void Join()
        {
            SystemThread.Join();
        }

    }
}

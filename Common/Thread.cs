
namespace Common
{
    public sealed class Thread
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern System.IntPtr GetCurrentThread();

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern System.IntPtr SetThreadAffinityMask(
            System.IntPtr hThread, System.IntPtr dwThreadAffinityMask);

        private readonly static int ProcessorCount = System.Environment.ProcessorCount;
        private static int _index = 0;

        public static void Bind()
        {
            System.Diagnostics.Debug.Assert(_index >= 0);
            System.Diagnostics.Debug.Assert(_index <= ProcessorCount);

            int i = _index % ProcessorCount;
            _index = i + 1;

            System.IntPtr threadHandle = GetCurrentThread();
            SetThreadAffinityMask(threadHandle, (System.IntPtr)0x01 << i);
        }

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
            System.Diagnostics.Debug.Assert(t != null);

            t.Priority = System.Threading.ThreadPriority.Highest;
            SystemThread = t;
        }

        public void Join()
        {
            System.Diagnostics.Debug.Assert(SystemThread != null);

            SystemThread.Join();
        }

    }
}

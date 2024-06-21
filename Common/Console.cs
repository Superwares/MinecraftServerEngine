

namespace Common
{

    public static class Console
    {
        private static VoidMethod startCancelRoutine = null;

        private static System.ConsoleCancelEventHandler OnCancelKeyPress()
        {
            return new System.ConsoleCancelEventHandler((_, _) =>
            {
                System.Diagnostics.Debug.Assert(startCancelRoutine != null);
                startCancelRoutine();
            });
        }

        static Console()
        {
            System.Console.CancelKeyPress += OnCancelKeyPress();

        }

        public static void Print(string msg)
        {
            System.Console.Write(msg);
        }

        public static void Printl(string msg)
        {
            System.Console.WriteLine(msg);
        }

        public static void NewLine()
        {
            System.Console.WriteLine();
        }

        public static void NewTab()
        {
            System.Console.Write("\t");
        }

        public static void HandleTerminatin(VoidMethod startRoutine)
        {
            startCancelRoutine = startRoutine;
        }

    }
}

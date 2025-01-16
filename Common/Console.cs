

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

        public static void Debug(string msg)
        {
            string currentTime = Time.Now().FormatToISO8601();

            System.Console.ForegroundColor = System.ConsoleColor.DarkGray;
            System.Console.WriteLine($"{currentTime} [DEBUG] {msg}");
            System.Console.ResetColor();
        }

        public static void Info(string msg)
        {
            string currentTime = Time.Now().FormatToISO8601();

            System.Console.ForegroundColor = System.ConsoleColor.Cyan;
            System.Console.WriteLine($"{currentTime} [INFO] {msg}");
            System.Console.ResetColor();
        }

        public static void Warn(string msg)
        {
            string currentTime = Time.Now().FormatToISO8601();

            System.Console.ForegroundColor = System.ConsoleColor.DarkYellow;
            System.Console.WriteLine($"{currentTime} [WARN] {msg}");
            System.Console.ResetColor();
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

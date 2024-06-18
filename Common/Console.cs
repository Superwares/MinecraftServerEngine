

namespace Common
{

    public static class Console
    {

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

        public static void HandleCancelEvent(StartRoutine f)
        {
            throw new System.NotImplementedException();
        }

    }
}

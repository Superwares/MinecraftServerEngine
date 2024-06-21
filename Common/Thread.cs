
namespace Common
{
    public sealed class Thread
    {
        public static Thread New(VoidMethod startRoutine)
        {
            // TODO: Check it is created in main thread.

            throw new System.NotImplementedException();
        }

        private readonly ulong _ID;

        private Thread(ulong id) 
        {
            _ID = id;
        }

        public void Join()
        {
            throw new System.NotImplementedException();
        }

    }
}

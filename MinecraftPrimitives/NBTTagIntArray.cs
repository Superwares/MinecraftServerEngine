
namespace MinecraftPrimitives
{
    public sealed class NBTTagIntArray : NBTTagBase, IReadableNBTTag<NBTTagIntArray>
    {
        public const int TypeId = 11;

        private bool _disposed = false;

        public readonly int[] Data;

        public static byte GetTypeId() => TypeId;

        public static NBTTagIntArray Read(System.IO.Stream s, int depth)
        {
            System.Diagnostics.Debug.Assert(s != null);
            System.Diagnostics.Debug.Assert(depth >= 0);

            int length = DataInputStreamUtils.ReadInt(s);

            int[] data = new int[length];

            for (int i = 0; i < length; ++i)
            {
                data[i] = DataInputStreamUtils.ReadInt(s);
            }

            return new NBTTagIntArray(data);
        }

        public NBTTagIntArray(int[] data)
        {
            System.Diagnostics.Debug.Assert(data != null);

            Data = data;
        }

        ~NBTTagIntArray()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        public override void Write(MinecraftDataStream s)
        {
            System.Diagnostics.Debug.Assert(s != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            s.WriteInt(Data.Length);
            foreach (int value in Data)
            {
                s.WriteInt(value);
            }
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}({Data.Length})";
        }

        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed == false)
            {

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing == true)
                {
                    // Dispose managed resources.
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                //CloseHandle(handle);
                //handle = IntPtr.Zero;

                // Note disposing has been done.
                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}

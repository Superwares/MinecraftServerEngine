
using System.Xml.Linq;

namespace MinecraftPrimitives
{
    public sealed class NBTTagString : NBTTagBase, IReadableNBTTag<NBTTagString>
    {
        public const int TypeId = 8;

        private bool _disposed = false;

        public readonly string Value;

        public static byte GetTypeId() => TypeId;

        public static NBTTagString Read(System.IO.Stream s, int depth)
        {
            System.Diagnostics.Debug.Assert(s != null);
            System.Diagnostics.Debug.Assert(depth >= 0);

            string value = DataInputStreamUtils.ReadModifiedUtf8String(s);
            return new NBTTagString(value);
        }

        public NBTTagString(string value)
        {
            System.Diagnostics.Debug.Assert(value != null);
            //System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(value) == false);

            if (value == null)
            {
                value = "";
            }

            Value = value;
        }

        ~NBTTagString()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }


        public override void Write(MinecraftProtocolDataStream s)
        {
            System.Diagnostics.Debug.Assert(s != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            s.WriteModifiedString(Value);
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}({Value})";
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

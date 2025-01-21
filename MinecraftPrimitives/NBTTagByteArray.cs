﻿
namespace MinecraftPrimitives
{
    public sealed class NBTTagByteArray : NBTTagBase, IReadableNBTTag<NBTTagByteArray>
    {
        public const int TypeId = 7;

        private bool _disposed = false;

        public readonly byte[] Data;

        public static NBTTagByteArray Read(System.IO.Stream s, int depth)
        {
            System.Diagnostics.Debug.Assert(s != null);
            System.Diagnostics.Debug.Assert(depth >= 0);

            int length = DataInputStreamUtils.ReadInt(s);

            byte[] data = new byte[length];
            DataInputStreamUtils.Read(s, data);

            return new NBTTagByteArray(data);
        }

        private NBTTagByteArray(byte[] data)
        {
            System.Diagnostics.Debug.Assert(data != null);

            Data = data;
        }

        ~NBTTagByteArray()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        public override void Write(MinecraftDataStream s)
        {
            System.Diagnostics.Debug.Assert(s != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            s.WriteData(Data);
        }

        public override string ToString()
        {
            return $"NBTTagByteArray({Data.Length})";
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

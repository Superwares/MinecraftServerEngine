namespace MinecraftPrimitives
{
    public sealed class NBTTagList<T> : NBTTagListBase, IReadableNBTTag<NBTTagList<T>>
        where T : NBTTagBase, IReadableNBTTag<T>
    {
        private bool _disposed = false;

        public readonly T[] Data;

        public static NBTTagList<T> Read(System.IO.Stream s, int depth)
        {
            System.Diagnostics.Debug.Assert(s != null);
            System.Diagnostics.Debug.Assert(depth >= 0);
            if (depth > 512)
            {
                throw new NBTTagException("Tried to read NBT tag with too high complexity, depth > 512");
            }

            int length = DataInputStreamUtils.ReadInt(s);

            T[] data = new T[length];

            for (int i = 0; i < length; i++)
            {
                data[i] = T.Read(s, depth + 1);
            }

            return new NBTTagList<T>(data);
        }


        public NBTTagList(T[] data) : base()
        {
            Data = data;
        }

        ~NBTTagList()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        public override void Write(MinecraftDataStream s)
        {
            System.Diagnostics.Debug.Assert(s != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            s.WriteInt(Data.Length);

            foreach (T item in Data)
            {
                item.Write(s);
            }
        }

        public override string ToString()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            string tab = "    ";

            string str = $"NBTTagList<{typeof(T).Name}>({Data.Length})";

            str += "[";
            if (Data.Length > 0)
            {
                for (int i = 0; i < Data.Length; i++)
                {
                    string _str = Data[i].ToString();
                    string indentedStr = string.Join($"\n{tab}",
                        _str.Split('\n', System.StringSplitOptions.RemoveEmptyEntries));

                    str += $"\n{tab}{indentedStr}";

                    if (i < Data.Length - 1)
                    {
                        str += $", ";
                    }
                }

                str += "\n";
            }
            str += "]";

            return str;
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
                    foreach (T item in Data)
                    {
                        item.Dispose();
                    }
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

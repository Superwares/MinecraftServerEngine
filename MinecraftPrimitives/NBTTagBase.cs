using System;
using System.ComponentModel;
using System.IO;
using System.Reflection.Metadata;

namespace MinecraftPrimitives
{

    public abstract class NBTTagBase : System.IDisposable
    {
        private bool _disposed = false;

        ~NBTTagBase() => System.Diagnostics.Debug.Assert(false);

        public abstract void Write(Stream s);

        public override string ToString()
        {
            throw new NotImplementedException();
        }

        public virtual void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(_disposed == false);

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }
    }
}

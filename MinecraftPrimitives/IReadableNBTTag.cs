using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
{
    public interface IReadableNBTTag<T> where T : IReadableNBTTag<T>
    {
        public static abstract T Read(Stream s, int depth);
    }
}

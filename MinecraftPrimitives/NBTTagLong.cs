﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
{
    public sealed class NBTTagLong : NBTBase
    {
        public const int TypeId = 4;

        private readonly long value;

        public static NBTTagLong Read(Stream s, int depth)
        {
            int b0 = s.ReadByte();
            int b1 = s.ReadByte();
            int b2 = s.ReadByte();
            int b3 = s.ReadByte();
            int b4 = s.ReadByte();
            int b5 = s.ReadByte();
            int b6 = s.ReadByte();
            int b7 = s.ReadByte();
            long value =
                ((long)(b0 & 0xff) << 56)
                | ((long)(b1 & 0xff) << 48)
                | ((long)(b2 & 0xff) << 40)
                | ((long)(b3 & 0xff) << 32)
                | ((long)(b4 & 0xff) << 24)
                | ((long)(b5 & 0xff) << 16)
                | ((long)(b6 & 0xff) << 8)
                | ((long)(b7 & 0xff) << 0);
            return new NBTTagLong(value);
        }

        private NBTTagLong(long value)
        {
            this.value = value;
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }
    }
}

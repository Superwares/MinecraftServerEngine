﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
{
    public sealed class NBTTagInt : NBTBase
    {
        public const int TypeId = 3;

        private readonly int value;

        public static NBTTagInt Read(Stream s, int depth)
        {
            int b0 = s.ReadByte();
            int b1 = s.ReadByte();
            int b2 = s.ReadByte();
            int b3 = s.ReadByte();
            int value =
                ((b0 & 0xff) << 24)
                | ((b1 & 0xff) << 16)
                | ((b2 & 0xff) << 8)
                | ((b3 & 0xff) << 0);
            return new NBTTagInt(value);
        }

        private NBTTagInt(int value)
        {
            this.value = value;
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }
    }
}

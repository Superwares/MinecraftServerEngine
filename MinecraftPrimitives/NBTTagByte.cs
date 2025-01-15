﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
{
    internal class NBTTagByte : NBTBase
    {
        public const int TypeId = 1;

        private readonly byte value;

        public static NBTTagByte Read(Stream s, int depth)
        {
            byte value = (byte)s.ReadByte();
            return new NBTTagByte(value);
        }

        private NBTTagByte(byte value)
        {
            this.value = value;
        }

        public override void Write(Stream s)
        {
            throw new NotImplementedException();
        }
    }
}

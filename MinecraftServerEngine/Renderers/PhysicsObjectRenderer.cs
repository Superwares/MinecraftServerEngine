﻿using Common;
using Containers;

using MinecraftServerEngine.Protocols;
using MinecraftServerEngine.Physics;
using MinecraftServerEngine.Blocks;

namespace MinecraftServerEngine.Renderers
{
    internal abstract class PhysicsObjectRenderer : Renderer
    {
        private bool _blindness;

        // This is different from the actual disconnection with the Connection.
        // It means the actual disconnection of the Renderer.
        private bool _disconnected = false;
        public bool IsDisconnected => _disconnected;


        private ChunkLocation _loc;
        private int _d = -1;  // render distance


        internal PhysicsObjectRenderer(
            ConcurrentQueue<ClientboundPlayingPacket> outPackets,
            ChunkLocation loc, int d, bool blindness)
            : base(outPackets)
        {
            System.Diagnostics.Debug.Assert(d > 0);

            _loc = loc;

            _d = d;

            _blindness = blindness;
        }


        internal void ApplyBlindness(bool f)
        {
            System.Diagnostics.Debug.Assert(_disconnected == false);

            _blindness = f;
        }

        public void Disconnect()
        {
            System.Diagnostics.Debug.Assert(_disconnected == false);

            _disconnected = true;
        }

        public void Update(ChunkLocation loc)
        {
            System.Diagnostics.Debug.Assert(_disconnected == false);

            _loc = loc;
        }

        public void Update(int d)
        {
            System.Diagnostics.Debug.Assert(_disconnected == false);
            System.Diagnostics.Debug.Assert(d > 0);

            _d = d;
        }

        public bool CanRender(Vector p)
        {
            System.Diagnostics.Debug.Assert(_disconnected == false);
            //if (_disconnected == true)
            //{
            //    return false;
            //}

            if (_blindness == true)
            {
                return false;
            }

            System.Diagnostics.Debug.Assert(_d > 0);

            ChunkGrid grid = ChunkGrid.Generate(_loc, _d);
            ChunkLocation loc = ChunkLocation.Generate(p);
            return grid.Contains(loc);
        }

        // particles
    }

}

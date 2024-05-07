using Containers;

namespace Protocol
{
    internal sealed class EntityRendererManager : System.IDisposable
    {
        private bool _disposed = false;

        private bool _movement = false;

        private readonly NumList _ID_LIST = new();

        private readonly Queue<EntityRenderer> _RENDERERS = new();

        public EntityRendererManager() { }

        ~EntityRendererManager() => System.Diagnostics.Debug.Assert(false);

        public EntityRenderer Apply(
            Queue<ClientboundPlayingPacket> outPackets,
            Chunk.Vector p, int renderDistance)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            int id = _ID_LIST.Alloc();

            EntityRenderer renderer = new(id, outPackets, p, renderDistance);
            _RENDERERS.Enqueue(renderer);

            return renderer;
        }

        public void MoveAndRotate(
            int entityId,
            Entity.Vector posNew, Entity.Vector pos, Entity.Angles look, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_movement);
            
            while (!_RENDERERS.Empty)
            {
                EntityRenderer renderer = _RENDERERS.Dequeue();

                if (renderer.IsDisconnected)
                {
                    _ID_LIST.Dealloc(renderer.Id);
                    continue;
                }

                renderer.MoveAndRotate(entityId, posNew, pos, look, onGround);
                _RENDERERS.Enqueue(renderer);

            }

            _movement = true;
        }

        public void Move(
            int entityId, Entity.Vector posNew, Entity.Vector pos, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_movement);

            while (!_RENDERERS.Empty)
            {
                EntityRenderer renderer = _RENDERERS.Dequeue();

                if (renderer.IsDisconnected)
                {
                    _ID_LIST.Dealloc(renderer.Id);
                    continue;
                }

                renderer.Move(entityId, posNew, pos, onGround);
                _RENDERERS.Enqueue(renderer);

            }

            _movement = true;
        }

        public void Rotate(int entityId, Entity.Angles look, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_movement);

            while (!_RENDERERS.Empty)
            {
                EntityRenderer renderer = _RENDERERS.Dequeue();

                if (renderer.IsDisconnected)
                {
                    _ID_LIST.Dealloc(renderer.Id);
                    continue;
                }

                renderer.Rotate(entityId, look, onGround);
                _RENDERERS.Enqueue(renderer);

            }

            _movement = true;
        }

        public void Stand(int entityId)
        {
            System.Diagnostics.Debug.Assert(!_movement);

            while (!_RENDERERS.Empty)
            {
                EntityRenderer renderer = _RENDERERS.Dequeue();

                if (renderer.IsDisconnected)
                {
                    _ID_LIST.Dealloc(renderer.Id);
                    continue;
                }

                renderer.Stand(entityId);
                _RENDERERS.Enqueue(renderer);

            }
        
            _movement = true;
        }

        public void DeterminToContinueRendering(int entityId, Entity.Vector pos, BoundingBox boundingBox)
        {
            System.Diagnostics.Debug.Assert(_movement);

            while (!_RENDERERS.Empty)
            {
                EntityRenderer renderer = _RENDERERS.Dequeue();

                if (!renderer.CanRender(pos, boundingBox))
                {
                    _ID_LIST.Dealloc(renderer.Id);
                    renderer.DestroyEntity(entityId);

                    continue;
                }

                _RENDERERS.Enqueue(renderer);
            }

            _movement = false;
        }

        public void ChangeForms(int entityId, bool sneaking, bool sprinting)
        {
            foreach (var renderer in _RENDERERS.GetValues())
            {
                renderer.ChangeForms(entityId, sneaking, sprinting);
            }
        }

        public void Teleport(
            int entityId, Entity.Vector pos, Entity.Angles look, bool onGround)
        {
            foreach (var renderer in _RENDERERS.GetValues())
            {
                renderer.Teleport(entityId, pos, look, onGround);
            }
        }

        public void Flush(int entityId)
        {
            while (!_RENDERERS.Empty)
            {
                EntityRenderer renderer = _RENDERERS.Dequeue();

                _ID_LIST.Dealloc(renderer.Id);
                renderer.Flush(entityId);
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            // Assertion
            System.Diagnostics.Debug.Assert(!_movement);

            System.Diagnostics.Debug.Assert(_ID_LIST.Empty);

            System.Diagnostics.Debug.Assert(_RENDERERS.Empty);

            if (disposing == true)
            {
                // Release managed resources.
                _ID_LIST.Dispose();

                _RENDERERS.Dispose();
            }

            // Release unmanaged resources.

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        public void Close() => Dispose();
    }

}

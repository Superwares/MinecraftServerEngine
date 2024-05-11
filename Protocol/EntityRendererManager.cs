using Containers;

namespace Protocol
{
    internal sealed class EntityRendererManager : System.IDisposable
    {
        private bool _disposed = false;

        private bool _movement = false;

        /*private readonly NumList _ID_LIST = new();*/

        private readonly Queue<EntityRenderer> _RENDERERS = new();

        public EntityRendererManager() { }

        ~EntityRendererManager() => System.Diagnostics.Debug.Assert(false);

        public EntityRenderer Apply(
            Queue<ClientboundPlayingPacket> outPackets,
            Chunk.Vector p, int renderDistance)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            /*int id = _ID_LIST.Alloc();*/

            EntityRenderer renderer = new(outPackets, p, renderDistance);
            _RENDERERS.Enqueue(renderer);

            return renderer;
        }

        public void MoveAndRotate(
            int entityId,
            Entity.Vector posNew, Entity.Vector pos, Entity.Angles look, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_movement);

            for (int i = 0; i < _RENDERERS.Count; ++i)
            {
                EntityRenderer renderer = _RENDERERS.Dequeue();

                if (renderer.IsDisconnected)
                {
                    /*_ID_LIST.Dealloc(renderer.Id);*/
                    renderer.Dispose();

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
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_movement);

            for (int i = 0; i < _RENDERERS.Count; ++i)
            {
                EntityRenderer renderer = _RENDERERS.Dequeue();

                if (renderer.IsDisconnected)
                {
                    /*_ID_LIST.Dealloc(renderer.Id);*/
                    renderer.Dispose();

                    continue;
                }

                renderer.Move(entityId, posNew, pos, onGround);
                _RENDERERS.Enqueue(renderer);

            }

            _movement = true;
        }

        public void Rotate(int entityId, Entity.Angles look, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_movement);

            for (int i = 0; i < _RENDERERS.Count; ++i)
            {
                EntityRenderer renderer = _RENDERERS.Dequeue();

                if (renderer.IsDisconnected)
                {
                    /*_ID_LIST.Dealloc(renderer.Id);*/
                    renderer.Dispose();

                    continue;
                }

                renderer.Rotate(entityId, look, onGround);
                _RENDERERS.Enqueue(renderer);

            }

            _movement = true;
        }

        public void Stand(int entityId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_movement);

            for (int i = 0; i < _RENDERERS.Count; ++i)
            {
                EntityRenderer renderer = _RENDERERS.Dequeue();

                if (renderer.IsDisconnected)
                {
                    /*_ID_LIST.Dealloc(renderer.Id);*/
                    renderer.Dispose();

                    continue;
                }

                renderer.Stand(entityId);
                _RENDERERS.Enqueue(renderer);

            }
        
            _movement = true;
        }

        public void DeterminToContinueRendering(int entityId, Entity.Vector pos, BoundingBox boundingBox)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_movement);

            for (int i = 0; i < _RENDERERS.Count; ++i)
            {
                EntityRenderer renderer = _RENDERERS.Dequeue();

                if (!renderer.CanRender(pos, boundingBox))
                {
                    /*_ID_LIST.Dealloc(renderer.Id);*/
                    renderer.DestroyEntity(entityId);
                    renderer.Dispose();

                    continue;
                }

                _RENDERERS.Enqueue(renderer);
            }

            _movement = false;
        }

        public void ChangeForms(int entityId, bool sneaking, bool sprinting)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            foreach (var renderer in _RENDERERS.GetValues())
            {
                renderer.ChangeForms(entityId, sneaking, sprinting);
            }
        }

        public void Teleport(
            int entityId, Entity.Vector pos, Entity.Angles look, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            foreach (var renderer in _RENDERERS.GetValues())
            {
                renderer.Teleport(entityId, pos, look, onGround);
            }
        }

        public void Flush(int entityId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            while (!_RENDERERS.Empty)
            {
                EntityRenderer renderer = _RENDERERS.Dequeue();

                /*_ID_LIST.Dealloc(renderer.Id);*/
                renderer.Flush(entityId);
                renderer.Dispose();
            }
        }

        public void Dispose()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // Assertion
            System.Diagnostics.Debug.Assert(!_movement);

            /*System.Diagnostics.Debug.Assert(_ID_LIST.Empty);*/

            System.Diagnostics.Debug.Assert(_RENDERERS.Empty);

            // Release  resources.
            /*_ID_LIST.Dispose();*/

            _RENDERERS.Dispose();

            // Finish
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }

}

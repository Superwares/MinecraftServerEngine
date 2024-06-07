using Containers;

namespace MinecraftServerFramework
{
    internal sealed class EntityRendererManager : System.IDisposable
    {
        private bool _disposed = false;

        private bool _movement = false;

        private readonly int _id;
        public int Id => _id;

        private readonly ConcurrentSet<int> _IDS = new();  // Disposable
        private readonly ConcurrentQueue<EntityRenderer> _RENDERERS = new();  // Disposable

        public EntityRendererManager(int id) 
        {
            _id = id;
        }

        ~EntityRendererManager() => System.Diagnostics.Debug.Assert(false);

        public bool Apply(EntityRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (renderer.Id == Id)
            {
                return false;
            }
            if (_IDS.Contains(renderer.Id))
            {
                return false;
            }

            _IDS.Insert(renderer.Id);
            _RENDERERS.Enqueue(renderer);
            return true;
        }

        public void HandleRendering(Vector p)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_movement);

            System.Diagnostics.Debug.Assert(!_IDS.Contains(id));

            for (int i = 0; i < _RENDERERS.Count; ++i)
            {
                EntityRenderer renderer = _RENDERERS.Dequeue();

                if (renderer.IsDisconnected)
                {
                    _IDS.Extract(renderer.Id);

                    continue;
                }
                else if (!renderer.CanRender(p))
                {
                    _IDS.Extract(renderer.Id);
                    renderer.DestroyEntity(Id);

                    continue;
                }

                _RENDERERS.Enqueue(renderer);
            }

        }

        public void MoveAndRotate(
            Vector p, Vector pPrev, Entity.Angles look, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_movement);

            System.Diagnostics.Debug.Assert(!_IDS.Contains(id));

            foreach (EntityRenderer renderer in _RENDERERS.GetValues())
            {
                renderer.MoveAndRotate(Id, p, pPrev, look, onGround);
            }

            _movement = true;
        }

        public void Move(Vector p, Vector pPrev, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_movement);

            System.Diagnostics.Debug.Assert(!_IDS.Contains(id));

            foreach (EntityRenderer renderer in _RENDERERS.GetValues())
            {
                renderer.Move(Id, p, pPrev, onGround);
            }

            _movement = true;
        }

        public void Rotate(Entity.Angles look, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_movement);

            System.Diagnostics.Debug.Assert(!_IDS.Contains(id));

            foreach (EntityRenderer renderer in _RENDERERS.GetValues())
            {
                renderer.Rotate(Id, look, onGround);
            }
            
            _movement = true;
        }

        public void Stand()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_movement);

            System.Diagnostics.Debug.Assert(!_IDS.Contains(id));

            foreach (EntityRenderer renderer in _RENDERERS.GetValues())
            {
                renderer.Stand(Id);
            }
        
            _movement = true;
        }

        public void FinishMovementRenderring()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_movement);

            _movement = false;
        }

        public void ChangeForms(bool sneaking, bool sprinting)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_IDS.Contains(id));

            foreach (var renderer in _RENDERERS.GetValues())
            {
                renderer.ChangeForms(Id, sneaking, sprinting);
            }
        }

        /*public void Teleport(
            int entityId, Entity.Vector pos, Entity.Angles look, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_IDENTIFIERS.Contains(entityId));

            foreach (var renderer in _RENDERERS.GetValues())
            {
                renderer.Teleport(entityId, pos, look, onGround);
            }
        }*/

        public void Flush()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_IDS.Contains(Id));

            while (!_RENDERERS.Empty)
            {
                EntityRenderer renderer = _RENDERERS.Dequeue();
                if (renderer.IsDisconnected)
                {
                    continue;
                }

                renderer.DestroyEntity(Id);

                System.Diagnostics.Debug.Assert(_IDS.Contains(renderer.Id));
            }
        }

        public void Dispose()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // Assertion
            System.Diagnostics.Debug.Assert(!_movement);
            System.Diagnostics.Debug.Assert(_RENDERERS.Empty);

            // Release  resources.
            _IDS.Dispose();
            _RENDERERS.Dispose();

            // Finish
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }

}

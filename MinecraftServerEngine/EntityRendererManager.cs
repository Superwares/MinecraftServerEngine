using Containers;
using PhysicsEngine;

namespace MinecraftServerEngine
{
    internal sealed class EntityRendererManager : System.IDisposable
    {
        private bool _disposed = false;

        private bool _movement = false;

        private readonly int _id;
        public int Id => _id;

        private readonly ConcurrentSet<int> IDS = new();  // Disposable
        private readonly ConcurrentQueue<EntityRenderer> RENDERERS = new();  // Disposable

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
            if (IDS.Contains(renderer.Id))
            {
                return false;
            }

            IDS.Insert(renderer.Id);
            RENDERERS.Enqueue(renderer);
            return true;
        }

        public void HandleRendering(Vector p)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_movement);

            for (int i = 0; i < RENDERERS.Count; ++i)
            {
                EntityRenderer renderer = RENDERERS.Dequeue();

                if (renderer.Disconnected)
                {
                    IDS.Extract(renderer.Id);

                    continue;
                }
                else if (!renderer.CanRender(p))
                {
                    IDS.Extract(renderer.Id);
                    renderer.DestroyEntity(Id);

                    continue;
                }

                RENDERERS.Enqueue(renderer);
            }

        }

        public void MoveAndRotate(
            Vector p, Vector pPrev, Look look, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_movement);

            foreach (EntityRenderer renderer in RENDERERS.GetValues())
            {
                renderer.MoveAndRotate(Id, p, pPrev, look, onGround);
            }

            _movement = true;
        }

        public void Move(Vector p, Vector pPrev, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_movement);

            foreach (EntityRenderer renderer in RENDERERS.GetValues())
            {
                renderer.Move(Id, p, pPrev, onGround);
            }

            _movement = true;
        }

        public void Rotate(Look look, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_movement);

            foreach (EntityRenderer renderer in RENDERERS.GetValues())
            {
                renderer.Rotate(Id, look, onGround);
            }
            
            _movement = true;
        }

        public void Stand()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_movement);

            foreach (EntityRenderer renderer in RENDERERS.GetValues())
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

            foreach (var renderer in RENDERERS.GetValues())
            {
                renderer.ChangeForms(Id, sneaking, sprinting);
            }
        }

        public void Teleport(Vector p, Look look, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            foreach (var renderer in RENDERERS.GetValues())
            {
                renderer.Teleport(Id, p, look, onGround);
            }
        }

        public void Flush()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!IDS.Contains(Id));

            while (!RENDERERS.Empty)
            {
                EntityRenderer renderer = RENDERERS.Dequeue();
                if (renderer.Disconnected)
                {
                    continue;
                }

                renderer.DestroyEntity(Id);

                System.Diagnostics.Debug.Assert(IDS.Contains(renderer.Id));
            }
        }

        public void Dispose()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // Assertion
            System.Diagnostics.Debug.Assert(!_movement);
            System.Diagnostics.Debug.Assert(RENDERERS.Empty);

            // Release  resources.
            IDS.Dispose();
            RENDERERS.Dispose();

            // Finish
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }

}

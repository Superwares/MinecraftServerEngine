using Containers;

namespace Protocol
{
    internal sealed class EntityRendererManager : System.IDisposable
    {
        private bool _disposed = false;

        private readonly Table<int, EntityRenderer> _Renderers = new();

        public EntityRendererManager() { }

        ~EntityRendererManager() => System.Diagnostics.Debug.Assert(false);

        internal bool ContainsRenderer(int connId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            return _Renderers.Contains(connId);
        }

        public void AddRenderer(int connId, EntityRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_Renderers.Contains(connId));
            _Renderers.Insert(connId, renderer);
        }

        public void RemoveRenderer(int connId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_Renderers.Contains(connId));
            _Renderers.Extract(connId);
        }

        public void MoveAndRotate(
            int entityId,
            Entity.Vector pos, Entity.Vector posPrev, Entity.Angles look, bool onGround)
        {
            foreach (var renderer in _Renderers.GetValues())
            {
                renderer.MoveAndRotate(entityId, pos, posPrev, look, onGround);
            }
        }

        public void Move(int entityId, Entity.Vector pos, Entity.Vector posPrev, bool onGround)
        {
            foreach (var renderer in _Renderers.GetValues())
            {
                renderer.Move(entityId, pos, posPrev, onGround);
            }
        }

        public void Rotate(int entityId, Entity.Angles look, bool onGround)
        {
            foreach (var renderer in _Renderers.GetValues())
            {
                renderer.Rotate(entityId, look, onGround);
            }
        }

        public void Stand(int entityId)
        {
            foreach (var renderer in _Renderers.GetValues())
            {
                renderer.Stand(entityId);
            }
        }

        public void ChangeForms(int entityId, bool sneaking, bool sprinting)
        {
            foreach (var renderer in _Renderers.GetValues())
            {
                renderer.ChangeForms(entityId, sneaking, sprinting);
            }
        }

        public void Teleport(
            int entityId, Entity.Vector pos, Entity.Angles look, bool onGround)
        {
            foreach (var renderer in _Renderers.GetValues())
            {
                renderer.Teleport(entityId, pos, look, onGround);
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            System.Diagnostics.Debug.Assert(_Renderers.Empty);

            if (disposing == true)
            {
                // Release managed resources.
                _Renderers.Dispose();
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


using Containers;

namespace MinecraftServerFramework
{
    internal class PlayerListRendererManager : System.IDisposable
    {
        private bool _disposed = false;

        private readonly Table<System.Guid, PlayerListRenderer> _RENDERERS = new();

        public PlayerListRendererManager() { }

        ~PlayerListRendererManager() => System.Diagnostics.Debug.Assert(false);

        public void Apply(System.Guid userId, PlayerListRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_RENDERERS.Contains(userId));
            _RENDERERS.Insert(userId, renderer);
        }

        public void Cancel(System.Guid userId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_RENDERERS.Contains(userId));
            _RENDERERS.Extract(userId);
        }

        public bool Contains(System.Guid userId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return _RENDERERS.Contains(userId);
        }

        public void AddPlayerWithLaytency(System.Guid userId, string username, long ticks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            foreach (PlayerListRenderer renderer in _RENDERERS.GetValues())
            {
                renderer.AddPlayerWithLaytency(userId, username, ticks);
            }
        }

        public void RemovePlayer(System.Guid userId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            foreach (PlayerListRenderer renderer in _RENDERERS.GetValues())
            {
                renderer.RemovePlayer(userId);
            }
        }

        public void UpdatePlayerLatency(System.Guid userId, long ticks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            foreach (PlayerListRenderer renderer in _RENDERERS.GetValues())
            {
                renderer.UpdatePlayerLatency(userId, ticks);
            }
        }

        public virtual void Dispose()
        {
            // Assertion.
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_RENDERERS.Empty);

            // Release resources.
            _RENDERERS.Dispose();

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }
    }
}

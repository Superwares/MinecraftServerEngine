
using Common;

using MinecraftPrimitives;
using MinecraftServerEngine;
using MinecraftServerEngine.PhysicsEngine;

namespace TestMinecraftServerApplication
{
    public sealed class SuperWorld : World
    {
        public readonly static GameContext GameContext = new();

        //public const double CenterX = 120.0;
        //public const double CenterZ = 216.0;
        //public const double DefaultWorldBorderRadiusInMeters = 40.0;

        public readonly static Time DefaultWorldTime = MinecraftTimes.DaytimeMid;

        //public static readonly Vector PosSpawning = new(151.5, 20.0, 214.5);
        //public static readonly Vector PosSpawning = new(5.0, 5.0, 5.0);
        //public static readonly Angles LookSpawning = new(90.0, 0.0);

        private bool _disposed = false;

        private bool _canCombat = true;
        public bool CanCombat
        {
            get
            {
                System.Diagnostics.Debug.Assert(_disposed == false);
                return _canCombat;
            }
        }

        public readonly double DefaultWorldBorderRadiusInMeters;

        public readonly Vector RespawningPosition;
        public readonly Angles RespawningLook;


        private IGameStage _currentGameProgressNode = new LobbyStage();


        public SuperWorld(

            double centerX, double centerZ,
            double defaultWorldBorderRadiusInMeters,

            Vector respawningPos,
            Angles respawningLook
            )
            : base(
                  centerX, centerZ,
                  defaultWorldBorderRadiusInMeters,
                  DefaultWorldTime
                  )
        {
            System.Diagnostics.Debug.Assert(defaultWorldBorderRadiusInMeters > 0.0);
            System.Diagnostics.Debug.Assert(defaultWorldBorderRadiusInMeters <= MaxWorldBorderRadiusInMeters);
            DefaultWorldBorderRadiusInMeters = defaultWorldBorderRadiusInMeters;

            RespawningPosition = respawningPos;
            RespawningLook = respawningLook;
        }

        ~SuperWorld()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        public override bool CanJoinWorld()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            return true;
        }

        protected override bool CanDespawnPlayerOnDisconnect()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(GameContext != null);
            return GameContext.IsStarted == false;
        }

        protected override void StartRoutine()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            bool canNext = _currentGameProgressNode.StartRoutine(GameContext, this);

            if (canNext == true)
            {
                _currentGameProgressNode = _currentGameProgressNode.CreateNextNode(GameContext);
            }
        }

        protected override AbstractPlayer CreatePlayer(UserId userId, string username)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(userId != UserId.Null);
            System.Diagnostics.Debug.Assert(username != null && string.IsNullOrEmpty(username) == false);

            return new SuperPlayer(userId, username, RespawningPosition, RespawningLook);
        }

        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed == false)
            {

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing == true)
                {

                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                //CloseHandle(handle);
                //handle = IntPtr.Zero;

                // Note disposing has been done.
                _disposed = true;
            }

            base.Dispose(disposing);
        }


    }
}

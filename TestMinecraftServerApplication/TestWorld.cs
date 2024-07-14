using MinecraftServerEngine;
using MinecraftServerEngine.PhysicsEngine;
using Common;

namespace TestMinecraftServerApplication
{
    internal sealed class Lobby : World
    {
        private bool _disposed = false;

        private readonly Vector PosSpawning = new(0.0D, 101.0D, 0.0D);
        private readonly Look LookSpawning = new(0.0F, 0.0F);

        public Lobby() : base() { }

        ~Lobby() => System.Diagnostics.Debug.Assert(false);

        public override bool CanJoinWorld()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return true;
        }

        protected override bool DetermineToDespawnPlayerOnDisconnect()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return false;
        }

        protected override void StartRoutine(long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (serverTicks % 20 == 0)
            {
                /*SpawnObject(
                    new ItemEntity(
                        new ItemStack(ItemType.Stick, 30), new Vector(0.0D, 120.0D, 0.0D)));
*/
                
            }

            if (serverTicks == (20 * 5))
            {
                PhysicsObject obj = new Flame(new Vector(0.0D, 110.0D, 0.0D));
                SpawnObject(obj);
            }
            
        }

        protected override AbstractPlayer CreatePlayer(UserId id)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(id != UserId.Null);

            return new Guest(id, PosSpawning, LookSpawning);
        }

        public override void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }
}

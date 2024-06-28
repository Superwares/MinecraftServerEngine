

using MinecraftServerEngine.PhysicsEngine;

namespace MinecraftServerEngine
{
    internal abstract class Control
    {

    }

    internal sealed class MoveControl : Control
    {
        public readonly Vector Position;

        public MoveControl(Vector p)
        {
            Position = p;
        }
    }

    internal sealed class RotControl : Control
    {
        public readonly Look Look; 

        public RotControl(Look look)
        {
            Look = look;
        }
    }

    internal sealed class StandControl : Control
    {
        public readonly bool OnGround;

        public StandControl(bool onGround)
        {
            OnGround = onGround;
        }
    }

    internal sealed class SneakControl : Control
    {
        public SneakControl() { }
    }

    internal sealed class UnsneakControl : Control
    {
        public UnsneakControl() { }
    }

    internal sealed class SprintControl : Control
    {
        public SprintControl() { }
    }

    internal sealed class UnsprintControl : Control
    {
        public UnsprintControl() { }
    }

    internal abstract class InventoryControl : Control
    {
        public InventoryControl() { }
    }

    internal sealed class ClickLeftItem : InventoryControl
    {

    }

    internal sealed class ClickRightItem : InventoryControl
    {

    }

    internal sealed class ShiftClickItem : InventoryControl
    {

    }

    internal sealed class DoubleClickItem : InventoryControl
    {

    }

    internal sealed class HotbarSwapItems : InventoryControl
    {

    }

    internal sealed class LeftDragItems : InventoryControl
    {

    }

    internal sealed class RightDragItems : InventoryControl
    {

    }

}

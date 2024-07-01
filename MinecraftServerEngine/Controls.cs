

using MinecraftServerEngine.PhysicsEngine;

namespace MinecraftServerEngine
{
    internal abstract class PlayerControl
    {

    }


    internal abstract class TransformationControl : PlayerControl
    {

    }

    internal sealed class MovementControl : TransformationControl
    {
        public readonly Vector Position;

        public MovementControl(Vector p)
        {
            Position = p;
        }
    }

    internal sealed class RotatingControl : TransformationControl
    {
        public readonly Look Look; 

        public RotatingControl(Look look)
        {
            Look = look;
        }
    }

    internal sealed class StandingControl : TransformationControl
    {
        public readonly bool OnGround;

        public StandingControl(bool onGround)
        {
            OnGround = onGround;
        }
    }

    internal sealed class SneakControl : TransformationControl
    {
        public SneakControl() { }
    }

    internal sealed class UnsneakControl : TransformationControl
    {
        public UnsneakControl() { }
    }

    internal sealed class SprintControl : TransformationControl
    {
        public SprintControl() { }
    }

    internal sealed class UnsprintControl : TransformationControl
    {
        public UnsprintControl() { }
    }

/*    internal sealed class SwingHandControl : PlayerControl
    {

    }

    internal sealed class SwingMainHandControl : PlayerControl
    {

    }

    internal sealed class SwingOffHandControl : PlayerControl
    {
        // item
    }*/

    /*internal abstract class InventoryControl : PlayerControl
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

    }*/

}

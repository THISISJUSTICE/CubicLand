namespace Commar.CubicLand.Cube
{
    public enum GolemMoveState
    {
        Idle, Moving, Charging
    }

    public interface IGolemMotionController
    {
        public IGolemGeometryProvider GeometryProvider { get; }
        public GolemMoveState MoveState { get; }
        public bool IsStun { get; }
        public bool IsJumping { get; }

        public void Move(Enums.Direction direction);
        public void Rotate(bool isRight);
        public void StartJumpCharge();
        public void ReleaseJump();
    }
}
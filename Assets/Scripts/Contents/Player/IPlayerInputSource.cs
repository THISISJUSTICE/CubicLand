namespace Commar.CubicLand.Player
{
    public enum PlayerInputType
    {
        None,
        MoveFront, MoveRight, MoveLeft, MoveBack,
        RotateRight, RotateLeft, Jump
    }

    public interface IPlayerInputSource
    {
        bool GetInput(PlayerInputType input);
        bool GetInputDown(PlayerInputType input);
        bool GetInputUp(PlayerInputType input);
    }
}
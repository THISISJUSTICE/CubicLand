using UnityEngine;

namespace Commar.CubicLand.Player
{
    public sealed class PlayerKeyboardInputSource : IPlayerInputSource
    {
        private KeyCode _moveFrontKey = KeyCode.W;
        private KeyCode _moveRightKey = KeyCode.D;
        private KeyCode _moveLeftKey = KeyCode.A;
        private KeyCode _moveBackKey = KeyCode.S;
        
        private KeyCode _rotateRightKey = KeyCode.E;
        private KeyCode _rotateLeftKey = KeyCode.Q;
        private KeyCode _jumpKey = KeyCode.Space;

        public PlayerKeyboardInputSource()
        { 
            // TODO: 로컬에 저장된 KeyCode 적용
        }

        public void ApplyKeyCode()
        {
            // TODO: 외부에서 정한 KeyCode 적용 및 로컬에 저장
        }

        public bool GetInput(PlayerInputType input)
        {
            return Input.GetKey(GetKeyCode(input));
        }

        public bool GetInputDown(PlayerInputType input)
        {
            return Input.GetKeyDown(GetKeyCode(input));
        }

        public bool GetInputUp(PlayerInputType input)
        {
            return Input.GetKeyUp(GetKeyCode(input));
        }

        private KeyCode GetKeyCode(PlayerInputType input)
        {
            switch (input)
            {
                case PlayerInputType.MoveFront:
                    return _moveFrontKey;
                case PlayerInputType.MoveRight:
                    return _moveRightKey;
                case PlayerInputType.MoveLeft:
                    return _moveLeftKey;
                case PlayerInputType.MoveBack:
                    return _moveBackKey;
                case PlayerInputType.RotateRight:
                    return _rotateRightKey;
                case PlayerInputType.RotateLeft:
                    return _rotateLeftKey;
                case PlayerInputType.Jump:
                    return _jumpKey;
            }

            return KeyCode.None;
        }
    }
}
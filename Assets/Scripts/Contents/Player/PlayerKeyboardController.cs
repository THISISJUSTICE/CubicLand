using Commar.CubicLand.Cube;
using UnityEngine;

namespace Commar.CubicLand.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerKeyboardController : MonoBehaviour
    {
        private enum InputType
        {
            None,
            MoveFront,
            MoveRight,
            MoveLeft,
            MoveBack,
            RotateRight,
            RotateLeft
        }

        [SerializeField] private KeyCode _moveFrontKey = KeyCode.W;
        [SerializeField] private KeyCode _moveRightKey = KeyCode.D;
        [SerializeField] private KeyCode _moveLeftKey = KeyCode.A;
        [SerializeField] private KeyCode _moveBackKey = KeyCode.S;
        [Space]
        [SerializeField] private KeyCode _rotateRightKey = KeyCode.E;
        [SerializeField] private KeyCode _rotateLeftKey = KeyCode.Q;
        [SerializeField] private KeyCode _jumpKey = KeyCode.Space;
        [Space]
        [SerializeField, Range(0f, 1f)] private float _inputBufferDuration = 0.15f;

        private IGolemMotionController _motionController;

        private InputType _bufferedInput;
        private KeyCode _bufferedKey;
        private float _bufferedInputTime;

        public void Initialize(IGolemMotionController motionController)
        {
            _motionController = motionController;

            ResetBufferedInput();
        }

        private void Update()
        {
            if (_motionController == null)
                return;

            HandleJumpInput();

            if (Input.GetKey(_jumpKey) || _motionController.IsStun)
                return;

            BufferMoveInput();

            if (_motionController.MoveState != GolemMoveState.Idle)
                return;

            ExecuteBufferedInput();
        }

        private void HandleJumpInput()
        {
            if (_motionController.IsStun)
                return;

            if (Input.GetKey(_jumpKey) && _motionController.MoveState == GolemMoveState.Idle)
            {
                _motionController.StartJumpCharge();
                ResetBufferedInput();
            }
            else if (!Input.GetKey(_jumpKey) && _motionController.MoveState == GolemMoveState.Charging)
                _motionController.ReleaseJump();
        }

        private void BufferMoveInput()
        {
            if (SetInput(_moveFrontKey, InputType.MoveFront))
                return;

            if (SetInput(_moveRightKey, InputType.MoveRight))
                return;

            if (SetInput(_moveLeftKey, InputType.MoveLeft))
                return;

            if (SetInput(_moveBackKey, InputType.MoveBack))
                return;

            if (SetInput(_rotateRightKey, InputType.RotateRight))
                return;

            if (SetInput(_rotateLeftKey, InputType.RotateLeft))
                return;

            bool SetInput(KeyCode keyCode, InputType inputType)
            {
                if (Input.GetKeyDown(keyCode))
                {
                    _bufferedInput = inputType;
                    _bufferedKey = keyCode;
                    _bufferedInputTime = Time.time;
                    return true;
                }

                return false;
            }

            if (_bufferedInput != InputType.None && Input.GetKey(_bufferedKey))
                _bufferedInputTime = Time.time;
        }

        private void ResetBufferedInput()
        {
            _bufferedInput = InputType.None;
            _bufferedKey = KeyCode.None;
            _bufferedInputTime = 0f;
        }

        private void ExecuteBufferedInput()
        {
            if (_bufferedInput == InputType.None)
                return;

            if (Time.time - _bufferedInputTime > _inputBufferDuration)
            {
                ResetBufferedInput();
                return;
            }

            switch (_bufferedInput)
            {
                case InputType.MoveFront:
                    _motionController.Move(Enums.Direction.Front);
                    break;

                case InputType.MoveRight:
                    _motionController.Move(Enums.Direction.Right);
                    break;

                case InputType.MoveLeft:
                    _motionController.Move(Enums.Direction.Left);
                    break;

                case InputType.MoveBack:
                    _motionController.Move(Enums.Direction.Back);
                    break;

                case InputType.RotateRight:
                    _motionController.Rotate(true);
                    break;

                case InputType.RotateLeft:
                    _motionController.Rotate(false);
                    break;
            }

            ResetBufferedInput();
        }
    }
}
using Commar.CubicLand.Cube;
using UnityEngine;
using VContainer;

namespace Commar.CubicLand.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerInputController : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float _inputBufferDuration = 0.15f;

        private IPlayerInputSource _inputSource;
        private IGolemMotionController _motionController;

        private PlayerInputType _bufferedInput;
        private float _bufferedInputTime;

        [Inject]
        public void Construct(IPlayerInputSource inputSource)
        {
            _inputSource = inputSource;
        }

        public void Initialize(IGolemMotionController motionController)
        {
            _motionController = motionController;

            ResetBufferedInput();
        }

        private void Update()
        {
            if (_inputSource == null || _motionController == null)
                return;

            HandleJumpInput();

            if (_inputSource.GetInput(PlayerInputType.Jump) || _motionController.IsStun)
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

            if (_inputSource.GetInput(PlayerInputType.Jump) && _motionController.MoveState == GolemMoveState.Idle)
            {
                _motionController.StartJumpCharge();
                ResetBufferedInput();
            }
            else if (!_inputSource.GetInput(PlayerInputType.Jump) && _motionController.MoveState == GolemMoveState.Charging)
                _motionController.ReleaseJump();
        }

        private void BufferMoveInput()
        {
            if (SetInput(PlayerInputType.MoveFront))
                return;

            if (SetInput(PlayerInputType.MoveRight))
                return;

            if (SetInput(PlayerInputType.MoveLeft))
                return;

            if (SetInput(PlayerInputType.MoveBack))
                return;

            if (SetInput(PlayerInputType.RotateRight))
                return;

            if (SetInput(PlayerInputType.RotateLeft))
                return;

            bool SetInput(PlayerInputType inputType)
            {
                if (_inputSource.GetInputDown(inputType))
                {
                    _bufferedInput = inputType;
                    _bufferedInputTime = Time.time;
                    return true;
                }

                return false;
            }

            if (_bufferedInput != PlayerInputType.None && _inputSource.GetInput(_bufferedInput))
                _bufferedInputTime = Time.time;
        }

        private void ResetBufferedInput()
        {
            _bufferedInput = PlayerInputType.None;
            _bufferedInputTime = 0f;
        }

        private void ExecuteBufferedInput()
        {
            if (_bufferedInput == PlayerInputType.None)
                return;

            if (Time.time - _bufferedInputTime > _inputBufferDuration)
            {
                ResetBufferedInput();
                return;
            }

            switch (_bufferedInput)
            {
                case PlayerInputType.MoveFront:
                    _motionController.Move(Direction.Front);
                    break;

                case PlayerInputType.MoveRight:
                    _motionController.Move(Direction.Right);
                    break;

                case PlayerInputType.MoveLeft:
                    _motionController.Move(Direction.Left);
                    break;

                case PlayerInputType.MoveBack:
                    _motionController.Move(Direction.Back);
                    break;

                case PlayerInputType.RotateRight:
                    _motionController.Rotate(true);
                    break;

                case PlayerInputType.RotateLeft:
                    _motionController.Rotate(false);
                    break;
            }

            if (!_inputSource.GetInput(_bufferedInput))
                ResetBufferedInput();
        }
    }
}
using System.Collections;
using UnityEngine;

namespace CustomTIJI.CubicLand.Cube
{
    public class GolemMotionController : IGolemMotionController, ICubeMotionAdjuster, IOnEnablable
    {
        private readonly IGolemObject _golemObject;
        private readonly GolemMotionMotor _motionMotor;

        private Coroutine _coroutine;

        public GolemMoveState MoveState { get; private set; }
        public bool IsStun { get; private set; }
        public bool IsJumping { get; private set; }
        public IGolemGeometryProvider GeometryProvider => _motionMotor;

        public GolemMotionController(IGolemObject golemObject)
        {
            _golemObject = golemObject;
            _motionMotor = new GolemMotionMotor(golemObject);

            golemObject.AddUnityRoutine(this);
        }

        public void OnEnable()
        {
            _coroutine = null;
            IsJumping = false;

            _motionMotor.OnEnable();

            _golemObject.SetAttackMode(false);
            _golemObject.Rigidbody.useGravity = true;
            _golemObject.Rigidbody.FreezePositionXZ(true);

            MoveState = GolemMoveState.Idle;
            IsStun = false;
        }

        public void Move(Enums.Direction direction)
        {
            if (IsStun || _coroutine != null)
                return;

            _coroutine = _golemObject.StartCoroutine(MoveCoroutine(direction));
        }

        public void Rotate(bool isRight)
        {
            if (IsStun || _coroutine != null)
                return;

            Enums.Direction direction = Enums.Direction.Right;
            if (!isRight)
                direction = Enums.Direction.Left;

            _coroutine = _golemObject.StartCoroutine(RotateCoroutine(direction));
        }

        public void StartJumpCharge()
        {
            if (IsStun || MoveState != GolemMoveState.Idle || _coroutine != null)
                return;

            _coroutine = _golemObject.StartCoroutine(StartJumpChargeCoroutine());
        }

        public void ReleaseJump()
        {
            if (MoveState != GolemMoveState.Charging)
                return;

            if (_coroutine != null)
                _golemObject.StopCoroutine(_coroutine);

            _coroutine = _golemObject.StartCoroutine(ReleaseJumpCoroutine());
        }

        public void ApplyKnockback(Vector3 impulse)
        {
            impulse = CubeUtil.GetValidKnockbackImpulse(impulse, _golemObject.Rigidbody.mass);
            if (impulse.magnitude <= 0f)
                return;

            _golemObject.Rigidbody.FreezePositionXZ(false);
            _golemObject.Rigidbody.AddForce(impulse, ForceMode.Impulse);

            if (_coroutine != null)
                _golemObject.StopCoroutine(_coroutine);
            _coroutine = _golemObject.StartCoroutine(HandleKnockback());
        }

        public void NormalizePose()
        {
            if (_coroutine != null)
                _golemObject.StopCoroutine(_coroutine);

            _coroutine = _golemObject.StartCoroutine(NormalizePoseCoroutine());
        }

        private IEnumerator MoveCoroutine(Enums.Direction direction)
        {
            MoveState = GolemMoveState.Moving;

            if (IsJumping)
            {
                // TODO: 올라가는 도중에만 가능
                // 일정 높이 이상은 Roll
                // 올라가는 도중은 Move (한 칸만 가능)
                // 점프 가속에 따라 속도 조정

                float velocity = CubeUtil.CalculateLiftForce(_golemObject.Rigidbody, CubeConfig.CUBE_BASE_LENGHT * 0.5f) / _golemObject.Rigidbody.mass;
                float duration = velocity / Mathf.Abs(Physics.gravity.y) * 1.2f;
                yield return _motionMotor.Move(direction, duration);
            }
            else
            {
                _golemObject.Rigidbody.useGravity = false;

                if (direction == Enums.Direction.Front)
                {
                    float velocity = CubeUtil.CalculateLiftForce(_golemObject.Rigidbody, CubeConfig.CUBE_BASE_LENGHT * 0.5f) / _golemObject.Rigidbody.mass;
                    float duration = velocity / Mathf.Abs(Physics.gravity.y) * 1.2f;
                    yield return _motionMotor.MoveWithJump(direction, duration);
                }
                else
                {
                    _golemObject.SetAttackMode(true);
                    yield return _motionMotor.MoveWithRoll(direction, CalculateMoveTime());
                    _golemObject.SetAttackMode(false);
                }

                _golemObject.Rigidbody.useGravity = true;
            }

            MoveState = GolemMoveState.Idle;
            _coroutine = null;
        }

        private IEnumerator RotateCoroutine(Enums.Direction direction)
        {
            MoveState = GolemMoveState.Moving;

            if (IsJumping)
            {
                // TODO: 내려가는 중이면, 회전 속도 약간 가속?
                // Friction 적용은 없음
            }
            else
            {
                int bottomCount = _motionMotor.FindEdgeCubeDatas(Enums.Direction3D.Down).Count;
                float moveTime = Mathf.Clamp(CalculateMoveTime() * 0.9f, CubeConfig.GOLEM_MIN_MOVE_TIME, CubeConfig.GOLEM_MAX_MOVE_TIME);
                float duration = Mathf.Max(moveTime, moveTime * (CubeConfig.GOLEM_ROTATE_FRICTION * (bottomCount - 1)));
                duration = Mathf.Clamp(duration, CubeConfig.GOLEM_MIN_MOVE_TIME, CubeConfig.GOLEM_MAX_MOVE_TIME);

                _golemObject.SetAttackMode(true);
                yield return _motionMotor.Rotate(direction, duration);
                _golemObject.SetAttackMode(false);
            }

            MoveState = GolemMoveState.Idle;
            _coroutine = null;
        }

        private IEnumerator StartJumpChargeCoroutine()
        {
            MoveState = GolemMoveState.Charging;
            yield return _motionMotor.ChargeJump();
            _coroutine = null;
        }

        private IEnumerator ReleaseJumpCoroutine()
        {
            IsJumping = true;
            yield return _motionMotor.ReleaseJump();
            _coroutine = null;

            // TODO: State는 지상에 착지할 때까지 유지
            // Ray는 Golem Bottom의 Axis 큐브의 모든 중심에서 쏴서, 가장 거리가 가까운 것을 사용
            // 지상 높이는 최초에 Raycast 후 해당 오브젝트가 파괴되거나 위치가 변경되지 않는 한 유지
        }

        private IEnumerator HandleKnockback()
        {
            IsStun = true;

            yield return YieldCache.WaitForFixedUpdate;

            float waitTime = CubeUtil.CalculateKnockbackTime(_golemObject.Rigidbody.linearVelocity, _golemObject.Rigidbody.mass);
            if (waitTime > 0f)
                yield return GlobalRoot.Instance.YieldCache.GetWaitForSeconds(waitTime);

            _golemObject.Rigidbody.ClearVelocity();

            _coroutine = null;

            NormalizePose();
        }

        private IEnumerator NormalizePoseCoroutine()
        {
            _golemObject.Rigidbody.FreezePositionXZ(true);
            IsStun = true;
            yield return CubeUtil.NormalizePoseCoroutine(_golemObject.Rigidbody);
            IsStun = false;
            _coroutine = null;
        }

        private float CalculateMoveTime()
        {
            return _golemObject.CalculateMoveTime(CubeConfig.GOLEM_INIT_MOVE_TIME, CubeConfig.GOLEM_MIN_MOVE_TIME, CubeConfig.GOLEM_MAX_MOVE_TIME);
        }
    }
}
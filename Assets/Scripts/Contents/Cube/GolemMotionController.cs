using System.Collections;
using UnityEngine;

namespace Commar.CubicLand.Cube
{
    public class GolemMotionController : IGolemMotionController, ICubeMotionAdjuster, IOnEnablable
    {
        private readonly IGolemObject _golemObject;
        private readonly IGroundSensor _groundSensor;
        private readonly GolemMotionMotor _motionMotor;

        private Coroutine _coroutine;

        public GolemMoveState MoveState { get; private set; }
        public bool IsStun { get; private set; }
        public bool IsAirborne => !_groundSensor.IsGrounded;
        public IGolemGeometryProvider GeometryProvider => _motionMotor;

        public GolemMotionController(IGolemObject golemObject, IGroundSensor groundSensor)
        {
            _golemObject = golemObject;
            _motionMotor = new GolemMotionMotor(golemObject);
            _groundSensor = groundSensor;

            golemObject.AddUnityRoutine(this);
        }

        public void OnEnable()
        {
            _coroutine = null;

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

            StartCoroutine(MoveCoroutine(direction));
        }

        public void Rotate(bool isRight)
        {
            if (IsStun || _coroutine != null)
                return;

            Enums.Direction direction = Enums.Direction.Right;
            if (!isRight)
                direction = Enums.Direction.Left;

            StartCoroutine(RotateCoroutine(direction));
        }

        public void StartJumpCharge()
        {
            if (IsStun || MoveState != GolemMoveState.Idle || _coroutine != null)
                return;

            StartCoroutine(StartJumpChargeCoroutine());
        }

        public void ReleaseJump()
        {
            if (MoveState != GolemMoveState.Charging)
                return;

            StopCurrentCoroutine();

            StartCoroutine(ReleaseJumpCoroutine());
        }

        public void ApplyKnockback(Vector3 impulse)
        {
            impulse = CubeUtil.GetValidKnockbackImpulse(impulse, _golemObject.Rigidbody.mass);
            if (impulse.magnitude <= 0f)
                return;

            bool wasCharging = MoveState == GolemMoveState.Charging;

            StopCurrentCoroutine();
            SetStunState();

            _golemObject.Rigidbody.ClearVelocity();
            _golemObject.Rigidbody.FreezePositionXZ(false);
            _golemObject.Rigidbody.AddForce(impulse, ForceMode.Impulse);

            StartCoroutine(HandleKnockback(wasCharging));
        }

        public void NormalizePose()
        {
            StopCurrentCoroutine();

            StartCoroutine(NormalizePoseCoroutine());
        }

        private IEnumerator MoveCoroutine(Enums.Direction direction)
        {
            MoveState = GolemMoveState.Moving;

            if (IsAirborne)
            {
                // TODO: 올라가는 도중에만 가능
                // 일정 높이 이상은 Tumble
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

            SetIdleState();
            CompleteCurrentCoroutine();
        }

        private IEnumerator RotateCoroutine(Enums.Direction direction)
        {
            MoveState = GolemMoveState.Moving;

            if (IsAirborne)
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

            SetIdleState();
            CompleteCurrentCoroutine();
        }

        private IEnumerator StartJumpChargeCoroutine()
        {
            MoveState = GolemMoveState.Charging;
            yield return _motionMotor.ChargeJump();
            CompleteCurrentCoroutine();
        }

        private IEnumerator ReleaseJumpCoroutine()
        {
            if (_motionMotor.ReleaseJumpForce())
                _groundSensor.NotifyAirborne();

            yield return _motionMotor.RestoreScale();

            SetIdleState();
            CompleteCurrentCoroutine();
        }

        private IEnumerator HandleKnockback(bool wasCharging)
        {
            yield return YieldCache.WaitForFixedUpdate;

            float waitTime = CubeUtil.CalculateKnockbackTime(_golemObject.Rigidbody.linearVelocity, _golemObject.Rigidbody.mass);

            if (wasCharging)
            {
                if (_motionMotor.ReleaseJumpForce())
                    _groundSensor.NotifyAirborne();

                float restoreStartTime = Time.time;
                yield return _motionMotor.RestoreScale();
                waitTime -= Time.time - restoreStartTime;
            }

            if (waitTime > 0f)
                yield return GlobalRoot.Instance.YieldCache.GetWaitForSeconds(waitTime);

            _golemObject.Rigidbody.ClearVelocity();

            yield return NormalizePoseInternal();
            CompleteCurrentCoroutine();
        }

        private IEnumerator NormalizePoseCoroutine()
        {
            yield return NormalizePoseInternal();
            CompleteCurrentCoroutine();
        }

        private IEnumerator NormalizePoseInternal()
        {
            SetStunState();
            _golemObject.Rigidbody.FreezePositionXZ(true);

            yield return CubeUtil.NormalizePoseCoroutine(_golemObject.Rigidbody);

            IsStun = false;
        }

        private float CalculateMoveTime()
        {
            return _golemObject.CalculateMoveTime(CubeConfig.GOLEM_INIT_MOVE_TIME, CubeConfig.GOLEM_MIN_MOVE_TIME, CubeConfig.GOLEM_MAX_MOVE_TIME);
        }

        private void StopCurrentCoroutine()
        {
            if (_coroutine != null)
                _golemObject.StopCoroutine(_coroutine);

            _coroutine = null;
        }

        private void StartCoroutine(IEnumerator coroutine)
        {
            _coroutine = _golemObject.StartCoroutine(coroutine);
        }

        private void CompleteCurrentCoroutine()
        {
            _coroutine = null;
        }

        private void SetIdleState()
        {
            MoveState = GolemMoveState.Idle;
            _golemObject.SetAttackMode(false);
            _golemObject.Rigidbody.useGravity = true;
        }

        private void SetStunState()
        {
            IsStun = true;
            SetIdleState();
        }
    }
}
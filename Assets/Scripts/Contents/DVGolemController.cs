using UnityEngine;
using System.Collections;

[RequireComponent(typeof(DVGolemCore))]
public class DVGolemController : MonoBehaviour
{
    #region Types
    protected struct MoveDirection
    {
        public Vector3 Right;
        public Vector3 Left { get => -Right; }
        public Vector3 Up;
        public Vector3 Down { get => -Up; }
        public Vector3 Front;
        public Vector3 Back { get => -Front; }

        public MoveDirection(Vector3 front, Vector3 right, Vector3 up)
        {
            Right = right.normalized;
            Up = up.normalized;
            Front = front.normalized;
        }

        public void Rotate(Vector3 eulerAngle)
        {
            Quaternion rotation = Quaternion.Euler(eulerAngle);

            Right = rotation * Right;
            Up = rotation * Up;
            Front = rotation * Front;
        }
    }

    protected struct ActingFlag
    {
        public bool Acting;
        public bool ActFlag;

        public ActingFlag(bool on = false)
        {
            Acting = on;
            ActFlag = on;
        }
    }

    protected enum ActState
    {
        IDLE,
        JUMP_MOVE,
        ROLL_MOVE,
        CHARGING,
        JUMP,


    }
    #endregion

    #region Variables
    public const int MAX_JUMP_HEIGHT = 8;

    protected DVGolemInfo _golemInfo;

    protected Rigidbody _rb;
    protected MoveDirection _moveDirection;

    protected ActingFlag _move;

    protected Coroutine _chargeCor;
    protected float _chargeHeight;

    protected int AnimationFrame { get => DVPerfomanceConfigs.AnimationFrame; }

    // TODO: 큐브 개수 및 능력치 고려
    protected float MoveTime { get => 0.7f; } // 큐브 수가 많을 수록 시간 증가, 곱연산으로 증가량이 점점 미미해질 수 있도록, 이속 강화를 계속해야 큐브가 많아져도 이속이 느려지지 않게
    protected float RotateTime { get => 0.5f; } // MoveTime * 0.8f;
    protected float JumpChargeTime { get => 0.3f; } // MoveTime * 0.2f;
    #endregion

    #region Unity Functions
    protected virtual void Awake()
    {
        _golemInfo = GetComponent<DVGolemCore>().GolemInfo;
        _moveDirection = new MoveDirection(transform.forward, transform.right, transform.up);
        _chargeHeight = 0f;
    }

    protected virtual void Start() { 

    }
    #endregion

    #region Settings
    #endregion

    #region Controller
    protected void CancelMoveGolem()
    {
        _move.ActFlag = false;
    }

    protected void CancelChargeCor()
    {
        if (_chargeCor != null)
        {
            StopCoroutine(_chargeCor);
            _chargeCor = null;
        }
    }

    protected void MoveGolem(DVEnums.Direction direction, float time, bool depend = false)
    {
        if (_move.Acting && !depend)
            return;
        _move.ActFlag = true;

        StartCoroutine(OneCubeMoveCor(GetDirection(direction), time));
    }

    protected void JumpGolem(float time, int jumpHeight)
    {
        if (_move.Acting)
            return;
        _move.ActFlag = true;

        StartCoroutine(HalfCubeJumpCor(time, jumpHeight));
    }

    protected void RollGolem(DVEnums.Direction direction)
    {
        if (_move.Acting)
            return;
        _move.ActFlag = true;

        StartCoroutine(RightAngleRollCor(direction, MoveTime));
    }

    protected void MoveGolemWithJump(DVEnums.Direction direction, int jumpHeight)
    {
        if (_move.Acting)
            return;
        _move.ActFlag = true;

        StartCoroutine(HalfCubeJumpCor(MoveTime * MoveTime, jumpHeight, roop: true));
        MoveGolem(direction, MoveTime * MoveTime, depend: true);
    }

    protected void RotateGolem(DVEnums.Direction direction)
    {
        if (_move.Acting)
            return;
        _move.ActFlag = true;

        StartCoroutine(RightAngleRotateCor(direction, RotateTime));
    }

    protected void ChargeJumpReady()
    {
        CancelChargeCor();

        _chargeCor = StartCoroutine(HalfCubeReSizeCor(JumpChargeTime));
    }

    protected void ChargeJumpAction(float keyingTime)
    {
        float jumpTime = MoveTime * MoveTime;
        int jumpHeight = 1;
        if (keyingTime > JumpChargeTime)
        {
            int add = Mathf.RoundToInt(Mathf.Floor(keyingTime / JumpChargeTime));
            add = Mathf.RoundToInt(Mathf.Clamp(add, 0, MAX_JUMP_HEIGHT - 1));
            jumpHeight += add;
            jumpTime = jumpTime + jumpTime / 4f * (float)add;
        }

        CancelChargeCor();
        JumpGolem(jumpTime, jumpHeight);

        StartCoroutine(DVHelper.In.WaitActCor(MoveTime, CancelMoveGolem));
    }
    #endregion

    #region Utils
    protected Vector3 GetDirection(DVEnums.Direction direction)
    {
        switch (direction)
        {
            case DVEnums.Direction.FRONT:
                return _moveDirection.Front;
            case DVEnums.Direction.BACK:
                return _moveDirection.Back;
            case DVEnums.Direction.LEFT:
                return _moveDirection.Left;
            case DVEnums.Direction.RIGHT:
                return _moveDirection.Right;
        }

        return Vector3.zero;
    }

    protected Vector3 GetDirection(DVEnums.Direction3D direction)
    {
        switch (direction)
        {
            case DVEnums.Direction3D.FRONT:
            case DVEnums.Direction3D.BACK:
            case DVEnums.Direction3D.LEFT:
            case DVEnums.Direction3D.RIGHT:
                return GetDirection(DVUtil.ConvertDirection3DTo2D(direction));
            case DVEnums.Direction3D.UP:
                return _moveDirection.Up;
            case DVEnums.Direction3D.DOWN:
                return _moveDirection.Down;
        }

        return Vector3.zero;
    }

    protected DVEnums.Direction3D ConvertMoveToTransformDirection(DVEnums.Direction3D direction) {
        Vector3[] tDirs = new Vector3[] {
            transform.right,
            -transform.right,
            transform.up,
            -transform.up,
            transform.forward,
            -transform.forward
        };

        Vector3 mDir = GetDirection(direction);
        DVUtil.GetClosestAxisVector(tDirs, mDir, out int index);

        return (DVEnums.Direction3D)index;
    }
    #endregion

    #region Coroutines
    protected IEnumerator OneCubeMoveCor(Vector3 dir, float time)
    {
        // 넉백으로도 사용 가능해 보임
        // TODO: 이동 시 벽에 충돌 시 데미지를 입거나 지나가지 못하도록 조정
        _move.Acting = true;

        Vector3 moveDir = dir.normalized * DVConfigs.CUBE_BASE_LENGHT;
        Vector3 addMove = moveDir / (float)AnimationFrame;
        float addTime = time / (float)AnimationFrame;

        while (_move.ActFlag)
        {
            for (int i = 0; i < AnimationFrame; i++)
            {
                transform.position += addMove;
                yield return DVHelper.In.YieldCache.GetWaitForSeconds(addTime);
            }
        }

        _move.Acting = false;
    }

    protected IEnumerator HalfCubeJumpCor(float time, int jumpHeight, bool roop = false)
    {
        _move.Acting = true;

        float addHeight;

        float startHeight = transform.position.y;
        float moveHeight = DVConfigs.CUBE_BASE_LENGHT * (float)jumpHeight * 0.5f + startHeight;
        int halfFrame = AnimationFrame / 2;
        float addTime = time / (float)AnimationFrame;

        int chargeFrame = halfFrame / 4 - halfFrame % 4;
        float chargeHeight = _chargeHeight / (float)chargeFrame;
        Vector3 chargeScale = (Vector3.one - transform.localScale) / (float)chargeFrame;
        _chargeHeight = 0f;

        while (_move.ActFlag)
        {
            for (int i = 0; i < halfFrame; i++)
            {
                addHeight = Mathf.Lerp(startHeight, moveHeight, DVUtil.GetEaseOut((float)(i + 1) / (float)halfFrame)) -
                    Mathf.Lerp(startHeight, moveHeight, DVUtil.GetEaseOut((float)i / (float)halfFrame));
                transform.position += Vector3.up * addHeight;

                // TODO: chargeHeight가 더 빨라야 함
                if (i < chargeFrame) {
                    transform.position += Vector3.up * chargeHeight;
                    transform.localScale = (transform.localScale + chargeScale).Clamp(0, 1f);
                    Debug.Log($"scale({transform.localScale}), chargeHeight({chargeHeight}), chargeFrame({chargeFrame})");
                }

                yield return DVHelper.In.YieldCache.GetWaitForSeconds(addTime);
            }

            for (int i = 0; i < halfFrame; i++)
            {
                addHeight = Mathf.Lerp(startHeight, moveHeight, DVUtil.GetEaseOut((float)(halfFrame - i - 1) / (float)halfFrame)) -
                    Mathf.Lerp(startHeight, moveHeight, DVUtil.GetEaseOut((float)(halfFrame - i) / (float)halfFrame));
                transform.position += Vector3.up * addHeight;

                yield return DVHelper.In.YieldCache.GetWaitForSeconds(addTime);
            }

            if (!roop)
                break;
            else {
                chargeFrame = 0;
                chargeHeight = 0f;
            }
        }

        _move.Acting = false;
    }

    protected IEnumerator RightAngleRollCor(DVEnums.Direction direction, float time)
    {
        _move.Acting = true;

        Vector3 dir = GetDirection(direction);
        Vector3 moveDir = -dir.normalized;
        float addTime = time / (float)AnimationFrame;
        float halfLine = DVConfigs.CUBE_BASE_LENGHT / 2f;

        Quaternion startRot, targetRot;
        int curCube, nextCube;
        float curHeight, nextHeight, angle, prevAngle;
        float rollHypot, rollAxisAngle;

        while (_move.ActFlag)
        {
            // TODO: 이동 시 벽에 충돌 시 데미지를 입거나 지나가지 못하도록 조정
            curCube = _golemInfo.GetDirectionSize(ConvertMoveToTransformDirection(DVEnums.Direction3D.DOWN));
            nextCube = _golemInfo.GetDirectionSize(ConvertMoveToTransformDirection(DVUtil.ConvertDirection2DTo3D(direction)));

            curHeight = ((float)curCube * 2f - 1f) * halfLine;
            nextHeight = ((float)nextCube * 2f - 1f) * halfLine;
            rollAxisAngle = DVUtil.GetAngle(nextHeight, curHeight);
            rollHypot = DVUtil.GetHypotenuse(nextHeight, curHeight);

            startRot = transform.rotation;
            targetRot = Quaternion.FromToRotation(dir, _moveDirection.Down) * startRot;
            prevAngle = 0;

            for (int i = 0; i < AnimationFrame; i++)
            {
                // Rotate
                transform.rotation = Quaternion.Slerp(startRot, targetRot, (float)(i + 1) / (float)AnimationFrame);
                angle = Quaternion.Angle(startRot, transform.rotation);

                // Height
                transform.position += Vector3.up *
                    (DVUtil.GetHeightLine(rollHypot, angle + rollAxisAngle)
                    - DVUtil.GetHeightLine(rollHypot, prevAngle + rollAxisAngle));

                // Move
                transform.position += moveDir *
                    (DVUtil.GetBaseLine(rollHypot, angle + rollAxisAngle)
                    - DVUtil.GetBaseLine(rollHypot, prevAngle + rollAxisAngle));

                prevAngle = angle;
                yield return DVHelper.In.YieldCache.GetWaitForSeconds(addTime);
            }
        }

        _move.Acting = false;
    }

    protected IEnumerator RightAngleRotateCor(DVEnums.Direction direction, float time)
    {
        _move.Acting = true;

        Vector3 dir;
        Quaternion startRot, targetRot, rot;
        float addTime = time / (float)AnimationFrame;

        while (_move.ActFlag)
        {
            startRot = transform.rotation;
            dir = GetDirection(direction);
            rot = Quaternion.FromToRotation(_moveDirection.Front, dir);
            targetRot = rot * startRot;

            for (int i = 0; i < AnimationFrame; i++)
            {
                transform.rotation = Quaternion.Slerp(startRot, targetRot, (float)(i + 1) / (float)AnimationFrame);
                yield return DVHelper.In.YieldCache.GetWaitForSeconds(addTime);
            }

            _moveDirection.Rotate(rot.eulerAngles);
        }

        _move.Acting = false;
    }

    protected IEnumerator HalfCubeReSizeCor(float chargeTime)
    {
        float sizeLength = (float)(_golemInfo.GetDirectionSize(ConvertMoveToTransformDirection(DVEnums.Direction3D.DOWN)) - 1)
            * DVConfigs.CUBE_BASE_LENGHT;
        float stdTime = chargeTime / (float)AnimationFrame * 2f;
        float addSize = 0.5f / (float)MAX_JUMP_HEIGHT / (float)AnimationFrame * 2f;
        float addHeight = (0.5f + sizeLength) * addSize;

        Vector3[] tDirs = new Vector3[] {
            transform.right, -transform.right,
            transform.up, -transform.up,
            transform.forward, -transform.forward,
        };
        Vector3[] scaleDirs = new Vector3[] { Vector3.right, Vector3.up, Vector3.forward };

        DVUtil.GetClosestAxisVector(tDirs, Vector3.up, out int index);

        for (int i = 0; i < MAX_JUMP_HEIGHT * AnimationFrame / 2; i++)
        {
            transform.localScale -= scaleDirs[index / 2] * addSize;
            transform.position -= Vector3.up * addHeight;
            _chargeHeight += addHeight;
            yield return DVHelper.In.YieldCache.GetWaitForSeconds(stdTime);
        }
    }
    #endregion
}

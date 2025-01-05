using UnityEngine;
using System.Collections;

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
            right = right.normalized;
            up = up.normalized;
            front = front.normalized;

            Right = right;
            Up = up;
            Front = front;
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

    protected Rigidbody _rb;
    protected MoveDirection _moveDirection;

    protected ActingFlag _move;

    protected Coroutine _chargeCor;
    protected float _chargeHeight;

    protected int AnimationFrame
    {
        get
        {
            // 기기의 Frame에 맞게 조정
            // 2의 배수로
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
            return 30 * 2;
#else
            return 7 * 2;
#endif
        }
    }

    // TODO: 큐브 개수 및 능력치 고려
    protected float MoveTime { get => 0.7f; } 
    protected float RotateTime { get => 0.5f; }
    protected float JumpChargeTime { get => 0.3f; }
    #endregion

    #region Unity Functions
    protected virtual void Awake()
    {
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

    protected void RollGolem(DVEnums.Direction direction, float time)
    {
        if (_move.Acting)
            return;
        _move.ActFlag = true;

        StartCoroutine(RightAngleRollCor(GetDirection(direction), time));
    }

    protected void MoveGolemWithJump(DVEnums.Direction direction, float time, int jumpHeight)
    {
        if (_move.Acting)
            return;
        _move.ActFlag = true;

        StartCoroutine(HalfCubeJumpCor(time, jumpHeight, roop: true));
        MoveGolem(direction, time, depend: true);
    }

    protected void RotateGolem(DVEnums.Direction direction, float time)
    {
        if (_move.Acting)
            return;
        _move.ActFlag = true;

        StartCoroutine(RightAngleRotateCor(direction, time));
    }

    protected void ChargeJumpReady(KeyCode key, float chargeTime)
    {
        CancelChargeCor();

        _chargeCor = StartCoroutine(HalfCubeReSizeCor(key, chargeTime));
    }

    protected void ChargeJumpAction(float time, float keyingTime, float chargeTime)
    {
        float jumpTime = time;
        int jumpHeight = 1;
        if (keyingTime > chargeTime)
        {
            int add = (int)Mathf.Floor(keyingTime / chargeTime);
            add = (int)Mathf.Clamp(add, 0, MAX_JUMP_HEIGHT - 1);
            jumpHeight += add;
            jumpTime = time + time / 4f * (float)add;
        }

        CancelChargeCor();
        JumpGolem(jumpTime, jumpHeight);

        StartCoroutine(DVHelper.In.WaitActCor(time, CancelMoveGolem));
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

    protected float GetKeyingTime(KeyCode key)
    {
        return DVKeyboardManager.Instance.GetKeyingTime(key);
    }
    #endregion

    #region Coroutines
    protected IEnumerator OneCubeMoveCor(Vector3 dir, float time)
    {
        // 넉백으로도 사용 가능해 보임
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
        float chargeHeight = _chargeHeight / (int)halfFrame;
        _chargeHeight = 0f;

        while (_move.ActFlag)
        {
            for (int i = 0; i < halfFrame; i++)
            {
                addHeight = Mathf.Lerp(startHeight, moveHeight, DVUtil.GetEaseOut((float)(i + 1) / (float)halfFrame)) -
                    Mathf.Lerp(startHeight, moveHeight, DVUtil.GetEaseOut((float)i / (float)halfFrame));

                transform.position += Vector3.up * (addHeight + chargeHeight);
                transform.localScale = (transform.localScale + Vector3.one * addHeight).Clamp(0, 1f);
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
        }

        _move.Acting = false;
    }

    protected IEnumerator RightAngleRollCor(Vector3 dir, float time)
    {
        _move.Acting = true;

        Vector3 moveDir = -dir.normalized;
        Quaternion startRot, targetRot;
        int curCube, nextCube;
        float curHeight, nextHeight, angle, prevAngle;
        float rollHypot, rollAxisAngle, rollTime;
        float addTime;

        float halfLine = DVConfigs.CUBE_BASE_LENGHT / 2f;

        while (_move.ActFlag)
        {
            // TODO: nextCube에 따른 이동량 구분
            curCube = 1;
            nextCube = 1;

            curHeight = ((float)curCube * 2f - 1f) * halfLine;
            nextHeight = ((float)nextCube * 2f - 1f) * halfLine;
            rollAxisAngle = DVUtil.GetAngle(nextHeight, curHeight);
            rollHypot = DVUtil.GetHypotenuse(nextHeight, curHeight);
            rollTime = (float)nextCube * time;

            startRot = transform.rotation;
            targetRot = Quaternion.FromToRotation(dir, _moveDirection.Down) * startRot;
            prevAngle = 0;

            addTime = rollTime / (float)AnimationFrame;

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

    protected IEnumerator HalfCubeReSizeCor(KeyCode key, float chargeTime)
    {
        // TODO: 보유 큐브들의 scale 값, position 값도 같이 변경
        int sizeCount = MAX_JUMP_HEIGHT;
        float stdTime = chargeTime / (float)AnimationFrame * 2f;
        float addSize = 0.5f / (float)sizeCount / (float)AnimationFrame * 2f;
        float addHeight = addSize / 2f;

        Vector3 scaleDir = DVUtil.GetClosestAxisVector(
            new Vector3[] { transform.right, transform.up, transform.forward }, Vector3.up, out var index);

        for (int i = 0; i < sizeCount * AnimationFrame / 2; i++)
        {
            if (index == 0)
            {
                transform.localScale -= Vector3.right * addSize;
            }
            else if (index == 1)
            {
                transform.localScale -= Vector3.up * addSize;
            }
            else if (index == 2)
            {
                transform.localScale -= Vector3.forward * addSize;
            }

            transform.position += Vector3.down * addHeight;
            _chargeHeight += addHeight;
            yield return DVHelper.In.YieldCache.GetWaitForSeconds(stdTime);
        }
    }
    #endregion
}

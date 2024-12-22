using UnityEngine;
using System.Collections;

public class DVPlayerController : MonoBehaviour
{
    #region Types
    protected struct MoveDirection {
        public Vector3 Front;
        public Vector3 Back { get => -Front; }
        public Vector3 Right;
        public Vector3 Left { get => -Right; }
        public Vector3 Up;
        public Vector3 Down { get => -Up; }

        public MoveDirection(Vector3 front, Vector3 right, Vector3 up) {
            front = front.normalized;
            right = right.normalized;
            up = up.normalized;

            Front = front;
            Right = right;
            Up = up;
        }

        public void Rotate(Vector3 eulerAngle) {
            Quaternion rotation = Quaternion.Euler(eulerAngle);

            Front = rotation * Front;
            Right = rotation * Right;
            Up = rotation * Up;
        }
    }

    protected struct ActingFlag {
        public bool Acting;
        public bool ActFlag;

        public ActingFlag(bool on = false) {
            Acting = on;
            ActFlag = on;
        }
    }
    #endregion

    #region Variables
    protected Rigidbody _rb;
    protected MoveDirection _moveDirection;

    protected ActingFlag _move;
    protected ActingFlag _jump;
    protected ActingFlag _roll;

    protected int AnimationFrame { 
        get 
        {
            // 2의 배수로
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
            return 15 * 2;
#else
            return 7 * 2;
#endif
        } 
    }
#endregion

    #region Unity Functions
    protected void Awake()
    {
        _moveDirection = new MoveDirection(transform.forward, transform.right, transform.up);
    }

    protected void Start()
    {
        KeySetting();

    }
    #endregion

    #region Settings
    protected void KeySetting() {
        float moveTime = 0.5f;

        // TODO: 구르는 이동
        DVKeyboardManager.Instance.SetKeyDownUp(KeyCode.W,
            () => MoveGolemWithJump(DVEnums.Direction.FRONT, moveTime, 1),
            () => CancelMoveGolemWithJump());
        DVKeyboardManager.Instance.SetKeyDownUp(KeyCode.D,
            () => RollGolem(DVEnums.Direction.RIGHT, moveTime * 1.5f),
            () => CancelRollGolem());
        DVKeyboardManager.Instance.SetKeyDownUp(KeyCode.A,
            () => RollGolem(DVEnums.Direction.LEFT, moveTime * 1.5f),
            () => CancelRollGolem());
        DVKeyboardManager.Instance.SetKeyDownUp(KeyCode.S,
            () => RollGolem(DVEnums.Direction.BACK, moveTime * 2f),
            () => CancelRollGolem());

        // TODO: 방향 회전

        // TODO: 차지 점프

        DVKeyboardManager.Instance.SetKeyLocks(new KeyCode[] { KeyCode.W, KeyCode.D, KeyCode.A, KeyCode.S });
    }
    #endregion

    #region Controller
    protected void MoveGolem(DVEnums.Direction direction, float time) {
        if (_move.Acting)
            return;
        _move.ActFlag = true;

        StartCoroutine(OneCubeMoveCor(GetDirection(direction), time));
    }

    protected void CancelMoveGolem() {
        _move.ActFlag = false;
    }

    protected void RollGolem(DVEnums.Direction direction, float time) { 
        if(_roll.Acting || _move.Acting) 
            return;
        _roll.ActFlag = true;

        MoveGolem(direction, time);
        StartCoroutine(RightAngleRollCor(GetDirection(direction), time));
    }

    protected void CancelRollGolem() {
        CancelMoveGolem();
        _roll.ActFlag = false;
    }

    protected void JumpGolem(float time, int jumpHeight) {
        if (_jump.Acting)
            return;
        _jump.ActFlag = true;

        StartCoroutine(HalfCubeJumpCor(time, jumpHeight));
    }

    protected void CancelJumpGolem() { 
        _jump.ActFlag = false;
    }

    protected void MoveGolemWithJump(DVEnums.Direction direction, float time, int jumpHeight)
    {
        if (_move.Acting || _jump.Acting)
            return;

        MoveGolem(direction, time);
        JumpGolem(time, jumpHeight);
    }

    protected void CancelMoveGolemWithJump() { 
        CancelMoveGolem();
        CancelJumpGolem();
    }
    #endregion

    #region Utils
    protected Vector3 GetDirection(DVEnums.Direction direction) {
        switch (direction) {
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
    #endregion

    #region Coroutines
    protected IEnumerator OneCubeMoveCor(Vector3 dir, float time) {
        _move.Acting = true;

        // TODO
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

    protected IEnumerator HalfCubeJumpCor(float time, int jumpHeight) {
        _jump.Acting = true;

        // TODO
        float moveHeight = DVConfigs.CUBE_BASE_LENGHT * (float)jumpHeight * 0.5f;
        Vector3 addHeight = _moveDirection.Up * moveHeight / (float)AnimationFrame;
        float addTime = time / (float)AnimationFrame;

        while (_jump.ActFlag)
        {
            for (int i = 0; i < AnimationFrame; i++)
            {
                if (i == AnimationFrame / 2)
                    addHeight *= -1f;
                transform.position += addHeight;
                yield return DVHelper.In.YieldCache.GetWaitForSeconds(addTime);
            }

            /*for (int i = 0; i < AnimationFrame / 2; i++)
            {
                transform.position -= addHeight;
                yield return DVHelper.In.YieldCache.GetWaitForSeconds(addTime);
            }*/
        }

        _jump.Acting = false;
    }

    protected IEnumerator RightAngleRollCor(Vector3 dir, float time) {
        // TODO: 보유 큐브가 많아 모양이 다른 경우의 위치 값도 고려
        _roll.Acting = true;

        Quaternion startRot, targetRot, baseRot;
        int curCube, nextCube;
        float curHeight, nextHeight, angle;

        float baseLine = DVConfigs.CUBE_BASE_LENGHT / 2f;
        float hypot = DVUtil.GetHypotenuse(baseLine, baseLine);
        float addTime = time / (float)AnimationFrame;

        while (_roll.ActFlag)
        {
            // TODO
            curCube = 1;
            nextCube = 1;

            curHeight = curCube * baseLine;
            nextHeight = nextCube * hypot;

            startRot = transform.rotation;
            targetRot = Quaternion.FromToRotation(dir, _moveDirection.Down) * startRot;
            baseRot = startRot;

            for (int i = 0; i < AnimationFrame; i++)
            {
                if (i == AnimationFrame / 2)
                {
                    /*baseRot = transform.rotation;
                    curHeight = nextHeight;
                    nextHeight = nextCube * baseLine;*/
                }
                transform.rotation = Quaternion.Slerp(startRot, targetRot, (float)(i + 1) / (float)AnimationFrame);
                angle = Quaternion.Angle(baseRot, transform.rotation);
                transform.position += Vector3.up * 
                    DVUtil.GetHeightLine(nextCube * DVConfigs.CUBE_BASE_LENGHT, angle);

                Debug.Log($"angle({angle}), addHeight({DVUtil.GetHeightLine(nextCube * DVConfigs.CUBE_BASE_LENGHT, angle)})");
                yield return DVHelper.In.YieldCache.GetWaitForSeconds(addTime);
            }
        }

        _roll.Acting = false;
    }
    #endregion
}

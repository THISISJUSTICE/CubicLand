using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

        public Vector3[] GetDirections() {
            return new Vector3[] {
                Right,
                Left,
                Up,
                Down,
                Front,
                Back
            };
        }

        public Vector3 GetDirection(DVEnums.Direction direction)
        {
            switch (direction)
            {
                case DVEnums.Direction.FRONT:
                    return Front;
                case DVEnums.Direction.BACK:
                    return Back;
                case DVEnums.Direction.LEFT:
                    return Left;
                case DVEnums.Direction.RIGHT:
                    return Right;
            }

            return Vector3.zero;
        }

        public Vector3 GetDirection(DVEnums.Direction3D direction)
        {
            switch (direction)
            {
                case DVEnums.Direction3D.FRONT:
                case DVEnums.Direction3D.BACK:
                case DVEnums.Direction3D.LEFT:
                case DVEnums.Direction3D.RIGHT:
                    return GetDirection(DVUtil.ConvertDirection3DTo2D(direction));
                case DVEnums.Direction3D.UP:
                    return Up;
                case DVEnums.Direction3D.DOWN:
                    return Down;
            }

            return Vector3.zero;
        }
    }

    protected struct ActingFlag
    {
        public bool Acting;
        public bool ActFlag;

        public ActingFlag(bool on)
        {
            Acting = on;
            ActFlag = on;
        }
    }
    #endregion

    #region Variables
    protected const float MAX_MOVE_TIME = 1f;
    protected const float MIN_MOVE_TIME = 0.1f;

    protected DVGolemCore _golemCore;

    protected MoveDirection _moveDirection;

    protected ActingFlag _move;

    protected Coroutine _chargeCor;
    protected float _chargeHeight;

    protected Vector3 _prevPos;

    protected bool _jumping;

    public Vector3 MoveVelocity { get; private set; }

    protected DVGolemInfo GolemInfo { get => _golemCore.CurrentGolemInfo; }
    protected int AnimationFrame { get => DVPerfomanceConfigs.AnimationFrame; }

    public float MoveTime
    {
        get {
            const float initMoveTime = 0.5f;
            DVStatus status = new DVStatus();
            status.SetInitValue();
            float initMass = DVUtil.GetCubeMass(status.CurrentStatus);
            float weightFactor = Mathf.Pow(_golemCore.rb.mass, 0.2f) / Mathf.Pow(initMass, 0.2f);
            float speedFactor = Mathf.Pow((float)DVStatusConfig.INIT_MOVE_SPEED /(float)_golemCore.GolemInfo.MoveSpeed, 0.3f);
            float moveTime = initMoveTime * weightFactor * speedFactor;

            return Mathf.Clamp(moveTime, MIN_MOVE_TIME, MAX_MOVE_TIME);
        }
    }
    public float RotateTime { get => Mathf.Clamp(MoveTime * 0.9f, MIN_MOVE_TIME, MAX_MOVE_TIME); }

    public int MaxJumpHeight { get => GetGolemHeight() * 3; } // 0.5 height per 1 jumpHeight
    protected float JumpChargeTime { get => (float)GetGolemHeight() * 0.1f; }
    protected float SizeUpTime { get => 0.1f; }
    #endregion

    #region Unity Functions
    protected virtual void Awake()
    {
        _golemCore = GetComponent<DVGolemCore>();
        _golemCore.SetGolemController(this);
        _moveDirection = new MoveDirection(transform.forward, transform.right, transform.up);
        _chargeHeight = 0f;
        _prevPos = transform.position;
        SetInit();
    }

    protected virtual void Start() {
        
        
    }

    protected virtual void OnDestroy()
    {
    }

    protected virtual void FixedUpdate() {
        MoveVelocity = (transform.position - _prevPos) / Time.fixedDeltaTime;
        _prevPos = transform.position;
    }
    #endregion

    #region Public Functions
    public void OnImpulse(Vector3 impulse) {
        _golemCore.rb.CancelVelocity();
        StopAllCoroutines();

        const float maxMove = 10f;
        float power = Mathf.Min(impulse.magnitude, _golemCore.rb.GetMoveForce(maxMove));
        StartCoroutine(KnockbackCor(impulse.normalized * power));
    }
    
    #endregion

    #region Controller
    protected void SetInit() {
        CancelMove();
        _golemCore.SetAttackMode(false);
        UseGravity(true);

        _move.Acting = false;
        _jumping = false;
    }

    protected void CancelMove()
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

    protected void RollGolem(DVEnums.Direction direction)
    {
        if (_move.Acting)
            return;
        _move.ActFlag = true;

        StartCoroutine(RollRightAngleCor(direction, MoveTime));
    }

    protected void MoveJump(DVEnums.Direction direction)
    {
        if (_move.Acting)
            return;
        _move.ActFlag = true;

        float gravity = Mathf.Abs(Physics.gravity.y);
        float velocity = _golemCore.rb.GetUpForce(0.5f) / _golemCore.rb.mass;
        float jumpTime = velocity / gravity * 1.2f;
        StartCoroutine(MoveJumpCor(_moveDirection.GetDirection(direction), jumpTime));
    }

    protected void Rotate(DVEnums.Direction direction)
    {
        if (_move.Acting)
            return;
        _move.ActFlag = true;

        StartCoroutine(RotateRightAngleCor(direction));
    }

    protected void ChargeJumpReady()
    {
        if (_jumping || _move.Acting || _chargeCor != null)
            return;

        CancelChargeCor();

        _chargeCor = StartCoroutine(ResizeDownCor(JumpChargeTime));
    }

    protected void ChargeJumpAction(float keyingTime)
    {
        if (_jumping || _chargeCor == null)
            return;

        int jumpHeight = 1;
        if (keyingTime > JumpChargeTime)
        {
            int add = Mathf.RoundToInt(Mathf.Floor(keyingTime / JumpChargeTime));
            add = Mathf.RoundToInt(Mathf.Clamp(add, 0, MaxJumpHeight - 1));
            jumpHeight += add;
        }

        CancelChargeCor();
        StartCoroutine(ChargeJumpCor(jumpHeight));
    }
    #endregion

    #region Utils
    protected DVEnums.Direction3D ConvertMoveToTransformDirection(DVEnums.Direction3D direction) {
        Vector3[] tDirs = transform.GetDirections();
        Vector3 mDir = _moveDirection.GetDirection(direction);

        return DVUtil.ConvertDirection(tDirs, mDir);
    }

    protected DVEnums.Direction3D ConvertTransformToMoveDirection(DVEnums.Direction3D direction) {
        Vector3[] mDirs = _moveDirection.GetDirections();
        Vector3 tDir = transform.GetDirections()[(int)direction];

        return DVUtil.ConvertDirection(mDirs, tDir);
    }

    protected void UseGravity(bool on)
    {
        _golemCore.rb.useGravity = on;
    }

    protected void UseRigidbody(bool on) { 
        UseGravity(on);
        _golemCore.rb.UseAngular(on);
    }

    protected List<Vector3Int> FindBottomCubes() {
        DVEnums.Direction3D direction = ConvertMoveToTransformDirection(DVEnums.Direction3D.DOWN);
        int bottomHeight = GolemInfo.GetDirectionSize(direction) - 1;
        return GolemInfo.FindEdgeChilds(direction, bottomHeight);
    }

    protected Vector3Int FindRotateAxis(DVEnums.Direction rotDirection)
    {
        var bottomCubes = FindBottomCubes();
        if (bottomCubes == null || bottomCubes.Count <= 0) // Error
            return Vector3Int.zero;

        if (bottomCubes.Count == 1)
            return bottomCubes[0];

        List<Vector3Int> nearestCubes = new List<Vector3Int>();
        float nearestDist = Vector3Int.Distance(Vector3Int.zero, bottomCubes[0]);
        foreach (var cube in bottomCubes)
        {
            float dist = Vector3Int.Distance(Vector3Int.zero, cube);
            if (dist < nearestDist)
            {
                nearestCubes.Clear();
                nearestCubes.Add(cube);
                nearestDist = dist;
            }
            else if (dist == nearestDist)
                nearestCubes.Add(cube);
        }

        if (nearestCubes.Count == 1)
            return nearestCubes[0];

        Dictionary<DVEnums.Direction3D, Vector3Int> dic = new Dictionary<DVEnums.Direction3D, Vector3Int>();
        foreach (var cube in nearestCubes) { 
            Vector3 dir = cube - Vector3Int.zero;
            dir.y = 0f;

            var key = DVUtil.ConvertDirection(_moveDirection.GetDirections(), dir);
            dic[key] = cube;
        }

        DVEnums.Direction3D[] priorities = new DVEnums.Direction3D[] {
            DVEnums.Direction3D.FRONT,
            DVEnums.Direction3D.RIGHT,
            DVEnums.Direction3D.BACK,
            DVEnums.Direction3D.LEFT
        };
        if (rotDirection == DVEnums.Direction.LEFT) {
            priorities = new DVEnums.Direction3D[] {
                DVEnums.Direction3D.BACK,
                DVEnums.Direction3D.LEFT,
                DVEnums.Direction3D.RIGHT,
                DVEnums.Direction3D.FRONT
            };
        }
        for (int i = 0; i < priorities.Length; i++) { 
            if(dic.ContainsKey(priorities[i]))
                return dic[priorities[i]];
        }

        return Vector3Int.zero;
    }

    protected float CalculateRotateTime() {
        const float slow = 1.03f;
        int bottomCount = FindBottomCubes().Count;
        float rotateTime = Mathf.Max(RotateTime, RotateTime * (slow * (float)(bottomCount - 1)));
        rotateTime = Mathf.Clamp(rotateTime, MIN_MOVE_TIME, MAX_MOVE_TIME);
        return rotateTime;
    }

    protected int GetGolemHeight() { 
        return GolemInfo.GetDirectionSize(ConvertMoveToTransformDirection(DVEnums.Direction3D.DOWN));
    }
    #endregion

    #region Coroutines
    protected IEnumerator MoveJumpCor(Vector3 dir, float time) {
        _move.Acting = true;
        if (!_jumping)
            UseGravity(false);

        Vector3 prevPos = transform.position.NormalizeCube();
        Vector3 targetPos = (prevPos + dir.normalized * DVConfigs.CUBE_BASE_LENGHT).NormalizeCube();
        Vector3 moveDir = targetPos - prevPos;
        Vector3 addMove = moveDir / (float)AnimationFrame;
        float addTime = time / (float)AnimationFrame;

        float startHeight = prevPos.y;
        float moveHeight = DVConfigs.CUBE_BASE_LENGHT * 0.5f + startHeight;
        float addHeight;
        int halfFrame = AnimationFrame / 2;
        int halfIndex;

        while (_move.ActFlag)
        {
            for (int i = 0; i < AnimationFrame; i++)
            {
                transform.position += addMove;

                if (!_jumping)
                {

                    if (i < AnimationFrame / 2)
                    {
                        addHeight = Mathf.Lerp(startHeight, moveHeight, DVUtil.GetEaseOut((float)(i + 1) / (float)halfFrame)) -
                        Mathf.Lerp(startHeight, moveHeight, DVUtil.GetEaseOut((float)i / (float)halfFrame));
                    }
                    else {
                        halfIndex = i % halfFrame;
                        addHeight = Mathf.Lerp(startHeight, moveHeight, DVUtil.GetEaseOut((float)(halfFrame - halfIndex - 1) / (float)halfFrame)) -
                        Mathf.Lerp(startHeight, moveHeight, DVUtil.GetEaseOut((float)(halfFrame - halfIndex) / (float)halfFrame));
                    }

                    transform.position += Vector3.up * addHeight;
                }

                yield return DVHelper.In.YieldCache.GetWaitForSeconds(addTime);
            }

            transform.position = transform.position.NormalizeCube();
        }

        if (!_jumping)
            SetInit();
    }

    protected IEnumerator RollRightAngleCor(DVEnums.Direction direction, float time)
    {
        _move.Acting = true;
        _golemCore.SetAttackMode(true);
        if (!_jumping)
            UseGravity(false);

        Vector3 dir = _moveDirection.GetDirection(direction);
        Vector3 moveDir = -dir.normalized;
        float addTime = time / (float)AnimationFrame;
        float halfLine = DVConfigs.CUBE_BASE_LENGHT / 2f;

        Quaternion startRot, targetRot;
        int curCube, nextCube;
        float curHeight, nextHeight, angle, prevAngle;
        float rollHypot, rollAxisAngle;
        
        while (_move.ActFlag)
        {
            curCube = GetGolemHeight();
            nextCube = GolemInfo.GetDirectionSize(ConvertMoveToTransformDirection(DVUtil.ConvertDirection2DTo3D(direction)));

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

            transform.position = transform.position.NormalizeCube();
        }

        if (!_jumping)
            SetInit();
    }

    protected IEnumerator RotateRightAngleCor(DVEnums.Direction direction)
    {
        _move.Acting = true;
        _golemCore.SetAttackMode(true);

        float time = CalculateRotateTime();
        float addTime = time / (float)AnimationFrame;
        DVGolemCube axisCube = _golemCore.FindCube(FindRotateAxis(direction));

        Vector3 dir, axisPos, addPos;
        Quaternion startRot, targetRot, rot;

        while (_move.ActFlag)
        {
            startRot = transform.rotation;
            dir = _moveDirection.GetDirection(direction);
            axisPos = axisCube.transform.position;
            axisPos.y = 0f;
            rot = Quaternion.FromToRotation(_moveDirection.Front, dir);
            targetRot = rot * startRot;

            for (int i = 0; i < AnimationFrame; i++)
            {
                transform.rotation = Quaternion.Slerp(startRot, targetRot, (float)(i + 1) / (float)AnimationFrame);
                addPos = axisCube.transform.position;
                addPos.y = 0f;
                transform.position += axisPos - addPos;
                yield return DVHelper.In.YieldCache.GetWaitForSeconds(addTime);
            }

            _moveDirection.Rotate(rot.eulerAngles);
        }

        if (!_jumping)
            SetInit();
    }

    protected IEnumerator ResizeDownCor(float chargeTime)
    {
        _move.Acting = true;

        float sizeLength = (float)(GetGolemHeight() - 1)
            * DVConfigs.CUBE_BASE_LENGHT;
        float stdTime = chargeTime / (float)AnimationFrame * 2f;
        float addSize = 0.5f / (float)MaxJumpHeight / (float)AnimationFrame * 2f;
        float addHeight = (0.5f + sizeLength) * addSize;

        Vector3[] tDirs = new Vector3[] {
            transform.right, -transform.right,
            transform.up, -transform.up,
            transform.forward, -transform.forward,
        };
        Vector3[] scaleDirs = new Vector3[] { Vector3.right, Vector3.up, Vector3.forward };

        DVUtil.GetClosestAxisVector(tDirs, Vector3.up, out int index);

        for (int i = 0; i < MaxJumpHeight * AnimationFrame / 2; i++)
        {
            transform.localScale -= scaleDirs[index / 2] * addSize;
            transform.position -= Vector3.up * addHeight;
            _chargeHeight += addHeight;
            yield return DVHelper.In.YieldCache.GetWaitForSeconds(stdTime);
        }

        while (true)
            yield return DVHelper.In.YieldCache.GetWaitForSeconds(10f);
    }

    protected IEnumerator ResizeUpCor(float time)
    {
        int halfFrame = AnimationFrame / 2;
        int chargeFrame = halfFrame / 4 - halfFrame % 4;
        float addTime = time / (float)chargeFrame;

        Vector3 chargeScale = (Vector3.one - transform.localScale).Abs() / (float)chargeFrame;
        float chargeHeight = _chargeHeight / (float)chargeFrame;

        _chargeHeight = 0f;

        for (int i = 0; i < chargeFrame; i++)
        {
            transform.position += Vector3.up * chargeHeight;
            transform.localScale = (transform.localScale + chargeScale).Clamp(0, 1f);
            yield return DVHelper.In.YieldCache.GetWaitForSeconds(addTime);
        }
    }

    protected IEnumerator ChargeJumpCor(int jumpHeight) {
        UseGravity(true);
        _move.Acting = false;
        _golemCore.SetAttackMode(true);

        _jumping = true;
        _golemCore.rb.AddForce(Vector3.up * _golemCore.rb.GetUpForce((float)jumpHeight / 2f), ForceMode.Impulse);

        yield return StartCoroutine(ResizeUpCor(SizeUpTime));
        _move.ActFlag = false;
        _move.Acting = true;

        while (_golemCore.rb.linearVelocity.y > 0f)
            yield return null;

        while(_golemCore.rb.CheckGravity())
            yield return null;

        SetInit();
    }

    protected IEnumerator KnockbackCor(Vector3 impulse) {
        UseRigidbody(true);
        _move.Acting = true;

        Vector3 prevPos = transform.position;

        _golemCore.rb.ImpulseCube(impulse);
        float waitTime = Mathf.Min(_golemCore.rb.GetMoveTimeFromImpulse(impulse), 0.1f);

        StartCoroutine(NormalizeSizeCor(SizeUpTime));
        float rotateTime = RotateTime / 2f;
        StartCoroutine(NormalizeRotationCor(rotateTime));
        float restTime = Mathf.Max(rotateTime, SizeUpTime);

        if (waitTime > 0f)
        {
            yield return DVHelper.In.YieldCache.GetWaitForSeconds(waitTime);
            restTime -= waitTime;
        }

        _golemCore.rb.CancelVelocity();
        UseRigidbody(false);
        UseGravity(true);

        float moveDist = DVUtil.GetDistanceXZ(transform.position, prevPos);
        Vector3 normalizePos = transform.position.NormalizeCube();
        float normalizeDist = DVUtil.GetDistanceXZ(transform.position, normalizePos);

        if (normalizeDist > 0f)
        {
            const float moveTime = DVConfigs.MAX_CUBE_NOMALIZE_TIME;
            float normalizeTime = moveTime * normalizeDist;
            if (moveDist > 0f && waitTime > 0f) {
                float timeUnit = waitTime / moveDist;
                timeUnit = Mathf.Min(moveTime, timeUnit);
                normalizeTime = timeUnit * normalizeDist;
            }

            yield return StartCoroutine(DVUtil.NormalizePositionCor(transform, transform.position, normalizeTime));
            restTime -= normalizeTime;
        }

        while (_golemCore.rb.CheckGravity()) {
            yield return null;
            if (restTime > 0f)
                restTime -= Time.deltaTime;
        }

        if(restTime > 0f)
            yield return DVHelper.In.YieldCache.GetWaitForSeconds(restTime);

        SetInit();
    }

    protected IEnumerator NormalizeSizeCor(float time) {
        Vector3 chargeScale = (Vector3.one - transform.localScale).Abs();
        float sizeRate = chargeScale.magnitude;
        if (sizeRate > 0f) {
            int halfFrame = AnimationFrame / 2;
            int chargeFrame = halfFrame / 4 - halfFrame % 4;
            float addTime = time / (float)chargeFrame;
            float sizeLength = (float)(GetGolemHeight() - 1)
            * DVConfigs.CUBE_BASE_LENGHT;

            float force = _golemCore.rb.GetUpForce(sizeLength * sizeRate);
            _golemCore.rb.AddForce(Vector3.up * force, ForceMode.Impulse);

            for (int i = 0; i < chargeFrame; i++)
            {
                transform.localScale = (transform.localScale + chargeScale).Clamp(0, 1f);
                yield return DVHelper.In.YieldCache.GetWaitForSeconds(addTime);
            }
        }
    }

    protected IEnumerator NormalizeRotationCor(float time)
    {
        Quaternion prevRot = transform.rotation;
        Quaternion normalRot = transform.rotation.NormalizeCube();
        Quaternion deltaRot = normalRot * Quaternion.Inverse(prevRot);
        deltaRot.ToAngleAxis(out float angle, out Vector3 axis);

        if (angle <= 0f)
            yield break;

        yield return StartCoroutine(DVUtil.NormalizeRotationCor(transform, prevRot, time));
    }
    #endregion
}

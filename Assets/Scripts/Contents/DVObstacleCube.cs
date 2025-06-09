using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DVObstacleCube : DVCubeBase
{
    #region Variables
    private Rigidbody _rb;

    public Vector3 Velocity { get => _rb.linearVelocity; }
    #endregion

    #region Unity Functions
    protected override void Awake()
    {
        base.Awake();
        _rb = GetComponent<Rigidbody>();
    }

    protected override void Start()
    {
        base.Start();
        _rb.Reset();
        _rb.UseAngular(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null)
            return;

        if (collision.gameObject.tag == "Map")
        {
            OnDamaged(_rb.mass, collision.impulse);
            return;
        }

        // Golem Collision 시 GolemCore에서 충돌 호출
        var core = collision.gameObject.GetComponent<DVGolemCore>();
        if (core != null)
            return;

        // Obstacle Collision
        var obstacle = collision.gameObject.GetComponent<DVObstacleCube>();
        if (obstacle != null) {
            // 충격량 계산
            DVHelper.Instance.WaitFrameAct(1, () => OnDamaged(_rb.mass, collision.impulse, obstacle.CubeInfo));
        }

        // TODO: Skill Collision

        /*DVGolemCube colCube = collision.gameObject.GetComponent<DVGolemCube>();
        if (colCube == null)
            return;

        DVCubeInfo cubeInfo = colCube.CubeInfo;
        StartCoroutine(DVHelper.In.WaitFrameActCor(1, () => OnDamaged(cubeInfo, colCube)));*/
    }
    #endregion

    #region Public Functions
    public void OnDamaged(Vector3 impulse, DVCubeInfo colCubeInfo) {
        OnDamaged(_rb.mass, impulse, colCubeInfo, out float damageRate);
    }

    public override void OnDamaged(float selfMass, Vector3 impulse, DVCubeInfo? colCubeInfo, out float damageRate)
    {
        base.OnDamaged(selfMass, impulse, colCubeInfo, out damageRate);
        _rb.mass = CubeMass;

        if (this.Usable())
        {
            StopAllCoroutines();
            float sign = impulse.y > 0f ? 1f : -1f;
            impulse.y = Mathf.Min(0.01f, Mathf.Abs(impulse.y)) * sign;
            StartCoroutine(KnockbackCor(impulse));
        }
    }

    public void NormalizeTransform()
    {
        const float time = DVConfigs.MAX_CUBE_NOMALIZE_TIME;
        StartCoroutine(DVUtil.NormalizePositionCor(transform, transform.position, time));
        StartCoroutine(DVUtil.NormalizeRotationCor(transform, transform.rotation, time));
    }
    #endregion

    #region Coroutines
    private IEnumerator KnockbackCor(Vector3 impulse) {
        Vector3 prevPos = transform.position;
        _rb.ImpulseCube(impulse);
        float waitTime = Mathf.Min(_rb.GetMoveTimeFromImpulse(impulse), 0.4f);

        if (waitTime > 0f)
            yield return DVHelper.YieldCache.GetWaitForSeconds(waitTime);
        _rb.CancelVelocity();

        float moveDist = DVUtil.GetDistanceXZ(transform.position, prevPos);
        Vector3 normalizePos = transform.position.NormalizeCube();
        float normalizeDist = DVUtil.GetDistanceXZ(transform.position, normalizePos);

        if (normalizeDist > 0f)
        {
            const float moveTime = DVConfigs.MAX_CUBE_NOMALIZE_TIME;
            float normalizeTime = moveTime * normalizeDist;
            if (moveDist > 0f && waitTime > 0f)
            {
                float timeUnit = waitTime / moveDist;
                timeUnit = Mathf.Min(moveTime, timeUnit);
                normalizeTime = timeUnit * normalizeDist;
            }

            yield return StartCoroutine(DVUtil.NormalizePositionCor(transform, transform.position, normalizeTime));
        }
    }
    #endregion

    #region Utils
    private void OnDamaged(float selfMass, Vector3 impulse)
    {
        OnDamaged(selfMass, impulse, null, out float damageRate);
    }
    #endregion
}

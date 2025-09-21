using UnityEngine;
using System.Collections;

namespace CustomTIJI.CubicLand
{
    public class ObstacleCube : CubeBase
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

            // Golem Collision 시 Core 쪽에서 충돌 호출
            var core = collision.gameObject.GetComponent<CubeCore<CubeBase>>();
            if (core != null)
                return;

            // Obstacle Collision
            var cube = collision.gameObject.GetComponent<CubeBase>();
            if (cube != null)
            {
                // 충격량 계산
                this.WaitFrameAct(1, () => OnDamaged(_rb.mass, collision.impulse, cube.CubeInfo));
            }

            // TODO: Skill Collision

        }
        #endregion

        #region Public Functions
        public void OnDamaged(Vector3 impulse, CubeInfo colCubeInfo)
        {
            OnDamaged(_rb.mass, impulse, colCubeInfo, out float damageRate);
        }

        public override void OnDamaged(float selfMass, Vector3 impulse, CubeInfo? colCubeInfo, out float damageRate)
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
            const float time = Configs.MAX_CUBE_NOMALIZE_TIME;
            StartCoroutine(Utils.NormalizePositionCor(transform, transform.position, time));
            StartCoroutine(Utils.NormalizeRotationCor(transform, transform.rotation, time));
        }
        #endregion

        #region Coroutines
        private IEnumerator KnockbackCor(Vector3 impulse)
        {
            Vector3 prevPos = transform.position;
            _rb.ImpulseCube(impulse);
            float waitTime = Mathf.Min(_rb.GetMoveTimeFromImpulse(impulse), 0.4f);

            if (waitTime > 0f)
                yield return Helper.YieldCache.GetWaitForSeconds(waitTime);
            _rb.CancelVelocity();

            float moveDist = Utils.GetDistanceXZ(transform.position, prevPos);
            Vector3 normalizePos = transform.position.NormalizeCube();
            float normalizeDist = Utils.GetDistanceXZ(transform.position, normalizePos);

            if (normalizeDist > 0f)
            {
                const float moveTime = Configs.MAX_CUBE_NOMALIZE_TIME;
                float normalizeTime = moveTime * normalizeDist;
                if (moveDist > 0f && waitTime > 0f)
                {
                    float timeUnit = waitTime / moveDist;
                    timeUnit = Mathf.Min(moveTime, timeUnit);
                    normalizeTime = timeUnit * normalizeDist;
                }

                yield return StartCoroutine(Utils.NormalizePositionCor(transform, transform.position, normalizeTime));
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
}
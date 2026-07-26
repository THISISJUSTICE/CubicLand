using System.Collections;
using UnityEngine;

namespace Commar.CubicLand.Cube
{
    [RequireComponent(typeof(CubeObject), typeof(Rigidbody))]
    public class ObstacleCube : MonoBehaviour, ICubeMotionAdjuster
    {
        private ICubeCollisionResolver _cubeCollisionResolver;
        private Rigidbody _rigidbody;

        public CubeObject Cube { get; private set; }

        private void Awake()
        {
            Cube = GetComponent<CubeObject>();
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            _rigidbody.FreezePositionXZ(true);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision == null)
                return;

            _cubeCollisionResolver.OnCollision(gameObject, this, collision, false);
        }

        internal void Initialize(ICubeCollisionResolver cubeCollisionResolver)
        {
            _cubeCollisionResolver = cubeCollisionResolver;
        }

        public void ApplyKnockback(Vector3 impulse)
        {
            impulse = CubeUtil.GetValidKnockbackImpulse(impulse, Cube.Mass);
            if (impulse.magnitude <= 0f)
                return;

            StopAllCoroutines();
            _rigidbody.FreezePositionXZ(false);
            _rigidbody.AddForce(impulse, ForceMode.Impulse);
            StartCoroutine(HandleKnockback());
        }

        public void NormalizePose()
        {
            _rigidbody.FreezePositionXZ(true);
            CubeUtil.StartNormalizePose(_rigidbody, this);
        }

        private IEnumerator HandleKnockback()
        {
            yield return YieldCache.WaitForFixedUpdate;

            float waitTime = CubeUtil.CalculateKnockbackTime(_rigidbody.linearVelocity, Cube.Mass);
            if (waitTime > 0f)
                yield return GlobalRoot.Instance.YieldCache.GetWaitForSeconds(waitTime);

            _rigidbody.ClearVelocity();
            NormalizePose();
        }
    }
}
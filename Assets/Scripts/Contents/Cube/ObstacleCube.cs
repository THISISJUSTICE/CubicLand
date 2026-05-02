using System.Collections;
using UnityEngine;

namespace CustomTIJI.CubicLand.Cube
{
    [RequireComponent(typeof(CubeObject), typeof(Rigidbody))]
    public class ObstacleCube : MonoBehaviour, ICubeMotionAdjuster
    {
        private ICubeCollisionResolver _cubeCollisionResolver;
        private Rigidbody _rigidbody;

        public CubeObject Cube { get; private set; }
        public Vector3 MoveVelocity => _rigidbody.linearVelocity;

        private void Awake()
        {
            Cube = GetComponent<CubeObject>();
            _rigidbody = GetComponent<Rigidbody>();
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

        public void NormalizeTransform()
        {
            _rigidbody.FreezePositionXZ(true);
            CubeUtil.StartNormalize(_rigidbody, this);
        }

        private IEnumerator HandleKnockback()
        { 
            yield return null;

            float waitTime = CubeUtil.CalculateKnockbackTime(_rigidbody.linearVelocity, Cube.Mass);
            if (waitTime > 0f)
                yield return GlobalRoot.Instance.YieldCache.GetWaitForSeconds(waitTime);

            _rigidbody.ClearVelocity();
            NormalizeTransform();
        }
    }
}
using System.Collections;
using UnityEngine;

namespace CustomTIJI.CubicLand.Cube
{
    internal static class CubeUtil
    {
        public static float CalculateBasicCubeObjectMass()
        {
            return CalcualteCubeObjectMass(new StatusValue(new StatusPoint()));
        }

        public static float CalcualteCubeObjectMass(StatusValue statusValue)
        {
            return CubeConfig.ONE_CUBE_MASS *
                ((float)statusValue.HP / statusValue.MaxHP * statusValue.Armor * CubeConfig.MASS_RATE + 1f);
        }

        public static Vector3 GetValidKnockbackImpulse(Vector3 impulse, float mass)
        {
            float minForce = Utils.CalculateForceForDistance(mass, CubeConfig.MIN_KNOCKBACK_DISTANCE);
            float maxForce = Utils.CalculateForceForDistance(mass, CubeConfig.MAX_KNOCKBACK_DISTANCE);
            Vector2 impulseXZ = Utils.ConvertXZVector2(impulse);
            float xzForce = impulseXZ.magnitude;

            if (xzForce < minForce)
                impulse = Vector3.up * impulse.y;
            else if (xzForce > maxForce)
            {
                impulseXZ = impulseXZ.normalized * maxForce;
                impulse = new Vector3(impulseXZ.x, impulse.y, impulseXZ.y);
            }

            impulse.y = Mathf.Min(impulse.y, CubeConfig.MAX_KNOCKBACK_DISTANCE);

            return impulse;
        }

        public static float CalculateKnockbackTime(Vector3 impulse, float mass)
        {
            float time = Utils.ConvertXZVector2(impulse).magnitude / mass / CubeConfig.KNOCKBACK_DECELERATION;

            return Mathf.Min(CubeConfig.MAX_KNOCKBACK_TIME, time);
        }

        public static float GetMaxAxisAngleDifference(Quaternion a, Quaternion b)
        {
            Vector3 aEuler = a.eulerAngles;
            Vector3 bEuler = b.eulerAngles;

            float deltaX = Mathf.Abs(Mathf.DeltaAngle(aEuler.x, bEuler.x));
            float deltaY = Mathf.Abs(Mathf.DeltaAngle(aEuler.y, bEuler.y));
            float deltaZ = Mathf.Abs(Mathf.DeltaAngle(aEuler.z, bEuler.z));

            return Mathf.Max(deltaX, deltaY, deltaZ);
        }

        public static float CalculateLiftForce(Rigidbody rigidbody, float height)
        {
            float v = Mathf.Sqrt(Mathf.Abs(2f * Physics.gravity.y * height));
            return v * rigidbody.mass;
        }

        #region Normalize
        public static Vector3 GetNormalizedPosition(Vector3 position)
        {
            position.x = Mathf.Round(position.x / CubeConfig.CUBE_BASE_LENGHT) * CubeConfig.CUBE_BASE_LENGHT;
            position.z = Mathf.Round(position.z / CubeConfig.CUBE_BASE_LENGHT) * CubeConfig.CUBE_BASE_LENGHT;

            return position;
        }

        public static Quaternion GetNormalizedRotation(Quaternion rotation)
        {
            Vector3 eulerAngles = rotation.eulerAngles;
            eulerAngles.x = Mathf.Round(eulerAngles.x / 90f) * 90f;
            eulerAngles.y = Mathf.Round(eulerAngles.y / 90f) * 90f;
            eulerAngles.z = Mathf.Round(eulerAngles.z / 90f) * 90f;

            return Quaternion.Euler(eulerAngles);
        }

        public static void StartNormalize(Rigidbody rigidbody, MonoBehaviour actor)
        {
            if (actor != null && actor.IsEnable())
            {
                actor.StartCoroutine(NormalizePosition(rigidbody));
                actor.StartCoroutine(NormalizeRotation(rigidbody));
            }
        }

        public static IEnumerator NormalizePosition(Rigidbody rigidbody)
        {
            Vector3 start = rigidbody.position;
            Vector3 end = GetNormalizedPosition(start);
            float duration = Vector3.Distance(start, end) / CubeConfig.MAX_CUBE_NOMALIZE_DISTANCE * CubeConfig.MAX_CUBE_NOMALIZE_TIME;
            float time = 0f;

            while (time < duration)
            {
                rigidbody.MovePosition(Vector3.Lerp(start, end, time / duration));
                time += Time.fixedDeltaTime;
                yield return YieldCache.WaitForFixedUpdate;
            }
        }

        public static IEnumerator NormalizeRotation(Rigidbody rigidbody)
        {
            const float maxCubeNormalizeAngle = 45f;

            Quaternion start = rigidbody.rotation;
            Quaternion end = GetNormalizedRotation(start);
            float duration = GetMaxAxisAngleDifference(start, end) / maxCubeNormalizeAngle * CubeConfig.MAX_CUBE_NOMALIZE_TIME;
            float time = 0f;

            while (time < duration)
            {
                rigidbody.MoveRotation(Quaternion.Lerp(start, end, time / duration));
                time += Time.fixedDeltaTime;
                yield return YieldCache.WaitForFixedUpdate;
            }
        }
        #endregion
    }
}
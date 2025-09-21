using System;
using System.Collections;
using UnityEngine;
using CustomTIJI.CubicLand;

namespace CustomTIJI
{
    public static class UnityExtensions
    {
        #region Transform
        public static void Reset(this Transform tf)
        {
            tf.transform.localScale = Vector3.one;
            tf.transform.localRotation = Quaternion.identity;
            tf.transform.localPosition = Vector3.zero;
        }

        public static Vector3[] GetDirections(this Transform tf)
        {
            return new Vector3[] {
        tf.right,
        -tf.right,
        tf.up,
        -tf.up,
        tf.forward,
        -tf.forward
    };
        }
        #endregion

        #region Vector
        public static Vector3 Abs(this Vector3 vector)
        {
            return new Vector3(
                Mathf.Abs(vector.x),
                Mathf.Abs(vector.y),
                Mathf.Abs(vector.z)
            );
        }

        public static Vector3 Clamp(this Vector3 vector, float min, float max)
        {
            return new Vector3(
                Mathf.Clamp(vector.x, min, max),
                Mathf.Clamp(vector.y, min, max),
                Mathf.Clamp(vector.z, min, max)
            );
        }

        public static Vector3Int Abs(this Vector3Int vector)
        {
            return new Vector3Int(
                Mathf.RoundToInt(Mathf.Abs(vector.x)),
                Mathf.RoundToInt(Mathf.Abs(vector.y)),
                Mathf.RoundToInt(Mathf.Abs(vector.z))
            );
        }

        public static Vector3 NormalizeCube(this Vector3 pos)
        {
            Vector3 normalPos = pos;
            normalPos.x = Mathf.Round(pos.x / Configs.CUBE_BASE_LENGHT) * Configs.CUBE_BASE_LENGHT;
            normalPos.z = Mathf.Round(pos.z / Configs.CUBE_BASE_LENGHT) * Configs.CUBE_BASE_LENGHT;

            return normalPos;
        }
        #endregion

        #region Quaternion
        public static Quaternion NormalizeCube(this Quaternion quaternion)
        {
            Vector3 eulerAngles = quaternion.eulerAngles;
            eulerAngles.x = Mathf.Round(eulerAngles.x / 90f) * 90f;
            eulerAngles.y = Mathf.Round(eulerAngles.y / 90f) * 90f;
            eulerAngles.z = Mathf.Round(eulerAngles.z / 90f) * 90f;

            return Quaternion.Euler(eulerAngles);
        }
        #endregion

        #region Color
        public static Color Clamp(this Color color, Color min, Color max)
        {
            return new Color(
                Mathf.Clamp(color.r, min.r, max.r),
                Mathf.Clamp(color.g, min.g, max.g),
                Mathf.Clamp(color.b, min.b, max.b),
                Mathf.Clamp(color.a, min.a, max.a)
            );
        }
        #endregion

        #region Rigidbody
        public static void Reset(this Rigidbody rb)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;
        }

        public static void UseOnlyGravity(this Rigidbody rb)
        {
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.FreezePositionX
                    | RigidbodyConstraints.FreezePositionZ
                    | RigidbodyConstraints.FreezeRotation;
        }

        public static void UseLinear(this Rigidbody rb, bool on)
        {
            if (!on)
                rb.constraints |= RigidbodyConstraints.FreezePositionX
                    | RigidbodyConstraints.FreezePositionZ;
            else
                rb.constraints &= ~RigidbodyConstraints.FreezePositionX
                    & ~RigidbodyConstraints.FreezePositionZ;
        }

        public static float GetLinearXZSpeed(this Rigidbody rb)
        {
            return Mathf.Abs(Utils.ConvertXZVector2(rb.linearVelocity).magnitude);
        }

        public static float GetGravitySpeed(this Rigidbody rb)
        {
            return Mathf.Abs(rb.linearVelocity.y);
        }

        public static bool CheckGravity(this Rigidbody rb)
        {
            return rb.GetGravitySpeed() > 0.01f;
        }

        public static float GetAngularSpeed(this Rigidbody rb)
        {
            return Mathf.Abs(rb.angularVelocity.magnitude);
        }

        public static void UseAngular(this Rigidbody rb, bool on)
        {
            if (!on)
                rb.constraints |= RigidbodyConstraints.FreezeRotation;
            else
                rb.constraints &= ~RigidbodyConstraints.FreezeRotation;
        }

        public static float GetUpForce(this Rigidbody rb, float height)
        {
            float v = Mathf.Sqrt(Mathf.Abs(2f * Physics.gravity.y * height));
            return v * rb.mass;
        }

        public static float GetMoveForce(this Rigidbody rb, float distance)
        {
            float v = Mathf.Sqrt(2f * distance);
            return v * rb.mass;
        }

        public static float GetVelocityForce(this Rigidbody rb, float distance, float time)
        {
            if (time <= 0f || distance <= 0f)
                return 0f;

            float velocity = distance / time;
            float force = velocity * rb.mass;

            return force;
        }

        public static void ImpulseCube(this Rigidbody rb, Vector3 impulse)
        {
            float minForce = rb.GetMoveForce(0.7f);
            float xzForce = Utils.ConvertXZVector2(impulse).magnitude;
            if (xzForce < minForce)
            {
                impulse.x = 0f; impulse.z = 0f;
            }

            rb.AddForce(impulse, ForceMode.Impulse);
        }

        public static float GetMoveDistance(this Rigidbody rb, Vector3 impulse, float deceleration = 2f)
        {
            if (rb.mass <= 0f || impulse == Vector3.zero || deceleration <= 0f)
                return 0f;

            float v = impulse.magnitude / rb.mass;
            return (v * v) / (2f * deceleration);
        }

        public static float GetMoveTimeFromImpulse(this Rigidbody rb, Vector3 impulse, float deceleration = 2f)
        {
            if (rb.mass <= 0f || impulse == Vector3.zero || deceleration <= 0f)
                return 0f;

            float v = Utils.ConvertXZVector2(impulse).magnitude / rb.mass;
            return v / deceleration;
        }

        public static void CancelVelocity(this Rigidbody rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        #endregion

        #region MonoBehaviour
        public static bool Usable(this MonoBehaviour mb)
        {
            if (mb == null || !mb.gameObject.activeInHierarchy)
                return false;

            return true;
        }

        public static Coroutine WaitTimeAct(this MonoBehaviour mb, float waitTime, Action callback)
        {
            return mb.StartCoroutine(WaitTimeActCor(waitTime, callback));
        }

        public static Coroutine WaitFrameAct(this MonoBehaviour mb, int frame, Action callback)
        {
            return mb.StartCoroutine(WaitFrameActCor(frame, callback));
        }

        private static IEnumerator WaitTimeActCor(float waitTime, Action callback)
        {
            yield return Helper.YieldCache.GetWaitForSeconds(waitTime);
            callback?.Invoke();
        }

        private static IEnumerator WaitFrameActCor(int frame, Action callback)
        {
            for (int i = 0; i < frame; i++)
                yield return null;
            callback?.Invoke();
        }
        #endregion

        #region String
        public static bool Contains(this string str, params string[] checks)
        {
            bool res = false;
            foreach (string check in checks)
            {
                if (str.Contains(check))
                {
                    res = true;
                    break;
                }
            }

            return res;
        }

        public static Vector3 ParseVector3(this string str)
        {
            int startIndex = str.IndexOf('(');
            if (startIndex == -1)
            {
                Debug.LogError($"잘못된 형식의 문자열입니다.");
                return Vector3.zero;
            }

            string vectorPart = str.Substring(startIndex).Trim('(', ')');

            // 쉼표(,) 기준으로 분리
            string[] parts = vectorPart.Split(',');

            if (parts.Length != 3)
            {
                Debug.LogError($"Vector3는 3개의 요소가 필요합니다.");
                return Vector3.zero;
            }

            // 실수형 변환
            float x = float.Parse(parts[0].Trim());
            float y = float.Parse(parts[1].Trim());
            float z = float.Parse(parts[2].Trim());

            return new Vector3(x, y, z);
        }

        public static string UpperFirst(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return char.ToUpper(str[0]) + str.Substring(1);
        }

        public static bool IsSame(this string str, params string[] checks)
        {
            for (int i = 0; i < checks.Length; i++)
            {
                if (str.Equals(checks[i]))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region Long
        public static string SizeToString(this long size)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            double len = size;
            int order = 0;

            while (len >= 1024d && order < sizes.Length - 1)
            {
                order++;
                len /= 1024d;
            }

            return $"{len:F2}{sizes[order]}";
        }
        #endregion

        #region Cube
        public static void SetupChildCube(this GolemCube childCube, GolemCube parentCube)
        {
            Vector3Int shapePos = childCube.CubeInfo.ShapePosition;

            childCube.name = $"Child_{shapePos.x}_{shapePos.y}_{shapePos.z}";
            childCube.transform.SetParent(parentCube.transform);
            childCube.transform.localPosition = (Vector3)(shapePos - parentCube.CubeInfo.ShapePosition) * Configs.CUBE_BASE_LENGHT;
        }
        #endregion
    }
}
using UnityEngine;

namespace CustomTIJI
{
    public static class UnityExtensions
    {
        public static T GetOrAddComponenet<T>(this GameObject go) where T : Component
        {
            if (go == null)
                return null;

            T componenet = go.GetComponent<T>();
            if (componenet == null)
                componenet = go.AddComponent<T>();

            return componenet;
        }

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
        public static void Abs(this Vector3 vector)
        {
            vector = new Vector3(
                Mathf.Abs(vector.x),
                Mathf.Abs(vector.y),
                Mathf.Abs(vector.z)
            );
        }

        public static void Clamp(this Vector3 vector, float min, float max)
        {
            vector = new Vector3(
                Mathf.Clamp(vector.x, min, max),
                Mathf.Clamp(vector.y, min, max),
                Mathf.Clamp(vector.z, min, max)
            );
        }

        public static void Abs(this Vector3Int vector)
        {
            vector = new Vector3Int(
                Mathf.RoundToInt(Mathf.Abs(vector.x)),
                Mathf.RoundToInt(Mathf.Abs(vector.y)),
                Mathf.RoundToInt(Mathf.Abs(vector.z))
            );
        }
        #endregion

        #region Rigidbody
        public static void FreezePosition(this Rigidbody rigidbody, bool on)
        {
            if (on)
                rigidbody.constraints |= RigidbodyConstraints.FreezePosition;
            else
                rigidbody.constraints &= ~RigidbodyConstraints.FreezePosition;
        }

        public static void FreezePositionXZ(this Rigidbody rigidbody, bool on)
        {
            if (on)
                rigidbody.constraints |= RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            else
                rigidbody.constraints &= ~(RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ);
        }

        public static void FreezeRotation(this Rigidbody rigidbody, bool on)
        {
            if (on)
                rigidbody.constraints |= RigidbodyConstraints.FreezeRotation;
            else
                rigidbody.constraints &= ~RigidbodyConstraints.FreezeRotation;
        }

        public static void ClearVelocity(this Rigidbody rigidbody)
        {
            rigidbody.linearVelocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }
        #endregion

        #region Color
        public static void Clamp(this Color color, Color min, Color max)
        {
            color = new Color(
                Mathf.Clamp(color.r, min.r, max.r),
                Mathf.Clamp(color.g, min.g, max.g),
                Mathf.Clamp(color.b, min.b, max.b),
                Mathf.Clamp(color.a, min.a, max.a)
            );
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
    }
}
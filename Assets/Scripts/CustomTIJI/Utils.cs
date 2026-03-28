using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace CustomTIJI
{
    public static partial class Utils
    {
        #region Math
        public static int GetGCD(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return Mathf.Abs(a);
        }

        public static float GetHypotenuseBH(float baseLine, float heightLine) => Mathf.Sqrt(Mathf.Pow(baseLine, 2) + Mathf.Pow(heightLine, 2));

        public static float GetHeightLineHyA(float hypotenuse, float angle) => hypotenuse * Mathf.Sin(angle * Mathf.Deg2Rad);

        public static float GetBaseLineHyA(float hypotenuse, float angle) => hypotenuse * Mathf.Cos(angle * Mathf.Deg2Rad);

        public static float GetBaseLineHA(float height, float angle) => height / Mathf.Tan(angle * Mathf.Deg2Rad);

        public static float GetAngleBH(float baseLine, float heightLine) => Mathf.Atan2(heightLine, baseLine) * Mathf.Rad2Deg;
        #endregion

        #region Vector
        public static Vector3 GetClosestAxisVector(Vector3[] vectors, Vector3 direction, out int index)
        {
            if (vectors == null || vectors.Length == 0)
            {
                index = -1;
                return Vector3.zero;
            }

            direction = direction.normalized;

            Vector3 closestVector = vectors[0];
            float maxDot = Vector3.Dot(vectors[0].normalized, direction);
            index = 0;

            for (int i = 1; i < vectors.Length; i++)
            {
                float dot = Vector3.Dot(vectors[i].normalized, direction);
                if (dot > maxDot)
                {
                    maxDot = dot;
                    index = i;
                    closestVector = vectors[i];
                }
            }

            return closestVector;
        }

        public static Vector2 ConvertXZVector2(Vector3 vec)
        {
            return new Vector2(vec.x, vec.z);
        }

        public static float GetDistanceXZ(Vector3 vec1, Vector3 vec2)
        {
            return Vector2.Distance(ConvertXZVector2(vec1), ConvertXZVector2(vec2));
        }
        #endregion

        public static float GetEaseOut(float ratio)
        {
            Mathf.Clamp01(ratio);
            return 1f - Mathf.Pow(1f - ratio, 2f);
        }

        public static float CalculateProgressValue(float value, float loadingValue)
        {
            loadingValue = Mathf.Clamp01(loadingValue);

            if (value < loadingValue)
            {
                float diff = loadingValue - value;
                if (diff > 0.2f)
                    value += Time.deltaTime * 0.5f;
                else if (diff > 0.02f)
                    value += Time.deltaTime * 0.25f;
                else
                {
                    if (loadingValue > 0.9f)
                        value += Time.deltaTime * 0.1f;
                    else
                        value += Time.deltaTime * 0.01f;
                }
            }

            return Mathf.Clamp(value, 0f, loadingValue);
        }

        public static async void WaitTimeAct(float waitTime, Action callback)
        {
            await UniTask.WaitForSeconds(waitTime);
            callback?.Invoke();
        }

        public static async void WaitFrameAct(int frame, Action callback)
        {
            for (int i = 0; i < frame; i++)
                await UniTask.NextFrame();
            callback?.Invoke();
        }
    }
}
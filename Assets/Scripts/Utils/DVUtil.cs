using UnityEngine;

public static class DVUtil
{
    public static void ResetTransform(Transform tf)
    {
        tf.transform.localScale = Vector3.one;
        tf.transform.localRotation = Quaternion.identity;
        tf.transform.localPosition = Vector3.zero;
    }

    public static void ResetTransform(GameObject go) { 
        ResetTransform(go.transform);
    }

    public static float CalculateProgressValue(float value, float loadingValue)
    {

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

        return Mathf.Clamp(value, 0f, 1f);
    }

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

    public static float GetHypotenuse(float baseLine, float heightLine) => Mathf.Sqrt(Mathf.Pow(baseLine, 2) + Mathf.Pow(heightLine, 2));

    public static float GetHeightLine(float hypotenuse, float angle) => hypotenuse * Mathf.Sin(angle * Mathf.Deg2Rad);
}

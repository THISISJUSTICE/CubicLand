using UnityEngine;

public static class DVUnityExtensions
{
    public static void Reset(this Transform tf)
    {
        tf.transform.localScale = Vector3.one;
        tf.transform.localRotation = Quaternion.identity;
        tf.transform.localPosition = Vector3.zero;
    }

    public static Vector3 Abs(this Vector3 vector)
    {
        return new Vector3(
            Mathf.Abs(vector.x),
            Mathf.Abs(vector.y),
            Mathf.Abs(vector.z)
        );
    }

    public static Vector3 Clamp(this Vector3 vector, float min, float max) {
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

    public static Color Clamp(this Color color, Color min, Color max) {
        return new Color(
            Mathf.Clamp(color.r, min.r, max.r),
            Mathf.Clamp(color.g, min.g, max.g),
            Mathf.Clamp(color.b, min.b, max.b),
            Mathf.Clamp(color.a, min.a, max.a)
        );
    }

}

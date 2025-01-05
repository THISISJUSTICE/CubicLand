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
            (int)Mathf.Abs(vector.x),
            (int)Mathf.Abs(vector.y),
            (int)Mathf.Abs(vector.z)
        );
    }

}

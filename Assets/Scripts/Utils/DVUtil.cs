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


}

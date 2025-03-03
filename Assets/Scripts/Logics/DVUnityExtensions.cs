using UnityEngine;

public static class DVUnityExtensions
{
    #region Transform
    public static void Reset(this Transform tf)
    {
        tf.transform.localScale = Vector3.one;
        tf.transform.localRotation = Quaternion.identity;
        tf.transform.localPosition = Vector3.zero;
    }

    public static Vector3[] GetDirections(this Transform tf) {
        if (tf == null)
            return null;

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
    #endregion

    #region Color
    public static Color Clamp(this Color color, Color min, Color max) {
        return new Color(
            Mathf.Clamp(color.r, min.r, max.r),
            Mathf.Clamp(color.g, min.g, max.g),
            Mathf.Clamp(color.b, min.b, max.b),
            Mathf.Clamp(color.a, min.a, max.a)
        );
    }
    #endregion

    #region Rigidbody
    public static void Reset(this Rigidbody rb) {
        if (rb == null)
            return;

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;
    }

    public static void SetGolemMass(this Rigidbody rb, DVGolemCore core = null) {
        if (rb == null)
            return;

        if (core == null)
            rb.mass = DVConfigs.OBSTACLE_CUBE_MASS;
        else
            rb.mass = core.CurrentGolemInfo.Shape.Count * DVConfigs.ONE_CUBE_MASS;
    }

    public static void UseOnlyGravity(this Rigidbody rb)
    {
        if (rb == null)
            return;

        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezePositionX
                | RigidbodyConstraints.FreezePositionZ
                | RigidbodyConstraints.FreezeRotation;
    }

    public static void UseLinear(this Rigidbody rb, bool on) {
        if (rb == null)
            return;

        if (!on)
            rb.constraints |= RigidbodyConstraints.FreezePositionX
                | RigidbodyConstraints.FreezePositionZ;
        else
            rb.constraints &= ~RigidbodyConstraints.FreezePositionX
                & ~RigidbodyConstraints.FreezePositionZ;
    }

    public static float GetLinearXZSpeed(this Rigidbody rb) {
        if (rb == null)
            return 0f;

        return Mathf.Abs(DVUtil.ConvertXZVector2(rb.linearVelocity).magnitude);
    }

    public static float GetGravitySpeed(this Rigidbody rb)
    {
        if (rb == null)
            return 0f;

        return Mathf.Abs(rb.linearVelocity.y);
    }

    public static float GetAngularSpeed(this Rigidbody rb)
    {
        if (rb == null)
            return 0f;

        return Mathf.Abs(rb.angularVelocity.magnitude);
    }

    public static void UseAngular(this Rigidbody rb, bool on) {
        if (rb == null)
            return;

        if (!on)
            rb.constraints |= RigidbodyConstraints.FreezeRotation;
        else
            rb.constraints &= ~RigidbodyConstraints.FreezeRotation;
    }

    public static float GetUpForce(this Rigidbody rb, float height) {
        if (rb == null)
            return 0f;

        float v = Mathf.Sqrt(Mathf.Abs(2f * Physics.gravity.y * height));
        return v * rb.mass;
    }
    #endregion
}

using System;
using System.Collections;
using UnityEngine;

public static partial class DVUtil
{
    #region Directions
    public static int GetEnumLength(Type type) {
        if (!type.IsEnum)
        {
            Debug.LogError($"{type} is not Enum");
            return 0;
        }
        return Enum.GetValues(type).Length;
    }

    public static int DirectionLength { get => GetEnumLength(typeof(DVEnums.Direction)); }
    public static int Direction3DLength { get => GetEnumLength(typeof(DVEnums.Direction3D)); }

    public static Vector3Int GetDirection3DValue(DVEnums.Direction3D direction) {
        switch (direction) {
            case DVEnums.Direction3D.RIGHT:
                return Vector3Int.right;
            case DVEnums.Direction3D.LEFT:
                return Vector3Int.left;
            case DVEnums.Direction3D.UP:
                return Vector3Int.up;
            case DVEnums.Direction3D.DOWN:
                return Vector3Int.down;
            case DVEnums.Direction3D.FRONT:
                return Vector3Int.forward;
            case DVEnums.Direction3D.BACK:
                return Vector3Int.back;
            default:
                return Vector3Int.zero;
        }
    }

    public static DVEnums.Direction3D ConvertDirection2DTo3D(DVEnums.Direction direction) {
        switch (direction) {
            default:
            case DVEnums.Direction.RIGHT:
                return DVEnums.Direction3D.RIGHT;
            case DVEnums.Direction.LEFT:
                return DVEnums.Direction3D.LEFT;
            case DVEnums.Direction.FRONT:
                return DVEnums.Direction3D.FRONT;
            case DVEnums.Direction.BACK:
                return DVEnums.Direction3D.BACK;
        }
    }

    public static DVEnums.Direction ConvertDirection3DTo2D(DVEnums.Direction3D direction)
    {
        switch (direction)
        {
            default:
            case DVEnums.Direction3D.RIGHT:
                return DVEnums.Direction.RIGHT;
            case DVEnums.Direction3D.LEFT:
                return DVEnums.Direction.LEFT;
            case DVEnums.Direction3D.FRONT:
                return DVEnums.Direction.FRONT;
            case DVEnums.Direction3D.BACK:
                return DVEnums.Direction.BACK;
        }
    }

    public static DVEnums.Direction ReverseDirection(DVEnums.Direction direction) {
        switch (direction)
        {
            default:
            case DVEnums.Direction.RIGHT:
                return DVEnums.Direction.LEFT;
            case DVEnums.Direction.LEFT:
                return DVEnums.Direction.RIGHT;
            case DVEnums.Direction.FRONT:
                return DVEnums.Direction.BACK;
            case DVEnums.Direction.BACK:
                return DVEnums.Direction.FRONT;
        }
    }

    public static DVEnums.Direction3D ReverseDirection(DVEnums.Direction3D direction) {
        switch (direction)
        {
            default:
            case DVEnums.Direction3D.RIGHT:
                return DVEnums.Direction3D.LEFT;
            case DVEnums.Direction3D.LEFT:
                return DVEnums.Direction3D.RIGHT;
            case DVEnums.Direction3D.UP:
                return DVEnums.Direction3D.DOWN;
            case DVEnums.Direction3D.DOWN:
                return DVEnums.Direction3D.UP;
            case DVEnums.Direction3D.FRONT:
                return DVEnums.Direction3D.BACK;
            case DVEnums.Direction3D.BACK:
                return DVEnums.Direction3D.FRONT;
        }
    }

    public static DVEnums.Direction3D ConvertDirection(Vector3[] dirs, Vector3 dir) {
        GetClosestAxisVector(dirs, dir, out int index);
        return (DVEnums.Direction3D)index;
    }
    #endregion

    #region Cube Transform
    public static IEnumerator NormalizePositionCor(Transform tf, Vector3 prevPos, float time)
    {
        Vector3 normalPos = prevPos.NormalizeCube();

        Vector3 moveDir = normalPos - prevPos;
        Vector3 addMove = moveDir / (float)DVPerfomanceConfigs.AnimationFrame;
        float addTime = time / (float)DVPerfomanceConfigs.AnimationFrame;

        for (int i = 0; i < DVPerfomanceConfigs.AnimationFrame; i++)
        {
            tf.position += addMove;
            yield return DVHelper.In.YieldCache.GetWaitForSeconds(addTime);
        }
    }

    public static IEnumerator NormalizeRotationCor(Transform tf, Quaternion prevRot, float time)
    {
        Quaternion normalRot = prevRot.NormalizeCube();
        float addTime = time / (float)DVPerfomanceConfigs.AnimationFrame;

        for (int i = 0; i < DVPerfomanceConfigs.AnimationFrame; i++)
        {
            tf.rotation = Quaternion.Slerp(prevRot, normalRot, (float)(i + 1) / (float)DVPerfomanceConfigs.AnimationFrame);
            yield return DVHelper.In.YieldCache.GetWaitForSeconds(addTime);
        }
    }
    #endregion

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

    public static float GetHypotenuse(float baseLine, float heightLine) => Mathf.Sqrt(Mathf.Pow(baseLine, 2) + Mathf.Pow(heightLine, 2));

    public static float GetHeightLine(float hypotenuse, float angle) => hypotenuse * Mathf.Sin(angle * Mathf.Deg2Rad);

    public static float GetBaseLine(float hypotenuse, float angle) => hypotenuse * Mathf.Cos(angle * Mathf.Deg2Rad);

    public static float GetAngle(float baseLine, float heightLine) => Mathf.Atan2(heightLine, baseLine) * Mathf.Rad2Deg;
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
        float minAngle = Vector3.Angle(vectors[0].normalized, direction);
        index = 0;

        for (int i = 1; i < vectors.Length; i++)
        {
            float angle = Vector3.Angle(vectors[i].normalized, direction);
            if (angle < minAngle)
            {
                minAngle = angle;
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

    #region Rigidbody
    public static float GetCubeMass(DVCurrentStatus status)
    {
        const float massRate = 0.2f;
        return DVConfigs.ONE_CUBE_MASS *
            (status.HP / status.MaxHP
            * status.Armor * massRate + 1f);
    }

    public static Vector3 EstimateImpulse(Vector3 velocityA, float massA, Vector3 velocityB, float massB, Vector3 normal) 
    {
        Vector3 impulse = Vector3.zero;
        Vector3 relativeVelocity = velocityA - velocityB;
        float velAlong = Vector3.Dot(relativeVelocity, -normal);
        if (velAlong > 0f)
        {
            const float restitution = 0.5f;
            float power = (-(1 + restitution) * velAlong) / (1f / massA + 1f / massB);
            impulse = power * normal;
        }

        return impulse;
    }
    #endregion

    public static float GetEaseOut(float ratio) { 
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

    
}

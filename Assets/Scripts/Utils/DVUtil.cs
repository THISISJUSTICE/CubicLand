using System;
using System.Collections;
using UnityEngine;

public static partial class DVUtil
{
    #region Directions
    public static int GetEnumLength(Type type)
    {
        if (!type.IsEnum)
        {
            Debug.LogError($"{type} is not Enum");
            return 0;
        }
        return Enum.GetValues(type).Length;
    }

    public static int DirectionLength { get => GetEnumLength(typeof(DVEnums.Direction)); }
    public static int Direction3DLength { get => GetEnumLength(typeof(DVEnums.Direction3D)); }

    public static Vector3Int GetDirection3DValue(DVEnums.Direction3D direction)
    {
        switch (direction)
        {
            case DVEnums.Direction3D.Right:
                return Vector3Int.right;
            case DVEnums.Direction3D.Left:
                return Vector3Int.left;
            case DVEnums.Direction3D.Up:
                return Vector3Int.up;
            case DVEnums.Direction3D.Down:
                return Vector3Int.down;
            case DVEnums.Direction3D.Front:
            default:
                return Vector3Int.forward;
            case DVEnums.Direction3D.Back:
                return Vector3Int.back;
        }
    }

    public static DVEnums.Direction3D ConvertDirection2DTo3D(DVEnums.Direction direction)
    {
        switch (direction)
        {
            default:
            case DVEnums.Direction.Right:
                return DVEnums.Direction3D.Right;
            case DVEnums.Direction.Left:
                return DVEnums.Direction3D.Left;
            case DVEnums.Direction.Front:
                return DVEnums.Direction3D.Front;
            case DVEnums.Direction.Back:
                return DVEnums.Direction3D.Back;
        }
    }

    public static DVEnums.Direction ConvertDirection3DTo2D(DVEnums.Direction3D direction)
    {
        switch (direction)
        {
            default:
            case DVEnums.Direction3D.Right:
                return DVEnums.Direction.Right;
            case DVEnums.Direction3D.Left:
                return DVEnums.Direction.Left;
            case DVEnums.Direction3D.Front:
                return DVEnums.Direction.Front;
            case DVEnums.Direction3D.Back:
                return DVEnums.Direction.Back;
        }
    }

    public static DVEnums.Direction ReverseDirection(DVEnums.Direction direction)
    {
        switch (direction)
        {
            default:
            case DVEnums.Direction.Right:
                return DVEnums.Direction.Left;
            case DVEnums.Direction.Left:
                return DVEnums.Direction.Right;
            case DVEnums.Direction.Front:
                return DVEnums.Direction.Back;
            case DVEnums.Direction.Back:
                return DVEnums.Direction.Front;
        }
    }

    public static DVEnums.Direction3D ReverseDirection(DVEnums.Direction3D direction)
    {
        switch (direction)
        {
            default:
            case DVEnums.Direction3D.Right:
                return DVEnums.Direction3D.Left;
            case DVEnums.Direction3D.Left:
                return DVEnums.Direction3D.Right;
            case DVEnums.Direction3D.Up:
                return DVEnums.Direction3D.Down;
            case DVEnums.Direction3D.Down:
                return DVEnums.Direction3D.Up;
            case DVEnums.Direction3D.Front:
                return DVEnums.Direction3D.Back;
            case DVEnums.Direction3D.Back:
                return DVEnums.Direction3D.Front;
        }
    }

    public static DVEnums.Direction3D ConvertDirection(Vector3[] dirs, Vector3 dir)
    {
        GetClosestAxisVector(dirs, dir, out int index);
        return (DVEnums.Direction3D)index;
    }

    public static DVEnums.Direction3D ConvertDirection(Vector3 dir)
    {
        Vector3[] dirs = new Vector3[] {
            Vector3.right,
            Vector3.left,
            Vector3.up,
            Vector3.down,
            Vector3.forward,
            Vector3.back
        };
        return ConvertDirection(dirs, dir);
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
            yield return DVHelper.YieldCache.GetWaitForSeconds(addTime);
        }
    }

    public static IEnumerator NormalizeRotationCor(Transform tf, Quaternion prevRot, float time)
    {
        Quaternion normalRot = prevRot.NormalizeCube();
        float addTime = time / (float)DVPerfomanceConfigs.AnimationFrame;

        for (int i = 0; i < DVPerfomanceConfigs.AnimationFrame; i++)
        {
            tf.rotation = Quaternion.Slerp(prevRot, normalRot, (float)(i + 1) / (float)DVPerfomanceConfigs.AnimationFrame);
            yield return DVHelper.YieldCache.GetWaitForSeconds(addTime);
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


}

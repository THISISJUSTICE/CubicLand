using System;
using UnityEngine;

public static class DVUtil
{
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

    public static int GetEnumLength(Type type) {
        if (!type.IsEnum)
        {
            Debug.LogError($"{type} is not Enum");
            return 0;
        }
        return Enum.GetValues(type).Length;
    }

    public static int DirectionLength { get => GetEnumLength(typeof(DVEnums.Direction));}
    public static int Direction3DLength { get => GetEnumLength(typeof(DVEnums.Direction3D));}

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

    public static float GetEaseOut(float ratio) { 
        Mathf.Clamp01(ratio);
        return 1f - Mathf.Pow(1f - ratio, 2f);
    }

    public static Vector3 GetClosestAxisVector(Vector3[] vectors, Vector3 direction, out int index) {
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
}

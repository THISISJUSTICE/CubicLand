using UnityEngine;

public static class DVConfigs
{
    public const float CUBE_BASE_LENGHT = 1f;

    public static float GetCubeScaledLength(float value) { 
        return CUBE_BASE_LENGHT * value;
    }
}

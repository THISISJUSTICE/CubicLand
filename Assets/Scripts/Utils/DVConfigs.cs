using UnityEngine;

public static class DVConfigs
{
    public const long KB = 1024;
    public const long MB = 1024 * KB;
    public const long GB = 1024 * MB;

    public const int MAX_FRAME_RATE = 120;

    public const float CUBE_BASE_LENGHT = 1f;

    public static float GetCubeScaledLength(float value) { 
        return CUBE_BASE_LENGHT * value;
    }
}

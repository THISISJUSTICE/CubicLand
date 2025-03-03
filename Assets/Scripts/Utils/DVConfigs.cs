using UnityEngine;

public static class DVConfigs
{
    public const long KB = 1024;
    public const long MB = 1024 * KB;
    public const long GB = 1024 * MB;

    public const float CUBE_BASE_LENGHT = 1f;

    public const float ONE_CUBE_MASS = 5f;
    public static float OBSTACLE_CUBE_MASS { get => ONE_CUBE_MASS * 10f; }

    // TODO: 맵 정보 클래스에서 맵 마다 입력
    public const float MAP_HEIGHT = 0.5f;
    public static float CubeBottomHeight { get => MAP_HEIGHT + CUBE_BASE_LENGHT / 2f; }

}

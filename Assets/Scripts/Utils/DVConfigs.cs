using UnityEngine;

public static class DVConfigs
{
    public const long KB = 1024;
    public const long MB = 1024 * KB;
    public const long GB = 1024 * MB;

    public const float CUBE_BASE_LENGHT = 1f;

    public const float ONE_CUBE_MASS = 10f;

    public const float IMPULSE_DAMAGE_RATE = 0.3f;

    public const float MAX_CUBE_NOMALIZE_TIME = 0.2f;

    public static float INIT_CUBE_MASS { get; private set; }

    // TODO: 맵 정보 클래스에서 맵 마다 입력
    public const float MAP_HEIGHT = 0.5f;
    public static float CubeBottomHeight { get => MAP_HEIGHT + CUBE_BASE_LENGHT / 2f; }

    public static string DataPath { get; private set; }

    public static void Setup()
    {
        DVStatus status = new DVStatus();
        status.SetInitValue();
        DVCurrentStatus currentStatus = new DVCurrentStatus();
        currentStatus.SetInitValue(status);
        INIT_CUBE_MASS = DVUtil.GetCubeMass(currentStatus);

        DataPath = Application.persistentDataPath;
    }
}

using UnityEngine;

namespace CustomTIJI.CubicLand.Cube
{
    public static class CubeConfig
    {
        public static readonly Vector3Int CORE_POSITION = Vector3Int.zero;

        public const float CUBE_BASE_LENGHT = 1f;
        public const float ONE_CUBE_MASS = 10f;
        public const float MASS_RATE = 0.2f;

        public const float DAMAGE_SCALING_FACTOR = 6f;
        public const float ADDITIONAL_DAMAGE_IMPULSE_DIVISOR = 0.3f;
        public const float ARMOR_EXPONENTIAL_SCALE = 50f;

        public const float MAX_CUBE_NOMALIZE_TIME = 0.2f;

        public const float COLOR_CHILD_RATE = 1.1f;

        public const float RESTITUTION = 0.5f;

        public const float WEIGHT_EXPONENT = 0.2f;
        public const float SPEED_EXPONENT = 0.3f;

        public static class Status
        {
            public const int INIT_MOVE_SPEED = 10;
            public const int ADD_MOVE_SPEED = 2;

            public const int INIT_HP = 10;
            public const int ADD_HP = 5;

            public const int INIT_ARMOR = 1;
            public const int ADD_ARMOR = 1;

            public const int INIT_ATTACK = 5;
            public const int ADD_ATTACK = 1;
        }
    }
}
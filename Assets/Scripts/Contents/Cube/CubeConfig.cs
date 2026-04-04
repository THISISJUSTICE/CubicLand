namespace CustomTIJI.CubicLand.Cube
{
    public static class CubeConfig
    {
        public const float CUBE_BASE_LENGHT = 1f;

        public const float ONE_CUBE_MASS = 10f;

        public const float IMPULSE_DAMAGE_RATE = 0.3f;

        public const float MAX_CUBE_NOMALIZE_TIME = 0.2f;

        public const float COLOR_CHILD_RATE = 1.1f;

        public const float DAMAGE_IMPULSE_RATE = 6f;

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

            public const float DAMAGE_ARMOR_RATE = 50f;
        }
    }
}

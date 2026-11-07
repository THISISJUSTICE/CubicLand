using UnityEngine;

namespace Commar.CubicLand.Cube
{
    public static class CubeConfig
    {
        public static readonly Vector3Int CORE_POSITION = Vector3Int.zero;
        public const float CUBE_BASE_LENGTH = 1f;

        public static float OneCubeMass { get; private set; } = 10f;
        public static float MassRate { get; private set; } = 0.2f;

        public static float DamageScalingFactor { get; private set; } = 6f;
        public static float AdditionalDamageImpulseDivisor { get; private set; } = 0.3f;
        public static float ArmorExponentialScale { get; private set; } = 50f;

        public static float MaxCubeNomalizeTime { get; private set; } = 0.2f;
        public static float MaxCubeNomalizeDistance { get; private set; } = Mathf.Sqrt(CUBE_BASE_LENGTH * 2f);

        public static float StatusColorRate { get; private set; } = 0.999f;
        public static float GolemColorRate { get; private set; } = 1f - (1f - StatusColorRate) / 3f;

        public static float Restitution { get; private set; } = 0.5f;

        public static float WeightExponent { get; private set; } = 0.2f;
        public static float SpeedExponent { get; private set; } = 0.3f;

        public static float KnockbackDeceleration { get; private set; } = 2f;
        public static float MaxKnockbackTime { get; private set; } = 0.5f;
        public static float MaxKnockbackDistance { get; private set; } = 5f;
        public static float MinKnockbackDistance { get; private set; } = 0.7f;

        public static float GolemInitMoveTime { get; private set; } = 0.4f;
        public static float GolemMaxMoveTime { get; private set; } = 0.8f;
        public static float GolemMinMoveTime { get; private set; } = 0.03f;

        public static float GolemRotateFriction { get; private set; } = 1.03f;
        public static float GolemJumpChargeTime { get; private set; } = 0.01f;
        public static float GolemSizeUpTime { get; private set; } = 0.1f;

        public static class Status
        {
            public const float CHILD_REDUCE_RATE = 0.7f;

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
using UnityEngine;

namespace Commar.CubicLand.Cube
{
    public class CubeData
    {
        public StatusPoint StatusPoint { get; protected set; }
        public StatusValue StatusValue { get; protected set; }

        public Color Color { get; protected set; }

        public CubeData(StatusPoint statusPoint, Color color)
        {
            StatusPoint = statusPoint;
            StatusValue = new StatusValue(statusPoint);
            Color = color;
        }

        public virtual CubeData Clone()
        {
            CubeData clone = new CubeData(StatusPoint, Color);
            clone.StatusValue = StatusValue.Clone();
            return clone;
        }

        internal void EnhanceStatus(StatusPoint statusPoint)
        {
            StatusPoint = statusPoint;
            if (StatusValue.HP > 0)
                StatusValue.EnhanceStatus(statusPoint);
            else
                StatusValue = new StatusValue(statusPoint);
        }

        internal void ApplyDamage(float selfMass, Vector3 impulse, CubeData collider, float extraDamage)
        {
            int damage = 1;
            if (collider != null)
                damage = Mathf.RoundToInt((float)collider.StatusValue.HP / collider.StatusValue.MaxHP
                   * collider.StatusValue.Armor + extraDamage);

            ApplyDamage(damage, selfMass, impulse);
        }

        internal void ApplyDamage(float selfMass, Vector3 impulse)
        {
            ApplyDamage(1, selfMass, impulse);
        }

        private void ApplyDamage(int rawDamage, float selfMass, Vector3 impulse)
        {
            // 충격량에 따라 데미지 비율 변경
            float damage = Mathf.Round(rawDamage *
                Mathf.Clamp01(impulse.magnitude / CubeConfig.DAMAGE_SCALING_FACTOR));

            // 물리 충격량에 따른 추가 데미지 반영
            // 기본 데미지의 2배를 넘지 못하도록 제한
            damage += Mathf.Min(rawDamage * 2f,
                impulse.magnitude / selfMass * CubeConfig.ADDITIONAL_DAMAGE_IMPULSE_DIVISOR);

            // 방어 스텟에 따른 데미지 감소
            damage *= Mathf.Exp(-StatusValue.Armor / CubeConfig.ARMOR_EXPONENTIAL_SCALE);

            // 최소 데미지 제한
            // TODO: 최소 데미지 제한을 0으로 해도 문제 없는지 확인 필요
            damage = Mathf.Max(damage, 1f);

            // 데미지 반영
            StatusValue.ApplyDamage(Mathf.RoundToInt(damage));
        }
    }

    public class GolemCubeData : CubeData
    {
        public readonly Vector3Int shapePoisition;

        public bool IsAttackMode { get; internal set; }
        public bool IsBreaked { get; internal set; }

        public GolemCubeData(StatusPoint statusPoint, Color color, Vector3Int shapePoisition, bool isBreaked = false)
            : base(statusPoint, color)
        {
            this.shapePoisition = shapePoisition;
            IsAttackMode = false;
            IsBreaked = isBreaked;
        }

        public new GolemCubeData Clone()
        {
            GolemCubeData clone = new GolemCubeData(StatusPoint, Color, shapePoisition, IsBreaked);
            clone.StatusValue = StatusValue.Clone();
            clone.IsAttackMode = IsAttackMode;
            return clone;
        }

        public GolemCubeData MakeChildData(Vector3Int shapePosition)
        {
            Color color = Color * CubeConfig.COLOR_CHILD_RATE;
            color = color.Clamp(color, Color.white);

            return new GolemCubeData(StatusPoint.MakeChildStatus(), color, shapePosition, IsBreaked);
        }

        public float CalculateExtraDamage()
        {
            return IsAttackMode ? StatusValue.Attack : 0f;
        }

        internal void EnhanceChildStatus(CubeData parent)
        {
            StatusPoint = parent.StatusPoint.MakeChildStatus();
            StatusValue.EnhanceStatus(StatusPoint, !IsBreaked);

            Color color = parent.Color * CubeConfig.COLOR_CHILD_RATE;
            color = color.Clamp(color, Color.white);
            Color = color;
        }
    }
}
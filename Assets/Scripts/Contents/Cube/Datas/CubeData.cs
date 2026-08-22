using UnityEngine;

namespace Commar.CubicLand.Cube
{
    public class CubeData
    {
        public StatusPoint StatusPoint { get; private set; }
        public StatusValue StatusValue { get; private set; }
        public Vector3Int ShapePoisition { get; private set; }
        public bool IsAttackMode { get; internal set; }
        public Color Color { get; private set; }
        public bool IsBreaked { get; internal set; }

        public CubeData(StatusPoint statusPoint, Vector3Int shapePosition, Color color, bool isBreaked = false)
        {
            StatusPoint = statusPoint;
            StatusValue = new StatusValue(statusPoint);
            ShapePoisition = shapePosition;
            IsAttackMode = false;
            Color = color;
            IsBreaked = isBreaked;
        }

        public CubeData Clone()
        {
            CubeData clone = new CubeData(StatusPoint, ShapePoisition, Color, IsBreaked);
            clone.StatusValue = StatusValue.Clone();
            return clone;
        }

        public CubeData MakeChildData(Vector3Int shapePosition)
        {
            Color color = Color * CubeConfig.COLOR_CHILD_RATE;
            color = color.Clamp(color, Color.white);

            return new CubeData(StatusPoint.MakeChildStatus(), shapePosition, color, IsBreaked);
        }

        internal void EnhanceStatus(StatusPoint statusPoint)
        {
            StatusPoint = statusPoint;
            if (StatusValue.HP > 0)
                StatusValue.EnhanceStatus(statusPoint);
            else
                StatusValue = new StatusValue(statusPoint);
        }

        internal void EnhanceChildStatus(CubeData parent)
        {
            StatusPoint = parent.StatusPoint.MakeChildStatus();
            StatusValue.EnhanceStatus(StatusPoint);

            Color color = parent.Color * CubeConfig.COLOR_CHILD_RATE;
            color = color.Clamp(color, Color.white);
            Color = color;
        }

        internal void ApplyDamage(float selfMass, Vector3 impulse, CubeData collider)
        {
            int damage = 1;
            if (collider != null)
                damage = Mathf.RoundToInt((float)collider.StatusValue.HP / collider.StatusValue.MaxHP
                   * collider.StatusValue.Armor
                   + (collider.IsAttackMode ? collider.StatusValue.Attack : 0f));

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
}
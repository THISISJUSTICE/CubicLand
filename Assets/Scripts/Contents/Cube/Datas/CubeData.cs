using UnityEngine;

namespace CustomTIJI.CubicLand.Cube
{
    public class CubeData
    {
        public StatusPoint StatusPoint { get; private set; }
        public StatusValue StatusValue { get; private set; }
        public Vector3Int ShapePoisition { get; private set; }
        public bool IsAttackMode { get; internal set; }
        public Color Color { get; private set; }

        public CubeData(StatusPoint statusPoint, Vector3Int shapePosition, Color color)
        {
            StatusPoint = statusPoint;
            StatusValue = new StatusValue();
            StatusValue.Initialize(statusPoint);
            ShapePoisition = shapePosition;
            IsAttackMode = false;
            Color = color;
        }

        public CubeData Copy()
        {
            return new CubeData(StatusPoint, ShapePoisition, Color);
        }

        public CubeData MakeChildData(Vector3Int shapePosition)
        {
            Color color = Color * CubeConfig.COLOR_CHILD_RATE;
            color.Clamp(color, Color.white);

            return new CubeData(StatusPoint.MakeChildStatus(), shapePosition, color);
        }

        public void EnhanceStatus(StatusPoint statusPoint)
        {
            StatusPoint = statusPoint;
            StatusValue.EnhanceStatus(statusPoint);
        }

        public void OnDamaged(float selfMass, Vector3 impulse, CubeData collider)
        {
            int damage = Mathf.RoundToInt((float)collider.StatusValue.HP / collider.StatusValue.MaxHP
                * collider.StatusValue.Armor
                + (collider.IsAttackMode ? collider.StatusValue.Attack : 0f));

            StatusValue.OnDamaged(damage, selfMass, impulse);
        }

        public void OnDamaged(float selfMass, Vector3 impulse)
        {
            StatusValue.OnDamaged(1, selfMass, impulse);
        }
    }
}
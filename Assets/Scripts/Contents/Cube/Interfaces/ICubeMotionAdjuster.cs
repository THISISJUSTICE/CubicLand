using UnityEngine;

namespace CustomTIJI.CubicLand.Cube
{
    public interface ICubeMotionAdjuster
    {
        public Vector3 MoveVelocity { get; }
        public void ApplyKnockback(Vector3 impulse);
        public void NormalizeTransform();
    }
}
using UnityEngine;

namespace CustomTIJI.CubicLand.Cube
{
    public interface IGolemObject
    {
        internal Rigidbody Rigidbody { get; }
        public GolemData GolemData { get; }

        internal void SetUnityRoutine(IOnEnablable onEnablable);
        internal void SetUnityRoutine(IFixedUpdatable fixedUpdatable);
        internal void SetAttackMode(bool attackMode);

        public float CalculateMoveTime(float initTime, float minTime, float maxTime);
        public CubeObject FindCube(Vector3Int position);
    }
}
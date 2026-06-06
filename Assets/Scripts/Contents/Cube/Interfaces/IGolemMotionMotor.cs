using System.Collections;
using UnityEngine;

namespace CustomTIJI.CubicLand.Cube
{
    public interface IGolemMotionMotor
    {
        public IEnumerator MoveRollGolem(Enums.Direction direction);
        public IEnumerator MoveJumpGolem(Enums.Direction direction);
        public IEnumerator MoveGolem(Enums.Direction direction);
        public IEnumerator RotateGolem(Enums.Direction direction);
        public IEnumerator ChargeJumpReadyGolem();
        public IEnumerator ChargeJumpActionGolem();
    }

    public interface IGolemGeometryProvider
    {
        public CubeObject RotateAxisCube { get; }
        public CubeObject UpEdgeCube { get; }
        public CubeObject BackEdgeCube { get; }
        public CubeObject FrontEdgeCube { get; }

        public int GolemWidth { get; }
        public int GolemHeight { get; }
        public int GolemBack { get; }

        public Quaternion ViewRotation { get; }
    }
}
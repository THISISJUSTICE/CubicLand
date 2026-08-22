using System.Collections.Generic;
using UnityEngine;

namespace Commar.CubicLand.Cube
{
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

        public void FindEdgeCubeDatas(Enums.Direction3D direction, IList<CubeData> cubeDatas);
        public void FindVisibleCubeDatas(Enums.Direction3D direction, IList<CubeData> cubes);
    }
}
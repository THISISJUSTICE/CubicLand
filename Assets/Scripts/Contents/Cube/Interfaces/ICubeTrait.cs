using UnityEngine;

namespace CustomTIJI.CubicLand.Cube
{
    public interface ICubeTrait
    {
        public void SetMeshRenderer(MeshRenderer renderer);
        public MaterialPropertyBlock BuildPropertyBlock(CubeData cubeData);
        public float CalculateMass(StatusValue statusValue);
    }
}
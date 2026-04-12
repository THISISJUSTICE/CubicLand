namespace CustomTIJI.CubicLand.Cube
{
    public interface ICubeFactory
    {
        public CubeObject CreateObjectCube(CubeData cubeData);
        public ObstacleCube CreateObstacleCube(CubeData cubeData);
        public GolemCore CreateGolem(GolemData golemData);
        public void DestoryCube(CubeObject golemcube);
    }
}
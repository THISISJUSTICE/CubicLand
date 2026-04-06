namespace CustomTIJI.CubicLand.Cube
{
    public interface IGolemCubeFactory
    {
        public CubeObject CreateCube(CubeData cubeData);
        public void DestoryCube(CubeObject golemcube);
    }
}
namespace CustomTIJI.CubicLand.Cube
{
    public interface ICubeFactory
    {
        public CubeObject CreateCube(CubeData cubeData);
        public void DestoryCube(CubeObject golemcube);
    }
}
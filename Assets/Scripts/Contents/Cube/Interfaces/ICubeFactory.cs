namespace Commar.CubicLand.Cube
{
    public interface ICubeFactory
    {
        public CubeObject CreateCube(CubeData cubeData, CubeSpawnOptions options);
        public void DestoryCube(CubeObject cube);
    }

    public readonly struct CubeSpawnOptions
    {
        public bool IsAnimating { get; }

        public readonly static CubeSpawnOptions IMMEDIATE = new CubeSpawnOptions(false);
        public readonly static CubeSpawnOptions ANIMATED = new CubeSpawnOptions(true);

        public CubeSpawnOptions(bool isAnimating)
        {
            IsAnimating = isAnimating;
        }
    }
}
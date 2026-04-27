namespace CustomTIJI.CubicLand.Cube
{
    internal class OrphanedObjectCubeHandler : IOrphanedCubeHandler
    {
        private readonly ICubeFactory _cubeFactory;

        internal OrphanedObjectCubeHandler(ICubeFactory cubeFactory)
        {
            _cubeFactory = cubeFactory;
        }

        public void HandleOrphanedCube(CubeObject cube)
        {
            cube.CubeData.IsAttackMode = false;
            ObstacleCube obstacleCube = _cubeFactory.CreateCube(cube.CubeData.Copy()).GetComponent<ObstacleCube>();
            obstacleCube.transform.position = cube.transform.position;
            obstacleCube.transform.rotation = cube.transform.rotation;
            obstacleCube.NormalizeTransform();

            _cubeFactory.DestoryCube(cube);
        }
    }
}
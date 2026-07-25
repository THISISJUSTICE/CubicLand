namespace CustomTIJI.CubicLand.Cube
{
    public class ObstacleCubeFactory : ObjectCubeFactory
    {
        private readonly ICubeCollisionResolver _collisionResolver;

        protected override string LoadKey => "ObstacleCube";

        public ObstacleCubeFactory(IObjectPool objectPool, IAsyncAssetLoader assetLoader, ICubeSpawnEffect spawnEffect, ICubeCollisionResolver collisionResolver)
            : base(objectPool, assetLoader, spawnEffect)
        {
            _collisionResolver = collisionResolver;
        }

        protected override void OnCubeInitialized(CubeObject cube)
        {
            base.OnCubeInitialized(cube);

            cube.GetComponent<ObstacleCube>().Initialize(_collisionResolver);
        }
    }
}
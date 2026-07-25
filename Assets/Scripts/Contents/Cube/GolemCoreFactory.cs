
namespace CustomTIJI.CubicLand.Cube
{
    public class GolemCoreFactory : ObjectCubeFactory
    {
        private ICubeCollisionResolver _cubeCollisionResolver;
        private IOrphanedCubeHandler _orphanedCubeHandler;
        private ICubeFactory _cubeFactory;

        protected override string LoadKey => "GolemCore";
        protected override bool AutoReleaseOnDestroyed => false;

        public GolemCoreFactory(ICubeCollisionResolver cubeCollisionResolver, IOrphanedCubeHandler orphanedCubeHandler, ICubeFactory cubeFactory,
            IObjectPool objectPool, IAsyncAssetLoader assetLoader, ICubeSpawnEffect spawnEffect)
            : base(objectPool, assetLoader, spawnEffect)
        {
            _cubeCollisionResolver = cubeCollisionResolver;
            _orphanedCubeHandler = orphanedCubeHandler;
            _cubeFactory = cubeFactory;
        }

        protected override void OnCubeInitialized(CubeObject cube)
        {
            base.OnCubeInitialized(cube);

            GolemCore core = cube.GetComponent<GolemCore>();
            GolemMotionController motionController = new GolemMotionController(core);
            core.Initialize(_cubeCollisionResolver, _orphanedCubeHandler, _cubeFactory, motionController);
            core.onReleased += DestoryCube;
        }
    }
}
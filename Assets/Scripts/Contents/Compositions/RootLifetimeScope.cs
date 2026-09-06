using Commar.CubicLand.Cube;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Commar.CubicLand.Compositions
{
    public class RootLifetimeScope : LifetimeScope
    {
        [SerializeField, Min(0)] private int _maxPoolSizeInWindows = 1000;
        [SerializeField, Min(0)] private int _maxPoolSizeInMobile = 100;
        [SerializeField] private UnityLoopHandler _unityLoopHandler;

        private int MaxPoolSize
        {
            get
            {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                return _maxPoolSizeInWindows;
#else
                return _maxPoolSizeInMobile;
#endif
            }
        }

        protected override void Configure(IContainerBuilder builder)
        {
            RegisterCoreServices(builder);
            RegisterCube(builder);
            RegisterHandlers(builder);
        }

        private void RegisterCoreServices(IContainerBuilder builder)
        {
            builder.Register<ObjectPool>(Lifetime.Singleton)
                .WithParameter(MaxPoolSize)
                .As<IObjectPool>();
            builder.Register<LocalAddressableAssetLoader>(Lifetime.Singleton)
                .As<IAsyncAssetLoader>();
        }

        private void RegisterCube(IContainerBuilder builder)
        {
            builder.Register<CubeCollisionResolver>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<GolemCubeFactory>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .Keyed(CubeFactoryKey.Object);
            builder.Register<GolemCoreFactory>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .Keyed(CubeFactoryKey.Core);
            builder.Register<ObstacleCubeFactory>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .Keyed(CubeFactoryKey.Obstacle);

            builder.Register<OrphanedObjectCubeHandler>(Lifetime.Singleton)
                .As<IOrphanedCubeHandler>();
            builder.Register<GolemFactory>(Lifetime.Singleton)
                .As<IGolemFactory>();
        }

        private void RegisterHandlers(IContainerBuilder builder)
        {
            builder.RegisterComponent(_unityLoopHandler);
        }
    }
}
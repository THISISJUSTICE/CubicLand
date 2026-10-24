using Commar.CubicLand.Cube;
using Commar.CubicLand.DataStorage;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Commar.CubicLand.Composition
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
                .AsImplementedInterfaces();
            builder.Register<LocalAddressableAssetLoader>(Lifetime.Singleton)
                .AsImplementedInterfaces();
            builder.Register<LocalDataStorage>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }

        private void RegisterCube(IContainerBuilder builder)
        {
            builder.Register<CubeCollisionResolver>(Lifetime.Singleton)
                .AsImplementedInterfaces();
            builder.Register<EmptyCubeSpawnEffect>(Lifetime.Singleton)
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
                .AsImplementedInterfaces();
            builder.Register<GolemFactory>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }

        private void RegisterHandlers(IContainerBuilder builder)
        {
            builder.RegisterComponent(_unityLoopHandler);
        }
    }
}
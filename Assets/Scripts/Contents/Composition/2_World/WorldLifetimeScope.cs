using Commar.CubicLand.Game;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Commar.CubicLand.Composition
{
    public class WorldLifetimeScope : LifetimeScope
    {
        [SerializeField] private WorldManager _worldManager;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<RandomStatusEnhancer>(Lifetime.Scoped);
            builder.RegisterComponent(_worldManager);
        }
    }
}

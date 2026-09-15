using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Commar.CubicLand.Compositions
{
    public class IntroLifetimeScope : LifetimeScope
    {
        [SerializeField] private IntroManager _introManager;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<OperationWaiter>(Lifetime.Scoped);
            builder.RegisterComponent(_introManager);
        }
    }
}
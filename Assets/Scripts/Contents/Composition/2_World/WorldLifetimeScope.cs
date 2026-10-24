using Commar.CubicLand.Game;
using Commar.CubicLand.Player;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Commar.CubicLand.Composition
{
    public class WorldLifetimeScope : LifetimeScope
    {
        [SerializeField] private WorldManager _worldManager;
        [SerializeField] private PlayerInputController _inputController;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<RandomStatusEnhancer>(Lifetime.Scoped);
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            // TODO: 모바일 InputSource 적용
#else
            builder.Register<PlayerKeyboardInputSource>(Lifetime.Scoped).AsImplementedInterfaces();
#endif
            builder.RegisterComponent(_worldManager);
            builder.RegisterComponent(_inputController);
        }
    }
}
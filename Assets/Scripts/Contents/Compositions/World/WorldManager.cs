using Commar.CubicLand.Cube;
using Commar.CubicLand.Game;
using Commar.CubicLand.Player;
using UnityEngine;
using VContainer;

namespace Commar.CubicLand.Compositions
{
    public class WorldManager : MonoBehaviour
    {
        private IGolemFactory _golemFactory;
        private RandomStatusEnhancer _statusEnhancer;

        [Inject]
        public void Initialize(IGolemFactory golemFactory, RandomStatusEnhancer statusEnhancer)
        {
            _golemFactory = golemFactory;
            _statusEnhancer = statusEnhancer;
        }

        private void Start()
        {
            MakeTempPlayer();
        }

        private void MakeTempPlayer()
        {
            GolemData golemData = new GolemData();
            _statusEnhancer.EnhanceRandomStatus(golemData, 50);

            GolemCore player = _golemFactory.CreateGolem("Player", golemData, CubeSpawnOptions.IMMEDIATE);
            if (player == null)
            {
                Debug.LogError("Failed to create Player Golem", this);
                return;
            }

            player.transform.position = Vector3.up * (player.MotionController.GeometryProvider.GolemHeight + 1) * CubeConfig.CUBE_BASE_LENGTH;

            PlayerKeyboardController keyboardController = player.gameObject.GetOrAddComponenet<PlayerKeyboardController>();
            keyboardController.Initialize(player.MotionController);
        }
    }
}
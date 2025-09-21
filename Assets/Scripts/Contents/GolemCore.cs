using UnityEngine;

namespace CustomTIJI.CubicLand
{
    [RequireComponent(typeof(GolemCube))]
    public class GolemCore : CubeCore<GolemCube>
    {
        #region Variables
        private GolemController _golemController;

        protected override Vector3 MoveVelocity => _golemController.MoveVelocity;
        #endregion

        #region Unity Functions
        protected override void Awake()
        {
            base.Awake();
            _golemCube = GetComponent<GolemCube>();
        }
        #endregion

        #region Public Functions
        public void SetInit(GolemInfo golemInfo)
        {
            SetupChilds();
            SetGolemInfo(golemInfo);

            _rb.Reset();
            _rb.UseAngular(false);
        }

        public void SetGolemController(GolemController golemController)
        {
            _golemController = golemController;
        }

        public void SetAttackMode(bool on)
        {
            _golemCube.SetAttackMode(on);
        }
        #endregion

        #region Utils
        protected override void OnImpulse(Vector3 impulse)
        {
            _golemController.OnImpulse(impulse);
        }
        #endregion
    }
}
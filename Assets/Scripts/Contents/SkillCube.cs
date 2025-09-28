using UnityEngine;

namespace CustomTIJI.CubicLand
{
    public class SkillCube : CubeBase
    {
        #region Variables
        private Rigidbody _rb;

        private TrailRenderer _trail;
        #endregion

        #region Unity Functions
        protected override void Awake()
        {
            _trail = GetComponent<TrailRenderer>();
            base.Awake();
        }
        #endregion

        #region Public Functions
        public override void SetCubeInfo(CubeInfo cubeInfo)
        {
            _cubeInfo = cubeInfo;
            SetCubeShader();
            cubeInfo.AttackMode = true;
            CubeMass = Configs.ONE_CUBE_MASS;
        }
        #endregion

        #region Utils
        protected override void SetCubeShader()
        {

        }

        protected override void OnCubeDestroied()
        {
            // TODO: 파괴 이펙트

            ObjectManager.Instance.DestroyGameObject(gameObject);
        }
        #endregion
    }
}
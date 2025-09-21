using UnityEngine;

namespace CustomTIJI.CubicLand
{
    public abstract class CubeBase : MonoBehaviour
    {
        #region Variables
        [Header("Viewer")]
        [SerializeField] protected CubeInfo _cubeInfo;

        protected BoxCollider _collider;
        protected MeshRenderer _meshRen;

        public CubeInfo CubeInfo { get => _cubeInfo; }
        public BoxCollider Collider { get => _collider; }
        public float CubeMass { get; protected set; }
        #endregion

        #region Unity Functions
        protected virtual void Awake()
        {
            _collider = GetComponent<BoxCollider>();
            _meshRen = GetComponent<MeshRenderer>();
        }

        protected virtual void Start()
        {
            SetupCollider();
        }
        #endregion

        #region Settings
        protected void SetupCollider()
        {
            _collider.size = Vector3.one * (Configs.CUBE_BASE_LENGHT - Physics.defaultContactOffset * 4f);
        }
        #endregion

        #region Public Functions
        public virtual void SetCubeInfo(CubeInfo cubeInfo)
        {
            _cubeInfo = cubeInfo;
            SetCubeShader();
            CubeMass = Utils.GetCubeMass(cubeInfo.CurrentStatus);
        }

        public void OnDamaged(float selfMass, Vector3 impulse, CubeInfo? colCubeInfo = null)
        {
            OnDamaged(selfMass, impulse, colCubeInfo, out float damageRate);
        }

        public virtual void OnDamaged(float selfMass, Vector3 impulse, CubeInfo? colCubeInfo, out float damageRate)
        {
            int prevHP = _cubeInfo.CurrentStatus.HP;

            if (colCubeInfo != null)
                _cubeInfo.CurrentStatus.OnDamaged(selfMass, impulse, (CubeInfo)colCubeInfo);
            else
                _cubeInfo.CurrentStatus.OnDamaged(selfMass, impulse);
            SetCubeShader();

            damageRate = (float)(prevHP - _cubeInfo.CurrentStatus.HP) / (float)_cubeInfo.CurrentStatus.MaxHP;

            if (_cubeInfo.CurrentStatus.HP <= 0)
                OnCubeDestroied();
            else
            {
                CubeMass = Utils.GetCubeMass(_cubeInfo.CurrentStatus);
            }
        }
        #endregion

        #region Utils
        protected virtual void SetCubeShader()
        {
            _meshRen.sharedMaterial.SetColor("_Color", _cubeInfo.Status.Color);

            // TODO: 애니메이션?
            float rateHP = (float)_cubeInfo.CurrentStatus.HP / (float)_cubeInfo.CurrentStatus.MaxHP;
            _meshRen.sharedMaterial.SetFloat("_Range", Mathf.Clamp01(1f - rateHP));
            _meshRen.sharedMaterial.SetFloat("_FadeAlpha", Mathf.Clamp01(1f - rateHP) * 0.5f);
        }

        protected virtual void OnCubeDestroied()
        {
            EffectManager.Instance.MakeCubeDestroyEffect(transform.position, _cubeInfo.Status.Color);

            // TODO: 드롭 아이템

            ObjectManager.Instance.DestroyObject(gameObject);
        }
        #endregion
    }
}
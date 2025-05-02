using UnityEngine;

public abstract class DVCubeBase : MonoBehaviour
{
    #region Variables
    [Header("Viewer")]
    [SerializeField] protected DVCubeInfo _cubeInfo;

    protected BoxCollider _collider;
    protected MeshRenderer _meshRen;

    public DVCubeInfo CubeInfo
    {
        get { return _cubeInfo; }
    }

    public float CubeMass { get; private set; }
    #endregion

    #region Unity Functions
    protected virtual void Awake()
    {
        _collider = GetComponent<BoxCollider>();
        _meshRen = GetComponent<MeshRenderer>();
    }

    protected virtual void OnEnable()
    {

    }

    protected virtual void OnDisable()
    {

    }

    protected virtual void Start()
    {
        SetupCollider();
    }
    #endregion

    #region Settings
    protected void SetupCollider()
    {
        _collider.size = Vector3.one * (DVConfigs.CUBE_BASE_LENGHT - Physics.defaultContactOffset * 4f);
    }
    #endregion

    #region Public Functions
    public void SetInit(DVCubeInfo cubeInfo) {
        _cubeInfo = cubeInfo;
        SetCubeShader();
        CubeMass = DVUtil.GetCubeMass(cubeInfo.Status.CurrentStatus);
    }

    public void OnDamaged(float selfMass, Vector3 impulse, DVCubeInfo colCubeInfo)
    {
        OnDamaged(selfMass, impulse, colCubeInfo, out float damageRate);
    }

    public virtual void OnDamaged(float selfMass, Vector3 impulse, DVCubeInfo? colCubeInfo, out float damageRate)
    {
        int prevHP = _cubeInfo.Status.CurrentStatus.HP;

        if (colCubeInfo != null)
            _cubeInfo.Status.CurrentStatus.OnDamaged(selfMass, impulse, (DVCubeInfo)colCubeInfo);
        else
            _cubeInfo.Status.CurrentStatus.OnDamaged(selfMass, impulse);
        SetCubeShader();

        damageRate = (float)(prevHP - _cubeInfo.Status.CurrentStatus.HP) / (float)_cubeInfo.Status.CurrentStatus.MaxHP;

        if (_cubeInfo.Status.CurrentStatus.HP <= 0)
            OnCubeDestroied();
        else
        {
            CubeMass = DVUtil.GetCubeMass(_cubeInfo.Status.CurrentStatus);
        }
    }
    #endregion

    #region Utils
    protected void SetCubeShader()
    {
        _meshRen.sharedMaterial.SetColor("_Color", _cubeInfo.Status.Color);

        // TODO: 애니메이션?
        float rateHP = (float)_cubeInfo.Status.CurrentStatus.HP / (float)_cubeInfo.Status.CurrentStatus.MaxHP;
        _meshRen.sharedMaterial.SetFloat("_Range", Mathf.Clamp01(1f - rateHP));
        _meshRen.sharedMaterial.SetFloat("_FadeAlpha", Mathf.Clamp01(1f - rateHP) * 0.5f);
    }

    protected virtual void OnCubeDestroied()
    {
        DVEffectManager.Instance.MakeEffect("CubeDestroyEffect", transform.position);
        DVEffectManager.Instance.MakeCubeDestroyEffect(transform.position, _cubeInfo.Status.Color);

        // TODO: 드롭 아이템

        DVObjectManager.Instance.DestroyObject(gameObject);
    }
    #endregion
}

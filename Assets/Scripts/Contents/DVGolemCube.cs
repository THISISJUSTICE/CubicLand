using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DVGolemCube : MonoBehaviour
{
    #region Variables
    [Header("Viewer")]
    [SerializeField] private DVCubeInfo _cubeInfo;
    [SerializeField] private DVGolemCube _parent;
    [SerializeField] private DVGolemCube[] _childs;
    [SerializeField] private DVGolemCore _core;

    // TODO: 어드레서블로 변경
    [Header("Temp")]
    [SerializeField] private Material _coreMaterial;

    private BoxCollider _collider;
    private MeshRenderer _meshRen;
    private Rigidbody _rb;

    private Material _cubeMaterial;
    #endregion

    #region Properties
    public DVGolemCore Core { get => _core; }

    public DVCubeInfo CubeInfo
    {
        get { return _cubeInfo; }
    }
    #endregion

    #region Unity Functions
    private void Awake()
    {
         _collider = GetComponent<BoxCollider>();
        _meshRen = GetComponent<MeshRenderer>();
        _cubeMaterial = _meshRen.sharedMaterial;
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    private void Start()
    {
        SetupCollider();
    }

    private void Update()
    {
        if (_core == null && _rb != null) {
            _cubeInfo.AttackMode = Mathf.Abs(_rb.linearVelocity.y) > 0.01f;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null)
            return;

        if (collision.gameObject.tag == "Map") {

            return;
        }

        DVGolemCube colCube = collision.gameObject.GetComponent<DVGolemCube>();
        if (colCube == null)
            return;

        if (colCube.CubeInfo.AttackMode || _cubeInfo.AttackMode)
        {
            DVCubeInfo cubeInfo = colCube.CubeInfo;
            StartCoroutine(DVHelper.In.WaitFrameActCor(1, () => OnDamaged(cubeInfo, colCube)));
        }
        else
        {
            
        }
    }
    #endregion

    #region Settings
    private void SetupCollider() {
        _collider.size = Vector3.one * (DVConfigs.CUBE_BASE_LENGHT - Physics.defaultContactOffset * 4f);
    }
    #endregion

    #region Public Functions
    public void SetGolemCubeInfo(DVCubeInfo cubeInfo, DVGolemCube parent, DVGolemCore core) { 
        _cubeInfo = cubeInfo;
        _parent = parent;
        _childs = null;
        _core = core;

        if (_core.GolemCube == this)
        {
            _meshRen.sharedMaterial = _coreMaterial;
        }
        else {
            
        }


        SetCubeShader();
    }

    public void SetGolemChild(DVGolemCube[] childs) { 
        _childs = childs;
    }

    public void OnParentDestroied() {
        _parent = null;
        _core = null;
        transform.SetParent(null);

        _rb = DVObjectManager.Instance.AddComponent<Rigidbody>(gameObject);
        _rb.SetGolemMass();
        _rb.UseOnlyGravity();

        _cubeInfo.AttackMode = false;
        _cubeInfo.Status.CurrentStatus.SetAttackOff();

        if (_childs != null)
        {
            foreach (var child in _childs)
                child.OnParentDestroied();
            _childs = null;
        }

        NormalizeTransform();
    }

    public void OnChildDestroied(DVGolemCube child) { 
        List<DVGolemCube> childs = new List<DVGolemCube>();
        foreach (var prevChild in _childs) { 
            if(prevChild != child)
                childs.Add(prevChild);
        }
        if(childs.Count > 0)
            _childs = childs.ToArray();
        else
            _childs = null;
    }

    public void SetAttackMode(bool on) { 
        _cubeInfo.AttackMode = on;
        if (_childs != null)
        {
            foreach (var child in _childs)
                child.SetAttackMode(on);
        }
    }
    #endregion

    #region Utils
    private void SetCubeShader() {
        _meshRen.sharedMaterial.SetColor("_Color", _cubeInfo.Status.Color);
        float rateHP = (float)_cubeInfo.Status.CurrentStatus.HP / (float)_cubeInfo.Status.CurrentStatus.MaxHP;
        _meshRen.sharedMaterial.SetFloat("_Range", Mathf.Clamp01(1f - rateHP));
        _meshRen.sharedMaterial.SetFloat("_FadeAlpha", Mathf.Clamp01(1f - rateHP) * 0.5f);
    }

    private void OnDamaged(DVCubeInfo cubeInfo, DVGolemCube colCube) {
        _cubeInfo.Status.CurrentStatus.OnDamaged(cubeInfo);
        SetCubeShader();
        if (_cubeInfo.Status.CurrentStatus.HP <= 0)
            OnCubeDestroied(colCube);
        else {
            
        }
    }

    private void OnCubeDestroied(DVGolemCube colCube) {
        // TODO: 파괴 이펙트 파티클

        if (_parent != null)
        {
            _parent.OnChildDestroied(this);
            _parent = null;
        }
        if (_childs != null)
        {
            foreach (var child in _childs)
            {
                child.OnParentDestroied();
            }
            _childs = null;
        }

        if (_core != null)
        {
            _core = null;
        }

        _meshRen.sharedMaterial = _cubeMaterial;

        // TODO: 드롭 아이템

        DVObjectManager.Instance.DestroyObject(gameObject);
    }

    private void NormalizeTransform() {
        const float time = 0.3f;
        StartCoroutine(DVUtil.NormalizePositionCor(transform, transform.position, time));
        StartCoroutine(DVUtil.NormalizeRotationCor(transform, transform.rotation, time));
    }
    #endregion
}

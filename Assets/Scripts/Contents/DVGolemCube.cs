using UnityEngine;

public class DVGolemCube : MonoBehaviour
{
    #region Variables
    [Header("Viewer")]
    [SerializeField] private DVCubeInfo _cubeInfo;
    [SerializeField] private DVStatus _curStatus;
    [SerializeField] private DVGolemCube _parent;
    [SerializeField] private DVGolemCube[] _childs;

    private BoxCollider _collider;
    
    #endregion

    #region Properties
    public DVCubeInfo CubeInfo
    {
        get { return _cubeInfo; }
    }

    public DVStatus CurStatus {
        set { _curStatus = value; }
        get { return _curStatus; }
    }
    #endregion

    #region Unity Functions
    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
        _childs = new DVGolemCube[DVUtil.Direction3DLength];
    }

    private void OnEnable()
    {
        _curStatus = _cubeInfo.Status;
    }

    private void OnDisable()
    {
        
    }

    private void Start()
    {
        SetupCollider();
    }

    private void OnTriggerEnter(Collider other)
    {
        SendMessageToParent(other);
    }
    #endregion

    #region Settings
    private void SetupCollider() {
        _collider.size = Vector3.one * DVConfigs.CUBE_BASE_LENGHT;
        _collider.isTrigger = true;
    }
    #endregion

    #region Public Functions
    public void SetCubeInfo(DVCubeInfo cubeInfo) { 
        _cubeInfo = cubeInfo;
    }

    public bool TryGetChilds(DVEnums.Direction3D direction, out DVGolemCube golemCube) {
        golemCube = _childs[(int)direction];
        if (golemCube == null) {
            return false;
        }

        return true;
    }

    public void ReceiveMessage(Object value) {
        if (_parent != null) {
            SendMessageToParent(value);
            return;
        }

        if (!_cubeInfo.IsCore)
            return;

        // TODO: 구현        
    }
    #endregion

    #region Utils
    // TODO: 메시지 형식
    private void SendMessageToParent(Object value)
    {
        _parent.ReceiveMessage(value);
    }
    #endregion
}

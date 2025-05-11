using UnityEngine;

public class DVPlayerFollower : MonoBehaviour
{
    #region Variables
    [SerializeField] private float _lerp;

    private GameObject _playerObject;
    private DVPlayerController _playerController;

    private Camera _mainCam;

    private Vector3 PlayerBack 
    { 
        get 
        {
            float backDist = DVUtil.GetDistanceXZ(
                _playerController.BackEdgeCube.transform.position, transform.position);
            float upDist = DVUtil.GetDistanceXZ(
                _playerController.UpEdgeCube.transform.position, transform.position);

            Vector3 position = backDist < upDist ? 
                _playerController.BackEdgeCube.transform.position : 
                _playerController.UpEdgeCube.transform.position;
            position.y = _playerController.AxisCube.transform.position.y;

            return position; 
        } 
    }
    private Vector3 PlayerUp { get => Vector3.up * (float)_playerController.GolemHeight; }
    #endregion

    #region Unity Functions
    private void Start()
    {
        _mainCam = Camera.main;
    }

    private void LateUpdate()
    {
        if (_playerObject == null || _playerController == null)
            return;

        transform.rotation = _playerController.PlayerRotation;

        Vector3 targetPos = PlayerBack + PlayerUp * 0.8f - transform.forward * GetFollowDistance();
        if(Vector3.Distance(transform.position, targetPos) < 0.02f)
            transform.position = Vector3.Lerp(transform.position, targetPos, _lerp);
        else
            transform.position = targetPos;

        transform.LookAt(PlayerBack + PlayerUp * 0.5f);
    }
    #endregion

    public void SetPlayer(GameObject playerObject) { 
        _playerObject = playerObject;
        _playerController = _playerObject.GetComponent<DVPlayerController>();
    }

    #region Utils
    private float GetFollowDistance() {
        float maxWidth = Mathf.Max(_playerController.GolemHeight, _playerController.GolemWidth);
        float distance = Mathf.Max(
            DVUtil.GetBaseLineHA(maxWidth, _mainCam.fieldOfView),
            _playerController.GolemBack + 1f);

        distance = distance * 1.2f + 1f;

        return distance;
    }
    #endregion
}

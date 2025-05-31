using UnityEngine;

public class DVPlayerViewFollower : MonoBehaviour
{
    #region Variables
    private DVPlayerController _playerController;

    private Vector3 PlayerFront {
        get {
            Vector3 backPos = _playerController.BackEdgeCube.transform.position;
            backPos.y = 0;
            Vector3 frontPos = _playerController.FrontEdgeCube.transform.position;
            frontPos.y = 0;
            Vector3 dir = (frontPos - backPos).normalized;

            return _playerController.FrontEdgeCube.transform.position +
                dir * DVConfigs.CUBE_BASE_LENGHT / 2f;
        } 
    }
    #endregion

    #region Unity Functions
    private void LateUpdate()
    {
        if (_playerController == null)
            return;

        transform.rotation = _playerController.PlayerRotation;
        transform.position = PlayerFront;
    }
    #endregion

    public void SetPlayer(GameObject playerObject)
    {
        _playerController = playerObject.GetComponent<DVPlayerController>();
    }
}

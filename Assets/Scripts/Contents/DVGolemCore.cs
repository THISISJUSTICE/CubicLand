using UnityEngine;

[RequireComponent(typeof(DVGolemCube))]
public class DVGolemCore : MonoBehaviour
{
    #region Variables
    private DVGolemInfo _golemInfo;
    private DVGolemCube _golemCube;

    #endregion

    #region Properties
    public DVGolemInfo GolemInfo { get => _golemInfo; }
    #endregion

    #region Unity Functions
    private void Awake()
    {
        _golemCube = GetComponent<DVGolemCube>();
    }
    #endregion

    #region Public Functions
    public void SetGolemInfo(DVGolemInfo golemInfo) { 
        _golemInfo = golemInfo;
    }
    #endregion

    #region Utils
    #endregion
}

using UnityEngine;

public class DVHelper : SingletonMonoBehaviour<DVHelper>
{
    #region Variables
    private DVYieldCache _yieldCache;
    #endregion

    #region Properties
    public static DVHelper In { get => Instance as DVHelper; }

    public DVYieldCache YieldCache { get { return _yieldCache; } }
    #endregion

    #region Unity Functions
    private void Awake()
    {
        _yieldCache = new DVYieldCache();
    }
    #endregion
}

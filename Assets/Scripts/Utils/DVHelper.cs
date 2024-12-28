using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

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

    #region Coroutines
    public IEnumerator WaitActCor(float waitTime, Action callback) {
        yield return _yieldCache.GetWaitForSeconds(waitTime);
        if(callback != null )
            callback();
    }
    #endregion
}

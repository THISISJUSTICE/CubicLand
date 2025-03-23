using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class DVHelper : SingletonMonoBehaviour<DVHelper>
{
    #region Variables
    private string _dataPath;
    private DVYieldCache _yieldCache;
    #endregion

    #region Properties
    public static DVHelper In { get => Instance as DVHelper; }

    public string DataPath { get { return _dataPath; } }
    public DVYieldCache YieldCache { get { return _yieldCache; } }
    #endregion

    #region Unity Functions
    private void Awake()
    {
        _yieldCache = new DVYieldCache();
        _dataPath = Application.persistentDataPath;
    }
    #endregion

    #region Public Functions
    public void WaitTimeAct(float waitTime, Action callback) { 
        StartCoroutine(WaitTimeActCor(waitTime, callback));
    }

    public void WaitFrameAct(int frame, Action callback) {
        StartCoroutine(WaitFrameActCor(frame, callback));
    }
    #endregion

    #region Coroutines
    public IEnumerator WaitTimeActCor(float waitTime, Action callback) {
        yield return _yieldCache.GetWaitForSeconds(waitTime);
        callback?.Invoke();
    }

    public IEnumerator WaitFrameActCor(int frame, Action callback)
    {
        for (int i = 0; i < frame; i++) 
            yield return null;
        callback?.Invoke();
    }
    #endregion
}

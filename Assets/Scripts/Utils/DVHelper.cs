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
    public static string DataPath { get { return Instance._dataPath; } }
    public static DVYieldCache YieldCache { get { return Instance._yieldCache; } }
    #endregion

    #region Unity Functions
    private void Awake()
    {
        _yieldCache = new DVYieldCache();
        _dataPath = Application.persistentDataPath;
    }
    #endregion

    #region Public Functions
    public async void WaitTimeAct(float waitTime, Action callback) {
        await Awaitable.WaitForSecondsAsync(waitTime);
        callback?.Invoke();
    }

    public async void WaitFrameAct(int frame, Action callback) {
        for (int i = 0; i < frame; i++)
            await Awaitable.NextFrameAsync();
        callback?.Invoke();
    }
    #endregion
}

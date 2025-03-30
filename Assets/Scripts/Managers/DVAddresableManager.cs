using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DVAddresableManager : SingletonMonoBehaviour<DVAddresableManager>
{
    #region Variables
    private Dictionary<string, AsyncOperationHandle> _assetHandles = new Dictionary<string, AsyncOperationHandle>();
    #endregion

    #region Coroutines
    public IEnumerator LoadAssetAsync<T>(string key, bool release = true, Action<T> onFinishedCallback = null) {
        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
        handle.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                onFinishedCallback?.Invoke(op.Result);
            }
            else {
                Debug.LogError($"Load Failed {key}\n{op.OperationException}");
            }
        };

        yield return handle;

        if (release)
            ReleaseHandle(handle);
        else
        {
            ReleaseAsset(key);
            _assetHandles[key] = handle;
        }
    }
    #endregion

    #region Utils
    public void ReleaseAsset(string key)
    {
        if (string.IsNullOrEmpty(key))
            return;

        if (_assetHandles.TryGetValue(key, out var handle))
        {
            ReleaseHandle(handle);
            _assetHandles.Remove(key);
        }
    }

    private void ReleaseHandle(AsyncOperationHandle handle)
    {
        if (handle.IsValid())
            Addressables.Release(handle);
    }
    #endregion
}

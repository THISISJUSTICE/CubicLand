using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

public class DVAddresableManager : SingletonMonoBehaviour<DVAddresableManager>
{
    #region Variables
    private Dictionary<string, AsyncOperationHandle> _assetHandles = new Dictionary<string, AsyncOperationHandle>();
    #endregion

    #region Load Functions
    public async Awaitable<T> LoadAsset<T>(string key, Action<bool> onDefineSuccess = null)
    {
        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
        T res = await handle;
        ReleaseAsset(key);
        _assetHandles[key] = handle;

        onDefineSuccess?.Invoke(handle.Status == AsyncOperationStatus.Succeeded);

        return res;
    }

    public async Awaitable<IList<T>> LoadAssets<T>(string key, Action<bool> onDefineSuccess = null)
    {
        AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(key);
        IList<T> res = await handle;
        ReleaseAsset(key);
        _assetHandles[key] = handle;

        onDefineSuccess?.Invoke(handle.Status == AsyncOperationStatus.Succeeded);

        return res;
    }

    public async Awaitable<IList<T>> LoadAssets<T>(params string[] keys) => await LoadAssets<T>(null, keys);

    public async Awaitable<IList<T>> LoadAssets<T>(Action<bool> onDefineSuccess, params string[] keys)
    {
        List<UniTask<T>> loadTasks = new List<UniTask<T>>();
        bool success = true;

        for (int i = 0; i < keys.Length; i++)
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(keys[i]);
            handle.Completed += (res) => 
            success &= handle.Status == AsyncOperationStatus.Succeeded;

            loadTasks.Add(handle.ToUniTask());
        }

        IList<T> res = await UniTask.WhenAll(loadTasks);

        onDefineSuccess?.Invoke(success);

        return res;
    }

    public static async Awaitable<GameObject> InstantiateAsync(string key, Transform parent = null)
    {
        return await Addressables.InstantiateAsync(key, parent);
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

    public static void ReleaseInstance(GameObject go)
    {
        Addressables.ReleaseInstance(go);
    }

    private void ReleaseHandle(AsyncOperationHandle handle)
    {
        if (handle.IsValid())
            Addressables.Release(handle);
    }
    #endregion
}

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

namespace CustomTIJI.CubicLand
{
    /// <summary>
    /// Local Load만 할 것이기에 실패는 고려하지 않음
    /// </summary>
    public class AddresableManager : SingletonMonoBehaviour<AddresableManager>
    {
        private Dictionary<string, AsyncOperationHandle> _assetHandles = new Dictionary<string, AsyncOperationHandle>();

        public async UniTask<T> LoadAsset<T>(string key)
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
            T res = await handle;
            ReleaseAsset(key);
            _assetHandles[key] = handle;

            return res;
        }

        public async UniTask<IList<T>> LoadAssets<T>(string key)
        {
            AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(key);
            IList<T> res = await handle;
            ReleaseAsset(key);
            _assetHandles[key] = handle;

            return res;
        }

        public async UniTask<IList<T>> LoadAssets<T>(params string[] keys)
        {
            List<UniTask<T>> loadTasks = new List<UniTask<T>>();

            for (int i = 0; i < keys.Length; i++)
            {
                AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(keys[i]);
                loadTasks.Add(handle.ToUniTask());
                ReleaseAsset(keys[i]);
                _assetHandles[keys[i]] = handle;
            }

            IList<T> res = await UniTask.WhenAll(loadTasks);

            return res;
        }

        public static async UniTask<GameObject> InstantiateAsync(string key, Transform parent = null)
        {
            return await Addressables.InstantiateAsync(key, parent);
        }

        public static void ReleaseInstance(GameObject go)
        {
            Addressables.ReleaseInstance(go);
        }

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
    }
}
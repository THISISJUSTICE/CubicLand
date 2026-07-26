using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Commar.CubicLand
{
    public class LocalAddressableAssetLoader : IAsyncAssetLoader
    {
        private readonly Dictionary<string, AsyncOperationHandle> _assetHandles = new Dictionary<string, AsyncOperationHandle>();

        public async UniTask<T> LoadAsset<T>(string key) where T : Object
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
            T res = await handle;
            ReleaseAsset(key);
            _assetHandles[key] = handle;

            return res;
        }

        public async UniTask<IList<T>> LoadAssets<T>(string key) where T : Object
        {
            AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(key);
            IList<T> res = await handle;
            ReleaseAsset(key);
            _assetHandles[key] = handle;

            return res;
        }

        public async UniTask<IList<T>> LoadAssets<T>(params string[] keys) where T : Object
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
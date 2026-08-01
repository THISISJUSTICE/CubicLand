using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Commar.CubicLand
{
    public class LocalAddressableAssetLoader : IAsyncAssetLoader
    {
        private class AssetHandle
        {
            private readonly AsyncOperationHandle _handle;

            public int Count { get; private set; }
            public object Value { get; private set; }

            public AssetHandle(AsyncOperationHandle handle, object value)
            {
                _handle = handle;
                Value = value;
                Count = 1;
            }

            public void Increase() => Count++;
            public void Decrease() => Count--;
            public void Release() => _handle.Release();
        }

        private readonly Dictionary<string, AssetHandle> _assetHandles = new Dictionary<string, AssetHandle>();
        private readonly InvalidKeyException _nullKeyException = new InvalidKeyException("IsNullOrEmpty");

        public async UniTask<OperationResult<T>> LoadAsset<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                return OperationResult.GetFailedResult<T>(_nullKeyException.Message);

            if (_assetHandles.TryGetValue(key, out AssetHandle assetHandle))
            {
                assetHandle.Increase();
                return OperationResult.GetSuccessResult((T)assetHandle.Value);
            }

            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
            T result = await handle;

            _assetHandles.Add(key, new AssetHandle(handle, result));

            return OperationResult.GetSuccessResult(result);
        }

        public async UniTask<OperationResult<IList<T>>> LoadAssets<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                return OperationResult.GetFailedResult<IList<T>>(_nullKeyException.Message);

            if (_assetHandles.TryGetValue(key, out AssetHandle assetHandle))
            {
                assetHandle.Increase();
                return OperationResult.GetSuccessResult((IList<T>)assetHandle.Value);
            }

            AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(key);
            IList<T> result = await handle;

            _assetHandles.Add(key, new AssetHandle(handle, result));

            return OperationResult.GetSuccessResult(result);
        }

        public void ReleaseAsset(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;

            if (_assetHandles.TryGetValue(key, out AssetHandle assetHandle))
            {
                if (assetHandle.Count > 1)
                    assetHandle.Decrease();
                else
                {
                    assetHandle.Release();
                    _assetHandles.Remove(key);
                }
            }
        }
    }
}
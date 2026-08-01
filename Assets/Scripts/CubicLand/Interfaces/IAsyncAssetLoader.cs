using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Commar.CubicLand
{
    public interface IAsyncAssetLoader
    {
        public UniTask<OperationResult<T>> LoadAsset<T>(string key);
        public UniTask<OperationResult<IList<T>>> LoadAssets<T>(string key);
        public void ReleaseAsset(string key);
    }
}
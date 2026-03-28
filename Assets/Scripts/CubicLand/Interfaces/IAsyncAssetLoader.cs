using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace CustomTIJI.CubicLand
{
    public interface IAsyncAssetLoader
    {
        public UniTask<T> LoadAsset<T>(string key) where T : Object;
        public UniTask<IList<T>> LoadAssets<T>(string key) where T : Object;
        public UniTask<IList<T>> LoadAssets<T>(params string[] keys) where T : Object;
        public void ReleaseAsset(string key);
    }
}
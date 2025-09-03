using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

public class DVResourceManager : SingletonMonoBehaviour<DVResourceManager>
{
    #region Variables
    private readonly string[] keys = new string[] { "Cubes", "Effects" };

    private Dictionary<DVAssets.AssetType, DVAssets> _assets = new Dictionary<DVAssets.AssetType, DVAssets>();
    #endregion

    #region Unity Functions
    private void OnDestroy()
    {
        for (int i = 0; i < keys.Length; i++) 
            DVAddresableManager.Instance?.ReleaseAsset(keys[i]);
    }
    #endregion

    #region Coroutines
    public async UniTask LoadAssets() 
    {
        IList<DVAssets> assets = await DVAddresableManager.Instance.LoadAssets<DVAssets>(keys);

        foreach (DVAssets asset in assets) 
            _assets[asset.Type] = asset;
    }
    #endregion

    #region Utils
    public bool TryGetAssetDictionary(DVAssets.AssetType assetType, out Dictionary<string, UnityEngine.Object> dic) {
        dic = null;
        if (_assets.TryGetValue(assetType, out var assets)) {
            dic = assets.MakeDictionary();
            return true;
        }

        return false;
    }
    #endregion
}

using UnityEngine;
using System;
using System.Collections;
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
    public IEnumerator LoadAssets(Action<bool> onFinishedCallback = null) {
        DVLoadFlag flag = new DVLoadFlag(keys.Length);

        for (int i = 0; i < keys.Length; i++) {
            StartCoroutine(DVAddresableManager.Instance.LoadAssetAsync<DVAssets>(keys[i], release: false,
                (success, asset) =>
                {
                    if (asset != null)
                    {
                        _assets[asset.Type] = asset;
                    }
                    flag.SetFlag(success);
                }));
        }

        while(flag.Loading)
            yield return null;

        onFinishedCallback?.Invoke(flag.IsSuccess);
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

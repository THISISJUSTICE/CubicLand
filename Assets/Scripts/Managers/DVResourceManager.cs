using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class DVResourceManager : SingletonMonoBehaviour<DVResourceManager>
{
    private readonly string[] keys = new string[] { "Cubes", "Effects" };

    private Dictionary<string, DVAssetPackSO> _assets = new Dictionary<string, DVAssetPackSO>();

    private void OnDestroy()
    {
        for (int i = 0; i < keys.Length; i++) 
            DVAddresableManager.Instance?.ReleaseAsset(keys[i]);
    }

    public async UniTask LoadAssets() 
    {
        IList<DVAssetPackSO> assets = await DVAddresableManager.Instance.LoadAssets<DVAssetPackSO>(keys);

        foreach (DVAssetPackSO asset in assets)
            _assets[asset.name] = asset;
    }

    public bool TryGetAssetDictionary(string assetPackName, out Dictionary<string, UnityEngine.Object> dic)
    {
        dic = null;

        if (_assets.TryGetValue(assetPackName, out var assets))
        {
            dic = assets.MakeDictionary();
            return true;
        }

        return false;
    }
}

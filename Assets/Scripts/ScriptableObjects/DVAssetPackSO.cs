using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "Assets", menuName = "Scriptable Object/Assets", order = int.MaxValue)]
public class DVAssetPackSO : ScriptableObject
{
    public List<UnityEngine.Object> assets;

#if UNITY_EDITOR
    [Space]
    public string assetType = "prefab";
    public DefaultAsset[] folders;
#endif

    public Dictionary<string, UnityEngine.Object> MakeDictionary()
    {
        Dictionary<string, UnityEngine.Object> dic = new Dictionary<string, UnityEngine.Object>();
        for (int i = 0; i < assets.Count; i++)
        {
            dic[assets[i].name] = assets[i];
        }

        return dic;
    }
}

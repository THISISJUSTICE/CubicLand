using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName ="Assets", menuName ="Scriptable Object/Assets", order = int.MaxValue)]
public class DVAssets : ScriptableObject
{
    #region Types
    public enum AssetType { 
        Cube,
        Effect,
    }
    #endregion

    #region Variables
    public string assetType;
    public List<UnityEngine.Object> assets;
    #endregion

    #region Utils
    public AssetType Type { 
        get {
            Enum.TryParse(assetType, out AssetType type);
            return type;
        } 
    }

    public Dictionary<string, UnityEngine.Object> MakeDictionary() {
        Dictionary<string, UnityEngine.Object> dic = new Dictionary<string, UnityEngine.Object>();
        for (int i = 0; i < assets.Count; i++)
        {
            dic[assets[i].name] = assets[i];
        }

        return dic;
    }
    #endregion
}

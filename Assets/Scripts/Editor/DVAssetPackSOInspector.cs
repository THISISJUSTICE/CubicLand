using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(DVAssetPackSO))]
public class DVAssetPackSOInspector : Editor
{
    private DVAssetPackSO _assetPack;

    public override void OnInspectorGUI()
    {
        _assetPack = (DVAssetPackSO)target;

        base.OnInspectorGUI();

        GUILayout.Space(20f);
        DVEditorUtil.SpaceHorizontalLayout(20f, () =>
        {
            if (GUILayout.Button("Update"))
                UpdateAsset();
        });
    }

    private void UpdateAsset()
    {
        HashSet<UnityEngine.Object> assets = new HashSet<UnityEngine.Object>();
        if (_assetPack.assets != null)
        {
            foreach (UnityEngine.Object asset in _assetPack.assets)
                assets.Add(asset);
        }

        if (_assetPack.folders == null)
            return;

        string[] folders = new string[_assetPack.folders.Length];
        for (int i = 0; i < _assetPack.folders.Length; i++)
            folders[i] = AssetDatabase.GetAssetPath(_assetPack.folders[i]);

        string[] guids = AssetDatabase.FindAssets($"t:{_assetPack.assetType}", folders);
        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            assets.Add(asset);
        }

        List<UnityEngine.Object> assetList = assets.ToList();
        assetList.Sort((a, b) => a.name.CompareTo(b.name));
        _assetPack.assets = assetList;
        EditorUtility.SetDirty(_assetPack);
    }
}

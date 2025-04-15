using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(DVAssets))]
public class DVAssetsEditor : Editor
{
    #region Variables
    private DVAssets _assets;

    private bool _showEditor = true;
    #endregion

    #region GUI Functions
    public override void OnInspectorGUI()
    {
        _assets = (DVAssets)target;

        GUILayout.BeginHorizontal();
        GUILayout.Space(200f);
        if (GUILayout.Button("Show Editor")) { 
            _showEditor = !_showEditor;
        }

        GUILayout.Space(10f);
        if (GUILayout.Button("Select Self")) {
            Selection.activeObject = _assets;
        }

        GUILayout.Space(20f);
        if (GUILayout.Button("Find Assets")) {
            FindAssets();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10f);
        if (_showEditor) {
            var type = (DVAssets.AssetType)EditorGUILayout.EnumPopup("Asset Type", _assets.Type);
            _assets.assetType = type.ToString();

            GUILayout.Space(10f);
            SerializedProperty prop = serializedObject.FindProperty("assets");
            EditorGUILayout.PropertyField(prop);
        }
        else
            base.OnInspectorGUI();
    }
    #endregion

    #region Utils
    private void FindAssets() {
        const string mainPath = "Assets";
        string folderPath = EditorUtility.OpenFolderPanel("", mainPath, "");
        if (string.IsNullOrEmpty(folderPath))
            return;

        string projectPath = Directory.GetCurrentDirectory().Replace("\\", "/");
        folderPath = folderPath.Replace("\\", "/");

        if (folderPath.StartsWith(projectPath, StringComparison.OrdinalIgnoreCase))
        {
            string relativePath = folderPath.Substring(projectPath.Length + 1);
            if (relativePath.StartsWith(mainPath))
                folderPath = relativePath;
        }

        if (!folderPath.StartsWith(mainPath))
            return;

        if (!Directory.Exists(folderPath))
            return;

        string[] paths = Directory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly);
        paths = paths
            .Where(path => !path.EndsWith(".meta"))
            .ToArray();
        HashSet<UnityEngine.Object> objects = new HashSet<UnityEngine.Object>();

        foreach (var path in paths) {
            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            objects.Add(obj);
        }

        for (int i = 0; i < _assets.assets.Count; i++) {
            if (_assets.assets[i] != null)
                objects.Add(_assets.assets[i]);
        }

        _assets.assets = objects.ToList();
        _assets.assets.Sort((a, b) =>  a.name.CompareTo(b.name));
    }
    #endregion
}

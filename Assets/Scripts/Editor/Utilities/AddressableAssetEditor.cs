using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;

namespace CustomTIJI
{
    public class AddressableAssetEditor : EditorWindow
    {
        private abstract class EditorOption
        {
            public abstract void DrawOption();
        }

        private static List<UnityEngine.Object> _assets = new List<UnityEngine.Object>();
        private static bool _assetFieldFoldOut = true;

        private Vector2 _scrollPosition;
        private int _tabIndex;
        private string[] _tabNames = { "Default Asset" };
        private readonly EditorOption[] _editorOptions = { new DefaultAssetOption() };


        [MenuItem("Custom Editor Utils/Addressable Asset Editor")]
        public static void ShowWindow()
        {
            GetWindow<AddressableAssetEditor>("Addressable Asset Editor").Show();
        }

        private void OnGUI()
        {
            _tabIndex = GUILayout.Toolbar(_tabIndex, _tabNames);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height - 30f));

            GUILayout.Space(20);
            _editorOptions[_tabIndex].DrawOption();

            EditorGUILayout.EndScrollView();
        }

        private class DefaultAssetOption : EditorOption
        {
            private bool _optionFoldout = false;

            private int _optionGroupIndex;
            private string _optionGroupName;
            private string _optionAddressableLabel = string.Empty;
            private string _optionPrefixText = string.Empty;

            public override void DrawOption()
            {
                _assets = EditorUtil.DrawAssetList("Assets", ref _assetFieldFoldOut, _assets, false) as List<UnityEngine.Object>;

                GUILayout.Space(20f);
                EditorUtil.SpaceHorizontalLayout(20f, () =>
                {
                    if (GUILayout.Button("Set Default Asset"))
                        SetDefaultAsset();
                });

                GUILayout.Space(10f);
                _optionFoldout = EditorGUILayout.Foldout(_optionFoldout, "Options");
                if (_optionFoldout)
                {
                    const float horizontalLayoutSpace = 20f;

                    EditorUtil.SpaceHorizontalLayout(horizontalLayoutSpace, () =>
                    {
                        string[] groupContents = AddressableUtil.GetGroupContents();

                        EditorGUILayout.LabelField("Addressable Group");
                        _optionGroupIndex = EditorGUILayout.Popup(_optionGroupIndex, groupContents);
                        _optionGroupName = groupContents[_optionGroupIndex].Replace(AddressableUtil.DEFAULT_GROUP_SIGN, "");
                    });

                    GUILayout.Space(5f);
                    EditorUtil.SpaceHorizontalLayout(horizontalLayoutSpace, () =>
                    {
                        EditorGUILayout.LabelField("Addressable Label");
                        _optionAddressableLabel = EditorGUILayout.TextField(_optionAddressableLabel);
                    });

                    GUILayout.Space(5f);
                    EditorUtil.SpaceHorizontalLayout(horizontalLayoutSpace, () =>
                    {
                        EditorGUILayout.LabelField("Addressable Name Prefix Text");
                        _optionPrefixText = EditorGUILayout.TextField(_optionPrefixText);
                    });
                }
            }

            private void SetDefaultAsset()
            {
                if (_assets == null || _assets.Count < 1)
                    return;

                for (int i = 0; i < _assets.Count; i++)
                {
                    if (_assets[i] == null)
                        continue;

                    string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_assets[i]));
                    AddressableAssetEntry entry;
                    string addressableName;


                    if (_optionFoldout)
                    {
                        entry = AddressableUtil.GetOrAddEntry(guid, _optionGroupName, out bool isCreate);
                        addressableName = _assets[i].name.Replace(" ", "");
                        if (!string.IsNullOrEmpty(_optionPrefixText))
                            addressableName = $"{_optionPrefixText}_{addressableName}";
                    }
                    else
                    {
                        entry = AddressableUtil.GetOrAddEntry(guid, out bool isCreate);
                        addressableName = _assets[i].name.Replace(" ", "");
                    }

                    if (!string.IsNullOrEmpty(_optionAddressableLabel))
                        AddressableUtil.AddLabel(_optionAddressableLabel);

                    AddressableUtil.SetupEntry(entry, addressableName, _optionAddressableLabel);
                }

                AddressableUtil.SaveSettings();

                Debug.Log("Set Default Asset");
                Selection.activeObject = _assets[_assets.Count - 1];
            }
        }
    }
}
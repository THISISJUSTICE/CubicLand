using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Commar
{
    public class AddressableAssetEditor : TabEditorWindow
    {
        protected abstract class WindowOption<T> : EditorOption where T : AddressableAssetEditor
        {
            protected T _window;

            protected IList<UnityEngine.Object> Assets
            {
                get => _window._assets;
                set => _window._assets = value;
            }

            public WindowOption(T window)
            {
                _window = window;
            }
        }

        public const string EDITOR_NAME = "Addressable Asset Editor";

        protected IList<UnityEngine.Object> _assets = new List<UnityEngine.Object>();
        private bool _assetFieldFoldOut = true;

        protected int _optionGroupIndex;
        protected string _optionGroupName;

        protected readonly List<EditorOption> _editorOptionList = new List<EditorOption>();
        private EditorOption[] _editorOptions;
        protected override EditorOption[] EditorOptions => _editorOptions;

        [MenuItem(EditorUtil.MAIN_MENU + "/" + EDITOR_NAME)]
        public static void ShowWindow()
        {
            ShowThisWindow<AddressableAssetEditor>();
        }

        protected static AddressableAssetEditor ShowThisWindow<T>() where T : AddressableAssetEditor
        {
            T window = GetWindow<T>(EDITOR_NAME);
            window.AddOptions();

            window._editorOptions = window._editorOptionList.ToArray();
            SetTabs(window);

            window.Show();

            return window;
        }

        protected virtual void AddOptions()
        {
            _editorOptionList.Add(new DefaultAssetOption(this));
            _editorOptionList.Add(new DependencyPathCleaner(this));
        }

        protected void DrawAssetList()
        {
            _assets = EditorUtil.DrawAssetList("Assets", ref _assetFieldFoldOut, _assets, false);
        }

        protected void DrawGroupPopup()
        {
            string[] groupContents = AddressableUtil.GetGroupContents();

            EditorGUILayout.LabelField("Addressable Group");
            _optionGroupIndex = EditorGUILayout.Popup(_optionGroupIndex, groupContents);
            _optionGroupName = groupContents[_optionGroupIndex].Replace(AddressableUtil.DEFAULT_GROUP_SIGN, "");
        }

        protected void GroupAddressable(string label = "", string prefixText = "")
        {
            if (_assets == null || _assets.Count < 1)
                return;

            if (!string.IsNullOrEmpty(label))
                AddressableUtil.AddLabel(label);

            for (int i = 0; i < _assets.Count; i++)
            {
                if (_assets[i] == null)
                    continue;

                string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_assets[i]));
                AddressableAssetEntry entry = AddressableUtil.GetOrAddEntry(guid, _optionGroupName, out bool isCreate);
                string addressableName = _assets[i].name.Replace(" ", "");

                if (!string.IsNullOrEmpty(prefixText))
                    addressableName = $"{prefixText}_{addressableName}";

                AddressableUtil.SetupEntry(entry, addressableName, label);
            }

            AddressableUtil.SaveSettings();

            Debug.Log($"Groupped Assets {_assets.Count}");
            Selection.activeObject = _assets[_assets.Count - 1];
        }

        private class DefaultAssetOption : WindowOption<AddressableAssetEditor>
        {
            private bool _optionFoldout = false;

            protected int _optionGroupIndex;
            protected string _optionGroupName;
            private string _optionPrefixText = string.Empty;

            public override string TabName => "Default Asset";

            public DefaultAssetOption(AddressableAssetEditor window)
                : base(window) { }

            public override void DrawOption()
            {
                _window.DrawAssetList();

                GUILayout.Space(20f);
                GUILayout.BeginHorizontal();

                GUILayout.Space(10f);
                if (GUILayout.Button("Set Default Asset"))
                    SetDefaultAsset();
                GUILayout.EndHorizontal();

                GUILayout.Space(10f);
                _optionFoldout = EditorGUILayout.Foldout(_optionFoldout, "Options");
                if (_optionFoldout)
                {
                    const float horizontalLayoutSpace = 20f;

                    EditorUtil.SpaceHorizontalLayout(horizontalLayoutSpace, () =>
                    {
                        _window.DrawGroupPopup();
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
                _window.GroupAddressable("");
            }
        }

        private class DependencyPathCleaner : WindowOption<AddressableAssetEditor>
        {
            private const string GROUP_NAME = "Dependencies";

            private AddressableAssetGroup _group;
            private readonly List<string> _dirtyKeys = new List<string>();

            private bool _foldout;
            private bool _dirtyFoldout;

            public override string TabName => "Dependency Path Cleaner";

            public DependencyPathCleaner(AddressableAssetEditor window)
                : base(window)
            {
                _group = AddressableUtil.GetOrAddGroup(GROUP_NAME, out bool created);
            }

            public override void DrawOption()
            {
                Refresh();

                GUILayout.BeginHorizontal();
                GUILayout.Space(5f);
                _foldout = EditorGUILayout.Foldout(_foldout, $"All Keys ({_group.entries.Count})");
                GUILayout.EndHorizontal();
                if (_foldout)
                {
                    GUILayout.Space(3f);
                    foreach (AddressableAssetEntry entry in _group.entries)
                        DrawKey(entry.address);
                }

                GUILayout.Space(10f);
                GUILayout.BeginHorizontal();
                GUILayout.Space(5f);
                _dirtyFoldout = EditorGUILayout.Foldout(_dirtyFoldout, $"Dirty Keys ({_dirtyKeys.Count})");
                GUILayout.EndHorizontal();
                if (_dirtyFoldout)
                {
                    GUILayout.Space(3f);
                    foreach (string key in _dirtyKeys)
                        DrawKey(key);
                }

                GUILayout.Space(20f);
                if (GUILayout.Button("Clean"))
                    Clean();
            }

            private void Refresh()
            {
                _dirtyKeys.Clear();

                foreach (AddressableAssetEntry entry in _group.entries)
                {
                    if (CheckHasPrefix(entry.address))
                        continue;

                    _dirtyKeys.Add(entry.address);
                }
            }

            private void DrawKey(string key)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20f);
                EditorGUILayout.LabelField(key);
                GUILayout.Space(10f);
                GUILayout.EndHorizontal();
            }

            private void Clean()
            {
                const string prefix = GROUP_NAME + "/";

                foreach (AddressableAssetEntry entry in _group.entries)
                {
                    if (CheckHasPrefix(entry.address))
                        continue;

                    entry.SetAddress(prefix + Path.GetFileName(entry.address));
                }
            }

            private bool CheckHasPrefix(string key)
            {
                const string prefix = GROUP_NAME + "/";

                return key.Substring(0, prefix.Length) == prefix;
            }
        }
    }
}
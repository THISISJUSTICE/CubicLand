using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CustomTIJI
{
    /// <summary>
    /// Target Type: DefaultAsset's List or Array
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class FoldersAttribute : PropertyAttribute
    {
        public string TargetVariableName { get; }

        public FoldersAttribute(string targetVariableName) => TargetVariableName = targetVariableName;
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(FoldersAttribute))]
    public class FoldersAttributeEditor : PropertyDrawer
    {
        private enum TargetListType { Array, List, ETC };

        private FoldersAttribute _foldersAttribute;
        private UnityEngine.Object _targetObject;
        private FieldInfo _folderField;
        private FieldInfo _targetField;
        private Type _targetFieldType;
        private TargetListType _targetListType;

        private const BindingFlags FIELD_FLAG =
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;

        private bool _once = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, label);

            if (_once)
            {
                _once = false;

                _foldersAttribute = (FoldersAttribute)attribute;
                _targetObject = property.serializedObject.targetObject;

                ValidateType(property);
            }

            if (_folderField == null || _targetField == null)
                return;

            IList<DefaultAsset> folders = _folderField.GetValue(_targetObject) as IList<DefaultAsset>;
            if (folders == null || folders.Count == 0)
                return;

            if (property.displayName != $"Element {folders.Count - 1}")
                return;

            GUILayout.Space(10f);
            GUILayout.BeginHorizontal();
            GUILayout.Space(position.width * 0.5f);
            if (GUILayout.Button($"Find {_targetFieldType.Name}s"))
            {
                FindTargets(folders);
            }

            GUILayout.EndHorizontal();
        }

        private void ValidateType(SerializedProperty property)
        {
            _folderField = _targetObject.GetType().GetField(property.propertyPath.Split('.')[0], FIELD_FLAG);

            if (_folderField == null)
                return;

            if (!typeof(IList<DefaultAsset>).IsAssignableFrom(_folderField.FieldType))
            {
                _folderField = null;
                return;
            }

            _targetField = _targetObject.GetType().GetField(_foldersAttribute.TargetVariableName, FIELD_FLAG);

            if (_targetField == null)
                return;

            Type fieldType = _targetField.FieldType;
            if (typeof(IList).IsAssignableFrom(fieldType))
            {
                if (fieldType.IsArray)
                    _targetListType = TargetListType.Array;
                else if (fieldType.IsGenericType &&
                    fieldType.GetGenericTypeDefinition() == typeof(List<>))
                    _targetListType = TargetListType.List;
                else
                    _targetListType = TargetListType.ETC;

                if (_targetListType == TargetListType.Array)
                    _targetFieldType = fieldType.GetElementType();
                else if (_targetListType == TargetListType.List)
                    _targetFieldType = fieldType.GetGenericArguments()[0];
                else
                {
                    _folderField = null;
                    return;
                }

                if (!typeof(UnityEngine.Object).IsAssignableFrom(_targetFieldType))
                {
                    _folderField = null;
                    return;
                }
            }
            else
            {
                _folderField = null;
                return;
            }
        }

        private void FindTargets(IList<DefaultAsset> folders)
        {
            HashSet<string> directories = new HashSet<string>();
            foreach (DefaultAsset folder in folders)
            {
                if (folder != null)
                    directories.Add(AssetDatabase.GetAssetPath(folder));
            }

            HashSet<UnityEngine.Object> assets = new HashSet<UnityEngine.Object>();
            IList<UnityEngine.Object> list = _targetField.GetValue(_targetObject) as IList<UnityEngine.Object>;
            if (list != null && list.Count > 0)
            {
                foreach (UnityEngine.Object obj in list)
                {
                    if (obj != null)
                        assets.Add(obj);
                }
            }

            string[] guids = AssetDatabase.FindAssets("", directories.ToArray());
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);

                if (_targetFieldType.IsAssignableFrom(asset.GetType()))
                    assets.Add(asset);
            }

            SetValue(assets);
        }

        private void SetValue(HashSet<UnityEngine.Object> assets)
        {
            if (assets.Count > 0)
            {
                if (_targetListType == TargetListType.Array)
                {
                    Array array = Array.CreateInstance(_targetFieldType, assets.Count);
                    int index = 0;

                    foreach (UnityEngine.Object asset in assets)
                        array.SetValue(asset, index++);

                    _targetField.SetValue(_targetObject, array);
                }
                else if (_targetListType == TargetListType.List)
                {
                    IList list = (IList)Activator.CreateInstance(_targetField.FieldType);

                    foreach (UnityEngine.Object asset in assets)
                        list.Add(asset);

                    _targetField.SetValue(_targetObject, list);
                }
            }
        }
    }
#endif
}